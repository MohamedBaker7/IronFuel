

$('#QuantityInput').on('change', function () {
    const quantity = $(this).val();
    const itemSKU = $(this).data('item-sku');

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
                location.reload();
                $('#subtotal').text(response.sub_total);
                $('#cart-total').text(response.cart_total);
            }
        },
        error: function (xhr) {
            console.error('Update failed:', xhr.responseText);
        }
    });
});

$('.js-remove-item').on('click', function () {
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
                //$('#cart-total').text(response.cart_total);
            }
        },
        error: function (xhr) {
            console.error('Remove failed:', xhr.responseText);
        }
    });
});