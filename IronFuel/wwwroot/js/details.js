var allSizes = [];
var servings;

function fillSelectSizes(sizes) {

    allSizes = sizes;

    var sizesDropdown = $('#Sizes');
    sizesDropdown.empty();

    $.each(sizes, function (i, item) {
        var size = item.weightG >= 1000 ? `${item.weightG / 1000} kg` : `${item.weightG} g`;
        sizesDropdown.append(
            `<option value="${item.weightG}" data-serving-size="${item.servingSizeG}">${size} (${Math.round(item.weightG / item.servingSizeG)} Servings)</option>`
        );
    });
}

function getSizesByFlavour(productId, flavourId) {
    $.ajax({
        url: '/Products/GetSizesByFlavour',
        type: 'GET',
        data: {
            productId: productId,
            flavourId: flavourId
        },
        success: function (data) {
            fillSelectSizes(data);
            getProductPrice(productId, flavourId, allSizes[0].weightG);
            refreshSessionStorage(productId, flavourId, allSizes[0].weightG, allSizes[0].servingSizeG);
            servingsPerContainer(allSizes[0].weightG, allSizes[0].servingSizeG);
        }
    });
}

function getProductPrice(productId, flavourId, weightG) {
    $.ajax({
        url: '/Products/GetProductPrice',
        type: 'GET',
        data: {
            productId: productId,
            flavourId: flavourId,
            weightG: weightG
        },
        success: function (data) {
            $('#Price').text(`${data} EGP`);
        }
    });
}

function refreshSessionStorage(productId, flavourId, weightG, servingSizeG) {

    var productDetails = {
        productId: productId,
        flavourId: flavourId,
        weightG: weightG,
        servingSizeG, servingSizeG,
    };

    sessionStorage.setItem('productDetails', JSON.stringify(productDetails));
}

function sizeChangeHandler(productId, flavourId, weightG, servingSizeG) {
    servingsPerContainer(weightG, servingSizeG);
    getProductPrice(productId, flavourId, weightG);
    refreshSessionStorage(productId, flavourId, weightG, servingSizeG);
}

function servingsPerContainer(weightG, servingSizeG) {
    $('#ServingsPerContainer').text(Math.round(weightG / servingSizeG) + ' Servings');
}


$(document).on('click', '.thumb-img', function () {
    var src = $(this).attr('src');
    if (src) {
        $('#mainProductImage').attr('src', src);
    }
});

$(document).ready(function () {

    var productId = parseFloat($('#ProductId').val());
    var flavourId;
    var weightG;
    var servingSizeG;


    var product = JSON.parse(sessionStorage.getItem('productDetails'));

    if (sessionStorage.getItem('productDetails') && productId == product.productId) {

        $('#Flavors').val(product.flavourId);
        $('#Sizes').val(product.weightG);

        getSizesByFlavour(productId, product.flavourId);
        getProductPrice(productId, product.flavourId, product.weightG);
    }
    else {
        flavourId = parseFloat($('#Flavors').val());
        weightG = $('#Sizes').val();
        servingSizeG = $('#Sizes').data('serving-size');

        getSizesByFlavour(productId, flavourId);
        getProductPrice(productId, flavourId, weightG);
    }


    $('#Flavors').on('change', function () {
        flavourId = parseFloat($(this).val());
        getSizesByFlavour(productId, flavourId);
    });

    $('#Sizes').on('change', function () {
        weightG = $(this).val();
        servingSizeG = $(this).find(':selected').data('serving-size');
        sizeChangeHandler(productId, flavourId, weightG, servingSizeG);
    });


    var thumbImages = $('.thumb-img');

    thumbImages.on('click', function () {
        var currentImg = $(this);
        thumbImages.removeClass('selectedThumb');
        currentImg.addClass('selectedThumb');
    });

    if (thumbImages.length > 0) {
        $(thumbImages[0]).addClass('selectedThumb');
    }

    var dots = $('.bi-dot');
    dots.on('click', function () {
        var currentDot = $(this);
        var newSrc = currentDot.attr('data-src');

        $('#mainProductImage').attr('src', newSrc);

        dots.removeClass('active');
        currentDot.addClass('active');

    });

    if (dots.length > 0) {
        $(dots[0]).addClass('active');
    }





});