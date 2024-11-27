using NeoErp.Planning.Service.Interface;
using NeoErp.Planning.Service.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace NeoERP.Planning.Controllers
{
    public class DistributionPlaningController : Controller
    {
        private IDistributionPlaning _distributionPlaningRepo;
        public DistributionPlaningController(IDistributionPlaning distributionPlanningRepo)
        {
            this._distributionPlaningRepo = distributionPlanningRepo;
        }
        // GET: DistributionPlaning
        public ActionResult Index()
        {
            return View();
        }
        public ActionResult CalendarRouteSetup()
        {
            return PartialView();
        }

        public ActionResult BrandingCalendarRouteSetup()
        {
            return PartialView();
        }
        public ActionResult RouteSetup()
        {
            return PartialView();
        }
        public ActionResult CreateRoute()
        {
            return PartialView();
        }
        public ActionResult RouteList()
        {
            return PartialView();
        }
        public ActionResult EmployeeRouteSetup(string routecode)
        {
            ViewBag.routecode = routecode;
            return PartialView();
        }

        [HttpPost]
        public JsonResult SaveEmployeeRoutePlanData(FormCollection collData)
        {
            try
            {
                List<DIST_ROUTE_DETAIL> routeDetailList = new List<DIST_ROUTE_DETAIL>();
                List<string> allKeys = collData.AllKeys.ToList();
                int planCode = 0;
                int.TryParse(collData.GetValue("planCode").AttemptedValue,out planCode);
                for (int i = 1; i < collData.AllKeys.Length; i++)
                {
                    string key = collData.Keys[i];
                    string value = collData[i];
                    string[] employee = value.Split(',');
                    foreach (var item in employee)
                    {
                        DateTime assignDate = Convert.ToDateTime(key.Split('_')[0].ToString()).Date;
                        string route = key.Split('_')[2].ToString();
                        routeDetailList.Add(new DIST_ROUTE_DETAIL()
                        {
                            PLAN_CODE=planCode,
                            ROUTE_CODE = route,
                            EMP_CODE = item,
                            ASSIGN_DATE = assignDate,
                        });
                    }
                }
                if (this._distributionPlaningRepo.SaveEmployeeRoutePlan(routeDetailList))
                {
                    return Json("success", JsonRequestBehavior.AllowGet);
                }
                else
                {
                    return Json("error", JsonRequestBehavior.AllowGet);
                }
            }
            catch (Exception)
            {

                throw;
            }
        }
        public ActionResult TargetSetup()
        {
            return PartialView();
        }
        public ActionResult CreateTargetSetup()
        {
            return PartialView();
        }

        public ActionResult ViewTarget(string targetId)
        {
            return PartialView();
        }
        public ActionResult EditTarget(string targetId)
        {
            return PartialView();
        }
    }
}