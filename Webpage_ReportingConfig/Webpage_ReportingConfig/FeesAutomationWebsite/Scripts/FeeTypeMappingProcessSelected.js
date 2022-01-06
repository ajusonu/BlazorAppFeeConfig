// When the user wants to To Update a Select Row AutoApply
$("#btn-update-autoapply-selected").on("click", function () {

    // Grab the row ids from each selected row checkboxes (it's stored against data-row-id)
    var idsChecked = $.map($(".chkBx-col-id-row-level:checked").filter(function (i, e) {
        return parseInt($(this).data("item-id"));
    }), function (e, i) { return parseInt($(e).data("item-id")); });

    var idsUnchecked = $.map($(".chkBx-col-id-row-level:not(:checked)").filter(function (i, e) {
        return parseInt($(this).data("item-id"));
    }), function (e, i) { return parseInt($(e).data("item-id")); });

    // Show a warning
    var message = "Are you sure you want to update Auto Apply?";
    // TODO: use jquery UI for the popups
    if (window.confirm(message) === true) {

        // Attempt to tag Selected ids for Invoice
        $.post("/FeeTypeMapping/UpdateAutoApply/", {
            selectedIds: idsChecked,
            unSelectedIds: idsUnchecked

        })
            .done(function (data) {
                if (data.toLowerCase().indexOf("failed") > -1) {
                    Notify("error", data, 10);
                }
                else {
                    Notify("success", "Updated Successfully", 5);
                }
            })
            .fail(function (xhr, textStatus, errorThrown) {
                // TODO: show this error in the standard error alert on the layout (currently only possible to set that alert from codebehind)
                if (xhr.responseText.indexOf("401") > 0) {
                    Notify("error", "Error while Updating Auto Apply: Error - UnAuthorise Access or Session Time out" + ". Please check your connection and reload the page, or contact support if it persists.", 10);
                }
                else {
                    Notify("error", "Error while Updating Auto Apply: Error code " + xhr.responseText + ". Please check your connection and reload the page, or contact support if it persists.", 10);
                }
            });
    }

});
function Notify(className, messageText, autoHideDelayInSeconds) {
    $.notify(messageText,
        {
            position: "top center", clickToHide: true, autoHide: true, autoHideDelay: autoHideDelayInSeconds * 1000,
            // show the arrow pointing at the element
            arrowShow: true,
            className: className.toLowerCase()
        });
}