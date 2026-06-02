var updatedRow;
var datatable;

function isDesktop() {
    return window.innerWidth >= 992;
}

function isTablet() {
    return window.innerWidth < 992 && window.innerWidth >= 768;
}

function isMobile() {
    return window.innerWidth < 768;
}

function applySelect2() {
    $('.js-select2').select2({
        width: '100%',
        dropdownParent: $('body'),
        minimumResultsForSearch: Infinity,
    });


}

function adjustCanvas() {
    var canvas = $("#mobileFilter");

    if (isMobile()) {
        canvas.removeClass("offcanvas-end")
            .addClass("offcanvas-bottom");
    } else if (isTablet()) {
        canvas.removeClass('offcanvas-bottom')
            .addClass('offcanvas-end');
    }
}

function DisableSubmit(btn) {
    btn.prop("disabled", true);
    btn.find(".indicator-progress").removeClass("d-none");
    btn.find(".indicator-label").addClass("d-none");
}

function ShowSubmitButton(btn) {
    btn.prop("disabled", false);
    btn.find(".indicator-progress").addClass("d-none");
    btn.find(".indicator-label").removeClass("d-none");
}

function initDatatable() {
    datatable = $('.js-datatables').DataTable();
}

function showSuccessMessage(message) {
    toastr.success(message, "Success");
}

function showErrorMessage(message) {
    toastr.error(message, "Error");
}

function onModelBegin() {
    DisableSubmit($('#Modal').find(':submit'));
}

function onModalSuccess(row) {
    showSuccessMessage("Saved Successfully!");
    // Modal Handeler
    $('#Modal').modal("hide");

    // Handle Add Row Or Edit It (Datatable Only)
    var newRow = $(row);
    datatable.row.add(newRow).draw();


    if (updatedRow !== undefined) {
        datatable.row(updatedRow).remove().draw();
        updatedRow = undefined;
    }
}

function onModelComplete() {
    ShowSubmitButton($('#Modal').find(':submit'));
}

function updateCartUI(cartCount, cartTotal) {
    $('#cart-count').text(cartCount);
    $('#cart-total').text(cartTotal);
    showToast('Item added to cart!', 'success');
}

function showCartSummaryOffcanvas() {
    // Update total in offcanvas footer
    //$('#cartOffcanvasTotal').text('$' + parseFloat(response.cart_total).toFixed(2));

    if (window.location.pathname.toLowerCase().includes('/cart')) {
        location.reload();
        return;
    }

    $('#cartOffcanvasBody').load('/Cart/CartSummary', function () {


        const offcanvas = new bootstrap.Offcanvas($('#cartOffcanvas')[0], {
            scroll: true,
            backdrop: true
        });

        offcanvas.show();
    });
}

function showCartSuccessAddedOffcanvas(response) {
    const item = response.item;

    var sizeLabel = item.size >= 1000 ?
        item.size / 1000 + ' kg'
        : item.size + ' g';

    $('#cartOffcanvasImage')
        .attr('src', '/' + item.image?.trimStart('/'))
        .attr('alt', item.name);

    $('#cartOffcanvasName').text(item.name);
    $('#cartOffcanvasPrice').text(parseFloat(item.totalPrice).toFixed(2) + ' EGP');

    $('#cartOffcanvasSize_Flavour').html(`<span class="text-muted fw-semibold">${item.flavour} / ${sizeLabel} (${item.servings} Servings)</span>`);

    const offcanvas = new bootstrap.Offcanvas($('#successAddedOffcanvas')[0], {
        scroll: false,
        backdrop: true  
    });

    offcanvas.show();
}

// Handle Save Button Submit On Modal
$('.js-modal-save').on('click', function () {
    $('.js-form').submit();
});

//Handle bootstrap modal
$('body').delegate('.js-render-modal', 'click', function () {
    var btn = $(this);
    var modal = $('#Modal');
    var title = modal.find('#Modal-title').text(btn.data('title'));

    // Handle Update Row in Edit Case
    if (btn.data('update') !== undefined) {
        updatedRow = btn.parents('tr');
    }

    $.get({
        url: btn.data('url'),
        success: function (form) {
            modal.find('.modal-body').html(form);
            $.validator.unobtrusive.parse(modal);

        },
        error: function () {
            showSuccessMessage("Something went wrong!");
        }
    });

    modal.modal('show');

});

//Handle Toggle Status 
$(document).on('click', '.js-toggle-status', function () {
    var btn = $(this);

    bootbox.confirm({
        message: 'Are you sure that you want to toggle this item status?',
        buttons: {
            confirm: {
                label: 'Yes',
                className: 'btn-dark rounded-0 border-0 shadow-none'
            },
            cancel: {
                label: 'No',
                className: 'btn-light rounded-0 border-0 shadow-none'
            }
        },
        callback: function (result) {
            if (!result) return;

            $.post({
                url: btn.data('url'),
                data: {
                    '__RequestVerificationToken': $('input[name="__RequestVerificationToken"]').val()
                },
                success: function (lastUpdatedOn) {
                    var parentRow = btn.closest('tr');
                    var childSpan = parentRow.find('.js-status');
                    var isDeleted = childSpan.text().trim() === 'Deleted';

                    childSpan
                        .text(isDeleted ? 'Available' : 'Deleted')
                        .toggleClass('text-black text-danger');

                    //Update lastUpdatedOn :
                    parentRow.find('.js-updated-on').html(lastUpdatedOn);

                    // Re-trigger Animate.css without page reload
                    parentRow.removeClass('animate__animated animate__flash');
                    void parentRow[0].offsetWidth; // force reflow
                    parentRow.addClass('animate__animated animate__flash');

                    toastr.success("Item toggled successfully.", "Success");
                },
                error: function () {
                    toastr.error("Failed", "Error");
                }
            });
        }
    });
});

// run on resize
$(window).resize(function () {
    adjustCanvas();
});


$(document).ready(function () {
    applySelect2();

    adjustCanvas();

    $(".offcanvas .nav-link").on("click", function () {
        $("#mobileMenu").removeClass("show");
    });

    initDatatable();

})


$(document).on('submit', 'form', function () {
    var form = $(this);
    var submitBtn = form.find(':submit');

    DisableSubmit(submitBtn);

    // Re-show button if server returns validation errors (page stays same)
    $(window).on("load", function () {
        ShowSubmitButton(submitBtn);
    });
});


$(document).on('click', '#ShowCartSummary', function () {
    showCartSummaryOffcanvas();
})

// ── Add To Cart
$(document).on('click', '#AddToCartBtn', function () {
    const $btn = $(this);
    const sku = $('#SKU').val(); // data-sku for cards, #SKU for details
    const qty = parseInt($('#QuantityInput').val()) || 1;

    if (!sku) {
        return;
    }

    $btn.prop('disabled', true).text('Adding...');



    $.ajax({
        url: '/Cart/AddToCart',
        method: 'POST',
        data: {
            sku: sku,
            qty: qty,
            __RequestVerificationToken: $('[name=__RequestVerificationToken]').val()
        },
        success: function (response) {
            if (response.success) {
                updateCartUI(response.cart_count, response.cart_total);
                $('#QuantityInput').val(1);

                setTimeout(function () {
                    showCartSuccessAddedOffcanvas(response);
                }, 2000);

            }
        },
        error: function (xhr) {
            const message = xhr.responseJSON?.message || 'Something went wrong';
            showToast(message, 'error');
        },
        complete: function (response) {
            setTimeout(() => {
                $btn.prop('disabled', false).text('Add to Cart');
            }, 2000)
        }
    });
});

// Add class to body when offcanvas is open to prevent background scroll
document.querySelectorAll('.offcanvas').forEach(function (el) {
    el.addEventListener('show.bs.offcanvas', function () {
        document.body.classList.add('offcanvas-open');
    });

    el.addEventListener('hidden.bs.offcanvas', function () {
        document.body.classList.remove('offcanvas-open');
    });
});