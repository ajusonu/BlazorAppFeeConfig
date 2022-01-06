$(document).ready(function () {
    $("#Id").prop("readonly", true);
    $("#Id").css("background", "LightGrey");

    $("#OutletCode").prop("readonly", true);
    $("#OutletCode").css("background", "LightGrey");

    $("#Description").prop("readonly", true);
    $("#Description").css("background", "LightGrey");

    if (parseInt($("#Id").val()) !== 0) {
        $("#BookingType").prop("readonly", true);
        $("#BookingType").css("background", "LightGrey");

        $("#FeeType").prop("readonly", true);
        $("#FeeType").css("background", "LightGrey");

        $("#QueryType").prop("readonly", true);
        $("#QueryType").css("background", "LightGrey");
    } else {

        $("#FeeType").prop("readonly", false);
        $("#FeeType").css("background", "white");

        $("#BookingType").prop("readonly", false);
        $("#BookingType").css("background", "white");

        $("#QueryType").prop("readonly", false);
        $("#QueryType").css("background", "white");
    }

    var scope = $("#Scope").val();
    if (scope !== "") {
        var i = scope.length;
        var tCode = "";
        var tValue = "";
        while (i--) {
            tCode = scope.charAt(i);
            $("#ddlSelScope > option").each(function () {
                if (tCode === this.value) {
                    tValue = this.text;
                    managefeeScopeSelections(tCode, tValue);
                }
            });
        }
    }
    $("#QueryType").on('change', function () {
        $("#Description").val($("#QueryType option:selected").text());
    });
    $("#FeeType").on('change', function () {
        var desc = $("#FeeType option:selected").text();
        var bookingType = "";
        bookingType = desc.indexOf("Online Assisted") > 0 && bookingType === "" ? "Online Assisted" : bookingType;
        bookingType = desc.indexOf("Online") > 0 && bookingType === "" ? "Online" : bookingType;
        bookingType = desc.indexOf("Offline") > 0 && bookingType === "" ? "Offline" : bookingType;
        $("#BookingType").val(bookingType);
    });
    $("#ddlSelScope").on('change', function () {
        var name = $("#ddlSelScope option:selected").text();
        var val = $("#ddlSelScope").val();
        managefeeScopeSelections(val, name);

    });
    //add user selection of traveller - check for duplicates
    function managefeeScopeSelections(tCode, tName) {
        if ($("#selectedScopeCodes .feeScope").length && $("#selectedScopeCodes").find("[data-feescopecode='" + tCode + "']").length > 0) {
            return false;
        }
        var div = $('<div />').html(tName).attr("data-feescopecode", tCode).attr('class', 'feeScope');
        var anchor = $('<a />').attr('class', 'remove').attr("title", "Remove").attr("href", "#").text(" ").on("click", function (e) {
            $(this).parent().fadeOut(300, function () { $(this).remove(); updateSelectedScopeCodes(); });
            e.preventDefault();
        });
        div.append(anchor);
        $("#selectedScopeCodes").append(div);
        updateSelectedScopeCodes();
    }

    //Populate SelectedScopeCodes into hidden field
    function updateSelectedScopeCodes() {
        var selectedScopeCodes = [];
        $("#selectedScopeCodes .feeScope").each(function () {

            selectedScopeCodes.push($(this).data("feescopecode"));
        });
        $("#ScopeCodes").val(selectedScopeCodes.join());
        $("#Scope").val($("#ScopeCodes").val().replace(",", ""));
        $("#ddlSelScope").val("");
    }
});
