document.addEventListener("DOMContentLoaded", function () {
    const swiper = new Swiper('.swiper', {
        modules: [EffectCarousel],
        effect: 'carousel',
        carouselEffect: {
            opacityStep: 0.33,
            scaleStep: 0.2,
            sideSlides: 2,
        },
        grabCursor: true,
        loop: true,
        loopAdditionalSlides: 1,
        slidesPerView: 'auto',
        navigation: {
            nextEl: '.swiper-button-next',
            prevEl: '.swiper-button-prev',
        },
        pagination: {
            el: '.swiper-pagination',
        },

        autoplay: {
            delay: 5000,
        },
    });

});