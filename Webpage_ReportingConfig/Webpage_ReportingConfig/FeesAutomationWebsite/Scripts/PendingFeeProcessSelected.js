var initializeinvoiceSelectedList =
    function (dataTable, table, tagForInvoiceUrl, tagForCancelUrl) {
        // When the select all checkbox is checked/unchecked
        $("#chkBx-Select-all").change(function () {
            // IF it was checked
            if (this.checked) {
                // Check all the individual checkboxes
                $(".chkBx-col-id-row-level").prop('checked', true);
                return;
            }
            // Otherwise uncheck all of the individual checkboxes
            $(".chkBx-col-id-row-level").prop('checked', false);
            // And disable the To Invoice option as no rows are selected anymore
            // $("#btn-queue-selected").addClass("disabled");
        });

        table.on('change', '.btn-queue-selected', function () {
            // If the checkbox was checked
            if (this.checked) {
                // Check to see if all checkboxes are now checked
                if ($('.chkBx-col-id-row-level:checked').length === $('.To Invoice-select').length) {
                    // IF so, check the select all checkbox so they stay in sync
                    $("#btn-queue-selected").prop('checked', true);
                }
                return;
            }
            // If a checkbox has been unchecked, make sure the select all is unchecked too to keep the checkbox in sync
            $("#btn-queue-selected").prop('checked', false);


        });
        
        // When the user wants to To Queue a row
        $("#btn-queue-selected").on("click", function () {
            // Grab the row ids from each selected row checkboxes (it's stored against data-row-id)
            var idsToTagForInvoice = $.map($(".chkBx-col-id-row-level:checked").filter(function (i, e)
            { return Number.isInteger($(this).data("item-id")); }), function (e, i)
                { return parseInt($(e).data("item-id")); });

            // Show a warning
            var message = idsToTagForInvoice.length === 1 ? "Are you sure you want to Invoice the selected item?" : "Are you sure you want to Invoice the " + idsToTagForInvoice.length + " selected items?";
            // TODO: use jquery UI for the popups
            if (window.confirm(message) === true) {
                // Attempt to tag Selected ids for Invoice
                $.post(tagForInvoiceUrl, { ids: idsToTagForInvoice })
                    .done(function (data) {
                        // If it was successfull, tell Datatables to update the current row list so the rows that were To Invoiced disappear
                        dataTable.ajax.reload(function () {
                            $("#btn-queue-selected").prop('checked', false);
                        });
                        //showFeeData("NEW");
                    })
                    .fail(function (xhr, textStatus, errorThrown) {
                        // TODO: show this error in the standard error alert on the layout (currently only possible to set that alert from codebehind)
                        alert("Error while Invoicing item: Error code " + xhr.status + ". Please check your connection and reload the page, or contact support if it persists.");
                    });
            }
        });
        // When the user wants to To Cancel a row
        $("#btn_cancel-selected").on("click", function () {
            // Grab the row ids from each selected row checkboxes (it's stored against data-row-id)
            var idsToTagForCancel = $.map($(".chkBx-col-id-row-level:checked").filter(function (i, e) { return Number.isInteger($(this).data("item-id")); }), function (e, i) { return parseInt($(e).data("item-id")); });

            // Show a warning
            var message = idsToTagForCancel.length === 1 ? "Are you sure you want to delete the selected item?" : "Are you sure you want to delete the " + idsToTagForCancel.length + " selected items?";
            // TODO: use jquery UI for the popups
            if (window.confirm(message) === true) {
                // Attempt to tag Selected ids for Invoice
                $.post(tagForCancelUrl, { ids: idsToTagForCancel })
                    .done(function (data) {
                        // If it was successfull, tell Datatables to update the current row list so the rows that were To Invoiced disappear
                        dataTable.ajax.reload(function () {
                            $("#btn-queue-selected").prop('checked', false);
                        });
                        //showFeeData("NEW");
                    })
                    .fail(function (xhr, textStatus, errorThrown) {
                        // TODO: show this error in the standard error alert on the layout (currently only possible to set that alert from codebehind)
                        alert("Error while Deleting item: Error code " + xhr.status + ". Please check your connection and reload the page, or contact support if it persists.");
                    });
            }
        });
    };