using NeoErp.Core.Models;
using NeoErp.Distribution.Service.Model.Mobile;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web;

namespace NeoErp.Distribution.Service.Service.Mobile
{
    public interface IMobileOfflineService
    {
        List<string> GetSyncIds(string tableName, List<string> SyncIds, NeoErpCoreEntity dbContext);

        #region Fetching Data
        List<LoginResponseModel> Login(LoginModel model, NeoErpCoreEntity dbContext);
        Dictionary<string, string> Logout(LogoutRequestModel model, NeoErpCoreEntity dbContext);
        List<VisitEntityModel> GetVisitPlan(VisitPlanRequestModel model, NeoErpCoreEntity dbContext, PreferenceModel pref);
        List<VisitBrdModel> GetBrdVisitPlan(VisitPlanRequestModel model, NeoErpCoreEntity dbContext, PreferenceModel pref);
        Dictionary<string, List<EntityResponseModel>> FetchEntity(CommonRequestModel model, NeoErpCoreEntity dbContext);
        Dictionary<string, List<EntityResponseModel>> FetchAllCompanyEntity(string companyCode, string spCode, NeoErpCoreEntity dbContext, PreferenceModel pref);
        List<ItemModel> FetchItems(CommonRequestModel model, NeoErpCoreEntity dbContext);
        QuestionResponseModel FetchAllQuestions(QuestionRequestModel model, NeoErpCoreEntity dbContext);
        List<GeneralModel> GetQuestion(QuestionRequestModel model, string setId, NeoErpCoreEntity dbContext);
        List<AreaResponseModel> FetchArea(CommonRequestModel model, NeoErpCoreEntity dbContext);
        Dictionary<string, OutletResponseModel> FetchOutlets(CommonRequestModel model, NeoErpCoreEntity dbContext);
        ClosingStockResponseModel GetEntityItemByBrand(ClosingStockRequestModel model, NeoErpCoreEntity dbContext);
        Dictionary<string, Dictionary<string, MuCodeResponseModel>> FetchMU(CommonRequestModel model, NeoErpCoreEntity dbContext);
        List<TransactionResponseModel> FetchTransactions(TransactionRequestModel model, NeoErpCoreEntity dbContext);
        Dictionary<string, List<PurchaseOrderResponseModel>> FetchPurchaseOrder(PurchaseOrderRequestModel model, NeoErpCoreEntity dbContext);
        SalesAgeReportResponseModel SalesAgingReport(ReportRequestModel model, NeoErpCoreEntity dbContext);
        Dictionary<string, string> MonthWiseSales(ReportRequestModel model, NeoErpCoreEntity dbContext);
        Dictionary<string, string> AgingReport(ReportRequestModel model, NeoErpCoreEntity dbContext);
        DistributorItemResponseModel FetchEntityPartyTypeAndMu(EntityRequestModel model, NeoErpCoreEntity dbContext);
        Dictionary<string, List<EntityResponseModel>> FetchPartyTypeBillingEntity(EntityRequestModel model, NeoErpCoreEntity dbContext);
        List<EntityResponseModel> FetchEntityById(EntityRequestModel model, NeoErpCoreEntity dbContext);
        Dictionary<string, List<EntityResponseModel>> FetchDistributorWithConstraint(CommonRequestModel model, NeoErpCoreEntity dbContext);
        Dictionary<string, object> SyncData(VisitPlanRequestModel model, NeoErpCoreEntity dbContext);
        Dictionary<string, object> SyncDataTopic(VisitPlanRequestModel model, NeoErpCoreEntity dbContext);
        List<SPEntityModel> FetchSpPartyType(VisitPlanRequestModel model, NeoErpCoreEntity dbContext);
        List<SPEntityModel> FetchSpCustomer(VisitPlanRequestModel model, NeoErpCoreEntity dbContext);
        List<ImageCategoryModel> FetchImageCategory(CommonRequestModel model, NeoErpCoreEntity dbContext);
        List<ImageCategoryModel>  FetchImageDistributionCategory(CommonRequestModel model, NeoErpCoreEntity dbContext);
        List<ResellerEntityModel> FetchResellerEntity(CommonRequestModel model, PreferenceModel pref, string spCode, NeoErpCoreEntity dbContext);
        List<DistributorItemModel> FetchDistributorItems(CommonRequestModel model, NeoErpCoreEntity dbContext);
        List<ResellerGroupModel> GetResellerGroups(CommonRequestModel model, NeoErpCoreEntity dbContext);
        List<ContractModel> GetContracts(CommonRequestModel model, NeoErpCoreEntity dbContext);

        #endregion Fetching Data

        #region Inserting Data
        Dictionary<string, string> UpdateMyLocation(UpdateRequestModel model, NeoErpCoreEntity dbContext);
        Dictionary<string, string> UpdateCurrentLocation(UpdateRequestModel model, NeoErpCoreEntity dbContext);
        Dictionary<string, string> SaveExtraActivity(UpdateRequestModel model, NeoErpCoreEntity dbContext);
        Dictionary<string, string> UpdateCustomerLocation(UpdateCustomerRequestModel model, NeoErpCoreEntity dbContext);
        Dictionary<string, string> NewPurchaseOrder(PurchaseOrderModel model, NeoErpCoreEntity dbContext);
        Dictionary<string, string> CancelPurchaseOrder(CancelPurchaseOrderModal model, NeoErpCoreEntity dbContext);
        Dictionary<string, string> NewCollection(CollectionRequestModel model, NeoErpCoreEntity dbContext);
        Dictionary<string, string> NewMarketingInformation(InformationSaveModel model, NeoErpCoreEntity dbContext);
        Dictionary<string, string> NewCompetitorInformation(InformationSaveModel model, NeoErpCoreEntity dbContext);
        Dictionary<string, Dictionary<string, string>> SaveQuestionaire(QuestionaireSaveModel model, NeoErpCoreEntity dbContext);
        Dictionary<string, string> UpdateDealerStock(UpdateEntityRequestModel model, NeoErpCoreEntity dbContext);
        Dictionary<string, string> UpdateDistributorStock(UpdateEntityRequestModel model, NeoErpCoreEntity dbContext);
        Dictionary<string, string> UpdateResellerStock(UpdateEntityRequestModel model, NeoErpCoreEntity dbContext);
        Dictionary<string, string> CreateReseller(CreateResellerModel model, NeoErpCoreEntity dbContext);
        Dictionary<string, string> UploadEntityMedia(List<EntityRequestModelOffline> model, HttpFileCollection files, NeoErpCoreEntity dbContext);
        Dictionary<string, string> UploadResellerEntityMedia(List<EntityRequestModelOffline> list, HttpFileCollection files, NeoErpCoreEntity dbContext);
        Dictionary<string, string> SaveScheme(SchemeModel model, NeoErpCoreEntity dbContext);
        Dictionary<string, string> SaveCompAns(CompAnsModel model, NeoErpCoreEntity dbContext);
        Dictionary<string, string> SaveDeviceLog(MobileLogModel model, NeoErpCoreEntity dbContext);
        Dictionary<string, string> SaveCrmTask(CrmModel model, NeoErpCoreEntity dbContext);
        Dictionary<string, string> CreateDistributor(CreateDistributorModel model, NeoErpCoreEntity dbContext);

        Dictionary<string, string> SaveDistSalesReturn(DistributionSalesReturnModel returnModel, NeoErpCoreEntity dbContext);
        #endregion Inserting Data
    }
}