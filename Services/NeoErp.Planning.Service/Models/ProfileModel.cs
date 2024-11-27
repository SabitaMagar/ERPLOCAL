using System;
using System.Collections.Generic;

namespace NeoErp.Planning.Service.Models
{
    public class ProfileModel
    {
        public int TargetId { get; set; }
        public string TargetName { get; set; }
        public string TargetType { get; set; }
        public string SubTargetType { get; set; }
        public string DateFilter { get; set; }
        public string EmployeeMasterGroup { get; set; }
        public string EmployeeGroup { get; set; }
        public List<string> Employees { get; set; }
        public string ItemGroup { get; set; }
        public string ItemMasterGroup { get; set; }
        public List<Item> Items { get; set; }
        public string CustomerGroup { get; set; }
        public string CustomerMasterGroup { get; set; }
        public List<string> Customers { get; set; }
        public List<GridData> GridData { get; set; }
        public string FLAG { get; set; }
    }
    public class Item
    {
        public string ITEM_CODE { get; set; }
        public string MU_CODE { get; set; }
    }

    public class GridData
    {
        public string ItemCode { get; set; }
        public string muCode { get; set; }
        public string Date { get; set; }
        public decimal Quantity { get; set; }
        public decimal Amount { get; set; }
    }
    public class TARGET_PLAN
    {
        public int TARGET_ID { get; set; }
        public string TARGET_NAME { get; set; }
        public DateTime? FROM_DATE { get; set; }
        public DateTime? END_DATE { get; set; }
        public string TARGET_TYPE { get; set; }
        public string SUB_TARGET_TYPE { get; set; }
    }
    public class TARGET_DETAILS
    {
        public List<TargetData> TargetData { get; set; }

        public List<DateFilter> DateFilter { get; set; }
    }
    public class DateFilter
    {
        public DateTime START_DATE { get; set; }
        public DateTime LAST_DATE { get; set; }
        public string DATE_FILTER { get; set; }

    }
    public class TargetData
    {
        public int TARGET_ID { get; set; }
        public string TARGET_NAME { get; set; }
        public string MONTH { get; set; }
        public string MASTER_CODE { get; set; }
        public string CODE { get; set; }
        public string TARGET_TYPE { get; set; }
        public string SUB_TARGET_TYPE { get; set; }
        public string MU_CODE { get; set; }
        public decimal? TARGET_QUANTITY { get; set; }
        public decimal? TARGET_AMOUNT { get; set; }
        public DateTime FROM_DATE { get; set; }
        public DateTime END_DATE { get; set; }
        public string COMPANY_CODE { get; set; }
        public string BRANCH_CODE { get; set; }
        public string ITEM_GROUP { get; set; }
        public string EMPLOYEE_GROUP { get; set; }
        public string CUSTOMER_GROUP { get; set; }
        public string ASSIGN_EMPLOYEE { get; set; }
        public DateTime CREATED_DATE { get; set; }
        public string CREATED_BY { get; set; }
        public string FLAG { get; set; }
        public char DELETED_FLAG { get; set; }
    }

    public class CustomerGroup
    {
        public int GROUP_ID { get; set; }
        public string GROUP_EDESC { get; set; }
        public string GROUP_CODE { get; set; }

    }
    public class CustomerSNGroup
    {
        public string GROUP_ID { get; set; }
        public string GROUP_EDESC { get; set; }
        public string GROUP_CODE { get; set; }
        public string MASTER_CUSTOMER_CODE { get; set; }
        public string PRE_CUSTOMER_CODE { get; set; }

    }
}
