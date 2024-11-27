using NeoErp.Core;
using NeoErp.Core.Models;
using NeoErp.Distribution.Service.Model.Mobile;
using NeoErp.Distribution.Service.Service.Mobile;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Web.Http;

namespace NeoErp.Distribution.Controllers.MobileAPI
{
    public class MobileDistributionController : ApiController
    {
        private IActionSelector _actionSelector;
        private NeoErpCoreEntity _dbContext;
        private IMobileOfflineService _service;
        
        public MobileDistributionController(IActionSelector actionSelector, NeoErpCoreEntity dbContext, IMobileOfflineService service)
        {
            this._actionSelector = actionSelector;
            this._service = service;
            this._dbContext = dbContext; //same dbcontext is passed as parameter and used allover
        }                                //in mobile service in order to maintain the transaction


        public object API(HttpRequestMessage request)
        {
            CommonModel<object> Output = new CommonModel<object>();
            var Syncs = new Dictionary<string, object>();
            using (var trans = _dbContext.Database.BeginTransaction())
            {
                try
                {

                   if (System.Web.HttpContext.Current.Request.ContentType.Contains("multipart/form-data"))
                    {
                        var ValueForm = System.Web.HttpContext.Current.Request.Form;
                        // Create a dictionary to hold form data
                        var formData = new Dictionary<string, string>();

                        // Iterate through the form collection and add key-value pairs to the dictionary
                        foreach (string key in ValueForm)
                        {
                            formData.Add(key, ValueForm[key]);
                        }
                        // Serialize the dictionary to JSON
                        string json = JsonConvert.SerializeObject(formData);
                        System.Web.HttpFileCollection hfc = System.Web.HttpContext.Current.Request.Files;
                        var data = this._actionSelector.SelectAction(ValueForm, hfc, _dbContext);
                        Output.result = data;
                        Output.response = true;
                        Output.error = "";
                    }
                    //var files = await Request.Content.ReadAsMultipartAsync();
                    //var Json = await Request.Content.ReadAsStringAsync();
                    else
                    {
                        string Json = new StreamReader(System.Web.HttpContext.Current.Request.InputStream).ReadToEnd();
                        var token = JToken.Parse(Json);
                        var action = (string)token.SelectToken("action");

                        #region Offline services
                        if (action.Equals("syncUpload", StringComparison.OrdinalIgnoreCase))
                        {
                            var newToken = token.SelectToken("updateMyLocation");
                            var Locations = (newToken == null || !newToken.HasValues) ? new List<UpdateRequestModel>() : newToken.ToObject<List<UpdateRequestModel>>();

                            newToken = token.SelectToken("updateMyNewLocation");
                            var NewLocations = (newToken == null || !newToken.HasValues) ? new List<UpdateRequestModel>() : newToken.ToObject<List<UpdateRequestModel>>();

                            newToken = token.SelectToken("saveExtraActivity");
                            var ExtraActivities = (newToken == null || !newToken.HasValues) ? new List<UpdateRequestModel>() : newToken.ToObject<List<UpdateRequestModel>>();

                            newToken = token.SelectToken("updateCustomerLocation");
                            var CustomerLocations = (newToken == null || !newToken.HasValues) ? new List<UpdateCustomerRequestModel>() : newToken.ToObject<List<UpdateCustomerRequestModel>>();

                            newToken = token.SelectToken("newPurchaseOrder");
                            var PurchaseOrders = (newToken == null || !newToken.HasValues) ? new List<PurchaseOrderModel>() : newToken.ToObject<List<PurchaseOrderModel>>();

                            newToken = token.SelectToken("cancelPurchaseOrder");
                            var cancledPurchaseOrder = (newToken == null || !newToken.HasValues) ? new List<CancelPurchaseOrderModal>() : newToken.ToObject<List<CancelPurchaseOrderModal>>();

                            newToken = token.SelectToken("newCollection");
                            var NewCollections = (newToken == null || !newToken.HasValues) ? new List<CollectionRequestModel>() : newToken.ToObject<List<CollectionRequestModel>>();

                            newToken = token.SelectToken("newMarketingInformation");
                            var MarketingInfo = (newToken == null || !newToken.HasValues) ? new List<InformationSaveModel>() : newToken.ToObject<List<InformationSaveModel>>();

                            newToken = token.SelectToken("newCompetitorInformation");
                            var Competitorinfo = (newToken == null || !newToken.HasValues) ? new List<InformationSaveModel>() : newToken.ToObject<List<InformationSaveModel>>();

                            newToken = token.SelectToken("saveQuestionnaire");
                            var Questionnaire = (newToken == null || !newToken.HasValues) ? new List<QuestionaireSaveModel>() : newToken.ToObject<List<QuestionaireSaveModel>>();

                            newToken = token.SelectToken("updateDealerStock");
                            var DealerStock = (newToken == null || !newToken.HasValues) ? new List<UpdateEntityRequestModel>() : newToken.ToObject<List<UpdateEntityRequestModel>>();

                            newToken = token.SelectToken("updateDistributorStock");
                            var DistributorStock = (newToken == null || !newToken.HasValues) ? new List<UpdateEntityRequestModel>() : newToken.ToObject<List<UpdateEntityRequestModel>>();

                            newToken = token.SelectToken("updateResellerStock");
                            var ResellerStock = (newToken == null || !newToken.HasValues) ? new List<UpdateEntityRequestModel>() : newToken.ToObject<List<UpdateEntityRequestModel>>();

                            newToken = token.SelectToken("createReseller");
                            var Resellers = (newToken == null || !newToken.HasValues) ? new List<CreateResellerModel>() : newToken.ToObject<List<CreateResellerModel>>();
                            /*sashi*/
                            newToken = token.SelectToken("createDistributor");
                            var Distributors = (newToken == null || !newToken.HasValues) ? new List<CreateDistributorModel>() : newToken.ToObject<List<CreateDistributorModel>>();
                            /*sashi*/
                            newToken = token.SelectToken("schemes");
                            var Schemes = (newToken == null || !newToken.HasValues) ? new List<SchemeModel>() : newToken.ToObject<List<SchemeModel>>();

                            newToken = token.SelectToken("compAnswers");
                            var CompAnswers = (newToken == null || !newToken.HasValues) ? new List<CompAnsModel>() : newToken.ToObject<List<CompAnsModel>>();

                            newToken = token.SelectToken("mobileLogs");
                            var MobileLogs = (newToken == null || !newToken.HasValues) ? new List<MobileLogModel>() : newToken.ToObject<List<MobileLogModel>>();

                            newToken = token.SelectToken("crmTasks");
                            var crmTasks = (newToken == null || !newToken.HasValues) ? new List<CrmModel>() : newToken.ToObject<List<CrmModel>>();

                            //Dist sales Return 
                            newToken = token.SelectToken("salesReturn");
                            var salesReturn = (newToken == null || !newToken.HasValues) ? new List<DistributionSalesReturnModel>() : newToken.ToObject<List<DistributionSalesReturnModel>>();

                            //if last date then remove the existing SYNC_ID
                            var lastDate = (string)token.SelectToken("lastDate");

                            //synced informations
                            var syncedLocations = new Dictionary<string, string>();
                            var syncedNewLocations = new Dictionary<string, string>();
                            var syncedExtraActivities = new Dictionary<string, string>();
                            var syncedCustomerLocations = new Dictionary<string, string>();
                            var syncedPOs = new Dictionary<string, string>();
                            var syncedCollections = new Dictionary<string, string>();
                            var syncedMarkets = new Dictionary<string, string>();
                            var syncedCompetitors = new Dictionary<string, string>();
                            var syncedGeneralQuestionnaires = new Dictionary<string, string>();
                            var syncedTabularQuestionnaires = new Dictionary<string, string>();
                            var syncedDealerStocks = new Dictionary<string, string>();
                            var syncedDistributorStocks = new Dictionary<string, string>();
                            var syncedResellerStocks = new Dictionary<string, string>();
                            var syncedResellers = new Dictionary<string, string>();
                            var syncedResellerDetails = new Dictionary<string, string>();
                            var syncedSchemes = new Dictionary<string, string>();
                            var syncedCompAnswers = new Dictionary<string, string>();
                            var syncedMobileLogs = new Dictionary<string, string>();
                            var syncedCrmTasks = new Dictionary<string, string>();
                            var syncedSalesReturn = new Dictionary<string, string>();
                            var syncedCancelPurchaseOrder = new Dictionary<string, string>();

                            //if (!string.IsNullOrWhiteSpace(lastDate))
                            //{
                            //synced locations
                            var inputSyncIds = Locations.Select(x => x.Sync_Id).ToList();
                            List<string> SyncIds = _service.GetSyncIds("DIST_LOCATION_TRACK", inputSyncIds, _dbContext);
                            if (SyncIds.Count > 0)
                            {
                                syncedLocations = Locations.FindAll(x => SyncIds.Contains(x.Sync_Id))
                                        .ToDictionary(x => x.Sync_Id, x => "Previously Synced");
                                Locations.RemoveAll(x => SyncIds.Contains(x.Sync_Id));
                            }

                            //synced new Locations
                            inputSyncIds = NewLocations.Select(x => x.Sync_Id).ToList();
                            SyncIds = _service.GetSyncIds("DIST_LM_LOCATION_TRACKING", inputSyncIds, _dbContext);
                            if (SyncIds.Count > 0)
                            {
                                syncedNewLocations = NewLocations.FindAll(x => SyncIds.Contains(x.Sync_Id))
                                        .ToDictionary(x => x.Sync_Id, x => "Previously Synced");
                                NewLocations.RemoveAll(x => SyncIds.Contains(x.Sync_Id));
                            }

                            //synced extra activities
                            inputSyncIds = ExtraActivities.Select(x => x.Sync_Id).ToList();
                            SyncIds = _service.GetSyncIds("DIST_EXTRA_ACTIVITY", inputSyncIds, _dbContext);
                            if (SyncIds.Count > 0)
                            {
                                syncedExtraActivities = ExtraActivities.FindAll(x => SyncIds.Contains(x.Sync_Id))
                                        .ToDictionary(x => x.Sync_Id, x => "Previously Synced");
                                ExtraActivities.RemoveAll(x => SyncIds.Contains(x.Sync_Id));
                            }

                            //synced customer locations
                            inputSyncIds = CustomerLocations.Select(x => x.Sync_Id).ToList();
                            SyncIds = _service.GetSyncIds("DIST_DEALER_MASTER", inputSyncIds, _dbContext);
                            SyncIds.AddRange(_service.GetSyncIds("DIST_DISTRIBUTOR_MASTER", inputSyncIds, _dbContext));
                            SyncIds.AddRange(_service.GetSyncIds("DIST_RESELLER_MASTER", inputSyncIds, _dbContext));
                            if (SyncIds.Count > 0)
                            {
                                syncedCustomerLocations = CustomerLocations.FindAll(x => SyncIds.Contains(x.Sync_Id))
                                        .ToDictionary(x => x.Sync_Id, x => "Previously Synced");
                                CustomerLocations.RemoveAll(x => SyncIds.Contains(x.Sync_Id));
                            }

                            //synced purchase orders
                            inputSyncIds = new List<string>();
                            PurchaseOrders.ForEach(
                                po => inputSyncIds.AddRange(po.products.Select(x => x.Sync_Id).ToList())
                                );
                            SyncIds = _service.GetSyncIds("DIST_IP_SSD_PURCHASE_ORDER", inputSyncIds, _dbContext);
                            SyncIds.AddRange(_service.GetSyncIds("DIST_IP_SSR_PURCHASE_ORDER", inputSyncIds, _dbContext));
                            if (SyncIds.Count > 0)
                            {
                                for (int i = 0; i < PurchaseOrders.Count; i++)
                                {
                                    syncedPOs = syncedPOs.Concat(PurchaseOrders[i].products.FindAll(x => SyncIds.Contains(x.Sync_Id))
                                            .ToDictionary(x => x.Sync_Id, x => "Previously Synced")).ToDictionary(x => x.Key, x => x.Value);
                                    PurchaseOrders[i].products.RemoveAll(x => SyncIds.Contains(x.Sync_Id));
                                }
                            }


                            //syncd cancel purchase order 
                            inputSyncIds = new List<string>();
                            cancledPurchaseOrder.ForEach(
                                  cpr => inputSyncIds.AddRange(cpr.products.Select(x=>x.SYNC_ID).ToList())
                                );
                            SyncIds = _service.GetSyncIds("DIST_IP_SSD_PURCHASE_ORDER", inputSyncIds, _dbContext);
                            SyncIds.AddRange(_service.GetSyncIds("DIST_IP_SSR_PURCHASE_ORDER", inputSyncIds, _dbContext));
                            if (SyncIds.Count > 0)
                            {
                                for (int i = 0; i < cancledPurchaseOrder.Count; i++)
                                {
                                    syncedCancelPurchaseOrder = syncedCancelPurchaseOrder.Concat(cancledPurchaseOrder[i].products.FindAll(x => SyncIds.Contains(x.SYNC_ID))
                                            .ToDictionary(x => x.SYNC_ID, x => "Previously Synced")).ToDictionary(x => x.Key, x => x.Value);
                                    //cancledPurchaseOrder[i].products.RemoveAll(x => SyncIds.Contains(x.SYNC_ID));
                                }
                            }

                            //sync dist sales Return 
                            inputSyncIds = new List<string>();
                            salesReturn.ForEach(
                                sr => inputSyncIds.AddRange(sr.products.Select(x => x.SYNC_ID).ToList())
                            );
                            SyncIds = _service.GetSyncIds("DIST_SALES_RETURN", inputSyncIds, _dbContext);
                            SyncIds.AddRange(_service.GetSyncIds("DIST_SALES_RETURN", inputSyncIds, _dbContext));
                            if (SyncIds.Count > 0)
                            {
                                for (int i = 0; i < salesReturn.Count; i++)
                                {
                                    syncedSalesReturn = syncedSalesReturn.Concat(salesReturn[i].products.FindAll(x => SyncIds.Contains(x.SYNC_ID))
                                            .ToDictionary(x => x.SYNC_ID, x => "Previously Synced")).ToDictionary(x => x.Key, x => x.Value);
                                    salesReturn[i].products.RemoveAll(x => SyncIds.Contains(x.SYNC_ID));
                                }
                            }


                            //synced new collections
                            inputSyncIds = NewCollections.Select(x => x.Sync_Id).ToList();
                            SyncIds = _service.GetSyncIds("DIST_COLLECTION", inputSyncIds, _dbContext);
                            if (SyncIds.Count > 0)
                            {
                                syncedCollections = NewCollections.FindAll(x => SyncIds.Contains(x.Sync_Id))
                                        .ToDictionary(x => x.Sync_Id, x => "Previously Synced");
                                NewCollections.RemoveAll(x => SyncIds.Contains(x.Sync_Id));
                            }

                            //synced market informations
                            inputSyncIds = MarketingInfo.Select(x => x.Sync_Id).ToList();
                            SyncIds = _service.GetSyncIds("DIST_MKT_INFO", inputSyncIds, _dbContext);
                            if (SyncIds.Count > 0)
                            {
                                syncedMarkets = MarketingInfo.FindAll(x => SyncIds.Contains(x.Sync_Id))
                                        .ToDictionary(x => x.Sync_Id, x => "Previously Synced");
                                MarketingInfo.RemoveAll(x => SyncIds.Contains(x.Sync_Id));
                            }

                            //synced competitor informations
                            inputSyncIds = Competitorinfo.Select(x => x.Sync_Id).ToList();
                            SyncIds = _service.GetSyncIds("DIST_COMPT_INFO", inputSyncIds, _dbContext);
                            if (SyncIds.Count > 0)
                            {
                                syncedCompetitors = Competitorinfo.FindAll(x => SyncIds.Contains(x.Sync_Id))
                                        .ToDictionary(x => x.Sync_Id, x => "Previously Synced");
                                Competitorinfo.RemoveAll(x => SyncIds.Contains(x.Sync_Id));
                            }

                            //synced general questions
                            inputSyncIds = new List<string>();
                            Questionnaire.ForEach(
                                po => inputSyncIds.AddRange(po.general.Select(x => x.Sync_Id).ToList())
                                );
                            SyncIds = _service.GetSyncIds("DIST_QA_ANSWER", inputSyncIds, _dbContext);
                            if (SyncIds.Count > 0)
                            {
                                for (int i = 0; i < Questionnaire.Count; i++)
                                {
                                    syncedGeneralQuestionnaires = syncedGeneralQuestionnaires.Concat(Questionnaire[i].general.Where(x => SyncIds.Contains(x.Sync_Id))
                                        .ToDictionary(x => x.Sync_Id, x => "Previously Synced")).ToDictionary(x => x.Key, x => x.Value);
                                    //get list of questions that are not in synced list
                                    var newDictionary = Questionnaire[i].general.Where(x => !SyncIds.Contains(x.Sync_Id)).ToList();
                                    Questionnaire[i].general = newDictionary;
                                }
                            }

                            //synced tabular questions
                            inputSyncIds = new List<string>();
                            //get the input sync ids from all items in question list
                            Questionnaire.ForEach(
                                po => inputSyncIds.AddRange(po.tabular.Select(x => x.Sync_Id).ToList())
                                );

                            SyncIds = _service.GetSyncIds("DIST_QA_TAB_CELL_ANSWER", inputSyncIds, _dbContext);
                            if (SyncIds.Count > 0)
                            {
                                for (int i = 0; i < Questionnaire.Count; i++)
                                {
                                    var test = Questionnaire[i].tabular.FindAll(x => SyncIds.Contains(x.Sync_Id));
                                    var dict = test.ToDictionary(x => x.Sync_Id, x => "Previously Synced");
                                    syncedTabularQuestionnaires = syncedTabularQuestionnaires.Concat(dict).ToDictionary(x => x.Key, x => x.Value);
                                    Questionnaire[i].tabular.RemoveAll(x => SyncIds.Contains(x.Sync_Id));
                                }
                            }

                            //synced dealer stocks
                            inputSyncIds = new List<string>();
                            DealerStock.ForEach(
                                po => inputSyncIds.AddRange(po.stock.Select(x => x.Sync_Id).ToList())
                                );
                            SyncIds = _service.GetSyncIds("DIST_DEALER_STOCK", inputSyncIds, _dbContext);
                            if (SyncIds.Count > 0)
                            {
                                syncedDealerStocks = DealerStock.FindAll(x => SyncIds.Contains(x.Sync_Id))
                                        .ToDictionary(x => x.Sync_Id, x => "Previously Synced");
                                for (int i = 0; i < DealerStock.Count; i++)
                                    DealerStock[i].stock.RemoveAll(x => SyncIds.Contains(x.Sync_Id));
                            }

                            //synced distributor stocks
                            inputSyncIds = new List<string>();
                            DistributorStock.ForEach(
                                po => inputSyncIds.AddRange(po.stock.Select(x => x.Sync_Id).ToList())
                                );
                            SyncIds = _service.GetSyncIds("DIST_DISTRIBUTOR_STOCK", inputSyncIds, _dbContext);
                            if (SyncIds.Count > 0)
                            {
                                syncedDistributorStocks = DistributorStock.FindAll(x => SyncIds.Contains(x.Sync_Id))
                                        .ToDictionary(x => x.Sync_Id, x => "Previously Synced");
                                for (int i = 0; i < DistributorStock.Count; i++)
                                    DistributorStock[i].stock.RemoveAll(x => SyncIds.Contains(x.Sync_Id));
                            }

                            //synced reseller stocks
                            inputSyncIds = new List<string>();
                            ResellerStock.ForEach(
                                po => inputSyncIds.AddRange(po.stock.Select(x => x.Sync_Id).ToList())
                                );
                            SyncIds = _service.GetSyncIds("DIST_RESELLER_STOCK", inputSyncIds, _dbContext);
                            if (SyncIds.Count > 0)
                            {
                                syncedResellerStocks = ResellerStock.FindAll(x => SyncIds.Contains(x.Sync_Id))
                                        .ToDictionary(x => x.Sync_Id, x => "Previously Synced");
                                for (int i = 0; i < ResellerStock.Count; i++)
                                    ResellerStock[i].stock.RemoveAll(x => SyncIds.Contains(x.Sync_Id));
                            }

                            //synced resellers
                            inputSyncIds = Resellers.Select(x => x.Sync_Id).ToList();
                            SyncIds = _service.GetSyncIds("DIST_RESELLER_MASTER", inputSyncIds, _dbContext);
                            if (SyncIds.Count > 0)
                            {
                                syncedResellers = Resellers.FindAll(x => SyncIds.Contains(x.Sync_Id))
                                        .ToDictionary(x => x.Sync_Id, x => "Previously Synced");
                                Resellers.RemoveAll(x => SyncIds.Contains(x.Sync_Id));
                            }

                            //synced reseller details
                            Resellers.ForEach(
                                po => inputSyncIds.AddRange(po.contact.Select(x => x.Sync_Id).ToList())
                                );
                            SyncIds = _service.GetSyncIds("DIST_RESELLER_DETAIL", inputSyncIds, _dbContext);
                            if (SyncIds.Count > 0)
                            {
                                for (int i = 0; i < Resellers.Count; i++)
                                {
                                    syncedResellerDetails = syncedResellerDetails.Concat(Resellers[i].contact.FindAll(x => SyncIds.Contains(x.Sync_Id))
                                        .ToDictionary(x => x.Sync_Id, x => "Previously Synced")).ToDictionary(x => x.Key, x => x.Value);
                                    Resellers[i].contact.RemoveAll(x => SyncIds.Contains(x.Sync_Id));
                                }
                            }

                            //synced Schemes
                            inputSyncIds = Schemes.Select(x => x.Sync_Id).ToList();
                            SyncIds = _service.GetSyncIds("BRD_SCHEME", inputSyncIds, _dbContext);
                            if (SyncIds.Count > 0)
                            {
                                syncedSchemes = Schemes.FindAll(x => SyncIds.Contains(x.Sync_Id))
                                        .ToDictionary(x => x.Sync_Id, x => "Previously Synced");
                                Schemes.RemoveAll(x => SyncIds.Contains(x.Sync_Id));
                            }

                            //synced compAnswers
                            inputSyncIds = CompAnswers.Select(x => x.Sync_Id).ToList();
                            SyncIds = _service.GetSyncIds("DIST_COMP_QA", inputSyncIds, _dbContext);
                            if (SyncIds.Count > 0)
                            {
                                syncedCompAnswers = CompAnswers.FindAll(x => SyncIds.Contains(x.Sync_Id))
                                        .ToDictionary(x => x.Sync_Id, x => "Previously Synced");
                                CompAnswers.RemoveAll(x => SyncIds.Contains(x.Sync_Id));
                            }

                            //synced mobileLogs
                            inputSyncIds = MobileLogs.Select(x => x.Sync_Id).ToList();
                            SyncIds = _service.GetSyncIds("DIST_USER_DEVICE_LOG", inputSyncIds, _dbContext);
                            if (SyncIds.Count > 0)
                            {
                                syncedMobileLogs = MobileLogs.FindAll(x => SyncIds.Contains(x.Sync_Id))
                                        .ToDictionary(x => x.Sync_Id, x => "Previously Synced");
                                MobileLogs.RemoveAll(x => SyncIds.Contains(x.Sync_Id));
                            }

                            //}

                            //location updates
                            var locationRes = new Dictionary<string, string>();
                            foreach (var item in Locations)
                            {
                                try
                                {
                                    var result = this._service.UpdateMyLocation(item, _dbContext);
                                    locationRes = locationRes.Concat(result).ToDictionary(x => x.Key, x => x.Value);
                                }
                                catch (Exception ex) { }
                            }

                            //new location updates
                            var currentLocationRes = new Dictionary<string, string>();
                            foreach (var item in NewLocations)
                            {
                                try
                                {
                                    var result = this._service.UpdateCurrentLocation(item, _dbContext);
                                    currentLocationRes = currentLocationRes.Concat(result).ToDictionary(x => x.Key, x => x.Value);
                                }
                                catch (Exception ex) { }
                            }

                            //extra activities updates
                            var extraActivitiesRes = new Dictionary<string, string>();
                            foreach (var item in ExtraActivities)
                            {
                                try
                                {
                                    var result = this._service.SaveExtraActivity(item, _dbContext);
                                    extraActivitiesRes = extraActivitiesRes.Concat(result).ToDictionary(x => x.Key, x => x.Value);
                                }
                                catch (Exception ex) { }
                            }

                            //customer locations updates
                            var customerLocationRes = new Dictionary<string, string>();
                            foreach (var item in CustomerLocations)
                            {
                                try
                                {
                                    var result = this._service.UpdateCustomerLocation(item, _dbContext);
                                    customerLocationRes = customerLocationRes.Concat(result).ToDictionary(x => x.Key, x => x.Value);
                                }
                                catch (Exception ex) { }
                            }

                            //purchase orders updates
                            var purchaseOrderRes = new Dictionary<string, string>();
                            foreach (var item in PurchaseOrders)
                            {
                                try
                                {
                                    var result = this._service.NewPurchaseOrder(item, _dbContext);
                                    purchaseOrderRes = purchaseOrderRes.Concat(result).ToDictionary(x => x.Key, x => x.Value);
                                }
                                catch (Exception ex) { }
                            }

                            //cancel purchase orders updates
                            var cancelPurchaseOrderRes = new Dictionary<string, string>();  
                            foreach (var item in cancledPurchaseOrder)
                            {
                                try
                                {
                                    var result = this._service.CancelPurchaseOrder(item, _dbContext);
                                    cancelPurchaseOrderRes = cancelPurchaseOrderRes.Concat(result).ToDictionary(x => x.Key, x => x.Value);
                                }
                                catch (Exception ex) { }
                            }

                            //Dist sales return
                            var distSalesReturnRes = new Dictionary<string, string>();
                            foreach (var sr in salesReturn)
                            {
                                try
                                {
                                    var salesReturnResult = _service.SaveDistSalesReturn(sr, _dbContext);
                                    distSalesReturnRes = distSalesReturnRes.Concat(salesReturnResult).ToDictionary(x => x.Key, x => x.Value);
                                }
                                catch (Exception ex) { throw ex; }
                            }

                            //new collections
                            var newCollectionsRes = new Dictionary<string, string>();
                            foreach (var item in NewCollections)
                            {
                                try
                                {
                                    var result = this._service.NewCollection(item, _dbContext);
                                    newCollectionsRes = newCollectionsRes.Concat(result).ToDictionary(x => x.Key, x => x.Value);
                                }
                                catch (Exception ex) { }
                            }

                            //market informations
                            var mktInformationRes = new Dictionary<string, string>();
                            foreach (var item in MarketingInfo)
                            {
                                try
                                {
                                    var result = this._service.NewMarketingInformation(item, _dbContext);
                                    mktInformationRes = mktInformationRes.Concat(result).ToDictionary(x => x.Key, x => x.Value);
                                }
                                catch (Exception ex) { }
                            }

                            //competitors informations
                            var competitorInfoRes = new Dictionary<string, string>();
                            foreach (var item in Competitorinfo)
                            {
                                try
                                {
                                    var result = this._service.NewCompetitorInformation(item, _dbContext);
                                    competitorInfoRes = competitorInfoRes.Concat(result).ToDictionary(x => x.Key, x => x.Value);
                                }
                                catch (Exception ex) { }
                            }

                            //questionnaire
                            var questionnaireRes = new Dictionary<string, Dictionary<string, string>>();
                            foreach (var item in Questionnaire)
                            {
                                try
                                {
                                    var result = this._service.SaveQuestionaire(item, _dbContext);
                                    questionnaireRes = questionnaireRes.Concat(result).ToDictionary(x => x.Key, x => x.Value);
                                }
                                catch (Exception ex) { }
                            }

                            //update dealer stocks
                            var dealerStockRes = new Dictionary<string, string>();
                            foreach (var item in DealerStock)
                            {
                                try
                                {
                                    var result = this._service.UpdateDealerStock(item, _dbContext);
                                    dealerStockRes = dealerStockRes.Concat(result).ToDictionary(x => x.Key, x => x.Value);
                                }
                                catch (Exception ex) { }
                            }

                            //update distributor stocks
                            var distributorStockRes = new Dictionary<string, string>();
                            foreach (var item in DistributorStock)
                            {
                                try
                                {
                                    var result = this._service.UpdateDistributorStock(item, _dbContext);
                                    distributorStockRes = distributorStockRes.Concat(result).ToDictionary(x => x.Key, x => x.Value);
                                }
                                catch (Exception ex) { }
                            }
                            //update reseller stocks
                            var resellerStockRes = new Dictionary<string, string>();
                            foreach (var item in ResellerStock)
                            {
                                try
                                {
                                    var result = this._service.UpdateResellerStock(item, _dbContext);
                                    resellerStockRes = resellerStockRes.Concat(result).ToDictionary(x => x.Key, x => x.Value);
                                }
                                catch (Exception ex) { }
                            }

                            //update dealer stocks
                            var createResellerRes = new Dictionary<string, string>();
                            foreach (var item in Resellers)
                            {
                                try
                                {
                                    var result = this._service.CreateReseller(item, _dbContext);
                                    createResellerRes = createResellerRes.Concat(result).ToDictionary(x => x.Key, x => x.Value);
                                }
                                catch (Exception ex)
                                {
                                    if (ex.Message == "EXISTS")
                                    {
                                        var result = new Dictionary<string, string>();
                                        result.Add(item.Sync_Id, "Reseller already exists with same pan number");
                                        createResellerRes = createResellerRes.Concat(result).ToDictionary(x => x.Key, x => x.Value);
                                    }
                                }
                            }
                            /*sashi*/
                            // Add Distributor
                            //update dealer stocks
                            var createDistributorDis = new Dictionary<string, string>();
                            foreach (var item in Distributors)
                            {
                                try
                                {
                                    var result = this._service.CreateDistributor(item, _dbContext);
                                    createDistributorDis = createDistributorDis.Concat(result).ToDictionary(x => x.Key, x => x.Value);
                                }
                                catch (Exception ex)
                                {
                                    if (ex.Message == "EXISTS")
                                    {
                                        var result = new Dictionary<string, string>();
                                        result.Add(item.Sync_Id, "Distributor already exists with same pan number");
                                        createDistributorDis = createDistributorDis.Concat(result).ToDictionary(x => x.Key, x => x.Value);
                                    }
                                }
                            }

                            /*sashi*/
                            //Schemes
                            var schemeRes = new Dictionary<string, string>();
                            foreach (var item in Schemes)
                            {
                                try
                                {
                                    var result = this._service.SaveScheme(item, _dbContext);
                                    schemeRes = schemeRes.Concat(result).ToDictionary(x => x.Key, x => x.Value);
                                }
                                catch (Exception ex) { }
                            }

                            //competitor Items
                            var CompAnsRes = new Dictionary<string, string>();
                            foreach (var item in CompAnswers)
                            {
                                try
                                {
                                    var result = this._service.SaveCompAns(item, _dbContext);
                                    CompAnsRes = CompAnsRes.Concat(result).ToDictionary(x => x.Key, x => x.Value);
                                }
                                catch (Exception ex) { }
                            }

                            //mobile logs
                            var MobileRes = new Dictionary<string, string>();
                            foreach (var item in MobileLogs)
                            {
                                try
                                {
                                    var result = this._service.SaveDeviceLog(item, _dbContext);
                                    MobileRes = MobileRes.Concat(result).ToDictionary(x => x.Key, x => x.Value);
                                }
                                catch (Exception ex) { }
                            }

                            //crm tasks
                            var crmres = new Dictionary<string, string>();
                            foreach (var item in crmTasks)
                            {
                                try
                                {
                                    var result = this._service.SaveCrmTask(item, _dbContext);
                                    crmres = crmres.Concat(result).ToDictionary(x => x.Key, x => x.Value);
                                }
                                catch (Exception ex) { }
                            }

                            Syncs["updateMyLocation"] = locationRes.Concat(syncedLocations);
                            Syncs["updateMyNewLocation"] = currentLocationRes.Concat(syncedNewLocations);
                            Syncs["saveExtraActivity"] = extraActivitiesRes.Concat(syncedExtraActivities);
                            Syncs["updateCustomerLocation"] = customerLocationRes.Concat(syncedCustomerLocations);
                            Syncs["newPurchaseOrder"] = purchaseOrderRes.Concat(syncedPOs);
                            Syncs["newCollection"] = newCollectionsRes.Concat(syncedCollections);
                            Syncs["newMarketingInformation"] = mktInformationRes.Concat(syncedMarkets);
                            Syncs["newCompetitorInformation"] = competitorInfoRes.Concat(syncedCompetitors);

                            if (questionnaireRes.Count == 0)
                            {
                                questionnaireRes["general"] = (syncedGeneralQuestionnaires);
                                questionnaireRes["tabular"] = (syncedTabularQuestionnaires);
                            }
                            questionnaireRes["general"] = questionnaireRes["general"].Concat(syncedGeneralQuestionnaires).ToDictionary(x => x.Key, x => x.Value);
                            questionnaireRes["tabular"] = questionnaireRes["tabular"].Concat(syncedTabularQuestionnaires).ToDictionary(x => x.Key, x => x.Value);

                            //to maintain the same response format as List<Key,Value>
                            var generalList = questionnaireRes["general"].Select(x => new { Key = x.Key, Value = x.Value }).ToList();
                            var tabularList = questionnaireRes["tabular"].Select(x => new { Key = x.Key, Value = x.Value }).ToList();
                            var questionsFinal = new Dictionary<string, object>();
                            questionsFinal["general"] = generalList;
                            questionsFinal["tabular"] = tabularList;
                            //to maintain the same response format as List<Key,Value>
                            Syncs["saveQuestionnaire"] = questionsFinal;

                            Syncs["updateDealerStock"] = dealerStockRes.Concat(syncedDealerStocks);
                            Syncs["updateDistributorStock"] = distributorStockRes.Concat(syncedDistributorStocks);
                            Syncs["updateResellerStock"] = resellerStockRes.Concat(syncedResellerStocks);
                            Syncs["createReseller"] = createResellerRes.Concat(syncedResellers);
                            Syncs["schemes"] = schemeRes.Concat(syncedSchemes);
                            Syncs["compAnswers"] = CompAnsRes.Concat(syncedCompAnswers);
                            Syncs["mobileLogs"] = MobileRes.Concat(syncedMobileLogs);
                            Syncs["CrmTasks"] = crmres.ToList(); //no use of sync ids in the crm table. Also, only update operation is carried out.
                            Syncs["DistSalesReturn"] = distSalesReturnRes.ToList();
                            Syncs["CancelPurchaseOrder"] = cancelPurchaseOrderRes.ToList();


                            Output.error = "";
                            Output.response = true;
                            Output.result = Syncs;
                        }
                        else if (action.Equals("syncDownload", StringComparison.OrdinalIgnoreCase))
                        {
                            VisitPlanRequestModel model = token.ToObject<VisitPlanRequestModel>();
                            var data = new Dictionary<string, object>();
                            if (!model.entities.Any())
                                data = _service.SyncData(model, _dbContext);
                            else
                                data = _service.SyncDataTopic(model, _dbContext);
                            Output.result = data;
                            Output.response = true;
                            Output.error = "";
                        }
                        #endregion Offline services
                        //#region Profile Details
                        //else if (action.Equals("profileDetails", StringComparison.OrdinalIgnoreCase))
                        //{
                        //    ProfileDetailsModel model = token.ToObject<ProfileDetailsModel>();
                        //    var data = new Dictionary<string, object>();
                        //    if (model == null)
                        //    {
                        //        throw new ArgumentException("Model is null or empty");
                        //    }
                        //    else
                        //    {
                        //        data = _service.SynProfileData(model, _dbContext);
                        //    }
                        //    Output.result = data;
                        //    Output.response = true;
                        //    Output.error = "";
                        //}
                        //#endregion Profile Details
                        else //go for online processing
                        {
                            if (token is JObject)
                            {
                                var op = this._actionSelector.SelectAction(token, _dbContext);
                                Output.result = op;
                                Output.response = true;
                                Output.error = "";
                            }
                            else if (token is JArray)
                            {
                                var JsonArray = JsonConvert.DeserializeObject<List<object>>(Json);
                                var Outputs = new List<object>();
                                foreach (var item in JsonArray)
                                {
                                    JToken Arraytoken = (JToken)item;
                                    var op = this._actionSelector.SelectAction(Arraytoken, _dbContext);
                                    Outputs.Add(op);
                                }
                                Output.result = Outputs.LastOrDefault();
                                Output.response = true;
                                Output.error = "";
                            }
                        }
                    }
                    trans.Commit();
                }
                catch (JsonReaderException ex)
                {
                    Output.result = new List<object>();
                    Output.response = false;
                    Output.error = "Invalid JSON format";
                }
                catch (Exception ex)
                {
                    if (ex.Message == "IMEI_REG_ERROR")
                    {
                        Output.error = "Device is not registered. Requested for registration";
                        trans.Commit();
                    }
                    else
                    {
                        Output.error = ex.Message;
                        trans.Rollback();
                    }
                    Output.result = new List<object>();
                    Output.response = false;
                }
            }
            return Output;
        }
    }
}
