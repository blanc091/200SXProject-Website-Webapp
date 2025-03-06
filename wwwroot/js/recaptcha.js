window.onload = function () {
    if (typeof grecaptcha !== 'undefined') {
        grecaptcha.ready(function () {
            try {
                const forms = [
                    { formId: "contactForm", recaptchaId: "g-recaptcha-contact" },
                    { formId: "newsletterForm", recaptchaId: "g-recaptcha-newsletter" },
                    { formId: "registerForm", recaptchaId: "g-recaptcha-register" }
                ];
                forms.forEach(({ formId, recaptchaId }) => {
                    grecaptcha.render(recaptchaId, {
                        'sitekey': '6LdVNIgqAAAAAGdAW2AVoZGG3uoSYm0LgW-se5OH',
                        'callback': function (token) {
                            console.log(`${formId} token received:`, token);
                            onSubmitWithEvent(token, window.lastClickedButtonEvent);
                        },
                        'size': 'invisible'
                    });
                });
            } catch (error) {
                console.error("Error initializing reCAPTCHA render:", error);
            }
        });
    } else {
        console.error('reCAPTCHA library not loaded.');
    }
};
document.addEventListener("DOMContentLoaded", function () {
    const buttons = document.querySelectorAll('.g-recaptcha');
    buttons.forEach(button => {
        button.addEventListener("click", function (e) {
            console.log("Button clicked, saving event:", e);
            window.lastClickedButtonEvent = e;
        });
    });
});
window.onSubmitWithEvent = function (token, event) {
    console.log("ReCAPTCHA token received:", token);
    if (!event) {
        console.warn("Event is null, attempting fallback to lastClickedButtonEvent.");
        event = window.lastClickedButtonEvent;
    }
    if (!event || !event.target) {
        console.error("Event or event target is invalid. Cannot proceed:", event);
        return;
    }
    const formId = event.target.getAttribute('data-formid');
    console.log("Form ID extracted:", formId);
    const form = document.getElementById(formId);
    if (!form) {
        console.error("No form found for ID:", formId);
        return;
    }
    const recaptchaInputName = formId === "contactForm"
        ? "gRecaptchaResponseContact"
        : formId === "newsletterForm"
            ? "gRecaptchaResponseNewsletter"
            : formId === "registerForm"
                ? "gRecaptchaResponseRegister"
                : null;
    const recaptchaResponse = form.querySelector(`input[name="${recaptchaInputName}"]`);
    console.log("Hidden input found:", recaptchaResponse);
    if (!recaptchaResponse) {
        console.error("No reCAPTCHA input found with name:", recaptchaInputName);
        return;
    }
    recaptchaResponse.value = token;
    console.log("Submitting form with ID:", formId);
    form.submit();
};
