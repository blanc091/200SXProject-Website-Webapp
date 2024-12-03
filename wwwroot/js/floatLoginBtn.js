document.addEventListener('DOMContentLoaded', function () {
    console.log("DOM fully loaded and parsed");
    const isAuthenticated = document.body.getAttribute('data-is-authenticated') === 'true';
    console.log("Is authenticated: ", isAuthenticated);
    const button = document.createElement('button');
    button.id = 'tibi';
    button.style.position = 'fixed';
    button.style.top = '200px';
    button.style.right = '15px';
    button.style.zIndex = '1000';
    button.style.padding = '10px 15px';
    button.style.backgroundColor = 'white';
    button.style.color = 'blue';
    button.style.border = 'none';
    button.style.borderRadius = '5px';
    button.style.cursor = 'pointer';
    button.style.fontSize = '10px';
    button.style.height = '25px';
    button.style.width = '50px';
    button.style.boxShadow = '0 2px 5px rgba(0, 0, 0, 0.2)';
    button.style.transition = 'opacity 0.5s ease';
    button.style.opacity = '1';
    button.style.textAlign = 'center';
    button.style.alignItems = 'center';
    button.style.justifyContent = 'center';
    button.style.backgroundColor = 'rgba(255, 255, 255, 0.5)';

    if (isAuthenticated) {
        button.innerText = 'Profile';
        button.addEventListener('click', function () {
            window.location.href = '/account/user-profile';
        });
    } else {
        button.innerText = 'Login';
        button.addEventListener('click', function () {
            const returnUrl = encodeURIComponent(window.location.pathname + window.location.search);
            window.location.href = '/login-page?returnUrl=' + returnUrl;
        });
    }
    const isMobile = window.innerWidth <= 768;
    if (isMobile) {
        button.style.display = 'flex';
    } else {
        button.style.display = 'none';
    }
    document.body.appendChild(button);

    window.addEventListener('scroll', function () {
        const scrollTop = window.scrollY || document.documentElement.scrollTop;
        const documentHeight = document.documentElement.scrollHeight;
        const windowHeight = window.innerHeight;

        if (scrollTop > (documentHeight - windowHeight) * 0.05) {
            if (button.style.opacity === '0' || !button.style.opacity) {
                button.style.display = 'flex';
                setTimeout(() => {
                    button.style.opacity = '1';
                }, 10);
            }
        } else {
            if (!isMobile) {
                if (button.style.opacity === '1') {
                    button.style.opacity = '0';
                    setTimeout(() => {
                        button.style.display = 'none';
                    }, 500);
                }
            }
        }
    });
});
