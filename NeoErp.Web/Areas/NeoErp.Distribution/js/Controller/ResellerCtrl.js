
distributionModule.controller('ResellerCtrl', function ($scope, $http, ResellerService, $routeParams) {
    console.log("Query string is " + JSON.stringify($routeParams));
    $scope.isViewLoading = false;
    $scope.Source = "B";
    $scope.Contacts = [];
    $scope.Preferences = {};
    $scope.$on('$routeChangeStart', function () {
        $scope.isViewLoading = true;
    });
    $scope.$on('$routeChangeSuccess', function () {
        $scope.isViewLoading = false;
        $scope.fetchSuboutlets();
        ResellerService.GetPreference().then(function (res) {
            $scope.Preferences = res.data;
        }, function (ex) {
            $scope.Preferences.SET_RES_MAP_WHOLESALER = "N";
        });
        //$scope.AddContact();
    });
    $scope.$on('$routeChangeError', function () {
        $scope.isViewLoading = false;
    });


    $scope.param = $routeParams.param;
    $scope.pageName = "Add Reseller";
    $scope.saveAction = "Save";
    $scope.createPanel = false;

    $scope.areaSelectOptions = {
        dataTextField: "AREA_NAME",
        dataValueField: "AREA_CODE",
        height: 600,
        valuePrimitive: true,
        maxSelectedItems: 1,
        headerTemplate: '<div class="col-md-offset-3"><strong>Area...</strong></div>',
        placeholder: "Select Area...",
        autoClose: false,
        dataBound: function (e) {
            var current = this.value();
            this._savedOld = current.slice(0);
            $("#" + e.sender.element[0].id + "_listbox").slimScroll({ 'height': '179px', 'scroll': 'scroll' });
        },
        dataSource: {
            transport: {
                read: {
                    url: window.location.protocol + "//" + window.location.host + "/api/Distribution/GetDistArea",
                    dataType: "json"
                }
            }
        },
        change: function (e) {
            GetIndividualReport(e);
            var multiSelect = $("#distCustomerMultiSelect").data("kendoMultiSelect");
            var ResellermultiSelect = $("#distWholesellerMultiSelect").data("kendoMultiSelect");
            if (e.sender.value().length > 0) {
                //dist select
                var selectedDistributor = _.pluck(_.filter(multiSelect.dataSource.data(), function (x) {
                    return x.AREA_CODE == e.sender.value()[0];
                }), "DISTRIBUTOR_CODE");
                multiSelect.value(selectedDistributor);

                //reseller select
                var selectedReseller = _.pluck(_.filter(ResellermultiSelect.dataSource.data(), function (x) {
                    return x.AREA_CODE == e.sender.value()[0];
                }), "CUSTOMER_CODE");
                ResellermultiSelect.value(selectedReseller);
            } else {
                multiSelect.value([]);
                ResellermultiSelect.value([]);
            }
        }
    };
    function GetIndividualReport(evt) {
        $http({
            method: 'GET',
            url: window.location.protocol + "//" + window.location.host + "/api/Distribution/GetIndividualGroup?SingleAreaCode=" + evt.sender.value()[0],
        }).then(function successCallback(response) {
            response.data[0] ? $("#distGroupSelect").data("kendoMultiSelect").value(response.data[0].GROUPID) : $("#distGroupSelect").data("kendoMultiSelect").value("");
        }, function errorCallback(response) {
        });
    }
    $scope.distCustomerSelectOptions = {
        dataTextField: "CUSTOMER_EDESC",
        dataValueField: "DISTRIBUTOR_CODE",
        height: 600,
        valuePrimitive: true,
        change: myFunction,
        //maxSelectedItems: 1,
        headerTemplate: '<div class="col-md-offset-3"><strong>Distributors...</strong></div>',
        placeholder: "Select Distributors...",
        autoClose: false,
        dataBound: function (e) {
            var current = this.value();
            this._savedOld = current.slice(0);
            $("#" + e.sender.element[0].id + "_listbox").slimScroll({ 'height': '179px', 'scroll': 'scroll' });
        },
        dataSource: {
            transport: {
                read: {
                    url: window.location.protocol + "//" + window.location.host + "/api/Distribution/GetIndividualCustomer",
                    dataType: "json"
                }
            }
        }
    };

    $scope.distWholesellersSelectOptions = {
        dataTextField: "CUSTOMER_EDESC",
        dataValueField: "CUSTOMER_CODE",
        height: 600,
        valuePrimitive: true,
        //maxSelectedItems: 1,
        headerTemplate: '<div class="col-md-offset-3"><strong>Wholesellers...</strong></div>',
        placeholder: "Select Wholesellers...",
        autoClose: false,
        dataBound: function (e) {
            var current = this.value();
            this._savedOld = current.slice(0);
            $("#" + e.sender.element[0].id + "_listbox").slimScroll({ 'height': '179px', 'scroll': 'scroll' });
        },
        dataSource: {
            transport: {
                read: {
                    url: window.location.protocol + "//" + window.location.host + "/api/Distribution/GetWholeSellers",
                    dataType: "json"
                }
            }
        }
    };

    function myFunction(evt) {
        if (this.dataItem() == undefined) {
            $("#areaMultiSelect").data("kendoMultiSelect").value([]);
            $scope.ResellerCreateForm.areaMultiSelect.$error.required = true;
            $scope.ResellerCreateForm.areaMultiSelect.$invalid = true;
        }
        else {
            var area_code = this.dataItem().AREA_CODE;

            $("#areaMultiSelect").data("kendoMultiSelect").value([area_code]);
            $scope.selectedArea = area_code;
            $scope.ResellerCreateForm.areaMultiSelect.$error.required = false;
            $scope.ResellerCreateForm.areaMultiSelect.$invalid = false;
        }



    }
    $scope.distGroupSelectOptions = {

        close: function () {
            var selected = $("#distGroupSelect").data("kendoMultiSelect").dataItem();
            $scope.selectedGroup = typeof (selected) == 'undefined' ? [] : [String(selected.GROUPID)];
            //$scope.$apply();
        },
        dataTextField: "GROUP_EDESC",
        dataValueField: "GROUPID",
        height: 600,
        valuePrimitive: true,
        maxSelectedItems: 1,
        headerTemplate: '<div class="col-md-offset-3"><strong>Group...</strong></div>',
        placeholder: "Select Group...",
        autoClose: false,
        dataBound: function (e) {
            var current = this.value();
            this._savedOld = current.slice(0);
            $("#" + e.sender.element[0].id + "_listbox").slimScroll({ 'height': '179px', 'scroll': 'scroll' });
        },
        dataSource: {
            transport: {
                read: {
                    url: window.location.protocol + "//" + window.location.host + "/api/Setup/getAllResellerGroups",
                    dataType: "json"
                }
            }
        }
    };

    $scope.distOutletSelectOptions = {
        close: function (e) {
            //clear sub outlet
            $scope.selectedSubOutlet = '';
            $("#distSubOutletSelect").data("kendoMultiSelect").value([]);

            var selected = $("#distOutletSelect").data("kendoMultiSelect").dataItem();
            $scope.selectedOutlet = typeof (selected) == 'undefined' ? [] : [String(selected.TYPE_ID)];
            if (typeof (selected) != 'undefined')
                $scope.fetchSuboutlets(selected.TYPE_ID);
            //$scope.$apply();
        },
        dataTextField: "TYPE_EDESC",
        dataValueField: "TYPE_ID",
        height: 600,
        valuePrimitive: true,
        maxSelectedItems: 1,
        headerTemplate: '<div class="col-md-offset-3"><strong>Outlet...</strong></div>',
        placeholder: "Select Outlet...",
        autoClose: false,
        dataBound: function (e) {
            var current = this.value();
            this._savedOld = current.slice(0);
            $("#" + e.sender.element[0].id + "_listbox").slimScroll({ 'height': '179px', 'scroll': 'scroll' });
        },
        dataSource: {
            transport: {
                read: {
                    url: window.location.protocol + "//" + window.location.host + "/api/Setup/getAllOutletList",
                    dataType: "json"
                }
            }
        }
    };

    $scope.fetchSuboutlets = function (typeId) {
        var url = '';
        if (typeof (typeId) == 'undefined')
            url = window.location.protocol + "//" + window.location.host + "/api/Setup/getAllSubOutletList";
        else
            url = window.location.protocol + "//" + window.location.host + "/api/Setup/getAllSubOutletList?TYPE_ID=" + typeId
        var dataSource = $("#distSubOutletSelect").data("kendoMultiSelect");
        if (typeof (dataSource) != 'undefined' && dataSource != null) {
            $("#distSubOutletSelect").data("kendoMultiSelect").dataSource.options.transport.read.url = url;
            $("#distSubOutletSelect").data("kendoMultiSelect").dataSource.read();
            return;
        }
        $scope.distSubOutletSelectOptions = {
            dataTextField: "SUBTYPE_EDESC",
            dataValueField: "SUBTYPE_ID",
            height: 600,
            valuePrimitive: true,
            maxSelectedItems: 1,
            headerTemplate: '<div class="col-md-offset-3"><strong>Outlet Category...</strong></div>',
            placeholder: "Select Outlet Category...",
            autoClose: false,
            dataBound: function (e) {
                var current = this.value();
                this._savedOld = current.slice(0);
                $("#" + e.sender.element[0].id + "_listbox").slimScroll({ 'height': '179px', 'scroll': 'scroll' });
            },
            dataSource: {
                transport: {
                    read: {
                        url: url,
                        dataType: "json"
                    }
                }
            }
        };
        //$("#distSubOutletSelect").data("kendoMultiSelect").dataSource.refresh();
    }
    var bsFromDate = $('#FromDateVoucher').val(); // Get value from FromDateVoucher
    var bsToDate = $('#ToDateVoucher').val();

    var fromDate = moment(bsFromDate, 'YYYY-MMM-DD').format('DD-MMM-YYYY');
    var toDate = moment(bsToDate, 'YYYY-MMM-DD').format('DD-MMM-YYYY');

    //grid
    var reportConfig = GetReportSetting("ResellerSetup");
    var statusQ = $("#outLetStatus").val();
    $scope.grid = {
        dataSource: {
            type: "json",
            transport: {
                read: {
                    url: window.location.protocol + "//" + window.location.host + "/api/Setup/GetResellerList?Source=" + $scope.Source + "&status=" + $routeParams.status,
                    dataType: "json",
                    type: "POST",
                    contentType: "application/json; charset=utf-8"
                },
                parameterMap: function (options, type) {
                    var paramMap = JSON.stringify($.extend(options, ReportFilter.filterAdditionalData()));
                    //var paramMap = JSON.stringify($.extend(options, ReportFilter.filterAdditionalData(), {
                    //    formDate: fromDate,
                    //    ToDate: toDate
                    //}));
                    delete paramMap.$inlinecount; // <-- remove inlinecount parameter.
                    delete paramMap.$format; // <-- remove format parameter.
                    return paramMap;
                }
            },
            schema: {
                //data: "data",
                parse: function (data) {
                    for (let i = 0; i < data.data.length; i++) {
                        data.data[i].Location = (data.data[i].LATITUDE && data.data[i].LONGITUDE) ? (data.data[i].LATITUDE + "," + data.data[i].LONGITUDE) : "Missing"
                    }
                    if (data.inActive > 0) {
                        Metronic.alert({
                            container: $('.content').val(), // alerts parent container
                            place: 'append', // append or prepent in container
                            type: 'success', // alert's type
                            message: data.inActive+ " new outlets are waiting for activation.", // alert's message
                            close: true, // make alert closable
                            reset: true, // close all previouse alerts first
                            focus: true, // auto scroll to the alert after shown
                            closeInSeconds: 10, // auto close after defined seconds
                            icon: 'fa fa-user' // put icon class before the message
                        });
                    }
                    return data.data;
                },
                model: {
                    fields: {
                        // ASSIGN_DATE: { type: "date" },
                    }
                },
            },
            error: function (e) {
                displayPopupNotification("Sorry error occured while processing data", "error");
            },
            pageSize: reportConfig.defaultPageSize,
        },
        toolbar: kendo.template($("#toolbar-template").html()),
        excel: {
            fileName: "Reseller Setup",
            allPages: true,
        },
        excelExport: function (e) {
            ExportToExcel(e);
            e.preventDefault();
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
        reorderable: true,
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
                SaveReportSetting('ResellerSetup', 'grid');
        },
        columnHide: function (e) {
            if ($(".k-widget.k-reset.k-header.k-menu.k-menu-vertical").is(":visible") && checkboxItem != "")
                SaveReportSetting('ResellerSetup', 'grid');
        },
        pageable: {
            refresh: true,
            pageSizes: reportConfig.itemPerPage,
            buttonCount: 5
        },
        //scrollable: {
        //    virtual: true
        //},
        pageable: true,
        dataBound: function (o) {
            GetSetupSetting("ResellerSetup");
            var grid = o.sender;
            if (grid.dataSource.data().length == 0) {
                var colCount = grid.columns.length;
                $(o.sender.wrapper)
                    .find('tbody')
                    .append('<tr class="kendo-data-row" style="font-size:12px;"><td colspan="' + colCount + '" class="alert alert-danger">Sorry, no data :(</td></tr>');
                displayPopupNotification("No Data Found.", "info");
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

            UpdateReportUsingSetting("ResellerSetup", "grid");
            $('div').removeClass('.k-header k-grid-toolbar');
        },
        columns: [
            {
                field: "Reseller_CODE",
                title: "Code",
                width: "10%"
            },
            {
                field: "Reseller_NAME",
                title: "Name",
            },
            {
                field: "AREA_Name",
                title: "Area",
            },
            //{
            //    field: "Distributor_Name",
            //    title: "Distributor Name",
            //},
            {
                field: "WholeSeller",
                title: "WholeSeller",
                template: "#= WholeSeller == 'Y' ? 'Yes' : 'No' # ",

            },

            {
                field: "Primary_Contact",
                title: "Contact",
            },
            {
                field: "Active",
                title: "Status",
                width: "9%",
            },
            {
                field: "IsClosed",
                title: "Closed/Open",
                width: "9%",
            },
            {
                field: "Location",
                title: "Location",
                template: "#= (LATITUDE && LONGITUDE) ? Location : '<span style=\"color:red\">'+Location+'</span>' # ",
                width: "9%",
            },
            {
                field: "Created_by_name",
                title: "Created By",
                width: "9%",
            },
            {
                field: "created_date",
                title: "Created Date",
                width: "9%",
                template: function (dataItem) {
                    return kendo.toString(new Date(dataItem.created_date), "dd-MMM-yyyy");
                }
            },
            {
                title: "Action",
                template: " <a class='fa fa-edit editAction' ng-click='UpdateReseller($event)' title='Edit'></a>&nbsp &nbsp<a class='fa fa-trash-o deleteAction' ng-click='deleteReseller($event)' title='delete'></a> ",
                width: "8%"
            }
        ]
    };

    $scope.detailDistributorOptions = function (dataItem) {
        var id = dataItem.Reseller_CODE;
        return {
            dataSource: {
                type: "json",
                transport: {
                    read: {
                        url: window.location.protocol + "//" + window.location.host + "/api/Setup/GetReseller?ResellerId=" + id,
                        dataType: "json",
                        type: "GET",
                        contentType: "application/json; charset=utf-8"
                    },
                },
                schema: {
                    parse: function (data) {
                        return data.DATA.Distributors;
                    }
                },
                error: function (e) {
                    displayPopupNotification("Sorry error occured while processing data", "error");
                },
            },
            scrollable: false,
            sortable: false,
            pageable: false,
            columns: [
                { field: "Name", title: "Distributor Name", width: "100%" },
            ]
        };
    };
    $scope.detailWholesellerOptions = function (dataItem) {
        var id = dataItem.Reseller_CODE;
        return {
            dataSource: {
                type: "json",
                transport: {
                    read: {
                        url: window.location.protocol + "//" + window.location.host + "/api/Setup/GetReseller?ResellerId=" + id,
                        dataType: "json",
                        type: "GET",
                        contentType: "application/json; charset=utf-8"
                    },
                },
                schema: {
                    parse: function (data) {
                        return data.DATA.Wholesellers;
                    }
                },
                error: function (e) {
                    displayPopupNotification("Sorry error occured while processing data", "error");
                },
            },
            scrollable: false,
            sortable: false,
            pageable: false,
            columns: [
                { field: "Name", title: "Wholeseller Name", width: "100%" },
            ]
        };
    };
    $scope.detailContactOptions = function (dataItem) {
        var id = dataItem.Reseller_CODE;
        return {
            dataSource: {
                type: "json",
                transport: {
                    read: {
                        url: window.location.protocol + "//" + window.location.host + "/api/Setup/GetReseller?ResellerId=" + id,
                        dataType: "json",
                        type: "GET",
                        contentType: "application/json; charset=utf-8"
                    },
                },
                schema: {
                    parse: function (data) {
                        return data.DATA.Contacts;
                    }
                },
                error: function (e) {
                    displayPopupNotification("Sorry error occured while processing data", "error");
                },
            },
            scrollable: false,
            sortable: false,
            pageable: false,
            columns: [
                { field: "ContactSuffix", title: "Contact Suffix", width: "10%" },
                { field: "Name", title: "Contact Name", width: "50%" },
                { field: "Number", title: "Number", width: "25%" },
                { field: "Designation", title: "Designation", width: "15%" },
            ]
        };
    };
    $scope.detailImageOptions = function (dataItem) {
        var id = dataItem.Reseller_CODE;
        return {
            dataSource: {
                type: "json",
                transport: {
                    read: {
                        url: window.location.protocol + "//" + window.location.host + "/api/Setup/GetReseller?ResellerId=" + id,
                        dataType: "json",
                        type: "GET",
                        contentType: "application/json; charset=utf-8"
                    },
                },
                schema: {
                    parse: function (data) {
                        return [data.DATA];
                    }
                },
                error: function (e) {
                    displayPopupNotification("Sorry error occured while processing data", "error");
                },
            },
            scrollable: false,
            sortable: false,
            pageable: false,
            columns: [
                {
                    //field: "FILENAME",
                    title: "Store Images",
                    width: "50%",
                    template: function (data) {
                        var img = '';
                        if (_.isEmpty(data.StorePhotos))
                            data.StorePhotos = "nophoto";
                        _.each(data.StorePhotos, function (x, i) {
                            var imgUrl = window.location.protocol + "//" + window.location.host + '/Areas/NeoErp.Distribution/Images/ResellerImages/' + x;
                            if (x == "nophoto")
                                imgUrl = window.location.protocol + "//" + window.location.host + "/images/nophoto.png"; //"http://placehold.it/600x900";
                            if (i == 0)
                                img += '<a class="fancybox" href="' + imgUrl + '" data-fancybox="group_store_' + data.Reseller_CODE + data.Reseller_NAME + '" data-caption="' + data.Reseller_NAME + '"> ' +
                                     '<img src="' + imgUrl + '" class="img-responsive img-thumbnail" style="width:63px;height:35px;margin: 0 auto;" /> ' +
                                     '</a>';
                            else
                                img += '<a class="fancybox" href="' + imgUrl + '" data-fancybox="group_store_' + data.Reseller_CODE + data.Reseller_NAME + '" class="sr-only" data-caption="' + data.Reseller_NAME + '"> ' +
                                     '<img src="' + imgUrl + '" class="img-responsive img-thumbnail" style="width:63px;height:35px;margin: 0 auto;" /> ' +
                                     '</a>';
                        });
                        return img;
                    },
                },
                {
                    //field: "FILENAME",
                    title: "Primary Contact Images",
                    width: "50%",
                    template: function (data) {
                        var img = '';
                        if (_.isEmpty(data.PContactPhotos))
                            data.PContactPhotos = "nophoto";
                        _.each(data.PContactPhotos, function (x, i) {
                            var imgUrl = window.location.protocol + "//" + window.location.host + '/Areas/NeoErp.Distribution/Images/ResellerImages/' + x;
                            if (x == "nophoto")
                                imgUrl = window.location.protocol + "//" + window.location.host + "/images/nophoto.png"; //"http://placehold.it/600x900";
                            if (i == 0)
                                img += '<a class="fancybox" href="' + imgUrl + '" data-fancybox="group_pContact_' + data.Reseller_CODE + data.Reseller_NAME + '" data-caption="' + data.Reseller_NAME + '"> ' +
                                     '<img src="' + imgUrl + '" class="img-responsive img-thumbnail" style="width:63px;height:35px;margin: 0 auto;" /> ' +
                                     '</a >';
                            else
                                img += '<a class="fancybox" href="' + imgUrl + '" data-fancybox="group_pContact_' + data.Reseller_CODE + data.Reseller_NAME + '" class="sr-only" data-caption="' + data.Reseller_NAME + '"> ' +
                                     '<img src="' + imgUrl + '" class="img-responsive img-thumbnail" style="width:63px;height:35px;margin: 0 auto;" /> ' +
                                     '</a >';
                        });
                        return img;
                    },
                },
            ]
        };
    };

    ////create empty temp array
    var markersArray = [];
    var map;
    var marker;
    $scope.initialize = function () {
        var myLatlng = new google.maps.LatLng(27.700769, 85.300140);

        var mapOptions = {
            zoom: 10,
            center: myLatlng,
            mapTypeId: google.maps.MapTypeId.ROADMAP
        };

        map = new google.maps.Map(document.getElementById('map-canvas'), mapOptions);

        marker = new google.maps.Marker({
            map: map,
            position: myLatlng,
            title: 'Center',
            draggable: true
        });



        /***************** search box function *****************/
        // Create the search box and link it to the UI element.


        if (document.getElementById('mapSearchBox') == null)
            $("#Reseller-map-panel").prepend('<div id="Reseller-map-fullscreen"></div><input id="mapSearchBox" class="controls" type="text" placeholder="Search Box">');
        var input = document.getElementById('mapSearchBox');
        var searchBox = new google.maps.places.SearchBox(input);
        map.controls[google.maps.ControlPosition.TOP_LEFT].push(input);
        // Bias the SearchBox results towards current map's viewport.
        map.addListener('bounds_changed', function () {
            searchBox.setBounds(map.getBounds());
        });

        // Listen for the event fired when the user selects a prediction and retrieve
        // more details for that place.
        searchBox.addListener('places_changed', function () {
            var places = searchBox.getPlaces();
            // console.log(places);

            if (places.length == 0) {
                return;
            }

            if (!places[0].geometry) {
                console.log("Returned place contains no geometry");
                return;
            }

            var latlng = new google.maps.LatLng(places[0].geometry.location.lat(), places[0].geometry.location.lng());
            marker.setPosition(latlng);

            var bounds = new google.maps.LatLngBounds();

            if (places[0].geometry.viewport) {
                bounds.union(places[0].geometry.viewport);
            } else {
                bounds.extend(places[0].geometry.location);
            }
            map.fitBounds(bounds);
            $('#maplat').html(places[0].geometry.location.lat());
            $('#maplng').html(places[0].geometry.location.lng());
        });


        /***************** search box function *****************/


        google.maps.event.addListener(map, 'click', function (e) {
            marker.setPosition(e.latLng);
            writeLabels(e.latLng);
            $('#maplat').html(e.latLng.lat());
            $('#maplng').html(e.latLng.lng());
        });

        google.maps.event.addListener(marker, 'drag', function (e) {
            $('#maplat').html(e.latLng.lat());
            $('#maplng').html(e.latLng.lng());
        });


        /***************** map full screen button function *****************/
        map.controls[google.maps.ControlPosition.TOP_LEFT].push(document.getElementById('Reseller-map-fullscreen'));
        document.getElementById('Reseller-map-fullscreen').style.display = 'block';

        $('#Reseller-map-fullscreen').on('click', function () {
            var mapCenter = map.getCenter();
            if (!$(this).is('.map-fullscreen')) {
                $('body').css({
                    overflow: 'hidden'
                });

                $('#Reseller-map-panel').css({
                    position: "fixed",

                    top: 0,
                    left: 0,
                    height: $(window).height(),
                    width: "100%",
                    zIndex: 9999,
                    margin: "0 auto"
                });
                $(this).addClass('map-fullscreen');
            }
            else {

                $('#Reseller-map-panel').css({
                    position: "relative",
                    top: 'auto',
                    left: 'auto',
                    height: "425px",
                    width: "100%",
                    zIndex: 1,
                    margin: "0 auto"
                });
                $('body').css({
                    overflow: 'auto'
                });
                $(this).removeClass('map-fullscreen');
            }
            google.maps.event.trigger(map, "resize");
            var latlng = new google.maps.LatLng(document.getElementById('maplat').value, document.getElementById('maplng').value);
            map.setCenter(latlng);
        });
        /***************** map full screen button function *****************/
    }

    $scope.initialize();

    $scope.mapReset = function () {
        //$("#mapReset span").addClass("fa-spin");
        var latlong = new google.maps.LatLng(27.700769, 85.300140);
        map.setCenter(latlong);
        map.setZoom(10);
        marker.setPosition(latlong);
        $('#maplat').html(27.700769);
        $('#maplng').html(85.300140);
        google.maps.event.trigger(document.getElementById('map-canvas'), 'resize');
        //google.maps.event.addListenerOnce(map, 'idle', function () {
        //   $("#mapReset span").removeClass("fa-spin");
        //});

    }

    $scope.ResellerCreate = function (isValid) {
        
        //$scope.selectedGroup = [$("#distGroupSelect").data("kendoMultiSelect").dataItem().GROUPID];
        if (isValid) {
            for (let i = 0; i < $scope.Contacts.length; i++) {
                if ($scope.Contacts[i].Name == "" && $scope.Contacts[i].Number == "" && $scope.Contacts[i].Designation == "")
                    $scope.RemoveContact(i);
                else if ($scope.Contacts[i].Name == "") {
                    isValid = false;
                    break;
                }
            }
        }
        if (!isValid) {
            displayPopupNotification("Invalid Field", "warning");
            if ($scope.ResellerCreateForm.resellerName.$invalid == true) {
                $('#resellerName').focus();
            }

            else if ($scope.ResellerCreateForm.distCustomerMultiSelect.$invalid == true) {

                $('#distCustomerMultiSelect').focus();
            }
            else if ($scope.ResellerCreateForm.distWholesellerMultiSelect.$invalid == true) {

                $('#distWholesellerMultiSelect').focus();
            }
            else if ($scope.ResellerCreateForm.areaMultiSelect.$invalid == true) {
                $('#areaMultiSelect').focus();
            }
            else if ($scope.ResellerCreateForm.Address.$invalid == true) {
                $('#Address').focus();
            }

            else if ($scope.ResellerCreateForm.ContactName.$invalid == true) {
                $('#ContactName').focus();
            }

            else if ($scope.ResellerCreateForm.ContactNo.$invalid == true) {
                $('#ContactNo').focus();
            }
            return;

        }
        var selectedCustomer = $("#distCustomerMultiSelect").data("kendoMultiSelect").value();
        var selectedWholeseller = [];
        var selectedWholesellerArray = [];
        var selectedCustomerArray = [];
        if ($scope.Preferences.SET_RES_MAP_WHOLESALER == "Y")
            selectedWholeseller = $("#distWholesellerMultiSelect").data("kendoMultiSelect").value();
        for (let i = 0; i < selectedWholeseller.length; i++) {
            selectedWholesellerArray.push({ Code: selectedWholeseller[i] });
        }
        for (let i = 0; i < selectedCustomer.length; i++) {
            selectedCustomerArray.push({ Code: selectedCustomer[i] });
        }
        var isArray = $scope.selectedArea;
        if ($.isArray(isArray) == true) {

            var selectedArea = $scope.selectedArea[0];
        }
        else {
            selectedArea = $scope.selectedArea;
        }
        var lat = $('#maplat').html();
        var long = $('#maplng').html();
        var groupId = $("#distGroupSelect").data("kendoMultiSelect").dataItem();
        if (groupId == undefined) {
            GROUP_ID = null;
        }
        else {
            GROUP_ID = $("#distGroupSelect").data("kendoMultiSelect").dataItem().GROUPID;
        }
        var data = {
            Reseller_CODE: $scope.Reseller_CODE,
            Reseller_NAME: $scope.resellerName,
            Distributors: selectedCustomerArray,
            Wholesellers: selectedWholesellerArray,
            //Distributor_CODE: selectedCustomer,
            AREA_Code: selectedArea,
            ADDRESS: $scope.Address,
            Email: $scope.Email,
            Primary_Contact_Name: $scope.ContactName,
            Primary_Contact_No: $scope.ContactNo,
            LATITUDE: $('#maplat').html(),
            LONGITUDE: $('#maplng').html(),
            Vat_No: $scope.VatNo,
            Pan_No: $scope.PanNo,
            GROUP_ID: GROUP_ID,
            //GROUP_ID: $("#distGroupSelect").data("kendoMultiSelect").dataItem().GROUPID,
            OUTLET_TYPE_ID:
                $scope.selectedOutlet
                    ? $scope.selectedOutlet[0]
                    : '', //$("#distOutletSelect").data("kendoMultiSelect").dataItem().TYPE_ID,
            OUTLET_SUBTYPE_ID:
                $scope.selectedSubOutlet
                    ? $scope.selectedSubOutlet[0]
                    : '', //$("#distSubOutletSelect").data("kendoMultiSelect").dataItem().SUBTYPE_ID,
            WholeSeller: $('input[name=wholeSellerCheckbox]').prop('checked') == true ? 'Y' : 'N',
            Active: $('input[name=ActiveCheckbox]').prop('checked') == true ? 'Y' : 'N',
            RESELLER_CONTACT: $scope.ResellerNumber,
            REMARKS: $scope.REMARKS,
            Contacts: $scope.Contacts,
            IsClosed: $('input[name=ClosedCheckbox]').prop('checked') == true ? 'Y' : 'N',
        };
        if ($scope.saveAction == "Update") //update mode
        {
            ResellerService.UpdateReseller(data).then(function (result) {
                if (result.data.STATUS_CODE === 200) {
                    displayPopupNotification(result.data.MESSAGE, "success");
                    $("#grid").data("kendoGrid").dataSource.read();
                    // $('.createPanel').hide();
                    $scope.createPanel = false;
                }
                else {
                    displayPopupNotification(result.data.MESSAGE, "warning");
                }
            }, function (error) {
                displayPopupNotification("Error", "error");
            });
        }
        else { //add mode
            ResellerService.AddReseller(data).then(function (result) {
                if (result.data.STATUS_CODE === 200) {
                    displayPopupNotification(result.data.MESSAGE, "success");
                    $("#grid").data("kendoGrid").dataSource.read();
                    //$('.createPanel').hide();
                    $scope.createPanel = false;
                }
                else {
                    displayPopupNotification(result.data.MESSAGE, "warning");
                }
            }, function (error) {
                displayPopupNotification("Error", "error");
            });
        }

    }

    $scope.AddButtonClickEvent = function () {
        $scope.clear();
        $scope.createPanel = true;
        //update map   
        var latlong = new google.maps.LatLng(27.700769, 85.300140);
        map.setCenter(latlong);
        marker.setPosition(latlong);
        $('#maplat').html(27.700769);
        $('#maplng').html(85.300140);

        setTimeout(function () {
            google.maps.event.trigger(document.getElementById('map-canvas'), 'resize');
            map.setCenter(latlong);
        });


    }

    $scope.UpdateReseller = function (evt) {
        debugger;
        var grid = $("#grid").data("kendoGrid");
        var row = evt.target.closest("tr");
        var item = grid.dataItem(row);


        ResellerService.GetReseller(item.Reseller_CODE).then(function (result) {
            debugger;
            $scope.Contacts = [];
            item = result.data.DATA;
            //$scope.Contacts = result.data.DATA.Contacts;
            //result.data.DATA.Contacts.forEach(function (value, key) {
            //    $scope.AddContact(value);
            //});

            var distributors = _.pluck(item.Distributors, "Code");
            var wholesellers = _.pluck(item.Wholesellers, "Code");

            $("#distCustomerMultiSelect").data("kendoMultiSelect").value(distributors);

            if ($scope.Preferences.SET_RES_MAP_WHOLESALER == "Y")
                $("#distWholesellerMultiSelect").data("kendoMultiSelect").value(wholesellers);
            $("#areaMultiSelect").data("kendoMultiSelect").value([item.AREA_Code]);
            if (item.WholeSeller == "Y") {
                $('input[name=wholeSellerCheckbox]').prop('checked', true);
            }
            else {
                $('input[name=wholeSellerCheckbox]').prop('checked', false);
            }

            if (item.Active == "Y") {
                $('input[name=ActiveCheckbox]').prop('checked', true);
            }
            else {
                $('input[name=ActiveCheckbox]').prop('checked', false);
            }
            $scope.selectedArea = [item.AREA_Code];
            $scope.selecteddistCustomer = distributors;
            $scope.selecteddistWholeseller = wholesellers;
            $scope.pageName = "Update Reseller";
            $scope.Reseller_CODE = item.Reseller_CODE;
            $scope.resellerName = item.Reseller_NAME;
            $scope.ResellerNumber = item.RESELLER_CONTACT != "" ? parseInt(item.RESELLER_CONTACT) : item.RESELLER_CONTACT;
            $("#ResellerNumber").val(item.RESELLER_CONTACT).trigger('change');//angular model binding was not working
            $scope.Address = item.ADDRESS;
            $scope.Email = item.Email;
            $scope.ContactName = item.Primary_Contact_Name;
            $scope.ContactNo = item.Primary_Contact_No != "" ? parseInt(item.Primary_Contact_No) : item.Primary_Contact_No;
            $("#ContactNo").val(item.Primary_Contact_No).trigger('change');//angular model binding was not working
            $scope.VatNo = item.Vat_No;
            $scope.PanNo = item.Pan_No;
            $scope.WholeSeller = item.WholeSeller == 'Y' ? true : false;
            $scope.saveAction = "Update";
            $scope.selectedGroup = [item.GROUP_ID];
            $scope.selectedOutlet = [item.OUTLET_TYPE_ID];
            $scope.selectedSubOutlet = [item.OUTLET_SUBTYPE_ID];
            $("#distGroupSelect").data("kendoMultiSelect").value([item.GROUP_ID]);
            $("#distOutletSelect").data("kendoMultiSelect").value([item.OUTLET_TYPE_ID]);
            $("#distSubOutletSelect").data("kendoMultiSelect").value([item.OUTLET_SUBTYPE_ID]);
            $scope.Contacts = item.Contacts;
            //update map   
            var latlong = new google.maps.LatLng(item.LATITUDE, item.LONGITUDE);
            map.setCenter(latlong);
            marker.setPosition(latlong);
            $('#maplat').html(item.LATITUDE);
            $('#maplng').html(item.LONGITUDE);
            //$('.createPanel').show();
            $scope.createPanel = true;

        }, function (error) {
            displayPopupNotification("Cannot retrieve contacts", "error");
        });

        if (!_.isEmpty(item.Distributor_CODE)) {
            var disCode = item.Distributor_CODE.replace(/ /g, '').split(',')
            $("#distCustomerMultiSelect").data("kendoMultiSelect").value(disCode);
        }

        $("#areaMultiSelect").data("kendoMultiSelect").value([item.AREA_Code]);
        if (item.WholeSeller == "Y") {
            $('input[name=wholeSellerCheckbox]').prop('checked', true);
        }
        else {
            $('input[name=wholeSellerCheckbox]').prop('checked', false);
        }
        $scope.selectedArea = [item.AREA_Code];
        $scope.selecteddistCustomer = disCode;
        $scope.pageName = "Update Reseller";
        $scope.Reseller_CODE = item.Reseller_CODE;
        $scope.resellerName = item.Reseller_NAME;
        $scope.Address = item.ADDRESS;
        $scope.Email = item.Email;
        $scope.ContactName = item.Primary_Contact_Name;
        $scope.ContactNo = item.Primary_Contact_No != "" ? parseInt(item.Primary_Contact_No) : item.Primary_Contact_No;
        $scope.VatNo = item.Vat_No;
        $scope.PanNo = item.Pan_No;
        $scope.Is_Closed = item.IsClosed == "Y" ? true : false;
        $scope.WholeSeller = item.WholeSeller == 'Y' ? true : false;
        $scope.REMARKS = item.REMARKS;
        $scope.saveAction = "Update";
        //$scope.selectedGroup=[item.GROUP_ID];
        $scope.selectedOutlet = [item.OUTLET_TYPE_ID];
        $scope.selectedSubOutlet = [item.OUTLET_SUBTYPE_ID];
        //$("#distGroupSelect").data("kendoMultiSelect").value([item.GROUP_ID]);
        $("#distOutletSelect").data("kendoMultiSelect").value([item.OUTLET_TYPE_ID]);
        $("#distSubOutletSelect").data("kendoMultiSelect").value([item.OUTLET_SUBTYPE_ID]);
        debugger;
        $scope.ResellerNumber = item.RESELLER_CONTACT != "" ? parseInt(item.RESELLER_CONTACT) : item.RESELLER_CONTACT;
        $scope.Contacts = item.Contacts;
        //update map   
        var latlong = new google.maps.LatLng(item.LATITUDE, item.LONGITUDE);
        map.setCenter(latlong);
        marker.setPosition(latlong);
        $('#maplat').html(item.LATITUDE);
        $('#maplng').html(item.LONGITUDE);



        //$('.createPanel').show();
        $scope.createPanel = true;

    }

    $scope.deleteReseller = function (evt) {

        bootbox.confirm({
            title: "Delete",
            message: "Are you sure?",
            buttons: {
                confirm: {
                    label: 'Yes',
                    className: 'btn-success',
                    label: '<i class="fa fa-check"></i> Yes',
                },
                cancel: {
                    label: 'No',
                    className: 'btn-danger',
                    label: '<i class="fa fa-times"></i> No',
                }
            },
            callback: function (result) {
                if (result == true) {
                    var grid = $("#grid").data("kendoGrid");
                    var row = evt.target.closest("tr");
                    var item = grid.dataItem(row);

                    var Reseller_CODE = item.Reseller_CODE;
                    var Reseller_NAME = item.Reseller_NAME;

                    var data = {
                        Reseller_CODE: Reseller_CODE,
                        Reseller_NAME: Reseller_NAME,
                    }

                    var deleteReseller = ResellerService.deleteReseller(data);
                    deleteReseller.then(function (response) {
                        if (response.data.STATUS_CODE == 400) {
                            displayPopupNotification(response.data.MESSAGE, "Success");
                            $("#grid").data("kendoGrid").dataSource.read();
                            $scope.clear();
                        }
                        else {
                            displayPopupNotification(response.data.MESSAGE, "error");
                        }

                    });
                }

            }
        });

    };

    $scope.Cancel = function () {
        $scope.createPanel = false;
    }

    $scope.clear = function () {
        $scope.pageName = "Add Reseller";
        $('#resellerName').css("border-color", "#e5e5e5")
        $scope.Reseller_CODE = "";
        $scope.resellerName = "";
        $scope.Address = "";
        $scope.Email = "";
        $scope.ContactName = "";
        $scope.ContactNo = "";
        $scope.ResellerNumber = "";
        $scope.VatNo = "";
        $scope.PanNo = "";
        $scope.selectedGroup = '';
        $scope.selectedOutlet = '';
        $scope.selectedSubOutlet = '';
        $("#distGroupSelect").data("kendoMultiSelect").value([]);
        $("#distOutletSelect").data("kendoMultiSelect").value([]);
        $("#distSubOutletSelect").data("kendoMultiSelect").value([]);
        $scope.WholeSeller = true;
        $('input[name=wholeSellerCheckbox]').prop('checked', false);
        $("#distCustomerMultiSelect").data("kendoMultiSelect").value([]);
        if ($scope.Preferences.SET_RES_MAP_WHOLESALER == "Y")
            $("#distWholesellerMultiSelect").data("kendoMultiSelect").value([]);
        $("#areaMultiSelect").data("kendoMultiSelect").value([]);
        $scope.selecteddistCustomer = null;
        $scope.selecteddistWholeseller = null;
        $scope.selectedArea = null;
        $scope.saveAction = "Save";
        $scope.Contacts = [];
    }

    //Contacts operations
    $scope.SuffixSource = {
        dataTextField: "Text",
        dataValueField: "Value",
        dataSource: [
            { Text: "Mr.", Value: "Mr." },
            { Text: "Mrs.", Value: "Mrs." },
        ],
    };

    $scope.AddContact = function (item) {
        var i = $scope.Contacts.length;
        var available = $scope.Contacts;
        $scope.Contacts = [];
        if (typeof (item) == "undefined" || item == null)
            $scope.Contacts.push({
                Sn: i + 1,
                ContactSuffix: "Mr.",
                Name: '',
                Number: '',
                Designation: '',
                Primary: 'N',
            });
        else
            $scope.Contacts.push({
                Sn: i + 1,
                ContactSuffix: item.ContactSuffix,
                Name: item.Name,
                Number: item.Number,
                Designation: item.Designation,
                Primary: 'N',
            });
        for (var i = 0; i < available.length; i++) {
            var item = available[i];
            $scope.Contacts.push(item);
        }
    }

    $scope.RemoveContact = function (index) {
        $scope.Contacts.splice(index, 1);
        for (var i = 0; i < $scope.Contacts.length; i++) {
            $scope.Contacts[i].Sn = $scope.Contacts.length - i;
        }
    };

    function ExportToExcel(e) {

        //Report header details
        var companyName = ('@workingContent.CurrentUserinformation.company_name');
        var fromADdate = $("#FromDateVoucher").val()
        var toADdate = $("#ToDateVoucher").val();
        var fromBSdate = $("#fromInputDateVoucher").val();
        var toBSdate = $("#toInputDateVoucher").val();
        var branches = ReportFilter.getBranchs();

        //create a footer row to display totals
        //var footer = [];
        //footer.push({ value: "Total", colSpan: 4, textAlign: "center", bold: true, })
        //var footerTemp = e.workbook.sheets[0].rows[e.workbook.sheets[0].rows.length - 1].cells;
        var SheetRow = [];//array to store the rows
        //push the header to row array
        SheetRow.push({
            cells: [
                { value: "Code", background: "#A9A7A6" },
                { value: "Name", background: "#A9A7A6", },
                { value: "Area", background: "#A9A7A6", },
                { value: "Wholeseller", background: "#A9A7A6", },
                { value: "Contact", background: "#A9A7A6", },
                { value: "Email", background: "#A9A7A6", },
                { value: "Status", background: "#A9A7A6", },
                { value: "Closed/Open", background: "#A9A7A6", },
                { value: "Address", background: "#A9A7A6", },
                { value: "Location", background: "#A9A7A6", },
                { value: "Group", background: "#A9A7A6", },
                { value: "Outlet Type", background: "#A9A7A6", },
                { value: "Outlet Category", background: "#A9A7A6", },
                { value: "Pan No.", background: "#A9A7A6" },
                { value: "Vat No.", background: "#A9A7A6" },
                { value: "Distributers", background: "#A9A7A6", },
                { value: "Created By", background: "#A9A7A6", },
                { value: "S. Contacts", background: "#A9A7A6", },
            ]
        });

        WriteData(e.data);

        //recursive function to write the grouped/ungrouped data
        function WriteData(array) {
            array.forEach(function (row, index) {
                if (typeof (row.items) != "undefined") { //if array contains nested items, write a row with group field and enter recursion
                    SheetRow.push({
                        cells: [{
                            value: row.field + " : " + row.value,
                            background: "#E1E1E1",
                            colSpan: SheetRow[0].length,//to span the total number of columns
                            fontSize: 12,
                        }]
                    });
                    WriteData(row.items);
                }
                else { //if array contains no nested items write the row to excelsheet
                    SheetRow.push({
                        cells: [{
                            value: row.Reseller_CODE,
                        }, {
                            value: row.Reseller_NAME,
                        }, {
                            value: row.AREA_Name,
                        }, {
                            value: row.WholeSeller,
                        }, {
                            value: row.Primary_Contact,
                        }, {
                            value: row.Email,
                        }, {
                            value: row.Status,
                        }, {
                            value: row.IsClosed,
                        }, {
                            value: row.ADDRESS,
                        }, {
                            value: row.Location,
                        }, {
                            value: row.GROUP_NAME,
                        }, {
                            value: row.OUTLET_TYPE,
                        }, {
                            value: row.OUTLET_SUBTYPE,
                        }, {
                            value: row.Pan_No,
                        }, {
                            value: row.Vat_No,
                        }, {
                            value: row.DISTRIBUTER_DETAILS,
                        }, {
                            value: row.Created_by_name,
                        }, {
                            value: row.CONTACT_DETAILS,
                        }]
                    });
                }
            });
        }

        //push the footer variable containing total values
        //SheetRow.push({
        //    cells: footer,
        //});

        //pushing Report header details
        SheetRow.unshift({
            cells: [{ value: "Outlet Lists".split('').join(' '), colSpan: SheetRow[0].length }]
        });
        //initializing a workbook with above prepared rows
        var Workbook = new kendo.ooxml.Workbook({
            sheets: [{
                columns: [
                    { autoWidth: true },
                    { autoWidth: true },
                    { autoWidth: true },
                    { autoWidth: true },
                    { autoWidth: true },
                    { autoWidth: true },
                    { autoWidth: true },
                    { width: 100 },
                    { width: 100 },
                    { width: 100 },
                ],
                rows: SheetRow,
            }]
        });
        //finally saving the excel sheet
        kendo.saveAs({
            dataURI: Workbook.toDataURL(),
            fileName: "Outlet.xlsx"
        });
    }

    $scope.ApplySource = function () {
        var url = window.location.protocol + "//" + window.location.host + "/api/Setup/GetResellerList?Source=" + $scope.Source;
        var grid = $("#grid").data("kendoGrid");
        grid.dataSource.transport.options.read.url = url;
        $("#grid").data("kendoGrid").dataSource.read();
        $("#resellerSourceModal").modal('toggle');
    }

    $("#applydp").on("click", function () {
        var url = window.location.protocol + "//" + window.location.host + "/api/Setup/GetResellerList?Source=" + $scope.Source;
        var grid = $("#grid").data("kendoGrid");
        grid.dataSource.transport.options.read.url = url;
        $("#grid").data("kendoGrid").dataSource.read();
        $("#resellerSourceModal").modal('toggle');
    });

    $scope.ClosedChangeEvt = function(val) {
        if (val)
            $scope.active = false;
    }
});

distributionModule.controller('ResellerDetailCtrl', function ($scope) {
    debugger;
    var reportConfig = GetReportSetting("ResellerDetailReport");
    $scope.grid = {
        dataSource: {
            type: "json",
            transport: {
                read: {
                    url: window.location.protocol + "//" + window.location.host + "/api/Report/GetResellerDetailReport?Source=" + $scope.Source,
                    dataType: "json",
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
            schema: {
                model: {
                    fields: {
                    }
                }
            },
            error: function (e) {
                displayPopupNotification("Sorry error occured while processing data", "error");
            },
            pageSize: reportConfig.defaultPageSize,
        },
        toolbar: kendo.template($("#toolbar-template").html()),
        excel: {
            fileName: "Reseller Details",
            allPages: true,
        },
        //excelExport: function (e) {
        //    ExportToExcel(e);
        //    e.preventDefault();
        //},
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
        reorderable: true,
        groupable: true,
        resizable: true,
        scrollable: {
            virtual: true
        },
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
                SaveReportSetting('ResellerSetup', 'grid');
        },
        columnHide: function (e) {
            if ($(".k-widget.k-reset.k-header.k-menu.k-menu-vertical").is(":visible") && checkboxItem != "")
                SaveReportSetting('ResellerSetup', 'grid');
        },
        pageable: {
            refresh: true,
            pageSizes: reportConfig.itemPerPage,
            buttonCount: 5
        },
        pageable: true,
        dataBound: function (o) {
            GetSetupSetting("ResellerSetup");
            var grid = o.sender;
            if (grid.dataSource.total() == 0) {
                var colCount = grid.columns.length;
                $(o.sender.wrapper)
                    .find('tbody')
                    .append('<tr class="kendo-data-row" style="font-size:12px;"><td colspan="' + colCount + '" class="alert alert-danger">Sorry, no data :(</td></tr>');
                displayPopupNotification("No Data Found.", "info");
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

            UpdateReportUsingSetting("ResellerDetailReport", "grid");
            $('div').removeClass('.k-header k-grid-toolbar');
        },
        columns: [{
            field: "Reseller_CODE",
            title: "Code",
        }, {
            field: "Reseller_NAME",
            title: "Name",
        }, {
            field: "ADDRESS",
            title: "Address",
        }, {
            field: "Email",
            title: "Email",
        }, {
            field: "RESELLER_CONTACT",
            title: "Contact",
        }, {
            field: "AREA_Name",
            title: "Area",
        }, {
            field: "Primary_Contact",
            title: "Contact",
        }, {
            field: "OUTLET_TYPE",
            title: "Category",
        }, {
            field: "OUTLET_SUBTYPE",
            title: "Sub-Category",
        }, {
            field: "GROUP_NAME",
            title: "Zone",
        }, {
            field: "DISTRIBUTER_DETAILS",
            title: "Distributors"
        }, {
            field: "ROUTE",
            title: "Route"
        }, {
            field: "SALES_PERSON",
            title: "Sales Person"
        }, {
            field: "WholeSeller",
            title: "WholeSeller",
        }, {
            field: "Active",
            title: "Active",
        }, {
            field: "SOURCE",
            title: "Source",
        }, {
            field: "DELETED_FLAG",
            title: "Deleted",
        }, {
                field: "created_date",
                title: "Created date",
        }, {
                field: "LUPDATE_DATE",
            title: "Update Date",
        }]
    };

});