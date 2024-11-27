
QMModule.controller('itemCtrl', function ($scope, $rootScope, $http, $routeParams, $window, $filter) {
    $scope.childRowIndex;
    $scope.productDataSource = {
        type: "json",
        serverFiltering: true,
        transport: {
            read: {
                url: "/api/TemplateApi/GetAllProductsListByFilter",

            },
            parameterMap: function (data, action) {

                var ItemCodeValue = $rootScope.ITEM_CODE_DEFAULTVAL;
                var newParams;
                if (data.filter != undefined) {
                    if (data.filter.filters[0] != undefined) {
                        //newParams = {
                        //    filter: data.filter.filters[0].value
                        //};
                        //return newParams;
                        if (data.filter.filters[0].value != "") {
                            newParams = {
                                filter: data.filter.filters[0].value
                            };
                            return newParams;
                        }
                        else {
                            newParams = {
                                filter: "!@$"
                            };
                            return newParams;
                        }

                    }
                    else {
                        //newParams = {
                        //    filter: ""
                        //};
                        //return newParams;

                        if (ItemCodeValue != "" && ItemCodeValue != undefined) {
                            newParams = {
                                filter: ItemCodeValue
                            };
                            return newParams;
                        }
                        else {
                            newParams = {
                                filter: "!@$"
                            };
                            return newParams;
                        }
                    }

                }
                else {
                    //newParams = {
                    //    filter: ""
                    //};
                    //return newParams;
                    if (ItemCodeValue != "" && ItemCodeValue != undefined) {
                        newParams = {
                            filter: ItemCodeValue
                        };
                        return newParams;
                    }
                    else {
                        newParams = {
                            filter: "!@$"
                        };
                        return newParams;
                    }
                }
            }
        },

    };



    //$scope.ItemCodeOption = {
    //    dataSource: $scope.productDataSource,
    //    dataBound: function (e) {
    //    var product = $("#products").data("kendoComboBox");
    //    if (product != undefined) {
    //        product.setOptions({
    //            template: $.proxy(kendo.template("#= formatValue(ItemDescription, this.text()) #"), product)
    //            });
    //        }
    //    }
    //}

    $scope.ItemCodeOption = {
        dataSource: $scope.productDataSource,
        template: '<span>{{dataItem.ItemDescription}}</span>  --- ' +
            '<span>{{dataItem.Type}}</span>',
        dataBound: function (e) {
            if (this.element[0].attributes['location-index'] == undefined) {
                var product = $($(".cproducts")).data("kendoComboBox");
            }
            else {
                var index = this.element[0].attributes['product-index'].value;
                var productLength = ((parseInt(index) + 1) * 3) - 1;
                var product = $($(".cproducts")[productLength]).data("kendoComboBox");
            }
            //DOne due to error of formatValue
            //var index = this.element[0].attributes['product-index'].value;
            //var productLength = ((parseInt(index) + 1) * 3) - 1;
            //var product = $($(".cproducts")[productLength]).data("kendoComboBox");
            //if (product != undefined) {
            //    product.setOptions({
            //        template: $.proxy(kendo.template("#=formatValue(ItemDescription,Type,this.text()) #"), product)
            //    });
            //    $scope.getmucode(index, $rootScope.ITEM_CODE_DEFAULTVAL);

            //}
            setTimeout(function () {
                if ($scope.havRefrence == 'Y' && $scope.freeze_master_ref_flag == "Y") {
                    $(".cproducts").prop('readonly', true);
                }
            }, 50);
        }
    }

    //customer popup advanced search// --start

    var getProductsByUrl = window.location.protocol + "//" + window.location.host + "/api/TemplateApi/GetProducts";
    $scope.treeData = new kendo.data.HierarchicalDataSource({
        transport: {
            read: {
                url: getProductsByUrl,
                type: 'GET',
                data: function (data, evt) {
                }
            },

        },
        schema: {
            parse: function (data) {
                return data;
            },
            model: {
                id: "ITEM_CODE",
                parentId: "PRE_ITEM_CODE",
                children: "Items",
                fields: {
                    ITEM_CODE: { field: "ITME_CODE", type: "string" },
                    ITEM_EDESC: { field: "ITEM_EDESC", type: "string" },
                    parentId: { field: "PRE_ITEM_CODE", type: "string", defaultValue: "00" },
                }
            }
        }
    });

    //treeview expand on startup
    $scope.onDataBound = function () {
        //$('#producttree_' + $scope.childRowIndex).data("kendoTreeView").expand('.k-item');
    }

    //treeview on select
    $scope.options = {
        loadOnDemand: false,
        select: function (e) {
            var currentItem = e.sender.dataItem(e.node);
            $('#productGrid_' + $scope.childRowIndex).removeClass("show-displaygrid");
            $("#productGrid_" + $scope.childRowIndex).html("");
            BindProductGrid(currentItem.itemCode, currentItem.masterItemCode, "");
            $scope.$apply();
        },
    };

    //search whole data on search button click
    $scope.BindSearchGrid = function () {
        $scope.searchText = $scope.txtSearchString;
        BindProductGrid("", "", $scope.searchText);
    }

    //Grid Binding main Part
    function BindProductGrid(itemCode, itemMasterCode, searchText) {
        $scope.productGridOptions = {
            dataSource: {
                type: "json",
                transport: {
                    read: "/api/TemplateApi/GetProductListByItemCode?itemCode=" + itemCode + '&itemMastercode=' + itemMasterCode + '&searchText=' + searchText,
                },
                schema: {
                    type: "json",
                    model: {
                        fields: {
                            ITEM_CODE: { type: "string" },
                            ITEM_EDESC: { type: "string" }
                        }
                    }
                },
                pageSize: 30,
            },
            scrollable: true,
            sortable: true,
            resizable: false,
            pageable: true,
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
                        startswith: "Starts with",
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
            },
            dataBound: function (e) {
                $("#productGrid_" + $scope.childRowIndex + " tbody tr").css("cursor", "pointer");
                $("#productGrid_" + $scope.childRowIndex + " tbody tr").on('dblclick', function () {

                    var productcode = $(this).find('td span').html();
                    var productName = $(this).find('td span[ng-bind="dataItem.ITEM_EDESC"]').html();
                    console.log("ProductName==================>>>" + productName);
                    $("#products_" + $scope.childRowIndex).data('kendoComboBox').dataSource.data([{ ItemCode: productcode, ItemDescription: productName, Type: "code" }]);
                    $scope.childModels[$scope.childRowIndex]["ITEM_CODE"] = productcode;
                    if ($($(".cproduct_" + $scope.childRowIndex)[0]).closest('div').parent().hasClass('borderRed')) {
                        $($(".cproduct_" + $scope.childRowIndex)[0]).closest('div').parent().removeClass('borderRed')
                    }
                    $('#productModal_' + $scope.childRowIndex).modal('toggle');
                    $scope.getmucode($scope.childRowIndex, productcode);
                    if ($scope.ModuleCode === "04") {

                        if ($scope.serial_tracking_flag == "Y") {

                            var config = {
                                async: false
                            };
                            $scope.BATCH_MODAL.push({ ITEM_CODE: "", ITEM_EDESC: "", MU_CODE: "", LOCATION_CODE: "", QUANTITY: "", TRACK: [] });
                            $scope.BATCH_CHILD_MODAL.push({ SERIAL_NO: 1, TRACKING_SERIAL_NO: "" });
                            for (var itt = 0; itt < $scope.childModels.length; itt++) {

                                var responsevaluewise = $http.get('/api/TemplateApi/getItemCountResult?code=' + $scope.childModels[itt].ITEM_CODE, config);
                                responsevaluewise.then(function (resvaluewise) {

                                    if (resvaluewise.data == true) {


                                        for (var it = 0; it < $scope.childModels.length; it++) {

                                            if ($scope.childModels[it].hasOwnProperty("FROM_LOCATION_CODE") || $scope.childModels[it].hasOwnProperty("FROM_LOCATION_CODE")) {
                                                if ($scope.childModels[it].FROM_LOCATION_CODE != 'undefined' || $scope.childModels[it].TO_LOCATION_CODE != 'undefined') {
                                                    locationcode = $scope.childModels[it].FROM_LOCATION_CODE == 'undefined' ? $scope.childModels[it].TO_LOCATION_CODE : $scope.childModels[it].FROM_LOCATION_CODE;


                                                    var ibdrreq = "/api/TemplateApi/GetDataForBatchModalsales?itemcode=" + $scope.childModels[it].ITEM_CODE + "&loactioncode=" + $scope.childModels[it].FROM_LOCATION_CODE;
                                                    $http.get(ibdrreq).then(function (ibdrreqresults) {

                                                        if (ibdrreqresults.data.length > 0) {
                                                            var rows = ibdrreqresults.data;

                                                            //for (var itn = 0; itn < $scope.childModels.length; itn++) {
                                                            //    setbatchDataOnModal(rows, itn);
                                                            //}
                                                            //$scope.dynamicSerialTrackingModalData = [];
                                                            var batchModel = angular.copy($scope.BATCH_MODAL[0]);
                                                            var batchChildModel = angular.copy($scope.BATCH_CHILD_MODAL[0]);
                                                            var rowsObj = rows[$scope.childRowIndex];
                                                            //$scope.dynamicSerialTrackingModalData.push(batchModel);
                                                            $scope.dynamicSerialTrackingModalData[$scope.childRowIndex] = $scope.getObjWithKeysFromOtherObj(batchModel, rowsObj);
                                                            $scope.dynamicSerialTrackingModalData[$scope.childRowIndex].TRACK = [];
                                                            for (var bm = 0; bm < rows.length; bm++) {


                                                                $scope.dynamicSerialTrackingModalData[$scope.childRowIndex].TRACK.push(batchChildModel);
                                                            }
                                                            for (var a = 0; a < rows.length; a++) {


                                                                for (var b = 0; b < $scope.dynamicSerialTrackingModalData.length; b++) {

                                                                    if (rows[a].ITEM_CODE == $scope.dynamicSerialTrackingModalData[b].ITEM_CODE) {

                                                                        $scope.dynamicSerialTrackingModalData[b].TRACK[a] = $scope.getObjWithKeysFromOtherObj(batchChildModel, rows[a]);
                                                                    }

                                                                }
                                                            }
                                                            $scope.ShowBatchTransDetail($scope.childRowIndex);
                                                        }
                                                    })
                                                }
                                            }
                                        }
                                    }

                                });

                            }
                        }
                    }
                    else {
                        if ($scope.serial_tracking_flag == "Y") {

                            var icrreq = "/api/TemplateApi/getItemCountResult?code=" + productcode;
                            $http.get(icrreq).then(function (icrreqresults) {

                                if (icrreqresults.data == true) {

                                    $scope.dynamicSerialTrackingModalData[$scope.childRowIndex].ITEM_CODE = productcode;
                                    $scope.dynamicSerialTrackingModalData[$scope.childRowIndex].ITEM_EDESC = productName;

                                    $scope.dynamicSerialTrackingModalData[$scope.childRowIndex].MU_CODE = $scope.childModels[$scope.childRowIndex].MU_CODE;
                                    if ($scope.childModels[0].hasOwnProperty("FROM_LOCATION_CODE")) {
                                        $scope.dynamicSerialTrackingModalData[$scope.childRowIndex].LOCATION_CODE = $scope.childModels[$scope.childRowIndex]["FROM_LOCATION_CODE"];
                                    }
                                    $(".serialtrackFlag_" + $scope.childRowIndex).modal('toggle');
                                }

                            });

                        }
                        if ($scope.batch_tracking_flag == "Y") {
                            debugger;
                            var icrreq = "/api/TemplateApi/getBatchItemCountResult?code=" + productcode;
                            $http.get(icrreq).then(function (icrreqresults) {
                                debugger
                                if (icrreqresults.data == true) {
                                    debugger;
                                    $scope.dynamicBatchTrackingModalData[$scope.childRowIndex].ITEM_CODE = productcode;
                                    $scope.dynamicBatchTrackingModalData[$scope.childRowIndex].ITEM_EDESC = productName;
                                    $scope.dynamicBatchTrackingModalData[$scope.childRowIndex].MU_CODE = $scope.childModels[$scope.childRowIndex].MU_CODE;
                                    if ($scope.childModels[0].hasOwnProperty("FROM_LOCATION_CODE")) {
                                        $scope.dynamicBatchTrackingModalData[$scope.childRowIndex].LOCATION_CODE = $scope.childModels[$scope.childRowIndex]["FROM_LOCATION_CODE"];
                                    }

                                    $(".batchtrackflag_" + $scope.childRowIndex).modal('toggle');
                                }

                            });


                        }
                    }

                    $scope.$apply();
                })
            },
            columns: [{
                hidden: true,
                field: "ITEM_CODE",

            },
            {
                field: "ITEM_EDESC",
                title: "item Name",
                width: "120px"
            },
            {
                field: "MU_EDESC",
                title: "INDEX UNIT",
                width: "120px"
            },
            {
                field: "CATEGORY_EDESC",
                title: "CATEGORY",
                width: "120px"
            },
            {
                field: "PURCHASE_PRICE",
                title: "Purchase Price",
                width: "120px",
                template: "#= (PURCHASE_PRICE == null) ? '0 ' : PURCHASE_PRICE #"
            },

            {
                field: "CREATED_BY",
                title: "CREATED BY",
                width: "120px"
            }, {
                field: "CREATED_DATE",
                title: "CREATED DATE",
                template: "#= kendo.toString(kendo.parseDate(CREATED_DATE),'dd MMM yyyy') #",
                width: "120px"
            }


            ]
        };
    }



    //show modal popup
    $scope.BrowseTreeListForProducts = function (index) {

        if ($scope.havRefrence == 'Y' && $scope.freeze_master_ref_flag == "Y") {
            var referencenumber = $('#refrencetype').val();
            var custref = $(".referenceCustomer").val();
            var btnref = $("#refrenceTypeMultiSelect").data('kendoDropDownList').dataItem().REF_CODE;
            if ($scope.ModuleCode != '01' && (referencenumber !== "" || custref !== "" || btnref !== "")) {
                return;
            }
        }
        if ($scope.freeze_master_ref_flag == "N") {
            $('#productModal_' + index).modal('show');
            $scope.childRowIndex = index;
            document.popupindex = index;
            if ($('#producttree_' + $scope.childRowIndex).data("kendoTreeView") != undefined)
                $('#producttree_' + $scope.childRowIndex).data("kendoTreeView").expand('.k-item');
        }
    }



});

