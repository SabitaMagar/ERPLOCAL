using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NeoErp.Distribution.Service.Model
{
    public class CollectionModel
    {
        public CollectionModel()
        {
            DIVISIONS = new List<DivisionModel>();
        }
        public List<DivisionModel> DIVISIONS { get; set; }
        public string SP_CODE { get; set; }
        public string SALESPERSON_NAME { get; set; }
        public string ENTITY_CODE { get; set; }
        public string ENTITY_TYPE { get; set; }
        public string ENTITY_NAME { get; set; }
        public string BILL_NO { get; set; }
        public string PAYMENT_MODE { get; set; }
        public string CHEQUE_NO { get; set; }
        public string BANK_NAME { get; set; }
        public decimal? AMOUNT { get; set; }
        public string REMARKS { get; set; }
        public DateTime? CHEQUE_CLEARANCE_DATE { get; set; }
        public string MITI { get; set; }
        public string CHEQUE_DEPOSIT_BANK { get; set; }
        public string DELETED_FLAG { get; set; }
        public string CREATED_DATE { get; set; }
        public string SAVE_FLAG { get; set; }
        public string CHEQUE_IMAGE { get; set; }
        public string SIGNATURE_IMAGE { get; set; }
        public string CASH_IMAGE { get; set; }

    }
    public class DivisionModel
    {
        public string DIVISION_CODE { get; set; }
        public string DIVISION_EDESC { get; set; }
        public decimal AMOUNT { get; set; }
    }
}
