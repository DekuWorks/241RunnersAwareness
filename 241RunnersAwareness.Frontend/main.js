document.addEventListener("DOMContentLoaded", function () {

  // 🔹 INDIVIDUAL FORM SUBMISSION
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
          headers: { "Content-Type": "application/json" },
          body: JSON.stringify(data)
        });

        const message = response.ok
          ? "✅ Individual added successfully!"
          : `❌ Error: ${await response.text()}`;

        if (responseBox) responseBox.textContent = message;
        if (response.ok) form.reset();
      } catch (error) {
        if (responseBox) responseBox.textContent = `❌ Request failed: ${error.message}`;
      }
    });
  }

  // 🔹 EMERGENCY CONTACT FORM SUBMISSION
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
          headers: { "Content-Type": "application/json" },
          body: JSON.stringify(contactData)
        });

        const message = response.ok
          ? "✅ Emergency contact added!"
          : `❌ Error: ${await response.text()}`;

        if (emergencyResponseBox) emergencyResponseBox.textContent = message;
        if (response.ok) emergencyForm.reset();
      } catch (error) {
        if (emergencyResponseBox) emergencyResponseBox.textContent = `❌ Request failed: ${error.message}`;
      }
    });

    // 🔹 Populate Dropdown
    async function populateIndividualDropdown() {
      const dropdown = document.getElementById("individualID");
      if (!dropdown) return;

      try {
        const response = await fetch("https://241runnersawareness-backend-bhf9dth5hccdeme8.canadacentral-01.azurewebsites.net/api/Individuals");
        const individuals = await response.json();

        dropdown.innerHTML = '<option value="">-- Select Individual --</option>';
        individuals.forEach(ind => {
          const option = document.createElement("option");
          option.value = ind.individualID;
          option.textContent = `${ind.fullName} (DOB: ${ind.dateOfBirth})`;
          dropdown.appendChild(option);
        });
      } catch (error) {
        dropdown.innerHTML = '<option value="">⚠️ Error loading individuals</option>';
        console.error("Dropdown error:", error);
      }
    }

    populateIndividualDropdown();
  }

  // 🔹 VIEW RECORDS PAGE
  const individualListContainer = document.getElementById("individualList");

  if (individualListContainer) {
    Promise.all([
      fetch("https://241runnersawareness-backend-bhf9dth5hccdeme8.canadacentral-01.azurewebsites.net/api/Individuals").then(res => res.json()),
      fetch("https://241runnersawareness-backend-bhf9dth5hccdeme8.canadacentral-01.azurewebsites.net/api/EmergencyContact").then(res => res.json())
    ])
    .then(([individuals, contacts]) => {
      if (!individuals.length) {
        individualListContainer.textContent = "No records found.";
        return;
      }

      const list = document.createElement("ul");

      individuals.forEach(ind => {
        const linkedContacts = contacts.filter(c => c.individualID === ind.individualID);

        let contactHTML = linkedContacts.length
          ? `<ul>${linkedContacts.map(c => `
              <li>
                <strong>${c.name}</strong><br/>
                Phone: ${c.phone}<br/>
                Relationship: ${c.relationship}<br/>
                Email: ${c.email}<br/>
                Address: ${c.address}
              </li>`).join("<br/>")}
            </ul>`
          : "<em>No emergency contacts listed.</em>";

        const item = document.createElement("li");
        item.innerHTML = `
          <div class="record-block" id="record-${ind.individualID}">
            <strong>${ind.fullName}</strong> (DOB: ${ind.dateOfBirth})<br/>
            Status: ${ind.currentStatus || "Not specified"}<br/>
            Last Seen: ${ind.lastSeenLocation || "Not available"}<br/><br/>
            <u>Emergency Contacts:</u><br/>
            ${contactHTML}
            <button onclick="printRecord('record-${ind.individualID}')">🖨️ Print</button>
            <hr/>
          </div>
        `;
        list.appendChild(item);
      });

      individualListContainer.innerHTML = "";
      individualListContainer.appendChild(list);
    })
    .catch(error => {
      individualListContainer.textContent = "⚠️ Failed to load data.";
      console.error("Data load error:", error);
    });
  }

  // 🔹 VIEW CONTACTS PAGE (with search)
  const contactListContainer = document.getElementById("contactList");
  const searchInput = document.getElementById("searchInput");

  if (contactListContainer && searchInput) {
    let allContacts = [];
    let allIndividuals = [];

    Promise.all([
      fetch("https://241runnersawareness-backend-bhf9dth5hccdeme8.canadacentral-01.azurewebsites.net/api/EmergencyContact").then(res => res.json()),
      fetch("https://241runnersawareness-backend-bhf9dth5hccdeme8.canadacentral-01.azurewebsites.net/api/Individuals").then(res => res.json())
    ])
    .then(([contacts, individuals]) => {
      allContacts = contacts;
      allIndividuals = individuals;
      renderContacts(allContacts, allIndividuals);
    })
    .catch(error => {
      contactListContainer.textContent = "⚠️ Failed to load contact data.";
      console.error("Contact list error:", error);
    });

    searchInput.addEventListener("input", function () {
      const keyword = this.value.toLowerCase();
      const filtered = allContacts.filter(c => {
        const person = allIndividuals.find(i => i.individualID === c.individualID);
        return (
          c.name.toLowerCase().includes(keyword) ||
          c.phone.toLowerCase().includes(keyword) ||
          (person && person.fullName.toLowerCase().includes(keyword))
        );
      });
      renderContacts(filtered, allIndividuals);
    });

    function renderContacts(contactsToRender, individuals) {
      contactListContainer.innerHTML = "";

      if (!contactsToRender.length) {
        contactListContainer.textContent = "No matching contacts found.";
        return;
      }

      const list = document.createElement("ul");
      contactsToRender.forEach(contact => {
        const linkedPerson = individuals.find(ind => ind.individualID === contact.individualID);
        const linkedName = linkedPerson ? linkedPerson.fullName : "Unknown Individual";

        const item = document.createElement("li");
        item.innerHTML = `
          <div class="record-block">
            <strong>${contact.name}</strong><br/>
            Relationship: ${contact.relationship}<br/>
            Phone: ${contact.phone}<br/>
            Email: ${contact.email}<br/>
            Address: ${contact.address}<br/>
            Linked To: <em>${linkedName}</em>
          </div>
        `;
        list.appendChild(item);
      });
      contactListContainer.appendChild(list);
    }
  }

  // 🔧 GLOBAL PRINT FUNCTION
  window.printRecord = function (recordId) {
    const originalContent = document.body.innerHTML;
    const printSection = document.getElementById(recordId).innerHTML;

    document.body.innerHTML = printSection;
    window.print();
    document.body.innerHTML = originalContent;
    location.reload(); // optional refresh
  };
});
