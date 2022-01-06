$(document).ready(function () {

    $.extend(true, $.fn.dataTable.defaults, {
        dom:
            "<'row be-datatable-header'<'col-sm-6'l><'col-sm-6'f>>" +
            "<'row be-datatable-body'<'col-sm-12'tr>>" +
            "<'row be-datatable-footer'<'col-sm-5'i<'select-options'>><'col-sm-7'p>>"
    });
    var dataTable = showFeeData();
    // Set up the checkboxes added To Invoice or Cancel based on User Selected
    processPendingFeeSelectedList(dataTable, $("#pendingfees-table"), "/PendingFee/ProcessSelectedAsync", "/PendingFee/CancelSelected");

  
   

});

var showFeeData =
    function () {

        var outlet = $("#OutletCode option:Selected").text();
        var accessKey = $("#AccessKey").val();
        var selectedText = $("#SelectedText").val();
        var feeStatusNew = $("#FeeStatus option:Selected").val();
        var category = $("#Category option:Selected").val();
        var amendedColumnVisible = category.toLowerCase().indexOf("amend") !== -1 || category === "" ? true : false;

        var FolderScopeCodeColor = "Violet";
        var SegmentScopeCodeColor = "SlateBlue";
        var DurationScopeCodeColor = "Orange";

        $("#FolderScopeCode").css("background-color", FolderScopeCodeColor);
        $("#SegmentScopeCode").css("background-color", SegmentScopeCodeColor);
        $("#DurationScopeCode").css("background-color", DurationScopeCodeColor);
        
        // This is the processing for the pendingfees listing.
        var dataTable = $("#pendingfees-table").DataTable({
            "dom": '<Blfi<t>p>',
            "ajax": {
                url: "/PendingFee/PendingFee_Search",
                type: "POST",
                async: true,
                datatype: "json",
                "data": function (d) {
                    d.outlet = outlet,
                        d.accessKey = accessKey,
                        d.feeStatus = feeStatusNew,
                        d.SelectedText = selectedText,
                        d.Category = category;
                }
            },
            "destroy": true,
            "serverSide": true,
            "paging": false,
            "bFilter": true,
            "order": [0, "asc"],
            "processing": true,
            "language": { "processing": "Processing, please wait" },
            "autoWidth": false,
            "columns": [
                {
                    "data": "Id",
                    "render": function (data, type, row, meta) {
                        if (type === "sort" || type === "type") {
                            return data;
                        }
                        var foreColor = row.Scope === "F" ? FolderScopeCodeColor : row.Scope === "S" ? SegmentScopeCodeColor : DurationScopeCodeColor;

                        return row.FeeValue === 0 ? '<font color=' + foreColor + '>' + row.Status + '</font>' : row.EnableRowSelection === false ?
                            '<font color=' + foreColor + '>' + row.Status + '</font>' :
                            '<label class="custom-control custom-checkbox custom-control-inline"><input class="chkBx-col-id-row-level custom-control-input" type="checkbox" data-item-id="' + data + '" /><span class="custom-control-label custom-control-color"></span></label>';
                    },
                    "orderable": false,
                    "targets": 'no-sort',
                    "width": 140
                },
                {
                    "data": "CompanyName",
                    "width": 200
                },
                { "data": "FolderNumber" },
                {
                    "data": "Status",
                    "visible": false
                },
                {
                    "data": "BranchCode",
                    "visible": false
                },
                {
                    "data": "FolderCreation",
                    "render": function (data, type, row, meta) {
                        if (data === null) { return data; } else {
                            return moment(data).utc(false).format("DD/MM/YYYY HH:mm");
                        }
                    },
                    "visible": true
                },
                {
                    "data": "DateStamp",
                    "render": function (data, type, row, meta) {
                        return row.Description.toLowerCase().indexOf("amend") !== -1 ? moment(data).utc(false).format("DD/MM/YYYY") : "";
                    },
                    "width": 100,
                    "visible": amendedColumnVisible
                },
                {
                    "data": "ActionDate",
                    "render": function (data, type, row, meta) {
                        if (data === null) { return data; } else {
                            return moment(data).utc(false).format("DD/MM/YYYY");
                        }
                    },
                    "visible": true
                },
                {
                    "data": "FolderOwner",
                    "width": 180
                },
                {
                    "data": "Description",
                    "width": 250
                },
                {
                    "data": "Id",
                    "render": function (data, type, row, meta) {
                        if (type === "sort" || type === "type") {
                            return data;
                        }
                        var foreColor = row.Scope === "F" ? FolderScopeCodeColor : row.Scope === "S" ? SegmentScopeCodeColor : DurationScopeCodeColor;

                        return row.FeeValue === 0 && row.Status !== "Cancelled" ? "<a href='/Fee/Create?Id=" + data + "' style='color:" + foreColor+";'>Add Fee " + row.FeeType + "</a>" :
                            '<font color=' + foreColor+'><b>' + row.FeeType + '</b></font>';
                    },
                    "orderable": false,
                    "targets": 'no-sort',
                    "width": 150
                },
                {
                    "data": "FeeValue",
                    "render": $.fn.dataTable.render.number(',', '.', 2, '$')

                },
               
                {
                    "data": "Category",
                    "visible": false
                },
                {
                    "data": "Id",
                    "render": function (data, type, row, meta) {
                        if (type === "sort" || type === "type") {
                            return data;
                        }
                        return row.FeeValue === 0 && row.Status !== "Cancelled" ? "<a class='btn-sm btn-danger' onclick=cancelSelectedPendingFee('" + data + "')>Delete</a>" : "";
                    },
                    "orderable": false,
                    "targets": 'no-sort',
                    "width": 100
                },
                {
                    "data": "EnableRowSelection",
                    "visible": false
                },
                {
                    "data": "CancellationReason",
                    "render": function (data, type, row, meta) {
                        if (row.Status.toLowerCase() === "new") {
                            return "";
                        }
                        else {
                            return "<font color=red>" + data + "</font>";
                        }
                    },
                    "orderable": false,
                    "targets": 'no-sort',
                    "width": 100
                }

            ],
            buttons:
            [
                {
                    extend: 'csvHtml5',
                    text: 'Export',
                    filename: 'PendingFeeList',
                    messageTop: null,
                    title: null,
                    exportOptions:
                    {
                        columns: [1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11]
                    }
                }
            ]
        });

        return dataTable;
    };
function cancelSelectedPendingFee(id) {
    var table = $('#pendingfees-table').DataTable();
    var postMethodUrlDelete = "/PendingFee/CancelSelected";
    // Show a warning
    var message = "Are you sure you want to delete selected item?";
    // TODO: use jquery UI for the popups
    if (window.confirm(message) === true) {
        // Attempt to delete
        $.post(postMethodUrlDelete, { ids: id })
            .done(function (data) {
                //Refresh Data
                table.ajax.reload(function () {
                });
            })
            .fail(function (xhr, textStatus, errorThrown) {

                alert("Error while Deleting item: Error code " + xhr.responseText + ". Please check your connection and reload the page, or contact support if it persists.");

            });

    }
}