﻿planningModule.controller('BrandingCalendarRouteCtrl', function ($scope, $compile, $http, uiCalendarConfig) {

    var date = new Date();
    var d = date.getDate();
    var m = date.getMonth();
    var y = date.getFullYear();

    $scope.showroutelistdiv = false;

    $scope.startdate = "";
    $scope.enddate = "";

    $scope.notFromCalendar = false;

    $scope.eventSource = {};
    $scope.eventSources = [];
    $scope.events = [];
    $scope.routeList = [];

    $scope.IsEdit = false;
    $scope.add_edit_option = "Update";

    
    function clearCalendar() {
        var clientEvents = $('#calendar').fullCalendar('clientEvents');
        angular.forEach(clientEvents, function (value) {
            $('#calendar').fullCalendar('removeEvents', value.id);
        });
    };

    $scope.clearEventOfCalendar = function () {
        if ($scope.events.length > 0)
            $scope.events.splice(0, $scope.events.length);
    }

    function renderRouteList() {
        var t = $scope.employees,
            e = "/api/DistributionPlaningApi/GetAllBrandingRouteByFilters?filter=&empCode=" + t;
        $http.get(e).then(function (t) {
            $scope.routeList = t.data,
                setTimeout(function () {
                    $("#external-events .fc-event").each(function () {
                        $(this).data("event", {
                            code: $.trim($(this).val()),
                            title: $.trim($(this).text()),
                            stick: !0
                        }),
                            $(this).draggable({
                                zIndex: 999,
                                revert: !0,
                                revertDuration: 0
                            })
                    })
                }, 0)
        })
    }

    
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
        if ($scope.add_edit_option == 'Update') {
            $scope.IsEdit = true;
            $scope.add_edit_option = 'New'
            return;
        }
        $scope.IsEdit = false;
        $scope.add_edit_option = 'Update';
        $scope.initCalendar();
    }


    $scope.routesDataSource = {
        type: "jsonp",
        serverFiltering: false,
        transport: {
            read: {
                url: "/api/DistributionPlaningApi/GetRouteBrandingList?plancode=",
            }
        },
    };

    $scope.routeListOptions = {
        optionLabel: "--Select Route--",
        filter: "contains",
        dataTextField: "PLAN_EDESC",
        dataValueField: "PLAN_CODE",
        dataSource: $scope.routesDataSource,
        change: function () {
            $scope.notFromCalendar = false;
            var planCode = $("#routeList").data("kendoDropDownList").value();
            $scope.planName = $("#routeList").data("kendoDropDownList").text();
            $http.get("/api/DistributionPlaningApi/GetBrandingRouteByPlanCode?PLAN_ROUTE_CODE=" + planCode).then(function (response) {
                if ($scope.events.length > 0)
                    $scope.events.splice(0, $scope.events.length);
                clearCalendar();

                var isBreak = false;
                var uniqCount = _.uniq(_.pluck(response.data, "EMP_CODE"));
                if (uniqCount.length !== 1)
                    isBreak = true;
                if (isBreak) {
                    $scope.planName = "";
                    $scope.notFromCalendar = true;
                    displayPopupNotification("selected route plan is not set from calendar route setup.", "error");
                    return;
                }
                angular.forEach(response.data, function (value) {
                    var dateLen = value.ASSIGN_DATE.split('-').length;
                    var year = parseInt(value.ASSIGN_DATE.split('-')[dateLen - 1]);
                    var month = parseInt(value.ASSIGN_DATE.split('-')[dateLen - 2]);
                    var day = parseInt(value.ASSIGN_DATE.split('-')[dateLen - 3]);

                    $scope.employees = value.EMP_CODE;

                    //var req = "/api/DistributionPlaningApi/GetAllRouteByFilters?filter=&empCode=" + value.EMP_CODE;
                    //$http.get(req).then(function (response) {
                    //    $scope.routeList = response.data;
                    //});
                    $("#ToDateVoucher").val(moment(value.END_DATE).format('YYYY-MMM-DD'));
                    $("#FromDateVoucher").val(moment(value.START_DATE).format('YYYY-MMM-DD'));
                    $scope.startdate = moment(value.START_DATE).format('YYYY-MMM-DD');
                    $scope.enddate = moment(value.END_DATE).format('YYYY-MMM-DD')

                    $scope.events.push({
                        code: value.ROUTE_CODE,
                        title: value.ROUTE_NAME + '-(' + value.ROUTE_CODE + ')',
                        start: new Date(year, month - 1, day),
                        end: new Date(year, month - 1, day)
                    });

                });
                if ($scope.employees != undefined || $scope.employees != "")
                    $scope.showroutelistdiv = true;
                else
                    $scope.showroutelistdiv = false;
                renderRouteList();
                $scope.initCalendar();
            });

        },
        dataBound: function () {

        }
    };
    
    function validation() {
        var returnValue = true;
        var empCode = $("#employees").data('kendoDropDownList').value();
        if ($scope.planName == "" || $scope.planName == undefined) {
            displayPopupNotification("Invalid/Empty route plan name.", "error");
            return returnValue = false;
        }
        if ($scope.employees == "" || $scope.employees == undefined) {
            displayPopupNotification("Please select any employee", "error");
            return returnValue = false;
        }
        if (empCode == "" || empCode == undefined) {
            displayPopupNotification("Please select any employee", "error");
            return returnValue = false;
        }
        return returnValue;
    }

    $scope.cancelCalendarWiseRoute = function () {
        $scope.planName = "";
        $scope.employees = "";
        $scope.startdate = "";
        $scope.enddate = "";
        $scope.customfrequencyday = "";
        $("#routeList").data("kendoDropDownList").value([]);
        $("#employees").data("kendoDropDownList").value([]);
        $scope.routeList = [];
        $("#ddlDateFilterVoucher").val('This Month');
        $("#ddlDateFilterVoucher").trigger('change');
        clearCalendar();
        if ($scope.events.length > 0)
            $scope.events.splice(0, $scope.events.length);
    }

    $scope.saveCalendarWiseRoute = function () {
        var valid = validation();
        if (valid) {
            var dataList = $('#calendar').fullCalendar('clientEvents');
            var eventArr = [];
            angular.forEach(dataList, function (value) {
                var codeLen = value.title.split("-").length;
                var routeCode = value.title.split("-")[codeLen - 1];
                eventArr.push({
                    routeCode: routeCode.match(/\(([^)]+)\)/)[1],
                    title: value.title,
                    start: moment(value.start).format('MM-DD-YYYY'),
                    end: moment(value.end).format('MM-DD-YYYY') === "Invalid date" ? "" : moment(value.end).format('MM-DD-YYYY'),
                });
            });

            if (eventArr.length <= 0)
                return displayPopupNotification("There is no any assigned route.", "error");
            //var date = $('#calendar').fullCalendar('getDate')._d;
            //var firstDay = new Date(date.getFullYear(), date.getMonth(), 1);
            //var lastDay = new Date(date.getFullYear(), date.getMonth() + 1, 0);
            var lastDay = $("#ToDateVoucher").val();
            var firstDay = $("#FromDateVoucher").val();
            var employeeCode = $("#employees").data('kendoDropDownList').value();
            var planCode = $("#routeList").data("kendoDropDownList").value();
            var param = {
                planCode: planCode,
                addEdit: $scope.add_edit_option === "New" ? "Update" : "New",
                empCode: employeeCode == "" ? $scope.employees : employeeCode,
                planName: $scope.planName,
                startDate: moment(firstDay).format('MM-DD-YYYY'),
                endDate: moment(lastDay).format('MM-DD-YYYY'),
                eventArr: eventArr
            };

            var url = window.location.protocol + "//" + window.location.host + '/Api/DistributionPlaningApi/SaveCalendarBrandingRoutePlanData';

            var response = $http({
                method: "post",
                url: url,
                data: param,
                dataType: "json"
            });
            response.then(function (result) {
                if (result.data === "SUCCESS") {
                    displayPopupNotification("Calendar Route Save successfully.", "success");
                    $("#routeList").data("kendoDropDownList").dataSource.read();
                    $scope.planName = "";
                    $("#employees").data("kendoDropDownList").value([]);
                    $("#routeList").data("kendoDropDownList").value([]);
                    $scope.routeList = [];
                    //var clientEvents = $('#calendar').fullCalendar('clientEvents');
                    //angular.forEach(clientEvents, function (value) {
                    //    $('#calendar').fullCalendar('removeEvents', value.id);
                    //});
                    clearCalendar();
                    if ($scope.events.length > 0)
                        $scope.events.splice(0, $scope.events.length);
                    $scope.initCalendar();
                }
                else {
                    displayPopupNotification("Error in route save.", "error");
                }
            }, function (error) {
                displayPopupNotification("Error in route assignment." + error, "error");
            });
        }
    }

    $scope.employeeOptions = {
        optionLabel: "-- Select Employee --",
        dataTextField: "EMPLOYEE_EDESC",
        dataValueField: "EMPLOYEE_CODE",
        autoBind: false,
        dataSource: {
            type: "json",
            serverFiltering: true,
            transport: {
                read: {
                    url: "/api/DistributionPlaningApi/GetBrandingEmployees",
                },
                parameterMap: function (data, action) {

                    if (data.filter != undefined) {
                        if (data.filter.filters[0] != undefined) {
                            var newParams = {
                                filter: data.filter.filters[0].value,
                                empGroup: ""
                            };
                            return newParams;
                        }
                        else {
                            var newParams = {
                                filter: "",
                                empGroup: ""
                            };
                            return newParams;
                        }
                    }
                    else {
                        var newParams = {
                            filter: "",//planCode,
                            empGroup: ""//$("#groups").data("kendoDropDownList").dataItem().GROUPID
                        };
                        return newParams;
                    }
                }
            }
        },
        dataBound: function () {

        },
        change: function () {
            $scope.showroutelistdiv = true;
            clearCalendar();
            //var clientEvents = $('#calendar').fullCalendar('clientEvents');
            //angular.forEach(clientEvents, function (value) {
            //    $('#calendar').fullCalendar('removeEvents', value.id);
            //});
            renderRouteList();
            $scope.initCalendar();
        }

    };


    /* event source that calls a function on every view switch */
    $scope.eventsF = function (start, end, timezone, callback) {
        //var s = new Date(start).getTime() / 1000;
        //var e = new Date(end).getTime() / 1000;
        //var m = new Date(start).getMonth();
        //var events = [];
        //callback(events);
    };

    /* alert on eventClick */
    $scope.alertOnEventClick = function (calEvent, jsEvent, view) {
        // $('#calendar').fullCalendar('removeEvents', calEvent.id);
    };
    /* alert on Drop */
    $scope.alertOnDrop = function (event, delta, revertFunc, jsEvent, ui, view) {
        //var date = $('#calendar').fullCalendar('getDate')._d;
        //var firstDay = new Date(date.getFullYear(), date.getMonth(), 1);
        //var lastDay = new Date(date.getFullYear(), date.getMonth() + 1, 0);
        var lastDay = $("#ToDateVoucher").val();
        var firstDay = $("#FromDateVoucher").val();
        if (event.start < firstDay || event.start > lastDay) {
            revertFunc();
        }
        $scope.events;
    };
    /* alert on Resize */
    $scope.alertOnResize = function (event, delta, revertFunc, jsEvent, ui, view) {
        $scope.alertMessage = ('Event Resized to make dayDelta ' + delta);
    };
    /* event receive*/
    $scope.eventReceive = function (event) {

    }
    /* add and removes an event source of choice */
    $scope.addRemoveEventSource = function (sources, source) {
        var canAdd = 0;
        angular.forEach(sources, function (value, key) {
            if (sources[key] === source) {
                sources.splice(key, 1);
                canAdd = 1;
            }
        });
        if (canAdd === 0) {
            sources.push(source);
        }
    };
    /* add custom event*/
    $scope.addEvent = function (title, year, month, day) {
        $scope.events.push({
            title: title,
            start: new Date(year, month - 1, day),
            end: new Date(year, month - 1, day),
        });
    };
    /* remove event */
    $scope.remove = function (index) {
        $scope.events.splice(index, 1);
    };
    $scope.changeView = function (view, calendar) {
        uiCalendarConfig.calendars[calendar].fullCalendar('changeView', view);
    };
    $scope.renderCalender = function (calendar) {
        if (uiCalendarConfig.calendars[calendar]) {
            uiCalendarConfig.calendars[calendar].fullCalendar('render');
        }
    };
    $scope.eventRender = function (event, element, view) {
        element.attr({
            'tooltip': event.title,
            'tooltip-append-to-body': true
        });
        element.append("<span class='closeon'>(X)</span>");
        element.find(".closeon").click(function () {
            $('#calendar').fullCalendar('removeEvents', event._id);
            $scope.events.splice(_.findIndex($scope.events, { _id: event._id }), 1);
            $('#calendar').fullCalendar('refetchEvents')
            //var $this = $(this);
            //var title = $this.parent().find('span').html();
            //var codeLen = title.split("-").length;
            //var routeCode = title.split("-")[codeLen - 1].match(/\(([^)]+)\)/)[1];

            //if ($('input#frequencyWiseRouteAssign').is(':checked')) {
            //    var events = $('#calendar').fullCalendar('clientEvents');
            //    debugger;
            //    $scope.events;
            //    angular.forEach(events, function (value) {
            //        if (value.code == routeCode)
            //        {
            //            $('#calendar').fullCalendar('removeEvents', value._id);
            //            $scope.events = _.filter($scope.events, function (x) { return x._id != value._id });;
            //        }
            //    });
            //}
        });
        //if (event.start < new Date())
        //{
        //    $('#calendar').fullCalendar('removeEvents', event.id);
        //}
        $compile(element)($scope);
    };

    $scope.setRouteByFrequency = function (eventCode, eventName, event) {
        var el = event._d;
        var autoFillValue = $('input#frequencyWiseRouteAssign').is(':checked');
        if (autoFillValue) {
            autoFillValue = "yes";
        }
        else {
            autoFillValue = "no";
        }
        var incresedEl = null;
        var incresedElArray = [];
        $scope.isDefaultFreq = true;
        var frequency = "DAILY";
        var choosedDayfreq = parseInt($('#customday').val());
        $scope.customfrequencyday = choosedDayfreq;
        if ($scope.customfrequencyday == 1 || $scope.customfrequencyday == 7 || $scope.customfrequencyday == 30) {
            $scope.isDefaultFreq = true;
        }
        else {
            $scope.isDefaultFreq = false;
        }

        if (frequency == "DAILY" && autoFillValue === "yes") {
            var currDate = moment(el).toArray();
            // var endDate = moment(new Date(date.getFullYear(), date.getMonth() + 1, 0)).toArray();
            var endDate = moment($("#ToDateVoucher").val()).toArray();
            var a = moment([currDate[0], currDate[1], currDate[2]]);
            var b = moment([endDate[0], endDate[1], endDate[2]]);
            var daysDiff = b.diff(a, 'days');
            for (var i = 0 + $scope.customfrequencyday; i <= daysDiff; i = i + $scope.customfrequencyday) {
                incresedEl = moment(el).add({ days: i });
                $scope.events.push({
                    code: eventCode,
                    title: eventName,
                    start: incresedEl,
                    end: incresedEl
                });
            }
        }

    }


    $scope.initCalendar = function () {
        clearCalendar();
        $scope.uiConfig = {};
        //renderRouteList();
        /* config object */
        $scope.uiConfig = {
            calendar: {
                height: 450,
                editable: true,
                droppable: true,
                defaultDate: moment($("#FromDateVoucher").val()).format('YYYY-MM-DD'),
                duration: { months: 3 },
                header: {
                    left: 'prev,next today',
                    center: 'title',
                    right: 'month,agendaWeek'
                },
                displayEventTime: false,
                eventClick: $scope.alertOnEventClick,
                eventDrop: $scope.alertOnDrop,
                //eventResize: $scope.alertOnResize,
                eventRender: $scope.eventRender,
                //eventReceive: $scope.eventReceive,
                eventSources: function (start, end, timezone, callback) {
                    callback($scope.events);
                },
                eventConstraint: {
                    start: moment($("#FromDateVoucher").val()).format('YYYY-MM-DD') < moment().format('YYYY-MM-DD') ? moment().format('YYYY-MM-DD') : moment($("#FromDateVoucher").val()).format('YYYY-MM-DD'),//moment().format('YYYY-MM-DD'),
                    //end: '2100-01-01'
                    end: moment($("#ToDateVoucher").val()).add({ days: 1 }).format('YYYY-MM-DD'),//moment($("#ToDateVoucher").val()).format('YYYY-MM-DD'),
                    color: '#ff9f89'
                },
                drop: function (event, delta, revertFunc, jsEvent, ui, view) {
                    //var date = $('#calendar').fullCalendar('getDate')._d;
                    var eventId = $(this).val();
                    var eventName = $(this).html();
                    //var eventName = $(this).find('span').html();

                    //var title = $(this).find('span').html();
                    //var codeLen = title.split("-").length;
                    //var routeCode = title.split("-")[codeLen - 1].match(/\(([^)]+)\)/)[1];

                    //var events = $('#calendar').fullCalendar('clientEvents');
                    //angular.forEach(events, function (value) {
                    //    if (value.code == routeCode) {
                    //        $('#calendar').fullCalendar('removeEvents', value._id);
                    //        $scope.events = _.filter($scope.events, function (x) { return x._id != value._id })
                    //    }
                    //});

                    //var firstDay = new Date(date.getFullYear(), date.getMonth(), 1);
                    //var lastDay = new Date(date.getFullYear(), date.getMonth() + 1, 1);
                    //if (event._d < firstDay || event._d > lastDay) {
                    //    $('#calendar').fullCalendar('removeEvents', $(this).val());
                    //}
                    //$scope.events.push({});
                    //if (start.isBefore(moment(new Date()))) {
                    //    $('#calendar').fullCalendar('unselect');
                    //    return false;
                    //}
                    //if (event._d < new Date()) {
                    //    $('#calendar').fullCalendar('removeEvents', $(this).val());
                    //}
                    if ($scope.frequencyWiseRouteAssign)
                        $scope.setRouteByFrequency(eventId, eventName, event);

                },
            }
        };

        /* event sources array*/
        //$scope.eventSources = [];
        // $scope.eventSources = [$scope.events, $scope.eventSource, $scope.eventsF];
    }
    $scope.eventSources = [$scope.events, $scope.eventSource, $scope.eventsF];

    $scope.initCalendar();


});