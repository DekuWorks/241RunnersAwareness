document.addEventListener("DOMContentLoaded", function () {
    const form = document.getElementById("individualForm");
  
    form.addEventListener("submit", async function (e) {
      e.preventDefault();
  
      const data = {
        fullName: document.getElementById("fullName").value,
        gender: document.getElementById("gender").value,
        dateOfBirth: document.getElementById("age").value,
        specialNeedsDescription: document.getElementById("diagnosis").value,
        lastSeenLocation: document.getElementById("lastSeenLocation").value,
        photoPath: document.getElementById("photoPath").value,
        notes: document.getElementById("notes").value,
        currentStatus: document.getElementById("currentStatus").value,
        thumbprintPath: document.getElementById("thumbprint").value,
        fingerprintPath: document.getElementById("fingerprint").value,
        placementStatus: document.getElementById("placementStatus").value
      };
  
      try {
        const response = await fetch("https://your-api.azurewebsites.net/api/Individuals", {
          method: "POST",
          headers: {
            "Content-Type": "application/json"
          },
          body: JSON.stringify(data)
        });
  
        if (response.ok) {
          document.getElementById("response").textContent = "✅ Individual added successfully!";
          form.reset();
        } else {
          const errorText = await response.text();
          document.getElementById("response").textContent = `❌ Error: ${errorText}`;
        }
      } catch (error) {
        document.getElementById("response").textContent = `❌ Request failed: ${error.message}`;
      }
    });
  });
  