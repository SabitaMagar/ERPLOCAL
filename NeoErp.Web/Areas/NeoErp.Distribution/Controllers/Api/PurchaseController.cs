using NeoErp.Core;
using NeoErp.Core.Helpers;
using NeoErp.Core.Models;
using NeoErp.Distribution.Service;
using NeoErp.Distribution.Service.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;

namespace NeoErp.Distribution.Controllers.Api
{
    public class DistributionPurchaseController : ApiController
    {
        public IDistributionService _distributionService { get; set; }
        private NeoErpCoreEntity _objectEntity;
        private IWorkContext _workContext;

        public DistributionPurchaseController(IDistributionService distributionService, NeoErpCoreEntity objectEntity, IWorkContext workContext)
        {
            this._distributionService = distributionService;
            this._objectEntity = objectEntity;
            this._workContext = workContext;
        }

        [System.Web.Http.HttpPost]
        public List<PurchaseOrderModel> GetDistributionPurchaseOrder(filterOption model, string requestStatus = "Active")
        {
            var items = _distributionService.GetPurchaseOrder(model.ReportFilters, _workContext.CurrentUserinformation, requestStatus);
            //var models = new List<PurchaseOrderModel>();
            //foreach (var item in items)
            //{
            //    if (models.Any(x => x.ORDER_NO == item.ORDER_NO))
            //        continue;
            //    var modelPurchase = new PurchaseOrderModel();
            //    modelPurchase = item;

            //    modelPurchase.GrantTotalAmount = items.Where(x => x.ORDER_NO == item.ORDER_NO).Sum(x => x.TOTAL_PRICE);
            //    modelPurchase.GRAND_APPROVE_QUENTITY = items.Where(x => x.ORDER_NO == item.ORDER_NO).Sum(x => (x.QUANTITY * x.CONVERSION_FACTOR));

            //    item.APPROVED_FLAGBOOL = item.APPROVED_FLAG == "N" ? false : true;
            //    item.APPROVEQTY = item.QUANTITY;
            //    models.Add(modelPurchase);
            //}
            return items;

            //  return data;
        }
        [System.Web.Http.HttpPost]
        public List<PurchaseOrderModel> GetDealerSalesOrder(filterOption model, string requestStatus = "Active")
        {
            var items = _distributionService.GetDealerSalesOrder(model.ReportFilters, _workContext.CurrentUserinformation, requestStatus);
            return items;
        }


        [System.Web.Http.HttpPost]
        public List<PurchaseOrderModel> GetDistributionPurchaseOrderSummary(filterOption model, string requestStatus = "Active")
        {
            var items = _distributionService.GetPurchaseOrderSummary(model.ReportFilters, _workContext.CurrentUserinformation, requestStatus);

            return items;

            //  return data;
        }


        [System.Web.Http.HttpPost]
        public List<DistSalesReturnViewModel> GetDistributionSalesReturn(filterOption filterOption, string companyCode = "01")
        {
            var distSalesReturnData = _distributionService.GetDistributionSalesReturn(filterOption.ReportFilters, companyCode);
            return distSalesReturnData;
        }

        [System.Web.Http.HttpPost]
        public List<DistSalesReturnItemViewModel> GetDistributionSalesReturnDetail(filterOption filterOption, string companyCode, string returnNo)
        {
            var items = _distributionService.GetSalesReturnDetail(filterOption.ReportFilters, companyCode, returnNo);
            return items;
        }

        [System.Web.Http.HttpPost]
        public DSRResponse UpdateDistributionSalesReturn(string returnNo, string updateFlag)
        {
            var updateRes = _distributionService.UpdateDistSalesReturn(returnNo, updateFlag);
            return updateRes;
        }


        [System.Web.Http.HttpPost]
        public List<DistSalesReturnViewModel> FilterDistributionSalesReturn(string value)
        {
            var result = _distributionService.FilterDistSalesReturn(value);
            return result;
        }

        [System.Web.Http.HttpPost]
        public List<PurchaseOrderModel> GetDistributionPurchaseOrderDaily(filterOption model)
        {

            var items = _distributionService.GetPurchaseOrderDaily(model.ReportFilters, _workContext.CurrentUserinformation);
            return items;
        }

        [System.Web.Http.HttpGet]
        public HttpResponseMessage GetItems(string companyCode = "", string branchCode = "", string distributorCode = "")
        {
            companyCode = companyCode == "" ? _workContext.CurrentUserinformation.company_code : companyCode;
            branchCode = branchCode == "" ? _workContext.CurrentUserinformation.branch_code : branchCode;
            distributorCode = _workContext.CurrentUserinformation.LoginType == "Distributor" ? _workContext.CurrentUserinformation.DistributerNo : distributorCode;
            try
            {
                var items = _distributionService.FetchItems(companyCode, branchCode, distributorCode);
                return Request.CreateResponse(HttpStatusCode.OK, items);
            }
            catch (Exception ex)
            {
                return Request.CreateResponse(HttpStatusCode.OK, new { MESSAGE = "Something went wrong!!!", TYPE = "warning" });
            }
        }
        [System.Web.Http.HttpGet]
        public HttpResponseMessage GetAllItems(string type = "D")
        {
            try
            {
                var items = _distributionService.FetchAllItems(_workContext.CurrentUserinformation, type);
                return Request.CreateResponse(HttpStatusCode.OK, items);
            }
            catch (Exception ex)
            {
                return Request.CreateResponse(HttpStatusCode.OK, new { MESSAGE = "Something went wrong!!!", TYPE = "warning" });
            }
        }
        [System.Web.Http.HttpPost]
        public HttpResponseMessage UpdatePurchaseOrder(PurchaseOrderModel model)
        {
            var user = _workContext.CurrentUserinformation.User_id;
            var result = _distributionService.UpdatePO(model, user.ToString());
            if (result)
                return Request.CreateResponse(HttpStatusCode.OK, new { MESSAGE = "Success", TYPE = "success" });
            else
                return Request.CreateResponse(HttpStatusCode.OK, new { MESSAGE = "Something went wrong!!!", TYPE = "error" });
        }
        [System.Web.Http.HttpPost]
        public HttpResponseMessage UpdateDealerSalesOrder(PurchaseOrderModel model)
        {
            var user = _workContext.CurrentUserinformation.User_id;
            var result = _distributionService.UpdateDSO(model, user.ToString());
            if (result)
                return Request.CreateResponse(HttpStatusCode.OK, new { MESSAGE = "Success", TYPE = "success" });
            else
                return Request.CreateResponse(HttpStatusCode.OK, new { MESSAGE = "Something went wrong!!!", TYPE = "error" });
        }
        [System.Web.Http.HttpPost]
        public HttpResponseMessage CancelDealerSalesOrder(PurchaseOrderModel model)
        {
            var user = _workContext.CurrentUserinformation.User_id;
            var result = _distributionService.CancelDSO(model, user.ToString());
            if (result)
                return Request.CreateResponse(HttpStatusCode.OK, new { MESSAGE = "Success", TYPE = "success" });
            else
                return Request.CreateResponse(HttpStatusCode.OK, new { MESSAGE = "Something went wrong!!!", TYPE = "error" });
        }
        [System.Web.Http.HttpPost]
        public CreditDaysBalanceModel GetCreditDaysBalance(string companyCode, string subCode, string creditLimit = "", string daysLimit = "")
        {
            return _distributionService.GetCreditDaysBalance(companyCode, subCode, creditLimit, daysLimit);
        }



        //public HttpResponseMessage CheckCreditLimit(string CUSTOMERCODE,decimal totalqty=0,decimal totalamount=0)
        //{
        //    var query = "select credit_limit  from sa_customer_setup where customer_code='"+ CUSTOMERCODE + "' and company_code='01'";
        //    var query = "select credit_limit  from sa_customer_setup where customer_code='"+ CUSTOMERCODE + "' and company_code='01'";
        //    return Request.CreateResponse(HttpStatusCode.OK, new { MESSAGE = "Voucher Successfully retrived", STATUS_CODE = (int)HttpStatusCode.NotFound });
        //}

        //AA
        [System.Web.Http.HttpPost]
        public List<PurchaseOrderModel> GetDistributionPurchaseOrderDetail(string companyCode, string Orderno, string ORDER_ENTITY, string requestStatus = "Active")
        {
            var items = _distributionService.GetPurchaseOrderDetail(companyCode, Orderno, ORDER_ENTITY, requestStatus);
            return items;
        }
        [System.Web.Http.HttpPost]
        public HttpResponseMessage UpdateDistributionPurchaseOrderDetail([System.Web.Http.ModelBinding.ModelBinder] List<PurchaseOrderModel> model)
        {
            var rowAffected = 0;
            var masterRowaffected = 0;
            var orderno = 0M;
            var ORDER_ENTITY = "";
            DateTime orderdata = DateTime.Now;
            string formCode = string.Empty;
            using (var transaction = _objectEntity.Database.BeginTransaction())
            {
                try
                {

                    var modelrequest = Request.Content.ReadAsAsync<List<PurchaseOrderModel>>().Result;
                    if (modelrequest.Count <= 0)
                        return Request.CreateResponse(HttpStatusCode.OK, new { MESSAGE = "Purchase Order Approve is not selected.", STATUS_CODE = (int)HttpStatusCode.NotFound });
                    else
                        formCode = modelrequest.FirstOrDefault().FORM_CODE;
                    var serialnumber = 1;

                    var voucherCode = "select  FN_NEW_VOUCHER_NO('" + modelrequest.FirstOrDefault().COMPANY_CODE + "','" + formCode + "',TRUNC(sysdate),'SA_SALES_ORDER') from dual";
                    var data = _objectEntity.SqlQuery<string>(voucherCode).FirstOrDefault();
                    if (data == null)
                        return Request.CreateResponse(HttpStatusCode.OK, new { MESSAGE = "Sales Order No Not Generated.Please Once Again", STATUS_CODE = (int)HttpStatusCode.NotFound });
                    //var sumamount = modelrequest.Where(x => x.APPROVED_FLAGBOOL == true).Sum(x => x.TOTAL_PRICE);
                    foreach (var request in modelrequest)
                    {
                        orderno = request.ORDER_NO;
                        orderdata = request.ORDER_DATE ?? DateTime.Now;
                        if (request.APPROVED_FLAGBOOL ?? false)
                        {
                            if (request.UNIT_PRICE <= 0)
                                request.UNIT_PRICE = 0;

                            if (request.UNIT_PRICE == null)
                                request.UNIT_PRICE = 0;
                            request.ApprovedAmount = request.APPROVEQTY * request.UNIT_PRICE;
                            var query = string.Format(@"Insert into SA_SALES_ORDER
                                                           (ORDER_NO, ORDER_DATE, CUSTOMER_CODE, SERIAL_NO, ITEM_CODE, MU_CODE, QUANTITY, UNIT_PRICE, TOTAL_PRICE, CALC_QUANTITY, CALC_UNIT_PRICE, CALC_TOTAL_PRICE, FORM_CODE, COMPANY_CODE, BRANCH_CODE, CREATED_BY, CREATED_DATE, DELETED_FLAG, DELIVERY_DATE, CURRENCY_CODE, EXCHANGE_RATE, TRACKING_NO, STOCK_BLOCK_FLAG,MODIFY_BY,MODIFY_DATE,PARTY_TYPE_CODE)
                                                         Values
                                                           ('" + data + @"', to_date('" + request.ORDER_DATE + "','MM/DD/YYYY HH:MI:SS AM'), '" + request.CUSTOMER_CODE + @"'," + serialnumber + @",
                                                            '" + request.ITEM_CODE + @"', '" + request.MU_CODE + @"'," + request.APPROVEQTY + @" , " + request.UNIT_PRICE + @", " + request.ApprovedAmount + @",
                                                           " + request.APPROVEQTY + @" , " + request.UNIT_PRICE + @", " + request.ApprovedAmount + @",
                                                            '" + request.FORM_CODE + @"', '" + request.COMPANY_CODE + @"', '" + request.BRANCH_CODE + @"', UPPER('" + _workContext.CurrentUserinformation.login_code + @"'), sysdate,
                                                            'N', TO_DATE(sysdate), 'NRS', 1,
                                                            '0', 'N',UPPER('" + _workContext.CurrentUserinformation.login_code + @"'), to_date('" + request.ORDER_DATE + @"','MM/DD/YYYY HH:MI:SS AM'),'" + request.PARTY_TYPE_CODE + "')");

                            rowAffected = _objectEntity.ExecuteSqlCommand(query);
                            //if (rowAffected > 0)
                            //{
                            //    //var queryforupdate = string.Format(@"UPDATE dist_ip_ssd_purchase_order  SET APPROVED_FLAG = 'Y',REJECT_FLAG='N',SALES_ORDER_NO = '{2}',APPROVE_QTY = {3}, APPROVE_AMT = {4}  WHERE ORDER_NO = {0} and ITEM_CODE = {1}", request.ORDER_NO, request.ITEM_CODE, data, request.APPROVEQTY, request.ApprovedAmount);
                            //    //var update = _objectEntity.ExecuteSqlCommand(queryforupdate);
                            //    if (request.QUANTITY != request.APPROVEQTY)
                            //    {

                            //        var queryforhalfupdate = string.Format(@"UPDATE dist_ip_ssd_purchase_order  SET APPROVED_FLAG = 'N',REJECT_FLAG='N',SALES_ORDER_NO = '{2}',APPROVE_QTY = {3}, APPROVE_AMT = {4} ,QUANTITY = {5} WHERE ORDER_NO = {0} and ITEM_CODE = {1}", request.ORDER_NO, request.ITEM_CODE, data, request.APPROVEQTY, request.ApprovedAmount, request.QUANTITY - request.APPROVEQTY);
                            //        var update = _objectEntity.ExecuteSqlCommand(queryforhalfupdate);


                            //        //var queryforremupdate = string.Format(@"UPDATE dist_ip_ssd_purchase_order  SET APPROVED_FLAG = 'N',DELETED_FLAG='N',REJECT_FLAG='N',QUANTITY = {2}  WHERE ORDER_NO = {0} and ITEM_CODE = {1}", request.ORDER_NO, request.ITEM_CODE, request.QUANTITY - request.APPROVEQTY);
                            //        //var updateRem = _objectEntity.ExecuteSqlCommand(queryforremupdate);



                            //        //var insertRemaining = $@"INSERT INTO DIST_IP_SSD_PURCHASE_ORDER(ORDER_NO,ORDER_DATE,CUSTOMER_CODE,ITEM_CODE,CREATED_BY,CREATED_DATE,QUANTITY,
                            //        //                                                                COMPANY_CODE,BRANCH_CODE,APPROVED_FLAG,DISPATCH_FLAG,ACKNOWLEDGE_FLAG,
                            //        //                                                                REJECT_FLAG,DELETED_FLAG,ORDER_FROM,MODIFY_BY,MODIFY_DATE)
                            //        //                         VALUES('{request.ORDER_NO}',to_date('{request.ORDER_DATE.Value.ToShortDateString()}','MM/DD/YYYY'),'{request.CUSTOMER_CODE}','{request.ITEM_CODE}','{request.CREATED_BY}',to_date('{request.CREATED_DATE.Value.ToShortDateString()}','MM/DD/YYYY')
                            //        //                                 ,'{request.QUANTITY - request.APPROVEQTY}','{request.COMPANY_CODE}','{request.BRANCH_CODE}','N','N','N','N','N','H','{_workContext.CurrentUserinformation.login_code}',SYSDATE)";
                            //        //_objectEntity.ExecuteSqlCommand(insertRemaining);
                            //    }
                            //    else
                            //    {
                            //        var queryforupdate = string.Format(@"UPDATE dist_ip_ssd_purchase_order  SET APPROVED_FLAG = 'N',REJECT_FLAG='N',SALES_ORDER_NO = '{2}',APPROVE_QTY = {3}, APPROVE_AMT = {4}  WHERE ORDER_NO = {0} and ITEM_CODE = {1}", request.ORDER_NO, request.ITEM_CODE, data, request.APPROVEQTY, request.ApprovedAmount);
                            //        var update = _objectEntity.ExecuteSqlCommand(queryforupdate);
                            //    }
                            //}
                            serialnumber++;
                        }
                    }
                    if (rowAffected > 0)
                    {
                        var sumamount = modelrequest.Where(x => x.APPROVED_FLAGBOOL == true).Sum(x => x.ApprovedAmount);
                        var masterQuery = @"Insert into MASTER_TRANSACTION
                                               (VOUCHER_NO, VOUCHER_AMOUNT, FORM_CODE, CHECKED_BY, AUTHORISED_BY, POSTED_BY, COMPANY_CODE, BRANCH_CODE, CREATED_BY, CREATED_DATE, DELETED_FLAG, VOUCHER_DATE, CURRENCY_CODE, EXCHANGE_RATE, PRINT_COUNT,  PRINT_FLAG)
                                             Values
                                               ('" + data + @"', " + sumamount + @", '" + modelrequest.FirstOrDefault().FORM_CODE + @"', '', '',
                                                '', '" + modelrequest.FirstOrDefault().COMPANY_CODE + "', '" + modelrequest.FirstOrDefault().BRANCH_CODE + "', UPPER('" + _workContext.CurrentUserinformation.login_code + @"'),
                                                sysdate, 'N',  to_date('" + orderdata + @"','MM/DD/YYYY HH:MI:SS AM'),
                                                'NRS', 1, 0,'N')";
                        masterRowaffected = _objectEntity.ExecuteSqlCommand(masterQuery);
                    }

                    var dataforUpdate = _distributionService.GetPurchaseOrderDetail("01", orderno.ToString(), ORDER_ENTITY, "Active");
                    int count = 0;
                    var apprQty = 0;
                    var apprAmt = 0;
                    foreach (var dataupdate in dataforUpdate)
                    {
                        if (modelrequest.Any(x => x.ORDER_NO == dataupdate.ORDER_NO && x.ITEM_CODE == dataupdate.ITEM_CODE))
                        {
                            //if (count == 0) { apprQty = Convert.ToInt32(modelrequest[count].APPROVEQTY); apprAmt = Convert.ToInt32(modelrequest[count].ApprovedAmount); } else
                            //{
                            apprQty = Convert.ToInt32(dataupdate.APPROVEQTY) + Convert.ToInt32(modelrequest[count].APPROVEQTY);
                            apprAmt = Convert.ToInt32(modelrequest[count].ApprovedAmount);
                            if (apprAmt <= 0)
                            {
                                apprAmt = Convert.ToInt32(modelrequest[count].NEW_TOTAL_AMOUNT);
                            }
                            //}

                            //apprAmt = dataupdate.APPROVEQTY + modelrequest[count].APPROVEQTY;
                            var partialUpdate = $@"UPDATE DIST_IP_SSD_PURCHASE_ORDER SET REJECT_FLAG='N',APPROVE_QTY = {apprQty}, APPROVE_AMT = {apprAmt}, QUANTITY={modelrequest[count].QUANTITY - modelrequest[count].APPROVEQTY},SALES_ORDER_NO='{data}' WHERE ORDER_NO = {modelrequest[count].ORDER_NO} and ITEM_CODE = {modelrequest[count].ITEM_CODE}";
                            var Pupdate = _objectEntity.ExecuteSqlCommand(partialUpdate);
                            count++;
                        }
                        else
                        {
                            var queryforupdate = string.Format(@"UPDATE dist_ip_ssd_purchase_order  SET REJECT_FLAG = 'N' ,APPROVED_FLAG='N'  WHERE ORDER_NO = {0} and ITEM_CODE = {1}", dataupdate.ORDER_NO, dataupdate.ITEM_CODE);
                            var update = _objectEntity.ExecuteSqlCommand(queryforupdate);
                        }
                        //var queryforupdate = string.Format(@"UPDATE dist_ip_ssd_purchase_order  SET REJECT_FLAG = 'Y'  WHERE ORDER_NO = {0} and ITEM_CODE = {1} AND APPROVED_FLAG <> 'Y'", dataupdate.ORDER_NO, dataupdate.ITEM_CODE);


                    }

                    transaction.Commit();

                    // dynamic obj = Request.Content.ReadAsAsync<JObject>();
                    //var test=  (PurchaseOrderModel)Convert.ChangeType(obj.Result[0], typeof(PurchaseOrderModel));
                    // var oMycustomclassname = Newtonsoft.Json.JsonConvert.DeserializeObject<test>(obj.Result);
                    //  obj1 = Request.Content.ReadAsAsync<List<PurchaseOrderModel>>();
                    // var y = obj.var1;

                }


                catch (Exception e)
                {

                    transaction.Rollback();
                    if (e.Message.Contains("Unique constraint"))
                        return Request.CreateResponse(HttpStatusCode.OK, new { MESSAGE = "Unique Constraint Voilent", STATUS_CODE = (int)HttpStatusCode.InternalServerError });
                    return Request.CreateResponse(HttpStatusCode.OK, new { MESSAGE = "Error To Converted to Sales Order", STATUS_CODE = (int)HttpStatusCode.NotModified });
                }
            }

            // var TEST=    this.DeserializeObject<IEnumerable<PurchaseOrderModel>>("model");
            //IEnumerable<PurchaseOrderModel>20
            //  var data = _distributionService.GetPurchaseOrderDetail("01", Orderno);
            return Request.CreateResponse(HttpStatusCode.OK, new { MESSAGE = "Purchase order converted into Sales Order.", STATUS_CODE = (int)HttpStatusCode.OK });
            //  return data;
        }

        [System.Web.Http.HttpPost]
        public List<PurchaseOrderModel> GetDealerSalesOrderDetail(string companyCode, string Orderno, string ORDER_ENTITY, string requestStatus = "Active")
        {
            var items = _distributionService.GetDealerSalesOrderDetail(companyCode, Orderno, ORDER_ENTITY, requestStatus);
            return items;
        }
        [System.Web.Http.HttpPost]
        public HttpResponseMessage UpdateDealerSalesOrderDetail([System.Web.Http.ModelBinding.ModelBinder] List<PurchaseOrderModel> model)
        {
            var rowAffected = 0;
            var masterRowaffected = 0;
            var orderno = 0M;
            var ORDER_ENTITY = "";
            DateTime orderdata = DateTime.Now;
            string formCode = string.Empty;
            using (var transaction = _objectEntity.Database.BeginTransaction())
            {
                try
                {

                    var modelrequest = Request.Content.ReadAsAsync<List<PurchaseOrderModel>>().Result;
                    if (modelrequest.Count <= 0)
                        return Request.CreateResponse(HttpStatusCode.OK, new { MESSAGE = "Purchase Order Approve is not selected.", STATUS_CODE = (int)HttpStatusCode.NotFound });
                    else
                        formCode = modelrequest.FirstOrDefault().FORM_CODE;
                    var serialnumber = 1;

                    var voucherCode = "select  FN_NEW_VOUCHER_NO('" + modelrequest.FirstOrDefault().COMPANY_CODE + "','" + formCode + "',TRUNC(sysdate),'SA_SALES_ORDER') from dual";
                    var data = _objectEntity.SqlQuery<string>(voucherCode).FirstOrDefault();
                    if (data == null)
                        return Request.CreateResponse(HttpStatusCode.OK, new { MESSAGE = "Sales Order No Not Generated.Please Once Again", STATUS_CODE = (int)HttpStatusCode.NotFound });
                    //var sumamount = modelrequest.Where(x => x.APPROVED_FLAGBOOL == true).Sum(x => x.TOTAL_PRICE);
                    foreach (var request in modelrequest)
                    {
                        orderno = request.ORDER_NO;
                        orderdata = request.ORDER_DATE ?? DateTime.Now;
                        if (request.APPROVED_FLAGBOOL ?? false)
                        {
                            if (request.UNIT_PRICE <= 0)
                                request.UNIT_PRICE = 0;

                            if (request.UNIT_PRICE == null)
                                request.UNIT_PRICE = 0;
                            request.ApprovedAmount = request.APPROVEQTY * request.UNIT_PRICE;
                            var query = string.Format(@"Insert into SA_SALES_ORDER
                                                           (ORDER_NO, ORDER_DATE, CUSTOMER_CODE, SERIAL_NO, ITEM_CODE, MU_CODE, QUANTITY, UNIT_PRICE, TOTAL_PRICE, CALC_QUANTITY, CALC_UNIT_PRICE, CALC_TOTAL_PRICE, FORM_CODE, COMPANY_CODE, BRANCH_CODE, CREATED_BY, CREATED_DATE, DELETED_FLAG, DELIVERY_DATE, CURRENCY_CODE, EXCHANGE_RATE, TRACKING_NO, STOCK_BLOCK_FLAG,MODIFY_BY,MODIFY_DATE,PARTY_TYPE_CODE)
                                                         Values
                                                           ('" + data + @"', to_date('" + request.ORDER_DATE + "','MM/DD/YYYY HH:MI:SS AM'), '" + request.CUSTOMER_CODE + @"'," + serialnumber + @",
                                                            '" + request.ITEM_CODE + @"', '" + request.MU_CODE + @"'," + request.APPROVEQTY + @" , " + request.UNIT_PRICE + @", " + request.ApprovedAmount + @",
                                                           " + request.APPROVEQTY + @" , " + request.UNIT_PRICE + @", " + request.ApprovedAmount + @",
                                                            '" + request.FORM_CODE + @"', '" + request.COMPANY_CODE + @"', '" + request.BRANCH_CODE + @"', UPPER('" + _workContext.CurrentUserinformation.login_code + @"'), sysdate,
                                                            'N', TO_DATE(sysdate), 'NRS', 1,
                                                            '0', 'N',UPPER('" + _workContext.CurrentUserinformation.login_code + @"'), to_date('" + request.ORDER_DATE + @"','MM/DD/YYYY HH:MI:SS AM'),'" + request.PARTY_TYPE_CODE + "')");

                            rowAffected = _objectEntity.ExecuteSqlCommand(query);
                            //if (rowAffected > 0)
                            //{
                            //    //var queryforupdate = string.Format(@"UPDATE dist_ip_ssd_purchase_order  SET APPROVED_FLAG = 'Y',REJECT_FLAG='N',SALES_ORDER_NO = '{2}',APPROVE_QTY = {3}, APPROVE_AMT = {4}  WHERE ORDER_NO = {0} and ITEM_CODE = {1}", request.ORDER_NO, request.ITEM_CODE, data, request.APPROVEQTY, request.ApprovedAmount);
                            //    //var update = _objectEntity.ExecuteSqlCommand(queryforupdate);
                            //    if (request.QUANTITY != request.APPROVEQTY)
                            //    {

                            //        var queryforhalfupdate = string.Format(@"UPDATE dist_ip_ssd_purchase_order  SET APPROVED_FLAG = 'N',REJECT_FLAG='N',SALES_ORDER_NO = '{2}',APPROVE_QTY = {3}, APPROVE_AMT = {4} ,QUANTITY = {5} WHERE ORDER_NO = {0} and ITEM_CODE = {1}", request.ORDER_NO, request.ITEM_CODE, data, request.APPROVEQTY, request.ApprovedAmount, request.QUANTITY - request.APPROVEQTY);
                            //        var update = _objectEntity.ExecuteSqlCommand(queryforhalfupdate);


                            //        //var queryforremupdate = string.Format(@"UPDATE dist_ip_ssd_purchase_order  SET APPROVED_FLAG = 'N',DELETED_FLAG='N',REJECT_FLAG='N',QUANTITY = {2}  WHERE ORDER_NO = {0} and ITEM_CODE = {1}", request.ORDER_NO, request.ITEM_CODE, request.QUANTITY - request.APPROVEQTY);
                            //        //var updateRem = _objectEntity.ExecuteSqlCommand(queryforremupdate);



                            //        //var insertRemaining = $@"INSERT INTO DIST_IP_SSD_PURCHASE_ORDER(ORDER_NO,ORDER_DATE,CUSTOMER_CODE,ITEM_CODE,CREATED_BY,CREATED_DATE,QUANTITY,
                            //        //                                                                COMPANY_CODE,BRANCH_CODE,APPROVED_FLAG,DISPATCH_FLAG,ACKNOWLEDGE_FLAG,
                            //        //                                                                REJECT_FLAG,DELETED_FLAG,ORDER_FROM,MODIFY_BY,MODIFY_DATE)
                            //        //                         VALUES('{request.ORDER_NO}',to_date('{request.ORDER_DATE.Value.ToShortDateString()}','MM/DD/YYYY'),'{request.CUSTOMER_CODE}','{request.ITEM_CODE}','{request.CREATED_BY}',to_date('{request.CREATED_DATE.Value.ToShortDateString()}','MM/DD/YYYY')
                            //        //                                 ,'{request.QUANTITY - request.APPROVEQTY}','{request.COMPANY_CODE}','{request.BRANCH_CODE}','N','N','N','N','N','H','{_workContext.CurrentUserinformation.login_code}',SYSDATE)";
                            //        //_objectEntity.ExecuteSqlCommand(insertRemaining);
                            //    }
                            //    else
                            //    {
                            //        var queryforupdate = string.Format(@"UPDATE dist_ip_ssd_purchase_order  SET APPROVED_FLAG = 'N',REJECT_FLAG='N',SALES_ORDER_NO = '{2}',APPROVE_QTY = {3}, APPROVE_AMT = {4}  WHERE ORDER_NO = {0} and ITEM_CODE = {1}", request.ORDER_NO, request.ITEM_CODE, data, request.APPROVEQTY, request.ApprovedAmount);
                            //        var update = _objectEntity.ExecuteSqlCommand(queryforupdate);
                            //    }
                            //}
                            serialnumber++;
                        }
                    }
                    if (rowAffected > 0)
                    {
                        var sumamount = modelrequest.Where(x => x.APPROVED_FLAGBOOL == true).Sum(x => x.ApprovedAmount);
                        var masterQuery = @"Insert into MASTER_TRANSACTION
                                               (VOUCHER_NO, VOUCHER_AMOUNT, FORM_CODE, CHECKED_BY, AUTHORISED_BY, POSTED_BY, COMPANY_CODE, BRANCH_CODE, CREATED_BY, CREATED_DATE, DELETED_FLAG, VOUCHER_DATE, CURRENCY_CODE, EXCHANGE_RATE, PRINT_COUNT,  PRINT_FLAG)
                                             Values
                                               ('" + data + @"', " + sumamount + @", '" + modelrequest.FirstOrDefault().FORM_CODE + @"', '', '',
                                                '', '" + modelrequest.FirstOrDefault().COMPANY_CODE + "', '" + modelrequest.FirstOrDefault().BRANCH_CODE + "', UPPER('" + _workContext.CurrentUserinformation.login_code + @"'),
                                                sysdate, 'N',  to_date('" + orderdata + @"','MM/DD/YYYY HH:MI:SS AM'),
                                                'NRS', 1, 0,'N')";
                        masterRowaffected = _objectEntity.ExecuteSqlCommand(masterQuery);
                    }

                    var dataforUpdate = _distributionService.GetDealerSalesOrderDetail("01", orderno.ToString(), ORDER_ENTITY, "Active");
                    int count = 0;
                    var apprQty = 0;
                    var apprAmt = 0;
                    foreach (var dataupdate in dataforUpdate)
                    {
                        if (modelrequest.Any(x => x.ORDER_NO == dataupdate.ORDER_NO && x.ITEM_CODE == dataupdate.ITEM_CODE))
                        {
                            //if (count == 0) { apprQty = Convert.ToInt32(modelrequest[count].APPROVEQTY); apprAmt = Convert.ToInt32(modelrequest[count].ApprovedAmount); } else
                            //{
                            apprQty = Convert.ToInt32(dataupdate.APPROVEQTY) + Convert.ToInt32(modelrequest[count].APPROVEQTY);
                            apprAmt = Convert.ToInt32(modelrequest[count].ApprovedAmount);
                            if (apprAmt <= 0)
                            {
                                apprAmt = Convert.ToInt32(modelrequest[count].NEW_TOTAL_AMOUNT);
                            }
                            //}

                            //apprAmt = dataupdate.APPROVEQTY + modelrequest[count].APPROVEQTY;
                            var partialUpdate = $@"UPDATE DIST_IP_SSD_PURCHASE_ORDER SET REJECT_FLAG='N',APPROVE_QTY = {apprQty}, APPROVE_AMT = {apprAmt} ,APPROVED_FLAG='Y',QUANTITY={modelrequest[count].QUANTITY - modelrequest[count].APPROVEQTY} WHERE ORDER_NO = {modelrequest[count].ORDER_NO} and ITEM_CODE = {modelrequest[count].ITEM_CODE}";
                            var Pupdate = _objectEntity.ExecuteSqlCommand(partialUpdate);
                            count++;
                        }
                        else
                        {
                            var queryforupdate = string.Format(@"UPDATE dist_ip_ssd_purchase_order  SET REJECT_FLAG = 'N' ,APPROVED_FLAG='N'  WHERE ORDER_NO = {0} and ITEM_CODE = {1}", dataupdate.ORDER_NO, dataupdate.ITEM_CODE);
                            var update = _objectEntity.ExecuteSqlCommand(queryforupdate);
                        }
                        //var queryforupdate = string.Format(@"UPDATE dist_ip_ssd_purchase_order  SET REJECT_FLAG = 'Y'  WHERE ORDER_NO = {0} and ITEM_CODE = {1} AND APPROVED_FLAG <> 'Y'", dataupdate.ORDER_NO, dataupdate.ITEM_CODE);


                    }

                    transaction.Commit();

                    // dynamic obj = Request.Content.ReadAsAsync<JObject>();
                    //var test=  (PurchaseOrderModel)Convert.ChangeType(obj.Result[0], typeof(PurchaseOrderModel));
                    // var oMycustomclassname = Newtonsoft.Json.JsonConvert.DeserializeObject<test>(obj.Result);
                    //  obj1 = Request.Content.ReadAsAsync<List<PurchaseOrderModel>>();
                    // var y = obj.var1;

                }


                catch (Exception e)
                {

                    transaction.Rollback();
                    if (e.Message.Contains("Unique constraint"))
                        return Request.CreateResponse(HttpStatusCode.OK, new { MESSAGE = "Unique Constraint Voilent", STATUS_CODE = (int)HttpStatusCode.InternalServerError });
                    return Request.CreateResponse(HttpStatusCode.OK, new { MESSAGE = "Error To Converted to Sales Order", STATUS_CODE = (int)HttpStatusCode.NotModified });
                }
            }

            // var TEST=    this.DeserializeObject<IEnumerable<PurchaseOrderModel>>("model");
            //IEnumerable<PurchaseOrderModel>20
            //  var data = _distributionService.GetPurchaseOrderDetail("01", Orderno);
            return Request.CreateResponse(HttpStatusCode.OK, new { MESSAGE = "Purchase order converted into Sales Order.", STATUS_CODE = (int)HttpStatusCode.OK });
            //  return data;
        }




        [System.Web.Http.HttpPost]
        public HttpResponseMessage DeleteDistributionPurchaseOrderDetail([System.Web.Http.ModelBinding.ModelBinder] List<PurchaseOrderModel> model, bool parent = false)
        {
            var modelrequest = Request.Content.ReadAsAsync<List<PurchaseOrderModel>>().Result;
            if (modelrequest.Count <= 0)
                return Request.CreateResponse(HttpStatusCode.OK, new { MESSAGE = "Item is not selected.", STATUS_CODE = (int)HttpStatusCode.NotFound });

            PurchaseOrderModel modelData = modelrequest.FirstOrDefault();

            bool flag = _distributionService.DeletePOItem(modelData.ORDER_NO, modelData.ITEM_CODE, parent);
            return Request.CreateResponse(HttpStatusCode.OK, new { MESSAGE = "Item Deleted Successfully.", STATUS_CODE = (int)HttpStatusCode.OK });
        }

        [System.Web.Http.HttpPost]
        public List<PurchaseOrderReportModel> GetPurchaseOrderList(filterOption model, string requestStatus = "Active")
        {

            var data = _distributionService.GetPurchaseOrderList(model.ReportFilters, requestStatus, _workContext.CurrentUserinformation);
            return data;
        }

        [System.Web.Http.HttpPost]
        public List<PurchaseOrderReportModel> GetWholeSellerPurchaseOrderReport(filterOption model, string requestStatus = "Active")
        {

            var data = _distributionService.GetWholeSellerPurchaseOrderReport(model.ReportFilters, requestStatus, _workContext.CurrentUserinformation);
            return data;
        }

        [System.Web.Http.HttpPost]
        public List<PurchaseOrderReportModel> GetResellerPurchaseOrderList(filterOption model, string requestStatus = "Active")
        {
            var data = _distributionService.GetResellerPurchaseOrderList(model.ReportFilters, requestStatus, _workContext.CurrentUserinformation);
            return data;
        }
        [System.Web.Http.HttpPost]
        public HttpResponseMessage UpdateResellerPurchaseOrder([System.Web.Http.ModelBinding.ModelBinder] List<PurchaseOrderModel> model)
        {
            var modelrequest = Request.Content.ReadAsAsync<List<PurchaseOrderModel>>().Result;
             if (modelrequest.Count <= 0)
                Request.CreateResponse(HttpStatusCode.OK, new { MESSAGE = "Approver data not found please select item first and try again.", STATUS_CODE = (int)HttpStatusCode.NoContent });
            foreach (var data in modelrequest)
            {
                string itemCode = "";
                if (data.ITEM_CODE != null)
                    itemCode = $" AND ITEM_CODE = '{data.ITEM_CODE}'";
                if (data.IsDeleted == "Delete")
                {
                    var query = $@"update DIST_IP_SSR_PURCHASE_ORDER
                                set REJECT_FLAG = 'Y',MODIFY_DATE=sysdate,REMARKS_REVIEW='{data.CREDITLIMITREMARKS}'
                                WHERE ORDER_NO = '{data.ORDER_NO}' {itemCode}";
                    var flag = _objectEntity.ExecuteSqlCommand(query);
                    // return Request.CreateResponse(HttpStatusCode.OK, new { MESSAGE = "Sales Order Rejected Successfully", STATUS_CODE = (int)HttpStatusCode.OK });
                }
                else if (data.APPROVED_FLAGBOOL == true)
                {
                    var queryforupdate = string.Format(@"UPDATE DIST_IP_SSR_PURCHASE_ORDER  SET APPROVE_QTY = '{0}' ,APPROVED_FLAG='Y',REMARKS_REVIEW='{3}' WHERE ORDER_NO = {1} AND ITEM_CODE='{2}' AND APPROVED_FLAG <> 'Y'", data.APPROVEQTY, data.ORDER_NO, data.ITEM_CODE, data.CREDITLIMITREMARKS);
                    var update = _objectEntity.ExecuteSqlCommand(queryforupdate);
                    //  return Request.CreateResponse(HttpStatusCode.OK, new { MESSAGE = "Sales Order Generated Successfully", STATUS_CODE = (int)HttpStatusCode.OK });
                }

            }
            if (modelrequest.Where(x => x.IsDeleted == "Delete").Count() > 0)
            {
                return Request.CreateResponse(HttpStatusCode.OK, new { MESSAGE = "Sales Order Rejected Successfully.", STATUS_CODE = (int)HttpStatusCode.OK });

            }
            // var data = _distributionService.GetPurchaseOrderList();
            return Request.CreateResponse(HttpStatusCode.OK, new { MESSAGE = "Transaction are completed Successfully.", STATUS_CODE = (int)HttpStatusCode.OK });
        }

        [System.Web.Http.HttpPost]
        public HttpResponseMessage DeleteResellerPurchaseOrderDetail([System.Web.Http.ModelBinding.ModelBinder] List<PurchaseOrderModel> model, bool parent = false)
        {
            var modelrequest = Request.Content.ReadAsAsync<List<PurchaseOrderModel>>().Result;
            if (modelrequest.Count <= 0)
                return Request.CreateResponse(HttpStatusCode.OK, new { MESSAGE = "Item is not selected.", STATUS_CODE = (int)HttpStatusCode.NotFound });

            PurchaseOrderModel modelData = modelrequest.FirstOrDefault();
            var query = $@"update DIST_IP_SSR_PURCHASE_ORDER
                                set REJECT_FLAG = 'Y',MODIFY_DATE=sysdate,REMARKS_REVIEW='{modelData.CREDITLIMITREMARKS}'
                                WHERE ORDER_NO = '{modelData.ORDER_NO}' AND ITEM_CODE = '{modelData.ITEM_CODE}'";
            var flag = _objectEntity.ExecuteSqlCommand(query);
            //  bool flag = _distributionService.DeletePOItem(modelData.ORDER_NO, modelData.ITEM_CODE, parent);
            return Request.CreateResponse(HttpStatusCode.OK, new { MESSAGE = "Item Deleted Successfully.", STATUS_CODE = (int)HttpStatusCode.OK });
        }
    }
}
