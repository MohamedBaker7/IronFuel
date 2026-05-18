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



(() => {
    const container = document.getElementById("variants-container");
    const addButton = document.getElementById("add-variant-btn");
    const template = document.getElementById("variant-template");

    if (!container || !addButton || !template) {
        return;
    }

    const reindexVariantRows = () => {
        const rows = container.querySelectorAll(".variant-row");

        rows.forEach((row, index) => {
            row.querySelectorAll("[data-variant-field]").forEach((field) => {
                const fieldName = field.getAttribute("data-variant-field");
                field.name = `Variants[${index}].${fieldName}`;
                field.id = `Variants_${index}__${fieldName}`;
            });
        });
    };

    addButton.addEventListener("click", () => {
        const fragment = template.content.cloneNode(true);
        container.prepend(fragment);
        reindexVariantRows();
    });

    container.addEventListener("click", (event) => {
        const button = event.target.closest(".remove-variant-btn");
        if (!button) {
            return;
        }

        const rows = container.querySelectorAll(".variant-row");
        if (rows.length === 1) {
            return;
        }

        button.closest(".variant-row")?.remove();
        reindexVariantRows();
    });

    const existingImagesContainer = document.getElementById("existing-images-list");
    const removedImageIdsInput = document.getElementById("RemovedImageIds");
    const existingOrderInput = document.getElementById("ExistingImageOrder");
    const removedIds = new Set();

    const syncImageState = () => {
        if (!existingImagesContainer || !removedImageIdsInput || !existingOrderInput) {
            return;
        }

        const orderedIds = [...existingImagesContainer.querySelectorAll(".existing-image-item")]
            .map((item) => item.getAttribute("data-image-id"))
            .filter((id) => id);

        existingOrderInput.value = orderedIds.join(",");
        removedImageIdsInput.value = [...removedIds].join(",");
    };

    if (existingImagesContainer) {
        existingImagesContainer.addEventListener("click", (event) => {
            const item = event.target.closest(".existing-image-item");
            if (!item) {
                return;
            }

            if (event.target.closest(".js-image-remove")) {
                removedIds.add(item.getAttribute("data-image-id"));
                item.remove();
                syncImageState();
                return;
            }

            if (event.target.closest(".js-image-up")) {
                const prev = item.previousElementSibling;
                if (prev) {
                    existingImagesContainer.insertBefore(item, prev);
                    syncImageState();
                }
                return;
            }

            if (event.target.closest(".js-image-down")) {
                const next = item.nextElementSibling;
                if (next) {
                    existingImagesContainer.insertBefore(next, item);
                    syncImageState();
                }
            }
        });

        syncImageState();
    }
})();



(() => {
    const fileInput = document.getElementById('ProductVideo');
    const preview = document.getElementById('videoPreview');
    const source = document.getElementById('videoPreviewSource');
    const player = document.getElementById('videoPreviewPlayer');
    const removeCheckbox = document.getElementById('RemoveProductVideo');
    const existingWrap = document.getElementById('existingProductVideoWrap');

    let objectUrl = null;

    const revokeObjectUrl = () => {
        if (objectUrl) {
            URL.revokeObjectURL(objectUrl);
            objectUrl = null;
        }
    };

    const mimeForFile = (name) => {
        const n = (name || '').toLowerCase();
        if (n.endsWith('.webm')) return 'video/webm';
        if (n.endsWith('.ogg')) return 'video/ogg';
        return 'video/mp4';
    };

    fileInput?.addEventListener('change', function () {
        const file = this.files && this.files[0];
        revokeObjectUrl();

        if (!file || !preview || !source || !player) {
            if (preview) preview.classList.add('d-none');
            if (existingWrap) existingWrap.classList.remove('d-none');
            return;
        }

        if (removeCheckbox) removeCheckbox.checked = false;

        objectUrl = URL.createObjectURL(file);
        source.src = objectUrl;
        source.type = mimeForFile(file.name);
        player.load();
        preview.classList.remove('d-none');
        if (existingWrap) existingWrap.classList.add('d-none');
    });

    removeCheckbox?.addEventListener('change', function () {
        if (this.checked) {
            revokeObjectUrl();
            if (fileInput) fileInput.value = '';
            if (preview) preview.classList.add('d-none');
            if (source) source.src = '';
        } else if (existingWrap) {
            existingWrap.classList.remove('d-none');
        }
    });
})();
