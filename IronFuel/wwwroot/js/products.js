function applyFilters(form) {

    form = $(form);

    var filter = {};

    filter.inStockOnly = form.find('[name="inStock"]').prop('checked');
    filter.minPrice = form.find('[name="minPrice"]').val();
    filter.maxPrice = form.find('[name="maxPrice"]').val();

    priceFilterValidation(filter, form);

    filter.flavors = [];
    form.find('.js-falvour:checked').each(function () {
        filter.flavors.push($(this).val());
    });

    filter.sizes = [];
    form.find('.js-size:checked').each(function () {
        filter.sizes.push($(this).val());
    });

    filter.categoryId = $('#CategoryId').val();

    sessionStorage.setItem('productsFilter', JSON.stringify(filter));

    $.ajax({
        url: "/Products/Filter",
        type: "POST",
        traditional: true,
        data: filter,
        success: function (result) {
            $('#ProductsContainer').html(result);

            var productsCount = $('#product-count-hidden').val();
            $('#ProductsCount').text(productsCount + ' products');
        }
    });
}

function priceFilterValidation(filter, form) {

    var maxInput = form.find('[name="maxPrice"]');
    var minInput = form.find('[name="minPrice"]');

    var allowedMaxPrice = maxInput.attr('max');

    if (allowedMaxPrice !== '')
        allowedMaxPrice = parseFloat(allowedMaxPrice);

    filter.minPrice = filter.minPrice === '' ? null : parseFloat(filter.minPrice);
    filter.maxPrice = filter.maxPrice === '' ? null : parseFloat(filter.maxPrice);

    if (filter.minPrice > allowedMaxPrice - 1 && filter.minPrice != null) {
        filter.minPrice = allowedMaxPrice - 1;
        minInput.val(filter.minPrice);
    }

    if (filter.maxPrice > allowedMaxPrice && filter.maxPrice != null) {
        filter.maxPrice = allowedMaxPrice;
        maxInput.val(filter.maxPrice);
    }

    if (filter.minPrice < 0) {
        filter.minPrice = null;
        minInput.val(filter.minPrice);
    }

    if (filter.maxPrice < 1) {
        filter.maxPrice = null;
        maxInput.val(filter.maxPrice);
    }

    if (filter.minPrice != null && filter.maxPrice != null) {
        if (filter.minPrice == filter.maxPrice) {
            filter.minPrice--;
            minInput.val(filter.minPrice);
        }

        if (filter.minPrice > filter.maxPrice) {
            maxInput.val(filter.minPrice + 1);
        }
    }
}

function isDesktop() {
    return window.innerWidth >= 992;
}

function filterHandling() {
    // Price Inputs Filter
    var priceInputs = $('input[type="number"]');
    priceInputs.on('keydown', function (e) {
        if (e.key === 'Enter') {
            this.blur();
        }
    });

    priceInputs.on('blur', function () {
        if (isDesktop())
            applyFilters($(this).closest('form'));
    });

    $('.js-filter').on('change keyup', function () {
        if (isDesktop()) applyFilters($(this).closest('form'));
    });

    var applyFilterBtn = $('.apply-filter-btn');

    applyFilterBtn.on('click', function () {
        applyFilters($(this).closest('form'));
        bootstrap.Offcanvas.getOrCreateInstance('#mobileFilter').hide();
    });
}

function loadFilters() {
    var savedFilters = sessionStorage.getItem('productsFilter');

    if (savedFilters) {
        var filter = JSON.parse(savedFilters);
        var form = $('form'); 

        form.find('[name="inStock"]').prop('checked', filter.inStockOnly);
        form.find('[name="minPrice"]').val(filter.minPrice);
        form.find('[name="maxPrice"]').val(filter.maxPrice);

        form.find('.js-falvour').each(function () {
            $(this).prop('checked', filter.flavors.includes($(this).val()));
        });

        form.find('.js-size').each(function () {
            $(this).prop('checked', filter.sizes.includes($(this).val()));
        });

        applyFilters(form);
    }
}

$(document).ready(function () {

    window.addEventListener('beforeunload', function () {

        if (!sessionStorage.getItem('isReload')) {
            sessionStorage.removeItem('productsFilter');
        }

        sessionStorage.removeItem('isReload');
    });

    window.addEventListener('load', function () {
        const nav = performance.getEntriesByType("navigation")[0];

        if (nav.type === "reload") {
            sessionStorage.setItem('isReload', 'true');
            loadFilters();
        } else {
            sessionStorage.removeItem('productFilters');
        }

    });

    filterHandling();

    // Flavors CheckBox Labels
    var formCheckBox = $('#checkDefault');
    formCheckBox.on('click', function () {
        if (formCheckBox.prop('checked')) {
            formCheckBox.siblings('.form-check-label').removeClass('text-muted');
        }
        else {
            formCheckBox.siblings('.form-check-label').addClass('text-muted');
        }
    })

});