using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NeoErp.Distribution.Service.Model.Mobile
{
    public class UpdateResponseModel
    {
        public int ID { get; set; }
        public string CODE { get; set; }
        public string TYPE_EDESC { get; set; }
        public string DELETED_FLAG { get; set; }
    }
}
