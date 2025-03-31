$(document).ready(function () {
    console.log("Admin dashboard JS loaded (custom popup version).");

    $('#openProductModal').click(function () {
        console.log("openProductModal button clicked.");
        $('#productModal').show();
        loadProductList();
    });

    $('#closeProductModal').click(function () {
        console.log("closeProductModal button clicked.");
        $('#productModal').hide();
    });

    function loadProductList() {
        console.log("Attempting to load product list.");
        var productsApiUrl = $('#products-api-url').data('products-api-url');
        if (!productsApiUrl) {
            console.error("Products API URL not found.");
            return;
        }
        console.log("Products API URL:", productsApiUrl);

        $.ajax({
            url: productsApiUrl,
            method: 'GET',
            dataType: 'json',
            headers: {
                'X-Requested-With': 'XMLHttpRequest'
            },
            success: function (products) {
                console.log("Products received:", products);
                var container = $('#productListContainer');
                if (!products || products.length === 0) {
                    container.html('<p>No products found.</p>');
                    console.log("No products found.");
                    return;
                }
                var html = '<table class="table table-striped">';
                html += '<thead><tr><th>ID</th><th>Category</th><th>Name</th><th>Price</th><th>Action</th></tr></thead><tbody>';
                $.each(products, function (index, product) {
                    console.log("Rendering product:", product);
                    html += '<tr>';
                    html += '<td>' + product.id + '</td>';
					html += '<td>' + product.category + '</td>';
                    html += '<td>' + product.name + '</td>';
					html += '<td>' + product.price + '</td>';
                    html += '<td><button class="btn-delete-product" data-product-id="' + product.id + '">Delete</button></td>';
                    html += '</tr>';
                });
                html += '</tbody></table>';
                container.html(html);
                console.log("Product list rendered successfully.");
            },
            error: function (xhr, status, error) {
                console.error("Error loading product list:", error);
                $('#productListContainer').html('<p>Error loading products.</p>');
            }
        });
    }

    $('#productListContainer').on('click', '.btn-delete-product', function () {
        var productId = $(this).data('product-id');
        console.log("Delete button clicked for product ID:", productId);
        if (!productId) {
            console.error("No product ID found on delete button.");
            return;
        }
        if (confirm("Are you sure you want to delete product " + productId + "?")) {
            console.log("User confirmed deletion for product ID:", productId);
            deleteProduct(productId);
        } else {
            console.log("User canceled deletion for product ID:", productId);
        }
    });

    function deleteProduct(productId) {
        console.log("Initiating deletion for product ID:", productId);
        var productDeleteUrl = $('#product-delete-url').data('product-delete-url');
        if (!productDeleteUrl) {
            console.error("Product delete URL not found.");
            return;
        }
        console.log("Product delete URL:", productDeleteUrl);
        
        var token = $('input[name="__RequestVerificationToken"]').val() || "";
        console.log("Anti-forgery token retrieved:", token);

        var data = {
            productId: productId,
            __RequestVerificationToken: token
        };
        console.log("Data payload for deletion:", data);

        $.ajax({
            url: productDeleteUrl,
            method: 'POST',
            data: $.param(data),
            headers: {
                'X-Requested-With': 'XMLHttpRequest'
            },
            success: function (result) {
                console.log("Delete result:", result);
                if (result.success) {
                    alert("Product deleted successfully.");
                    loadProductList();
                } else {
                    alert("Error deleting product: " + (result.errors ? result.errors.join(", ") : "unknown error"));
                }
            },
            error: function (xhr, status, error) {
                console.error("Error during product deletion:", error);
            }
        });
    }
});
