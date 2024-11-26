document.addEventListener('DOMContentLoaded', function () {
    const scrollToTopBtn = document.createElement('button');
    scrollToTopBtn.innerText = 'Top';
    scrollToTopBtn.id = 'scrollToTop';
    scrollToTopBtn.style.position = 'fixed';
    scrollToTopBtn.style.bottom = '100px';
    scrollToTopBtn.style.right = '10px';
    scrollToTopBtn.style.display = 'none';
    scrollToTopBtn.style.zIndex = '1000';
    scrollToTopBtn.style.padding = '10px 15px';
    scrollToTopBtn.style.backgroundColor = 'white';
    scrollToTopBtn.style.color = 'blue';
    scrollToTopBtn.style.border = 'none';
    scrollToTopBtn.style.borderRadius = '5px';
    scrollToTopBtn.style.cursor = 'pointer';
    scrollToTopBtn.style.fontSize = '16px';
    scrollToTopBtn.style.height = '30px';
    scrollToTopBtn.style.width = '60px';
    scrollToTopBtn.style.boxShadow = '0 2px 5px rgba(0, 0, 0, 0.2)';
    scrollToTopBtn.style.transition = 'opacity 0.5s ease';
    scrollToTopBtn.style.opacity = '0';
    scrollToTopBtn.style.textAlign = 'center';
    scrollToTopBtn.style.display = 'flex';
    scrollToTopBtn.style.alignItems = 'center';
    scrollToTopBtn.style.justifyContent = 'center';
    document.body.appendChild(scrollToTopBtn);
    window.addEventListener('scroll', function () {
        const scrollTop = window.scrollY || document.documentElement.scrollTop;
        const documentHeight = document.documentElement.scrollHeight;
        const windowHeight = window.innerHeight;

        if (scrollTop > (documentHeight - windowHeight) * 0.25) {
            if (scrollToTopBtn.style.opacity === '0' || !scrollToTopBtn.style.opacity) {
                scrollToTopBtn.style.display = 'flex'; 
                setTimeout(() => {
                    scrollToTopBtn.style.opacity = '1';
                }, 10);
            }
        } else {
            if (scrollToTopBtn.style.opacity === '1') {
                scrollToTopBtn.style.opacity = '0'; 
                setTimeout(() => {
                    scrollToTopBtn.style.display = 'none'; 
                }, 500);
            }
        }
    });
    scrollToTopBtn.addEventListener('click', function () {
        window.scrollTo({ top: 0, behavior: 'smooth' });
    });
});