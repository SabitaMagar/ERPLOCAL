using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NeoErp.Distribution.Service.Model.Mobile
{
    public class ProductWiseModel
    {
        public string NAME { get; set; }
        public decimal TARGET_QUANTITY { get; set; }
        public decimal TARGET_AMOUNT { get; set; }
        public decimal  QUANTITY_ACHIEVED { get; set; }
        public decimal AMOUNT_ACHIEVED { get; set; }
        public decimal QUANTITY_ACCOMPLISED { get; set; }
        public decimal AMOUNT_ACCOMPLISED { get; set; }
    }
    public class ProductQuantityWiseModel
    {
        public string ITEM_NAME { get; set; }
        public string ITEM_CODE { get; set; }
        public string Quantity_name { get; set; }
        public string Amount_name { get; set; }
        public decimal TARGET_QUANTITY { get; set; }
        public decimal TARGET_AMOUNT { get; set; }
        public decimal QUANTITY_ACHIEVED { get; set; }
        public decimal AMOUNT_ACHIEVED { get; set; }
        public decimal QUANTITY_ACCOMPLISED { get; set; }
        public decimal AMOUNT_ACCOMPLISED { get; set; }
    }
    public class QuantityModel
    {
        public string NAME { get; set; }
        public decimal TARGET_QUANTITY { get; set; }
        public decimal QUANTITY_ACHIEVED { get; set; }
        public decimal QUANTITY_ACCOMPLISED { get; set; }
        public decimal TARGET_AMOUNT { get; set; }
        public decimal AMOUNT_ACHIEVED { get; set; }
        public string AMOUNT_ACCOMPLISED { get; set; }

    }
    public class CustomerWiseModel
    {
        public string NAME { get; set; }
        public decimal TARGET_QUANTITY { get; set; }
        public decimal TARGET_AMOUNT { get; set; }
        public decimal QUANTITY_ACHIEVED { get; set; }
        public decimal AMOUNT_ACHIEVED { get; set; }
        public decimal QUANTITY_ACCOMPLISED { get; set; }
        public decimal AMOUNT_ACCOMPLISED { get; set; }
    }
    public class AreaWiseModel
    {
        public string NAME { get; set; }
        public decimal TARGET_QUANTITY { get; set; }
        public decimal TARGET_AMOUNT { get; set; }
        public decimal QUANTITY_ACHIEVED { get; set; }
        public decimal AMOUNT_ACHIEVED { get; set; }
        public decimal QUANTITY_ACCOMPLISED { get; set; }
        public decimal AMOUNT_ACCOMPLISED { get; set; }
    }
    public class AreaCusWiseModel
    {
        public string NAME { get; set; }
        public string AREA_CODE { get; set; }
        public string CUSTOMER_EDESC { get; set; }
        public string CUSTOMER_CODE { get; set; }
        public decimal TARGET_QUANTITY { get; set; }
        public decimal TARGET_AMOUNT { get; set; }
        public decimal QUANTITY_ACHIEVED { get; set; }
        public decimal AMOUNT_ACHIEVED { get; set; }
        public decimal QUANTITY_ACCOMPLISED { get; set; }
        public decimal AMOUNT_ACCOMPLISED { get; set; }
    }
    public class VisitPlanWiseModel
    {
        public string NAME { get; set; }
        public decimal TARGET_QUANTITY { get; set; }
        public decimal QUANTITY_ACHIEVED { get; set; }
        public decimal QUANTITY_ACCOMPLISED { get; set; }

        public string TARGET_AMOUNT { get; set; }
        public string AMOUNT_ACHIEVED { get; set; }
        public string AMOUNT_ACCOMPLISED { get; set; }
    }
}
