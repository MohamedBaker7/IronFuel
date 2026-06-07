



$(document).on('change', '.js-cart-quantity', function () {

    const $input = $(this);
    var quantity = $(this).val();
    const itemSKU = $(this).data('item-sku');
    const stock = $(this).data('item-stock');
    quantity = quantity < 1 ? 1 : quantity;

   
    if (quantity > stock) {
        qauntity = stock;
        showErrorMessage('Only ' + stock + ' items in stock');
    }

    $input.val(quantity);

    $.ajax({
        url: '/Cart/UpdateCart',
        method: 'POST',
        data: {
            sku: itemSKU,
            qty: quantity,
            '__RequestVerificationToken': $('input[name="__RequestVerificationToken"]').val()
        },
        success: function (response) {
            if (response.success) {
                $('#subtotal').text(response.sub_total);
                $('#cart-total').text(response.cart_total);
            }
        },
        error: function (xhr) {
            console.error('Update failed:', xhr.responseText);
        }
    });
});




$(document).on('click', '.js-remove-item', function () {
    const itemSKU = $(this).data('item-sku');
    const $row = $(this).closest('tr, .cart-item');


    $.ajax({
        url: '/Cart/RemoveFromCart',
        method: 'POST',
        data: {
            sku: itemSKU,
            '__RequestVerificationToken': $('input[name="__RequestVerificationToken"]').val()
        },
        success: function (response) {
            if (response.success) {

                $row.remove();

                if (response.cart_count == 0)
                    location.reload();
                //$('#cart-count').text(response.cart_count);

                var newPrice = formatPrice(response.cart_total) + ' EGP';
                $('#TotalPrice, #offcanvasTotalPrice').text(newPrice);
            }
        },
        error: function (xhr) {
            console.error('Remove failed:', xhr.responseText);
        }
    });

    
});