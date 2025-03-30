document.addEventListener('DOMContentLoaded', function () {
    if (sessionStorage.getItem("email")) {
        setTimeout(function () {
            document.getElementById('resend-link').style.display = 'inline';
        }, 20000);
    }
});

document.getElementById('forgotPasswordForm').addEventListener('submit', function (e) {
    var email = document.querySelector('input[name="Email"]').value;
    if (email) {
        sessionStorage.setItem("email", email);
    }
});

document.getElementById('resend-link').addEventListener('click', function () {
    var email = sessionStorage.getItem("email");
    if (!email) {
        console.error("No email stored.");
        return;
    }
    var token = document.querySelector('input[name="__RequestVerificationToken"]').value;
    if (token) {
    console.log("Anti-forgery token:", token.value);
				} else {
					console.error("Anti-forgery token not found !");
				}
    var data = new URLSearchParams();
    data.append("Email", email);
    data.append("__RequestVerificationToken", token);
	var forgotPassResetElem = document.getElementById('forgot-pass-reset-url');
    var forgotPassResetUrl = forgotPassResetElem ? forgotPassResetElem.getAttribute('data-forgot-reset-url') : null;	
	var forgotPasswordElem = document.getElementById('forgot-password-url');
    var forgotPasswordUrl = forgotPasswordElem ? forgotPasswordElem.getAttribute('data-forgot-password-url') : null;
	
    fetch(forgotPasswordUrl, {
        method: 'POST',
        headers: {
            'Content-Type': 'application/x-www-form-urlencoded'
        },
        body: data.toString()
    })
    .then(function (response) {
        if (!response.ok) {
            throw new Error("Network response was not ok.");
        }
        return response.text();
    })
    .then(function (result) {
    console.log("Password reset link resent.");
    window.location.href = forgotPassResetUrl + "?message=" + encodeURIComponent("Password reset link emailed !");
})
    .catch(function (error) {
        console.error("Error resending password reset link:", error);
    });
});
