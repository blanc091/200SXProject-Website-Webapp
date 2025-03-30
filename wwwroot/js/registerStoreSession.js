document.addEventListener('DOMContentLoaded', function () {
    var emailInput = document.querySelector('input[name="Email"]');
    if (emailInput) {
        emailInput.addEventListener('change', function () {
            sessionStorage.setItem("email", emailInput.value);
        });
    }
});
