using NeoErp.Planning.Service.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web;

namespace NeoErp.Planning.Service.Interface
{
    public interface IDistributionPlaning
    {
        string createPlanWiseRoute(RoutePlanModels model);
        string AddUpdateRoutes(RouteModels model);
        void deleteRoute(int code);
        List<DIST_ROUTE_PLAN> getAllPlanRoutes(string plancode);

        List<EMP_GROUP> getAllEmpGroups();
        List<Area_Master> getAllRoutes();
        List<RouteModels> GetRouteByRouteCode(string code);
        List<RoutePlanModel> GetRouteByPlanCode(string code);
        List<RouteModels> getAllRoutesByFilter(string filter);

        List<RouteModels> getAllRoutesByFilter(string filter,string empCode);
        bool checkifexists(RouteModels model);
        List<EmployeeModels> getEmployees(string filter, string empGroup);
        List<EmployeeModels> getSNGEmployees(string filter, string empGroup);

        List<CustomerSNGroup> GetGroupEmployees(string filter);

        List<FrequencyModels> getFrequencyByFilter(string filter);
        List<RoutePlanDateSeries> getDateSeries(string plancode);
        bool SaveEmployeeRoutePlan(List<DIST_ROUTE_DETAIL> routeDetailList);
        List<DIST_ROUTE_DETAIL> fetchAssignedEmployeesOfRoute(string plancode);
        string removeRouteFromPlan(string plancode, string routecode);
        string addRoutesToPlan(string plancode, string routecode);
        string saveCalendarRoute(DIST_CALENDAR_ROUTE model);
        string saveCalendarBrandingRoute(DIST_CALENDAR_ROUTE model);
        List<EmployeeModels> getBrandingEmployees(string filter, string empGroup);
        List<RouteModels> getAllBrandingRoutesByFilter(string filter, string empCode);
        List<DIST_ROUTE_PLAN> getAllBrandingPlanRoutes(string plancode);
        List<RoutePlanModel> GetBrandingRouteByPlanCode(string code);
        List<RouteModels> getAllRoutesByFilterRouteType(string filter, string empCode, string RouteType = "D");

        string UpdateRouteExpireEndDate(UpdateExpEndDateModal updateModal);
        string saveExcelPlan(HttpPostedFile File);
        List<ItemGroupModel> GetItemGroup(string filter);
        List<ItemGroupModel> GetItemLists(string filter, string itmGroup);
        List<HolidayModel> GetHolidayDetails(string fromDate,string toDate);
        string saveTargetData(ProfileModel model);
        string updateTargetData(ProfileModel model);
        List<TARGET_PLAN> getAllTargets();
        string UpdateTarget(string targetId);
        TARGET_DETAILS GetTargetData(string targetId);
        List<CustomerGroup> GetCustomerGroup(string filter);
        List<CustomerSNGroup> GetCustomerSNGGroup(string filter);
        List<CustomerGroupModel> GetCustomerLists(string filter,string cusGroup);
        List<CustomerGroupModel> GetCustomerSNGLists(string filter, string cusGroup);
    }
}
