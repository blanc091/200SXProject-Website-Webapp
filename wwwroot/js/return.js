﻿initialize();

async function initialize() {
  const queryString = window.location.search;
  const urlParams = new URLSearchParams(queryString);
  const sessionId = urlParams.get('session_id');
  const response = await fetch(`/session-status?session_id=${sessionId}`);
  const session = await response.json();

  if (session.status == 'open') {
      //window.replace('https://200sxproject.azurewebsites.net/Stripe/StripeProcessor')
      window.replace('https://localhost:7109/Stripe/StripeProcessor') 
  } else if (session.status == 'complete') {
    document.getElementById('success').classList.remove('hidden');
    document.getElementById('customer-email').textContent = session.customer_email
  }
}