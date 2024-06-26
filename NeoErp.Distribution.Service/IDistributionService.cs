﻿using NeoErp.Core.Domain;
using NeoErp.Core.Helpers;
using NeoErp.Core.Models;
using NeoErp.Distribution.Service.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NeoErp.Distribution.Service
{
   public interface IDistributionService
    {
        List<RouteList> GetRoute();
        List<SalesRegisterModel> GetSalesRegister(ReportFiltersModel filters, User userinfo);
         List<VistitedPlanModel> GetCurrentVistedEntity(string date, string CompanyCode);
        List<MrVisitedTrackingModel> GetCurrentMrTrackingDetails(DateTime SUBMITDATE);
        //List<MrVisitedTrackingModel> GetCurrentMrTrackingDetails();
        List<MrVisitedTrackingRecord> GetDateWiseMrVistedRoute(string date, string spCode, string companyCode = "01");
        List<SalesDistrictModel> GetSalesByDistrictAndAreaBoundary(string CompanyCode = "01");
        List<AreaBoundaryViewModel> GetSalesAreaBoundary(string CompanyCode = "01");
        List<PurchaseOrderModel> GetPurchaseOrder(ReportFiltersModel model, User userInfo, string requestStatus);
        List<PurchaseOrderModel> GetDealerSalesOrder(ReportFiltersModel model, User userInfo, string requestStatus);
        List<PurchaseOrderModel> GetPurchaseOrderSummary(ReportFiltersModel model, User userInfo, string requestStatus);
        List<DistSalesReturnViewModel> GetDistributionSalesReturn(ReportFiltersModel reportFilters,string companyCode);
        DSRResponse UpdateDistSalesReturn(string returnNo, string updateFlag);
        List<DistSalesReturnViewModel> FilterDistSalesReturn(string value);
        List<PurchaseOrderModel> GetPurchaseOrderDaily(ReportFiltersModel model, User userInfo);
        List<ItemModel> FetchItems(string CompanyCode, string BranchCode, string DistCode);
        List<ItemModel> FetchAllItems(User userInfo, string type);
        List<CustomerModel> CompItems(User userInfo);
        List<PurchaseOrderModel> GetPurchaseOrderDetail(string CompanyCode, string ORDER_ENTITY, string orderCode, string requestStatus);
        List<PurchaseOrderModel> GetDealerSalesOrderDetail(string CompanyCode, string ORDER_ENTITY, string orderCode, string requestStatus);
        List<DistSalesReturnItemViewModel> GetSalesReturnDetail(ReportFiltersModel model, string CompanyCode,  string orderCode);
        List<VisitSummaryViewModel> GetVisitSummaryReport(ReportFiltersModel model, User userInfo);
        List<VisitSummaryViewModel> GetVisitSummaryBrandingReport(ReportFiltersModel model, User userInfo);
        List<VisitSummaryViewModel> GetVisitSummaryReportAll(ReportFiltersModel model, User userInfo);
        List<EmployeeActivityDetail> GetEmployeeActivityReport(ReportFiltersModel model, User userInfo);
        List<VisitTimeModel> GetVisitTimeSummary(ReportFiltersModel model, User userInfo);
        List<VisitTimeModel> GetLastLocations(User userInfo);
        CreditDaysBalanceModel GetCreditDaysBalance(string companyCode, string subCode, string creditLimit, string daysLimit);
        List<QuestionaireCustomerModel> GetQuestionaireReport(ReportFiltersModel model, User userInfo, string surveyCode);
        List<QuestionaireModel> GetQuestionaire(ReportFiltersModel model, string CustomerCode, string CustomerType, string surveyCode, User userInfo);
        List<SurveyDDl> GetSurveys(User userInfo);
        bool DeletePOItem(decimal orderCode, string itemCode,bool isParent);
        bool UpdatePO(PurchaseOrderModel model, string userId);
        bool UpdateDSO(PurchaseOrderModel model, string userId);
        bool CancelDSO(PurchaseOrderModel model, string userId);
        List<FormSetupModel> GetFormCode(User userIndo);
        List<SURVEY_COLUMN_MODEL> GetSurveyReportCol();
        List<SURVEY_COLUMN_MODEL> GetWebSurveyReportQUE(User userInfo);
        List<DistAreaModel> GetDistributionArea(User userInfo);
        List<DistAreaModel> GetIndividualGroup(User userInfo, string SingleAreaCode);
        List<SURVEY_REPORT_MODEL> GetSurveyReport(filterOption model);
        List<SURVEY_REPORT_AATA_MODEL> GetSurveyReportAata(filterOption model);
        List<SURVEY_REPORT_AATA_MODEL> GetSurveyReportAataTab(filterOption model);

        List<SURVEY_REPORT_MODEL> GetSurveyReportDynamic(filterOption model);
        List<SURVEY_REPORT_MODEL> GetDynamicWebSurveyReport(filterOption model, User user);
        List<SURVEY_REPORT_MODEL> getSurveyReport_JGI(filterOption model);
        List<SURVEY_REPORT_BRANDING_MODEL> GetBrandingSurveyReportTab(filterOption model);
        List<MerchandisingStockModel> GetMerchandisingStockReport(filterOption model, User user);
        List<CustomerIModel> GetIndividualCustomer(User userInfo);
        List<ResellerListModel> GetResellerList(User userInfo);
        List<CustomerIModel> GetWholeSellers(User userInfo);
        List<DistAreaModel> GetRouteDistributor(string areaCode, User userInfo);
        List<ResellerAreaModel> GetRouteReseller(string areaCode, User userInfo);
        List<DistAreaModel> GetDistributionAreaByRouteCode(string routeCode, User userInfo);
        List<RouteEntityModel> getSelectedCustomerByRouteCode(string routeCode, User userInfo);
        List<PhotoInfoModel> GetPhotoInfo(string entityType, string entityCode);
        DistCustomerInfoModel GetDistCustomerInfo(string entityType, string entityCode, string CompanyCode);
        List<PartyTypeAreaModel> GetRouteDealer(string areaCode, User userInfo);
        List<HoardingAreaModel> GetRouteHoarding(string areaCode, User userInfo);
        List<CustomerSales> GetCustomerSales(string DivisionCode = "", string CompanyCode = "01", string IsorderFromOutlet = "false");
        List<CollectionModel> GetCollectionReport(ReportFiltersModel model, User userInfo);
        List<PerformanceModel> GetPerformanceReportList(ReportFiltersModel dateFilter);
        List<SumOutletModel> GetSumOutletReportList(ReportFiltersModel dateFilter);
        List<OutletSummaryModel> GetOutletSummaryReportList(ReportFiltersModel dateFilter);
        List<TopEffectiveModel> GetTopEffectiveCallsReportList(string percentEffectiveCalls,ReportFiltersModel dateFilter);
        List<PurchaseOrderReportModel> GetPurchaseOrderList(ReportFiltersModel model, string requestStatus,User userInfo);
        List<PurchaseOrderReportModel> GetWholeSellerPurchaseOrderReport(ReportFiltersModel model, string requestStatus, User userInfo);
        List<PurchaseOrderReportModel> GetResellerPurchaseOrderList(ReportFiltersModel model, string requestStatus,User userInfo);
        List<SalesPersonPoModel> GetSalesPersonList(ReportFiltersModel model,string requestStatus,User userInfo);
        List<SalesPersonPoModel> GetItemCumulativeReport(ReportFiltersModel model, User userInfo,string dateFlag);
        PreferenceSetupModel GetPrefSetting(User userInfo);
        List<AreaWiseDistributorModel> GetAreaWiseDistributor(filterOption model, User userInfo, string type);
        List<TopEffectiveModel> GetALLPerformanceReport(ReportFiltersModel model, User userInfo);
        List<DetailTopEffective> GetPresentASMBeat(ReportFiltersModel model, User userInfo);
        List<DetailTopEffective> GetASMBeat(ReportFiltersModel model, User userInfo);

        List<TopEffectiveModel> GetALLEmployeeReport(ReportFiltersModel model, User userInfo);
        List<TopEffectiveModel> GetALLEmployeeReportNew(ReportFiltersModel model, User userInfo);
        List<DetailTopEffective> GetDetailEmployeeReport(ReportFiltersModel model, User userInfo);
        List<DetailTopEffective> GetDetailBrandingEmployeeReport(ReportFiltersModel model, User userInfo);
        List<EmployeeWisePerformance> GetEmployeeProductive(ReportFiltersModel model, User userInfo, string SP_CODE);
        List<EmployeeWisePerformance> GetBrandingEmployeeProductive(ReportFiltersModel model, User userInfo, string SP_CODE);
        List<EmpBrandWiseModel> GetBrandwiseEmpData(ReportFiltersModel model, User userInfo, string SpCode);

        PurchaseOrderReportModel GetMaxOrderNoFromDistributor();
        List<EODModel> GetEODList(ReportFiltersModel model, User userInfo);
        List<EODModel> GetEODDetail(ReportFiltersModel model, string SP_CODE, string type, User userInfo);
        List<AttendanceModel> GetAttendanceReport(ReportFiltersModel model, User userInfo);
        List<AttendanceModel> GetBrandingAttendanceReport(ReportFiltersModel model, User userInfo);
        List<MrVisitedTrackingModel> GetMRVisitTracking(ReportFiltersModel model, User userInfo);
        List<MrVisitedTrackingModel> GetMRVisitedAllAssignRoute(string date,string spcode, string companyCode);
        List<MrVisitedTrackingModel> GetMRVisitedAssignRouteWithOrder(string date, string spcode, string companyCode);
        List<MrVisitedTrackingModel> GetMRVisitedUnAssignRouteWithOrder(string date, string spcode, string companyCode);
        List<DailyActivityModel> GetDailyAcivityList(ReportFiltersModel model, User userInfo);
        List<ClosingReportModel> GetOutletClosingReport(ReportFiltersModel model, User userInfo);
        List<VisitImageModel> GetVisiterList(ReportFiltersModel filter, User userinfo,string Distributor="",string Reseller="");
        List<VisitImageModel> GetBrandingVisiterList(ReportFiltersModel filter, User userinfo, string Distributor = "", string Reseller = "");
        List<VisitImageModel> GetVisiterListCondition(ReportFiltersModel filter, User userinfo, string Distributor = "", string Reseller = "");
        List<CompReportModel> GetCompReport(ReportFiltersModel filter, User userInfo, string Item_code);
        List<CompReportModel> GetCompReportMonthly(ReportFiltersModel filter, User userInfo, string Item_code, string Category);
        List<SPDistanceModel> GetSPTravelReport(ReportFiltersModel filter, User userInfo);
        List<SPTraveModel> GetSPRouteReport(ReportFiltersModel filter, User userInfo,string source);
        List<SPDistanceModel> GetSPVisitEntity(ReportFiltersModel filter, User userInfo);
        List<ResellerListModel> GetResellerDetailReport(ReportFiltersModel filter, User userInfo, string source);
        List<ItemWiseMinMaxReport> GetItemsMinMaxList(ReportFiltersModel model, User userInfo);
        List<RouteModel> getAllRoutesByFilter(string filter, string empCode);
        List<DIST_PLAN_ROUTE> getAllPlanRoutes(string plancode, User userInfo);
        List<RouteModelPlan> GetRouteByPlanCode(string code, User userInfo);
        List<ModelEmployee> getEmployees(string filter, string empGroup, User userInfo);
        DetailColumnModel GetDynamicFields(User userInfo);
        List<MobileLogModel> GetDeviceLog(filterOption model, User userInfo);
        List<AreaWiseDistributorModel> GetUserWiseOUtlet(filterOption model, User userInfo, string type);

        List<SummaryReport> GetAllSummaryReport(filterOption model,User userInfo);

        List<AttendanceReportCalendarModel> GetAttendanceForCalendar(filterOption model);
        List<DistResellerStockModel> GetDistResellerStockList(filterOption model, User userInfo, string Distributor = "", string Reseller = "");
        List<DistResellerStockConversionModel> GetDistResellerBrandItemStockList(filterOption model, User userInfo, string Distributor = "", string Reseller = "");
        List<DistdistributorStockModel> GetDistDistributorBrandItemStockList(filterOption model, User userInfo, string Distributor = "", string Reseller = "");
        List<DetailTopEffective> GetDetailEmpReportIndivisual(ReportFiltersModel model, User userInfo);
        List<DistAttendanceReportSummary> GetAttendanceSummaryGroup(ReportFiltersModel model, User userInfo);
        List<DistAttendanceReportSummary> GetAttendanceSummaryEmployeeWise(ReportFiltersModel model, User userInfo, String GroupWise);
        List<DetailTopEffective> GetDetailEmployeeReportDetail(ReportFiltersModel model, User userInfo);
        List<EmpBrandWiseModel> GetBrandwiseEmpDataConversion(ReportFiltersModel model, User userInfo, string SpCode);
        List<AttendanceModel> GetAttendanceReportEmployeeWise(ReportFiltersModel model, User userInfo, string sp_code);
        List<StockSummary> GetStockGroupSummary(ReportFiltersModel model, User userInfo);
        List<StockSummary> GetAreaGroupSummary(ReportFiltersModel model, User userInfo, string groupid);
        List<StockSummary> GetResellerCodeStock(ReportFiltersModel model, User userInfo, string AreaId);
        List<StockDetailReort> GetResellerCodeStockDetail(ReportFiltersModel model, User userInfo, string AreaId);
        List<AttendanceModel> GetEmployeeRoute(ReportFiltersModel model, User userInfo, string sp_code);
        List<EmpBrandWiseModel> GetItemwiseEmpDataConversion(ReportFiltersModel model, User userInfo, string SpCode);
        List<SchemeModel> GetSchemeName(User userInfo);
        List<SchemeReportModel> GetSchemeSalesPersonList(User userInfo, string SchemeID);
        List<SchemeModel> GetSchemeAndDetails(User userInfo);
        List<SalesPersonPoModel> GetSchemeReport(User userInfo, string ID, string MinVal, string MaxVal, string fromDate, string toDate);
    }
    
}

