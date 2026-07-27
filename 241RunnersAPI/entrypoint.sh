#!/bin/sh
set -e
export ASPNETCORE_URLS="http://0.0.0.0:${PORT:-8080}"
exec dotnet 241RunnersAPI.dll
