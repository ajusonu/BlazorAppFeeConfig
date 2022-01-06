var dataTable = "";
$(document).ready(function () {
    // This is the processing for the Fee Type mapping listing.
    dataTable = $("#FeeTypeMapping-table").DataTable({
        "ajax": {
            "url": "/FeeTypeMapping/FeeTypeMapping_Search",
            "type": "GET"
        },
        "columns": [
            {
                "data": "Id",
                "render": function (data, type, row, meta) {
                    if (type === "sort" || type === "type") {
                        return data;
                    }
                    return "<a href='/FeeTypeMapping/Edit?Id=" + data + "'>Edit </a>";
                },
                "width": 50,
                "orderable": false
            },

            {
                "data": "OutletCode",
                "width": 50
            },
            {
                "data": "FeeType",
                "width": 50
            },
            {
                "data": "Description",
                "width": 250
            },
            {
                "data": "Scope",
                "width": 50
            },
            {
                "data": "QueryType",
                "width": 50
            },
            {
                "data": "ExclusionCode",
                "width": 50
            },
            {
                "data": "Id",
                "render": function (data, type, row, meta) {
                    if (type === "sort" || type === "type") {
                        return data;
                    }
                    return row.AutoApply ? '<label class="custom-control custom-checkbox custom-control-inline"><input class="chkBx-col-id-row-level custom-control-input"  type="checkbox" data-item-id="' + data + '" checked/><span class="custom-control-label custom-control-color"></span></label>' :
                        '<label class="custom-control custom-checkbox custom-control-inline"><input class="chkBx-col-id-row-level custom-control-input"  type="checkbox" data-item-id="' + data + '" /><span class="custom-control-label custom-control-color"></span></label>';
                },
                "orderable": false,
                "targets": 'no-sort',
                "width": 50
            },
            {
                "data": "Id",
                "render": function (data, type, row, meta) {
                    if (type === "sort" || type === "type") {
                        return data;
                    }
                    return "<a class='btn-sm btn-danger' onclick=deleteSelectedFee('" + data + "')>Delete</a>";
                },
                "orderable": false,
                "targets": 'no-sort',
                "width": 100
            }
        ],
        pageLength: 15,
        dom: 'Bfrtip',
        buttons:
            [
                {
                    extend: 'csvHtml5',
                    text: 'Export',
                    filename: 'ConfigList',
                    messageTop: null,
                    title: null,
                    exportOptions:
                    {
                        columns: [1, 2, 3, 4, 5, 6]
                    }
                }
            ]
    });

});

function deleteSelectedFee(id) {
    var methodUrlDelete = "/FeeTypeMapping/Delete";
    // Show a warning
    var message = "Are you sure you want to delete selected item?";
    // TODO: use jquery UI for the popups
    if (window.confirm(message) === true) {
        // Attempt to delete
        $.get(methodUrlDelete, { id: id })
            .done(function (data) {
                //Refresh Data
                dataTable.ajax.reload(function () {
                });
            })
            .fail(function (xhr, textStatus, errorThrown) {

                alert("Error while Deleting item: Error code " + xhr.responseText + ". Please check your connection and reload the page, or contact support if it persists.");

            });

    }
}

