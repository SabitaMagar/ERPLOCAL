using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace NeoErp.Distribution.Service.Model
{
    public class UserSetupModel
    {
        public int USERID { get; set; }
        public int? ROLE_CODE { get; set; }
        public string PASS_WORD { get; set; }
        public string USER_NAME { get; set; }
        public string USER_TYPE { get; set; }
        public string SP_CODE { get; set; }
        [DisplayFormat(ApplyFormatInEditMode = true, DataFormatString = "{0:dd-MM-yy}")]
        public DateTime EXPIRY_DATE { get; set; }
        public string IS_MOBILE { get; set; }
        public string ACTIVE { get; set; }
        public string ROLE_NAME { get; set; }
        public string SUPERVISOR_CODE { get; set; }
        public DateTime CREATED_DATE { get; set; }
        public string ATTENDANCE { get; set; }
        public string OLD_SP_CODE { get; set; }
        public string GROUPID { get; set; }
    }



    public class UserSetupTreeModel
    {
        public int CODE { get; set; }
        public int? MASTER_CODE { get; set; }
        public int? MASTER_CUSTOMER_CODE { get; set; }
        public string NAME { get; set; }
        public string PASSWORD { get; set; }
        public string FULLNAME { get; set; }
        public string EMPLOYEE_EDESC { get; set; }
        public string ROLE_NAME { get; set; }
        public string CONTACT_NO { get; set; }
        public int? ROLE_CODE { get; set; }
        public string EMPLOYEE_CODE { get; set; }
        public string ATTENDENCE { get; set; } = "N";
        public string MOBILE { get; set; } = "N";
        public string EMAIL { get; set; }
        public int? GROUPID { get; set; }
        public String IS_GROUP { get; set; }
        public List<string> AREA { get; set; }
        public List<string> CUSTOMER { get; set; }
        public List<string> ITEMS { get; set; }
        public string ITEM_CODE { get; set; }
        public string AREA_CODE { get; set; }
        public string CUSTOMER_CODE { get; set; }

        public string AREA_NAME { get; set; }
        public string ACTIVE { get; set; }
        public string BRANDING { get; set; }
        public string SUPER_USER { get; set; }
    }

   

    public class UserRoleModel
    {        
        public int ROLE_CODE { get; set; }
        public string ROLE_NAME { get; set; }
    }

    public class DistUserEmployeeModel {
        public string LOGIN_SP_CODE { get; set; }
        public string LOGIN_SP_EDESC { get; set; }
        public string DIST_SP_CODE { get; set; }
        public string DIST_SP_EDESC { get; set; }
    }
}