var dataTable = "";
$(document).ready(function () {

    var FolderScopeCodeColor = "Violet";
    var SegmentScopeCodeColor = "SlateBlue";
    var DurationScopeCodeColor = "Orange";

    $("#FolderScopeCode").css("background-color", FolderScopeCodeColor);
    $("#SegmentScopeCode").css("background-color", SegmentScopeCodeColor);
    $("#DurationScopeCode").css("background-color", DurationScopeCodeColor);

    // This is the processing for the fees listing.
    dataTable = $("#fee-table").DataTable({
        "ajax": {
            "url": "/Fee/Fee_Search",
            "type": "GET"
        },
        "columns": [
            {
                "data": "Id",
                "render": function (data, type, row, meta) {
                    if (type === "sort" || type === "type") {
                        return data;
                    }
                    var bgColor = row.FeePerFolder > 0 ? FolderScopeCodeColor : row.FeePerSegment > 0 ? SegmentScopeCodeColor : DurationScopeCodeColor;

                    return "<a href='/Fee/Edit?Id=" + data + "'><font color=" + bgColor + ">Edit</font></a>";
                },
                "width": 50
            },
            {
                "data": "Id",
                "width": 50
            },
            {
                "data": "OutletCode",
                "width": 50
            },
            {
                "data": "BranchCode",
                "width": 50
            },
            {
                "data": "PricingProfile",
                "width": 75
            },
            {
                "data": "CompanyId",
                "width": 50
            },
            {
                "data": "FeeType",
                "width": 50
            },
            {
                "data": "Description",
                "width": 300
            },
            {
                "data": "FeePerSegment",
                "render": $.fn.dataTable.render.number(',', '.', 2, '$'),
                "width": 50
            },
            {
                "data": "FeePerFolder",
                "render": $.fn.dataTable.render.number(',', '.', 2, '$'),
                "width": 50
            },
            {
                "data": "FeePerDuration",
                "render": $.fn.dataTable.render.number(',', '.', 2, '$'),
                "width": 50
            },
            {
                "data": "IsActive",
                "width": 50,
                "visible": false
            },
            {
                "data": "Id",
                "render": function (data, type, row, meta) {
                    if (type === "sort" || type === "type") {
                        return data;
                    }
                    return row.EditAutoApply ?
                        row.AutoApply ? '<label class="custom-control custom-checkbox custom-control-inline"><input class="chkBx-col-id-row-level custom-control-input"  type="checkbox" data-item-id="' + data + '" checked/><span class="custom-control-label custom-control-color"></span></label>' :
                            '<label class="custom-control custom-checkbox custom-control-inline"><input class="chkBx-col-id-row-level custom-control-input"  type="checkbox" data-item-id="' + data + '" /><span class="custom-control-label custom-control-color"></span></label>'
                        : '';
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
                "width": 50
            },
            {
                "data": "EditAutoApply",
                "width": 50,
                "visible": false
            }
        ],
        "pageLength": 15,
        dom: 'Bfrtip',
        buttons:
            [
                {
                    extend: 'csvHtml5',
                    text: 'Export',
                    filename: 'FeeList',
                    messageTop: null,
                    title: null,
                    exportOptions:
                    {
                        columns: [1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 13]
                    }
                }
            ]
    });

});
function deleteSelectedFee(id) {
    var postMethodUrlDelete = "/Fee/Delete";
    // Show a warning
    var message = "Are you sure you want to delete selected item?";
    // TODO: use jquery UI for the popups
    if (window.confirm(message) === true) {
        // Attempt to delete
        $.post(postMethodUrlDelete, { id: id })
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
