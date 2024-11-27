using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NeoErp.Distribution.Service.Model.Mobile
{
    public class CreateDistributorModel : CommonRequestModel
    {
        public CreateDistributorModel()
        {
            contact = new List<ContactModel>();
            customer = new List<CustomerModel>();
        }
        public string distributor_code { get; set; }
        public string distributor_name { get; set; }
        public string area_code { get; set; }
        public string address { get; set; }
        public string pan { get; set; }
        // public string wholeseller { get; set; }
        public string DISTRIBUTOR_TYPE_ID { get; set; }
        public string DISTRIBUTOR_SUBTYPE_ID { get; set; }
        public string Group_id { get; set; }
        public string email { get; set; }

        public string ROUTE_CODE { get; set; }
        public string Distributor_contact { get; set; }
        public List<ContactModel> contact { get; set; }
        public List<CustomerModel> customer { get; set; }
        //public string ROUTE_CODE { get; set; }
    }
    //public class ContactModel
    //{
    //    public string contact_suffix { get; set; }
    //    public string name { get; set; }
    //    public string number { get; set; }
    //    public string designation { get; set; }
    //    public string primary { get; set; }
    //    public string Sync_Id { get; set; }
    //}
}
