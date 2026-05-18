var lastChanged = null;
var filtersLoaded = false;

function applyFilters(form) {

    $('#pageSpinner').removeClass('d-none'); // show spinner

    form = $(form);

    var filter = {};

    filter.inStockOnly = form.find('[name="inStock"]').prop('checked');
    filter.minPrice = form.find('[name="minPrice"]').val();
    filter.maxPrice = form.find('[name="maxPrice"]').val();

    priceFilterValidation(filter, form);

    filter.flavors = [];
    form.find('.js-falvour:checked').each(function () {
        var val = $(this).val();
        if (!filter.flavors.includes(val))
            filter.flavors.push(val);
    });

    filter.sizes = [];
    form.find('.js-size:checked').each(function () {
        var val = $(this).val();
        if (!filter.sizes.includes(val))
            filter.sizes.push(val);
    });

    filter.categoryId = $('#CategoryId').val();

    sessionStorage.setItem('productsFilter', JSON.stringify(filter));

    

    $.ajax({
        url: '/Products/Filter',
        type: "POST",
        traditional: true,
        data: {
            '__RequestVerificationToken': $('input[name="__RequestVerificationToken"]').val(),
            ...filter
        },
        success: function (result) {
            $('#ProductsContainer').html(result.html);

            $('#ProductsContainer .product-card').each(function (index) {
                $(this).css('animation-delay', (index * 0.05) + 's'); // animate cards
            });

            $('#ProductsCount').text(result.totalCount + ' products');

            buildActiveFilters(filter);

            if (lastChanged !== 'flavor') {
                $('.js-falvour').each(function () {
                    var flavor = $(this).val();
                    var isChecked = $(this).prop('checked');
                    var count = result.availableFlavors[flavor] ?? 0;
                    var isAvailable = isChecked || count > 0;

                    $(this).prop('disabled', !isAvailable);
                    $(`label[data-flavor="${flavor}"]`)
                        .toggleClass('disabled-option', !isAvailable)
                        .find('.flavor-count').text(count);
                });
            }

            // update sizes — everyone updates except size itself
            if (lastChanged !== 'size') {
                $('.js-size').each(function () {
                    var size = $(this).val();
                    var isChecked = $(this).prop('checked');
                    var count = result.availableSizes[size] ?? 0;
                    var isAvailable = isChecked || count > 0;

                    $(this).prop('disabled', !isAvailable);
                    $(`label[data-size="${size}"]`)
                        .toggleClass('disabled-option', !isAvailable)
                        .find('.size-count').text(count);
                });
            }

            lastChanged = null; // reset
            $('#pageSpinner').addClass('d-none'); 
            showPageContent();
        },
        error: function (xhr) {
            console.log("Error:", xhr.status, xhr.responseText);
            $('#pageSpinner').addClass('d-none');
            showPageContent();
        }
    });
}

function priceFilterValidation(filter, form) {
    var minInput = form.find('[name="minPrice"]');
    var maxInput = form.find('[name="maxPrice"]');

    var allowedMin = parseFloat(minInput.attr('min')) || 0;
    var allowedMax = parseFloat(maxInput.attr('max')) || null;

    // parse values
    filter.minPrice = filter.minPrice === '' || isNaN(filter.minPrice) ? null : parseFloat(filter.minPrice);
    filter.maxPrice = filter.maxPrice === '' || isNaN(filter.maxPrice) ? null : parseFloat(filter.maxPrice);

    // clamp minPrice within allowed range
    if (filter.minPrice !== null) {
        if (filter.minPrice < allowedMin) {
            filter.minPrice = null;
        } else if (allowedMax !== null && filter.minPrice >= allowedMax) {
            filter.minPrice = allowedMax - 1;
        }
        minInput.val(filter.minPrice ?? '');
    }

    // clamp maxPrice within allowed range
    if (filter.maxPrice !== null) {
        if (filter.maxPrice < allowedMin + 1) {
            filter.maxPrice = null;
        } else if (allowedMax !== null && filter.maxPrice > allowedMax) {
            filter.maxPrice = allowedMax;
        }
        maxInput.val(filter.maxPrice ?? '');
    }

    // fix min >= max conflict
    if (filter.minPrice !== null && filter.maxPrice !== null) {
        if (filter.minPrice >= filter.maxPrice) {
            // check which input was last changed
            if (lastChanged === 'price' && minInput.is(':focus')) {
                // user is typing in min → push max up
                filter.maxPrice = filter.minPrice + 1;
                if (allowedMax !== null && filter.maxPrice > allowedMax) {
                    filter.maxPrice = allowedMax;
                    filter.minPrice = allowedMax - 1;
                    minInput.val(filter.minPrice);
                }
                maxInput.val(filter.maxPrice);
            } else {
                // user is typing in max → push min down
                filter.minPrice = filter.maxPrice - 1;
                if (filter.minPrice < allowedMin) {
                    filter.minPrice = allowedMin;
                    filter.maxPrice = allowedMin + 1;
                    maxInput.val(filter.maxPrice);
                }
                minInput.val(filter.minPrice);
            }
        }
    }

    // visual feedback — highlight invalid inputs
    minInput.toggleClass('is-invalid', filter.minPrice !== null && filter.minPrice < allowedMin);
    maxInput.toggleClass('is-invalid', filter.maxPrice !== null && allowedMax !== null && filter.maxPrice > allowedMax);
}

function loadFilters() {
    if (filtersLoaded) return;
    filtersLoaded = true;

    var savedFilters = sessionStorage.getItem('productsFilter');

    if (savedFilters) {
        var filter = JSON.parse(savedFilters);
        var form = $('form');

        form.find('[name="inStock"]').prop('checked', filter.inStockOnly);
        form.find('[name="minPrice"]').val(filter.minPrice);
        form.find('[name="maxPrice"]').val(filter.maxPrice);

        form.find('.js-falvour').each(function () {
            $(this).prop('checked', filter.flavors?.includes($(this).val()) ?? false);
        });

        form.find('.js-size').each(function () {
            $(this).prop('checked', filter.sizes?.includes($(this).val()) ?? false);
        });

        expandActiveFilters(filter);

        lastChanged = null; // load → update everything
        applyFilters(form);
    }
}

function buildActiveFilters(filter) {
    var bar = $('#activeFiltersBar');
    bar.empty();

    var hasAny = false;

    // in stock
    if (filter.inStockOnly) {
        hasAny = true;
        bar.append(createChip('In Stock', 'stock'));
    }

    // price range
    if (filter.minPrice !== null || filter.maxPrice !== null) {
        hasAny = true;
        var label = '';
        if (filter.minPrice !== null && filter.maxPrice !== null)
            label = `${filter.minPrice} EGP - ${filter.maxPrice} EGP`;
        else if (filter.minPrice !== null)
            label = `Min ${filter.minPrice} EGP`;
        else
            label = `Max ${filter.maxPrice} EGP`;

        bar.append(createChip(label, 'price'));
    }

    // flavors
    if (filter.flavors?.length > 0) {
        hasAny = true;
        filter.flavors.forEach(function (flavor) {
            bar.append(createChip(flavor, 'flavor', flavor));
        });
    }

    // sizes
    if (filter.sizes?.length > 0) {
        hasAny = true;
        filter.sizes.forEach(function (size) {
            bar.append(createChip(`${size} Kg`, 'size', size));
        });
    }

    // clear all button
    if (hasAny) {
        bar.append(`
            <button class="btn px-3 rounded-0 border-0" id="clearFiltersBtn">
                <span class="swap-reverse fs-5">Clear all</span>
            </button>
        `);
    }
}

function filterHandling() {
    var priceInputs = $('input[type="number"]');
    var lastPriceValue = {};

    priceInputs.each(function () {
        lastPriceValue[$(this).attr('name')] = $(this).val();
    });

    priceInputs.on('keydown', function (e) {
        if (e.key === 'Enter') {
            this.blur();
        }
    });

    priceInputs.on('focus', function () {
        lastPriceValue[$(this).attr('name')] = $(this).val();
    });

    priceInputs.on('blur', function () {
        if (isDesktop()) {
            var name = $(this).attr('name');
            var currentValue = $(this).val();

            if (currentValue !== lastPriceValue[name]) {
                lastChanged = 'price';
                applyFilters($(this).closest('form'));
            }
        }
    });

    // flavor changed
    $(document).on('change', '.js-falvour', function () {
        lastChanged = 'flavor';
        if (isDesktop()) applyFilters($(this).closest('form'));
    });

    // size changed
    $(document).on('change', '.js-size', function () {
        lastChanged = 'size';
        if (isDesktop()) applyFilters($(this).closest('form'));
    });

    // instock changed
    $(document).on('change', '[name="inStock"]', function () {
        lastChanged = 'stock';
        if (isDesktop()) applyFilters($(this).closest('form'));
    });

    // mobile apply button
    $('.apply-filter-btn').on('click', function () {
        lastChanged = null;
        applyFilters($(this).closest('form'));
        bootstrap.Offcanvas.getOrCreateInstance('#mobileFilter').hide();
    });
}

function expandActiveFilters(filter) {
    setTimeout(function () {
        if (filter.minPrice !== null || filter.maxPrice !== null) {
            var priceCollapse = document.getElementById('PriceExpand');
            bootstrap.Collapse.getOrCreateInstance(priceCollapse).show();
        }

        if (filter.flavors?.length > 0) {
            var flavourCollapse = document.getElementById('FlavourExpand');
            bootstrap.Collapse.getOrCreateInstance(flavourCollapse).show();
        }

        if (filter.sizes?.length > 0) {
            var sizeCollapse = document.getElementById('SizeExpand');
            bootstrap.Collapse.getOrCreateInstance(sizeCollapse).show();
        }
    }, 100);
}

function clearAllFilters(form) {
    form = form || $('form');

    form.find('.js-falvour, .js-size, [name="inStock"]').prop('checked', false);

    form.find('[name="minPrice"]').val('');
    form.find('[name="maxPrice"]').val('');

    sessionStorage.removeItem('productsFilter');

    lastChanged = null;

    applyFilters(form);
}


function createChip(label, type, value = null) {

    return `
        <span class="badge d-flex align-items-center gap-1 px-3 py-3 bg-light text-dark border-0 rounded-0 fs-6 bg-gray-color"
              data-type="${type}" data-value="${value}">
            ${label}
            <i class="fas fa-times ms-1 chip-remove" 
               style="cursor:pointer; font-size:0.7rem;"
               data-type="${type}" data-value="${value}">
            </i>
        </span>
    `;
}

function hideSpinner() {
    $('#pageSpinner').addClass('d-none');
}

function showPageContent() {
    $('#pageContent').addClass('ready');
}

$(document).on('click', '#clearFiltersBtn', function () {
    clearAllFilters($('form'));
});

// remove single chip
$(document).on('click', '.chip-remove', function () {
    var type = $(this).data('type');
    var value = $(this).data('value');
    var form = $('form');

    if (type === 'stock') {
        form.find('[name="inStock"]').prop('checked', false);
        lastChanged = 'stock';
    }

    if (type === 'price') {
        form.find('[name="minPrice"]').val('');
        form.find('[name="maxPrice"]').val('');
        lastChanged = 'price';
    }

    if (type === 'flavor') {
        form.find(`.js-falvour[value="${value}"]`).prop('checked', false);
        lastChanged = 'flavor';
    }

    if (type === 'size') {
        form.find(`.js-size[value="${value}"]`).prop('checked', false);
        lastChanged = 'size';
    }

    applyFilters(form);
});


window.addEventListener('beforeunload', function () {
    sessionStorage.setItem('isReload', 'true');
});

window.addEventListener('load', function () {
    const nav = performance.getEntriesByType("navigation")[0];

    if (nav.type === "reload") {
        loadFilters();
    } else if (nav.type === "navigate") {
        loadFilters();

    }
    else if (nav.type === "back_forward") {
        loadFilters();
    }

    else {
        sessionStorage.removeItem('productsFilter');
    }

    sessionStorage.removeItem('isReload');
});


$(document).ready(function () {
    filterHandling();
});

// hover → show secondary
$(document).on('mouseenter', '.product-image-wrapper', function () {
    var secondary = $(this).data('image-secondary');
    if (secondary) {
        $(this).css('background-image', `url(${secondary})`);
    }
});

// leave → back to primary
$(document).on('mouseleave', '.product-image-wrapper', function () {
    var primary = $(this).data('image-primary');
    $(this).css('background-image', `url(${primary})`);
});
