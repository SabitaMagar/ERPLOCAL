using NeoErp.Distribution.Service.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NeoErp.Data;
using NeoErp.Core.Models;
using NeoErp.Core.Domain;

namespace NeoErp.Distribution.Service.Service
{
    public class UserService : IUserService
    {
        private NeoErpCoreEntity _objectEntity;
        public UserService(NeoErpCoreEntity objectEntity)
        {
            this._objectEntity = objectEntity;
        }

        public List<UserSetupModel> GetAllUserList(User userInfo)
        {

            //string query = @"select  USERID, USER_NAME , PASS_WORD,USER_TYPE,SP_CODE,EXPIRY_DATE,IS_MOBILE,ACTIVE FROM DIST_LOGIN_USER";
            //            string query =string.Format(@"select a.USERID , a.USER_NAME,a.PASS_WORD, a.USER_TYPE,a.SP_CODE,a.EXPIRY_DATE,a.IS_MOBILE,a.ACTIVE ,d.ROLE_CODE,m.ROLE_NAME FROM 
            //DIST_LOGIN_USER a 
            //left JOIN DIST_ROLE_USER d on a.USERID = d.USERID
            //left JOIN DIST_ROLE_MASTER_SETUP m on m.ROLE_CODE = d.ROLE_CODE");
            string query = @"select a.USERID , a.USER_NAME,a.PASS_WORD, TO_CHAR(a.GROUPID) GROUPID, a.USER_TYPE,a.SP_CODE,TO_CHAR(x.SUPERVISOR_CODE)SUPERVISOR_CODE,a.EXPIRY_DATE,a.IS_MOBILE,a.ACTIVE,a.ATTENDANCE ,d.ROLE_CODE,m.ROLE_NAME FROM 
                            DIST_LOGIN_USER a 
                            left JOIN DIST_ROLE_USER d on a.USERID = d.USERID
                            left JOIN DIST_ROLE_MASTER_SETUP m on m.ROLE_CODE = d.ROLE_CODE
                            left JOIN DIST_SALESPERSON_MASTER X ON X.SP_CODE = A.SP_CODE
                            where 1=1 and a.USERID <> 1 and a.COMPANY_CODE ='" + userInfo.company_code + "' ORDER BY TRIM(USER_NAME) ASC";
            var data = _objectEntity.SqlQuery<UserSetupModel>(query).ToList();
            return data;

            //string query = "select a.USERID , a.USER_NAME,a.PASS_WORD, a.USER_TYPE,a.SP_CODE,a.EXPIRY_DATE,a.IS_MOBILE,a.ACTIVE ,d.ROLE_CODE,m.ROLE_NAME FROM DIST_LOGIN_USER a, dist_role_user d,dist_role_master_setup m WHERE d.userid = a.userid and m.ROLE_CODE = d.ROLE_CODE";

        }
        public List<EmployeeModel> getEmployeeList()
        {

            var query = @"
            SELECT EMPLOYEE_CODE ,  EMPLOYEE_EDESC 
                                FROM  hr_employee_setup
                                       WHERE deleted_flag = 'N'";
            var data = _objectEntity.SqlQuery<EmployeeModel>(query).ToList();
            return data;
        }
        public List<EmployeeModel> getUserEmployee(User userEmployee)
        {
            var query = $@"SELECT distinct M.EMPLOYEE_CODE ,  M.EMPLOYEE_EDESC 
                                FROM  hr_employee_setup M, SC_APPLICATION_USERS N
                                       WHERE M.deleted_flag = 'N' AND N.DELETED_FLAG='N' and M.COMPANY_CODE = '{userEmployee.company_code}' AND M.EMPLOYEE_CODE = N.EMPLOYEE_CODE and  N.EMPLOYEE_CODE IS NOT NULL AND 
                                       M.COMPANY_CODE = N.COMPANY_CODE";
            var data = _objectEntity.SqlQuery<EmployeeModel>(query).ToList();
            return data;
        }
        public List<EmployeeModel> GetSalesPersonList(User userInfo)
        {
            var data = _objectEntity.SqlQuery<EmployeeModel>($"SELECT SP_CODE EMPLOYEE_CODE,FULL_NAME EMPLOYEE_EDESC FROM DIST_LOGIN_USER WHERE USER_TYPE = 'S' AND COMPANY_CODE = '{userInfo.company_code}'").ToList();
            return data;
        }

        public List<DistributorModel> getDistributorList(User userInfo)
        {


            var query = $@"SELECT A.DISTRIBUTOR_CODE,A.REG_OFFICE_ADDRESS,A.CONTACT_NO,
                           A.EMAIL,A.PAN_NO,A.VAT_NO,A.CREATED_BY,A.LUPDATE_BY,
                           A.CREATED_DATE,A.LUPDATE_DATE,A.ACTIVE,(B.CUSTOMER_EDESC)
                        FROM DIST_DISTRIBUTOR_MASTER A,
                        SA_CUSTOMER_SETUP B
                        WHERE A.DISTRIBUTOR_CODE=B.CUSTOMER_CODE
                        AND A.COMPANY_CODE = B.COMPANY_CODE
                        AND A.ACTIVE = 'Y'
                        AND A.COMPANY_CODE = '{userInfo.company_code}'
                        ORDER BY TRIM(B.CUSTOMER_EDESC)";


            var data = _objectEntity.SqlQuery<DistributorModel>(query).ToList();
            return data;
        }

        public List<NewPartyTypeModel> GetPartyType()
        {
            string query = @" SELECT PARTY_TYPE_CODE,PARTY_TYPE_EDESC from IP_PARTY_TYPE_CODE";
            var data = _objectEntity.SqlQuery<NewPartyTypeModel>(query).ToList();
            return data;
        }



        public List<NewPartyTypeModel> GetPartyType(User userInfo)
        {
            string query = $@" SELECT PARTY_TYPE_CODE,PARTY_TYPE_EDESC from IP_PARTY_TYPE_CODE
                                WHERE COMPANY_CODE = '{userInfo.company_code}'
                                AND DELETED_FLAG = 'N' AND PARTY_TYPE_FLAG='D' AND GROUP_SKU_FLAG='I'
                                ORDER BY TRIM(PARTY_TYPE_EDESC) ASC";
            var data = _objectEntity.SqlQuery<NewPartyTypeModel>(query).ToList();
            return data;
        }

        public List<ResellerModel> getResellerList(User userInfo)
        {
            string query = $@"SELECT RESELLER_CODE,RESELLER_NAME,REG_OFFICE_ADDRESS,CONTACT_NO,EMAIL,PAN_NO,
                            VAT_NO,CREATED_BY,CREATED_DATE,ACTIVE
                            FROM DIST_RESELLER_MASTER WHERE COMPANY_CODE = '{userInfo.company_code}'
                                 AND ACTIVE='Y' AND IS_CLOSED = 'N'
                ORDER BY RESELLER_NAME DESC";
            var data = _objectEntity.SqlQuery<ResellerModel>(query).ToList();
            return data;
        }

        public List<SalesPersonModel> getSalesPersonList(User userInfo)
        {
            var filter = "";
            if (!string.IsNullOrWhiteSpace(userInfo.sp_codes))
                filter = $" AND DLU.SP_CODE IN ({userInfo.sp_codes})";

            string query = $@"SELECT DLU.SP_CODE,HES.EMPLOYEE_EDESC,DLU.GROUPID,WM_CONCAT(DISTINCT DUA.AREA_CODE) AREA_CODE
                FROM DIST_LOGIN_USER DLU
                INNER JOIN DIST_USER_AREAS DUA ON DLU.SP_CODE = DUA.SP_CODE AND DLU.COMPANY_CODE = DUA.COMPANY_CODE
                INNER JOIN HR_EMPLOYEE_SETUP HES ON DLU.SP_CODE = HES.EMPLOYEE_CODE AND DLU.COMPANY_CODE = HES.COMPANY_CODE
                WHERE DLU.COMPANY_CODE = '{userInfo.company_code}'
                            AND DLU.ACTIVE = 'Y'
                            AND DLU.USER_TYPE = 'S' {filter}
                            AND HES.DELETED_FLAG = 'N'
                GROUP BY DLU.SP_CODE,HES.EMPLOYEE_EDESC,DLU.GROUPID";
            var data = _objectEntity.SqlQuery<SalesPersonModel>(query).ToList();
            return data;
        }
        public List<ItemModel> GetItemAndBrand(User userInfo)
        {
            try
            {
                var spCode = "";
                if (!string.IsNullOrWhiteSpace(userInfo.sp_codes))
                    spCode = $" AND DIM.SP_CODE IN ({userInfo.sp_codes})";

                string query = $@" SELECT DISTINCT DIM.SP_CODE, IMS.ITEM_CODE, IMS.ITEM_EDESC, ISS.BRAND_NAME, IMS.INDEX_MU_CODE AS UNIT
                    FROM IP_ITEM_MASTER_SETUP IMS,DIST_USER_ITEM_MAPPING DIM,IP_ITEM_SPEC_SETUP ISS
                     WHERE IMS.ITEM_CODE = DIM.ITEM_CODE
                       AND DIM.COMPANY_CODE=IMS.COMPANY_CODE
                     AND ISS.ITEM_CODE = IMS.ITEM_CODE(+) AND ISS.COMPANY_CODE = IMS.COMPANY_CODE
                    AND IMS.COMPANY_CODE ='{userInfo.company_code}' AND IMS.DELETED_FLAG = 'N' AND IMS.GROUP_SKU_FLAG='I' {spCode}";

                var data = _objectEntity.SqlQuery<ItemModel>(query).ToList();
                return data;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
        public string insertSalesPerson(UserSetupModel modal, User userinfo)
        {
            // string TEST = Convert.ToDecimal(modal.SUPERVISOR_CODE).ToString("0000000");
            //  if(modal.OLD_SP_CODE!=null)
            ////   {
            //       string Query = $@"UPDATE DIST_SALESPERSON_MASTER SET SP_CODE='{modal.SP_CODE}',SUPERVISOR_CODE='{modal.SUPERVISOR_CODE}', CREATED_DATE=TO_DATE('{DateTime.Now.ToShortDateString()}','MM/DD/YYYY'),COMPANY_CODE='{userinfo.company_code}',BRANCH_CODE='{userinfo.branch_code}' WHERE SP_CODE={modal.OLD_SP_CODE}";
            //       var count = _objectEntity.ExecuteSqlCommand(Query);
            // }

            string query = $@"select SP_CODE from DIST_SALESPERSON_MASTER where SP_CODE ='{modal.SP_CODE}'";

            var data = _objectEntity.SqlQuery<UserSetupModel>(query).ToList();

            if (data.Count > 0)
            {
                var Query = $@"UPDATE DIST_SALESPERSON_MASTER SET SP_CODE='{modal.SP_CODE}',SUPERVISOR_CODE='{modal.SUPERVISOR_CODE}', CREATED_DATE=TO_DATE('{DateTime.Now.ToShortDateString()}','MM/DD/YYYY'),COMPANY_CODE='{userinfo.company_code}',BRANCH_CODE='{userinfo.branch_code}' WHERE SP_CODE={modal.SP_CODE}";
                _objectEntity.ExecuteSqlCommand(Query);

            }
            else
            {
                var insertQuery = $@"Insert into DIST_SALESPERSON_MASTER
                           (SP_CODE, SUPERVISOR_CODE, CREATED_DATE, COMPANY_CODE,BRANCH_CODE)
                         Values
                           ('{modal.SP_CODE}', '{modal.SUPERVISOR_CODE}', SYSDATE, '{userinfo.company_code}','{userinfo.branch_code}')";
                _objectEntity.ExecuteSqlCommand(insertQuery);
            }






            //else
            //  {
            //      var insertQuery = $@"Insert into DIST_SALESPERSON_MASTER
            //                 (SP_CODE, SUPERVISOR_CODE, CREATED_DATE, COMPANY_CODE,BRANCH_CODE)
            //               Values
            //                 ('{modal.SP_CODE}', '{modal.SUPERVISOR_CODE}', SYSDATE, '{userinfo.company_code}','{userinfo.branch_code}')";
            //      _objectEntity.ExecuteSqlCommand(insertQuery);
            //  }
            //var insertquey = string.Format(@"INSERT INTO DIST_SALESPERSON_MASTER(SP_CODE,SUPERVISOR_CODE,CREATED_DATE,ACTIVE ,COMPANY_CODE,BRANCH_CODE)
            //    VALUES({0}','{1}','{2}','{3}','{4}','{5}')", modal.SP_CODE, modal.SUPERVISOR_CODE, modal.CREATED_DATE.ToString("01/02/2017"), modal.ACTIVE,01,01.01);

            _objectEntity.SaveChanges();
            return "saved";

        }

        public string insertUserDetails(UserSetupModel modal, User userInfo)
        {

            try
            {
                var spCode = string.Empty;
                if (modal.USER_TYPE == "L")
                {
                    //string getCustomerCodeByPartyTypePanNumber = $@"SELECT SA.CUSTOMER_CODE
                    //                                                FROM IP_PARTY_TYPE_CODE IP
                    //                                                 ,SA_CUSTOMER_SETUP SA
                    //                                                WHERE IP.PARTY_TYPE_CODE = SA.PARTY_TYPE_CODE
                    //                                                 AND SA.TPIN_VAT_NO = IP.PAN_NO
                    //                                                    AND SA.COMPANY_CODE=IP.COMPANY_CODE
                    //                                                    AND SA.DELETED_FLAG='N'
                    //                                                 AND IP.PARTY_TYPE_CODE ='{modal.SP_CODE}'";
                    //spCode = _objectEntity.SqlQuery<string>(getCustomerCodeByPartyTypePanNumber).FirstOrDefault();

                    string getCustomerCodeByPartyTypePanNumber = $@"SELECT SA.CUSTOMER_CODE
                                                                    FROM IP_PARTY_TYPE_CODE IP
	                                                                    ,SA_CUSTOMER_SETUP SA
                                                                        ,FA_SUB_LEDGER_DEALER_MAP DM
                                                                    WHERE DM.PARTY_TYPE_CODE = IP.PARTY_TYPE_CODE
                                                                        AND DM.CUSTOMER_CODE = SA.CUSTOMER_CODE
	                                                                    AND SA.TPIN_VAT_NO = IP.PAN_NO
                                                                        AND SA.COMPANY_CODE=IP.COMPANY_CODE
                                                                        AND SA.DELETED_FLAG='N'
	                                                                    AND IP.PARTY_TYPE_CODE ='{modal.SP_CODE}'";
                    spCode = _objectEntity.SqlQuery<string>(getCustomerCodeByPartyTypePanNumber).FirstOrDefault();
                    if (string.IsNullOrEmpty(spCode))
                    {
                        return "NotMapped";
                    }

                }
                else
                {
                    spCode = modal.SP_CODE;
                }
                if (modal.USERID == 0)
                {

                    //string insertquery = @"insert into DIST_LOGIN_USER(USERID,USER_NAME,USER_TYPE,SP_CODE,EXPIRY_DATE,IS_MOBILE) values("SQ_USERID.nextval",'" + modal.USER_NAME + "','" + modal.USER_TYPE + "','" + modal.SP_CODE + "','"+ modal.EXPIRY_DATE+"','"+modal.IS_MOBILE+"')";

                    var insertquey = string.Format(@"INSERT INTO DIST_LOGIN_USER(USERID,USER_NAME,PASS_WORD,USER_TYPE ,SP_CODE,EXPIRY_DATE,IS_MOBILE,ATTENDANCE,COMPANY_CODE,BRANCH_CODE,CREATED_BY,CREATED_DATE,GROUPID)
                VALUES({0}, '{1}', '{2}','{3}','{4}', TO_DATE('{5}', 'MM/dd/yyyy'),'{6}','{7}','{8}','{9}','{10}',TO_DATE(SYSDATE),'{11}')",
          "SEQ_LOGIN_USER.nextval", modal.USER_NAME, modal.PASS_WORD, modal.USER_TYPE, spCode, modal.EXPIRY_DATE.ToString("MM/dd/yyyy"), modal.IS_MOBILE, modal.ATTENDANCE, userInfo.company_code, userInfo.branch_code, userInfo.login_code, modal.GROUPID);
                    var result = _objectEntity.ExecuteSqlCommand(insertquey);
                    if (modal.USER_TYPE == "S")
                    {
                        this.insertSalesPerson(modal, userInfo);
                    }

                    return "Insert Successfully";
                }
                else
                {
                    string updatequery = string.Format(@"UPDATE DIST_LOGIN_USER SET USER_NAME  = '{0}',PASS_WORD= '{1}', USER_TYPE='{2}',SP_CODE = '{3}',EXPIRY_DATE = TO_DATE('{4}', 'mm/dd/yyyy'),IS_MOBILE=('{5}'),ACTIVE=('{6}'),ATTENDANCE=('{7}'),GROUPID=('{8}') WHERE USERID IN ({9})",
          modal.USER_NAME, modal.PASS_WORD, modal.USER_TYPE, spCode, modal.EXPIRY_DATE.ToString("MM/dd/yyyy"), modal.IS_MOBILE, modal.ACTIVE, modal.ATTENDANCE, modal.GROUPID, modal.USERID);
                    var rowCount = _objectEntity.ExecuteSqlCommand(updatequery);
                    if (modal.USER_TYPE == "S")
                    {
                        this.insertSalesPerson(modal, userInfo);
                    }
                    return "Update Successfully";

                }

            }
            catch (Exception e)
            {
                return "Data Already exist";
            }
        }

        //public string UpdateUserAssign(UserSetupModel modal)
        //{
        //    try
        //    {

        //        string query = string.Format(@"SELECT * FROM DIST_ROLE_USER WHERE USERID='" + modal.USERID + "'");
        //        var result = _objectEntity.ExecuteSqlCommand(query);
        //        return result.ToString();
        //    }
        //   catch(Exception e)
        //    {
        //        throw;
        //    }
        //}
        public List<UserSetupModel> UpdateUserAssign(UserSetupModel modal, User userInfo)
        {
            string Query = string.Empty;
            try
            {
                Query = $@"UPDATE DIST_ROLE_USER SET ROLE_CODE='{modal.ROLE_CODE}' WHERE USERID='{modal.USERID}'";
                var count = _objectEntity.ExecuteSqlCommand(Query);
                if (count == 0)
                {
                    Query = $@"Insert into DIST_ROLE_USER
                           (USERID, ROLE_CODE, CREATED_BY, CREATED_DATE, ACTIVE,COMPANY_CODE)
                         Values
                           ({modal.USERID}, {modal.ROLE_CODE}, {modal.USERID}, SYSDATE, 'Y','{userInfo.company_code}')";
                    _objectEntity.ExecuteSqlCommand(Query);
                }
                _objectEntity.SaveChanges();
            }
            catch (Exception)
            {
                Query = $@"Insert into DIST_ROLE_USER
                           (USERID, ROLE_CODE, CREATED_BY, CREATED_DATE, ACTIVE)
                         Values
                           ({modal.USERID}, {modal.ROLE_CODE}, {modal.USERID}, SYSDATE, 'Y')";
                _objectEntity.ExecuteSqlCommand(Query);
                _objectEntity.SaveChanges();

            }
            return new List<UserSetupModel>();

        }

        public List<UserSetupModel> GetAllAssignList(UserSetupModel modal)
        {
            string query = @"SELECT d.USERID,d.ROLE_CODE,m.ROLE_NAME FROM dist_role_user d, dist_role_master_setup m where m.ROLE_CODE=d.ROLE_CODE and d.USERID='1'";
            var data = _objectEntity.SqlQuery<UserSetupModel>(query).ToList();
            return data;

        }

        public string UpdatePreferenceSetup(PreferenceSetupModel modal, User userInfo)
        {


            //var companyCode = string.Join(",", modal.CompanyFilter);
            //  companyCode = companyCode == "" ? userInfo.company_code : companyCode;

            string updateQuery = string.Format(@"UPDATE DIST_PREFERENCE_SETUP SET MO_GPS='{0}',MO_AGPS='{1}', PO_PARTY_TYPE  = '{2}',PO_BILLING_NAME= '{3}',
                PO_SYN_RATE='{4}',PO_CUSTOM_RATE = '{5}',PO_REMARKS='{6}',PO_CONVERSION_UNIT='{7}',PO_CONVERSION_FACTOR='{8}',
                CS_CONVERSION_UNIT='{9}',QA_MKT_INFO='{10}',QA_COMPT_INFO='{11}',BRANCH_CODE='{12}',SO_CREDIT_LIMIT_CHK='{13}',
                PO_COMPANY_LIST='{14}',SQL_NN_CONVERSION_UNIT_FACTOR='{15}',PO_SHIPPING_ADDRESS='{16}',PO_SHIPPING_CONTACT='{17}',
                PO_SALES_TYPE='{18}',SO_CREDIT_DAYS_CHK='{19}',SO_CONSOLIDATE_DEFAULT='{20}',ATN_DEFAULT='{21}' ,ATN_IMAGE='{22}',
                SET_RES_MAP_WHOLESALER='{23}',PO_DISPLAY_DIST_ITEM='{24}',SQL_MULTIPLE_COMPANY='{25}',SQL_COMPANY_ENTITY='{26}',
                LO_BG_TRACK='{27}',PO_RATE_TABLE='{28}',PO_RATE_COLUMN='{29}',PO_DIST_RATE_COLUMN='{30}',
                LO_BG_TIME='{31}',SQL_PEV_DAYS='{32}',SQL_FOL_DAYS='{33}',MO_DISABLE_PLAYSTORE='{34}',MO_SAVE_DATA='{35}',SQL_GROUP_MAP='{36}',SQL_SP_FILTER='{37}',SQL_OPEN_ADDOUTLET='{38}',TRACK_ACTUAL_LOCATION='{39}',DISABLE_LOCATION='{40}',
                SO_REPO_RATE_TABLE='{41}',SO_REPO_RATE_COLUMN='{42}',PO_PRIORITY='{43}',SO_SYN_DISCOUNT='{44}',SO_CUSTOM_DISCOUNT='{45}',SO_RESELLER_DISCOUNT='{46}',SO_SALES_ORDER='{47}'
                WHERE COMPANY_CODE IN({48})"
                , modal.MO_GPS, modal.MO_AGPS, modal.PO_PARTY_TYPE, modal.PO_BILLING_NAME, modal.PO_SYN_RATE, modal.PO_CUSTOM_RATE,
                modal.PO_REMARKS, modal.PO_CONVERSION_UNIT, modal.PO_CONVERSION_FACTOR, modal.CS_CONVERSION_UNIT, modal.QA_MKT_INFO,
                modal.QA_COMPT_INFO, userInfo.branch_code, modal.SO_CREDIT_LIMIT_CHK, modal.PO_COMPANY_LIST, modal.SQL_NN_CONVERSION_UNIT_FACTOR,
                modal.PO_SHIPPING_ADDRESS, modal.PO_SHIPPING_CONTACT, modal.PO_SALES_TYPE, modal.SO_CREDIT_DAYS_CHK, modal.SO_CONSOLIDATE_DEFAULT,
                modal.ATN_DEFAULT, modal.ATN_IMAGE, modal.SET_RES_MAP_WHOLESALER, modal.PO_DISPLAY_DIST_ITEM, modal.SQL_MULTIPLE_COMPANY,
                modal.SQL_COMPANY_ENTITY, modal.LO_BG_TRACK, modal.PO_RATE_TABLE, modal.PO_RATE_COLUMN, modal.PO_DIST_RATE_COLUMN,
                modal.LO_BG_TIME, modal.SQL_PEV_DAYS, modal.SQL_FOL_DAYS, modal.MO_DISABLE_PLAYSTORE, modal.MO_SAVE_DATA, modal.SQL_GROUP_MAP, modal.SQL_SP_FILTER, modal.SQL_OPEN_ADDOUTLET, modal.TRACK_ACTUAL_LOCATION, modal.DISABLE_LOCATION, modal.SO_REPO_RATE_TABLE, modal.SO_REPO_RATE_COLUMN,modal.PO_PRIORITY, modal.SO_SYN_DISCOUNT,modal.SO_CUSTOM_DISCOUNT,modal.SO_RESELLER_DISCOUNT,modal.SO_SALES_ORDER, userInfo.company_code);
            var count = _objectEntity.ExecuteSqlCommand(updateQuery);
            // return data.ToString();
            if (count == 0)
            {



                var insertQuery = $@"Insert into DIST_PREFERENCE_SETUP
                           (MO_GPS,MO_AGPS,PO_PARTY_TYPE ,PO_BILLING_NAME ,PO_SYN_RATE ,PO_CUSTOM_RATE ,PO_REMARKS, PO_CONVERSION_UNIT ,PO_CONVERSION_FACTOR ,CS_CONVERSION_UNIT,QA_MKT_INFO,QA_COMPT_INFO,COMPANY_CODE ,BRANCH_CODE,SO_CREDIT_LIMIT_CHK,SQL_NN_CONVERSION_UNIT_FACTOR,PO_COMPANY_LIST,PO_SHIPPING_ADDRESS,PO_SHIPPING_CONTACT,PO_SALES_TYPE,SO_CREDIT_DAYS_CHK,SO_CONSOLIDATE_DEFAULT,SET_RES_MAP_WHOLESALER,PO_DISPLAY_DIST_ITEM,SQL_MULTIPLE_COMPANY,SQL_COMPANY_ENTITY,SQL_GROUP_MAP,SQL_SP_FILTER,SQL_OPEN_ADDOUTLET,TRACK_ACTUAL_LOCATION,LO_BG_TRACK,DISABLE_LOCATION,PO_PRIORITY,SO_SYN_DISCOUNT,SO_CUSTOM_DISCOUNT,SO_RESELLER_DISCOUNT,SO_SALES_ORDER)
                         Values
                           ('{modal.MO_GPS}','{modal.MO_AGPS}','{modal.PO_PARTY_TYPE}', '{modal.PO_BILLING_NAME}', '{modal.PO_SYN_RATE}', '{modal.PO_CUSTOM_RATE}', '{modal.PO_REMARKS}', '{modal.CS_CONVERSION_UNIT}', '{modal.PO_CONVERSION_FACTOR}', '{modal.CS_CONVERSION_UNIT}','{modal.QA_MKT_INFO}','{modal.QA_COMPT_INFO}', '{userInfo.company_code}', '{userInfo.branch_code}','{modal.SO_CREDIT_LIMIT_CHK}','{modal.SQL_NN_CONVERSION_UNIT_FACTOR}','{modal.PO_COMPANY_LIST}','{modal.PO_SHIPPING_ADDRESS}','{modal.PO_SHIPPING_CONTACT}','{modal.PO_SALES_TYPE}','{modal.SO_CREDIT_DAYS_CHK}','{modal.SO_CONSOLIDATE_DEFAULT}','{modal.SET_RES_MAP_WHOLESALER}','{modal.PO_DISPLAY_DIST_ITEM}','{modal.SQL_MULTIPLE_COMPANY}','{modal.SQL_COMPANY_ENTITY}','{modal.SQL_GROUP_MAP}','{modal.SQL_SP_FILTER}','{modal.SQL_OPEN_ADDOUTLET}','{modal.TRACK_ACTUAL_LOCATION}','{modal.LO_BG_TRACK}','{modal.DISABLE_LOCATION}','{modal.PO_PRIORITY}','{modal.SO_SYN_DISCOUNT}','{modal.SO_CUSTOM_DISCOUNT}','{modal.SO_RESELLER_DISCOUNT}','{modal.SO_SALES_ORDER}')";
                _objectEntity.ExecuteSqlCommand(insertQuery);
                _objectEntity.SaveChanges();
            }

            return count.ToString();
        }

        public PreferenceSetupModel GetPreferenceSetup(User userInfo)
        {
            PreferenceSetupModel psm = new Model.PreferenceSetupModel();
            string Query = @"SELECT MO_GPS,MO_AGPS,PO_COMPANY_LIST,PO_PARTY_TYPE,PO_BILLING_NAME,PO_SYN_RATE,PO_CUSTOM_RATE,
                            PO_REMARKS,PO_CONVERSION_UNIT,PO_CONVERSION_FACTOR,SO_CREDIT_LIMIT_CHK,SO_CONSOLIDATE_DEFAULT,
                            SO_CREDIT_DAYS_CHK,CS_CONVERSION_UNIT,QA_MKT_INFO,QA_COMPT_INFO,BRANCH_CODE,SQL_NN_CONVERSION_UNIT_FACTOR,
                            PO_SHIPPING_ADDRESS,PO_SHIPPING_CONTACT,PO_SALES_TYPE,ATN_DEFAULT,ATN_IMAGE,SET_RES_MAP_WHOLESALER,
                            PO_DISPLAY_DIST_ITEM,SQL_MULTIPLE_COMPANY,SQL_COMPANY_ENTITY,LO_BG_TRACK,PO_RATE_TABLE,PO_RATE_COLUMN,
                            PO_DIST_RATE_COLUMN,LO_BG_TIME,SQL_PEV_DAYS,SQL_FOL_DAYS,MO_DISABLE_PLAYSTORE,MO_SAVE_DATA,SQL_GROUP_MAP,SQL_SP_FILTER,SQL_OPEN_ADDOUTLET,TRACK_ACTUAL_LOCATION,SO_REPO_RATE_TABLE,SO_REPO_RATE_COLUMN,PO_PRIORITY,SO_SYN_DISCOUNT,SO_CUSTOM_DISCOUNT,SO_SALES_ORDER,SO_RESELLER_DISCOUNT
                            --,DISABLE_LOCATION 
                            FROM DIST_PREFERENCE_SETUP WHERE COMPANY_CODE =" + userInfo.company_code + "";
            var data = _objectEntity.SqlQuery<PreferenceSetupModel>(Query).FirstOrDefault();
            return data;
        }

        public List<MobileRegModel> GetAllDevices(User userInfo)
        {
            string query = string.Empty;
            query = $@"SELECT DLD.USERID,DLU.USER_NAME,DLD.IMEI_NO,DLD.DEVICE_NAME,DLD.APPROVED_FLAG,
                                DLD.ACTIVE,DLU.SP_CODE,DLU.FULL_NAME,DLD.APP_VERSION,CURRENT_LOGIN
                    FROM DIST_LOGIN_DEVICE DLD
                    JOIN DIST_LOGIN_USER DLU ON DLU.USERID=DLD.USERID
                    WHERE DLU.COMPANY_CODE='{userInfo.company_code}'";
            var data = _objectEntity.SqlQuery<MobileRegModel>(query).ToList();
            return data;
        }

        public string UpdateDevice(MobileRegModel model, User userInfo)
        {
            var upQuery = $@"UPDATE DIST_LOGIN_DEVICE SET 
                            APPROVED_FLAG='{model.APPROVED_FLAG}',ACTIVE='{model.ACTIVE}', UPDATED_BY='{userInfo.USER_NO}',
                            UPDATED_DATE=SYSDATE WHERE IMEI_NO='{model.IMEI_NO}'";
            var row = _objectEntity.ExecuteSqlCommand(upQuery);
            return "500";
        }

        public string DeleteDevice(MobileRegModel model)
        {
            var query = $"DELETE FROM DIST_LOGIN_DEVICE WHERE IMEI_NO='{model.IMEI_NO}'";
            var row = _objectEntity.ExecuteSqlCommand(query);
            return "500";
        }

        public string GetSPCodeByCustomerCode(string SP_CODE, string USER_TYPE, int USERID, User userInfo)
        {
            //string getCustomerCodeByPartyTypePanNumber = $@"SELECT IP.PARTY_TYPE_CODE
            //                                                        FROM IP_PARTY_TYPE_CODE IP
            //                                                         ,SA_CUSTOMER_SETUP SA
            //                                                        WHERE IP.PARTY_TYPE_CODE = SA.PARTY_TYPE_CODE
            //                                                         AND SA.TPIN_VAT_NO = IP.PAN_NO
            //                                                            AND SA.COMPANY_CODE=IP.COMPANY_CODE
            //                                                            AND SA.DELETED_FLAG='N'
            //                                                         --AND IP.PARTY_TYPE_CODE ='{SP_CODE}'
            //                                                            AND SA.CUSTOMER_CODE='{SP_CODE}'";
            string getCustomerCodeByPartyTypePanNumber = $@"SELECT IP.PARTY_TYPE_CODE
                                                                    FROM fa_sub_ledger_dealer_map IP
                                                                    WHERE IP.CUSTOMER_CODE='{SP_CODE}'
                                                                    AND IP.COMPANY_CODE='{userInfo.company_code}'
                                                                    AND IP.DELETED_FLAG='N'";
            var spCode = _objectEntity.SqlQuery<string>(getCustomerCodeByPartyTypePanNumber).FirstOrDefault();
            return spCode;

        }
    }
}
