using NeoErp.Core;
using NeoErp.Distribution.Service.Model;
using NeoErp.Distribution.Service.Service;
using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Web.Http;

namespace NeoErp.Distribution.Controllers.Api
{
    public class UserSetupController : ApiController
    {
        public IUserService _objectEntity { get; set; }
        private IWorkContext _workContext;

        public UserSetupController(IUserService userService, IWorkContext workContext)
        {
            this._objectEntity = userService;
            this._workContext = workContext;
        }
        [HttpGet]
        public List<UserSetupModel> GetUserList()
        {
            var data = _objectEntity.GetAllUserList(_workContext.CurrentUserinformation);
            return data;
        }

        public List<EmployeeModel> GetEmployee()
        {
            var data = _objectEntity.getEmployeeList();
            return data;
        }
        public List<EmployeeModel> getUserEmployee()
        {
            var data = _objectEntity.getUserEmployee(_workContext.CurrentUserinformation);
            return data;
        }
        public List<EmployeeModel> GetSalesPersonList()
        {
            var data = _objectEntity.GetSalesPersonList(_workContext.CurrentUserinformation);
            return data;
        }
        public List<DistributorModel>GetDistributor()
        {
            var data = _objectEntity.getDistributorList(_workContext.CurrentUserinformation);
            return data;

        }
        public List<NewPartyTypeModel> GetPartyType()
        {
            var data = _objectEntity.GetPartyType(_workContext.CurrentUserinformation);
            return data;
        }

        public List<ResellerModel>GetReseller()
        {
            var data = _objectEntity.getResellerList(_workContext.CurrentUserinformation);
            return data;
        }

        public List<SalesPersonModel> GetSalePerson()
        {
            var data = _objectEntity.getSalesPersonList(_workContext.CurrentUserinformation);
            return data;
        }
        public List<ItemModel> GetItemAndBrand()
        {
            var data = _objectEntity.GetItemAndBrand(_workContext.CurrentUserinformation);
            return data;
        }

        [HttpPost]
        public string InsertUserDetails(UserSetupModel modal)
        {
            if(modal !=null)
            {

            string message =_objectEntity.insertUserDetails(modal, _workContext.CurrentUserinformation);
                return message;
                   
            }
            else
            {
                return "Failed to insert";
            }
        }
        public string GetSPCodeByCustomerCode(string SP_CODE,string USER_TYPE,int USERID)
        {
            var data = _objectEntity.GetSPCodeByCustomerCode(SP_CODE, USER_TYPE, USERID,_workContext.CurrentUserinformation);
            return data;
        }

        //public string UpdateAssign (UserSetupModel modal)
        //{

        //        _objectEntity.UpdateUserAssign(modal);
        //        return "assign success";

        //    {
        //        return "Failed to insert";
        //    }
        //}

        public List<UserSetupModel> UpdateAssign(UserSetupModel modal)
        {
            var result = _objectEntity.UpdateUserAssign(modal, _workContext.CurrentUserinformation);
            return result;
        }


        [HttpGet]
        public List<UserSetupModel> GetAssignList(UserSetupModel modal)
        {
            var data = _objectEntity.GetAllAssignList(modal);
            return data;
        }

        [HttpPost]
        public string UpdatePreferenceSetup(PreferenceSetupModel modal)
        {
            _objectEntity.UpdatePreferenceSetup(modal,_workContext.CurrentUserinformation);
            return "success";
                
        }

        [HttpPost]
        public HttpResponseMessage UpdateMobileReg(MobileRegModel model)
        {
            try
            {
                var result = _objectEntity.UpdateDevice(model, _workContext.CurrentUserinformation);
                if (result == "500")
                    return Request.CreateResponse(HttpStatusCode.OK, new { TYPE = "success", MESSAGE = "Mobile device updated successfully" });
                else
                    return Request.CreateResponse(HttpStatusCode.OK, new { TYPE = "error", MESSAGE = "Something went wrong" });
            }
            catch (Exception ex)
            {
                return Request.CreateResponse(HttpStatusCode.OK, new { TYPE = "error", MESSAGE = "Something went wrong" });
            }
        }

        [HttpPost]
        public HttpResponseMessage DeleteMobileReg(MobileRegModel model)
        {
            try
            {
                var result = _objectEntity.DeleteDevice(model);
                if (result == "500")
                    return Request.CreateResponse(HttpStatusCode.OK, new { TYPE = "success", MESSAGE = "Mobile device deleted successfully" });
                else
                    return Request.CreateResponse(HttpStatusCode.OK, new { TYPE = "error", MESSAGE = "Something went wrong" });
            }
            catch (Exception ex)
            {
                return Request.CreateResponse(HttpStatusCode.OK, new { TYPE = "error", MESSAGE = "Something went wrong" });
            }
        }
    }
}
