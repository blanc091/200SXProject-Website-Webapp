function scrollToElement(elementId) {
    var element = document.getElementById(elementId);
    if (element) {
        element.scrollIntoView({ behavior: 'smooth' });
    } else {
        console.error(`${elementId} not found!`);
    }
}

document.addEventListener("DOMContentLoaded", function () {
    var isFormSubmitted = document.body.getAttribute('data-is-form-submitted');
    var isFormSuccess = document.body.getAttribute('data-is-form-success');
    var isUserLoggedIn = document.body.getAttribute('data-is-user-logged-in');
    var isFormRegisterSuccess = document.body.getAttribute('data-is-form-register-success');
    var isEmailVerifiedLogin = document.body.getAttribute('data-is-email-verified-login');
    var isUserVerified = document.body.getAttribute('data-is-user-verified');
    var isEntrySuccess = document.body.getAttribute('is-entry-success');
    var userExists = document.body.getAttribute('data-user-exists');
    var passResetEmailSent = document.body.getAttribute('data-is-pass-reset-email-sent');
    var isPassResetSuccess = document.body.getAttribute('data-is-pass-reset-success');
    var isNewsletterSubbed = document.body.getAttribute('data-is-newsletter-subbed');
    var isNewsletterError = document.body.getAttribute('data-is-newsletter-error');
    var isNiceTry = document.body.getAttribute('data-is-nice-try');
    var formSubmitted = isFormSubmitted === "True";
    var formSuccess = isFormSuccess === "True";

    console.log("Current Pathname:", window.location.pathname);

    switch (window.location.pathname) {
        case '/Dashboard/Dashboard':
            scrollToElement("maintenApp");
            break;
        case '/LoginRegister/Login':
            scrollToElement("header");
            break;
        case '/LoginRegister/ResetPassword':
        case '/LoginRegister/ForgotPassReset':
            scrollToElement("maintenApp");
            break;
        case '/LoginRegister/Register':
            scrollToElement("registerForm");
            break;
        case '/Newsletter/Subscribe':
            scrollToElement("newsletterForm");
            break;
    }

    if ((isUserLoggedIn && !formSubmitted) ||
        (formSubmitted && formSuccess) ||
        isFormRegisterSuccess == "yes" ||
        isEmailVerifiedLogin == "yes" ||
        isUserVerified == 'no' ||
        isPassResetSuccess == "yes" ||
        isEntrySuccess == "yes" ||
        userExists == "yes" ||
        isNiceTry == "yes" ||
        isNewsletterSubbed == "yes" ||
        passResetEmailSent == "yes") {
        setTimeout(function () {
            $('#messageModal').fadeOut(1250);
        }, 3000);
    }

    if (formSubmitted && !formSuccess) {
        scrollToContactForm();
    }

    if (window.location.pathname != '/Account/Login') {
        window.addEventListener('scroll', function () {
            var introElement = document.getElementById('intro');
            if (introElement) {
                if (window.scrollY > 0) {
                    introElement.classList.add('hidden');
                } else {
                    introElement.classList.remove('hidden');
                }
            }
        });
    }

    function scrollToContactForm() {
        var contactForm = document.getElementById('contactForm');
        if (contactForm) {
            contactForm.scrollIntoView({ behavior: 'smooth' });
        } else {
            console.error('Contact form not found!');
        }
    }

    $(document).ready(function () {
        const urlParams = new URLSearchParams(window.location.search);
        if (urlParams.has('scrollToContactForm')) {
            scrollToContactForm();
        }

        var contactLink = document.getElementById('contactLink');
        if (contactLink) {
            contactLink.addEventListener('click', function (event) {
                event.preventDefault();
                if (window.location.pathname === '/Home/Index') {
                    scrollToContactForm();
                } else {
                    window.location.href = '/Home/Index?scrollToContactForm=true';
                }
            });
        }
    });
});
