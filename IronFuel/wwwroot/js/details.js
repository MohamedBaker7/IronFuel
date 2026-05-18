var allSizes = [];
var servings;
var flavorOffcanvas;
var sizeOffcanvas;


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

    const sizesDropdown = $('#Sizes');
    sizesDropdown.empty();

    const container = document.querySelector('#sizeOffcanvas .offcanvas-body');
    container.innerHTML = '';

    $.each(sizes, function (i, item) {
        const sizeLabel = item.weightG >= 1000
            ? `${item.weightG / 1000} kg`
            : `${item.weightG} g`;
        const text = `${sizeLabel} (${Math.round(item.weightG / item.servingSizeG)} Servings)`;

        // native select option
        sizesDropdown.append(
            `<option value="${item.weightG}" data-serving-size="${item.servingSizeG}">${text}</option>`
        );

        // offcanvas button
        const btn = document.createElement('button');
        btn.type = 'button';
        btn.className = 'btn text-muted rounded-0 text-uppercase w-100 px-4 py-3 mb-1 text-center border-0 shadow-none size-option';
        btn.dataset.value = item.weightG;
        btn.dataset.bsDismiss = 'offcanvas';
        btn.textContent = text;

        btn.addEventListener('click', function () {
            const select = document.getElementById('Sizes');
            select.value = this.dataset.value;
            select.dispatchEvent(new Event('change'));
            highlightOption(container, 'size-option', this.dataset.value);
        });

        container.appendChild(btn);
    });

    sizeOffcanvas?.syncSelection();
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

function getProductPrice(productId, flavourId, weightG) {
    $.ajax({
        url: '/Products/GetProductPrice',
        type: 'GET',
        data: { productId, flavourId, weightG },
        success: function (data) {
            $('#Price').text(`${data} EGP`);
        }
    });
}

function refreshSessionStorage(productId, flavourId, weightG, servingSizeG) {
    sessionStorage.setItem('productDetails', JSON.stringify({
        productId, flavourId, weightG, servingSizeG
    }));
}

function restoreOrDefaultSelections() {
    const productId = parseFloat($('#ProductId').val());
    const product = JSON.parse(sessionStorage.getItem('productDetails'));

    if (product && productId === product.productId) {
        $('#Flavors').val(product.flavourId);
        flavorOffcanvas?.syncSelection();
        getSizesByFlavour(productId, product.flavourId, product.weightG);
    } else {
        $('#Flavors').prop('selectedIndex', 0);
        flavorOffcanvas?.syncSelection();
        getSizesByFlavour(productId, parseFloat($('#Flavors').val()));
    }
}

function sizeChangeHandler(productId, flavourId, weightG, servingSizeG) {
    console.log(`Size changed: ${weightG}g with serving size ${servingSizeG}g, flavour id: ${flavourId}`);
    servingsPerContainer(weightG, servingSizeG);
    getProductPrice(productId, flavourId, weightG);
    refreshSessionStorage(productId, flavourId, weightG, servingSizeG);
}

function servingsPerContainer(weightG, servingSizeG) {
    $('#ServingsPerContainer').text(`${Math.round(weightG / servingSizeG)} Servings`);
}

function changeQuantity(step) {
    const input = $('#QuantityInput');
    const current = parseInt(input.val(), 10);
    input.val(Math.max(1, (isNaN(current) ? 1 : current) + step));
}



document.addEventListener('DOMContentLoaded', function () {

    // product page vars

    const productId = parseFloat($('#ProductId').val());
    let flavourId, weightG, servingSizeG;

    flavorOffcanvas = initSelectOffcanvas('Flavors', 'flavorOffcanvas', 'flavor-option');
    sizeOffcanvas = initSelectOffcanvas('Sizes', 'sizeOffcanvas', 'size-option');

    restoreOrDefaultSelections();

    $('#Flavors').on('change', function () {
        flavourId = parseFloat($(this).val());
        getSizesByFlavour(productId, flavourId);
    });

    $('#Sizes').on('change', function () {
        weightG = $(this).val();
        servingSizeG = $(this).find(':selected').data('serving-size');
        flavourId = parseFloat($('#Flavors').val());
        sizeChangeHandler(productId, flavourId, weightG, servingSizeG);
    });

    // ── dot navigation (mobile gallery) ──
    const dots = document.querySelectorAll('.dot-btn');
    const mainImage = document.getElementById('mainProductImage');

    dots.forEach((dot, index) => {
        dot.addEventListener('click', function () {
            updateImageFromDot(this, index);
        });
        dot.addEventListener('keydown', function (e) {
            if (e.key === 'ArrowRight' && index < dots.length - 1) {
                dots[index + 1].focus(); dots[index + 1].click();
            } else if (e.key === 'ArrowLeft' && index > 0) {
                dots[index - 1].focus(); dots[index - 1].click();
            }
        });
    });

    function updateImageFromDot(dotElement, index) {
        const newSrc = dotElement.getAttribute('data-src');
        if (newSrc && mainImage) {
            mainImage.src = newSrc;
            mainImage.alt = `Product image ${index + 1}`;
            dots.forEach((d, i) => {
                d.setAttribute('aria-selected', i === index ? 'true' : 'false');
                d.setAttribute('tabindex', i === index ? '0' : '-1');
            });
        }
    }

    // ── thumbnail gallery (desktop) ──
    const thumbnails = document.querySelectorAll('.thumb-img');

    thumbnails.forEach(thumb => {
        thumb.addEventListener('click', function () {
            if (mainImage) {
                mainImage.src = this.src;
                mainImage.alt = this.alt;
            }
            thumbnails.forEach(t => t.classList.remove('active'));
            this.classList.add('active');
        });
        thumb.addEventListener('keydown', function (e) {
            if (e.key === 'Enter' || e.key === ' ') {
                e.preventDefault(); this.click();
            }
        });
    });

    // ── quantity controls ──
    const quantityInput = document.getElementById('QuantityInput');
    const quantityDecrease = document.getElementById('QuantityDecrease');
    const quantityIncrease = document.getElementById('QuantityIncrease');

    quantityDecrease?.addEventListener('click', () => {
        const current = parseInt(quantityInput.value) || 1;
        if (current > 1) { quantityInput.value = current - 1; quantityInput.dispatchEvent(new Event('change')); }
    });

    quantityIncrease?.addEventListener('click', () => {
        quantityInput.value = (parseInt(quantityInput.value) || 1) + 1;
        quantityInput.dispatchEvent(new Event('change'));
    });

    quantityInput?.addEventListener('change', function () {
        let value = parseInt(this.value) || 1;
        if (value < 1) value = 1;
        this.value = value;
    });

    quantityInput?.addEventListener('input', function () {
        this.value = this.value.replace(/[^0-9]/g, '');
    });

    // ── add to cart ──
    const addToCartBtn = document.getElementById('AddToCartBtn');
    const flavorSelect = document.getElementById('Flavors');
    const sizeSelect = document.getElementById('Sizes');

    addToCartBtn?.addEventListener('click', async function () {
        const productId = document.getElementById('ProductId')?.value;
        const quantity = parseInt(quantityInput?.value || 1);
        const flavor = flavorSelect?.value;
        const size = sizeSelect?.value;

        if (!productId) return console.error('Product ID not found');

        addToCartBtn.disabled = true;
        const originalText = addToCartBtn.textContent;
        addToCartBtn.textContent = 'Adding...';

        try {
            const response = await fetch('/api/cart/add', {
                method: 'POST',
                headers: {
                    'Content-Type': 'application/json',
                    'X-CSRF-TOKEN': document.querySelector('[name="__RequestVerificationToken"]')?.value
                },
                body: JSON.stringify({ productId, quantity, flavor, size })
            });

            if (!response.ok) throw new Error(`HTTP error! status: ${response.status}`);

            await response.json();

            addToCartBtn.textContent = '✓ Added to Cart';
            addToCartBtn.classList.replace('btn-dark', 'btn-success');

            setTimeout(() => {
                addToCartBtn.textContent = originalText;
                addToCartBtn.classList.replace('btn-success', 'btn-dark');
                addToCartBtn.disabled = false;
            }, 2000);

            showSuccessMessage('Added to cart!');

        } catch (error) {
            console.error('Error adding to cart:', error);
            addToCartBtn.textContent = '❌ Error';
            showErrorMessage('Failed to add item. Please try again.');
            setTimeout(() => { addToCartBtn.textContent = originalText; addToCartBtn.disabled = false; }, 2000);
        }
    });

    // ── collapse toggle animations ──
    document.querySelectorAll('.collapse-toggle').forEach(toggle => {
        const collapseElement = document.querySelector(toggle.getAttribute('data-bs-target'));
        if (collapseElement) {
            collapseElement.addEventListener('show.bs.collapse', () => toggle.setAttribute('aria-expanded', 'true'));
            collapseElement.addEventListener('hide.bs.collapse', () => toggle.setAttribute('aria-expanded', 'false'));
        }
    });

    // ── lazy loading images ──
    if ('IntersectionObserver' in window) {
        const imageObserver = new IntersectionObserver((entries, observer) => {
            entries.forEach(entry => {
                if (entry.isIntersecting) {
                    entry.target.classList.add('loaded');
                    observer.unobserve(entry.target);
                }
            });
        });
        document.querySelectorAll('img[loading="lazy"]').forEach(img => imageObserver.observe(img));
    }

});