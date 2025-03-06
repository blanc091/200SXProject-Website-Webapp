document.addEventListener("DOMContentLoaded", function () {
    console.log("DOM fully loaded, adding event listener...");

    const form = document.getElementById("newsletterForm");

    if (form) {
        console.log("Form found! Adding submit event listener...");

        form.addEventListener("submit", function (event) {
            console.log("Form submit event fired!");

            const emailInput = document.querySelector("input[name='email']");
            console.log("Email input value:", emailInput.value);

            if (!emailInput.value.trim()) {
                alert("Email cannot be empty!");
                event.preventDefault();
                return;
            }

            if (!emailInput.checkValidity()) {
                alert("Please enter a valid email address!");
                event.preventDefault();
            }
        });
    } else {
        console.log("Form not found!");
    }
});
