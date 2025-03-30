document.addEventListener('DOMContentLoaded', function () {
    setTimeout(function () {
        var resendLink = document.getElementById('register-resend-link');
        if (resendLink) {
            resendLink.style.display = 'inline';
        }
    }, 40000);

    var registerWidgetId;
    grecaptcha.ready(function () {
        registerWidgetId = grecaptcha.render('g-recaptcha-register', {
            'sitekey': '6LdVNIgqAAAAAGdAW2AVoZGG3uoSYm0LgW-se5OH',
            'size': 'invisible',
            'callback': function (token) {
                console.log("Registration token received (via callback):", token);
                submitRegistrationWithToken(token);
            }
        });
        console.log("Register widget rendered with ID:", registerWidgetId);
    });

    var registerResendLink = document.getElementById('register-resend-link');
    if (registerResendLink) {
        registerResendLink.addEventListener('click', function (e) {
            e.preventDefault();
            grecaptcha.execute(registerWidgetId);
        });
    }
});

function submitRegistrationWithToken(token) {
    var email = sessionStorage.getItem("email");
    if (!email) {
        console.error("No email stored for registration.");
        return;
    }
	
    var tokenElement = document.querySelector('input[name="__RequestVerificationToken"]');
    if (!tokenElement) {
        console.error("Anti-forgery token not found!");
        return;
    }
	
    var formToken = tokenElement.value;
    var data = new URLSearchParams();
    var username = document.querySelector('input[name="Username"]').value;
    var password = document.querySelector('input[name="Password"]').value;
    var subscribe = document.querySelector('input[name="SubscribeToNewsletter"]').checked;

    data.append("Username", username);
    data.append("Password", password);
    data.append("Email", email);
    data.append("SubscribeToNewsletter", subscribe);
    data.append("gRecaptchaResponseRegister", token);
    data.append("__RequestVerificationToken", formToken);

    var registerPostElem = document.getElementById('register-post-url');
    var registerPostUrl = registerPostElem ? registerPostElem.getAttribute('data-register-post-url') : null;
    var registerViewElem = document.getElementById('register-url');
    var registerViewUrl = registerViewElem ? registerViewElem.getAttribute('data-register-url') : null;

    if (!registerPostUrl || !registerViewUrl) {
        console.error("Registration URLs not found.");
        return;
    }

    fetch(registerPostUrl, {
        method: 'POST',
        headers: {
            'Content-Type': 'application/x-www-form-urlencoded',
            'X-Requested-With': 'XMLHttpRequest'
        },
        body: data.toString()
    })
        .then(function (response) {
            if (!response.ok) {
                throw new Error("Network response was not ok for registration.");
            }
            return response.text();
        })
        .then(function (result) {
            console.log("Registration re-trigger submitted successfully.");
            window.location.href = registerViewUrl + "?message=" + encodeURIComponent("Activation email resent.");
        })
        .catch(function (error) {
            console.error("Error during registration re-trigger:", error);
        });
}
