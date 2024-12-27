document.addEventListener("DOMContentLoaded", function () {
    const isMobile = /Mobi|Android/i.test(navigator.userAgent);
    const desktopOnlyDiv = document.querySelector(".desktop-only");

    if (isMobile) {
        if (desktopOnlyDiv) {
            const mobileMessage = document.createElement("p");
            mobileMessage.textContent = "Note : On mobile devices, logging in via Microsoft is disabled currently !";
            mobileMessage.style.textAlign = "center";
            mobileMessage.style.fontSize = "14px";
            mobileMessage.style.margin = "10px 0";
            desktopOnlyDiv.replaceWith(mobileMessage);
        }
    }
});

