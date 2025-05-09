document.addEventListener("DOMContentLoaded", () => {
    const form = document.getElementById("individualForm");

    if (!form) {
        console.error("Form not found. Make sure the form has id='individualForm'");
        return;
    }

    form.addEventListener("submit", async (e) => {
        e.preventDefault();

        const formData = {
            firstName: document.getElementById("firstName").value,
            lastName: document.getElementById("lastName").value,
            age: parseInt(document.getElementById("age").value),
            gender: document.getElementById("gender").value
        };

        try {
            const response = await fetch("https://your-api-url/api/Individuals", {
                method: "POST",
                headers: { "Content-Type": "application/json" },
                body: JSON.stringify(formData)
            });

            if (response.ok) {
                alert("Individual successfully added!");
                form.reset();
            } else {
                const errorText = await response.text();
                alert("Error: " + errorText);
            }
        } catch (err) {
            alert("Network or server error: " + err.message);
        }
    });
});
