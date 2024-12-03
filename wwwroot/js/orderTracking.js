document.addEventListener('DOMContentLoaded', function () {
    document.querySelectorAll('.save-btn').forEach(button => {
        button.addEventListener('click', function (e) {
            e.preventDefault();
            const row = this.closest('tr');
            if (!row) {
                console.error('Row not found');
                return; 
            }
            const orderId = row.dataset.orderId;
            const status = row.querySelector('.status-input').value;
            const carrier = row.querySelector('.carrier-input').value;
            const trackingNumber = row.querySelector('.tracking-number-input').value;

            fetch('/pendingorders/update-order-tracking', {
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
    document.querySelectorAll(".view-cart-btn").forEach((button) => {
        button.addEventListener("click", async (event) => {
            const orderId = event.target.dataset.orderId;
            try {
                const response = await fetch(`/pendingorders/get-cart-items?orderId=${orderId}`);
                if (response.ok) {
                    const cartItems = await response.json();
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

    closePopup.addEventListener("click", () => {
        cartItemsPopup.style.display = "none";
    });
});
