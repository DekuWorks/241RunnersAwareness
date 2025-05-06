document.getElementById("individualForm").addEventListener("submit", async function (e) {
    e.preventDefault();
  
    const data = {
      fullName: document.getElementById("fullName").value,
      gender: document.getElementById("gender").value,
      age: parseInt(document.getElementById("age").value),
      diagnosis: document.getElementById("diagnosis").value,
      lastSeenLocation: document.getElementById("lastSeenLocation").value,
      lastSeenDate: document.getElementById("lastSeenDate").value,
      photoPath: document.getElementById("photoPath").value,
      notes: document.getElementById("notes").value,
      currentStatus: document.getElementById("currentStatus").value,
      thumbprintPath: document.getElementById("thumbprintPath").value,
      fingerprintPath: document.getElementById("fingerprintPath").value,
      placementStatus: document.getElementById("placementStatus").value,
      specialNeedsDescription: document.getElementById("specialNeedsDescription").value,
      emergencyContacts: []
    };
  
    try {
      const res = await fetch("https://your-api.azurewebsites.net/api/Individuals", {
        method: "POST",
        headers: { "Content-Type": "application/json" },
        body: JSON.stringify(data)
      });
  
      const result = await res.json();
      document.getElementById("response").innerText = "Submitted! ID: " + result.individualId;
    } catch (err) {
      document.getElementById("response").innerText = "Error: " + err.message;
    }
  });
  