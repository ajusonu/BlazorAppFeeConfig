
var processPendingFeeSelectedList =
    function (dataTable, table, tagForInvoiceUrl, tagForCancelUrl) {

        $("#FeeStatus")
            .change(function () {
                var feeStatus = $("#FeeStatus option:Selected").text();
                //$('#pendingfees-table').DataTable().search("feeStatus=" + feeStatus).draw();
                showFeeData();

            });
        $("#Category")
            .change(function () {
                var category = $("#Category option:Selected").text();
                showFeeData();
            });
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
        function changeButtonStateBasedOnProcessing(processing) {
            if (processing) {
                //Un Check All Check Boxes
                $(".chkBx-col-id-row-level").prop('checked', false);
                // Disable apply and Cancel button till processing in not complete
                $("#btn-queue-selected").attr('disabled', true);
                $("#btn_cancel-selected").attr('disabled', true);

                $("#btn-queue-selected").removeClass("btn-success");
                $("#btn_cancel-selected").removeClass("btn-danger");
            }
            else {
                // Enable apply and Cancel button till processing in not complete
                $("#btn-queue-selected").attr('disabled', false);
                $("#btn_cancel-selected").attr('disabled', false);
                $("#btn-queue-selected").addClass("btn-success");
                $("#btn_cancel-selected").addClass("btn-danger");
            }

        }
        // When the user wants to To Queue a row
        $("#btn-queue-selected").on("click", function () {

            // Grab the row ids from each selected row checkboxes (it's stored against data-row-id)
            var idsToTagForInvoice = $.map($(".chkBx-col-id-row-level:checked").filter(function (i, e) {
                return parseInt($(this).data("item-id"));
            }), function (e, i) { return parseInt($(e).data("item-id")); });

            // Show a warning
            var message = idsToTagForInvoice.length === 1 ? "Are you sure you want to add a fee to selected item?" : "Are you sure you want to add a fee to the " + idsToTagForInvoice.length + " selected items?";
            // TODO: use jquery UI for the popups
            if (window.confirm(message) === true) {

                changeButtonStateBasedOnProcessing(true);
                // Attempt to tag Selected ids for Invoice
                $.post(tagForInvoiceUrl, {
                    ids: idsToTagForInvoice

                })
                    .done(function (data) {
                        if (data === "OK") {
                            showFeeData();
                            Notify("success", "Queued to add Fee to Folders", 5);
                        }
                        else {

                            Notify("Error", "Error while adding Fee: " + data
                                + ". Please check your connection and reload the page, or contact support if it persists.", 10);
                        }
                        changeButtonStateBasedOnProcessing(false);

                    })
                    .fail(function (xhr, textStatus, errorThrown) {
                        // TODO: show this error in the standard error alert on the layout (currently only possible to set that alert from codebehind)
                        if (xhr.responseText.indexOf("401") > 0) {
                            Notify("Error", "Error while adding Fee: Error - UnAuthorise Access or Session Time out" + ". Please check your connection and reload the page, or contact support if it persists.", 10);
                        }
                        else {
                            Notify("Error", "Error while adding Fee: Error code " + xhr.responseText + ". Please check your connection and reload the page, or contact support if it persists.", 10);
                        }
                        changeButtonStateBasedOnProcessing(false);
                    });
            }



        });
        function IsCheckBoxSelected(arrIndex, arrElement) {
            return $(arrElement).data("item-id");

        }
        // When the user wants to To Cancel a row
        $("#btn_cancel-selected").on("click", function () {
            // Grab the row ids from each selected row checkboxes (it's stored against data-row-id)
            var idsToTagForCancel = $.map($(".chkBx-col-id-row-level:checked").filter(function (i, e) { return parseInt($(this).data("item-id")); }), function (e, i) { return parseInt($(e).data("item-id")); });

            // Show a warning
            var message = idsToTagForCancel.length === 1 ? "Are you sure you want to delete the selected item?" : "Are you sure you want to delete the " + idsToTagForCancel.length + " selected items?";
            // TODO: use jquery UI for the popups
            if (window.confirm(message) === true) {
                // Disable apply and Cancel button till processing in not complete
                changeButtonStateBasedOnProcessing(true);

                // Attempt to tag Selected ids for Invoice
                $.post(tagForCancelUrl, { ids: idsToTagForCancel })
                    .done(function (data) {
                        if (data === "OK") {
                            showFeeData();
                            Notify("success", "Deleted the selected item", 5);
                        }
                        else {
                            Notify("Error", "Error while Deleting item: Error " + data + ". Please check your connection and reload the page, or contact support if it persists.", 10);

                        }
                        changeButtonStateBasedOnProcessing(false);
                    })
                    .fail(function (xhr, textStatus, errorThrown) {
                        // TODO: show this error in the standard error alert on the layout (currently only possible to set that alert from codebehind)
                        if (xhr.responseText.indexOf("401") > 0) {
                            Notify("Error", "Error while Deleting item: Error - UnAuthorise Access or Session Time out" + ". Please check your connection and reload the page, or contact support if it persists.", 10);
                        }
                        else {
                            Notify("Error", "Error while Deleting item: Error code " + xhr.responseText + ". Please check your connection and reload the page, or contact support if it persists.", 10);
                        }
                        changeButtonStateBasedOnProcessing(false);
                    });
            }
        });

    };

var Notify = function (className, messageText, autoHideDelayInSeconds) {
    $.notify(messageText,
        {
            position: "top center", clickToHide: true, autoHide: true, autoHideDelay: autoHideDelayInSeconds * 1000,
            // show the arrow pointing at the element
            arrowShow: true,
            className: className.toLowerCase()
        });
};
