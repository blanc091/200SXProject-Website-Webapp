    document.addEventListener('DOMContentLoaded', function () {
    const cartIcon = document.createElement('div');
    cartIcon.id = 'cart-icon';
    cartIcon.style.position = 'fixed';
    cartIcon.style.bottom = '20px';
    cartIcon.style.right = '20px';
    cartIcon.style.width = '50px';
    cartIcon.style.height = '50px';
    cartIcon.style.backgroundColor = '#bdbbbf';
    cartIcon.style.borderRadius = '50%';
    cartIcon.style.display = 'flex';
    cartIcon.style.justifyContent = 'center';
    cartIcon.style.alignItems = 'center';
    cartIcon.style.cursor = 'pointer';
    cartIcon.style.zIndex = '1000';
    cartIcon.style.boxShadow = '0 2px 5px rgba(0, 0, 0, 0.2)';

    const cartImage = document.createElement('img');
    cartImage.src = '/images/carticon.png';
    cartImage.alt = 'Cart';
    cartImage.style.width = '75%';
    cartImage.style.height = '60%';
    cartIcon.appendChild(cartImage);

    const cartCount = document.createElement('span');
    cartCount.id = 'cart-count';
    cartCount.innerText = '0';
    cartCount.style.position = 'absolute';
    cartCount.style.top = '-5px';
    cartCount.style.right = '-5px';
    cartCount.style.backgroundColor = 'red';
    cartCount.style.color = 'white';
    cartCount.style.fontSize = '12px';
    cartCount.style.borderRadius = '50%';
    cartCount.style.padding = '2px 5px';
    cartIcon.appendChild(cartCount);

    document.body.appendChild(cartIcon);

    fetch('/Cart/GetCartItemCount')
        .then(response => response.json())
        .then(data => {
        cartCount.innerText = data;
        })
        .catch(error => console.error('Error fetching cart count:', error));

    cartIcon.addEventListener('click', function () {
        window.location.href = '/Cart/CartView';
    });
});
