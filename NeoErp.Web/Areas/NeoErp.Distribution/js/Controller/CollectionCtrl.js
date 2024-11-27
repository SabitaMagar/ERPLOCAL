
distributionModule.controller('CollectionCtrl', function ($scope, DistSetupService, $routeParams) {
    $scope.isViewLoading = false;

    $scope.$on('$routeChangeStart', function () {
        $scope.isViewLoading = true;
    });

    $scope.$on('$routeChangeSuccess', function () {
        $scope.isViewLoading = false;
    });

    $scope.$on('$routeChangeError', function () {
        $scope.isViewLoading = false;
    });

    getDateFormat = function (date) {
        var test = new Date(date);
        if (test.getFullYear() == '1970')
            return '-';
        return kendo.format("{0:" + reportConfig.dateFormat + "}", new Date(date));
    }

    function bindGrid() {
        reportConfig = GetReportSetting("CollectionsReport");
        $("#grid").kendoGrid({
            dataSource: {
                type: "json",
                transport: {
                    read: {
                        url: window.location.protocol + "//" + window.location.host + "/api/Distribution/GetCollectionsSales", // <-- Get data from here.
                        dataType: "json", // <-- The default was "jsonp".
                        type: "POST",
                        contentType: "application/json; charset=utf-8"
                    },
                    parameterMap: function (options, type) {
                        var paramMap = JSON.stringify($.extend(options, ReportFilter.filterAdditionalData()));
                        delete paramMap.$inlinecount; // <-- remove inlinecount parameter.
                        delete paramMap.$format; // <-- remove format parameter.
                        return paramMap;
                    }
                },

                error: function (e) {
                    displayPopupNotification("Sorry error occured while processing data", "error");
                },
                //serverFiltering: false,
                //serverAggregates: true,
                schema: {
                    data: "collectionViewModels", // records are returned in the "data" field of the response
                    total: "total", // total number of records is in the "total" field of the response
                    aggregates: "AggregationResult",
                },
                model: {
                    fields: {
                        CREATED_DATE: { type: "date"},
                        CHEQUE_CLEARANCE_DATE: { type: "date" },
                        AMOUNT :{type:"number"},
                    }
                },
                group: {
                    field: "MITI", dir: "desc", aggregates: [
                        { field: "AMOUNT", aggregate: "sum" },
                    ]},
                sort: [{ field: "ENTITY_NAME", dir: "asc" }],
                pageSize: reportConfig.defaultPageSize,
            },
            toolbar: kendo.template($("#toolbar-template").html()),
            excel: {
                fileName: "Collection Report",
                allPages: true,
            },
            pdf: {
                fileName: "Received Schedule",
                allPages: true,
                avoidLinks: true,
                pageSize: "auto",
                margin: {
                    top: "2m",
                    right: "1m",
                    left: "1m",
                    buttom: "1m",
                },
                landscape: true,
                repeatHeaders: true,
                scale: 0.8,
            },
            height: window.innerHeight - 50,
            sortable: true,
            // reorderable: true,
            detailTemplate: kendo.template($("#template").html()),
            detailInit: detailInit,
            pageable: true,
            groupable: true,
            resizable: true,
            filterable: {
                extra: false,
                operators: {
                    number: {
                        eq: "Is equal to",
                        neq: "Is not equal to",
                        gte: "is greater than or equal to	",
                        gt: "is greater than",
                        lte: "is less than or equal",
                        lt: "is less than",
                    },
                    string: {

                        eq: "Is equal to",
                        neq: "Is not equal to",
                        startswith: "Starts with	",
                        contains: "Contains",
                        doesnotcontain: "Does not contain",
                        endswith: "Ends with",
                    },
                    date: {
                        eq: "Is equal to",
                        neq: "Is not equal to",
                        gte: "Is after or equal to",
                        gt: "Is after",
                        lte: "Is before or equal to",
                        lt: "Is before",
                    }
                }
            },
            columnMenu: true,
            columnMenuInit: function (e) {
                wordwrapmenu(e);
                checkboxItem = $(e.container).find('input[type="checkbox"]');
            },
            columnShow: function (e) {
                if ($(".k-widget.k-reset.k-header.k-menu.k-menu-vertical").is(":visible") && checkboxItem != "")
                    SaveReportSetting('CollectionsReport', 'grid');
            },
            columnHide: function (e) {
                if ($(".k-widget.k-reset.k-header.k-menu.k-menu-vertical").is(":visible") && checkboxItem != "")
                    SaveReportSetting('CollectionsReport', 'grid');
            },
            pageable: {
                refresh: true,
                pageSizes: reportConfig.itemPerPage,
                buttonCount: 5
            },
            autoBind: true,
            dataBound: function (o) {
                var grid = o.sender;
                if (grid.dataSource.total() == 0) {
                    var colCount = grid.columns.length;
                    $(o.sender.wrapper)
                        .find('tbody')
                        .append('<tr class="kendo-data-row" style="font-size:12px;"><td colspan="' + colCount + '" class="alert alert-danger">Sorry, no data :(</td></tr>');
                    displayPopupNotification("No Data Found Given Date Filter.", "info");
                }
                else {
                    var g = $("#grid").data("kendoGrid");
                    for (var i = 0; i < g.columns.length; i++) {
                        g.showColumn(i);
                    }
                    $("div.k-group-indicator").each(function (i, v) {
                        g.hideColumn($(v).data("field"));
                    });
                }

                UpdateReportUsingSetting("CollectionsReport", "grid");
                $('div').removeClass('.k-header k-grid-toolbar');
            },
            columns: [
                {
                    field: "ENTITY_NAME",
                    title: "Party Name",
                    width: "170px",
                    groupFooterTemplate:"Total"
                },
                {
                    field: "BILL_NO",
                    title: "MR No", 
                    width: "90px"
                },
                {
                    field: "PAYMENT_MODE",
                    title: "Mode",
                    width: "90px"
                },
                {
                    title: "Chq No.",
                    field: "CHEQUE_NO",
                    width: "50px"
                },
                {
                    field: "AMOUNT",
                    title: "Amount",
                    attributes:
                    {
                        style: "text-align:right;"
                    },
                    aggregates: ["sum"],
                    groupFooterTemplate: "#= kendo.toString(sum, 'n')#",
                    format: "{0:n2}",
                    width: "100px",
                },
                {
                    field: "CHEQUE_DEPOSIT_BANK",
                    title: "Bank",
                    width: "80px"
                },
                {
                    field: "SALESPERSON_NAME",
                    title: "Sales Person",
                    // template: "#if(Visit_ROUTE_CODE !== null && Visit_ROUTE_CODE != '-') {##:Visited_ROUTE## #( #:Visit_ROUTE_CODE#)#} else{#-#}#",
                    // template: "#= Visited_ROUTE # (#= Visit_ROUTE_CODE #) ",
                    width: "125px"
                },
                {
                    field: "REMARKS",
                    title: "Remarks",
                    width: "140px"
                },
                {
                    field: "CREATED_DATE",
                    title: "Collection Date",
                    template: "#= getDateFormat(CREATED_DATE) #",
                    groupHeaderTemplate: "#= getDateFormat(value) #",
                    //template: "#= difference(CUST_LAT,CUST_LONG,VISIT_LAT,VISIT_LONG,'K',Visit_Type) #",
                    width: "80px",
                    hidden: true,
                },
                {
                    field: "MITI",
                    title: "Miti",
                    width: "80px",
                    groupHeaderTemplate:"#=value#"
                },
                {
                    field: "CHEQUE_CLEARANCE_DATE",
                    title: "Chq Clr. Date",
                    template: "#= getDateFormat(CHEQUE_CLEARANCE_DATE)#",
                    groupHeaderTemplate: "#= getDateFormat(value) #",
                    width: "80px"
                },
                {
                    field: "SIGNATURE_IMAGE",
                    title: "Signature",
                    width: "85px",
                    template: function (data) {
                        var img = '';
                        if (_.isEmpty(data.SIGNATURE_IMAGE))
                            data.SIGNATURE_IMAGE = "nophoto";
                        _.each(data.SIGNATURE_IMAGE.split(','), function (x, i) {
                            var imgUrl = window.location.protocol + "//" + window.location.host + '/Areas/NeoErp.Distribution/Images/EntityImages/' + x;
                            if (x == "nophoto")
                                imgUrl = window.location.protocol + "//" + window.location.host + "/images/nophoto.png"; //"http://placehold.it/600x900";
                            if (i == 0)
                                img += '<a class="fancybox" href="' + imgUrl + '" data-fancybox="group_' + data.CREATED_DATE + data.SP_CODE + '" data-caption="' + data.SIGNATURE_IMAGE + '"> ' +
                                    '<img src="' + imgUrl + '" class="img-responsive img-thumbnail" style="width:63px;height:35px;margin: 0 auto;" /> ' +
                                    '</a >';
                            else
                                img += '<a class="fancybox" href="' + imgUrl + '" data-fancybox="group_' + data.CREATED_DATE + data.SP_CODE + '" class="sr-only" data-caption="' + data.SIGNATURE_IMAGE + '"> ' +
                                    '<img src="' + imgUrl + '" class="img-responsive img-thumbnail" style="width:63px;height:35px;margin: 0 auto;" /> ' +
                                    '</a >';
                        });
                        return img;
                    },
                },
                {
                    field: "CHEQUE_IMAGE",
                    title: "Cheque",
                    width: "85px",
                    template: function (data) {
                        var img = '';
                        if (_.isEmpty(data.CHEQUE_IMAGE))
                            data.CHEQUE_IMAGE = "nophoto";
                        _.each(data.CHEQUE_IMAGE.split(','), function (x, i) {
                            var imgUrl = window.location.protocol + "//" + window.location.host + '/Areas/NeoErp.Distribution/Images/EntityImages/' + x;
                            if (x == "nophoto")
                                imgUrl = window.location.protocol + "//" + window.location.host + "/images/nophoto.png"; //"http://placehold.it/600x900";
                            if (i == 0)
                                img += '<a class="fancybox" href="' + imgUrl + '" data-fancybox="group_' + data.CREATED_DATE + data.SP_CODE + '" data-caption="' + data.CHEQUE_IMAGE + '"> ' +
                                    '<img src="' + imgUrl + '" class="img-responsive img-thumbnail" style="width:63px;height:35px;margin: 0 auto;" /> ' +
                                    '</a >';
                            else
                                img += '<a class="fancybox" href="' + imgUrl + '" data-fancybox="group_' + data.CREATED_DATE + data.SP_CODE + '" class="sr-only" data-caption="' + data.CHEQUE_IMAGE + '"> ' +
                                    '<img src="' + imgUrl + '" class="img-responsive img-thumbnail" style="width:63px;height:35px;margin: 0 auto;" /> ' +
                                    '</a >';
                        });
                        return img;
                    },
                },
            ]
        })
    };

    function detailInit(e) {
        
        var detailRow = e.detailRow;
        detailRow.find(".collectionDetail").kendoGrid({
            dataSource: {
                type: "json",
                transport: {
                    read: {
                        url: window.location.protocol + "//" + window.location.host + "/api/distributor/GetCollectionDetail?billNo=" + e.data.BILL_NO,
                        dataType: "json", // <-- The default was "jsonp".
                        type: "GET",
                    },
                },
                aggregate: [
                    { field: "AMOUNT", aggregate: "sum" },
                ],
                pageSize: 100,
                pageable: true,
            },
            sortable: true,
            pageable: false,
            sortable: true,
            reorderable: true,
            resizable: true,
            scrollable: {
                virtual: true
            },
            columns: [
                {
                    field: "DIVISION_EDESC",
                    title: "Division",
                    width: "50%",
                    footerTemplate: '<span style="float:right">Total</span>',
                }, {
                    field: "AMOUNT",
                    title: "Amount",
                    width: "25%",
                    attributes: {
                        style: "text-align: right;"
                    },
                    format: "{0:n}",
                    footerTemplate: '<span style="float:right">#= kendo.toString(sum, "n")#</span>',
                },
            ]
        });
    }

    DateFilter.init(function () {
        consolidate.init(function () {
            bindGrid();
        });
        $("#DistEmployeeMultiSelect").data("kendoMultiSelect").options.maxSelectedItems = 100;
    });

    $(".applydp").on("click", function (evt) {
        evt.preventDefault();
        KendoGridRefresh()
    });

    $("#RunQuery").click(function (evt) {
        evt.preventDefault();
        KendoGridRefresh();
    });

    $("#loadAdvancedFilters").on('click',function (evt) {
        
        evt.preventDefault();
        KendoGridRefresh();
    });

    function KendoGridRefresh() {
        $('#grid').data().kendoGrid.destroy();
        $('#grid').empty();
        bindGrid();
    }

});