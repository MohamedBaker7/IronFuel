// Select2
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
})
