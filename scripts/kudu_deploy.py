#!/usr/bin/env python3
"""Extract Kudu (MSDeploy) credentials from an Azure publish profile."""
import base64
import json
import os
import re
import sys
import urllib.request


def load_profile_text() -> str:
    path = os.environ.get("PROFILE_FILE")
    if path and os.path.isfile(path):
        with open(path, "rb") as f:
            return f.read().lstrip(b"\xef\xbb\xbf").strip().decode("utf-8", errors="replace")
    raw = os.environ.get("PUBLISH_PROFILE", "")
    return raw.lstrip("\ufeff").strip()


def extract_credentials(text: str) -> tuple[str, str]:
    patterns = [
        r'<publishProfile\b[^>]*publishMethod="MSDeploy"[^>]*/>',
        r'<publishProfile\b[^>]*publishMethod="ZipDeploy"[^>]*/>',
        r'<publishProfile\b[^>]*publishMethod="Web Deploy"[^>]*/>',
    ]
    for pattern in patterns:
        match = re.search(pattern, text, re.IGNORECASE)
        if match:
            tag = match.group(0)
            user = re.search(r'userName="([^"]+)"', tag)
            pwd = re.search(r'userPWD="([^"]+)"', tag)
            if user and pwd:
                return user.group(1), pwd.group(1)

    for attrs in re.findall(r"<publishProfile\b([^>]*)/>", text, re.IGNORECASE):
        if re.search(r'publishMethod="(?:MSDeploy|ZipDeploy|Web Deploy)"', attrs, re.IGNORECASE):
            user = re.search(r'userName="([^"]+)"', attrs)
            pwd = re.search(r'userPWD="([^"]+)"', attrs)
            if user and pwd:
                return user.group(1), pwd.group(1)

    user = re.search(r'userName="([^"]+)"', text)
    pwd = re.search(r'userPWD="([^"]+)"', text)
    return (user.group(1) if user else "", pwd.group(1) if pwd else "")


def scm_host(app_name: str) -> str:
    return f"{app_name}.scm.azurewebsites.net"


def kudu_request(host: str, username: str, password: str, path: str, method: str = "GET", body=None) -> str:
    auth = base64.b64encode(f"{username}:{password}".encode()).decode()
    headers = {"Authorization": f"Basic {auth}"}
    payload = None
    if body is not None:
        headers["Content-Type"] = "application/json"
        payload = json.dumps(body).encode()
    req = urllib.request.Request(f"https://{host}{path}", data=payload, headers=headers, method=method)
    with urllib.request.urlopen(req, timeout=600) as resp:
        return resp.read().decode()


def main() -> int:
    action = sys.argv[1] if len(sys.argv) > 1 else ""
    app_name = os.environ.get("APP_NAME", "").strip()
    if not app_name:
        print("APP_NAME is required", file=sys.stderr)
        return 1

    text = load_profile_text()
    username, password = extract_credentials(text)
    host = scm_host(app_name)
    if not all([username, password]):
        print("Could not extract MSDeploy credentials from publish profile", file=sys.stderr)
        return 1

    if action == "configure":
        settings = {
            "ConnectionStrings__DefaultConnection": os.environ["AZURE_CS"],
            "JWT_KEY": os.environ["JWT_KEY"],
            "JWT_ISSUER": "241RunnersAwareness",
            "JWT_AUDIENCE": "241RunnersAwareness",
            "ASPNETCORE_ENVIRONMENT": "Production",
        }
        print(f"Applying app settings via Kudu ({host})")
        kudu_request(host, username, password, "/api/settings", "POST", settings)
        print("Restarting app")
        try:
            kudu_request(host, username, password, "/api/restart", "POST", {})
        except Exception:
            pass
        print("App Service configured")
        return 0

    if action == "deploy":
        zip_path = os.environ.get("ZIP_PATH", "")
        if not zip_path or not os.path.isfile(zip_path):
            print("ZIP_PATH must point to deploy.zip", file=sys.stderr)
            return 1
        with open(zip_path, "rb") as f:
            payload = f.read()
        auth = base64.b64encode(f"{username}:{password}".encode()).decode()
        req = urllib.request.Request(
            f"https://{host}/api/zipdeploy?isAsync=true",
            data=payload,
            headers={"Authorization": f"Basic {auth}", "Content-Type": "application/zip"},
            method="POST",
        )
        print(f"Deploying to https://{host}/api/zipdeploy")
        with urllib.request.urlopen(req, timeout=600) as resp:
            print(resp.read().decode() or "Deploy accepted")
        return 0

    print(f"Unknown action: {action}", file=sys.stderr)
    return 1


if __name__ == "__main__":
    raise SystemExit(main())
