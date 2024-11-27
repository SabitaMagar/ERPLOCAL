planningModule.controller('EditTarget', function ($scope, $http, $route, $routeParams, $timeout) {

    $scope.targetId = $routeParams.targetId;

    var date = new Date();
    var d = date.getDate();
    var m = date.getMonth();
    var y = date.getFullYear();
    $scope.bsFromDate = '';
    $scope.bsToDate = '';

    $scope.startdate = "";
    $scope.enddate = "";
    $scope.holidayList = [];
    $scope.GroupOptions = {};
    $scope.selectedEmpGroup = {};
    $scope.targetType = null;
    $scope.itemGroup = [];
    $scope.shouldShowSubTargetType = false;
    $scope.showIndividualType = false;
    $scope.showCustomerType = false;
    $scope.selectedSubTarget = "";
    $scope.Type = null;
    $scope.storedData = [];
    $scope.targetTypes = [
        { text: "Sales", value: "SAL" },
        { text: "Collection", value: "COL" }
    ];
    $scope.Types = [
        { text: "Synergy", value: "SNG" },
        { text: "DNM", value: "DNM" }
    ];
    $scope.subTargetTypeOptions = {
        dataTextField: "text",
        dataValueField: "value",
        dataSource: [{ text: "Item", value: "ITM" }, { text: "Customer", value: "CUS" }],
        optionLabel: "Select Sub Target"
    };
    $scope.onTypeChange = function (selectedType) {
        if (selectedType && selectedType.value) {
            $scope.Type = selectedType.value;
            //// Trigger a read operation to reload the data
            $scope.customerGroupOptions.dataSource.read();
            $scope.employeeGroupOptions.dataSource.read();
        }
    };
    $scope.onSubTargetChange = function () {
        if ($scope.selectedSubTarget && $scope.selectedSubTarget.value === "ITM") {
            $scope.showIndividualType = true;
            $scope.showCustomerType = false;
            $scope.subTargetChange();
        } else if ($scope.selectedSubTarget && $scope.selectedSubTarget.value === "CUS") {
            $scope.showIndividualType = false;
            $scope.showCustomerType = true;
            $scope.showEmpType = true;
            $scope.subTargetChange();
        }
    };
    $scope.employeeGroupOptions = {
        optionLabel: "-- Select Group --",
        filter: "contains",
        dataTextField: "GROUP_EDESC",
        dataValueField: "GROUP_ID",
        autoBind: false,
        dataSource: new kendo.data.DataSource({  // Ensure this is a Kendo UI DataSource
            type: "json",
            serverFiltering: true,
            transport: {
                read: {
                    url: function () {
                        // Dynamically generate the URL based on $scope.Type
                        return $scope.Type === "SNG"
                            ? "/api/DistributionPlaningApi/GetGroupEmployees"
                            : "/api/DistributionPlaningApi/GetCustomerGroup";
                    },
                    type: "GET",
                    dataType: "json"
                },
                parameterMap: function (data, type) {
                    var filterValue = (data.filter && data.filter.filters && data.filter.filters.length > 0) ? data.filter.filters[0].value : "";
                    return {
                        filter: filterValue,  // Pass the filter value
                    };
                }
            }
        }),
        filtering: function (e) {
            // Prevent clearing the text that user has typed
            var inputValue = e.filter ? e.filter.value : '';
            if (!inputValue) {
                e.preventDefault(); // Prevent filtering if there's no input
            }
        },
        dataBound: function (e) {
            // Set the value when data has been bound, if necessary
            if ($scope.empGroup) {
                var empGroupArr = $scope.empGroup.split(',');
                this.value(empGroupArr);  // Set the value after data has been bound
            }
        },
    };
    $scope.itemGroupOptions = {
        optionLabel: "-- Select Group --",
        filter: "contains",
        dataTextField: "ITEM_EDESC",
        dataValueField: "ITEM_CODE",
        autoBind: false,
        dataSource: new kendo.data.DataSource({  // Ensure this is a Kendo UI DataSource
            type: "json",
            serverFiltering: true,
            transport: {
                read: {
                    url: "/api/DistributionPlaningApi/GetItemGroup",
                    type: "GET",
                    dataType: "json"
                },
                parameterMap: function (data, type) {
                    var filterValue = (data.filter && data.filter.filters && data.filter.filters.length > 0) ? data.filter.filters[0].value : "";
                    return {
                        filter: filterValue,  // Pass the filter value
                    };
                }
            }
        }),
        filtering: function (e) {
            // Prevent clearing the text that user has typed
            var inputValue = e.filter ? e.filter.value : '';
            if (!inputValue) {
                e.preventDefault(); // Prevent filtering if there's no input
            }
        },
        dataBound: function (e) {
            // Set the value when data has been bound, if necessary
            if ($scope.itemGroup) {
                var itemGroupArr = $scope.itemGroup.split(',');
                this.value(itemGroupArr);  // Set the value after data has been bound
            }
        },
    };
    $scope.customerGroupOptions = {
        optionLabel: "-- Select Group --",
        filter: "contains",
        dataTextField: "GROUP_EDESC",
        dataValueField: "GROUP_ID",
        autoBind: false,
        dataSource: new kendo.data.DataSource({  // Ensure this is a Kendo UI DataSource
            type: "json",
            serverFiltering: true,
            transport: {
                read: {
                    url: function () {
                        // Dynamically generate the URL based on $scope.Type
                        return $scope.Type === "SNG"
                            ? "/api/DistributionPlaningApi/GetCustomerSNGGroup"
                            : "/api/DistributionPlaningApi/GetCustomerGroup";
                    },
                    type: "GET",
                    dataType: "json"
                },
                parameterMap: function (data, type) {
                    var filterValue = (data.filter && data.filter.filters && data.filter.filters.length > 0) ? data.filter.filters[0].value : "";
                    return {
                        filter: filterValue,  // Pass the filter value
                    };
                }
            }
        }),
        filtering: function (e) {
            // Prevent clearing the text that user has typed
            var inputValue = e.filter ? e.filter.value : '';
            if (!inputValue) {
                e.preventDefault(); // Prevent filtering if there's no input
            }
        },
        dataBound: function (e) {
            if ($scope.cusGroup) {
                var cusGroupArr = $scope.cusGroup.split(',');
                this.value(cusGroupArr);
            }
        },
    };
    // Ensure the function is called when selectedSubTarget changes
    $scope.$watch('selectedSubTarget', function (newVal, oldVal) {
        if (newVal !== oldVal) {
            $scope.onSubTargetChange();
        }
    });
    $scope.onTargetTypeChange = function (type) {
        if (type && type.value === 'SAL') {
            $scope.shouldShowSubTargetType = true;
            $scope.targetType = type.value;
            $scope.cancelClick();
        } else {
            $scope.showEmpType = true;
            $scope.showCustomerType = false;
            $scope.shouldShowSubTargetType = false;
            $scope.showIndividualType = false;
            $scope.targetType = type.value;
            $scope.cancelClick();
        }
    };
    $scope.getMasterEmployeeCodes = function () {
        if ($scope.selectedEmpGroup && $scope.selectedEmpGroup.length > 0) {
            if ($scope.Type == "SNG") {
                return $scope.selectedEmpGroup.map(function (emp) {
                    return "'" + emp.MASTER_CUSTOMER_CODE + "'";
                }).join(', ');
            } else {
                return $scope.selectedEmpGroup.map(function (emp) {
                    return "'" + emp.GROUP_ID + "'";
                }).join(', ');
            }
        }
        return '';
    };
    $scope.getItemCodes = function () {
        if ($scope.selectedItmGroup && $scope.selectedItmGroup.length > 0) {
            return $scope.selectedItmGroup.map(function (emp) {
                return "'" + emp.MASTER_ITEM_CODE + "'";
            }).join(', ');
        }
        return '';
    };
    $scope.getCustomerCodes = function () {
        if ($scope.selectedCusGroup && $scope.selectedCusGroup.length > 0) {
            if ($scope.Type == "SNG") {
                return $scope.selectedCusGroup.map(function (cus) {
                    return "'" + cus.MASTER_CUSTOMER_CODE + "'";
                }).join(', ');
            } else {
                return $scope.selectedCusGroup.map(function (cus) {
                    return "'" + cus.GROUP_ID + "'";
                }).join(', ');
            }
        }
        return '';
    };
    $scope.employeeOptions = {
        optionLabel: "-- Select Employee --",
        filter: "contains", // Enable 'contains' filter mode
        dataTextField: "EMPLOYEE_EDESC",
        dataValueField: "EMPLOYEE_CODE",
        autoBind: false, 
        delay: 300, 
        dataSource: new kendo.data.DataSource({
            type: "json",
            serverFiltering: true, // Enable server-side filtering
            transport: {
                read: {
                    url: function () {
                        // Dynamically generate the URL based on $scope.Type
                        return $scope.Type === "SNG"
                            ? "/api/DistributionPlaningApi/getSNGEmployees"
                            : "/api/DistributionPlaningApi/GetEmployees";
                    },
                    type: "GET",
                    dataType: "json"
                },
                parameterMap: function (data, type) {
                    var empGroupCodes = $scope.getMasterEmployeeCodes();
                    var filterValue = (data.filter && data.filter.filters && data.filter.filters.length > 0) ? data.filter.filters[0].value : "";
                    return {
                        filter: filterValue,  // Pass the filter value
                        empGroup: empGroupCodes  // Pass additional parameters like employee group codes
                    };
                }
            }
        }),
        filtering: function (e) {
            // Prevent clearing the text that user has typed
            var inputValue = e.filter ? e.filter.value : '';
            if (!inputValue) {
                e.preventDefault(); // Prevent filtering if there's no input
            }
        },
        dataBound: function (e) {
            if ($scope.individualEmp) {
                var EmpArr = $scope.individualEmp.split(',');
                this.value(EmpArr); // Set MultiSelect value after data is loaded
            }
        },
    };
    $scope.individualOptions = {
        optionLabel: "-- Select Item --",
        filter: "contains",
        dataTextField: "ITEM_EDESC",
        dataValueField: "ITEM_CODE",
        autoBind: false,
        dataSource: new kendo.data.DataSource({  // Ensure this is a Kendo UI DataSource
            type: "json",
            serverFiltering: true,
            transport: {
                read: {
                    url: "/api/DistributionPlaningApi/GetItemLists",
                    type: "GET",
                    dataType: "json"
                },
                parameterMap: function (data, type) {
                    var itmGroupCodes = $scope.getItemCodes();
                    var filterValue = (data.filter && data.filter.filters && data.filter.filters.length > 0) ? data.filter.filters[0].value : "";
                    return {
                        filter: filterValue,
                        itmGroup: itmGroupCodes
                    };
                }
            }
        }),
        filtering: function (e) {
            // Prevent clearing the text that user has typed
            var inputValue = e.filter ? e.filter.value : '';
            if (!inputValue) {
                e.preventDefault(); // Prevent filtering if there's no input
            }
        },
        dataBound: function (e) {
            // Set the value when data has been bound, if necessary
            if ($scope.individualItem) {
                var itemArr = $scope.individualItem.split(',');
                this.value(itemArr);  // Set the value after data has been bound
            }
        },
    };
    $scope.customerOptions = {
        optionLabel: "-- Select Item --",
        filter: "contains",
        dataTextField: "CUSTOMER_EDESC",
        dataValueField: "CUSTOMER_CODE",
        autoBind: false,
        dataSource: new kendo.data.DataSource({  // Ensure this is a Kendo UI DataSource
            type: "json",
            serverFiltering: true,
            transport: {
                read: {
                    url: function () {
                        // Dynamically generate the URL based on $scope.Type
                        return $scope.Type === "SNG"
                            ? "/api/DistributionPlaningApi/GetCustomerSNGLists"
                            : "/api/DistributionPlaningApi/GetCustomerLists";
                    },
                    type: "GET",
                    dataType: "json"
                },
                parameterMap: function (data, type) {
                    var cusGroupCodes = $scope.getCustomerCodes();
                    var filterValue = (data.filter && data.filter.filters && data.filter.filters.length > 0) ? data.filter.filters[0].value : "";
                    return {
                        filter: filterValue,
                        cusGroup: cusGroupCodes
                    };
                }
            }
        }),
        filtering: function (e) {
            // Prevent clearing the text that user has typed
            var inputValue = e.filter ? e.filter.value : '';
            if (!inputValue) {
                e.preventDefault(); // Prevent filtering if there's no input
            }
        },
        dataBound: function (e) {
            // Set the value when data has been bound, if necessary
            if ($scope.individualCustomer) {
                var cusArr = $scope.individualCustomer.split(',');
                this.value(cusArr);  // Set the value after data has been bound
            }
        },
    };
    $scope.individualOptions.dataSource.read();
    $scope.customerOptions.dataSource.read();
    $scope.employeeOptions.dataSource.read();
    $scope.employeeGroupOptions.dataSource.read();
    $scope.customerGroupOptions.dataSource.read();

    $scope.$watch('selectedItmGroup', function (newGroups, oldGroups) {
        var individualMultiSelect = $("#individual").data("kendoMultiSelect");
        if (individualMultiSelect) {
            individualMultiSelect.dataSource.read();
        }
    });
    $scope.$watch('selectedCusGroup', function (newVal, oldVal) {
        // Refresh the employee MultiSelect to fetch new data based on the updated group selection
        var customerMultiSelect = $("#customerList").data("kendoMultiSelect");
        if (customerMultiSelect) {
            customerMultiSelect.dataSource.read();
        }
    }, true);

    // Fetch data from API
    $http.get('/api/DistributionPlaningApi/getDataView?targetId=' + $scope.targetId)
        .then(function (response) {
            // Process the response data
            $scope.targetData = response.data.TargetData;
            $scope.targetName = $scope.targetData[0].TARGET_NAME;
            $scope.selectedTarget = $scope.targetData[0].TARGET_TYPE;
            $scope.targetType = $scope.selectedTarget;
            $scope.SubTarget = $scope.targetData[0].SUB_TARGET_TYPE;
            $scope.itemGroup = $scope.targetData[0].ITEM_GROUP;
            $scope.empGroup = $scope.targetData[0].EMPLOYEE_GROUP;
            $scope.cusGroup = $scope.targetData[0].CUSTOMER_GROUP;
            $scope.Type = $scope.targetData[0].FLAG;
            $scope.selectedType = $scope.Type;
            $scope.employeeOptions.dataSource.read();
            $scope.employeeGroupOptions.dataSource.read();
            $scope.customerGroupOptions.dataSource.read();
            $scope.customerOptions.dataSource.read();
            var arrItem = [];
            if (!_.isEmpty($scope.itemGroup))
                $.each($scope.itemGroup.split(','), function (i, obj) {
                    arrItem.push(obj);
                });
            $("#itemGroup").data("kendoMultiSelect").value(arrItem);
            var arrEmp = [];
            if (!_.isEmpty($scope.empGroup))
                $.each($scope.empGroup.split(','), function (i, obj) {
                    arrEmp.push(obj);
                });
            $("#empGroup").data("kendoMultiSelect").value(arrEmp);
            var arrCustomer = [];
            if (!_.isEmpty($scope.cusGroup))
                $.each($scope.cusGroup.split(','), function (i, obj) {
                    arrCustomer.push(obj);
                });
            $("#customerGroup").data("kendoMultiSelect").value(arrCustomer);

            var assignEmployees = $scope.targetData.map(function (item) {
                return item.ASSIGN_EMPLOYEE; // Collect all assign_employee values
            });

            var distinctAssignEmployees = _.uniq(assignEmployees);
            $scope.individualEmp = distinctAssignEmployees.join(',');

            $("#employees").data("kendoMultiSelect").value(distinctAssignEmployees);

            var itemList = $scope.targetData.map(function (item) {
                return item.CODE; // Collect all assign_employee values
            });
            var distinctItems = _.uniq(itemList);
            if ($scope.SubTarget == 'ITM') {
                $scope.individualItem = distinctItems.join(',');
            } else if ($scope.SubTarget == 'CUS') {
                $scope.individualCustomer = distinctItems.join(',');
            }
            else {
                $scope.individualEmp = distinctItems.join(',');
            }
            // Set visibility based on target types
            $scope.shouldShowSubTargetType = ($scope.selectedTarget === 'SAL');
            $scope.showIndividualType = ($scope.SubTarget === 'ITM');
            $scope.showCustomerType = ($scope.SubTarget === 'CUS');

            $scope.dateData = response.data.DateFilter;

            // Initialize or update the grid
            initializeGrid();
            // Update dropdown
            $timeout(function () {
                var dropdown = $("#subTargetType").data("kendoDropDownList");
                if (dropdown) {
                    dropdown.value($scope.SubTarget);
                    dropdown.trigger("change");
                }
            }, 100);

        }, function (error) {
            displayPopupNotification(error.data, "error");
        });

    function transformData(data, SubTarget) {
        const transformedData = {};
        const dateCounter = {}; // To keep track of the counter for each employee

        // Use _.uniqBy to filter unique entries based on FROM_DATE for each MASTER_CODE
        const uniqueData = _.uniq(data, item => item.MASTER_CODE + item.FROM_DATE);

        uniqueData.forEach(item => {
            // Initialize the entry for this MASTER_CODE if it doesn't exist
            if (!transformedData[item.MASTER_CODE]) {
                let edescKey, codeKey;

                if (SubTarget === 'ITM') {
                    edescKey = 'ITEM_EDESC';
                    codeKey = 'ITEM_CODE';
                } else if (SubTarget === 'CUS') {
                    edescKey = 'CUSTOMER_EDESC';
                    codeKey = 'CUSTOMER_CODE';
                } else {
                    edescKey = 'EMPLOYEE_EDESC';
                    codeKey = 'EMPLOYEE_CODE';
                }

                transformedData[item.MASTER_CODE] = {
                    [edescKey]: item.MASTER_CODE,
                    [codeKey]: item.CODE
                };
                dateCounter[item.MASTER_CODE] = 0; // Initialize counter for this entry
            }

            // Use the counter for quantity and amount fields instead of the date
            const counter = dateCounter[item.MASTER_CODE];
            transformedData[item.MASTER_CODE][`MU_CODE`] = item.MU_CODE;
            // Set quantity and amount fields based on the counter
            transformedData[item.MASTER_CODE][`quantity_${counter}`] = item.TARGET_QUANTITY;
            transformedData[item.MASTER_CODE][`amount_${counter}`] = item.TARGET_AMOUNT;
            // Increment the counter for the next date
            dateCounter[item.MASTER_CODE]++;
        });

        return Object.values(transformedData);
    }

    function initializeGrid() {
        if ($scope.dateData.length > 0) {
            var startDate = $scope.dateData[0].START_DATE;
            var endDate = $scope.dateData[0].LAST_DATE;
            var dateFilter = $scope.dateData[0].DATE_FILTER;
            var columns = [];

            var formattedStartDate = moment(startDate).format('DD-MMM-YYYY');
            var formattedEndDate = moment(endDate).format('DD-MMM-YYYY');

            $("#ddlDateFilterVoucher").val(dateFilter).prop('readonly', true);
            $("#FromDateVoucher").val(formattedStartDate).prop('readonly', true);
            $("#ToDateVoucher").val(formattedEndDate).prop('readonly', true);
            $("#fromInputDateVoucher").val(AD2BS(moment(formattedStartDate).format("YYYY-MM-DD"))).prop('readonly', true);
            $("#toInputDateVoucher").val(AD2BS(moment(formattedEndDate).format("YYYY-MM-DD"))).prop('readonly', true);

            const transformedData = transformData($scope.targetData, $scope.SubTarget);
            var startDt = new Date(startDate);
            var endDt = new Date(endDate);
            var dayCounter = 0;

            // Add Employee Name (MASTER_CODE) column
            if ($scope.SubTarget == "ITM") {
                columns.push({
                    title: "Item Name",
                    field: "ITEM_EDESC",
                    width: 150, locked: true
                });
            } else if ($scope.SubTarget == "CUS") {
                columns.push({
                    title: "Customer Name",
                    field: "CUSTOMER_EDESC",
                    width: 150, locked: true
                });
            } else {
                columns.push({
                    title: "Employee Name",
                    field: "EMPLOYEE_EDESC",
                    width: 150, locked: true
                });

            }


            // Fetch holiday details and update the grid
            $http.get('/api/DistributionPlaningApi/HolidayDetails?fromDate=' + formattedStartDate + '&toDate=' + formattedEndDate).then(function (response) {
                $scope.holidayList = response.data.map(function (holiday) {
                    return moment(holiday.HOLIDAY_DATE).format("YYYY-MM-DD");
                });

                // Generate columns based on dates, skipping holidays and Saturdays
                while (startDt <= endDt) {
                    var currentDate = moment(startDt).format("YYYY-MM-DD");
                    var bsDate = AD2BS(currentDate);
                    var quantityField = `quantity_${dayCounter}`;
                    var amountField = `amount_${dayCounter}`;
                    var currentDay = startDt.getDay();

                    if (currentDay !== 6 && !$scope.holidayList.includes(currentDate)) {
                        var columnConfig = {
                            title: bsDate,
                            headerAttributes: { style: "text-align: center;" },
                            columns: []
                        };

                        if ($scope.targetType == "SAL") {
                            columnConfig.columns.push({
                                field: quantityField,
                                title: "Quantity",
                                template: `<input type="number" ng-model="dataItem.${quantityField}" ng-model-options="{debounce: 999999999}" class="form-control" style="text-align: right;" />`,
                                width: 80
                            });
                        }

                        columnConfig.columns.push({
                            field: amountField,
                            title: "Amount",
                            template: `<input type="number" ng-model="dataItem.${amountField}" ng-model-options="{debounce: 999999999}" class="form-control" style="text-align: right;"/>`,
                            width: 100
                        });

                        columns.push(columnConfig);
                        dayCounter++;
                    }

                    startDt.setDate(startDt.getDate() + 1);
                }

                if ($scope.targetType == "SAL") {
                    // Add Total Quantity and Total Amount columns
                    columns.push({
                        title: "Total Quantity",
                        field: "totalQuantity",
                        attributes: {
                            style: "text-align: right;" // Right-align the content
                        },
                        width: 100, locked: true
                    });
                }
                columns.push({
                    title: "Total Amount",
                    field: "totalAmount",
                    attributes: {
                        style: "text-align: right;" // Right-align the content
                    },
                    width: 100, locked: true
                });
                $scope.calculateRowTotals(transformedData, columns);
                $scope.addTotalRow(transformedData, columns, $scope.SubTarget);
                $scope.gridOptions.dataSource.data = transformedData;
                $scope.gridOptions.columns = columns;
                $("#grid").data("kendoGrid").setOptions($scope.gridOptions);
                $("#grid").data("kendoGrid").refresh();
            });
        }
    }
    $scope.updateTotals = function (datas) {
        var selectedType = $scope.selectedSubTarget.value;
        var dataSource = $scope.gridOptions.dataSource;
        var data = datas;
        var columns = dataSource.fields;
        // Filter date columns
        var dateColumns = columns.filter(function (col) {
            if (col.columns) {
                return col.columns.some(function (subCol) {
                    return subCol.field && (subCol.field.startsWith('quantity_') || subCol.field.startsWith('amount_'));
                });
            }
            return false;
        });
        var filteredData = data.filter(function (item) {
            return !(item.EMPLOYEE_EDESC === "Total" || item.ITEM_EDESC === "Total" || item.CUSTOMER_EDESC === "Total");
        });

        $scope.calculateRowTotals(filteredData, dateColumns);
        $scope.addTotalRow(filteredData, dateColumns, selectedType);
        $scope.gridOptions.dataSource.data = filteredData;
        $scope.gridOptions.columns = columns;
        $("#grid").data("kendoGrid").setOptions($scope.gridOptions);
        $("#grid").data("kendoGrid").refresh();
    };

    $scope.calculateRowTotals = function (data, dynamicColumns) {
        data.forEach(function (item) {
            var totalQuantity = 0;
            var totalAmount = 0;
            dynamicColumns.forEach(function (col) {
                if (Array.isArray(col.columns)) {
                    col.columns.forEach(function (subCol) {
                        if (subCol.field.startsWith('quantity_')) {
                            var quantityValue = parseFloat(item[subCol.field]) || 0;
                            totalQuantity += quantityValue;
                        } else if (subCol.field.startsWith('amount_')) {
                            var amountValue = parseFloat(item[subCol.field]) || 0;
                            totalAmount += amountValue;
                        }
                    });
                }
            });
            item.totalQuantity = totalQuantity.toFixed(2);
            item.totalAmount = totalAmount.toFixed(2);
        });
    }
    $scope.addTotalRow = function (data, columns, selectedType) {
        var totals = {
            totalQuantity: 0,
            totalAmount: 0
        };

        // Initialize totals object with zeros
        columns.forEach(function (col) {
            if (Array.isArray(col.columns)) {
                col.columns.forEach(function (subCol) {
                    if (subCol.field.startsWith('quantity_') || subCol.field.startsWith('amount_')) {
                        totals[subCol.field] = 0;
                    }
                });
            }
        });

        // Accumulate totals for each column
        data.forEach(function (item) {
            columns.forEach(function (col) {
                if (Array.isArray(col.columns)) {
                    col.columns.forEach(function (subCol) {
                        if (subCol.field.startsWith('quantity_')) {
                            var quantityValue = parseFloat(item[subCol.field]) || 0;
                            totals[subCol.field] += quantityValue;
                        } else if (subCol.field.startsWith('amount_')) {
                            var amountValue = parseFloat(item[subCol.field]) || 0;
                            totals[subCol.field] += amountValue;
                        }
                    });
                }
            });
        });
        // Accumulate totals
        data.forEach(function (item) {
            totals.totalQuantity += parseFloat(item.totalQuantity) || 0;
            totals.totalAmount += parseFloat(item.totalAmount) || 0;
        });
        var key;
        if (selectedType) {
            if (selectedType == "ITM") {
                key = 'ITEM_EDESC';
            } else if (selectedType == "CUS") {
                key = 'CUSTOMER_EDESC';
            }
        } else {
            key = 'EMPLOYEE_EDESC';
        }
        var totalRow = {
            [key]: 'Total',
            totalQuantity: totals.totalQuantity.toFixed(2),
            totalAmount: totals.totalAmount.toFixed(2)
        };

        columns.forEach(function (col) {
            if (Array.isArray(col.columns)) {
                col.columns.forEach(function (subCol) {
                    if (subCol.field.startsWith('quantity_') || subCol.field.startsWith('amount_')) {
                        totalRow[subCol.field] = parseFloat(parseFloat(totals[subCol.field]).toFixed(2));
                    }
                });
            }
        });
        data.push(totalRow);
    }

    $scope.$watch('selectedEmpGroup', function (newVal, oldVal) {
        // Refresh the employee MultiSelect to fetch new data based on the updated group selection
        var employeeMultiSelect = $("#employees").data("kendoMultiSelect");
        if (employeeMultiSelect) {
            employeeMultiSelect.dataSource.read();
        }
    }, true);
    $scope.remove = function (index) {
        $scope.events.splice(index, 1);
    };
    $scope.cancelClick = function () {
        window.location.href = "/Planning/DistributionPlaning/Index#!Planning/TargetSetup";
    };
    $scope.subTargetChange = function () {
        // Reset the form fields
        $scope.selectedItmGroup = [];
        $scope.selectedEmpGroup = [];
        $scope.selectedIndividual = [];
        $scope.selectedEmployees = [];
        $scope.customfrequencyday = null;
        $scope.frequencyWiseRouteAssign = false;

        // Reset Kendo UI widgets
        var kendoWidgets = [
            $("#itemGroup").data("kendoMultiSelect"),
            $("#empGroup").data("kendoMultiSelect"),
            $("#individual").data("kendoMultiSelect"),
            $("#employees").data("kendoMultiSelect")
        ];
        kendoWidgets.forEach(function (widget) {
            if (widget) {
                widget.value(""); // Clear value for DropDownList and MultiSelect
                widget.dataSource.read(); // Refresh the data source if necessary
            }
        });

        // Reset the form validation state
        if ($scope.targetForm) {
            $scope.targetForm.$setPristine();
            $scope.targetForm.$setUntouched();
        }
    };
    $scope.assignTargets = function () {
        var dataToAssign = $scope.collectDataForAssign();
        $http.post('/api/DistributionPlaningApi/SaveTargetData', dataToAssign)
            .then(function (response) {
                window.location = "/Planning/DistributionPlaning/Index#!Planning/TargetSetup";
                displayPopupNotification(response.data, "success");
            }, function (error) {
                displayPopupNotification(response.data, "error");
            });
    };
    $scope.collectDataForAssign = function () {
        // Store as arrays or null
        if (_.isEmpty($scope.selectedItmGroup)) {
            $scope.selectedItmGroup = $("#itemGroup").data("kendoMultiSelect").value();
            $scope.selectedItmGroup = _.filter($("#itemGroup").data("kendoMultiSelect").dataSource.data(), function (da) {
                return _.includes($scope.selectedItmGroup, da.ITEM_CODE); // Check if EMPLOYEE_CODE is in the selectedEmployees array
            });
        }

        var itmGroup = $scope.selectedItmGroup && $scope.selectedItmGroup.length > 0
            ? $scope.selectedItmGroup.map(function (item) {
                return item.ITEM_CODE;
            }).join(',')  // Join array elements into a comma-separated string
            : '';
        if (_.isEmpty($scope.selectedEmpGroup)) {
            $scope.selectedEmpGroup = $("#empGroup").data("kendoMultiSelect").value();
            $scope.selectedEmpGroup = _.filter($("#empGroup").data("kendoMultiSelect").dataSource.data(), function (da) {
                return _.includes($scope.selectedEmpGroup, da.GROUP_ID); // Check if EMPLOYEE_CODE is in the selectedEmployees array
            });
        }

        var empGroup = $scope.selectedEmpGroup && $scope.selectedEmpGroup.length > 0
            ? $scope.selectedEmpGroup.map(function (emp) {
                return emp.GROUP_ID; // Or the relevant property
            }).join(',')  // Join array elements into a comma-separated string
            : '';
        if (_.isEmpty($scope.selectedCusGroup)) {
            $scope.selectedCusGroup = $("#customerGroup").data("kendoMultiSelect").value();
            $scope.selectedCusGroup = _.filter($("#customerGroup").data("kendoMultiSelect").dataSource.data(), function (da) {
                return _.includes($scope.selectedCusGroup, da.GROUP_ID); // Check if EMPLOYEE_CODE is in the selectedEmployees array
            });
        }

        var cusGroup = $scope.selectedCusGroup && $scope.selectedCusGroup.length > 0
            ? $scope.selectedCusGroup.map(function (cus) {
                return cus.GROUP_ID; // Or the relevant property
            }).join(',')  // Join array elements into a comma-separated string
            : '';
        var cusMasterGroup = $scope.selectedCusGroup && $scope.selectedCusGroup.length > 0
            ? $scope.selectedCusGroup.map(function (item) {
                return item.MASTER_CUSTOMER_CODE;
            }).join(',')  // Join array elements into a comma-separated string
            : '';
        var itmMasterGroup = $scope.selectedItmGroup && $scope.selectedItmGroup.length > 0
            ? $scope.selectedItmGroup.map(function (item) {
                return item.MASTER_ITEM_CODE;
            }).join(',')  // Join array elements into a comma-separated string
            : '';
        if ($scope.Type == 'SNG') {
            var empMasterGroup = $scope.selectedEmpGroup && $scope.selectedEmpGroup.length > 0
                ? $scope.selectedEmpGroup.map(function (emp) {
                    return emp.MASTER_CUSTOMER_CODE;
                }).join(',')
                : '';
        } else {
            var empMasterGroup = $scope.selectedEmpGroup && $scope.selectedEmpGroup.length > 0
                ? $scope.selectedEmpGroup.map(function (emp) {
                    return emp.GROUP_ID;
                }).join(',')
                : '';
        }
        if (_.isEmpty($scope.selectedIndividual)) {
            $scope.selectedIndividual = $("#individual").data("kendoMultiSelect").value();
            $scope.selectedIndividual = _.filter($("#individual").data("kendoMultiSelect").dataSource.data(), function (da) {
                return _.includes($scope.selectedIndividual, da.ITEM_CODE); // Check if EMPLOYEE_CODE is in the selectedEmployees array
            });
        }

        var individual = $scope.selectedIndividual && $scope.selectedIndividual.length > 0 ? $scope.selectedIndividual.map(function (itm) {
            return {
                item_code: itm.ITEM_CODE,
                mu_code: itm.MU_CODE
            };
        }) : [];
        if (_.isEmpty($scope.selectedEmployees)) {
            $scope.selectedEmployees = $("#employees").data("kendoMultiSelect").value(); // Array of selected values
            $scope.selectedEmployees = _.filter($("#employees").data("kendoMultiSelect").dataSource.data(), function (da) {
                return _.includes($scope.selectedEmployees, da.EMPLOYEE_CODE); // Check if EMPLOYEE_CODE is in the selectedEmployees array
            });
        }


        var employees = $scope.selectedEmployees && $scope.selectedEmployees.length > 0 ? $scope.selectedEmployees.map(function (emp) {
            return emp.EMPLOYEE_CODE; // Or the relevant property
        }) : [];
        if (_.isEmpty($scope.selectedCustomer)) {
            $scope.selectedCustomer = $("#customerList").data("kendoMultiSelect").value(); // Array of selected values
            $scope.selectedCustomer = _.filter($("#customerList").data("kendoMultiSelect").dataSource.data(), function (da) {
                return _.includes($scope.selectedCustomer, da.CUSTOMER_CODE); // Check if EMPLOYEE_CODE is in the selectedEmployees array
            });
        }


        var customers = $scope.selectedCustomer && $scope.selectedCustomer.length > 0 ? $scope.selectedCustomer.map(function (cus) {
            return cus.CUSTOMER_CODE; // Or the relevant property
        }) : [];
        // Prepare the data to assign, if any field is empty, it will be set to null
        var dataToAssign = {
            targetId: $scope.targetId || 0,
            targetType: $scope.targetType || null,
            DateFilter: $("#ddlDateFilterVoucher").val() ? $("#ddlDateFilterVoucher").val() : null,
            subTargetType: $scope.selectedSubTarget ? $scope.selectedSubTarget.value : null,
            targetName: $scope.targetName || null,
            itemGroup: itmGroup,
            employeeGroup: empGroup,
            itemMasterGroup: itmMasterGroup,
            customerGroup: cusGroup,
            customerMasterGroup: cusMasterGroup,
            employeeMasterGroup: empMasterGroup,
            flag: $scope.Type,
            items: individual,
            customers: customers,
            employees: employees, // Store as an array or null
            gridData: $scope.getGridData() || null // Assuming getGridData() could return empty or undefined
        };
        return dataToAssign;
    };

    $scope.getGridData = function () {
        var gridData = [];
        var grid = $("#grid").data("kendoGrid");
        var data = grid.dataSource.view(); // Get the data currently displayed in the grid
        data.forEach(function (rowData) {
            var count = 0;
            grid.columns.forEach(function (column, index) {
                if (column.columns) {
                    var quantityField = `quantity_${count}`;
                    var amountField = `amount_${count}`;
                    // Retrieve the quantity and amount, default to null if not found or invalid
                    var quantity = rowData[quantityField] !== undefined && rowData[quantityField] !== null && rowData[quantityField] !== '' ? rowData[quantityField] : 0;
                    var amount = rowData[amountField] !== undefined && rowData[amountField] !== null && rowData[amountField] !== '' ? rowData[amountField] : 0;
                    // Only push to gridData if itemCode exists (or EMPLOYEE_CODE), and handle missing MU_CODE
                    if (rowData.ITEM_CODE || rowData.EMPLOYEE_CODE || rowData.CUSTOMER_CODE) {
                        gridData.push({
                            itemCode: rowData.ITEM_CODE || rowData.EMPLOYEE_CODE || rowData.CUSTOMER_CODE,
                            muCode: rowData.MU_CODE || "",
                            date: moment(BS2AD(column.title)).format("DD-MMM-YYYY"),
                            quantity: quantity,
                            amount: amount
                        });
                    }
                    count++;
                }
            });
        });
        return gridData;
    };
    $scope.searchClick = function () {
        // Initial validation checks
        if (!$scope.targetType) {
            displayPopupNotification("Select Target type!", "error");
            return;
        } else if (!$scope.targetName) {
            displayPopupNotification("Target Name is required!", "error");
            return;
        } else if ($scope.selectedSubTarget == null && $scope.targetType == 'SAL') {
            displayPopupNotification("Select Sub Target Type!", "error");
            return;
        }

        var selectedType = $scope.selectedSubTarget;
        var selectedEmployees = $scope.selectedEmployees || [];
        if (_.isEmpty($scope.selectedEmployees)) {
            $scope.selectedEmployees = $("#employees").data("kendoMultiSelect").value(); // Array of selected values
            selectedEmployees = _.filter($("#employees").data("kendoMultiSelect").dataSource.data(), function (da) {
                return _.includes($scope.selectedEmployees, da.EMPLOYEE_CODE); // Check if EMPLOYEE_CODE is in the selectedEmployees array
            });
        }
        var selectedItems = $scope.selectedIndividual || [];
        if (_.isEmpty($scope.selectedIndividual)) {
            $scope.selectedIndividual = $("#individual").data("kendoMultiSelect").value();
            selectedItems = _.filter($("#individual").data("kendoMultiSelect").dataSource.data(), function (da) {
                return _.includes($scope.selectedIndividual, da.ITEM_CODE); // Check if EMPLOYEE_CODE is in the selectedEmployees array
            });
        }
        var selectedCustomer = $scope.selectedCustomer || [];
        if (_.isEmpty($scope.selectedCustomer)) {
            $scope.selectedCustomer = $("#customerList").data("kendoMultiSelect").value();
            selectedCustomer = _.filter($("#customerList").data("kendoMultiSelect").dataSource.data(), function (da) {
                return _.includes($scope.selectedCustomer, da.CUSTOMER_CODE); // Check if EMPLOYEE_CODE is in the selectedEmployees array
            });
        }
        var bsFromDate = $('#FromDateVoucher').val() || '';
        var bsToDate = $('#ToDateVoucher').val() || '';

        var formattedFromDate = moment(bsFromDate, 'YYYY-MMM-DD').format('DD-MMM-YYYY');
        var formattedToDate = moment(bsToDate, 'YYYY-MMM-DD').format('DD-MMM-YYYY');

        // Fetching holiday details from the API
        $http.get('/api/DistributionPlaningApi/HolidayDetails?fromDate=' + formattedFromDate + '&toDate=' + formattedToDate).then(function (response) {
            $scope.holidayList = response.data.map(function (holiday) {
                return moment(holiday.HOLIDAY_DATE).format("YYYY-MM-DD");
            });

            function getDateDifferenceInDays(startDate, endDate) {
                var start = new Date(startDate);
                var end = new Date(endDate);
                var totalDays = 0;

                while (start <= end) {
                    var currentDay = start.getDay();
                    var currentDate = moment(start).format("YYYY-MM-DD");

                    if (currentDay !== 6 && !$scope.holidayList.includes(currentDate)) {
                        totalDays++;
                    }

                    start.setDate(start.getDate() + 1);
                }

                return totalDays;
            }

            var dateDifference = getDateDifferenceInDays(bsFromDate, bsToDate);
            var quantityPerDay = $scope.customQty ? ($scope.customQty / dateDifference).toFixed(2) : 0;
            var amountPerDay = $scope.customAmt ? ($scope.customAmt / dateDifference).toFixed(2) : 0;

            function generateBSDateRangeColumns(bsStartDate, bsEndDate) {
                var columns = [];
                var startDate = new Date(bsStartDate);
                var endDate = new Date(bsEndDate);
                var dayCounter = 0; // Counter to create unique fields for each date

                while (startDate <= endDate) {
                    var bsDate = AD2BS(moment(startDate).format("YYYY-MM-DD"));
                    var quantityField = `quantity_${dayCounter}`;
                    var amountField = `amount_${dayCounter}`;

                    var currentDay = startDate.getDay();
                    var currentDate = moment(startDate).format("YYYY-MM-DD");

                    // Skip Saturdays and holidays
                    if (currentDay !== 6 && !$scope.holidayList.includes(currentDate)) {
                        if ($scope.targetType !== undefined && $scope.targetType !== null && $scope.targetType !== "" && $scope.targetType == "SAL") {
                            columns.push({
                                title: bsDate,
                                headerAttributes: { style: "text-align: center;" },
                                columns: [
                                    {
                                        field: quantityField,
                                        title: "Quantity",
                                        attributes: {
                                            style: "text-align: right;" // Right-align the content
                                        },
                                        template: `<input type="number" ng-model="dataItem.${quantityField}" ng-model-options="{debounce: 999999999}" class="form-control" style="text-align: right;" />`,
                                        width: 80
                                    },
                                    {
                                        field: amountField,
                                        title: "Amount",
                                        attributes: {
                                            style: "text-align: right;" // Right-align the content
                                        },
                                        template: `<input type="number" ng-model="dataItem.${amountField}" ng-model-options="{debounce: 999999999}" class="form-control" style="text-align: right;"/>`,
                                        width: 100
                                    }
                                ]
                            });
                        } else {
                            columns.push({
                                title: bsDate,
                                headerAttributes: { style: "text-align: center;" },
                                columns: [
                                    {
                                        field: amountField,
                                        title: "Amount",
                                        attributes: {
                                            style: "text-align: right;" // Right-align the content
                                        },
                                        template: `<input type="number" ng-model="dataItem.${amountField}" ng-model-options="{debounce: 999999999}" class="form-control" style="text-align: right;"/>`,
                                        width: 100
                                    }
                                ]
                            });
                        }
                        dayCounter++;
                    }

                    startDate.setDate(startDate.getDate() + 1);
                }
                return columns;
            }

            function createTotalColumn() {
                if ($scope.targetType !== undefined && $scope.targetType !== null && $scope.targetType !== "" && $scope.targetType == "SAL") {
                    return {
                        columns: [
                            {
                                title: "Total Quantity",
                                field: "totalQuantity",
                                width: 100,
                                locked: true,
                                attributes: {
                                    style: "text-align: right;" // Right-align the content
                                }
                            },
                            {
                                title: "Total Amount",
                                field: "totalAmount",
                                width: 100,
                                locked: true,
                                attributes: {
                                    style: "text-align: right;" // Right-align the content
                                }
                            }
                        ]
                    };
                } else {
                    return {
                        columns: [
                            {
                                title: "Total Amount",
                                field: "totalAmount",
                                width: 100,
                                locked: true,
                                attributes: {
                                    style: "text-align: right;" // Right-align the content
                                }
                            }
                        ]
                    };
                }
            }

            // Generate columns dynamically based on the date range
            var dynamicColumns = generateBSDateRangeColumns(bsFromDate, bsToDate);
            var totalColumn = createTotalColumn();

            // Handle selected employees and items
            if (selectedEmployees.length > 0 && typeof selectedEmployees[0] === 'object') {
                selectedEmployees = selectedEmployees.map(function (obj) {
                    return obj.EMPLOYEE_CODE; // Extract employee codes from objects
                });
            } else {
                selectedEmployees = selectedEmployees.map(String).map(code => code.trim());
            }

            if (selectedItems.length > 0 && typeof selectedItems[0] === 'object') {
                selectedItems = selectedItems.map(function (obj) {
                    return obj.ITEM_CODE; // Extract item codes from objects
                });
            } else {
                selectedItems = selectedItems.map(String).map(code => code.trim());
            }
            if (selectedCustomer.length > 0 && typeof selectedCustomer[0] === 'object') {
                selectedCustomer = selectedCustomer.map(function (obj) {
                    return obj.CUSTOMER_CODE; // Extract item codes from objects
                });
            } else {
                selectedCustomer = selectedCustomer.map(String).map(code => code.trim());
            }

            // Fetch and manipulate data for employees or items
            if (selectedType && selectedType.value == "ITM") {
                $scope.individualOptions.dataSource.read().then(function () {
                    var data = $scope.individualOptions.dataSource.view();
                    if (selectedItems.length > 0) {
                        data = data.filter(function (item) {
                            var itemCode = String(item.ITEM_CODE).trim();
                            return selectedItems.includes(itemCode);
                        });
                    }
                    data.forEach(function (item) {
                        dynamicColumns.forEach(function (subCol) {
                            subCol.columns.forEach(function (col) {
                                if (col.field.startsWith('quantity_')) {
                                    item[col.field] = parseFloat(parseFloat(quantityPerDay).toFixed(2));
                                }
                                else if (col.field.startsWith('amount_')) {
                                    item[col.field] = parseFloat(parseFloat(amountPerDay).toFixed(2));
                                }
                            });
                        });
                    });
                    $scope.calculateRowTotals(data, dynamicColumns);
                    $scope.addTotalRow(data, dynamicColumns, selectedType.value); // Pass dynamicColumns here
                    $scope.gridOptions.dataSource.data = data;
                    $scope.gridOptions.columns = [
                        { field: "ITEM_EDESC", title: "Item Name", width: 150, locked: true }, // Non-editable
                        ...dynamicColumns,
                        ...totalColumn.columns // Add total columns
                    ];
                    $("#individual").data("kendoMultiSelect").value(selectedItems);
                    $("#employees").data("kendoMultiSelect").value(selectedEmployees);
                    $("#customerList").data("kendoMultiSelect").value(selectedCustomer);
                    $("#grid").data("kendoGrid").setOptions($scope.gridOptions);
                    $("#grid").data("kendoGrid").refresh();
                });
            } else if (selectedType && selectedType.value == "CUS") {
                $scope.customerOptions.dataSource.read().then(function () {
                    var data = $scope.customerOptions.dataSource.view();
                    if (selectedCustomer.length > 0) {
                        data = data.filter(function (item) {
                            var cusCode = String(item.CUSTOMER_CODE).trim();
                            return selectedCustomer.includes(cusCode);
                        });
                    }
                    data.forEach(function (cus) {
                        dynamicColumns.forEach(function (subCol) {
                            subCol.columns.forEach(function (col) {
                                if (col.field.startsWith('quantity_')) {
                                    cus[col.field] = parseFloat(parseFloat(quantityPerDay).toFixed(2));
                                }
                                else if (col.field.startsWith('amount_')) {
                                    cus[col.field] = parseFloat(parseFloat(amountPerDay).toFixed(2));
                                }
                            });
                        });
                    });
                    $scope.calculateRowTotals(data, dynamicColumns);
                    $scope.addTotalRow(data, dynamicColumns, selectedType.value); // Pass dynamicColumns here
                    $scope.gridOptions.dataSource.data = data;
                    $scope.gridOptions.columns = [
                        { field: "CUSTOMER_EDESC", title: "Customer Name", width: 150, locked: true }, // Non-editable
                        ...dynamicColumns,
                        ...totalColumn.columns // Add total columns
                    ];
                    $("#individual").data("kendoMultiSelect").value(selectedItems);
                    $("#employees").data("kendoMultiSelect").value(selectedEmployees);
                    $("#customerList").data("kendoMultiSelect").value(selectedCustomer);
                    $("#grid").data("kendoGrid").setOptions($scope.gridOptions);
                    $("#grid").data("kendoGrid").refresh();
                });
            }
            else {
                $scope.employeeOptions.dataSource.read().then(function () {
                    var data = $scope.employeeOptions.dataSource.view();
                    if (selectedEmployees.length > 0) {
                        data = data.filter(function (item) {
                            var employeeCode = String(item.EMPLOYEE_CODE).trim();
                            return selectedEmployees.includes(employeeCode);
                        });
                    }
                    data.forEach(function (item) {
                        dynamicColumns.forEach(function (subCol) {
                            subCol.columns.forEach(function (col) {
                                if (col.field.startsWith('quantity_')) {
                                    item[col.field] = parseFloat(parseFloat(quantityPerDay).toFixed(2));
                                }
                                else if (col.field.startsWith('amount_')) {
                                    item[col.field] = parseFloat(parseFloat(amountPerDay).toFixed(2));
                                }
                            });
                        });
                    });
                    $scope.calculateRowTotals(data, dynamicColumns);
                    $scope.addTotalRow(data, dynamicColumns, selectedType.value); // Pass dynamicColumns here
                    $scope.gridOptions.dataSource.data = data;
                    $scope.gridOptions.columns = [
                        { field: "EMPLOYEE_EDESC", title: "Employee Name", width: 150, locked: true }, // Non-editable
                        ...dynamicColumns,
                        ...totalColumn.columns // Add total columns
                    ];
                    $("#employees").data("kendoMultiSelect").value(selectedEmployees);
                    $("#grid").data("kendoGrid").setOptions($scope.gridOptions);
                    $("#grid").data("kendoGrid").refresh();
                });
            }
        });
    };
    $scope.gridOptions = {
        dataSource: {
            data: [], // Initially empty, will be filled after search
            schema: {
                model: {
                    fields: {
                        EMPLOYEE_EDESC: { type: "string", editable: false },
                        ITEM_EDESC: { type: "string", editable: false },
                        totalAmount: { type: "number", editable: false },
                        totalQuantity: { type: "number", editable: false } // Field for total column
                    }
                }
            },
            pageSize: 50
        },
        scrollable: true,
        height: 400,
        pageable: {
            refresh: true,         
            pageSizes: true,       
            buttonCount: 5,        
            info: true,           
            previousNext: true     
        },
        columns: [], // Columns will be set dynamically
        dataBound: function (e) {
            var grid = this;
            // Add a 'change' event listener to input elements within the grid's tbody
            grid.tbody.find('input').on('change', function () {
                var dataItem = grid.dataItem($(this).closest('tr')); // Get the data item (row) that was edited
                var newValue = parseFloat($(this).val()) || 0; // Get the new value from the input, default to 0 if NaN
                var $input = $(this);
                var ngModel = $input.attr('ng-model');
                var field = ngModel.split('.').pop();
                // Update the data item with the new value
                dataItem.set(field, newValue); ''
                // Update the totals across all rows
                var dataSource = grid.dataSource.data();
                $scope.updateTotals(dataSource);
                // Refresh the grid
                grid.refresh();
            });
        }
    };
});
