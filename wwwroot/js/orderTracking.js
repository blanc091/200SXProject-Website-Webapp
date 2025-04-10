document.addEventListener('click', function (e) {
	
   if (e.target.matches('.save-btn') && e.target.id !== 'closePopup') {
        e.preventDefault();
        const row = e.target.closest('tr');
		
        if (!row) {
            console.error('Row not found');
            return;
        }
		
        const orderId = row.dataset.orderId;
        const status = row.querySelector('.status-input').value;
        const carrier = row.querySelector('.carrier-input').value;
        const email = row.querySelector('.email-cell')
            ? row.querySelector('.email-cell').textContent.trim()
            : "";
        const trackingNumber = row.querySelector('.tracking-number-input').value;

        fetch('/pendingorders/update-order-tracking', {
            method: 'POST',
            headers: {
                'Content-Type': 'application/json',
                'RequestVerificationToken': document.querySelector('input[name="__RequestVerificationToken"]').value
            },
            body: JSON.stringify({
                orderId: orderId,
                email: email,
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
                alert(error.message);
            });
    }

    if (e.target.matches('.view-cart-btn')) {
        const orderId = e.target.dataset.orderId;
        (async () => {
            try {
                const response = await fetch(`/pendingorders/get-cart-items?orderId=${orderId}`);
				
                if (response.ok) {
                    const cartItems = await response.json();
                    const cartItemsTable = document.getElementById("cartItemsTable");
                    const cartItemsPopup = document.getElementById("cartItemsPopup");
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
        })();
    }
	
    if (e.target.matches('.view-details-btn')) {
        const orderId = e.target.dataset.orderId;
        fetch(`/pendingorders/get-user-orders-json?orderId=${orderId}`)
            .then(response => {
                if (response.ok) return response.json();
                throw new Error("Error fetching order details");
            })
            .then(orderDetails => {
				console.log("Received order details:", orderDetails);
				let detailsHtml = '';
				
				let orderData = Array.isArray(orderDetails) ? orderDetails[0] : orderDetails;
				let placements = orderData.orderPlacements || orderData.OrderPlacements;
	
				 if (placements && placements.length > 0) {
					const placement = placements[0];
					
					let formattedDate = '';
					let rawDate = placement.orderDate || placement.OrderDate;
					if (rawDate) {
						const dateObj = new Date(rawDate);
						const day = dateObj.getDate().toString().padStart(2, '0');
						const month = (dateObj.getMonth() + 1).toString().padStart(2, '0');
						const year = dateObj.getFullYear();
						const hours = dateObj.getHours().toString().padStart(2, '0');
						const minutes = dateObj.getMinutes().toString().padStart(2, '0');
						const seconds = dateObj.getSeconds().toString().padStart(2, '0');
						formattedDate = `${day}/${month}/${year} ${hours}:${minutes}:${seconds}`;
					}
					
					detailsHtml += `<p><strong>Full Name:</strong> ${placement.fullName || placement.FullName}</p>`;
					detailsHtml += `<p><strong>Address Line 1:</strong> ${placement.addressLine1 || placement.AddressLine1}</p>`;
					detailsHtml += `<p><strong>Address Line 2:</strong> ${placement.addressLine2 || placement.AddressLine2}</p>`;
					detailsHtml += `<p><strong>City:</strong> ${placement.city || placement.City}</p>`;
					detailsHtml += `<p><strong>County:</strong> ${placement.county || placement.County}</p>`;
					detailsHtml += `<p><strong>Phone Number:</strong> ${placement.phoneNumber || placement.PhoneNumber}</p>`;
					detailsHtml += `<p><strong>Email:</strong> ${placement.email || placement.Email}</p>`;
					detailsHtml += `<p><strong>Order Notes:</strong> ${placement.orderNotes || placement.OrderNotes}</p>`;
					detailsHtml += `<p><strong>Order Date:</strong> ${formattedDate}</p>`;
				} else {
					detailsHtml = '<p>No order placement details found.</p>';
				}

                document.getElementById('orderDetailsContent').innerHTML = detailsHtml;
                document.getElementById('orderDetailsPopup').style.display = 'block';
            })
            .catch(error => {
                console.error("Error loading order details:", error);
                alert("Error loading order details: " + error.message);
            });
    }

    if (e.target.matches('#closePopup')) {
        document.getElementById('cartItemsPopup').style.display = 'none';
    }
	
    if (e.target.matches('#closeOrderDetailsPopup')) {
        document.getElementById('orderDetailsPopup').style.display = 'none';
    }
});
