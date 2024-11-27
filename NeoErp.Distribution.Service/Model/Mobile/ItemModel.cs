using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NeoErp.Distribution.Service.Model.Mobile
{
    public class ItemModel:CommonRequestModel
    {
        public string ITEM_CODE { get; set; }
        public string ITEM_EDESC { get; set; }
        public string BRAND_NAME { get; set; }
        public string UNIT { get; set; }
        public string MU_EDESC { get; set; }
        public string MU_CODE { get; set; }
        public string CONVERSION_UNIT { get; set; }
        public string CONVERSION_FACTOR { get; set; }
        public string SALES_RATE { get; set; }
        public string APPLY_DATE { get; set; }
    }
    public class ItemModelNew// : CommonRequestModel
    {
        public string ITEM_CODE { get; set; }
        public string ITEM_EDESC { get; set; }
        public string BRAND_NAME { get; set; }
        public string UNIT { get; set; }
        public string MU_EDESC { get; set; }
        public string MU_CODE { get; set; }
        public string CONVERSION_UNIT { get; set; }
        public string CONVERSION_FACTOR { get; set; }
        public string COMPANY_CODE { get; set; }
        public string BRANCH_CODE { get; set; }
        //public string CONTRACT_CODE { get; set; }
    }
    public class ItemModelRate
    {
        public string CUSTOMER_CODE { get; set; }
        public string ITEM_CODE { get; set; }
        public string SALES_RATE { get; set; }
        public string SALES_RATE_ZERO { get; set; } 
        public string MRP_RATE { get; set; }
        public string RETAIL_PRICE { get; set; }
        public string APPLY_DATE { get; set; }
        public string COMPANY_CODE { get; set; }
        public string BRANCH_CODE { get; set; }
    }
    public class DiscountModel
    {
        public string CUSTOMER_CODE { get; set; }
        public string ITEM_CODE { get; set; }
        public string MU_CODE { get; set; }
        public decimal DISCOUNT_RATE { get; set; }
        public decimal DISCOUNT_PERCENT { get; set; }
        public string FORM_CODE { get; set; }
        public string SYNERGY_FLAG { get; set; }

    }
}