
var QMModule = angular.module('QMModule', ['ngRoute', 'ngMessages', 'datatables', 'kendo.directives', 'cfp.hotkeys', 'ngHandsontable', 'angularjs-dropdown-multiselect']).run(['$rootScope', function ($rootScope) {

}]);
QMModule.config(function ($routeProvider, $locationProvider) {
    $routeProvider.when('/',
        {
            templateUrl: function () {
                return '/QuotationManagement/Home/Dashboard';
            },
        });

    $routeProvider.when('/Dashboard',
        {
            templateUrl: function () {
                return '/QuotationManagement/Home/Dashboard';
            },
        });
    $routeProvider.when('/QuotationSetup', {
        templateUrl: '/QuotationManagement/Home/QuotationSetup',
    });
    $routeProvider.when('/QuotationDetail', {
        templateUrl: '/QuotationManagement/Home/QuotationDetail',
    });
    $routeProvider.when('/AddQuotation', {
        templateUrl: '/QuotationManagement/Home/AddQuotation',
    });

    $routeProvider.when('/QuotationDetail/:id',
        {
            templateUrl: function (stateParams) {

                return '/QuotationManagement/Home/QuotationDetail?id=' + stateParams.id;
            },
            controller: 'quotationDetail',
            resolve: {
                module: function ($route) { return $route.current.params.id; }
            }

        });

    $routeProvider.when('/QuotationDetailItemwise/:quotationNo/:tenderNo', {

        templateUrl: function (stateParams) {

            return '/QuotationManagement/Home/QuotationDetailItemwise?id=' + stateParams.quotationNo + '&tenderNo=' + stateParams.tenderNo;
        },
        controller: 'quotationDetailItemWise',
        resolve:

        {
            quotationNo: function ($route) { return $route.current.params.quotationNo; },
            tenderNo: function ($route) { return $route.current.params.tenderNo; }

        }

    })
    $routeProvider.when('/ViewQuotation/:id',
        {
            templateUrl: function (stateParams) {

                return '/QuotationManagement/Home/ViewQuotation?id=' + stateParams.id;
            },
            controller: 'ViewQuotation',
            resolve: {
                module: function ($route) { return $route.current.params.id; }
            }

        });
    $routeProvider.when('/EditQuotation/:id',
        {
            templateUrl: function (stateParams) {

                return '/QuotationManagement/Home/EditQuotation?id=' + stateParams.id;
            },
            controller: 'EditQuotation',
            resolve: {
                module: function ($route) { return $route.current.params.id; }
            }

        });
    $routeProvider.when('/SummaryReport', {
        templateUrl: '/QuotationManagement/Home/SummaryReport',
    });
    $routeProvider.when('/TenderNoSetup', {
        templateUrl: '/QuotationManagement/Home/TenderSetup',
    });
    $routeProvider.otherwise({
        redirectTo: function () {
            return '/QuotationManagement/Home/Dashboard';
        }
    })
    $locationProvider.html5Mode({ enable: true }).hashPrefix('!QM');

});

QMModule.config.$inject = ['$routeProvider', '$locationProvider'];
