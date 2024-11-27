using NeoErp.Core;
using NeoErp.Core.Caching;
using NeoErp.Core.Domain;
using NeoErp.Core.Models;
using NeoErp.Core.Models.Log4NetLoggin;
using NeoErp.Data;
using System;
using NeoErp.Core.Services.CommonSetting;
using Newtonsoft.Json;
using System.Collections.Generic;
using System.Linq;
using NeoErp.Core.Models.CustomModels;
using System.Text;
using NeoERP.QuotationManagement.Service.Interface;
using NeoERP.QuotationManagement.Service.Models;
using System.Web;
using System.IO;
using System.Net.Http;
using System.Net;

namespace NeoERP.QuotationManagment.Service.Repository
{
    public class QuotationSetup : IQuotationRepo
    {
        IWorkContext _workContext;
        IDbContext _dbContext;

        public QuotationSetup(IWorkContext workContext, IDbContext dbContext)
        {
            this._workContext = workContext;
            this._dbContext = dbContext;
        }
        public List<Company> GetCompany()
        {
            var company_code = _workContext.CurrentUserinformation.company_code;
            string query = $@"select COMPANY_CODE,COMPANY_EDESC,ADDRESS,EMAIL,LOGO_FILE_NAME from COMPANY_SETUP WHERE COMPANY_CODE='{company_code}'";
            List<Company> company = _dbContext.SqlQuery<Company>(query).ToList();
            return company;
        }
        public List<Products> GetAllProducts()
        {
            try
            {
                List<Products> ProductsList = new List<Products>();

                string query = $@"select 
                    COALESCE(iims.item_code,' ') as ItemCode
                    ,COALESCE(iims.item_edesc,' ') as ItemDescription
                    ,COALESCE(iims.index_mu_code,' ') as ItemUnit
                    ,COALESCE(iiss.ITEM_SPECIFICATION,' ') as SPECIFICATION
                    ,COALESCE(iiss.BRAND_NAME,' ') as BRAND_NAME
                    ,COALESCE(iiss.INTERFACE,' ') as INTERFACE
                    ,COALESCE(iiss.TYPE,' ') as TYPE
                     ,COALESCE(iiss.LAMINATION,' ') as LAMINATION
                    ,COALESCE(iiss.ITEM_SIZE,' ') as ITEM_SIZE
                    ,COALESCE(iiss.THICKNESS,' ') as THICKNESS
                    ,COALESCE(iiss.COLOR,' ') as COLOR
                    ,COALESCE(iiss.GRADE,' ') as GRADE
                     ,COALESCE(iiss.SIZE_LENGHT,0) as SIZE_LENGHT
                    ,COALESCE(iiss.SIZE_WIDTH,0) as SIZE_WIDTH
                    from ip_item_master_setup iims,IP_ITEM_SPEC_SETUP iiss
                    where iims.item_code=iiss.item_code and iims.company_code=iiss.company_code
                     and iims.deleted_flag='N'
                    AND iims.company_code='{_workContext.CurrentUserinformation.company_code}' and iims.branch_code='{_workContext.CurrentUserinformation.branch_code}'
                     order by iims.item_code desc";
                ProductsList = this._dbContext.SqlQuery<Products>(query).ToList();
                return ProductsList;
            }
            catch (Exception)
            {

                throw;
            }
        }

        public List<Quotation_setup> GetQuotationId()
        {
            //string query = $@"select FN_NEW_TENDERNO('{_workContext.CurrentUserinformation.company_code}','{_workContext.CurrentUserinformation.branch_code}') as TENDER_NO from dual";
            string query = $@"select GENERATE_TENDER_NO('{_workContext.CurrentUserinformation.company_code}') as TENDER_NO from dual";
            List<Quotation_setup> id = _dbContext.SqlQuery<Quotation_setup>(query).ToList();
            //string fQuery = $@"select FORM_CODE from form_setup where form_edesc ='Quotation Enquiry' and  company_code= '{_workContext.CurrentUserinformation.company_code}'";
            //string formcode = _dbContext.SqlQuery<string>(fQuery).FirstOrDefault();
            //var companycode = _workContext.CurrentUserinformation.company_code;
            //string tablename = "IP_QUOTATION_INQUIRY";
            //string transactiondate = DateTime.Now.ToString("dd-MMM-yyyy");
            //string query = string.Format(@"select FN_NEW_VOUCHER_NO('{0}','{1}','{2}','{3}') as TENDER_NO FROM DUAL", companycode, formcode, transactiondate, tablename);
            //List<Quotation_setup> id = _dbContext.SqlQuery<Quotation_setup>(query).ToList();
            return id;
        }
        public bool InsertQuotationData(Quotation_setup data)
        {
            try
            {
                int tenderId = data.ID;
                if (tenderId == 0)
                {
                    var idquery = $@"SELECT COALESCE(MAX(ID) + 1, 1) AS id FROM sa_quotation_setup";
                    int id = _dbContext.SqlQuery<int>(idquery).FirstOrDefault();
                    string insertQuery = string.Format(@"INSERT INTO sa_quotation_setup(TENDER_NO, VALID_DATE, ISSUE_DATE, CREATED_DATE, CREATED_BY, COMPANY_CODE, STATUS,REMARKS,ID,APPROVED_STATUS,BRANCH_CODE) 
                                 VALUES('{0}', TO_DATE('{1}', 'DD-MON-YYYY'), TO_DATE('{2}', 'DD-MON-YYYY'), TO_DATE('{3}', 'DD-MON-YYYY'), '{4}', '{5}', '{6}','{7}',{8},'{9}','{10}')",
                                              data.TENDER_NO,
                                              data.VALID_DATE.HasValue ? $"{data.VALID_DATE.Value.ToString("dd-MMM-yyyy")}" : null,
                                              data.ISSUE_DATE.HasValue ? $"{data.ISSUE_DATE.Value.ToString("dd-MMM-yyyy")}" : null,
                                              DateTime.Now.ToString("dd-MMM-yyyy"),
                                              _workContext.CurrentUserinformation.login_code,
                                              _workContext.CurrentUserinformation.company_code,
                                              "E", data.REMARKS, id, "N", _workContext.CurrentUserinformation.branch_code);
                    _dbContext.ExecuteSqlCommand(insertQuery);
                    List<Item> itemData = data.Items;
                    if (itemData != null)
                    {
                        foreach (var item in itemData)
                        {

                            InsertItemData(item, data.TENDER_NO); // Pass each item individually to InsertItemData
                        }
                    }
                }
                else
                {
                    string updateQuery = $@"UPDATE sa_quotation_setup 
                       SET VALID_DATE = {(data.VALID_DATE.HasValue ? $"'{data.VALID_DATE.Value.ToString("dd-MMM-yyyy")}'" : "null")},
                           ISSUE_DATE = {(data.ISSUE_DATE.HasValue ? $"'{data.ISSUE_DATE.Value.ToString("dd-MMM-yyyy")}'" : "null")},
                           MODIFIED_DATE = '{DateTime.Now.ToString("dd-MMM-yyyy")}',REMARKS='{data.REMARKS}',
                           MODIFIED_BY = '{_workContext.CurrentUserinformation.login_code}',
                           COMPANY_CODE = '{_workContext.CurrentUserinformation.company_code}'
                       WHERE id = '{data.ID}' 
                       AND tender_no = '{data.TENDER_NO}'";
                    _dbContext.ExecuteSqlCommand(updateQuery);
                    List<Item> itemData = data.Items;
                    if (itemData != null)
                    {
                        foreach (var item in itemData)
                        {
                            var query = $@"SELECT * FROM sa_quotation_Items WHERE tender_no='{data.TENDER_NO}' AND id='{item.ID}'";
                            List<Item> itemDetails = _dbContext.SqlQuery<Item>(query).ToList();

                            if (itemDetails.Any())
                            {
                                    UpdateItemData(item, data.TENDER_NO);
                            }
                            else
                            {
                                InsertItemData(item, data.TENDER_NO);
                            }

                        }
                    }
                }
                return true;
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }

        public bool InsertItemData(Item item, string tenderNo)
        {
            try
            {
                item = ProcessImageData(item);
                var idquery = $@"SELECT COALESCE(MAX(ID) + 1, 1) AS id FROM sa_quotation_Items";
                int id = _dbContext.SqlQuery<int>(idquery).FirstOrDefault();
                string insertItemQuery = string.Format(@"INSERT INTO sa_quotation_Items (ID,TENDER_NO, ITEM_CODE, SPECIFICATION, IMAGE, UNIT, QUANTITY, Category, BRAND_NAME, INTERFACE, TYPE, LAMINATION, ITEM_SIZE, THICKNESS, COLOR, GRADE, SIZE_LENGTH, SIZE_WIDTH,DELETED_FLAG) 
                             VALUES({0}, '{1}', '{2}', '{3}', '{4}', '{5}', {6}, '{7}', '{8}', '{9}', '{10}', '{11}', '{12}', '{13}', '{14}', '{15}', {16}, {17},'{18}')",
                                     id, tenderNo,item.ITEM_CODE, item.SPECIFICATION, item.IMAGE, item.UNIT, item.QUANTITY, item.CATEGORY, item.BRAND_NAME, item.INTERFACE, item.TYPE, item.LAMINATION, item.ITEM_SIZE, item.THICKNESS, item.COLOR, item.GRADE, item.SIZE_LENGTH, item.SIZE_WIDTH,"N");

                _dbContext.ExecuteSqlCommand(insertItemQuery);
                return true;
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }
        private Item ProcessImageData(Item item)
        {
            if (!string.IsNullOrEmpty(item.IMAGE))
            {
                byte[] imageBytes = Convert.FromBase64String(item.IMAGE);

                string folderPath = "~/Areas/NeoERP.QuotationManagement/Image/Items/";
                string imageName = $"{Guid.NewGuid()}.png"; // Generating unique image name
                string imagePath = $"{folderPath}{imageName}"; // Combining folder path and image name
                string physicalPath = HttpContext.Current.Server.MapPath(imagePath);
                File.WriteAllBytes(physicalPath, imageBytes);
                item.IMAGE = imageName;
            }
            else
            {
                item.IMAGE = item.IMAGE_NAME;
            }

            return item;
        }

        public bool UpdateItemData(Item item, string tenderNo)
        {
            try
            {
                item = ProcessImageData(item);

                string updateItemQuery = string.Format(@"UPDATE sa_quotation_Items  SET ITEM_CODE = '{2}', 
                 SPECIFICATION = '{3}', IMAGE = '{4}',UNIT = '{5}', QUANTITY = {6},Category = '{7}',BRAND_NAME = '{8}', 
                 INTERFACE = '{9}',TYPE = '{10}',LAMINATION = '{11}', ITEM_SIZE = '{12}', THICKNESS = '{13}', COLOR = '{14}', 
                 GRADE = '{15}',SIZE_LENGTH = {16},SIZE_WIDTH = {17} WHERE TENDER_NO = '{1}' AND ID = {0}",
                                         item.ID, tenderNo, item.ITEM_CODE, item.SPECIFICATION, item.IMAGE, item.UNIT, item.QUANTITY, item.CATEGORY, item.BRAND_NAME, item.INTERFACE, item.TYPE, item.LAMINATION, item.ITEM_SIZE, item.THICKNESS, item.COLOR, item.GRADE, item.SIZE_LENGTH, item.SIZE_WIDTH, "N");
                _dbContext.ExecuteSqlCommand(updateItemQuery);
                return true;
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }
        public List<Quotation_setup> GetTenderId(string tenderNo)
        {
            string query = $@"select id from sa_quotation_setup where tender_no='{tenderNo}'";
            List<Quotation_setup> id = _dbContext.SqlQuery<Quotation_setup>(query).ToList();
            return id;
        }

        public List<Quotation_setup> ListAllTenders()
        {
            string query = $@"SELECT  TENDER_NO,ISSUE_DATE,VALID_DATE,CREATED_DATE,bs_date(ISSUE_DATE) as NEPALI_DATE,
       CASE   WHEN approved_status = 'Y' THEN 'Approved'  ELSE 'Pending'  END AS approved_status FROM sa_quotation_setup WHERE status = 'E' and company_code='{_workContext.CurrentUserinformation.company_code}' ORDER BY id desc";
            List<Quotation_setup> tenderDetails = _dbContext.SqlQuery<Quotation_setup>(query).ToList();
            return tenderDetails;
        }
        public bool deleteQuotationId(string tenderNo)
        {
            try
            {
                var UPDATE_QUERY = $@"UPDATE sa_quotation_setup SET STATUS ='D' WHERE TENDER_NO='{tenderNo}'";
                _dbContext.ExecuteSqlCommand(UPDATE_QUERY);
                return true;
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }
        public List<Quotation_setup> GetQuotationById(string tenderNo)
        {
            try
            {
                // Fetch project data
                string Query = $@"SELECT TENDER_NO,ISSUE_DATE,VALID_DATE,CREATED_DATE,bs_date(ISSUE_DATE) as NEPALI_DATE,bs_date(VALID_DATE) as DELIVERY_DT_BS,REMARKS,ID,
                (CASE WHEN APPROVED_STATUS='Y' THEN 'Approved' else 'Pending' END) AS APPROVED_STATUS FROM sa_quotation_setup WHERE TENDER_NO = '{tenderNo}' AND STATUS = 'E'";
                List<Quotation_setup> quotations = this._dbContext.SqlQuery<Quotation_setup>(Query).ToList();
                foreach (var quotation in quotations)
                {
                    string query = $@"select * from sa_quotation_Items where TENDER_NO='{quotation.TENDER_NO}' AND DELETED_FLAG='N' order by id";
                    List<Item> itemData = this._dbContext.SqlQuery<Item>(query).ToList();
                    quotation.Items = itemData;
                }

                return quotations;
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }
        public bool updateItemsById(string tenderNo,string id)
        {
            try
            {
                var UPDATE_QUERY = $@"UPDATE sa_quotation_Items SET deleted_flag ='Y' WHERE TENDER_NO='{tenderNo}' and id='{id}'";
                _dbContext.ExecuteSqlCommand(UPDATE_QUERY);
                return true;
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }
        public List<Quotation_Details> ListQuotationDetails()
        {
            string query = $@"SELECT QD.QUOTATION_NO,QD.TENDER_NO,QD.PAN_NO,QD.PARTY_NAME,QD.ADDRESS,QD.CONTACT_NO,QD.EMAIL,QD.CURRENCY,QD.CURRENCY_RATE,QD.DELIVERY_DATE,
            QD.TOTAL_AMOUNT,QD.TOTAL_DISCOUNT,QD.TOTAL_EXCISE,QD.TOTAL_TAXABLE_AMOUNT,QD.TOTAL_VAT,QD.TOTAL_NET_AMOUNT,
            (CASE WHEN QD.STATUS='RQ' then 'Pending' when QD.status='AP' then 'Approved' else 'Reject' end) AS STATUS,
            QD.TERM_CONDITION FROM  QUOTATION_DETAILS QD,SA_QUOTATION_SETUP SQS WHERE SQS.TENDER_NO=QD.TENDER_NO AND
            SQS.COMPANY_CODE='{_workContext.CurrentUserinformation.company_code}' ORDER BY QD.QUOTATION_NO DESC";
            List<Quotation_Details> tenderDetails = _dbContext.SqlQuery<Quotation_Details>(query).ToList();
            return tenderDetails;
        }
        public List<Quotation_Details> QuotationDetailsById(string quotationNo,string tenderNo)
        {
            try
            {
                string Query = $@"SELECT SQS.ISSUE_DATE,SQS.VALID_DATE,BS_DATE(SQS.ISSUE_DATE) AS NEPALI_DATE,BS_DATE(SQS.VALID_DATE) AS DELIVERY_DT_BS,QD.QUOTATION_NO,QD.TENDER_NO,QD.PAN_NO,scs.supplier_edesc as party_name,scs.regd_office_eaddress as address,
                scs.tel_mobile_no1 as contact_no,scs.EMAIL,QD.CURRENCY,QD.CURRENCY_RATE,QD.DELIVERY_DATE,BS_DATE(QD.DELIVERY_DATE) as DELIVERY_DT_NEP,
                QD.TOTAL_AMOUNT,QD.TOTAL_DISCOUNT,QD.TOTAL_EXCISE,QD.TOTAL_TAXABLE_AMOUNT,QD.TOTAL_VAT,QD.TOTAL_NET_AMOUNT,
                CASE 
                WHEN QD.STATUS = 'AP' THEN 'Approved' WHEN QD.STATUS = 'R' THEN 'Reject'
                WHEN NOT EXISTS (SELECT 1 FROM QUOTATION_DETAILS WHERE TENDER_NO = QD.TENDER_NO AND STATUS = 'AP') THEN 'Pending'
                ELSE 'Reject' END AS STATUS,QD.DISCOUNT_TYPE,SQS.REMARKS  FROM  QUOTATION_DETAILS  QD,SA_QUOTATION_SETUP SQS , ip_supplier_setup scs WHERE SQS.TENDER_NO=QD.TENDER_NO and scs.supplier_code=qd.supplier_code and sqs.company_code=scs.company_code AND  QD.QUOTATION_NO='{quotationNo}'";
                List<Quotation_Details> quotations = this._dbContext.SqlQuery<Quotation_Details>(Query).ToList();
                foreach (var quotation in quotations)
                {
                    string query =$@"SELECT IIMS.ITEM_CODE,SQI.SPECIFICATION, SQI.IMAGE,SQI.UNIT,SQI.QUANTITY,SQI.CATEGORY, SQI.BRAND_NAME,SQI.INTERFACE,
                        SQI.TYPE, SQI.LAMINATION, SQI.ITEM_SIZE,SQI.THICKNESS,SQI.COLOR,SQI.GRADE,SQI.SIZE_LENGTH, SQI.SIZE_WIDTH,QDI.RATE,
                        QDI.AMOUNT, QDI.DISCOUNT, QDI.DISCOUNT_AMOUNT,QDI.EXCISE,QDI.TAXABLE_AMOUNT, QDI.VAT_AMOUNT,QDI.NET_AMOUNT
                        FROM   SA_QUOTATION_ITEMS SQI, QUOTATION_DETAIL_ITEMWISE QDI,ip_item_master_setup iims, sa_quotation_setup sqs,quotation_details qd  WHERE SQI.ITEM_CODE = QDI.ITEM_CODE AND
                        IIMS.ITEM_CODE = SQI.ITEM_CODE AND SQS.TENDER_NO = SQI.TENDER_NO AND IIMS.COMPANY_CODE = SQS.COMPANY_CODE AND qd.tender_no = sqi.tender_no and qdi.quotation_no=qd.quotation_no AND
                        QDI.QUOTATION_NO = '{quotationNo}' AND SQI.DELETED_FLAG = 'N' ORDER BY SQI.ID";
                    List<Item_details> itemData = this._dbContext.SqlQuery<Item_details>(query).ToList();
                    quotation.Item_Detail = itemData;
                    string vquery = $@"SELECT * FROM QUOTATION_TERM_CONDITION WHERE TENDER_NO='{quotation.TENDER_NO}' AND QUOTATION_NO='{quotationNo}'";
                    List<Term_Conditions> TermCondition = this._dbContext.SqlQuery<Term_Conditions>(vquery).ToList();
                    quotation.TermsCondition = TermCondition;
                }
                List<QuotationTransaction> imagelist = new List<QuotationTransaction>();
                if (quotations.Count > 0)
                {
                    string imagequery = $@"SELECT * FROM QUOTATION_TRANSACTION WHERE QUOTATION_NO ='{quotationNo}' and COMPANY_CODE='{_workContext.CurrentUserinformation.company_code}' and TENDER_NO='{tenderNo}' and DELETED_FLAG='N'";
                    imagelist = this._dbContext.SqlQuery<QuotationTransaction>(imagequery).ToList();
                    quotations[0].IMAGES_LIST = imagelist;
                }
                return quotations;
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }
        public List<Quotation_Details> QuotationDetailsId(string quotationNo, string tenderNo)
        {
            try
            {
                string Query = $@"SELECT SQS.ISSUE_DATE,SQS.VALID_DATE,BS_DATE(SQS.ISSUE_DATE) AS NEPALI_DATE,BS_DATE(SQS.VALID_DATE) AS DELIVERY_DT_BS,QD.QUOTATION_NO,QD.TENDER_NO,QD.PAN_NO,scs.supplier_edesc as party_name,scs.regd_office_eaddress as address,
                scs.tel_mobile_no1 as contact_no,scs.EMAIL,QD.CURRENCY,QD.CURRENCY_RATE,QD.DELIVERY_DATE,BS_DATE(QD.DELIVERY_DATE) as DELIVERY_DT_NEP,
                QD.TOTAL_AMOUNT,QD.TOTAL_DISCOUNT,QD.TOTAL_EXCISE,QD.TOTAL_TAXABLE_AMOUNT,QD.TOTAL_VAT,QD.TOTAL_NET_AMOUNT,
                CASE 
                WHEN QD.STATUS = 'AP' THEN 'Approved' WHEN QD.STATUS = 'R' THEN 'Reject'
                WHEN NOT EXISTS (SELECT 1 FROM QUOTATION_DETAILS WHERE TENDER_NO = QD.TENDER_NO AND STATUS = 'AP') THEN 'Pending'
                ELSE 'Reject' END AS STATUS,QD.DISCOUNT_TYPE,SQS.REMARKS  FROM  QUOTATION_DETAILS  QD,SA_QUOTATION_SETUP SQS , ip_supplier_setup scs WHERE SQS.TENDER_NO=QD.TENDER_NO and scs.supplier_code=qd.supplier_code and sqs.company_code=scs.company_code AND  QD.QUOTATION_NO='{quotationNo}'";
                List<Quotation_Details> quotations = this._dbContext.SqlQuery<Quotation_Details>(Query).ToList();
                foreach (var quotation in quotations)
                {
                    string query = $@"SELECT IIMS.ITEM_EDESC as item_code,SQI.SPECIFICATION, SQI.IMAGE,SQI.UNIT,SQI.QUANTITY,SQI.CATEGORY, SQI.BRAND_NAME,SQI.INTERFACE,
                        SQI.TYPE, SQI.LAMINATION, SQI.ITEM_SIZE,SQI.THICKNESS,SQI.COLOR,SQI.GRADE,SQI.SIZE_LENGTH, SQI.SIZE_WIDTH,QDI.RATE,
                        QDI.AMOUNT, QDI.DISCOUNT, QDI.DISCOUNT_AMOUNT,QDI.EXCISE,QDI.TAXABLE_AMOUNT, QDI.VAT_AMOUNT,QDI.NET_AMOUNT
                        FROM   SA_QUOTATION_ITEMS SQI, QUOTATION_DETAIL_ITEMWISE QDI,ip_item_master_setup iims, sa_quotation_setup sqs,quotation_details qd  WHERE SQI.ITEM_CODE = QDI.ITEM_CODE AND
                        IIMS.ITEM_CODE = SQI.ITEM_CODE AND SQS.TENDER_NO = SQI.TENDER_NO AND IIMS.COMPANY_CODE = SQS.COMPANY_CODE AND qd.tender_no = sqi.tender_no and qdi.quotation_no=qd.quotation_no AND
                        QDI.QUOTATION_NO = '{quotationNo}' AND SQI.DELETED_FLAG = 'N' ORDER BY SQI.ID";
                    List<Item_details> itemData = this._dbContext.SqlQuery<Item_details>(query).ToList();
                    quotation.Item_Detail = itemData;
                    string vquery = $@"SELECT * FROM QUOTATION_TERM_CONDITION WHERE TENDER_NO='{quotation.TENDER_NO}' AND QUOTATION_NO='{quotationNo}'";
                    List<Term_Conditions> TermCondition = this._dbContext.SqlQuery<Term_Conditions>(vquery).ToList();
                    quotation.TermsCondition = TermCondition;
                }
                List<QuotationTransaction> imagelist = new List<QuotationTransaction>();
                if (quotations.Count > 0)
                {
                    string imagequery = $@"SELECT * FROM QUOTATION_TRANSACTION WHERE QUOTATION_NO ='{quotationNo}' and COMPANY_CODE='{_workContext.CurrentUserinformation.company_code}' and TENDER_NO='{tenderNo}' and DELETED_FLAG='N'";
                    imagelist = this._dbContext.SqlQuery<QuotationTransaction>(imagequery).ToList();
                    quotations[0].IMAGES_LIST = imagelist;
                }
                return quotations;
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }
        public List<SummaryReport> TendersItemWise()
        {
            try
            {
                string Query = $@"SELECT SQS.TENDER_NO,SQS.CREATED_DATE,SQS.VALID_DATE,IIMS.ITEM_EDESC as ITEM_DESC, CASE WHEN EXISTS (
                 SELECT 1 FROM QUOTATION_DETAILS QD WHERE QD.TENDER_NO = SQS.TENDER_NO AND QD.STATUS = 'AP') THEN 'Close'
                 ELSE 'Open' END AS STATUS FROM SA_QUOTATION_SETUP SQS,SA_QUOTATION_ITEMS SQI,IP_ITEM_MASTER_SETUP IIMS
                 WHERE SQS.TENDER_NO=SQI.TENDER_NO AND IIMS.ITEM_CODE=SQI.ITEM_CODE AND SQS.COMPANY_CODE=IIMS.COMPANY_CODE
                 AND SQS.STATUS='E' AND SQI.DELETED_FLAG='N' ORDER BY SQS.ID DESC";
                List<SummaryReport> tenderItemwise = this._dbContext.SqlQuery<SummaryReport>(Query).ToList();
                return tenderItemwise;
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }
        public List<Quotation> ItemDetailsTenderNo(string tenderNo)
        {
            try
            {
                string query = $@"SELECT TENDER_NO, ISSUE_DATE, VALID_DATE, CREATED_DATE, bs_date(ISSUE_DATE) as NEPALI_DATE,bs_date(VALID_DATE) as DELIVERY_DT_BS, COMPANY_CODE FROM SA_QUOTATION_SETUP WHERE STATUS='E' AND TENDER_NO='{tenderNo}'";
                List<Quotation> quotations = this._dbContext.SqlQuery<Quotation>(query).ToList();
                foreach (var quotation in quotations)
                {
                    string vquery = $@"SELECT QD.QUOTATION_NO,scs.supplier_edesc as party_name,ROUND((QDI.TAXABLE_AMOUNT-QDI.EXCISE)/SQI.QUANTITY,2) AS ACTUAL_PRICE,QDI.ITEM_CODE,QD.STATUS,QD.REVISE FROM QUOTATION_DETAILS QD,QUOTATION_DETAIL_ITEMWISE QDI,SA_QUOTATION_ITEMS SQI,ip_supplier_setup scs
                    WHERE QD.QUOTATION_NO=QDI.QUOTATION_NO AND SQI.TENDER_NO=QD.TENDER_NO AND QDI.ITEM_CODE=SQI.ITEM_CODE AND scs.supplier_code=qd.supplier_code AND QD.TENDER_NO = '{quotation.TENDER_NO}' AND SCS.COMPANY_CODE='{_workContext.CurrentUserinformation.company_code}' AND SQI.deleted_flag = 'N' order by quotation_no";
                    List<PARTY_DETAIL> partyData = this._dbContext.SqlQuery<PARTY_DETAIL>(vquery).ToList();
                    quotation.PartDetails = partyData;
                    string vQuery = $@"SELECT SQI.*, 
                                   IIMS.ITEM_EDESC AS ITEM_DESC,IIR.CALC_UNIT_PRICE AS LAST_PRICE,IIR.SUPPLIER_CODE AS LAST_VENDOR
                            FROM SA_QUOTATION_ITEMS SQI
                            LEFT JOIN IP_ITEM_MASTER_SETUP IIMS ON IIMS.ITEM_CODE = SQI.ITEM_CODE
                            LEFT JOIN VPRINT_IP_PURCHASE_REQUEST IIR ON IIR.ITEM_CODE = SQI.ITEM_CODE
                            WHERE SQI.TENDER_NO = '{quotation.TENDER_NO}'
                            AND SQI.deleted_flag = 'N'
                            AND IIMS.COMPANY_CODE ='{quotation.COMPANY_CODE}'";
                            List<Item_Detail> itemDetail = this._dbContext.SqlQuery<Item_Detail>(vQuery).ToList();
                        quotation.Items = itemDetail;
                }
                return quotations;
            }   
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }
        public bool acceptQuotation(string quotationNo, string status)
        {
            try
            {
                bool insertedData = false;

                var UPDATE_QUERY = $@"UPDATE quotation_details SET status ='{status}',approved_by='{_workContext.CurrentUserinformation.login_code}',approved_date='{DateTime.Now.ToString("dd - MMM - yyyy")}' WHERE QUOTATION_NO='{quotationNo}'";
                _dbContext.ExecuteSqlCommand(UPDATE_QUERY);

                string fQuery = $@"select FORM_CODE from form_setup where quotation_flag='Y' and  company_code= '{_workContext.CurrentUserinformation.company_code}'";
                string formCode = _dbContext.SqlQuery<string>(fQuery).FirstOrDefault();

                string vquery = $@"select qs.quotation_no,qs.tender_no,qs.created_date,qs.supplier_code,scs.regd_office_eaddress as address,scs.tel_mobile_no1 as contact_no ,iims.item_code,sqi.specification,
                iims.index_mu_code,sqi.quantity,qdi.rate,qdi.amount as total_net_amount,sqs.company_code,sqs.branch_code,qs.currency,qs.currency_rate,qs.delivery_date
                from quotation_details qs ,ip_supplier_setup scs,sa_quotation_items sqi,ip_item_master_setup iims ,sa_quotation_setup sqs,quotation_detail_itemwise qdi
                where scs.supplier_code=qs.supplier_code and iims.item_code=sqi.item_code and qs.tender_no=sqi.tender_no and sqs.tender_no=qs.tender_no and 
                sqs.company_code=iims.company_code and sqs.company_code=scs.company_code and qdi.quotation_no=qs.quotation_no and  sqi.item_code=qdi.item_code
                and  qs.quotation_no='{quotationNo}' and sqi.deleted_flag='N' and sqs.status='E'";
                List<QuotationDetails> quoteData = _dbContext.SqlQuery<QuotationDetails>(vquery).ToList();
                quoteData.ForEach(q => q.Form_Code = formCode);

                insertedData = InsertQuotesData(quoteData);
                if (insertedData)
                {
                    string query = $@"SELECT qs.tender_no,  qs.created_date,  qs.supplier_code,  qs.total_net_amount, sqs.company_code,
                      sqs.branch_code,  qs.currency, qs.currency_rate,  qs.delivery_date FROM  quotation_details qs,
                       sa_quotation_setup sqs WHERE qs.tender_no =sqs.tender_no  AND qs.quotation_no = '{quotationNo}' AND sqs.status = 'E'";
                    List<QuotationDetails> masterData = _dbContext.SqlQuery<QuotationDetails>(query).ToList();
                    masterData.ForEach(q => q.Form_Code = formCode);
                    SaveMasterColumnValue(masterData);
                }
                return true;
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }
        public bool InsertQuotesData(List<QuotationDetails> quoteData)
        {
            try
            {
                foreach (var data in quoteData)
                {
                    var idquery = $@"SELECT COALESCE(MAX(SERIAL_NO) + 1, 1) AS id FROM ip_quotation_inquiry where QUOTE_NO='{data.Tender_No}'";
                    int id = _dbContext.SqlQuery<int>(idquery).FirstOrDefault();
                    //var date=data.Created_Date-data.Delivery_Date;
                    TimeSpan dateDifference = data.Delivery_Date.Date - data.Created_Date.Date;
                    int deliveryDays = (int)dateDifference.TotalDays;
                    string sQuery = $@"SELECT MYSEQUENCE.NEXTVAL FROM DUAL";
                    var sessionRowId =_dbContext.SqlQuery<int>(sQuery).FirstOrDefault();


                    string insertItemQuery = $@"
                    INSERT INTO ip_quotation_inquiry (
                    QUOTE_NO, QUOTE_DATE, ORDER_NO, REQUEST_NO, MANUAL_NO, SUPPLIER_CODE, ADDRESS, CONTACT_PERSON, PHONE_NO, SERIAL_NO, ITEM_CODE,
                    SPECIFICATION, MU_CODE, QUANTITY, UNIT_PRICE, TOTAL_PRICE, REMARKS, FORM_CODE, COMPANY_CODE, BRANCH_CODE, CREATED_BY, CREATED_DATE, CURRENCY_CODE, EXCHANGE_RATE,
                    BRAND_NAME, DELIVERY_DATE, APPROVED_FLAG, APPROVED_BY, APPROVED_DATE,SESSION_ROWID,DELETED_FLAG,DELIVERY_DAYS
                    ) VALUES (
                '{data.Tender_No}', TO_DATE('{data.Created_Date.ToString("dd-MMM-yyyy")}', 'DD-MON-YYYY'), null,null, '{id}', '{data.SUPPLIER_Code}',
                '{data.Address}', NULL, '{data.Contact_No}', {id}, '{data.Item_Code}', '{data.Specification}', '{data.Index_Mu_Code}', {data.Quantity}, '{data.Rate}', '{data.Total_Net_Amount}', null, '{data.Form_Code}',
                '{data.Company_Code}', '{data.Branch_Code}', '{_workContext.CurrentUserinformation.login_code}', '{DateTime.Now.ToString("dd-MMM-yyyy")}','{data.Currency}', '{data.Currency_Rate}', '{data.Brand_Name}', TO_DATE('{data.Delivery_Date.ToString("dd-MMM-yyyy")}', 'DD-MON-YYYY'), 'Y', '{_workContext.CurrentUserinformation.login_code}', '{DateTime.Now.ToString("dd-MMM-yyyy")}'
                ,'{sessionRowId}','N',{deliveryDays} )";
                    _dbContext.ExecuteSqlCommand(insertItemQuery);
                }
                return true;
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);

            }

        }
        public bool SaveMasterColumnValue(List<QuotationDetails> quodata)
        {
            try
            {
                foreach (var data in quodata)
                {
                    string sQuery = $@"SELECT MYSEQUENCE.NEXTVAL FROM DUAL";
                    var sessionRowId = _dbContext.SqlQuery<int>(sQuery).FirstOrDefault();

                    string insertmasterQuery = $@"INSERT INTO MASTER_TRANSACTION(VOUCHER_NO, VOUCHER_AMOUNT, FORM_CODE, COMPANY_CODE, BRANCH_CODE, CREATED_BY, DELETED_FLAG, CURRENCY_CODE, CREATED_DATE, VOUCHER_DATE, SESSION_ROWID, SYN_ROWID, EXCHANGE_RATE, IS_SYNC_WITH_IRD, IS_REAL_TIME) 
            VALUES ('{data.Tender_No}', '{data.Total_Net_Amount}', '{data.Form_Code}', '{_workContext.CurrentUserinformation.company_code}', '{_workContext.CurrentUserinformation.branch_code}', '{_workContext.CurrentUserinformation.login_code.ToUpper()}', 'N', '{data.Currency}', '{DateTime.Now.ToString("dd-MMM-yyyy")}',
            '{DateTime.Now.ToString("dd-MMM-yyyy")}', '{sessionRowId}', '1', '{data.Currency_Rate}', 'N', 'N')";
                    _dbContext.ExecuteSqlCommand(insertmasterQuery);
                }
                return true;
            }
            catch (Exception ex)
            {
                //throw new Exception(ex.StackTrace);
                throw new Exception(ex.Message);
            }
        }
        public bool rejectQuotation(string quotationNo, string status)
        {
            try
            {
                var UPDATE_QUERY = $@"UPDATE quotation_details SET status ='{status}',rejected_by='{_workContext.CurrentUserinformation.login_code}',rejected_date='{DateTime.Now.ToString("dd - MMM - yyyy")}' WHERE QUOTATION_NO='{quotationNo}'";
                _dbContext.ExecuteSqlCommand(UPDATE_QUERY);
                return true;
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }
        public bool InsertTenderData(Tender data)
        {
            try
            {
                int tenderId = data.ID;
                if (tenderId == 0)
                {
                    var idquery = $@"SELECT COALESCE(MAX(ID) + 1, 1) AS id FROM QA_TENDER_SETUP";
                    int id = _dbContext.SqlQuery<int>(idquery).FirstOrDefault();
                    string insertQuery = string.Format(@"INSERT INTO QA_TENDER_SETUP(ID, PREFIX, SUFFIX,BODY_LENGTH,STATUS,CREATED_DATE, CREATED_BY, COMPANY_CODE) 
                                 VALUES({0},'{1}','{2}',{3},'{4}', TO_DATE('{5}', 'DD-MON-YYYY'),'{6}','{7}')",
                                              id,data.PREFIX,data.SUFFIX,data.BODY_LENGTH,"E",
                                              DateTime.Now.ToString("dd-MMM-yyyy"),
                                              _workContext.CurrentUserinformation.login_code,
                                              _workContext.CurrentUserinformation.company_code
                                              );
                    _dbContext.ExecuteSqlCommand(insertQuery);
                }
                else
                {
                    string updateQuery = $@"UPDATE QA_TENDER_SETUP 
                       SET PREFIX = '{data.PREFIX}',SUFFIX='{data.SUFFIX}',BODY_LENGTH={data.BODY_LENGTH},
                           MODIFIED_DATE = '{DateTime.Now.ToString("dd-MMM-yyyy")}',
                           MODIFIED_BY = '{_workContext.CurrentUserinformation.login_code}',
                           COMPANY_CODE = '{_workContext.CurrentUserinformation.company_code}'
                       WHERE id = '{data.ID}' ";
                    _dbContext.ExecuteSqlCommand(updateQuery);
                }
                return true;
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }

        public List<Tender> getTenderDetails()
        {
            try
            {
                string query = $@"SELECT * FROM QA_TENDER_SETUP WHERE STATUS='E' ORDER BY ID DESC";
                List<Tender> tenders = this._dbContext.SqlQuery<Tender>(query).ToList();
                return tenders;
            }
            catch(Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }
        public bool deleteTenderId(string id)
        {
            try
            {
                var UPDATE_QUERY = $@"UPDATE QA_TENDER_SETUP SET STATUS ='D' WHERE ID='{id}'";
                _dbContext.ExecuteSqlCommand(UPDATE_QUERY);
                return true;
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }
        public List<Tender> getTenderById( string id)
        {
            try
            {
                string query = $@"SELECT * FROM QA_TENDER_SETUP WHERE STATUS='E' AND ID='{id}'";
                List<Tender> tenders = this._dbContext.SqlQuery<Tender>(query).ToList();
                return tenders;
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }
        public List<QuotationCount> GetQuotationCount()
        {
            try
            {
                string query = $@"SELECT COUNT(*) AS count, 'Quotation Request' AS heading, '#bbbbf0' AS color, 'fa fa-star fa-2x' AS icon, 1 AS sortOrder
                                FROM sa_quotation_setup
                                WHERE status = 'E'
                                UNION 
                                SELECT COUNT(*) AS count, 'Quotation Received ' AS heading, '#f0bbee' AS color, 'fa fa-asterisk' AS icon, 2 AS sortOrder
                                FROM quotation_details sps 
                                LEFT JOIN sa_quotation_setup ps ON (ps.tender_no = sps.tender_no)
                                WHERE  ps.status = 'E' 
                                UNION 
                                SELECT COUNT(DISTINCT sps.quotation_no) AS count, 'Quotation Approved ' AS heading, '#f0d19c' AS color, 'fa fa-spinner' AS icon, 3 AS sortOrder
                                 FROM quotation_details sps 
                                LEFT JOIN sa_quotation_setup ps ON (ps.tender_no = sps.tender_no)
                                WHERE  ps.status = 'E' and sps.status='AP'
                                UNION 
                                SELECT ((SELECT COUNT(*) FROM sa_quotation_setup WHERE status = 'E') - (SELECT COUNT(DISTINCT sps.tender_no) 
                                 FROM quotation_details sps WHERE sps.status = 'AP' AND sps.tender_no IN (SELECT tender_no FROM sa_quotation_setup WHERE status = 'E'))) AS count, 
                                'Quotation Open' AS heading,  '#d4fa93' AS color,'fa fa-opera' AS icon, 4 AS sortOrder from dual ORDER BY sortOrder";

                List<QuotationCount> entity = this._dbContext.SqlQuery<QuotationCount>(query).ToList();
                return entity;
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }

    }
}
