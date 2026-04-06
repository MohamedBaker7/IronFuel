function getSizeByFlavour(productId, flavourId) {
    $.ajax({
        url: '/Products/GetSizesByFlavour',
        type: 'GET',
        data: {
            productId: productId,
            flavourId: flavourId
        },
        success: function (data) {

            var sizesDropdown = $('#Sizes');
            sizesDropdown.empty();

            $.each(data, function (i, item) {
                var size = item.size >= 1000 ? `${item.size / 1000} kg` : `${item.size} g`;
                sizesDropdown.append(
                    `<option value="${item.size}">${size} (${Math.round(item.size / item.servingWeight)} Servings)</option>`
                );
            });
        }
    });
}

$(document).on('click', '.thumb-img', function () {
    var src = $(this).attr('src');
    if (src) {
        $('#mainProductImage').attr('src', src);
    }
});

$(document).ready(function () {

    var productId = $('#ProductId').val();
    var flavourId = $('#Flavors').val();

    getSizeByFlavour(productId, flavourId);

    $('#Flavors').on('change', function () {
        flavourId = $(this).val();
        getSizeByFlavour(productId, flavourId);
    });

    var thumbImages = $('.thumb-img');

    thumbImages.on('click', function () {
        var currentImg = $(this);
        thumbImages.removeClass('selectedThumb');
        currentImg.addClass('selectedThumb');
       
    })




});