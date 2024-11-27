using NeoErp.Core.Models.CustomModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NeoErp.Distribution.Service.Model
{
    public class QuestionaireCustomerModel
    {
        public string ENTITY_CODE { get; set; }
        public string ENTITY_TYPE { get; set; }
        public string ENTITY_NAME { get; set; }
        public string ADDRESS { get; set; }
        public string CONTACT_NO { get; set; }
        public string AREA_NAME { get; set; }
        public string FULL_NAME { get; set; }

    }
    
    public class QuestionaireModel
    {
        public string CUSTOMER_CODE { get; set; }
        public string Question { get; set; }
        public string Answer { get; set; }
        public string Options { get; set; }
        public string Type { get; set; }
        public DateTime? CREATED_Date { get; set; }

        public int? TABLE_ID { get; set; }
        public string TABLE_TITLE { get; set; }
        public int? CELL_ID { get; set; }
        public int CELL_NO { get; set; }
        public int ROW_NO { get; set; }
        public string CELL_TYPE { get; set; }
        public string CELL_LABEL { get; set; }
        public int? ANSWER_ID { get; set; }
    }
    
    public class QuestionaireTempModel
    {
        public DateTime CREATED_DATE { get; set; }
        public int? TABLE_ID { get; set; }
        public string TABLE_TITLE { get; set; }
        public int? CELL_ID { get; set; }
        public int? CELL_NO { get; set; }
        public int? ROW_NO { get; set; }
        public string CELL_TYPE { get; set; }
        public string CELL_LABEL { get; set; }
        public int? ANSWER_ID { get; set; }
        public string ANSWER { get; set; }
    }
    
    public class DistributorListModel
    {
        public string DISTRIBUTOR_CODE { get; set; }
        public string DISTRIBUTOR_NAME { get; set; }
        public string CUSTOMERFLAG { get; set; }
        public string AREA_Code { get; set; }
        public string AREA_Name { get; set; }
        public string ADDRESS { get; set; }
        public string LONGITUDE { get; set; }
        public string LATITUDE { get; set; }
        public DateTime? CREATED_DATE { get; set; }
        public string ISACTIVE { get; set; }
        public string PARTY_TYPE_EDESC { get; set; }
        public string DEALER_CODE { get; set; }
        public string DEALER_NAME { get; set; }
        public string ACTIVE { get; set; }
        public int? GROUPID { get; set; }
        public string GROUP_EDESC { get; set; }
        public string ITEM_CODE { get; set; }
        public string COMPANY_CODE { get; set; }
        public string BRANCH_CODE { get; set; }
        public List<string> ItemCode { get; set; }
        public string ItemCodeString { get; set; }
        public int? WEIGHT { get; set; }
        public DistributorListModel()
        {
            ItemCode = new List<string>();
        }
    }
    
    public class OrderModel
    {
        public string DISTRIBUTOR_CODE { get; set; }
        public int OLD_INDEX { get; set; }
        public int NEW_INDEX { get; set; }
        public string GROUPID { get; set; }
        public List<string> DISTRIBUTOR_LIST { get; set; }
        public OrderModel()
        {
            DISTRIBUTOR_LIST = new List<string>();
        }
    }

    public class DealerModal
    {
        public string AREA_Code { get; set; }
        public string AREA_Name { get; set; }
        public string ACC_CODE { get; set; }
        public string ADDRESS { get; set; }
        public string LONGITUDE { get; set; }
        public string LATITUDE { get; set; }
        public DateTime CREATED_DATE { get; set; }
        public string ISACTIVE { get; set; }
        public string DEALER_CODE { get; set; }
        public string DEALER_NAME { get; set; }
        public string CONTACT_NO { get; set; }
        public string EMAIL { get; set; }
        public string PAN_NO { get; set; }
        public string VAT_NO { get; set; }
    }

    public class CustomerModel
    {
        public string Code { get; set; }
        public string Name { get; set; }
        
    }

    public class CategoryModel
    {
        public string CATEGORY_CODE { get; set; }
        public string CATEGORY_EDESC { get; set; }
    }

    

    public class ResellerListModel
    {
        public ResellerListModel()
        {
            Contacts = new List<ContactModel>();
            Distributors = new List<CustomerModel>();
            Wholesellers = new List<CustomerModel>();
        }
        public List<CustomerModel> Distributors { get; set; }
        public List<CustomerModel> Wholesellers { get; set; }
        public string Reseller_CODE { get; set; }
        public string Reseller_NAME { get; set; }
        public string RESELLER_CONTACT { get; set; }
        public string Distributor_CODE { get; set; }
        public string Distributor_Name { get; set; }
        public string AREA_Code { get; set; }
        public string AREA_Name { get; set; }
        public string ADDRESS { get; set; }
        public string Email { get; set; }
        public string Primary_Contact { get; set; }
        public string Primary_Contact_Name { get; set; }
        public string Primary_Contact_No { get; set; }
        public string LONGITUDE { get; set; }
        public string LATITUDE { get; set; }
        public string Vat_No { get; set; }
        public string Pan_No { get; set; }
        public string WholeSeller { get; set; }        
        public string Active { get; set; }
        public string REMARKS { get; set; }
        public int GROUP_ID { get; set; }
        public string GROUP_NAME { get; set; }
        public int OUTLET_TYPE_ID { get; set; }
        public int OUTLET_SUBTYPE_ID { get; set; }
        public string OUTLET_TYPE { get; set; }
        public string OUTLET_SUBTYPE { get; set; }
        public string DISTRIBUTER_DETAILS { get; set; }
        public string WHOLESELLER_DETAILS { get; set; }
        public string CONTACT_DETAILS { get; set; }
        public List<ContactModel> Contacts { get; set; }
        public List<string> StorePhotos { get; set; }
        public List<string> PContactPhotos { get; set; }
        public string ROUTE { get; set; }
        public string DELETED_FLAG { get; set; }
        public string SALES_PERSON { get; set; }
        public string SOURCE { get; set; }
        public string CONTACT_SUFFIX { get; set; }
        public string ROUTE_CODE { get; set; }
        public string IsClosed { get; set; }
        public string Created_by_name { get; set; }
        public DateTime? created_date { get; set; }
        public DateTime? LUPDATE_DATE { get; set; }
    }

    public class ContactModel
    {
        public string ContactSuffix { get; set; }
        public string Name { get; set; }
        public string Number { get; set; }
        public string Designation { get; set; }
        public string Primary { get; set; }
    }

    public class RouteGroup
    {
        public IGrouping<string, RouteImportModel> Data { get; set; }
    }

    public class RouteListModel
    {
        public string ROUTE_CODE { get; set; }
        public string ROUTE_NAME { get; set; }
        public int ENTITY_COUNT { get; set; }
        public int DIST_COUNT { get; set; }
        public int RESELLER_COUNT { get; set; }
        public int DEALER_COUNT { get; set; }
        public int HOARDING_COUNT { get; set; }
        public List<string> AREA_CODE { get; set; }      
        public List<RouteEntityModel> RouteEntityModel { get; set; }
        public string CREATED_BY { get; set; }
        public string CREATED_DATE { get; set; }
        public string DELETED_FLAG { get; set; }
        public string MODIFY_DATE { get; set; }
        public string MODIFY_BY { get; set; }        
        public string COMPANY_CODE { get; set; }
        public string BRANCH_CODE { get; set; }
        public string ROUTE_TYPE { get; set; } = "D";
    }
    public class RouteImportModel
    {
        public string ROUTE_NAME { get; set; }
        public string AREA { get; set; }
        public string ROUTE_TYPE { get; set; }
        public string ENTITY { get; set; }
        public string ENTITY_TYPE { get; set; }
    }

    public class RouteEntityModel
    {
        public string ENTITY_Code { get; set; }
        public string ENTITY_TYPE { get; set; }
        public string ORDER_NO { get; set; }
    }

    public class CustomerSales
    {
        public string customer_code { get; set; }
        public string LATITUDE { get; set; }
        public string LONGITUDE { get; set; }
        public decimal? sales { get; set; }
    }

    public class ItemWiseMinMaxReport
    {
        public decimal MINANS { get; set; }
        public decimal MAXANS { get; set; }
        public string ITEM_EDESC { get; set; }
        public string COMP_ITEM{ get; set; }
        public string AREA_NAME { get; set; }
        public decimal SALES_RATE { get; set; }
    }

    public partial class EODModel
    {
        public string SP_CODE { get; set; }
        public DateTime? SUBMIT_DATE { get; set; }
        public String EMPLOYEE_EDESC { get; set; }
        public String ADDRESS { get; set; }
        public String CONTACT { get; set; }
        public int PO_DCOUNT { get; set; }
        public int PO_RCOUNT { get; set; }

        public decimal ORDER_NO { get; set; }
        public string ITEM_CODE { get; set; }
        public string ITEM_EDESC { get; set; }
        public string BILLING_NAME { get; set; }
        public string CUSTOMER_EDESC { get; set; }
        public String GROUP_EDESC { get; set; }



        public string ORDER_ENTITY { get; set; }
        public string RESELLER_NAME { get; set; }
        public DateTime? ORDER_DATE { get; set; }
        public string CUSTOMER_CODE { get; set; }
        public string PARTY_TYPE_CODE { get; set; }
        public string PARTY_TYPE_EDESC { get; set; }
        public string PO_PARTY_TYPE { get; set; }
        public string MU_CODE { get; set; }
        public decimal? QUANTITY { get; set; }
        public decimal? UNIT_PRICE { get; set; } = 0;
        public decimal? TOTAL_PRICE { get; set; }
        public string REMARKS { get; set; }
        public string CREATED_BY { get; set; }
        public string AREA_NAME { get; set; }

        public int CURRENT_STOCK { get; set; }
        public int PURCHASE_QTY { get; set; }
        public string TYPE { get; set; }
        public string NAME { get; set; }
        public string EOD_LOCATION { get; set; }
    }

    public partial class AttendanceModel
    {
        public string SP_CODE { get; set; }
        public DateTime? ATT_DATE { get; set; }
        public DateTime? CHECKIN { get; set; }
        public DateTime? CHECKOUT { get; set; }
        public String FULL_NAME { get; set; }
        public string USER_NAME { get; set; }
        public string CONTACT_NO { get; set; }
        public string EMAIL { get; set; }       
        public String GROUP_EDESC { get; set; }
        public string ATN_LOCATION { get; set; }
        public string EOD_LOCATION { get; set; }
        public string FILENAME { get; set; }
        public DateTime? ATTNCHECKOUT_TIME { get; set; }
        public DateTime? ATTNCHECKIN_TIME { get; set; }
        public DateTime? FIRST_CALL { get; set; }
        public DateTime? LAST_CALL { get; set; }
        public DateTime? ATTNCHECKOUT { get; set; }
        public string ROUTE_NAME { get; set; }
        public string REMARKS { get; set; }


    }

    public partial class AdvancedFilterDistributionModel
    {
        public List<String> OutletFilter { get; set; } = new List<string>();
        public List<string> DistributorFilter { get; set; } = new List<string>();        
    }

    public class AccountStatementModel
    {
        public string VOUCHER_NO { get; set; }
        public DateTime VOUCHER_DATE { get; set; }
        public string PARTICULARS { get; set; }
        public string DR_AMOUNT { get; set; }
        public string CR_AMOUNT { get; set; }
        public string TRANSACTION_TYPE { get; set; }
        public string SUB_EDESC { get; set; }
        public string ACC_EDESC { get; set; }
        public decimal? BALANCE { get; set; }
    }

    public class AccountsFilterModel
    {
        public string AccountCode { get; set; }
        public string AccountName { get; set; }
    }
    public partial class DailyActivityModel
    {
        public string SP_CODE { get; set; }
        public string EMPLOYEE_EDESC { get; set; }
        public String GROUP_EDESC { get; set; }
        public string TOD_ROUTE_CODE { get; set; }
        public string TOD_ROUTE_NAME { get; set; }
        public string ADDRESS { get; set; }
        public DateTime? ATN_DATE { get; set; }
        public DateTime? EOD_DATE { get; set; }
        public int? TARGET { get; set; }
        public int? VISITED { get; set; }
        public int? NOT_VISITED { get; set; }
        public int? PJP_PRODUCTIVE { get; set; }
        public int? PJP_NON_PRODUCTIVE { get; set; }
        public int? PERCENT_EFFECTIVE_CALLS { get; set; }
        public string REMAKRS { get; set; }
        public string ATN_IMAGE { get; set; }
        public string ATN_LONGITUDE { get; set; }
        public string ATN_LATITUDE { get; set; }
        public string EOD_LONGITUDE { get; set; }
        public string EOD_LATITUDE { get; set; }
    }

    public class ClosingReportModel
    {
        public string ITEM_CODE { get; set; }
        public string ITEM_EDESC { get; set; }
        public string BRAND_NAME { get; set; }
        public string MU_CODE { get; set; }
        public string CUSTOMER_CODE { get; set; }
        public string CUSTOMER_EDESC { get; set; }
        public decimal OPENING_QTY { get; set; }
        public decimal PURCHASE_QTY { get; set; }
        public decimal QUANTITY { get; set; }
        public decimal SALES_QTY { get; set; }
        public decimal CLOSING_QTY { get; set; }
        public string NEPALIMONTH { get; set; }
        public DateTime? ORDER_DATE { get; set; }
    }

    public class OtherEntity
    {
        public string CODE { get; set; }
        public string DESCRIPTION { get; set; }
        public string CONTACT_PERSON { get; set; }
        public string AREA_CODE { get; set; }
        public int? GROUP_ID { get; set; }
        public string CONTACT_NO { get; set; }
        public string LATITUDE { get; set; }
        public string LONGITUDE { get; set; }
        public string COMPANY_CODE { get; set; }
        public string BRANCH_CODE { get; set; }
        public string DELETED_FLAG { get; set; }
        public string CREATED_BY { get; set; }
        public string AREA_NAME { get; set; }
        public DateTime? CREATED_DATE { get; set; }
        public string TYPE { get; set; }
    }

    public class ColumnModel
    {
        public string Header { get; set; }
        public string FieldName { get; set; }
    }
    public class DetailColumnModel
    {
        public DetailColumnModel()
        {
            OutletTypes = new List<ColumnModel>();
            OutletSubTypes = new List<ColumnModel>();
            Brands = new List<ColumnModel>();
        }
        public List<ColumnModel> OutletTypes { get; set; }
        public List<ColumnModel> OutletSubTypes { get; set; }
        public List<ColumnModel> Brands { get; set; }
    }

    public class MobileLogModel
    {
        public string SP_CODE { get; set; }
        public string FULL_NAME { get; set; }
        public string SWITCH_STATUS { get; set; }
        public int BATTERY_PERCENT { get; set; }
        public string LOG_TIME { get; set; }
        public string LOG_DATE { get; set; }
    }

    public class SummaryReport
    {
        public string GROUP_NAME { get; set; }
        public string FULL_NAME { get; set; }
        public string EMAIL { get; set; }
        public decimal? TOTAL_VISIT { get; set; }
        public decimal? TOTAL_OUTLET { get; set; }
        public decimal? TOTAL_COLLECTION { get; set; }
        public decimal? TOTAL_SALES { get; set; }
        public decimal? EFFECTIVECALLS { get; set; }
    }
    public class AttendanceReportCalendarModel
    {
        public string MONTH_DAY { get; set; }
        public string EMPLOYEE_ID { get; set; }
        public string ATTENDANCE_DT { get; set; }
        public string IN_TIME { get; set; }
        public string OUT_TIME { get; set; }
        public string ATTENDANCE_STATUS { get; set; }
        public string OVERALL_STATUS { get; set; }
    }
    public class DistResellerStockModel
    {
        public string CUSTOMER_CODE { get; set; }
        public string CUSTOMER_EDESC { get; set; }
        public string ITEM_CODE { get; set; }
        public string ITEM_EDESC { get; set; }
        public string MU_CODE { get; set; }
        public string SP_CODE { get; set; }
        public string EMPLOYEE_EDESC { get; set; }
        public int? CURRENT_STOCK { get; set; }
        public int? PURCHASE_QTY { get; set; }
        public string TYPE { get; set; }
        public DateTime? CREATED_DATE { get; set; }
        public string BRAND_NAME { get; set; }
        public string GROUP_EDESC { get; set; }
        public string TYPE_EDESC { get; set; }
        public string SUBTYPE_EDESC { get; set; }
        
    }
    public class DistResellerStockConversionModel
    {
        public string CUSTOMER_CODE { get; set; }
        public string CUSTOMER_EDESC { get; set; }
        public string ITEM_CODE { get; set; }
        public string ITEM_EDESC { get; set; }
        public string MU_CODE { get; set; }
        public string SP_CODE { get; set; }
        public string EMPLOYEE_EDESC { get; set; }
        public double? CURRENT_STOCK { get; set; }
        public int? PURCHASE_QTY { get; set; }
        public string TYPE { get; set; }
        public DateTime? CREATED_DATE { get; set; }
        public string BRAND_NAME { get; set; }
        public string GROUP_EDESC { get; set; }
        public string TYPE_EDESC { get; set; }
        public string SUBTYPE_EDESC { get; set; }

    }
    public class ResellerDistributorModel
    {
        public string CUSTOMER_EDESC { get; set; }
        public string CUSTOMER_CODE { get; set; }

    }
    public class DistdistributorStockModel
    {
        public string CUSTOMER_CODE { get; set; }
        public string CUSTOMER_EDESC { get; set; }
        public string ITEM_CODE { get; set; }
        public string ITEM_EDESC { get; set; }
        public string MU_CODE { get; set; }
        public string SP_CODE { get; set; }
        public string EMPLOYEE_EDESC { get; set; }
        public int? CURRENT_STOCK { get; set; }
        public int? PURCHASE_QTY { get; set; }
        public string TYPE { get; set; }
        public DateTime? CREATED_DATE { get; set; }
        public string BRAND_NAME { get; set; }
        public string GROUP_EDESC { get; set; }
        public string TYPE_EDESC { get; set; }
        public string SUBTYPE_EDESC { get; set; }
    }

    public class DistAttendanceReportSummary
    {
        public string GroupName { get; set; }
        public Decimal TotalPresent { get; set; }
        public Decimal TotalAbsent { get; set; }
        public Decimal TotalLeave { get; set; }
        public Decimal DayOff { get; set; }
        public Decimal Holiday { get; set; }
        public Decimal WorkAtDayOff { get; set; }
        public Decimal WorkAtHoliday { get; set; }
        public string FULL_NAME { get; set; }
        public string EMPLOYEE_ID { get; set; }

        public decimal? TARGET { get; set; }
        public decimal? TOTAL_VISIT { get; set; }
        public decimal? PRODUCTIVE { get; set; }
        public decimal? Coverage { get; set; }
        public decimal? Effective { get; set; }
    }

    public class StockSummary
    {
        
        public string RESELLER_CODE { get; set; }
        public string RESELLER_NAME { get; set; }
        public String  AREA_CODE { get; set; }
        public string AREA_NAME { get; set; }
        public int GROUPID { get; set; }
        public string GROUP_EDESC { get; set; }
        public decimal? BO { get; set; }
        public decimal? RVG { get; set; }
        public decimal? RV { get; set; }
        public decimal? WB { get; set; }
        public decimal? NB { get; set; }
        public decimal? RT { get; set; }
        public decimal? RR { get; set; }
        public decimal? RGS { get; set; }
        public decimal? RVU { get; set; }
        public decimal? MI { get; set; }
        public decimal? RSB { get; set; }
        public decimal? CE { get; set; }
        public decimal? HB { get; set; }
        public decimal? GO { get; set; }
        public decimal? UG { get; set; }
        public decimal? BD { get; set; }
        public decimal? MS { get; set; }
        public decimal? GMR { get; set; }
        public decimal? HA { get; set; }
        public string DISTRIBUTOR_NAME { get; set; }
    }

    public class StockDetailReort
    {
        public string FULL_NAME { get; set; }
        public DateTime CREATED_DATE { get; set; }
        public string RESELLER_CODE { get; set; }
        public string RESELLER_NAME { get; set; }
        public string BRAND_NAME { get; set; }
        public decimal? CURRENT_STOCK { get; set; }
        public string MU_CODE { get; set; }
       public string ITEM_EDESC { get; set; }
    }
}

