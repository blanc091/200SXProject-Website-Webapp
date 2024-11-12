document.addEventListener('DOMContentLoaded', function () {
    console.log("DOM fully loaded and parsed");

    const isAuthenticated = document.body.getAttribute('data-is-authenticated') === 'true';
    console.log("Is authenticated: ", isAuthenticated);
    if (!isAuthenticated) {
        const loginBtn = document.createElement('button');
        loginBtn.innerText = 'Login';
        loginBtn.id = 'loginBtn';
        loginBtn.style.position = 'fixed';
        loginBtn.style.top = '200px';
        loginBtn.style.right = '15px';
        loginBtn.style.zIndex = '1000';
        loginBtn.style.padding = '10px 15px';
        loginBtn.style.backgroundColor = 'white';
        loginBtn.style.color = 'blue';
        loginBtn.style.border = 'none';
        loginBtn.style.borderRadius = '5px';
        loginBtn.style.cursor = 'pointer';
        loginBtn.style.fontSize = '10px';
        loginBtn.style.height = '25px';
        loginBtn.style.width = '50px';
        loginBtn.style.boxShadow = '0 2px 5px rgba(0, 0, 0, 0.2)';
        loginBtn.style.transition = 'opacity 0.5s ease';
        loginBtn.style.opacity = '1';
        loginBtn.style.textAlign = 'center';
        loginBtn.style.display = 'flex';
        loginBtn.style.alignItems = 'center';
        loginBtn.style.justifyContent = 'center';
        loginBtn.style.backgroundColor = 'rgba(255, 255, 255, 0.5)';

        document.body.appendChild(loginBtn);

        loginBtn.addEventListener('click', function () {
            const returnUrl = encodeURIComponent(window.location.pathname + window.location.search);
            window.location.href = '/LoginRegister/Login?returnUrl=' + returnUrl;
        });
    }
});
