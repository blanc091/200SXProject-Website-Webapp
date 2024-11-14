function loadPayPalSDK(nonce) {
    const script = document.createElement("script");
    script.src = "https://www.paypal.com/sdk/js?client-id=Adw9NCrTE-oQzVZMfbwC_0YpUl6EC6au7VEkyy25xYhqmffo52-G_XsyE52UduS6CKzR0gVxqFhZCA5t&buyer-country=US&currency=EUR&components=buttons&enable-funding=card&disable-funding=venmo,paylater";
    script.setAttribute("data-sdk-integration-source", "developer-studio");
    if (nonce) {
        script.setAttribute("nonce", nonce);
    }
    script.onload = () => {
        window.paypal
            .Buttons({
                style: {
                    shape: "pill",
                    layout: "vertical",
                    color: "silver",
                    label: "pay",
                },

                async createOrder() {
                    try {
                        const response = await fetch("/api/orders", {
                            method: "POST",
                            headers: {
                                "Content-Type": "application/json",
                            },
                            // use the "body" param to optionally pass additional order information
                            // like product ids and quantities
                            body: JSON.stringify({
                                cart: [
                                    {
                                        id: "YOUR_PRODUCT_ID",
                                        quantity: "YOUR_PRODUCT_QUANTITY",
                                    },
                                ],
                            }),
                        });

                        const orderData = await response.json();

                        if (orderData.id) {
                            return orderData.id;
                        }
                        const errorDetail = orderData?.details?.[0];
                        const errorMessage = errorDetail
                            ? `${errorDetail.issue} ${errorDetail.description} (${orderData.debug_id})`
                            : JSON.stringify(orderData);

                        throw new Error(errorMessage);
                    } catch (error) {
                        console.error(error);
                        // resultMessage(`Could not initiate PayPal Checkout...<br><br>${error}`);
                    }
                },

                async onApprove(data, actions) {
                    try {
                        const response = await fetch(
                            `/api/orders/${data.orderID}/authorize`,
                            {
                                method: "POST",
                                headers: {
                                    "Content-Type": "application/json",
                                },
                            }
                        );

                        const orderData = await response.json();
                        // Three cases to handle:
                        //   (1) Recoverable INSTRUMENT_DECLINED -> call actions.restart()
                        //   (2) Other non-recoverable errors -> Show a failure message
                        //   (3) Successful transaction -> Show confirmation or thank you message

                        const errorDetail = orderData?.details?.[0];

                        if (errorDetail?.issue === "INSTRUMENT_DECLINED") {
                            // (1) Recoverable INSTRUMENT_DECLINED -> call actions.restart()
                            // recoverable state, per
                            // https://developer.paypal.com/docs/checkout/standard/customize/handle-funding-failures/
                            return actions.restart();
                        } else if (errorDetail) {
                            // (2) Other non-recoverable errors -> Show a failure message
                            throw new Error(
                                `${errorDetail.description} (${orderData.debug_id})`
                            );
                        } else if (!orderData.purchase_units) {
                            throw new Error(JSON.stringify(orderData));
                        } else {
                            // (3) Successful transaction -> Show confirmation or thank you message
                            // Or go to another URL:  actions.redirect('thank_you.html');
                            const transaction =
                                orderData?.purchase_units?.[0]?.payments
                                    ?.captures?.[0] ||
                                orderData?.purchase_units?.[0]?.payments
                                    ?.authorizations?.[0];
                            resultMessage(
                                `Transaction ${transaction.status}: ${transaction.id}<br>
          <br>See console for all available details`
                            );
                            console.log(
                                "Capture result",
                                orderData,
                                JSON.stringify(orderData, null, 2)
                            );
                        }
                    } catch (error) {
                        console.error(error);
                        resultMessage(
                            `Sorry, your transaction could not be processed...<br><br>${error}`
                        );
                    }
                },

                onCancel: (data) => {
                    // Show a cancel page or return to cart
                    window.location.assign("/your-error-page-here");
                },
            })
            .render("#paypal-button-container"); 
    };
    document.head.appendChild(script);
}

