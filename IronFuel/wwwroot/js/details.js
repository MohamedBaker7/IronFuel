var allSizes = [];
var flavorOffcanvas;
var sizeOffcanvas;
var variantDataMap = {};

function isMobile() { return window.innerWidth < 992; }
function isDesktop() { return window.innerWidth >= 992; }

function storeSKUInSession(sku) {
    if (sku) {
        sessionStorage.setItem('product-sku', sku);
        $('#SKU').val(sku);
    }
}

function highlightOption(container, optionClass, value) {
    container.querySelectorAll(`.${optionClass}`).forEach(btn => {
        const isSelected = btn.dataset.value === String(value);
        btn.classList.toggle('text-muted', !isSelected);
        btn.classList.toggle('active', isSelected);
        btn.classList.toggle('text-dark', isSelected);
    });
}

function initSelectOffcanvas(selectId, offcanvasId, optionClass) {
    const select = document.getElementById(selectId);
    const canvas = document.getElementById(offcanvasId);
    if (!select || !canvas) return;

    function syncSelection() {
        highlightOption(canvas, optionClass, select.value);
    }

    $(select).on('select2:opening', function (e) {
        if (isMobile()) {
            e.preventDefault();
            bootstrap.Offcanvas.getOrCreateInstance(canvas).show();
        }
    });

    select.addEventListener('touchstart', function (e) {
        if (!isDesktop()) {
            e.preventDefault();
            bootstrap.Offcanvas.getOrCreateInstance(canvas).show();
        }
    }, { passive: false });

    canvas.querySelectorAll(`.${optionClass}`).forEach(btn => {
        btn.addEventListener('click', function () {
            select.value = this.dataset.value;
            select.dispatchEvent(new Event('change'));
            $(select).trigger('change.select2');
            highlightOption(canvas, optionClass, this.dataset.value);
        });
    });

    syncSelection();
    return { syncSelection };
}

function fillSelectSizes(sizes) {
    allSizes = sizes;
    variantDataMap = {}; // reset

    const $sizesDropdown = $('#Sizes');
    const container = document.querySelector('#sizeOffcanvas .offcanvas-body');

    // Destroy Select2 before touching options
    if ($sizesDropdown.hasClass('select2-hidden-accessible')) {
        $sizesDropdown.select2('destroy');
    }

    $sizesDropdown.empty();
    container.innerHTML = '';

    $.each(sizes, function (i, item) {
        const sizeLabel = item.weightG >= 1000
            ? `${item.weightG / 1000} kg`
            : `${item.weightG} g`;

        const servings = item.servingSizeG > 0
            ? Math.round(item.weightG / item.servingSizeG)
            : 0;

        const outOfStock = item.stock === 0;

        const text = `${sizeLabel} (${servings} Servings)`;

        // Store variant data in JS map keyed by weightG
        variantDataMap[item.weightG] = {
            sku: item.sku,
            price: item.price,
            stock: item.stock,
            servingSizeG: item.servingSizeG,
            servings: servings
        };

        // Plain <option> — just value and text, no data attributes needed
        $sizesDropdown.append(
            $('<option>', {
                value: item.weightG,
                text: text,
                'data-out-of-stock': outOfStock ? 'true' : 'false'
            })
        );

        // Offcanvas button
        const btn = document.createElement('button');
        btn.type = 'button';
        btn.className = 'btn text-muted rounded-0 text-uppercase w-100 px-4 py-3 mb-1 text-center border-0 shadow-none size-option';
        btn.dataset.value = item.weightG;
        btn.dataset.bsDismiss = 'offcanvas';
        btn.textContent = text;

        if (outOfStock) {
            btn.style.opacity = '0.4';
            btn.style.textDecoration = 'line-through';
        }

        btn.addEventListener('click', function () {
            $sizesDropdown.val(this.dataset.value)
                .trigger('change')
                .trigger('change.select2');
            highlightOption(container, 'size-option', this.dataset.value);
        });

        container.appendChild(btn);
    });

    $sizesDropdown.select2({
        minimumResultsForSearch: Infinity,
        width: '100%',
        templateResult: formatSizeOption,
        templateSelection: formatSizeSelection
    });

    const productId = parseFloat($('#ProductId').val());
    const storedProductId = JSON.parse(sessionStorage.getItem('productDetails'))?.productId;

    if (!storedProductId || productId != storedProductId) {
        setTimeout(() => {
            const firstAvailable = sizes.find(s => s.stock > 0);
            if (firstAvailable) {
                $sizesDropdown.val(String(firstAvailable.weightG)).trigger('change');
            } else {
                $sizesDropdown.prop('selectedIndex', 0);
                $('#AddToCartBtn').prop('disabled', true).text('Out of Stock');
            }
        }, 0);
    }

    sizeOffcanvas?.syncSelection();
}

function formatSizeOption(option) {
    if (!option.id) return option.text; // placeholder

    const isOutOfStock = $(option.element).data('out-of-stock') === true
        || $(option.element).attr('data-out-of-stock') === 'true';

    if (isOutOfStock) {
        return $(`<span style="color:#bbb; text-decoration:line-through;">${option.text}</span>`);
    }

    return option.text;
}

function formatSizeSelection(option) {
    if (!option.id) return option.text;

    const isOutOfStock = $(option.element).data('out-of-stock') === true
        || $(option.element).attr('data-out-of-stock') === 'true';

    if (isOutOfStock) {
        return $(`<span style="color:#bbb; text-decoration:line-through;">${option.text}</span>`);
    }

    return option.text;
}

function getSizesByFlavour(productId, flavourId, restoreWeightG = null) {
    $.ajax({
        url: '/Products/GetSizesByFlavour',
        type: 'GET',
        data: { productId, flavourId },

        success: function (data) {
            fillSelectSizes(data);

            if (restoreWeightG) {
                $('#Sizes').val(parseFloat(restoreWeightG));
            } else {
                $('#Sizes').prop('selectedIndex', 0);
            }

            sizeOffcanvas?.syncSelection();
            $('#Sizes').trigger('change');
        }
    });
}

function resolveVariantFromSelect() {
    const weightG = parseFloat($('#Sizes').val());
    return variantDataMap[weightG] || null;
}

function updateVariantUI() {
    const variant = resolveVariantFromSelect();
    const $badge = $('#SoldOutBadge');
    const $addBtn = $('#AddToCartBtn');


    // Price
    $('#Price').text(variant.price > 0 ? `${variant.price.toFixed(2)} EGP` : '');

    // Servings
    $('#ServingsPerContainer').text(
        variant.servings > 0 ? `${variant.servings} Servings` : ''
    );

    // Stock state
    const sizeOutOfStock = variant.stock === 0;
    const lowStock = variant.stock > 0 && variant.stock <= 5;

    if (sizeOutOfStock) {
        $badge.removeClass('d-none');
        $addBtn.prop('disabled', true).text('Sold Out');
    } else {
        $badge.addClass('d-none');
        $addBtn.prop('disabled', false).text('Add to Cart');
    }

    // Low stock warning
    $('#low-stock-warning').remove();
    if (lowStock) {
        $('#ServingsPerContainer').after(
            `<span id="low-stock-warning" class="text-danger small d-block mt-1">
                Only ${variant.stock} left!
            </span>`
        );
    }

    // Save to session
    storeSKUInSession(variant.sku);
}

function restoreOrDefaultSelections() {
    const productId = parseFloat($('#ProductId').val());
    const savedSku = sessionStorage.getItem('product-sku');
    const savedSizes = sessionStorage.getItem('productDetails');

    let restoreWeightG = null;

    if (savedSizes) {
        const parsed = JSON.parse(savedSizes);
        if (parsed.productId === productId) {
            $('#Flavors').val(parsed.flavourId);
            restoreWeightG = parsed.weightG;
            flavorOffcanvas?.syncSelection();
        }
    } else {
        $('#Flavors').prop('selectedIndex', 0);
        flavorOffcanvas?.syncSelection();
    }

    getSizesByFlavour(
        productId,
        parseFloat($('#Flavors').val()),
        restoreWeightG
    );
}

function updateCartBadge(count) {
    const $badge = $('#cart-badge');
    $badge
        .text(count > 99 ? '99+' : count)
        .toggleClass('d-none', count === 0)
        .removeClass('bump');

    setTimeout(() => $badge.addClass('bump'), 10);
    setTimeout(() => $badge.removeClass('bump'), 210);
}

function showToast(message, type = 'success') {
    const $toast = $('#cart-toast');
    $toast
        .removeClass('toast-success toast-error')
        .addClass(type === 'success' ? 'toast-success' : 'toast-error')
        .text(message)
        .addClass('show');

    setTimeout(() => $toast.removeClass('show'), 2500);
}

document.addEventListener('DOMContentLoaded', function () {

    const productId = parseFloat($('#ProductId').val());

    flavorOffcanvas = initSelectOffcanvas('Flavors', 'flavorOffcanvas', 'flavor-option');
    sizeOffcanvas = initSelectOffcanvas('Sizes', 'sizeOffcanvas', 'size-option');

    restoreOrDefaultSelections();

    // ── Flavour change ────────────────────────────────────────
    $('#Flavors').on('change', function () {
        const flavourId = parseFloat($(this).val());
        sessionStorage.removeItem('productDetails');
        getSizesByFlavour(productId, flavourId);
    });

    // ── Size change ───────────────────────────────────────────
    $('#Sizes').on('change', function () {
        const weightG = parseFloat($(this).val());
        const flavourId = parseFloat($('#Flavors').val());
        const variant = resolveVariantFromSelect();

        if (!variant) return;

        // Save selection for restore
        sessionStorage.setItem('productDetails', JSON.stringify({
            productId, flavourId, weightG,
            servingSizeG: variant.servingSizeG
        }));

        updateVariantUI();
    });

    // ── Quantity controls ─────────────────────────────────────
    const $qtyInput = $('#QuantityInput');

    $('#QuantityDecrease').on('click', () => {
        const val = parseInt($qtyInput.val()) || 1;
        if (val > 1) $qtyInput.val(val - 1);
    });

    $('#QuantityIncrease').on('click', () => {
        const variant = resolveVariantFromSelect();
        const max = variant.stock > 0 ? Math.min(variant.stock, 10) : 10;
        const val = parseInt($qtyInput.val()) || 1;
        $qtyInput.val(Math.min(val + 1, max));
    });

    $qtyInput.on('input', function () {
        this.value = this.value.replace(/[^0-9]/g, '');
    });

    $qtyInput.on('change', function () {
        let val = parseInt(this.value) || 1;
        if (val < 1) val = 1;
        this.value = val;
    });

    // ── Dot navigation (mobile gallery) ──────────────────────
    const dots = document.querySelectorAll('.dot-btn');
    const mainImage = document.getElementById('mainProductImage');

    dots.forEach((dot, index) => {
        dot.addEventListener('click', function () {
            const newSrc = this.getAttribute('data-src');
            if (newSrc && mainImage) {
                mainImage.src = newSrc;
                mainImage.alt = `Product image ${index + 1}`;
                dots.forEach((d, i) => {
                    d.setAttribute('aria-selected', i === index ? 'true' : 'false');
                    d.setAttribute('tabindex', i === index ? '0' : '-1');
                });
            }
        });

        dot.addEventListener('keydown', function (e) {
            if (e.key === 'ArrowRight' && index < dots.length - 1) { dots[index + 1].focus(); dots[index + 1].click(); }
            else if (e.key === 'ArrowLeft' && index > 0) { dots[index - 1].focus(); dots[index - 1].click(); }
        });
    });

    // ── Thumbnail gallery (desktop) ───────────────────────────
    const thumbnails = document.querySelectorAll('.thumb-img');

    thumbnails.forEach(thumb => {
        thumb.addEventListener('click', function () {
            if (mainImage) { mainImage.src = this.src; mainImage.alt = this.alt; }
            thumbnails.forEach(t => t.classList.remove('active'));
            this.classList.add('active');
        });

        thumb.addEventListener('keydown', function (e) {
            if (e.key === 'Enter' || e.key === ' ') { e.preventDefault(); this.click(); }
        });
    });

    // ── Collapse toggle animations ────────────────────────────
    document.querySelectorAll('.collapse-toggle').forEach(toggle => {
        const target = document.querySelector(toggle.getAttribute('data-bs-target'));
        if (target) {
            target.addEventListener('show.bs.collapse', () => toggle.setAttribute('aria-expanded', 'true'));
            target.addEventListener('hide.bs.collapse', () => toggle.setAttribute('aria-expanded', 'false'));
        }
    });

    // ── Lazy loading images ───────────────────────────────────
    if ('IntersectionObserver' in window) {
        const observer = new IntersectionObserver((entries, obs) => {
            entries.forEach(entry => {
                if (entry.isIntersecting) {
                    entry.target.classList.add('loaded');
                    obs.unobserve(entry.target);
                }
            });
        });
        document.querySelectorAll('img[loading="lazy"]').forEach(img => observer.observe(img));
    }

});