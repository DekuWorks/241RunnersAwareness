document.addEventListener("DOMContentLoaded", () => {
    const form = document.getElementById("individualForm");

    if (!form) {
        console.error("Form not found. Make sure it has id='individualForm'");
        return;
    }

    form.addEventListener("submit", async (e) => {
        e.preventDefault();

        const formData = {
            fullName: document.getElementById("fullName").value,
            dateOfBirth: document.getElementById("dateOfBirth").value,
            gender: document.getElementById("gender").value,
            race: document.getElementById("race").value,
            stateLastSeen: document.getElementById("stateLastSeen").value,
            countyLastSeen: document.getElementById("countyLastSeen").value,
            lastSeenLocation: document.getElementById("lastSeenLocation").value,
            height: document.getElementById("height").value,
            weight: document.getElementById("weight").value,
            disability: document.getElementById("disability").value,
            specialNeedsDescription: document.getElementById("specialNeedsDescription").value
        };

        try {
            const response = await fetch("https://your-api-url/api/Individuals", {
                method: "POST",
                headers: {
                    "Content-Type": "application/json"
                },
                body: JSON.stringify(formData)
            });

            if (response.ok) {
                alert("Individual successfully added!");
                form.reset();
            } else {
                const errorMsg = await response.text();
                alert("Error submitting form: " + errorMsg);
            }
        } catch (error) {
            alert("Server error: " + error.message);
        }
    });
});
