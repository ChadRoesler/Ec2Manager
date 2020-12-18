// Please see documentation at https://docs.microsoft.com/aspnet/core/client-side/bundling-and-minification
// for details on configuring this project to bundle and minify static web assets.

// Write your JavaScript code.
$(function () {

    // Header Master Checkbox Event
    $("#masterCheck").on("click", function () {
        if ($("input:checkbox").prop("checked")) {
            $("input:checkbox[name='row-check']").prop("checked", true);
        } else {
            $("input:checkbox[name='row-check']").prop("checked", false);
        }
    });

    // Check event on each table row checkbox
    $("input:checkbox[name='row-check']").on("change", function () {
        var total_check_boxes = $("input:checkbox[name='row-check']").length;
        var total_checked_boxes = $("input:checkbox[name='row-check']:checked").length;

        // If all checked manually then check master checkbox
        if (total_check_boxes === total_checked_boxes) {
            $("#masterCheck").prop("checked", true);
        }
        else {
            $("#masterCheck").prop("checked", false);
        }
    });

    $("#enableConfirm").on("click", function () {
        var modal = $("#enableModal").find(".modal-body");
        $("input:checkbox[name='row-check']:checked").each(function (index, instance) {
            $(modal).append("<input name='instancesArray' value='{\"Id\":\"" + instance.id.split("|")[0] + "\", \"Account\": \"" + instance.id.split("|")[1] + "\"}'  type='hidden' />");
        });
        //$("#instanceTable > tbody > tr").each(function (index, tr) {
        //    console.log(index);
        //    console.log(tr);
        //    var input = $(tr).find($("input:checkbox[name='row-check']").prop("checked"));
        //    console.log(input);
        //});
    });
    $("#rebootConfirm").on("click", function () {
        var modal = $("#rebootModal").find(".modal-body");
        $("input:checkbox[name='row-check']:checked").each(function (index, instance) {
            $(modal).append("<input name='instancesArray' value='{\"Id\":\"" + instance.id.split("|")[0] + "\", \"Account\": \"" + instance.id.split("|")[1] + "\"}'  type='hidden' />");
        });
        //$("#instanceTable > tbody > tr").each(function (index, tr) {
        //    console.log(index);
        //    console.log(tr);
        //    var input = $(tr).find($("input:checkbox[name='row-check']").prop("checked"));
        //    console.log(input);
        //});
    });
    $("#stopConfirm").on("click", function () {
        var modal = $("#stopModal").find(".modal-body");
        $("input:checkbox[name='row-check']:checked").each(function (index, instance) {
            $(modal).append("<input name='instancesArray' value='{\"Id\":\"" + instance.id.split("|")[0] + "\", \"Account\": \"" + instance.id.split("|")[1] + "\"}'  type='hidden' />");
        });
        //$("#instanceTable > tbody > tr").each(function (index, tr) {
        //    console.log(index);
        //    console.log(tr);
        //    var input = $(tr).find($("input:checkbox[name='row-check']").prop("checked"));
        //    console.log(input);
        //});
    });
    //$("#enableConfirm").on("click", function () {

    //    // find the modal body
    //    var modal = $("#enableModal").find(".modal-body");
    //    var elements = document.getElementsByName("row-check");
    //    elements.each(function () {
    //        $(modal).append("<input name='instancesArray' value='" + $(this).val() + "'  type='hidden' />")
    //    });
    //    // loop through all the check boxes (class checkbox)
    //    //$("input:checkbox[name='row-check']").each(function (index) {

    //    //    // if they are checked, add them to the modal
    //    //    var instanceId = $("input:checkbox[name='row-check']").val;
    //    //    if ($("input:checkbox[name='row-check']").prop("checked")) {

    //    //        // add a hidden input element to modal with article ID as value
    //    //        $(modal).append("<input name='instancesArray' value='" + instanceId + "'  type='hidden' />")
    //    //    }
    //    //});
    //});
});