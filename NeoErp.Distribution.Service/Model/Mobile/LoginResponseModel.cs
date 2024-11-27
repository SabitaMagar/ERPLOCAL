using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NeoErp.Distribution.Service.Model.Mobile
{
    public class LoginResponseModel// : CommonModel<LoginResponseModel>
    {
        public LoginResponseModel()
        {
            COMPANY = new Dictionary<string, CompanyModel>();
        }
        public string USER_ID { get; set; }
        public string IS_MOBILE { get; set; }
        public string ATTENDANCE { get; set; }
        public string USER_NAME { get; set; }
        public string FULL_NAME { get; set; }
        public string PASS_WORD { get; set; }
        public string CONTACT_NO { get; set; }
        public string SP_CODE { get; set; }
        public string GROUP_ID { get; set; }
        public string USER_TYPE { get; set; }
        public string SUPER_USER { get; set; }
        public string EXPIRY_DATE { get; set; }
        public string LINK_SYN_USER_NO { get; set; }
        public string ROLE_CODE { get; set; }
        public string COMPANY_CODE { get; set; }
        public string BRANCH_CODE { get; set; }
        public string AREA_CODE { get; set; }
        public string BRANDING { get; set; }
        public Dictionary<string,CompanyModel> COMPANY { get; set; }
    }
    public class CompanyModel
    {
        public CompanyModel()
        {
            BRANCH = new Dictionary<string, BranchModel>();
            PREFERENCE = new PreferenceModel();
        }
        public string COMPANY_CODE { get; set; }
        public string COMPANY_EDESC { get; set; }
        public string FISCAL_START { get; set; }
        public string FISCAL_END { get; set; }
        public Dictionary<string, BranchModel> BRANCH { get; set; }
        public PreferenceModel PREFERENCE { get; set; }
    }
    public class BranchModel
    {
        public string BRANCH_CODE { get; set; }
        public string BRANCH_EDESC { get; set; }
    }
    public class PreferenceModel
    {
        public string MO_GPS { get; set; }
        public string MO_AGPS { get; set; }
        public string PO_PARTY_TYPE { get; set; }
        public string PO_COMPANY_LIST { get; set; }
        public string PO_BILLING_NAME { get; set; }
        public string PO_SYN_RATE { get; set; }
        public string PO_CUSTOM_RATE { get; set; }
        public string PO_REMARKS { get; set; }
        public string PO_CONVERSION_UNIT { get; set; }
        public string PO_CONVERSION_FACTOR { get; set; }
        public string PO_SALES_TYPE { get; set; }
        public string PO_SHIPPING_ADDRESS { get; set; }
        public string PO_SHIPPING_CONTACT { get; set; }
        public string PO_DISPLAY_DIST_ITEM { get; set; }
        public string SO_CREDIT_LIMIT_CHK { get; set; }
        public string SO_CREDIT_DAYS_CHK { get; set; }
        public string SO_CONSOLIDATE_DEFAULT { get; set; }
        public string CS_CONVERSION_UNIT { get; set; }
        public string SQL_NN_CONVERSION_UNIT_FACTOR { get; set; }
        public string QA_MKT_INFO { get; set; }
        public string QA_COMPT_INFO { get; set; }
        public string COMPANY_CODE { get; set; }
        public string BRANCH_CODE { get; set; }
        public string ATN_DEFAULT { get; set; }
        public string ATN_IMAGE { get; set; }
        public string SET_RES_MAP_WHOLESALER { get; set; }
        public string SQL_MULTIPLE_COMPANY { get; set; }
        public string SQL_COMPANY_ENTITY { get; set; }
        public string SQL_GROUP_MAP { get; set; }
        public string LO_BG_TRACK { get; set; }
        public string PO_RATE_TABLE { get; set; }
        public string PO_RATE_COLUMN { get; set; }
        public string PO_DIST_RATE_COLUMN { get; set; }
        public decimal? LO_BG_TIME { get; set; }
        public decimal? SQL_PEV_DAYS { get; set; }
        public decimal? SQL_FOL_DAYS { get; set; }
        public string MO_DISABLE_PLAYSTORE { get; set; }
        public string MO_SAVE_DATA { get; set; }
        public string SQL_OPEN_ADDOUTLET { get; set; }
        public string SO_SYN_DISCOUNT { get; set; }
        public string SO_CUSTOM_DISCOUNT { get; set; }
        public string SO_SALES_ORDER { get; set; }
        public string SO_RESELLER_DISCOUNT { get; set; }
        public string TRACK_ACTUAL_LOCATION { get; set; }
    }
    public class TempCompanyModel
    {
        public string COMPANY_CODE { get; set; }
        public string COMPANY_EDESC { get; set; }
        public string FISCAL_START { get; set; }
        public string FISCAL_END { get; set; }
        public string BRANCH_CODE { get; set; }
        public string BRANCH_EDESC { get; set; }
    }
}
