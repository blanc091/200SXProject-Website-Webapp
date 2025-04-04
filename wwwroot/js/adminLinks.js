document.addEventListener('DOMContentLoaded', function() {
  const buttons = document.querySelectorAll('.btn-save');
  console.log('Found buttons:', buttons);

  buttons.forEach(function(button) {
    if (button.id === 'togglePendingChats') {
      console.log('Skipping button:', button.id);
      return;
    }

    button.addEventListener('click', function() {
      const targetUrl = button.getAttribute('data-href');
      console.log('Button clicked, target:', targetUrl);
      if (targetUrl) {
        window.location.href = targetUrl;
      } else {
        console.error('No data-href found on button:', button);
      }
    });
  });
});
