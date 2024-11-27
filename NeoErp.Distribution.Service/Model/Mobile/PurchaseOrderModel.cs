using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NeoErp.Distribution.Service.Model.Mobile
{
    public class PurchaseOrderModel:CommonRequestModel
    {
        public string reseller_code { get; set; }
        public string distributor_code { get; set; }
        public string type { get; set; }
        public string login_code { get; set; }
        public string Order_No { get; set; }
        public string Order_Date { get; set; }
        public string Dispatch_From { get; set; }
        public string WholeSeller_Code { get; set; }
        public UpdateRequestModel LocationInfo { get; set; }
        public List<ProductModel> products { get; set; }
    }
    public class ProductModel
    {
        public string item_code { get; set; }
        public string mu_code { get; set; }
        public string form_code { get; set; }
        public int quantity { get; set; }
        public string reject_flag { get; set; }
        public decimal rate { get; set; }
        public decimal discountRate { get; set; }
        public decimal discountPercentage { get; set; }
        public decimal discount { get; set; }
        public string billing_name { get; set; }
        public string remarks { get; set; }
        public string party_type_code { get; set; }
        public string Sync_Id { get; set; }
        public string PRIORITY_STATUS_CODE { get; set; }
        public string CITY_CODE { get; set; }
        public string SHIPPING_CONTACT { get; set; }
        public string SALES_TYPE_CODE { get; set; }
    }

    public class CancelledProductModal
    {
        public string SYNC_ID { get; set; }
        public string ITEM_CODE { get; set; }
        public string MU_CODE { get; set; }
        public decimal RATE { get; set; }
        public decimal QUANTITY { get; set; }
        public string SHIPPING_CONTACT { get; set; }
        public string REMARKS { get; set; }
        public string BILLING_NAME { get; set; }
        public string PARTY_TYPE_CODE { get; set; }
        public string REJECT_FLAG { get; set; }
    }

    public class CancelPurchaseOrderModal : CommonRequestModel
    {
        public string ORDER_NO { get; set; }
        public string SERVER_GENERATED_ORDER_NO { get; set; }
        public string ORDER_DATE { get; set; }
        public string TYPE { get; set; }
        public string RESELLER_CODE { get; set; }
        public string DISTRIBUTOR_CODE { get; set; }
        public DateTime CREATED_DATE { get; set; }
        public DateTime SAVED_DATE { get; set; }
        public string WHOLESELLER_CODE { get; set; }
        public string BILLING_NAME { get; set; }
        public string DISPATCH_FROM { get; set; }
        public string TEMP_SYNC_ID { get; set; }
        public List<CancelledProductModal> products { get; set; }
        public UpdateRequestModel LocationInfo { get; set; }
    }
}
