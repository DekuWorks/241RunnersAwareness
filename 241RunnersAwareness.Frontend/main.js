document.addEventListener("DOMContentLoaded", function () {
  // 🟢 INDIVIDUAL FORM LOGIC
  const form = document.getElementById("individualForm");
  const responseBox = document.getElementById("response");

  if (form) {
    form.addEventListener("submit", async function (e) {
      e.preventDefault();

      const data = {
        fullName: document.getElementById("fullName").value,
        gender: document.getElementById("gender").value,
        dateOfBirth: document.getElementById("dateOfBirth").value,
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
        const response = await fetch("https://241runnersawareness-backend-bhf9dth5hccdeme8.canadacentral-01.azurewebsites.net/api/Individuals", {
          method: "POST",
          headers: {
            "Content-Type": "application/json"
          },
          body: JSON.stringify(data)
        });

        if (response.ok) {
          if (responseBox) responseBox.textContent = "✅ Individual added successfully!";
          form.reset();
        } else {
          const errorText = await response.text();
          if (responseBox) responseBox.textContent = `❌ Error: ${errorText}`;
        }
      } catch (error) {
        if (responseBox) responseBox.textContent = `❌ Request failed: ${error.message}`;
      }
    });
  }

  // 🟢 EMERGENCY CONTACT FORM LOGIC
  const emergencyForm = document.getElementById("emergencyForm");
  const emergencyResponseBox = document.getElementById("emergencyResponse");

  if (emergencyForm) {
    emergencyForm.addEventListener("submit", async function (e) {
      e.preventDefault();

      const contactData = {
        name: document.getElementById("name").value,
        phone: document.getElementById("phone").value,
        relationship: document.getElementById("relationship").value,
        email: document.getElementById("email").value,
        address: document.getElementById("address").value,
        individualID: document.getElementById("individualID").value
      };

      try {
        const response = await fetch("https://241runnersawareness-backend-bhf9dth5hccdeme8.canadacentral-01.azurewebsites.net/api/EmergencyContact", {
          method: "POST",
          headers: {
            "Content-Type": "application/json"
          },
          body: JSON.stringify(contactData)
        });

        if (response.ok) {
          if (emergencyResponseBox) emergencyResponseBox.textContent = "✅ Emergency contact added!";
          emergencyForm.reset();
        } else {
          const errorText = await response.text();
          if (emergencyResponseBox) emergencyResponseBox.textContent = `❌ Error: ${errorText}`;
        }
      } catch (error) {
        if (emergencyResponseBox) emergencyResponseBox.textContent = `❌ Request failed: ${error.message}`;
      }
    });
  }
});
