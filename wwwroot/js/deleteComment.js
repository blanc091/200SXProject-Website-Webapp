document.addEventListener('DOMContentLoaded', function () {
    const deleteLinks = document.querySelectorAll('.delete-comment-link');
    deleteLinks.forEach(link => {
        link.addEventListener('click', function (event) {
            event.preventDefault();
            const form = link.closest('.delete-comment-form');
            if (confirm('Are you sure you want to delete this comment ? This cannot be undone.')) {
                form.submit();
            }
        });
    });
});