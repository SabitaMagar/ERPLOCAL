/// <reference path="~/Scripts/_references.js" />
var planningModule = angular.module('planningModule', ['ngRoute', 'ngMessages', 'datatables', 'kendo.directives', 'ui.calendar']);
//planningModule.run(function ($rootScope) {
//    $rootScope.reservedBudgetData = 'ap';
//})
planningModule.config(function ($routeProvider, $locationProvider, $httpProvider) {

    $httpProvider.interceptors.push(function ($rootScope, $q) {
        return {
            request: function (config) {
                config.timeout = 1000000;
                return config;
            },
            responseError: function (rejection) {
                switch (rejection.status) {
                    case 408:
                        console.log('connection timed out');
                        break;
                }
                return $q.reject(rejection);
            }
        }
    })
    // redirect to Saleshome dashboard (for now) or Login page.
    $routeProvider.when('/',
        {
            templateUrl: '/Planning/Home/Dashboard',
            //controller: 'planningCtrl'
        });
    $routeProvider.when('/Organiser',
        {
            templateUrl: '/Planning/Home/PlanningDashboard',
            //controller: 'planningCtrl'
        });
    $routeProvider.when('/Dashboard',
        {
            templateUrl: '/Planning/Home/Dashboard',
            //controller: 'planningCtrl'
        });
    //$routeProvider.when('/Planning/Home/PlanningDashboard',
    //{
    //    templateUrl: '/Planning/Home/PlanningDashboard',
    //    //controller: 'planningCtrl'
    //});


    $routeProvider.when('/PlanList',
        {
            templateUrl: '/Planning/Home/PlanList',
            controller: 'planningCtrl'
        });


    $routeProvider.when('/CreatePlan',
        {
            templateUrl: '/Planning/Plan/Index',
            controller: 'planningCtrl'
        });
    $routeProvider.when('/Setup',
        {
            templateUrl: '/Planning/Setup/PreferenceSetup',
            controller: 'setupCtrl'
        });
    $routeProvider.when('/TempTableSetup',
        {
            templateUrl: '/Planning/Setup/TempTableSetup',
            //controller: 'tempTablesetupCtrl'
        });
    $routeProvider.when('/CreatePlan/:plancode',
        {
            templateUrl: function (stateParams) {
                return '/Planning/Plan/Index?plancode=' + stateParams.plancode;
            },
            controller: 'planningCtrl'
        });
    $routeProvider.when('/EditPlan/:plancode',
        {
            templateUrl: function (stateParams) {
                return '/Planning/Plan/EditPlan?plancode=' + stateParams.plancode;
            },
            controller: 'editPlanCtrl'
        });

    $routeProvider.when('/Startdate/:startdate/Enddate/:enddate',
        {
            templateUrl: function (stateParams) {
                return '/Planning/Home/PlanList?startdate=' + stateParams.startdate + '&enddate=' + stateParams.enddate;
            },
        });
    $routeProvider.when('/SubplanList',
        {
            templateUrl: '/Planning/Home/SubPlanList',
            controller: 'subplanlistCtrl'
        });
    $routeProvider.when('/MasterPlanSetup/:plancode',
        {
            templateUrl: function (stateParams) {
                return '/Planning/Home/MasterPlanSetup?plancode=' + stateParams.plancode;
            },
        });
    $routeProvider.when('/MasterPlanSetup',
        {
            templateUrl: '/Planning/Home/MasterPlanSetup',
        });
    $routeProvider.when('/SalesPlanWiseReport/:plancode',
        {
            templateUrl: function (stateParams) {
                return '/Planning/Home/SalesPlanWiseReport?plancode=' + stateParams.plancode;
            },
        });
    $routeProvider.when('/SalesPlanWiseReport',
        {
            templateUrl: '/Planning/Home/SalesPlanWiseReport',
        });
    $routeProvider.when('/PlanSetup',
        {
            templateUrl: '/Planning/PlanSetup/PlanSetup',
            controller: 'planSetupCtrl'
        });
    $routeProvider.when('/PlanSetup/:plancode',
        {
            templateUrl: function (stateParams) {
                return '/Planning/PlanSetup/PlanSetup?plancode=' + stateParams.plancode;
            },
            controller: 'planSetupCtrl'
        });
    $routeProvider.when('/MasterPlan', {
        templateUrl: '/Planning/Home/MasterPlan',
        controller: 'masterPlanCtrl'
    })
    $routeProvider.when('/MasterPlan/:masterPlanCode', {
        templateUrl: function (stateParams) {
            return '/Planning/Home/MasterPlan?masterplancode=' + stateParams.masterPlanCode;
        },
        controller: 'masterPlanCtrl'
    })
    $routeProvider.when('/MasterPlanView/:masterPlanCode', {
        templateUrl: function (stateParams) {
            return '/Planning/Home/MasterPlanView?masterplancode=' + stateParams.masterPlanCode;
        },
        //controller: 'masterPlanCtrl'
    })
    $routeProvider.when('/SubPlan',
        {
            templateUrl: '/Planning/Plan/SubPlan',
            controller: 'subPlanCtrl'
        });
    $routeProvider.when('/SubPlan/:planCode',
        {
            templateUrl: function (stateParams) { return '/Planning/Plan/SubPlan?planCode=' + stateParams.planCode; },
            controller: 'subPlanCtrl'
        });
    $routeProvider.when('/FrequencySetup',
        {
            templateUrl: '/Planning/Plan/FrequencySetup',
            controller: 'frequencySetupCtrl'
        });
    $routeProvider.when('/EmployeeTree',
        {
            templateUrl: '/Planning/Plan/EmployeeTree',
        });

    $routeProvider.when('/RouteSetup',
        {
            templateUrl: '/Planning/DistributionPlaning/RouteSetup',
            controller: 'routeSetupCtrl'
        });

    $routeProvider.when('/CreateRoute',
        {
            templateUrl: '/Planning/DistributionPlaning/CreateRoute',
            controller: 'routeSetupCtrl'
        });
    $routeProvider.when('/ExcelUpload',
        {
            
            templateUrl: '/Planning/ExcelUpload/Index',
            controller: 'ExcelUpload'
        });
    $routeProvider.when('/EmployeeRouteSetup',
        {
            
            templateUrl: '/Planning/DistributionPlaning/EmployeeRouteSetup',
            controller: 'EmployeeRoutePlan'
        });
    $routeProvider.when('/EmployeeRouteSetup/:routeCode',
        {
            templateUrl: function (stateParams) { return '/Planning/DistributionPlaning/EmployeeRouteSetup?routecode=' + stateParams.routeCode; },
            controller: 'EmployeeRoutePlan'
        });
    $routeProvider.when('/CalendarRouteSetup',
        {
            templateUrl: '/Planning/DistributionPlaning/CalendarRouteSetup',
            controller: 'calendarRouteCtrl'
        });
    $routeProvider.when('/CalendarBrandingRouteSetup',
        {
            templateUrl: '/Planning/DistributionPlaning/BrandingCalendarRouteSetup',
            controller: 'BrandingCalendarRouteCtrl'
        });
    $routeProvider.when('/RouteList',
        {
            templateUrl: '/Planning/DistributionPlaning/RouteList',
        });
    $routeProvider.when('/TargetSetup',
        {
            templateUrl: '/Planning/DistributionPlaning/TargetSetup',
        });
    $routeProvider.when('/CreateTargetSetup',
        {
            templateUrl: '/Planning/DistributionPlaning/CreateTargetSetup',
            controller: 'TargetSetup'
        });
    $routeProvider.when('/ViewTarget/:targetId',
         {
             templateUrl: function (stateParams) {
                 return '/Planning/DistributionPlaning/ViewTarget?targetId=' + stateParams.targetId;
              },
        });
    $routeProvider.when('/EditTarget/:targetId',
        {
            templateUrl: function (stateParams) {
                return '/Planning/DistributionPlaning/EditTarget?targetId=' + stateParams.targetId;
            },
        });
    // plan report
    $routeProvider.when('/MonthlyWiseSalesPlanReport',
        {
            templateUrl: '/Planning/Home/MonthlyWiseSalesPlanReport',
        });

    $routeProvider.when('/FavSalesPlanReport',
        {
            templateUrl: '/Planning/Home/FavSalesPlanReport',
        });
    $routeProvider.when('/FavBrandingPlanReport',
        {
            templateUrl: '/Planning/BrandingPlan/FavBrandingPlanReport',
        });
    $routeProvider.when('/FavProcurementPlanReport',
        {
            templateUrl: '/Planning/ProcurementPlan/FavProcurementPlanReport',
        });
    $routeProvider.when('/FavProductionPlanReport',
        {
            templateUrl: '/Planning/ProductionPlan/FavProductionPlanReport',
        });
    $routeProvider.when('/FavBudgetPlanReport',
        {
            templateUrl: '/Planning/LedgerBudgetPlan/FavBudgetPlanReport',
        });
    $routeProvider.when('/FavLedgerPlanReport',
        {
            templateUrl: '/Planning/BudgetPlan/FavLedgerPlanReport',
        });
    $routeProvider.when('/FavMaterialPlanReport',
        {
            templateUrl: '/Planning/MaterialPlan/FavMaterialPlanReport',
        });



    // budget plan
    $routeProvider.when('/BudgetPlan',
        {
            templateUrl: '/Planning/BudgetPlan/Index',
        });
    $routeProvider.when('/CreateBudgetPlan',
        {
            templateUrl: '/Planning/BudgetPlan/CreateBudgetPlan',
            controller: 'planningBudgetCtrl'
        });
    $routeProvider.when('/CreateBudgetPlan/:plancode',
        {
            templateUrl: function (stateParams) {
                return '/Planning/BudgetPlan/CreateBudgetPlan?plancode=' + stateParams.plancode;
            },
            controller: 'planningBudgetCtrl'
        });
    $routeProvider.when('/BudgetPlanSetup',
        {
            templateUrl: '/Planning/BudgetPlan/BudgetPlanSetup',
            controller: 'budgetPlanSetupCtrl'
        });
    $routeProvider.when('/BudgetPlanSetup/:plancode',
        {
            templateUrl: function (stateParams) {
                return '/Planning/BudgetPlan/BudgetPlanSetup?planCode=' + stateParams.plancode;
            },
            controller: 'budgetPlanSetupCtrl'
        });

    //Ledger Budget Plan
    $routeProvider.when('/LedgerBudgetPlan',
        {
            templateUrl: '/Planning/LedgerBudgetPlan/Index',
        });
    $routeProvider.when('/CreateLedgerBudgetPlan',
        {
            templateUrl: '/Planning/LedgerBudgetPlan/CreateLedgerBudgetPlan',
            controller: 'LedgerBudgetCtrl'
        });
    $routeProvider.when('/CreateLedgerBudgetPlan/:plancode',
        {
            templateUrl: function (stateParams) {
                return '/Planning/LedgerBudgetPlan/CreateLedgerBudgetPlan?plancode=' + stateParams.plancode;
            },
            controller: 'LedgerBudgetCtrl'
        });
    $routeProvider.when('/LedgerBudgetPlanSetup',
        {
            templateUrl: '/Planning/LedgerBudgetPlan/LedgerBudgetPlanSetup',
            controller: 'LedgerBudgetPlanSetupCtrl'
        });
    $routeProvider.when('/LedgerBudgetPlanSetup/:plancode',
        {
            templateUrl: function (stateParams) {
                return '/Planning/LedgerBudgetPlan/LedgerBudgetPlanSetup?planCode=' + stateParams.plancode;
            },
            controller: 'LedgerBudgetPlanSetupCtrl'
        });

    //Production Plan

    $routeProvider.when('/ProductionPlan',
        {
            templateUrl: '/Planning/ProductionPlan/Index',
        });
    $routeProvider.when('/ProductionPlanNew',
        {
            templateUrl: '/Planning/ProductionPlan/IndexNew',
        });
    $routeProvider.when('/ProductionPlan/Startdate/:startdate/Enddate/:enddate',
        {
            templateUrl: function (stateParams) {
                return '/Planning/ProductionPlan/IndexNew?startdate=' + stateParams.startdate + '&enddate=' + stateParams.enddate;
            },
        });
    $routeProvider.when('/ProductionPlan/Startdate/:startdate/Enddate/:enddate',
        {
            templateUrl: function (stateParams) {
                return '/Planning/ProductionPlan/Index?startdate=' + stateParams.startdate + '&enddate=' + stateParams.enddate;
            },
        });

    $routeProvider.when('/CreateProductionPlan',
        {
            templateUrl: '/Planning/ProductionPlan/CreateProductionPlan',
            controller: 'ProductionPlanCtrl'
        });

    $routeProvider.when('/CreateProductionPlan/:plancode',
        {
            templateUrl: function (stateParams) {
                return '/Planning/ProductionPlan/CreateProductionPlan?plancode=' + stateParams.plancode;
            },
            controller: 'ProductionPlanCtrl'
        });
    $routeProvider.when('/ProductionPlanSetup',
        {
            templateUrl: '/Planning/ProductionPlan/ProductionPlanSetup',
            controller: 'ProductionPlanSetupCtrl'
        });

    $routeProvider.when('/ProductionPlanSetup/:plancode',
        {
            templateUrl: function (stateParams) {
                return '/Planning/ProductionPlan/ProductionPlanSetup?plancode=' + stateParams.plancode;
            },
            controller: 'ProductionPlanSetupCtrl'
        });
    $routeProvider.when('/ProductionPlanSetup/:plancode',
        {
            templateUrl: function (stateParams) {
                return '/Planning/ProductionPlan/ProductionPlanSetup?planCode=' + stateParams.plancode;
            },
            controller: 'ProductionPlanSetupCtrl'
        });

    //Procurement Plan

    //ProcurementPlan index route
    $routeProvider.when('/ProcurementPlan',
        {
            templateUrl: '/Planning/ProcurementPlan/Index',
        });
   
    $routeProvider.when('/ProcureFromMaterial',
        {
            templateUrl: '/Planning/ProcurementPlan/ProcureFromMaterial',
            controller: 'ProcureFromMaterialCtrl'
        });

   
    $routeProvider.when('/ProcurementPlan/Startdate/:startdate/Enddate/:enddate',
        {
            templateUrl: function (stateParams) {
                return '/Planning/ProcurementPlan/Index?startdate=' + stateParams.startdate + '&enddate=' + stateParams.enddate;
            },
        });
   

      



    //Procurement Plan Create
    $routeProvider.when('/CreateProcurementPlan',
        {
            templateUrl: '/Planning/ProcurementPlan/CreateProcurementPlan',
            controller: 'ProcurementPlanCtrl'
        });
   
    $routeProvider.when('/CreateProcurementPlan/:plancode',
        {
            templateUrl: function (stateParams) {
                return '/Planning/ProcurementPlan/CreateProcurementPlan?plancode=' + stateParams.plancode;
            },
            controller: 'ProcurementPlanCtrl'
        });

    //Procurement Plan Setup
    $routeProvider.when('/ProcurementPlanSetup',
        {
            templateUrl: '/Planning/ProcurementPlan/ProcurementPlanSetup',
            controller: 'ProcurementPlanSetupCtrl'
        });

    $routeProvider.when('/ProcurementPlanSetup/:plancode',
        {
            templateUrl: function (stateParams) {
                return '/Planning/ProcurementPlan/ProcurementPlanSetup?plancode=' + stateParams.plancode;
            },
            controller: 'ProcurementPlanSetupCtrl'
        });
    $routeProvider.when('/ProcurementPlanSetup/:plancode',
        {
            templateUrl: function (stateParams) {
                return '/Planning/ProcurementPlan/ProcurementPlanSetup?planCode=' + stateParams.plancode;
            },
            controller: 'ProcurementPlanSetupCtrl'
        });

    //Collection Plan

    //CollectionPlan index route
    $routeProvider.when('/CollectionPlan',
        {
            templateUrl: '/Planning/CollectionPlan/Index',
        });
    $routeProvider.when('/CollectionFromMaterial',
        {
            templateUrl: '/Planning/CollectionPlan/CollectionFromMaterial',
            controller: 'CollectionFromMaterialCtrl'
        });
    $routeProvider.when('/CollectionPlan/Startdate/:startdate/Enddate/:enddate',
        {
            templateUrl: function (stateParams) {
                return '/Planning/CollectionPlan/Index?startdate=' + stateParams.startdate + '&enddate=' + stateParams.enddate;
            },
        });
     //Collection Plan Create
    $routeProvider.when('/CreateCollectionPlan',
        {
            templateUrl: '/Planning/CollectionPlan/CreateCollectionPlan',
            controller: 'CollectionPlanCtrl'
        });
    $routeProvider.when('/CreateCollectionPlan/:plancode',
        {
            templateUrl: function (stateParams) {
                return '/Planning/CollectionPlan/CreateCollectionPlan?plancode=' + stateParams.plancode;
            },
            controller: 'CollectionPlanCtrl'
        });

    //CollectionPlan Plan Setup
    $routeProvider.when('/CollectionPlanSetup',
        {
            templateUrl: '/Planning/CollectionPlan/CollectionPlanSetup',
            controller: 'CollectionPlanSetupCtrl'
        });

    $routeProvider.when('/CollectionPlanSetup/:plancode',
        {
            templateUrl: function (stateParams) {
                return '/Planning/CollectionPlan/CollectionPlanSetup?plancode=' + stateParams.plancode;
            },
            controller: 'CollectionPlanSetupCtrl'
        });
    $routeProvider.when('/CollectionPlanPlanSetup/:plancode',
        {
            templateUrl: function (stateParams) {
                return '/Planning/CollectionPlan/CollectionPlanSetup?planCode=' + stateParams.plancode;
            },
            controller: 'CollectionPlanSetupCtrl'
        });



    

    //Branding Plan

    //BrandingPlan index route
    $routeProvider.when('/BrandingPlan',
        {
            templateUrl: '/Planning/BrandingPlan/Index',
        });

    $routeProvider.when('/BrandingPlan/Startdate/:startdate/Enddate/:enddate',
        {
            templateUrl: function (stateParams) {
                return '/Planning/BrandingPlan/Index?startdate=' + stateParams.startdate + '&enddate=' + stateParams.enddate;
            },
        });

    //Branding Plan Create
    $routeProvider.when('/CreateBrandingPlan',
        {
            templateUrl: '/Planning/BrandingPlan/CreateBrandingPlan',
            controller: 'BrandingPlanCtrl'
        });

    $routeProvider.when('/CreateBrandingPlan/:plancode',
        {
            templateUrl: function (stateParams) {
                return '/Planning/BrandingPlan/CreateBrandingPlan?plancode=' + stateParams.plancode;
            },
            controller: 'BrandingPlanCtrl'
        });
    //Branding Plan Setup
    $routeProvider.when('/BrandingPlanSetup',
        {
            templateUrl: '/Planning/BrandingPlan/BrandingPlanSetup',
            controller: 'BrandingPlanSetupCtrl'
        });

    $routeProvider.when('/BrandingPlanSetup/:plancode',
        {
            templateUrl: function (stateParams) {
                return '/Planning/BrandingPlan/BrandingPlanSetup?plancode=' + stateParams.plancode;
            },
            controller: 'BrandingPlanSetupCtrl'
        });
    $routeProvider.when('/BrandingPlanSetup/:plancode',
        {
            templateUrl: function (stateParams) {
                return '/Planning/BrandingPlan/BrandingPlanSetup?planCode=' + stateParams.plancode;
            },
            controller: 'BrandingPlanSetupCtrl'
        });
    // end of budget plan

    //MaterialPlan index route
    $routeProvider.when('/MaterialPlan',
        {
            templateUrl: '/Planning/MaterialPlan/Index',
        });

    $routeProvider.when('/MaterialPlan/Startdate/:startdate/Enddate/:enddate',
        {
            templateUrl: function (stateParams) {
                return '/Planning/MaterialPlan/Index?startdate=' + stateParams.startdate + '&enddate=' + stateParams.enddate;
            },
        });
    //MaterialPlan index route
    $routeProvider.when('/MaterialPlanPI',
        {
            templateUrl: '/Planning/MaterialPlan/IndexPI',
        });

    $routeProvider.when('/MaterialPlanPI/Startdate/:startdate/Enddate/:enddate',
        {
            templateUrl: function (stateParams) {
                return '/Planning/MaterialPlan/IndexPI?startdate=' + stateParams.startdate + '&enddate=' + stateParams.enddate;
            },
        });
    $routeProvider.when('/MaterialFromProductionPI',
        {
            templateUrl: '/Planning/MaterialPlan/MaterialFromProductionPI',
            controller: 'MaterialFromProductionPICtrl'
        });

    $routeProvider.when('/MaterialFromProduction',
        {
            templateUrl: '/Planning/MaterialPlan/MaterialFromProduction',
            controller: 'MaterialFromProductionCtrl'
        });

    $routeProvider.when('/MaterialFromProduction/:plancode',
        {
            templateUrl: function (stateParams) {
                return '/Planning/MaterialPlan/MaterialFromProduction?plancode=' + stateParams.plancode;
            },
            controller: 'MaterialFromProductionCtrl'
        });
    //Material Plan Create
    $routeProvider.when('/CreateMaterialPlan',
        {
            templateUrl: '/Planning/MaterialPlan/CreateMaterialPlan',
            controller: 'MaterialPlanCtrl'
        });
    $routeProvider.when('/CreateMaterialPlan/:plancode',
        {
            templateUrl: function (stateParams) {
                return '/Planning/MaterialPlan/CreateMaterialPlan?plancode=' + stateParams.plancode;
            },
            controller: 'MaterialPlanCtrl'
        });
    //Reference Material Plan Create
    $routeProvider.when('/CreateMaterialPlanReference',
        {
            templateUrl: '/Planning/MaterialPlan/CreateMaterialPlanReference',
            controller: 'MaterialPlanReferenceCtrl'
        });
    $routeProvider.when('/CreateMaterialPlanReference/:plancode',
        {
            templateUrl: function (stateParams) {
                return '/Planning/MaterialPlan/CreateMaterialPlanReference?plancode=' + stateParams.plancode;
            },
            controller: 'MaterialPlanReferenceCtrl'
        });

    //Material Plan Setup
    $routeProvider.when('/MaterialPlanSetup',
        {
            templateUrl: '/Planning/MaterialPlan/MaterialPlanSetup',
            controller: 'MaterialPlanSetupCtrl'
        });

    $routeProvider.when('/MaterialPlanSetup/:plancode',
        {
            templateUrl: function (stateParams) {
                return '/Planning/MaterialPlan/MaterialPlanSetup?plancode=' + stateParams.plancode;
            },
            controller: 'MaterialPlanSetupCtrl'
        });
    $routeProvider.when('/MaterialPlanSetup/:plancode',
        {
            templateUrl: function (stateParams) {
                return '/Planning/MaterialPlan/MaterialPlanSetup?planCode=' + stateParams.plancode;
            },
            controller: 'MaterialPlanSetupCtrl'
        });

    $routeProvider.otherwise({ redirectTo: '/Planning/Home/DashBoard' });
    $locationProvider.html5Mode({ enable: true }).hashPrefix('!Planning');
});

planningModule.config.$inject = ['$routeProvider', '$locationProvider'];