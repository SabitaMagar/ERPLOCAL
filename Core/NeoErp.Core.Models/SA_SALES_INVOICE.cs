//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated from a template.
//
//     Manual changes to this file may cause unexpected behavior in your application.
//     Manual changes to this file will be overwritten if the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

namespace NeoErp.Core.Models
{
    using System;
    using System.Collections.Generic;
    
    public partial class SA_SALES_INVOICE
    {
        public string SALES_NO { get; set; }
        public System.DateTime SALES_DATE { get; set; }
        public string MANUAL_NO { get; set; }
        public string CUSTOMER_CODE { get; set; }
        public string BUDGET_FLAG { get; set; }
        public short SERIAL_NO { get; set; }
        public string ITEM_CODE { get; set; }
        public string MU_CODE { get; set; }
        public decimal QUANTITY { get; set; }
        public Nullable<decimal> UNIT_PRICE { get; set; }
        public Nullable<decimal> TOTAL_PRICE { get; set; }
        public Nullable<decimal> CALC_QUANTITY { get; set; }
        public Nullable<decimal> CALC_UNIT_PRICE { get; set; }
        public Nullable<decimal> CALC_TOTAL_PRICE { get; set; }
        public Nullable<decimal> COMPLETED_QUANTITY { get; set; }
        public string REMARKS { get; set; }
        public string FORM_CODE { get; set; }
        public string COMPANY_CODE { get; set; }
        public string BRANCH_CODE { get; set; }
        public string CREATED_BY { get; set; }
        public System.DateTime CREATED_DATE { get; set; }
        public string DELETED_FLAG { get; set; }
        public Nullable<byte> CREDIT_DAYS { get; set; }
        public string CURRENCY_CODE { get; set; }
        public Nullable<decimal> EXCHANGE_RATE { get; set; }
        public string SYN_ROWID { get; set; }
        public string TRACKING_NO { get; set; }
        public string FROM_LOCATION_CODE { get; set; }
        public string SESSION_ROWID { get; set; }
        public string BATCH_NO { get; set; }
        public Nullable<System.DateTime> MODIFY_DATE { get; set; }
        public string MODIFY_BY { get; set; }
        public Nullable<decimal> FREE_QTY { get; set; }
        public string PARTY_TYPE_CODE { get; set; }
        public string SMS_FLAG { get; set; }
        public Nullable<decimal> ROLL_QTY { get; set; }
        public string PRIORITY_CODE { get; set; }
        public string PAYMENT_MODE { get; set; }
        public string MEMBER_SHIP_CARD { get; set; }
        public string PAYMODE_VALUE { get; set; }
        public string DESCRIPTION { get; set; }
        public string SHIPPING_ADDRESS { get; set; }
        public string SHIPPING_CONTACT_NO { get; set; }
        public string SALES_TYPE_CODE { get; set; }
        public string REASON { get; set; }
        public string MISC_CODE { get; set; }
        public string AGENT_CODE { get; set; }
        public string DIVISION_CODE { get; set; }
        public string EMPLOYEE_CODE { get; set; }
    }
}
