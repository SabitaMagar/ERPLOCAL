using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NeoErp.Distribution.Service.Model.Mobile
{
    public class DistributionSalesReturnModel: CommonRequestModel
    {
        public string Id { get; set; }
        public string Response { get; set; }
        public string ORDER_NO { get; set; }
        public UpdateRequestModel locationinfo { get; set; }
        public string SERVER_GENERATED_ORDER_NO { get; set; }
        public string P_KEY { get; set; }
        public DateTime ORDER_DATE { get; set; }
        public string ENTITY_TYPE { get; set; }
        public string RESELLER_CODE { get; set; }
        public string CUSTOMER_CODE { get; set; }
        public string DISTRIBUTOR_CODE { get; set; }
        public string CREATED_BY { get; set; }
        public DateTime CREATED_DATE { get; set; }
        public DateTime SAVED_DATE { get; set; }
        public string CURRENCY_CODE { get; set; }
        public string EXCHANGE_RATE { get; set; }
        public string APPROVED_FLAG { get; set; }
        public string DISPATCH_FLAG { get; set; }
        public string WHOLESELLER_CODE { get; set; }
        public string ACKNOWLEDGE_FLAG { get; set; }
        public string REJECT_FLAG { get; set; }
        public string DELETED_FLAG { get; set; }
        public string SYN_ROWID { get; set; }
        public DateTime MODIFY_DATE { get; set; }
        public string MODIFY_BY { get; set; }
        public string BILLING_NAME { get; set; }
        public string DISPATCH_FROM { get; set; }
        public string CONDITION { get; set; }
        public string COMPLAIN_TYPE { get; set; }
        public string SERIOUSNESS { get; set; }
        public string REMARKS_DIST { get; set; }
        public string REMARKS_ASM { get; set; }
        public List<SalesReturnProductInfo> products { get; set; } = new List<SalesReturnProductInfo>();

    }


    public class SalesReturnLocationUpdate
    {
        public string USER_ID { get; set; }
        public string SP_CODE { get; set; }
        public string LATITUDE { get; set; }
        public string LONGITUDE { get; set; }
        public string CUSTOMER_CODE {get;set;}
        public string CUSTOMER_TYPE { get; set; }
        public string REMARKS { get; set; }
        public DateTime UPDATE_DATE { get; set; }
        public DateTime SAVED_DATE { get; set; }
        public string ROUTE_CODE { get; set; }
        public string IS_VISITED { get; set; }
        public string COMPANY_CODE { get; set; }
        public string BRANCH_CODE { get; set; }
        
    }

    public class SalesReturnProductInfo
    {
       public string  SYNC_ID { get; set; }
       public string MU_CODE { get; set; }
       public string  ITEM_CODE { get; set; }
       public string  ITEM_EDESC { get; set; }
       public string  MBF_DATA { get; set; }
       public string EXP_DATE { get; set; }
       public string BATCH_NO { get; set; }
       public string CONDITION { get; set; }
       public string COMPLAIN_TYPE { get; set; }
       public string SERIOUSNESS { get; set; }
       public string REMARKS_DIST { get; set; }
       public string REMARKS_ASM { get; set; }
       public string  QUANTITY { get; set; }
       public string  SHIPPING_CONTACT { get; set; }
       public string  BILLING_NAME { get; set; }
       public string  PARTY_TYPE_CODE { get; set; }
    }


}
