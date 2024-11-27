using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NeoErp.Distribution.Service.Model.Mobile
{
    public class ReturnConditionModel
    {
        public int ID { get; set; }
        public string CODE { get; set; }
        public string RETURN_TYPE { get; set; }
        public string COMPANY_CODE { get; set; }
    }

    public class DATERANGE
    {
        public string RANGENAME { get; set; }
        public string STARTDATE { get; set; }
        public string ENDDATE { get; set; }

    }
}
