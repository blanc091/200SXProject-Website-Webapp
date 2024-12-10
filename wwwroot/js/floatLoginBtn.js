document.addEventListener('DOMContentLoaded', function () {
    const isWebView = /WebView|wv/i.test(navigator.userAgent);
    const isMobile = window.innerWidth <= 768 || /Mobi|Android|iPhone|iPad|iPod/i.test(navigator.userAgent) || isWebView;
    const isAuthenticated = document.body.getAttribute('data-is-authenticated') === 'true';
    const button = document.createElement('button');
    button.id = 'tibi';
    Object.assign(button.style, {
        position: 'fixed',
        top: '200px',
        right: '15px',
        zIndex: '1000',
        padding: '10px 15px',
        backgroundColor: 'rgba(255, 255, 255, 0.5)',
        color: 'blue',
        border: 'none',
        borderRadius: '5px',
        cursor: 'pointer',
        fontSize: '10px',
        height: '25px',
        width: '50px',
        boxShadow: '0 2px 5px rgba(0, 0, 0, 0.2)',
        transition: 'opacity 0.5s ease',
        opacity: '0',
        visibility: 'hidden',
        textAlign: 'center',
        display: 'none',
        alignItems: 'center',
        justifyContent: 'center',
    });
    button.innerText = isAuthenticated ? 'Profile' : 'Login';
    button.addEventListener('click', function () {
        if (isAuthenticated) {
            window.location.href = '/account/user-profile';
        } else {
            const returnUrl = encodeURIComponent(window.location.pathname + window.location.search);
            window.location.href = '/login-page?returnUrl=' + returnUrl;
        }
    });
    document.body.appendChild(button);
    let lastScrollTop = window.scrollY || document.documentElement.scrollTop;
    function updateButtonVisibility() {
        const scrollTop = window.scrollY || document.documentElement.scrollTop;
        const documentHeight = document.documentElement.scrollHeight;
        const windowHeight = window.innerHeight;

        console.log({
            isMobile,
            isWebView,
            scrollTop,
            documentHeight,
            windowHeight,
        });

        const shouldShowButton = scrollTop > (documentHeight - windowHeight) * 0.05;
        if (shouldShowButton) {
            if (button.style.visibility === 'hidden') {
                button.style.display = 'flex';
                setTimeout(() => {
                    button.style.opacity = '1';
                    button.style.visibility = 'visible';
                }, 10);
            }
        } else {
            if (!isMobile && button.style.visibility === 'visible') {
                button.style.opacity = '0';
                setTimeout(() => {
                    button.style.visibility = 'hidden';
                    button.style.display = 'none';
                }, 500);
            }
        }
        if (scrollTop < lastScrollTop && scrollTop < (documentHeight - windowHeight) * 0.05 && button.style.visibility === 'visible') {
            button.style.opacity = '0';
            setTimeout(() => {
                button.style.visibility = 'hidden';
                button.style.display = 'none';
            }, 500);
        }
        lastScrollTop = scrollTop;
    }
    updateButtonVisibility();
    window.addEventListener('scroll', updateButtonVisibility);
    setInterval(() => {
        const currentScrollTop = window.scrollY || document.documentElement.scrollTop;
        if (currentScrollTop !== lastScrollTop) {
            updateButtonVisibility();
            lastScrollTop = currentScrollTop;
        }
    }, 250);
});
