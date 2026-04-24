﻿var updatedRow;
var datatable;

function applySelect2() {
    $('.js-select2').select2();
    //Revalidate Select2 List After Select Item
    //$('.js-select2').on('select2:select', function (e) {
    //    $('form').not('#SignOut').validate().element('#' + $(this).attr('id'))
    //});
}

function adjustCanvas() {
    var canvas = $("#mobileFilter");

    if (window.innerWidth <= 700) {
        canvas.removeClass("offcanvas-end")
            .addClass("offcanvas-bottom");
    } else {
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

// Handle Save Button Submit On Modal
$('.js-modal-save').on('click', function() {
    $('.js-form').submit();
});

//Handle bootstrap modal
$('body').delegate('.js-render-modal', 'click', function() {
    var btn = $(this);
    var modal = $('#Modal');
    var title = modal.find('#Modal-title').text(btn.data('title'));

    // Handle Update Row in Edit Case
    if (btn.data('update') !== undefined) {
        updatedRow = btn.parents('tr');
    }

    $.get({
        url: btn.data('url'),
        success: function(form) {
            modal.find('.modal-body').html(form);
            $.validator.unobtrusive.parse(modal);

        },
        error: function() {
            showSuccessMessage("Something went wrong!");
        }
    });

    modal.modal('show');

});

//Handle Toggle Status 
$(document).on('click', '.js-toggle-status', function() {
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
        callback: function(result) {
            if (!result) return;

            $.post({
                url: btn.data('url'),
                data: {
                    '__RequestVerificationToken': $('input[name="__RequestVerificationToken"]').val()
                },
                success: function(lastUpdatedOn) {
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
                error: function() {
                    toastr.error("Failed", "Error");
                }
            });
        }
    });
});



// run on resize
$(window).resize(function() {
    adjustCanvas();
});


$(document).ready(function() {
    applySelect2();

    adjustCanvas();

    $(".offcanvas .nav-link").on("click", function() {
        $("#mobileMenu").removeClass("show");
    });

    initDatatable();        

})




$(document).on('submit', 'form', function() {
    var form = $(this);
    var submitBtn = form.find(':submit');

    DisableSubmit(submitBtn);

    // Re-show button if server returns validation errors (page stays same)
    $(window).on("load", function() {
        ShowSubmitButton(submitBtn);
    });
});
