function isDesktop() {
    return window.innerWidth >= 992;
}

function updateSwapClass() {
    if (!isDesktop()) {
        $('.swap-light').removeClass('swap-light').addClass('swap-dark');
    } else {
        $('.swap-dark').removeClass('swap-dark').addClass('swap-light');
    }
}

function showSubmenu(name, title) {
    $('#mainMenu').addClass('d-none');
    $('[id^="submenu-"]').addClass('d-none');
    $(`#submenu-${name}`).removeClass('d-none');
    $('#menuTitle').text(title);
    $('#backBtn').removeClass('d-none');
}

function showMainMenu() {
    $('[id^="submenu-"]').addClass('d-none');
    $('#mainMenu').removeClass('d-none');
    //$('#menuTitle').text('Menu');
    $('#backBtn').addClass('d-none');
}

$(document).ready(function () {

    $(document).on('click', '[data-submenu]', function () {
        var name = $(this).data('submenu');
        var title = $(this).data('title');

        showSubmenu(name, title);

    });

    $(document).on('click', '#backBtn', function () {
        showMainMenu();
    });

    $('#mobileMenu').on('hidden.bs.offcanvas', function () {
        showMainMenu();
    });
});

// run on load
updateSwapClass();

// run on resize
$(window).on('resize', function () {
    updateSwapClass();
});