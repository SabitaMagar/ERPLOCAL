using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NeoErp.Planning.Service.Models
{
    public class PlanReportModel
    {
        public string PLAN_CODE { get; set; }
        public string PLAN_EDESC { get; set; }
        public string PLAN_DATE { get; set; }
        public string MITI { get; set; }
        public string MONTH { get; set; }
        public string YEAR { get; set; }
        public string MONTHINT { get; set; }
        public string ITEM_CODE { get; set; }
        public string ITEM_EDESC { get; set; }

        public string ITEM_SUBGROUP_EDESC { get; set; }
        public string ITEM_GROUP_EDESC { get; set; }


        public string CUSTOMER_CODE { get; set; }
        public string CUSTOMER_EDESC { get; set; }

        public string BRANCH_CODE { get; set; }
        public string BRANCH_EDESC { get; set; }

        public string BUDGET_CODE { get; set; }
        public string BUDGET_EDESC { get; set; }
        public string DIVISION_CODE { get; set; }
        public string DIVISION_EDESC { get; set; }

        public string EMPLOYEE_CODE { get; set; }
        public string EMPLOYEE_EDESC { get; set; }

        public decimal? REQUIRED_QUANTITY { get; set; }
        public decimal? MATERIAL_QUANTITY { get; set; }
        public decimal? STOCK { get; set; }
        public decimal? PO_PENDING { get; set; }
        public decimal? PO { get; set; }

        public decimal? PER_DAY_AMOUNT { get; set; }
        public decimal? PER_DAY_QUANTITY { get; set; }

        public string COMPANY_CODE { get; set; }

        public string REMARKS { get; set; }
        
        public string CREATED_BY { get; set; }
        public DateTime CREATED_DATE { get; set; }
        public int TotalRecord { get; set; }
        public string DESCRIPTION { get; set; }
    }
    public class ItemGroupModel
    {
        public string ITEM_CODE { get; set; }
        public string ITEM_EDESC { get; set; }
        public string MU_CODE { get; set; }
        public string MASTER_ITEM_CODE { get; set; }
        public string PRE_ITEM_CODE { get; set; }
    }
    public class CustomerGroupModel
    {
        public string CUSTOMER_CODE { get; set; }
        public string CUSTOMER_EDESC { get; set; }
        public string MASTER_CUSTOMER_CODE { get; set; }
        public string PRE_CUSTOMER_CODE { get; set; }
    }
}
