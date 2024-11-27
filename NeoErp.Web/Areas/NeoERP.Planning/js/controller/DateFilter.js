

var DateFilter = function () {
    return {

        init: function (callback) {
            DateFilter.loadDateFilter("#ddlDateFilterVoucher", callback);

            $("#ddlDateFilterVoucher").change(function () {
                if ($('option:selected', this).val() != "Custom") {
                    $("#FromDateVoucher").val(moment($('option:selected', this).attr("data-start-date")).format("YYYY-MMM-DD"));
                    $("#ToDateVoucher").val(moment($('option:selected', this).attr("data-end-date")).format("YYYY-MMM-DD"));
                }
                $('#fromInputDateVoucher').val(AD2BS(moment($("#FromDateVoucher").val()).format("YYYY-MM-DD")));
                $('#toInputDateVoucher').val(AD2BS(moment($("#ToDateVoucher").val()).format("YYYY-MM-DD")));
            });
            $('#fromInputDateVoucher').nepaliDatePicker({
                ndpEnglishInput: 'FromDateVoucher',
                npdMonth: true,
                npdYear: true,
                npdYearCount: 20,
                onChange: function () {

                    //var fromdate = $('#fromInputDateVoucher').val();
                    //var todate = $('#toInputDateVoucher').val();
                    //if (fromdate < todate) {
                    //    displayPopupNotification("End date is greater than start date.", "error");
                    //    $('#fromInputDateVoucher').val(getNepaliDate());
                    $('#ddlDateFilterVoucher').val("Custom");
                    $("#FromDateVoucher").val(moment(BS2AD($("#fromInputDateVoucher").val())).format("YYYY-MMM-DD"))
                    //}
                },

            });
            $('#toInputDateVoucher').nepaliDatePicker({
                ndpEnglishInput: 'ToDateVoucher',
                npdMonth: true,
                npdYear: true,
                npdYearCount: 20,
                onChange: function () {
                    //var fromdate = $('#fromInputDateVoucher').val();
                    //var todate = $('#toInputDateVoucher').val();
                    //if (fromdate > todate) {
                    //    displayPopupNotification("End date is greater than start date.", "error");
                    //    $('#toInputDateVoucher').val(getNepaliDate());
                    $('#ddlDateFilterVoucher').val("Custom");
                    $("#ToDateVoucher").val(moment(BS2AD($("#toInputDateVoucher").val())).format("YYYY-MMM-DD"))
                    //}
                }
            });
            var current_date = getNepaliDate();
            $('#fromInputDateVoucher').val(current_date);
            $('#toInputDateVoucher').val(current_date);
            $("#FromDateVoucher").datepicker({
                dateFormat: 'yy-M-d', changeMonth: true,
                changeYear: true,
                showButtonPanel: true,


            });
            $("#ToDateVoucher").datepicker({
                dateFormat: 'yy-M-d', changeMonth: true,
                changeYear: true,
                showButtonPanel: true,

            });

            //change
            $('#FromDateVoucher').val(moment(BS2AD($("#fromInputDateVoucher").val())).format("YYYY-MMM-DD"));
            $('#ToDateVoucher').val(moment(BS2AD($("#toInputDateVoucher").val())).format("YYYY-MMM-DD"));
            $('#FromDateVoucher').change(function () {

                $('#ddlDateFilterVoucher').val("Custom");
                $('#fromInputDateVoucher').val(AD2BS(moment($('#FromDateVoucher').val()).format("YYYY-MM-DD")));
            });
            $('#ToDateVoucher').change(function () {
                $('#ddlDateFilterVoucher').val("Custom");
                $('#toInputDateVoucher').val(AD2BS(moment($('#ToDateVoucher').val()).format("YYYY-MM-DD")));
            });
            var formNepali = $('#fromInputDateVoucher').val();
            var toNepali = $('#toInputDateVoucher').val();
            var FormEnglish = $('#FromDateVoucher').val();
            var toEnglish = $('#ToDateVoucher').val();
            var stringFormat = formNepali + "(" + FormEnglish + ")  To " + toNepali + "(" + toEnglish + ")"
            $("#spandate").html(stringFormat);
            $(".date-tooltip").attr("data-original-title", $("#spandate").html());

        },




        loadDateFilter: function (selector, callback) {
            $.support.cors = true;
            var current_date = getNepaliDate();
            var date = current_date.split("-");
            var currentFiscalYear = (parseInt(date[0]) - 1).toString() + "/" + date[0].toString().substring(2, date[0].toString().length);
            var readUrl = window.location.protocol + "//" + window.location.host + "/api/Common/GetDateFilters?fiscalYear=" + currentFiscalYear;

            $.ajax({
                type: "GET",
                url: readUrl,
                beforeSend: function () {
                    $(selector).html("<option> loading ... </option>");
                },
                success: function (data) {
                    $(selector).empty();
                    $.each(data, function (i) {
                        //function for the selected week
                        //if (i.RangeName === 'Last Week')
                        //{
                        //    $('#FromDateVoucher').val(data[i].StartDateString);
                        //    $('#ToDateVoucher').val(data[i].EndDateString);

                        //    $(selector).append($("<option selected='true'></option>")
                        //              .attr("value", data[i].RangeName).attr("data-start-date", data[i].StartDateString).attr("data-end-date", data[i].EndDateString)
                        //              .text(data[i].RangeName));
                        //} else {
                        //    $(selector).append($("<option></option>")
                        //              .attr("value", data[i].RangeName).attr("data-start-date", data[i].StartDateString).attr("data-end-date", data[i].EndDateString)
                        //              .text(data[i].RangeName));
                        //}
                        $(selector).append($("<option></option>")
                            .attr("value", data[i].RangeName).attr("data-start-date", data[i].StartDateString).attr("data-end-date", data[i].EndDateString)
                            .text(data[i].RangeName));
                    });

                    var ddlselector = $(selector + " option[value='This Month']")
                    ddlselector.prop('selected', true);
                    //will get the selected date in this area
                    //$('#ddlDateFilterVoucher').trigger('change');
                    $('#FromDateVoucher').val(moment(ddlselector.attr("data-start-date")).format("YYYY-MMM-DD"));
                    $('#ToDateVoucher').val(moment(ddlselector.attr("data-end-date")).format("YYYY-MMM-DD"));
                    $('#fromInputDateVoucher').val(AD2BS(moment($("#FromDateVoucher").val()).format("YYYY-MM-DD")));
                    $('#toInputDateVoucher').val(AD2BS(moment($("#ToDateVoucher").val()).format("YYYY-MM-DD")));

                    //selected date reflected on the top righ panel 
                    var formNepali = $('#fromInputDateVoucher').val();
                    var toNepali = $('#toInputDateVoucher').val();
                    var FormEnglish = $('#FromDateVoucher').val();
                    var toEnglish = $('#ToDateVoucher').val();
                    var stringFormat = formNepali + "(" + FormEnglish + ")  To " + toNepali + "(" + toEnglish + ")"
                    $("#spandate").html(stringFormat);
                    $(".date-tooltip").attr("data-original-title", $("#spandate").html());
                    $("#applydp").on("click", function () {
                        var formNepali = $('#fromInputDateVoucher').val();
                        var toNepali = $('#toInputDateVoucher').val();
                        var FormEnglish = $('#FromDateVoucher').val();
                        var toEnglish = $('#ToDateVoucher').val();
                        var stringFormat = formNepali + "(" + FormEnglish + ")  To " + toNepali + "(" + toEnglish + ")"
                        $("#spandate").html(stringFormat);
                        $(".date-tooltip").attr("data-original-title", $("#spandate").html());
                        //$('#form_modal2').modal('toggle');
                        $('#form_modal2').modal('hide');

                        // $('#form_modal2').modal('disabled', 'disabled');
                    });


                    //$("#applydp").on("click", function () {

                    //    var formNepali = $('#fromInputDateVoucher').val();
                    //    var toNepali = $('#toInputDateVoucher').val();
                    //    var FormEnglish = $('#FromDateVoucher').val();
                    //    var toEnglish = $('#ToDateVoucher').val();
                    //    var stringFormat = formNepali + "(" + FormEnglish + ")  To " + toNepali + "(" + toEnglish + ")"
                    //    $("#spandate").html(stringFormat);
                    //    $('#form_modal2').modal('toggle');
                    //    //once the modal apply then it will desabled the mod

                    //    //$('#form_modal2').modal('disabled', 'disabled');
                    //});



                    if (callback != undefined) {
                        callback();
                    }

                },
                error: function (xhr, status, error) {
                    $(selector).html("<option> Error </option>");
                }
            });
        }

    };


}();

var DatePicker = function () {
    return {
        appendText: false,
        appendToText: "",
        init: function (append, text) {
            $("#ddlDatePickerFilter").change(function () {
                if ($('option:selected', this).val() !== "CT") {
                    $("#datePickerAd").val($('option:selected', this).attr("data-end-date"));
                }
                $('#datePickerBs').val(AD2BS($("#datePickerAd").val()));
            });
            $('#datePickerBs').nepaliDatePicker({
                ndpEnglishInput: 'datePickerAd'

            });
            $('#datePickerBs').nepaliDatePicker({
                ndpEnglishInput: 'ToDateVoucher',
                changeMonth: true,
                changeYear: true,
                showButtonPanel: true,
            });
            var current_date = getNepaliDate();
            $('#datePickerBs').val(current_date);
            $("#datePickerAd").datepicker({
                dateFormat: 'yy-mm-dd', changeMonth: true,
                changeYear: true,
                showButtonPanel: true,
            });

            $('#datePickerAd').val(BS2AD($("#datePickerBs").val()));

            $('#datePickerAd').change(function () {
                $('#datePickerBs').val(AD2BS($('#datePickerAd').val()));
            });

            DatePicker.appendText = (append === "True");
            DatePicker.appendToText = text;
            DatePicker.loadDateFilter("#ddlDatePickerFilter");
        },
        loadDateFilter: function (selector) {
            $.support.cors = true;
            var current_date = getNepaliDate();
            var date = current_date.split("-");
            var currentFiscalYear = (parseInt(date[0]) - 1).toString() + "/" + date[0].toString().substring(2, date[0].toString().length);
            var readUrl = window.location.protocol + "//" + window.location.host + "/api/Common/GetDateFilters?fiscalYear=" + currentFiscalYear;
            if (DatePicker.appendText) {
                readUrl += "&appendText=true&textToAppend=" + DatePicker.appendToText;
            }
            $.ajax({
                type: "GET",
                url: readUrl,
                beforeSend: function () {
                    $(selector).html("<option> loading ... </option>");
                },
                success: function (data) {
                    $(selector).empty();
                    $.each(data, function (i) {
                        $(selector).append($("<option></option>")
                            .attr("value", data[i].RangeName).attr("data-start-date", data[i].StartDateString).attr("data-end-date", data[i].EndDateString)
                            .text(data[i].RangeName));
                    });
                },
                error: function (xhr, status, error) {
                    $(selector).html("<option> Error </option>");
                }
            });
        }

    };
}();