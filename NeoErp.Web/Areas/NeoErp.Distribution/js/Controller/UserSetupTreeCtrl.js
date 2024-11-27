distributionModule.controller('UserSetupCtrl', function ($scope, DistSetupService, $routeParams) {
    $scope.param = $routeParams.param;
    $scope.pageName = "Add User";
    $scope.EmployeeMultiSelectName = "Employee";
    $scope.createPanel = true;
    $scope._old = {};
    $scope.selectedArea = null;
    //load multiselect 
    $scope.EmployeeMultiSelect = {
        dataSource: new kendo.data.DataSource({
            transport: {
                read: {
                    url: window.location.protocol + "//" + window.location.host + "/api/UserSetup/getUserEmployee",
                    dataType: "json",
                    contentType: "application/json; charset=utf-8"
                }
            }
        }),
        dataTextField: "EMPLOYEE_EDESC",
        dataValueField: "EMPLOYEE_CODE",
        maxSelectedItems: 1,
        enable: false,
        filter: "contains",
        height: 600,
        headerTemplate: '<div class="col-md-offset-4"><strong>Employee...</strong></div>',
        placeholder: "Select Employee...",
        autoClose: true,
        dataBound: function () {
            var current = this.value();
            this._savedOld = current.slice(0);
            angular.element('#EmployeeMultiSelect_listbox').slimScroll({
                height: '200px'
            });
        },
        change: function (evt) {
            if (evt.sender.dataItem() != undefined)
                $scope.userSetupTree.FullName = evt.sender.dataItem().EMPLOYEE_EDESC;
        }
    }
    $scope.RoleMultiSelect = {
        dataSource: new kendo.data.DataSource({
            transport: {
                read: {
                    url: window.location.protocol + "//" + window.location.host + "/api/Setup/GetDistUserRole",
                    dataType: "json",
                    contentType: "application/json; charset=utf-8"
                }
            }
        }),
        dataTextField: "ROLE_NAME",
        dataValueField: "ROLE_CODE",
        maxSelectedItems: 1,
        filter: "contains",
        height: 600,
        headerTemplate: '<div class="col-md-offset-4"><strong>Role...</strong></div>',
        placeholder: "Select Role...",
        autoClose: true,
        dataBound: function () {
            var current = this.value();
            this._savedOld = current.slice(0);
            angular.element('#RoleMultiSelect_listbox').slimScroll({
                height: '100px'
            });
        },
        change: function (evt) {
            if (evt.sender.dataItem() == undefined)
                return false;
            if (evt.sender.dataItem().ROLE_CODE == 2) {
                //this function call only if the the role is Distributor
                DistributionDataSet();
            }
            else if ($scope._old.EmployeeMultiSelectData != undefined) {
                angular.element('#EmployeeMultiSelect').data("kendoMultiSelect").setDataSource(new kendo.data.DataSource({
                    data: $scope._old.EmployeeMultiSelectData,
                }));
                $scope.EmployeeMultiSelectName = "Employee";
            }
        }
    }

    $scope.AreaMultiSelect = {
        dataTextField: "AREA_NAME",
        dataValueField: "AREA_CODE",
        height: 600,
        valuePrimitive: true,
        filter: "contains",
        //maxSelectedItems: 1,
        headerTemplate: '<div class="col-md-offset-3"><strong>Area...</strong></div>',
        placeholder: "Select Area...",
        autoClose: false,
        dataBound: function (e) {
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
    };

    var productsDataSource = new kendo.data.DataSource({
        transport: {
            read: {
                url: window.location.protocol + "//" + window.location.host + "/api/DistributionPurchase/GetAllItems",
                dataType: "json"
            }
        }
    });
    // Watch for changes in AreaMultiSelect
    $scope.$watch('userSetupTree.AreaMultiSelect', function (newVal, oldVal) {
        if (newVal !== oldVal) {
            $scope.selectedArea = newVal;
            var customerMultiSelect = $("#distCustomerSelect").data("kendoMultiSelect");

            if (customerMultiSelect) {
                // Clear previous values (optional)
                customerMultiSelect.value([]);

                // Force the customer MultiSelect to refresh its data
                customerMultiSelect.dataSource.transport.read();
                customerMultiSelect.dataSource.read();
                customerMultiSelect.refresh();  // Ensures the MultiSelect UI updates
            }
        }
    });

    // Define customer MultiSelect options
    $scope.distCustomerSelectOptions = {
        dataTextField: "CUSTOMER_EDESC",
        dataValueField: "CUSTOMER_CODE",
        height: 600,
        valuePrimitive: true,
        filter: "contains",
        headerTemplate: '<div class="col-md-offset-3"><strong>Customer...</strong></div>',
        placeholder: "Select Customer...",
        autoClose: false,
        dataBound: function (e) {
            $("#" + e.sender.element[0].id + "_listbox").slimScroll({ 'height': '179px', 'scroll': 'scroll' });
        },
        dataSource: new kendo.data.DataSource({
            type: "json",
            serverFiltering: true,
            transport: {
                read: {
                    url: window.location.protocol + "//" + window.location.host + "/api/Distribution/GetCustomersByArea",
                    type: "GET",
                    dataType: "json",
                    data: function () {
                        var areaCodes = $scope.selectedArea ? $scope.selectedArea.map(function (area) {
                            return "'" + area + "'"; // Wrap each area code in single quotes
                        }).join(', ') : '';

                        return { areaCode: areaCodes };
                    }
                }
            }
        })
    };


    //productsDataSource.fetch(function () {
    //    $scope.distBrandSelectOptions = {
    //        dataTextField: "BRAND_NAME",
    //        dataValueField: "BRAND_NAME",
    //        height: 600,
    //        valuePrimitive: true,
    //        headerTemplate: '<div class="col-md-offset-3"><strong>Brands...</strong></div>',
    //        placeholder: "Select Brands...",
    //        autoClose: false,
    //        dataBound: function (e) {
    //            $("#" + e.sender.element[0].id + "_listbox").slimScroll({ 'height': '179px', 'scroll': 'scroll' });
    //        },
    //        dataSource: new kendo.data.DataSource({
    //            data: _.uniq(this.data(), "BRAND_NAME"),
    //        }),
    //        change: function () {
    //            buildFilters(this.dataItems());
    //        }
    //    };
    //});

    productsDataSource.fetch(function () {
        $scope.distBrandSelectOptions = {
            dataTextField: "BRAND_NAME",
            dataValueField: "ITEM_CODE",
            height: 600,
            valuePrimitive: true,
            headerTemplate: '<div class="col-md-offset-3"><strong>Brands...</strong></div>',
            placeholder: "Select Brands...",
            autoClose: false,
            dataBound: function (e) {
                       var current = this.value();
                    this._savedOld = current.slice(0);
                    $("#" + e.sender.element[0].id + "_listbox").slimScroll({ 'height': '179px', 'scroll': 'scroll' });
            },
            dataSource: new kendo.data.DataSource({
                data: _.uniq(this.data(), "BRAND_NAME"),
            }),
            change: function () {
                buildFilters(this.dataItems());
            }
        };
    });

    $scope.distItemsSelectOptions = {
        dataTextField: "ITEM_EDESC",
        dataValueField: "ITEM_CODE",
        height: 600,
        valuePrimitive: true,
        headerTemplate: '<div class="col-md-offset-3"><strong>Items...</strong></div>',
        placeholder: "Select Items...",
        autoClose: false,
        dataBound: function (e) {
            var current = this.value();
            this._savedOld = current.slice(0);
            $("#" + e.sender.element[0].id + "_listbox").slimScroll({ 'height': '179px', 'scroll': 'scroll' });
        },
        dataSource: productsDataSource

    };

    function buildFilters(dataItems) {
        var filters = [],
            length = dataItems.length,
            idx = 0, dataItem;
        if (length == 0) {
            $("#distItemsSelect").data("kendoMultiSelect").value("");
        }
        for (; idx < length; idx++) {
            dataItem = dataItems[idx];

            var data = $("#distItemsSelect").data("kendoMultiSelect").dataSource.data();
            var filterdata = _.filter(data, function (da) { return da.BRAND_NAME == dataItem.BRAND_NAME; });
            for (var i = 0; i < filterdata.length; i++) {
                filters.push(filterdata[i].ITEM_CODE);
            }

            $("#distItemsSelect").data("kendoMultiSelect").value(filters);
        }


    };
    function Clear() {
        $scope.userSetupTree = {};
        angular.element('#EmployeeMultiSelect').data("kendoMultiSelect").value([]);
        angular.element('#EmployeeMultiSelect').data("kendoMultiSelect").options.enable = true;
        angular.element('#RoleMultiSelect').data("kendoMultiSelect").value([]);
        angular.element('#AreaMultiSelect').data("kendoMultiSelect").value([]);
        angular.element('#distItemsSelect').data("kendoMultiSelect").value([]);
    }

    //treelist
    angular.element('#treelist').kendoTreeList({
        dataSource: new kendo.data.TreeListDataSource({
            transport: {
                read: {
                    url: window.location.protocol + "//" + window.location.host + "/api/Setup/GetUserSetupTreeList",
                    dataType: "json"
                }
            },
            schema: {
                model: {
                    id: "CODE",
                    parentId: "MASTER_CODE",
                    fields: {
                        CODE: { type: "number", nullable: false },
                        MASTER_CODE: { field: "MASTER_CODE", type: "number", nullable: true }
                    }
                }
            }
        }),

        dataBound: function (e) {
            GetSetupSetting("UserSetUpTree");
        },
        editable: {
            move: true
        },
        dragstart: function (e) {
            if (e.source.IS_GROUP == 'Y')
                e.preventDefault();
        },
        drop: function (e) {
            $scope.UpdateUserTreeOrder(e);
        },
        columns: [
            { field: "NAME", expandable: true, title: "Name" },
            { field: "FULLNAME", title: "Full Name" },
            { field: "ROLE_NAME", title: "Role" },
            { field: "AREA_NAME", title: "Area" },
            { field: "EMAIL", title: "Email" },
            { field: "CONTACT_NO", title: "Contact No" }
        ],
    });

    function DistributionDataSet() {
        var dataSource = new kendo.data.DataSource({
            transport: {
                read: {
                    url: window.location.protocol + "//" + window.location.host + "/api/UserSetup/GetDistributor",
                    dataType: "json",
                    contentType: "application/json; charset=utf-8"
                }
            },
            schema: {
                parse: function (response) {
                    _.each(response, function (x) {
                        x.EMPLOYEE_CODE = x.DISTRIBUTOR_CODE;
                        x.EMPLOYEE_EDESC = x.CUSTOMER_EDESC;
                    });
                    return response
                }
            }
        });
        $scope._old.EmployeeMultiSelectData = $.extend(true, {}, angular.element('#EmployeeMultiSelect').data("kendoMultiSelect").dataSource.data());
        angular.element('#EmployeeMultiSelect').data("kendoMultiSelect").setDataSource(dataSource);
        $scope.EmployeeMultiSelectName = "Distributor";
    }

    // datasource for the salesperson

    function salesPersonDataSet() {
        var salesDataSource = new kendo.data.DataSource({
            transport: {
                read: {
                    url: window.location.protocol + "//" + window.location.host + "/api/UserSetup/getUserEmployee",
                    dataType: "json",
                    contentType: "application/json; charset=utf-8"
                }
            },


        });
        $scope._old.EmployeeMultiSelectData = $.extend(true, {}, angular.element('#EmployeeMultiSelect').data("kendoMultiSelect").dataSource.data());
        angular.element('#EmployeeMultiSelect').data("kendoMultiSelect").setDataSource(salesDataSource);
        $scope.EmployeeMultiSelectName = "Employee";
    }


    //right click
    angular.element('#menu').kendoContextMenu({
        target: "#treelist",
        filter: "tbody > tr",
        select: function (e) {
            var button = $(e.item);
            var row = $(e.target);
            var dataItem = $("#treelist").data("kendoTreeList").dataItem(row);

            var areaMultiSelect = angular.element('#AreaMultiSelect').data("kendoMultiSelect");
            $scope._old.areaMultiSelectData = $scope._old.areaMultiSelectData == undefined ? areaMultiSelect.dataSource.data() : $scope._old.areaMultiSelectData;
            areaMultiSelect.setDataSource(new kendo.data.DataSource({
                data: _.filter($scope._old.areaMultiSelectData, function (x) { return x.GROUPID == dataItem.GROUPID }),
                //data: $scope._old.areaMultiSelectData,
            }));

            if (button.text() == "Update") {
                $scope.pageName = "Update User";
                $scope.saveButtonText = "Update";
                var roleMultiSelect = angular.element('#RoleMultiSelect').data("kendoMultiSelect");
                $scope._old.roleMultiSelect = $scope._old.roleMultiSelect == undefined ? roleMultiSelect.dataSource.data() : $scope._old.roleMultiSelect;
                if (dataItem.ROLE_CODE !== undefined) {
                    if (dataItem.ROLE_CODE == '2') {
                        //during the update time this function call
                        DistributionDataSet();
                        roleMultiSelect.setDataSource(new kendo.data.DataSource({
                            data: _.filter($scope._old.roleMultiSelect, function (x) { return x.ROLE_CODE == 2 })
                        }));
                    }


                    else {
                        salesPersonDataSet();
                        roleMultiSelect.setDataSource(new kendo.data.DataSource({
                            data: _.filter($scope._old.roleMultiSelect, function (x) { return x.ROLE_CODE !== 2 })
                        }));
                    }

                }
                $scope.userSetupTree = {
                    attendanceCheckbox: dataItem.ATTENDENCE == 'Y' ? true : false,
                    mobileCheckbox: dataItem.MOBILE == 'Y' ? true : false,
                    activeCheckbox: dataItem.ACTIVE == 'Y' ? true : false,
                    brandingCheckbox: dataItem.BRANDING == 'Y' ? true : false,
                    superCheckbox: dataItem.SUPER_USER == 'Y' ? true : false,
                    CODE: dataItem.CODE,
                    FullName: dataItem.FULLNAME,
                    NAME: dataItem.NAME,
                    EMAIL: dataItem.EMAIL,
                    Password: dataItem.PASSWORD,
                    CONTACT_NO: dataItem.CONTACT_NO,
                    EmployeeMultiSelect: [dataItem.EMPLOYEE_CODE],
                    RoleMultiSelect: [dataItem.ROLE_CODE],
                    AreaMultiSelect1: dataItem.AREA_CODE !== null ? dataItem.AREA_CODE.split(',') : [],
                    ItemCodeMultiSelect: dataItem.ITEM_CODE !== null ? dataItem.ITEM_CODE.split(',') : [],
                    CustomerMultiSelect: dataItem.CUSTOMER_CODE !== null ? dataItem.CUSTOMER_CODE.split(',') : [],
                    BrandMultiSelect: dataItem.ITEM_CODE !== null ? dataItem.ITEM_CODE.split(',') : [],
                }



                var employeeMultiSelect = angular.element('#EmployeeMultiSelect').data("kendoMultiSelect");
                employeeMultiSelect.value($scope.userSetupTree.EmployeeMultiSelect);
                employeeMultiSelect.enable(false);

                //var areaMultiSelect = angular.element('#AreaMultiSelect').data("kendoMultiSelect");
                //$scope._old.areaMultiSelectData = $scope._old.areaMultiSelectData == undefined ? areaMultiSelect.dataSource.data() : $scope._old.areaMultiSelectData;
                //areaMultiSelect.setDataSource(new kendo.data.DataSource({
                //    data: _.filter($scope._old.areaMultiSelectData, function (x) { return x.GROUPID == dataItem.GROUPID}), 
                //    //data: $scope._old.areaMultiSelectData,
                //}));
                areaMultiSelect.value($scope.userSetupTree.AreaMultiSelect1);
                $scope.userSetupTree.AreaMultiSelect = $scope.userSetupTree.AreaMultiSelect1;
                angular.element('#RoleMultiSelect').data("kendoMultiSelect").value($scope.userSetupTree.RoleMultiSelect);
                angular.element('#userSetupTreeCreateModal').modal('show');
            }
            else if (button.text() == "Add") {
                Clear();
                $scope.pageName = "Save User";
                $scope.saveButtonText = "Save";
                angular.element('#EmployeeMultiSelect').data("kendoMultiSelect").enable(true);
                angular.element('#userSetupTreeCreateModal').modal('show');
                $scope.userSetupTree.GROUPID = dataItem.GROUPID;

            }
            else if (button.text() == "Delete") {
                bootbox.confirm({
                    message: "Do you want to delete this User? This cannot be undone.",
                    buttons: {
                        cancel: {
                            label: 'Cancel'
                        },
                        confirm: {
                            label: 'Confirm'
                        }
                    },
                    callback: function (result) {
                        if (result) {
                            $.ajax({
                                type: 'GET',
                                contentType: "application/json; charset=utf-8",
                                dataType: "json",
                                url: window.location.protocol + "//" + window.location.host + "/api/Setup/DeleteUserTree?Code=" + dataItem.CODE,
                                success: function (result) {
                                    if (result == "200") {
                                        displayPopupNotification("Deleted Successfully", "success");
                                        angular.element('#treelist').data("kendoTreeList").dataSource.read();
                                    } else {
                                        displayPopupNotification("Error", "error");
                                    }



                                },
                            });
                        }

                    }
                });
            }

            if (dataItem.parentId == null) {
                $scope.userSetupTree.MASTER_CODE = dataItem.CODE;
            }
            else {
                $scope.userSetupTree.MASTER_CODE = dataItem.GROUPID;
                $scope.userSetupTree.MASTER_CUSTOMER_CODE = dataItem.CODE;
            }
            $scope.$apply();

        }
    });

    $scope.saveUser = function (isValid) {
        debugger;
        if (!isValid) {
            displayPopupNotification("Invalid Field", "warning");
            return;
        }
        var obj = $scope.userSetupTree;
        obj.EMPLOYEE_CODE = $scope.userSetupTree.EmployeeMultiSelect[0];
        obj.ROLE_CODE = $scope.userSetupTree.RoleMultiSelect[0];
        obj.ATTENDENCE = $scope.userSetupTree.attendanceCheckbox ? 'Y' : 'N';
        obj.MOBILE = $scope.userSetupTree.mobileCheckbox ? 'Y' : 'N';
        obj.ACTIVE = $scope.userSetupTree.activeCheckbox ? 'Y' : 'N';
        obj.BRANDING = $scope.userSetupTree.brandingCheckbox ? 'Y' : 'N';
        obj.SUPER_USER = $scope.userSetupTree.superCheckbox ? 'Y' : 'N';
        obj.AREA = $scope.userSetupTree.AreaMultiSelect == null ? [] : $scope.userSetupTree.AreaMultiSelect;
        obj.CUSTOMER = $scope.userSetupTree.CustomerMultiSelect == null ? [] : $scope.userSetupTree.CustomerMultiSelect;
        obj.GROUPID = $scope.userSetupTree.GROUPID;
        obj.BRAND = $scope.userSetupTree.BrandMultiSelect == null ? [] : $scope.userSetupTree.BrandMultiSelect;
        //obj.ITEMS = $scope.userSetupTree.ItemCodeMultiSelect == null ? [] : $scope.userSetupTree.ItemCodeMultiSelect;
        obj.ITEMS = $("#distItemsSelect").data("kendoMultiSelect").value();
        //if ($scope.userSetupTree.ItemCodeMultiSelect.length == 0 && $("#distItemsSelect").data("kendoMultiSelect").value().length>0)
        //    obj.ITEMS = $("#distItemsSelect").data("kendoMultiSelect").value();
        if ($scope.saveButtonText == "Save") {
            DistSetupService.AddUserTree(obj)
                .then(function (result) {
                    console.log("usertree", result);
                    if (result.data.STATUS_CODE === 200) {
                        displayPopupNotification("Saved Successfully", "success");
                        angular.element('#treelist').data("kendoTreeList").dataSource.read();
                        angular.element('#userSetupTreeCreateModal').modal('hide');
                        Clear();
                    }
                    else if (result.data.STATUS_CODE === 300) {
                        displayPopupNotification(result.data.MESSAGE, "warning");
                    }
                    else {
                        displayBarNotification(result.data.MESSAGE, "error");
                    }
                }, function () {
                    displayPopupNotification("Error", "error");
                });
        }
        else {
            DistSetupService.UpdateUserTree(obj)
                .then(function (result) {
                    if (result.data === "200") {
                        displayPopupNotification("Update Successfully", "success");
                        angular.element('#treelist').data("kendoTreeList").dataSource.read();
                        angular.element('#userSetupTreeCreateModal').modal('hide');
                        Clear();
                    }
                    else {
                        displayPopupNotification("Error", "error");
                        angular.element('#userSetupTreeCreateModal').modal('hide');
                    }
                }, function () {
                    displayPopupNotification("Error", "error");
                });
        }

    }


    $scope.UpdateUserTreeOrder = function (e) {
        if (e.valid) {
            //check if destination is group   

            if (e.destination != undefined) // && e.destination.IS_GROUP == 'Y'
                e.source.GROUPID = e.destination.GROUPID;
            else
                e.source.GROUPID = null;
            //console.log("drop", e.source, e.destination, e.valid);
            DistSetupService.UpdateUserTreeOrder(e.source);
        }
    }


    $("#treelist").on("mousedown", "tr[role='row']", function (e) {
        if (e.which === 3) {
            //first remove all selected row
            $('tr.k-state-selected', '#treelist').removeClass('k-state-selected');
            //then display selected row
            $(this).addClass("k-state-selected");
        }
    });


});