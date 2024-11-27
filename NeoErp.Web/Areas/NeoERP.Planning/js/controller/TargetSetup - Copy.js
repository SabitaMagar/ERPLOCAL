planningModule.controller('TargetSetup', function ($scope, $http, $route) {

    var date = new Date();
    var d = date.getDate();
    var m = date.getMonth();
    var y = date.getFullYear();
    $scope.bsFromDate = '';
    $scope.bsToDate = '';

    $scope.startdate = "";
    $scope.enddate = "";
    $scope.endDateToEdit = Date.now();
    $scope.holidayList = [];
    $scope.GroupOptions = {};
    $scope.selectedEmpGroup = {};
    $scope.IsEdit = false;
    $scope.targetType = null;
    $scope.add_edit_option = "Edit";
    $scope.itemGroup = [];
    $scope.shouldShowSubTargetType = false;
    $scope.showIndividualType = false;
    $scope.selectedSubTarget = "";

    $scope.targetTypes = [
        { text: "Sales", value: "SAL" },
        { text: "Collection", value: "COL" }
    ];
    $scope.subTargetTypeOptions = {
        dataTextField: "text",
        dataValueField: "value",
        dataSource: [{ text: "Item", value: "ITM" }, { text: "Customer", value: "CUS" }],
        optionLabel: "Select Sub Target"
    };
    $scope.onSubTargetChange = function () {
        if ($scope.selectedSubTarget && $scope.selectedSubTarget.value === "ITM") {
            $scope.showIndividualType = true;
            $scope.subTargetChange();
        } else if ($scope.selectedSubTarget && $scope.selectedSubTarget.value === "CUS") {
            $scope.showIndividualType = false;
            $scope.showEmpType = true;
            $scope.subTargetChange();
        }
    };

    $scope.employeeGroupOptions = {
        optionLabel: "-- Select Group --",
        filter: "contains",
        dataTextField: "EMPLOYEE_EDESC",
        dataValueField: "EMPLOYEE_CODE",
        autoBind: false,
        dataSource: {
            type: "json",
            serverFiltering: true,
            transport: {
                read: {
                    url: "/api/DistributionPlaningApi/GetGroupEmployees",
                    dataType: "json" // Ensure the correct data type
                },
            }
        },
        dataBound: function () {
        },
        change: function () {
            $scope.showroutelistdiv = true;
        }
    };
    $scope.itemGroupOptions = {
        optionLabel: "-- Select Group --",
        filter: "contains",
        dataTextField: "ITEM_EDESC",
        dataValueField: "ITEM_CODE",
        autoBind: false,
        dataSource: {
            type: "json",
            serverFiltering: true,
            transport: {
                read: {
                    url: "/api/DistributionPlaningApi/GetItemGroup",
                    dataType: "json" // Ensure the correct data type
                },
            }
        },
        dataBound: function () {
        },
        change: function () {
            $scope.showroutelistdiv = true;
        }
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
            $scope.shouldShowSubTargetType = false;
            $scope.showIndividualType = false;
            $scope.targetType = type.value;
            $scope.cancelClick();
        }
    };
    $scope.getMasterEmployeeCodes = function () {
        if ($scope.selectedEmpGroup && $scope.selectedEmpGroup.length > 0) {
            return $scope.selectedEmpGroup.map(function (emp) {
                return "'" + emp.MASTER_EMPLOYEE_CODE + "'";
            }).join(', ');
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
    $scope.employeeOptions = {
        optionLabel: "-- Select Employee --",
        filter: "contains",
        dataTextField: "EMPLOYEE_EDESC",
        dataValueField: "EMPLOYEE_CODE",
        autoBind: false,
        dataSource: new kendo.data.DataSource({  // Ensure this is a Kendo UI DataSource
            type: "json",
            serverFiltering: true,
            transport: {
                read: {
                    url: "/api/DistributionPlaningApi/GetEmployees",
                    type: "GET",
                    dataType: "json"
                },
                parameterMap: function (data, type) {
                    var empGroupCodes = $scope.getMasterEmployeeCodes();
                    return {
                        filter: data.filter ? data.filter.filters[0].value : "",
                        empGroup: empGroupCodes
                    };
                }
            }
        })
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
                    return {
                        itmGroup: itmGroupCodes
                    };
                }
            }
        })
    };
    // Trigger a read operation on the DataSource, only if necessary
    $scope.individualOptions.dataSource.read();
    // Watch for changes in the selected group and refresh the individual data source
    $scope.$watch('selectedItmGroup', function (newGroups, oldGroups) {
        var individualMultiSelect = $("#individual").data("kendoMultiSelect");
        if (individualMultiSelect) {
            individualMultiSelect.dataSource.read();
        }
    });

    $scope.AddEditOption = function () {
        $scope.planName = "";
        $scope.employees = "";
        $("#routeList").data("kendoDropDownList").value([]);
        $("#employees").data("kendoDropDownList").value([]);
        $scope.routeList = [];
        $("#ddlDateFilterVoucher").val('This Month');
        $("#ddlDateFilterVoucher").trigger('change');


        //$("#calendar").fullCalendar('destroy');
        //$("#ToDateVoucher").trigger('change');
        clearCalendar();
        if ($scope.events.length > 0)
            $scope.events.splice(0, $scope.events.length);
        if ($scope.add_edit_option == 'Edit') {
            $scope.IsEdit = true;
            $scope.add_edit_option = 'New'

            return;
        }
        $scope.IsEdit = false;
        $scope.add_edit_option = 'Edit';
        $scope.initCalendar();
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
        } else if ($scope.customQty == null && $scope.targetType == 'SAL' && $scope.selectedSubTarget == 'ITM') {
            displayPopupNotification("Quantity is required!", "error");
            return;
        } else if ($scope.customAmt == null) {
            displayPopupNotification("Amount is required!", "error");
            return;
        }

        var selectedType = $scope.selectedSubTarget;
        var selectedEmployees = $scope.selectedEmployees || [];
        var selectedItems = $scope.selectedIndividual || [];
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
                var dayCounter = 1; // Counter to create unique fields for each date

                while (startDate <= endDate) {
                    var bsDate = AD2BS(moment(startDate).format("YYYY-MM-DD"));
                    var quantityField = `quantity_${dayCounter}`;
                    var amountField = `amount_${dayCounter}`;

                    var currentDay = startDate.getDay();
                    var currentDate = moment(startDate).format("YYYY-MM-DD");

                    // Skip Saturdays and holidays
                    if (currentDay !== 6 && !$scope.holidayList.includes(currentDate)) {
                        if (selectedType !== undefined && selectedType !== null && selectedType !== "" && selectedType.value == "ITM") {
                            columns.push({
                                title: bsDate,
                                headerAttributes: { style: "text-align: center;" },
                                columns: [
                                    {
                                        field: quantityField,
                                        title: "Quantity",
                                        template: `<input type="number" ng-model="dataItem.${quantityField}" ng-model-options="{debounce: 999999999}" class="form-control" ng-init="dataItem.${quantityField} = dataItem.${quantityField} || ${quantityPerDay}" value="${quantityPerDay}" />`,
                                        width: 80
                                    },
                                    {
                                        field: amountField,
                                        title: "Amount",
                                        template: `<input type="number" ng-model="dataItem.${amountField}" ng-model-options="{debounce: 999999999}" class="form-control" ng-init="dataItem.${amountField} = dataItem.${amountField} || ${amountPerDay}" value="${amountPerDay}" />`,
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
                                        template: `<input type="number" ng-change="updateTotals()" ng-model="dataItem.${amountField}" ng-model-options="{debounce: 999999999}" class="form-control" ng-init="dataItem.${amountField} = dataItem.${amountField} || ${amountPerDay}" value="${amountPerDay}"/>`,
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
                if (selectedType !== undefined && selectedType !== null && selectedType !== "" && selectedType.value == "ITM") {
                    return {
                        title: "Total",
                        columns: [
                            {
                                title: "Total Quantity",
                                field: "totalQuantity",
                                width: 100,
                                locked:true
                            },
                            {
                                title: "Total Amount",
                                field: "totalAmount",
                                width: 100,
                                locked: true
                            }
                        ]
                    };
                } else {
                    return {
                        title: "Total",
                        columns: [
                            {
                                title: "Total Amount",
                                field: "totalAmount",
                                width: 100,
                                locked: true
                            }
                        ]
                    };
                }
            }
            //Horizantaly calculation
            function calculateRowTotals(data) {
                data.forEach(function (item) {
                    var totalQuantity = 0;
                    var totalAmount = 0;
                    dynamicColumns.forEach(function (col) {
                        col.columns.forEach(function (subCol) {
                            if (subCol.field.startsWith('quantity_')) {
                                const templateString = subCol.template;
                                const $tempDiv = $('<div>').html(templateString);
                                var quantityValue = parseFloat($tempDiv.find('input').val()) || 0;
                                quantityValue = parseFloat(quantityValue) || 0;
                                totalQuantity += quantityValue
                            } else if (subCol.field.startsWith('amount_')) {
                                const templateString = subCol.template;
                                const $tempDiv = $('<div>').html(templateString);
                                var amountValue = parseFloat($tempDiv.find('input').val()) || 0;
                                amountValue = parseFloat(amountValue) || 0;
                                totalAmount += amountValue
                            }
                        });
                    });
                    item.totalQuantity = totalQuantity.toFixed(2);
                    item.totalAmount = totalAmount.toFixed(2);
                });
            }
            //vertically calculation 
            function addTotalRow(data, columns) {
                var totals = {
                    totalQuantity: 0,
                    totalAmount: 0};

                // Initialize totals object with zeros
                columns.forEach(function (col) {
                    col.columns.forEach(function (subCol) {
                        if (subCol.field.startsWith('quantity_') || subCol.field.startsWith('amount_')) {
                            totals[subCol.field] = 0;
                        }
                    });
                });

                // Accumulate totals for each column
                data.forEach(function (item) {
                    columns.forEach(function (col) {
                        col.columns.forEach(function (subCol) {
                            if (subCol.field.startsWith('quantity_')) {
                                const templateString = subCol.template;
                                const $tempDiv = $('<div>').html(templateString);
                                var quantityValue = parseFloat($tempDiv.find('input').val()) || 0;
                                quantityValue = parseFloat(quantityValue) || 0;
                                totals[subCol.field] += quantityValue;
                            } else if (subCol.field.startsWith('amount_')) {
                                 const templateString = subCol.template;
                                const $tempDiv = $('<div>').html(templateString);
                                var amountValue = parseFloat($tempDiv.find('input').val()) || 0;
                                amountValue = parseFloat(amountValue) || 0;
                                totals[subCol.field] += amountValue;
                            }
                        });
                    });
                });
                // Accumulate totals
                data.forEach(function (item) {
                    totals.totalQuantity += parseFloat(item.totalQuantity) || 0;
                    totals.totalAmount += parseFloat(item.totalAmount) || 0;
                });
                // Create total row with formatted totals for display
                var key;
                if (selectedType && selectedType.value == "ITM") {
                    key = 'ITEM_EDESC';
                } else {
                    key = 'EMPLOYEE_EDESC';
                }
                var totalRow = {
                    [key]: 'Total',
                    totalQuantity: totals.totalQuantity.toFixed(2),
                    totalAmount: totals.totalAmount.toFixed(2)
                };

                columns.forEach(function (col) {
                    col.columns.forEach(function (subCol) {
                        if (subCol.field.startsWith('quantity_') || subCol.field.startsWith('amount_')) {
                            totalRow[subCol.field] = parseFloat(parseFloat(totals[subCol.field]).toFixed(2));
                        }
                    });
                });
                data.push(totalRow);
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
                    calculateRowTotals(data);
                    addTotalRow(data, dynamicColumns); // Pass dynamicColumns here
                    $scope.gridOptions.dataSource.data = data;
                    $scope.gridOptions.columns = [
                        { field: "ITEM_EDESC", title: "Item Name", width: 150, locked: true },
                        ...dynamicColumns,
                        ...totalColumn.columns // Add total columns
                    ];
                    $("#grid").data("kendoGrid").setOptions($scope.gridOptions);
                    $("#grid").data("kendoGrid").refresh();
                });
            } else {
                $scope.employeeOptions.dataSource.read().then(function () {
                    var data = $scope.employeeOptions.dataSource.view();
                    if (selectedEmployees.length > 0) {
                        data = data.filter(function (item) {
                            var employeeCode = String(item.EMPLOYEE_CODE).trim();
                            return selectedEmployees.includes(employeeCode);
                        });
                    }
                    calculateRowTotals(data);
                    addTotalRow(data, dynamicColumns); // Pass dynamicColumns here
                    $scope.gridOptions.dataSource.data = data;
                    $scope.gridOptions.columns = [
                        { field: "EMPLOYEE_EDESC", title: "Employee Name", width: 150, locked: true },
                        ...dynamicColumns,
                        ...totalColumn.columns // Add total columns
                    ];
                    $("#grid").data("kendoGrid").setOptions($scope.gridOptions);
                    $("#grid").data("kendoGrid").refresh();
                });
            }
        });
    };
    $scope.updateTotals = function () {
        console.log("updateTotals called");
        $scope.searchClick();
    };
    $scope.gridOptions = {
        dataSource: {
            data: [], // Initially empty, will be filled after search
            schema: {
                model: {
                    fields: {
                        EMPLOYEE_EDESC: { type: "string" },
                        ITEM_EDESC: { type: "string" },
                        targetValue: { type: "number" },
                        total: { type: "number" } // Field for total column
                    }
                }
            },
            pageSize: 50
        },
        scrollable: true,
        height: 400,
        pageable: true,
        columns: [], // Columns will be set dynamically
        dataBound: function () {
            this.tbody.find('input').on('change', function () {
                $scope.updateTotals();
            });
        }
    };

    $scope.cancelClick = function () {
        // Reset the form fields
        $scope.targetName = '';
        $scope.customQty = '';
        $scope.customAmt = '';
        $scope.selectedTarget = null;
        $scope.selectedSubTarget = null;
        $scope.selectedItmGroup = [];
        $scope.selectedEmpGroup = [];
        $scope.selectedIndividual = [];
        $scope.selectedEmployees = [];
        $scope.customfrequencyday = null;
        $scope.frequencyWiseRouteAssign = false;

        // Reset Kendo UI widgets
        var kendoWidgets = [
            $("#subTargetType").data("kendoDropDownList"),
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
        var itmGroup = $scope.selectedItmGroup && $scope.selectedItmGroup.length > 0 ? $scope.selectedItmGroup.map(function (item) {
            return item.MASTER_ITEM_CODE;
        }) : [];

        var empGroup = $scope.selectedEmpGroup && $scope.selectedEmpGroup.length > 0 ? $scope.selectedEmpGroup.map(function (emp) {
            return emp.MASTER_EMPLOYEE_CODE; // Or the relevant property
        }) : [];

        var individual = $scope.selectedIndividual && $scope.selectedIndividual.length > 0 ? $scope.selectedIndividual.map(function (itm) {
            return {
                item_code: itm.ITEM_CODE,
                mu_code: itm.MU_CODE
            };
        }) : [];

        var employees = $scope.selectedEmployees && $scope.selectedEmployees.length > 0 ? $scope.selectedEmployees.map(function (emp) {
            return emp.EMPLOYEE_CODE; // Or the relevant property
        }) : [];

        // Prepare the data to assign, if any field is empty, it will be set to null
        var dataToAssign = {
            targetType: $scope.targetType || null,
            subTargetType: $scope.selectedSubTarget ? $scope.selectedSubTarget.value : null,
            targetName: $scope.targetName || null,
            itemGroup: itmGroup,
            employeeGroup: empGroup,
            items: individual,
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
            grid.columns.forEach(function (column, index) {
                if (index >= 1 && column.title) { // Skip the first column and ensure the column has a title
                    var count = index; // Using the current index for field naming
                    var quantityField = `quantity_${count}`; // Ensure this matches your data structure
                    var amountField = `amount_${count}`;

                    // Retrieve the quantity and amount, default to null if not found or invalid
                    var quantity = rowData[quantityField] !== undefined && rowData[quantityField] !== null && rowData[quantityField] !== '' ? rowData[quantityField] : null;
                    var amount = rowData[amountField] !== undefined && rowData[amountField] !== null && rowData[amountField] !== '' ? rowData[amountField] : null;

                    // Only push to gridData if itemCode exists (or EMPLOYEE_CODE), and handle missing MU_CODE
                    if (rowData.ITEM_CODE || rowData.EMPLOYEE_CODE) {
                        gridData.push({
                            itemCode: rowData.ITEM_CODE || rowData.EMPLOYEE_CODE,
                            muCode: rowData.MU_CODE || 0,
                            date: moment(BS2AD(column.title)).format("DD-MMM-YYYY"),
                            quantity: quantity,
                            amount: amount
                        });
                    }
                }
            });
        });
        return gridData;
    };

});
