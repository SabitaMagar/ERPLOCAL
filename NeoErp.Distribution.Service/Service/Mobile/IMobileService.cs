using NeoErp.Core.Models;
using NeoErp.Distribution.Service.Model.Mobile;
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web;

namespace NeoErp.Distribution.Service.Service.Mobile
{
    public interface IMobileService
    {
        #region Fetching Data
        List<LoginResponseModel> Login(LoginModel model, NeoErpCoreEntity dbContext);
        Dictionary<string, string> Logout(LogoutRequestModel model, NeoErpCoreEntity dbContext);
        Dictionary<string, VisitPlanResponseModel> GetVisitPlan(VisitPlanRequestModel model, NeoErpCoreEntity dbContext);
        Dictionary<string, List<EntityResponseModel>> FetchEntity(CommonRequestModel model, NeoErpCoreEntity dbContext);
        Dictionary<string, List<EntityResponseModel>> FetchAllCompanyEntity(NeoErpCoreEntity dbContext);
        List<ItemModel> FetchItems(CommonRequestModel model, NeoErpCoreEntity dbContext);
        QuestionResponseModel FetchAllQuestions(QuestionRequestModel model, NeoErpCoreEntity dbContext);
        List<AreaResponseModel> FetchArea(CommonRequestModel model, NeoErpCoreEntity dbContext);
        Dictionary<string, OutletResponseModel> FetchOutlets(CommonRequestModel model, NeoErpCoreEntity dbContext);
        ClosingStockResponseModel GetEntityItemByBrand(ClosingStockRequestModel model, NeoErpCoreEntity dbContext);
        Dictionary<string, Dictionary<string, MuCodeResponseModel>> FetchMU(CommonRequestModel model, NeoErpCoreEntity dbContext);
        List<TransactionResponseModel> FetchTransactions(TransactionRequestModel model, NeoErpCoreEntity dbContext);
        dynamic FetchSubLedgers(TransactionRequestModel model, NeoErpCoreEntity dbContext);
        Dictionary<string, List<PurchaseOrderResponseModel>> FetchPurchaseOrder(PurchaseOrderRequestModel model, NeoErpCoreEntity dbContext);
        SalesAgeReportResponseModel SalesAgingReport(ReportRequestModel model, NeoErpCoreEntity dbContext);
        Dictionary<string, string> MonthWiseSales(ReportRequestModel model, NeoErpCoreEntity dbContext);
        Dictionary<string, string> AgingReport(ReportRequestModel model, NeoErpCoreEntity dbContext);
        List<Dictionary<string, string>> AgingReportGroup(ReportRequestModel model, NeoErpCoreEntity dbContext);
        DistributorItemResponseModel FetchEntityPartyTypeAndMu(EntityRequestModel model, NeoErpCoreEntity dbContext);
        Dictionary<string, List<EntityResponseModel>> FetchPartyTypeBillingEntity(EntityRequestModel model, NeoErpCoreEntity dbContext);
        List<EntityResponseModel> FetchEntityById(EntityRequestModel model, NeoErpCoreEntity dbContext);
        Dictionary<string, List<EntityResponseModel>> FetchDistributorWithConstraint(CommonRequestModel model, NeoErpCoreEntity dbContext);
        List<SPEntityModel> FetchSpPartyType(VisitPlanRequestModel model, NeoErpCoreEntity dbContext);
        List<SPEntityModel> FetchSpCustomer(VisitPlanRequestModel model, NeoErpCoreEntity dbContext);
        Dictionary<string, List<PurchaseOrderResponseModel>> FetchPOStatus(PurchaseOrderRequestModel model,NeoErpCoreEntity dbContext);
        List<ImageCategoryModel> FetchImageCategory(CommonRequestModel model, NeoErpCoreEntity dbContext);
        List<ResellerEntityModel> FetchResellerEntity(EntityRequestModel model, NeoErpCoreEntity dbContext);
        List<DistributorItemModel> FetchDistributorItems(EntityRequestModel model, NeoErpCoreEntity dbContext);
        List<ResellerGroupModel> GetResellerGroups(CommonRequestModel model, NeoErpCoreEntity dbContext);
        List<ContractModel> GetContracts(CommonRequestModel model, NeoErpCoreEntity dbContext);
        List<AchievementReportResponseModel> GetAchievementData(AchievementReportRequestModel model, NeoErpCoreEntity dbContext);
        List<AchievementReportResponseModel> fetchAchievementReportMonthWise(AchievementReportRequestModel model, NeoErpCoreEntity dbContext);
        Dictionary<string, object> fetchProfileDetails(ProfileDetails model, NeoErpCoreEntity dbContext);
        Dictionary<string, object> SynProfileData(ProfileDetailsModel model, NeoErpCoreEntity dbContext);
        Dictionary<string, object> SynAreaCustomerData(ProfileDetailsModel model, NeoErpCoreEntity dbContext);
        Dictionary<string, object> SynProductQuantityData(ProfileDetailsModel model, NeoErpCoreEntity dbContext);

        List<SalesVsCollectionModel> fetchSalesVsCollectionData(ProfileDetailsModel model, NeoErpCoreEntity dbContext);
        List<ClosingStockDtlModel> fetchLatestClosingStock(ClosingStockModel model, NeoErpCoreEntity dbContext);
        List<SchemeReportResponseModel> fetchSchemeReportData(SchemeReportRequestModel model, NeoErpCoreEntity dbContext);
        List<MoveTransactionResponseModel> FetchMovementTransactions(TransactionRequestModel model, NeoErpCoreEntity dbContext);
        #endregion Fetching Data

        #region Inserting Data
        Dictionary<string, string> UpdateMyLocation(UpdateRequestModel model, NeoErpCoreEntity dbContext);
        Dictionary<string, string> UpdateCurrentLocation(UpdateRequestModel model, NeoErpCoreEntity dbContext);
        bool SaveExtraActivity(UpdateRequestModel model, NeoErpCoreEntity dbContext);
        bool UpdateCustomerLocation(UpdateCustomerRequestModel model, NeoErpCoreEntity dbContext);
        string NewPurchaseOrder(PurchaseOrderModel model, NeoErpCoreEntity dbContext);
        //bool NewCollection(CollectionRequestModel model, NeoErpCoreEntity dbContext);
        bool NewCollection(CollectionRequestModel model, HttpFileCollection Files, NeoErpCoreEntity dbContext);
        bool NewMarketingInformation(InformationSaveModel model, NeoErpCoreEntity dbContext);
        bool NewCompetitorInformation(InformationSaveModel model, NeoErpCoreEntity dbContext);
        bool SaveQuestionaire(QuestionaireSaveModel model, NeoErpCoreEntity dbContext);
        UpdateEntityResponsetModel UpdateDealerStock(UpdateEntityRequestModel model, NeoErpCoreEntity dbContext);
        UpdateEntityResponsetModel UpdateDistributorStock(UpdateEntityRequestModel model, NeoErpCoreEntity dbContext);
        UpdateEntityResponsetModel UpdateResellerStock(UpdateEntityRequestModel model, NeoErpCoreEntity dbContext);
        EntityResponseModel CreateReseller(CreateResellerModel model, HttpFileCollection Files, Dictionary<string, string> descriptions, NeoErpCoreEntity dbContext);
        EntityResponseModel CreateDistributor(CreateDistributorModel model, HttpFileCollection Files, Dictionary<string, string> descriptions, NeoErpCoreEntity dbContext);
        string UpdateReseller(CreateResellerModel model, NeoErpCoreEntity dbContext);
        Dictionary<string, string> UploadEntityMedia(EntityRequestModel model, HttpFileCollection files, Dictionary<string, ImageSaveModel> descriptions, NeoErpCoreEntity dbContext);
        Dictionary<string, string> UploadAttendencePic(EntityRequestModel model, HttpFileCollection Files, Dictionary<string, string> descriptions, NeoErpCoreEntity dbContext);
        //Dictionary<string, string> UploadDistSalesReturnPic(NameValueCollection Form, HttpFileCollection Files, NeoErpCoreEntity dbContext);
        Dictionary<string, string> UploadDistSalesReturnPic(DistributionSalesReturnModel model, HttpFileCollection Files, Dictionary<string, string> descriptions, NeoErpCoreEntity dbContext);
        string SaveScheme(SchemeModel model, NeoErpCoreEntity dbContext);
        #endregion Inserting Data

        #region Sending Mail
        bool SendEODMail(List<UpdateEodUpdate> model, NeoErpCoreEntity dbContext);
        #endregion Sending Mail
    }
}