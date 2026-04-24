$(document).ready(function () {

    $('#BtnExport').remove();

    // Handle Search Bar On Datatable
    $('[data-kt-filter="search"]').on('keyup', function () {
        var input = $(this);
        datatable.search(input.val()).draw();
    });


    // Handle Datatable On Server Side 
    var datatable = $('#Products').DataTable({
        serverSide: true,
        processing: true, // Add Spinner To Datatable
        stateSave: true,
        autoWidth: false,
        language: {
            processing: '<div class="d-flex justify-content-center text-primary align-items-center dt-spinner"><div class= "spinner-border" role="status"><span class="visually-hidden">Loading...</span></div><span class="text-muted ms-2">Loading...</span></div>'
        },
        ajax: {
            url: "/Admin/Products/GetDataTableProducts",
            type: "POST"
        },
        order: [[1, "asc"]],
        columnDefs: [{
            targets: [0],
            visible: false,
            searchable: false
        }],
        // Handle cols of data from controller to datatable 
        columns: [
            { "data": "id", "name": "Id", "className": "d-none" },
            { "data": "name", "name": "Name" },
            { "data": "category.name", "name": "Category" },
            { "data": "brand.name", "name": "Brand" },
            {
                "name": "IsDeleted",
                "render": function (data, type, row) {
                    return `<span class="badge text-${(row.isDeleted ? "danger" : "black")} fw-bold py-2 js-status">
                                  ${(row.isDeleted ? "Deleted" : "Available")}
                            </span>`
                }
            },

            {
                "className": 'text-end',
                "orderable": false,
                "render": function (data, type, row) {
                    return `<div class="dropdown">
                              <button class="btn dropdown-toggle rounded-0 border-0" type="button" data-bs-toggle="dropdown" aria-expanded="false">
                                <span class="svg-icon svg-icon-muted svg-icon-2hx">
                                    <svg width="24" height="24" viewBox="0 0 24 24" fill="none" xmlns="http://www.w3.org/2000/svg">
                                        <path d="M3 2H10C10.6 2 11 2.4 11 3V10C11 10.6 10.6 11 10 11H3C2.4 11 2 10.6 2 10V3C2 2.4 2.4 2 3 2Z" fill="currentColor"/>
                                        <path opacity="0.3" d="M14 2H21C21.6 2 22 2.4 22 3V10C22 10.6 21.6 11 21 11H14C13.4 11 13 10.6 13 10V3C13 2.4 13.4 2 14 2Z" fill="currentColor"/>
                                        <path opacity="0.3" d="M3 13H10C10.6 13 11 13.4 11 14V21C11 21.6 10.6 22 10 22H3C2.4 22 2 21.6 2 21V14C2 13.4 2.4 13 3 13Z" fill="currentColor"/>
                                        <path opacity="0.3" d="M14 13H21C21.6 13 22 13.4 22 14V21C22 21.6 21.6 22 21 22H14C13.4 22 13 21.6 13 21V14C13 13.4 13.4 13 14 13Z" fill="currentColor"/>
                                    </svg>
                                </span>
                              </button>
                              <ul class="dropdown-menu rounded-0 js-dropdown-actions">
                                <li><a class="dropdown-item btn btn-sm btn-outline-secondary rounded-0"href="/Products/Details/${row.id}" target="_blank" rel="noopener">
                                        View
                                    </a>
                                </li>
                                <li><a class="dropdown-item btn btn-sm btn-outline-dark rounded-0" href="/Admin/Products/Edit/${row.id}">
                                        Edit
                                    </a>
                                </li>
                                <li><a class="dropdown-item btn btn-sm btn-outline-danger rounded-0 js-toggle-status" data-url="/Admin/Products/ToggleStatus/${row.id}">
                                        Toggle Status
                                    </a>
                                </li>
                              </ul>
                            </div>`
                },
            },

        ]
    });
});