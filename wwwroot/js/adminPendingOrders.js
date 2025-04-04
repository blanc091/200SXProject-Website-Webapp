$(document).ready(function () {
    console.log("Admin dashboard Orders JS loaded (custom popup version).");

    // Bind click event for opening the orders modal on button click
    $('#openAllOrders').click(function () {
        console.log("openAllOrders button clicked.");
        $('#ordersModal').show();
        loadOrderList();
    });

    // Bind click event for closing the orders modal
    $('#closeOrdersModal').click(function () {
        console.log("closeOrdersModal button clicked.");
        $('#ordersModal').hide();
    });

    function loadOrderList() {
        console.log("Attempting to load order list.");
        var ordersApiUrl = $('#orders-api-url').data('orders-api-url');
        if (!ordersApiUrl) {
            console.error("Orders API URL not found.");
            return;
        }
        console.log("Orders API URL:", ordersApiUrl);

        $.ajax({
            url: ordersApiUrl,
            method: 'GET',
            dataType: 'json',
            headers: {
                'X-Requested-With': 'XMLHttpRequest'
            },
            success: function (orders) {
                console.log("Orders received:", orders);
                var container = $('#ordersContent');
                if (!orders || orders.length === 0) {
                    container.html('<p>No orders found.</p>');
                    console.log("No orders found.");
                    return;
                }
                var html = '<table id="orderTable" class="table table-striped">';
                html += '<thead><tr>';
                html += '<th>Order ID</th><th>Email</th><th>Status</th><th>Carrier</th><th>Tracking Number</th><th>Action</th>';
                html += '</tr></thead><tbody>';
                $.each(orders, function (index, order) {
                    console.log("Rendering order:", order);
					html += '<tr data-order-id="' + order.orderId + '">';
					html += '<td>' + order.orderId + '</td>';
					html += '<td class="email-cell">' + order.email + '</td>';
					html += '<td><input type="text" class="status-input" value="' + order.status + '" /></td>';
					html += '<td><input type="text" class="carrier-input" value="' + order.carrier + '" /></td>';
					html += '<td><input type="text" class="tracking-number-input" value="' + order.trackingNumber + '" /></td>';
					html += '<td><button class="save-btn">Save</button></td>';
					html += '<td><button class="view-cart-btn" data-order-id="' + order.orderId + '">Cart Items</button></td>';
					html += '<td><button class="view-details-btn" data-order-id="' + order.orderId + '">Order Details</button></td>';
                    html += '</tr>';
                });
                html += '</tbody></table>';
                container.html(html);
                console.log("Order list rendered successfully.");
            },
            error: function (xhr, status, error) {
                console.error("Error loading order list:", error);
                $('#ordersContent').html('<p>Error loading orders.</p>');
            }
        });
    }
});
