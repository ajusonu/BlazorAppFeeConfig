$(document).ready(function () {
    var pricingProfile = $('#PricingProfile').val();
    var companyId = $('#CompanyId').val();
    var divCheckBoxes = document.getElementById("divCheckBoxes");
    if (companyId !== "0" && pricingProfile !== "") {
        $("#divCheckBoxes").show();
        $("#ValidateConfigType").html("<span class='text-danger'>Please select Fee Config Type required : Is it by Profile or by Company Id <span>");
    } else {
        $("#divCheckBoxes").hide();
    }
    //show only on edit
    //var divFeeTypeDescription = document.getElementById("DivFeeTypeDescription");
    if ($('#EditMode').val() === "True") {
        $("#DivFeeTypeDescription").show();
        $("#Description").prop("readonly", true);
        $("#Description").css("background", "LightGrey");

     
    } else {
        $("#DivFeeTypeDescription").hide();

    }
    if (!$('#EditAutoApply').prop("checked")) {
        $("#DivAutoApply").hide();

    }
    $(function () {
        $(function validateForm(event) {

            if ($('#CompanyId').val() !== "0" && $('#PricingProfile').val() !== "") {
                //event.preventDefault();
                return false;

            }
            else {
                if ($('#Id').val() === "0") {
                    $.post("@Url.Action('ValidatingFee', 'Fee')",
                        {
                            pricingProfile: $('#PricingProfile').val(),
                            companyId: $('#CompanyId').val(),
                            outletCode: $('#OutletCode').val(),
                            branchCode: $('#BranchCode').val(),
                            feeType: $('#FeeType').val()
                        })
                        .done(function (data) {
                            if (data !== "") {
                                $("#ValidateStatus").html("<font color='red'>" + data + "Not a valid entry<font>");
                                event.preventDefault();
                            }
                            else {
                                $("#ValidateStatus").html("<font color='green'><font>");
                            }
                        });
                }
                else {
                    //For Edit Disable ability to edit main Fields
                    $("#FeeType").prop("readonly", true);
                    $("#FeeType").css("background", "LightGrey");

                    $("#CompanyId").prop("readonly", true);
                    $("#CompanyId").css("background", "LightGrey");

                    $("#PricingProfile").prop("readonly", true);
                    $("#PricingProfile").css("background", "LightGrey");
                }
            }
        });
    });
    $("#FeeType").on('change', function () {
        validateFee();
    });
    $("#FeePerFolder").on('input', function () {
        validateFeeValue();
    });
    $("#FeePerSegment").on('input', function () {
        validateFeeValue();
    });
    $("#ConfigType").on('change', function () {

        if ($("#ConfigType").val() === "1") { $('#CompanyId').val("0"); }
        if ($("#ConfigType").val() === "2") { $('#PricingProfile').val(""); }

        var pricingProfile = $('#PricingProfile').val();
        var companyId = $('#CompanyId').val();

        if (companyId !== "0" && pricingProfile !== "") {
            $("#ValidateConfigType").html("<span class='text-danger'>Please select Fee Config Type required : Is it by Profile or by Company Id <span>");
        } else {
            $("#ValidateConfigType").html("<span class='text-danger'><span>");
        }

    });

    $("#OutletCode").prop("readonly", true);
    $("#OutletCode").css("background", "LightGrey");

    //$("#BranchCode").prop('disabled', true);
    $("#BranchCode").prop("readonly", true);
    $("#BranchCode").css("background", "LightGrey");

    //On Profile code change
    $("#PricingProfile").on('input', function () {
        if ($('#PricingProfile').val() !== "") {
            //$("#CompanyId").prop('disabled', true);
            $("#CompanyId").prop("readonly", true);
            $("#CompanyId").css("background", "LightGrey");
            $('#CompanyId').val("0");
        }
        else {
            $("#CompanyId").prop("readonly", false);
            $("#CompanyId").css("background", "white");

        }
    });
    //On Company Id change event
    $("#CompanyId").on('input', function () {
        if ($('#CompanyId').val() !== "0") {
            $("#PricingProfile").prop("readonly", true);
            $("#PricingProfile").css("background", "LightGrey");
            $('#PricingProfile').val("");
        }
        else {
            $("#PricingProfile").prop("readonly", false);
            $("#PricingProfile").css("background", "white");
        }
    });
});
function validateFee() {
    if ($("#FeeType").val() !== "") {
        $("#ValidateStatus").html("Validating...");
        $.post("@Url.Action('ValidatingFee', 'Fee')",
            {
                pricingProfile: $('#PricingProfile').val(),
                companyId: $('#CompanyId').val(),
                outletCode: $('#OutletCode').val(),
                branchCode: $('#BranchCode').val(),
                feeType: $('#FeeType').val()
            })
            .done(function (data) {
                if (data !== "") {
                    $("#ValidateStatus").html("<span class='text-danger'>" + data + "Not a valid entry<span>");
                }
                else {
                    $("#ValidateStatus").html("<font color='green'><font>");
                }
            });
    }

}
function validateFeeValue() {
    //if (parseInt($("#FeePerFolder").val()) !== 0) {
    //    $("#FeePerSegment").prop("readonly", true);
    //    $("#FeePerSegment").css("background", "LightGrey");
    //} else {
    //    $("#FeePerSegment").prop("readonly", false);
    //    $("#FeePerSegment").css("background", "white");
    //}
    //if (parseInt($("#FeePerSegment").val()) !== 0) {
    //    $("#FeePerFolder").prop("readonly", true);
    //    $("#FeePerFolder").css("background", "LightGrey");
    //} else {
    //    $("#FeePerFolder").prop("readonly", false);
    //    $("#FeePerFolder").css("background", "white");
    //}
    if (parseInt($("#FeePerFolder").val()) + parseInt($("#FeePerSegment").val()) + parseInt($("#FeePerDuration").val()) === 0) {
        $("#ValidateStatus").html("<span class='text-danger'>Please enter one Fee Value<span>");
    } else {
        $("#ValidateStatus").html("<font color='green'><font>");
    }
}
