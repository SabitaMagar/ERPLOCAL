using NeoErp.Core;
using NeoErp.Core.Helpers;
using NeoErp.Distribution.Service;
using NeoErp.Distribution.Service.Model;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Globalization;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using System.Web.Http.ModelBinding;

namespace NeoErp.Distribution.Controllers.Api
{
    public class DistributionController : ApiController
    {
        public IDistributionService _distributionService { get; set; }
        private IWorkContext _workContext;

        public DistributionController(IDistributionService distributionService, IWorkContext workContext)
        {
            this._distributionService = distributionService;
            this._workContext = workContext;
        }

        [HttpPost]
        public VisitedPlainList GetVistitedPlain(dynamic fullData)
        {

            filterOption reportFilter = Newtonsoft.Json.JsonConvert.DeserializeObject<filterOption>(fullData.modelPar.ToString());
            AdvancedFilterDistributionModel distReportFilter = Newtonsoft.Json.JsonConvert.DeserializeObject<AdvancedFilterDistributionModel>(fullData.distModel.ToString());
            var model = new VisitedPlainList();
            var modelrequest = reportFilter;
            if (string.IsNullOrEmpty(modelrequest.ReportFilters.FromDate as string))
                modelrequest.ReportFilters.FromDate = DateTime.Now.ToString();

            var data = _distributionService.GetCurrentVistedEntity(Convert.ToDateTime(modelrequest.ReportFilters.FromDate).ToString("dd-MMM-yyyy"), _workContext.CurrentUserinformation.company_code);
            if (distReportFilter.DistributorFilter.Count() > 0)
                model.distributor = data.Where(x => x.TYPE == "distributor" && distReportFilter.DistributorFilter.Contains(x.CODE)).ToList();
            else
                model.distributor = data.Where(x => x.TYPE == "distributor").ToList();
            if (distReportFilter.OutletFilter.Count() > 0)
                model.reseller = data.Where(x => x.TYPE == "reseller").ToList();
            else
                model.reseller = data.Where(x => x.TYPE == "reseller" && distReportFilter.OutletFilter.Contains(x.CODE)).ToList();



            model.dealer = data.Where(x => x.TYPE == "dealer").ToList();
            model.MrVisited = _distributionService.GetCurrentMrTrackingDetails(Convert.ToDateTime(modelrequest.ReportFilters.FromDate));
            return model;
        }
        [HttpPost]
        public VisitedPlainList GetNewAddresOutlet(dynamic fullData)
        {

            filterOption reportFilter = Newtonsoft.Json.JsonConvert.DeserializeObject<filterOption>(fullData.modelPar.ToString());
            AdvancedFilterDistributionModel distReportFilter = Newtonsoft.Json.JsonConvert.DeserializeObject<AdvancedFilterDistributionModel>(fullData.distModel.ToString());
            var model = new VisitedPlainList();
            var modelrequest = reportFilter;
            if (string.IsNullOrEmpty(modelrequest.ReportFilters.FromDate as string))
                modelrequest.ReportFilters.FromDate = DateTime.Now.ToString();

            var data = _distributionService.GetCurrentVistedEntity(Convert.ToDateTime(modelrequest.ReportFilters.FromDate).ToString("dd-MMM-yyyy"), _workContext.CurrentUserinformation.company_code);
            if (distReportFilter.DistributorFilter.Count() > 0)
                model.distributor = data.Where(x => x.TYPE == "distributor" && distReportFilter.DistributorFilter.Contains(x.CODE)).ToList();
            else
                model.distributor = data.Where(x => x.TYPE == "distributor").ToList();
            if (distReportFilter.OutletFilter.Count() > 0)
                model.reseller = data.Where(x => x.TYPE == "reseller").ToList();
            else
                model.reseller = data.Where(x => x.TYPE == "reseller" && distReportFilter.OutletFilter.Contains(x.CODE)).ToList();



            model.dealer = data.Where(x => x.TYPE == "dealer").ToList();
            model.MrVisited = _distributionService.GetCurrentMrTrackingDetails(Convert.ToDateTime(modelrequest.ReportFilters.FromDate));
            return model;
        }
        [HttpPost]
        public DistCustomerInfo GetPhotoInfo(string entityTye, string entityCode, string companyCode)
        {
            DistCustomerInfo model = new DistCustomerInfo();
            companyCode = string.IsNullOrWhiteSpace(companyCode) ? _workContext.CurrentUserinformation.company_code : companyCode;
            model.photoInfo = _distributionService.GetPhotoInfo(entityTye, entityCode);
            model.distCustomerInfo = _distributionService.GetDistCustomerInfo(entityTye, entityCode, this._workContext.CurrentUserinformation.company_code);
            string url = ConfigurationManager.AppSettings["baseUrl"];
            foreach (var d in model.photoInfo)
            {
                d.FILENAME = string.Format(url + "areas/NeoErp.Distribution/Images/EntityImages/{0}", d.FILENAME);
            }
            return model;

        }
        [HttpPost]
        public List<MrVisitedTrackingModel> GetMrVistitedPlain(filterOption model)
        {
            if (model == null)
                model = new filterOption();
            var data = _distributionService.GetMRVisitTracking(model.ReportFilters, _workContext.CurrentUserinformation);
            return data;
        }
        [HttpPost]
        public MRVisitedTrackingList GetMrVistitedPlainDateWise(string date, string spCode)
        {

            //DateTime dt = DateTime.ParseExact(date, "dd-MM-yyyy", CultureInfo.InvariantCulture);
            //var model = new VisitedPlainList();
            string datetime = string.Empty;
            if (string.IsNullOrEmpty(date) || date == "null")
                datetime = DateTime.Now.ToString("dd-MMM-yyyy");
            else
            {
                DateTime dt;
                DateTime.TryParseExact(date, "dd-MM-yyyy", CultureInfo.InvariantCulture, DateTimeStyles.None, out dt);
                datetime = dt.ToString("dd-MMM-yyyy");
            }
            var comp = this._workContext.CurrentUserinformation.company_code;
            filterOption model = new filterOption();
            model.ReportFilters.FromDate = datetime;
            model.ReportFilters.ToDate = datetime;

            MRVisitedTrackingList data = new MRVisitedTrackingList();
            data.VisitActivityList = _distributionService.GetDateWiseMrVistedRoute(datetime, spCode, comp);
            data.AssignRouteList = _distributionService.GetMRVisitedAllAssignRoute(datetime, spCode, comp);
            data.VisitEffectiveCalls = _distributionService.GetMRVisitTracking(model.ReportFilters, _workContext.CurrentUserinformation)
                                         .Where(x => x.SP_CODE == spCode).ToList();
            var AssignRouteWithOrder = _distributionService.GetMRVisitedAssignRouteWithOrder(datetime, spCode, comp);
            if (AssignRouteWithOrder.Count() > 0)
                foreach (var item in data.AssignRouteList)
                    if (AssignRouteWithOrder.Any(x => x.ENTITY_CODE == item.ENTITY_CODE))
                        item.GET_ORDER = "Y";
            var UnAssignRouteWithOrder = _distributionService.GetMRVisitedUnAssignRouteWithOrder(datetime, spCode, comp);
            if (UnAssignRouteWithOrder.Count() > 0)
                foreach (var item in data.VisitActivityList)
                    if (UnAssignRouteWithOrder.Any(x => x.ENTITY_CODE == item.ENTITY_CODE))
                        item.GET_ORDER = "Y";

            return data;

        }
        [HttpPost]
        public SalesDistrictViewModel GetSalesByDistrictAndAreaBoundary(string companyCode)
        {
            SalesDistrictViewModel model = new SalesDistrictViewModel();
            Dictionary<string, decimal?> dicsales = new Dictionary<string, decimal?>();
            var companyCode1 = _workContext.CurrentUserinformation.company_code;


            var data = _distributionService.GetSalesByDistrictAndAreaBoundary(companyCode1);
            var groupdata = data.GroupBy(x => x.district).Select(y => new { district = y.Key, Amount = y.Sum(z => z.sales) });
            foreach (var group in groupdata)
            {
                dicsales.Add(group.district, group.Amount);
            }

            var areadata = _distributionService.GetSalesAreaBoundary(companyCode1);
            try
            {
                foreach (var d in areadata)
                {
                    model.boundary.Add(d.AREA_NAME, new areaboundary
                    {
                        code = d.AREA_CODE,
                        data = d.AREA_PATH,
                        description = d.REMARKS,
                        district = d.DISTRICT_NAME
                    });
                }
            }
            catch (Exception)
            {

            }

            model.sales = dicsales;

            // model.salesDecimal = abc;


            return model;
        }

        [HttpPost]
        public VisitSummaryModel GetVisitSummaryReport(filterOption model)
        {
            VisitSummaryModel reportData = new VisitSummaryModel();
            reportData.VisitSummaryViewModel = this._distributionService.GetVisitSummaryReport(model.ReportFilters, _workContext.CurrentUserinformation).ToList();
            reportData.total = reportData.VisitSummaryViewModel.Count();
            reportData.AggregationResult = KendoGridHelper.GetAggregation(reportData.VisitSummaryViewModel, model.aggregate);
            return reportData;

        }
        [HttpPost]
        public VisitSummaryModel GetVisitSummaryBrandingReport(filterOption model)
        {
            VisitSummaryModel reportData = new VisitSummaryModel();
            reportData.VisitSummaryViewModel = this._distributionService.GetVisitSummaryBrandingReport(model.ReportFilters, _workContext.CurrentUserinformation).ToList();
            reportData.total = reportData.VisitSummaryViewModel.Count();
            reportData.AggregationResult = KendoGridHelper.GetAggregation(reportData.VisitSummaryViewModel, model.aggregate);
            return reportData;

        }

        [HttpPost]
        public VisitSummaryModel GetVisitSummaryReportAll(filterOption model)
        {
            VisitSummaryModel reportData = new VisitSummaryModel();
            reportData.VisitSummaryViewModel = this._distributionService.GetVisitSummaryReportAll(model.ReportFilters, _workContext.CurrentUserinformation).ToList();
            reportData.total = reportData.VisitSummaryViewModel.Count();
            reportData.AggregationResult = KendoGridHelper.GetAggregation(reportData.VisitSummaryViewModel, model.aggregate);
            return reportData;

        }
        [HttpPost]
        public  List<EmployeeActivityDetail> GetEmployeeActivityReport(filterOption model)
        {
           // EmployeeActivityDetail reportData = new EmployeeActivityDetail();
           var  reportData = this._distributionService.GetEmployeeActivityReport(model.ReportFilters, _workContext.CurrentUserinformation).ToList();
           // reportData.total = reportData.VisitSummaryViewModel.Count();
           // reportData.AggregationResult = KendoGridHelper.GetAggregation(reportData.VisitSummaryViewModel, model.aggregate);
            return reportData;

        }

        [HttpGet]
        public List<VisitTimeModel> GetLastLocations()
        {
            var result = _distributionService.GetLastLocations(_workContext.CurrentUserinformation);
            return result;
        }

        [HttpPost]
        public List<VisitTimeModel> GetVisitSummaryTimeReport(filterOption model)
        {
            var data = _distributionService.GetVisitTimeSummary(model.ReportFilters, _workContext.CurrentUserinformation);
            return data;
        }

        [HttpPost]
        public List<SPDistanceModel> GetSPVisitCount(filterOption model)
        {
            var data = _distributionService.GetSPVisitEntity(model.ReportFilters, _workContext.CurrentUserinformation);
            return data;
        }

        public IList<FormSetupModel> GetFormCode()
        {
            return this._distributionService.GetFormCode(_workContext.CurrentUserinformation);

        }


        public IList<DistAreaModel> GetDistArea()
        {
            return this._distributionService.GetDistributionArea(_workContext.CurrentUserinformation);

        }
        public IList<CustomerIModel> GetCustomersByArea(string areaCode)
        {
            return this._distributionService.GetCustomersByArea(areaCode, _workContext.CurrentUserinformation);
        }

        public List<DistAreaModel> GetIndividualGroup(string SingleAreaCode)
        {
            var list =_distributionService.GetIndividualGroup(_workContext.CurrentUserinformation, SingleAreaCode);
            return list;
        }


        public IList<CustomerIModel> GetIndividualCustomer()
        {
            return this._distributionService.GetIndividualCustomer(_workContext.CurrentUserinformation);
        }

        public IList<ResellerListModel> GetResellerList()
        {
            return this._distributionService.GetResellerList(_workContext.CurrentUserinformation);
        }

        public IList<CustomerIModel> GetWholesellers()
        {
            return this._distributionService.GetWholeSellers(_workContext.CurrentUserinformation);
        }

        public AreaDistResellerModel GetRouteDistResellerByArea(string areaCode)
        {
            var user = _workContext.CurrentUserinformation;
            AreaDistResellerModel model = new AreaDistResellerModel();
            model.distributors = _distributionService.GetRouteDistributor(areaCode, user);
            model.resellers = _distributionService.GetRouteReseller(areaCode, user);
            model.dealers = _distributionService.GetRouteDealer(areaCode, user);
            model.hoardings = _distributionService.GetRouteHoarding(areaCode, user);
            return model;
        }


        public IList<DistAreaModel> GetAreaByRouteCode(string routeCode)
        {
            return this._distributionService.GetDistributionAreaByRouteCode(routeCode, _workContext.CurrentUserinformation);
        }



        public IList<RouteEntityModel> GetSelectedCustomerByRouteCode(string routeCode)
        {
            return this._distributionService.getSelectedCustomerByRouteCode(routeCode, _workContext.CurrentUserinformation);
        }


        [HttpPost]
        public List<CustomerSales> GetCustomerSales(string DivisoinCode="",string Company_code="",string orderfromoutlet="true")
        {
            var AfterEncodeDivision = System.Net.WebUtility.HtmlDecode(DivisoinCode);
            var AfterCompany = System.Net.WebUtility.HtmlDecode(Company_code);
            if (string.IsNullOrEmpty(AfterCompany))
                AfterCompany = _workContext.CurrentUserinformation.company_code;
            var data = _distributionService.GetCustomerSales(AfterEncodeDivision, AfterCompany, orderfromoutlet);
            
            return data;
        }
        [HttpPost]
        public CollectionviewModel GetCollectionsSales(filterOption model)
        {
            var models = new CollectionviewModel();


            models.collectionViewModels = _distributionService.GetCollectionReport(model.ReportFilters, _workContext.CurrentUserinformation).ToList();
            models.total = models.collectionViewModels.Count();
            models.AggregationResult = KendoGridHelper.GetAggregation(models.collectionViewModels, model.aggregate);
            return models;

        }

        [HttpGet]
        public PreferenceSetupModel GetDistPreferences()
        {
            var user = _workContext.CurrentUserinformation;
            var data = _distributionService.GetPrefSetting(user);
            return data;
        }
    }
}
