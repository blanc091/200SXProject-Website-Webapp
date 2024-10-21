document.addEventListener('DOMContentLoaded', function () {
    var logoutLink = document.getElementById("logoutLink");

    if (logoutLink) { 
        logoutLink.addEventListener("click", function (event) {
            event.preventDefault(); 

            fetch('/LoginRegister/Logout', {
                method: 'POST',
                headers: {
                    'RequestVerificationToken': document.querySelector('input[name="__RequestVerificationToken"]').value
                }
            }).then(response => {
                if (response.ok) {
                    window.location.href = '/';
                } else {
                    console.error("Logout failed");
                }
            }).catch(error => {
                console.error('Error:', error);
            });
        });
    } else {
        console.error('Logout link not found');
    }
});
