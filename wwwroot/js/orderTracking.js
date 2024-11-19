document.addEventListener('DOMContentLoaded', function () {
    document.querySelectorAll('.save-btn').forEach(button => {
        button.addEventListener('click', function (e) {
            e.preventDefault();

            const row = this.closest('tr');
            const orderId = row.dataset.orderId;
            const status = row.querySelector('.status-input').value;
            const carrier = row.querySelector('.carrier-input').value;
            const trackingNumber = row.querySelector('.tracking-number-input').value;

            fetch('/PendingOrders/UpdateOrderTrackingAjax', {
                method: 'POST',
                headers: {
                    'Content-Type': 'application/json',
                    'RequestVerificationToken': document.querySelector('input[name="__RequestVerificationToken"]').value
                },
                body: JSON.stringify({
                    orderId: orderId,
                    status: status,
                    carrier: carrier,
                    trackingNumber: trackingNumber
                })
            })
                .then(response => {
                    if (response.ok) {
                        alert('Order tracking updated successfully!');
                    } else {
                        return response.text().then(text => {
                            throw new Error(text);
                        });
                    }
                })
                .catch(error => {
                    alert('Error updating order tracking: ' + error.message);
                });
        });
    });
});

document.addEventListener("DOMContentLoaded", () => {
    const cartItemsPopup = document.getElementById("cartItemsPopup");
    const cartItemsTable = document.getElementById("cartItemsTable");
    const closePopup = document.getElementById("closePopup");

    // Open the popup when "View Cart Items" button is clicked
    document.querySelectorAll(".view-cart-btn").forEach((button) => {
        button.addEventListener("click", async (event) => {
            const orderId = event.target.dataset.orderId;

            // Fetch cart items via AJAX
            try {
                const response = await fetch(`/PendingOrders/GetCartItems?orderId=${orderId}`);
                if (response.ok) {
                    const cartItems = await response.json();

                    // Populate the cart items table
                    cartItemsTable.innerHTML = cartItems
                        .map(
                            (item) => `
                        <tr>
                            <td>${item.productName}</td>
                            <td>${item.quantity}</td>
                            <td>${item.price}</td>
                        </tr>`
                        )
                        .join("");

                    // Show the popup
                    cartItemsPopup.style.display = "block";
                } else {
                    alert("Failed to fetch cart items. Please try again.");
                }
            } catch (error) {
                console.error("Error fetching cart items:", error);
                alert("An error occurred. Please try again.");
            }
        });
    });

    // Close the popup when "Close" button is clicked
    closePopup.addEventListener("click", () => {
        cartItemsPopup.style.display = "none";
    });
});
