using NeoERP.DocumentTemplate.Service.Models;
using NeoERP.DocumentTemplate.Service.Interface;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using System.Web.Http.Results;
using System.Web.ModelBinding;
//using Microsoft.Owin;
using Newtonsoft.Json.Linq;
using NeoErp.Core;
using NeoErp.Data;
using NeoErp.Core.Caching;
using System.Text;
using Newtonsoft.Json;
using System.Data.SqlClient;
using NeoErp.Core.Models;
using NeoErp.Core.Models.Log4NetLoggin;

namespace NeoERP.DocumentTemplate.Controllers.Api
{
    public class InventoryApiController : ApiController
    {
        private ITestTemplateRepo _TestTemplateRepo;
        private IFormTemplateRepo _FormTemplateRepo;
        //subin change
        private IFormSetupRepo _FormSetupRepo;
        IInventoryVoucher _inventoryvoucher;
        IContraVoucher _contravoucher;
        private ISalesOrderRepo _SalesOrderRepo;
        private IDbContext _dbContext;
        private IWorkContext _workContext;
        private ICacheManager _cacheManager;
        private IContraVoucher _Reference;
        private NeoErpCoreEntity _objectEntity;
        private DefaultValueForLog _defaultValueForLog;
        private readonly ILogErp _logErp;
        public InventoryApiController(ITestTemplateRepo TestTemplateRepo, IFormTemplateRepo FormTemplateRepo, IFormSetupRepo FormSetupRepo, ISalesOrderRepo SalesOrderRepo, IDbContext dbContext, IWorkContext workContext, ICacheManager cacheManager, IContraVoucher Reference, IContraVoucher contravoucher, IInventoryVoucher inventoryvoucher, NeoErpCoreEntity objectEntity)
        {
            this._TestTemplateRepo = TestTemplateRepo;
            this._FormTemplateRepo = FormTemplateRepo;
            //subin change
            this._FormSetupRepo = FormSetupRepo;
            this._dbContext = dbContext;
            this._workContext = workContext;
            this._cacheManager = cacheManager;
            this._SalesOrderRepo = SalesOrderRepo;
            this._Reference = Reference;
            this._contravoucher = contravoucher;
            this._inventoryvoucher = inventoryvoucher;
            this._objectEntity = objectEntity;
            this._defaultValueForLog = new DefaultValueForLog(this._workContext);
            this._logErp = new LogErp(this, _defaultValueForLog.LogUser, _defaultValueForLog.LogCompany, _defaultValueForLog.LogBranch, _defaultValueForLog.LogTypeCode, _defaultValueForLog.LogModule,_defaultValueForLog.FormCode);
        }
        [HttpGet]
        public List<Customers> GetAllCustomerSetupByFilter(string filter)
        {
            var userid = _workContext.CurrentUserinformation.User_id;
            var company_code = _workContext.CurrentUserinformation.company_code;
            var branch_code = _workContext.CurrentUserinformation.branch_code;
            var AllFilterCustomer = this._FormTemplateRepo.GetAllCustomerSetup(filter);

            if (filter == null)
                return AllFilterCustomer;
            if (filter.Contains("#"))
                return AllFilterCustomer;

            var Filterdata = new List<Customers>();
            if (AllFilterCustomer.Count >= 1)
            {
                if (this._cacheManager.IsSet($"GetAllCustomerSetupByFilter_{userid}_{company_code}_{branch_code}_{filter}"))
                {
                    var data = _cacheManager.Get<List<Customers>>($"GetAllCustomerSetupByFilter_{userid}_{company_code}_{branch_code}_{filter}");
                    AllFilterCustomer = data;
                }
                else
                {
                    var CustomerCodes = AllFilterCustomer.Where(x => x.CustomerCode.ToLower().Contains(filter));
                    var StartWithCustomer = CustomerCodes.Where(x => x.CustomerCode.ToLower().StartsWith(filter.Trim()));
                    var EndWithCustomerCode = CustomerCodes.Where(x => x.CustomerCode.ToLower().EndsWith(filter.Trim()));
                    var ContainsCustomerCode = CustomerCodes.SkipWhile(x => x.CustomerCode.ToLower().StartsWith(filter.Trim()) || x.CustomerCode.ToLower().EndsWith(filter.Trim()));

                    Filterdata.AddRange(StartWithCustomer);
                    Filterdata.AddRange(ContainsCustomerCode);
                    Filterdata.AddRange(EndWithCustomerCode);
                    Filterdata.ForEach(s => s.Type = "CustomerCode");

                    var Removedata = AllFilterCustomer.RemoveAll(x => x.CustomerCode.ToLower().Contains(filter));
                    var CustomerNames = AllFilterCustomer.Where(x => x.CustomerName.ToLower().Contains(filter));

                    var StartWithCustomerName = CustomerNames.Where(x => x.CustomerName.ToLower().StartsWith(filter.Trim())).ToList();

                    var EndWithCustomerName = CustomerNames.Where(x => x.CustomerName.ToLower().EndsWith(filter.Trim()));
                    var ContainsCustomerName = CustomerNames.SkipWhile(x => x.CustomerName.ToLower().StartsWith(filter.Trim()) || x.CustomerName.ToLower().EndsWith(filter.Trim()));
                    StartWithCustomerName.ForEach(s => s.Type = "CustomerName");
                    EndWithCustomerName.ToList().ForEach(s => s.Type = "CustomerName");
                    ContainsCustomerName.ToList().ForEach(s => s.Type = "CustomerName");

                    var c = new List<Customers>();

                    var a = ContainsCustomerName.ToList();

                    c.AddRange(a);

                    c.ForEach(x => x.CustomerName.Substring(1));


                    Filterdata.AddRange(StartWithCustomerName);
                    Filterdata.AddRange(ContainsCustomerName);
                    Filterdata.AddRange(EndWithCustomerName);
                    this._cacheManager.Set($"GetAllBranchCodeByFilte_{userid}_{company_code}_{branch_code}_{filter}", Filterdata, 20);

                    return Filterdata;
                }

            }
            return AllFilterCustomer;

        }
        public List<FinanceVoucherReference> GetReferenceList(string formcode)
        {
            var userid = _workContext.CurrentUserinformation.User_id;
            var company_code = _workContext.CurrentUserinformation.company_code;
            var branch_code = _workContext.CurrentUserinformation.branch_code;
            var response = new List<FinanceVoucherReference>();
            //if (this._cacheManager.IsSet($"GetReferenceList_{userid}_{company_code}_{branch_code}_{formcode}"))
            //{
            //    var data = _cacheManager.Get<List<FinanceVoucherReference>>($"GetReferenceList_{userid}_{company_code}_{branch_code}_{formcode}");
            //    response = data;
            //}
            //else
            //{
            //    var financeVoucherReferenceList = this._Reference.GetFinanceVoucherReferenceList(formcode);
            //    this._cacheManager.Set($"GetReferenceList_{userid}_{company_code}_{branch_code}_{formcode}", financeVoucherReferenceList, 20);
            //    response = financeVoucherReferenceList;
            //}
            var financeVoucherReferenceList = this._Reference.GetFinanceVoucherReferenceList(formcode);
            //this._cacheManager.Set($"GetReferenceList_{userid}_{company_code}_{branch_code}_{formcode}", financeVoucherReferenceList, 20);
            response = financeVoucherReferenceList;
            return response;
            // return this._Reference.GetFinanceVoucherReferenceList(formcode);
        }
        [HttpGet]
        public List<Branch> GetAllBranchCodeByFilter(string filter)
        {
            //var colName = filter.Split('#')[0].ToString().ToLower();
            var userid = _workContext.CurrentUserinformation.User_id;
            var company_code = _workContext.CurrentUserinformation.company_code;
            var branch_code = _workContext.CurrentUserinformation.branch_code;
            var Filterdata = new List<Branch>();

            if (filter == "!@$")
                return Filterdata;
            if (filter == null)
                filter = "";
            if (filter.Contains("#"))
            {

                var colName = filter.Split('#')[0].ToString().ToLower();
                filter = filter.Split('#')[1].ToString().ToLower();
                if (colName == "code")
                {
                    if (this._cacheManager.IsSet($"GetAllBranchCodeByFilter_{userid}_{company_code}_{branch_code}_{filter}_{colName}"))
                    {
                        var data = _cacheManager.Get<List<Branch>>($"GetAllBranchCodeByFilter_{userid}_{company_code}_{branch_code}_{filter}_{colName}");
                        Filterdata = data;
                    }
                    else
                    {
                        var AllFilterBranch = this._FormTemplateRepo.GetAllBranchCodeByFilter(filter);
                        var BranchCodes = AllFilterBranch.Where(x => x.BRANCH_CODE.ToLower().Contains(filter));
                        var StartWithBranchCode = BranchCodes.Where(x => x.BRANCH_CODE.ToLower().StartsWith(filter.Trim())).ToList();
                        var startWithCodes = StartWithBranchCode.Select(x => x.BRANCH_CODE.ToLower()).ToList();
                        var EndWithBranchCode = BranchCodes.Where(x => !startWithCodes.Contains(x.BRANCH_CODE.ToLower()) && x.BRANCH_CODE.ToLower().EndsWith(filter.Trim()));
                        var endWithCodes = EndWithBranchCode.Select(x => x.BRANCH_EDESC.ToLower()).ToList();
                        var ContainsBranchCode = BranchCodes.Where(x => !startWithCodes.Contains(x.BRANCH_CODE.ToLower()) && !endWithCodes.Contains(x.BRANCH_CODE.ToLower()));

                        Filterdata.AddRange(StartWithBranchCode);
                        Filterdata.AddRange(ContainsBranchCode);
                        Filterdata.AddRange(EndWithBranchCode);
                        Filterdata.ForEach(s => s.Type = "Code");
                        this._cacheManager.Set($"GetAllBranchCodeByFilter_{userid}_{company_code}_{branch_code}_{filter}_{colName}", Filterdata, 20);
                    }
                    return Filterdata;
                }
                if (colName == "name")
                {
                    if (this._cacheManager.IsSet($"GetAllBranchCodeByFilter_{userid}_{company_code}_{branch_code}_{filter}_{colName}"))
                    {
                        var data = _cacheManager.Get<List<Branch>>($"GetAllBranchCodeByFilter_{userid}_{company_code}_{branch_code}_{filter}_{colName}");
                        Filterdata = data;
                    }
                    else
                    {
                        var AllFilterBranch = this._FormTemplateRepo.GetAllBranchCodeByFilter(filter);
                        var BranchNames = AllFilterBranch.Where(x => x.BRANCH_EDESC.ToLower().Contains(filter));
                        var StartWithBranchName = BranchNames.Where(x => x.BRANCH_EDESC.ToLower().StartsWith(filter.Trim())).ToList();
                        var startWithNames = StartWithBranchName.Select(x => x.BRANCH_EDESC.ToLower()).ToList();
                        var EndWithBranchName = BranchNames.Where(x => !startWithNames.Contains(x.BRANCH_EDESC.ToLower()) && x.BRANCH_EDESC.ToLower().EndsWith(filter.Trim()));
                        var endWithNames = EndWithBranchName.Select(x => x.BRANCH_EDESC.ToLower()).ToList();
                        var ContainsBranchName = BranchNames.Where(x => !startWithNames.Contains(x.BRANCH_EDESC.ToLower()) && !endWithNames.Contains(x.BRANCH_EDESC.ToLower()));


                        StartWithBranchName.ForEach(s => s.Type = "Name");
                        EndWithBranchName.ToList().ForEach(s => s.Type = "Name");
                        ContainsBranchName.ToList().ForEach(s => s.Type = "Name");

                        Filterdata.AddRange(StartWithBranchName);
                        Filterdata.AddRange(ContainsBranchName);
                        Filterdata.AddRange(EndWithBranchName);
                        this._cacheManager.Set($"GetAllBranchCodeByFilter_{userid}_{company_code}_{branch_code}_{filter}_{colName}", Filterdata, 20);
                    }
                    return Filterdata;
                }
                if (colName == "address")
                {
                    if (this._cacheManager.IsSet($"GetAllBranchCodeByFilter_{userid}_{company_code}_{branch_code}_{filter}_{colName}"))
                    {
                        var data = _cacheManager.Get<List<Branch>>($"GetAllBranchCodeByFilter_{userid}_{company_code}_{branch_code}_{filter}_{colName}");
                        Filterdata = data;
                    }
                    else
                    {
                        var AllFilterAddress = this._FormTemplateRepo.GetAllBranchCodeByFilter(filter);
                        var Address = AllFilterAddress.Where(x => x.ADDRESS.ToLower().Contains(filter));
                        var StartWithAddressName = Address.Where(x => x.ADDRESS.ToLower().StartsWith(filter.Trim())).ToList();
                        var startWithAddress = StartWithAddressName.Select(x => x.ADDRESS.ToLower()).ToList();
                        var EndWithAddressName = Address.Where(x => !startWithAddress.Contains(x.ADDRESS.ToLower()) && x.ADDRESS.ToLower().EndsWith(filter.Trim()));
                        var endWithAddress = EndWithAddressName.Select(x => x.ADDRESS.ToLower()).ToList();
                        var ContainsAddressName = Address.Where(x => !startWithAddress.Contains(x.ADDRESS.ToLower()) && !endWithAddress.Contains(x.ADDRESS.ToLower()));


                        StartWithAddressName.ForEach(s => s.Type = "Addr");
                        EndWithAddressName.ToList().ForEach(s => s.Type = "Addr");
                        ContainsAddressName.ToList().ForEach(s => s.Type = "Addr");

                        Filterdata.AddRange(StartWithAddressName);
                        Filterdata.AddRange(ContainsAddressName);
                        Filterdata.AddRange(EndWithAddressName);
                        this._cacheManager.Set($"GetAllBranchCodeByFilter_{userid}_{company_code}_{branch_code}_{filter}_{colName}", Filterdata, 20);
                    }
                    return Filterdata;
                }
                if (colName == "phoneno")
                {
                    if (this._cacheManager.IsSet($"GetAllBranchCodeByFilter_{userid}_{company_code}_{branch_code}_{filter}_{colName}"))
                    {
                        var data = _cacheManager.Get<List<Branch>>($"GetAllBranchCodeByFilter_{userid}_{company_code}_{branch_code}_{filter}_{colName}");
                        Filterdata = data;
                    }
                    else
                    {
                        var AllFilterPhoneNo = this._FormTemplateRepo.GetAllBranchCodeByFilter(filter);
                        var PhoneNoNames = AllFilterPhoneNo.Where(x => x.TELEPHONE_NO.ToLower().Contains(filter));
                        var StartWithPhoneNoName = PhoneNoNames.Where(x => x.TELEPHONE_NO.ToLower().StartsWith(filter.Trim())).ToList();
                        var startWithNames = StartWithPhoneNoName.Select(x => x.TELEPHONE_NO.ToLower()).ToList();
                        var EndWithPhoneNoName = PhoneNoNames.Where(x => !startWithNames.Contains(x.TELEPHONE_NO.ToLower()) && x.TELEPHONE_NO.ToLower().EndsWith(filter.Trim()));
                        var endWithNames = EndWithPhoneNoName.Select(x => x.TELEPHONE_NO.ToLower()).ToList();
                        var ContainsPhoneNoName = PhoneNoNames.Where(x => !startWithNames.Contains(x.TELEPHONE_NO.ToLower()) && !endWithNames.Contains(x.TELEPHONE_NO.ToLower()));


                        StartWithPhoneNoName.ForEach(s => s.Type = "Ph");
                        EndWithPhoneNoName.ToList().ForEach(s => s.Type = "Ph");
                        ContainsPhoneNoName.ToList().ForEach(s => s.Type = "Ph");

                        Filterdata.AddRange(StartWithPhoneNoName);
                        Filterdata.AddRange(ContainsPhoneNoName);
                        Filterdata.AddRange(EndWithPhoneNoName);

                        this._cacheManager.Set($"GetAllBranchCodeByFilter_{userid}_{company_code}_{branch_code}_{filter}_{colName}", Filterdata, 20);
                    }
                    return Filterdata;


                }
            }

            else
            {
                var AllFilterBranch = this._FormTemplateRepo.GetAllBranchCodeByFilter(filter);
                if (AllFilterBranch.Count >= 1)
                {
                    if (this._cacheManager.IsSet($"GetAllBranchCodeByFilter_{userid}_{company_code}_{branch_code}_{filter}"))
                    {
                        var data = _cacheManager.Get<List<Branch>>($"GetAllBranchCodeByFilter_{userid}_{company_code}_{branch_code}_{filter}");
                        Filterdata = data;
                    }
                    else
                    {
                        var BranchCodes = AllFilterBranch.Where(x => x.BRANCH_CODE.ToLower().Contains(filter));
                        var StartWithBranchCode = BranchCodes.Where(x => x.BRANCH_CODE.ToLower().StartsWith(filter.Trim())).ToList();
                        var startWithCodes = StartWithBranchCode.Select(x => x.BRANCH_CODE.ToLower()).ToList();
                        var EndWithBranchCode = BranchCodes.Where(x => !startWithCodes.Contains(x.BRANCH_CODE.ToLower()) && x.BRANCH_CODE.ToLower().EndsWith(filter.Trim()));
                        var endWithCodes = EndWithBranchCode.Select(x => x.BRANCH_CODE.ToLower()).ToList();
                        var ContainsBranchCode = BranchCodes.Where(x => !startWithCodes.Contains(x.BRANCH_CODE.ToLower()) && !endWithCodes.Contains(x.BRANCH_CODE.ToLower()));

                        Filterdata.AddRange(StartWithBranchCode);
                        Filterdata.AddRange(ContainsBranchCode);
                        Filterdata.AddRange(EndWithBranchCode);
                        Filterdata.ForEach(s => s.Type = "Code");

                        var Removedata = AllFilterBranch.RemoveAll(x => x.BRANCH_CODE.ToLower().Contains(filter));
                        var BranchNames = AllFilterBranch.Where(x => x.BRANCH_EDESC.ToLower().Contains(filter));
                        var StartWithBranchName = BranchNames.Where(x => x.BRANCH_EDESC.ToLower().StartsWith(filter.Trim())).ToList();
                        var startWithNames = StartWithBranchName.Select(x => x.BRANCH_EDESC.ToLower()).ToList();
                        var EndWithBranchName = BranchNames.Where(x => !startWithNames.Contains(x.BRANCH_EDESC.ToLower()) && x.BRANCH_EDESC.ToLower().EndsWith(filter.Trim()));
                        var endWithNames = EndWithBranchName.Select(x => x.BRANCH_EDESC.ToLower()).ToList();
                        var ContainsBranchName = BranchNames.Where(x => !startWithNames.Contains(x.BRANCH_EDESC.ToLower()) && !endWithNames.Contains(x.BRANCH_EDESC.ToLower()));


                        StartWithBranchName.ForEach(s => s.Type = "Name");
                        EndWithBranchName.ToList().ForEach(s => s.Type = "Name");
                        ContainsBranchName.ToList().ForEach(s => s.Type = "Name");

                        Filterdata.AddRange(StartWithBranchName);
                        Filterdata.AddRange(ContainsBranchName);
                        Filterdata.AddRange(EndWithBranchName);


                        AllFilterBranch.RemoveAll(x => x.BRANCH_EDESC.ToLower().Contains(filter));
                        var Addresses = AllFilterBranch.Where(x => x.ADDRESS.ToLower().Contains(filter));
                        var StartWithAddress = Addresses.Where(x => x.ADDRESS.ToLower().StartsWith(filter.Trim())).ToList();
                        var startWithAddress = StartWithAddress.Select(x => x.ADDRESS.ToLower()).ToList();
                        var EndWithAddress = Addresses.Where(x => !startWithAddress.Contains(x.ADDRESS.ToLower()) && x.ADDRESS.ToLower().EndsWith(filter.Trim()));
                        var endWithAddress = EndWithAddress.Select(x => x.ADDRESS.ToLower()).ToList();
                        var ContainsAddress = Addresses.Where(x => !startWithAddress.Contains(x.ADDRESS.ToLower()) && !endWithAddress.Contains(x.ADDRESS.ToLower()));


                        StartWithAddress.ForEach(s => s.Type = "Addr");
                        EndWithAddress.ToList().ForEach(s => s.Type = "Addr");
                        ContainsAddress.ToList().ForEach(s => s.Type = "Addr");

                        Filterdata.AddRange(StartWithAddress);
                        Filterdata.AddRange(ContainsAddress);
                        Filterdata.AddRange(EndWithAddress);

                        AllFilterBranch.RemoveAll(x => x.ADDRESS.ToLower().Contains(filter));

                        var PhoneNoNames = AllFilterBranch.Where(x => x.TELEPHONE_NO.ToLower().Contains(filter));

                        var StartWithPhoneNoName = PhoneNoNames.Where(x => x.TELEPHONE_NO.ToLower().StartsWith(filter.Trim())).ToList();
                        var startWithPhone = StartWithPhoneNoName.Select(x => x.TELEPHONE_NO.ToLower()).ToList();
                        var EndWithPhoneNoName = PhoneNoNames.Where(x => !startWithPhone.Contains(x.TELEPHONE_NO.ToLower()) && x.TELEPHONE_NO.ToLower().EndsWith(filter.Trim()));
                        var endWithPhone = EndWithPhoneNoName.Select(x => x.TELEPHONE_NO.ToLower()).ToList();
                        var ContainsPhoneNoName = PhoneNoNames.Where(x => !startWithPhone.Contains(x.TELEPHONE_NO.ToLower()) && !endWithPhone.Contains(x.TELEPHONE_NO.ToLower()));


                        StartWithPhoneNoName.ForEach(s => s.Type = "Ph");
                        EndWithPhoneNoName.ToList().ForEach(s => s.Type = "Ph");
                        ContainsPhoneNoName.ToList().ForEach(s => s.Type = "Ph");

                        Filterdata.AddRange(StartWithPhoneNoName);
                        Filterdata.AddRange(ContainsPhoneNoName);
                        Filterdata.AddRange(EndWithPhoneNoName);
                        this._cacheManager.Set($"GetAllBranchCodeByFilter_{userid}_{company_code}_{branch_code}_{filter}", Filterdata, 20);
                        return Filterdata;

                    }
                }
                return AllFilterBranch;
            }
            return Filterdata;
        }
        public IHttpActionResult GetVouchersCount(string FORM_CODE, string TABLE_NAME)
        {
            try
            {
                var rslt = _inventoryvoucher.GetTotalVoucher(FORM_CODE, TABLE_NAME);
                return Ok(rslt);
            }
            catch (Exception ex)
            {
                return Content(HttpStatusCode.NotFound, ex.Message);
            }


        }
        [HttpPost]
        public HttpResponseMessage SaveInventoryFormDataOld(FormDetails model)
        {
            using (var trans = _objectEntity.Database.BeginTransaction())
            {
                try
                {
                    var voucherno = model.Order_No;
                    var primarydatecolumn = _FormTemplateRepo.GetPrimaryDateByTableName(model.Table_Name);
                    var primarycolname = _contravoucher.GetPrimaryColumnByTableName(model.Table_Name);
                    string primarydate = string.Empty, primarycolumn = string.Empty, today = DateTime.Now.ToString("dd-MMM-yyyy"), createddatestring = "TO_DATE('" + today + "'" + ",'DD-MON-YYYY hh24:mi:ss')", todaystring = System.DateTime.Now.ToString("yyyyMMddHHmmss"), manualno = string.Empty, currencyformat = "NRS", VoucherDate = createddatestring, grandtotal = model.Grand_Total;
                    string newvoucherNo = _FormTemplateRepo.NewVoucherNo(this._workContext.CurrentUserinformation.company_code, model.Form_Code, today, model.Table_Name);
                    decimal exchangrate = 1;
                    var quantityvalue = 0.00;

                    Newtonsoft.Json.Linq.JObject mastercolumn = (Newtonsoft.Json.Linq.JObject)Newtonsoft.Json.JsonConvert.DeserializeObject(model.Master_COLUMN_VALUE);
                    Newtonsoft.Json.Linq.JObject customcolumn = (Newtonsoft.Json.Linq.JObject)Newtonsoft.Json.JsonConvert.DeserializeObject(model.Custom_COLUMN_VALUE);

                    dynamic childcolumnvalues = JsonConvert.DeserializeObject(model.Child_COLUMN_VALUE);
                    //dynamic childbudgetcentervalues = JsonConvert.DeserializeObject(model.BUDGET_TRANS_VALUE);
                    StringBuilder Columnbuilder = new StringBuilder();
                    StringBuilder valuesbuilder = new StringBuilder();
                    bool insertmaintable = false, insertmastertable = false;
                    var staticsalesordercolumns = "SERIAL_NO, FORM_CODE,COMPANY_CODE,BRANCH_CODE,CREATED_BY,CREATED_DATE,DELETED_FLAG";
                    foreach (var m in mastercolumn)
                    {
                        if (m.Key.ToString() == "CURRENCY_CODE")
                        {
                            Columnbuilder.Append(m.Key.ToString()).Append(",");
                        }
                        else if (m.Key.ToString() == "EXCHANGE_RATE")
                        {
                            Columnbuilder.Append(m.Key.ToString()).Append(",");
                        }
                        else Columnbuilder.Append(m.Key.ToString()).Append(",");
                    }
                    var getPrevDataQuery = $@"SELECT VOUCHER_NO,SESSION_ROWID, CREATED_BY, CREATED_DATE FROM MASTER_TRANSACTION WHERE VOUCHER_NO = '{voucherno}'";
                    var defaultData = this._objectEntity.SqlQuery<SalesOrderDetail>(getPrevDataQuery).ToList();
                    var defaultCol = "MODIFY_BY,MODIFY_DATE";
                    string createdDateForEdit = string.Empty, createdByForEdit = string.Empty, voucherNoForEdit = string.Empty;
                    var sessionRowIDForedit = 0;
                    foreach (var def in defaultData)
                    {
                        voucherNoForEdit = def.VOUCHER_NO.ToString();
                        createdDateForEdit = "TO_DATE('" + def.CREATED_DATE.ToString() + "', 'MM-DD-YYYY hh12:mi:ss pm')";
                        createdByForEdit = def.CREATED_BY.ToString().ToUpper();
                        ///sessionRowIDForedit = Convert.ToInt32(def.SESSION_ROWID);
                    }
                    Columnbuilder.Append(model.Child_COLUMNS);
                    Columnbuilder.Append(staticsalesordercolumns);
                    foreach (var v in mastercolumn)
                    {
                        if (v.Key == primarycolname)
                        {
                            primarycolumn = v.Value.ToString();
                        }
                        string lastName = v.Key.Split('_').Last();
                        if (lastName == "DATE")
                        {
                            if (v.Value.ToString() == "")
                            {
                                valuesbuilder.Append("SYSDATE").Append(",");
                            }
                            else
                            {
                                if (v.Key == primarydatecolumn)
                                {
                                    primarydate = v.Value.ToString();
                                    VoucherDate = "trunc(TO_DATE(" + "'" + primarydate + "'" + ",'DD-MON-YYYY hh24:mi:ss'))";
                                    valuesbuilder.Append("TO_DATE(" + "'" + primarydate + "'" + ",'DD-MON-YYYY hh24:mi:ss')").Append(",");
                                }
                                else
                                {
                                    valuesbuilder.Append("TO_DATE(" + "'" + v.Value + "'" + ",'DD-MON-YYYY hh24:mi:ss')").Append(",");
                                }
                            }
                        }
                        else if (v.Key.ToString() == primarycolname)
                        {
                            if (v.Value.ToString() == "")
                            {
                                valuesbuilder.Append("'" + newvoucherNo + "'").Append(",");
                            }
                            else
                            {
                                valuesbuilder.Append("'" + v.Value + "'").Append(",");
                            }
                        }
                        else if (v.Key.ToString() == "MANUAL_NO")
                        {
                            valuesbuilder.Append("'" + v.Value + "'").Append(",");
                            manualno = v.Value.ToString();
                        }
                        else if (v.Key.ToString() == "CURRENCY_CODE")
                        {
                            valuesbuilder.Append("'" + v.Value + "'").Append(",");
                            currencyformat = v.Value.ToString();
                        }
                        else if (v.Key.ToString() == "EXCHANGE_RATE")
                        {
                            valuesbuilder.Append("'" + v.Value + "'").Append(",");
                            exchangrate = Convert.ToDecimal(v.Value.ToString());
                        }
                        else { valuesbuilder.Append("'" + v.Value + "'").Append(","); }
                    }
                    int serialno = 1;
                    if (voucherno == "undefined")
                    {
                        foreach (var item in childcolumnvalues)
                        {
                            StringBuilder childvaluesbuilder = new StringBuilder();
                            StringBuilder masterchildvaluesbuilder = new StringBuilder();
                            var itemarray = JsonConvert.DeserializeObject(item.ToString());
                            var budget_flag = "L";
                            foreach (var data in itemarray)
                            {
                                var dataname = data.Name.ToString();
                                string[] datanamesplit = dataname.Split('_');
                                string datalastName = datanamesplit.Last();
                                var datavalue = data.Value;
                                if (datalastName == "DATE")
                                {
                                    if (datavalue.Value.ToString() == "")
                                    {
                                        childvaluesbuilder.Append("SYSDATE").Append(",");
                                    }
                                    else
                                    {
                                        childvaluesbuilder.Append("TO_DATE(" + "'" + datavalue.Value + "'" + ",'DD-MON-YYYY hh24:mi:ss')").Append(",");
                                    }
                                }
                                else if (datalastName == "PRICE")
                                {
                                    if (dataname == "UNIT_PRICE")
                                    {
                                        if (datavalue.Value == null)
                                        {
                                            childvaluesbuilder.Append("0.00").Append(",");
                                        }
                                        else
                                        {
                                            childvaluesbuilder.Append(datavalue.Value).Append(",");
                                        }
                                    }
                                    if (dataname == "TOTAL_PRICE")
                                    {
                                        if (datavalue.Value == null)
                                        {
                                            childvaluesbuilder.Append("0.00").Append(",");

                                        }
                                        else
                                        {
                                            childvaluesbuilder.Append(datavalue.Value).Append(",");
                                        }
                                    }
                                    else if (dataname == "CALC_UNIT_PRICE")
                                    {
                                        if (datavalue.Value.ToString() == "")
                                        {
                                            childvaluesbuilder.Append("0.00").Append(",");
                                        }
                                        else
                                        {
                                            childvaluesbuilder.Append(datavalue.Value).Append(",");
                                        }
                                    }
                                    else if (dataname == "CALC_TOTAL_PRICE")
                                    {
                                        if (datavalue.Value.ToString() == "")
                                        {
                                            childvaluesbuilder.Append("0.00").Append(",");
                                        }
                                        else
                                        {
                                            childvaluesbuilder.Append(datavalue.Value).Append(",");
                                        }
                                    }
                                }
                                else if (dataname == "AMOUNT")
                                {
                                    if (datavalue.Value.ToString() == "")
                                    {
                                        childvaluesbuilder.Append("0.00").Append(",");
                                    }
                                    else
                                    {
                                        childvaluesbuilder.Append(datavalue.Value).Append(",");
                                    }
                                }
                                else if (dataname == "QUANTITY")
                                {
                                    if (datavalue.Value == null)
                                    {
                                        childvaluesbuilder.Append(quantityvalue).Append(",");
                                    }
                                    else
                                    {
                                        childvaluesbuilder.Append(datavalue.Value).Append(",");
                                    }

                                }
                                else if (dataname == "CALC_QUANTITY")
                                {
                                    if (datavalue.Value == null)
                                    {
                                        childvaluesbuilder.Append(quantityvalue).Append(",");
                                    }
                                    else
                                    {
                                        childvaluesbuilder.Append(datavalue.Value).Append(",");
                                    }
                                }
                                else if (dataname == "COMPLETED_QUANTITY")
                                {

                                    if (datavalue.Value == null)
                                    {
                                        childvaluesbuilder.Append(quantityvalue).Append(",");
                                    }
                                    else
                                    {
                                        childvaluesbuilder.Append(datavalue.Value).Append(",");
                                    }
                                }

                                else if (dataname == "BUDGET_FLAG" && datavalue.Value.ToString() == "")
                                {
                                    if (datavalue.Value.ToString() == "")
                                    {
                                        childvaluesbuilder.Append("'" + budget_flag + "'").Append(",");
                                    }
                                    else
                                    {
                                        childvaluesbuilder.Append("'" + datavalue.Value + "'").Append(",");
                                    }
                                }
                                else if (dataname == "PARTICULARS")
                                {
                                    if (datavalue.Value.ToString() == null)
                                    {
                                        childvaluesbuilder.Append("' '").Append(",");
                                    }
                                    else
                                    {
                                        childvaluesbuilder.Append("'" + datavalue.Value + "'").Append(",");
                                    }
                                }

                                else
                                {
                                    if (datavalue.Value.ToString() == "")
                                    {
                                        childvaluesbuilder.Append("' '").Append(",");

                                    }
                                    else
                                    {
                                        childvaluesbuilder.Append("'" + datavalue.Value + "'").Append(",");
                                    }
                                }
                            }
                            var values = "";
                            masterchildvaluesbuilder.Append(valuesbuilder);
                            masterchildvaluesbuilder.Append(childvaluesbuilder);
                            values = masterchildvaluesbuilder.ToString().TrimEnd(',');
                            var insertQuery = string.Format(@"insert into " + model.Table_Name + "({0}) values({1},{2},{3},{4},{5},{6},{7},{8})", Columnbuilder, values, serialno, "'" + model.Form_Code + "'", "'" + this._workContext.CurrentUserinformation.company_code + "'", "'" + this._workContext.CurrentUserinformation.branch_code + "'", "'" + this._workContext.CurrentUserinformation.login_code.ToUpper() + "'", createddatestring, "'N'");
                            this._objectEntity.ExecuteSqlCommand(insertQuery);
                            insertmaintable = true;
                            masterchildvaluesbuilder.Length = 0;
                            masterchildvaluesbuilder.Capacity = 0;
                            childvaluesbuilder.Length = 0;
                            childvaluesbuilder.Capacity = 0;
                            serialno++;
                        }
                        if (insertmaintable == true)
                        {
                            string insertmasterQuery = string.Format(@"INSERT INTO MASTER_TRANSACTION(VOUCHER_NO,VOUCHER_AMOUNT,FORM_CODE,COMPANY_CODE,BRANCH_CODE,CREATED_BY,DELETED_FLAG,CURRENCY_CODE,CREATED_DATE,VOUCHER_DATE,SYN_ROWID,EXCHANGE_RATE) VALUES('{0}','{1}','{2}','{3}','{4}','{5}','{6}','{7}',TO_DATE('{8}','DD-MON-YYYY hh24:mi:ss'),{9},'{10}',{11})",
                              newvoucherNo, grandtotal, model.Form_Code, this._workContext.CurrentUserinformation.company_code, this._workContext.CurrentUserinformation.branch_code, this._workContext.CurrentUserinformation.login_code.ToUpper(), 'N', currencyformat, today, VoucherDate, manualno, exchangrate);
                            this._objectEntity.ExecuteSqlCommand(insertmasterQuery);
                            if (!string.IsNullOrEmpty(model.TempCode))
                            {
                                string UpdateQuery = $@"UPDATE FORM_TEMPLATE_SETUP  SET SAVED_DRAFT='Y' WHERE TEMPLATE_NO='{model.TempCode}'  AND  COMPANY_CODE ='{this._workContext.CurrentUserinformation.company_code}' AND BRANCH_CODE='{ this._workContext.CurrentUserinformation.branch_code}'";
                                this._objectEntity.ExecuteSqlCommand(UpdateQuery);
                            }
                            insertmastertable = true;
                        }
                        if (model.BUDGET_TRANS_VALUE != null)
                        {
                            dynamic childbudgetcentervalues = JsonConvert.DeserializeObject(model.BUDGET_TRANS_VALUE);
                            foreach (var item in childbudgetcentervalues)
                            {
                                var cbcitemarray = JsonConvert.DeserializeObject(item.ToString());
                                string budgetflag = string.Empty, locationcode = string.Empty;
                                foreach (var cbcdata in cbcitemarray)
                                {
                                    var cbcdataname = cbcdata.Name.ToString();
                                    var cbcdatavalue = cbcdata.Value;
                                    if (cbcdataname == "BUDGET_FLAG")
                                    {
                                        if (cbcdatavalue != "")
                                        {
                                            budgetflag = cbcdatavalue.ToString();
                                        }
                                        else
                                        {
                                            budgetflag = "L";
                                        }
                                    }
                                    else if (cbcdataname == "LOCATION_CODE")
                                    {
                                        locationcode = cbcdatavalue.ToString();
                                    }
                                    else if (cbcdataname == "BUDGET")
                                    {
                                        dynamic budgetdatas = JsonConvert.DeserializeObject(cbcdatavalue.ToString());
                                        var budgetserialno = 1;
                                        foreach (var budgetdata in budgetdatas)
                                        {
                                            string budgetval = string.Empty, acccode = string.Empty, quantity = string.Empty;
                                            var amount = 0.00;
                                            var budgetarray = JsonConvert.DeserializeObject(budgetdata.ToString());
                                            foreach (var bdata in budgetarray)
                                            {
                                                var taname = bdata.Name.ToString();
                                                var tavalue = bdata.Value.ToString();
                                                if (taname == "BUDGET_VAL")
                                                {
                                                    budgetval = tavalue.ToString();
                                                }
                                                else if (taname == "QUANTITY" && tavalue != "")
                                                {
                                                    quantity = tavalue.ToString();
                                                }
                                                else if (taname == "ACCOUNT_ALLOCATION")
                                                {
                                                    acccode = tavalue.ToString();
                                                }

                                            }
                                            if (budgetval != "")
                                            {
                                                string query = string.Format(@"SELECT TO_NUMBER((MAX(TO_NUMBER(TRANSACTION_NO))+1)) as TRANSACTIONNO FROM BUDGET_TRANSACTION");
                                                int newtransno = this._objectEntity.SqlQuery<int>(query).FirstOrDefault();
                                                string insertbudgettransQuery = $@"INSERT INTO BUDGET_TRANSACTION(
                                                              TRANSACTION_NO,FORM_CODE,REFERENCE_NO,BUDGET_FLAG,SERIAL_NO,BUDGET_CODE,
                                                              BUDGET_AMOUNT,COMPANY_CODE,BRANCH_CODE,CREATED_BY,CREATED_DATE,DELETED_FLAG,
                                                              CURRENCY_CODE,EXCHANGE_RATE,VALIDATION_FLAG,ACC_CODE)
                                                              VALUES('{newtransno}','{model.Form_Code}','{newvoucherNo}','{budgetflag}',{budgetserialno},'{budgetval}',
                                                             '{amount}','{this._workContext.CurrentUserinformation.company_code}','{this._workContext.CurrentUserinformation.branch_code}','{this._workContext.CurrentUserinformation.login_code.ToUpper()}',SYSDATE,'N','{currencyformat}',{exchangrate},'Y','{acccode}')";
                                                this._objectEntity.ExecuteSqlCommand(insertbudgettransQuery);
                                                budgetserialno++;
                                            }
                                        }
                                    }
                                }
                            }
                        }
                        if (customcolumn.Count > 0)
                        {
                            foreach (var r in customcolumn)
                            {
                                string insertQuery = $@"INSERT INTO CUSTOM_TRANSACTION(
                                                              VOUCHER_NO,FIELD_NAME,FIELD_VALUE,FORM_CODE,COMPANY_CODE,BRANCH_CODE,CREATED_BY,CREATED_DATE,DELETED_FLAG)
                                                              VALUES('{newvoucherNo}','{r.Key.ToString()}','{r.Value.ToString()}','{model.Form_Code}','{this._workContext.CurrentUserinformation.company_code}','{this._workContext.CurrentUserinformation.branch_code}',
                                                         '{this._workContext.CurrentUserinformation.login_code}',SYSDATE,'N')";
                                this._objectEntity.ExecuteSqlCommand(insertQuery);
                            }
                        }
                        #region CLEAR CACHE
                        List<string> keystart = new List<string>();
                        keystart.Add("GetAllMenuItems");
                        keystart.Add("GetAllSalesOrderDetails");
                        keystart.Add("GetSalesOrderDetailFormDetailByFormCodeAndOrderNo");
                        keystart.Add("GetAllOrederNoByFlter");
                        keystart.Add("GetSubMenuList");
                        keystart.Add("GetSubMenuDetailList");
                        List<string> Record = new List<string>();
                        Record = this._cacheManager.GetAllKeys();
                        this._cacheManager.RemoveCacheByKey(keystart, Record);
                        #endregion
                        if (model.Save_Flag == "0")
                        {
                            trans.Commit();
                            return Request.CreateResponse(HttpStatusCode.OK, new { MESSAGE = "INSERTED", STATUS_CODE = (int)HttpStatusCode.OK, VoucherNo = newvoucherNo, VoucherDate = primarydate, FormCode = model.Form_Code });
                        }
                        else if (model.Save_Flag == "1")
                        {
                            trans.Commit();
                            return Request.CreateResponse(HttpStatusCode.OK, new { MESSAGE = "SAVEANDCONTINUE", STATUS_CODE = (int)HttpStatusCode.OK, VoucherNo = newvoucherNo, VoucherDate = primarydate, FormCode = model.Form_Code });
                        }
                        else
                        {
                            trans.Commit();
                            return Request.CreateResponse(HttpStatusCode.OK, new { MESSAGE = "SAVENPRINT", STATUS_CODE = (int)HttpStatusCode.OK, VoucherNo = newvoucherNo, VoucherDate = primarydate, FormCode = model.Form_Code });
                        }
                    }
                    //for updates 
                    else
                    {
                        //delete maintable
                        string deletequery = string.Format(@"DELETE FROM " + model.Table_Name + " where " + primarycolname + "='{0}' and COMPANY_CODE='{1}'", voucherno, this._workContext.CurrentUserinformation.company_code);
                        this._objectEntity.ExecuteSqlCommand(deletequery);
                        //for insert into main table
                        int updateserialno = 1;
                        foreach (var item in childcolumnvalues)
                        {
                            StringBuilder childvaluesbuilder = new StringBuilder();
                            StringBuilder masterchildvaluesbuilder = new StringBuilder();
                            var itemarray = JsonConvert.DeserializeObject(item.ToString());
                            var budget_flag = "L";
                            foreach (var data in itemarray)
                            {
                                var dataname = data.Name.ToString();
                                string[] datanamesplit = dataname.Split('_');
                                string datalastName = datanamesplit.Last();
                                var datavalue = data.Value;
                                if (datalastName == "DATE")
                                {
                                    if (datavalue.Value.ToString() == "")
                                    {
                                        childvaluesbuilder.Append("SYSDATE").Append(",");
                                    }
                                    else
                                    {
                                        childvaluesbuilder.Append("TO_DATE(" + "'" + datavalue.Value + "'" + ",'DD-MON-YYYY hh24:mi:ss')").Append(",");
                                    }
                                }
                                else if (datalastName == "PRICE")
                                {
                                    if (dataname == "UNIT_PRICE")
                                    {

                                        if (datavalue.Value == null)
                                        {
                                            childvaluesbuilder.Append("0.00").Append(",");
                                        }
                                        else
                                        {
                                            childvaluesbuilder.Append(datavalue.Value).Append(",");
                                        }
                                    }
                                    if (dataname == "TOTAL_PRICE")
                                    {
                                        if (datavalue.Value == null)
                                        {
                                            childvaluesbuilder.Append("0.00").Append(",");
                                        }
                                        else
                                        {
                                            childvaluesbuilder.Append(datavalue.Value).Append(",");
                                        }

                                    }
                                    else if (dataname == "CALC_UNIT_PRICE")
                                    {
                                        if (datavalue.Value.ToString() == "")
                                        {
                                            childvaluesbuilder.Append("0.00").Append(",");
                                        }
                                        else
                                        {
                                            childvaluesbuilder.Append(datavalue.Value).Append(",");
                                        }
                                    }
                                    else if (dataname == "CALC_TOTAL_PRICE")
                                    {
                                        if (datavalue.Value.ToString() == "")
                                        {
                                            childvaluesbuilder.Append("0.00").Append(",");
                                        }
                                        else
                                        {
                                            childvaluesbuilder.Append(datavalue.Value).Append(",");
                                        }
                                    }
                                }
                                else if (dataname == "AMOUNT")
                                {
                                    if (datavalue.Value.ToString() == "")
                                    {
                                        childvaluesbuilder.Append("0.00").Append(",");
                                    }
                                    else
                                    {
                                        childvaluesbuilder.Append(datavalue.Value).Append(",");
                                    }

                                }
                                else if (datalastName == "QUANTITY")
                                {
                                    if (datavalue.Value == null)
                                    {
                                        childvaluesbuilder.Append("0.00").Append(",");
                                    }
                                    else
                                    {
                                        childvaluesbuilder.Append(datavalue.Value).Append(",");
                                    }

                                }
                                else if (dataname == "CALC_QUANTITY")
                                {

                                    if (datavalue.Value == null)
                                    {
                                        childvaluesbuilder.Append("0.00").Append(",");
                                    }
                                    else
                                    {
                                        childvaluesbuilder.Append(datavalue.Value).Append(",");
                                    }
                                }
                                else if (dataname == "BUDGET_FLAG" && datavalue.Value.ToString() == "")
                                {
                                    if (datavalue.Value.ToString() == "")
                                    {
                                        childvaluesbuilder.Append("'" + budget_flag + "'").Append(",");
                                    }
                                    else
                                    {
                                        childvaluesbuilder.Append("'" + datavalue.Value + "'").Append(",");
                                    }
                                }
                                else if (dataname == "PARTICULARS")
                                {
                                    if (datavalue.Value == null)
                                    {
                                        childvaluesbuilder.Append("' '").Append(",");
                                    }
                                    else
                                    {
                                        childvaluesbuilder.Append("'" + datavalue.Value + "'").Append(",");
                                    }
                                }
                                else if (dataname == "REMARKS")
                                {
                                    if (datavalue.Value == null)
                                    {
                                        childvaluesbuilder.Append("' '").Append(",");
                                    }
                                    else
                                    {
                                        childvaluesbuilder.Append("'" + datavalue.Value + "'").Append(",");
                                    }
                                }
                                else
                                {
                                    if (datavalue.Value.ToString() == "")
                                    {
                                        childvaluesbuilder.Append("' '").Append(",");
                                    }
                                    else
                                    {
                                        childvaluesbuilder.Append("'" + datavalue.Value + "'").Append(",");
                                    }
                                }

                            }
                            var values = "";
                            masterchildvaluesbuilder.Append(valuesbuilder);
                            masterchildvaluesbuilder.Append(childvaluesbuilder);
                            values = masterchildvaluesbuilder.ToString().TrimEnd(',');
                            var insertQuery = $@"insert into {model.Table_Name} ({Columnbuilder},{defaultCol}) values({values},{updateserialno},'{model.Form_Code}','{this._workContext.CurrentUserinformation.company_code}','{this._workContext.CurrentUserinformation.branch_code}','{createdByForEdit}',{createdDateForEdit},'N','{this._workContext.CurrentUserinformation.login_code.ToUpper()}',SYSDATE)";
                            this._objectEntity.ExecuteSqlCommand(insertQuery);
                            insertmaintable = true;
                            masterchildvaluesbuilder.Length = 0;
                            masterchildvaluesbuilder.Capacity = 0;
                            childvaluesbuilder.Length = 0;
                            childvaluesbuilder.Capacity = 0;
                            updateserialno++;
                        }
                        //update master table 
                        string query = $@"UPDATE MASTER_TRANSACTION SET VOUCHER_AMOUNT='{ model.Grand_Total}',MODIFY_BY = '{this._workContext.CurrentUserinformation.login_code}' , MODIFY_DATE = SYSDATE,SYN_ROWID='{manualno}',CURRENCY_CODE='{currencyformat}',EXCHANGE_RATE={exchangrate}   where VOUCHER_NO = '{voucherno}' and COMPANY_CODE IN ({this._workContext.CurrentUserinformation.company_code})";
                        var rowCount = _objectEntity.ExecuteSqlCommand(query);
                        //update budget transaction
                        string deletebudgetcenterquery = string.Format(@"DELETE FROM BUDGET_TRANSACTION where REFERENCE_NO='{0}' and COMPANY_CODE='{1}'", voucherno, this._workContext.CurrentUserinformation.company_code);
                        this._objectEntity.ExecuteSqlCommand(deletebudgetcenterquery);
                        if (model.BUDGET_TRANS_VALUE != null)
                        {
                            dynamic childbudgetcentervalues = JsonConvert.DeserializeObject(model.BUDGET_TRANS_VALUE);
                            foreach (var item in childbudgetcentervalues)
                            {
                                var cbcitemarray = JsonConvert.DeserializeObject(item.ToString());
                                var budgetflag = "";
                                var locationcode = "";
                                foreach (var cbcdata in cbcitemarray)
                                {
                                    var cbcdataname = cbcdata.Name.ToString();
                                    var cbcdatavalue = cbcdata.Value;
                                    if (cbcdataname == "BUDGET_FLAG")
                                    {
                                        if (cbcdatavalue != "")
                                        {
                                            budgetflag = cbcdatavalue.ToString();
                                        }
                                        else
                                        {
                                            budgetflag = "L";
                                        }
                                    }
                                    else if (cbcdataname == "LOCATION_CODE")
                                    {
                                        locationcode = cbcdatavalue.ToString();
                                    }
                                    else if (cbcdataname == "BUDGET")
                                    {
                                        dynamic budgetdatas = JsonConvert.DeserializeObject(cbcdatavalue.ToString());
                                        var budgetserialno = 1;
                                        foreach (var budgetdata in budgetdatas)
                                        {
                                            string budgetval = string.Empty, acccode = string.Empty;
                                            var amount = 0.00;
                                            var budgetarray = JsonConvert.DeserializeObject(budgetdata.ToString());
                                            foreach (var bdata in budgetarray)
                                            {
                                                var taname = bdata.Name.ToString();
                                                var tavalue = bdata.Value.ToString();
                                                if (taname == "BUDGET_VAL")
                                                {
                                                    budgetval = tavalue.ToString();
                                                }
                                                else if (taname == "ACCOUNT_ALLOCATION")
                                                {
                                                    acccode = tavalue.ToString();
                                                }
                                            }
                                            if (budgetval != "")
                                            {
                                                string transquery = string.Format(@"SELECT TO_NUMBER((MAX(TO_NUMBER(TRANSACTION_NO))+1)) as TRANSACTIONNO FROM BUDGET_TRANSACTION");
                                                int newtransno = this._objectEntity.SqlQuery<int>(transquery).FirstOrDefault();
                                                string insertbudgettransQuery = $@"INSERT INTO BUDGET_TRANSACTION(
                                                              TRANSACTION_NO,FORM_CODE,REFERENCE_NO,BUDGET_FLAG,SERIAL_NO,BUDGET_CODE,
                                                              BUDGET_AMOUNT,COMPANY_CODE,BRANCH_CODE,CREATED_BY,CREATED_DATE,DELETED_FLAG,
                                                              CURRENCY_CODE,EXCHANGE_RATE,VALIDATION_FLAG,ACC_CODE,MODIFY_BY,MODIFY_DATE)
                                                              VALUES('{newtransno}','{model.Form_Code}','{voucherno}','{budgetflag}',{budgetserialno},'{budgetval}',
                                                                  '{amount}','{this._workContext.CurrentUserinformation.company_code}','{this._workContext.CurrentUserinformation.branch_code}','{createdByForEdit}',{createdDateForEdit},'N','{currencyformat}',{exchangrate},'Y','{acccode}','{this._workContext.CurrentUserinformation.login_code}',SYSDATE)";
                                                this._objectEntity.ExecuteSqlCommand(insertbudgettransQuery);
                                                budgetserialno++;
                                            }
                                        }
                                    }
                                }
                            }
                        }
                        string deletecustomcolumn = string.Format(@"DELETE FROM CUSTOM_TRANSACTION where VOUCHER_NO='{0}' and COMPANY_CODE='{1}'", voucherno, this._workContext.CurrentUserinformation.company_code);
                        this._objectEntity.ExecuteSqlCommand(deletecustomcolumn);
                        if (customcolumn.Count > 0)
                        {
                            foreach (var r in customcolumn)
                            {
                                string insertQuery = $@"INSERT INTO CUSTOM_TRANSACTION(
                                                              VOUCHER_NO,FIELD_NAME,FIELD_VALUE,FORM_CODE,COMPANY_CODE,BRANCH_CODE,CREATED_BY,CREATED_DATE,DELETED_FLAG,MODIFY_DATE,MODIFY_BY)
                                                              VALUES('{voucherno}','{r.Key.ToString()}','{r.Value.ToString()}','{model.Form_Code}','{this._workContext.CurrentUserinformation.company_code}','{this._workContext.CurrentUserinformation.branch_code}',
                                                             '{createdByForEdit}',{createdDateForEdit},'N',SYSDATE,'{this._workContext.CurrentUserinformation.login_code}')";
                                this._objectEntity.ExecuteSqlCommand(insertQuery);
                            }
                        }
                        #region CLEAR CACHE
                        List<string> keystart = new List<string>();
                        keystart.Add("GetAllMenuItems");
                        keystart.Add("GetAllSalesOrderDetails");
                        keystart.Add("GetSalesOrderDetailFormDetailByFormCodeAndOrderNo");
                        keystart.Add("GetAllOrederNoByFlter");
                        keystart.Add("GetSubMenuList");
                        keystart.Add("GetSubMenuDetailList");
                        List<string> Record = new List<string>();
                        Record = this._cacheManager.GetAllKeys();
                        this._cacheManager.RemoveCacheByKey(keystart, Record);
                        #endregion
                        if (model.Save_Flag == "0")
                        {
                            trans.Commit();
                            return Request.CreateResponse(HttpStatusCode.OK, new { MESSAGE = "UPDATED", STATUS_CODE = (int)HttpStatusCode.OK, VoucherNo = voucherno, VoucherDate = primarydate, FormCode = model.Form_Code });
                        }
                        else if (model.Save_Flag == "4")
                        {
                            trans.Commit();
                            return Request.CreateResponse(HttpStatusCode.OK, new { MESSAGE = "UPDATEDANDPRINT", STATUS_CODE = (int)HttpStatusCode.OK, VoucherNo = voucherno, VoucherDate = primarydate, FormCode = model.Form_Code });
                        }
                        else
                        {
                            trans.Rollback();
                            return Request.CreateResponse(HttpStatusCode.InternalServerError, new { MESSAGE = "ERROR", STATUS_CODE = (int)HttpStatusCode.InternalServerError });
                        }
                    }
                }
                catch (Exception ex)
                {
                    trans.Rollback();
                    return Request.CreateResponse(HttpStatusCode.InternalServerError, new { MESSAGE = "ERROR", STATUS_CODE = (int)HttpStatusCode.InternalServerError });
                }
            }
        }
        [HttpPost]
        public HttpResponseMessage SaveInventoryFormData(FormDetails model)
        {
            _logErp.InfoInFile("Inventory voucher Form data save method started=============");
            using (var trans = _objectEntity.Database.BeginTransaction())
            {
                try
                {
                    var voucherno = model.Order_No;

                    // This For Get Primary Date Column from all table
                    var primarydatecolumn = _FormTemplateRepo.GetPrimaryDateByTableName(model.Table_Name);

                    // This For Ger Primary column Etc VoucherNo,Sales_no
                    var primarycolname = _contravoucher.GetPrimaryColumnByTableName(model.Table_Name);

                    string primarydate = string.Empty, primarycolumn = string.Empty, today = DateTime.Now.ToString("dd-MMM-yyyy"), createddatestring = "SYSDATE", todaystring = System.DateTime.Now.ToString("yyyyMMddHHmmss"), manualno = string.Empty, currencyformat = "NRS", VoucherDate = createddatestring, grandtotal = model.Grand_Total;
                    // Get New VoucherNo from Database
                    string newvoucherNo = _FormTemplateRepo.NewVoucherNo(this._workContext.CurrentUserinformation.company_code, model.Form_Code, today, model.Table_Name);
                    decimal exchangrate = 1;
                    var VoucherNumberGeneratedNo = string.Empty;
                    //DeserializeObject Master 
                    var masterColumn = _inventoryvoucher.MapMasterColumnWithValue(model.Master_COLUMN_VALUE);
                    //DeserializeObject Child 
                    var childColumn = _inventoryvoucher.MapChildColumnWithValue(model.Child_COLUMN_VALUE);
                    //var customTran = _inventoryvoucher.MapCustomTransactionWithValue(model.Custom_COLUMN_VALUE);
                    var customColumn = new Newtonsoft.Json.Linq.JObject();
                    var customColList = new List<CustomOrderColumn>();
                    //DeserializeObject Customer value
                    if (model.Custom_COLUMN_VALUE != null)
                    {

                        customColList = _inventoryvoucher.MapCustomTransactionWithValue(model.Custom_COLUMN_VALUE);
                    }
                    var budgetTransaction = _inventoryvoucher.MapBudgetTransactionColumnValue(model.BUDGET_TRANS_VALUE);
                    var batchTransaction = _inventoryvoucher.MapBatchTransactionValue(model.SERIAL_TRACKING_VALUE);
                    var batchTransValues = _inventoryvoucher.MapBatchTransValue(model.BATCH_TRACKING_VALUE);
                    var charges = _inventoryvoucher.MapChargesColumnWithValue(model.CHARGES);
                    string createdDateForEdit = string.Empty, createdByForEdit = string.Empty, voucherNoForEdit = string.Empty;
                    bool insertedToChild = false;
                    bool insertedToMaster = false;
                    var msg = string.Empty;
                    var sessionRowIDForedit = 0;
                    var shippingDetailValues = _inventoryvoucher.MapShippingDetailsColumnValue(model.SHIPPING_DETAILS_VALUE);
                    _logErp.WarnInDB("Shipping details for sales order : " + shippingDetailValues);
                    if (voucherno == "undefined")
                    {
                        //string updateTransactionNo = _FormTemplateRepo.NewVoucherNo(this._workContext.CurrentUserinformation.company_code, model.Form_Code, DateTime.Now.ToString("dd-MMM-yyyy"), model.Table_Name);
                        VoucherNumberGeneratedNo = newvoucherNo;
                        var commonValue = new CommonFieldsForInventory
                        {
                            FormCode = model.Form_Code,
                            TableName = model.Table_Name,
                            ExchangeRate = exchangrate,
                            CurrencyFormat = currencyformat,
                            VoucherNumber = voucherno,
                            NewVoucherNumber = VoucherNumberGeneratedNo,
                            TempCode = model.TempCode,
                            VoucherDate = VoucherDate,
                            Grand_Total = model.Grand_Total,
                            FormRef = model.FROM_REF,
                            ManualNumber = masterColumn.MANUAL_NO,
                            MODULE_CODE=model.MODULE_CODE
                        };
                        masterColumn.CREATED_BY =  _workContext.CurrentUserinformation.login_code.ToUpper();
                        if (childColumn.Count > 0)
                        {
                            insertedToChild = _inventoryvoucher.SaveChildColumnValue(childColumn, masterColumn, commonValue, model, primarydatecolumn, primarycolname,_objectEntity);
                           
                        }
                        if (insertedToChild)
                        {
                            insertedToMaster = _inventoryvoucher.SaveMasterColumnValue(masterColumn, commonValue,_objectEntity);
                        }
                        if (commonValue.FormRef)
                        {
                            foreach (var refa in model.REF_MODEL)
                            {
                                var childData = childColumn.Where(x => x.ITEM_CODE == refa.ITEM_CODE).FirstOrDefault();
                                //refa.SERIAL_NO = childData.SERIAL_NO.ToString();
                                refa.Qty = childData.QUANTITY;
                                refa.calc_qty = childData.CALC_QUANTITY;
                                refa.cal_unit_price = childData.CALC_UNIT_PRICE;
                                refa.cal_total_price = childData.CALC_TOTAL_PRICE;
                                refa.Total_price = childData.TOTAL_PRICE??0;
                                refa.SUB_PROJECT_CODE = childData.SUB_PROJECT_CODE;
                            }

                            //_inventoryvoucher.GetFormReference(childColumn, masterColumn, commonValue, model, primarydatecolumn, primarycolname, _objectEntity);
                            _inventoryvoucher.GetFormReference(commonValue,model.REF_MODEL, _objectEntity);
                        }
                        if (budgetTransaction != null)
                        {
                            for (int i = 0; i < budgetTransaction.Count ; i++)
                            {
                                if (budgetTransaction[i].BUDGET != null)
                                {
                                    budgetTransaction[i].SUB_PROJECT_CODE = childColumn[i].SUB_PROJECT_CODE;
                                }
                            }

                            _inventoryvoucher.SaveBudgetTransactionColumnValue(budgetTransaction, commonValue, _objectEntity);
                            //_inventoryvoucher.SaveBudgetTransactionColumnValue(budgetTransaction, commonValue, _objectEntity);
                        }

                        if (batchTransaction != null)
                        {
                            _inventoryvoucher.SaveBatchTransactionValues(masterColumn,batchTransaction, commonValue, _objectEntity);
                        }
                        if (batchTransValues != null)
                        {
                            _inventoryvoucher.SaveBatchTransValues(childColumn,masterColumn, batchTransValues, commonValue, _objectEntity);
                        }
                        if (charges != null)
                        {
                            _inventoryvoucher.SaveChargeColumnValue(charges, commonValue, _objectEntity);
                        }
                        if (customColList.Count > 0)
                        {
                            _inventoryvoucher.SaveCustomTransaction(customColList, commonValue, _objectEntity);
                        }
                        if (shippingDetailValues != null)
                        {
                            _inventoryvoucher.SaveShippingDetailsColumnValue(shippingDetailValues, commonValue, _objectEntity);
                        }
                        #region CLEAR CACHE
                        List<string> keystart = new List<string>();
                        keystart.Add("GetAllMenuItems");
                        keystart.Add("GetAllSalesOrderDetails");
                        keystart.Add("GetSalesOrderDetailFormDetailByFormCodeAndOrderNo");
                        keystart.Add("GetAllOrederNoByFlter");
                        keystart.Add("GetSubMenuList");
                        keystart.Add("GetSubMenuDetailList");
                        List<string> Record = new List<string>();
                        Record = this._cacheManager.GetAllKeys();
                        this._cacheManager.RemoveCacheByKey(keystart, Record);
                        #endregion
                        if (model.Save_Flag == "0" && insertedToChild == true && insertedToMaster == true)
                        {
                            trans.Commit();
                            msg = "INSERTED";
                        }
                        else if (model.Save_Flag == "3" && insertedToChild == true && insertedToMaster == true)
                        {
                            trans.Commit();
                            msg = "SAVEANDPRINT";
                        }
                        else
                        {
                            trans.Commit();
                            msg = "INSERTEDANDCONTINUE";
                        }
                    }
                    //for updates 
                    else
                    {
                        bool updatedChild = false;
                        bool updatedMaster = false;

                        var defaultData = _inventoryvoucher.GetMasterTransactionByVoucherNo(voucherno);
                        foreach (var def in defaultData)
                        {
                            voucherNoForEdit = def.VOUCHER_NO.ToString();
                            createdDateForEdit = "TO_DATE('" + def.CREATED_DATE.ToString() + "', 'DD-MON-YY')";
                            //createdDateForEdit = "TO_DATE('" + def.CREATED_DATE.ToString() + "', 'DD-MON-YY')";
                            createdByForEdit = def.CREATED_BY.ToString().ToUpper();
                            //sessionRowIDForedit = Convert.ToInt32(def.SESSION_ROWID);
                        }
                        var commonUpdateValue = new CommonFieldsForInventory
                        {
                            FormCode = model.Form_Code,
                            TableName = model.Table_Name,
                            ExchangeRate = exchangrate,
                            CurrencyFormat = currencyformat,
                            VoucherNumber = voucherno,
                            NewVoucherNumber = voucherno,
                            TempCode = model.TempCode,
                            VoucherDate = createdDateForEdit,
                            PrimaryColumn = primarycolname,
                            PrimaryDateColumn = primarydatecolumn,
                            DrTotal = Convert.ToDecimal(model.Grand_Total),
                            ManualNumber = masterColumn.MANUAL_NO,
                            MODULE_CODE = model.MODULE_CODE

                        };
                        masterColumn.MODIFY_DATE = VoucherDate;
                        masterColumn.MODIFY_BY = _workContext.CurrentUserinformation.login_code.ToUpper();
                        masterColumn.CREATED_BY = createdByForEdit;

                        if (updatedChild)
                        {
                            _inventoryvoucher.DeleteChildTransaction(commonUpdateValue);
                            updatedMaster = _inventoryvoucher.UpdateMasterTransaction(commonUpdateValue,_objectEntity);
                        }
                        if (budgetTransaction != null)
                        {
                            _inventoryvoucher.DeleteBudgetTransaction(commonUpdateValue.VoucherNumber,_objectEntity);
                            _inventoryvoucher.SaveBudgetTransactionColumnValue(budgetTransaction, commonUpdateValue,_objectEntity);
                        }
                        if (batchTransaction != null)
                        {
                            _inventoryvoucher.DeleteBatchTransaction(commonUpdateValue.VoucherNumber, _objectEntity);
                            _inventoryvoucher.SaveBatchTransactionValues(masterColumn, batchTransaction, commonUpdateValue, _objectEntity);
                        }
                        if (batchTransValues != null)
                        {
                            _inventoryvoucher.DeleteBatchTransaction(commonUpdateValue.VoucherNumber, _objectEntity);
                            _inventoryvoucher.SaveBatchTransValues(childColumn,masterColumn, batchTransValues, commonUpdateValue, _objectEntity);
                        }
                        if (charges != null)
                        {
                            _inventoryvoucher.DeleteChargeTransaction(commonUpdateValue.VoucherNumber, _objectEntity);
                            _inventoryvoucher.SaveChargeColumnValue(charges, commonUpdateValue, _objectEntity);
                        }
                        if (customColList.Count > 0)
                        {
                            _inventoryvoucher.DeleteCustomTransaction(commonUpdateValue.VoucherNumber,_objectEntity);
                            _inventoryvoucher.SaveCustomTransaction(customColList, commonUpdateValue,_objectEntity);
                        }
                        if (shippingDetailValues != null)
                        {
                            _inventoryvoucher.UpdateShippingDetailsColumnValue(shippingDetailValues, commonUpdateValue, _objectEntity);
                        }
                        #region CLEAR CACHE
                        List<string> keystart = new List<string>();
                        keystart.Add("GetAllMenuItems");
                        keystart.Add("GetAllSalesOrderDetails");
                        keystart.Add("GetSalesOrderDetailFormDetailByFormCodeAndOrderNo");
                        keystart.Add("GetAllOrederNoByFlter");
                        keystart.Add("GetSubMenuList");
                        keystart.Add("GetSubMenuDetailList");
                        List<string> Record = new List<string>();
                        Record = this._cacheManager.GetAllKeys();
                        this._cacheManager.RemoveCacheByKey(keystart, Record);
                        #endregion
                        if (model.Save_Flag == "0" && updatedChild == true && updatedMaster == true)
                        {
                            trans.Commit();
                            msg = "UPDATED";
                        }
                        else if (model.Save_Flag == "4" && updatedChild == true && updatedMaster == true)
                        {
                            trans.Commit();
                            msg = "UPDATEDANDPRINT";
                        }
                        else
                        {
                            trans.Rollback();
                            return Request.CreateResponse(HttpStatusCode.InternalServerError, new { MESSAGE = "ERROR", STATUS_CODE = (int)HttpStatusCode.InternalServerError });
                        }
                    }
                    return Request.CreateResponse(HttpStatusCode.OK, new { MESSAGE = msg, STATUS_CODE = (int)HttpStatusCode.OK, VoucherNo = VoucherNumberGeneratedNo, 
                        No = newvoucherNo, VoucherDate = primarydate, FormCode = model.Form_Code });
                }
                catch (Exception ex)
                {
                    trans.Rollback();
                    _logErp.ErrorInDB("Error while saving Inventory voucher: " + ex.Message);                   
                    return Request.CreateResponse(HttpStatusCode.InternalServerError, new { MESSAGE = ex.Message, STATUS_CODE = (int)HttpStatusCode.InternalServerError });
                    //throw new Exception(ex.Message);
                }
            }
        }
        public IHttpActionResult GetAllOptionsByFilter(string filter)
        {
            try
            {
                return Ok();
            }
            catch (Exception ex)
            {
                return Content(HttpStatusCode.NotFound, ex.Message);
            }
        }
        [HttpGet]
        public List<FinancialBudgetTransaction> GetDataForBudgetModal(string VOUCHER_NO)
        {
            var userid = _workContext.CurrentUserinformation.User_id;
            var company_code = _workContext.CurrentUserinformation.company_code;
            var branch_code = _workContext.CurrentUserinformation.branch_code;
            var response = new List<FinancialBudgetTransaction>();
            //if (this._cacheManager.IsSet($"GetDataForBudgetModal_{userid}_{company_code}_{branch_code}_{VOUCHER_NO}"))
            //{
            //    var data = _cacheManager.Get<List<FinancialBudgetTransaction>>($"GetDataForBudgetModal_{userid}_{company_code}_{branch_code}_{VOUCHER_NO}");
            //    response = data;
            //}
            //else
            //{
            //    var financialBudgetTransactionList = this._inventoryvoucher.Getbudgetdetail(VOUCHER_NO);
            //    this._cacheManager.Set($"GetDataForBudgetModal_{userid}_{company_code}_{branch_code}_{VOUCHER_NO}", financialBudgetTransactionList, 20);
            //    response = financialBudgetTransactionList;
            //}
            var financialBudgetTransactionList = this._inventoryvoucher.Getbudgetdetail(VOUCHER_NO);
            response = financialBudgetTransactionList;
            return response;
            //return this._inventoryvoucher.Getbudgetdetail(VOUCHER_NO);
        }
        [HttpGet]
        public List<BATCHTRANSACTIONDATA> GetDataForBatchModal(string VOUCHER_NO)
        {      
            var response = new List<BATCHTRANSACTIONDATA>();
          
            var batchTransactionList = this._inventoryvoucher.Getbatchdetail(VOUCHER_NO);
            response = batchTransactionList;
            return response;           
        }
        [HttpGet]
        public List<BATCH_TRANSACTION_DATA> GetDataForBatchTrackingModal(string VOUCHER_NO)
        {
            var response = new List<BATCH_TRANSACTION_DATA>();

            var batchTransactionList = this._inventoryvoucher.Getbatchtrackingdetail(VOUCHER_NO);
            response = batchTransactionList;
            return response;
        }
        [HttpPost]
        public HttpResponseMessage DeleteInvVoucher(string voucherno, string formcode)
        {
            try
            {
                bool isposted = _FormTemplateRepo.CheckVoucherNoPosted(voucherno);
                if (isposted == true)
                {
                    return Request.CreateResponse(HttpStatusCode.OK, new { MESSAGE = "POSTED", STATUS_CODE = (int)HttpStatusCode.OK, VoucherNo = voucherno });
                }


                string checkreferenced = _FormTemplateRepo.CheckVoucherNoReferenced(voucherno);
                if (checkreferenced != "" && checkreferenced != null && checkreferenced != "undefined")
                {
                    return Request.CreateResponse(HttpStatusCode.OK, new { MESSAGE = "REFERENCED", STATUS_CODE = (int)HttpStatusCode.OK, VoucherNo = checkreferenced });
                }
                else
                {
                    var primarycolumn = string.Empty;
                    var fomdetails = _FormTemplateRepo.GetFormDetailSetup(formcode);
                    if (fomdetails.Count > 0)
                    {
                        primarycolumn = _FormTemplateRepo.GetPrimaryColumnByTableName(fomdetails[0].TABLE_NAME);
                    }
                    bool deleteres = _inventoryvoucher.deletevouchernoInv(fomdetails[0].TABLE_NAME, formcode, voucherno, primarycolumn);

                    return Request.CreateResponse(HttpStatusCode.OK, new { MESSAGE = "DELETED", STATUS_CODE = (int)HttpStatusCode.OK });
                }


            }
            catch (Exception ex)
            {

                return Request.CreateResponse(HttpStatusCode.InternalServerError, new { MESSAGE = ex.Message, STATUS_CODE = (int)HttpStatusCode.InternalServerError });
            }
        }

        public List<Employee> GetAllEmployees()
        {
            List<Employee> response = new List<Employee>();
            try
            {
                response = _FormTemplateRepo.GetAllEmployees();
                return response;

            }
            catch (Exception ex)
            {
                throw new Exception(ex.StackTrace);
            }
        }

    }
}
