using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NeoErp.Distribution.Service.Model.Mobile
{
    public class CollectionModel
    {
        public string NAME { get; set; }
        public decimal TARGET_AMOUNT { get; set; }
        public decimal AMOUNT_ACHIEVED { get; set; }
        public decimal AMOUNT_ACCOMPLISED { get; set; }
        public string QUANTITY_ACHIEVED { get; set; }
        public string TARGET_QUANTITY { get; set; }
        public string QUANTITY_ACCOMPLISED { get; set; }
    }
}
