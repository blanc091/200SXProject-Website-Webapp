function scrollToElement(elementId) {
    var element = document.getElementById(elementId);
    if (element) {
        element.scrollIntoView({ behavior: 'smooth' });
    } else {
        console.error(`${elementId} not found!`);
    }
}
document.addEventListener("DOMContentLoaded", () => {
    const deleteLinks = document.querySelectorAll(".delete-comment-link");

    deleteLinks.forEach(link => {
        link.addEventListener("click", (event) => {
            event.preventDefault();
            const form = link.closest("form");
            if (form) {
                form.submit();
            }
        });
    });
});
document.addEventListener("DOMContentLoaded", function () {
    var itemAddedToCart = document.body.getAttribute('data-item-added');
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
    var commentSubmitted = document.body.getAttribute('data-is-comment-posted');
    var unsubscribed = document.body.getAttribute('data-unsubscribed'); 
    var userDeleted = document.body.getAttribute('data-is-user-deleted');

    console.log("Current Pathname:", window.location.pathname);
    if (window.location.pathname.startsWith('/detailed-user-build')) {
        scrollToElement("detailedViewSection");
    }
    if (window.location.pathname.startsWith('/products/detailed-product-view')) {
        scrollToElement("detailedProductViewSection");
    }
    if (window.location.pathname.startsWith('/checkout/your-order')) {
        scrollToElement("orderSuccess");
    }
    switch (window.location.pathname) {
        case '/mainten-app':
            scrollToElement("maintenApp");
            break;
        case '/login-page':
            scrollToElement("header");
            break;
        case '/reset-password':
        case '/forgot-my-password':
        case '/reset-my-password':
        case '/newsletter/create-newsletter-admin':
            scrollToElement("maintenApp");
            break;
        case '/register-new-account':
            scrollToElement("registerFormSpan");
            break;
        case '/newsletter/subscribe':
            scrollToElement("newsletterFormSpan");
            break;
        case '/add-new-build':
            scrollToElement("addBuild");
            break;
        case '/user-builds':
            scrollToElement("userContentDash");
            break;
        case '/products/view-products':
            scrollToElement("productsDash");
            break;
        case '/cart/view-cart':
            scrollToElement("cartView");
            break;
        case '/checkout/view-checkout':
            scrollToElement("checkoutView");
            break;
        case '/pendingorders/get-all-orders-admin':
            scrollToElement("ordersAll");
            break;
        case '/account/user-profile':
            scrollToElement("userProfileDash");
            break;
        case '/pendingorders/view-my-orders':
            scrollToElement("customerOrders");
            break;
        case '/cart/remove-cart-item':
            scrollToElement("cartView");
            break;
        case '/account/delete-account-confirmation':
            scrollToElement("userProfileDashDelete");
            break;
		case '/account/admin-dashboard':
            scrollToElement("adminInterfaceHeader");
            break;
		case '/products/add-product-interface':
            scrollToElement("addProduct");
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
        commentSubmitted == "yes" ||
        isUserLoggedIn == "no" ||
        unsubscribed == "yes" ||
        userDeleted == 'yes' ||
        itemAddedToCart == "yes" ||
		isNewsletterError == "yes" ||
        passResetEmailSent == "yes") {
        setTimeout(function () {
            $('#messageModal').fadeOut(1250);
        }, 3000);
    }    
    if (formSubmitted && !formSuccess) {
        scrollToContactForm();
    }
    if (window.location.pathname != '/account/login-account') {
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
    fetch('/api/is-logged-in')
        .then(response => response.json())
        .then(isUserLoggedIn => {
            if (isUserLoggedIn && window.location.pathname === '/') {
                scrollToElement("posts");
                $('#tibi').css("display", "flex");
            }
        });
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
                if (window.location.pathname === '/home/index') {
                    scrollToContactForm();
                } else {
                    window.location.href = '/home/index?scrollToContactForm=true';
                }
            });
        }
    });
});
