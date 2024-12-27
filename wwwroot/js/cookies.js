document.addEventListener('DOMContentLoaded', function () {
    var modal = document.getElementById('cookieModal');
    //localStorage.removeItem('cookieConsent');
    if (modal) {
        modal.style.display = 'none';
        document.body.classList.remove('modal-open');
    }
    var userChoice = localStorage.getItem('cookieConsent');
    if (userChoice !== 'accepted' && userChoice !== 'denied') {
        if (modal) {
            modal.style.display = 'block';
            document.body.classList.add('modal-open');
        }
    }
    document.getElementById('acceptCookies').addEventListener('click', function () {
        localStorage.setItem('cookieConsent', 'accepted');
        loadGoogleAnalytics();
        loadGoogleAds();
        console.log('Google Analytics and Ads scripts appended to head');
        if (modal) {
            modal.style.display = 'none';
            document.body.classList.remove('modal-open');
        }
        localStorage.setItem('scrollToTopAfterReload', 'true');
        location.reload();

    });
    window.onload = function () {
        if (localStorage.getItem('scrollToTopAfterReload') === 'true') {
            setTimeout(() => {
                window.focus();
                window.scrollTo(0, 0);
            }, 100); 
            localStorage.removeItem('scrollToTopAfterReload');
        }
    };
    document.getElementById('denyCookies').addEventListener('click', function () {
        localStorage.setItem('cookieConsent', 'denied');
        console.log('User denied cookie consent');
        if (modal) {
            modal.style.display = 'none';
            document.body.classList.remove('modal-open');
        }
        location.reload();
    });
});

function loadGoogleAnalytics() {
    if (!document.getElementById('google-analytics-script')) {
        const script = document.createElement('script');
        script.id = 'google-analytics-script';
        script.src = 'https://www.googletagmanager.com/gtag/js?id=G-6TGFNLDLZ5';
        script.async = true;

        // Add script to head and log confirmation once loaded
        script.onload = function () {
            console.log('Google Analytics script loaded');
            console.log(document.getElementById('google-analytics-script'));
        };

        document.head.appendChild(script);
    } else {
        console.log('Google Analytics script already loaded');
    }
}
function loadGoogleAds() {
    var adsScript = document.createElement('script');
    adsScript.src = 'https://pagead2.googlesyndication.com/pagead/js/adsbygoogle.js?client=ca-pub-8385228399742318';
    adsScript.async = true;
    adsScript.crossOrigin = 'anonymous';
    document.head.appendChild(adsScript);
    console.log('Google Ads script loaded');
}

function removeGoogleAnalytics() {
    var gtagScript = document.querySelector('script[src="https://www.googletagmanager.com/gtag/js?id=G-6TGFNLDLZ5"]');
    if (gtagScript) {
        gtagScript.parentNode.removeChild(gtagScript);
    }

    var inlineScript = document.querySelector('script[data-gtag-init]');
    if (inlineScript) {
        inlineScript.parentNode.removeChild(inlineScript);
    }

    if (window.dataLayer) {
        window.dataLayer = [];
    }
}
function removeGoogleAds() {
    var adsScript = document.querySelector('script[src="https://pagead2.googlesyndication.com/pagead/js/adsbygoogle.js?client=ca-pub-8385228399742318"]');
    if (adsScript) {
        adsScript.parentNode.removeChild(adsScript);
    }
}

