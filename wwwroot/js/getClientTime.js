document.addEventListener("DOMContentLoaded", function () {
        const userTimeZone = Intl.DateTimeFormat().resolvedOptions().timeZone;
        document.cookie = `userTimeZone=${userTimeZone}; path=/`;
    });