const stripe = Stripe("pk_test_51QKz9AKYqPJ7nhalLLU4WJJH8e6rB4GdUyjRwL6TfFCFeo23EEFSuFKwH8xNGqcSsy8cFEEy8SX9RX7M48kI8Qbm00v8uInVlQ");

initialize();

// Create a Checkout Session
async function initialize() {
    const fetchClientSecret = async () => {
        const response = await fetch("/create-checkout-session", {
            method: "POST",
        });
        const { clientSecret } = await response.json();
        return clientSecret;
    };

    const checkout = await stripe.initEmbeddedCheckout({
        fetchClientSecret,
    });

    // Mount Checkout
    checkout.mount('#checkout');
}