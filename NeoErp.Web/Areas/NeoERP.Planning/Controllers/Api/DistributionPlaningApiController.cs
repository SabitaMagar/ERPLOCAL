using System;
using NeoErp.Planning.Service.Interface;
using NeoErp.Planning.Service.Models;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using System.Web;
using ExcelDataReader;
using System.Data;
using System.IO;
using System.Web.Http.Results;



namespace NeoERP.Planning.Controllers.Api
{
    public class DistributionPlaningApiController : ApiController
    {
        private IDistributionPlaning _iDistributionPlaning { get; set; }
        public DistributionPlaningApiController(IDistributionPlaning _iDistributionPlaning)
        {
            this._iDistributionPlaning = _iDistributionPlaning;
        }

        [HttpGet]
        public List<FrequencyModels> GetAllFrequencyByFilters(string filter)
        {
            return _iDistributionPlaning.getFrequencyByFilter(filter);
        }
        [HttpPost]
        public HttpResponseMessage CreateRoute(RouteModels route)
        {
            if (ModelState.IsValid)
            {
                if (_iDistributionPlaning.checkifexists(route))
                {
                    return Request.CreateResponse(HttpStatusCode.OK, new { MESSAGE = "Alreadyexists", STATUS_CODE = (int)HttpStatusCode.OK });
                }
                else
                {
                    var message = _iDistributionPlaning.AddUpdateRoutes(route);
                    if (message == "ExistsButDeleted")
                    {
                        return Request.CreateResponse(HttpStatusCode.OK, new { MESSAGE = "ExistsButDeleted", STATUS_CODE = (int)HttpStatusCode.OK });
                    }
                    else
                    {
                        return Request.CreateResponse(HttpStatusCode.OK, new { MESSAGE = "Success", STATUS_CODE = (int)HttpStatusCode.OK });
                    }
                }
            }
            else
            {
                return Request.CreateResponse(HttpStatusCode.InternalServerError, new { MESSAGE = "Error", STATUS_CODE = (int)HttpStatusCode.InternalServerError });
            }
        }

        [HttpPost]
        public HttpResponseMessage CreatePlanWiseRoute(RoutePlanModels model)
        {
            try
            {
                if (ModelState.IsValid)
                {

                    var message = _iDistributionPlaning.createPlanWiseRoute(model);
                    if (message == "validation")
                    {
                        return Request.CreateResponse(HttpStatusCode.OK, new { MESSAGE = "Validation", STATUS_CODE = (int)HttpStatusCode.OK });
                    }
                    else
                    {
                        return Request.CreateResponse(HttpStatusCode.OK, new { MESSAGE = "Success", PLAN_CODE = message, STATUS_CODE = (int)HttpStatusCode.OK });
                    }
                }
                else
                {
                    return Request.CreateResponse(HttpStatusCode.InternalServerError, new { MESSAGE = "fieldValidation", STATUS_CODE = (int)HttpStatusCode.InternalServerError });
                }
            }
#pragma warning disable CS0168 // The variable 'ex' is declared but never used
            catch (Exception ex)
#pragma warning restore CS0168 // The variable 'ex' is declared but never used
            {
                return Request.CreateResponse(HttpStatusCode.InternalServerError, new { MESSAGE = "DbError", STATUS_CODE = (int)HttpStatusCode.InternalServerError });
            }
        }
        public List<Area_Master> GetAllRoutes()
        {
            return _iDistributionPlaning.getAllRoutes();
        }
        [HttpGet]
        public List<DIST_ROUTE_PLAN> GetRouteBrandingList(string plancode)
        {
            return _iDistributionPlaning.getAllBrandingPlanRoutes(plancode);

        }
        public List<RouteModels> GetAllRouteByFilters(string filter)
        {
            var result = _iDistributionPlaning.getAllRoutesByFilter(filter);
            return result;
        }

        public List<RouteModels> GetAllRouteByFilters(string filter, string empCode)
        {
            var result = _iDistributionPlaning.getAllRoutesByFilterRouteType(filter, empCode,"D");
            return result;
        }
        public List<RouteModels> GetAllBrandingRouteByFilters(string filter, string empCode)
        {
            var result = _iDistributionPlaning.getAllBrandingRoutesByFilter(filter, empCode);
            return result;
        }
        [HttpGet]
        public HttpResponseMessage DeleteRoute(int ROUTE_CODE)
        {
            _iDistributionPlaning.deleteRoute(ROUTE_CODE);
            return Request.CreateResponse(HttpStatusCode.OK, new { MESSAGE = "Success", STATUS_CODE = (int)HttpStatusCode.OK });
        }

        [HttpGet]
        public List<DIST_ROUTE_PLAN> GetRouteList(string plancode)
        {
            return _iDistributionPlaning.getAllPlanRoutes(plancode);

        }
        [HttpGet]
        public List<EMP_GROUP> GetEmpGroupList()
        {
            return _iDistributionPlaning.getAllEmpGroups();

        }
        [HttpGet]
        public List<RouteModels> GetRouteByRouteCode(string PLAN_ROUTE_CODE)
        {
            return _iDistributionPlaning.GetRouteByRouteCode(PLAN_ROUTE_CODE);

        }
        [HttpGet]
        public List<RoutePlanModel> GetRouteByPlanCode(string PLAN_ROUTE_CODE)
        {
            return _iDistributionPlaning.GetRouteByPlanCode(PLAN_ROUTE_CODE);

        }

        [HttpPost]
        public string UpdateExpEndDate(UpdateExpEndDateModal modal)
        {
            return _iDistributionPlaning.UpdateRouteExpireEndDate(modal);
        }
        [HttpGet]
        public List<RoutePlanModel> GetBrandingRouteByPlanCode(string PLAN_ROUTE_CODE)
        {
            return _iDistributionPlaning.GetBrandingRouteByPlanCode(PLAN_ROUTE_CODE);

        }
        [HttpGet]
        public List<EmployeeModels> GetEmployees(string filter, string empGroup)
        {
            var employees = this._iDistributionPlaning.getEmployees(filter, empGroup);
            return employees;
        }
        [HttpGet]
        public List<EmployeeModels> GetSNGEmployees(string filter, string empGroup)
        {
            var employees = this._iDistributionPlaning.getSNGEmployees(filter, empGroup);
            return employees;
        }
        [HttpGet]
        public List<CustomerSNGroup> GetGroupEmployees(string filter)
        {
            var employees = this._iDistributionPlaning.GetGroupEmployees(filter);
            return employees;
        }

        [HttpGet]
        public List<EmployeeModels> GetBrandingEmployees(string filter, string empGroup)
        {
            var employees = this._iDistributionPlaning.getBrandingEmployees(filter, empGroup);
            return employees;
        }
        [HttpGet]
        public List<DIST_ROUTE_DETAIL> GetAssignedEmployeesOfRoute(string plancode)
        {
            var assignedEmployeesOfRoute = this._iDistributionPlaning.fetchAssignedEmployeesOfRoute(plancode);
            return assignedEmployeesOfRoute;
        }

        [HttpGet]
        public HttpResponseMessage GetPlanDates(string plancode)
        {
            List<RoutePlanDateSeries> dateSeries = this._iDistributionPlaning.getDateSeries(plancode);
            return Request.CreateResponse(HttpStatusCode.OK, dateSeries);
        }

        [HttpPost]
        public HttpResponseMessage SaveEmployeeRoutePlanData([FromUri] string collectionData)
        {
            return Request.CreateResponse(HttpStatusCode.OK, "success");
        }

        [HttpGet]
        public HttpResponseMessage removeRouteFromPlan(string plancode, string routecode)
        {
            string result_removeRouteFromPlan = this._iDistributionPlaning.removeRouteFromPlan(plancode, routecode);
            if (result_removeRouteFromPlan == "success")
            {
                return Request.CreateResponse(HttpStatusCode.OK, "success");
            }
            else
            {
                return Request.CreateResponse(HttpStatusCode.OK, result_removeRouteFromPlan);
            }
        }

        [HttpGet]
        public HttpResponseMessage addRoutesToPlan(string plancode, string routecode)
        {
            if (!string.IsNullOrEmpty(plancode) && !string.IsNullOrEmpty(routecode)) 
            {
                string actionresult = this._iDistributionPlaning.addRoutesToPlan(plancode, routecode);
                return Request.CreateResponse(HttpStatusCode.OK, actionresult);
            }
            else
            {
                return Request.CreateResponse(HttpStatusCode.OK, "Plan Code or Route Code are not selected.");
            }
        }

        [HttpPost]
        public HttpResponseMessage SaveCalendarRoutePlanData(DIST_CALENDAR_ROUTE model)
        {
            string actionresult = this._iDistributionPlaning.saveCalendarRoute(model);
            return Request.CreateResponse(HttpStatusCode.OK, actionresult);

        }
        [HttpPost]
        public HttpResponseMessage SaveCalendarBrandingRoutePlanData(DIST_CALENDAR_ROUTE model)
        {
            string actionresult = this._iDistributionPlaning.saveCalendarBrandingRoute(model);
            return Request.CreateResponse(HttpStatusCode.OK, actionresult);

        }

        [HttpPost]
        public HttpResponseMessage ImportPlan()
        {
            var file = HttpContext.Current.Request.Files.Count > 0 ? HttpContext.Current.Request.Files[0] : null;
            //Previous one
            //ExcelRoutePlan plan = new ExcelRoutePlan { 
            //file=file,
            //empCode= empCode,
            //frmdate= frmdate,
            //todate= todate,
            //PlanName= planName
            //};
            string actionresult = this._iDistributionPlaning.saveExcelPlan(file);
            return Request.CreateResponse(HttpStatusCode.OK, actionresult);
        }
        [HttpGet]
        public HttpResponseMessage GetItemGroup(string filter)
        {           try            
            {
                List<ItemGroupModel> itemGroup = this._iDistributionPlaning.GetItemGroup(filter);
                return Request.CreateResponse(HttpStatusCode.OK, itemGroup);
            }
            catch
            {
                return Request.CreateResponse(HttpStatusCode.OK, "");
            }
        }
        [HttpGet]
        public HttpResponseMessage GetCustomerGroup(string filter)
        {
            try
            {
                List<CustomerGroup> itemGroup = this._iDistributionPlaning.GetCustomerGroup(filter);
                return Request.CreateResponse(HttpStatusCode.OK, itemGroup);
            }
            catch
            {
                return Request.CreateResponse(HttpStatusCode.OK, "");
            }
        }
        [HttpGet]
        public HttpResponseMessage GetCustomerSNGGroup(string filter)
        {
            try
            {
                List<CustomerSNGroup> itemGroup = this._iDistributionPlaning.GetCustomerSNGGroup(filter);
                return Request.CreateResponse(HttpStatusCode.OK, itemGroup);
            }
            catch
            {
                return Request.CreateResponse(HttpStatusCode.OK, "");
            }
        }
        [HttpGet]
        public HttpResponseMessage GetItemLists(string filter,string itmGroup)
        {
            try
            {
                List<ItemGroupModel> itemGroup = this._iDistributionPlaning.GetItemLists(filter,itmGroup);
                return Request.CreateResponse(HttpStatusCode.OK, itemGroup);
            }
            catch
            {
                return Request.CreateResponse(HttpStatusCode.OK, "");
            }
        }
        [HttpGet]
        public HttpResponseMessage GetCustomerLists(string filter,string cusGroup)
        {
            try
            {
                List<CustomerGroupModel> itemGroup = this._iDistributionPlaning.GetCustomerLists(filter,cusGroup);
                return Request.CreateResponse(HttpStatusCode.OK, itemGroup);
            }
            catch
            {
                return Request.CreateResponse(HttpStatusCode.OK, "");
            }
        }
        [HttpGet]
        public HttpResponseMessage GetCustomerSNGLists(string filter, string cusGroup)
        {
            try
            {
                List<CustomerGroupModel> itemGroup = this._iDistributionPlaning.GetCustomerSNGLists(filter, cusGroup);
                return Request.CreateResponse(HttpStatusCode.OK, itemGroup);
            }
            catch
            {
                return Request.CreateResponse(HttpStatusCode.OK, "");
            }
        }
        [HttpGet]
        public List<HolidayModel> HolidayDetails(string fromDate,string toDate)
        {
            var holidays = this._iDistributionPlaning.GetHolidayDetails(fromDate,toDate);
            return holidays;
        }
        public HttpResponseMessage SaveTargetData(ProfileModel model)
        {
            try {
                string actionresult;
                if (model.TargetId == 0)
                {
                     actionresult = this._iDistributionPlaning.saveTargetData(model);
                    if (actionresult == "success")
                    {
                        return Request.CreateResponse(HttpStatusCode.OK, "Targets created successfully!");
                    }
                    else
                    {
                        return Request.CreateResponse(HttpStatusCode.InternalServerError, "Failed to create targets!");
                    }
                }
                else
                {
                     actionresult = this._iDistributionPlaning.updateTargetData(model);
                    if (actionresult == "success")
                    {
                        return Request.CreateResponse(HttpStatusCode.OK, "Targets updated successfully!");
                    }
                    else
                    {
                        return Request.CreateResponse(HttpStatusCode.InternalServerError, "Failed to create targets!");
                    }
                }

            } catch
            {
                return Request.CreateResponse(HttpStatusCode.OK, "Failed to create targets!");
            }

        }
        [HttpGet]
        public List<TARGET_PLAN> GetTargetList()
        {
            return _iDistributionPlaning.getAllTargets();

        }

        [HttpPost]
        public HttpResponseMessage UpdateTarget(string targetId)
        {
            var message = this._iDistributionPlaning.UpdateTarget(targetId);
            if (message == "success")
            {
                return Request.CreateResponse(HttpStatusCode.OK, new { MESSAGE = "Target deleted Successfully", STATUS_CODE = (int)HttpStatusCode.OK });
            }
            else
            {
                return Request.CreateResponse(HttpStatusCode.OK, new { MESSAGE = "Something Wrong !! Try again Later", STATUS_CODE = (int)HttpStatusCode.BadRequest });
            }
        }
        [HttpGet]
        public TARGET_DETAILS getDataView(string targetId)
        {
            return this._iDistributionPlaning.GetTargetData(targetId);
        }
    }
}
