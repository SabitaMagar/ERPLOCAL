using NeoErp.Core.Models;
using NeoErp.Distribution.Service.Model.Mobile;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using System.Xml.Linq;

namespace NeoErp.Distribution.Service.Service.Mobile
{
    public class MobileOfflineService : IMobileOfflineService
    {
        private const string CATEGORY_CODE = "FG";
        private const string GROUP_SKU_FLAG = "I";
        private readonly string UploadPath = System.Web.Hosting.HostingEnvironment.ApplicationPhysicalPath + @"Areas\NeoErp.Distribution\Images";
        public NeoErpCoreEntity _objectEntity = new NeoErpCoreEntity();
        public List<string> GetSyncIds(string tableName, List<string> SyncIds, NeoErpCoreEntity dbContext)
        {
            if (tableName == "DIST_SALES_RETURN")
            {
                var syncIdString = string.Join("'),(1,'", SyncIds);
                var query = $"SELECT SYN_ROWID FROM {tableName} WHERE (1,SYN_ROWID) IN ((1,'{syncIdString}'))"; //query might be confusing but it is the simplest way to handle more than 1000 items inside IN statement
                var data = dbContext.SqlQuery<string>(query).ToList();
                return data;
            }
            else
            {
                var syncIdString = string.Join("'),(1,'", SyncIds);
                var query = $"SELECT SYNC_ID FROM {tableName} WHERE (1,SYNC_ID) IN ((1,'{syncIdString}'))"; //query might be confusing but it is the simplest way to handle more than 1000 items inside IN statement
                var data = dbContext.SqlQuery<string>(query).ToList();
                return data;
            }
        }

        #region Private Functions
        private int GetMaxId(string table, string column, NeoErpCoreEntity dbContext)
        {
            var query = $"SELECT nvl(max({column}),0) +1 as p_key FROM {table}";
            var result = dbContext.SqlQuery<decimal>(query).FirstOrDefault();
            return Convert.ToInt32(result);
        }

        private long GetMaxIdSalesReturn(string table, string column, NeoErpCoreEntity dbContext)
        {
            var query = $"SELECT nvl(max({column}),0) +1 as p_key FROM {table}";
            var result = dbContext.SqlQuery<long>(query).FirstOrDefault();
            return result;
        }

        private List<ItemModelNew> FetchAllCompanyItems(VisitPlanRequestModel model, NeoErpCoreEntity dbContext,PreferenceModel pref)
        {
            string conversionClause = "";
            string CompanyFilter = "";
            if (pref.SQL_NN_CONVERSION_UNIT_FACTOR == "Y")
                conversionClause = "AND IUS.MU_CODE IS NOT NULL AND IUS.CONVERSION_FACTOR IS NOT NULL";
            CompanyFilter = pref.SQL_MULTIPLE_COMPANY == "Y" ? "SELECT COMPANY_CODE FROM COMPANY_SETUP" : $"'{model.COMPANY_CODE}'";
            var Query = $@"SELECT IM.ITEM_CODE, IM.ITEM_EDESC, ISS.BRAND_NAME, IM.INDEX_MU_CODE AS UNIT, IM.INDEX_MU_CODE AS MU_CODE, MC.MU_EDESC, IUS.MU_CODE CONVERSION_UNIT,TO_CHAR(IUS.CONVERSION_FACTOR) CONVERSION_FACTOR, IM.COMPANY_CODE, IM.BRANCH_CODE
				FROM IP_ITEM_MASTER_SETUP IM
				  LEFT JOIN IP_MU_CODE MC ON MC.MU_CODE = IM.INDEX_MU_CODE AND MC.COMPANY_CODE = IM.COMPANY_CODE
				  INNER JOIN IP_ITEM_SPEC_SETUP ISS ON ISS.ITEM_CODE = IM.ITEM_CODE AND ISS.COMPANY_CODE = IM.COMPANY_CODE AND TRIM(ISS.BRAND_NAME) IS NOT NULL
				  LEFT JOIN IP_ITEM_UNIT_SETUP IUS ON IUS.ITEM_CODE = ISS.ITEM_CODE AND IUS.COMPANY_CODE = ISS.COMPANY_CODE
                  LEFT JOIN DIST_USER_ITEM_MAPPING duim on duim.item_code = iss.item_code AND duim.company_code = iss.company_code
				WHERE 1 = 1	AND IM.COMPANY_CODE IN ({CompanyFilter}) AND IM.CATEGORY_CODE IN (select CATEGORY_CODE from IP_CATEGORY_CODE WHERE CATEGORY_TYPE IN ('FG','TF', 'HA') AND COMPANY_CODE='{model.COMPANY_CODE}')
                AND IM.GROUP_SKU_FLAG = '{GROUP_SKU_FLAG}' AND IM.DELETED_FLAG = 'N' and duim.sp_code='{model.spcode}' and duim.deleted_flag='N'
                {conversionClause}
				ORDER BY IM.COMPANY_CODE, IM.BRANCH_CODE, UPPER(IM.ITEM_EDESC) ASC";
            var data = dbContext.SqlQuery<ItemModelNew>(Query).ToList();
            return data;
        }
        private List<ItemModelNew> FetchAllSchemeItems(VisitPlanRequestModel model, NeoErpCoreEntity dbContext, PreferenceModel pref)
        {
            string conversionClause = "";
            string CompanyFilter = "";
            if (pref.SQL_NN_CONVERSION_UNIT_FACTOR == "Y")
                conversionClause = "AND IUS.MU_CODE IS NOT NULL AND IUS.CONVERSION_FACTOR IS NOT NULL";

            string item_category_code = "";
            try
            {
                var url = System.Web.HttpContext.Current.Request.Url.AbsoluteUri;
                string xmlpath = HttpContext.Current.Server.MapPath("~/Areas/NeoERP.Distribution/GiftItemCategory.xml");
                var xml = XDocument.Load(xmlpath);
                var condition_query = from c in xml.Root.Descendants("Vendor")
                                      where (string)c.Attribute("ModuleName") == "Distribution"
                                      select c.Element("ConditionQuery").Value;
                var result = condition_query.FirstOrDefault();
                if (result != null)
                {
                    item_category_code = result;
                }
            }
            catch (Exception)
            {
                item_category_code = "";
            }
            CompanyFilter = pref.SQL_MULTIPLE_COMPANY == "Y" ? "SELECT COMPANY_CODE FROM COMPANY_SETUP" : $"'{model.COMPANY_CODE}'";
            var Query = $@"SELECT IM.ITEM_CODE, IM.ITEM_EDESC, ISS.BRAND_NAME, IM.INDEX_MU_CODE AS UNIT, IM.INDEX_MU_CODE AS MU_CODE, MC.MU_EDESC, IUS.MU_CODE CONVERSION_UNIT,TO_CHAR(IUS.CONVERSION_FACTOR) CONVERSION_FACTOR, IM.COMPANY_CODE, IM.BRANCH_CODE
				FROM IP_ITEM_MASTER_SETUP IM
                  INNER JOIN BRD_CONTRACT_SCHEME_ITEM BCI ON IM.ITEM_CODE = BCI.ITEM_CODE AND BCI.COMPANY_CODE = IM.COMPANY_CODE
				  LEFT JOIN IP_MU_CODE MC ON MC.MU_CODE = IM.INDEX_MU_CODE AND MC.COMPANY_CODE = IM.COMPANY_CODE
				  INNER JOIN IP_ITEM_SPEC_SETUP ISS ON ISS.ITEM_CODE = IM.ITEM_CODE AND ISS.COMPANY_CODE = IM.COMPANY_CODE 
				  LEFT JOIN IP_ITEM_UNIT_SETUP IUS ON IUS.ITEM_CODE = ISS.ITEM_CODE AND IUS.COMPANY_CODE = ISS.COMPANY_CODE
                  LEFT JOIN DIST_USER_ITEM_MAPPING duim ON duim.item_code = iss.item_code AND duim.company_code = iss.company_code
				WHERE 1 = 1
				AND IM.COMPANY_CODE IN ({CompanyFilter}) AND IM.GROUP_SKU_FLAG = '{GROUP_SKU_FLAG}' AND IM.DELETED_FLAG = 'N'
                {conversionClause} {item_category_code} and duim.sp_code='{model.spcode}' and duim.deleted_flag='N'
				ORDER BY IM.COMPANY_CODE, IM.BRANCH_CODE, UPPER(IM.ITEM_EDESC) ASC";
            var data = dbContext.SqlQuery<ItemModelNew>(Query).ToList();
            return data;
        }

        private List<ItemModelNew> FetchAllSchemeGiftItems(VisitPlanRequestModel model, NeoErpCoreEntity dbContext, PreferenceModel pref)
        {
            string conversionClause = "";
            string CompanyFilter = "";
            if (pref.SQL_NN_CONVERSION_UNIT_FACTOR == "Y")
                conversionClause = "AND IUS.MU_CODE IS NOT NULL AND IUS.CONVERSION_FACTOR IS NOT NULL";
            CompanyFilter = pref.SQL_MULTIPLE_COMPANY == "Y" ? "SELECT COMPANY_CODE FROM COMPANY_SETUP" : $"'{model.COMPANY_CODE}'";

            string item_category_code = "";
            try
            {
                var url = System.Web.HttpContext.Current.Request.Url.AbsoluteUri;
                string xmlpath = HttpContext.Current.Server.MapPath("~/Areas/NeoERP.Distribution/GiftItemCategory.xml");
                var xml = XDocument.Load(xmlpath);
                var condition_query = from c in xml.Root.Descendants("Vendor")
                                      where (string)c.Attribute("ModuleName") == "Branding"
                                      select c.Element("ConditionQuery").Value;
                var result = condition_query.FirstOrDefault();
                if (result != null)
                {
                    item_category_code = result;
                }
            }
            catch (Exception)
            {
                item_category_code = "";
            }

            var Query = $@"SELECT IM.ITEM_CODE, IM.ITEM_EDESC, ISS.BRAND_NAME, IM.INDEX_MU_CODE AS UNIT, IM.INDEX_MU_CODE AS MU_CODE, MC.MU_EDESC, IUS.MU_CODE CONVERSION_UNIT,TO_CHAR(IUS.CONVERSION_FACTOR) CONVERSION_FACTOR, IM.COMPANY_CODE, IM.BRANCH_CODE
				FROM IP_ITEM_MASTER_SETUP IM
                  INNER JOIN BRD_CONTRACT_ITEMS BCI ON IM.ITEM_CODE = BCI.ITEM_CODE AND BCI.COMPANY_CODE = IM.COMPANY_CODE
				  LEFT JOIN IP_MU_CODE MC ON MC.MU_CODE = IM.INDEX_MU_CODE AND MC.COMPANY_CODE = IM.COMPANY_CODE
				  INNER JOIN IP_ITEM_SPEC_SETUP ISS ON ISS.ITEM_CODE = IM.ITEM_CODE AND ISS.COMPANY_CODE = IM.COMPANY_CODE 
				  LEFT JOIN IP_ITEM_UNIT_SETUP IUS ON IUS.ITEM_CODE = ISS.ITEM_CODE AND IUS.COMPANY_CODE = ISS.COMPANY_CODE
                LEFT JOIN DIST_USER_ITEM_MAPPING duim ON duim.item_code = iss.item_code AND duim.company_code = iss.company_code
				WHERE 1 = 1
				AND IM.COMPANY_CODE IN ({CompanyFilter}) --AND IM.CATEGORY_CODE = '{CATEGORY_CODE}'
                AND IM.GROUP_SKU_FLAG = '{GROUP_SKU_FLAG}' AND IM.DELETED_FLAG = 'N' and duim.sp_code='{model.spcode}' and duim.deleted_flag='N'
                {conversionClause} 
				ORDER BY IM.COMPANY_CODE, IM.BRANCH_CODE, UPPER(IM.ITEM_EDESC) ASC";
            var data = dbContext.SqlQuery<ItemModelNew>(Query).ToList();
            return data;
        }

        private List<ItemModelRate> FetchAllCompanyBranchItemRate(VisitPlanRequestModel model, NeoErpCoreEntity dbContext,PreferenceModel pref)
        {
            var CompanyFilter = pref.SQL_MULTIPLE_COMPANY == "Y" ? "SELECT COMPANY_CODE FROM COMPANY_SETUP" : $"'{model.COMPANY_CODE}'";
            var Query = string.Empty;
            if (pref.PO_RATE_TABLE == "IP_ITEM_RATE_SCHEDULE_SETUP")
                Query = $@"SELECT DISTINCT A.CS_CODE CUSTOMER_CODE,A.COMPANY_CODE,A.ITEM_CODE,
                            TO_CHAR(NVL(A.STANDARD_RATE, 0)) SALES_RATE,TO_CHAR(NVL(A.MRP_RATE,0)) MRP_RATE,
                            TO_CHAR(NVL(A.RETAIL_PRICE,0)) RETAIL_PRICE,TO_CHAR(A.EFFECTIVE_DATE) APPLY_DATE
                    FROM IP_ITEM_RATE_SCHEDULE_SETUP A,
                    LEFT JOIN DIST_USER_ITEM_MAPPING duim ON duim.item_code = a.item_code AND duim.company_code = a.company_code
                    WHERE EFFECTIVE_DATE = (SELECT MAX(TO_DATE(EFFECTIVE_DATE)) FROM IP_ITEM_RATE_SCHEDULE_SETUP 
                            WHERE ITEM_CODE = A.ITEM_CODE 
                            AND CS_CODE=A.CS_CODE
                            AND COMPANY_CODE = A.COMPANY_CODE )
                    AND A.COMPANY_CODE IN ({CompanyFilter}) and duim.sp_code='{model.spcode}' and duim.deleted_flag='N'
                    ORDER BY A.ITEM_CODE,A.CS_CODE";
            else
                Query = $@"SELECT B.COMPANY_CODE, B.BRANCH_CODE, A.ITEM_CODE, TO_CHAR(NVL(B.{pref.PO_RATE_COLUMN}, 0)) SALES_RATE, TO_CHAR(A.APPLY_DATE) APPLY_DATE
                            FROM (SELECT ITEM_CODE, COMPANY_CODE, BRANCH_CODE,MAX(APP_DATE) APPLY_DATE
                            FROM {pref.PO_RATE_TABLE}
                            GROUP BY ITEM_CODE, COMPANY_CODE, BRANCH_CODE) A
                            INNER JOIN {pref.PO_RATE_TABLE} B
                            ON B.ITEM_CODE = A.ITEM_CODE
                            AND B.APP_DATE = A.APPLY_DATE
                            AND B.COMPANY_CODE = A.COMPANY_CODE
                            AND B.COMPANY_CODE IN ({CompanyFilter})
                            AND B.BRANCH_CODE = A.BRANCH_CODE
                            AND SALES_RATE <> 0
                            LEFT JOIN DIST_USER_ITEM_MAPPING duim ON duim.item_code = a.item_code AND duim.company_code = a.company_code    
                            where duim.sp_code='{model.spcode}' and duim.deleted_flag='N' ORDER BY B.COMPANY_CODE, B.BRANCH_CODE, TO_NUMBER(A.ITEM_CODE)";
            var data = dbContext.SqlQuery<ItemModelRate>(Query).ToList();
            return data;
        }

        private List<SubLedgerMapModel> FetchAllCompanySubLedgerMap(VisitPlanRequestModel model, NeoErpCoreEntity dbContext,PreferenceModel pref)
        {
            var CompanyFilter = pref.SQL_MULTIPLE_COMPANY == "Y" ? "SELECT COMPANY_CODE FROM COMPANY_SETUP" : $"'{model.COMPANY_CODE}'";

            var Query = $@"SELECT SLM.ACC_CODE, SLM.SUB_CODE, CS.PARTY_TYPE_CODE AS DEFAULT_PARTY_TYPE_CODE, SLM.COMPANY_CODE
                FROM FA_SUB_LEDGER_MAP SLM
                INNER JOIN SA_CUSTOMER_SETUP CS ON TRIM(CS.LINK_SUB_CODE) = TRIM(SLM.SUB_CODE) AND CS.GROUP_SKU_FLAG = 'I' AND CS.COMPANY_CODE = SLM.COMPANY_CODE
                LEFT join DIST_USER_AREAS  dua on dua.customer_code=cs.customer_code AND dua.company_code = slm.company_code
                WHERE SUBSTR(SLM.SUB_CODE, 1, 1) = 'C'
                AND SLM.COMPANY_CODE IN({CompanyFilter}) and dua.sp_code='{model.spcode}' and dua.deleted_flag='N'
                ORDER BY TO_NUMBER(SLM.ACC_CODE), TO_NUMBER(SUBSTR(SLM.SUB_CODE, 2)), SLM.COMPANY_CODE";
            var data = dbContext.SqlQuery<SubLedgerMapModel>(Query).ToList();
            return data;
        }

        private List<PartyTypeModel> FetchAllCompanyPartyType(VisitPlanRequestModel model, NeoErpCoreEntity dbContext,PreferenceModel pref)
        {
            var CompanyFilter = pref.SQL_MULTIPLE_COMPANY == "Y" ? "SELECT COMPANY_CODE FROM COMPANY_SETUP" : $"'{model.COMPANY_CODE}'";
            var query = $@"SELECT a.party_type_code,a.party_type_edesc party_type_name, to_char(a.credit_days) credit_days,to_char(a.credit_limit) credit_limit,
                    a.company_code FROM ip_party_type_code a
                    left join sa_customer_setup b on a.party_type_code=b.party_type_code and a.company_code=b.company_code
                    left join dist_user_areas c on c.customer_code=b.customer_code and  c.company_code=b.company_code
                    WHERE
                    a.company_code IN ({CompanyFilter} ) and b.group_sku_flag='I' and b.deleted_flag = 'N' and b.active_flag='Y' AND a.deleted_flag = 'N' 
                    and c.deleted_flag = 'N' and c.sp_code='{model.spcode}'";
            var data = dbContext.SqlQuery<PartyTypeModel>(query).ToList();
            return data;
        }

        private List<CustomerModel> FetchAllCompanySaCustomer(VisitPlanRequestModel model, NeoErpCoreEntity dbContext, PreferenceModel pref)
        {
            var CompanyFilter = pref.SQL_MULTIPLE_COMPANY == "Y" ? "SELECT COMPANY_CODE FROM COMPANY_SETUP" : $"'{model.COMPANY_CODE}'";

            ////old query by Himal dai

            var Query = $@"SELECT a.customer_code, a.customer_edesc customer_name,a.regd_office_eaddress   address,a.party_type_code,
            a.link_sub_code,a.acc_code, to_char(a.credit_days) credit_days,to_char(a.credit_limit) credit_limit,a.company_code, a.branch_code
            FROM sa_customer_setup a left join dist_user_areas b on a.customer_code=b.customer_code and a.company_code=b.company_code
            WHERE  a.group_sku_flag = 'I' AND a.company_code IN ({CompanyFilter})  AND a.deleted_flag = 'N'  and b.sp_code='{model.spcode}'  AND  b.deleted_flag = 'N'";

            //new query by Bikalp dai
            //var Query = $@"SELECT DP.CUSTOMER_CODE,CS.CUSTOMER_EDESC CUSTOMER_NAME,CS.REGD_OFFICE_EADDRESS ADDRESS,DP.PARTY_TYPE_CODE,CS.LINK_SUB_CODE, CS.ACC_CODE,
            //     TO_CHAR(CS.CREDIT_DAYS) CREDIT_DAYS, To_CHAR(CS.CREDIT_LIMIT) CREDIT_LIMIT, CS.COMPANY_CODE, CS.BRANCH_CODE
            //    FROM FA_SUB_LEDGER_DEALER_MAP DP ,IP_PARTY_TYPE_CODE IP,SA_CUSTOMER_SETUP CS
            //    WHERE IP.COMPANY_CODE=DP.COMPANY_CODE
            //    AND IP.PARTY_TYPE_CODE=DP.PARTY_TYPE_CODE
            //    AND CS.CUSTOMER_CODE=DP.CUSTOMER_CODE
            //    AND CS.COMPANY_CODE=IP.COMPANY_CODE
            //    AND DP.DELETED_FLAG='N'
            //    AND IP.PARTY_TYPE_FLAG='D'
            //    AND DP.COMPANY_CODE IN ({CompanyFilter})";
            var data = dbContext.SqlQuery<CustomerModel>(Query).ToList();
            return data;
        }

        private List<AreaResponseModel> FetchAllCompanyArea(VisitPlanRequestModel model, NeoErpCoreEntity dbContext, PreferenceModel pref)
        {
            var CompanyFilter = pref.SQL_MULTIPLE_COMPANY == "Y" ? "SELECT COMPANY_CODE FROM COMPANY_SETUP" : $"'{model.COMPANY_CODE}'";
            string Query = $@"SELECT A.AREA_CODE,A.AREA_NAME,A.ZONE_CODE,A.DISTRICT_CODE,A.VDC_CODE,A.REG_CODE,
                       B.ZONE_NAME,B.DISTRICT_NAME,B.VDC_NAME,B.REG_NAME,A.COMPANY_CODE,TO_CHAR(A.GROUPID) GROUP_ID
                FROM DIST_AREA_MASTER A,DIST_ADDRESS_MASTER B,dist_user_areas  c
                WHERE (A.DISTRICT_CODE = B.DISTRICT_CODE and a.area_code = c.area_code and a.company_code = c.company_code
                AND  A.REG_CODE = B.REG_CODE
                AND  A.ZONE_CODE = B.ZONE_CODE
                AND a.COMPANY_CODE IN ({CompanyFilter}) and c.sp_code='{model.spcode}' and c.deleted_flag='N'
                AND  A.VDC_CODE = B.VDC_CODE) ORDER BY UPPER(A.AREA_NAME) ASC";
            var data = dbContext.SqlQuery<AreaResponseModel>(Query).ToList();
            return data;
        }

        private List<SalesTypeModel> FetchAllCompanySaSalesType(string companyCode, NeoErpCoreEntity dbContext, PreferenceModel pref)
        {
            var CompanyFilter = pref.SQL_MULTIPLE_COMPANY == "Y" ? "SELECT COMPANY_CODE FROM COMPANY_SETUP" : $"'{companyCode}'";
            string Query = $@"SELECT SALES_TYPE_CODE, SALES_TYPE_EDESC, COMPANY_CODE FROM SA_SALES_TYPE
                            WHERE GROUP_SKU_FLAG = '{GROUP_SKU_FLAG}' AND DELETED_FLAG = 'N'
                            AND COMPANY_CODE IN ({CompanyFilter})
                            ORDER BY COMPANY_CODE, UPPER(TRIM(SALES_TYPE_EDESC))";
            var data = dbContext.SqlQuery<SalesTypeModel>(Query).ToList();
            return data;
        }

        private List<ShippingAddressModel> FetchShippingAddress(NeoErpCoreEntity dbContext, PreferenceModel pref)
        {
            //var CompanyFilter = pref.SQL_MULTIPLE_COMPANY == "Y" ? "SELECT COMPANY_CODE FROM COMPANY_SETUP" : $"'{companyCode}'";
            string Query = $@"SELECT TRIM(CC.CITY_CODE) CITY_CODE, TRIM(CC.CITY_EDESC) CITY_EDESC, TRIM(DC.DISTRICT_EDESC) DISTRICT_EDESC,
                (CASE WHEN TRIM(CC.CITY_EDESC) = TRIM(DC.DISTRICT_EDESC) THEN TRIM(CC.CITY_EDESC) ELSE TRIM(CC.CITY_EDESC) || ', ' || TRIM(DC.DISTRICT_EDESC) END) CITY
                FROM CITY_CODE CC
                INNER JOIN DISTRICT_CODE DC ON TRIM(DC.DISTRICT_CODE) = TRIM(CC.DISTRICT_CODE)
                WHERE 1 = 1 
                AND DELETED_FLAG = 'N'
                ORDER BY UPPER(TRIM(CITY_EDESC))";
            var data = dbContext.SqlQuery<ShippingAddressModel>(Query).ToList();
            data = data == null ? new List<ShippingAddressModel>() : data;
            return data;
        }

        private PreferenceModel FetchPreferences(string comp_code, NeoErpCoreEntity dbContext)
        {
            var PreferenceQuery = $"SELECT * FROM DIST_PREFERENCE_SETUP WHERE COMPANY_CODE='{comp_code}'";
            var preferences = dbContext.SqlQuery<PreferenceModel>(PreferenceQuery).FirstOrDefault();
            return preferences;
        }

        private List<NotificationModel> GetNotifications(string sp_code, NeoErpCoreEntity dbContext)
        {
            var query = $"SELECT NOTIFICATION_ID,NOTIFICATION_TITLE,NOTIFICATION_TEXT FROM DIST_NOTIFICATIONS WHERE DELETED_FLAG='N' AND (TRIM(SP_CODE) IS NULL OR TRIM(SP_CODE)='{sp_code}') AND ROWNUM<=10";
            var data = dbContext.SqlQuery<NotificationModel>(query).ToList();
            return data;
        }

        private List<CompetitorItemModel> GetCompItems(VisitPlanRequestModel model, NeoErpCoreEntity dbContext)
        {
            string Query = $@"SELECT DISTINCT a.item_id AS item_code, a.item_edesc
            FROM dist_comp_item_master a left join DIST_USER_ITEM_MAPPING b on a.item_id=b.item_code and a.company_code=b.company_code
            WHERE  a.deleted_flag = 'N' and b.deleted_flag = 'N' and b.sp_code='{model.spcode}' and a.company_code in ('{model.COMPANY_CODE}')";
            var data = dbContext.SqlQuery<CompetitorItemModel>(Query).ToList();
            return data;
        }
        
        private List<CompetitorItemMapModel> GetCompItemMaps(VisitPlanRequestModel model, NeoErpCoreEntity dbContext)
        {
            string Query = $@"SELECT IMS.ITEM_EDESC, DIM.ITEM_CODE,
                             WM_CONCAT (DIM.COMP_ITEM_ID) COMP_ITEM_CODES
                        FROM DIST_COMP_ITEM_MAP DIM INNER JOIN DIST_COMP_ITEM_MASTER DCM ON DCM.ITEM_ID = DIM.COMP_ITEM_ID AND DCM.COMPANY_CODE = DIM.COMPANY_CODE
                             INNER JOIN IP_ITEM_MASTER_SETUP IMS ON IMS.ITEM_CODE = DIM.ITEM_CODE AND IMS.COMPANY_CODE = DIM.COMPANY_CODE
                            LEFT JOIN DIST_USER_ITEM_MAPPING duim ON duim.item_code = dim.item_code AND duim.company_code = dim.company_code 
                        WHERE IMS.DELETED_FLAG = 'N' duim.deleted_flag = 'N' and dim.company_code in ('{model.COMPANY_CODE}') and duim.sp_code='{model.spcode}'
                             AND DCM.DELETED_FLAG = 'N'
                    GROUP BY IMS.ITEM_EDESC, DIM.ITEM_CODE
                    ORDER BY ITEM_CODE";
            var data = dbContext.SqlQuery<CompetitorItemMapModel>(Query).ToList();
            return data;
        }

        private List<CompetitorItemFields> GetCompItemFields(VisitPlanRequestModel model, NeoErpCoreEntity dbContext)
        {
            var query = $@"SELECT a.FIELD_ID QUESTION_ID, a.ITEM_CODE,a.COL_NAME,a.COL_DATA_TYPE FROM DIST_COMP_FIELDS a
                LEFT JOIN DIST_USER_ITEM_MAPPING b on a.item_code=b.item_code and a.company_code=b.company_code
                WHERE a.COMPANY_CODE in  ('{model.COMPANY_CODE}') and b.sp_code='{model.spcode}'";
            var data = dbContext.SqlQuery<CompetitorItemFields>(query).ToList();
            return data;
        }

        private List<GroupMapModel> GetGroupMaps(string CompanyCode, NeoErpCoreEntity dbContext)
        {
            var query = $@"SELECT TO_CHAR(GROUPID) GROUP_CODE,WM_CONCAT(MAPPED_GROUPID) MAPPED_GROUPS,
                        (SELECT WM_CONCAT(''''||AREA_CODE||'''') FROM DIST_AREA_MASTER WHERE GROUPID IN (SELECT MAPPED_GROUPID FROM DIST_GROUP_MAPPING WHERE GROUPID = DGM.GROUPID)) MAPPED_GROUP_CODES
                        FROM DIST_GROUP_MAPPING DGM
                        WHERE COMPANY_CODE = '{CompanyCode}'
                        GROUP BY GROUPID";
            var data = dbContext.SqlQuery<GroupMapModel>(query).ToList();
            return data;
        }

        private List<CrmModel> GetCrmTasks(string sp_code, NeoErpCoreEntity dbContext)
        {
            var query = $@"SELECT CLT.LEAD_NO,CLT.AGENT_CODE,HES.EMPLOYEE_EDESC,CLT.DESCRIPTION LEAD_ISSUE,CPF.DESCRIPTION REMARKS,
                CLT.CUSTOMER_CODE,PM.PROCESS_EDESC,CPF.START_DATE,CPF.PROCESS_NO,CPF.PROCESS_CODE,SCS.CUSTOMER_EDESC
                FROM CRM_LEAD_TRANSACTION CLT
                INNER JOIN CRM_PROCESS_FOLLOWUP CPF ON CLT.LEAD_NO = CPF.LEAD_NO AND CLT.COMPANY_CODE = CPF.COMPANY_CODE
                INNER JOIN HR_EMPLOYEE_SETUP HES ON CLT.AGENT_CODE = HES.EMPLOYEE_CODE AND CLT.COMPANY_CODE = HES.COMPANY_CODE
                INNER JOIN CRM_PROCESS_MASTER PM ON CPF.PROCESS_CODE=PM.PROCESS_CODE AND CLT.COMPANY_CODE=PM.COMPANY_CODE
                INNER JOIN SA_CUSTOMER_SETUP SCS ON SCS.CUSTOMER_CODE = CLT.CUSTOMER_CODE AND SCS.COMPANY_CODE = CLT.COMPANY_CODE
                WHERE CLT.AGENT_CODE = '{sp_code}' AND CPF.CLOSE_FLAG = '0' AND CLT.CLOSE_FLAG = '0' AND PM.VISIT_FLAG='1'";
            var data = dbContext.SqlQuery<CrmModel>(query).ToList();
            return data;
        }
        #endregion Private Functions

        #region Fetching Data
        public List<LoginResponseModel> Login(LoginModel model, NeoErpCoreEntity dbContext)
        {
            LoginResponseModel result = new LoginResponseModel();
            if (string.IsNullOrEmpty(model.UserName as string))
                throw new Exception("Username is empty.");

            if (string.IsNullOrEmpty(model.Password as string))
                throw new Exception("Password is empty.");

            //user validation
            string UserQuery = $@"SELECT TO_CHAR(LU.USERID) AS USER_ID, TO_CHAR(LU.GROUPID) AS GROUP_ID, LU.USER_NAME, LU.IS_MOBILE, LU.ATTENDANCE, 
                    ES.EMPLOYEE_EDESC AS FULL_NAME, LU.PASS_WORD, LU.CONTACT_NO, LU.SP_CODE, 
                    LU.USER_TYPE, LU.SUPER_USER, TO_CHAR(LU.EXPIRY_DATE) EXPIRY_DATE, TO_CHAR(LU.LINK_SYN_USER_NO) AS LINK_SYN_USER_NO,TO_CHAR(DRU.ROLE_CODE) AS ROLE_CODE, LU.COMPANY_CODE 
                FROM DIST_LOGIN_USER LU
                INNER JOIN DIST_SALESPERSON_MASTER SM ON SM.SP_CODE = LU.SP_CODE
                INNER JOIN HR_EMPLOYEE_SETUP ES ON ES.EMPLOYEE_CODE = SM.SP_CODE AND ES.COMPANY_CODE = LU.COMPANY_CODE --for user's actual company code in synergy
                INNER JOIN DIST_ROLE_USER DRU ON DRU.USERID = LU.USERID
                WHERE 1 = 1
                AND LU.USER_NAME = '{model.UserName}' 
                AND LU.PASS_WORD = '{model.Password}'
                AND LU.ACTIVE = 'Y'";
            result = dbContext.SqlQuery<LoginResponseModel>(UserQuery).FirstOrDefault();
            if (result == null)
                throw new Exception("Incorrect username or password.");
            DateTime userDate;
            DateTime.TryParse(result.EXPIRY_DATE, out userDate);
            if (userDate < DateTime.Now)
                throw new Exception("User is expired.");
            if (result.IS_MOBILE == "N")
                throw new Exception("Not a Mobile User.");

            //attendance
            model.Attendance = model.Attendance == null ? "N" : model.Attendance;
            if (model.Attendance.Equals("Y", StringComparison.OrdinalIgnoreCase))
            {
                if (result.USER_TYPE.ToUpper() == "E" || result.USER_TYPE.ToUpper() == "S")
                {
                    var time = DateTime.Now.ToString("dd-MMM-yyyy hh:mm:ss.fffffff tt", CultureInfo.InvariantCulture);
                    string AttendanceQuery = $@"INSERT INTO HRIS_ATTENDANCE (EMPLOYEE_ID,ATTENDANCE_DT,ATTENDANCE_FROM,ATTENDANCE_TIME) VALUES ('{result.SP_CODE}',TRUNC(SYSDATE),'MOBILE', TO_TIMESTAMP('{time}'))";
                    var row = dbContext.ExecuteSqlCommand(AttendanceQuery);
                    try
                    {
                        var thumbId = dbContext.SqlQuery<string>($"SELECT ID_THUMB_ID FROM HRIS_EMPLOYEES WHERE EMPLOYEE_ID = '{result.SP_CODE}'").FirstOrDefault();
                        if (!string.IsNullOrWhiteSpace(thumbId))
                        {
                            var hris_procedure = $"BEGIN HRIS_ATTENDANCE_INSERT ({thumbId}, TRUNC(SYSDATE), NULL, 'MOBILE', TO_TIMESTAMP('{time}')); END;";
                            dbContext.ExecuteSqlCommand(hris_procedure);
                        }
                    }
                    catch(Exception ex)
                    {

                    }
                    if (row <= 0)
                        throw new Exception("Attendance could not be made");
                }
            }

            var compQuery = $@"SELECT CC.COMPANY_CODE, INITCAP(TRIM(CS.COMPANY_EDESC)) COMPANY_EDESC, 
                       TO_CHAR(PS.FY_START_DATE, 'YYYY-MM-DD') AS FISCAL_START, TO_CHAR(PS.FY_END_DATE, 'YYYY-MM-DD') AS FISCAL_END, 
                       BC.BRANCH_CODE, TRIM(BS.BRANCH_EDESC) BRANCH_EDESC
                FROM SC_COMPANY_CONTROL CC
                INNER JOIN SC_APPLICATION_USERS AU ON AU.USER_NO = CC.USER_NO AND AU.COMPANY_CODE = CC.COMPANY_CODE AND AU.EMPLOYEE_CODE = '{result.SP_CODE}'
                INNER JOIN COMPANY_SETUP CS ON CS.COMPANY_CODE = CC.COMPANY_CODE
                LEFT JOIN SC_BRANCH_CONTROL BC ON BC.USER_NO = AU.USER_NO AND BC.COMPANY_CODE = CC.COMPANY_CODE
                INNER JOIN FA_BRANCH_SETUP BS ON BS.BRANCH_CODE = BC.BRANCH_CODE AND BS.COMPANY_CODE = BC.COMPANY_CODE AND BS.GROUP_SKU_FLAG = 'I' AND BS.DELETED_FLAG = 'N'
                LEFT JOIN (SELECT DISTINCT COMPANY_CODE, FY_START_DATE, FY_END_DATE FROM PREFERENCE_SETUP) PS ON PS.COMPANY_CODE = CC.COMPANY_CODE
                WHERE 1 = 1
                ORDER BY UPPER(TRIM(CS.COMPANY_EDESC)), UPPER(TRIM(BS.BRANCH_EDESC)), CC.COMPANY_CODE, BS.BRANCH_CODE";
            var allCompany = dbContext.SqlQuery<TempCompanyModel>(compQuery).GroupBy(x => x.COMPANY_CODE);
            var companies = new List<CompanyModel>();
            foreach (var group in allCompany)
            {
                var compTemp = group.FirstOrDefault();
                var comp = new CompanyModel()
                {
                    COMPANY_CODE = compTemp.COMPANY_CODE,
                    COMPANY_EDESC = compTemp.COMPANY_EDESC,
                    FISCAL_START = compTemp.FISCAL_START,
                    FISCAL_END = compTemp.FISCAL_END
                };
                foreach (var branch in group)
                {
                    comp.BRANCH.Add(branch.BRANCH_CODE, new BranchModel
                    {
                        BRANCH_CODE = branch.BRANCH_CODE,
                        BRANCH_EDESC = branch.BRANCH_EDESC
                    });
                }
                comp.PREFERENCE = FetchPreferences(comp.COMPANY_CODE, dbContext);
                result.COMPANY.Add(comp.COMPANY_CODE, comp);
            }


            //fetching Company and branches
            //if (result.USER_TYPE == "S")
            //{
            //    string companyQuery = string.Empty;
            //    if (result.LINK_SYN_USER_NO == null)
            //    {
            //        companyQuery = $@"SELECT DISTINCT A.COMPANY_CODE,A.COMPANY_EDESC,
            //        TO_CHAR(B.FY_START_DATE, 'YYYY-MM-DD') AS FISCAL_START, TO_CHAR(B.FY_END_DATE, 'YYYY-MM-DD') AS FISCAL_END  
            //        FROM COMPANY_SETUP A LEFT JOIN PREFERENCE_SETUP B ON B.COMPANY_CODE=A.COMPANY_CODE
            //        WHERE A.DELETED_FLAG='N'";
            //    }
            //    else
            //    {
            //        companyQuery = $@"SELECT DISTINCT A.COMPANY_CODE,A.COMPANY_EDESC,
            //        TO_CHAR(B.FY_START_DATE, 'YYYY-MM-DD') AS FISCAL_START, TO_CHAR(B.FY_END_DATE, 'YYYY-MM-DD') AS FISCAL_END
            //        FROM COMPANY_SETUP A  LEFT JOIN PREFERENCE_SETUP B ON B.COMPANY_CODE=A.COMPANY_CODE
            //        WHERE A.DELETED_FLAG='N' AND
            //        A.COMPANY_CODE IN (SELECT COMPANY_CODE FROM SC_COMPANY_CONTROL WHERE USER_NO ='{ result.LINK_SYN_USER_NO }')";
            //    }
            //    var companies = dbContext.SqlQuery<CompanyModel>(companyQuery).ToList();

            //    string branchQuery = string.Empty;
            //    for (int i = 0; i < companies.Count; i++)
            //    {
            //        var comp = companies[i];

            //        //fetching branches
            //        if (result.LINK_SYN_USER_NO == null)
            //        {
            //            branchQuery = $@"SELECT BRANCH_CODE,BRANCH_EDESC FROM FA_BRANCH_SETUP WHERE DELETED_FLAG='N' AND GROUP_SKU_FLAG='I' AND COMPANY_CODE='{comp.COMPANY_CODE}' ORDER BY BRANCH_EDESC ASC";
            //        }
            //        else
            //        {
            //            branchQuery = $@"SELECT BRANCH_CODE,BRANCH_EDESC FROM FA_BRANCH_SETUP 
            //            WHERE DELETED_FLAG='N' AND 
            //            BRANCH_CODE IN (SELECT BRANCH_CODE FROM SC_BRANCH_CONTROL WHERE USER_NO ='{result.LINK_SYN_USER_NO }' AND COMPANY_CODE='{comp.COMPANY_CODE}') ORDER BY BRANCH_EDESC ASC";
            //        }
            //        var branches = dbContext.SqlQuery<BranchModel>(branchQuery).ToList();
            //        foreach (var branch in branches)
            //        {
            //            comp.BRANCH.Add(branch.BRANCH_CODE, branch);
            //        }

            //        //fetching preferences
            //        //PreferenceQuery = $"SELECT * FROM DIST_PREFERENCE_SETUP WHERE COMPANY_CODE='{comp.COMPANY_CODE}'";
            //        //var preferences = dbContext.SqlQuery<PreferenceModel>(PreferenceQuery).FirstOrDefault();
            //        comp.PREFERENCE = FetchPreferences(comp.COMPANY_CODE, dbContext);

            //        result.COMPANY.Add(comp.COMPANY_CODE, comp);
            //    }
            //    //result.COMPANY = companies;
            //}

            var list = new List<LoginResponseModel>();
            list.Add(result);
            return list;
        }

        public Dictionary<string, string> Logout(LogoutRequestModel model, NeoErpCoreEntity dbContext)
        {
            string AttendanceQuery = $@"INSERT INTO HRIS_ATTENDANCE (EMPLOYEE_ID,ATTENDANCE_DT,ATTENDANCE_FROM,ATTENDANCE_TIME) VALUES ('{model.SP_CODE}',TRUNC(SYSDATE),'MOBILE',CURRENT_TIMESTAMP)";
            var row = dbContext.ExecuteSqlCommand(AttendanceQuery);
            var time = DateTime.Now.ToString("dd-MMM-yyyy hh:mm:ss.fffffff tt", CultureInfo.InvariantCulture);
            try
            {
                var thumbId = dbContext.SqlQuery<string>($"SELECT ID_THUMB_ID FROM HRIS_EMPLOYEES WHERE EMPLOYEE_ID = '{model.SP_CODE}'").FirstOrDefault();
                if (!string.IsNullOrWhiteSpace(thumbId))
                {
                    var hris_procedure = $"BEGIN HRIS_ATTENDANCE_INSERT ({thumbId}, TRUNC(SYSDATE), NULL, 'MOBILE', TO_TIMESTAMP('{time}')); END;";
                    dbContext.ExecuteSqlCommand(hris_procedure);
                }
            }
            catch(Exception ex)
            {

            }
            if (row <= 0)
                throw new Exception("Attendance could not be made");
            else
            {
                Dictionary<string, string> result = new Dictionary<string, string>();
                result.Add("msg", "Your logout attendance has been made successfully");
                return result;
            }
        }

        //public Dictionary<string, VisitPlanResponseModel> GetVisitPlan(VisitPlanRequestModel model, NeoErpCoreEntity dbContext)
        //{
        //    string Superuser;

        //    string today = string.Empty;
        //    if (string.IsNullOrEmpty(model.date))
        //        today = DateTime.Now.AddDays(-15).ToString("dd-MMM-yyyy");
        //    else
        //        today = model.date;

        //    Dictionary<string, VisitPlanResponseModel> Result = new Dictionary<string, VisitPlanResponseModel>();
        //    VisitPlanResponseModel VisitPlans = new VisitPlanResponseModel();

        //    if (string.IsNullOrEmpty(model.spcode as string))
        //        throw new Exception("User code is empty.");
        //    string VisitQuery = string.Empty;
        //    string UserQuery = $@"select super_user from dist_login_user where sp_code = '{model.spcode}'";
        //    var users = dbContext.SqlQuery<string>(UserQuery).ToList();

        //    if (users.Count == 0)
        //        throw new Exception("Invalid user");
        //    else
        //        Superuser = users[0];//for future use. if the user is super user, different query will be used

        //    VisitQuery = $@"SELECT * FROM (
        //        (SELECT
        //        DM.DEALER_CODE AS CODE,
        //        PT.PARTY_TYPE_EDESC AS NAME,
        //        PT.ACC_CODE,
        //        DM.CONTACT_NO AS P_CONTACT_NO, DM.REG_OFFICE_ADDRESS AS ADDRESS,
        //        RM.ROUTE_CODE, TO_CHAR(RD.ASSIGN_DATE,'DD-MON-RRRR') AS ASSIGN_DATE, RM.ROUTE_NAME,
        //        AM.AREA_CODE, AM.AREA_NAME,
        //        TO_CHAR(RE.ORDER_NO) AS ORDER_NO,
        //        RM.COMPANY_CODE,
        //        'dealer' AS TYPE,
        //        'N' AS WHOLESELLER,
        //        '' DEFAULT_PARTY_TYPE_CODE,
        //        '' PARENT_DISTRIBUTOR_CODE,
        //        '' PARENT_DISTRIBUTOR_NAME,
        //        NVL(DM.LATITUDE,0) LATITUDE, NVL(DM.LONGITUDE,0) LONGITUDE,
        //        LT.LAST_VISIT_DATE, LT.LAST_VISIT_BY, LT.LAST_VISIT_STATUS, NVL(LT.IS_VISITED, 'X') AS IS_VISITED, LT.REMARKS    ,RD.emp_code    
        //        FROM DIST_ROUTE_MASTER RM
        //        INNER JOIN DIST_ROUTE_DETAIL RD ON RD.ROUTE_CODE = RM.ROUTE_CODE AND RD.COMPANY_CODE = RM.COMPANY_CODE
        //        INNER JOIN DIST_ROUTE_ENTITY RE ON RE.ROUTE_CODE = RD.ROUTE_CODE AND RE.ENTITY_TYPE = 'P' AND RE.COMPANY_CODE = RM.COMPANY_CODE
        //        INNER JOIN DIST_DEALER_MASTER DM ON DM.DEALER_CODE = RE.ENTITY_CODE AND DM.COMPANY_CODE = RM.COMPANY_CODE
        //        INNER JOIN IP_PARTY_TYPE_CODE PT ON PT.PARTY_TYPE_CODE = DM.DEALER_CODE AND PT.COMPANY_CODE = DM.COMPANY_CODE
        //        INNER JOIN DIST_AREA_MASTER AM ON AM.AREA_CODE = DM.AREA_CODE AND AM.COMPANY_CODE = DM.COMPANY_CODE
        //        LEFT JOIN (SELECT A.SP_CODE, A.CUSTOMER_CODE, A.CUSTOMER_TYPE, A.COMPANY_CODE, C.EMPLOYEE_EDESC AS LAST_VISIT_BY, A.IS_VISITED AS LAST_VISIT_STATUS, 
        //                  (CASE 
        //                    WHEN A.IS_VISITED IS NULL THEN 'X' 
        //                      ELSE
        //                        CASE WHEN TO_CHAR(SYSDATE, 'DD-MON-RRRR') = TO_CHAR(A.UPDATE_DATE, 'DD-MON-RRRR') THEN A.IS_VISITED ELSE 'X' END
        //                   END
        //                  ) IS_VISITED, 
        //                  A.REMARKS,
        //                  TO_CHAR(MAX(A.UPDATE_DATE), 'RRRR-MM-DD HH24:MI A.M.') LAST_VISIT_DATE
        //                  FROM DIST_LOCATION_TRACK A
        //                  INNER JOIN IP_PARTY_TYPE_CODE B ON B.PARTY_TYPE_CODE = A.CUSTOMER_CODE AND B.COMPANY_CODE = A.COMPANY_CODE
        //                  INNER JOIN HR_EMPLOYEE_SETUP C ON C.EMPLOYEE_CODE = A.SP_CODE AND C.COMPANY_CODE = A.COMPANY_CODE
        //                  WHERE 1 = 1
        //                  -- AND SP_CODE = '1000097' -- Commented out because customer can be visited by another SP_CODE
        //                  AND A.CUSTOMER_TYPE = 'P'
        //                  AND A.UPDATE_DATE = (SELECT MAX(UPDATE_DATE) FROM DIST_LOCATION_TRACK WHERE CUSTOMER_CODE = A.CUSTOMER_CODE AND CUSTOMER_TYPE = A.CUSTOMER_TYPE)
        //                  GROUP BY A.SP_CODE, A.CUSTOMER_CODE, A.CUSTOMER_TYPE, A.COMPANY_CODE, C.EMPLOYEE_EDESC, A.IS_VISITED, 
        //                      (CASE 
        //                        WHEN A.IS_VISITED IS NULL THEN 'X' 
        //                          ELSE
        //                            CASE WHEN TO_CHAR(SYSDATE, 'DD-MON-RRRR') = TO_CHAR(A.UPDATE_DATE, 'DD-MON-RRRR') THEN A.IS_VISITED ELSE 'X' END
        //                       END
        //                      ), 
        //                      A.REMARKS
        //                  ) LT
        //                  ON LT.CUSTOMER_CODE = DM.DEALER_CODE AND LT.COMPANY_CODE = RM.COMPANY_CODE
        //        WHERE 1 = 1
        //        AND TO_DATE(RD.ASSIGN_DATE, 'DD-MON-RRRR') > TO_DATE('{today}', 'DD-MON-RRRR')
        //        --AND RD.EMP_CODE = '{model.spcode}'
        //        AND RD.COMPANY_CODE = '{model.COMPANY_CODE}'
        //        )
        //      UNION
        //        (SELECT
        //        DM.DISTRIBUTOR_CODE AS CODE,
        //        CS.CUSTOMER_EDESC AS NAME,
        //        '' AS ACC_CODE,
        //        DM.CONTACT_NO AS P_CONTACT_NO, NVL(DM.REG_OFFICE_ADDRESS, CS.REGD_OFFICE_EADDRESS) AS ADDRESS,
        //        RM.ROUTE_CODE, TO_CHAR(RD.ASSIGN_DATE,'DD-MON-RRRR') AS ASSIGN_DATE, RM.ROUTE_NAME,
        //        AM.AREA_CODE, AM.AREA_NAME,
        //        TO_CHAR(RE.ORDER_NO) AS ORDER_NO,
        //        RM.COMPANY_CODE,
        //        'distributor' AS TYPE,
        //        'N' AS WHOLESELLER,
        //        CS.PARTY_TYPE_CODE AS DEFAULT_PARTY_TYPE_CODE,
        //        '' PARENT_DISTRIBUTOR_CODE,
        //        '' PARENT_DISTRIBUTOR_NAME,
        //        NVL(DM.LATITUDE,0) LATITUDE, NVL(DM.LONGITUDE,0) LONGITUDE,
        //        LT.LAST_VISIT_DATE, LT.LAST_VISIT_BY, LT.LAST_VISIT_STATUS, NVL(LT.IS_VISITED, 'X') AS IS_VISITED, LT.REMARKS    ,RD.emp_code    
        //        FROM DIST_ROUTE_MASTER RM
        //        INNER JOIN DIST_ROUTE_DETAIL RD ON RD.ROUTE_CODE = RM.ROUTE_CODE AND RD.COMPANY_CODE = RM.COMPANY_CODE
        //        INNER JOIN DIST_ROUTE_ENTITY RE ON RE.ROUTE_CODE = RD.ROUTE_CODE AND RE.ENTITY_TYPE = 'D' AND RE.COMPANY_CODE = RM.COMPANY_CODE
        //        INNER JOIN DIST_DISTRIBUTOR_MASTER DM ON DM.DISTRIBUTOR_CODE = RE.ENTITY_CODE AND DM.COMPANY_CODE = RM.COMPANY_CODE
        //        INNER JOIN SA_CUSTOMER_SETUP CS ON CS.CUSTOMER_CODE = DM.DISTRIBUTOR_CODE AND CS.COMPANY_CODE = DM.COMPANY_CODE
        //        INNER JOIN DIST_AREA_MASTER AM ON AM.AREA_CODE = DM.AREA_CODE AND AM.COMPANY_CODE = DM.COMPANY_CODE
        //        LEFT JOIN (SELECT A.SP_CODE, A.CUSTOMER_CODE, A.CUSTOMER_TYPE, A.COMPANY_CODE, C.EMPLOYEE_EDESC AS LAST_VISIT_BY, A.IS_VISITED AS LAST_VISIT_STATUS, 
        //                  (CASE 
        //                    WHEN A.IS_VISITED IS NULL THEN 'X' 
        //                      ELSE
        //                        CASE WHEN TO_CHAR(SYSDATE, 'DD-MON-RRRR') = TO_CHAR(A.UPDATE_DATE, 'DD-MON-RRRR') THEN A.IS_VISITED ELSE 'X' END
        //                   END
        //                  ) IS_VISITED, 
        //                  A.REMARKS,
        //                  TO_CHAR(MAX(A.UPDATE_DATE), 'RRRR-MM-DD HH24:MI A.M.') LAST_VISIT_DATE
        //                  FROM DIST_LOCATION_TRACK A
        //                  INNER JOIN SA_CUSTOMER_SETUP B ON B.CUSTOMER_CODE = A.CUSTOMER_CODE AND B.COMPANY_CODE = A.COMPANY_CODE
        //                  INNER JOIN HR_EMPLOYEE_SETUP C ON C.EMPLOYEE_CODE = A.SP_CODE AND C.COMPANY_CODE = A.COMPANY_CODE
        //                  WHERE 1 = 1
        //                  -- AND SP_CODE = '1000097' -- Commented out because customer can be visited by another SP_CODE
        //                  AND A.CUSTOMER_TYPE = 'D'
        //                  AND A.UPDATE_DATE = (SELECT MAX(UPDATE_DATE) FROM DIST_LOCATION_TRACK WHERE CUSTOMER_CODE = A.CUSTOMER_CODE AND CUSTOMER_TYPE = A.CUSTOMER_TYPE)
        //                  GROUP BY A.SP_CODE, A.CUSTOMER_CODE, A.CUSTOMER_TYPE, A.COMPANY_CODE, C.EMPLOYEE_EDESC, A.IS_VISITED, 
        //                      (CASE 
        //                        WHEN A.IS_VISITED IS NULL THEN 'X' 
        //                          ELSE
        //                            CASE WHEN TO_CHAR(SYSDATE, 'DD-MON-RRRR') = TO_CHAR(A.UPDATE_DATE, 'DD-MON-RRRR') THEN A.IS_VISITED ELSE 'X' END
        //                       END
        //                      ), 
        //                      A.REMARKS
        //                  ) LT
        //                  ON LT.CUSTOMER_CODE = DM.DISTRIBUTOR_CODE AND LT.COMPANY_CODE = RM.COMPANY_CODE
        //        WHERE 1 = 1
        //        AND TO_DATE(RD.ASSIGN_DATE, 'DD-MON-RRRR') > TO_DATE('{today}', 'DD-MON-RRRR')
        //      --  AND RD.EMP_CODE = '{model.spcode}'
        //        AND RD.COMPANY_CODE = '{model.COMPANY_CODE}'
        //        )
        //    UNION
        //        (SELECT
        //        RES.RESELLER_CODE AS CODE,
        //        RES.RESELLER_NAME AS NAME,
        //        '' AS ACC_CODE,
        //        RES.CONTACT_NO AS P_CONTACT_NO, RES.REG_OFFICE_ADDRESS AS ADDRESS,
        //        RM.ROUTE_CODE, TO_CHAR(RD.ASSIGN_DATE,'DD-MON-RRRR') AS ASSIGN_DATE, RM.ROUTE_NAME,
        //        AM.AREA_CODE, AM.AREA_NAME,
        //        TO_CHAR(RE.ORDER_NO) AS ORDER_NO,
        //        RM.COMPANY_CODE,
        //        'reseller' AS TYPE,
        //        RES.WHOLESELLER AS WHOLESELLER,
        //        '' AS DEFAULT_PARTY_TYPE_CODE,
        //        DISTRIBUTOR_CODE AS PARENT_DISTRIBUTOR_CODE,
        //        CS.CUSTOMER_EDESC AS PARENT_DISTRIBUTOR_NAME,
        //        NVL(RES.LATITUDE,0) LATITUDE, NVL(RES.LONGITUDE,0) LONGITUDE,
        //        LT.LAST_VISIT_DATE, LT.LAST_VISIT_BY, LT.LAST_VISIT_STATUS, NVL(LT.IS_VISITED, 'X') AS IS_VISITED, LT.REMARKS    ,RD.emp_code                   
        //        FROM DIST_ROUTE_MASTER RM
        //        INNER JOIN DIST_ROUTE_DETAIL RD ON RD.ROUTE_CODE = RM.ROUTE_CODE AND RD.COMPANY_CODE = RM.COMPANY_CODE
        //        INNER JOIN DIST_ROUTE_ENTITY RE ON RE.ROUTE_CODE = RD.ROUTE_CODE AND RE.ENTITY_TYPE = 'R' AND RE.COMPANY_CODE = RM.COMPANY_CODE
        //        INNER JOIN DIST_RESELLER_MASTER RES ON RES.RESELLER_CODE = RE.ENTITY_CODE AND RES.COMPANY_CODE = RM.COMPANY_CODE
        //        LEFT JOIN DIST_AREA_MASTER AM ON AM.AREA_CODE = RES.AREA_CODE AND AM.COMPANY_CODE = RM.COMPANY_CODE
        //        LEFT JOIN SA_CUSTOMER_SETUP CS ON CS.CUSTOMER_CODE = RES.DISTRIBUTOR_CODE AND CS.COMPANY_CODE = RM.COMPANY_CODE
        //        LEFT JOIN (SELECT A.SP_CODE, A.CUSTOMER_CODE, A.CUSTOMER_TYPE, A.COMPANY_CODE, C.EMPLOYEE_EDESC AS LAST_VISIT_BY, A.IS_VISITED AS LAST_VISIT_STATUS, 
        //                  (CASE 
        //                    WHEN A.IS_VISITED IS NULL THEN 'X' 
        //                      ELSE
        //                        CASE WHEN TO_CHAR(SYSDATE, 'DD-MON-RRRR') = TO_CHAR(A.UPDATE_DATE, 'DD-MON-RRRR') THEN A.IS_VISITED ELSE 'X' END
        //                   END
        //                  ) IS_VISITED, 
        //                  A.REMARKS,
        //                  TO_CHAR(MAX(A.UPDATE_DATE), 'RRRR-MM-DD HH24:MI A.M.') LAST_VISIT_DATE
        //                  FROM DIST_LOCATION_TRACK A
        //                  INNER JOIN DIST_RESELLER_MASTER B ON B.RESELLER_CODE = A.CUSTOMER_CODE AND B.COMPANY_CODE = A.COMPANY_CODE
        //                  INNER JOIN HR_EMPLOYEE_SETUP C ON C.EMPLOYEE_CODE = A.SP_CODE AND C.COMPANY_CODE = A.COMPANY_CODE
        //                  WHERE 1 = 1
        //                  AND A.CUSTOMER_TYPE = 'R'
        //                  AND A.UPDATE_DATE = (SELECT MAX(UPDATE_DATE) FROM DIST_LOCATION_TRACK WHERE CUSTOMER_CODE = A.CUSTOMER_CODE AND CUSTOMER_TYPE = A.CUSTOMER_TYPE)
        //                  GROUP BY A.SP_CODE, A.CUSTOMER_CODE, A.CUSTOMER_TYPE, A.COMPANY_CODE, C.EMPLOYEE_EDESC, A.IS_VISITED, 
        //                      (CASE 
        //                        WHEN A.IS_VISITED IS NULL THEN 'X' 
        //                          ELSE
        //                            CASE WHEN TO_CHAR(SYSDATE, 'DD-MON-RRRR') = TO_CHAR(A.UPDATE_DATE, 'DD-MON-RRRR') THEN A.IS_VISITED ELSE 'X' END
        //                       END
        //                      ), 
        //                      A.REMARKS
        //                  ) LT
        //                  ON LT.CUSTOMER_CODE = RES.RESELLER_CODE AND LT.COMPANY_CODE = RM.COMPANY_CODE
        //        WHERE 1 = 1
        //        AND TO_DATE(RD.ASSIGN_DATE, 'DD-MON-RRRR') > TO_DATE('{today}', 'DD-MON-RRRR')
        //        AND RD.COMPANY_CODE = '{model.COMPANY_CODE}'
        //        )
        //    ) where emp_code in( SELECT  distinct
        //            sp_code
        //                FROM DIST_LOGIN_USER
        //                WHERE user_type = 'S' 
        //                CONNECT BY PRIOR userid = Parent_userid 
        //                start with sp_code ='{model.spcode}')
        //    ORDER BY UPPER(ROUTE_NAME), ORDER_NO, UPPER(AREA_NAME), UPPER(NAME), LAST_VISIT_DATE DESC";

        //    var Visitlist = dbContext.SqlQuery<VisitEntityModel>(VisitQuery).ToList();
        //    if (Visitlist.Count <= 0)
        //        throw new Exception("No records found");

        //    var RouteCode = Visitlist[0].Route_Code;
        //    var RouteName = Visitlist[0].Route_Name;
        //    foreach (var visit in Visitlist)
        //    {
        //        var code = visit.Code;
        //        VisitPlans.entity.Add(code, visit);
        //    }
        //    VisitPlans.code = RouteCode;
        //    Result.Add(RouteName, VisitPlans);

        //    return Result;
        //}
        public List<VisitEntityModel> GetVisitPlan(VisitPlanRequestModel model, NeoErpCoreEntity dbContext, PreferenceModel pref)
        {
            string Superuser;
            int prevDays = pref.SQL_PEV_DAYS == null ? 5 : (int)pref.SQL_PEV_DAYS.Value;
            int sucDays = pref.SQL_FOL_DAYS == null ? 5 : (int)pref.SQL_FOL_DAYS.Value;
            string StartDate = DateTime.Now.AddDays(-prevDays).ToString("dd-MMM-yyyy");
            string EndDate = DateTime.Now.AddDays(sucDays).ToString("dd-MMM-yyyy");

            if (string.IsNullOrEmpty(model.spcode as string))
                throw new Exception("User code is empty.");
            string VisitQuery = string.Empty;
            string UserQuery = $@"select super_user from dist_login_user where sp_code = '{model.spcode}'";
            var users = dbContext.SqlQuery<string>(UserQuery).ToList();

            if (users.Count == 0)
                throw new Exception("Invalid user");
            else
                Superuser = users[0];//for future use. if the user is super user, different query will be used

            VisitQuery = $@"SELECT * FROM (
                (SELECT
                ES.EMPLOYEE_CODE, TRIM(ES.EMPLOYEE_EDESC) EMPLOYEE_EDESC,
                DM.DEALER_CODE AS CODE,DM.EMAIL,
                PT.PARTY_TYPE_EDESC AS NAME,
                PT.ACC_CODE,
                DM.CONTACT_NO AS P_CONTACT_NO, DM.REG_OFFICE_ADDRESS AS ADDRESS,
                RM.ROUTE_CODE, TO_CHAR(RD.ASSIGN_DATE,'DD-MON-RRRR') AS ASSIGN_DATE, RM.ROUTE_NAME,
                AM.AREA_CODE, AM.AREA_NAME,
                TO_CHAR(RE.ORDER_NO) AS ORDER_NO,
                RM.COMPANY_CODE,
                'dealer' AS TYPE,
                'N' AS WHOLESELLER,
                '' DEFAULT_PARTY_TYPE_CODE,
                '' PARENT_DISTRIBUTOR_CODE,
                '' PARENT_DISTRIBUTOR_NAME,
                NVL(DM.LATITUDE,0) LATITUDE, NVL(DM.LONGITUDE,0) LONGITUDE,
                LT.LAST_VISIT_DATE, LT.LAST_VISIT_BY, LT.LAST_VISIT_STATUS, NVL(LT.IS_VISITED, 'X') AS IS_VISITED, LT.REMARKS    ,RD.emp_code    
                FROM DIST_ROUTE_MASTER RM
                INNER JOIN DIST_ROUTE_DETAIL RD ON RD.ROUTE_CODE = RM.ROUTE_CODE AND RD.COMPANY_CODE = RM.COMPANY_CODE
                INNER JOIN HR_EMPLOYEE_SETUP ES ON ES.EMPLOYEE_CODE = RD.EMP_CODE AND ES.COMPANY_CODE = RD.COMPANY_CODE
                INNER JOIN DIST_ROUTE_ENTITY RE ON RE.ROUTE_CODE = RD.ROUTE_CODE AND RE.ENTITY_TYPE = 'P' AND RE.COMPANY_CODE = RM.COMPANY_CODE
                INNER JOIN DIST_DEALER_MASTER DM ON DM.DEALER_CODE = RE.ENTITY_CODE AND DM.COMPANY_CODE = RM.COMPANY_CODE
                INNER JOIN IP_PARTY_TYPE_CODE PT ON PT.PARTY_TYPE_CODE = DM.DEALER_CODE AND PT.COMPANY_CODE = DM.COMPANY_CODE
                INNER JOIN DIST_AREA_MASTER AM ON AM.AREA_CODE = DM.AREA_CODE AND AM.COMPANY_CODE = DM.COMPANY_CODE
                LEFT JOIN (SELECT A.SP_CODE, A.CUSTOMER_CODE, A.CUSTOMER_TYPE, A.COMPANY_CODE, C.EMPLOYEE_EDESC AS LAST_VISIT_BY, A.IS_VISITED AS LAST_VISIT_STATUS, 
                          (CASE 
                            WHEN A.IS_VISITED IS NULL THEN 'X' 
                              ELSE
                                CASE WHEN TO_CHAR(SYSDATE, 'DD-MON-RRRR') = TO_CHAR(A.UPDATE_DATE, 'DD-MON-RRRR') THEN A.IS_VISITED ELSE 'X' END
                           END
                          ) IS_VISITED, 
                          A.REMARKS,
                          TO_CHAR(MAX(A.UPDATE_DATE), 'RRRR-MM-DD HH24:MI A.M.') LAST_VISIT_DATE
                          FROM DIST_LOCATION_TRACK A
                          INNER JOIN IP_PARTY_TYPE_CODE B ON B.PARTY_TYPE_CODE = A.CUSTOMER_CODE AND B.COMPANY_CODE = A.COMPANY_CODE
                          INNER JOIN HR_EMPLOYEE_SETUP C ON C.EMPLOYEE_CODE = A.SP_CODE AND C.COMPANY_CODE = A.COMPANY_CODE
                          WHERE 1 = 1
                          -- AND SP_CODE = '1000097' -- Commented out because customer can be visited by another SP_CODE
                          AND A.CUSTOMER_TYPE = 'P'
                          AND A.UPDATE_DATE = (SELECT MAX(UPDATE_DATE) FROM DIST_LOCATION_TRACK WHERE CUSTOMER_CODE = A.CUSTOMER_CODE AND CUSTOMER_TYPE = A.CUSTOMER_TYPE)
                          GROUP BY A.SP_CODE, A.CUSTOMER_CODE, A.CUSTOMER_TYPE, A.COMPANY_CODE, C.EMPLOYEE_EDESC, A.IS_VISITED, 
                              (CASE 
                                WHEN A.IS_VISITED IS NULL THEN 'X' 
                                  ELSE
                                    CASE WHEN TO_CHAR(SYSDATE, 'DD-MON-RRRR') = TO_CHAR(A.UPDATE_DATE, 'DD-MON-RRRR') THEN A.IS_VISITED ELSE 'X' END
                               END
                              ), 
                              A.REMARKS
                          ) LT
                          ON LT.CUSTOMER_CODE = DM.DEALER_CODE AND LT.COMPANY_CODE = RM.COMPANY_CODE
                WHERE 1 = 1
                AND RM.ROUTE_TYPE = 'D'
                --AND RD.EMP_CODE = '{model.spcode}'
                AND RD.COMPANY_CODE = '{model.COMPANY_CODE}'
                )
              UNION
                (SELECT
                ES.EMPLOYEE_CODE, TRIM(ES.EMPLOYEE_EDESC) EMPLOYEE_EDESC,
                DM.DISTRIBUTOR_CODE AS CODE,DM.EMAIL,
                CS.CUSTOMER_EDESC AS NAME,
                '' AS ACC_CODE,
                DM.CONTACT_NO AS P_CONTACT_NO, NVL(DM.REG_OFFICE_ADDRESS, CS.REGD_OFFICE_EADDRESS) AS ADDRESS,
                RM.ROUTE_CODE, TO_CHAR(RD.ASSIGN_DATE,'DD-MON-RRRR') AS ASSIGN_DATE, RM.ROUTE_NAME,
                AM.AREA_CODE, AM.AREA_NAME,
                TO_CHAR(RE.ORDER_NO) AS ORDER_NO,
                RM.COMPANY_CODE,
                'distributor' AS TYPE,
                'N' AS WHOLESELLER,
                CS.PARTY_TYPE_CODE AS DEFAULT_PARTY_TYPE_CODE,
                '' PARENT_DISTRIBUTOR_CODE,
                '' PARENT_DISTRIBUTOR_NAME,
                NVL(DM.LATITUDE,0) LATITUDE, NVL(DM.LONGITUDE,0) LONGITUDE,
                LT.LAST_VISIT_DATE, LT.LAST_VISIT_BY, LT.LAST_VISIT_STATUS, NVL(LT.IS_VISITED, 'X') AS IS_VISITED, LT.REMARKS    ,RD.emp_code    
                FROM DIST_ROUTE_MASTER RM
                INNER JOIN DIST_ROUTE_DETAIL RD ON RD.ROUTE_CODE = RM.ROUTE_CODE AND RD.COMPANY_CODE = RM.COMPANY_CODE
                INNER JOIN HR_EMPLOYEE_SETUP ES ON ES.EMPLOYEE_CODE = RD.EMP_CODE AND ES.COMPANY_CODE = RD.COMPANY_CODE
                INNER JOIN DIST_ROUTE_ENTITY RE ON RE.ROUTE_CODE = RD.ROUTE_CODE AND RE.ENTITY_TYPE = 'D' AND RE.COMPANY_CODE = RM.COMPANY_CODE
                INNER JOIN DIST_DISTRIBUTOR_MASTER DM ON DM.DISTRIBUTOR_CODE = RE.ENTITY_CODE AND DM.COMPANY_CODE = RM.COMPANY_CODE
                INNER JOIN SA_CUSTOMER_SETUP CS ON CS.CUSTOMER_CODE = DM.DISTRIBUTOR_CODE AND CS.COMPANY_CODE = DM.COMPANY_CODE
                INNER JOIN DIST_AREA_MASTER AM ON AM.AREA_CODE = DM.AREA_CODE AND AM.COMPANY_CODE = DM.COMPANY_CODE
                LEFT JOIN (SELECT A.SP_CODE, A.CUSTOMER_CODE, A.CUSTOMER_TYPE, A.COMPANY_CODE, C.EMPLOYEE_EDESC AS LAST_VISIT_BY, A.IS_VISITED AS LAST_VISIT_STATUS, 
                          (CASE 
                            WHEN A.IS_VISITED IS NULL THEN 'X' 
                              ELSE
                                CASE WHEN TO_CHAR(SYSDATE, 'DD-MON-RRRR') = TO_CHAR(A.UPDATE_DATE, 'DD-MON-RRRR') THEN A.IS_VISITED ELSE 'X' END
                           END
                          ) IS_VISITED, 
                          A.REMARKS,
                          TO_CHAR(MAX(A.UPDATE_DATE), 'RRRR-MM-DD HH24:MI A.M.') LAST_VISIT_DATE
                          FROM DIST_LOCATION_TRACK A
                          INNER JOIN SA_CUSTOMER_SETUP B ON B.CUSTOMER_CODE = A.CUSTOMER_CODE AND B.COMPANY_CODE = A.COMPANY_CODE
                          INNER JOIN HR_EMPLOYEE_SETUP C ON C.EMPLOYEE_CODE = A.SP_CODE AND C.COMPANY_CODE = A.COMPANY_CODE
                          WHERE 1 = 1
                          -- AND SP_CODE = '1000097' -- Commented out because customer can be visited by another SP_CODE
                          AND A.CUSTOMER_TYPE = 'D'
                          AND A.UPDATE_DATE = (SELECT MAX(UPDATE_DATE) FROM DIST_LOCATION_TRACK WHERE CUSTOMER_CODE = A.CUSTOMER_CODE AND CUSTOMER_TYPE = A.CUSTOMER_TYPE)
                          GROUP BY A.SP_CODE, A.CUSTOMER_CODE, A.CUSTOMER_TYPE, A.COMPANY_CODE, C.EMPLOYEE_EDESC, A.IS_VISITED, 
                              (CASE 
                                WHEN A.IS_VISITED IS NULL THEN 'X' 
                                  ELSE
                                    CASE WHEN TO_CHAR(SYSDATE, 'DD-MON-RRRR') = TO_CHAR(A.UPDATE_DATE, 'DD-MON-RRRR') THEN A.IS_VISITED ELSE 'X' END
                               END
                              ),
                              A.REMARKS
                          ) LT
                          ON LT.CUSTOMER_CODE = DM.DISTRIBUTOR_CODE AND LT.COMPANY_CODE = RM.COMPANY_CODE
                WHERE 1 = 1
                AND RM.ROUTE_TYPE = 'D'
                AND DM.ACTIVE = 'Y' AND DM.DELETED_FLAG = 'N'
                AND TO_DATE(RD.ASSIGN_DATE, 'DD-MON-RRRR') BETWEEN TO_DATE('{StartDate}', 'DD-MON-RRRR') AND TO_DATE('{EndDate}', 'DD-MON-RRRR')
              --  AND RD.EMP_CODE = '{model.spcode}'
                AND RD.COMPANY_CODE = '{model.COMPANY_CODE}'
                )
            UNION
                (SELECT
                ES.EMPLOYEE_CODE, TRIM(ES.EMPLOYEE_EDESC) EMPLOYEE_EDESC,
                RES.RESELLER_CODE AS CODE,RES.EMAIL,
                RES.RESELLER_NAME AS NAME,
                '' AS ACC_CODE,
                RES.CONTACT_NO AS P_CONTACT_NO, RES.REG_OFFICE_ADDRESS AS ADDRESS,
                RM.ROUTE_CODE, TO_CHAR(RD.ASSIGN_DATE,'DD-MON-RRRR') AS ASSIGN_DATE, RM.ROUTE_NAME,
                AM.AREA_CODE, AM.AREA_NAME,
                TO_CHAR(RE.ORDER_NO) AS ORDER_NO,
                RM.COMPANY_CODE,
                'reseller' AS TYPE,
                RES.WHOLESELLER AS WHOLESELLER,
                '' AS DEFAULT_PARTY_TYPE_CODE,
                DISTRIBUTOR_CODE AS PARENT_DISTRIBUTOR_CODE,
                CS.CUSTOMER_EDESC AS PARENT_DISTRIBUTOR_NAME,
                NVL(RES.LATITUDE,0) LATITUDE, NVL(RES.LONGITUDE,0) LONGITUDE,
                LT.LAST_VISIT_DATE, LT.LAST_VISIT_BY, LT.LAST_VISIT_STATUS, NVL(LT.IS_VISITED, 'X') AS IS_VISITED, LT.REMARKS    ,RD.emp_code                   
                FROM DIST_ROUTE_MASTER RM
                INNER JOIN DIST_ROUTE_DETAIL RD ON RD.ROUTE_CODE = RM.ROUTE_CODE AND RD.COMPANY_CODE = RM.COMPANY_CODE
                INNER JOIN HR_EMPLOYEE_SETUP ES ON ES.EMPLOYEE_CODE = RD.EMP_CODE AND ES.COMPANY_CODE = RD.COMPANY_CODE
                INNER JOIN DIST_ROUTE_ENTITY RE ON RE.ROUTE_CODE = RD.ROUTE_CODE AND RE.ENTITY_TYPE = 'R' AND RE.COMPANY_CODE = RM.COMPANY_CODE
                INNER JOIN DIST_RESELLER_MASTER RES ON RES.RESELLER_CODE = RE.ENTITY_CODE AND RES.COMPANY_CODE = RM.COMPANY_CODE
                LEFT JOIN DIST_AREA_MASTER AM ON AM.AREA_CODE = RES.AREA_CODE AND AM.COMPANY_CODE = RM.COMPANY_CODE
                LEFT JOIN SA_CUSTOMER_SETUP CS ON CS.CUSTOMER_CODE = RES.DISTRIBUTOR_CODE AND CS.COMPANY_CODE = RM.COMPANY_CODE
                LEFT JOIN (SELECT A.SP_CODE, A.CUSTOMER_CODE, A.CUSTOMER_TYPE, A.COMPANY_CODE, C.EMPLOYEE_EDESC AS LAST_VISIT_BY, A.IS_VISITED AS LAST_VISIT_STATUS, 
                          (CASE 
                            WHEN A.IS_VISITED IS NULL THEN 'X' 
                              ELSE
                                CASE WHEN TO_CHAR(SYSDATE, 'DD-MON-RRRR') = TO_CHAR(A.UPDATE_DATE, 'DD-MON-RRRR') THEN A.IS_VISITED ELSE 'X' END
                           END
                          ) IS_VISITED, 
                          A.REMARKS,
                          TO_CHAR(MAX(A.UPDATE_DATE), 'RRRR-MM-DD HH24:MI A.M.') LAST_VISIT_DATE
                          FROM DIST_LOCATION_TRACK A
                          INNER JOIN DIST_RESELLER_MASTER B ON B.RESELLER_CODE = A.CUSTOMER_CODE AND B.COMPANY_CODE = A.COMPANY_CODE
                          INNER JOIN HR_EMPLOYEE_SETUP C ON C.EMPLOYEE_CODE = A.SP_CODE AND C.COMPANY_CODE = A.COMPANY_CODE
                          WHERE 1 = 1
                          AND B.IS_CLOSED = 'N'
                          AND A.CUSTOMER_TYPE = 'R'
                          AND A.UPDATE_DATE = (SELECT MAX(UPDATE_DATE) FROM DIST_LOCATION_TRACK WHERE CUSTOMER_CODE = A.CUSTOMER_CODE AND CUSTOMER_TYPE = A.CUSTOMER_TYPE)
                          GROUP BY A.SP_CODE, A.CUSTOMER_CODE, A.CUSTOMER_TYPE, A.COMPANY_CODE, C.EMPLOYEE_EDESC, A.IS_VISITED, 
                              (CASE 
                                WHEN A.IS_VISITED IS NULL THEN 'X' 
                                  ELSE
                                    CASE WHEN TO_CHAR(SYSDATE, 'DD-MON-RRRR') = TO_CHAR(A.UPDATE_DATE, 'DD-MON-RRRR') THEN A.IS_VISITED ELSE 'X' END
                               END
                              ), 
                              A.REMARKS
                          ) LT
                          ON LT.CUSTOMER_CODE = RES.RESELLER_CODE AND LT.COMPANY_CODE = RM.COMPANY_CODE
                WHERE 1 = 1
                AND RM.ROUTE_TYPE = 'D'  AND RES.IS_CLOSED = 'N'
                AND TO_DATE(RD.ASSIGN_DATE, 'DD-MON-RRRR') BETWEEN TO_DATE('{StartDate}', 'DD-MON-RRRR') AND TO_DATE('{EndDate}', 'DD-MON-RRRR')
                AND RD.COMPANY_CODE = '{model.COMPANY_CODE}'
                )
                UNION
                (
                SELECT RD.EMP_CODE AS EMPLOYEE_CODE,ES.EMPLOYEE_EDESC,'' CODE,'' EMAIL, '' NAME, '' ACC_CODE,''P_CONTACT_NO,'' ADDRESS,RM.ROUTE_CODE,TO_CHAR(RD.ASSIGN_DATE,'DD-MON-RRRR') ASSIGN_DATE,RM.ROUTE_NAME,
                             '' AREA_CODE, '' AREA_NAME,TO_CHAR(RE.ORDER_NO) ORDER_NO,RM.COMPANY_CODE,'' TYPE, 'N' AS WHOLESELLER, '' DEFAULT_PARTY_TYPE_CODE, '' PARENT_DISTRIBUTOR_CODE,
                             '' PARENT_DISTRIBUTOR_NAME, '' LATITUDE, '' LONGITUDE, '' LAST_VISIT_DATE, '' LAST_VISIT_BY, '' LAST_VISIT_STATUS, '' IS_VISITED,'' REMARKS,RD.EMP_CODE
                FROM DIST_ROUTE_MASTER RM
                JOIN DIST_ROUTE_DETAIL RD ON RD.ROUTE_CODE = RM.ROUTE_CODE AND RD.COMPANY_CODE = RM.COMPANY_CODE
                LEFT JOIN DIST_ROUTE_ENTITY RE ON RE.ROUTE_CODE = RM.ROUTE_CODE AND RE.COMPANY_CODE = RM.COMPANY_CODE
                INNER JOIN  HR_EMPLOYEE_SETUP ES ON ES.EMPLOYEE_CODE = RD.EMP_CODE AND ES.COMPANY_CODE = RD.COMPANY_CODE
                WHERE RE.ENTITY_CODE IS NULL
                )
            ) WHERE TO_DATE(ASSIGN_DATE, 'DD-MON-RRRR') BETWEEN TO_DATE('{StartDate}', 'DD-MON-RRRR') AND TO_DATE('{EndDate}', 'DD-MON-RRRR')
                AND EMP_CODE IN( SELECT  DISTINCT
                    SP_CODE
                        FROM DIST_LOGIN_USER
                        WHERE USER_TYPE = 'S' 
                        CONNECT BY PRIOR USERID = PARENT_USERID 
                        START WITH SP_CODE ='{model.spcode}')
            ORDER BY UPPER(ROUTE_NAME), ORDER_NO, UPPER(AREA_NAME), UPPER(NAME), LAST_VISIT_DATE DESC";

            var Visitlist = dbContext.SqlQuery<VisitEntityModel>(VisitQuery).ToList();
            if (Visitlist.Count <= 0)
                throw new Exception("No records found");

            return Visitlist;
        }

        public List<VisitBrdModel> GetBrdVisitPlan(VisitPlanRequestModel model, NeoErpCoreEntity dbContext, PreferenceModel pref)
        {
            int prevDays = pref.SQL_PEV_DAYS == null ? 5 : (int)pref.SQL_PEV_DAYS.Value;
            int sucDays = pref.SQL_FOL_DAYS == null ? 5 : (int)pref.SQL_FOL_DAYS.Value;
            string StartDate = DateTime.Now.AddDays(-prevDays).ToString("dd-MMM-yyyy");
            string EndDate = DateTime.Now.AddDays(sucDays).ToString("dd-MMM-yyyy");

            if (string.IsNullOrEmpty(model.spcode as string))
                throw new Exception("User code is empty.");
            string VisitQuery = string.Empty;
            string UserQuery = $@"select super_user from dist_login_user where sp_code = '{model.spcode}'";
            var users = dbContext.SqlQuery<string>(UserQuery).ToList();

            if (users.Count == 0)
                throw new Exception("Invalid user");

            string Query = $@"SELECT
                ES.EMPLOYEE_CODE, TRIM(ES.EMPLOYEE_EDESC) EMPLOYEE_EDESC, DM.CODE, DM.CONTACT_NO, RM.ROUTE_CODE, DM.DESCRIPTION ENTITY_EDESC,
                TO_CHAR(RD.ASSIGN_DATE,'DD-MON-RRRR') AS ASSIGN_DATE, RM.ROUTE_NAME, AM.AREA_CODE, AM.AREA_NAME,
                TO_CHAR(RE.ORDER_NO) AS ORDER_NO, RM.COMPANY_CODE, NVL(DM.LATITUDE,0) LATITUDE, NVL(DM.LONGITUDE,0) LONGITUDE
                FROM DIST_ROUTE_MASTER RM
                INNER JOIN  DIST_BRANDING_ROUTE_DETAIL RD ON RD.ROUTE_CODE = RM.ROUTE_CODE AND RD.COMPANY_CODE = RM.COMPANY_CODE
                INNER JOIN HR_EMPLOYEE_SETUP ES ON ES.EMPLOYEE_CODE = RD.EMP_CODE AND ES.COMPANY_CODE = RD.COMPANY_CODE
                INNER JOIN BRD_ROUTE_ENTITY RE ON RE.ROUTE_CODE = RD.ROUTE_CODE AND RE.COMPANY_CODE = RM.COMPANY_CODE
                INNER JOIN BRD_OTHER_ENTITY DM ON DM.CODE = RE.ENTITY_CODE AND DM.COMPANY_CODE = RM.COMPANY_CODE
                INNER JOIN DIST_AREA_MASTER AM ON AM.AREA_CODE = DM.AREA_CODE AND AM.COMPANY_CODE = DM.COMPANY_CODE
                WHERE TO_DATE(RD.ASSIGN_DATE, 'DD-MON-RRRR') BETWEEN TO_DATE('{StartDate}', 'DD-MON-RRRR') AND TO_DATE('{EndDate}', 'DD-MON-RRRR')
                AND RD.EMP_CODE = '{model.spcode}'
                AND RD.DELETED_FLAG = 'N'
                AND RM.ROUTE_TYPE = 'B'
                AND RD.COMPANY_CODE = '{model.COMPANY_CODE}'";
            var Visitlist = dbContext.SqlQuery<VisitBrdModel>(Query).ToList();
            if (Visitlist.Count <= 0)
                throw new Exception("No records found");
            return Visitlist;
        }

        public List<VisitEntityModel> GetBrandingVisitPlan(VisitPlanRequestModel model, NeoErpCoreEntity dbContext, PreferenceModel pref)
        {
            string Superuser;
            int prevDays = pref.SQL_PEV_DAYS == null ? 5 : (int)pref.SQL_PEV_DAYS.Value;
            int sucDays = pref.SQL_FOL_DAYS == null ? 5 : (int)pref.SQL_FOL_DAYS.Value;
            string StartDate = DateTime.Now.AddDays(-prevDays).ToString("dd-MMM-yyyy");
            string EndDate = DateTime.Now.AddDays(sucDays).ToString("dd-MMM-yyyy");

            if (string.IsNullOrEmpty(model.spcode as string))
                throw new Exception("User code is empty.");
            string VisitQuery = string.Empty;
            string UserQuery = $@"select super_user from dist_login_user where sp_code = '{model.spcode}'";
            var users = dbContext.SqlQuery<string>(UserQuery).ToList();

            if (users.Count == 0)
                throw new Exception("Invalid user");
            else
                Superuser = users[0];//for future use. if the user is super user, different query will be used

            VisitQuery = $@"SELECT * FROM (
                (SELECT
                ES.EMPLOYEE_CODE, TRIM(ES.EMPLOYEE_EDESC) EMPLOYEE_EDESC,
                DM.DEALER_CODE AS CODE,DM.EMAIL,
                PT.PARTY_TYPE_EDESC AS NAME,
                PT.ACC_CODE,
                DM.CONTACT_NO AS P_CONTACT_NO, DM.REG_OFFICE_ADDRESS AS ADDRESS,
                RM.ROUTE_CODE, TO_CHAR(RD.ASSIGN_DATE,'DD-MON-RRRR') AS ASSIGN_DATE, RM.ROUTE_NAME,
                AM.AREA_CODE, AM.AREA_NAME,
                TO_CHAR(RE.ORDER_NO) AS ORDER_NO,
                RM.COMPANY_CODE,
                'dealer' AS TYPE,
                'N' AS WHOLESELLER,
                '' DEFAULT_PARTY_TYPE_CODE,
                '' PARENT_DISTRIBUTOR_CODE,
                '' PARENT_DISTRIBUTOR_NAME,
                NVL(DM.LATITUDE,0) LATITUDE, NVL(DM.LONGITUDE,0) LONGITUDE,
                LT.LAST_VISIT_DATE, LT.LAST_VISIT_BY, LT.LAST_VISIT_STATUS, NVL(LT.IS_VISITED, 'X') AS IS_VISITED, LT.REMARKS    ,RD.emp_code    
                FROM DIST_ROUTE_MASTER RM
                INNER JOIN DIST_BRANDING_ROUTE_DETAIL RD ON RD.ROUTE_CODE = RM.ROUTE_CODE AND RD.COMPANY_CODE = RM.COMPANY_CODE
                INNER JOIN HR_EMPLOYEE_SETUP ES ON ES.EMPLOYEE_CODE = RD.EMP_CODE AND ES.COMPANY_CODE = RD.COMPANY_CODE
                INNER JOIN DIST_ROUTE_ENTITY RE ON RE.ROUTE_CODE = RD.ROUTE_CODE AND RE.ENTITY_TYPE = 'P' AND RE.COMPANY_CODE = RM.COMPANY_CODE
                INNER JOIN DIST_DEALER_MASTER DM ON DM.DEALER_CODE = RE.ENTITY_CODE AND DM.COMPANY_CODE = RM.COMPANY_CODE
                INNER JOIN IP_PARTY_TYPE_CODE PT ON PT.PARTY_TYPE_CODE = DM.DEALER_CODE AND PT.COMPANY_CODE = DM.COMPANY_CODE
                INNER JOIN DIST_AREA_MASTER AM ON AM.AREA_CODE = DM.AREA_CODE AND AM.COMPANY_CODE = DM.COMPANY_CODE
                LEFT JOIN (SELECT A.SP_CODE, A.CUSTOMER_CODE, A.CUSTOMER_TYPE, A.COMPANY_CODE, C.EMPLOYEE_EDESC AS LAST_VISIT_BY, A.IS_VISITED AS LAST_VISIT_STATUS, 
                          (CASE 
                            WHEN A.IS_VISITED IS NULL THEN 'X' 
                              ELSE
                                CASE WHEN TO_CHAR(SYSDATE, 'DD-MON-RRRR') = TO_CHAR(A.UPDATE_DATE, 'DD-MON-RRRR') THEN A.IS_VISITED ELSE 'X' END
                           END
                          ) IS_VISITED, 
                          A.REMARKS,
                          TO_CHAR(MAX(A.UPDATE_DATE), 'RRRR-MM-DD HH24:MI A.M.') LAST_VISIT_DATE
                          FROM DIST_LOCATION_TRACK A
                          INNER JOIN IP_PARTY_TYPE_CODE B ON B.PARTY_TYPE_CODE = A.CUSTOMER_CODE AND B.COMPANY_CODE = A.COMPANY_CODE
                          INNER JOIN HR_EMPLOYEE_SETUP C ON C.EMPLOYEE_CODE = A.SP_CODE AND C.COMPANY_CODE = A.COMPANY_CODE
                          WHERE 1 = 1
                          -- AND SP_CODE = '1000097' -- Commented out because customer can be visited by another SP_CODE
                          AND A.CUSTOMER_TYPE = 'P'
                          AND A.UPDATE_DATE = (SELECT MAX(UPDATE_DATE) FROM DIST_LOCATION_TRACK WHERE CUSTOMER_CODE = A.CUSTOMER_CODE AND CUSTOMER_TYPE = A.CUSTOMER_TYPE)
                          GROUP BY A.SP_CODE, A.CUSTOMER_CODE, A.CUSTOMER_TYPE, A.COMPANY_CODE, C.EMPLOYEE_EDESC, A.IS_VISITED, 
                              (CASE 
                                WHEN A.IS_VISITED IS NULL THEN 'X' 
                                  ELSE
                                    CASE WHEN TO_CHAR(SYSDATE, 'DD-MON-RRRR') = TO_CHAR(A.UPDATE_DATE, 'DD-MON-RRRR') THEN A.IS_VISITED ELSE 'X' END
                               END
                              ), 
                              A.REMARKS
                          ) LT
                          ON LT.CUSTOMER_CODE = DM.DEALER_CODE AND LT.COMPANY_CODE = RM.COMPANY_CODE
                WHERE 1 = 1
                AND RM.ROUTE_TYPE = 'B'
                --AND RD.EMP_CODE = '{model.spcode}'
                AND RD.COMPANY_CODE = '{model.COMPANY_CODE}'
                )
              UNION
                (SELECT
                ES.EMPLOYEE_CODE, TRIM(ES.EMPLOYEE_EDESC) EMPLOYEE_EDESC,
                DM.DISTRIBUTOR_CODE AS CODE,DM.EMAIL,
                CS.CUSTOMER_EDESC AS NAME,
                '' AS ACC_CODE,
                DM.CONTACT_NO AS P_CONTACT_NO, NVL(DM.REG_OFFICE_ADDRESS, CS.REGD_OFFICE_EADDRESS) AS ADDRESS,
                RM.ROUTE_CODE, TO_CHAR(RD.ASSIGN_DATE,'DD-MON-RRRR') AS ASSIGN_DATE, RM.ROUTE_NAME,
                AM.AREA_CODE, AM.AREA_NAME,
                TO_CHAR(RE.ORDER_NO) AS ORDER_NO,
                RM.COMPANY_CODE,
                'distributor' AS TYPE,
                'N' AS WHOLESELLER,
                CS.PARTY_TYPE_CODE AS DEFAULT_PARTY_TYPE_CODE,
                '' PARENT_DISTRIBUTOR_CODE,
                '' PARENT_DISTRIBUTOR_NAME,
                NVL(DM.LATITUDE,0) LATITUDE, NVL(DM.LONGITUDE,0) LONGITUDE,
                LT.LAST_VISIT_DATE, LT.LAST_VISIT_BY, LT.LAST_VISIT_STATUS, NVL(LT.IS_VISITED, 'X') AS IS_VISITED, LT.REMARKS    ,RD.emp_code    
                FROM DIST_ROUTE_MASTER RM
                INNER JOIN DIST_BRANDING_ROUTE_DETAIL RD ON RD.ROUTE_CODE = RM.ROUTE_CODE AND RD.COMPANY_CODE = RM.COMPANY_CODE
                INNER JOIN HR_EMPLOYEE_SETUP ES ON ES.EMPLOYEE_CODE = RD.EMP_CODE AND ES.COMPANY_CODE = RD.COMPANY_CODE
                INNER JOIN DIST_ROUTE_ENTITY RE ON RE.ROUTE_CODE = RD.ROUTE_CODE AND RE.ENTITY_TYPE = 'D' AND RE.COMPANY_CODE = RM.COMPANY_CODE
                INNER JOIN DIST_DISTRIBUTOR_MASTER DM ON DM.DISTRIBUTOR_CODE = RE.ENTITY_CODE AND DM.COMPANY_CODE = RM.COMPANY_CODE
                INNER JOIN SA_CUSTOMER_SETUP CS ON CS.CUSTOMER_CODE = DM.DISTRIBUTOR_CODE AND CS.COMPANY_CODE = DM.COMPANY_CODE
                INNER JOIN DIST_AREA_MASTER AM ON AM.AREA_CODE = DM.AREA_CODE AND AM.COMPANY_CODE = DM.COMPANY_CODE
                LEFT JOIN (SELECT A.SP_CODE, A.CUSTOMER_CODE, A.CUSTOMER_TYPE, A.COMPANY_CODE, C.EMPLOYEE_EDESC AS LAST_VISIT_BY, A.IS_VISITED AS LAST_VISIT_STATUS, 
                          (CASE 
                            WHEN A.IS_VISITED IS NULL THEN 'X' 
                              ELSE
                                CASE WHEN TO_CHAR(SYSDATE, 'DD-MON-RRRR') = TO_CHAR(A.UPDATE_DATE, 'DD-MON-RRRR') THEN A.IS_VISITED ELSE 'X' END
                           END
                          ) IS_VISITED, 
                          A.REMARKS,
                          TO_CHAR(MAX(A.UPDATE_DATE), 'RRRR-MM-DD HH24:MI A.M.') LAST_VISIT_DATE
                          FROM DIST_LOCATION_TRACK A
                          INNER JOIN SA_CUSTOMER_SETUP B ON B.CUSTOMER_CODE = A.CUSTOMER_CODE AND B.COMPANY_CODE = A.COMPANY_CODE
                          INNER JOIN HR_EMPLOYEE_SETUP C ON C.EMPLOYEE_CODE = A.SP_CODE AND C.COMPANY_CODE = A.COMPANY_CODE
                          WHERE 1 = 1
                          -- AND SP_CODE = '1000097' -- Commented out because customer can be visited by another SP_CODE
                          AND A.CUSTOMER_TYPE = 'D'
                          AND A.UPDATE_DATE = (SELECT MAX(UPDATE_DATE) FROM DIST_LOCATION_TRACK WHERE CUSTOMER_CODE = A.CUSTOMER_CODE AND CUSTOMER_TYPE = A.CUSTOMER_TYPE)
                          GROUP BY A.SP_CODE, A.CUSTOMER_CODE, A.CUSTOMER_TYPE, A.COMPANY_CODE, C.EMPLOYEE_EDESC, A.IS_VISITED, 
                              (CASE 
                                WHEN A.IS_VISITED IS NULL THEN 'X' 
                                  ELSE
                                    CASE WHEN TO_CHAR(SYSDATE, 'DD-MON-RRRR') = TO_CHAR(A.UPDATE_DATE, 'DD-MON-RRRR') THEN A.IS_VISITED ELSE 'X' END
                               END
                              ),
                              A.REMARKS
                          ) LT
                          ON LT.CUSTOMER_CODE = DM.DISTRIBUTOR_CODE AND LT.COMPANY_CODE = RM.COMPANY_CODE
                WHERE 1 = 1
                AND RM.ROUTE_TYPE = 'B'
                AND DM.ACTIVE = 'Y' AND DM.DELETED_FLAG = 'N'
                AND TO_DATE(RD.ASSIGN_DATE, 'DD-MON-RRRR') BETWEEN TO_DATE('{StartDate}', 'DD-MON-RRRR') AND TO_DATE('{EndDate}', 'DD-MON-RRRR')
              --  AND RD.EMP_CODE = '{model.spcode}'
                AND RD.COMPANY_CODE = '{model.COMPANY_CODE}'
                )
            UNION
                (SELECT
                ES.EMPLOYEE_CODE, TRIM(ES.EMPLOYEE_EDESC) EMPLOYEE_EDESC,
                RES.RESELLER_CODE AS CODE,RES.EMAIL,
                RES.RESELLER_NAME AS NAME,
                '' AS ACC_CODE,
                RES.CONTACT_NO AS P_CONTACT_NO, RES.REG_OFFICE_ADDRESS AS ADDRESS,
                RM.ROUTE_CODE, TO_CHAR(RD.ASSIGN_DATE,'DD-MON-RRRR') AS ASSIGN_DATE, RM.ROUTE_NAME,
                AM.AREA_CODE, AM.AREA_NAME,
                TO_CHAR(RE.ORDER_NO) AS ORDER_NO,
                RM.COMPANY_CODE,
                'reseller' AS TYPE,
                RES.WHOLESELLER AS WHOLESELLER,
                '' AS DEFAULT_PARTY_TYPE_CODE,
                DISTRIBUTOR_CODE AS PARENT_DISTRIBUTOR_CODE,
                CS.CUSTOMER_EDESC AS PARENT_DISTRIBUTOR_NAME,
                NVL(RES.LATITUDE,0) LATITUDE, NVL(RES.LONGITUDE,0) LONGITUDE,
                LT.LAST_VISIT_DATE, LT.LAST_VISIT_BY, LT.LAST_VISIT_STATUS, NVL(LT.IS_VISITED, 'X') AS IS_VISITED, LT.REMARKS    ,RD.emp_code                   
                FROM DIST_ROUTE_MASTER RM
                INNER JOIN DIST_BRANDING_ROUTE_DETAIL RD ON RD.ROUTE_CODE = RM.ROUTE_CODE AND RD.COMPANY_CODE = RM.COMPANY_CODE
                INNER JOIN HR_EMPLOYEE_SETUP ES ON ES.EMPLOYEE_CODE = RD.EMP_CODE AND ES.COMPANY_CODE = RD.COMPANY_CODE
                INNER JOIN DIST_ROUTE_ENTITY RE ON RE.ROUTE_CODE = RD.ROUTE_CODE AND RE.ENTITY_TYPE = 'R' AND RE.COMPANY_CODE = RM.COMPANY_CODE
                INNER JOIN DIST_RESELLER_MASTER RES ON RES.RESELLER_CODE = RE.ENTITY_CODE AND RES.COMPANY_CODE = RM.COMPANY_CODE
                LEFT JOIN DIST_AREA_MASTER AM ON AM.AREA_CODE = RES.AREA_CODE AND AM.COMPANY_CODE = RM.COMPANY_CODE
                LEFT JOIN SA_CUSTOMER_SETUP CS ON CS.CUSTOMER_CODE = RES.DISTRIBUTOR_CODE AND CS.COMPANY_CODE = RM.COMPANY_CODE
                LEFT JOIN (SELECT A.SP_CODE, A.CUSTOMER_CODE, A.CUSTOMER_TYPE, A.COMPANY_CODE, C.EMPLOYEE_EDESC AS LAST_VISIT_BY, A.IS_VISITED AS LAST_VISIT_STATUS, 
                          (CASE 
                            WHEN A.IS_VISITED IS NULL THEN 'X' 
                              ELSE
                                CASE WHEN TO_CHAR(SYSDATE, 'DD-MON-RRRR') = TO_CHAR(A.UPDATE_DATE, 'DD-MON-RRRR') THEN A.IS_VISITED ELSE 'X' END
                           END
                          ) IS_VISITED, 
                          A.REMARKS,
                          TO_CHAR(MAX(A.UPDATE_DATE), 'RRRR-MM-DD HH24:MI A.M.') LAST_VISIT_DATE
                          FROM DIST_LOCATION_TRACK A
                          INNER JOIN DIST_RESELLER_MASTER B ON B.RESELLER_CODE = A.CUSTOMER_CODE AND B.COMPANY_CODE = A.COMPANY_CODE
                          INNER JOIN HR_EMPLOYEE_SETUP C ON C.EMPLOYEE_CODE = A.SP_CODE AND C.COMPANY_CODE = A.COMPANY_CODE
                          WHERE 1 = 1
                          AND B.IS_CLOSED = 'N'
                          AND A.CUSTOMER_TYPE = 'R'
                          AND A.UPDATE_DATE = (SELECT MAX(UPDATE_DATE) FROM DIST_LOCATION_TRACK WHERE CUSTOMER_CODE = A.CUSTOMER_CODE AND CUSTOMER_TYPE = A.CUSTOMER_TYPE)
                          GROUP BY A.SP_CODE, A.CUSTOMER_CODE, A.CUSTOMER_TYPE, A.COMPANY_CODE, C.EMPLOYEE_EDESC, A.IS_VISITED, 
                              (CASE 
                                WHEN A.IS_VISITED IS NULL THEN 'X' 
                                  ELSE
                                    CASE WHEN TO_CHAR(SYSDATE, 'DD-MON-RRRR') = TO_CHAR(A.UPDATE_DATE, 'DD-MON-RRRR') THEN A.IS_VISITED ELSE 'X' END
                               END
                              ), 
                              A.REMARKS
                          ) LT
                          ON LT.CUSTOMER_CODE = RES.RESELLER_CODE AND LT.COMPANY_CODE = RM.COMPANY_CODE
                WHERE 1 = 1
                AND RM.ROUTE_TYPE = 'B'  AND RES.IS_CLOSED = 'N'
                AND TO_DATE(RD.ASSIGN_DATE, 'DD-MON-RRRR') BETWEEN TO_DATE('{StartDate}', 'DD-MON-RRRR') AND TO_DATE('{EndDate}', 'DD-MON-RRRR')
                AND RD.COMPANY_CODE = '{model.COMPANY_CODE}'
                )
                UNION
                (
                SELECT RD.EMP_CODE AS EMPLOYEE_CODE,ES.EMPLOYEE_EDESC,'' CODE,'' EMAIL, '' NAME, '' ACC_CODE,''P_CONTACT_NO,'' ADDRESS,RM.ROUTE_CODE,TO_CHAR(RD.ASSIGN_DATE,'DD-MON-RRRR') ASSIGN_DATE,RM.ROUTE_NAME,
                             '' AREA_CODE, '' AREA_NAME,TO_CHAR(RE.ORDER_NO) ORDER_NO,RM.COMPANY_CODE,'' TYPE, 'N' AS WHOLESELLER, '' DEFAULT_PARTY_TYPE_CODE, '' PARENT_DISTRIBUTOR_CODE,
                             '' PARENT_DISTRIBUTOR_NAME, '' LATITUDE, '' LONGITUDE, '' LAST_VISIT_DATE, '' LAST_VISIT_BY, '' LAST_VISIT_STATUS, '' IS_VISITED,'' REMARKS,RD.EMP_CODE
                FROM DIST_ROUTE_MASTER RM
                JOIN DIST_BRANDING_ROUTE_DETAIL RD ON RD.ROUTE_CODE = RM.ROUTE_CODE AND RD.COMPANY_CODE = RM.COMPANY_CODE
                LEFT JOIN DIST_ROUTE_ENTITY RE ON RE.ROUTE_CODE = RM.ROUTE_CODE AND RE.COMPANY_CODE = RM.COMPANY_CODE
                INNER JOIN  HR_EMPLOYEE_SETUP ES ON ES.EMPLOYEE_CODE = RD.EMP_CODE AND ES.COMPANY_CODE = RD.COMPANY_CODE
                WHERE RE.ENTITY_CODE IS NULL
                )
            ) WHERE TO_DATE(ASSIGN_DATE, 'DD-MON-RRRR') BETWEEN TO_DATE('{StartDate}', 'DD-MON-RRRR') AND TO_DATE('{EndDate}', 'DD-MON-RRRR')
                AND EMP_CODE IN( SELECT  DISTINCT
                    SP_CODE
                        FROM DIST_LOGIN_USER
                        WHERE USER_TYPE = 'S' 
                        CONNECT BY PRIOR USERID = PARENT_USERID 
                        START WITH SP_CODE ='{model.spcode}')
            ORDER BY UPPER(ROUTE_NAME), ORDER_NO, UPPER(AREA_NAME), UPPER(NAME), LAST_VISIT_DATE DESC";

            var Visitlist = dbContext.SqlQuery<VisitEntityModel>(VisitQuery).ToList();
            if (Visitlist.Count <= 0)
                throw new Exception("No records found");

            return Visitlist;
        }

        public Dictionary<string, List<EntityResponseModel>> FetchEntity(CommonRequestModel model, NeoErpCoreEntity dbContext)
        {
            var result = new Dictionary<string, List<EntityResponseModel>>();
            string EntityQuery = string.Empty;
            EntityQuery = $@"SELECT * FROM (
                  (SELECT
                     DM.DEALER_CODE AS CODE,
                     PT.PARTY_TYPE_EDESC AS NAME,
                     PT.ACC_CODE,
                     DM.CONTACT_NO AS P_CONTACT_NO, '' AS P_CONTACT_NAME, DM.REG_OFFICE_ADDRESS AS ADDRESS,DM.EMAIL,
                     --RM.ROUTE_CODE, RM.ROUTE_NAME,
                     AM.AREA_CODE, AM.AREA_NAME,
                     NVL(DM.LATITUDE,0) LATITUDE, NVL(DM.LONGITUDE,0) LONGITUDE,
                     'dealer' AS TYPE,
                     '' DEFAULT_PARTY_TYPE_CODE,
                     '' PARENT_DISTRIBUTOR_CODE,
                     '' PARENT_DISTRIBUTOR_NAME,
                     LT.LAST_VISIT_DATE, LT.LAST_VISIT_BY, LT.LAST_VISIT_STATUS, NVL(LT.IS_VISITED, 'X') AS IS_VISITED, LT.REMARKS,
                     DM.COMPANY_CODE, DM.BRANCH_CODE
                   FROM
                     DIST_DEALER_MASTER DM
                     LEFT JOIN DIST_ROUTE_ENTITY RE ON RE.ENTITY_CODE = DM.DEALER_CODE AND RE.ENTITY_TYPE = 'P' AND RE.COMPANY_CODE = DM.COMPANY_CODE
                     LEFT JOIN DIST_ROUTE_MASTER RM ON RM.ROUTE_CODE = RE.ROUTE_CODE AND RM.COMPANY_CODE = DM.COMPANY_CODE
                     INNER JOIN IP_PARTY_TYPE_CODE PT ON PT.PARTY_TYPE_CODE = DM.DEALER_CODE AND PT.COMPANY_CODE = DM.COMPANY_CODE
                     INNER JOIN DIST_AREA_MASTER AM ON AM.AREA_CODE = DM.AREA_CODE AND AM.COMPANY_CODE = DM.COMPANY_CODE
                     LEFT JOIN (SELECT A.SP_CODE, A.CUSTOMER_CODE, A.CUSTOMER_TYPE, A.COMPANY_CODE, C.EMPLOYEE_EDESC AS LAST_VISIT_BY, A.IS_VISITED AS LAST_VISIT_STATUS,
                                  (CASE 
                                    WHEN A.IS_VISITED IS NULL THEN 'X' 
                                      ELSE
                                        CASE WHEN TO_CHAR(SYSDATE, 'DD-MON-RRRR') = TO_CHAR(A.UPDATE_DATE, 'DD-MON-RRRR') THEN A.IS_VISITED ELSE 'X' END
                                   END
                                  ) IS_VISITED, 
                                  A.REMARKS,
                                  TO_CHAR(MAX(A.UPDATE_DATE), 'RRRR-MM-DD HH24:MI A.M.') LAST_VISIT_DATE
                                FROM DIST_LOCATION_TRACK A
                                  INNER JOIN IP_PARTY_TYPE_CODE B ON B.PARTY_TYPE_CODE = A.CUSTOMER_CODE AND B.COMPANY_CODE = A.COMPANY_CODE
                                  INNER JOIN HR_EMPLOYEE_SETUP C ON C.EMPLOYEE_CODE = A.SP_CODE AND C.COMPANY_CODE = A.COMPANY_CODE
                                WHERE 1 = 1
                                      AND A.CUSTOMER_TYPE = 'P'
                                      AND A.UPDATE_DATE = (SELECT MAX(UPDATE_DATE) FROM DIST_LOCATION_TRACK WHERE CUSTOMER_CODE = A.CUSTOMER_CODE AND CUSTOMER_TYPE = A.CUSTOMER_TYPE)
                                GROUP BY A.SP_CODE, A.CUSTOMER_CODE, A.CUSTOMER_TYPE, A.COMPANY_CODE, C.EMPLOYEE_EDESC, A.IS_VISITED, 
                                  (CASE 
                                    WHEN A.IS_VISITED IS NULL THEN 'X' 
                                      ELSE
                                        CASE WHEN TO_CHAR(SYSDATE, 'DD-MON-RRRR') = TO_CHAR(A.UPDATE_DATE, 'DD-MON-RRRR') THEN A.IS_VISITED ELSE 'X' END
                                   END
                                  ), 
                                  A.REMARKS
                               ) LT
                       ON LT.CUSTOMER_CODE = DM.DEALER_CODE AND LT.COMPANY_CODE = DM.COMPANY_CODE
                   WHERE 1 = 1
                         AND DM.COMPANY_CODE = '{model.COMPANY_CODE}'
                  )
                UNION
                  (SELECT
                     DM.DISTRIBUTOR_CODE AS CODE,
                     CS.CUSTOMER_EDESC AS NAME,
                     CS.ACC_CODE,
                     DM.CONTACT_NO AS P_CONTACT_NO, '' AS P_CONTACT_NAME, NVL(DM.REG_OFFICE_ADDRESS, CS.REGD_OFFICE_EADDRESS) AS ADDRESS,DM.EMAIL,
                     --RM.ROUTE_CODE, RM.ROUTE_NAME,
                     AM.AREA_CODE, AM.AREA_NAME,
                     NVL(DM.LATITUDE,0) LATITUDE, NVL(DM.LONGITUDE,0) LONGITUDE,
                     'distributor' AS TYPE,
                     CS.PARTY_TYPE_CODE AS DEFAULT_PARTY_TYPE_CODE,
                     '' PARENT_DISTRIBUTOR_CODE,
                     '' PARENT_DISTRIBUTOR_NAME,
                     LT.LAST_VISIT_DATE, LT.LAST_VISIT_BY, LT.LAST_VISIT_STATUS, NVL(LT.IS_VISITED, 'X') AS IS_VISITED, LT.REMARKS,
                     DM.COMPANY_CODE, DM.BRANCH_CODE
                   FROM
                     DIST_DISTRIBUTOR_MASTER DM
                     LEFT JOIN DIST_ROUTE_ENTITY RE ON RE.ENTITY_CODE = DM.DISTRIBUTOR_CODE AND RE.ENTITY_TYPE = 'D' AND RE.COMPANY_CODE = DM.COMPANY_CODE
                     LEFT JOIN DIST_ROUTE_MASTER RM ON RM.ROUTE_CODE = RE.ROUTE_CODE AND RM.COMPANY_CODE = DM.COMPANY_CODE
                     LEFT JOIN SA_CUSTOMER_SETUP CS ON CS.CUSTOMER_CODE = DM.DISTRIBUTOR_CODE AND CS.COMPANY_CODE = DM.COMPANY_CODE
                     INNER JOIN DIST_AREA_MASTER AM ON AM.AREA_CODE = DM.AREA_CODE AND AM.COMPANY_CODE = DM.COMPANY_CODE
                     LEFT JOIN (SELECT A.SP_CODE, A.CUSTOMER_CODE, A.CUSTOMER_TYPE, A.COMPANY_CODE, C.EMPLOYEE_EDESC AS LAST_VISIT_BY, A.IS_VISITED AS LAST_VISIT_STATUS,
                                  (CASE 
                                    WHEN A.IS_VISITED IS NULL THEN 'X' 
                                      ELSE
                                        CASE WHEN TO_CHAR(SYSDATE, 'DD-MON-RRRR') = TO_CHAR(A.UPDATE_DATE, 'DD-MON-RRRR') THEN A.IS_VISITED ELSE 'X' END
                                   END
                                  ) IS_VISITED, 
                                  A.REMARKS,
                                  TO_CHAR(MAX(A.UPDATE_DATE), 'RRRR-MM-DD HH24:MI A.M.') LAST_VISIT_DATE
                                FROM DIST_LOCATION_TRACK A
                                  INNER JOIN SA_CUSTOMER_SETUP B ON B.CUSTOMER_CODE = A.CUSTOMER_CODE AND B.COMPANY_CODE = A.COMPANY_CODE
                                  INNER JOIN HR_EMPLOYEE_SETUP C ON C.EMPLOYEE_CODE = A.SP_CODE AND C.COMPANY_CODE = A.COMPANY_CODE
                                WHERE 1 = 1
                                      AND A.CUSTOMER_TYPE = 'D'
                                      AND A.UPDATE_DATE = (SELECT MAX(UPDATE_DATE) FROM DIST_LOCATION_TRACK WHERE CUSTOMER_CODE = A.CUSTOMER_CODE AND CUSTOMER_TYPE = A.CUSTOMER_TYPE)
                                GROUP BY A.SP_CODE, A.CUSTOMER_CODE, A.CUSTOMER_TYPE, A.COMPANY_CODE, C.EMPLOYEE_EDESC, A.IS_VISITED, 
                                  (CASE 
                                    WHEN A.IS_VISITED IS NULL THEN 'X' 
                                      ELSE
                                        CASE WHEN TO_CHAR(SYSDATE, 'DD-MON-RRRR') = TO_CHAR(A.UPDATE_DATE, 'DD-MON-RRRR') THEN A.IS_VISITED ELSE 'X' END
                                   END
                                  ), 
                                  A.REMARKS
                               ) LT
                       ON LT.CUSTOMER_CODE = DM.DISTRIBUTOR_CODE AND LT.COMPANY_CODE = DM.COMPANY_CODE
                   WHERE 1 = 1
                         AND DM.COMPANY_CODE = '{model.COMPANY_CODE}'
                         AND DM.ACTIVE = 'Y' AND DM.DELETED_FLAG = 'N'
                  )
                  UNION
                  (SELECT
                     REM.RESELLER_CODE AS CODE,
                     REM.RESELLER_NAME AS NAME,
                     '' AS ACC_CODE,
                     REM.CONTACT_NO AS P_CONTACT_NO, REM.CONTACT_NAME AS P_CONTACT_NAME, REM.REG_OFFICE_ADDRESS AS ADDRESS,REM.EMAIL,
                     --RM.ROUTE_CODE, RM.ROUTE_NAME,
                     AM.AREA_CODE, AM.AREA_NAME,
                     NVL(REM.LATITUDE,0) LATITUDE, NVL(REM.LONGITUDE,0) LONGITUDE,
                     'reseller' AS TYPE,
                     '' AS DEFAULT_PARTY_TYPE_CODE,
                     DISTRIBUTOR_CODE AS PARENT_DISTRIBUTOR_CODE,
                     CS.CUSTOMER_EDESC AS PARENT_DISTRIBUTOR_NAME,
                     LT.LAST_VISIT_DATE, LT.LAST_VISIT_BY, LT.LAST_VISIT_STATUS, NVL(LT.IS_VISITED, 'X') AS IS_VISITED, LT.REMARKS,
                     REM.COMPANY_CODE, REM.BRANCH_CODE
                   FROM DIST_RESELLER_MASTER REM
                     LEFT JOIN DIST_ROUTE_ENTITY RE ON RE.ENTITY_CODE = REM.RESELLER_CODE AND RE.ENTITY_TYPE = 'R' AND RE.COMPANY_CODE = REM.COMPANY_CODE
                     LEFT JOIN DIST_ROUTE_MASTER RM ON RM.ROUTE_CODE = RE.ROUTE_CODE AND RM.COMPANY_CODE = REM.COMPANY_CODE
                     INNER JOIN DIST_AREA_MASTER AM ON AM.AREA_CODE = REM.AREA_CODE AND AM.COMPANY_CODE = REM.COMPANY_CODE
                     LEFT JOIN SA_CUSTOMER_SETUP CS ON CS.CUSTOMER_CODE = REM.DISTRIBUTOR_CODE AND CS.COMPANY_CODE = REM.COMPANY_CODE
                     LEFT JOIN (SELECT A.SP_CODE, A.CUSTOMER_CODE, A.CUSTOMER_TYPE, A.COMPANY_CODE, C.EMPLOYEE_EDESC AS LAST_VISIT_BY, A.IS_VISITED AS LAST_VISIT_STATUS,
                                 (CASE 
                                        WHEN A.IS_VISITED IS NULL THEN 'X' 
                                          ELSE
                                            CASE WHEN TO_CHAR(SYSDATE, 'DD-MON-RRRR') = TO_CHAR(A.UPDATE_DATE, 'DD-MON-RRRR') THEN A.IS_VISITED ELSE 'X' END
                                       END
                                      ) IS_VISITED, 
                                 A.REMARKS,
                                 TO_CHAR(MAX(A.UPDATE_DATE), 'RRRR-MM-DD HH24:MI A.M.') LAST_VISIT_DATE
                                FROM DIST_LOCATION_TRACK A
                                  INNER JOIN DIST_RESELLER_MASTER B ON B.RESELLER_CODE = A.CUSTOMER_CODE AND B.COMPANY_CODE = A.COMPANY_CODE
                                  INNER JOIN HR_EMPLOYEE_SETUP C ON C.EMPLOYEE_CODE = A.SP_CODE AND C.COMPANY_CODE = A.COMPANY_CODE
                                WHERE 1 = 1
                                      AND B.IS_CLOSED = 'N'
                                      AND A.CUSTOMER_TYPE = 'R'
                                      AND A.UPDATE_DATE = (SELECT MAX(UPDATE_DATE) FROM DIST_LOCATION_TRACK WHERE CUSTOMER_CODE = A.CUSTOMER_CODE AND CUSTOMER_TYPE = A.CUSTOMER_TYPE)
                                GROUP BY A.SP_CODE, A.CUSTOMER_CODE, A.CUSTOMER_TYPE, A.COMPANY_CODE, C.EMPLOYEE_EDESC, A.IS_VISITED, 
                                  (CASE 
                                    WHEN A.IS_VISITED IS NULL THEN 'X' 
                                      ELSE
                                        CASE WHEN TO_CHAR(SYSDATE, 'DD-MON-RRRR') = TO_CHAR(A.UPDATE_DATE, 'DD-MON-RRRR') THEN A.IS_VISITED ELSE 'X' END
                                   END
                                  ), 
                                  A.REMARKS
                               ) LT
                       ON LT.CUSTOMER_CODE = REM.RESELLER_CODE AND LT.COMPANY_CODE = REM.COMPANY_CODE
                   WHERE 1 = 1
                         AND REM.COMPANY_CODE = '{model.COMPANY_CODE}'
                         AND REM.IS_CLOSED = 'N'
                  )
                )
                ORDER BY UPPER(NAME)";
            var Entities = dbContext.SqlQuery<EntityResponseModel>(EntityQuery).GroupBy(x => x.TYPE);
            if (Entities.Count() <= 0)
                throw new Exception("No records found");
            foreach (var EntGroup in Entities)
            {
                result.Add(EntGroup.Key, EntGroup.ToList());
            }
            return (result);
        }

        public Dictionary<string, List<EntityResponseModel>> FetchAllCompanyEntity(string companyCode, string spCode, NeoErpCoreEntity dbContext, PreferenceModel pref)
        {
            var result = new Dictionary<string, List<EntityResponseModel>>();
            var CompanyFilter = pref.SQL_MULTIPLE_COMPANY == "Y" ? "SELECT COMPANY_CODE FROM COMPANY_SETUP" : $"'{companyCode}'";
            var entityFIlter = "";
            if (pref.SQL_COMPANY_ENTITY == "N")
                entityFIlter = $@" AND REM.AREA_CODE IN (SELECT AREA_CODE FROM DIST_AREA_MASTER WHERE GROUPID IN
                        (SELECT MAPPED_GROUPID FROM DIST_GROUP_MAPPING WHERE GROUPID = 
                        (SELECT GROUPID FROM DIST_LOGIN_USER WHERE SP_CODE = '{spCode}') UNION (SELECT GROUPID FROM DIST_LOGIN_USER WHERE SP_CODE = '{spCode}')))";

            string EntityQuery = string.Empty;
            EntityQuery = $@"SELECT * FROM (
                  (SELECT
                     DM.DEALER_CODE AS CODE,
                     TRIM(PT.PARTY_TYPE_EDESC) AS NAME,
                     PT.ACC_CODE,'' AS CONTACT_NO,
                     DM.CONTACT_NO AS P_CONTACT_NO, '' AS P_CONTACT_NAME, DM.REG_OFFICE_ADDRESS AS ADDRESS,
                     --RM.ROUTE_CODE, RM.ROUTE_NAME,
                     AM.AREA_CODE, AM.AREA_NAME,
                     NVL(DM.LATITUDE,0) LATITUDE, NVL(DM.LONGITUDE,0) LONGITUDE,
                     'dealer' AS TYPE,
                     'N' AS WHOLESELLER,
                     '' DEFAULT_PARTY_TYPE_CODE,
                     '' PARENT_DISTRIBUTOR_CODE,
                     '' PARENT_DISTRIBUTOR_NAME,
                     LT.LAST_VISIT_DATE, LT.LAST_VISIT_BY, LT.LAST_VISIT_STATUS, NVL(LT.IS_VISITED, 'X') AS IS_VISITED, LT.REMARKS,
                     DM.COMPANY_CODE, DM.BRANCH_CODE,'' TYPE_EDESC,'' SUBTYPE_EDESC
                     FROM DIST_DEALER_MASTER DM
                     LEFT JOIN DIST_ROUTE_ENTITY RE ON RE.ENTITY_CODE = DM.DEALER_CODE AND RE.ENTITY_TYPE = 'P' AND RE.COMPANY_CODE = DM.COMPANY_CODE
                     LEFT JOIN DIST_ROUTE_MASTER RM ON RM.ROUTE_CODE = RE.ROUTE_CODE AND RM.COMPANY_CODE = DM.COMPANY_CODE
                     INNER JOIN IP_PARTY_TYPE_CODE PT ON PT.PARTY_TYPE_CODE = DM.DEALER_CODE AND PT.COMPANY_CODE = DM.COMPANY_CODE
                     INNER JOIN DIST_AREA_MASTER AM ON AM.AREA_CODE = DM.AREA_CODE AND AM.COMPANY_CODE = DM.COMPANY_CODE
                    left join dist_user_areas dua on dua.AREA_CODE = DM.AREA_CODE AND dua.COMPANY_CODE = DM.COMPANY_CODE
                     LEFT JOIN (SELECT A.SP_CODE, A.CUSTOMER_CODE, A.CUSTOMER_TYPE, A.COMPANY_CODE, C.EMPLOYEE_EDESC AS LAST_VISIT_BY, A.IS_VISITED AS LAST_VISIT_STATUS,
                                  (CASE 
                                    WHEN A.IS_VISITED IS NULL THEN 'X' 
                                      ELSE
                                        CASE WHEN TO_CHAR(SYSDATE, 'DD-MON-RRRR') = TO_CHAR(A.UPDATE_DATE, 'DD-MON-RRRR') THEN A.IS_VISITED ELSE 'X' END
                                   END
                                  ) IS_VISITED, 
                                  A.REMARKS,
                                  TO_CHAR(MAX(A.UPDATE_DATE), 'RRRR-MM-DD HH24:MI A.M.') LAST_VISIT_DATE
                                FROM DIST_LOCATION_TRACK A
                                  INNER JOIN IP_PARTY_TYPE_CODE B ON B.PARTY_TYPE_CODE = A.CUSTOMER_CODE AND B.COMPANY_CODE = A.COMPANY_CODE
                                  INNER JOIN HR_EMPLOYEE_SETUP C ON C.EMPLOYEE_CODE = A.SP_CODE AND C.COMPANY_CODE = A.COMPANY_CODE
                                WHERE 1 = 1
                                      AND A.CUSTOMER_TYPE = 'P'
                                      AND A.UPDATE_DATE = (SELECT MAX(UPDATE_DATE) FROM DIST_LOCATION_TRACK WHERE CUSTOMER_CODE = A.CUSTOMER_CODE AND CUSTOMER_TYPE = A.CUSTOMER_TYPE)
                                GROUP BY A.SP_CODE, A.CUSTOMER_CODE, A.CUSTOMER_TYPE, A.COMPANY_CODE, C.EMPLOYEE_EDESC, A.IS_VISITED, 
                                  (CASE 
                                    WHEN A.IS_VISITED IS NULL THEN 'X' 
                                      ELSE
                                        CASE WHEN TO_CHAR(SYSDATE, 'DD-MON-RRRR') = TO_CHAR(A.UPDATE_DATE, 'DD-MON-RRRR') THEN A.IS_VISITED ELSE 'X' END
                                   END
                                  ), 
                                  A.REMARKS
                               ) LT
                       ON LT.CUSTOMER_CODE = DM.DEALER_CODE AND LT.COMPANY_CODE = DM.COMPANY_CODE where dua.sp_code='{spCode}' and dua.deleted_flag='N')
                UNION
                  (SELECT
                     DM.DISTRIBUTOR_CODE AS CODE,
                     TRIM(CS.CUSTOMER_EDESC) AS NAME,
                     CS.ACC_CODE,'' AS CONTACT_NO,
                     DM.CONTACT_NO AS P_CONTACT_NO, '' AS P_CONTACT_NAME, NVL(DM.REG_OFFICE_ADDRESS, CS.REGD_OFFICE_EADDRESS) AS ADDRESS,
                     --RM.ROUTE_CODE, RM.ROUTE_NAME,
                     AM.AREA_CODE, AM.AREA_NAME,
                     NVL(DM.LATITUDE,0) LATITUDE, NVL(DM.LONGITUDE,0) LONGITUDE,
                     'distributor' AS TYPE,
                     'N' AS WHOLESELLER,
                     CS.PARTY_TYPE_CODE AS DEFAULT_PARTY_TYPE_CODE,
                     '' PARENT_DISTRIBUTOR_CODE,
                     '' PARENT_DISTRIBUTOR_NAME,
                     LT.LAST_VISIT_DATE, LT.LAST_VISIT_BY, LT.LAST_VISIT_STATUS, NVL(LT.IS_VISITED, 'X') AS IS_VISITED, LT.REMARKS,
                     DM.COMPANY_CODE, DM.BRANCH_CODE,'' TYPE_EDESC,'' SUBTYPE_EDESC
                     FROM DIST_DISTRIBUTOR_MASTER DM
                     LEFT JOIN DIST_ROUTE_ENTITY RE ON RE.ENTITY_CODE = DM.DISTRIBUTOR_CODE AND RE.ENTITY_TYPE = 'D' AND RE.COMPANY_CODE = DM.COMPANY_CODE
                     LEFT JOIN DIST_ROUTE_MASTER RM ON RM.ROUTE_CODE = RE.ROUTE_CODE AND RM.COMPANY_CODE = DM.COMPANY_CODE
                     LEFT JOIN SA_CUSTOMER_SETUP CS ON CS.CUSTOMER_CODE = DM.DISTRIBUTOR_CODE AND CS.COMPANY_CODE = DM.COMPANY_CODE
                     INNER JOIN DIST_AREA_MASTER AM ON AM.AREA_CODE = DM.AREA_CODE AND AM.COMPANY_CODE = DM.COMPANY_CODE
                    left join dist_user_areas dua on dua.AREA_CODE = DM.AREA_CODE AND dua.COMPANY_CODE = DM.COMPANY_CODE
                     LEFT JOIN (SELECT A.SP_CODE, A.CUSTOMER_CODE, A.CUSTOMER_TYPE, A.COMPANY_CODE, C.EMPLOYEE_EDESC AS LAST_VISIT_BY, A.IS_VISITED AS LAST_VISIT_STATUS,
                                  (CASE 
                                    WHEN A.IS_VISITED IS NULL THEN 'X' 
                                      ELSE
                                        CASE WHEN TO_CHAR(SYSDATE, 'DD-MON-RRRR') = TO_CHAR(A.UPDATE_DATE, 'DD-MON-RRRR') THEN A.IS_VISITED ELSE 'X' END
                                   END
                                  ) IS_VISITED, 
                                  A.REMARKS,
                                  TO_CHAR(MAX(A.UPDATE_DATE), 'RRRR-MM-DD HH24:MI A.M.') LAST_VISIT_DATE
                                FROM DIST_LOCATION_TRACK A
                                  INNER JOIN SA_CUSTOMER_SETUP B ON B.CUSTOMER_CODE = A.CUSTOMER_CODE AND B.COMPANY_CODE = A.COMPANY_CODE
                                  INNER JOIN HR_EMPLOYEE_SETUP C ON C.EMPLOYEE_CODE = A.SP_CODE AND C.COMPANY_CODE = A.COMPANY_CODE
                                WHERE 1 = 1
                                      AND A.CUSTOMER_TYPE = 'D'
                                      AND A.UPDATE_DATE = (SELECT MAX(UPDATE_DATE) FROM DIST_LOCATION_TRACK WHERE CUSTOMER_CODE = A.CUSTOMER_CODE AND CUSTOMER_TYPE = A.CUSTOMER_TYPE)
                                GROUP BY A.SP_CODE, A.CUSTOMER_CODE, A.CUSTOMER_TYPE, A.COMPANY_CODE, C.EMPLOYEE_EDESC, A.IS_VISITED, 
                                  (CASE 
                                    WHEN A.IS_VISITED IS NULL THEN 'X' 
                                      ELSE
                                        CASE WHEN TO_CHAR(SYSDATE, 'DD-MON-RRRR') = TO_CHAR(A.UPDATE_DATE, 'DD-MON-RRRR') THEN A.IS_VISITED ELSE 'X' END
                                   END
                                  ), 
                                  A.REMARKS
                               ) LT
                       ON LT.CUSTOMER_CODE = DM.DISTRIBUTOR_CODE AND LT.COMPANY_CODE = DM.COMPANY_CODE
                        WHERE DM.ACTIVE = 'Y' AND DM.DELETED_FLAG = 'N' and dua.sp_code='{spCode}' and dua.deleted_flag='N')
                  UNION
                  (SELECT
                     REM.RESELLER_CODE AS CODE,
                     TRIM(REM.RESELLER_NAME) AS NAME,
                     '' AS ACC_CODE,REM.RESELLER_CONTACT AS CONTACT_NO,
                     REM.CONTACT_NO AS P_CONTACT_NO, REM.CONTACT_NAME AS P_CONTACT_NAME, REM.REG_OFFICE_ADDRESS AS ADDRESS,
                     --RM.ROUTE_CODE, RM.ROUTE_NAME,
                     AM.AREA_CODE, AM.AREA_NAME,
                     NVL(REM.LATITUDE,0) LATITUDE, NVL(REM.LONGITUDE,0) LONGITUDE,
                     'reseller' AS TYPE,
                     REM.WHOLESELLER AS WHOLESELLER,
                     '' AS DEFAULT_PARTY_TYPE_CODE,
                      DISTRIBUTOR_CODE        AS parent_distributor_code,
                     CS.CUSTOMER_EDESC AS PARENT_DISTRIBUTOR_NAME,
                     LT.LAST_VISIT_DATE, LT.LAST_VISIT_BY, LT.LAST_VISIT_STATUS, NVL(LT.IS_VISITED, 'X') AS IS_VISITED, LT.REMARKS,
                     REM.COMPANY_CODE, REM.BRANCH_CODE,
                     DOT.TYPE_EDESC,DOS.SUBTYPE_EDESC
                   FROM DIST_RESELLER_MASTER REM
                     LEFT JOIN DIST_ROUTE_ENTITY RE ON RE.ENTITY_CODE = REM.RESELLER_CODE AND RE.ENTITY_TYPE = 'R' AND RE.COMPANY_CODE = REM.COMPANY_CODE
                     LEFT JOIN DIST_ROUTE_MASTER RM ON RM.ROUTE_CODE = RE.ROUTE_CODE AND RM.COMPANY_CODE = REM.COMPANY_CODE
                     INNER JOIN DIST_AREA_MASTER AM ON AM.AREA_CODE = REM.AREA_CODE AND AM.COMPANY_CODE = REM.COMPANY_CODE
                     LEFT JOIN SA_CUSTOMER_SETUP CS ON CS.CUSTOMER_CODE = REM.DISTRIBUTOR_CODE AND CS.COMPANY_CODE = REM.COMPANY_CODE
                    left join dist_user_areas dua on dua.AREA_CODE = REM.AREA_CODE AND dua.COMPANY_CODE = REM.COMPANY_CODE
                     LEFT JOIN (SELECT A.SP_CODE, A.CUSTOMER_CODE, A.CUSTOMER_TYPE, A.COMPANY_CODE, C.EMPLOYEE_EDESC AS LAST_VISIT_BY, A.IS_VISITED AS LAST_VISIT_STATUS,
                                 (CASE 
                                        WHEN A.IS_VISITED IS NULL THEN 'X' 
                                          ELSE
                                            CASE WHEN TO_CHAR(SYSDATE, 'DD-MON-RRRR') = TO_CHAR(A.UPDATE_DATE, 'DD-MON-RRRR') THEN A.IS_VISITED ELSE 'X' END
                                       END
                                      ) IS_VISITED, 
                                 A.REMARKS,
                                 TO_CHAR(MAX(A.UPDATE_DATE), 'RRRR-MM-DD HH24:MI A.M.') LAST_VISIT_DATE
                                FROM DIST_LOCATION_TRACK A
                                  INNER JOIN DIST_RESELLER_MASTER B ON B.RESELLER_CODE = A.CUSTOMER_CODE AND B.COMPANY_CODE = A.COMPANY_CODE
                                  INNER JOIN HR_EMPLOYEE_SETUP C ON C.EMPLOYEE_CODE = A.SP_CODE AND C.COMPANY_CODE = A.COMPANY_CODE
                                WHERE 1 = 1
                                      AND B.IS_CLOSED = 'N'
                                      AND A.CUSTOMER_TYPE = 'R'
                                      AND A.UPDATE_DATE = (SELECT MAX(UPDATE_DATE) FROM DIST_LOCATION_TRACK WHERE CUSTOMER_CODE = A.CUSTOMER_CODE AND CUSTOMER_TYPE = A.CUSTOMER_TYPE)
                                GROUP BY A.SP_CODE, A.CUSTOMER_CODE, A.CUSTOMER_TYPE, A.COMPANY_CODE, C.EMPLOYEE_EDESC, A.IS_VISITED, 
                                  (CASE 
                                    WHEN A.IS_VISITED IS NULL THEN 'X' 
                                      ELSE
                                        CASE WHEN TO_CHAR(SYSDATE, 'DD-MON-RRRR') = TO_CHAR(A.UPDATE_DATE, 'DD-MON-RRRR') THEN A.IS_VISITED ELSE 'X' END
                                   END
                                  ), 
                                  A.REMARKS
                               ) LT ON LT.CUSTOMER_CODE = REM.RESELLER_CODE AND LT.COMPANY_CODE = REM.COMPANY_CODE
               LEFT JOIN DIST_OUTLET_TYPE DOT ON REM.OUTLET_TYPE_ID=DOT.TYPE_ID AND REM.COMPANY_CODE=DOT.COMPANY_CODE
               LEFT JOIN DIST_OUTLET_SUBTYPE DOS ON REM.OUTLET_SUBTYPE_ID=DOS.SUBTYPE_ID AND REM.COMPANY_CODE=DOS.COMPANY_CODE
               WHERE REM.ACTIVE = 'Y' AND REM.IS_CLOSED = 'N' and dua.sp_code='{spCode}' and dua.deleted_flag='N'
                {entityFIlter}) )
                WHERE COMPANY_CODE IN ({CompanyFilter})
                ORDER BY TYPE";
            var tempEntities = dbContext.SqlQuery<EntityResponseModel>(EntityQuery).ToList();
            var Entities = tempEntities.GroupBy(x => x.TYPE);
            if (tempEntities.Count <= 0)
                throw new Exception("No records found");
            foreach (var EntGroup in Entities)
            {
                result.Add(EntGroup.Key, EntGroup.ToList());
            }
            return (result);
        }

        public List<ItemModel> FetchItems(CommonRequestModel model, NeoErpCoreEntity dbContext)
        {
            var pref = FetchPreferences(model.COMPANY_CODE, dbContext);
            string salesClause = "", conversionClause = "";
            if ("Y" == pref.PO_SYN_RATE)
                salesClause = "AND SALES_RATE IS NOT NULL AND SALES_RATE <> 0";
            if ("Y" == pref.SQL_NN_CONVERSION_UNIT_FACTOR)
                conversionClause = "AND IUS.MU_CODE IS NOT NULL AND IUS.CONVERSION_FACTOR IS NOT NULL";
            string ItemsQuery = string.Empty;
            ItemsQuery = $@"SELECT IM.ITEM_CODE, IM.ITEM_EDESC, ISS.BRAND_NAME, IM.INDEX_MU_CODE AS UNIT, MC.MU_EDESC, IUS.MU_CODE CONVERSION_UNIT,
                TO_CHAR(IUS.CONVERSION_FACTOR) AS CONVERSION_FACTOR, TO_CHAR(NVL(IR.SALES_RATE, 0)) SALES_RATE, TO_CHAR(IR.APPLY_DATE) AS APPLY_DATE
                FROM IP_ITEM_MASTER_SETUP IM
                  INNER JOIN IP_MU_CODE MC ON MC.MU_CODE = IM.INDEX_MU_CODE AND MC.COMPANY_CODE = IM.COMPANY_CODE
                  INNER JOIN IP_ITEM_SPEC_SETUP ISS ON ISS.ITEM_CODE = IM.ITEM_CODE AND ISS.COMPANY_CODE = IM.COMPANY_CODE AND TRIM(ISS.BRAND_NAME) IS NOT NULL
                  LEFT JOIN IP_ITEM_UNIT_SETUP IUS ON IUS.ITEM_CODE = ISS.ITEM_CODE AND IUS.COMPANY_CODE = ISS.COMPANY_CODE
                  LEFT JOIN (SELECT A.ITEM_CODE, A.APPLY_DATE, B.SALES_RATE, B.COMPANY_CODE
                              FROM (SELECT ITEM_CODE, COMPANY_CODE, MAX(APP_DATE) APPLY_DATE 
                                FROM IP_ITEM_RATE_APPLICAT_SETUP
                                WHERE COMPANY_CODE = '{model.COMPANY_CODE}' 
                                AND BRANCH_CODE = '{model.BRANCH_CODE}'
                                GROUP BY ITEM_CODE, COMPANY_CODE) A
                              INNER JOIN IP_ITEM_RATE_APPLICAT_SETUP B
                                ON B.ITEM_CODE = A.ITEM_CODE
                                AND B.APP_DATE = A.APPLY_DATE
                                AND B.COMPANY_CODE = '{model.COMPANY_CODE}'
                                AND B.BRANCH_CODE = '{model.BRANCH_CODE}') IR 
                    ON IR.ITEM_CODE = IM.ITEM_CODE AND IR.COMPANY_CODE = IM.COMPANY_CODE
                WHERE IM.COMPANY_CODE = '{model.COMPANY_CODE}' AND IM.CATEGORY_CODE = '{CATEGORY_CODE}' AND IM.GROUP_SKU_FLAG = '{GROUP_SKU_FLAG}' AND IM.DELETED_FLAG = 'N'
                {salesClause}
                {conversionClause}
                ORDER BY UPPER(IM.ITEM_EDESC) ASC";
            var Items = dbContext.SqlQuery<ItemModel>(ItemsQuery).ToList();
            if (Items.Count <= 0)
                throw new Exception("No records found");
            return Items;
        }
        private List<DiscountModel> FetchDiscount(string companyCode,string branchCode, NeoErpCoreEntity dbContext, PreferenceModel pref)
        {
          var Query = $@"SELECT distinct iids.cs_code AS customer_code,iids.item_code,iids.discount_rate,iids.discount_percent,iids.form_code,iids.mu_code,'Y' AS synergy_flag
                FROM  ip_item_discount_schedule   iids
                LEFT JOIN form_branch_map   fbm ON iids.form_code = fbm.form_code AND iids.company_code = fbm.company_code
                WHERE iids.company_code in ('{companyCode}')  AND fbm.branch_code in ('{branchCode}') AND iids.deleted_flag = 'N' AND fbm.deleted_flag = 'N'
                union 
                SELECT distinct iids.cs_code AS customer_code,iius.item_code, ROUND(nvl(iius.fraction,0) / nvl(iius.conversion_factor,1) * nvl(iids.discount_rate,0), 2) AS discount_rate,
                iids.discount_percent,iids.form_code,iius.mu_code,'Y' AS synergy_flag
                FROM  ip_item_discount_schedule   iids
                LEFT JOIN form_branch_map  fbm ON iids.form_code = fbm.form_code AND iids.company_code = fbm.company_code
                LEFT JOIN ip_item_unit_setup iius ON iius.item_code = iids.item_code AND iids.company_code = iius.company_code
                WHERE iids.company_code in ('{companyCode}') AND fbm.branch_code in ('{branchCode}') AND iius.deleted_flag = 'N' AND iids.deleted_flag = 'N' AND fbm.deleted_flag = 'N'";
            var data = dbContext.SqlQuery<DiscountModel>(Query).ToList();
            if (data.Count <= 0)
                throw new Exception("No records found");
            return data;
        }

        public QuestionResponseModel FetchAllQuestions(QuestionRequestModel model, NeoErpCoreEntity dbContext)
        {
            model.SetType = model.SetType ?? "G";
            QuestionResponseModel Result = new QuestionResponseModel();
            //general Questions
            var generalQuery = string.Empty;
            generalQuery = $@"SELECT TO_CHAR(A.QA_CODE) AS QA_CODE, A.QA_TYPE, A.QUESTION,B.QA_TYPE SET_TYPE,SPM.SURVEY_EDESC,SPM.AREA_CODES, SPM.GROUP_CODES
                    FROM DIST_QA_MASTER A
                    INNER JOIN  DIST_QA_SET B ON  B.SET_CODE=A.SET_CODE AND B.COMPANY_CODE=A.COMPANY_CODE
                    INNER JOIN DIST_QA_SURVEY_MAP MAP ON B.SET_CODE = MAP.SET_ID
                    INNER JOIN DIST_QA_SET_SALESPERSON_MAP SPM ON SPM.SURVEY_CODE = MAP.SURVEY_ID
                    WHERE SPM.SP_CODE = '{model.sp_code}'
                    AND TRUNC(SPM.EXPIRY_DATE) >= TRUNC(SYSDATE)
                    AND A.DELETED_FLAG = 'N'
                    --AND B.QA_TYPE IN({model.SetType})
                    AND MAP.SET_TYPE = 'G'
                    AND A.COMPANY_CODE='{model.COMPANY_CODE}'";
            var generalQuestion = dbContext.SqlQuery<GeneralModel>(generalQuery).ToList();
            for (int i = 0; i < generalQuestion.Count; i++)
            {
                string AnswerQuery = string.Empty;
                string[] types = { "MCR", "MCC" };
                if (types.Contains(generalQuestion[i].QA_TYPE))
                {
                    AnswerQuery = $@"SELECT ANSWERS FROM DIST_QA_DETAIL WHERE QA_CODE='{generalQuestion[i].QA_CODE}' AND COMPANY_CODE='{model.COMPANY_CODE}'";
                    var answers = dbContext.SqlQuery<string>(AnswerQuery).ToList();
                    generalQuestion[i].ANSWERS = answers;
                }
            }

            //tabular questions
            string TabularQuery = string.Empty;
            TabularQuery = $@"SELECT TBL. TABLE_ID,TBL.TABLE_TITLE,TBL.CREATED_DATE,TBL.DELETED_FLAG,TBL.COMPANY_CODE,TBL.BRANCH_CODE,TBL.QA_TYPE SET_TYPE,
                    TC.CELL_ID, TC.ROW_NO, TC.CELL_NO, TC.CELL_TYPE, TC.CELL_LABEL,SPM.SURVEY_EDESC,SPM.AREA_CODES, SPM.GROUP_CODES
                FROM DIST_QA_TAB_TABLE TBL
                LEFT JOIN DIST_QA_TAB_CELL TC ON TBL.TABLE_ID = TC.TABLE_ID
                INNER JOIN DIST_QA_SURVEY_MAP MAP ON TBL.TABLE_ID = MAP.SET_ID
                INNER JOIN DIST_QA_SET_SALESPERSON_MAP SPM ON SPM.SURVEY_CODE = MAP.SURVEY_ID
                WHERE SPM.SP_CODE = '{model.sp_code}'
                AND TRUNC(SPM.EXPIRY_DATE) >= TRUNC(SYSDATE)
                AND TBL.COMPANY_CODE = '{model.COMPANY_CODE}'
                --AND TBL.QA_TYPE IN({model.SetType})
                AND MAP.SET_TYPE = 'T'
                ORDER BY TBL.TABLE_ID, TC.ROW_NO, TC.CELL_NO";
            var tempData = dbContext.SqlQuery<TabularTemp>(TabularQuery).GroupBy(x => x.TABLE_ID);
            var tabularQuestions = new Dictionary<string, TabularModel>();
            foreach (var table in tempData)
            {
                var tables = table.ToList();
                var resultTable = new TabularModel
                {
                    TABLE_ID = tables[0].TABLE_ID.ToString(),
                    TABLE_TITLE = tables[0].TABLE_TITLE,
                    CREATED_DATE = tables[0].CREATED_DATE == null ? null : tables[0].CREATED_DATE.Value.ToString("dd-MMM-yy"),
                    DELETED_FLAG = tables[0].DELETED_FLAG,
                    SET_TYPE = tables[0].SET_TYPE,
                    SURVEY_EDESC = tables[0].SURVEY_EDESC,
                    AREA_CODES = tables[0].AREA_CODES,
                    GROUP_CODES = tables[0].GROUP_CODES
                };
                var groupTables = tables.GroupBy(x => x.ROW_NO);
                foreach (var rowGroup in groupTables)
                {
                    var CellGroup = (from r in rowGroup
                                     select new TabularCellModel
                                     {
                                         CELL_ID = r.CELL_ID.ToString(),
                                         CELL_LABEL = r.CELL_LABEL,
                                         CELL_NO = r.CELL_NO.ToString(),
                                         CELL_TYPE = r.CELL_TYPE,
                                         ROW_NO = r.ROW_NO.ToString()
                                     }).ToList();
                    resultTable.CELL_DATA.Add(CellGroup);
                }
                tabularQuestions.Add(table.Key.ToString(), resultTable);
            }
            Result.general = generalQuestion;
            Result.tabular = tabularQuestions;
            if (Result.general.Count <= 0 && Result.tabular.Count <= 0)
                throw new Exception("No records found");
            return Result;
        }

        public List<GeneralModel> GetQuestion(QuestionRequestModel model, string setId, NeoErpCoreEntity dbContext)
        {
            model.SetType = model.SetType ?? "G";
            //general Questions
            var generalQuery = string.Empty;
            generalQuery = $@"SELECT TO_CHAR(A.QA_CODE) AS QA_CODE, A.QA_TYPE, A.QUESTION,B.QA_TYPE SET_TYPE
                    FROM DIST_QA_MASTER A
                    INNER JOIN  DIST_QA_SET B ON  B.SET_CODE=A.SET_CODE AND B.COMPANY_CODE=A.COMPANY_CODE
                    WHERE A.DELETED_FLAG = 'N'
                    AND B.QA_TYPE = '{model.SetType}'
                     AND B.SET_CODE= '{setId}'
                    AND A.COMPANY_CODE='{model.COMPANY_CODE}'";
            var generalQuestion = dbContext.SqlQuery<GeneralModel>(generalQuery).ToList();
            string AnswerQuery = string.Empty;
            string[] types = { "MCR", "MCC" };
            foreach (var ques in generalQuestion)
            {
                if (types.Contains(ques.QA_TYPE))
                {
                    AnswerQuery = $@"SELECT ANSWERS FROM DIST_QA_DETAIL WHERE QA_CODE='{ques.QA_CODE}' AND COMPANY_CODE='{model.COMPANY_CODE}'";
                    var answers = dbContext.SqlQuery<string>(AnswerQuery).ToList();
                    ques.ANSWERS = answers;
                }
            }
            return generalQuestion.ToList();
        }

        public List<AreaResponseModel> FetchArea(CommonRequestModel model, NeoErpCoreEntity dbContext)
        {
            var AreaQuery = $@"SELECT A.AREA_CODE,A.AREA_NAME,A.ZONE_CODE,A.DISTRICT_CODE,A.VDC_CODE,A.REG_CODE,
                       B.ZONE_NAME,B.DISTRICT_NAME,B.VDC_NAME,B.REG_NAME
                FROM DIST_AREA_MASTER A,DIST_ADDRESS_MASTER B
                WHERE (A.DISTRICT_CODE = B.DISTRICT_CODE
                AND  A.REG_CODE = B.REG_CODE
                AND  A.ZONE_CODE = B.ZONE_CODE
                AND  A.VDC_CODE = B.VDC_CODE) AND A.COMPANY_CODE = '{model.COMPANY_CODE}' ORDER BY UPPER(A.AREA_NAME) ASC";

            var result = dbContext.SqlQuery<AreaResponseModel>(AreaQuery).ToList();
            if (result.Count <= 0)
                throw new Exception("No records found");
            return result;
        }

        public Dictionary<string, OutletResponseModel> FetchOutlets(CommonRequestModel model, NeoErpCoreEntity dbContext)
        {
            var result = new Dictionary<string, OutletResponseModel>();
            string OutletsQuery = $@"SELECT TO_CHAR(T.TYPE_ID) AS TYPE_ID, T.TYPE_CODE, T.TYPE_EDESC, T.DELETED_FLAG, TO_CHAR(NVL(ST.SUBTYPE_ID,0)) AS SUBTYPE_ID, ST.SUBTYPE_CODE, ST.SUBTYPE_EDESC FROM DIST_OUTLET_TYPE T
                LEFT JOIN DIST_OUTLET_SUBTYPE ST ON ST.TYPE_ID = T.TYPE_ID AND ST.COMPANY_CODE = T.COMPANY_CODE
                WHERE T.COMPANY_CODE='{model.COMPANY_CODE}' and T.DELETED_FLAG='N' AND ST.DELETED_FLAG='N'
                ORDER BY UPPER(T.TYPE_EDESC), ST.SUBTYPE_EDESC ASC";
            var tempOutlets = dbContext.SqlQuery<OutletTemp>(OutletsQuery).GroupBy(x => x.TYPE_ID);
            if (tempOutlets.Count() <= 0)
                throw new Exception("No records found");
            foreach (var typeGroup in tempOutlets)
            {
                var outlet = new OutletResponseModel();
                foreach (var subType in typeGroup)
                {
                    var SubObj = new SubTypeModel
                    {
                        SUBTYPE_ID = subType.SUBTYPE_ID,
                        SUBTYPE_CODE = subType.SUBTYPE_CODE,
                        SUBTYPE_EDESC = subType.SUBTYPE_EDESC,
                        TYPE_ID = typeGroup.Key
                    };
                    outlet.SIZE.Add(subType.SUBTYPE_ID, SubObj);
                }
                var typeObjTemp = typeGroup.FirstOrDefault();
                outlet.TYPE_ID = typeObjTemp.TYPE_ID;
                outlet.TYPE_CODE = typeObjTemp.TYPE_CODE;
                outlet.TYPE_EDESC = typeObjTemp.TYPE_EDESC;
                outlet.DELETED_FLAG = typeObjTemp.DELETED_FLAG;

                result.Add(typeGroup.Key, outlet);
            }
            return result;
        }
        public List<UpdateResponseModel> FetchUpdates(CommonRequestModel model, NeoErpCoreEntity dbContext)
        {
            string updateQuery = $@"SELECT ID,CODE,TYPE_EDESC,DELETED_FLAG FROM DIST_UPDATE_TYPE WHERE COMPANY_CODE='{model.COMPANY_CODE}' AND DELETED_FLAG='N' ORDER BY UPPER(TYPE_EDESC) ASC";
            var result = dbContext.SqlQuery<UpdateResponseModel>(updateQuery).ToList();
            if (result.Count <= 0)
                throw new Exception("No records found");
            return result;
        }
        public List<ComplainTypeModel> FetchComplainTypes(CommonRequestModel model, NeoErpCoreEntity dbContext)
        {
            string updateQuery = $@"SELECT ID,COMPLAIN_CODE,COMPLAIN_TYPE,COMPANY_CODE FROM dist_complain_type WHERE COMPANY_CODE='{model.COMPANY_CODE}' AND DELETED_FLAG='N' ORDER BY UPPER(COMPLAIN_TYPE) ASC";
            var result = dbContext.SqlQuery<ComplainTypeModel>(updateQuery).ToList();
            if (result.Count <= 0)
                throw new Exception("No records found");
            return result;
        }
        public List<SeriousessTypeModel> FetchSeriousnessTypes(CommonRequestModel model, NeoErpCoreEntity dbContext)
        {
            string updateQuery = $@"SELECT ID,CODE,SERIOUSNESS_TYPE,COMPANY_CODE FROM dist_seriousness_type WHERE COMPANY_CODE='{model.COMPANY_CODE}' AND DELETED_FLAG='N' ORDER BY UPPER(SERIOUSNESS_TYPE) ASC";
            var result = dbContext.SqlQuery<SeriousessTypeModel>(updateQuery).ToList();
            if (result.Count <= 0)
                throw new Exception("No records found");
            return result;
        }
        public List<ReturnConditionModel> FetchReturnCondition(CommonRequestModel model, NeoErpCoreEntity dbContext)
        {
            string updateQuery = $@"SELECT ID,CODE,RETURN_TYPE,COMPANY_CODE FROM dist_return_condition WHERE COMPANY_CODE='{model.COMPANY_CODE}' AND DELETED_FLAG='N' ORDER BY UPPER(RETURN_TYPE) ASC";
            var result = dbContext.SqlQuery<ReturnConditionModel>(updateQuery).ToList();
            if (result.Count <= 0)
                throw new Exception("No records found");
            return result;
        }
        public List<DATERANGE> FetchDateRange(CommonRequestModel model, NeoErpCoreEntity dbContext)
        {
            string query = $@"select RANGENAME,TO_CHAR(STARTDATE, 'DD-MON-YYYY') as STARTDATE,TO_CHAR(ENDDATE, 'DD-MON-YYYY') as ENDDATE from V_DATE_RANGE order by sortorder";
            var result = dbContext.SqlQuery<DATERANGE>(query).ToList();
            if (result.Count <= 0)
                throw new Exception("No records found");
            return result;
        }
        public List<Priority> FetchPriorityData(CommonRequestModel model, NeoErpCoreEntity dbContext)
        {
            string updateQuery = $@"Select
                        COALESCE(PRIORITY_CODE,' ') as PRIORITY_CODE, 
                        COALESCE(PRIORITY_EDESC,' ') as PRIORITY_EDESC,
                        COMPANY_CODE as COMPANY_CODE 
                        from IP_PRIORITY_CODE
                        where DELETED_FLAG='N' and COMPANY_CODE='{model.COMPANY_CODE}'  ORDER BY UPPER(PRIORITY_EDESC) ASC";
            var result = dbContext.SqlQuery<Priority>(updateQuery).ToList();
            if (result.Count <= 0)
                throw new Exception("No records found");
            return result;
        }
        public ClosingStockResponseModel GetEntityItemByBrand(ClosingStockRequestModel model, NeoErpCoreEntity dbContext)
        {
            ClosingStockResponseModel result = new ClosingStockResponseModel();
            var ItemsQuery = string.Empty;
            if (model.entity_type.ToUpper() == "R" || model.entity_type.ToUpper() == "RESELLER")
            {
                ItemsQuery = $@"SELECT A.BRAND_NAME,
                  B.ITEM_CODE,
                  B.ITEM_EDESC,
                  B.INDEX_MU_CODE,
                  CONCAT(CONCAT(NVL(C.LVS, 0), ' '), NVL(B.INDEX_MU_CODE, C.MU_CODE)) AS LVS
                FROM IP_ITEM_SPEC_SETUP A
                  INNER JOIN IP_ITEM_MASTER_SETUP B
                    ON B.ITEM_CODE = A.ITEM_CODE
                       AND B.CATEGORY_CODE = '{CATEGORY_CODE}'
                       AND B.GROUP_SKU_FLAG = '{GROUP_SKU_FLAG}'
                       AND B.DELETED_FLAG = A.DELETED_FLAG
                       AND A.COMPANY_CODE = B.COMPANY_CODE
                  LEFT JOIN (
                    SELECT TO_CHAR(T.CREATED_DATE, 'DD-MON-RRRR') CREATED_DATE, T.RESELLER_CODE, T.ITEM_CODE, T.MU_CODE, T.CURRENT_STOCK AS LVS
                    FROM DIST_RESELLER_STOCK T
                    WHERE T.RESELLER_CODE='{model.entity_code}'
                          AND T.CREATED_DATE = (SELECT MAX(CREATED_DATE) FROM DIST_RESELLER_STOCK WHERE RESELLER_CODE='{model.entity_code}' AND DELETED_FLAG='N' AND COMPANY_CODE='{model.COMPANY_CODE}')
                    ) C
                      ON C.ITEM_CODE = B.ITEM_CODE
                WHERE TRIM(A.BRAND_NAME) IS NOT NULL
                      AND A.COMPANY_CODE = '{model.COMPANY_CODE}'
                      AND A.DELETED_FLAG = 'N'
                ORDER BY UPPER(A.BRAND_NAME), UPPER(B.ITEM_EDESC) ASC";
            }
            else if (model.entity_type.ToUpper() == "D" || model.entity_type.ToUpper() == "DISTRIBUTOR")
            {
                ItemsQuery = $@"SELECT A.BRAND_NAME,
                  B.ITEM_CODE,
                  B.ITEM_EDESC,
                  B.INDEX_MU_CODE,
                  CONCAT(CONCAT(NVL(C.LVS, 0), ' '), NVL(B.INDEX_MU_CODE, C.MU_CODE)) AS LVS
                FROM IP_ITEM_SPEC_SETUP A
                  INNER JOIN IP_ITEM_MASTER_SETUP B
                    ON A.ITEM_CODE = B.ITEM_CODE
                       AND B.CATEGORY_CODE = '{CATEGORY_CODE}'
                       AND B.GROUP_SKU_FLAG = '{GROUP_SKU_FLAG}'
                       AND B.DELETED_FLAG = A.DELETED_FLAG
                       AND A.COMPANY_CODE = B.COMPANY_CODE
                  LEFT JOIN (
                    SELECT TO_CHAR(T.CREATED_DATE, 'DD-MON-RRRR') CREATED_DATE, T.DISTRIBUTOR_CODE, T.ITEM_CODE, T.MU_CODE, T.CURRENT_STOCK AS LVS
                    FROM DIST_DISTRIBUTOR_STOCK T
                    WHERE T.DISTRIBUTOR_CODE='{model.entity_type}'
                          AND T.CREATED_DATE = (SELECT MAX(CREATED_DATE) FROM DIST_DISTRIBUTOR_STOCK WHERE DISTRIBUTOR_CODE='{model.entity_type}' AND DELETED_FLAG='N' AND COMPANY_CODE='{model.COMPANY_CODE}')
                    ) C
                      ON C.ITEM_CODE = B.ITEM_CODE
                WHERE TRIM(A.BRAND_NAME) IS NOT NULL
                      AND A.COMPANY_CODE = '{model.COMPANY_CODE}'
                      AND A.DELETED_FLAG = 'N'
                      --AND UPPER(A.BRAND_NAME) = 'SUJI'
                ORDER BY UPPER(A.BRAND_NAME), UPPER(B.ITEM_EDESC) ASC";
            }
            else
                throw new Exception("Entity type not specified");
            var items = dbContext.SqlQuery<ClosingStockItemModel>(ItemsQuery).GroupBy(x => x.BRAND_NAME);

            foreach (var brand in items)
            {
                var temp = new Dictionary<string, ClosingStockItemModel>();
                foreach (var item in brand)
                {
                    temp.Add(item.ITEM_CODE, item);
                }
                result.item.Add(brand.Key, temp);
            }
            if (result.item.Count <= 0)
                throw new Exception("No records found");
            result.mu_code = this.FetchMU(model, dbContext);
            return result;
        }
        public Dictionary<string, Dictionary<string, MuCodeResponseModel>> FetchMU(VisitPlanRequestModel model, NeoErpCoreEntity dbContext)
        {
            Dictionary<string, MuCodeResponseModel> result = new Dictionary<string, MuCodeResponseModel>();
            var pref = FetchPreferences(model.COMPANY_CODE, dbContext);
            var conversionClause = "";
            if ("Y" == pref.SQL_NN_CONVERSION_UNIT_FACTOR)
            {
                conversionClause = "AND IUS.MU_CODE IS NOT NULL AND IUS.CONVERSION_FACTOR IS NOT NULL";
            }
            string MuQuery = $@"SELECT IMS.ITEM_CODE, IMS.ITEM_EDESC, IMS.INDEX_MU_CODE, IUS.MU_CODE, TO_CHAR(IUS.CONVERSION_FACTOR) AS CONVERSION_FACTOR
                FROM IP_ITEM_MASTER_SETUP IMS 
                LEFT JOIN IP_ITEM_UNIT_SETUP IUS
                ON IUS.ITEM_CODE = IMS.ITEM_CODE AND IUS.COMPANY_CODE = IMS.COMPANY_CODE
                LEFT JOIN DIST_USER_ITEM_MAPPING duim ON duim.item_code = ius.item_code AND duim.company_code = ius.company_code 
                WHERE 1 = 1
                AND IMS.COMPANY_CODE = '{model.COMPANY_CODE}'
                AND IMS.CATEGORY_CODE IN ('FG', 'HA')
                AND IMS.GROUP_SKU_FLAG = 'I' and duim.sp_code='{model.spcode}' and duim.deleted_flag='N'
                AND IMS.DELETED_FLAG = 'N'
                {conversionClause}
                ORDER BY UPPER(IMS.ITEM_EDESC), UPPER(IMS.INDEX_MU_CODE), UPPER(MU_CODE)";
            var allMu = dbContext.SqlQuery<MuCodeResponseModel>(MuQuery);
            foreach (var Mu in allMu)
            {
                if (Mu.MU_CODE != null)
                    Mu.CONVERSION_UNIT_FACTOR.Add(Mu.MU_CODE, Mu.CONVERSION_FACTOR);
                result.Add(Mu.ITEM_CODE, Mu);
            }
            var finalResult = new Dictionary<string, Dictionary<string, MuCodeResponseModel>>();
            finalResult.Add("UNIT", result);
            return finalResult;
        }

        public Dictionary<string, Dictionary<string, MuCodeResponseModel>> FetchMU(CommonRequestModel model, NeoErpCoreEntity dbContext)
        {
            Dictionary<string, MuCodeResponseModel> result = new Dictionary<string, MuCodeResponseModel>();
            var pref = FetchPreferences(model.COMPANY_CODE, dbContext);
            var conversionClause = "";
            if ("Y" == pref.SQL_NN_CONVERSION_UNIT_FACTOR)
            {
                conversionClause = "AND IUS.MU_CODE IS NOT NULL AND IUS.CONVERSION_FACTOR IS NOT NULL";
            }
            string MuQuery = $@"SELECT IMS.ITEM_CODE, IMS.ITEM_EDESC, IMS.INDEX_MU_CODE, IUS.MU_CODE, TO_CHAR(IUS.CONVERSION_FACTOR) AS CONVERSION_FACTOR
                FROM IP_ITEM_MASTER_SETUP IMS 
                LEFT JOIN IP_ITEM_UNIT_SETUP IUS
                ON IUS.ITEM_CODE = IMS.ITEM_CODE AND IUS.COMPANY_CODE = IMS.COMPANY_CODE
                WHERE 1 = 1
                AND IMS.COMPANY_CODE = '{model.COMPANY_CODE}'
                AND IMS.CATEGORY_CODE IN ('FG', 'HA')
                AND IMS.GROUP_SKU_FLAG = 'I' 
                AND IMS.DELETED_FLAG = 'N'
                {conversionClause}
                ORDER BY UPPER(IMS.ITEM_EDESC), UPPER(IMS.INDEX_MU_CODE), UPPER(MU_CODE)";
            var allMu = dbContext.SqlQuery<MuCodeResponseModel>(MuQuery);
            foreach (var Mu in allMu)
            {
                if (Mu.MU_CODE != null)
                    Mu.CONVERSION_UNIT_FACTOR.Add(Mu.MU_CODE, Mu.CONVERSION_FACTOR);
                result.Add(Mu.ITEM_CODE, Mu);
            }
            var finalResult = new Dictionary<string, Dictionary<string, MuCodeResponseModel>>();
            finalResult.Add("UNIT", result);
            return finalResult;
        }

        public List<TransactionResponseModel> FetchTransactions(TransactionRequestModel model, NeoErpCoreEntity dbContext)
        {
            var result = new List<TransactionResponseModel>();
            DateTime fromDate, toDate;
            if (!DateTime.TryParse(model.from_date, out fromDate) || !DateTime.TryParse(model.to_date, out toDate))
                throw new Exception("Invalaid Date");

            //opening balance
            string OpeningQuery = string.Empty;
            string NonOpeningQuery = string.Empty;
            if (!string.IsNullOrWhiteSpace(model.acc_code))
            {
                //old Query
                //OpeningQuery = $@"SELECT VGL.VOUCHER_NO, TO_CHAR(VGL.VOUCHER_DATE, 'DD-MON-YYYY') VOUCHER_DATE,
                //    TO_CHAR(NVL(VGL.DR_AMOUNT, 0)) DR_AMOUNT,TO_CHAR(NVL(VGL.CR_AMOUNT, 0)) CR_AMOUNT, VGL.TRANSACTION_TYPE
                //    FROM V$VIRTUAL_GENERAL_LEDGER1 VGL
                //    WHERE 1=1
                //    AND VGL.ACC_CODE = '{model.acc_code}'
                //    AND VGL.VOUCHER_DATE < '{fromDate.ToString("dd-MMM-yyyy")}'
                //    AND VGL.COMPANY_CODE = '{model.COMPANY_CODE}'
                //    ORDER BY VGL.VOUCHER_DATE, UPPER(VGL.VOUCHER_NO) ASC";
                OpeningQuery = $@"SELECT VGL.VOUCHER_NO, TO_CHAR(VGL.VOUCHER_DATE, 'DD-MON-RRRR') VOUCHER_DATE, VGL.PARTICULARS, TO_CHAR(NVL(VGL.DR_AMOUNT, 0)) DR_AMOUNT, TO_CHAR(NVL(VGL.CR_AMOUNT, 0)) CR_AMOUNT, VGL.TRANSACTION_TYPE
                    FROM V$VIRTUAL_GENERAL_LEDGER VGL
                    WHERE 1=1
                    AND TRIM(VGL.ACC_CODE) = TRIM('{model.acc_code}')
                    AND VGL.VOUCHER_DATE < '{fromDate.ToString("dd-MMM-yyyy")}'
                    AND VGL.COMPANY_CODE = '{model.COMPANY_CODE}'
                    ORDER BY VGL.VOUCHER_DATE, UPPER(VGL.VOUCHER_NO) ASC";

                //NonOpeningQuery = $@"SELECT VGL.VOUCHER_NO, TO_CHAR(VGL.VOUCHER_DATE, 'DD-MON-RRRR') VOUCHER_DATE, VGL.PARTICULARS,
                //    TO_CHAR(NVL(VGL.DR_AMOUNT, 0)) DR_AMOUNT, TO_CHAR(NVL(VGL.CR_AMOUNT, 0)) CR_AMOUNT, VGL.TRANSACTION_TYPE
                //    FROM V$VIRTUAL_GENERAL_LEDGER1 VGL
                //    WHERE 1=1
                //    AND VGL.ACC_CODE = '{model.acc_code}'
                //    AND VGL.VOUCHER_DATE BETWEEN '{fromDate.ToString("dd-MMM-yyyy")}' AND '{toDate.ToString("dd-MMM-yyyy")}'
                //    AND VGL.COMPANY_CODE = '{model.COMPANY_CODE}'
                //    ORDER BY VGL.VOUCHER_DATE, UPPER(VGL.VOUCHER_NO) ASC";
                NonOpeningQuery = $@"SELECT VGL.VOUCHER_NO, TO_CHAR(VGL.VOUCHER_DATE, 'DD-MON-RRRR') VOUCHER_DATE, VGL.PARTICULARS, TO_CHAR(NVL(VGL.DR_AMOUNT, 0)) DR_AMOUNT, TO_CHAR(NVL(VGL.CR_AMOUNT, 0)) CR_AMOUNT, VGL.TRANSACTION_TYPE
                    FROM V$VIRTUAL_GENERAL_LEDGER VGL
                    WHERE 1=1
                    AND TRIM(VGL.ACC_CODE) = TRIM('{model.acc_code}')
                    AND VGL.VOUCHER_DATE BETWEEN '{fromDate.ToString("dd-MMM-yyyy")}' AND '{toDate.ToString("dd-MMM-yyyy")}'
                    AND VGL.COMPANY_CODE = '{model.COMPANY_CODE}'
                    ORDER BY VGL.VOUCHER_DATE, UPPER(VGL.VOUCHER_NO) ASC";
            }
            else
            {
                //OpeningQuery = $@"SELECT VSL.VOUCHER_NO, TO_CHAR(VSL.VOUCHER_DATE, 'DD-MON-RRRR') VOUCHER_DATE, VSL.PARTICULARS,
                //    TO_CHAR(NVL(VSL.DR_AMOUNT, 0)) DR_AMOUNT, TO_CHAR(NVL(VSL.CR_AMOUNT, 0)) CR_AMOUNT, VSL.TRANSACTION_TYPE
                //    FROM V$VIRTUAL_SUB_LEDGER VSL
                //    WHERE 1=1
                //    AND VSL.SUB_CODE = '{model.sub_code}'
                //    AND VSL.VOUCHER_DATE < '{fromDate.ToString("dd-MMM-yyyy")}'
                //    AND VSL.COMPANY_CODE = '{model.COMPANY_CODE}'
                //    ORDER BY VSL.VOUCHER_DATE, UPPER(VSL.VOUCHER_NO) ASC";
                OpeningQuery = $@"SELECT VSL.VOUCHER_NO, TO_CHAR(VSL.VOUCHER_DATE, 'DD-MON-RRRR') VOUCHER_DATE, VSL.PARTICULARS, TO_CHAR(NVL(VSL.DR_AMOUNT, 0)) DR_AMOUNT, TO_CHAR(NVL(VSL.CR_AMOUNT, 0)) CR_AMOUNT, VSL.TRANSACTION_TYPE
                    FROM V$VIRTUAL_SUB_LEDGER VSL
                    WHERE 1 = 1
                    AND TRIM(VSL.SUB_CODE) = TRIM('{model.sub_code}')
                    AND VSL.VOUCHER_DATE < '{fromDate.ToString("dd-MMM-yyyy")}'
                    AND VSL.COMPANY_CODE = '{model.COMPANY_CODE}'
                    ORDER BY VSL.VOUCHER_DATE, UPPER(VSL.VOUCHER_NO) ASC";

                //NonOpeningQuery = $@"SELECT VSL.VOUCHER_NO, TO_CHAR(VSL.VOUCHER_DATE, 'DD-MON-RRRR') VOUCHER_DATE, VSL.PARTICULARS,
                //    TO_CHAR(NVL(VSL.DR_AMOUNT, 0)) DR_AMOUNT, TO_CHAR(NVL(VSL.CR_AMOUNT, 0)) CR_AMOUNT, VSL.TRANSACTION_TYPE
                //    FROM V$VIRTUAL_SUB_LEDGER VSL
                //    WHERE 1=1
                //    AND VSL.SUB_CODE = '{model.sub_code}'
                //    AND VSL.VOUCHER_DATE BETWEEN '{fromDate.ToString("dd-MMM-yyyy")}' AND '{toDate.ToString("dd-MMM-yyyy")}'
                //    AND VSL.COMPANY_CODE = '{model.COMPANY_CODE}'
                //    ORDER BY VSL.VOUCHER_DATE, UPPER(VSL.VOUCHER_NO) ASC";

                NonOpeningQuery = $@"SELECT VSL.VOUCHER_NO, TO_CHAR(VSL.VOUCHER_DATE, 'DD-MON-RRRR') VOUCHER_DATE, VSL.PARTICULARS, TO_CHAR(NVL(VSL.DR_AMOUNT, 0)) DR_AMOUNT, TO_CHAR(NVL(VSL.CR_AMOUNT, 0)) CR_AMOUNT, VSL.TRANSACTION_TYPE
                    FROM V\$VIRTUAL_SUB_LEDGER VSL
                    WHERE 1=1
                    AND TRIM(VSL.SUB_CODE) = TRIM('{model.sub_code}')
                    AND VSL.VOUCHER_DATE BETWEEN '{fromDate.ToString("dd-MMM-yyyy")}' AND '{toDate.ToString("dd-MMM-yyyy")}'
                    AND VSL.COMPANY_CODE = '{model.COMPANY_CODE}'
                    ORDER BY VSL.VOUCHER_DATE, UPPER(VSL.VOUCHER_NO) ASC";
            }
            var openingData = dbContext.SqlQuery<TransactionResponseModel>(OpeningQuery).ToList();
            decimal OpeningBalance = 0;
            foreach (var item in openingData)
            {
                decimal amount;
                if (item.TRANSACTION_TYPE == "DR")
                {
                    decimal.TryParse(item.DR_AMOUNT, out amount);
                    OpeningBalance += amount;
                }
                else
                {
                    decimal.TryParse(item.CR_AMOUNT, out amount);
                    OpeningBalance -= amount;
                }
            }
            var OpeningObject = new TransactionResponseModel
            {
                VOUCHER_NO = "0",
                PARTICULARS = "Opening Balance",
                VOUCHER_DATE = fromDate.ToString("dd-MMM-yyyy")
            };
            if (OpeningBalance >= 0)
            {
                OpeningObject.DR_AMOUNT = OpeningBalance.ToString();
                OpeningObject.CR_AMOUNT = "0";
                OpeningObject.TRANSACTION_TYPE = "DR";
            }
            else
            {
                OpeningObject.CR_AMOUNT = (-1 * OpeningBalance).ToString();
                OpeningObject.DR_AMOUNT = "0";
                OpeningObject.TRANSACTION_TYPE = "CR";
            }

            //non opening balance
            result = dbContext.SqlQuery<TransactionResponseModel>(NonOpeningQuery).ToList();
            var itemsToRemove = new List<TransactionResponseModel>();
            OpeningBalance = 0;
            bool hasOpening = false;
            foreach (var item in result)
            {
                if (item.VOUCHER_NO == "0")
                {
                    decimal amount;
                    if (item.TRANSACTION_TYPE == "DR")
                    {
                        decimal.TryParse(item.DR_AMOUNT, out amount);
                        OpeningBalance += amount;
                    }
                    else
                    {
                        decimal.TryParse(item.CR_AMOUNT, out amount);
                        OpeningBalance -= amount;
                    }
                    hasOpening = true;
                    itemsToRemove.Add(item);
                }
            }
            foreach (var item in itemsToRemove)
                result.Remove(item);
            var OpeningObject2 = new TransactionResponseModel
            {
                VOUCHER_NO = "0",
                PARTICULARS = "Opening Balance",
                VOUCHER_DATE = fromDate.ToString("dd-MMM-yyyy")
            };
            if (OpeningBalance >= 0)
            {
                OpeningObject2.DR_AMOUNT = OpeningBalance.ToString();
                OpeningObject2.CR_AMOUNT = "0";
                OpeningObject2.TRANSACTION_TYPE = "DR";
            }
            else
            {
                OpeningObject2.CR_AMOUNT = (-1 * OpeningBalance).ToString();
                OpeningObject2.DR_AMOUNT = "0";
                OpeningObject2.TRANSACTION_TYPE = "CR";
            }
            if (openingData.Count != 0)
                result.Insert(0, OpeningObject);
            if (hasOpening)
                result.Insert(0, OpeningObject2);
            if (result.Count <= 0)
                throw new Exception("No records found");
            return result;
        }

        public Dictionary<string, List<PurchaseOrderResponseModel>> FetchPurchaseOrder(PurchaseOrderRequestModel model, NeoErpCoreEntity dbContext)
        {
            var result = new Dictionary<string, List<PurchaseOrderResponseModel>>();
            string PurchaseQuery = string.Empty;
            string DateQuery = string.Empty;

            DateQuery = $@"select FY_START_DATE from PREFERENCE_SETUP where company_code = '{model.COMPANY_CODE}'";
            var startDate = dbContext.SqlQuery<DateTime>(DateQuery).FirstOrDefault();
            var endDate = DateTime.Now;
            if (model.type.Equals("distributor", StringComparison.OrdinalIgnoreCase))
                PurchaseQuery = $@"select a.distributor_code as code,TO_CHAR(c.order_no) order_no, to_char(c.order_date, 'DD-MON-YYYY') as order_date, c.item_code, b.item_edesc, c.mu_code,
             TO_CHAR(c.quantity) quantity, TO_CHAR(c.unit_price) unit_price,TO_CHAR(c.total_price) total_price, c.remarks, c.approved_flag, c.dispatch_flag, c.acknowledge_flag,
             c.reject_flag, TO_CHAR(NVL(IR.SALES_RATE, 0)) SALES_RATE, To_CHAR(IR.APPLY_DATE) APPLY_DATE
                    from dist_distributor_master a, ip_item_master_setup b, dist_ip_ssd_purchase_order c,
                    (SELECT A.ITEM_CODE, A.APPLY_DATE, B.SALES_RATE, B.COMPANY_CODE
                              FROM (SELECT ITEM_CODE, COMPANY_CODE, MAX(APP_DATE) APPLY_DATE 
                                FROM IP_ITEM_RATE_APPLICAT_SETUP
                                WHERE COMPANY_CODE = '{model.COMPANY_CODE}' 
                                AND BRANCH_CODE = '{model.BRANCH_CODE}'
                                GROUP BY ITEM_CODE, COMPANY_CODE) A
                              INNER JOIN IP_ITEM_RATE_APPLICAT_SETUP B
                                ON B.ITEM_CODE = A.ITEM_CODE
                                AND B.APP_DATE = A.APPLY_DATE
                                AND B.COMPANY_CODE = '{model.COMPANY_CODE}'
                                AND B.BRANCH_CODE = '{model.BRANCH_CODE}') IR
                    where a.distributor_code = '{model.code}'
                    and a.distributor_code = c.customer_code
                    and b.item_code = c.item_code
                    and b.group_sku_flag='{GROUP_SKU_FLAG}'
                    and b.category_code='{CATEGORY_CODE}'
                    and c.order_date between '{startDate.ToString("dd-MMM-yyyy")}' and '{endDate.ToString("dd-MMM-yyyy")}'
                    and a.company_code='{model.COMPANY_CODE}'
                    and a.company_code=b.company_code
                    and b.company_code=c.company_code
                    and a.active='Y'
                    and a.deleted_flag='N'
                    AND IR.ITEM_CODE(+) = c.ITEM_CODE AND IR.COMPANY_CODE(+) = c.COMPANY_CODE
                    order by c.order_no DESC";
            else if (model.type.Equals("reseller", StringComparison.OrdinalIgnoreCase))
                PurchaseQuery = $@"select a.distributor_code as code,TO_CHAR(c.order_no) order_no, to_char(c.order_date, 'DD-MON-YYYY') as order_date, c.item_code, b.item_edesc, c.mu_code,
             TO_CHAR(c.quantity) quantity, TO_CHAR(c.unit_price) unit_price,TO_CHAR(c.total_price) total_price, c.remarks, c.approved_flag, c.dispatch_flag, c.acknowledge_flag,
             c.reject_flag, TO_CHAR(NVL(IR.SALES_RATE, 0)) SALES_RATE, To_CHAR(IR.APPLY_DATE) APPLY_DATE
                    from dist_reseller_master a, ip_item_master_setup b, dist_ip_ssr_purchase_order c,
                    (SELECT A.ITEM_CODE, A.APPLY_DATE, B.SALES_RATE, B.COMPANY_CODE
                              FROM (SELECT ITEM_CODE, COMPANY_CODE, MAX(APP_DATE) APPLY_DATE 
                                FROM IP_ITEM_RATE_APPLICAT_SETUP
                                WHERE COMPANY_CODE = '{model.COMPANY_CODE}' 
                                AND BRANCH_CODE = '{model.BRANCH_CODE}'
                                GROUP BY ITEM_CODE, COMPANY_CODE) A
                              INNER JOIN IP_ITEM_RATE_APPLICAT_SETUP B
                                ON B.ITEM_CODE = A.ITEM_CODE
                                AND B.APP_DATE = A.APPLY_DATE
                                AND B.COMPANY_CODE = '{model.COMPANY_CODE}'
                                AND B.BRANCH_CODE = '{model.BRANCH_CODE}') IR
                    where a.reseller_code = '{model.code}'
                    and a.reseller_code = c.reseller_code
                    and b.item_code = c.item_code
                    and b.group_sku_flag='{GROUP_SKU_FLAG}'
                    and b.category_code='{CATEGORY_CODE}'
                    and c.order_date between '{startDate.ToString("dd-MMM-yyyy")}' and '{endDate.ToString("dd-MMM-yyyy")}'
                    and a.company_code='{model.COMPANY_CODE}'
                    and a.company_code=b.company_code
                    and b.company_code=c.company_code
                    AND IR.ITEM_CODE(+) = c.ITEM_CODE AND IR.COMPANY_CODE(+) = c.COMPANY_CODE
                    order by c.order_no DESC";
            else
                throw new Exception("Type Not specified");

            var data = dbContext.SqlQuery<PurchaseOrderResponseModel>(PurchaseQuery).ToList();
            if (data.Count <= 0)
                throw new Exception("No records found");
            foreach (var item in data)
            {
                var list = new List<PurchaseOrderResponseModel>();
                list.Add(item);
                result.Add(item.ORDER_NO, list);
            }
            return result;
        }

        public SalesAgeReportResponseModel SalesAgingReport(ReportRequestModel model, NeoErpCoreEntity dbContext)
        {
            var result = new SalesAgeReportResponseModel();
            result.sales = this.MonthWiseSales(model, dbContext);
            result.age = this.AgingReport(model, dbContext);
            return result;
        }

        public Dictionary<string, string> MonthWiseSales(ReportRequestModel model, NeoErpCoreEntity dbContext)
        {
            if (string.IsNullOrWhiteSpace(model.code) || string.IsNullOrEmpty(model.code))
                throw new Exception("Code cannot be empty");

            var result = new Dictionary<string, string>();
            var SalesQuery = string.Empty;
            SalesQuery = $@"SELECT 
                  UPPER(trim(substr(FN_CHARTBS_MONTH(SUBSTR(BS_DATE(SI.sales_date),6,2)),5,20))) MONTH, 
                  TO_CHAR(NVL(SUM(NVL(SI.CALC_TOTAL_PRICE,0))/1,0)) AS AMOUNT,
                  TO_CHAR(SUM(NVL(SI.CALC_QUANTITY,0))/'1') AS QUANTITY,
                  TO_CHAR(SUBSTR(BS_DATE(SI.SALES_DATE),1,7)) AS MONTHINT
                FROM SA_SALES_INVOICE SI
                WHERE 1 = 1
                  AND  SI.DELETED_FLAG = 'N'
                  AND SI.COMPANY_CODE IN ('{model.COMPANY_CODE}')
                  AND SI.CUSTOMER_CODE = '{model.code}'
              GROUP BY FN_CHARTBS_MONTH(SUBSTR(BS_DATE(SI.SALES_DATE),6,2)),SUBSTR(BS_DATE(SI.SALES_DATE),1,7)
              ORDER BY SUBSTR(BS_DATE(SI.SALES_DATE),1,7)";
            var data = dbContext.SqlQuery<SalesReportResponseModel>(SalesQuery).ToList();
            string[] Months = { "BAISAKH", "JESTHA", "ASHADH", "SHRAWAN", "BHADRA", "ASHOJ", "KARTIK", "MANGSIR", "POUSH", "MAGH", "FALGUN", "CHAITRA" };

            decimal Netsales = 0;
            foreach (var m in Months)
            {
                decimal amount = 0;
                var monthData = data.FirstOrDefault(x => x.MONTH == m);
                if (monthData == null)
                    result.Add(m, "0");
                else
                {
                    decimal.TryParse(monthData.AMOUNT, out amount);
                    Netsales += amount;
                    result.Add(m, monthData.AMOUNT);
                }
            }
            result.Add("NETSALES", Netsales.ToString());
            return result;
        }

        public Dictionary<string, string> AgingReport(ReportRequestModel model, NeoErpCoreEntity dbContext)
        {
            model.code = model.code.FirstOrDefault() == 'C' ? model.code : "C" + model.code;
            var result = new Dictionary<string, string>();
            string AgingQuery = string.Empty;
            var Dates = new List<AgingDateRange>();
            Dates.Add(new AgingDateRange { StartDate = DateTime.Now.AddDays(-30), EndDate = DateTime.Now });
            for (int i = 1; i < 5; i++)
            {
                var date = new AgingDateRange();
                date.StartDate = Dates[i - 1].StartDate.AddDays(-30);
                date.EndDate = Dates[i - 1].StartDate.AddDays(-1);
                Dates.Add(date);
            }
            AgingQuery += "SELECT DISTINCT A.SUB_CODE, ";
            for (int i = 5; i >= 0; i--)
            {
                if (i == 5)
                    AgingQuery += $@"
                    SUM((SELECT SUM(ROUND(NVL(C.CR_AMOUNT,0),2)) FROM V$CUSTOMER_BILLAGE_LEDGER C
                    WHERE C.COMPANY_CODE = A.COMPANY_CODE AND C.BRANCH_CODE = A.BRANCH_CODE AND C.SERIAL_NO = A.SERIAL_NO AND C.SUB_CODE = A.SUB_CODE
                    AND C.ACC_CODE = A.ACC_CODE AND C.FORM_CODE = A.FORM_CODE AND C.VOUCHER_NO = A.VOUCHER_NO
                    AND TO_DATE(TO_CHAR(C.VOUCHER_DATE,'DD-MON-YYYY')) <= TO_DATE('{DateTime.Now.ToString("dd-MMM-yyyy")}')
                    AND C.COMPANY_CODE='{model.COMPANY_CODE}'  AND C.BRANCH_CODE='{model.BRANCH_CODE}' )) CR_AMOUNT, ";
                else
                {
                    if (i == 4)
                        AgingQuery += $@"
                    SUM((SELECT SUM(ROUND(NVL(C.DR_AMOUNT,0),2)) FROM V$CUSTOMER_BILLAGE_LEDGER C WHERE C.COMPANY_CODE = A.COMPANY_CODE
                    AND C.BRANCH_CODE = A.BRANCH_CODE AND C.SERIAL_NO = A.SERIAL_NO AND C.SUB_CODE = A.SUB_CODE AND C.ACC_CODE = A.ACC_CODE
                    AND C.FORM_CODE = A.FORM_CODE AND C.VOUCHER_NO = A.VOUCHER_NO 
                    AND TO_DATE(TO_CHAR(C.VOUCHER_DATE,'DD-MON-YYYY')) <= TO_DATE('{Dates[i].EndDate.ToString("dd-MMM-yyyy")}')
                    AND C.COMPANY_CODE='{model.COMPANY_CODE}' AND C.BRANCH_CODE='{model.BRANCH_CODE}' )) DR_AMOUNT{i}, ";
                    else
                    {
                        AgingQuery += $@"
                    SUM((SELECT SUM(ROUND(NVL(C.DR_AMOUNT,0),2)) FROM V$CUSTOMER_BILLAGE_LEDGER C WHERE C.COMPANY_CODE = A.COMPANY_CODE
                    AND C.BRANCH_CODE = A.BRANCH_CODE AND C.SERIAL_NO = A.SERIAL_NO AND C.SUB_CODE = A.SUB_CODE AND C.ACC_CODE = A.ACC_CODE
                    AND C.FORM_CODE = A.FORM_CODE AND C.VOUCHER_NO = A.VOUCHER_NO AND TO_DATE(TO_CHAR(C.VOUCHER_DATE,'DD-MON-YYYY')) >= TO_DATE('{Dates[i].StartDate.ToString("dd-MMM-yyyy")}')
                    AND TO_DATE(TO_CHAR(C.VOUCHER_DATE,'DD-MON-YYYY')) <= TO_DATE('{Dates[i].EndDate.ToString("dd-MMM-yyyy")}')
                    AND C.COMPANY_CODE='{model.COMPANY_CODE}'  AND C.BRANCH_CODE='{model.BRANCH_CODE}' )) DR_AMOUNT";
                        AgingQuery += i;
                        if (i != 0)
                            AgingQuery += ", ";
                    }
                }
            }
            AgingQuery += $@"
            FROM V$CUSTOMER_BILLAGE_LEDGER A
            WHERE A.DELETED_FLAG='N' AND A.SUB_CODE ='{model.code}'
            AND TO_DATE(TO_CHAR(A.VOUCHER_DATE,'DD-MON-YYYY')) <= TO_DATE('{DateTime.Now.ToString("dd-MMM-yyyy")}')
            GROUP BY A.SUB_CODE";
            var data = dbContext.SqlQuery<AgingReportModel>(AgingQuery).FirstOrDefault();

            result.Add("SUB_CODE", model.code);
            result.Add("120+", "0");
            result.Add("91-120", "0");
            result.Add("61-90", "0");
            result.Add("31-60", "0");
            result.Add("0-30", "0");
            result.Add("total", "0");

            if (data != null)
            {
                result["SUB_CODE"] = data.SUB_CODE;
                decimal total = 0;
                var CRAmount = data.CR_AMOUNT == null ? 0 : data.CR_AMOUNT.Value;
                //DR_AMOUNT4
                if (data.DR_AMOUNT4 != null)
                {
                    if (CRAmount >= data.DR_AMOUNT4)
                        CRAmount = CRAmount - data.DR_AMOUNT4.Value;
                    else
                    {
                        result["120+"] = (data.DR_AMOUNT4 - CRAmount).ToString();
                        total += data.DR_AMOUNT4.Value - CRAmount;
                        CRAmount = 0;
                    }
                }
                //DR_AMOUNT3
                if (data.DR_AMOUNT3 != null)
                {
                    if (CRAmount >= data.DR_AMOUNT3)
                        CRAmount = CRAmount - data.DR_AMOUNT3.Value;
                    else
                    {
                        result["91-120"] = (data.DR_AMOUNT3 - CRAmount).ToString();
                        total += data.DR_AMOUNT3.Value - CRAmount;
                        CRAmount = 0;
                    }
                }
                //DR_AMOUNT2
                if (data.DR_AMOUNT2 != null)
                {
                    if (CRAmount >= data.DR_AMOUNT2)
                        CRAmount = CRAmount - data.DR_AMOUNT2.Value;
                    else
                    {
                        result["61-90"] = (data.DR_AMOUNT2 - CRAmount).ToString();
                        total += data.DR_AMOUNT2.Value - CRAmount;
                        CRAmount = 0;
                    }
                }
                //DR_AMOUNT1
                if (data.DR_AMOUNT1 != null)
                {
                    if (CRAmount >= data.DR_AMOUNT1)
                        CRAmount = CRAmount - data.DR_AMOUNT1.Value;
                    else
                    {
                        result["31-60"] = (data.DR_AMOUNT1 - CRAmount).ToString();
                        total += data.DR_AMOUNT1.Value - CRAmount;
                        CRAmount = 0;
                    }
                }
                //DR_AMOUNT0
                if (data.DR_AMOUNT0 != null)
                {
                    if (CRAmount >= data.DR_AMOUNT0)
                        CRAmount = CRAmount - data.DR_AMOUNT0.Value;
                    else
                    {
                        result["0-30"] = (data.DR_AMOUNT0 - CRAmount).ToString();
                        total += data.DR_AMOUNT0.Value - CRAmount;
                        CRAmount = 0;
                    }
                }
                result["total"] = total.ToString();
            }
            return result;
        }

        public DistributorItemResponseModel FetchEntityPartyTypeAndMu(EntityRequestModel model, NeoErpCoreEntity dbContext)
        {
            var result = new DistributorItemResponseModel();
            model.entity_code = model.entity_code.FirstOrDefault() == 'C' ? model.entity_code : "C" + model.entity_code;
            var PartyQuery = string.Empty;
            switch (model.entity_type.ToUpper())
            {
                case "D":
                case "DISTRIBUTOR":
                    PartyQuery = $@"SELECT 
                            PT.PARTY_TYPE_CODE AS CODE, 
                            PT.PARTY_TYPE_EDESC AS NAME, 
                            PT.ACC_CODE,
                            DM.CONTACT_NO AS P_CONTACT_NO, '' AS P_CONTACT_NAME, DM.REG_OFFICE_ADDRESS AS ADDRESS,
                            RM.ROUTE_CODE, RM.ROUTE_NAME,
                            AM.AREA_CODE, AM.AREA_NAME,
                            NVL(DM.LATITUDE, 0) LATITUDE, NVL(DM.LONGITUDE, 0) LONGITUDE,
                            'dealer' AS TYPE,
                            '' DEFAULT_PARTY_TYPE_CODE,
                            '' PARENT_DISTRIBUTOR_CODE,
                            '' PARENT_DISTRIBUTOR_NAME,
                            LT.LAST_VISIT_DATE, LT.LAST_VISIT_BY, LT.LAST_VISIT_STATUS, NVL(LT.IS_VISITED, 'X') AS IS_VISITED, LT.REMARKS,
                            DM.COMPANY_CODE, DM.BRANCH_CODE
                            FROM IP_PARTY_TYPE_CODE PT
                            LEFT JOIN DIST_DEALER_MASTER DM ON DM.DEALER_CODE = PT.PARTY_TYPE_CODE AND DM.COMPANY_CODE = PT.COMPANY_CODE
                            LEFT JOIN DIST_ROUTE_ENTITY RE ON RE.ENTITY_CODE = DM.DEALER_CODE AND RE.ENTITY_TYPE = 'P' AND RE.COMPANY_CODE = DM.COMPANY_CODE
                            LEFT JOIN DIST_ROUTE_MASTER RM ON RM.ROUTE_CODE = RE.ROUTE_CODE AND RM.COMPANY_CODE = DM.COMPANY_CODE
                            LEFT JOIN DIST_AREA_MASTER AM ON AM.AREA_CODE = DM.AREA_CODE AND AM.COMPANY_CODE = DM.COMPANY_CODE
                            LEFT JOIN (SELECT A.SP_CODE, A.CUSTOMER_CODE, A.CUSTOMER_TYPE, A.COMPANY_CODE, C.EMPLOYEE_EDESC AS LAST_VISIT_BY, A.IS_VISITED AS LAST_VISIT_STATUS,
                                              (CASE 
                                                WHEN A.IS_VISITED IS NULL THEN 'X' 
                                                  ELSE
                                                    CASE WHEN TO_CHAR(SYSDATE, 'DD-MON-RRRR') = TO_CHAR(A.UPDATE_DATE, 'DD-MON-RRRR') THEN A.IS_VISITED ELSE 'X' END
                                               END
                                              ) IS_VISITED, 
                                              A.REMARKS,
                                              TO_CHAR(MAX(A.UPDATE_DATE), 'RRRR-MM-DD HH24:MI A.M.') LAST_VISIT_DATE
                                            FROM DIST_LOCATION_TRACK A
                                              INNER JOIN IP_PARTY_TYPE_CODE B ON B.PARTY_TYPE_CODE = A.CUSTOMER_CODE AND B.COMPANY_CODE = A.COMPANY_CODE
                                              INNER JOIN HR_EMPLOYEE_SETUP C ON C.EMPLOYEE_CODE = A.SP_CODE AND C.COMPANY_CODE = A.COMPANY_CODE
                                            WHERE 1 = 1
                                                  AND A.CUSTOMER_TYPE = 'P'
                                                  AND A.UPDATE_DATE = (SELECT MAX(UPDATE_DATE) FROM DIST_LOCATION_TRACK WHERE CUSTOMER_CODE = A.CUSTOMER_CODE AND CUSTOMER_TYPE = A.CUSTOMER_TYPE)
                                            GROUP BY A.SP_CODE, A.CUSTOMER_CODE, A.CUSTOMER_TYPE, A.COMPANY_CODE, C.EMPLOYEE_EDESC, A.IS_VISITED, 
                                              (CASE 
                                                WHEN A.IS_VISITED IS NULL THEN 'X' 
                                                  ELSE
                                                    CASE WHEN TO_CHAR(SYSDATE, 'DD-MON-RRRR') = TO_CHAR(A.UPDATE_DATE, 'DD-MON-RRRR') THEN A.IS_VISITED ELSE 'X' END
                                               END
                                              ), 
                                              A.REMARKS
                                           ) LT
                                   ON LT.CUSTOMER_CODE = DM.DEALER_CODE AND LT.COMPANY_CODE = DM.COMPANY_CODE
                            WHERE TRIM(PT.ACC_CODE) IN (SELECT TRIM(ACC_CODE) FROM FA_SUB_LEDGER_MAP WHERE COMPANY_CODE = '{model.COMPANY_CODE}' AND SUB_CODE = TRIM('{model.entity_code}'))
                              AND PT.COMPANY_CODE = '{model.COMPANY_CODE}'
                              AND PT.DELETED_FLAG = 'N'
                            ORDER BY UPPER(PT.PARTY_TYPE_EDESC)";
                    break;
            }

            if (!string.IsNullOrWhiteSpace(PartyQuery))
            {
                var allParty = dbContext.SqlQuery<EntityResponseModel>(PartyQuery).ToList();
                if (allParty.Count > 0)
                {
                    foreach (var party in allParty)
                    {
                        result.PARTY.Add(party.CODE, party);
                    }
                }
            }

            //preferences
            var pref = this.FetchPreferences(model.COMPANY_CODE, dbContext);
            string salesRateClause = string.Empty;
            string conversionClause = string.Empty;
            if (pref.PO_SYN_RATE.Trim().ToUpper() == "Y")
                salesRateClause = "AND SALES_RATE IS NOT NULL AND SALES_RATE <> 0";
            if (pref.SQL_NN_CONVERSION_UNIT_FACTOR.Trim().ToUpper() == "Y")
                conversionClause = "AND IUS.MU_CODE IS NOT NULL AND IUS.CONVERSION_FACTOR IS NOT NULL";

            //Unit
            Dictionary<string, MuCodeResponseModel> MuResult = new Dictionary<string, MuCodeResponseModel>();
            string MuQuery = $@"SELECT IMS.ITEM_CODE, IMS.ITEM_EDESC, IMS.INDEX_MU_CODE, IUS.MU_CODE, IUS.CONVERSION_FACTOR, IMS.COMPANY_CODE
                FROM IP_ITEM_MASTER_SETUP IMS 
                LEFT JOIN IP_ITEM_UNIT_SETUP IUS
                ON IUS.ITEM_CODE = IMS.ITEM_CODE AND IUS.COMPANY_CODE = IMS.COMPANY_CODE
                WHERE 1 = 1
                AND IMS.COMPANY_CODE IN (SELECT COMPANY_CODE FROM COMPANY_SETUP)
                AND IMS.CATEGORY_CODE = '{CATEGORY_CODE}'
                AND IMS.GROUP_SKU_FLAG = '{GROUP_SKU_FLAG}'
                AND IMS.DELETED_FLAG = 'N'
                {conversionClause}
                ORDER BY IMS.COMPANY_CODE, UPPER(IMS.ITEM_EDESC), UPPER(IMS.INDEX_MU_CODE), UPPER(MU_CODE)";
            var allMu = dbContext.SqlQuery<MuCodeResponseModel>(MuQuery).GroupBy(x => x.COMPANY_CODE);
            foreach (var Mu in allMu)
            {
                var tempResult = new Dictionary<string, MuCodeResponseModel>();
                var CompanyMu = Mu.ToList();
                foreach (var CM in CompanyMu)
                {
                    if (CM.MU_CODE != null)
                        CM.CONVERSION_UNIT_FACTOR.Add(CM.MU_CODE, CM.CONVERSION_FACTOR);
                    tempResult.Add(CM.ITEM_CODE, CM);
                }
                result.UNIT[Mu.Key] = tempResult;
            }

            //sales types
            var salesTypes = FetchAllCompanySaSalesType(model.COMPANY_CODE, dbContext, pref).GroupBy(x => x.COMPANY_CODE);
            foreach (var type in salesTypes)
            {
                result.SALES_TYPE.Add(type.Key, type.ToList());
            }
            result.SHIPPING_ADDRESS = FetchShippingAddress(dbContext, pref); //shipping addresses

            return result;
        }

        public Dictionary<string, List<EntityResponseModel>> FetchPartyTypeBillingEntity(EntityRequestModel model, NeoErpCoreEntity dbContext)
        {
            if (string.IsNullOrWhiteSpace(model.COMPANY_CODE))
                throw new Exception("Company code not found!!!");
            if (string.IsNullOrWhiteSpace(model.ACC_CODE))
                throw new Exception("Account code not found!!!");

            string Query = $@"SELECT CS.CUSTOMER_CODE AS CODE,
                  CS.CUSTOMER_EDESC AS NAME,
                  CS.ACC_CODE AS ACC_CODE,
                  CS.PARTY_TYPE_CODE AS DEFAULT_PARTY_TYPE_CODE,
                  DM.CONTACT_NO AS P_CONTACT_NO, '' AS P_CONTACT_NAME, NVL(DM.REG_OFFICE_ADDRESS, CS.REGD_OFFICE_EADDRESS) AS ADDRESS,
                  AM.AREA_CODE, AM.AREA_NAME,
                  NVL(DM.LATITUDE, '0') AS LATITUDE,
                  NVL(DM.LONGITUDE, '0') AS LONGITUDE,
                  'distributor' AS TYPE,
                   '' AS PARENT_DISTRIBUTOR_CODE,
                   '' AS PARENT_DISTRIBUTOR_NAME,
                   LT.LAST_VISIT_DATE, LT.LAST_VISIT_BY, LT.LAST_VISIT_STATUS, NVL(LT.IS_VISITED, 'X') AS IS_VISITED, LT.REMARKS,
                   CS.COMPANY_CODE, CS.BRANCH_CODE
                FROM SA_CUSTOMER_SETUP CS
                LEFT JOIN DIST_DISTRIBUTOR_MASTER DM ON DM.DISTRIBUTOR_CODE = CS.CUSTOMER_CODE AND DM.COMPANY_CODE = CS.COMPANY_CODE
                LEFT JOIN DIST_AREA_MASTER AM ON AM.AREA_CODE = DM.AREA_CODE AND AM.COMPANY_CODE = CS.COMPANY_CODE
                LEFT JOIN (SELECT A.SP_CODE, A.CUSTOMER_CODE, A.CUSTOMER_TYPE, A.COMPANY_CODE, C.EMPLOYEE_EDESC AS LAST_VISIT_BY, A.IS_VISITED AS LAST_VISIT_STATUS,
                              (CASE 
                                WHEN A.IS_VISITED IS NULL THEN 'X' 
                                  ELSE
                                    CASE WHEN TO_CHAR(SYSDATE, 'DD-MON-RRRR') = TO_CHAR(A.UPDATE_DATE, 'DD-MON-RRRR') THEN A.IS_VISITED ELSE 'X' END
                               END
                              ) IS_VISITED,
                              A.REMARKS,
                              TO_CHAR(MAX(A.UPDATE_DATE), 'RRRR-MM-DD HH24:MI A.M.') LAST_VISIT_DATE
                          FROM DIST_LOCATION_TRACK A
                          INNER JOIN SA_CUSTOMER_SETUP B ON B.CUSTOMER_CODE = A.CUSTOMER_CODE AND B.COMPANY_CODE = A.COMPANY_CODE
                          INNER JOIN HR_EMPLOYEE_SETUP C ON C.EMPLOYEE_CODE = A.SP_CODE AND C.COMPANY_CODE = A.COMPANY_CODE
                          WHERE 1 = 1
                          AND A.CUSTOMER_TYPE = 'D'
                          AND A.UPDATE_DATE = (SELECT MAX(UPDATE_DATE) FROM DIST_LOCATION_TRACK WHERE CUSTOMER_CODE = A.CUSTOMER_CODE AND CUSTOMER_TYPE = A.CUSTOMER_TYPE)
                          GROUP BY A.SP_CODE, A.CUSTOMER_CODE, A.CUSTOMER_TYPE, A.COMPANY_CODE, C.EMPLOYEE_EDESC, A.IS_VISITED, 
                              (CASE
                                WHEN A.IS_VISITED IS NULL THEN 'X' 
                                  ELSE
                                    CASE WHEN TO_CHAR(SYSDATE, 'DD-MON-RRRR') = TO_CHAR(A.UPDATE_DATE, 'DD-MON-RRRR') THEN A.IS_VISITED ELSE 'X' END
                               END
                              ),
                              A.REMARKS
                          ) LT
                       ON LT.CUSTOMER_CODE = CS.CUSTOMER_CODE AND LT.COMPANY_CODE = CS.COMPANY_CODE                       
                WHERE CS.GROUP_SKU_FLAG = 'I' AND DM.ACTIVE = 'Y' AND DM.DELETED_FLAG = 'Y'
                AND TRIM(CS.LINK_SUB_CODE) IN (SELECT TRIM(SUB_CODE) FROM FA_SUB_LEDGER_MAP WHERE COMPANY_CODE = '{model.COMPANY_CODE}' AND ACC_CODE ='{model.ACC_CODE}')
                AND CS.COMPANY_CODE = '{model.COMPANY_CODE}'
                ORDER BY UPPER(CS.CUSTOMER_EDESC)";
            var AllParty = dbContext.SqlQuery<EntityResponseModel>(Query).ToList();
            var result = new Dictionary<string, List<EntityResponseModel>>();
            if (AllParty != null)
                result.Add("distributor", AllParty);
            return result;
        }

        public List<EntityResponseModel> FetchEntityById(EntityRequestModel model, NeoErpCoreEntity dbContext)
        {
            string Query = string.Empty;
            if (model.entity_type.Equals("P", StringComparison.OrdinalIgnoreCase) || model.entity_type.Equals("DEALER", StringComparison.OrdinalIgnoreCase))
            {
                Query = $@"SELECT
                     DM.DEALER_CODE AS CODE,
                     PT.PARTY_TYPE_EDESC AS NAME,
                     PT.ACC_CODE,
                     DM.CONTACT_NO AS P_CONTACT_NO, '' AS P_CONTACT_NAME, DM.REG_OFFICE_ADDRESS AS ADDRESS,
                     RM.ROUTE_CODE, RM.ROUTE_NAME,
                     AM.AREA_CODE, AM.AREA_NAME,
                     NVL(DM.LATITUDE,0) LATITUDE, NVL(DM.LONGITUDE,0) LONGITUDE,
                     'dealer' AS TYPE,
                     '' DEFAULT_PARTY_TYPE_CODE,
                     '' PARENT_DISTRIBUTOR_CODE,
                     '' PARENT_DISTRIBUTOR_NAME,
                     LT.LAST_VISIT_DATE, LT.LAST_VISIT_BY, LT.LAST_VISIT_STATUS, NVL(LT.IS_VISITED, 'X') AS IS_VISITED, LT.REMARKS,
                     DM.COMPANY_CODE, DM.BRANCH_CODE
                   FROM
                     DIST_DEALER_MASTER DM
                     LEFT JOIN DIST_ROUTE_ENTITY RE ON RE.ENTITY_CODE = DM.DEALER_CODE AND RE.ENTITY_TYPE = 'P' AND RE.COMPANY_CODE = DM.COMPANY_CODE
                     LEFT JOIN DIST_ROUTE_MASTER RM ON RM.ROUTE_CODE = RE.ROUTE_CODE AND RM.COMPANY_CODE = DM.COMPANY_CODE
                     INNER JOIN IP_PARTY_TYPE_CODE PT ON PT.PARTY_TYPE_CODE = DM.DEALER_CODE AND PT.COMPANY_CODE = DM.COMPANY_CODE
                     INNER JOIN DIST_AREA_MASTER AM ON AM.AREA_CODE = DM.AREA_CODE AND AM.COMPANY_CODE = DM.COMPANY_CODE
                     LEFT JOIN (SELECT A.SP_CODE, A.CUSTOMER_CODE, A.CUSTOMER_TYPE, A.COMPANY_CODE, C.EMPLOYEE_EDESC AS LAST_VISIT_BY, A.IS_VISITED AS LAST_VISIT_STATUS,
                                  (CASE 
                                    WHEN A.IS_VISITED IS NULL THEN 'X' 
                                      ELSE
                                        CASE WHEN TO_CHAR(SYSDATE, 'DD-MON-RRRR') = TO_CHAR(A.UPDATE_DATE, 'DD-MON-RRRR') THEN A.IS_VISITED ELSE 'X' END
                                   END
                                  ) IS_VISITED, 
                                  A.REMARKS,
                                  TO_CHAR(MAX(A.UPDATE_DATE), 'RRRR-MM-DD HH24:MI A.M.') LAST_VISIT_DATE
                                FROM DIST_LOCATION_TRACK A
                                  INNER JOIN SA_CUSTOMER_SETUP B ON B.CUSTOMER_CODE = A.CUSTOMER_CODE AND B.COMPANY_CODE = A.COMPANY_CODE
                                  INNER JOIN HR_EMPLOYEE_SETUP C ON C.EMPLOYEE_CODE = A.SP_CODE AND C.COMPANY_CODE = A.COMPANY_CODE
                                WHERE 1 = 1
                                      AND A.CUSTOMER_TYPE = 'P'
                                      AND A.CUSTOMER_CODE = '{model.entity_code}'
                                      AND A.UPDATE_DATE = (SELECT MAX(UPDATE_DATE) FROM DIST_LOCATION_TRACK WHERE CUSTOMER_CODE = A.CUSTOMER_CODE AND CUSTOMER_TYPE = A.CUSTOMER_TYPE)
                                GROUP BY A.SP_CODE, A.CUSTOMER_CODE, A.CUSTOMER_TYPE, A.COMPANY_CODE, C.EMPLOYEE_EDESC, A.IS_VISITED, 
                                  (CASE 
                                    WHEN A.IS_VISITED IS NULL THEN 'X' 
                                      ELSE
                                        CASE WHEN TO_CHAR(SYSDATE, 'DD-MON-RRRR') = TO_CHAR(A.UPDATE_DATE, 'DD-MON-RRRR') THEN A.IS_VISITED ELSE 'X' END
                                   END
                                  ), 
                                  A.REMARKS
                               ) LT
                       ON LT.CUSTOMER_CODE = DM.DEALER_CODE AND LT.COMPANY_CODE = DM.COMPANY_CODE
                   WHERE 1 = 1
                         AND DM.COMPANY_CODE = '{model.COMPANY_CODE}'
                         AND DM.DEALER_CODE = '{model.entity_code}'";
            }
            else if (model.entity_type.Equals("D", StringComparison.OrdinalIgnoreCase) || model.entity_type.Equals("DISTRIBUTOR", StringComparison.OrdinalIgnoreCase))
            {
                Query = $@"SELECT
                     DM.DISTRIBUTOR_CODE AS CODE,
                     CS.CUSTOMER_EDESC AS NAME,
                     CS.ACC_CODE,
                     DM.CONTACT_NO AS P_CONTACT_NO, '' AS P_CONTACT_NAME, NVL(DM.REG_OFFICE_ADDRESS, CS.REGD_OFFICE_EADDRESS) AS ADDRESS,
                     RM.ROUTE_CODE, RM.ROUTE_NAME,
                     AM.AREA_CODE, AM.AREA_NAME,
                     NVL(DM.LATITUDE,0) LATITUDE, NVL(DM.LONGITUDE,0) LONGITUDE,
                     'distributor' AS TYPE,
                     CS.PARTY_TYPE_CODE AS DEFAULT_PARTY_TYPE_CODE,
                     '' PARENT_DISTRIBUTOR_CODE,
                     '' PARENT_DISTRIBUTOR_NAME,
                     LT.LAST_VISIT_DATE, LT.LAST_VISIT_BY, LT.LAST_VISIT_STATUS, NVL(LT.IS_VISITED, 'X') AS IS_VISITED, LT.REMARKS,
                     DM.COMPANY_CODE, DM.BRANCH_CODE
                   FROM
                     DIST_DISTRIBUTOR_MASTER DM
                     LEFT JOIN DIST_ROUTE_ENTITY RE ON RE.ENTITY_CODE = DM.DISTRIBUTOR_CODE AND RE.ENTITY_TYPE = 'D' AND RE.COMPANY_CODE = DM.COMPANY_CODE
                     LEFT JOIN DIST_ROUTE_MASTER RM ON RM.ROUTE_CODE = RE.ROUTE_CODE AND RM.COMPANY_CODE = DM.COMPANY_CODE
                     LEFT JOIN SA_CUSTOMER_SETUP CS ON CS.CUSTOMER_CODE = DM.DISTRIBUTOR_CODE AND CS.COMPANY_CODE = DM.COMPANY_CODE
                     INNER JOIN DIST_AREA_MASTER AM ON AM.AREA_CODE = DM.AREA_CODE AND AM.COMPANY_CODE = DM.COMPANY_CODE
                     LEFT JOIN (SELECT A.SP_CODE, A.CUSTOMER_CODE, A.CUSTOMER_TYPE, A.COMPANY_CODE, C.EMPLOYEE_EDESC AS LAST_VISIT_BY, A.IS_VISITED AS LAST_VISIT_STATUS,
                                  (CASE 
                                    WHEN A.IS_VISITED IS NULL THEN 'X' 
                                      ELSE
                                        CASE WHEN TO_CHAR(SYSDATE, 'DD-MON-RRRR') = TO_CHAR(A.UPDATE_DATE, 'DD-MON-RRRR') THEN A.IS_VISITED ELSE 'X' END
                                   END
                                  ) IS_VISITED, 
                                  A.REMARKS,
                                  TO_CHAR(MAX(A.UPDATE_DATE), 'RRRR-MM-DD HH24:MI A.M.') LAST_VISIT_DATE
                                FROM DIST_LOCATION_TRACK A
                                  INNER JOIN SA_CUSTOMER_SETUP B ON B.CUSTOMER_CODE = A.CUSTOMER_CODE AND B.COMPANY_CODE = A.COMPANY_CODE
                                  INNER JOIN HR_EMPLOYEE_SETUP C ON C.EMPLOYEE_CODE = A.SP_CODE AND C.COMPANY_CODE = A.COMPANY_CODE
                                WHERE 1 = 1
                                      AND A.CUSTOMER_TYPE = 'D'
                                      AND A.CUSTOMER_CODE = '{model.entity_code}'
                                      AND A.UPDATE_DATE = (SELECT MAX(UPDATE_DATE) FROM DIST_LOCATION_TRACK WHERE CUSTOMER_CODE = A.CUSTOMER_CODE AND CUSTOMER_TYPE = A.CUSTOMER_TYPE)
                                GROUP BY A.SP_CODE, A.CUSTOMER_CODE, A.CUSTOMER_TYPE, A.COMPANY_CODE, C.EMPLOYEE_EDESC, A.IS_VISITED, 
                                  (CASE 
                                    WHEN A.IS_VISITED IS NULL THEN 'X' 
                                      ELSE
                                        CASE WHEN TO_CHAR(SYSDATE, 'DD-MON-RRRR') = TO_CHAR(A.UPDATE_DATE, 'DD-MON-RRRR') THEN A.IS_VISITED ELSE 'X' END
                                   END
                                  ), 
                                  A.REMARKS
                               ) LT
                       ON LT.CUSTOMER_CODE = DM.DISTRIBUTOR_CODE AND LT.COMPANY_CODE = DM.COMPANY_CODE
                   WHERE 1 = 1
                         AND DM.COMPANY_CODE = '{model.COMPANY_CODE}'
                         AND DM.DISTRIBUTOR_CODE = '{model.entity_code}'
                         AND DM.ACTIVE = 'Y' AND DM.DELETED_FLAG = 'Y'";
            }
            else if (model.entity_type.Equals("R", StringComparison.OrdinalIgnoreCase) || model.entity_type.Equals("RESELLER", StringComparison.OrdinalIgnoreCase))
            {
                Query = $@"SELECT
                     REM.RESELLER_CODE AS CODE,
                     REM.RESELLER_NAME AS NAME,
                     '' AS ACC_CODE,
                     REM.CONTACT_NO AS P_CONTACT_NO, REM.CONTACT_NAME AS P_CONTACT_NAME, REM.REG_OFFICE_ADDRESS AS ADDRESS,
                     RM.ROUTE_CODE, RM.ROUTE_NAME,
                     AM.AREA_CODE, AM.AREA_NAME,
                     NVL(REM.LATITUDE,0) LATITUDE, NVL(REM.LONGITUDE,0) LONGITUDE,
                     'reseller' AS TYPE,
                     '' AS DEFAULT_PARTY_TYPE_CODE,
                     DISTRIBUTOR_CODE AS PARENT_DISTRIBUTOR_CODE,
                     CS.CUSTOMER_EDESC AS PARENT_DISTRIBUTOR_NAME,
                     LT.LAST_VISIT_DATE, LT.LAST_VISIT_BY, LT.LAST_VISIT_STATUS, NVL(LT.IS_VISITED, 'X') AS IS_VISITED, LT.REMARKS,
                     REM.COMPANY_CODE, REM.BRANCH_CODE
                   FROM DIST_RESELLER_MASTER REM
                     LEFT JOIN DIST_ROUTE_ENTITY RE ON RE.ENTITY_CODE = REM.RESELLER_CODE AND RE.ENTITY_TYPE = 'R' AND RE.COMPANY_CODE = REM.COMPANY_CODE
                     LEFT JOIN DIST_ROUTE_MASTER RM ON RM.ROUTE_CODE = RE.ROUTE_CODE AND RM.COMPANY_CODE = REM.COMPANY_CODE
                     INNER JOIN DIST_AREA_MASTER AM ON AM.AREA_CODE = REM.AREA_CODE AND AM.COMPANY_CODE = REM.COMPANY_CODE
                     LEFT JOIN SA_CUSTOMER_SETUP CS ON CS.CUSTOMER_CODE = REM.DISTRIBUTOR_CODE AND CS.COMPANY_CODE = REM.COMPANY_CODE
                     LEFT JOIN (SELECT A.SP_CODE, A.CUSTOMER_CODE, A.CUSTOMER_TYPE, A.COMPANY_CODE, C.EMPLOYEE_EDESC AS LAST_VISIT_BY, A.IS_VISITED AS LAST_VISIT_STATUS,
                                  (CASE 
                                    WHEN A.IS_VISITED IS NULL THEN 'X' 
                                      ELSE
                                        CASE WHEN TO_CHAR(SYSDATE, 'DD-MON-RRRR') = TO_CHAR(A.UPDATE_DATE, 'DD-MON-RRRR') THEN A.IS_VISITED ELSE 'X' END
                                   END
                                  ) IS_VISITED, 
                                  A.REMARKS,
                                  TO_CHAR(MAX(A.UPDATE_DATE), 'RRRR-MM-DD HH24:MI A.M.') LAST_VISIT_DATE
                                FROM DIST_LOCATION_TRACK A
                                  INNER JOIN DIST_RESELLER_MASTER B ON B.RESELLER_CODE = A.CUSTOMER_CODE AND B.COMPANY_CODE = A.COMPANY_CODE
                                  INNER JOIN HR_EMPLOYEE_SETUP C ON C.EMPLOYEE_CODE = A.SP_CODE AND C.COMPANY_CODE = A.COMPANY_CODE
                                WHERE 1 = 1
                                      AND B.IS_CLOSED = 'N'
                                      AND A.CUSTOMER_TYPE = 'R'
                                      AND A.CUSTOMER_CODE = '{model.entity_code}'
                                      AND A.UPDATE_DATE = (SELECT MAX(UPDATE_DATE) FROM DIST_LOCATION_TRACK WHERE CUSTOMER_CODE = A.CUSTOMER_CODE AND CUSTOMER_TYPE = A.CUSTOMER_TYPE)
                                GROUP BY A.SP_CODE, A.CUSTOMER_CODE, A.CUSTOMER_TYPE, A.COMPANY_CODE, C.EMPLOYEE_EDESC, A.IS_VISITED, 
                                  (CASE 
                                    WHEN A.IS_VISITED IS NULL THEN 'X' 
                                      ELSE
                                        CASE WHEN TO_CHAR(SYSDATE, 'DD-MON-RRRR') = TO_CHAR(A.UPDATE_DATE, 'DD-MON-RRRR') THEN A.IS_VISITED ELSE 'X' END
                                   END
                                  ), 
                                  A.REMARKS
                               ) LT
                       ON LT.CUSTOMER_CODE = REM.RESELLER_CODE AND LT.COMPANY_CODE = REM.COMPANY_CODE
                   WHERE 1 = 1
                         AND REM.COMPANY_CODE = '{model.COMPANY_CODE}'
                         AND REM.IS_CLOSED = 'N'
                         AND REM.RESELLER_CODE = '{model.entity_code}'";
            }
            else
                throw new Exception("Invalid entity type");
            if (string.IsNullOrWhiteSpace(Query))
                return new List<EntityResponseModel>();
            var result = dbContext.SqlQuery<EntityResponseModel>(Query).ToList();
            return result;
        }

        public Dictionary<string, List<EntityResponseModel>> FetchDistributorWithConstraint(CommonRequestModel model, NeoErpCoreEntity dbContext)
        {
            string Query = string.Empty;
            Query = $@"SELECT
                     DM.DISTRIBUTOR_CODE AS CODE,
                     CS.CUSTOMER_EDESC AS NAME,
                     CS.ACC_CODE,
                     DM.CONTACT_NO AS P_CONTACT_NO, '' AS P_CONTACT_NAME, NVL(DM.REG_OFFICE_ADDRESS, CS.REGD_OFFICE_EADDRESS) AS ADDRESS,
                     RM.ROUTE_CODE, RM.ROUTE_NAME,
                     AM.AREA_CODE, AM.AREA_NAME,
                     NVL(DM.LATITUDE,0) LATITUDE, NVL(DM.LONGITUDE,0) LONGITUDE,
                     'distributor' AS TYPE,
                     CS.PARTY_TYPE_CODE AS DEFAULT_PARTY_TYPE_CODE,
                     '' PARENT_DISTRIBUTOR_CODE,
                     '' PARENT_DISTRIBUTOR_NAME,
                     LT.LAST_VISIT_DATE, LT.LAST_VISIT_BY, LT.LAST_VISIT_STATUS, NVL(LT.IS_VISITED, 'X') AS IS_VISITED, LT.REMARKS,
                     DM.COMPANY_CODE, DM.BRANCH_CODE
                   FROM
                     DIST_DISTRIBUTOR_MASTER DM
                     LEFT JOIN DIST_ROUTE_ENTITY RE ON RE.ENTITY_CODE = DM.DISTRIBUTOR_CODE AND RE.ENTITY_TYPE = 'D' AND RE.COMPANY_CODE = DM.COMPANY_CODE
                     LEFT JOIN DIST_ROUTE_MASTER RM ON RM.ROUTE_CODE = RE.ROUTE_CODE AND RM.COMPANY_CODE = DM.COMPANY_CODE
                     LEFT JOIN SA_CUSTOMER_SETUP CS ON CS.CUSTOMER_CODE = DM.DISTRIBUTOR_CODE AND CS.COMPANY_CODE = DM.COMPANY_CODE
                     INNER JOIN DIST_AREA_MASTER AM ON AM.AREA_CODE = DM.AREA_CODE AND AM.COMPANY_CODE = DM.COMPANY_CODE
                     LEFT JOIN (SELECT A.SP_CODE, A.CUSTOMER_CODE, A.CUSTOMER_TYPE, A.COMPANY_CODE, C.EMPLOYEE_EDESC AS LAST_VISIT_BY, A.IS_VISITED AS LAST_VISIT_STATUS,
                                  (CASE 
                                    WHEN A.IS_VISITED IS NULL THEN 'X' 
                                      ELSE
                                        CASE WHEN TO_CHAR(SYSDATE, 'DD-MON-RRRR') = TO_CHAR(A.UPDATE_DATE, 'DD-MON-RRRR') THEN A.IS_VISITED ELSE 'X' END
                                   END
                                  ) IS_VISITED, 
                                  A.REMARKS,
                                  TO_CHAR(MAX(A.UPDATE_DATE), 'RRRR-MM-DD HH24:MI A.M.') LAST_VISIT_DATE
                                FROM DIST_LOCATION_TRACK A
                                  INNER JOIN SA_CUSTOMER_SETUP B ON B.CUSTOMER_CODE = A.CUSTOMER_CODE AND B.COMPANY_CODE = A.COMPANY_CODE
                                  INNER JOIN HR_EMPLOYEE_SETUP C ON C.EMPLOYEE_CODE = A.SP_CODE AND C.COMPANY_CODE = A.COMPANY_CODE
                                WHERE 1 = 1
                                      AND A.CUSTOMER_TYPE = 'D'
                                      AND A.UPDATE_DATE = (SELECT MAX(UPDATE_DATE) FROM DIST_LOCATION_TRACK WHERE CUSTOMER_CODE = A.CUSTOMER_CODE AND CUSTOMER_TYPE = A.CUSTOMER_TYPE)
                                GROUP BY A.SP_CODE, A.CUSTOMER_CODE, A.CUSTOMER_TYPE, A.COMPANY_CODE, C.EMPLOYEE_EDESC, A.IS_VISITED, 
                                  (CASE 
                                    WHEN A.IS_VISITED IS NULL THEN 'X' 
                                      ELSE
                                        CASE WHEN TO_CHAR(SYSDATE, 'DD-MON-RRRR') = TO_CHAR(A.UPDATE_DATE, 'DD-MON-RRRR') THEN A.IS_VISITED ELSE 'X' END
                                   END
                                  ), 
                                  A.REMARKS
                               ) LT
                       ON LT.CUSTOMER_CODE = DM.DISTRIBUTOR_CODE AND LT.COMPANY_CODE = DM.COMPANY_CODE
                   WHERE 1 = 1
                         AND DM.COMPANY_CODE = '{model.COMPANY_CODE}'
                         AND DM.ACTIVE = 'Y' AND DM.DELETED_FLAG = 'Y'";
            var distributors = dbContext.SqlQuery<EntityResponseModel>(Query).ToList();
            var result = new Dictionary<string, List<EntityResponseModel>>();
            result.Add("distributor", distributors);
            return result;
        }

        public Dictionary<string, object> SyncData(VisitPlanRequestModel model, NeoErpCoreEntity dbContext)
        {
            var result = new Dictionary<string, object>();
            var pref = FetchPreferences(model.COMPANY_CODE, dbContext);
            //preferences
            result.Add("PREFERENCES", new
            {
                result = pref,
                response = true,
                error = ""
            });

            //All company Areas
            try
            {
                var areasData = FetchAllCompanyArea(model, dbContext, pref);
                result.Add("AREA", new
                {
                    result = areasData,
                    response = areasData.Count > 0 ? true : false,
                    error = ""
                });
            }
            catch (Exception ex)
            {
                result.Add("AREA", new
                {
                    result = new object[] { },
                    response = false,
                    error = ex.Message
                });
            }

            //Visit Plans
            try
            {
                var visitPlanData = GetVisitPlan(model, dbContext, pref);
                result.Add("VISIT_PLAN", new
                {
                    result = visitPlanData,
                    response = visitPlanData.Count > 0 ? true : false,
                    error = ""
                });
            }
            catch (Exception ex)
            {
                result.Add("VISIT_PLAN", new
                {
                    result = new object[] { },
                    response = false,
                    error = ex.Message
                });
            }

            //Branding Visit Plans
            try
            {
                if (model.user_type != "B")
                    throw new Exception("not a branding user");
                var visitPlanData = GetBrandingVisitPlan(model, dbContext, pref);
                result.Add("BRD_VISIT_PLAN", new
                {
                    result = visitPlanData,
                    response = visitPlanData.Count > 0 ? true : false,
                    error = ""
                });
            }
            catch (Exception ex)
            {
                result.Add("BRD_VISIT_PLAN", new
                {
                    result = new object[] { },
                    response = false,
                    error = ex.Message
                });
            }

            //Branding Visit Plans (for Hoarding Boards)
            try
            {
                if (model.user_type != "B")
                    throw new Exception("not a branding user");
                var visitPlanData = GetBrdVisitPlan(model, dbContext, pref);
                result.Add("OTHER_VISIT_PLAN", new
                {
                    result = visitPlanData,
                    response = visitPlanData.Count > 0 ? true : false,
                    error = ""
                });
            }
            catch (Exception ex)
            {
                result.Add("OTHER_VISIT_PLAN", new
                {
                    result = new object[] { },
                    response = false,
                    error = ex.Message
                });
            }

            //Entity List (Dealer/Distributors/Resellers)
            try
            {
                var entityData = FetchAllCompanyEntity(model.COMPANY_CODE,model.spcode, dbContext, pref);
                result.Add("ENTITY", new
                {
                    result = entityData,
                    response = entityData.Count > 0 ? true : false,
                    error = ""
                });
            }
            catch (Exception ex)
            {
                result.Add("ENTITY", new
                {
                    result = new object(),
                    response = false,
                    error = ex.Message
                });
            }
            //Discount
            try
            {
                var discountData = FetchDiscount(model.COMPANY_CODE, model.BRANCH_CODE, dbContext, pref);
                result.Add("DISCOUNT", new
                {
                    result = discountData,
                    response = discountData.Count > 0 ? true : false,
                    error = ""
                });
            }
            catch (Exception ex)
            {
                result.Add("DISCOUNT", new
                {
                    result = new object[] { },
                    response = false,
                    error = ex.Message
                });
            }
            //Item list with their respective Brand
            try
            {
                var itemData = FetchAllCompanyItems(model, dbContext, pref);
                result.Add("ITEM", new
                {
                    result = itemData,
                    response = itemData.Count > 0 ? true : false,
                    error = ""
                });
            }

            catch (Exception ex)
            {
                result.Add("ITEM", new
                {
                    result = new object[] { },
                    response = false,
                    error = ex.Message
                });
            }
            //Item list for scheme
            try
            {
                var itemData = FetchAllSchemeItems(model, dbContext, pref);
                result.Add("SCHEME_ITEM", new
                {
                    result = itemData,
                    response = itemData.Count > 0 ? true : false,
                    error = ""
                });
            }

            catch (Exception ex)
            {
                result.Add("SCHEME_ITEM", new
                {
                    result = new object[] { },
                    response = false,
                    error = ex.Message
                });
            }
            //Gift Item list for scheme
            try
            {
                var itemData = FetchAllSchemeGiftItems(model, dbContext, pref);
                result.Add("SCHEME_GIFT_ITEM", new
                {
                    result = itemData,
                    response = itemData.Count > 0 ? true : false,
                    error = ""
                });
            }

            catch (Exception ex)
            {
                result.Add("SCHEME_GIFT_ITEM", new
                {
                    result = new object[] { },
                    response = false,
                    error = ex.Message
                });
            }
            //Rate of item per branch
            try
            {
                var rateData = FetchAllCompanyBranchItemRate(model, dbContext, pref);
                result.Add("RATE", new
                {
                    result = rateData,
                    response = rateData.Count > 0 ? true : false,
                    error = ""
                });
            }

            catch (Exception ex)
            {
                result.Add("RATE", new
                {
                    result = new object[] { },
                    response = false,
                    error = ex.Message
                });
            }

            //Measurement Units
            try
            {
                var muData = FetchMU(model, dbContext);
                result.Add("MU", new
                {
                    result = muData,
                    response = muData.Count > 0 ? true : false,
                    error = ""
                });
            }

            catch (Exception ex)
            {
                result.Add("MU", new
                {
                    result = new object(),
                    response = false,
                    error = ex.Message
                });
            }

            //Outlets
            try
            {
                var outletData = FetchOutlets(model, dbContext);
                result.Add("OUTLET", new
                {
                    result = outletData,
                    response = outletData.Count > 0 ? true : false,
                    error = ""
                });
            }

            catch (Exception ex)
            {
                result.Add("OUTLET", new
                {
                    result = new object(),
                    response = false,
                    error = ex.Message
                });
            }
            //Distributors sashi
                try
                {
                    var outletData = FetchOutlets(model, dbContext);
                    result.Add("distributor", new
                    {
                        result = outletData,
                        response = outletData.Count > 0 ? true : false,
                        error = ""
                    });
                }

                catch (Exception ex)
                {
                    result.Add("distributor", new
                    {
                        result = new object(),
                        response = false,
                        error = ex.Message
                    });
                }
            
            //General and Tabular Questions Lists
            try
            {
                var questionReq = new QuestionRequestModel
                {
                    BRANCH_CODE = model.BRANCH_CODE,
                    COMPANY_CODE = model.COMPANY_CODE,
                    latitude = model.latitude,
                    longitude = model.longitude,
                    sp_code = model.spcode,
                    user_id = model.user_id
                };
                var questionData = FetchAllQuestions(questionReq, dbContext);
                result.Add("QUESTION", new
                {
                    result = questionData,
                    response = questionData != null ? true : false,
                    error = ""
                });
            }

            catch (Exception ex)
            {
                result.Add("QUESTION", new
                {
                    result = new 
                    {
                        general = new List<GeneralModel>(), 
                        tabular = new Dictionary<string, TabularModel>() 
                    },
                    response = false,
                    error = ex.Message
                });
            }

            //Sub ledger map
            try
            {
                if (model.user_type == "B")
                    throw new Exception("Branding user");
                var subLedgerMapData = FetchAllCompanySubLedgerMap(model, dbContext, pref);
                result.Add("SUB_LEDGER_MAP", new
                {
                    result = subLedgerMapData,
                    response = subLedgerMapData.Count > 0 ? true : false,
                    error = ""
                });
            }
            catch (Exception ex)
            {
                result.Add("SUB_LEDGER_MAP", new
                {
                    result = new object[] { },
                    response = false,
                    error = ex.Message
                });
            }

            //All party types
            try
            {
                if (model.user_type == "B")
                    throw new Exception("Branding user");
                var partyTypeData = FetchAllCompanyPartyType(model, dbContext, pref);
                result.Add("PARTY_TYPE", new
                {
                    result = partyTypeData,
                    response = partyTypeData.Count > 0 ? true : false,
                    error = ""
                });
            }
            catch (Exception ex)
            {
                result.Add("PARTY_TYPE", new
                {
                    result = new object[] { },
                    response = false,
                    error = ex.Message
                });
            }

            //All SA Customers
            try
            {
                if (model.user_type == "B")
                    throw new Exception("Branding user");
                var saCustomerData = FetchAllCompanySaCustomer(model, dbContext, pref);
                result.Add("SA_CUSTOMER", new
                {
                    result = saCustomerData,
                    response = saCustomerData.Count > 0 ? true : false,
                    error = ""
                });
            }
            catch (Exception ex)
            {
                result.Add("SA_CUSTOMER", new
                {
                    result = new object[] { },
                    response = false,
                    error = ex.Message
                });
            }

            //All ImageCategory
            try
            {
                var ImageCategories = new List<ImageCategoryModel>();
                if (model.user_type == "B")
                    ImageCategories = FetchImageCategory(model, dbContext);
                else
                {
                    ImageCategories = FetchImageDistributionCategory(model, dbContext);
                }

                    //if (model.user_type == "B")
                    //    throw new Exception("Branding user");
                    
                result.Add("IMAGE_CATEGORY", new
                {
                    result = ImageCategories,
                    response = ImageCategories.Count > 0 ? true : false,
                    error = ""
                });
            }
            catch (Exception ex)
            {
                result.Add("IMAGE_CATEGORY", new
                {
                    result = new object[] { },
                    response = false,
                    error = ex.Message
                });
            }

            //DistributorItems
            try
            {
                if (model.user_type == "B")
                    throw new Exception("Branding user");
                var DistributorItems = FetchDistributorItems(model, dbContext);
                result.Add("DISTRIBUTOR_ITEMS", new
                {
                    result = DistributorItems,
                    response = DistributorItems.Count > 0 ? true : false,
                    error = ""
                });
            }
            catch (Exception ex)
            {
                result.Add("DISTRIBUTOR_ITEMS", new
                {
                    result = new object[] { },
                    response = false,
                    error = ex.Message
                });
            }

            //Reseller Entities
            try
            {
                if (model.user_type == "B")
                    throw new Exception("Branding user");
                var ResellerEntities = FetchResellerEntity(model, pref, model.spcode, dbContext);
                result.Add("RESELLER_ENTITIES", new
                {
                    result = ResellerEntities,
                    response = ResellerEntities.Count > 0 ? true : false,
                    error = ""
                });
            }
            catch (Exception ex)
            {
                result.Add("RESELLER_ENTITIES", new
                {
                    result = new object[] { },
                    response = false,
                    error = ex.Message
                });
            }

            //DIST Groups
            try
            {
                if (model.user_type == "B")
                    throw new Exception("Branding user");
                var Groups = GetResellerGroups(model, dbContext);
                result.Add("DIST_GROUPS", new
                {
                    result = Groups,
                    response = Groups.Count > 0 ? true : false,
                    error = ""
                });
            }
            catch (Exception ex)
            {
                result.Add("DIST_GROUPS", new
                {
                    result = new object[] { },
                    response = false,
                    error = ex.Message
                });
            }

            //DIST_NOTIFICATIONS
            try
            {
                var Notifications = GetNotifications(model.spcode, dbContext);
                result.Add("DIST_NOTIFICATIONS", new
                {
                    result = Notifications,
                    response = Notifications.Count > 0 ? true : false,
                    error = ""
                });
            }
            catch (Exception ex)
            {
                result.Add("DIST_NOTIFICATIONS", new
                {
                    result = new object[] { },
                    response = false,
                    error = ex.Message
                });
            }

            //COMP_ITEMS
            try
            {
                var CompItems = GetCompItems(model, dbContext);
                result.Add("DIST_COMP_ITEMS", new
                {
                    result = CompItems,
                    response = CompItems.Count > 0 ? true : false,
                    error = ""
                });
            }
            catch (Exception ex)
            {
                result.Add("DIST_COMP_ITEMS", new
                {
                    result = new object[] { },
                    response = false,
                    error = ex.Message
                });
            }

            //COMP_ITEM_maps
            try
            {
                var CompItems = GetCompItemMaps(model, dbContext);
                result.Add("DIST_COMP_ITEM_MAP", new
                {
                    result = CompItems,
                    response = CompItems.Count > 0 ? true : false,
                    error = ""
                });
            }
            catch (Exception ex)
            {
                result.Add("DIST_COMP_ITEM_MAP", new
                {
                    result = new object[] { },
                    response = false,
                    error = ex.Message
                });
            }

            //COMP_ITEM_FIELDS
            try
            {
                var CompItemFields = GetCompItemFields(model, dbContext);
                result.Add("DIST_COMP_ITEM_FIELDS", new
                {
                    result = CompItemFields,
                    response = CompItemFields.Count > 0 ? true : false,
                    error = ""
                });
            }
            catch (Exception ex)
            {
                result.Add("DIST_COMP_ITEM_FIELDS", new
                {
                    result = new object[] { },
                    response = false,
                    error = ex.Message
                });
            }

            //DIST_GROUP_MAP
            try
            {
                var groupMaps = GetGroupMaps(model.COMPANY_CODE, dbContext);
                result.Add("DIST_GROUP_MAP", new
                {
                    result = groupMaps,
                    response = groupMaps.Count > 0 ? true : false,
                    error = ""
                });
            }
            catch (Exception ex)
            {
                result.Add("DIST_GROUP_MAP", new
                {
                    result = new object[] { },
                    response = false,
                    error = ex.Message
                });
            }
            //UPDATE_STATUS
            try
            {
                var updateData = FetchUpdates(model, dbContext);
                result.Add("UPDATE_STATUS", new
                {
                    result = updateData,
                    response = updateData.Count > 0 ? true : false,
                    error = ""
                });
            }

            catch (Exception ex)
            {
                result.Add("UPDATE_STATUS", new
                {
                    result = new object[] { },
                    response = false,
                    error = ex.Message
                });
            }
            //COMPLAIN_TYPES
            try
            {
                var complainData = FetchComplainTypes(model, dbContext);
                result.Add("COMPLAIN_TYPE", new
                {
                    result = complainData,
                    response = complainData.Count > 0 ? true : false,
                    error = ""
                });
            }

            catch (Exception ex)
            {
                result.Add("COMPLAIN_TYPE", new
                {
                    result = new object[] { },
                    response = false,
                    error = ex.Message
                });
            }
            //SERIOUSNESS_TYPES
            try
            {
                var SeriousnessData = FetchSeriousnessTypes(model, dbContext);
                result.Add("SERIOUSNESS_TYPES", new
                {
                    result = SeriousnessData,
                    response = SeriousnessData.Count > 0 ? true : false,
                    error = ""
                });
            }

            catch (Exception ex)
            {
                result.Add("SERIOUSNESS_TYPES", new
                {
                    result = new object[] { },
                    response = false,
                    error = ex.Message
                });
            }
            //Date Range
            try
            {
                var dateData = FetchDateRange(model, dbContext);
                result.Add("DATE_RANGE", new
                {
                    result = dateData,
                    response = dateData.Count > 0 ? true : false,
                    error = ""
                });
            }

            catch (Exception ex)
            {
                result.Add("DATE_RANGE", new
                {
                    result = new object[] { },
                    response = false,
                    error = ex.Message
                });
            }
            //RETURN_CONDITION_TYPES
            try
            {
                var ReturnTypesData = FetchReturnCondition(model, dbContext);
                result.Add("RETURN_CONDITION_TYPES", new
                {
                    result = ReturnTypesData,
                    response = ReturnTypesData.Count > 0 ? true : false,
                    error = ""
                });
            }

            catch (Exception ex)
            {
                result.Add("RETURN_CONDITION_TYPES", new
                {
                    result = new object[] { },
                    response = false,
                    error = ex.Message
                });
            }
            //PRIORITY_STATUS
            try
            {
                var priorityData = FetchPriorityData(model, dbContext);
                result.Add("PRIORITY_STATUS", new
                {
                    result = priorityData,
                    response = priorityData.Count > 0 ? true : false,
                    error = ""
                });
            }

            catch (Exception ex)
            {
                result.Add("PRIORITY_STATUS", new
                {
                    result = new object[] { },
                    response = false,
                    error = ex.Message
                });
            }
            var prefs = FetchPreferences(model.COMPANY_CODE, dbContext);
            if (prefs.PO_SALES_TYPE.Trim().ToUpper() == "Y")
            {
                //All SA sales types
                try
                {
                    var saSalesTypeData = FetchAllCompanySaSalesType(model.COMPANY_CODE, dbContext, pref);
                    result.Add("SA_SALES_TYPE", new
                    {
                        result = saSalesTypeData,
                        response = saSalesTypeData.Count > 0 ? true : false,
                        error = ""
                    });
                }
                catch (Exception ex)
                {
                    result.Add("SA_SALES_TYPE", new
                    {
                        result = new object[] { },
                        response = false,
                        error = ex.Message
                    });
                }
            }

            if (prefs.PO_SHIPPING_ADDRESS.Trim().ToUpper() == "Y")
            {
                //All shipping addresses
                try
                {
                    var ShippingData = FetchShippingAddress(dbContext, pref);
                    result.Add("SHIPPING_ADDRESS", new
                    {
                        result = ShippingData,
                        response = ShippingData.Count > 0 ? true : false,
                        error = ""
                    });
                }
                catch (Exception ex)
                {
                    result.Add("SHIPPING_ADDRESS", new
                    {
                        result = new object[] { },
                        response = false,
                        error = ex.Message
                    });
                }
            }

            //Schemes
            try
            {
                var SchemeData = GetContracts(model, dbContext);
                result.Add("SCHEMES", new
                {
                    result = SchemeData,
                    response = SchemeData.Count > 0 ? true : false,
                    error = ""
                });
            }
            catch (Exception ex)
            {
                result.Add("SCHEMES", new
                {
                    result = new object[] { },
                    response = false,
                    error = ex.Message
                });
            }

            //CRM tasks
            try
            {
                var crmTasks = GetCrmTasks(model.spcode, dbContext);
                result.Add("CRM_TASKS", new
                {
                    result = crmTasks,
                    response = crmTasks.Count > 0 ? true : false,
                    error = ""
                });
            }
            catch (Exception ex)
            {
                result.Add("CRM_TASKS", new
                {
                    result = new object[] { },
                    response = false,
                    error = ex.Message
                });
            }

            return result;
        }

        public Dictionary<string, object> SyncDataTopic(VisitPlanRequestModel model, NeoErpCoreEntity dbContext)
        {
            var result = new Dictionary<string, object>();
            var pref = FetchPreferences(model.COMPANY_CODE, dbContext);
            //preferences
            result.Add("PREFERENCES", new
            {
                result = pref,
                response = true,
                error = ""
            });

            //All company Areas
            if (model.entities.Any(x => x.Equals("AREA", StringComparison.OrdinalIgnoreCase)))
            {
                try
                {
                    var areasData = FetchAllCompanyArea(model, dbContext, pref);
                    result.Add("AREA", new
                    {
                        result = areasData,
                        response = areasData.Count > 0 ? true : false,
                        error = ""
                    });
                }
                catch (Exception ex)
                {
                    result.Add("AREA", new
                    {
                        result = new object[] { },
                        response = false,
                        error = ex.Message
                    });
                }
            }

            //Visit Plans
            if (model.entities.Any(x => x.Equals("VISIT_PLAN", StringComparison.OrdinalIgnoreCase)))
            {
                try
                {
                    if (model.user_type == "B")
                        throw new Exception("branding user doesn't have visit plan.");
                    var visitPlanData = GetVisitPlan(model, dbContext, pref);
                    result.Add("VISIT_PLAN", new
                    {
                        result = visitPlanData,
                        response = visitPlanData.Count > 0 ? true : false,
                        error = ""
                    });
                }
                catch (Exception ex)
                {
                    result.Add("VISIT_PLAN", new
                    {
                        result = new object[] { },
                        response = false,
                        error = ex.Message
                    });
                }
            }

            //if (model.entities.Any(x => x.Equals("VISIT_PLAN", StringComparison.OrdinalIgnoreCase)))
            //{
            //    //Branding Visit Plans (for Hoarding Boards)
            //    try
            //    {
            //        if (model.user_type != "B")
            //            throw new Exception("not a branding user");
            //        var visitPlanData = GetBrdVisitPlan(model, dbContext, pref);
            //        result.Add("OTHER_VISIT_PLAN", new
            //        {
            //            result = visitPlanData,
            //            response = visitPlanData.Count > 0 ? true : false,
            //            error = ""
            //        });
            //    }
            //    catch (Exception ex)
            //    {
            //        result.Add("OTHER_VISIT_PLAN", new
            //        {
            //            result = new object(),
            //            response = false,
            //            error = ex.Message
            //        });
            //    }
            //}

            if (model.entities.Any(x => x.Equals("BRD_VISIT_PLAN", StringComparison.OrdinalIgnoreCase)))
            {
                //Branding Visit Plans
                try
                {
                    var visitPlanData = GetBrandingVisitPlan(model, dbContext, pref);
                    result.Add("BRD_VISIT_PLAN", new
                    {
                        result = visitPlanData,
                        response = visitPlanData.Count > 0 ? true : false,
                        error = ""
                    });
                }
                catch (Exception ex)
                {
                    result.Add("BRD_VISIT_PLAN", new
                    {
                        result = new object[] { },
                        response = false,
                        error = ex.Message
                    });
                }
            }
            if (model.entities.Any(x => x.Equals("BRD_VISIT_PLAN", StringComparison.OrdinalIgnoreCase)))
            {
                //Branding Visit Plans (for Hoarding Boards)
                try
                {
                    if (model.user_type != "B")
                        throw new Exception("not a branding user");
                    var visitPlanData = GetBrdVisitPlan(model, dbContext, pref);
                    result.Add("OTHER_VISIT_PLAN", new
                    {
                        result = visitPlanData,
                        response = visitPlanData.Count > 0 ? true : false,
                        error = ""
                    });
                }
                catch (Exception ex)
                {
                    result.Add("OTHER_VISIT_PLAN", new
                    {
                        result = new object[] { },
                        response = false,
                        error = ex.Message
                    });
                }
            }

            //Entity List (Dealer/Distributors/Resellers) 
            if (model.entities.Any(x => x.Equals("ENTITY", StringComparison.OrdinalIgnoreCase)))
            {
                try
                {
                    var entityData = FetchAllCompanyEntity(model.COMPANY_CODE,model.spcode, dbContext, pref);
                    result.Add("ENTITY", new
                    {
                        result = entityData,
                        response = entityData.Count > 0 ? true : false,
                        error = ""
                    });
                }
                catch (Exception ex)
                {
                    result.Add("ENTITY", new
                    {
                        result = new object(),
                        response = false,
                        error = ex.Message
                    });
                }
            }
            //Discount
            if (model.entities.Any(x => x.Equals("DISCOUNT", StringComparison.OrdinalIgnoreCase)))
            {
                try
                {
                    var discountData = FetchDiscount(model.COMPANY_CODE, model.BRANCH_CODE, dbContext, pref);
                    result.Add("DISCOUNT", new
                    {
                        result = discountData,
                        response = discountData.Count > 0 ? true : false,
                        error = ""
                    });
                }
                catch (Exception ex)
                {
                    result.Add("DISCOUNT", new
                    {
                        result = new object[] { },
                        response = false,
                        error = ex.Message
                    });
                }
            }
            //Item list with their respective Brand
            if (model.entities.Any(x => x.Equals("ITEM", StringComparison.OrdinalIgnoreCase)))
            {
                try
                {
                    var itemData = FetchAllCompanyItems(model, dbContext, pref);
                    result.Add("ITEM", new
                    {
                        result = itemData,
                        response = itemData.Count > 0 ? true : false,
                        error = ""
                    });
                }

                catch (Exception ex)
                {
                    result.Add("ITEM", new
                    {
                        result = new object[] { },
                        response = false,
                        error = ex.Message
                    });
                }


                try
                {
                    var muData = FetchMU(model, dbContext);
                    result.Add("MU", new
                    {
                        result = muData,
                        response = muData.Count > 0 ? true : false,
                        error = ""
                    });
                }

                catch (Exception ex)
                {
                    result.Add("MU", new
                    {
                        result = new object(),
                        response = false,
                        error = ex.Message
                    });
                }
            }
            if (model.entities.Any(x => x.Equals("SCHEME_ITEM", StringComparison.OrdinalIgnoreCase)))
            {
                try
                {
                    var itemData = FetchAllSchemeItems(model, dbContext, pref);
                    result.Add("SCHEME_ITEM", new
                    {
                        result = itemData,
                        response = itemData.Count > 0 ? true : false,
                        error = ""
                    });
                }

                catch (Exception ex)
                {
                    result.Add("SCHEME_ITEM", new
                    {
                        result = new object[] { },
                        response = false,
                        error = ex.Message
                    });
                }
            }
            if (model.entities.Any(x => x.Equals("SCHEME_GIFT_ITEM", StringComparison.OrdinalIgnoreCase)))
            {
                try
                {
                    var itemData = FetchAllSchemeGiftItems(model, dbContext, pref);
                    result.Add("SCHEME_GIFT_ITEM", new
                    {
                        result = itemData,
                        response = itemData.Count > 0 ? true : false,
                        error = ""
                    });
                }

                catch (Exception ex)
                {
                    result.Add("SCHEME_GIFT_ITEM", new
                    {
                        result = new object[] { },
                        response = false,
                        error = ex.Message
                    });
                }
            }
            //Rate of item per branch
            if (model.entities.Any(x => x.Equals("RATE", StringComparison.OrdinalIgnoreCase)))
            {
                try
                {
                    var rateData = FetchAllCompanyBranchItemRate(model, dbContext, pref);
                    result.Add("RATE", new
                    {
                        result = rateData,
                        response = rateData.Count > 0 ? true : false,
                        error = ""
                    });
                }

                catch (Exception ex)
                {
                    result.Add("RATE", new
                    {
                        result = new object[] { },
                        response = false,
                        error = ex.Message
                    });
                }
            }

            //Measurement Units
            if (model.entities.Any(x => x.Equals("MU", StringComparison.OrdinalIgnoreCase)))
            {
                try
                {
                    var muData = FetchMU(model, dbContext);
                    result.Add("MU", new
                    {
                        result = muData,
                        response = muData.Count > 0 ? true : false,
                        error = ""
                    });
                }

                catch (Exception ex)
                {
                    result.Add("MU", new
                    {
                        result = new object[] { },
                        response = false,
                        error = ex.Message
                    });
                }
            }

            //Outlets
            if (model.entities.Any(x => x.Equals("OUTLET", StringComparison.OrdinalIgnoreCase)))
            {
                try
                {
                    var outletData = FetchOutlets(model, dbContext);
                    result.Add("OUTLET", new
                    {
                        result = outletData,
                        response = outletData.Count > 0 ? true : false,
                        error = ""
                    });
                }

                catch (Exception ex)
                {
                    result.Add("OUTLET", new
                    {
                        result = new object[] { },
                        response = false,
                        error = ex.Message
                    });
                }
            }
            //Distributors sashi
            if (model.entities.Any(x => x.Equals("distributor", StringComparison.OrdinalIgnoreCase)))
            {
                try
                {
                    var outletData = FetchOutlets(model, dbContext);
                    result.Add("distributor", new
                    {
                        result = outletData,
                        response = outletData.Count > 0 ? true : false,
                        error = ""
                    });
                }

                catch (Exception ex)
                {
                    result.Add("distributor", new
                    {
                        result = new object(),
                        response = false,
                        error = ex.Message
                    });
                }
            }
            //DIST_UPDATE_TYPE
            if (model.entities.Any(x => x.Equals("UPDATE_STATUS", StringComparison.OrdinalIgnoreCase)))
            {
                //UPDATE_STATUS
                try
                {
                    var updateData = FetchUpdates(model, dbContext);
                    result.Add("UPDATE_STATUS", new
                    {
                        result = updateData,
                        response = updateData.Count > 0 ? true : false,
                        error = ""
                    });
                }

                catch (Exception ex)
                {
                    result.Add("UPDATE_STATUS", new
                    {
                        result = new object[] { },
                        response = false,
                        error = ex.Message
                    });
                }
            }
            //PRIORITY_STATUS
            if (model.entities.Any(x => x.Equals("PRIORITY_STATUS", StringComparison.OrdinalIgnoreCase)))
            {
                try
                {
                    var priorityData = FetchPriorityData(model, dbContext);
                    result.Add("PRIORITY_STATUS", new
                    {
                        result = priorityData,
                        response = priorityData.Count > 0 ? true : false,
                        error = ""
                    });
                }

                catch (Exception ex)
                {
                    result.Add("PRIORITY_STATUS", new
                    {
                        result = new object[] { },
                        response = false,
                        error = ex.Message
                    });
                }
            }

            //COMPLAIN_TYPE
            if (model.entities.Any(x => x.Equals("COMPLAIN_TYPE", StringComparison.OrdinalIgnoreCase)))
            {
                //UPDATE_STATUS
                try
                {
                    var complainData = FetchComplainTypes(model, dbContext);
                    result.Add("COMPLAIN_TYPE", new
                    {
                        result = complainData,
                        response = complainData.Count > 0 ? true : false,
                        error = ""
                    });
                }

                catch (Exception ex)
                {
                    result.Add("COMPLAIN_TYPE", new
                    {
                        result = new object[] { },
                        response = false,
                        error = ex.Message
                    });
                }
            }
            //SERIOUSNESS_TYPES
            if (model.entities.Any(x => x.Equals("SERIOUSNESS_TYPES", StringComparison.OrdinalIgnoreCase)))
            {
                try
                {
                    var seriousnessData = FetchSeriousnessTypes(model, dbContext);
                    result.Add("SERIOUSNESS_TYPES", new
                    {
                        result = seriousnessData,
                        response = seriousnessData.Count > 0 ? true : false,
                        error = ""
                    });
                }

                catch (Exception ex)
                {
                    result.Add("SERIOUSNESS_TYPES", new
                    {
                        result = new object[] { },
                        response = false,
                        error = ex.Message
                    });
                }
            }
            //RETURN_CONDITION_TYPES
            if (model.entities.Any(x => x.Equals("RETURN_CONDITION_TYPES", StringComparison.OrdinalIgnoreCase)))
            {
                try
                {
                    var ReturnConditionData = FetchReturnCondition(model, dbContext);
                    result.Add("RETURN_CONDITION_TYPES", new
                    {
                        result = ReturnConditionData,
                        response = ReturnConditionData.Count > 0 ? true : false,
                        error = ""
                    });
                }

                catch (Exception ex)
                {
                    result.Add("RETURN_CONDITION_TYPES", new
                    {
                        result = new object[] { },
                        response = false,
                        error = ex.Message
                    });
                }
            }
            //General and Tabular Questions Lists
            if (model.entities.Any(x => x.Equals("QUESTION", StringComparison.OrdinalIgnoreCase)))
            {
                try
                {
                    var questionReq = new QuestionRequestModel
                    {
                        BRANCH_CODE = model.BRANCH_CODE,
                        COMPANY_CODE = model.COMPANY_CODE,
                        latitude = model.latitude,
                        longitude = model.longitude,
                        sp_code = model.spcode,
                        user_id = model.user_id
                    };
                    if (model.user_type == "B")
                    {
                       questionReq.SetType="'B'";
                    }
                        var questionData = FetchAllQuestions(questionReq, dbContext);
                    result.Add("QUESTION", new
                    {
                        result = questionData,
                        response = questionData != null ? true : false,
                        error = ""
                    });
                }

                catch (Exception ex)
                {
                    result.Add("QUESTION", new
                    {
                        result = new
                        {
                            general = new List<GeneralModel>(),
                            tabular = new Dictionary<string, TabularModel>()
                        },
                        response = false,
                        error = ex.Message
                    });
                }
            }

            //Sub ledger map
            if (model.entities.Any(x => x.Equals("SUB_LEDGER_MAP", StringComparison.OrdinalIgnoreCase)))
            {
                try
                {
                    if (model.user_type == "B")
                        throw new Exception("Branding user");
                    var subLedgerMapData = FetchAllCompanySubLedgerMap(model, dbContext, pref);
                    result.Add("SUB_LEDGER_MAP", new
                    {
                        result = subLedgerMapData,
                        response = subLedgerMapData.Count > 0 ? true : false,
                        error = ""
                    });
                }
                catch (Exception ex)
                {
                    result.Add("SUB_LEDGER_MAP", new
                    {
                        result = new object[] { },
                        response = false,
                        error = ex.Message
                    });
                }
            }

            //All party types
            if (model.entities.Any(x => x.Equals("PARTY_TYPE", StringComparison.OrdinalIgnoreCase)))
            {
                try
                {
                    if (model.user_type == "B")
                        throw new Exception("Branding user");
                    var partyTypeData = FetchAllCompanyPartyType(model, dbContext, pref);
                    result.Add("PARTY_TYPE", new
                    {
                        result = partyTypeData,
                        response = partyTypeData.Count > 0 ? true : false,
                        error = ""
                    });
                }
                catch (Exception ex)
                {
                    result.Add("PARTY_TYPE", new
                    {
                        result = new object[] { },
                        response = false,
                        error = ex.Message
                    });
                }
            }

            //All SA Customers
            if (model.entities.Any(x => x.Equals("SA_CUSTOMER", StringComparison.OrdinalIgnoreCase)))
            {
                try
                {
                    if (model.user_type == "B")
                        throw new Exception("Branding user");
                    var saCustomerData = FetchAllCompanySaCustomer(model, dbContext, pref);
                    result.Add("SA_CUSTOMER", new
                    {
                        result = saCustomerData,
                        response = saCustomerData.Count > 0 ? true : false,
                        error = ""
                    });
                }
                catch (Exception ex)
                {
                    result.Add("SA_CUSTOMER", new
                    {
                        result = new object[] { },
                        response = false,
                        error = ex.Message
                    });
                }
            }

            //All ImageCategory
            if (model.entities.Any(x => x.Equals("IMAGE_CATEGORY", StringComparison.OrdinalIgnoreCase)))
            {
                try
                {
                    var ImageCategories = new List<ImageCategoryModel>();
                    if (model.user_type == "B")
                    {
                        ImageCategories = FetchImageCategory(model, dbContext);
                    }
                    else
                    {
                        ImageCategories = FetchImageDistributionCategory(model, dbContext);
                    }

                    result.Add("IMAGE_CATEGORY", new
                    {
                        result = ImageCategories,
                        response = ImageCategories.Count > 0 ? true : false,
                        error = ""
                    });
                }
                catch (Exception ex)
                {
                    result.Add("IMAGE_CATEGORY", new
                    {
                        result = new object[] { },
                        response = false,
                        error = ex.Message
                    });
                }
            }

            //DistributorItems
            if (model.entities.Any(x => x.Equals("DISTRIBUTOR_ITEMS", StringComparison.OrdinalIgnoreCase)))
            {
                try
                {
                    if (model.user_type == "B")
                        throw new Exception("Branding user");
                    var DistributorItems = FetchDistributorItems(model, dbContext);
                    result.Add("DISTRIBUTOR_ITEMS", new
                    {
                        result = DistributorItems,
                        response = DistributorItems.Count > 0 ? true : false,
                        error = ""
                    });
                }
                catch (Exception ex)
                {
                    result.Add("DISTRIBUTOR_ITEMS", new
                    {
                        result = new object[] { },
                        response = false,
                        error = ex.Message
                    });
                }
            }

            //Reseller Entities
            if (model.entities.Any(x => x.Equals("RESELLER_ENTITIES", StringComparison.OrdinalIgnoreCase)))
            {
                try
                {
                    if (model.user_type == "B")
                        throw new Exception("Branding user");
                    var ResellerEntities = FetchResellerEntity(model, pref, model.spcode, dbContext);
                    result.Add("RESELLER_ENTITIES", new
                    {
                        result = ResellerEntities,
                        response = ResellerEntities.Count > 0 ? true : false,
                        error = ""
                    });
                }
                catch (Exception ex)
                {
                    result.Add("RESELLER_ENTITIES", new
                    {
                        result = new object[] { },
                        response = false,
                        error = ex.Message
                    });
                }
            }

            //DIST Groups
            if (model.entities.Any(x => x.Equals("DIST_GROUPS", StringComparison.OrdinalIgnoreCase)))
            {
                try
                {
                    if (model.user_type == "B")
                        throw new Exception("Branding user");
                    var Groups = GetResellerGroups(model, dbContext);
                    result.Add("DIST_GROUPS", new
                    {
                        result = Groups,
                        response = Groups.Count > 0 ? true : false,
                        error = ""
                    });
                }
                catch (Exception ex)
                {
                    result.Add("DIST_GROUPS", new
                    {
                        result = new object[] { },
                        response = false,
                        error = ex.Message
                    });
                }
            }

            //DIST_NOTIFICATIONS
            if (model.entities.Any(x => x.Equals("DIST_NOTIFICATIONS", StringComparison.OrdinalIgnoreCase)))
            {
                try
                {
                    var Notifications = GetNotifications(model.spcode, dbContext);
                    result.Add("DIST_NOTIFICATIONS", new
                    {
                        result = Notifications,
                        response = Notifications.Count > 0 ? true : false,
                        error = ""
                    });
                }
                catch (Exception ex)
                {
                    result.Add("DIST_NOTIFICATIONS", new
                    {
                        result = new object[] { },
                        response = false,
                        error = ex.Message
                    });
                }
            }

            //COMP_ITEMS
            if (model.entities.Any(x => x.Equals("DIST_COMP_ITEMS", StringComparison.OrdinalIgnoreCase)))
            {
                try
                {
                    var CompItems = GetCompItems(model, dbContext);
                    result.Add("DIST_COMP_ITEMS", new
                    {
                        result = CompItems,
                        response = CompItems.Count > 0 ? true : false,
                        error = ""
                    });
                }
                catch (Exception ex)
                {
                    result.Add("DIST_COMP_ITEMS", new
                    {
                        result = new object[] { },
                        response = false,
                        error = ex.Message
                    });
                }
            }
            //DATE RANGE
            if (model.entities.Any(x => x.Equals("DATE_RANGE", StringComparison.OrdinalIgnoreCase)))
            {
                try
                {
                    var dateData = FetchDateRange(model, dbContext);
                    result.Add("DATE_RANGE", new
                    {
                        result = dateData,
                        response = dateData.Count > 0 ? true : false,
                        error = ""
                    });
                }

                catch (Exception ex)
                {
                    result.Add("DATE_RANGE", new
                    {
                        result = new object[] { },
                        response = false,
                        error = ex.Message
                    });
                }
            }
            //COMP_ITEM_maps
            if (model.entities.Any(x => x.Equals("DIST_COMP_ITEM_MAP", StringComparison.OrdinalIgnoreCase)))
            {
                try
                {
                    var CompItems = GetCompItemMaps(model, dbContext);
                    result.Add("DIST_COMP_ITEM_MAP", new
                    {
                        result = CompItems,
                        response = CompItems.Count > 0 ? true : false,
                        error = ""
                    });
                }
                catch (Exception ex)
                {
                    result.Add("DIST_COMP_ITEM_MAP", new
                    {
                        result = new object[] { },
                        response = false,
                        error = ex.Message
                    });
                }
            }

            //COMP_ITEM_FIELDS
            if (model.entities.Any(x => x.Equals("DIST_COMP_ITEM_FIELDS", StringComparison.OrdinalIgnoreCase)))
            {
                try
                {
                    var CompItemFields = GetCompItemFields(model, dbContext);
                    result.Add("DIST_COMP_ITEM_FIELDS", new
                    {
                        result = CompItemFields,
                        response = CompItemFields.Count > 0 ? true : false,
                        error = ""
                    });
                }
                catch (Exception ex)
                {
                    result.Add("DIST_COMP_ITEM_FIELDS", new
                    {
                        result = new object[] { },
                        response = false,
                        error = ex.Message
                    });
                }
            }

            //DIST_GROUP_MAP
            if (model.entities.Any(x => x.Equals("DIST_GROUP_MAP", StringComparison.OrdinalIgnoreCase)))
            {
                try
                {
                    var groupMaps = GetGroupMaps(model.COMPANY_CODE, dbContext);
                    result.Add("DIST_GROUP_MAP", new
                    {
                        result = groupMaps,
                        response = groupMaps.Count > 0 ? true : false,
                        error = ""
                    });
                }
                catch (Exception ex)
                {
                    result.Add("DIST_GROUP_MAP", new
                    {
                        result = new object[] { },
                        response = false,
                        error = ex.Message
                    });
                }
            }

            var prefs = FetchPreferences(model.COMPANY_CODE, dbContext);
            if (prefs.PO_SALES_TYPE.Trim().ToUpper() == "Y" && model.entities.Any(x => x.Equals("SA_SALES_TYPE", StringComparison.OrdinalIgnoreCase)))
            {
                //All SA sales types
                try
                {
                    var saSalesTypeData = FetchAllCompanySaSalesType(model.COMPANY_CODE, dbContext, pref);
                    result.Add("SA_SALES_TYPE", new
                    {
                        result = saSalesTypeData,
                        response = saSalesTypeData.Count > 0 ? true : false,
                        error = ""
                    });
                }
                catch (Exception ex)
                {
                    result.Add("SA_SALES_TYPE", new
                    {
                        result = new object[] { },
                        response = false,
                        error = ex.Message
                    });
                }
            }

            if (prefs.PO_SHIPPING_ADDRESS.Trim().ToUpper() == "Y" && model.entities.Any(x => x.Equals("SHIPPING_ADDRESS", StringComparison.OrdinalIgnoreCase)))
            {
                //All shipping addresses
                try
                {
                    var ShippingData = FetchShippingAddress(dbContext, pref);
                    result.Add("SHIPPING_ADDRESS", new
                    {
                        result = ShippingData,
                        response = ShippingData.Count > 0 ? true : false,
                        error = ""
                    });
                }
                catch (Exception ex)
                {
                    result.Add("SHIPPING_ADDRESS", new
                    {
                        result = new object[] { },
                        response = false,
                        error = ex.Message
                    });
                }
            }

            //Schemes
            if (model.entities.Any(x => x.Equals("SCHEMES", StringComparison.OrdinalIgnoreCase)))
            {
                try
                {
                    var SchemeData = GetContracts(model, dbContext);
                    result.Add("SCHEMES", new
                    {
                        result = SchemeData,
                        response = SchemeData.Count > 0 ? true : false,
                        error = ""
                    });
                }
                catch (Exception ex)
                {
                    result.Add("SCHEMES", new
                    {
                        result = new object[] { },
                        response = false,
                        error = ex.Message
                    });
                }
            }

            //CRM tasks
            if (model.entities.Any(x => x.Equals("CRM_TASKS", StringComparison.OrdinalIgnoreCase)))
            {
                try
                {
                    var crmTasks = GetCrmTasks(model.spcode, dbContext);
                    result.Add("CRM_TASKS", new
                    {
                        result = crmTasks,
                        response = crmTasks.Count > 0 ? true : false,
                        error = ""
                    });
                }
                catch (Exception ex)
                {
                    result.Add("CRM_TASKS", new
                    {
                        result = new object[] { },
                        response = false,
                        error = ex.Message
                    });
                }
            }

            return result;
        }



        public List<SPEntityModel> FetchSpPartyType(VisitPlanRequestModel model, NeoErpCoreEntity dbContext)
        {
            var Query = $@"SELECT
                DM.DEALER_CODE AS CODE,
                TRIM(PT.PARTY_TYPE_EDESC) AS NAME,
                TRIM(PT.ACC_CODE) ACC_CODE,
                DM.CONTACT_NO AS P_CONTACT_NO, DM.REG_OFFICE_ADDRESS AS ADDRESS,
                RM.ROUTE_CODE, RM.ROUTE_NAME,
                AM.AREA_CODE, AM.AREA_NAME,
                RE.ORDER_NO,
                RM.COMPANY_CODE,
                'dealer' AS TYPE,
                '' DEFAULT_PARTY_TYPE_CODE,
                '' PARENT_DISTRIBUTOR_CODE,
                '' PARENT_DISTRIBUTOR_NAME,
                NVL(DM.LATITUDE,0) LATITUDE, NVL(DM.LONGITUDE,0) LONGITUDE,
                LT.LAST_VISIT_DATE, LT.LAST_VISIT_BY, NVL(LT.LAST_VISIT_STATUS, 'X') LAST_VISIT_STATUS, NVL(LT.IS_VISITED, 'X') AS IS_VISITED, LT.REMARKS
                FROM DIST_ROUTE_MASTER RM
                INNER JOIN (SELECT ROUTE_CODE, EMP_CODE, COMPANY_CODE FROM DIST_ROUTE_DETAIL WHERE DELETED_FLAG = 'N' GROUP BY ROUTE_CODE, EMP_CODE, COMPANY_CODE) RD ON RD.ROUTE_CODE = RM.ROUTE_CODE AND RD.COMPANY_CODE = RM.COMPANY_CODE
                INNER JOIN DIST_ROUTE_ENTITY RE ON RE.ROUTE_CODE = RD.ROUTE_CODE AND RE.ENTITY_TYPE = 'P' AND RE.COMPANY_CODE = RM.COMPANY_CODE
                INNER JOIN DIST_DEALER_MASTER DM ON DM.DEALER_CODE = RE.ENTITY_CODE AND DM.COMPANY_CODE = RM.COMPANY_CODE
                INNER JOIN IP_PARTY_TYPE_CODE PT ON PT.PARTY_TYPE_CODE = DM.DEALER_CODE AND PT.COMPANY_CODE = DM.COMPANY_CODE
                INNER JOIN DIST_AREA_MASTER AM ON AM.AREA_CODE = DM.AREA_CODE AND AM.COMPANY_CODE = DM.COMPANY_CODE
                LEFT JOIN (SELECT A.SP_CODE, A.CUSTOMER_CODE, A.CUSTOMER_TYPE, A.COMPANY_CODE, C.EMPLOYEE_EDESC AS LAST_VISIT_BY, A.IS_VISITED AS LAST_VISIT_STATUS, 
                          (CASE 
                            WHEN A.IS_VISITED IS NULL THEN 'X' 
                              ELSE
                                CASE WHEN TO_CHAR(SYSDATE, 'DD-MON-RRRR') = TO_CHAR(A.UPDATE_DATE, 'DD-MON-RRRR') THEN A.IS_VISITED ELSE 'X' END
                           END
                          ) IS_VISITED, 
                          A.REMARKS,
                          TO_CHAR(MAX(A.UPDATE_DATE), 'DD-MON-RRRR HH24:MI A.M.') LAST_VISIT_DATE
                          FROM DIST_LOCATION_TRACK A
                          INNER JOIN IP_PARTY_TYPE_CODE B ON B.PARTY_TYPE_CODE = A.CUSTOMER_CODE AND B.COMPANY_CODE = A.COMPANY_CODE
                          INNER JOIN HR_EMPLOYEE_SETUP C ON C.EMPLOYEE_CODE = A.SP_CODE AND C.COMPANY_CODE = A.COMPANY_CODE
                          WHERE 1 = 1
                          -- AND SP_CODE = '1000097' -- Commented out because customer can be visited by another SP_CODE
                          AND A.CUSTOMER_TYPE = 'P'
                          AND A.UPDATE_DATE = (SELECT MAX(UPDATE_DATE) FROM DIST_LOCATION_TRACK WHERE CUSTOMER_CODE = A.CUSTOMER_CODE AND CUSTOMER_TYPE = A.CUSTOMER_TYPE)
                          GROUP BY A.SP_CODE, A.CUSTOMER_CODE, A.CUSTOMER_TYPE, A.COMPANY_CODE, C.EMPLOYEE_EDESC, A.IS_VISITED, 
                              (CASE 
                                WHEN A.IS_VISITED IS NULL THEN 'X' 
                                  ELSE
                                    CASE WHEN TO_CHAR(SYSDATE, 'DD-MON-RRRR') = TO_CHAR(A.UPDATE_DATE, 'DD-MON-RRRR') THEN A.IS_VISITED ELSE 'X' END
                               END
                              ), 
                              A.REMARKS
                          ) LT
                          ON LT.CUSTOMER_CODE = DM.DEALER_CODE AND LT.COMPANY_CODE = RM.COMPANY_CODE
                WHERE 1 = 1
                --AND TO_CHAR(RD.ASSIGN_DATE, 'DD-MON-RRRR') = UPPER('23-MAY-2017')
                AND RD.EMP_CODE = '{model.spcode}'
                AND RD.COMPANY_CODE = '{model.COMPANY_CODE}'
                GROUP BY 
                  DM.DEALER_CODE,
                  TRIM(PT.PARTY_TYPE_EDESC),
                  TRIM(PT.ACC_CODE),
                  DM.CONTACT_NO, DM.REG_OFFICE_ADDRESS,
                  RM.ROUTE_CODE, RM.ROUTE_NAME,
                  AM.AREA_CODE, AM.AREA_NAME,
                  RE.ORDER_NO,
                  RM.COMPANY_CODE,
                  'dealer',
                  '',
                  '',
                  '',
                  NVL(DM.LATITUDE,0), NVL(DM.LONGITUDE,0),
                  LT.LAST_VISIT_DATE, LT.LAST_VISIT_BY, NVL(LT.LAST_VISIT_STATUS, 'X'), NVL(LT.IS_VISITED, 'X'), LT.REMARKS
                ORDER BY UPPER(TRIM(PT.PARTY_TYPE_EDESC)) ASC";

            var data = dbContext.SqlQuery<SPEntityModel>(Query).ToList();
            return data;
        }

        public List<SPEntityModel> FetchSpCustomer(VisitPlanRequestModel model, NeoErpCoreEntity dbContext)
        {
            var Query = $@"SELECT
                DM.DISTRIBUTOR_CODE AS CODE,
                TRIM(CS.CUSTOMER_EDESC) AS NAME,
                TRIM(CS.ACC_CODE) AS ACC_CODE,
                DM.CONTACT_NO AS P_CONTACT_NO, NVL(DM.REG_OFFICE_ADDRESS, CS.REGD_OFFICE_EADDRESS) AS ADDRESS,
                RM.ROUTE_CODE, RM.ROUTE_NAME,
                AM.AREA_CODE, AM.AREA_NAME,
                RE.ORDER_NO,
                RM.COMPANY_CODE,
                'distributor' AS TYPE,
                CS.PARTY_TYPE_CODE AS DEFAULT_PARTY_TYPE_CODE,
                '' PARENT_DISTRIBUTOR_CODE,
                '' PARENT_DISTRIBUTOR_NAME,
                NVL(DM.LATITUDE,0) LATITUDE, NVL(DM.LONGITUDE,0) LONGITUDE,
                LT.LAST_VISIT_DATE, LT.LAST_VISIT_BY, NVL(LT.LAST_VISIT_STATUS, 'X') LAST_VISIT_STATUS, NVL(LT.IS_VISITED, 'X') AS IS_VISITED, LT.REMARKS
                FROM DIST_ROUTE_MASTER RM
                INNER JOIN DIST_ROUTE_DETAIL RD ON RD.ROUTE_CODE = RM.ROUTE_CODE AND RD.COMPANY_CODE = RM.COMPANY_CODE
                INNER JOIN DIST_ROUTE_ENTITY RE ON RE.ROUTE_CODE = RD.ROUTE_CODE AND RE.ENTITY_TYPE = 'D' AND RE.COMPANY_CODE = RM.COMPANY_CODE
                INNER JOIN DIST_DISTRIBUTOR_MASTER DM ON DM.DISTRIBUTOR_CODE = RE.ENTITY_CODE AND DM.COMPANY_CODE = RM.COMPANY_CODE
                INNER JOIN SA_CUSTOMER_SETUP CS ON CS.CUSTOMER_CODE = DM.DISTRIBUTOR_CODE AND CS.COMPANY_CODE = DM.COMPANY_CODE
                INNER JOIN DIST_AREA_MASTER AM ON AM.AREA_CODE = DM.AREA_CODE AND AM.COMPANY_CODE = DM.COMPANY_CODE
                LEFT JOIN (SELECT A.SP_CODE, A.CUSTOMER_CODE, A.CUSTOMER_TYPE, A.COMPANY_CODE, C.EMPLOYEE_EDESC AS LAST_VISIT_BY, A.IS_VISITED AS LAST_VISIT_STATUS, 
                          (CASE 
                            WHEN A.IS_VISITED IS NULL THEN 'X' 
                              ELSE
                                CASE WHEN TO_CHAR(SYSDATE, 'DD-MON-RRRR') = TO_CHAR(A.UPDATE_DATE, 'DD-MON-RRRR') THEN A.IS_VISITED ELSE 'X' END
                           END
                          ) IS_VISITED, 
                          A.REMARKS,
                          TO_CHAR(MAX(A.UPDATE_DATE), 'DD-MON-RRRR HH24:MI A.M.') LAST_VISIT_DATE
                          FROM DIST_LOCATION_TRACK A
                          INNER JOIN SA_CUSTOMER_SETUP B ON B.CUSTOMER_CODE = A.CUSTOMER_CODE AND B.COMPANY_CODE = A.COMPANY_CODE
                          INNER JOIN HR_EMPLOYEE_SETUP C ON C.EMPLOYEE_CODE = A.SP_CODE AND C.COMPANY_CODE = A.COMPANY_CODE
                          WHERE 1 = 1
                          -- AND SP_CODE = '1000097' -- Commented out because customer can be visited by another SP_CODE
                          AND A.CUSTOMER_TYPE = 'D'
                          AND A.UPDATE_DATE = (SELECT MAX(UPDATE_DATE) FROM DIST_LOCATION_TRACK WHERE CUSTOMER_CODE = A.CUSTOMER_CODE AND CUSTOMER_TYPE = A.CUSTOMER_TYPE)
                          GROUP BY A.SP_CODE, A.CUSTOMER_CODE, A.CUSTOMER_TYPE, A.COMPANY_CODE, C.EMPLOYEE_EDESC, A.IS_VISITED, 
                              (CASE 
                                WHEN A.IS_VISITED IS NULL THEN 'X' 
                                  ELSE
                                    CASE WHEN TO_CHAR(SYSDATE, 'DD-MON-RRRR') = TO_CHAR(A.UPDATE_DATE, 'DD-MON-RRRR') THEN A.IS_VISITED ELSE 'X' END
                               END
                              ), 
                              A.REMARKS
                          ) LT
                          ON LT.CUSTOMER_CODE = DM.DISTRIBUTOR_CODE AND LT.COMPANY_CODE = RM.COMPANY_CODE
                WHERE 1 = 1
                --AND TO_CHAR(RD.ASSIGN_DATE, 'DD-MON-RRRR') = UPPER(''23-MAY-2017'')
                AND RD.EMP_CODE = '{model.spcode}'
                AND RD.COMPANY_CODE = '{model.COMPANY_CODE}'
                AND DM.ACTIVE = 'Y' AND DM.DELETED_FLAG = 'Y'
                GROUP BY 
                  DM.DISTRIBUTOR_CODE,
                  TRIM(CS.CUSTOMER_EDESC),
                  TRIM(CS.ACC_CODE),
                  DM.CONTACT_NO, NVL(DM.REG_OFFICE_ADDRESS, CS.REGD_OFFICE_EADDRESS),
                  RM.ROUTE_CODE, RM.ROUTE_NAME,
                  AM.AREA_CODE, AM.AREA_NAME,
                  RE.ORDER_NO,
                  RM.COMPANY_CODE,
                  'dealer',
                  CS.PARTY_TYPE_CODE,
                  '',
                  '',
                  NVL(DM.LATITUDE,0), NVL(DM.LONGITUDE,0),
                  LT.LAST_VISIT_DATE, LT.LAST_VISIT_BY, NVL(LT.LAST_VISIT_STATUS, 'X'), NVL(LT.IS_VISITED, 'X'), LT.REMARKS
                ORDER BY UPPER(TRIM(CS.CUSTOMER_EDESC)) ASC";

            var data = dbContext.SqlQuery<SPEntityModel>(Query).ToList();
            return data;
        }

        public List<ImageCategoryModel> FetchImageCategory(CommonRequestModel model, NeoErpCoreEntity dbContext)
        {
            string Query = $@"SELECT CATEGORYID,CATEGORY_CODE,CATEGORY_EDESC,MAX_ITEMS,COMPANY_CODE FROM DIST_IMAGE_CATEGORY WHERE COMPANY_CODE='{model.COMPANY_CODE}' AND CATEGORYID<>'1' AND SELECTEDTYPE='B'";
            var data = dbContext.SqlQuery<ImageCategoryModel>(Query).ToList();
            return data;
        }

        public List<ImageCategoryModel> FetchImageDistributionCategory(CommonRequestModel model, NeoErpCoreEntity dbContext)
        {
            string Query = $@"SELECT CATEGORYID,CATEGORY_CODE,CATEGORY_EDESC,MAX_ITEMS,COMPANY_CODE FROM DIST_IMAGE_CATEGORY WHERE COMPANY_CODE='{model.COMPANY_CODE}' AND CATEGORYID<>'1'";
            var data = dbContext.SqlQuery<ImageCategoryModel>(Query).ToList();
            return data;
        }

        public List<ResellerEntityModel> FetchResellerEntity(CommonRequestModel model, PreferenceModel pref, string spCode, NeoErpCoreEntity dbContext)
        {
            var entityFIlter = "";
            if (pref.SQL_COMPANY_ENTITY == "N")
                entityFIlter = $@" AND REM.AREA_CODE IN (SELECT AREA_CODE FROM DIST_AREA_MASTER WHERE GROUPID IN
                        (SELECT MAPPED_GROUPID FROM DIST_GROUP_MAPPING WHERE GROUPID = 
                        (SELECT GROUPID FROM DIST_LOGIN_USER WHERE SP_CODE = '{spCode}') UNION (SELECT GROUPID FROM DIST_LOGIN_USER WHERE SP_CODE = '{spCode}')))";
            var query = $@"SELECT DRE.RESELLER_CODE,DRE.ENTITY_CODE,DRE.ENTITY_TYPE,DRE.COMPANY_CODE
                FROM DIST_RESELLER_ENTITY DRE
                INNER JOIN DIST_RESELLER_MASTER REM ON DRE.RESELLER_CODE = REM.RESELLER_CODE AND DRE.COMPANY_CODE = REM.COMPANY_CODE
                WHERE DRE.DELETED_FLAG='N'
                AND DRE.COMPANY_CODE='{model.COMPANY_CODE}'
                AND REM.IS_CLOSED = 'N'
                AND DRE.ENTITY_TYPE='D'
                AND REM.ACTIVE = 'Y' {entityFIlter}";
            var data = dbContext.SqlQuery<ResellerEntityModel>(query).ToList();
            return data;
        }
        public List<ResellerEntityModel> FetchResellerEntity(VisitPlanRequestModel model, PreferenceModel pref, string spCode, NeoErpCoreEntity dbContext)
        {
            var entityFIlter = "";
            if (pref.SQL_COMPANY_ENTITY == "N")
                entityFIlter = $@" AND REM.AREA_CODE IN (SELECT AREA_CODE FROM DIST_AREA_MASTER WHERE GROUPID IN
                        (SELECT MAPPED_GROUPID FROM DIST_GROUP_MAPPING WHERE GROUPID = 
                        (SELECT GROUPID FROM DIST_LOGIN_USER WHERE SP_CODE = '{spCode}') UNION (SELECT GROUPID FROM DIST_LOGIN_USER WHERE SP_CODE = '{spCode}')))";
            var query = $@"SELECT DRE.RESELLER_CODE,DRE.ENTITY_CODE,DRE.ENTITY_TYPE,DRE.COMPANY_CODE
                FROM DIST_RESELLER_ENTITY DRE
                INNER JOIN DIST_RESELLER_MASTER REM ON DRE.RESELLER_CODE = REM.RESELLER_CODE AND DRE.COMPANY_CODE = REM.COMPANY_CODE
                LEFT JOIN dist_user_areas dua on  dua.customer_code = dre.entity_code AND dua.company_code = dre.company_code
                WHERE DRE.DELETED_FLAG='N' and dua.deleted_flag = 'N' and dua.sp_code='{model.spcode}'
                AND DRE.COMPANY_CODE='{model.COMPANY_CODE}'
                AND REM.IS_CLOSED = 'N'
                AND DRE.ENTITY_TYPE='D'
                AND REM.ACTIVE = 'Y' {entityFIlter}";
            var data = dbContext.SqlQuery<ResellerEntityModel>(query).ToList();
            return data;
        }
        public List<DistributorItemModel> FetchDistributorItems(CommonRequestModel model, NeoErpCoreEntity dbContext)
        {
            var data = dbContext.SqlQuery<DistributorItemModel>($"SELECT DISTRIBUTOR_CODE,ITEM_CODE,COMPANY_CODE FROM DIST_DISTRIBUTOR_ITEM WHERE DELETED_FLAG='N' AND COMPANY_CODE='{model.COMPANY_CODE}'").ToList();
            return data;
        }
        public List<DistributorItemModel> FetchDistributorItems(VisitPlanRequestModel model, NeoErpCoreEntity dbContext)
        {
            var query = $@"SELECT a.distributor_code,a.item_code,a.company_code
                FROM dist_distributor_item a left join DIST_USER_ITEM_MAPPING b on a.item_code=b.item_code and a.company_code=b.company_code
                WHERE a.deleted_flag = 'N' and b.deleted_flag = 'N' and b.sp_code='{model.spcode}' AND a.company_code in  '{model.COMPANY_CODE}'";
            var data = dbContext.SqlQuery<DistributorItemModel>(query).ToList();
            return data;
        }
        public List<ResellerGroupModel> GetResellerGroups(CommonRequestModel model, NeoErpCoreEntity dbContext)
        {
            string Query = $"SELECT GROUPID,GROUP_EDESC,GROUP_CODE FROM DIST_GROUP_MASTER WHERE DELETED_FLAG='N' AND COMPANY_CODE='{model.COMPANY_CODE}' ORDER BY TRIM(GROUP_EDESC) ASC";
            var list = dbContext.SqlQuery<ResellerGroupModel>(Query).ToList();
            return list;
        }

        public List<ContractModel> GetContracts(CommonRequestModel model, NeoErpCoreEntity dbContext)
        {
            var query = $@"SELECT CON.CONTRACT_CODE,CON.SUPPLIER_CODE,SS.SUPPLIER_EDESC,CON.CUSTOMER_CODE,CS.CUSTOMER_EDESC,CON.CONTRACT_EDESC,
                                  CON.BRAND_CODE,CON.BRANDING_TYPE,CON.SPROVIDER_CODE,CON.START_DATE,CON.END_DATE,CON.AREA_CODE,
                                  CON.CONTRACT_TYPE,CON.AMOUNT_TYPE,CON.AMOUNT,CON.NEXT_PAYMENT_DATE,CON.PAYMENT_DATE,CON.ADVANCE_AMOUNT,
                                  CON.CONTRACTOR_DESIGNATION,CON.CONTRACTOR_NAME,CON.CONTRACTOR_ADDRESS,CON.CONTRACTOR_EMAIL,CON.CONTRACTOR_PHONE,
                                  CON.CONTRACTOR_MOBILE,CON.CONTRACTOR_PAN_NO,CON.CONTRACTOR_VAT_NO,CON.OWNER_NAME,CON.OWNER_ADDRESS,CON.OWNER_PHONE,
                                  CON.OWNER_MOBILE,CON.OWNER_COMPANY_NAME,CON.OWNER_PAN_NO,CON.OWNER_VAT_NO,CON.JOB_ORDER_NO,CON.DESCRIPTION,
                                  CON.REMARKS,CON.COMPANY_CODE,CON.BRANCH_CODE,CON.SET_CODE,CON.IS_ROUTE_PLAN
                        FROM BRD_CONTRACT CON
                        LEFT JOIN SA_CUSTOMER_SETUP CS ON CON.CUSTOMER_CODE=CS.CUSTOMER_CODE AND CON.COMPANY_CODE=CS.COMPANY_CODE
                        LEFT JOIN IP_SUPPLIER_SETUP SS ON CON.SUPPLIER_CODE=SS.SUPPLIER_CODE AND CON.COMPANY_CODE=SS.COMPANY_CODE
                        WHERE SYSDATE BETWEEN CON.START_DATE AND CON.END_DATE
                                AND CON.AMOUNT_TYPE='SCHEME_ITEM'
                                --AND CON.APPROVED_FLAG='Y'
                                AND CON.DELETED_FLAG='N'
                                AND CON.COMPANY_CODE='{model.COMPANY_CODE}'
                                AND CON.BRANCH_CODE='{model.BRANCH_CODE}'";
            var data = dbContext.SqlQuery<ContractModel>(query).ToList();

            var QuestionReq = new QuestionRequestModel
            {
                SetType = "S",
                user_id = model.user_id,
                COMPANY_CODE = model.COMPANY_CODE,
                BRANCH_CODE = model.BRANCH_CODE,

            };
            foreach (var item in data)
            {
                if (item.SET_CODE != null)
                {
                    item.General = GetQuestion(QuestionReq, item.SET_CODE.ToString(), dbContext);
                }
                var items = dbContext.SqlQuery<string>($"SELECT ITEM_CODE FROM BRD_CONTRACT_SCHEME_ITEM WHERE CONTRACT_CODE='{item.CONTRACT_CODE}' AND COMPANY_CODE='{item.COMPANY_CODE}'"); 
                var giftItems = dbContext.SqlQuery<string>($"SELECT ITEM_CODE FROM BRD_CONTRACT_ITEMS WHERE CONTRACT_CODE='{item.CONTRACT_CODE}' AND COMPANY_CODE='{item.COMPANY_CODE}'");
                var resellers= dbContext.SqlQuery<string>($"SELECT RESELLER_CODE FROM BRD_CONTRACT_RESELLER WHERE CONTRACT_CODE='{item.CONTRACT_CODE}' AND COMPANY_CODE='{item.COMPANY_CODE}'").ToList();
                item.ITEM_CODE = string.Join(",", items);
                item.GIFT_ITEM_CODE = string.Join(",", giftItems);
                item.RESELLER_CODE = string.Join(",", resellers);
            }
            return data;
        }
        #endregion Fetching Data

        #region Inserting Data
        public Dictionary<string, string> UpdateMyLocation(UpdateRequestModel model, NeoErpCoreEntity dbContext)
        {
            var result = new Dictionary<string, string>();
            var query = string.Empty;
            if (string.IsNullOrWhiteSpace(model.sp_code))
            {
                throw new Exception("Salesperson code missing.");
            }
            if (string.IsNullOrWhiteSpace(model.latitude))
            {
                throw new Exception("Latitude missing.");
            }
            if (string.IsNullOrWhiteSpace(model.longitude))
            {
                throw new Exception("Longitude missing.");
            }
            if (string.IsNullOrWhiteSpace(model.customer_code))
            {
                throw new Exception("Distributor/Reseller code missing.");
            }
            model.is_visited = (string.IsNullOrWhiteSpace(model.is_visited)) ? "Y" : model.is_visited;
            model.destination = model.destination ?? "X";

            if (model.customer_type.Equals("P", StringComparison.OrdinalIgnoreCase) || model.customer_type.Equals("DEALER", StringComparison.OrdinalIgnoreCase))
                model.customer_type = "P";
            else if (model.customer_type.Equals("D", StringComparison.OrdinalIgnoreCase) || model.customer_type.Equals("DISTRIBUTOR", StringComparison.OrdinalIgnoreCase))
                model.customer_type = "D";
            else if (model.customer_type.Equals("R", StringComparison.OrdinalIgnoreCase) || model.customer_type.Equals("RESELLER", StringComparison.OrdinalIgnoreCase))
                model.customer_type = "R";
            else
                throw new Exception("Invalid customer type");

            var time = model.Saved_Date ?? DateTime.Now.ToString("MM/dd/yyyy HH:mm:ss");

            query = $@"INSERT INTO DIST_LOCATION_TRACK (SP_CODE, UPDATE_DATE, LATITUDE, LONGITUDE, DESTINATION, CUSTOMER_CODE, CUSTOMER_TYPE, REMARKS, IS_VISITED,COMPANY_CODE,BRANCH_CODE,SYNC_ID)
            VALUES ('{model.sp_code}', TO_DATE('{time}', 'MM/dd/yyyy hh24:mi:ss'), '{model.latitude}', '{model.longitude}', '{model.destination}', '{model.customer_code}',
            '{model.customer_type}', '{model.remarks.Replace("'", "''")}', '{model.is_visited}', '{model.COMPANY_CODE}', '{model.BRANCH_CODE}','{model.Sync_Id}')";
            Random _r = new Random();
            var row = dbContext.ExecuteSqlCommand(query);
            if (row > 0)
            {
                if (string.IsNullOrWhiteSpace(model.Sync_Id))
                    model.Sync_Id = _r.Next().ToString();
                result.Add(model.Sync_Id, model.sp_code);
                return result;
            }
            else
                throw new Exception("Unable to update location.");
        }

        public Dictionary<string, string> UpdateCurrentLocation(UpdateRequestModel model, NeoErpCoreEntity dbContext)
        {
            //validate sp_code
            var query = $"SELECT * FROM HR_EMPLOYEE_SETUP where EMPLOYEE_CODE='{model.sp_code}' AND COMPANY_CODE='{model.COMPANY_CODE}'";
            var user = dbContext.SqlQuery<object>(query).FirstOrDefault();
            if (user == null)
                throw new Exception("Invalid User!!!");
            var insertQuery = $@"INSERT INTO DIST_LM_LOCATION_TRACKING (SP_CODE,SUBMIT_DATE,LATITUDE, LONGITUDE,COMPANY_CODE,BRANCH_CODE,SYNC_ID) VALUES 
                            ('{model.sp_code}',TO_DATE('{model.Saved_Date}','MM/DD/YYYY HH24:MI:SS'),'{model.latitude}','{model.longitude}','{model.COMPANY_CODE}','{model.BRANCH_CODE}','{model.Sync_Id}')";
            var row = dbContext.ExecuteSqlCommand(insertQuery);
            var result = new Dictionary<string, string>();
            result.Add(model.Sync_Id, model.sp_code);
            return result;
        }

        public Dictionary<string, string> SaveExtraActivity(UpdateRequestModel model, NeoErpCoreEntity dbContext)
        {
            var query = string.Empty;
            if (string.IsNullOrWhiteSpace(model.sp_code))
            {
                throw new Exception("Salesperson code missing.");
            }
            if (string.IsNullOrWhiteSpace(model.latitude))
            {
                throw new Exception("Latitude missing.");
            }
            if (string.IsNullOrWhiteSpace(model.longitude))
            {
                throw new Exception("Longitude missing.");
            }
            if (string.IsNullOrWhiteSpace(model.remarks))
            {
                throw new Exception("Remark is null.");
            }
            query = $@"INSERT INTO DIST_EXTRA_ACTIVITY (SP_CODE,LATITUDE,LONGITUDE,REMARKS,COMPANY_CODE,BRANCH_CODE,SYNC_ID) VALUES(
                    '{model.sp_code}','{model.latitude}','{model.longitude}','{model.remarks.Replace("'", "''")}','{model.COMPANY_CODE}','{model.BRANCH_CODE}','{model.Sync_Id}')";
            var row = dbContext.ExecuteSqlCommand(query);
            if (row > 0)
            {
                var result = new Dictionary<string, string>();
                result.Add(model.Sync_Id, model.sp_code);
                return result;
            }
            else
                throw new Exception("Error Processing Request");
        }

        public Dictionary<string, string> UpdateCustomerLocation(UpdateCustomerRequestModel model, NeoErpCoreEntity dbContext)
        {
            var query = string.Empty;
            int a = 0;
            a++;
            if (string.IsNullOrWhiteSpace(model.user_id))
            {
                throw new Exception("User ID missing.");
            }
            if (string.IsNullOrWhiteSpace(model.code))
            {
                throw new Exception("Customer code missing.");
            }
            if (string.IsNullOrWhiteSpace(model.latitude))
            {
                throw new Exception("Latitude missing.");
            }
            if (string.IsNullOrWhiteSpace(model.longitude))
            {
                throw new Exception("Longitude missing.");
            }
            if (string.IsNullOrWhiteSpace(model.type))
            {
                throw new Exception("Customer type missing.");
            }
            if (model.type.Equals("P", StringComparison.OrdinalIgnoreCase) || model.type.Equals("DEALER", StringComparison.OrdinalIgnoreCase))
                query = $@"UPDATE DIST_DEALER_MASTER SET LUPDATE_BY = '{model.user_id}', LUPDATE_DATE = TO_DATE('{DateTime.Now.ToString("MM/dd/yyyy HH:mm:ss")}','MM/dd/yyyy hh24:mi:ss'),
                LATITUDE = '{model.latitude}', LONGITUDE = '{model.longitude}', SYNC_ID='{model.Sync_Id}' WHERE DEALER_CODE = '{model.code}' AND COMPANY_CODE='{model.COMPANY_CODE}'";
            else if (model.type.Equals("D", StringComparison.OrdinalIgnoreCase) || model.type.Equals("DISTRIBUTOR", StringComparison.OrdinalIgnoreCase))
                query = $@"UPDATE DIST_DISTRIBUTOR_MASTER SET LUPDATE_BY = '{model.user_id}', LUPDATE_DATE = TO_DATE('{DateTime.Now.ToString("MM/dd/yyyy HH:mm:ss")}','MM/dd/yyyy hh24:mi:ss'),
                LATITUDE = '{model.latitude}', LONGITUDE = '{model.longitude}', SYNC_ID='{model.Sync_Id}' WHERE DISTRIBUTOR_CODE = '{model.code}' AND COMPANY_CODE='{model.COMPANY_CODE}'";
            else if (model.type.Equals("R", StringComparison.OrdinalIgnoreCase) || model.type.Equals("RESELLER", StringComparison.OrdinalIgnoreCase))
                query = $@"UPDATE DIST_RESELLER_MASTER SET LUPDATE_BY = '{model.user_id}', LUPDATE_DATE = TO_DATE('{DateTime.Now.ToString("MM/dd/yyyy HH:mm:ss")}','MM/dd/yyyy hh24:mi:ss'),
                LATITUDE = '{model.latitude}', LONGITUDE = '{model.longitude}', SYNC_ID='{model.Sync_Id}' WHERE RESELLER_CODE = '{model.code}' AND COMPANY_CODE='{model.COMPANY_CODE}'";
            else if (model.type.Equals("B", StringComparison.OrdinalIgnoreCase) || model.type.Equals("BRANDING", StringComparison.OrdinalIgnoreCase))
                query = $@"UPDATE BRD_OTHER_ENTITY SET LATITUDE = '{model.latitude}', LONGITUDE = '{model.longitude}', SYNC_ID='{model.Sync_Id}' WHERE CODE = '{model.code}' AND COMPANY_CODE='{model.COMPANY_CODE}'";
            else
                throw new Exception("Invalid customer type");

            var row = dbContext.ExecuteSqlCommand(query);
            if (row > 0)
            {
                var result = new Dictionary<string, string>
                {
                    { model.Sync_Id, model.code }
                };
                return result;
            }
            else
            {
                throw new Exception("Unable to update customer location.");
            }
        }

        public Dictionary<string, string> NewPurchaseOrder(PurchaseOrderModel model, NeoErpCoreEntity dbContext)
        {
            var result = new Dictionary<string, string>();
            if (model.LocationInfo != null)
            {
                model.LocationInfo.remarks = "PO Taken (auto)";
                var locationRes = this.UpdateMyLocation(model.LocationInfo, dbContext);
            }
            int id = 0;
            if (model.type.Equals("P", StringComparison.OrdinalIgnoreCase) || model.type.Equals("DEALER", StringComparison.OrdinalIgnoreCase)
                || model.type.Equals("D", StringComparison.OrdinalIgnoreCase) || model.type.Equals("DISTRIBUTOR", StringComparison.OrdinalIgnoreCase))
                id = this.GetMaxId("DIST_IP_SSD_PURCHASE_ORDER", "ORDER_NO", dbContext);
            else if (model.type.Equals("R", StringComparison.OrdinalIgnoreCase) || model.type.Equals("RESELLER", StringComparison.OrdinalIgnoreCase))
            {
                if (string.IsNullOrWhiteSpace(model.reseller_code))
                    throw new Exception("Reseller code is empty");
                id = this.GetMaxId("DIST_IP_SSR_PURCHASE_ORDER", "ORDER_NO", dbContext);
            }
            else
                throw new Exception("Invalid customer type");
            if (id <= 0)
                throw new Exception("Unable to get next ID for the purchase order.");


            var today = $"TO_DATE('{DateTime.Now.ToString("MM/dd/yyyy HH:mm:ss")}','MM/dd/yyyy hh24:mi:ss')";
            model.Saved_Date = string.IsNullOrWhiteSpace(model.Saved_Date) ? today : $"TO_DATE('{model.Saved_Date}','MM/dd/yyyy hh24:mi:ss')";
            var OrderDate = string.IsNullOrWhiteSpace(model.Order_Date) ? today : $"TO_DATE('{model.Order_Date}','MM/dd/yyyy hh24:mi:ss')";
            var serialnumber = 1;
                    try
                    {
                        foreach (var item in model.products)
                        {
                            item.party_type_code = item.party_type_code ?? "";
                            string InsertQuery = string.Empty;
                            string priceQuery = $"SELECT NVL(SALES_PRICE,0) SALES_PRICE FROM IP_ITEM_MASTER_SETUP WHERE ITEM_CODE = '{item.item_code}' AND COMPANY_CODE='{model.COMPANY_CODE}'";
                            decimal SP = dbContext.SqlQuery<decimal>(priceQuery).FirstOrDefault();
                            item.rate = item.rate == 0 ? SP : item.rate;
                            var total = item.rate * item.quantity;
                            var discount = item.discount + item.discountRate * item.quantity + (item.discountPercentage * total / 100);
                            total = Math.Round(total - discount, 2);

                            if (model.type.Equals("P", StringComparison.OrdinalIgnoreCase) || model.type.Equals("DEALER", StringComparison.OrdinalIgnoreCase)
                                || model.type.Equals("D", StringComparison.OrdinalIgnoreCase) || model.type.Equals("DISTRIBUTOR", StringComparison.OrdinalIgnoreCase))
                            {
                                InsertQuery = $@"INSERT INTO DIST_IP_SSD_PURCHASE_ORDER (ORDER_NO,ORDER_DATE,CUSTOMER_CODE,ITEM_CODE,MU_CODE,QUANTITY,BILLING_NAME,REMARKS,UNIT_PRICE,TOTAL_PRICE,CREATED_BY,CREATED_DATE,APPROVED_FLAG,DISPATCH_FLAG,ACKNOWLEDGE_FLAG,REJECT_FLAG,DELETED_FLAG,PARTY_TYPE_CODE,CITY_CODE,SALES_TYPE_CODE,SHIPPING_CONTACT,COMPANY_CODE,BRANCH_CODE,SYNC_ID,TEMP_ORDER_NO,DISCOUNT,DISCOUNT_RATE,DISCOUNT_PERCENTAGE,PRIORITY_STATUS_CODE)
                                    VALUES('{id}',{OrderDate},'{model.distributor_code}','{item.item_code}','{item.mu_code}','{item.quantity}','{item.billing_name}','{item.remarks}','{item.rate}','{total}','{model.user_id}',{model.Saved_Date},'N','N','N','{item.reject_flag}','N','{item.party_type_code}','{item.CITY_CODE}','{item.SALES_TYPE_CODE}','{item.SHIPPING_CONTACT}','{model.COMPANY_CODE}','{model.BRANCH_CODE}','{item.Sync_Id}','{model.Order_No}','{item.discount}','{item.discountRate}','{item.discountPercentage}','{item.PRIORITY_STATUS_CODE}')";
                                int distInsertResult = dbContext.ExecuteSqlCommand(InsertQuery);

                                if (distInsertResult == 0)
                                {
                                    throw new Exception("Failed to insert!");
                                }
                                var preferences = FetchPreferences(model.COMPANY_CODE, dbContext);
                                if (preferences.SO_SALES_ORDER.Trim().ToUpper() == "Y")
                                {
                                    var voucherCode = "select  FN_NEW_VOUCHER_NO('" + model.COMPANY_CODE + "','" + item.form_code + "',TRUNC(sysdate),'SA_SALES_ORDER') from dual";
                                    var data = dbContext.SqlQuery<string>(voucherCode).FirstOrDefault();
                                    var sessionQuery = "SELECT MYSEQUENCE.NEXTVAL FROM DUAL";
                                    int sessionId = dbContext.SqlQuery<int>(sessionQuery).FirstOrDefault();
                                    if (data == null)
                                        throw new Exception("Sales Order No. Not Generated.Please try once again");
                                    var query = string.Format(@"Insert into SA_SALES_ORDER
                                                                           (ORDER_NO, ORDER_DATE, CUSTOMER_CODE, SERIAL_NO, ITEM_CODE, MU_CODE, QUANTITY, UNIT_PRICE, TOTAL_PRICE, CALC_QUANTITY, CALC_UNIT_PRICE, CALC_TOTAL_PRICE, FORM_CODE, COMPANY_CODE, BRANCH_CODE, CREATED_BY, CREATED_DATE, DELETED_FLAG, DELIVERY_DATE, CURRENCY_CODE, EXCHANGE_RATE, TRACKING_NO, STOCK_BLOCK_FLAG,MODIFY_BY,MODIFY_DATE,PARTY_TYPE_CODE,REMARKS,SESSION_ROWID)
                                                                         Values
                                                                           ('" + data + @"', to_date('" + DateTime.Parse(model.Order_Date).ToString("MM/dd/yyyy") + "','MM/dd/yyyy'), '" + model.distributor_code + @"'," + serialnumber + @",
                                                                            '" + item.item_code + @"', '" + item.mu_code + @"'," + item.quantity + @" , " + item.rate + @", " + total + @",
                                                                           " + item.quantity + @" , " + item.rate + @", " + total + @",
                                                                            '" + item.form_code + @"', '" + model.COMPANY_CODE + @"', '" + model.BRANCH_CODE + @"', UPPER('" + model.login_code + @"'), sysdate,
                                                                            'N', TO_DATE(sysdate), 'NRS', 1,
                                                                            '0', 'N',UPPER('" + model.login_code + @"'), to_date('" + model.Order_Date + @"','MM/dd/yyyy hh24:mi:ss'),'" + item.party_type_code + @"', '" + item.remarks + @"','" + sessionId + @"')");
                                    int distInsert = dbContext.ExecuteSqlCommand(query);
                                    if (distInsert > 0)
                                    {
                                        var masterQuery = @"Insert into MASTER_TRANSACTION
                                               (VOUCHER_NO, VOUCHER_AMOUNT, FORM_CODE, CHECKED_BY, AUTHORISED_BY, POSTED_BY, COMPANY_CODE, BRANCH_CODE, CREATED_BY, CREATED_DATE, DELETED_FLAG, VOUCHER_DATE, CURRENCY_CODE, EXCHANGE_RATE, PRINT_COUNT,PRINT_FLAG,SESSION_ROWID)
                                             Values
                                               ('" + data + @"', " + total + @", '" + item.form_code + @"', '', '',
                                                '', '" + model.COMPANY_CODE + "', '" + model.BRANCH_CODE + "', UPPER('" + model.login_code + @"'),
                                                sysdate, 'N', to_date('" + DateTime.Parse(model.Order_Date).ToString("MM/dd/yyyy") + "','MM/dd/yyyy'),'NRS', 1, 0,'N', '" + sessionId + @"')";
                                        var masterRowaffected = dbContext.ExecuteSqlCommand(masterQuery);
                                    }
                                    var partialUpdate = $@"UPDATE DIST_IP_SSD_PURCHASE_ORDER SET REJECT_FLAG='N',APPROVE_QTY = {item.quantity}, APPROVE_AMT = {total}, QUANTITY={item.quantity},SALES_ORDER_NO='{data}' WHERE ORDER_NO = {id} and ITEM_CODE = {item.item_code}";
                                    var Pupdate = dbContext.ExecuteSqlCommand(partialUpdate);
                                    serialnumber++;
                                }
                            }
                            else
                            {
                                InsertQuery = $@"INSERT INTO DIST_IP_SSR_PURCHASE_ORDER (ORDER_NO,ORDER_DATE,RESELLER_CODE,CUSTOMER_CODE,ITEM_CODE,MU_CODE,QUANTITY,BILLING_NAME,REMARKS,UNIT_PRICE,TOTAL_PRICE,CREATED_BY,CREATED_DATE,APPROVED_FLAG,DISPATCH_FLAG,ACKNOWLEDGE_FLAG,REJECT_FLAG,DELETED_FLAG,PARTY_TYPE_CODE,CITY_CODE,SALES_TYPE_CODE,SHIPPING_CONTACT,COMPANY_CODE,BRANCH_CODE,SYNC_ID,TEMP_ORDER_NO,DISPATCH_FROM,WHOLESELLER_CODE,PRIORITY_STATUS_CODE,DISCOUNT,DISCOUNT_RATE,DISCOUNT_PERCENTAGE)
                                    VALUES('{id}',{OrderDate},'{model.reseller_code}','{model.distributor_code}','{item.item_code}','{item.mu_code}','{item.quantity}','{item.billing_name}','{item.remarks}','{item.rate}','{total}','{model.user_id}',{model.Saved_Date},'N','N','N','N','N','{item.party_type_code}','{item.CITY_CODE}','{item.SALES_TYPE_CODE}','{item.SHIPPING_CONTACT}','{model.COMPANY_CODE}','{model.BRANCH_CODE}','{item.Sync_Id}','{model.Order_No}','{model.Dispatch_From}','{model.WholeSeller_Code}','{item.PRIORITY_STATUS_CODE}','{item.discount}','{item.discountRate}','{item.discountPercentage}')";
                                int rowNum = dbContext.ExecuteSqlCommand(InsertQuery);
                            }
                            result.Add(item.Sync_Id, id.ToString());
                        }
                    }
                    catch (Exception ex)
                    {
                        throw ex;
                    }

          //  UpdateMyLocation
            return result;
        }
        public Dictionary<string, string> CancelPurchaseOrder(CancelPurchaseOrderModal model, NeoErpCoreEntity dbContext)
        {
            var result = new Dictionary<string, string>();
            foreach (var item in model.products)
            {
                
                string updateQuery = $"Update DIST_IP_SSD_PURCHASE_ORDER SET REJECT_FLAG='Y' WHERE ITEM_CODE = '{item.ITEM_CODE}' AND COMPANY_CODE='{model.COMPANY_CODE}' AND ORDER_NO='{model.ORDER_NO}'";
                var updateRes = dbContext.ExecuteSqlCommand(updateQuery);
                result.Add(item.SYNC_ID, model.ORDER_NO);
            }
            return result;
        }

        //public Dictionary<string, string> NewCollection(CollectionRequestModel model, NeoErpCoreEntity dbContext)
        //{
        //    if (string.IsNullOrWhiteSpace(model.sp_code))
        //        throw new Exception("Sp code is empty");
        //    if (string.IsNullOrWhiteSpace(model.entity_type))
        //        throw new Exception("Entity type is empty");
        //    if (string.IsNullOrWhiteSpace(model.created_by))
        //        throw new Exception("Created by is empty");
        //    decimal Amount;
        //    //if (model.LocationInfo != null)
        //    //{
        //    //    model.LocationInfo.remarks = "Collection Received(auto)";
        //    //    var locationRes = this.UpdateMyLocation(model.LocationInfo, dbContext);
        //    //}
        //    string[] types = { "P", "D", "R" };
        //    if (string.IsNullOrWhiteSpace(model.amount) || !decimal.TryParse(model.amount, out Amount))
        //        throw new Exception("Amount should be in Number");
        //    if (!types.Contains(model.entity_type.ToUpper()))
        //        throw new Exception(@"ENITY_TYPE must be 'P' or 'D' or 'R' ");
        //    string insertQuery = $@"INSERT INTO DIST_COLLECTION (SP_CODE,ENTITY_CODE,ENTITY_TYPE,BILL_NO, CHEQUE_NO, BANK_NAME, AMOUNT,PAYMENT_MODE,CHEQUE_CLEARANCE_DATE,CHEQUE_DEPOSIT_BANK,LATITUDE,LONGITUDE,REMARKS,CREATED_BY,DELETED_FLAG,COMPANY_CODE,BRANCH_CODE,SYNC_ID)
        //    VALUES ('{model.sp_code}','{model.entity_code}','{model.entity_type}','{model.bill_no}','{model.cheque_no}','{model.bank_name.Replace("'", "''")}','{model.amount}','{model.payment_mode}',TO_DATE('{model.cheque_clearance_date}','dd-mm-yyyy'),
        //    '{model.cheque_deposit_bank}', '{model.latitude}','{model.longitude}','{model.remarks.Replace("'", "''")}','{model.created_by}','N','{model.COMPANY_CODE}','{model.BRANCH_CODE}','{model.Sync_Id}')";
        //    var row = dbContext.ExecuteSqlCommand(insertQuery);
        //    if (row <= 0)
        //        throw new Exception("Unable to save collection");

        //    var result = new Dictionary<string, string>();
        //    result.Add(model.Sync_Id, model.sp_code);
        //    return result;
        //}
        public Dictionary<string, string> NewCollection(CollectionRequestModel model, NeoErpCoreEntity dbContext)
        {
            if (string.IsNullOrWhiteSpace(model.sp_code))
                throw new Exception("Sp code is empty");
            if (string.IsNullOrWhiteSpace(model.entity_type))
                throw new Exception("Entity type is empty");
            if (string.IsNullOrWhiteSpace(model.created_by))
                throw new Exception("Created by is empty");
            decimal Amount;
            //if (model.LocationInfo != null)
            //{
            //    model.LocationInfo.remarks = "Collection Received(auto)";
            //    var locationRes = this.UpdateMyLocation(model.LocationInfo, dbContext);
            //}
            string[] types = { "P", "D", "R" };
            if (string.IsNullOrWhiteSpace(model.amount) || !decimal.TryParse(model.amount, out Amount))
                throw new Exception("Amount should be in Number");
            if (!types.Contains(model.entity_type.ToUpper()))
                throw new Exception(@"ENITY_TYPE must be 'P' or 'D' or 'R' ");
            string insertQuery = $@"INSERT INTO DIST_COLLECTION (SP_CODE,ENTITY_CODE,ENTITY_TYPE,BILL_NO, CHEQUE_NO, BANK_NAME, AMOUNT,PAYMENT_MODE,CHEQUE_CLEARANCE_DATE,CHEQUE_DEPOSIT_BANK,LATITUDE,LONGITUDE,REMARKS,CREATED_BY,DELETED_FLAG,COMPANY_CODE,BRANCH_CODE,SYNC_ID,OTP_CODE)
            VALUES ('{model.sp_code}','{model.entity_code}','{model.entity_type}','{model.bill_no}','{model.cheque_no}','{model.bank_name.Replace("'", "''")}','{model.amount}','{model.payment_mode}',TO_DATE('{model.cheque_clearance_date}','dd-mm-yyyy'),
            '{model.cheque_deposit_bank}', '{model.latitude}','{model.longitude}','{model.remarks.Replace("'", "''")}','{model.created_by}','N','{model.COMPANY_CODE}','{model.BRANCH_CODE}','{model.Sync_Id}','{model.otp_code}')";
            var row = dbContext.ExecuteSqlCommand(insertQuery);
            if (row <= 0)
                throw new Exception("Unable to save collection");

            var result = new Dictionary<string, string>();
            result.Add(model.Sync_Id, model.sp_code);

            /*sashi*/
            //foreach (string tagName in Files)
            //{
            //    HttpPostedFile file = Files[tagName];
            //    string ChequePath = string.Empty;

            //    var ImageId = this.GetMaxId("DIST_VISIT_IMAGE", "IMAGE_CODE", dbContext);

            //    ChequePath = UploadPath + "\\EntityImages";

            //    if (!Directory.Exists(ChequePath))
            //        Directory.CreateDirectory(ChequePath);
            //    string FileName = string.Format("{0}{1}", model.entity_code, Path.GetExtension(file.FileName));
            //    string filePath = Path.Combine(ChequePath, FileName);
            //    int count = 1;
            //    while (File.Exists(filePath))
            //    {
            //        FileName = string.Format("{0}_{1}{2}", model.entity_code, count++, Path.GetExtension(file.FileName));
            //        filePath = Path.Combine(ChequePath, FileName);
            //    }
            //    string mediaType;
            //    int categoryId = 0;
            //    if (tagName.IndexOf("cheque", StringComparison.OrdinalIgnoreCase) >= 0)
            //    {
            //        mediaType = "CHEQUE";
            //        string chequeQuery = $"SELECT CATEGORYID FROM DIST_IMAGE_CATEGORY WHERE CATEGORY_CODE = 'cheque'";
            //        categoryId = dbContext.SqlQuery<int>(chequeQuery).FirstOrDefault();
            //        file.SaveAs(filePath);
            //    }
            //    else if (tagName.IndexOf("signature", StringComparison.OrdinalIgnoreCase) >= 0)
            //    {
            //        mediaType = "SIGNATURE";
            //        string chequeQuery = $"SELECT CATEGORYID FROM DIST_IMAGE_CATEGORY WHERE CATEGORY_CODE = 'signature'";
            //        categoryId = dbContext.SqlQuery<int>(chequeQuery).FirstOrDefault();
            //        file.SaveAs(filePath);
            //    }
            //    else
            //        continue;
            //    var InsertQuery = $@"INSERT INTO DIST_VISIT_IMAGE (IMAGE_CODE,IMAGE_NAME,IMAGE_TITLE,IMAGE_DESC,SP_CODE,ENTITY_CODE,TYPE,UPLOAD_DATE,LONGITUDE,LATITUDE,CATEGORYID,COMPANY_CODE,BRANCH_CODE,SYNC_ID)
            //                        VALUES ({ImageId}, '{FileName}', '{DBNull.Value}', '{mediaType}', '{model.sp_code}', '{model.entity_code}', '{model.entity_type}',TO_DATE('{model.Saved_Date}','MM/dd/yyyy  HH24:MI:SS'),'{model.longitude}', '{model.latitude}','{categoryId}','{model.COMPANY_CODE}', '{model.BRANCH_CODE}','{model.Sync_Id}')";
            //    row = dbContext.ExecuteSqlCommand(InsertQuery);
            //    //string ImageQuery = $@"INSERT INTO DIST_PHOTO_INFO (FILENAME,DESCRIPTION,ENTITY_TYPE,ENTITY_CODE,MEDIA_TYPE,CREATED_BY,CREATE_DATE,COMPANY_CODE,BRANCH_CODE) VALUES
            //    //            ('{FileName}','{descriptions[tagName]}','R','{ResellerCode}','{mediaType}','{model.user_id}',SYSDATE,'{model.COMPANY_CODE}','{model.BRANCH_CODE}')";
            //    //row = dbContext.ExecuteSqlCommand(ImageQuery);

            //    //var InsertQuery = $@"INSERT INTO DIST_VISIT_IMAGE (IMAGE_CODE,IMAGE_NAME,IMAGE_TITLE,IMAGE_DESC,SP_CODE,ENTITY_CODE,TYPE,UPLOAD_DATE,LONGITUDE,LATITUDE,CATEGORYID,COMPANY_CODE,BRANCH_CODE,SYNC_ID)
            //    //                    VALUES ({ImageId}, '{FileName}', '{DBNull.Value}', '{model.Description.Replace("'", "''")}', '{model.ACC_CODE}', '{model.entity_code}', '{model.entity_type}',TO_DATE('{model.Saved_Date}','MM/dd/yyyy  HH24:MI:SS'),'{model.longitude}', '{model.latitude}','{model.Categoryid}','{model.COMPANY_CODE}', '{model.BRANCH_CODE}','{model.Sync_Id}')";
            //    //row += dbContext.ExecuteSqlCommand(InsertQuery);

            //    //result.Add(model.Sync_Id, ImageId.ToString());



            //}
            /*sashi*/

            return result;
        }

        public Dictionary<string, string> NewMarketingInformation(InformationSaveModel model, NeoErpCoreEntity dbContext)
        {
           
            var today = $"TO_DATE('{DateTime.Now.ToString("MM/dd/yyyy")}','MM/dd/yyyy')";
            var MktCode = this.GetMaxId("DIST_MKT_INFO", "MKT_CODE", dbContext);
            if (model.entity_type.Equals("P", StringComparison.OrdinalIgnoreCase) || model.entity_type.Equals("DEALER", StringComparison.OrdinalIgnoreCase))
                model.entity_type = "P";
            else if (model.entity_type.Equals("D", StringComparison.OrdinalIgnoreCase) || model.entity_type.Equals("DISTRIBUTOR", StringComparison.OrdinalIgnoreCase))
                model.entity_type = "D";
            else if (model.entity_type.Equals("R", StringComparison.OrdinalIgnoreCase) || model.entity_type.Equals("RESELLER", StringComparison.OrdinalIgnoreCase))
                model.entity_type = "R";
            else
                throw new Exception("Invalid customer type");
            string InsertQuery = $@"INSERT INTO DIST_MKT_INFO (MKT_CODE, INFO_TEXT, ENTITY_TYPE, ENTITY_CODE, USER_ID, CREATE_DATE,COMPANY_CODE,BRANCH_CODE,SYNC_ID)
                VALUES ({MktCode}, '{model.information}', '{model.entity_type}', '{model.entity_code}', '{model.user_id}', {today},'{model.COMPANY_CODE}','{model.BRANCH_CODE}','{model.Sync_Id}')";
            var row = dbContext.ExecuteSqlCommand(InsertQuery);
            if (row <= 0)
                throw new Exception("Unable to add marketing information.");

            var result = new Dictionary<string, string>();
            result.Add(model.Sync_Id, MktCode.ToString());
            return result;
        }

        public Dictionary<string, string> NewCompetitorInformation(InformationSaveModel model, NeoErpCoreEntity dbContext)
        {
            var today = $"TO_DATE('{DateTime.Now.ToString("MM/dd/yyyy")}','MM/dd/yyyy')";
            var ComptCode = this.GetMaxId("DIST_COMPT_INFO", "COMPT_CODE", dbContext);
            if (model.entity_type.Equals("P", StringComparison.OrdinalIgnoreCase) || model.entity_type.Equals("DEALER", StringComparison.OrdinalIgnoreCase))
                model.entity_type = "P";
            else if (model.entity_type.Equals("D", StringComparison.OrdinalIgnoreCase) || model.entity_type.Equals("DISTRIBUTOR", StringComparison.OrdinalIgnoreCase))
                model.entity_type = "D";
            else if (model.entity_type.Equals("R", StringComparison.OrdinalIgnoreCase) || model.entity_type.Equals("RESELLER", StringComparison.OrdinalIgnoreCase))
                model.entity_type = "R";
            else
                throw new Exception("Invalid customer type");
            string InsertQuery = $@"INSERT INTO DIST_COMPT_INFO (COMPT_CODE, INFO_TEXT, ENTITY_TYPE, ENTITY_CODE, USER_ID, CREATE_DATE,COMPANY_CODE,BRANCH_CODE,SYNC_ID)
                VALUES ({ComptCode}, '{model.information}', '{model.entity_type}', '{model.entity_code}', '{model.user_id}', {today},'{model.COMPANY_CODE}','{model.BRANCH_CODE}','{model.Sync_Id}')";
            var row = dbContext.ExecuteSqlCommand(InsertQuery);
            if (row <= 0)
                throw new Exception("Unable to add competitor information.");

            var result = new Dictionary<string, string>();
            result.Add(model.Sync_Id, ComptCode.ToString());
            return result;
        }

        public Dictionary<string, Dictionary<string, string>> SaveQuestionaire(QuestionaireSaveModel model, NeoErpCoreEntity dbContext)
        {
            var result = new Dictionary<string, Dictionary<string, string>>();
            result["tabular"] = new Dictionary<string, string>();
            result["general"] = new Dictionary<string, string>();
            var today = string.IsNullOrWhiteSpace(model.Saved_Date) ? $"TO_DATE('{DateTime.Now.ToString("MM/dd/yyyy")}','MM/dd/yyyy')" : $"TO_DATE('{model.Saved_Date}','MM/dd/yyyy HH24:MI:SS')";
            var ComptCode = this.GetMaxId("DIST_COMPT_INFO", "COMPT_CODE", dbContext);
            if (model.entity_type.Equals("P", StringComparison.OrdinalIgnoreCase) || model.entity_type.Equals("DEALER", StringComparison.OrdinalIgnoreCase))
                model.entity_type = "P";
            else if (model.entity_type.Equals("D", StringComparison.OrdinalIgnoreCase) || model.entity_type.Equals("DISTRIBUTOR", StringComparison.OrdinalIgnoreCase))
                model.entity_type = "D";
            else if (model.entity_type.Equals("R", StringComparison.OrdinalIgnoreCase) || model.entity_type.Equals("RESELLER", StringComparison.OrdinalIgnoreCase))
                model.entity_type = "R";
            else if (model.entity_type.Equals("B", StringComparison.OrdinalIgnoreCase) || model.entity_type.Equals("BRANDING", StringComparison.OrdinalIgnoreCase))
                model.entity_type = "B";
            else
                throw new Exception("Invalid customer type");
            var AnsQuery = $"SELECT QA_CODE,ANSWER FROM DIST_QA_ANSWER WHERE CREATED_DATE ='{DateTime.Now.ToString("dd-MMM-yyyy")}' AND ENTITY_CODE='{model.entity_code}' AND ENTITY_TYPE='{model.entity_type}' AND DELETED_FLAG='N' AND COMPANY_CODE='{model.COMPANY_CODE}'";
            var answers = dbContext.SqlQuery<object>(AnsQuery).ToList();
            if (answers.Count > 0)
                throw new Exception("You Have Already Answers These Questionaire For The day!!");
            foreach (var general in model.general)
            {
                var InsertQuery = $@"INSERT INTO DIST_QA_ANSWER (SP_CODE, QA_CODE,ANSWER,ENTITY_TYPE,ENTITY_CODE,DELETED_FLAG,CREATED_DATE,CREATED_BY,COMPANY_CODE,BRANCH_CODE,SYNC_ID)
                    VALUES('{model.sp_code}','{general.qa_code}','{general.answer}','{model.entity_type}','{model.entity_code}','N',{today},'{model.entity_code}','{model.COMPANY_CODE}','{model.BRANCH_CODE}','{general.Sync_Id}')";
                var row = dbContext.ExecuteSqlCommand(InsertQuery);
                result["general"].Add(general.Sync_Id, model.sp_code);
                if (row <= 0)
                    throw new Exception("Error Processing Request.");
            }
            foreach (var tab in model.tabular)
            {

                var AnsId = this.GetMaxId("DIST_QA_TAB_CELL_ANSWER", "ANSWER_ID", dbContext);
                if (tab.answer.Length > 30) //since the column in database has length of only 30
                {
                    Byte[] bytes = Convert.FromBase64String(tab.answer);
                    File.WriteAllBytes($@"{UploadPath}\QAFiles\Upload_{AnsId}.jpg", bytes);
                    tab.answer = "Upload_" + AnsId;
                }
                var InsertQuery = $@"INSERT INTO DIST_QA_TAB_CELL_ANSWER (ANSWER_ID,CELL_ID,ANSWER,ENTITY_CODE,ENTITY_TYPE,SP_CODE,CREATED_DATE,SYNC_ID)
                    VALUES('{AnsId}','{tab.cell_id}','{tab.answer}','{model.entity_code}','{model.entity_type}','{model.sp_code}',{today},'{tab.Sync_Id}')";
                var row = dbContext.ExecuteSqlCommand(InsertQuery);
                result["tabular"].Add(tab.Sync_Id, AnsId.ToString());
                if (row <= 0)
                    throw new Exception("Error Processing Request.");
            }
            return result;
        }

        public Dictionary<string, string> UpdateDealerStock(UpdateEntityRequestModel model, NeoErpCoreEntity dbContext)
        {
            var CheckQuery = $"SELECT * FROM DIST_DEALER_STOCK WHERE COMPANY_CODE='{model.COMPANY_CODE}' AND DEALER_CODE='{model.customer_code}' AND trunc(CREATED_DATE)='{DateTime.Now.ToString("dd-MMM-yyyy")}'";
            var data = dbContext.SqlQuery<object>(CheckQuery).ToList();
            var result = new Dictionary<string, string>();
            if (data.Count <= 0)
            {
                var today = string.IsNullOrWhiteSpace(model.Saved_Date) ? $"TO_DATE('{DateTime.Now.ToString("MM/dd/yyyy HH:mm:ss")}','MM/DD/YYYY HH24:MI:SS')" : $"TO_DATE('{model.Saved_Date}','MM/DD/YYYY HH24:MI:SS')";
                foreach (var stock in model.stock)
                {
                    var InsertQuery = $@"INSERT INTO DIST_DEALER_STOCK(DEALER_CODE,ITEM_CODE,MU_CODE,CURRENT_STOCK,PURCHASE_QTY,SP_CODE,CREATED_DATE,DELETED_FLAG,COMPANY_CODE,BRANCH_CODE,SYNC_ID)
                        VALUES('{model.customer_code}','{stock.item_code}','{stock.mu_code}','{stock.cs}','{stock.p_qty}','{model.sp_code}',{today},'N','{model.COMPANY_CODE}','{model.BRANCH_CODE}','{stock.Sync_Id}')";
                    var row = dbContext.ExecuteSqlCommand(InsertQuery);
                    if (row <= 0)
                        throw new Exception("Error updating the stock");
                    result.Add(stock.Sync_Id, model.customer_code);
                }
            }
            else
                foreach (var stock in model.stock)
                    result.Add(stock.Sync_Id, model.customer_code);
            return result;
        }

        public Dictionary<string, string> UpdateDistributorStock(UpdateEntityRequestModel model, NeoErpCoreEntity dbContext)
        {

            var result = new Dictionary<string, string>();
            var CheckQuery = $"SELECT * FROM DIST_DISTRIBUTOR_STOCK WHERE COMPANY_CODE='{model.COMPANY_CODE}' AND DISTRIBUTOR_CODE='{model.customer_code}' AND trunc(CREATED_DATE)='{DateTime.Now.ToString("dd-MMM-yyyy")}'";
            var data = dbContext.SqlQuery<object>(CheckQuery).ToList();
            if (data.Count <= 0)
            {
                var StockId = dbContext.SqlQuery<int>("SELECT (NVL(MAX(STOCK_ID),0)+1) MAXID FROM DIST_DISTRIBUTOR_STOCK").FirstOrDefault();
                var today = string.IsNullOrWhiteSpace(model.Saved_Date) ? $"TO_DATE('{DateTime.Now.ToString("MM/dd/yyyy HH:mm:ss")}','MM/DD/YYYY HH24:MI:SS')" : $"TO_DATE('{model.Saved_Date}','MM/DD/YYYY HH24:MI:SS')";
                foreach (var stock in model.stock)
                {
                    var InsertQuery = $@"INSERT INTO DIST_DISTRIBUTOR_STOCK(STOCK_ID,DISTRIBUTOR_CODE,ITEM_CODE,MU_CODE,CURRENT_STOCK,PURCHASE_QTY,SP_CODE,CREATED_DATE,DELETED_FLAG,COMPANY_CODE,BRANCH_CODE,SYNC_ID)
                        VALUES('{StockId}','{model.customer_code}','{stock.item_code}','{stock.mu_code}','{stock.cs}','{stock.p_qty}','{model.sp_code}',{today},'N','{model.COMPANY_CODE}','{model.BRANCH_CODE}','{stock.Sync_Id}')";
                    var row = dbContext.ExecuteSqlCommand(InsertQuery);
                    if (row <= 0)
                        throw new Exception("Error updating the stock");
                    result.Add(stock.Sync_Id, model.customer_code);
                }
            }
            else
                foreach (var stock in model.stock)
                    result.Add(stock.Sync_Id, model.customer_code);

            return result;
        }

        public Dictionary<string, string> UpdateResellerStock(UpdateEntityRequestModel model, NeoErpCoreEntity dbContext)
        {
            var CheckQuery = $"SELECT * FROM DIST_RESELLER_STOCK WHERE COMPANY_CODE='{model.COMPANY_CODE}' AND RESELLER_CODE='{model.customer_code}' AND trunc(CREATED_DATE)='{DateTime.Now.ToString("dd-MMM-yyyy")}'";
            var data = dbContext.SqlQuery<object>(CheckQuery).ToList();
            var result = new Dictionary<string, string>();
            if (data.Count <= 0)
            {
                var today = string.IsNullOrWhiteSpace(model.Saved_Date) ? $"TO_DATE('{DateTime.Now.ToString("MM/dd/yyyy HH:mm:ss")}','MM/DD/YYYY HH24:MI:SS')" : $"TO_DATE('{model.Saved_Date}','MM/DD/YYYY HH24:MI:SS')";
                foreach (var stock in model.stock)
                {
                    var InsertQuery = $@"INSERT INTO DIST_RESELLER_STOCK(RESELLER_CODE,ITEM_CODE,MU_CODE,CURRENT_STOCK,PURCHASE_QTY,SP_CODE,CREATED_DATE,DELETED_FLAG,COMPANY_CODE,BRANCH_CODE,SYNC_ID)
                        VALUES('{model.customer_code}','{stock.item_code}','{stock.mu_code}','{stock.cs}','{stock.p_qty}','{model.sp_code}',{today},'N','{model.COMPANY_CODE}','{model.BRANCH_CODE}','{stock.Sync_Id}')";
                    var row = dbContext.ExecuteSqlCommand(InsertQuery);
                    if (row <= 0)
                        throw new Exception("Error updating the stock");

                    result.Add(stock.Sync_Id, stock.item_code);
                }
            }
            else
                foreach (var stock in model.stock)
                    result.Add(stock.Sync_Id, stock.item_code);
            return result;
        }

        public Dictionary<string, string> CreateReseller(CreateResellerModel model, NeoErpCoreEntity dbContext)
        {
            var result = new Dictionary<string, string>();
            //primary contact
            var primary = new ContactModel();
            foreach (var c in model.contact)
                if (c.primary.Equals("Y", StringComparison.OrdinalIgnoreCase))
                    primary = c;
            if (primary != null)
                model.contact.Remove(primary);

            if (string.IsNullOrWhiteSpace(model.address))
                throw new Exception("Address is empty.");
            if (string.IsNullOrWhiteSpace(model.latitude))
                throw new Exception("Latitude is empty.");
            if (string.IsNullOrWhiteSpace(model.longitude))
                throw new Exception("Longitude is empty.");
            if (string.IsNullOrWhiteSpace(model.area_code))
                throw new Exception("Area code not selected.");
            string testQuery = $"SELECT * FROM DIST_RESELLER_MASTER WHERE RESELLER_NAME = '{model.reseller_name.Replace("'","''")}' AND PAN_NO = '{model.pan}' AND COMPANY_CODE='{model.COMPANY_CODE}'";
            var testObj = dbContext.SqlQuery<object>(testQuery).ToList();
            if (testObj.Count > 0)
                //throw new Exception("Reseller with the provided name and PAN no. already exists.");
                throw new Exception("EXISTS");
            //Generate reseller code
            string RCodeQuery = $"SELECT 'R-{model.user_id.Trim()}-'||TO_CHAR(SYSDATE,'YYMMDD-HH24MMSS') FROM DUAL";
            string ResellerCode = dbContext.SqlQuery<string>(RCodeQuery).FirstOrDefault();

            //insert reseller
            string ResellerInsert = $@"INSERT INTO DIST_RESELLER_MASTER
                        (RESELLER_CODE,RESELLER_NAME,REG_OFFICE_ADDRESS,EMAIL,PAN_NO,LATITUDE,LONGITUDE,WHOLESELLER,AREA_CODE,CONTACT_SUFFIX,CONTACT_NAME,CONTACT_NO,OUTLET_TYPE_ID,OUTLET_SUBTYPE_ID,GROUPID,CREATED_BY,CREATED_BY_NAME,CREATED_DATE,COMPANY_CODE,BRANCH_CODE,RESELLER_CONTACT,SYNC_ID,SOURCE,ACTIVE,TEMP_ROUTE_CODE) VALUES 
                        ('{ResellerCode}','{model.reseller_name.Replace("'", "''")}','{model.address.Replace("'", "''")}','{model.email}','{model.pan}','{model.latitude}','{model.longitude}','{model.wholeseller}','{model.area_code}','{primary.contact_suffix}','{primary.name.Replace("'", "''")}',
                        '{primary.number}','{model.type_id}','{model.subtype_id}','{model.Group_id}','{model.user_id}',(SELECT FULL_NAME FROM DIST_LOGIN_USER WHERE USERID = '{model.user_id}'),TO_DATE('{model.Saved_Date}','MM/dd/yyyy hh24:mi:ss'),'{model.COMPANY_CODE}','{model.BRANCH_CODE}','{model.Reseller_contact}','{model.Sync_Id}','MOB','N','{model.ROUTE_CODE}')";
            var row = dbContext.ExecuteSqlCommand(ResellerInsert);
            result.Add(model.Sync_Id, ResellerCode);

            //insert contact details
            foreach (var con in model.contact)
            {
                string ContactQuery = $@"INSERT INTO DIST_RESELLER_DETAIL(RESELLER_CODE,COMPANY_CODE,CONTACT_SUFFIX,CONTACT_NAME,CONTACT_NO,DESIGNATION,CREATED_BY,CREATED_DATE,SYNC_ID) VALUES
                            ('{ResellerCode}','{model.COMPANY_CODE}','{con.contact_suffix}','{con.name.Replace("'", "''")}','{con.number}','{con.designation.Replace("'", "''")}','{model.user_id}',TO_DATE('{model.Saved_Date}','MM/dd/yyyy hh24:mi:ss'),'{con.Sync_Id}')";
                row = dbContext.ExecuteSqlCommand(ContactQuery);
                result.Add(con.Sync_Id, ResellerCode);
            }
            List<string> dist = new List<string>();
            List<string> Who = new List<string>();
            if (!string.IsNullOrWhiteSpace(model.distributor_code))
                dist = model.distributor_code.Replace(" ", string.Empty).Split(',').ToList();
            if (!string.IsNullOrWhiteSpace(model.wholeseller_code))
                Who = model.wholeseller_code.Replace(" ", string.Empty).Split(',').ToList();

            foreach (var distributor in dist)
            {
                string disInsertQuery = $@"INSERT INTO DIST_RESELLER_ENTITY (RESELLER_CODE,ENTITY_CODE,ENTITY_TYPE,CREATED_BY,CREATED_DATE,DELETED_FLAG,COMPANY_CODE,BRANCH_CODE)
                                                VALUES('{ResellerCode}','{distributor}','D','{model.user_id}',TO_DATE('{model.Saved_Date}','MM/dd/yyyy hh24:mi:ss'),'N','{model.COMPANY_CODE}','{model.BRANCH_CODE}')";
                row = dbContext.ExecuteSqlCommand(disInsertQuery);
            }
            foreach (var wholeseller in Who)
            {
                string whoInsertQuery = $@"INSERT INTO DIST_RESELLER_ENTITY (RESELLER_CODE,ENTITY_CODE,ENTITY_TYPE,CREATED_BY,CREATED_DATE,DELETED_FLAG,COMPANY_CODE,BRANCH_CODE)
                                                VALUES('{ResellerCode}','{wholeseller}','W','{model.user_id}',TO_DATE('{model.Saved_Date}','MM/dd/yyyy hh24:mi:ss'),'N','{model.COMPANY_CODE}','{model.BRANCH_CODE}')";
                row = dbContext.ExecuteSqlCommand(whoInsertQuery);
            }

            return result;
        }
        /*sashi*/
        public Dictionary<string, string> CreateDistributor(CreateDistributorModel model, NeoErpCoreEntity dbContext)
        {
            var result = new Dictionary<string, string>();
            //primary contact
            var primary = new ContactModel();
            foreach (var c in model.contact)
                if (c.primary.Equals("Y", StringComparison.OrdinalIgnoreCase))
                    primary = c;
            if (primary != null)
                model.contact.Remove(primary);

            if (string.IsNullOrWhiteSpace(model.address))
                throw new Exception("Address is empty.");
            if (string.IsNullOrWhiteSpace(model.latitude))
                throw new Exception("Latitude is empty.");
            if (string.IsNullOrWhiteSpace(model.longitude))
                throw new Exception("Longitude is empty.");
            if (string.IsNullOrWhiteSpace(model.area_code))
                throw new Exception("Area code not selected.");



            string testQuery = $"SELECT * FROM sa_customer_setup WHERE CUSTOMER_EDESC = '{model.distributor_name}' AND PAN_NO = '{model.pan}' AND COMPANY_CODE='{model.COMPANY_CODE}'";
            var testObj = dbContext.SqlQuery<object>(testQuery).ToList();
            if (testObj.Count > 0)
                throw new Exception("Distributor with the provided name and PAN no. already exists.");
            //Generate distributor code
            //sashi
            var newmaxitemcode = string.Empty;
            var newmaxitemcodequery = $@"SELECT MAX(TO_NUMBER(CUSTOMER_CODE))+1 as MASTER_CUSTOMER_CODE FROM SA_CUSTOMER_SETUP";
            newmaxitemcode = dbContext.SqlQuery<int>(newmaxitemcodequery).FirstOrDefault().ToString();

            using (var transaction = _objectEntity.Database.BeginTransaction())
            {
                try
                {

                    if (newmaxitemcodequery != null)
                    {

                        string CustomerQuery = $@"INSERT INTO sa_customer_setup(CUSTOMER_CODE,CUSTOMER_EDESC,CUSTOMER_NDESC,REGD_OFFICE_EADDRESS,REGD_OFFICE_NADDRESS,TEL_MOBILE_NO1,TEL_MOBILE_NO2,FAX_NO,EMAIL,PARTY_TYPE_CODE,CUSTOMER_FLAG,LINK_SUB_CODE,CREDIT_RATE,CREDIT_LIMIT
                                            ,CUSHION_PERCENT,DUE_BILL_COUNT,ACTIVE_FLAG,REMARKS,GROUP_SKU_FLAG,MASTER_CUSTOMER_CODE,PRE_CUSTOMER_CODE,DISCOUNT_FLAT_RATE,EXCLUSIVE_FLAG,DISCOUNT_DAYS,DISCOUNT_PERCENT,COMPANY_CODE,CREATED_BY
                                            ,CREATED_DATE,DELETED_FLAG,OPENING_DATE,MATURITY_DATE,CUSTOMER_GROUP_ID,COUNTRY_CODE,ZONE_CODE,DISTRICT_CODE,CITY_CODE,DEALING_PERSON,EXCISE_NO,TIN,EXPORT_FLAG,GST_NO,IEC_NO,FSSAI_NO,AD_CODE) VALUES
                                            ('{newmaxitemcode}','{model.distributor_name}','{model.distributor_name}','{model.address.Replace("'", "''")}','{model.address.Replace("'", "''")}','{primary.contact_suffix}','{primary.contact_suffix}','{null}','{model.email}','{null}','D','{'C' + newmaxitemcode}','{null}','{null}'
                                            ,'{null}','{null}','Y','{null}','G','01.02','01','{null}','{null}','{null}','{null}','01','ADMIN'
                                            ,TO_DATE('{model.Saved_Date}','MM/dd/yyyy hh24:mi:ss'),'N',TO_DATE('{model.Saved_Date}','MM/dd/yyyy hh24:mi:ss')
                                            ,TO_DATE('{model.Saved_Date}','MM/dd/yyyy hh24:mi:ss'),'{null}','{null}','{null}','{null}','{null}','{null}','{null}','{null}','N','{null}','{null}','{null}','{null}')";
                        var row = dbContext.ExecuteSqlCommand(CustomerQuery);


                        string DistributorQuery = $@"INSERT INTO DIST_DISTRIBUTOR_MASTER(DISTRIBUTOR_CODE,LATITUDE,LONGITUDE,CREATED_BY,CREATED_DATE,ACTIVE,COMPANY_CODE,BRANCH_CODE,AREA_CODE,GROUPID,WEIGHT,DELETED_FLAG) VALUES
                            ('{newmaxitemcode}','{model.latitude}','{model.longitude}','{model.user_id}',TO_DATE('{model.Saved_Date}', 'MM/dd/yyyy hh24:mi:ss'),'Y','{model.COMPANY_CODE}','{model.BRANCH_CODE}','{model.area_code}','{model.Group_id}','{0}','{'N'}')";
                        row = dbContext.ExecuteSqlCommand(DistributorQuery);

                        //insert contact details
                        foreach (var con in model.contact)
                        {
                            string ContactQuery = $@"INSERT INTO DIST_DISTRIBUTOR_DETAIL(DISTRIBUTOR_CODE,COMPANY_CODE,CONTACT_SUFFIX,CONTACT_NAME,CONTACT_NO,DESIGNATION,CREATED_BY,CREATED_DATE) VALUES
                            ('{newmaxitemcode}','{model.COMPANY_CODE}','{con.contact_suffix}','{con.name}','{con.number}','{con.designation}','{model.user_id}',TO_DATE(SYSDATE))";
                            row = dbContext.ExecuteSqlCommand(ContactQuery);
                        }

                        //upload files
                        //foreach (string tagName in Files)
                        //{
                        //    HttpPostedFile file = Files[tagName];
                        //    string DistributorPath = string.Empty;

                        //    DistributorPath = UploadPath + "\\DistributorImages";

                        //    if (!Directory.Exists(DistributorPath))
                        //        Directory.CreateDirectory(DistributorPath);
                        //    string FileName = string.Format("{0}{1}", newmaxitemcode, Path.GetExtension(file.FileName));
                        //    string filePath = Path.Combine(DistributorPath, FileName);
                        //    int count = 1;
                        //    while (File.Exists(filePath))
                        //    {
                        //        FileName = string.Format("{0}_{1}{2}", newmaxitemcode, count++, Path.GetExtension(file.FileName));
                        //        filePath = Path.Combine(DistributorPath, FileName);
                        //    }
                        //    string mediaType;
                        //    if (tagName.IndexOf("store", StringComparison.OrdinalIgnoreCase) >= 0)
                        //    {
                        //        mediaType = "STORE";
                        //        file.SaveAs(filePath);
                        //    }
                        //    else if (tagName.IndexOf("pcontact", StringComparison.OrdinalIgnoreCase) >= 0)
                        //    {
                        //        mediaType = "PCONTACT";
                        //        file.SaveAs(filePath);
                        //    }
                        //    else
                        //        continue;
                        //    string ImageQuery = $@"INSERT INTO DIST_PHOTO_INFO (FILENAME,DESCRIPTION,ENTITY_TYPE,ENTITY_CODE,MEDIA_TYPE,CREATED_BY,CREATE_DATE,COMPANY_CODE,BRANCH_CODE) VALUES
                        //    ('{FileName}','{descriptions[tagName]}','D','{newmaxitemcode}','{mediaType}','{model.user_id}',SYSDATE,'{model.COMPANY_CODE}','{model.BRANCH_CODE}')";
                        //    row = dbContext.ExecuteSqlCommand(ImageQuery);
                        //}
                        _objectEntity.SaveChanges();
                        transaction.Commit();
                    }
                }
                catch (Exception e)
                {
                    transaction.Rollback();
                }
            }
            //sashi
            var fetchModel = new EntityRequestModel
            {
                entity_code = newmaxitemcode,
                BRANCH_CODE = model.BRANCH_CODE,
                COMPANY_CODE = model.COMPANY_CODE,
                entity_type = "D"
            };
            var entityList = this.FetchEntityById(fetchModel, dbContext);
            //var result = entityList.FirstOrDefault();


            //string CodeQuery = $"SELECT '{model.user_id.Trim()} FROM DUAL";
            //string ResellerCode = dbContext.SqlQuery<string>(CodeQuery).FirstOrDefault();


            //var newmaxitemcode = string.Empty;
            //var newmaxitemcodequery = $@"SELECT MAX(TO_NUMBER(CUSTOMER_CODE))+1 as MASTER_CUSTOMER_CODE FROM SA_CUSTOMER_SETUP";
            //newmaxitemcode = dbContext.SqlQuery<int>(newmaxitemcodequery).FirstOrDefault().ToString();

            //string CustomerQuery = $@"INSERT INTO sa_customer_setup(CUSTOMER_CODE,CUSTOMER_EDESC,CUSTOMER_NDESC,REGD_OFFICE_EADDRESS,REGD_OFFICE_NADDRESS,TEL_MOBILE_NO1,TEL_MOBILE_NO2,FAX_NO,EMAIL,PARTY_TYPE_CODE,CUSTOMER_FLAG,LINK_SUB_CODE,CREDIT_RATE,CREDIT_LIMIT
            //                                ,CUSHION_PERCENT,DUE_BILL_COUNT,ACTIVE_FLAG,REMARKS,GROUP_SKU_FLAG,MASTER_CUSTOMER_CODE,PRE_CUSTOMER_CODE,DISCOUNT_FLAT_RATE,EXCLUSIVE_FLAG,DISCOUNT_DAYS,DISCOUNT_PERCENT,COMPANY_CODE,CREATED_BY
            //                                ,CREATED_DATE,DELETED_FLAG,OPENING_DATE,MATURITY_DATE,CUSTOMER_GROUP_ID,COUNTRY_CODE,ZONE_CODE,DISTRICT_CODE,CITY_CODE,DEALING_PERSON,EXCISE_NO,TIN,EXPORT_FLAG,GST_NO,IEC_NO,FSSAI_NO,AD_CODE) VALUES
            //                                ('{newmaxitemcode}','{model.distributor_name}','{model.distributor_name}','{model.address.Replace("'", "''")}','{model.address.Replace("'", "''")}','{primary.contact_suffix}','{primary.contact_suffix}','{null}','{model.email}','{null}','D','{'C' + newmaxitemcode}','{null}','{null}'
            //                                ,'{null}','{null}','Y','{null}','G','01.02','01','{null}','{null}','{null}','{null}','01','ADMIN'
            //                                ,TO_DATE('{model.Saved_Date}','MM/dd/yyyy hh24:mi:ss'),'N',TO_DATE('{model.Saved_Date}','MM/dd/yyyy hh24:mi:ss')
            //                                ,TO_DATE('{model.Saved_Date}','MM/dd/yyyy hh24:mi:ss'),'{null}','{null}','{null}','{null}','{null}','{null}','{null}','N','{null}','{null}','{null}','{null}')";
            //var row = dbContext.ExecuteSqlCommand(CustomerQuery);
            //result.Add(model.Sync_Id, newmaxitemcode);


            //string DistributorQuery = $@"INSERT INTO DIST_DISTRIBUTOR_MASTER(DISTRIBUTOR_CODE,LATITUDE,LONGITUDE,CREATED_BY,CREATED_DATE,ACTIVE,COMPANY_CODE,BRANCH_CODE,AREA_CODE,GROUPID,WEIGHT,DELETED_FLAG) VALUES
            //                ('{newmaxitemcode}','{model.latitude}','{model.longitude}','{model.user_id}','TO_DATE('{model.Saved_Date}', 'MM/dd/yyyy hh24:mi:ss')','{model.user_id}',TO_DATE('{model.Saved_Date}','MM/dd/yyyy hh24:mi:ss'),'{model.COMPANY_CODE}','{model.BRANCH_CODE}','{model.area_code}','{model.Group_id}','{0}','{'N'}')";
            //row = dbContext.ExecuteSqlCommand(DistributorQuery);
            //result.Add(model.Sync_Id, newmaxitemcode);

            ////insert contact details
            //foreach (var con in model.contact)
            //{
            //    string ContactQuery = $@"INSERT INTO DIST_DISTRIBUTOR_DETAIL(DISTRIBUTOR_CODE,COMPANY_CODE,CONTACT_SUFFIX,CONTACT_NAME,CONTACT_NO,DESIGNATION,CREATED_BY,CREATED_DATE,SYNC_ID) VALUES
            //                ('{newmaxitemcode}','{model.COMPANY_CODE}','{con.contact_suffix}','{con.name.Replace("'", "''")}','{con.number}','{con.designation.Replace("'", "''")}','{model.user_id}',TO_DATE('{model.Saved_Date}','MM/dd/yyyy hh24:mi:ss'),'{con.Sync_Id}')";
            //    row = dbContext.ExecuteSqlCommand(ContactQuery);
            //    result.Add(con.Sync_Id, newmaxitemcode);
            //}







            //            DISTRIBUTOR_CODE,REG_OFFICE_ADDRESS,CONTACT_NO,EMAIL,PAN_NO,VAT_NO,LATITUDE,LONGITUDE,CREATED_BY,LUPDATE_BY,CREATED_DATE,LUPDATE_DATE,ACTIVE,COMPANY_CODE,BRANCH_CODE,AREA_CODE,SYNC_ID,GROUPID,WEIGHT,DELETED_FLAG,DISTRIBUTOR_TYPE_ID,DISTRIBUTOR_SUBTYPE_ID
            //)


            //string customerQuery = $"SELECT * FROM sa_customer_setup WHERE " +
            //    $"SELLER_NAME = '{model.reseller_name.Replace("'", "''")}' AND PAN_NO = '{model.pan}' AND COMPANY_CODE='{model.COMPANY_CODE}'";
            //var testObj = dbContext.SqlQuery<object>(testQuery).ToList();

            //insert customer details
            //foreach (var cus in model.customer)
            //{
            //    string customerQuery = $"SELECT * FROM sa_customer_setup WHERE " +
            //        $"CUSTOMER_EDESC = '{cus.CUSTOMER_NAME.Replace("'", "''")}' AND PAN_NO = '{model.pan}' AND COMPANY_CODE='{model.COMPANY_CODE}'";
            //    var custObj = dbContext.SqlQuery<object>(customerQuery).ToList();

            //    if (custObj.Count < 1)
            //    {
            //        string CustomerQuery = $@"INSERT INTO sa_customer_setup(CUSTOMER_CODE,CUSTOMER_EDESC,CUSTOMER_NDESC,REGD_OFFICE_EADDRESS,REGD_OFFICE_NADDRESS,TEL_MOBILE_NO1,TEL_MOBILE_NO2,FAX_NO,EMAIL,PARTY_TYPE_CODE,CUSTOMER_FLAG,LINK_SUB_CODE,CREDIT_RATE,CREDIT_LIMIT
            //                                ,CUSHION_PERCENT,DUE_BILL_COUNT,ACTIVE_FLAG,REMARKS,GROUP_SKU_FLAG,MASTER_CUSTOMER_CODE,PRE_CUSTOMER_CODE,DISCOUNT_FLAT_RATE,EXCLUSIVE_FLAG,DISCOUNT_DAYS,DISCOUNT_PERCENT,COMPANY_CODE,CREATED_BY
            //                                ,CREATED_DATE,DELETED_FLAG,OPENING_DATE,MATURITY_DATE,CUSTOMER_GROUP_ID,COUNTRY_CODE,ZONE_CODE,DISTRICT_CODE,CITY_CODE,DEALING_PERSON,EXCISE_NO,TIN,EXPORT_FLAG,GST_NO,IEC_NO,FSSAI_NO,AD_CODE) VALUES
            //                                ('{cus.CUSTOMER_CODE}','{cus.CUSTOMER_NAME}','{cus.CUSTOMER_NAME}','{model.address.Replace("'", "''")}','{model.address.Replace("'", "''")}','{primary.contact_suffix}','{primary.contact_suffix}','{null}','{model.email}','{null}','D','{'C' + cus.CUSTOMER_CODE}','{null}','{null}'
            //                                ,'{null}','{null}','Y','{null}','G','01.02','01','{null}','{null}','{null}','{null}','01','ADMIN'
            //                                ,TO_DATE('{model.Saved_Date}','MM/dd/yyyy hh24:mi:ss'),'N',TO_DATE('{model.Saved_Date}','MM/dd/yyyy hh24:mi:ss')
            //                                ,TO_DATE('{model.Saved_Date}','MM/dd/yyyy hh24:mi:ss'),'{null}','{null}','{null}','{null}','{null}','{null}','{null}','N','{null}','{null}','{null}','{null}')";
            //        var row = dbContext.ExecuteSqlCommand(CustomerQuery);
            //        result.Add(model.Sync_Id, cus.CUSTOMER_CODE);
            //    }

            //    string ContactQuery = $@"INSERT INTO DIST_DISTRIBUTOR_MASTER(DISTRIBUTOR_CODE,LATITUDE,LONGITUDE,CREATED_BY,CREATED_DATE,ACTIVE,COMPANY_CODE,BRANCH_CODE,AREA_CODE,GROUPID,WEIGHT,DELETED_FLAG) VALUES
            //                ('{CodeQuery}','{model.latitude}','{model.longitude}','{model.user_id}','TO_DATE('{model.Saved_Date}', 'MM/dd/yyyy hh24:mi:ss')','{model.user_id}',TO_DATE('{model.Saved_Date}','MM/dd/yyyy hh24:mi:ss'),'{model.COMPANY_CODE}','{model.BRANCH_CODE}','{model.area_code}','{model.Group_id}','{0}','{'N'}')";
            //   var row1 = dbContext.ExecuteSqlCommand(ContactQuery);
            //    result.Add(model.Sync_Id, customerQuery);

            //    //string ContactQuery = $@"INSERT INTO DIST_RESELLER_DETAIL(RESELLER_CODE,COMPANY_CODE,CONTACT_SUFFIX,CONTACT_NAME,CONTACT_NO,DESIGNATION,CREATED_BY,CREATED_DATE,SYNC_ID) VALUES
            //    //            ('{ResellerCode}','{model.COMPANY_CODE}','{con.contact_suffix}','{con.name.Replace("'", "''")}','{con.number}','{con.designation.Replace("'", "''")}','{model.user_id}',TO_DATE('{model.Saved_Date}','MM/dd/yyyy hh24:mi:ss'),'{con.Sync_Id}')";
            //    //row = dbContext.ExecuteSqlCommand(ContactQuery);
            //    //result.Add(con.Sync_Id, ResellerCode
            //}


            //string testQuery = $"SELECT * FROM DIST_DISTRIBUTOR_MASTER WHERE RE" +
            //    $"SELLER_NAME = '{model.reseller_name.Replace("'", "''")}' AND PAN_NO = '{model.pan}' AND COMPANY_CODE='{model.COMPANY_CODE}'";
            //var testObj = dbContext.SqlQuery<object>(testQuery).ToList();
            //if (testObj.Count > 0)
            //    //throw new Exception("Reseller with the provided name and PAN no. already exists.");
            //    throw new Exception("EXISTS");
            ////Generate reseller code
            //string RCodeQuery = $"SELECT 'R-{model.user_id.Trim()}-'||TO_CHAR(SYSDATE,'YYMMDD-HH24MMSS') FROM DUAL";
            //string ResellerCode = dbContext.SqlQuery<string>(RCodeQuery).FirstOrDefault();

            ////insert reseller
            //string ResellerInsert = $@"INSERT INTO DIST_RESELLER_MASTER
            //            (RESELLER_CODE,RESELLER_NAME,REG_OFFICE_ADDRESS,EMAIL,PAN_NO,LATITUDE,LONGITUDE,WHOLESELLER,AREA_CODE,CONTACT_SUFFIX,CONTACT_NAME,CONTACT_NO,OUTLET_TYPE_ID,OUTLET_SUBTYPE_ID,GROUPID,CREATED_BY,CREATED_BY_NAME,CREATED_DATE,COMPANY_CODE,BRANCH_CODE,RESELLER_CONTACT,SYNC_ID,SOURCE,ACTIVE,TEMP_ROUTE_CODE) VALUES 
            //            ('{ResellerCode}','{model.reseller_name.Replace("'", "''")}','{model.address.Replace("'", "''")}','{model.email}','{model.pan}','{model.latitude}','{model.longitude}','{model.wholeseller}','{model.area_code}','{primary.contact_suffix}','{primary.name.Replace("'", "''")}',
            //            '{primary.number}','{model.type_id}','{model.subtype_id}','{model.Group_id}','{model.user_id}',(SELECT FULL_NAME FROM DIST_LOGIN_USER WHERE USERID = '{model.user_id}'),TO_DATE('{model.Saved_Date}','MM/dd/yyyy hh24:mi:ss'),'{model.COMPANY_CODE}','{model.BRANCH_CODE}','{model.Reseller_contact}','{model.Sync_Id}','MOB','N','{model.ROUTE_CODE}')";
            //var row = dbContext.ExecuteSqlCommand(ResellerInsert);
            //result.Add(model.Sync_Id, ResellerCode);

            ////insert contact details
            //foreach (var con in model.contact)
            //{
            //    string ContactQuery = $@"INSERT INTO DIST_RESELLER_DETAIL(RESELLER_CODE,COMPANY_CODE,CONTACT_SUFFIX,CONTACT_NAME,CONTACT_NO,DESIGNATION,CREATED_BY,CREATED_DATE,SYNC_ID) VALUES
            //                ('{ResellerCode}','{model.COMPANY_CODE}','{con.contact_suffix}','{con.name.Replace("'", "''")}','{con.number}','{con.designation.Replace("'", "''")}','{model.user_id}',TO_DATE('{model.Saved_Date}','MM/dd/yyyy hh24:mi:ss'),'{con.Sync_Id}')";
            //    row = dbContext.ExecuteSqlCommand(ContactQuery);
            //    result.Add(con.Sync_Id, ResellerCode);
            //}
            //List<string> dist = new List<string>();
            //List<string> Who = new List<string>();
            //if (!string.IsNullOrWhiteSpace(model.distributor_code))
            //    dist = model.distributor_code.Replace(" ", string.Empty).Split(',').ToList();
            //if (!string.IsNullOrWhiteSpace(model.wholeseller_code))
            //    Who = model.wholeseller_code.Replace(" ", string.Empty).Split(',').ToList();

            //foreach (var distributor in dist)
            //{
            //    string disInsertQuery = $@"INSERT INTO DIST_RESELLER_ENTITY (RESELLER_CODE,ENTITY_CODE,ENTITY_TYPE,CREATED_BY,CREATED_DATE,DELETED_FLAG,COMPANY_CODE,BRANCH_CODE)
            //                                    VALUES('{ResellerCode}','{distributor}','D','{model.user_id}',TO_DATE('{model.Saved_Date}','MM/dd/yyyy hh24:mi:ss'),'N','{model.COMPANY_CODE}','{model.BRANCH_CODE}')";
            //    row = dbContext.ExecuteSqlCommand(disInsertQuery);
            //}
            //foreach (var wholeseller in Who)
            //{
            //    string whoInsertQuery = $@"INSERT INTO DIST_RESELLER_ENTITY (RESELLER_CODE,ENTITY_CODE,ENTITY_TYPE,CREATED_BY,CREATED_DATE,DELETED_FLAG,COMPANY_CODE,BRANCH_CODE)
            //                                    VALUES('{ResellerCode}','{wholeseller}','W','{model.user_id}',TO_DATE('{model.Saved_Date}','MM/dd/yyyy hh24:mi:ss'),'N','{model.COMPANY_CODE}','{model.BRANCH_CODE}')";
            //    row = dbContext.ExecuteSqlCommand(whoInsertQuery);
            //}

            return result;
        }
        /*sashi*/
        public Dictionary<string, string> UploadEntityMedia(List<EntityRequestModelOffline> list, HttpFileCollection files, NeoErpCoreEntity dbContext)
        {
            var result = new Dictionary<string, string>();
            var syncedIds = dbContext.SqlQuery<string>($"SELECT SYNC_ID FROM DIST_VISIT_IMAGE WHERE SYNC_ID IN ('{string.Join("','", list.Select(x => x.Sync_Id))}')").ToList();
            if (syncedIds.Count > 0)
            {
                var Removed = list.RemoveAll(x => syncedIds.Contains(x.Sync_Id));
            }
            foreach (var model in list)
            {
                //var imageName = dbContext.SqlQuery<string>($"SELECT IMAGE_NAME FROM DIST_VISIT_IMAGE WHERE SYNC_ID='{model.Sync_Id}'").ToList();
                //if (imageName.Count > 0)
                //    continue;
                if (model.entity_type.Equals("P", StringComparison.OrdinalIgnoreCase) || model.entity_type.Equals("DEALER", StringComparison.OrdinalIgnoreCase))
                    model.entity_type = "P";
                else if (model.entity_type.Equals("D", StringComparison.OrdinalIgnoreCase) || model.entity_type.Equals("DISTRIBUTOR", StringComparison.OrdinalIgnoreCase))
                    model.entity_type = "D";
                else if (model.entity_type.Equals("R", StringComparison.OrdinalIgnoreCase) || model.entity_type.Equals("RESELLER", StringComparison.OrdinalIgnoreCase))
                    model.entity_type = "R";
                else if (model.entity_type.Equals("B", StringComparison.OrdinalIgnoreCase) || model.entity_type.Equals("BRANDING", StringComparison.OrdinalIgnoreCase))
                    model.entity_type = "B";
                else
                    throw new Exception("Invalid customer type");
                int row = 0;


                HttpPostedFile file = files[$"userfile[{model.Index}]"];
                var ImageId = this.GetMaxId("DIST_VISIT_IMAGE", "IMAGE_CODE", dbContext);
                var folderpath = UploadPath + "\\EntityImages";
                if (!Directory.Exists(folderpath))
                    Directory.CreateDirectory(folderpath);
                string FileName = string.Format("{0}{1}{2}", "EntityImage", ImageId, Path.GetExtension(file.FileName));
                string filePath = Path.Combine(folderpath, FileName);
                int count = 1;
                while (File.Exists(filePath))
                {
                    FileName = string.Format("{0}{1}_{2}{3}", "EntityImage", ImageId, count++, Path.GetExtension(file.FileName));
                    filePath = Path.Combine(folderpath, FileName);
                }

                file.SaveAs(filePath);
                var InsertQuery = $@"INSERT INTO DIST_VISIT_IMAGE (IMAGE_CODE,IMAGE_NAME,IMAGE_TITLE,IMAGE_DESC,SP_CODE,ENTITY_CODE,TYPE,UPLOAD_DATE,LONGITUDE,LATITUDE,CATEGORYID,COMPANY_CODE,BRANCH_CODE,SYNC_ID)
                                    VALUES ({ImageId}, '{FileName}', '{DBNull.Value}', '{model.Description.Replace("'", "''")}', '{model.ACC_CODE}', '{model.entity_code}', '{model.entity_type}',TO_DATE('{model.Saved_Date}','MM/dd/yyyy  HH24:MI:SS'),'{model.longitude}', '{model.latitude}','{model.Categoryid}','{model.COMPANY_CODE}', '{model.BRANCH_CODE}','{model.Sync_Id}')";
                row += dbContext.ExecuteSqlCommand(InsertQuery);

                result.Add(model.Sync_Id, ImageId.ToString());
            }
            return result;
        }

        public Dictionary<string, string> UploadResellerEntityMedia(List<EntityRequestModelOffline> list, HttpFileCollection files, NeoErpCoreEntity dbContext)
        {
            var result = new Dictionary<string, string>();
            var query = $"SELECT SYNC_ID FROM DIST_PHOTO_INFO WHERE SYNC_ID IN ('{string.Join("','", list.Select(x => x.Sync_Id))}')";
            var syncedIds = dbContext.SqlQuery<string>(query).ToList();
            if (syncedIds.Count > 0)
            {
                var Removed = list.RemoveAll(x => syncedIds.Contains(x.Sync_Id));
            }
            foreach (var model in list)
            {
                var synidChecked = result.FirstOrDefault(x => x.Key == model.Sync_Id);

                // var EntityCodes = model.entity_code.Split(',').ToList();
                // var MediaType = model.Media_Type.Split(',').ToList();
                //foreach (var entity in EntityCodes)
                // {
                var entitycode = string.Empty;
                foreach (var key in files.AllKeys)
                {
                    var keyvalue = key;
                    HttpPostedFile file = files[key];
                    var fileContant = keyvalue.Split('_').ToList();
                    var mediaType = fileContant[0];
                    var entityCode = fileContant[1];
                    var SynId = fileContant[2];
                    entitycode = entityCode;
                    var synid = list.ToList().Where(x => x.entity_code == entityCode && x.Media_Type.ToLower() == mediaType.ToLower()).Select(x => x.Sync_Id);
                    if (entityCode.ToUpper().Contains("Previously Synced".ToUpper()))
                        continue;

                    //string UserFolderpath = string.Empty;
                    string ResellerPath = string.Empty;


                    ResellerPath = UploadPath + "\\ResellerImages";
                    //ResellerPath = UploadPath + "\\" + model.user_id + "\\" + entityCode+"\\"+ mediaType;


                    //if (!Directory.Exists(UserFolderpath))
                    //    Directory.CreateDirectory(UserFolderpath);
                    if (!Directory.Exists(ResellerPath))
                        Directory.CreateDirectory(ResellerPath);
                    string FileName = string.Format("{0}{1}", entityCode, Path.GetExtension(file.FileName));
                    string filePath = Path.Combine(ResellerPath, FileName);
                    int count = 1;
                    while (File.Exists(filePath))
                    {
                        FileName = string.Format("{0}_{1}{2}", entityCode, count++, Path.GetExtension(file.FileName));
                        filePath = Path.Combine(ResellerPath, FileName);
                    }

                    if (mediaType.ToUpper().Contains("STORE"))
                    {
                        mediaType = "STORE";
                        file.SaveAs(filePath);
                    }
                    else if (mediaType.Contains("PCONTACT"))
                    {
                        mediaType = "PCONTACT";
                        file.SaveAs(filePath);
                    }
                    else
                        continue;
                    string ImageQuery = $@"INSERT INTO DIST_PHOTO_INFO (FILENAME,DESCRIPTION,ENTITY_TYPE,ENTITY_CODE,MEDIA_TYPE,CREATED_BY,CREATE_DATE,COMPANY_CODE,BRANCH_CODE,SYNC_ID) VALUES
                            ('{FileName}','{model.Description.Replace("'", "''")}','R','{entityCode}','{mediaType}','{model.user_id}',TO_DATE('{model.Saved_Date}','MM/dd/yyyy HH24:MI:SS'),'{model.COMPANY_CODE}','{model.BRANCH_CODE}','{model.Sync_Id}')";
                    var row = dbContext.ExecuteSqlCommand(ImageQuery);
                    result.Add(SynId, mediaType);
                }

            }
            return result;
        }

        public Dictionary<string,string> SaveScheme(SchemeModel model, NeoErpCoreEntity dbContext)
        {
            var result = new Dictionary<string, string>();
            try
            {
                var SchemeId = dbContext.SqlQuery<int>("SELECT NVL(MAX(SCHEME_CODE),0)+1 S_ID FROM BRD_SCHEME").FirstOrDefault();
                string InsertQuery = $@"INSERT INTO BRD_SCHEME (SCHEME_CODE, CONTRACT_CODE, RESELLER_CODE, EMPLOYEE_CODE, ITEM_CODE, QUANTITY, MU_CODE, END_USER,
                        DIVISION_CODE, BRAND_CODE, HANDOVER_DATE, CREATED_BY, CREATED_DATE, DELETED_FLAG,SYNC_ID,COMPANY_CODE,BRANCH_CODE,GIFT_ITEM_CODE,GIFT_ITEM_QUANTITY) VALUES(
                        '{SchemeId}','{model.CONTRACT_CODE}','{model.RESELLER_CODE}','{model.user_id}','{model.ITEM_CODE}','{model.QUANTITY}','{model.MU_CODE}','{model.END_USER}',
                        '{model.DIVISION_CODE}','{model.BRAND_CODE}',TO_DATE('{model.HANDOVER_DATE.ToString("dd-MMM-yyyy")}', 'DD-MON-RRRR'),'{model.user_id}',SYSDATE,'N','{model.Sync_Id}',
                        '{model.COMPANY_CODE}','{model.BRANCH_CODE}','{model.GIFT_ITEM_CODE}','{model.GIFT_ITEM_QUANTITY}')";
                dbContext.ExecuteSqlCommand(InsertQuery);

                foreach (var general in model.Answers)
                {
                    var AnsInsertQuery = $@"INSERT INTO BRD_SCHEME_ANSWERS (SCHEME_ID,RESELLER_CODE,COLUMN_NAME,ANSWER,QA_CODE,DELETED_FLAG,CREATED_DATE,CREATED_BY,COMPANY_CODE,BRANCH_CODE)
                    VALUES('{SchemeId}','{model.RESELLER_CODE}',(SELECT QUESTION FROM DIST_QA_MASTER WHERE QA_CODE='{general.qa_code}'),'{general.answer}','{general.qa_code}','N',SYSDATE,'{model.user_id}','{model.COMPANY_CODE}','{model.BRANCH_CODE}')";
                    var row = dbContext.ExecuteSqlCommand(AnsInsertQuery);
                    if (row <= 0)
                        throw new Exception("Error Processing Request.");
                }
                result.Add(model.Sync_Id, SchemeId.ToString());
            }
            catch
            {
                throw new Exception("Something went Wrong");
            }
            return result;
        }

        public Dictionary<string,string> SaveCompAns(CompAnsModel model, NeoErpCoreEntity dbContext)
        {
            var result = new Dictionary<string, string>();
            foreach(var answer in model.ANSWERS)
            {
                var query = $@"INSERT INTO DIST_COMP_QA (ENTITY_CODE,ENTITY_TYPE,ITEM_CODE,COMP_ITEM_CODE,QUESTION_ID,ANSWER,LATITUDE,LONGITUDE,COMPANY_CODE,CREATED_BY,CREATED_DATE,SYNC_ID)
                            VALUES('{model.ENTITY_CODE}','{model.ENTITY_TYPE}','{model.ITEM_CODE}','{answer.COMP_ITEM_CODE}','{answer.QUESTION_ID}','{answer.ANSWER}','{model.latitude}','{model.longitude}','{model.COMPANY_CODE}','{model.user_id}','{model.Saved_Date}','{answer.Sync_Id}')";
                var rows = dbContext.ExecuteSqlCommand(query);
                result.Add(answer.Sync_Id, answer.QUESTION_ID);
            }
            return result;
        }

        public Dictionary<string, string> SaveDeviceLog(MobileLogModel model, NeoErpCoreEntity dbContext)
        {
            var result = new Dictionary<string, string>();
            var query = $@"INSERT INTO DIST_USER_DEVICE_LOG (SP_CODE,SWITCH_STATUS,BATTERY_PERCENT,CREATED_DATE,COMPANY_CODE,SYNC_ID)
                            VALUES('{model.SP_CODE}','{model.SWITCH_STATUS}','{model.BATTERY_PERCENT}',TO_DATE('{model.Saved_Date}','MM/DD/YYYY HH24:MI:SS'),'{model.COMPANY_CODE}','{model.Sync_Id}')";
            var rows = dbContext.ExecuteSqlCommand(query);
            result.Add(model.Sync_Id, model.SP_CODE);
            return result;
        }

        public Dictionary<string, string> SaveCrmTask(CrmModel model, NeoErpCoreEntity dbContext)
        {
            var result = new Dictionary<string, string>();
            var query = $@"UPDATE CRM_PROCESS_FOLLOWUP SET DESCRIPTION = '{model.REMARKS}', MODIFY_DATE = SYSDATE
                WHERE PROCESS_NO = '{model.PROCESS_NO}' AND LEAD_NO = '{model.LEAD_NO}' AND PROCESS_CODE = '{model.PROCESS_CODE}' AND AGENT_CODE = '{model.AGENT_CODE}'";
            var rows = dbContext.ExecuteSqlCommand(query);
            result.Add(model.Sync_Id, model.AGENT_CODE.ToString());
            return result;
        }


        public Dictionary<string, string> SaveDistSalesReturn(DistributionSalesReturnModel returnModel, NeoErpCoreEntity dbContext)
        {
            try
            {
                var saveResult = new Dictionary<string, string>();
                if (returnModel.locationinfo != null)
                {
                    returnModel.locationinfo.remarks = "Sales Return Begin(auto)";
                    var locationRes = this.UpdateMyLocation(returnModel.locationinfo, dbContext);
                }

                int id = 0;
                long idL = 1L;
                if (returnModel.ENTITY_TYPE.Equals("P", StringComparison.OrdinalIgnoreCase) || returnModel.ENTITY_TYPE.Equals("DEALER", StringComparison.OrdinalIgnoreCase)
                    || returnModel.ENTITY_TYPE.Equals("D", StringComparison.OrdinalIgnoreCase) || returnModel.ENTITY_TYPE.Equals("DISTRIBUTOR", StringComparison.OrdinalIgnoreCase))
                    //idL =this.GetMaxIdSalesReturn("DIST_SALES_RETURN", "RETURN_NO", dbContext);
                    idL = 2;
                else if (returnModel.ENTITY_TYPE.Equals("R", StringComparison.OrdinalIgnoreCase) || returnModel.ENTITY_TYPE.Equals("RESELLER", StringComparison.OrdinalIgnoreCase))
                {
                    if (string.IsNullOrWhiteSpace(returnModel.RESELLER_CODE))
                        throw new Exception("Reseller code is empty");
                    //idL=this.GetMaxIdSalesReturn("DIST_SALES_RETURN", "RETURN_NO", dbContext);
                }
                else
                    throw new Exception("Invalid customer type");
                if (idL <= 0)
                    throw new Exception("Unable to get next ID for the sales return.");

                var today = $"TO_DATE('{DateTime.Now.ToString("MM/dd/yyyy HH:mm:ss")}','MM/dd/yyyy hh24:mi:ss')";
                returnModel.Saved_Date = string.IsNullOrWhiteSpace(returnModel.Saved_Date) ? today : $"TO_DATE('{returnModel.Saved_Date}','MM/dd/yyyy hh24:mi:ss')";
                //var OrderDate = string.IsNullOrWhiteSpace(returnModel.ORDER_DATE.ToString()) ? today : $"TO_DATE('{returnModel.ORDER_DATE}','MM/dd/yyyy hh24:mi:ss')";

                foreach (var item in returnModel.products)
                {
                    item.PARTY_TYPE_CODE = item.PARTY_TYPE_CODE ?? "";
                    string InsertQuery = string.Empty;
                    string priceQuery = $"SELECT NVL(SALES_PRICE,0) SALES_PRICE FROM IP_ITEM_MASTER_SETUP WHERE ITEM_CODE = '{item.ITEM_CODE}' AND COMPANY_CODE='{returnModel.COMPANY_CODE}'";
                    decimal SP = dbContext.SqlQuery<decimal>(priceQuery).FirstOrDefault();

                    var saveQuery = $@"INSERT INTO DIST_SALES_RETURN(RETURN_NO,RETURN_DATE,CUSTOMER_CODE,SERIAL_NO,ITEM_CODE,MU_CODE,QUANTITY,FORM_CODE,COMPANY_CODE,BRANCH_CODE,CREATED_BY,CREATED_DATE,
                                                    MFD_DATE,EXPIRY_DATE,RETRUN_CONDITIONS,COMPLAIN_TYPE,COMPLAIN_SERIOUSNESS,DISTRIBUTOR_REMARKS,ASM_REMARKS,BATCH_NO,CUSTOMER_TYPE,DELETED_FLAG) 
                                       VALUES('{returnModel.ORDER_NO}',TRUNC(TO_DATE('{returnModel.ORDER_DATE.ToShortDateString()}','MM/DD/YYYY')),'{returnModel.CUSTOMER_CODE}',
                                                   '{item.BATCH_NO}','{item.ITEM_CODE}','{item.MU_CODE}','{item.QUANTITY}','0','{returnModel.COMPANY_CODE}','{returnModel.BRANCH_CODE}',
                                                   '{returnModel.user_id}',SYSDATE,'{item.MBF_DATA}','{item.EXP_DATE}',
                                                    '{returnModel.CONDITION}','{returnModel.COMPLAIN_TYPE}','{returnModel.SERIOUSNESS}','{returnModel.REMARKS_DIST}','{returnModel.REMARKS_ASM}','{item.BATCH_NO}','{returnModel.ENTITY_TYPE}','N')";
                    var rowAffacted = dbContext.ExecuteSqlCommand(saveQuery);
                    saveResult.Add(item.SYNC_ID, returnModel.ORDER_NO);

                   

                }
                return saveResult;
            }
            catch (Exception ex)
            {
                throw ex;
            }

        }
        #endregion Inserting Data
    }
}


//                        Buddha
//
//                       _oo0oo_
//                      o8888888o
//                      88" . "88
//                      (| -_- |)
//                      0\  =  /0
//                    ___/`---'\___
//                  .' \\|     |// '.
//                 / \\|||  :  |||// \
//                / _||||| -:- |||||- \
//               |   | \\\  -  /// |   |
//               | \_|  ''\---/''  |_/ |
//               \  .-\__  '-'  ___/-. /
//              ___'. .'  /--.--\  `. .'___
//          ."" '<  `.___\_<|>_/___.' >' "".
//         | | :  `- \`.;`\ _ /`;.`/ - ` : | |
//         \  \ `_.   \_ __\ /__ _/   .-` /  /
//     =====`-.____`.___ \_____/___.-`___.-'=====
//                       `=---='