﻿using NeoErp.Core;
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
            string query = $@"select FN_NEW_TENDERNO('{_workContext.CurrentUserinformation.company_code}','{_workContext.CurrentUserinformation.branch_code}') as TENDER_NO from dual";
            List<Quotation_setup> id = _dbContext.SqlQuery<Quotation_setup>(query).ToList();
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
                    string insertQuery = string.Format(@"INSERT INTO sa_quotation_setup(TENDER_NO, VALID_DATE, ISSUE_DATE, CREATED_DATE, CREATED_BY, COMPANY_CODE, STATUS,REMARKS,ID,APPROVED_STATUS) 
                                 VALUES('{0}', TO_DATE('{1}', 'DD-MON-YYYY'), TO_DATE('{2}', 'DD-MON-YYYY'), TO_DATE('{3}', 'DD-MON-YYYY'), '{4}', '{5}', '{6}','{7}',{8},'{9}')",
                                              data.TENDER_NO,
                                              data.VALID_DATE.HasValue ? $"{data.VALID_DATE.Value.ToString("dd-MMM-yyyy")}" : null,
                                              data.ISSUE_DATE.HasValue ? $"{data.ISSUE_DATE.Value.ToString("dd-MMM-yyyy")}" : null,
                                              DateTime.Now.ToString("dd-MMM-yyyy"),
                                              _workContext.CurrentUserinformation.User_id,
                                              _workContext.CurrentUserinformation.company_code,
                                              "E", data.REMARKS, id, "N");
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
                           MODIFIED_BY = '{_workContext.CurrentUserinformation.User_id}',
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
       CASE   WHEN approved_status = 'N' THEN 'No'  ELSE 'Yes'  END AS approved_status FROM sa_quotation_setup WHERE status = 'E' and company_code='{_workContext.CurrentUserinformation.company_code}' ORDER BY id desc";
            List<Quotation_setup> tenderDetails = _dbContext.SqlQuery<Quotation_setup>(query).ToList();
            return tenderDetails;
        }
        public bool DeleteTender(string tenderNo)
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
                string Query = $@"SELECT TENDER_NO,ISSUE_DATE,VALID_DATE,CREATED_DATE,bs_date(ISSUE_DATE) as NEPALI_DATE,REMARKS,ID FROM sa_quotation_setup WHERE TENDER_NO = '{tenderNo}' AND STATUS = 'E'";
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
        public List<Quotation_Details> QuotationDetailsById(string quotationNo)
        {
            try
            {
                string Query = $@"SELECT QUOTATION_NO,TENDER_NO,PAN_NO,PARTY_NAME,ADDRESS,CONTACT_NO,EMAIL,CURRENCY,CURRENCY_RATE,DELIVERY_DATE,
            TOTAL_AMOUNT,TOTAL_DISCOUNT,TOTAL_EXCISE,TOTAL_TAXABLE_AMOUNT,TOTAL_VAT,TOTAL_NET_AMOUNT,
            (CASE WHEN STATUS='RQ' then 'Pending' when status='AP' then 'Approved' else 'Reject' end) AS STATUS,
            TERM_CONDITION FROM  QUOTATION_DETAILS WHERE QUOTATION_NO='{quotationNo}'";
                List<Quotation_Details> quotations = this._dbContext.SqlQuery<Quotation_Details>(Query).ToList();
                foreach (var quotation in quotations)
                {
                    string query = $@"select * from QUOTATION_DETAIL_ITEMWISE where QUOTATION_NO='{quotation.QUOTATION_NO}'  order by id";
                    List<Item_details> itemData = this._dbContext.SqlQuery<Item_details>(query).ToList();
                    quotation.Item_Detail = itemData;
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
                string query = $@"SELECT TENDER_NO, ISSUE_DATE, VALID_DATE, CREATED_DATE, bs_date(ISSUE_DATE) as NEPALI_DATE, COMPANY_CODE FROM SA_QUOTATION_SETUP WHERE STATUS='E' AND TENDER_NO='{tenderNo}'";
                List<Quotation> quotations = this._dbContext.SqlQuery<Quotation>(query).ToList();
                foreach (var quotation in quotations)
                {
                    string vquery = $@"SELECT QD.QUOTATION_NO,QD.PARTY_NAME,QDI.RATE,QDI.ITEM_CODE,QD.STATUS FROM QUOTATION_DETAILS QD,QUOTATION_DETAIL_ITEMWISE QDI
                                    WHERE QD.QUOTATION_NO=QDI.QUOTATION_NO AND QD.TENDER_NO = '{quotation.TENDER_NO}'";
                                    List<PARTY_DETAIL> partyData = this._dbContext.SqlQuery<PARTY_DETAIL>(vquery).ToList();
                    quotation.PartDetails = partyData;
                    string vQuery = $@"SELECT SQI.*, 
                                   IIMS.ITEM_EDESC AS ITEM_DESC
                            FROM SA_QUOTATION_ITEMS SQI
                            INNER JOIN IP_ITEM_MASTER_SETUP IIMS ON IIMS.ITEM_CODE = SQI.ITEM_CODE
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

    }
}