document.addEventListener('DOMContentLoaded', function () {
    const isMobile = window.innerWidth <= 768; 

    if (isMobile) {
        const spans = document.querySelectorAll('.user-info span');
        spans.forEach(function (span) {
            span.style.display = 'block';
            const br = document.createElement('br');
            span.parentNode.insertBefore(br, span.nextSibling);

        });
    }
});