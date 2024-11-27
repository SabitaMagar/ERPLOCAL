using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NeoErp.Distribution.Service.Model.Mobile
{
    public class CollectionRequestModel:CommonRequestModel
    {
        public string sp_code { get; set; }
        public string entity_code { get; set; }
        public string entity_type { get; set; }
        public string bill_no { get; set; }
        public string payment_mode { get; set; }
        public string cheque_clearance_date { get; set; }
        public string cheque_no { get; set; }
        public string bank_name { get; set; }
        public string cheque_deposit_bank { get; set; }
        public string amount { get; set; }
        public string remarks { get; set; }
        public string created_by { get; set; }
        public string otp_code { get; set; }

    }
}
