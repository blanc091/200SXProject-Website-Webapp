document.addEventListener('DOMContentLoaded', function () {
    var logoutLink = document.getElementById("logoutLink");
    console.log('DOM fully loaded and parsed');

    if (logoutLink) {
        logoutLink.addEventListener("click", function (event) {
            console.log('Logout link clicked');
            event.preventDefault();
            var csrfToken = document.querySelector('input[name="__RequestVerificationToken"]').value;

            fetch('/loginregister/logout', {
                method: 'POST',
                headers: {
                    'Content-Type': 'application/json',
                    'RequestVerificationToken': csrfToken
                }
            }).then(response => {
                if (response.ok) {
                    console.log('Logout successful');
                    window.location.href = '/';
                } else {
                    console.error("Logout failed");
                }
            }).catch(error => {
                console.error('Error:', error);
            });
        });
    } else {
        console.log('Logout link not found or user not logged in.');
    }
});
