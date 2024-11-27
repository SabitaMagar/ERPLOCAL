using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NeoErp.Core.Domain;
using NeoErp.Core.Models;
using NeoErp.Distribution.Service.Model;
using NeoErp.Core;
using System.Web;
using System.Xml.Linq;
using System.Globalization;

namespace NeoErp.Distribution.Service.Service
{
    public class SetupService : ISetupService
    {
        private NeoErpCoreEntity _objectEntity;
        public SetupService(NeoErpCoreEntity objectEntity)
        {
            this._objectEntity = objectEntity;
        }


        public IEnumerable<DistributorListModel> GetDistributorList(ReportFiltersModel model, User userInfo)
        {
            var companyCode = string.Join(",", model.CompanyFilter);
            companyCode = companyCode == "" ? userInfo.company_code : companyCode;

            var Query = $@"  SELECT A.DISTRIBUTOR_CODE,
                                 RGM.GROUPID,
                                 RGM.GROUP_EDESC,
                                 A.ACTIVE,
                                 B.CUSTOMER_EDESC DISTRIBUTOR_NAME,
                                 B.REGD_OFFICE_EADDRESS AS ADDRESS,
                                 A.LATITUDE,
                                 A.LONGITUDE,
                                 B.PAN_NO,
                                 A.CREATED_BY,
                                 A.CREATED_DATE,
                                 A.ACTIVE ISACTIVE,
                                 AM.AREA_NAME,
                                  A.WEIGHT,
                                 wm_concat(DI.ITEM_CODE) ItemCodeString         
                            FROM DIST_DISTRIBUTOR_MASTER A,
                                 SA_CUSTOMER_SETUP B,
                                 DIST_AREA_MASTER am,
                                 DIST_GROUP_MASTER RGM,
                                 DIST_DISTRIBUTOR_ITEM DI
                           WHERE     A.DISTRIBUTOR_CODE = B.CUSTOMER_CODE
                                 AND A.COMPANY_CODE = B.COMPANY_CODE
                                 AND A.COMPANY_CODE = DI.COMPANY_CODE(+)
                                 AND A.DISTRIBUTOR_CODE = DI.DISTRIBUTOR_CODE(+)
                                 AND (A.COMPANY_CODE = '{companyCode}')
                                 AND B.DELETED_FLAG = 'N'
                                 AND A.DELETED_FLAG ='N'
                                 --AND A.ACTIVE = 'Y'
                                 AND A.GROUPID = RGM.GROUPID(+)
                                 AND A.COMPANY_CODE = RGM.COMPANY_CODE(+)
                                 AND AM.COMPANY_CODE(+) = A.COMPANY_CODE
                                 AND AM.AREA_CODE(+) = A.AREA_CODE
                             GROUP BY A.DISTRIBUTOR_CODE,RGM.GROUPID,
                                 RGM.GROUP_EDESC,
                                 A.WEIGHT,
                                 A.ACTIVE,
                                 B.CUSTOMER_EDESC ,
                                 B.REGD_OFFICE_EADDRESS,
                                 A.LATITUDE,
                                 A.LONGITUDE,
                                 B.PAN_NO,
                                 A.CREATED_BY,
                                 A.CREATED_DATE,
                                 A.ACTIVE,
                                 AM.AREA_NAME
                        ORDER BY RGM.GROUP_EDESC, A.WEIGHT ASC";

            Query = string.Format(Query, companyCode);
            var data = _objectEntity.SqlQuery<DistributorListModel>(Query).ToList();

            return data;
        }
        public IEnumerable<ResellerListModel> GetResellerList(string Source, ReportFiltersModel model, User userInfo, string status)
        {
            var SourceFilter = "";
            var statusQ = "";
            var condition = "";
            if (Source == "W")
                SourceFilter = " AND RM.SOURCE = 'WEB'";
            else if (Source == "M")
                SourceFilter = " AND RM.SOURCE = 'MOB'";
            var companyCode = string.Join(",", model.CompanyFilter);
            if (status == "inactive") statusQ = " AND RM.ACTIVE !='Y'";
            companyCode = companyCode == "" ? userInfo.company_code : companyCode;

            if (!string.IsNullOrEmpty(model.FromDate) && !string.IsNullOrEmpty(model.ToDate))
            {
                condition = $" AND TRUNC(RM.CREATED_DATE) BETWEEN  TO_CHAR(TO_DATE('{model.FromDate}', 'YYYY-MON-DD'), 'DD-MON-YYYY') AND TO_CHAR(TO_DATE('{model.ToDate}', 'YYYY-MON-DD'), 'DD-MON-YYYY')";
            }

            var Query = $@"SELECT RM.RESELLER_CODE,
              RM.RESELLER_NAME,
              RM.DISTRIBUTOR_CODE,
              RM.REG_OFFICE_ADDRESS AS ADDRESS,
              RM.EMAIL,
              (RM.CONTACT_SUFFIX || ' ' || RM.CONTACT_NAME || ' : ' || RM.CONTACT_NO) as PRIMARY_CONTACT, 
              RM.CONTACT_NAME Primary_Contact_Name,
              RM.CONTACT_NO Primary_Contact_No,
              RM.LATITUDE,
              RM.LONGITUDE,
              RM.PAN_NO,
              RM.VAT_NO,
              RM.RESELLER_CONTACT,
              RM.WHOLESELLER,
              CASE
                WHEN RM.ACTIVE = 'Y' THEN 'Active'
                ELSE 'Inactive'
              END ACTIVE,
              CASE
                WHEN RM.IS_CLOSED = 'Y' THEN 'Closed'
                ELSE 'Open'
              END ISCLOSED,
              RM.REMARKS,
              RM.CREATED_BY_NAME,
              RM.CREATED_DATE,
              AM.AREA_CODE,
              AM.AREA_NAME
            FROM DIST_RESELLER_MASTER RM
            LEFT JOIN DIST_AREA_MASTER AM ON AM.AREA_CODE = RM.AREA_CODE AND AM.COMPANY_CODE = RM.COMPANY_CODE
            WHERE RM.COMPANY_CODE = '{companyCode}'AND RM.DELETED_FLAG ='N' {SourceFilter} {statusQ} {condition} order by rm.created_date desc
            --ORDER BY UPPER(RM.RESELLER_NAME) ASC";
            //Query = string.Format(Query, companyCode, SourceFilter, statusQ);
            var data = _objectEntity.SqlQuery<ResellerListModel>(Query).ToList();
            return data;
        }
        public int InactiveResellers(User userInfo)
        {
            var data = _objectEntity.SqlQuery<int>($"SELECT COUNT(*) FROM DIST_RESELLER_MASTER WHERE ACTIVE = 'N' AND SOURCE='MOB' AND DELETED_FLAG = 'N' AND IS_CLOSED = 'N' AND COMPANY_CODE = '{userInfo.company_code}'").FirstOrDefault();
            return data;
        }

        public IEnumerable<OtherEntity> getAllEntityList(ReportFiltersModel model, User userInfo)
        {
            try
            {
                var entityQuery = @"SELECT BOE.CODE, DGM.GROUP_EDESC,DAM.AREA_NAME,BOE.CODE,BOE.DESCRIPTION,BOE.CONTACT_PERSON,BOE.AREA_CODE,BOE.GROUP_ID,BOE.CONTACT_NO,BOE.LATITUDE,BOE.LONGITUDE, BOE.TYPE
                                    FROM BRD_OTHER_ENTITY BOE
                                    INNER JOIN DIST_AREA_MASTER DAM ON DAM.COMPANY_CODE = BOE.COMPANY_CODE AND DAM.AREA_CODE = BOE.AREA_CODE
                                    INNER JOIN DIST_GROUP_MASTER DGM ON DGM.GROUPID= BOE.GROUP_ID AND DGM.COMPANY_CODE = BOE.COMPANY_CODE WHERE BOE.DELETED_FLAG ='N'";
                var result = _objectEntity.SqlQuery<OtherEntity>(entityQuery).ToList();
                return result;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public IEnumerable<RouteListModel> GetRouteList(ReportFiltersModel model, User userInfo)
        {
            var companyCode = string.Join(",", model.CompanyFilter);
            companyCode = companyCode == "" ? userInfo.company_code : companyCode;

            var Query = $@"SELECT ROUTE_CODE,ROUTE_NAME,(DIST_COUNT+RESELLER_COUNT+DEALER_COUNT) ENTITY_COUNT,DIST_COUNT,RESELLER_COUNT,DEALER_COUNT FROM
                (SELECT A.ROUTE_CODE,A.ROUTE_NAME,
                     (SELECT COUNT(*) FROM DIST_ROUTE_ENTITY RE,DIST_DISTRIBUTOR_MASTER DM WHERE RE.ENTITY_CODE = DM.DISTRIBUTOR_CODE
                                      AND RE.ROUTE_CODE = A.ROUTE_CODE AND RE.ENTITY_TYPE = 'D'
                                      AND DM.ACTIVE = 'Y' AND DM.DELETED_FLAG = 'N') DIST_COUNT,
                     (SELECT COUNT(*) FROM DIST_ROUTE_ENTITY RE,DIST_RESELLER_MASTER RM WHERE RE.ENTITY_CODE = RM.RESELLER_CODE
                                      AND RE.ROUTE_CODE = A.ROUTE_CODE AND RE.ENTITY_TYPE = 'R'
                                      AND RM.ACTIVE = 'Y' AND RM.IS_CLOSED = 'N') RESELLER_COUNT,
                     (SELECT COUNT(*) FROM DIST_ROUTE_ENTITY WHERE ROUTE_CODE = A.ROUTE_CODE AND ENTITY_TYPE = 'P') DEALER_COUNT
                FROM DIST_ROUTE_MASTER A WHERE A.COMPANY_CODE = '{companyCode}' AND DELETED_FLAG ='N' AND ROUTE_TYPE = 'D'
                ORDER BY TO_NUMBER(SUBSTR(A.ROUTE_CODE,2)))";


            Query = string.Format(Query, companyCode);
            var data = _objectEntity.SqlQuery<RouteListModel>(Query).ToList();
            return data;
        }

        public IEnumerable<RouteListModel> GetBrandingRouteList(ReportFiltersModel model, User userInfo)
        {
            var companyCode = string.Join(",", model.CompanyFilter);
            companyCode = companyCode == "" ? userInfo.company_code : companyCode;

            var Query = $@"SELECT ROUTE_CODE,ROUTE_NAME,(DIST_COUNT+RESELLER_COUNT+DEALER_COUNT) ENTITY_COUNT,DIST_COUNT,RESELLER_COUNT,DEALER_COUNT,HOARDING_COUNT FROM
                (SELECT A.ROUTE_CODE,A.ROUTE_NAME,
                     (SELECT COUNT(*) FROM DIST_ROUTE_ENTITY RE,DIST_DISTRIBUTOR_MASTER DM WHERE RE.ENTITY_CODE = DM.DISTRIBUTOR_CODE
                                      AND RE.ROUTE_CODE = A.ROUTE_CODE AND RE.ENTITY_TYPE = 'D'
                                      AND DM.ACTIVE = 'Y' AND DM.DELETED_FLAG = 'N') DIST_COUNT,
                     (SELECT COUNT(*) FROM DIST_ROUTE_ENTITY RE,DIST_RESELLER_MASTER RM WHERE RE.ENTITY_CODE = RM.RESELLER_CODE
                                      AND RE.ROUTE_CODE = A.ROUTE_CODE AND RE.ENTITY_TYPE = 'R'
                                      AND RM.ACTIVE = 'Y' AND RM.IS_CLOSED = 'N') RESELLER_COUNT,
                     (SELECT COUNT(*) FROM DIST_ROUTE_ENTITY WHERE ROUTE_CODE = A.ROUTE_CODE AND ENTITY_TYPE = 'P') DEALER_COUNT,
                       ( SELECT COUNT(*) FROM BRD_ROUTE_ENTITY RE,BRD_OTHER_ENTITY OE WHERE RE.ENTITY_CODE = OE.CODE
                                      AND RE.ROUTE_CODE = A.ROUTE_CODE AND RE.ENTITY_TYPE = 'H') HOARDING_COUNT
                     FROM DIST_ROUTE_MASTER A WHERE A.COMPANY_CODE = '{companyCode}' AND DELETED_FLAG ='N' AND ROUTE_TYPE = 'B'
                ORDER BY TO_NUMBER(SUBSTR(A.ROUTE_CODE,2)))";

            //Query = string.Format(Query, companyCode);
            var data = _objectEntity.SqlQuery<RouteListModel>(Query).ToList();
            return data;
        }

        public string AddDistributor(DistributorListModel model, User userInfo)
        {
            var message = "";
            using (var transaction = _objectEntity.Database.BeginTransaction())
            {
                try
                {
                    if (string.IsNullOrWhiteSpace(model.DISTRIBUTOR_CODE) && model.CUSTOMERFLAG == 'Y'.ToString())
                    {
                        model.DISTRIBUTOR_CODE = _objectEntity.SqlQuery<int>("select max(to_number(customer_code))+1 from sa_customer_setup").FirstOrDefault().ToString();
                        var customer_query = $@"insert into sa_customer_setup(COMPANY_CODE,BRANCH_CODE, CREATED_BY, CREATED_DATE, CUSTOMER_CODE, CUSTOMER_EDESC, GROUP_SKU_FLAG, MASTER_CUSTOMER_CODE, PRE_CUSTOMER_CODE, DELETED_FLAG, LINK_SUB_CODE, PARTY_TYPE_CODE) values('{userInfo.company_code}','{userInfo.branch_code}','{userInfo.login_code}',sysdate,'{model.DISTRIBUTOR_CODE}','{CultureInfo.CurrentCulture.TextInfo.ToTitleCase(model.DISTRIBUTOR_NAME.ToLower())}','I','01.00','01', 'N','{model.DISTRIBUTOR_CODE}','SDL')";

                        var distributor_query = $@"Insert into DIST_DISTRIBUTOR_MASTER (DISTRIBUTOR_CODE,LATITUDE,LONGITUDE,CREATED_BY,CREATED_DATE,COMPANY_CODE,BRANCH_CODE,AREA_CODE,ACTIVE,GROUPID)
                                Values ('{model.DISTRIBUTOR_CODE}','{model.LATITUDE}','{model.LONGITUDE}',1, sysdate,'{userInfo.company_code}','{userInfo.branch_code}','{model.AREA_Code}','{model.ACTIVE}','{model.GROUPID}')";

                        _objectEntity.ExecuteSqlCommand(distributor_query);
                        _objectEntity.ExecuteSqlCommand(customer_query);
                        AddDistItems(model, userInfo);
                        _objectEntity.SaveChanges();
                        transaction.Commit();
                        message = "220";
                    }
                    else if (string.IsNullOrWhiteSpace(model.DISTRIBUTOR_CODE) && model.CUSTOMERFLAG == 'N'.ToString())
                    {
                        message = "444";
                        return message;
                    }
                    else
                    {
                        var data = _objectEntity.SqlQuery<int>($"SELECT COUNT(DISTRIBUTOR_CODE) FROM DIST_DISTRIBUTOR_MASTER WHERE COMPANY_CODE='{userInfo.company_code}' and DISTRIBUTOR_CODE='{model.DISTRIBUTOR_CODE}'").FirstOrDefault();
                        if (data > 0)
                        {

                            message = "409";
                            return message;
                        }
                        var Query2 = $@"Insert into DIST_DISTRIBUTOR_MASTER (DISTRIBUTOR_CODE,LATITUDE,LONGITUDE,CREATED_BY,CREATED_DATE,COMPANY_CODE,BRANCH_CODE,AREA_CODE,ACTIVE,GROUPID)
                                Values ('{model.DISTRIBUTOR_CODE}','{model.LATITUDE}','{model.LONGITUDE}',1, sysdate,'{userInfo.company_code}','{userInfo.branch_code}','{model.AREA_Code}','{model.ACTIVE}','{model.GROUPID}')";

                        _objectEntity.ExecuteSqlCommand(Query2);

                        AddDistItems(model, userInfo);
                        _objectEntity.SaveChanges();
                        transaction.Commit();
                        message = "200";
                    }

                }
                catch (Exception e)
                {
                    transaction.Rollback();
                    message = "500";
                }
            }
            return message;

        }

        private string AddDistItems(DistributorListModel model, User userInfo)
        {
            var ItemCodes = model.ItemCode;
            string deleteQuery = $@"DELETE DIST_DISTRIBUTOR_ITEM WHERE DISTRIBUTOR_CODE='{model.DISTRIBUTOR_CODE}' AND COMPANY_CODE ='{userInfo.company_code}'";
            _objectEntity.ExecuteSqlCommand(deleteQuery);
            for (int i = 0; i < ItemCodes.Count(); i++)
            {
                var Icode = ItemCodes[i];
                string query = $@"INSERT INTO DIST_DISTRIBUTOR_ITEM (DISTRIBUTOR_CODE,ITEM_CODE,COMPANY_CODE,CREATED_BY,CREATED_DATE)VALUES('{model.DISTRIBUTOR_CODE}','{Icode}','{userInfo.company_code}','{userInfo.User_id}',SYSDATE)";
                _objectEntity.ExecuteSqlCommand(query);
            }

            return "success";
        }


        public string UpdateDistributorOrder(OrderModel model)
        {
            string Query = string.Empty;

            for (int i = 0; i < model.DISTRIBUTOR_LIST.Count(); i++)
            {
                string GROUPID = $" AND GROUPID={model.GROUPID}";
                if (model.GROUPID == "''")
                {
                    GROUPID = " AND GROUPID IS NULL";
                }
                Query = $@"UPDATE DIST_DISTRIBUTOR_MASTER 
                                     SET WEIGHT={i}
                              WHERE DISTRIBUTOR_CODE = '{model.DISTRIBUTOR_LIST[i]}' {GROUPID}";
                _objectEntity.ExecuteSqlCommand(Query);
            }



            //string Query = $@"UPDATE DIST_DISTRIBUTOR_MASTER 
            //                         SET WEIGHT={model.NEW_INDEX}, GROUPID={model.GROUPID}
            //                  WHERE DISTRIBUTOR_CODE = '{model.DISTRIBUTOR_CODE}'";
            //_objectEntity.ExecuteSqlCommand(Query);
            return "Success";
        }

        public string deleteDistributor(DistributorListModel model, User userInfo)
        {
            try
            {
                string deleteQuery = $@"UPDATE DIST_DISTRIBUTOR_MASTER SET DELETED_FLAG ='Y' WHERE DISTRIBUTOR_CODE ='{model.DISTRIBUTOR_CODE}' AND COMPANY_CODE ='{userInfo.company_code}'";
                _objectEntity.ExecuteSqlCommand(deleteQuery);
                _objectEntity.SaveChanges();
                return "success";
            }
            catch (Exception e)
            {
                return "error";
            }
        }

        public string UpdateDistributor(DistributorListModel model, User userInfo)
        {
            var message = "";
            using (var transaction = _objectEntity.Database.BeginTransaction())
            {
                try
                {

                    var Query1 = $@"Update DIST_DISTRIBUTOR_MASTER 
                                     SET LATITUDE='{model.LATITUDE}',LONGITUDE='{model.LONGITUDE}',LUPDATE_BY ='1',LUPDATE_DATE=sysdate,AREA_CODE='{model.AREA_Code}',ACTIVE='{model.ACTIVE}',GROUPID='{model.GROUPID}'
                                 WHERE DISTRIBUTOR_CODE='{model.DISTRIBUTOR_CODE}'";

                    _objectEntity.ExecuteSqlCommand(Query1);

                    //var Query2 = $@"Update DIST_DISTRIBUTOR_AREA 
                    //                SET AREA_CODE='{model.AREA}'                              
                    //             WHERE DISTRIBUTOR_CODE='{model.DISTRIBUTOR_CODE}'";

                    //_objectEntity.ExecuteSqlCommand(Query2);
                    AddDistItems(model, userInfo);
                    _objectEntity.SaveChanges();
                    transaction.Commit();
                    message = "200";
                }
                catch (Exception X)
                {
                    transaction.Rollback();
                    message = "500";
                }
            }
            return message;

        }

        public IEnumerable<DealerModal> GetDealerList(ReportFiltersModel model, User userInfo)
        {
            var companyCode = string.Join(",", model.CompanyFilter);
            companyCode = companyCode == "" ? userInfo.company_code : companyCode;
            var Query = @"SELECT A.DEALER_CODE,A.CONTACT_NO,A.EMAIL,A.PAN_NO,A.VAT_NO, B.PARTY_TYPE_EDESC DEALER_NAME,B.ACC_CODE,A.REG_OFFICE_ADDRESS as ADDRESS, A.LATITUDE,A.LONGITUDE,B.TEL_NO, A.CREATED_BY, A.CREATED_DATE, A.ACTIVE ISACTIVE, AM.AREA_NAME
                            FROM dist_dealer_master A,ip_party_type_code B,DIST_AREA_MASTER am
                            WHERE A.DEALER_CODE=B.PARTY_TYPE_CODE AND A.COMPANY_CODE = B.COMPANY_CODE
                            AND (A.COMPANY_CODE = '{0}')            
                            AND B.DELETED_FLAG = 'N'
                            AND A.DELETED_FLAG ='N'
                            and AM.COMPANY_CODE(+)=A.COMPANY_CODE
                            and AM.AREA_CODE(+)=A.AREA_CODE
                            ORDER BY UPPER(B.PARTY_TYPE_EDESC), A.DEALER_CODE ASC";


            Query = string.Format(Query, companyCode);
            var data = _objectEntity.SqlQuery<DealerModal>(Query).ToList();
            return data;
        }

        public string AddDealer(DealerModal model, User userInfo)
        {
            var message = "";
            using (var transaction = _objectEntity.Database.BeginTransaction())
            {
                try
                {

                    string Query = $"SELECT DEALER_CODE FROM DIST_DEALER_MASTER WHERE DEALER_CODE='{model.DEALER_CODE}'";
                    var data = _objectEntity.SqlQuery<DealerModal>(Query).ToList();
                    if (data.Count() > 0)
                    {
                        message = "Already Exist";
                        return message;
                    }
                    var Query1 = $@"Insert into DIST_DEALER_MASTER (DEALER_CODE,REG_OFFICE_ADDRESS,CONTACT_NO,EMAIL,PAN_NO,VAT_NO,AREA_Code,LATITUDE,LONGITUDE,CREATED_BY,CREATED_DATE,COMPANY_CODE,BRANCH_CODE)
                                Values ('{model.DEALER_CODE}','{model.ADDRESS}','{model.CONTACT_NO}','{model.EMAIL}','{model.PAN_NO}','{model.VAT_NO}','{model.AREA_Code}','{model.LATITUDE}','{model.LONGITUDE}',1, sysdate,'{userInfo.company_code}','{userInfo.branch_code}')";

                    _objectEntity.ExecuteSqlCommand(Query1);



                    _objectEntity.SaveChanges();
                    transaction.Commit();
                    message = "Success";
                }
                catch (Exception e)
                {
                    message = "Error";
                }
            }

            return message;
        }

        public string deleteDealer(DealerModal model, User userInfo)
        {
            try
            {
                string deleteQuery = $@"UPDATE DIST_DEALER_MASTER SET DELETED_FLAG ='Y' WHERE DEALER_CODE ='{model.DEALER_CODE}' AND COMPANY_CODE ='{userInfo.company_code}'";
                _objectEntity.ExecuteSqlCommand(deleteQuery);
                _objectEntity.SaveChanges();
                return "success";
            }
            catch (Exception e)
            {
                return "error";
            }
        }

        public string UpdateDealer(DealerModal model, User userInfo)
        {
            var message = "";
            using (var transaction = _objectEntity.Database.BeginTransaction())
            {
                try
                {
                    var Query1 = $@"Update DIST_DEALER_MASTER 
                                     SET LATITUDE='{model.LATITUDE}',AREA_Code='{model.AREA_Code}',REG_OFFICE_ADDRESS='{model.ADDRESS}',CONTACT_NO='{model.CONTACT_NO}',EMAIL='{model.EMAIL}',PAN_NO='{model.PAN_NO}',VAT_NO='{model.VAT_NO}',LONGITUDE='{model.LONGITUDE}',LUPDATE_BY ='1',LUPDATE_DATE=sysdate                               
                                 WHERE DEALER_CODE='{model.DEALER_CODE}'";

                    _objectEntity.ExecuteSqlCommand(Query1);
                    _objectEntity.SaveChanges();
                    transaction.Commit();
                    message = "200";
                }
                catch (Exception e)
                {
                    message = "500";
                    transaction.Rollback();
                }
            }

            return message;
        }


        public string AddReseller(ResellerListModel model, User userInfo)
        {
            var message = "";

            var companyCode = userInfo.company_code;
            using (var transaction = _objectEntity.Database.BeginTransaction())
            {
                try
                {

                    string Query = $"select * from dist_reseller_master where reseller_name = '{model.Reseller_NAME}' and pan_no = '{model.Pan_No}' and COMPANY_CODE='{companyCode}'";
                    var data = _objectEntity.SqlQuery<ResellerListModel>(Query).ToList();
                    if (data.Count() > 0)
                    {

                        message = "409"; //aleady exist
                        return message;
                    }

                    var reselerCode = "";
                    if (string.IsNullOrWhiteSpace(model.Reseller_CODE))
                    {
                        var rDate = DateTime.Now.ToString("yMMd");
                        var rTime = DateTime.Now.ToString("Hms");
                        reselerCode = "R-" + userInfo.User_id + '-' + rDate + '-' + rTime;
                    }
                    else
                    {
                        reselerCode = model.Reseller_CODE;
                    }

                    var Query1 = $@"INSERT INTO DIST_RESELLER_MASTER (RESELLER_CODE,RESELLER_NAME,AREA_CODE,REG_OFFICE_ADDRESS,CONTACT_NAME,CONTACT_NO,EMAIL,LATITUDE,LONGITUDE,VAT_NO,PAN_NO,CREATED_BY,CREATED_DATE,WHOLESELLER,REMARKS,COMPANY_CODE,BRANCH_CODE,OUTLET_TYPE_ID,OUTLET_SUBTYPE_ID,RESELLER_CONTACT,GROUPID)
                                Values ('{reselerCode}','{model.Reseller_NAME.Replace("'", "''")}','{model.AREA_Code}', '{model.ADDRESS}','{model.Primary_Contact_Name}','{model.Primary_Contact_No}','{model.Email}','{model.LATITUDE}','{model.LONGITUDE}','{model.Vat_No}','{model.Pan_No}','{userInfo.User_id}',SYSDATE,
                                '{model.WholeSeller}','{model.REMARKS}','{userInfo.company_code}','{userInfo.branch_code}','{model.OUTLET_TYPE_ID}','{model.OUTLET_SUBTYPE_ID}','{model.RESELLER_CONTACT}',{model.GROUP_ID})";

                    _objectEntity.ExecuteSqlCommand(Query1);
                    foreach (var con in model.Contacts)
                    {
                        string ContactQuery = $@"INSERT INTO DIST_RESELLER_DETAIL(RESELLER_CODE,COMPANY_CODE,CONTACT_SUFFIX,CONTACT_NAME,CONTACT_NO,DESIGNATION,CREATED_BY,CREATED_DATE) VALUES
                            ('{reselerCode}','{userInfo.company_code}','{con.ContactSuffix}','{con.Name}','{con.Number}','{con.Designation}','{userInfo.User_id}',TO_DATE(SYSDATE))";
                        int row = _objectEntity.ExecuteSqlCommand(ContactQuery);
                    }
                    foreach (var distributor in model.Distributors)
                    {
                        string disInsertQuery = $@"INSERT INTO DIST_RESELLER_ENTITY (RESELLER_CODE,ENTITY_CODE,ENTITY_TYPE,CREATED_BY,CREATED_DATE,DELETED_FLAG,COMPANY_CODE,BRANCH_CODE)
                                                VALUES('{reselerCode}','{distributor.Code}','D','{userInfo.User_id}',TO_DATE(SYSDATE),'N','{userInfo.company_code}','{userInfo.branch_code}')";
                        int row = _objectEntity.ExecuteSqlCommand(disInsertQuery);
                    }
                    foreach (var wholeseller in model.Wholesellers)
                    {
                        string whoInsertQuery = $@"INSERT INTO DIST_RESELLER_ENTITY (RESELLER_CODE,ENTITY_CODE,ENTITY_TYPE,CREATED_BY,CREATED_DATE,DELETED_FLAG,COMPANY_CODE,BRANCH_CODE)
                                                VALUES('{reselerCode}','{wholeseller.Code}','W','{userInfo.User_id}',TO_DATE(SYSDATE),'N','{userInfo.company_code}','{userInfo.branch_code}')";
                        int row = _objectEntity.ExecuteSqlCommand(whoInsertQuery);
                    }

                    _objectEntity.SaveChanges();
                    transaction.Commit();
                    message = "200"; //success
                }
                catch (Exception e)
                {
                    transaction.Rollback();
                    message = "500"; //error
                }
            }
            return message;

        }

        public string AddOtherEntity(OtherEntity model, User userInfo)
        {
            try
            {

                var code = _objectEntity.SqlQuery<int>($@"SELECT NVL(MAX(TO_NUMBER(CODE))+1,1) MAXID FROM BRD_OTHER_ENTITY").FirstOrDefault();

                var insertEntity = $@"INSERT INTO BRD_OTHER_ENTITY(CODE,DESCRIPTION,CONTACT_PERSON,AREA_CODE,GROUP_ID,CONTACT_NO,LATITUDE,LONGITUDE,COMPANY_CODE,BRANCH_CODE,DELETED_FLAG,CREATED_BY,CREATED_DATE,TYPE)
                                VALUES('{code}','{model.CONTACT_PERSON}','{model.CONTACT_PERSON}','{model.AREA_CODE}',{model.GROUP_ID},'{model.CONTACT_NO}','{model.LATITUDE}','{model.LONGITUDE}','{userInfo.company_code}','{userInfo.branch_code}','N','{userInfo.login_code}',SYSDATE,'{model.TYPE}')";
                var result = _objectEntity.ExecuteSqlCommand(insertEntity);
                _objectEntity.SaveChanges();
                return "success";
            }
            catch (Exception e)
            {

                return "error";
            }
        }

        public string UpdateReseller(ResellerListModel model, User userInfo)
        {
            var message = "";
            using (var transaction = _objectEntity.Database.BeginTransaction())
            {
                try
                {
                    var route_code = _objectEntity.SqlQuery<string>($"SELECT TEMP_ROUTE_CODE FROM DIST_RESELLER_MASTER WHERE RESELLER_CODE = '{model.Reseller_CODE}'").FirstOrDefault();
                    if (model.Active == "Y" && !string.IsNullOrWhiteSpace(route_code))
                    {
                        var ENTITY_CODE = model.Reseller_CODE;
                        //get max order_no
                        var Order_query = $@"SELECT to_char(MAX(ORDER_NO) + 1) FROM DIST_ROUTE_ENTITY WHERE ROUTE_CODE = '{route_code}'";
                        var ORDER_NO = _objectEntity.SqlQuery<string>(Order_query).FirstOrDefault();
                        if (!string.IsNullOrEmpty(ORDER_NO))
                        {
                            var DIST_ROUTE_ENTITY_QUERY = $@"INSERT INTO DIST_ROUTE_ENTITY(ROUTE_CODE,ENTITY_CODE,ENTITY_TYPE,ORDER_NO,CREATED_BY,CREATED_DATE,DELETED_FLAG,COMPANY_CODE)  
                                     Values('{route_code}','{ENTITY_CODE}','R',{ORDER_NO},'{userInfo.User_id}',TO_DATE(SYSDATE),'N','{userInfo.company_code}')";
                            _objectEntity.ExecuteSqlCommand(DIST_ROUTE_ENTITY_QUERY);
                            //update DIST_RESELLER_MASTER
                            var UPDATE_QUERY = $@"UPDATE DIST_RESELLER_MASTER SET TEMP_ROUTE_CODE='' WHERE RESELLER_CODE='{ENTITY_CODE}'";
                            _objectEntity.ExecuteSqlCommand(UPDATE_QUERY);
                        }
                        //insert into DIST_ROUTE_ENTITY

                    }

                    var Query1 = $@"Update DIST_RESELLER_MASTER
                                   SET RESELLER_NAME='{model.Reseller_NAME.Replace("'", "''")}',AREA_CODE='{model.AREA_Code}',REG_OFFICE_ADDRESS='{model.ADDRESS.Replace("'", "''")}',
                                       CONTACT_NAME='{model.Primary_Contact_Name}',CONTACT_NO='{model.Primary_Contact_No}',EMAIL='{model.Email}',LATITUDE='{model.LATITUDE}',LONGITUDE='{model.LONGITUDE}',
                                      VAT_NO='{model.Vat_No}',PAN_NO='{model.Pan_No}',LUPDATE_BY='{userInfo.User_id}',LUPDATE_DATE=sysdate,WHOLESELLER='{model.WholeSeller}',REMARKS = '{model.REMARKS}',
                                    OUTLET_TYPE_ID='{model.OUTLET_TYPE_ID}',OUTLET_SUBTYPE_ID='{model.OUTLET_SUBTYPE_ID}',RESELLER_CONTACT='{model.RESELLER_CONTACT}',GROUPID = {model.GROUP_ID},ACTIVE='{model.Active}', IS_CLOSED = '{model.IsClosed}'
                                    WHERE COMPANY_CODE = '{userInfo.company_code}' AND RESELLER_CODE = '{model.Reseller_CODE}'";

                    _objectEntity.ExecuteSqlCommand(Query1);
                    _objectEntity.ExecuteSqlCommand($"DELETE FROM DIST_RESELLER_DETAIL WHERE COMPANY_CODE = '{userInfo.company_code}' AND RESELLER_CODE = '{model.Reseller_CODE}'");
                    _objectEntity.ExecuteSqlCommand($"DELETE FROM DIST_RESELLER_ENTITY WHERE COMPANY_CODE = '{userInfo.company_code}' AND RESELLER_CODE = '{model.Reseller_CODE}'");
                    foreach (var con in model.Contacts)
                    {
                        string ContactQuery = $@"INSERT INTO DIST_RESELLER_DETAIL(RESELLER_CODE,COMPANY_CODE,CONTACT_SUFFIX,CONTACT_NAME,CONTACT_NO,DESIGNATION,CREATED_BY,CREATED_DATE) VALUES
                            ('{model.Reseller_CODE}','{userInfo.company_code}','{con.ContactSuffix}','{con.Name}','{con.Number}','{con.Designation}','{userInfo.User_id}',TO_DATE(SYSDATE))";
                        int row = _objectEntity.ExecuteSqlCommand(ContactQuery);
                    }
                    foreach (var distributor in model.Distributors)
                    {
                        string disInsertQuery = $@"INSERT INTO DIST_RESELLER_ENTITY (RESELLER_CODE,ENTITY_CODE,ENTITY_TYPE,CREATED_BY,CREATED_DATE,DELETED_FLAG,COMPANY_CODE,BRANCH_CODE)
                                                VALUES('{model.Reseller_CODE}','{distributor.Code}','D','{userInfo.User_id}',TO_DATE(SYSDATE),'N','{userInfo.company_code}','{userInfo.branch_code}')";
                        int row = _objectEntity.ExecuteSqlCommand(disInsertQuery);
                    }
                    foreach (var wholeseller in model.Wholesellers)
                    {
                        string whoInsertQuery = $@"INSERT INTO DIST_RESELLER_ENTITY (RESELLER_CODE,ENTITY_CODE,ENTITY_TYPE,CREATED_BY,CREATED_DATE,DELETED_FLAG,COMPANY_CODE,BRANCH_CODE)
                                                VALUES('{model.Reseller_CODE}','{wholeseller.Code}','W','{userInfo.User_id}',TO_DATE(SYSDATE),'N','{userInfo.company_code}','{userInfo.branch_code}')";
                        int row = _objectEntity.ExecuteSqlCommand(whoInsertQuery);
                    }

                    _objectEntity.SaveChanges();
                    transaction.Commit();
                    message = "200";
                }
                catch (Exception X)
                {
                    transaction.Rollback();
                    message = "500";
                }
            }
            return message;

        }

        public string UpdateEntity(OtherEntity model, User userInfo)
        {
            var updateQuery = $@"UPDATE BRD_OTHER_ENTITY SET DESCRIPTION ='{model.CONTACT_PERSON}',CONTACT_PERSON='{model.CONTACT_PERSON}',AREA_CODE='{model.AREA_CODE}',GROUP_ID={model.GROUP_ID},CONTACT_NO='{model.CONTACT_NO}',LATITUDE='{model.LATITUDE}',LONGITUDE='{model.LONGITUDE}',TYPE ='{model.TYPE}' WHERE CODE ='{model.CODE}' and COMPANY_CODE ='{userInfo.company_code}'";
            var result = _objectEntity.SqlQuery<OtherEntity>(updateQuery).ToList();
            return "success";
        }

        public string deleteReseller(ResellerListModel model, User userInfo)
        {
            try
            {
                string deleteQuery = $@"UPDATE DIST_RESELLER_MASTER SET DELETED_FLAG ='Y' WHERE RESELLER_CODE ='{model.Reseller_CODE}' AND COMPANY_CODE ='{userInfo.company_code}'";
                _objectEntity.ExecuteSqlCommand(deleteQuery);
                _objectEntity.SaveChanges();
                return "success";
            }
            catch (Exception e)
            {
                return "error";
            }
        }

        public string DeleteOtherEntity(OtherEntity model, User userInfo)
        {
            try
            {
                var deleteQuery = $@"UPDATE BRD_OTHER_ENTITY SET DELETED_FLAG ='Y' WHERE CODE ='{model.CODE}' AND COMPANY_CODE ='{userInfo.company_code}'";
                var result = _objectEntity.ExecuteSqlCommand(deleteQuery);
                _objectEntity.SaveChanges();
                return "success";
            }
            catch (Exception e)
            {
                return "error";
            }
        }

        public string deleteArea(ResellerListModel model, User userInfo)
        {
            string deleteQuery = $@"UPDATE DIST_AREA_MASTER SET DELETED_FLAG ='Y' WHERE AREA_CODE = '{model.AREA_Code}' AND COMPANY_CODE = '{userInfo.company_code}'";
            _objectEntity.ExecuteSqlCommand(deleteQuery);
            _objectEntity.SaveChanges();
            return "success";
        }

        public ResellerListModel GetReseller(string Id, User userInfo)
        {
            var Query = $@"SELECT RM.RESELLER_CODE,
              RM.RESELLER_NAME,
              RM.DISTRIBUTOR_CODE,
              RM.REG_OFFICE_ADDRESS AS ADDRESS,
              RM.EMAIL,
              (RM.CONTACT_SUFFIX || ' ' || RM.CONTACT_NAME || ' : ' || RM.CONTACT_NO) as PRIMARY_CONTACT, 
              RM.CONTACT_NAME Primary_Contact_Name,
              RM.CONTACT_NO Primary_Contact_No,
              RM.LATITUDE,
              RM.LONGITUDE,
              RM.PAN_NO,
              RM.VAT_NO,
              RM.RESELLER_CONTACT,
              RM.GROUPID GROUP_ID,
              RM.WHOLESELLER,
              NVL(RM.OUTLET_TYPE_ID,0) OUTLET_TYPE_ID,
              NVL(RM.OUTLET_SUBTYPE_ID,0) OUTLET_SUBTYPE_ID,
              RM.ACTIVE,
              OT.TYPE_EDESC     AS OUTLET_TYPE,
              OST.SUBTYPE_EDESC AS OUTLET_SUBTYPE,
              CS.CUSTOMER_EDESC AS DISTRIBUTOR_NAME,
              AM.AREA_CODE,
              AM.AREA_NAME
            FROM DIST_RESELLER_MASTER RM
            LEFT JOIN DIST_OUTLET_TYPE OT
            ON OT.TYPE_ID       = RM.OUTLET_TYPE_ID
            AND OT.COMPANY_CODE = RM.COMPANY_CODE
            LEFT JOIN DIST_OUTLET_SUBTYPE OST
            ON OST.TYPE_ID       = OT.TYPE_ID
            AND OST.SUBTYPE_ID   = RM.OUTLET_SUBTYPE_ID
            AND OST.COMPANY_CODE = RM.COMPANY_CODE
            LEFT JOIN DIST_DISTRIBUTOR_MASTER DM
            ON DM.DISTRIBUTOR_CODE = RM.DISTRIBUTOR_CODE
            AND DM.COMPANY_CODE    = RM.COMPANY_CODE AND DM.ACTIVE = 'Y'
            LEFT JOIN SA_CUSTOMER_SETUP CS
            ON CS.CUSTOMER_CODE = RM.DISTRIBUTOR_CODE
            AND CS.COMPANY_CODE = RM.COMPANY_CODE
            LEFT JOIN DIST_AREA_MASTER AM
            ON AM.AREA_CODE       = RM.AREA_CODE
            AND AM.COMPANY_CODE   = RM.COMPANY_CODE
            WHERE RM.COMPANY_CODE = '{userInfo.company_code}'
                AND RM.RESELLER_CODE='{Id}'
            ORDER BY UPPER(RM.RESELLER_NAME) ASC";
            string ContactQuery = $"SELECT CONTACT_SUFFIX ContactSuffix,CONTACT_NAME Name,CONTACT_NO \"Number\",DESIGNATION Designation FROM DIST_RESELLER_DETAIL WHERE RESELLER_CODE='{Id}' AND DELETED_FLAG='N' AND COMPANY_CODE='{userInfo.company_code}'";

            var data = _objectEntity.SqlQuery<ResellerListModel>(Query).FirstOrDefault();
            data.Contacts = _objectEntity.SqlQuery<ContactModel>(ContactQuery).ToList();

            var distributorQuery = $@"SELECT DRE.ENTITY_CODE CODE, DDM.CUSTOMER_EDESC NAME FROM 
                    DIST_RESELLER_ENTITY DRE,
                    (SELECT DISTINCT DDM.DISTRIBUTOR_CODE,SCS.CUSTOMER_EDESC FROM
                    DIST_DISTRIBUTOR_MASTER DDM,SA_CUSTOMER_SETUP SCS
                    WHERE DDM.DISTRIBUTOR_CODE=SCS.CUSTOMER_CODE AND DDM.DELETED_FLAG = 'N' AND DDM.ACTIVE = 'Y') DDM
                    WHERE DRE.ENTITY_CODE=DDM.DISTRIBUTOR_CODE(+)
                    AND DRE.ENTITY_TYPE='D'
                    AND DRE.RESELLER_CODE='{data.Reseller_CODE}'";
            var wholesellerQuery = $@"SELECT DRE.ENTITY_CODE CODE,DRM.RESELLER_NAME NAME FROM 
                    DIST_RESELLER_ENTITY DRE,DIST_RESELLER_MASTER DRM
                    WHERE DRE.ENTITY_CODE=DRM.RESELLER_CODE
                    AND  DRM.WHOLESELLER='Y' 
                    AND DRE.ENTITY_TYPE='W'
                    AND DRE.RESELLER_CODE='{data.Reseller_CODE}'";

            data.Distributors = _objectEntity.SqlQuery<CustomerModel>(distributorQuery).ToList();
            data.Wholesellers = _objectEntity.SqlQuery<CustomerModel>(wholesellerQuery).ToList();
            data.StorePhotos = _objectEntity.SqlQuery<string>($"SELECT FILENAME FROM DIST_PHOTO_INFO WHERE ENTITY_CODE='{data.Reseller_CODE}' AND ENTITY_TYPE='R' AND MEDIA_TYPE='STORE'").ToList();
            data.PContactPhotos = _objectEntity.SqlQuery<string>($"SELECT FILENAME FROM DIST_PHOTO_INFO WHERE ENTITY_CODE='{data.Reseller_CODE}' AND ENTITY_TYPE='R' AND MEDIA_TYPE='PCONTACT'").ToList();
            return data;
        }

        public List<OtherEntity> GetIndividualEntity(string Code, User userInfo)
        {
            var subQuery = $@"SELECT  DGM.GROUP_EDESC,DAM.AREA_NAME,BOE.CODE,BOE.DESCRIPTION,BOE.CONTACT_PERSON,BOE.AREA_CODE,BOE.GROUP_ID,BOE.CONTACT_NO,BOE.LATITUDE,BOE.LONGITUDE,BOE.TYPE
                                    FROM BRD_OTHER_ENTITY BOE
                                    INNER JOIN DIST_AREA_MASTER DAM ON DAM.COMPANY_CODE = BOE.COMPANY_CODE AND DAM.AREA_CODE = BOE.AREA_CODE
                                    INNER JOIN DIST_GROUP_MASTER DGM ON DGM.GROUPID= BOE.GROUP_ID AND DGM.COMPANY_CODE = BOE.COMPANY_CODE WHERE BOE.CODE ='{Code}'";
            var result = _objectEntity.SqlQuery<OtherEntity>(subQuery).ToList();
            return result;
        }

        public List<AreaModel> GetDistrictList()
        {
            string selectQuery = @"select DISTINCT DISTRICT_CODE,DISTRICT_NAME from dist_address_master order by DISTRICT_NAME";
            var data = _objectEntity.SqlQuery<AreaModel>(selectQuery).ToList();
            return data;
        }
        public List<AreaModel> GetvdcList(string DISTRICT_CODE)
        {
            string selectQuery = @"select VDC_NAME,VDC_CODE FROM dist_address_master where DISTRICT_CODE=" + DISTRICT_CODE + "";
            var data = _objectEntity.SqlQuery<AreaModel>(selectQuery).ToList();
            return data;
        }
        public List<AreaModel> GetZoneList(string DISTRICT_CODE)
        {
            string selectQuery = @"SELECT ZONE_CODE , ZONE_NAME FROM dist_address_master where DISTRICT_CODE=" + DISTRICT_CODE + "";
            var data = _objectEntity.SqlQuery<AreaModel>(selectQuery).ToList();
            return data;
        }
        public List<AreaModel> GetRegionList(string DISTRICT_CODE)
        {
            string selectQuery = @"SELECT REG_CODE , REG_NAME FROM dist_address_master where DISTRICT_CODE=" + DISTRICT_CODE + "";
            var data = _objectEntity.SqlQuery<AreaModel>(selectQuery).ToList();
            return data;
        }
        public List<ResellerGroupModel> GetResellerGroups(User UserInfo)
        {
            string Query = $"SELECT GROUPID,GROUP_EDESC,GROUP_CODE FROM DIST_GROUP_MASTER WHERE DELETED_FLAG='N' AND COMPANY_CODE='{UserInfo.company_code}' ORDER BY TRIM(GROUP_EDESC) ASC";
            var list = _objectEntity.SqlQuery<ResellerGroupModel>(Query).ToList();
            return list;
        }
        public List<DistUserEmployeeModel> getLoginEmployee(User UserInfo)
        {
            string Query = $@"SELECT DISTINCT HS.EMPLOYEE_CODE LOGIN_SP_CODE, HS.EMPLOYEE_EDESC LOGIN_SP_EDESC FROM SC_APPLICATION_USERS SA, HR_EMPLOYEE_SETUP HS
                   WHERE SA.EMPLOYEE_CODE = HS.EMPLOYEE_CODE
                   AND SA.COMPANY_CODE = HS.COMPANY_CODE
                   AND SA.COMPANY_CODE = '{UserInfo.company_code}'
                   AND SA.DELETED_FLAG = 'N'";
            var list = _objectEntity.SqlQuery<DistUserEmployeeModel>(Query).ToList();
            return list;
        }
        public List<DistUserEmployeeModel> getDistLoginEmployee(User UserInfo)
        {
            string Query = $@"SELECT DISTINCT HS.EMPLOYEE_CODE DIST_SP_CODE, HS.EMPLOYEE_EDESC DIST_SP_EDESC FROM DIST_LOGIN_USER SA, HR_EMPLOYEE_SETUP HS
                   WHERE SA.SP_CODE = HS.EMPLOYEE_CODE
                   AND SA.COMPANY_CODE = HS.COMPANY_CODE
                   AND SA.COMPANY_CODE = '{UserInfo.company_code}'";
            var list = _objectEntity.SqlQuery<DistUserEmployeeModel>(Query).ToList();
            return list;
        }
        public List<ImageCategoryModel> GetCategoryImage(User UserInfo)
        {
            string Query = $"SELECT CATEGORYID,CATEGORY_EDESC,CATEGORY_CODE,MAX_ITEMS,selectedType FROM DIST_IMAGE_CATEGORY WHERE DELETED_FLAG='N' AND COMPANY_CODE='{UserInfo.company_code}' AND CATEGORYID != '1' ORDER BY TRIM(CATEGORY_EDESC) ASC";
            var list = _objectEntity.SqlQuery<ImageCategoryModel>(Query).ToList();
            return list;
        }
        public string AddResellerGroup(ResellerGroupModel model, User userInfo)
        {
            string Query = string.Empty;
            if (model.GROUPID > 0)
            {
                Query = $"UPDATE DIST_GROUP_MASTER SET GROUP_CODE='{model.GROUP_CODE}',GROUP_EDESC='{model.GROUP_EDESC}',MODIFY_BY='{userInfo.User_id}',MODIFY_DATE=TO_DATE(SYSDATE) WHERE GROUPID='{model.GROUPID}'";
            }
            else
            {
                var maxId = _objectEntity.SqlQuery<decimal>($"SELECT NVL(MAX(GROUPID),200) FROM DIST_GROUP_MASTER WHERE COMPANY_CODE ='{userInfo.company_code}'").FirstOrDefault();

                Query = $@"INSERT INTO DIST_GROUP_MASTER (GROUPID,GROUP_CODE,GROUP_EDESC,DELETED_FLAG,CREATED_BY,CREATED_DATE,COMPANY_CODE) VALUES
                        ('{++maxId}','{model.GROUP_CODE}','{model.GROUP_EDESC}','N','{userInfo.User_id}',TO_DATE(SYSDATE),'{userInfo.company_code}')";
            }
            var row = _objectEntity.ExecuteSqlCommand(Query);
            if (row <= 0)
                throw new Exception("Error processing the request");

            return "Success";
        }

        public string deleteGroup(ResellerGroupModel modal, User userInfo)
        {
            try
            {
                string deleteQuery = $@"UPDATE DIST_GROUP_MASTER SET DELETED_FLAG ='Y' WHERE GROUPID = '{modal.GROUPID}' AND COMPANY_CODE ='{userInfo.company_code}'";
                _objectEntity.ExecuteSqlCommand(deleteQuery);
                _objectEntity.SaveChanges();
                return "success";
            }
            catch (Exception e)
            {
                return "error";
            }

        }

        public string AddImageCategory(ImageCategoryModel model, User userInfo)
        {
            string Query = string.Empty;
            model.Max_Items = model.Max_Items == 0 ? 1 : model.Max_Items;
            if (model.CATEGORYID > 0)
            {
                Query = $"UPDATE DIST_IMAGE_CATEGORY SET CATEGORY_CODE='{model.CATEGORY_CODE}',CATEGORY_EDESC='{model.CATEGORY_EDESC}',MAX_ITEMS='{model.Max_Items}',MODIFY_BY='{userInfo.User_id}',MODIFY_DATE=TO_DATE(SYSDATE),selectedType='{model.selectedType}' WHERE CATEGORYID='{model.CATEGORYID}'";
            }
            else
            {
                var imageId = _objectEntity.SqlQuery<decimal>("SELECT NVL(MAX(CATEGORYID),0) FROM DIST_IMAGE_CATEGORY").FirstOrDefault();
                //var imageId = @"SELECT NVL(MAX(CATEGORYID),0) FROM DIST_IMAGE_CATEGORY";
                var maxId = imageId + 1;
                var DELETED_FLAG = 'N';
                Query = $@"INSERT INTO DIST_IMAGE_CATEGORY(CATEGORYID,CATEGORY_CODE,CATEGORY_EDESC,MAX_ITEMS,selectedType,DELETED_FLAG,CREATED_BY,CREATED_DATE,COMPANY_CODE) VALUES
                        ('{maxId}','{model.CATEGORY_CODE}','{model.CATEGORY_EDESC}','{model.Max_Items}','{model.selectedType}','{DELETED_FLAG}','{userInfo.User_id}',TO_DATE(SYSDATE),'{userInfo.company_code}')";
            }
            var row = _objectEntity.ExecuteSqlCommand(Query);
            if (row <= 0)
                throw new Exception("Error processing the request");

            return "Success";
        }

        public string deleteImage(ImageCategoryModel model, User userInfo)
        {
            try
            {
                string deleteQuery = $@"UPDATE DIST_IMAGE_CATEGORY SET DELETED_FLAG ='Y' WHERE CATEGORYID ='{model.CATEGORYID}' AND COMPANY_CODE ='{userInfo.company_code}'";
                _objectEntity.ExecuteSqlCommand(deleteQuery);
                _objectEntity.SaveChanges();
                return "success";
            }
            catch (Exception e)
            {
                return "error";
            }
        }
        //public string AddCategoryImage(ImageCategoryModel model, User userInfo)
        //{

        //    throw new NotImplementedException();
        //}
        public string DeleteResellerGroup(int GroupId, User UserInfo)
        {
            var Query = $"UPDATE DIST_GROUP_MASTER SET DELETED_FLAG='Y' WHERE GROUPID='{GroupId}'";
            var row = _objectEntity.ExecuteSqlCommand(Query);
            if (row <= 0)
                throw new Exception("Error processing the request");

            return "Success";
        }
        public string DeleteImageCategory(int CategoryId, User UserInfo)
        {
            var Query = $"UPDATE DIST_IMAGE_CATEGORY SET DELETED_FLAG='Y' WHERE CATEGORYID='{CategoryId}'";
            var row = _objectEntity.ExecuteSqlCommand(Query);
            if (row <= 0)
                throw new Exception("Error processing the request");

            return "Success";
        }

        public List<AreaModel> GetAllAreaList()
        {
            string selectQuery = @"SELECT AM1.AREA_CODE, AM1.AREA_NAME,AM1.GROUPID,GM.GROUP_EDESC, AM1.VDC_CODE,AM2.VDC_NAME, AM1.DISTRICT_CODE, AM3.DISTRICT_NAME, AM1.ZONE_CODE, AM4.ZONE_NAME
                 FROM DIST_AREA_MASTER AM1 
                 LEFT JOIN DIST_ADDRESS_MASTER AM2 ON AM2.VDC_CODE = AM1.VDC_CODE
                 LEFT JOIN DIST_GROUP_MASTER GM ON GM.GROUPID = AM1.GROUPID AND GM.COMPANY_CODE = AM1.COMPANY_CODE
                 LEFT JOIN (SELECT DISTRICT_CODE, DISTRICT_NAME FROM DIST_ADDRESS_MASTER WHERE DISTRICT_CODE IN (SELECT DISTRICT_CODE FROM DIST_ADDRESS_MASTER) GROUP BY DISTRICT_CODE, DISTRICT_NAME) AM3 ON AM3.DISTRICT_CODE = AM1.DISTRICT_CODE
                 LEFT JOIN (SELECT ZONE_CODE, ZONE_NAME FROM DIST_ADDRESS_MASTER WHERE ZONE_CODE IN (SELECT ZONE_CODE FROM DIST_ADDRESS_MASTER) GROUP BY ZONE_CODE, ZONE_NAME) AM4 ON AM4.ZONE_CODE = AM1.ZONE_CODE
                WHERE AM1.DELETED_FLAG ='N'
                ORDER BY TO_NUMBER(SUBSTR(AM1.AREA_CODE, 2))";
            var result = _objectEntity.SqlQuery<AreaModel>(selectQuery).ToList();
            return result;
        }

        public string AddArea(AreaModel modal, User userInfo)
        {
            var message = "";
            string selectQuery = @"SELECT NVL(MAX(TO_NUMBER(SUBSTR(AREA_CODE, 2)))+1, 1) MAXID FROM DIST_AREA_MASTER";
            var data = _objectEntity.SqlQuery<AreaModel>(selectQuery).ToList();

            var areaCode = data[0].MAXID;
            string str = "A";
            string AREA_CODE = str + "" + areaCode;
            // return data.ToString();
            {
                try
                {
                    var insertQuery = $@"INSERT INTO DIST_AREA_MASTER(AREA_CODE,AREA_NAME,ZONE_CODE,DISTRICT_CODE,VDC_CODE,REG_CODE,COMPANY_CODE,BRANCH_CODE,GROUPID) 
                                          VALUES('{AREA_CODE}','{modal.AREA_NAME}','{modal.ZONE_CODE}','{modal.DISTRICT_CODE}','{modal.VDC_CODE}','{modal.REG_CODE}','{userInfo.company_code}','{userInfo.branch_code}','{modal.GROUPID}')";


                    _objectEntity.ExecuteSqlCommand(insertQuery);
                    _objectEntity.SaveChanges();
                    message = "success";
                }
                catch (Exception e)
                {
                    message = "failed";
                }
            }

            return message;
        }

        public string UpdateArea(AreaModel modal, User userInfo)
        {
            var message = "";

            try
            {

                var updateQuery = $@"Update DIST_AREA_MASTER 
                                     SET AREA_CODE='{modal.AREA_CODE}',AREA_NAME='{modal.AREA_NAME}',ZONE_CODE='{modal.ZONE_CODE}',DISTRICT_CODE='{modal.DISTRICT_CODE}',VDC_CODE='{modal.VDC_CODE}',REG_CODE='{modal.REG_CODE}',COMPANY_CODE='{userInfo.company_code}',BRANCH_CODE='{userInfo.branch_code}' ,
                                         GROUPID ='{modal.GROUPID}'
                                 WHERE AREA_CODE='{modal.AREA_CODE}'";

                _objectEntity.ExecuteSqlCommand(updateQuery);

                _objectEntity.SaveChanges();
                message = "success";
            }
            catch (Exception X)
            {
                message = "failed";
            }

            return message;

        }

        public AreaModel GetAreaSaveObj(AreaModel model, User userInfo)
        {
            var Group = $"SELECT GROUPID FROM DIST_GROUP_MASTER WHERE UPPER(GROUP_EDESC) = UPPER('{model.GROUP_EDESC}') OR GROUPID LIKE '{model.GROUP_EDESC}' AND COMPANY_CODE = '06'";
            var District = $"SELECT DISTINCT TO_CHAR(DISTRICT_CODE) FROM DIST_ADDRESS_MASTER WHERE UPPER(DISTRICT_NAME) = UPPER('{model.DISTRICT_NAME.Trim()}') OR DISTRICT_CODE LIKE '{model.DISTRICT_NAME.Trim()}'";
            var Vdc = $"SELECT DISTINCT TO_CHAR(VDC_CODE) FROM DIST_ADDRESS_MASTER WHERE UPPER(VDC_NAME) = UPPER('{model.VDC_NAME.Trim()}') OR VDC_CODE LIKE '{model.VDC_NAME.Trim()}'";
            var Region = $"SELECT DISTINCT TO_CHAR(REG_CODE) FROM DIST_ADDRESS_MASTER WHERE UPPER(REG_NAME) = UPPER('{model.REG_NAME.Trim()}') OR REG_CODE LIKE '{model.REG_NAME.Trim()}'";
            var Zone = $"SELECT DISTINCT TO_CHAR(ZONE_CODE) FROM DIST_ADDRESS_MASTER WHERE UPPER(ZONE_NAME) = UPPER('{model.ZONE_NAME.Trim()}') OR ZONE_CODE LIKE '{model.ZONE_NAME.Trim()}'";

            model.GROUPID = _objectEntity.SqlQuery<int>(Group).FirstOrDefault();
            model.DISTRICT_CODE = _objectEntity.SqlQuery<string>(District).FirstOrDefault();
            model.VDC_CODE = _objectEntity.SqlQuery<string>(Vdc).FirstOrDefault();
            model.REG_CODE = _objectEntity.SqlQuery<string>(Region).FirstOrDefault();
            model.ZONE_CODE = _objectEntity.SqlQuery<string>(Zone).FirstOrDefault();

            return model;
        }

        public string AddRoute(RouteListModel model, User userInfo)
        {
            var message = "";

            var companyCode = userInfo.company_code;
            using (var transaction = _objectEntity.Database.BeginTransaction())
            {
                try
                {

                    string maxValue = $"SELECT NVL(MAX(TO_NUMBER(SUBSTR(ROUTE_CODE, 2)))+1, 1) MAXID FROM DIST_ROUTE_MASTER";
                    model.ROUTE_CODE = "R" + _objectEntity.SqlQuery<decimal>(maxValue).FirstOrDefault();


                    var Query1 = $@"INSERT INTO DIST_ROUTE_MASTER (ROUTE_CODE,ROUTE_NAME,COMPANY_CODE,BRANCH_CODE,ROUTE_TYPE)
                                VALUES ('{model.ROUTE_CODE}','{model.ROUTE_NAME}','{companyCode}','{userInfo.branch_code}','{model.ROUTE_TYPE}')";
                    _objectEntity.ExecuteSqlCommand(Query1);

                    foreach (var area in model.AREA_CODE)
                    {
                        var Query2 = $@"INSERT INTO DIST_ROUTE_AREA (ROUTE_CODE,AREA_CODE,COMPANY_CODE,BRANCH_CODE)
                                VALUES ('{model.ROUTE_CODE}','{area}','{companyCode}','{userInfo.branch_code}')";
                        _objectEntity.ExecuteSqlCommand(Query2);
                    }

                    foreach (var entity in model.RouteEntityModel)
                    {
                        string Query3 = string.Empty;
                        if (entity.ENTITY_TYPE.Trim() == "H")
                            Query3 = $@"INSERT INTO BRD_ROUTE_ENTITY (ROUTE_CODE,ENTITY_CODE,ORDER_NO,COMPANY_CODE,CREATED_BY,CREATED_DATE)
                                VALUES ('{model.ROUTE_CODE}','{entity.ENTITY_Code}','{entity.ORDER_NO}','{companyCode}','{userInfo.User_id}',SYSDATE)";
                        else
                            Query3 = $@"INSERT INTO DIST_ROUTE_ENTITY (ROUTE_CODE,ENTITY_CODE,ENTITY_TYPE,ORDER_NO,COMPANY_CODE,CREATED_BY,CREATED_DATE)
                                VALUES ('{model.ROUTE_CODE}','{entity.ENTITY_Code}','{entity.ENTITY_TYPE}','{entity.ORDER_NO}','{companyCode}','{userInfo.User_id}',SYSDATE)";
                        _objectEntity.ExecuteSqlCommand(Query3);
                    }


                    //_objectEntity.SaveChanges();
                    transaction.Commit();
                    message = "200"; //success
                }
                catch (Exception e)
                {
                    transaction.Rollback();
                    message = "500"; //error
                }
            }
            return message;

        }

        public string UpdateRoute(RouteListModel model, User userInfo)
        {
            var message = "";
            using (var transaction = _objectEntity.Database.BeginTransaction())
            {
                try
                {

                    var Query1 = $@"UPDATE DIST_ROUTE_MASTER 
                                      SET ROUTE_NAME='{model.ROUTE_NAME}'
                                 WHERE ROUTE_CODE='{model.ROUTE_CODE}' AND COMPANY_CODE='{userInfo.company_code}'";
                    _objectEntity.ExecuteSqlCommand(Query1);


                    var Query2 = $"DELETE FROM DIST_ROUTE_AREA WHERE ROUTE_CODE = '{model.ROUTE_CODE}' AND COMPANY_CODE='{userInfo.company_code}'";
                    _objectEntity.ExecuteSqlCommand(Query2);
                    foreach (var area in model.AREA_CODE)
                    {
                        var Query3 = $@"Insert into DIST_ROUTE_AREA (ROUTE_CODE,AREA_CODE,COMPANY_CODE,BRANCH_CODE)
                                Values ('{model.ROUTE_CODE}','{area}','{userInfo.company_code}','{userInfo.branch_code}')";
                        _objectEntity.ExecuteSqlCommand(Query3);
                    }

                    var Query4 = $"DELETE FROM DIST_ROUTE_ENTITY WHERE ROUTE_CODE = '{model.ROUTE_CODE}' AND COMPANY_CODE='{userInfo.company_code}'";
                    _objectEntity.ExecuteSqlCommand(Query4);
                    Query4 = $"DELETE FROM BRD_ROUTE_ENTITY WHERE ROUTE_CODE = '{model.ROUTE_CODE}' AND COMPANY_CODE='{userInfo.company_code}'";
                    _objectEntity.ExecuteSqlCommand(Query4);
                    foreach (var entity in model.RouteEntityModel)
                    {
                        string Query5 = string.Empty;
                        if (entity.ENTITY_TYPE.Trim() == "H")
                            Query5 = $@"INSERT INTO BRD_ROUTE_ENTITY (ROUTE_CODE,ENTITY_CODE,ORDER_NO,COMPANY_CODE,CREATED_BY,CREATED_DATE)
                                VALUES ('{model.ROUTE_CODE}','{entity.ENTITY_Code}','{entity.ORDER_NO}','{userInfo.company_code}','{userInfo.User_id}',SYSDATE)";
                        else
                            Query5 = $@"INSERT INTO DIST_ROUTE_ENTITY (ROUTE_CODE,ENTITY_CODE,ENTITY_TYPE,ORDER_NO,COMPANY_CODE,CREATED_BY,CREATED_DATE)
                                VALUES ('{model.ROUTE_CODE}','{entity.ENTITY_Code}','{entity.ENTITY_TYPE}','{entity.ORDER_NO}','{userInfo.company_code}','{userInfo.User_id}',SYSDATE)";
                        _objectEntity.ExecuteSqlCommand(Query5);
                    }

                    //_objectEntity.SaveChanges();
                    transaction.Commit();
                    message = "200";
                }
                catch (Exception X)
                {
                    transaction.Rollback();
                    message = "500";
                }
            }
            return message;

        }

        public string deleteRoute(RouteListModel model, User userInfo)
        {
            try
            {
                string deleteQuery = $@"UPDATE DIST_ROUTE_MASTER SET DELETED_FLAG ='Y' WHERE ROUTE_CODE ='{model.ROUTE_CODE}' AND COMPANY_CODE ='{userInfo.company_code}'";
                _objectEntity.ExecuteSqlCommand(deleteQuery);
                _objectEntity.SaveChanges();
                return "success";
            }
            catch (Exception e)
            {
                return "error";
            }
        }
        //get all outlet list
        public List<OutletModel> getAllOutletList()
        {
            string query = @"SELECT TYPE_ID,TYPE_CODE,TYPE_EDESC FROM DIST_OUTLET_TYPE WHERE DELETED_FLAG ='N' ORDER BY TRIM(TYPE_EDESC)";
            var result = _objectEntity.SqlQuery<OutletModel>(query).ToList();
            return result;
        }

        public string AddOutlet(OutletModel modal, User userInfo)
        {
            string maxValue = @"SELECT NVL(MAX(TO_NUMBER(TYPE_ID)) + 1, 1) MAXID FROM DIST_OUTLET_TYPE";
            var typeId = _objectEntity.SqlQuery<decimal>(maxValue).FirstOrDefault();

            //  var typeId = data[0].TYPE_ID;
            if (modal.TYPE_ID == 0)
            {
                var insertQuery = $@"INSERT INTO DIST_OUTLET_TYPE(TYPE_ID,TYPE_CODE,TYPE_EDESC,COMPANY_CODE,CREATED_BY,CREATED_DATE) VALUES({typeId},'{modal.TYPE_CODE}','{modal.TYPE_EDESC}','{userInfo.company_code}','{userInfo.login_code}',TO_DATE(SYSDATE))";
                var result = _objectEntity.ExecuteSqlCommand(insertQuery);
                this.addOutletType(modal, userInfo, typeId);
                return "success";
            }
            else
            {
                var updatequery = $@"UPDATE DIST_OUTLET_TYPE SET TYPE_CODE='{modal.TYPE_CODE}',TYPE_EDESC='{modal.TYPE_EDESC}',MODIFY_DATE=TO_DATE(SYSDATE),MODIFY_BY='{userInfo.login_code}' WHERE TYPE_ID='{modal.TYPE_ID}'AND COMPANY_CODE='{userInfo.company_code}'";
                _objectEntity.ExecuteSqlCommand(updatequery);
                this.addOutletType(modal, userInfo, typeId);
                return "update";
            }

        }

        public string deleteItem(OutletModel modal, User userInfo)
        {
            try
            {
                string deleteQuery = $@"UPDATE DIST_OUTLET_TYPE SET DELETED_FLAG = 'Y' WHERE TYPE_ID ='{modal.TYPE_ID}' AND COMPANY_CODE ='{userInfo.company_code}'";
                _objectEntity.ExecuteSqlCommand(deleteQuery);
                return "success";
            }
            catch (Exception e)
            {
                return "error";
            }


        }


        public string addOutletType(OutletModel modal, User userInfo, decimal typeId)
        {

            var subType = modal.subtypeArr;

            var deletequery = $@"DELETE FROM DIST_OUTLET_SUBTYPE WHERE TYPE_ID ='{modal.TYPE_ID}' AND COMPANY_CODE='{userInfo.company_code}'";
            _objectEntity.ExecuteSqlCommand(deletequery);
            _objectEntity.SaveChanges();

            if (modal.TYPE_ID != 0)
            {
                for (int i = 0; i < modal.subtypeArr.Count(); i++)
                {
                    if (subType[i].SUBTYPE_CODE != "")
                    {
                        string maxValue = @"SELECT NVL(MAX(TO_NUMBER(SUBTYPE_ID)) + 1, 1) MAXID FROM DIST_OUTLET_SUBTYPE";
                        var subTypeId = _objectEntity.SqlQuery<decimal>(maxValue).FirstOrDefault();
                        //for loop impliment each time and increment SubtypeCode 
                        var insertQuery = $@"INSERT INTO DIST_OUTLET_SUBTYPE(SUBTYPE_ID,SUBTYPE_CODE,SUBTYPE_EDESC,TYPE_ID,COMPANY_CODE,CREATED_BY,CREATED_DATE) VALUES('{subTypeId}','{subType[i].SUBTYPE_CODE}','{subType[i].SUBTYPE_EDESC}','{modal.TYPE_ID}','{userInfo.company_code}','{userInfo.login_code}',TO_DATE(SYSDATE))";
                        _objectEntity.ExecuteSqlCommand(insertQuery);
                    }


                    else
                    {
                        continue;
                    }

                }
            }
            else
            {
                for (int i = 0; i < modal.subtypeArr.Count(); i++)
                {
                    if (subType[i].SUBTYPE_CODE != "")
                    {
                        string maxValue = @"SELECT NVL(MAX(TO_NUMBER(SUBTYPE_ID)) + 1, 1) MAXID FROM DIST_OUTLET_SUBTYPE";
                        var subTypeId = _objectEntity.SqlQuery<decimal>(maxValue).FirstOrDefault();

                        var insertQuery = $@"INSERT INTO DIST_OUTLET_SUBTYPE(SUBTYPE_ID,SUBTYPE_CODE,SUBTYPE_EDESC,TYPE_ID,COMPANY_CODE,CREATED_BY,CREATED_DATE) VALUES('{subTypeId}','{subType[i].SUBTYPE_CODE}','{subType[i].SUBTYPE_EDESC}','{typeId}','{userInfo.company_code}','{userInfo.login_code}',SYSDATE)";


                        //  var updateQuery = $@"UPDATE DIST_OUTLET_SUBTYPE SET SUBTYPE_CODE='{subType[i].SUBTYPE_CODE}',SUBTYPE_EDESC='{subType[i].SUBTYPE_EDESC}' WHERE TYPE_ID='{typeId}' AND SUBTYPE_ID='{subTypeId}'";
                        _objectEntity.ExecuteSqlCommand(insertQuery);
                    }
                    else
                    {
                        continue;
                    }


                }

                _objectEntity.SaveChanges();
            }


            return "failed";
        }

        public List<OutletSubtypeModel> getAllSubOutletList(int TYPE_ID, User userInfo)
        {
            var query = $@"SELECT SUBTYPE_ID, SUBTYPE_CODE,SUBTYPE_EDESC, TO_NUMBER(TYPE_ID) TYPE_ID FROM DIST_OUTLET_SUBTYPE WHERE COMPANY_CODE ='{userInfo.company_code}' AND TYPE_ID='{TYPE_ID}' AND DELETED_FLAG='N'";
            var result = _objectEntity.SqlQuery<OutletSubtypeModel>(query).ToList();
            return result;
        }

        public List<OutletSubtypeModel> getAllSubOutletList(User userInfo)
        {
            var query = $@"SELECT SUBTYPE_ID, SUBTYPE_CODE,SUBTYPE_EDESC, TO_NUMBER(TYPE_ID) TYPE_ID FROM DIST_OUTLET_SUBTYPE WHERE COMPANY_CODE ='{userInfo.company_code}' AND DELETED_FLAG='N'";
            var result = _objectEntity.SqlQuery<OutletSubtypeModel>(query).ToList();
            return result;
        }

        public string AddGeneralQuestions(QuestionSetupModel model, User UserInfo)
        {
            string output = "";
            using (var trans = this._objectEntity.Database.BeginTransaction())
            {
                try
                {
                    //throw new Exception("Answer cannot be saved");
                    string setCode;
                    if (string.IsNullOrWhiteSpace(model.SetId))
                        setCode = this._objectEntity.SqlQuery<int>("SELECT NVL(MAX(TO_NUMBER(SET_CODE))+1, 1) MAXID FROM DIST_QA_SET").FirstOrDefault().ToString();
                    else
                    {
                        setCode = model.SetId;
                        var answers = this._objectEntity.SqlQuery<object>($"SELECT * FROM DIST_QA_ANSWER WHERE QA_CODE IN (SELECT QA_CODE FROM DIST_QA_MASTER WHERE SET_CODE='{setCode}')").ToList();
                        if (answers.Count > 0)
                            throw new Exception("Answered");

                        var questionsIds = this._objectEntity.SqlQuery<int>($"SELECT QA_CODE FROM DIST_QA_MASTER WHERE COMPANY_CODE='{UserInfo.company_code}' AND SET_CODE='{setCode}'").ToList();

                        //Delete Answers
                        var rows = this._objectEntity.ExecuteSqlCommand($"DELETE FROM DIST_QA_DETAIL WHERE COMPANY_CODE='{UserInfo.company_code}' AND QA_CODE IN('{string.Join("','", questionsIds)}')");

                        //Delete Questions
                        rows = this._objectEntity.ExecuteSqlCommand($"DELETE FROM DIST_QA_MASTER WHERE COMPANY_CODE='{UserInfo.company_code}' AND SET_CODE='{setCode}'");

                        //delete SET
                        rows = this._objectEntity.ExecuteSqlCommand($"DELETE FROM DIST_QA_SET WHERE COMPANY_CODE='{UserInfo.company_code}' AND SET_CODE='{setCode}'");

                    }

                    //inserting question set
                    string setInsertQuery = $@"INSERT INTO DIST_QA_SET (SET_CODE,SET_DATE,TITLE,CREATED_BY,CREATED_DATE,DELETED_FLAG,COMPANY_CODE,BRANCH_CODE,QA_TYPE)
                            VALUES('{setCode}',TO_DATE(SYSDATE),'{model.SetTitle}','{UserInfo.User_id}',TO_DATE(SYSDATE),'N',
                            '{UserInfo.company_code}','{UserInfo.branch_code}','{model.SetType}')";
                    int row = this._objectEntity.ExecuteSqlCommand(setInsertQuery);
                    if (row <= 0)
                        throw new Exception("Set cannot be saved");

                    //inserting the questions
                    foreach (var Q in model.Questions)
                    {
                        int QACode = this._objectEntity.SqlQuery<int>("SELECT NVL(MAX(TO_NUMBER(QA_CODE))+1, 1) MAXID FROM DIST_QA_MASTER").FirstOrDefault();
                        string QuestionInsertQuery = $@"INSERT INTO DIST_QA_MASTER (QA_CODE,QA_TYPE,SET_CODE,QUESTION,CREATED_BY,CREATED_DATE,DELETED_FLAG,COMPANY_CODE,BRANCH_CODE)
                            VALUES('{QACode}','{Q.Type}','{setCode}','{Q.Question}','{UserInfo.User_id}',TO_DATE(SYSDATE),'N','{UserInfo.company_code}','{UserInfo.branch_code}')";
                        row = this._objectEntity.ExecuteSqlCommand(QuestionInsertQuery);
                        if (row <= 0)
                            throw new Exception("Question cannot be saved");

                        //inserting answers if type is MCR or MCC
                        if (Q.Type.Trim().ToUpper() == "MCR" || Q.Type.Trim().ToUpper() == "MCC")
                        {
                            var ans = Q.Answer.Split(',').ToList();
                            foreach (var a in ans)
                            {
                                var QA_SN = this._objectEntity.SqlQuery<int>("SELECT NVL(MAX(TO_NUMBER(SNO))+1, 1) MAXID FROM DIST_QA_DETAIL").FirstOrDefault();
                                string AnsInsertQuery = $@"INSERT INTO DIST_QA_DETAIL(QA_CODE,SNO,ANSWERS,CREATED_BY,CREATED_DATE,DELETED_FLAG,COMPANY_CODE,BRANCH_CODE)
                                VALUES('{QACode}','{QA_SN}','{a}','{UserInfo.User_id}',TO_DATE(SYSDATE),'N','{UserInfo.company_code}','{UserInfo.branch_code}')";
                                row = this._objectEntity.ExecuteSqlCommand(AnsInsertQuery);
                                if (row <= 0)
                                    throw new Exception("Answer cannot be saved");
                            }
                        }
                    }
                    trans.Commit();
                    output = "success";
                }
                catch (Exception ex)
                {
                    trans.Rollback();
                    output = ex.Message;
                }
            }
            return output;
        }

        public string AddTabularQuestions(List<TabularModel> model, User UserInfo)
        {
            var result = "";

            using (var trans = this._objectEntity.Database.BeginTransaction())
            {
                try
                {
                    foreach (var tab in model)
                    {
                        string tableId;
                        //insert table first
                        if (string.IsNullOrWhiteSpace(tab.SetId))
                            tableId = this._objectEntity.SqlQuery<int>("SELECT nvl(max(TABLE_ID),1) +1 TABLE_ID FROM DIST_QA_TAB_TABLE").FirstOrDefault().ToString();
                        else
                        {
                            tableId = tab.SetId;
                            var answers = this._objectEntity.SqlQuery<object>($"SELECT * FROM DIST_QA_TAB_CELL_ANSWER WHERE CELL_ID IN (SELECT CELL_ID FROM DIST_QA_TAB_CELL WHERE TABLE_ID='{tableId}')").ToList();
                            if (answers.Count > 0)
                                throw new Exception("Answered");
                            //delete table
                            var drow = this._objectEntity.ExecuteSqlCommand($"DELETE FROM DIST_QA_TAB_CELL WHERE TABLE_ID='{tableId}'");
                            //delete table cells
                            if (drow > 0)
                                drow = this._objectEntity.ExecuteSqlCommand($"DELETE FROM DIST_QA_TAB_TABLE WHERE TABLE_ID='{tableId}' AND COMPANY_CODE='{UserInfo.company_code}'");
                        }

                        string tableInsertQuery = $@"INSERT INTO DIST_QA_TAB_TABLE(TABLE_ID,TABLE_TITLE,CREATED_DATE,DELETED_FLAG,COMPANY_CODE,BRANCH_CODE,QA_TYPE)
                            VALUES('{tableId}','{tab.SetTitle}',TO_DATE(SYSDATE),'N','{UserInfo.company_code}','{UserInfo.branch_code}','{tab.SetType}')";
                        var row = this._objectEntity.ExecuteSqlCommand(tableInsertQuery);
                        if (row <= 0)
                            throw new Exception("Cannot save table " + tab.SetTitle);

                        //inserting cell data
                        for (int i = 0; i < tab.Cells.Count; i++)
                        {
                            for (int j = 0; j < tab.Cells[i].Count; j++)
                            {
                                var curCell = tab.Cells[i][j];
                                curCell.Label = (curCell.Type.Trim().ToUpper() == "LBL" || curCell.Type.Trim().ToUpper() == "DDL" || curCell.Type.Trim().ToUpper() == "RDB") ? curCell.Label : "";
                                var cellId = this._objectEntity.SqlQuery<int>("SELECT nvl(max(CELL_ID),1) +1 CELL_ID FROM DIST_QA_TAB_CELL").FirstOrDefault();
                                string cellInsertQuery = $@"INSERT INTO DIST_QA_TAB_CELL(CELL_ID,TABLE_ID,ROW_NO,CELL_NO,CELL_TYPE,CELL_LABEL)
                                    VALUES('{cellId}','{tableId}','{i}','{j}','{curCell.Type}','{curCell.Label}')";
                                row = this._objectEntity.ExecuteSqlCommand(cellInsertQuery);
                                if (row <= 0)
                                    throw new Exception("Cannot save cells of table " + tab.SetTitle);
                            }
                        }
                    }
                    trans.Commit();
                    result = "success";
                }
                catch (Exception ex)
                {
                    trans.Rollback();
                    result = ex.Message;
                }
            }
            return result;
        }

        public QuestionListModel GetAllQuestions(User UserInfo)
        {
            var result = new QuestionListModel();

            //General Questions
            string GeneralSetQuery = $@"SELECT TO_CHAR(SET_CODE) SetId,TITLE SetTitle,
                CASE QA_TYPE
                    WHEN 'G' THEN 'General'
                    WHEN 'B' THEN 'Branding'
                    WHEN 'S' THEN 'Scheme'
                    WHEN 'w' THEN 'Web'
                    WHEN 'O' THEN 'Other'
                END SetType
                FROM DIST_QA_SET WHERE DELETED_FLAG='N' AND COMPANY_CODE='{UserInfo.company_code}'";
            var Generals = _objectEntity.SqlQuery<QuestionSetupModel>(GeneralSetQuery).ToList();
            for (int i = 0; i < Generals.Count; i++)
            {
                string DetailQuery = $@"SELECT QM.QA_TYPE Type , QM.QUESTION,QD.ANSWER
                     FROM DIST_QA_MASTER QM
                    LEFT JOIN (SELECT QA_CODE, LISTAGG(ANSWERS, ',') WITHIN GROUP (ORDER BY QA_CODE) AS Answer
                    FROM   DIST_QA_DETAIL
                    GROUP BY QA_CODE) QD on QM.QA_CODE=QD.QA_CODE
                    WHERE QM.COMPANY_CODE='{UserInfo.Company}' AND QM.DELETED_FLAG ='N'
                    AND QM.SET_CODE='{Generals[i].SetId}'
                    ORDER BY QM.CREATED_DATE DESC";
                Generals[i].Questions = _objectEntity.SqlQuery<GeneralQuestionModel>(DetailQuery).ToList();
            }
            result.General = Generals;

            //Tabular questions
            string TabularSetQuery = $@"SELECT TO_CHAR(TABLE_ID) SetId,TABLE_TITLE SetTitle,
                CASE QA_TYPE
                    WHEN 'G' THEN 'General'
                    WHEN 'B' THEN 'Branding'
                    WHEN 'S' THEN 'Scheme'
                    WHEN 'w' THEN 'Web'
                    WHEN 'O' THEN 'Other'
                END SetType
                FROM DIST_QA_TAB_TABLE WHERE DELETED_FLAG='N' AND COMPANY_CODE='{UserInfo.company_code}'";
            var Tabulars = _objectEntity.SqlQuery<TabularModel>(TabularSetQuery).ToList();
            for (int i = 0; i < Tabulars.Count; i++)
            {
                int rows = _objectEntity.SqlQuery<int>($"SELECT TO_NUMBER(NVL(MAX(ROW_NO),0))FROM DIST_QA_TAB_CELL WHERE TABLE_ID='{Tabulars[i].SetId}'").FirstOrDefault();
                for (int j = 0; j <= rows; j++)
                {
                    string CellsQuery = $@"SELECT TRIM(CELL_TYPE) Type,CELL_LABEL Label FROM DIST_QA_TAB_CELL
                            WHERE TABLE_ID='{Tabulars[i].SetId}'
                            AND ROW_NO='{j}'
                            ORDER BY CELL_NO";
                    var Cells = this._objectEntity.SqlQuery<CellModel>(CellsQuery).ToList();
                    Tabulars[i].Cells.Add(Cells);
                }
            }
            result.Tabular = Tabulars;
            return result;
        }

        public QuestionSetupModel GetGeneralBySetID(string setId, User UserInfo)
        {
            string GeneralSetQuery = $"SELECT TO_CHAR(SET_CODE) SETID,TITLE SETTITLE,QA_TYPE SETTYPE,IS_ACTIVE ISACTIVE FROM DIST_QA_SET WHERE DELETED_FLAG='N' AND COMPANY_CODE='{UserInfo.company_code}' AND SET_CODE='{setId}'";
            var Generals = _objectEntity.SqlQuery<QuestionSetupModel>(GeneralSetQuery).FirstOrDefault();
            if (Generals == null)
                Generals = new QuestionSetupModel();
            else
            {
                string DetailQuery = $@"SELECT QM.QA_TYPE Type , QM.QUESTION,QD.ANSWER
                     FROM DIST_QA_MASTER QM
                    LEFT JOIN (SELECT QA_CODE, LISTAGG(ANSWERS, ',') WITHIN GROUP (ORDER BY QA_CODE) AS Answer
                    FROM   DIST_QA_DETAIL
                    GROUP BY QA_CODE) QD on QM.QA_CODE=QD.QA_CODE
                    WHERE QM.COMPANY_CODE='{UserInfo.Company}'
                    AND QM.SET_CODE='{Generals.SetId}'
                    ORDER BY QM.CREATED_DATE DESC";
                Generals.Questions = _objectEntity.SqlQuery<GeneralQuestionModel>(DetailQuery).ToList();
            }
            return Generals;
        }
        public TabularModel GetTabularBySetID(string setId, User UserInfo)
        {
            string TabularSetQuery = $"SELECT TO_CHAR(TABLE_ID) SetId,TABLE_TITLE SetTitle,QA_TYPE SETTYPE,IS_ACTIVE ISACTIVE FROM DIST_QA_TAB_TABLE WHERE DELETED_FLAG='N' AND COMPANY_CODE='{UserInfo.company_code}' AND TABLE_ID='{setId}'";
            var Tabulars = _objectEntity.SqlQuery<TabularModel>(TabularSetQuery).FirstOrDefault();
            if (Tabulars == null)
            {
                Tabulars = new TabularModel();
            }
            else
            {
                int rows = _objectEntity.SqlQuery<int>($"SELECT TO_NUMBER(NVL(MAX(ROW_NO),0))FROM DIST_QA_TAB_CELL WHERE TABLE_ID='{Tabulars.SetId}'").FirstOrDefault();
                for (int j = 0; j <= rows; j++)
                {
                    string CellsQuery = $@"SELECT TRIM(CELL_TYPE) Type,CELL_LABEL Label FROM DIST_QA_TAB_CELL
                            WHERE TABLE_ID='{Tabulars.SetId}'
                            AND ROW_NO='{j}'
                            ORDER BY CELL_NO";
                    var Cells = this._objectEntity.SqlQuery<CellModel>(CellsQuery).ToList();
                    Tabulars.Cells.Add(Cells);
                }
            }
            return Tabulars;
        }

        public List<QuestionSetModel> GetAllQuestionSets(User userInfo)
        {
            var query = $@"SELECT SET_CODE,TITLE ||'(General)' TITLE,'G' TYPE FROM DIST_QA_SET WHERE QA_TYPE = 'G' AND COMPANY_CODE = '{userInfo.company_code}'           UNION ALL
                SELECT SET_CODE,TITLE ||'(w)''(General)' TITLE,'w' TYPE FROM DIST_QA_SET WHERE QA_TYPE = 'w' AND COMPANY_CODE = '{userInfo.company_code}'             UNION ALL
                SELECT TABLE_ID SET_CODE,TABLE_TITLE ||'(Tabular)' TITLE,'T' TYPE FROM DIST_QA_TAB_TABLE  WHERE QA_TYPE = 'G' AND COMPANY_CODE = '{userInfo.company_code}'";
            var qry = $@" SELECT SET_CODE, 
                                     CASE WHEN QA_TYPE='w' 
                                        THEN TITLE ||'(w) (General)'
                                         ELSE TITLE ||'(General)' 
                                        END AS TITLE ,'G' TYPE
                                        FROM DIST_QA_SET WHERE COMPANY_CODE = '{userInfo.company_code}'   
                                        UNION ALL
                                     SELECT TABLE_ID SET_CODE,TABLE_TITLE ||'(Tabular)' TITLE,'T' TYPE FROM DIST_QA_TAB_TABLE  WHERE COMPANY_CODE = '{userInfo.company_code}'";

            var data = _objectEntity.SqlQuery<QuestionSetModel>(qry).ToList();
            return data;
        }

        public string SaveSurvey(SurveyModel model, User userInfo)
        {
            var result = "";
            using (var trans = _objectEntity.Database.BeginTransaction())
            {
                try
                {
                    var row = 0;
                    var surveyCode = 0;
                    if (model.SURVEY_CODE == 0)
                    {
                        surveyCode = _objectEntity.SqlQuery<int>($"SELECT NVL(MAX(SURVEY_CODE),0)+1 FROM DIST_QA_SET_SALESPERSON_MAP WHERE COMPANY_CODE = '{userInfo.company_code}'").FirstOrDefault();
                    }
                    else
                    {
                        row = _objectEntity.ExecuteSqlCommand($"DELETE FROM DIST_QA_SET_SALESPERSON_MAP WHERE SURVEY_CODE = '{model.SURVEY_CODE}' AND COMPANY_CODE = '{userInfo.company_code}'");
                        row = _objectEntity.ExecuteSqlCommand($"DELETE FROM DIST_QA_SURVEY_MAP WHERE SURVEY_ID = '{model.SURVEY_CODE}'");
                        surveyCode = model.SURVEY_CODE;
                    }
                    foreach (var spcode in model.SP_CODES)
                    {
                        var query = $@"INSERT INTO DIST_QA_SET_SALESPERSON_MAP (SURVEY_CODE,SURVEY_EDESC,SP_CODE,AREA_CODES,GROUP_CODES,CREATED_BY,COMPANY_CODE,BRANCH_CODE,EXPIRY_DATE)
                                        VALUES('{surveyCode}','{model.SURVEY_EDESC}','{spcode}','{model.AREA_CODES}','{model.GROUP_CODES}','{userInfo.User_id}','{userInfo.company_code}','{userInfo.branch_code}','{model.EXPIRY_DATE.ToString("dd-MMM-yyyy")}')";
                        row = _objectEntity.ExecuteSqlCommand(query);
                    }

                    foreach (var ques in model.QUESTIONS)
                    {
                        var query = $@"INSERT INTO DIST_QA_SURVEY_MAP (SURVEY_ID,SET_ID,SET_TYPE) VALUES('{surveyCode}','{ques.SET_CODE}','{ques.TYPE}')";
                        row = _objectEntity.ExecuteSqlCommand(query);
                    }
                    trans.Commit();
                    result = "200";
                }
                catch (Exception ex)
                {
                    trans.Rollback();
                    result = "500";
                }
            }
            return result;
        }

        public List<SurveyModel> GetSurveyList(User userInfo)
        {
            var query = $@"SELECT SURVEY_CODE,WM_CONCAT(DISTINCT SP_CODE) SP_CODE_STR,SURVEY_EDESC, AREA_CODES, GROUP_CODES,
            REPLACE(WM_CONCAT(DISTINCT TITLE),',',',<br/>') TITLE,EXPIRY_DATE,
            WM_CONCAT(DISTINCT SET_INFO) SET_INFO
            FROM (
                    SELECT DQSP.SP_CODE,DQSP.SURVEY_CODE,DQSP.SURVEY_EDESC,DQSP.EXPIRY_DATE,DQS.TITLE||'(General)' TITLE,DQSM.SET_ID||'-'||DQSM.SET_TYPE SET_INFO,
                                DQSP.AREA_CODES, DQSP.GROUP_CODES
                    FROM DIST_QA_SET_SALESPERSON_MAP DQSP
                    INNER JOIN DIST_QA_SURVEY_MAP DQSM ON DQSP.SURVEY_CODE = DQSM.SURVEY_ID AND DQSM.SET_TYPE = 'G'
                    INNER JOIN DIST_QA_SET DQS ON  DQSM.SET_ID = DQS.SET_CODE AND DQSP.COMPANY_CODE = DQS.COMPANY_CODE
                    WHERE DQSP.COMPANY_CODE = '{userInfo.company_code}'
                    UNION ALL
                    SELECT DQSP.SP_CODE,DQSP.SURVEY_CODE,DQSP.SURVEY_EDESC,DQSP.EXPIRY_DATE,DQT.TABLE_TITLE||'(Tabular)' TITLE,DQSM.SET_ID||'-'||DQSM.SET_TYPE SET_INFO,
                                DQSP.AREA_CODES, DQSP.GROUP_CODES
                    FROM DIST_QA_SET_SALESPERSON_MAP DQSP
                    INNER JOIN DIST_QA_SURVEY_MAP DQSM ON DQSP.SURVEY_CODE = DQSM.SURVEY_ID AND DQSM.SET_TYPE = 'T'
                    INNER JOIN DIST_QA_TAB_TABLE DQT ON  DQSM.SET_ID = DQT.TABLE_ID AND DQSP.COMPANY_CODE = DQT.COMPANY_CODE
                    WHERE DQSP.COMPANY_CODE = '{userInfo.company_code}')
            WHERE TRUNC(EXPIRY_DATE) >= TRUNC(SYSDATE)
            GROUP BY SURVEY_CODE,SURVEY_EDESC,EXPIRY_DATE,AREA_CODES,GROUP_CODES";
            var data = _objectEntity.SqlQuery<SurveyModel>(query).ToList();
            return data;
        }

        public List<UserSetupTreeModel> GetUserSetupTreeList(User UserInfo)
        {
            string Query = $@"SELECT GROUPID CODE, NULL MASTER_CODE, GROUP_EDESC NAME,'' PASSWORD,''  FULLNAME,'' EMPLOYEE_CODE,'' EMPLOYEE_EDESC,NULL ROLE_CODE,'' CONTACT_NO,'' ROLE_NAME,'' ATTENDENCE,'' MOBILE,'' EMAIL,'' AREA_CODE,'' AREA_NAME,'' CUSTOMER_CODE,
                            ''ITEM_CODE,
                            GROUPID,'Y' IS_GROUP, '' ACTIVE ,''BRANDING,'' SUPER_USER
                                FROM DIST_GROUP_MASTER
                                WHERE DELETED_FLAG = 'N' AND COMPANY_CODE IN('{UserInfo.company_code}')
                            UNION ALL
                              SELECT LU.USERID CODE,(CASE WHEN PARENT_USERID IS NULL THEN LU.GROUPID ELSE PARENT_USERID END) MASTER_CODE,
                                     USER_NAME NAME,PASS_WORD PASSWORD,FULL_NAME FULLNAME,LU.SP_CODE EMPLOYEE_CODE,ES.EMPLOYEE_EDESC,RU.ROLE_CODE,LU.CONTACT_NO,RM.ROLE_NAME,LU.ATTENDANCE ATTENDENCE,
                                     LU.IS_MOBILE MOBILE,LU.EMAIL,wm_concat (distinct UA.AREA_CODE) AREA_CODE,wm_concat (distinct AM.AREA_NAME) AREA_NAME,wm_concat(distinct ua.customer_code) customer_code,
                                    wm_concat(distinct UIM.ITEM_CODE) ITEM_CODE,
                                LU.GROUPID,'N' IS_GROUP,LU.ACTIVE,LU.BRANDING,LU.SUPER_USER
                            FROM DIST_LOGIN_USER LU,
                                     HR_EMPLOYEE_SETUP ES,DIST_ROLE_USER RU,DIST_ROLE_MASTER_SETUP RM,DIST_USER_AREAS UA,DIST_AREA_MASTER AM
                                    ,DIST_USER_ITEM_MAPPING UIM
                                     WHERE 1 = 1
                                     AND LU.COMPANY_CODE = ES.COMPANY_CODE(+)
                                     AND LU.SP_CODE = ES.EMPLOYEE_CODE(+)
                                     AND LU.USERID = RU.USERID(+)
                                     AND RU.ROLE_CODE = RM.ROLE_CODE(+)
                                     AND LU.USERID = UA.USER_ID(+)
                                     AND LU.SP_CODE = UA.SP_CODE(+)
                                     AND LU.COMPANY_CODE = UA.COMPANY_CODE(+)
                                     AND UA.AREA_CODE = AM.AREA_CODE(+)
                                    -- AND LU.COMPANY_CODE = AM.COMPANY_CODE(+)
                                     AND LU.COMPANY_CODE IN ('{UserInfo.company_code}')
                                     AND LU.ACTIVE = 'Y'
                                     AND LU.USERID = UIM.USER_ID(+)
                                     AND LU.SP_CODE = UIM.SP_CODE(+)
                                    AND LU.COMPANY_CODE = UIM.COMPANY_CODE--(+)
                                     AND lu.USERID <> 1
                            GROUP BY LU.USERID,USER_NAME,PASS_WORD,FULL_NAME,LU.SP_CODE,ES.EMPLOYEE_EDESC,RU.ROLE_CODE,LU.CONTACT_NO,RM.ROLE_NAME,LU.ATTENDANCE,LU.IS_MOBILE,LU.EMAIL,
                                     PARENT_USERID,LU.GROUPID,LU.ACTIVE,LU.BRANDING,LU.SUPER_USER";
            var list = _objectEntity.SqlQuery<UserSetupTreeModel>(Query).ToList();
            return list;
        }

        public List<UserRoleModel> GetDistUserRole(User userInfo)
        {
            string query = $@"SELECT DISTINCT ROLE_CODE,ROLE_NAME FROM DIST_ROLE_MASTER_SETUP";
            var data = _objectEntity.SqlQuery<UserRoleModel>(query).ToList();
            return data;
        }


        public string SaveUserTree(UserSetupTreeModel model, User userInfo)
        {
            using (var transaction = _objectEntity.Database.BeginTransaction())
            {
                try
                {

                    //   var maxid = _objectEntity.SqlQuery<UserSetupTreeModel>($@"select GROUPID from dist_group_master WHERE COMPANY_CODE ='{userInfo.company_code}'");

                    //previous one with company filter 
                    //string maxUserId = $"SELECT COALESCE(MAX(USERID), 100) + 1 FROM DIST_LOGIN_USER WHERE COMPANY_CODE = {userInfo.company_code}";

                    //new one without company filter
                    string maxUserId = $"SELECT COALESCE(MAX(USERID), 100) + 1 FROM DIST_LOGIN_USER";

                    model.CODE = _objectEntity.SqlQuery<int>(maxUserId).FirstOrDefault();
                    var Obj = _objectEntity.SqlQuery<string>($"SELECT USER_NAME FROM DIST_LOGIN_USER WHERE SP_CODE='{model.EMPLOYEE_CODE}'").FirstOrDefault();
                    if (!string.IsNullOrWhiteSpace(Obj))
                        return "EXISTS";
                    var roleCode = string.Empty;
                    if (model.ROLE_CODE == 2)
                    {
                        roleCode = "D";
                    }
                    else
                    {
                        roleCode = "S";
                    }
                    //Unique for group ID

                    string Query = $@"INSERT INTO DIST_LOGIN_USER (USERID,USER_NAME,PASS_WORD,FULL_NAME,SP_CODE,CONTACT_NO,USER_TYPE,CREATED_BY,CREATED_DATE,EXPIRY_DATE,COMPANY_CODE,BRANCH_CODE,ATTENDANCE,GROUPID,PARENT_USERID,IS_MOBILE,EMAIL,ACTIVE,BRANDING,SUPER_USER)
                                VALUES ({model.CODE},'{model.NAME}','{model.PASSWORD}','{model.FULLNAME}','{model.EMPLOYEE_CODE}', '{model.CONTACT_NO}','{roleCode}','{userInfo.login_code}',TRUNC(SYSDATE),TRUNC(ADD_MONTHS(SYSDATE, 24)) ,'{userInfo.company_code}',
                                        '{userInfo.branch_code}','{model.ATTENDENCE}','{model.MASTER_CODE}','{model.MASTER_CUSTOMER_CODE}', '{model.MOBILE}','{model.EMAIL}','{model.ACTIVE}','{model.BRANDING}','{model.SUPER_USER}')";
                    _objectEntity.ExecuteSqlCommand(Query);

                    Query = $@"INSERT INTO DIST_ROLE_USER (USERID,ROLE_CODE,CREATED_BY,CREATED_DATE,COMPANY_CODE)
                                VALUES ({model.CODE},'{model.ROLE_CODE}','{userInfo.User_id}',TRUNC(SYSDATE) ,'{userInfo.company_code}')";
                    _objectEntity.ExecuteSqlCommand(Query);

                    Query = $@"INSERT INTO DIST_SALESPERSON_MASTER (SP_CODE,COMPANY_CODE,BRANCH_CODE)
                                VALUES ({model.EMPLOYEE_CODE},'{userInfo.company_code}','{userInfo.branch_code}')";
                    _objectEntity.ExecuteSqlCommand(Query);

                    foreach (var area in model.AREA)
                    {
                        foreach(var customer in model.CUSTOMER)
                        {
                            Query = $@"INSERT INTO DIST_USER_AREAS (SP_CODE,AREA_CODE,USER_ID,CREATED_BY,CREATED_DATE,COMPANY_CODE,CUSTOMER_CODE)
                                VALUES ('{model.EMPLOYEE_CODE}','{area}','{model.CODE}','{userInfo.login_code}',TRUNC(SYSDATE),'{userInfo.company_code}','{customer}')";
                            _objectEntity.ExecuteSqlCommand(Query);
                        }
                    }
                    foreach (var item in model.ITEMS)
                    {
                        Query = $@"INSERT INTO DIST_USER_ITEM_MAPPING (SP_CODE,ITEM_CODE,USER_ID,CREATED_BY,CREATED_DATE,COMPANY_CODE)
                                VALUES ('{model.EMPLOYEE_CODE}','{item}','{model.CODE}','{userInfo.login_code}',TRUNC(SYSDATE),'{userInfo.company_code}')";
                        _objectEntity.ExecuteSqlCommand(Query);
                    }


                    _objectEntity.SaveChanges();
                    transaction.Commit();

                    return "200";
                }
                catch (Exception ex)
                {

                    transaction.Rollback();
                    //if (ex.Message == "ORA-00001: unique constraint (JGI7374.UK_DIST_LOGIN_USR_USR_NAME) violated")
                    //    return "UserName Already Exist";
                    return $"Error{ex.Message}";
                }
            }
        }


        public string UpdateUserTree(UserSetupTreeModel model, User userInfo)
        {
            try
            {
                string Query = $@"UPDATE DIST_LOGIN_USER 
                                    SET USER_NAME = '{model.NAME}',PASS_WORD='{model.PASSWORD}',FULL_NAME='{model.FULLNAME}',
                                        SP_CODE='{model.EMPLOYEE_CODE}',CONTACT_NO='{model.CONTACT_NO}',ATTENDANCE='{model.ATTENDENCE}',ACTIVE = '{model.ACTIVE}', BRANDING ='{model.BRANDING}',
                                        IS_MOBILE ='{model.MOBILE}',EMAIL = '{model.EMAIL}',SUPER_USER = '{model.SUPER_USER}'
                                    WHERE USERID={model.CODE}";
                _objectEntity.ExecuteSqlCommand(Query);


                Query = $@"UPDATE DIST_ROLE_USER 
                                  SET ROLE_CODE = '{model.ROLE_CODE}'
                                  WHERE USERID = '{model.CODE}'";
                int x = _objectEntity.ExecuteSqlCommand(Query);
                if (x == 0)
                {
                    Query = $@"INSERT INTO DIST_ROLE_USER (USERID,ROLE_CODE,CREATED_BY,CREATED_DATE,COMPANY_CODE)
                                VALUES ({model.CODE},'{model.ROLE_CODE}','{userInfo.User_id}',TRUNC(SYSDATE) ,'{userInfo.company_code}')";
                    _objectEntity.ExecuteSqlCommand(Query);
                }

                Query = $@"DELETE DIST_USER_AREAS WHERE USER_ID='{model.CODE}' AND SP_CODE = '{model.EMPLOYEE_CODE}'";
                _objectEntity.ExecuteSqlCommand(Query);
                foreach (var area in model.AREA)
                {
                    foreach (var customer in model.CUSTOMER)
                    { 
                        Query = $@"INSERT INTO DIST_USER_AREAS (SP_CODE,AREA_CODE,USER_ID,CREATED_BY,CREATED_DATE,COMPANY_CODE,CUSTOMER_CODE)
                                VALUES ('{model.EMPLOYEE_CODE}','{area}','{model.CODE}','{userInfo.login_code}',TRUNC(SYSDATE),'{userInfo.company_code}','{customer}')";
                    _objectEntity.ExecuteSqlCommand(Query);
                    }
                }

                Query = $@"DELETE FROM DIST_USER_ITEM_MAPPING WHERE USER_ID='{model.CODE}' AND SP_CODE = '{model.EMPLOYEE_CODE}'";
                _objectEntity.ExecuteSqlCommand(Query);
                foreach (var item in model.ITEMS)
                {
                    Query = $@"INSERT INTO DIST_USER_ITEM_MAPPING (SP_CODE,ITEM_CODE,USER_ID,CREATED_BY,CREATED_DATE,COMPANY_CODE)
                                VALUES ('{model.EMPLOYEE_CODE}','{item}','{model.CODE}','{userInfo.login_code}',TRUNC(SYSDATE),'{userInfo.company_code}')";
                    _objectEntity.ExecuteSqlCommand(Query);
                }

                _objectEntity.SaveChanges();


                return "200";
            }
            catch (Exception)
            {
                return "500";
            }

        }

        public string DeleteUserTree(string code, User userInfo)
        {
            try
            {
                string Query = $@"UPDATE DIST_LOGIN_USER 
                                    SET ACTIVE = 'N'
                                    WHERE USERID={code} AND COMPANY_CODE='{userInfo.company_code}'";
                int COUNT = _objectEntity.ExecuteSqlCommand(Query);
                _objectEntity.SaveChanges();
                if (COUNT == 0)
                    return "403";

                return "200";
            }
            catch (Exception)
            {
                return "500";
            }
        }

        public string UpdateUserTreeOrder(UserSetupTreeModel model)
        {
            try
            {
                string Query = $@"UPDATE DIST_LOGIN_USER 
                                    SET PARENT_USERID = '{model.MASTER_CODE}',GROUPID = '{model.GROUPID}'
                                    WHERE USERID={model.CODE}";
                _objectEntity.ExecuteSqlCommand(Query);
                _objectEntity.SaveChanges();


                return "200";
            }
            catch (Exception)
            {
                return "500";
            }

        }

        public string SaveOpeningStockSetup(OpeningDetailModel model, User userInfo)
        {
            string result = "";
            string CreatedDate = "SYSDATE";
            int StockId = 0;
            using (var trans = _objectEntity.Database.BeginTransaction())
            {
                try
                {
                    if (model.OpeningStockId == 0)
                    {
                        StockId = _objectEntity.SqlQuery<int>("SELECT (NVL(MAX(OPENING_STOCK_ID),0)+1) MAXID FROM DIST_DISTRIBUTOR_OPENING_STOCK").FirstOrDefault();
                        result = "ADDED";
                    }
                    else
                    {
                        var CDate = _objectEntity.SqlQuery<DateTime>($"SELECT CREATED_DATE FROM DIST_DISTRIBUTOR_OPENING_STOCK WHERE OPENING_STOCK_ID='{model.OpeningStockId}' AND DISTRIBUTOR_CODE='{model.DistributerCode}'").FirstOrDefault();
                        CreatedDate = $"TO_DATE('{CDate.ToString("MM/dd/yyyy HH:mm:ss")}','MM/DD/RRRR HH24:MI:SS')";
                        var DeletedRows = _objectEntity.ExecuteSqlCommand($"DELETE FROM DIST_DISTRIBUTOR_OPENING_STOCK WHERE OPENING_STOCK_ID='{model.OpeningStockId}' AND DISTRIBUTOR_CODE='{model.DistributerCode}'");
                        if (DeletedRows > 0)
                        {
                            StockId = model.OpeningStockId;
                            result = "UPDATED";
                        }
                    }
                    foreach (var item in model.OpeningList)
                    {
                        string query = $@"INSERT INTO DIST_DISTRIBUTOR_OPENING_STOCK(OPENING_STOCK_ID,DISTRIBUTOR_CODE,ITEM_CODE,MU_CODE,CURRENT_STOCK,CREATED_DATE,COMPANY_CODE,BRANCH_CODE) 
                                    VALUES ('{StockId}','{model.DistributerCode}','{item.ITEM_CODE}','{item.MU_CODE}','{item.CURRENT_STOCK}',{CreatedDate},'{userInfo.company_code}','{userInfo.branch_code}')";
                        var COUNT = _objectEntity.ExecuteSqlCommand(query);
                        if (COUNT <= 0)
                            throw new Exception("Cannot insert");
                    }
                    trans.Commit();
                }
                catch (Exception)
                {
                    trans.Rollback();
                    result = "ERROR";
                }
            }
            return result;
        }

        public List<OpeningDetailModel> GetOpeningStock(User userInfo)
        {
            var Result = new List<OpeningDetailModel>();
            var customerFilter = string.Empty;
            customerFilter = userInfo.LoginType == "Distributor" ? $"AND OS.DISTRIBUTOR_CODE='{userInfo.DistributerNo}'" : "";
            var Query = $@"SELECT OS.OPENING_STOCK_ID,OS.DISTRIBUTOR_CODE,CS.CUSTOMER_EDESC DISTRIBUTOR_EDESC,CS.REGD_OFFICE_EADDRESS ADDRESS,
                        OS.ITEM_CODE,IMS.ITEM_EDESC,OS.MU_CODE,OS.CURRENT_STOCK,OS.CREATED_DATE,
                        OS.COMPANY_CODE,OS.BRANCH_CODE
                    FROM DIST_DISTRIBUTOR_OPENING_STOCK OS
                    INNER JOIN SA_CUSTOMER_SETUP CS ON OS.DISTRIBUTOR_CODE=CS.CUSTOMER_CODE AND OS.COMPANY_CODE=CS.COMPANY_CODE
                    INNER JOIN IP_ITEM_MASTER_SETUP IMS ON OS.ITEM_CODE=IMS.ITEM_CODE AND OS.COMPANY_CODE=IMS.COMPANY_CODE
                    WHERE OS.COMPANY_CODE='{userInfo.company_code}' {customerFilter}";
            var data = _objectEntity.SqlQuery<OpeningStockSetupModel>(Query).GroupBy(x => x.OPENING_STOCK_ID);
            foreach (var group in data)
            {
                var lst = group.ToList();
                Result.Add(new OpeningDetailModel
                {
                    OpeningStockId = group.Key,
                    DistributerCode = lst[0].DISTRIBUTOR_CODE,
                    DistributerName = lst[0].DISTRIBUTOR_EDESC,
                    DistributerAddress = lst[0].ADDRESS,
                    StockDate = lst[0].CREATED_DATE,
                    OpeningList = lst
                });
            }
            return Result;
        }

        public string CreateClosingStock(ClosingStock model, User userInfo)
        {
            using (var transaction = _objectEntity.Database.BeginTransaction())
            {
                try
                {
                    var StockId = _objectEntity.SqlQuery<int>("SELECT (NVL(MAX(STOCK_ID),0)+1) MAXID FROM DIST_DISTRIBUTOR_STOCK").FirstOrDefault();
                    string query = string.Empty;
                    for (int i = 0; i < model.OS_Details.Count(); i++)
                    {
                        query = $@"INSERT INTO DIST_DISTRIBUTOR_STOCK(STOCK_ID,DISTRIBUTOR_CODE,ITEM_CODE,MU_CODE,CURRENT_STOCK,SP_CODE,DELETED_FLAG,BRANCH_CODE,COMPANY_CODE,CREATED_DATE) VALUES('{StockId}','{model.DistCustomer}','{model.OS_Details[i].ITEM_CODE}','{model.OS_Details[i].MU_CODE}','{model.OS_Details[i].Current_STOCK}','0','N','{userInfo.branch_code}','{userInfo.company_code}',TO_DATE(SYSDATE))";
                        _objectEntity.ExecuteSqlCommand(query);
                    }
                    transaction.Commit();
                    return "200";
                }
                catch (Exception)
                {
                    transaction.Rollback();
                    return "500";
                }
            }
        }

        public string UpdateClosingStock(ClosingStock model, User userInfo)
        {

            string CreatedDate = "SYSDATE";
            using (var trans = _objectEntity.Database.BeginTransaction())
            {
                try
                {
                    var CDate = _objectEntity.SqlQuery<DateTime>($"SELECT CREATED_DATE FROM DIST_DISTRIBUTOR_STOCK WHERE STOCK_ID='{model.STOCK_ID}' AND DISTRIBUTOR_CODE='{model.DistCustomer}'").FirstOrDefault();
                    CreatedDate = $"TO_DATE('{CDate.ToString("MM/dd/yyyy HH:mm:ss")}','MM/DD/RRRR HH24:MI:SS')";
                    var DeletedRows = _objectEntity.ExecuteSqlCommand($"DELETE FROM DIST_DISTRIBUTOR_STOCK WHERE DISTRIBUTOR_CODE='{model.DistCustomer}' AND STOCK_ID='{model.STOCK_ID}'");
                    if (DeletedRows <= 0)
                    {
                        var StockId = _objectEntity.SqlQuery<int>("SELECT (NVL(MAX(STOCK_ID),0)+1) MAXID FROM DIST_DISTRIBUTOR_STOCK").FirstOrDefault();
                        model.STOCK_ID = StockId;
                    }
                    string query = string.Empty;
                    for (int i = 0; i < model.OS_Details.Count(); i++)
                    {
                        query = $@"INSERT INTO DIST_DISTRIBUTOR_STOCK(STOCK_ID,DISTRIBUTOR_CODE,ITEM_CODE,MU_CODE,CURRENT_STOCK,SP_CODE,DELETED_FLAG,BRANCH_CODE,COMPANY_CODE,CREATED_DATE) VALUES('{model.STOCK_ID}','{model.DistCustomer}','{model.OS_Details[i].ITEM_CODE}','{model.OS_Details[i].MU_CODE}','{model.OS_Details[i].Current_STOCK}','0','N','{userInfo.branch_code}','{userInfo.company_code}',{CreatedDate})";
                        _objectEntity.ExecuteSqlCommand(query);
                    }
                    trans.Commit();
                    return "200";
                }
                catch (Exception)
                {
                    trans.Rollback();
                    return "500";
                }
            }
        }

        public List<ClosingStock> GetClosingStock(User userInfo, string DistId)
        {
            var Result = new List<ClosingStock>();
            var customerFilter = string.Empty;
            var FirstQuery = string.Empty;
            var distCode = string.Empty;
            var DistItem = string.Empty;
            var DistributorCode = DistId;
            if (!string.IsNullOrEmpty(DistId as string))
            {
                FirstQuery = @"INNER JOIN DIST_DISTRIBUTOR_ITEM DDI ON DDI.ITEM_CODE = IMS.ITEM_CODE AND DDI.DISTRIBUTOR_CODE = DS.DISTRIBUTOR_CODE AND DDI.COMPANY_CODE = IMS.COMPANY_CODE";
                distCode = $"AND DDI.distributor_code='{DistributorCode}'";
                DistItem = @"DDI.ITEM_CODE";

            }

            else
            {
                DistItem = @"DS.ITEM_CODE";
            }


            customerFilter = userInfo.LoginType == "Distributor" ? $"AND DS.DISTRIBUTOR_CODE='{userInfo.DistributerNo}'" : "";
            var Query = $@"SELECT DS.STOCK_ID,DS.DISTRIBUTOR_CODE,CS.CUSTOMER_EDESC DISTRIBUTOR_EDESC,CS.REGD_OFFICE_EADDRESS ADDRESS,
                        {DistItem},IMS.ITEM_EDESC,DS.MU_CODE,DS.CURRENT_STOCK,DS.CREATED_DATE,
                        DS.COMPANY_CODE,DS.BRANCH_CODE
                    FROM    DIST_DISTRIBUTOR_STOCK DS
                    INNER JOIN SA_CUSTOMER_SETUP CS ON DS.DISTRIBUTOR_CODE=CS.CUSTOMER_CODE AND DS.COMPANY_CODE=CS.COMPANY_CODE
                    INNER JOIN IP_ITEM_MASTER_SETUP IMS ON DS.ITEM_CODE=IMS.ITEM_CODE AND DS.COMPANY_CODE=IMS.COMPANY_CODE
                        {FirstQuery}
                    WHERE DS.COMPANY_CODE='{userInfo.company_code}' {customerFilter} {distCode}";
            var data = _objectEntity.SqlQuery<ClosingStockItem>(Query).GroupBy(x => x.STOCK_ID);


            foreach (var group in data)
            {
                var lst = group.ToList();
                Result.Add(new ClosingStock
                {
                    STOCK_ID = group.Key,
                    DistCustomer = lst[0].DISTRIBUTOR_CODE,
                    DistributerName = lst[0].DISTRIBUTOR_EDESC,
                    DistributerAddress = lst[0].ADDRESS,
                    StockDate = lst[0].CREATED_DATE,
                    OS_Details = lst
                });
            }

            return Result;
        }
        public List<itemList> GetDistChildItems(string distCode, User userInfo)
        {
            var selectQuery = $@"SELECT DS.STOCK_ID,DS.DISTRIBUTOR_CODE,CS.CUSTOMER_EDESC DISTRIBUTOR_EDESC,CS.REGD_OFFICE_EADDRESS ADDRESS,
                        DDI.ITEM_CODE,IMS.ITEM_EDESC,DS.MU_CODE,DS.CURRENT_STOCK,DS.CREATED_DATE,
                        DS.COMPANY_CODE,DS.BRANCH_CODE
                    FROM    DIST_DISTRIBUTOR_STOCK DS
                    INNER JOIN SA_CUSTOMER_SETUP CS ON DS.DISTRIBUTOR_CODE=CS.CUSTOMER_CODE AND DS.COMPANY_CODE=CS.COMPANY_CODE
                    INNER JOIN IP_ITEM_MASTER_SETUP IMS ON DS.ITEM_CODE=IMS.ITEM_CODE AND DS.COMPANY_CODE=IMS.COMPANY_CODE
                    INNER JOIN DIST_DISTRIBUTOR_ITEM DDI ON DDI.ITEM_CODE = IMS.ITEM_CODE AND DDI.DISTRIBUTOR_CODE = DS.DISTRIBUTOR_CODE AND DDI.COMPANY_CODE = IMS.COMPANY_CODE
                    WHERE DS.COMPANY_CODE='{userInfo.company_code}' AND DDI.distributor_code='{distCode}'";
            var data = _objectEntity.SqlQuery<itemList>(selectQuery).ToList();
            return data;

        }

        public List<DistQueryBuilderModel> GetDistributorList()
        {
            var selectQuery = $@"SELECT DDM.DISTRIBUTOR_CODE,SCS.CUSTOMER_CODE,SCS.CUSTOMER_EDESC FROM DIST_DISTRIBUTOR_MASTER DDM,SA_CUSTOMER_SETUP SCS
WHERE DDM.DISTRIBUTOR_CODE=SCS.CUSTOMER_CODE AND DDM.COMPANY_CODE=SCS.COMPANY_CODE AND DDM.ACTIVE = 'Y' AND DDM.DELETED_FLAG = 'N' order by scs.customer_edesc asc";

            var result = _objectEntity.SqlQuery<DistQueryBuilderModel>(selectQuery).ToList();
            return result;
        }


        public string SaveWidgets(CreateDistWidgetsModel modal, User userInfo)
        {
            try
            {
                var order = 0;
                int.TryParse(modal.OrderNo, out order);
                var maxValue = 0;
                int.TryParse(modal.MaxValue, out maxValue);
                var minValue = 0;
                int.TryParse(modal.MinValue, out minValue);
                var isactive = modal.Isactive ? "Y" : "N";
                var midIsblink = modal.IsMidBlink ? "Y" : "N";
                var maxIsblink = modal.IsMaxBlink ? "Y" : "N";
                var Isblink = modal.IsBlink ? "Y" : "N";
                var IsDistributorCheckedAll = modal.isDistributorChecked ? "ALL" : "None";
                //  var formatedQuery= modal.sqlQuery.Replace("''", "");
                var formatedQuery = modal.sqlQuery;

                //get the max id from database

                var MAXiD = _objectEntity.SqlQuery<int>(@"SELECT COUNT(ID)+1 AS MAX FROM DIST_QUICKCAP").FirstOrDefault();



                var insertQuery = string.Format(@"INSERT INTO DIST_QUICKCAP
                    (Id, QUICKCAP_NO, QUICKCAP_EDESC, DISTRIBUTOR_CODE, SQL_STATEMENT, QUICKCAP_TITLE, QUICKCAP_BGCOLOR, QUICKCAP_FONTCOLOR, QUICKCAP_ICON,DELETED_FLAG,ISACTIVE,ORDERNO,QUICKTYPE,MIDBGCOLOR,
                    MIDFONTCOLOR,MIDISBLINK,MAXBGCOLOR,MAXFONTCOLOR,MAXISBLINK,MAXVALUES,MINVALUES,MINISBLINK,LABELPOSITION,SPEEDOMETERMAXVALUE,MAXVALUEQUERY,MINVALUEQUERY,DISTRIBUTOR_CHECK_TYPE)
                    VALUES
                    ({0}, '{1}', '{2}', '{3}', '{4}', '{5}', '{6}', '{7}', '{8}','N','{9}','{10}','{11}','{12}','{13}','{14}','{15}','{16}','{17}','{18}','{19}','{20}','{21}','{22}','{23}','{24}','{25}')", MAXiD, 10, modal.widgetsName, modal.DISTRIBUTOR_CODE, formatedQuery, modal.widgetsTitle, modal.WidgetsBGColor, modal.widgetsColor, modal.widgetFontIcon, isactive, order
                    , modal.ChartType, modal.MidBGColor, modal.MidFontColor, midIsblink, modal.MaxBGColor, modal.MaxFontColor, maxIsblink, maxValue, minValue, Isblink, modal.LABELPOSITION, modal.SPEEDOMETERMAXVALUE, modal.MAXVALUEQUERY, modal.MINVALUEQUERY, IsDistributorCheckedAll);


                _objectEntity.ExecuteSqlCommand(insertQuery);
                _objectEntity.SaveChanges();
                return "success";

            }
            catch (Exception ex)
            {
                return "failed";
            }
        }

        public List<DistTableList> GetDistTableList()
        {
            var selectTable = @"SELECT DISTINCT table_name AS TableName FROM user_tables where TABLE_NAME LIKE 'DIST_%'";
            var result = _objectEntity.SqlQuery<DistTableList>(selectTable).ToList();
            return result;
        }

        public List<DistTableColumn> GetColumnNameList(string tablesName)
        {
            var tableName = tablesName;
            var queryString = @"SELECT table_name as tableName, column_name as columnsName, data_type as dataType, data_length as dataLength,initcap(replace(column_name,'_',' ')) columnheader
                                FROM USER_TAB_COLUMNS
                                WHERE table_name = '" + tableName + "'";
            var columns = _objectEntity.SqlQuery<DistTableColumn>(queryString).ToList();
            return columns;
        }

        public List<NotificationModel> GetAllNotifications(User userInfo)
        {
            var query = $@"SELECT NOTIFICATION_ID,NOTIFICATION_TITLE,NOTIFICATION_TEXT,NOTIFICATION_TYPE,STATUS,WM_CONCAT(SP_CODE) SP_CODE
                        FROM DIST_NOTIFICATIONS
                        WHERE COMPANY_CODE='{userInfo.company_code}'
                              AND DELETED_FLAG = 'N'
                        GROUP BY NOTIFICATION_ID,NOTIFICATION_TITLE,NOTIFICATION_TEXT,NOTIFICATION_TYPE,STATUS";
            var data = _objectEntity.SqlQuery<NotificationModel>(query).ToList();
            return data;
        }

        public string SaveNotification(NotificationModel model, User userInfo)
        {
            var Query = string.Empty;
            if (model.SP_CODES.Count == 0)
                model.SP_CODES.Add("");
            else if (string.IsNullOrWhiteSpace(model.SP_CODES[0]))
                model.SP_CODES[0] = "";
            //int NotiId = 0;
            int row = 0;
            using (var trans = _objectEntity.Database.BeginTransaction())
            {
                try
                {
                    if (model.NOTIFICATION_ID > 0)
                    {
                        var delRows = _objectEntity.ExecuteSqlCommand($"DELETE FROM DIST_NOTIFICATIONS WHERE NOTIFICATION_ID='{model.NOTIFICATION_ID}'");
                    }
                    else
                        model.NOTIFICATION_ID = _objectEntity.SqlQuery<int>("SELECT NVL(MAX(NOTIFICATION_ID),0)+1 FROM DIST_NOTIFICATIONS").FirstOrDefault();
                    foreach (var spCode in model.SP_CODES)
                    {
                        Query = $@"INSERT INTO DIST_NOTIFICATIONS (NOTIFICATION_ID,NOTIFICATION_TITLE,NOTIFICATION_TEXT,NOTIFICATION_TYPE,SP_CODE,STATUS,COMPANY_CODE,BRANCH_CODE)
                            VALUES({model.NOTIFICATION_ID},'{model.NOTIFICATION_TITLE}','{model.NOTIFICATION_TEXT}','{model.NOTIFICATION_TYPE}','{spCode}','{model.STATUS}','{userInfo.company_code}','{userInfo.branch_code}')";

                        row += _objectEntity.ExecuteSqlCommand(Query);
                    }
                    trans.Commit();
                }
                catch
                {
                    row = 0;
                    trans.Rollback();
                }
                if (model.SP_CODES[0] == "")
                    model.SP_CODES.RemoveAt(0);

                if (row > 0)
                    return "200";
                else
                    return "500";
            }
        }

        public int DeleteNotification(string id)
        {
            var rows = _objectEntity.ExecuteSqlCommand($"UPDATE DIST_NOTIFICATIONS SET DELETED_FLAG='Y' WHERE NOTIFICATION_ID='{id}'");
            return rows;
        }

        public List<string> GetAllFCMDevices(List<string> sp_codes)
        {
            var query = string.Empty;
            if (sp_codes.Count == 0 || string.IsNullOrWhiteSpace(sp_codes[0]))
                query = $@"SELECT DISTINCT FIREBASE_ID FROM DIST_LOGIN_DEVICE WHERE TRIM(FIREBASE_ID) IS NOT NULL";
            else
                query = $@"SELECT DISTINCT FIREBASE_ID FROM DIST_LOGIN_DEVICE DLD
                    INNER JOIN DIST_LOGIN_USER DLU ON DLD.USERID=DLU.USERID
                     WHERE DLU.SP_CODE IN ('{string.Join("','", sp_codes)}') AND TRIM(DLD.FIREBASE_ID) IS NOT NULL";
            var data = _objectEntity.SqlQuery<string>(query).ToList();
            return data;
        }


        public List<ItemModel> GetAllCompItems(User userInfo)
        {
            string query = $"SELECT TO_CHAR(ITEM_ID) AS ITEM_CODE,ITEM_EDESC FROM DIST_COMP_ITEM_MASTER WHERE DELETED_FLAG= 'N' AND COMPANY_CODE = '{userInfo.company_code}'";
            var data = _objectEntity.SqlQuery<ItemModel>(query).ToList();
            return data;
        }

        public string SaveCompItemMap(List<CompItemModel> model, User userInfo)
        {
            var result = "200";
            using (var trans = _objectEntity.Database.BeginTransaction())
            {
                try
                {
                    //clear all map Items
                    var delRows = _objectEntity.ExecuteSqlCommand($"DELETE FROM DIST_COMP_ITEM_MAP WHERE COMPANY_CODE='{userInfo.company_code}'");

                    //insert all map items
                    foreach (var item in model)
                    {
                        foreach (var compItem in item.COMP_ITEMS)
                        {
                            var Query = $@"INSERT INTO DIST_COMP_ITEM_MAP(ITEM_CODE, COMP_ITEM_ID, CREATED_BY, CREATED_DATE, COMPANY_CODE) 
                                    VALUES ('{item.ITEM_CODE}','{compItem}','{userInfo.User_id}',SYSDATE,'{userInfo.company_code}')";
                            var Rows = _objectEntity.ExecuteSqlCommand(Query);
                        }
                    }
                    trans.Commit();
                }
                catch (Exception ex)
                {
                    trans.Rollback();
                    result = "500";
                }
            }
            return result;
        }

        public List<CompItemModel> GetCompItemMap(User userInfo)
        {
            var query = $@"SELECT IMS.ITEM_EDESC, DIM.ITEM_CODE,
                             WM_CONCAT (DIM.COMP_ITEM_ID) COMP_ITEM_CODES
                        FROM DIST_COMP_ITEM_MAP DIM INNER JOIN DIST_COMP_ITEM_MASTER DCM ON DCM.ITEM_ID = DIM.COMP_ITEM_ID AND DCM.COMPANY_CODE = DIM.COMPANY_CODE
                             INNER JOIN IP_ITEM_MASTER_SETUP IMS ON IMS.ITEM_CODE = DIM.ITEM_CODE AND IMS.COMPANY_CODE = DIM.COMPANY_CODE
                        WHERE IMS.DELETED_FLAG = 'N'
                             AND DCM.DELETED_FLAG = 'N'
                             --AND DIM.COMPANY_CODE = '{userInfo.company_code}'
                    GROUP BY IMS.ITEM_EDESC, DIM.ITEM_CODE
                    ORDER BY ITEM_CODE";
            var data = _objectEntity.SqlQuery<CompItemModel>(query).ToList();
            //change the string to list
            foreach (var item in data)
                item.COMP_ITEMS = item.COMP_ITEM_CODES.Split(',').ToList();

            return data;
        }

        public List<CompetitorItemFields> GetCompFields(User userInfo)
        {
            string Query = $@"SELECT DISTINCT DCF.ITEM_CODE,IMS.ITEM_EDESC
                FROM DIST_COMP_FIELDS DCF
                INNER JOIN IP_ITEM_MASTER_SETUP IMS ON IMS.ITEM_CODE = DCF.ITEM_CODE AND IMS.COMPANY_CODE = DCF.COMPANY_CODE
                WHERE DCF.COMPANY_CODE = '{userInfo.company_code}'";
            var data = _objectEntity.SqlQuery<CompetitorItemFields>(Query).ToList();
            foreach (var item in data)
            {
                item.FIELDS = _objectEntity.SqlQuery<FieldModel>($"SELECT COL_NAME,COL_DATA_TYPE FROM DIST_COMP_FIELDS WHERE COMPANY_CODE = '{userInfo.company_code}' and item_code='{item.ITEM_CODE}'").ToList();
            }
            return data;
        }

        public string SaveCompFields(CompetitorItemFields model, User userInfo)
        {
            var result = "200";
            using (var trans = _objectEntity.Database.BeginTransaction())
            {
                try
                {
                    var deletedRows = _objectEntity.ExecuteSqlCommand($"DELETE FROM DIST_COMP_FIELDS WHERE COMPANY_CODE = '{userInfo.company_code}' and item_code='{model.ITEM_CODE}'");
                    foreach (var field in model.FIELDS)
                    {
                        var query = $@"INSERT INTO DIST_COMP_FIELDS (FIELD_ID,ITEM_CODE,COL_NAME,COL_DATA_TYPE,CREATED_BY,CREATED_DATE,COMPANY_CODE)
                                VALUES((SELECT NVL(MAX(FIELD_ID),0)+1 NEWID FROM DIST_COMP_FIELDS),
                                '{model.ITEM_CODE}','{field.COL_NAME}','{field.COL_DATA_TYPE}','{userInfo.User_id}',SYSDATE,'{userInfo.company_code}')";
                        var rows = _objectEntity.ExecuteSqlCommand(query);
                    }
                    trans.Commit();
                }
                catch (Exception ex)
                {
                    result = "500";
                    trans.Rollback();
                }
            }
            return result;
        }

        public string DefaultCompFileds(User userInfo)
        {
            var result = "";
            using (var trans = _objectEntity.Database.BeginTransaction())
            {
                try
                {
                    if (_objectEntity.SqlQuery<int>("SELECT COUNT(*) FROM USER_SEQUENCES WHERE SEQUENCE_NAME = 'COMP_FIELDS_SEQ'").FirstOrDefault() > 0)
                        _objectEntity.ExecuteSqlCommand("DROP SEQUENCE COMP_FIELDS_SEQ");

                    var rows = _objectEntity.ExecuteSqlCommand("CREATE SEQUENCE COMP_FIELDS_SEQ MINVALUE 1 MAXVALUE 999999999999999999999999999 START WITH 1 INCREMENT BY 1");
                    rows = _objectEntity.ExecuteSqlCommand($"DELETE FROM DIST_COMP_FIELDS WHERE COL_NAME IN ('Rate','Available') AND COMPANY_CODE = '{userInfo.company_code}'");

                    var rateQuery = $@"INSERT INTO DIST_COMP_FIELDS ( FIELD_ID, ITEM_CODE,COL_NAME,COL_DATA_TYPE,CREATED_BY,CREATED_DATE,COMPANY_CODE)
                        SELECT COMP_FIELDS_SEQ.NEXTVAL, ITEM_CODE,'Rate','NUM','2',SYSDATE,'{userInfo.company_code}'
                        FROM IP_ITEM_MASTER_SETUP WHERE CATEGORY_CODE='FG' AND DELETED_FLAG='N' AND COMPANY_CODE='{userInfo.company_code}'";
                    var availableQuery = $@"INSERT INTO DIST_COMP_FIELDS ( FIELD_ID, ITEM_CODE,COL_NAME,COL_DATA_TYPE,CREATED_BY,CREATED_DATE,COMPANY_CODE)
                        SELECT COMP_FIELDS_SEQ.NEXTVAL, ITEM_CODE,'Available','BOL','2',SYSDATE,'{userInfo.company_code}'
                        FROM IP_ITEM_MASTER_SETUP WHERE CATEGORY_CODE='FG' AND DELETED_FLAG='N' AND COMPANY_CODE='{userInfo.company_code}'";
                    rows = _objectEntity.ExecuteSqlCommand(rateQuery);
                    rows = _objectEntity.ExecuteSqlCommand(availableQuery);

                    rows = _objectEntity.ExecuteSqlCommand("DROP SEQUENCE COMP_FIELDS_SEQ");
                    trans.Commit();
                    result = "200";
                }
                catch (Exception ex)
                {
                    trans.Rollback();
                    result = "500";
                }
            }
            return result;
        }

        public List<CompItemSetupModel> GetCompItem(User userInfo)
        {
            var query = string.Empty;
            query = $@"SELECT ITEM_ID ,ITEM_EDESC,RATE,UNIT,ITEM_CATEGORY CATEGORY FROM DIST_COMP_ITEM_MASTER WHERE DELETED_FLAG ='N' ORDER BY TO_NUMBER(ITEM_ID) ASC ";
            var result = _objectEntity.SqlQuery<CompItemSetupModel>(query).ToList();
            return result;
        }

        public List<string> GetCompCategories(User userInfo)
        {
            var result = _objectEntity.SqlQuery<string>($"SELECT DISTINCT ITEM_CATEGORY FROM DIST_COMP_ITEM_MASTER WHERE ITEM_CATEGORY IS NOT NULL").ToList();
            return result;
        }

        // Create/update CompItem
        public string CreateCompItem(CompItemSetupModel model, User userInfo)
        {
            var Query1 = string.Empty;
            if (model.ITEM_ID > 0)
            {
                Query1 = $@"UPDATE DIST_COMP_ITEM_MASTER SET ITEM_EDESC ='{model.ITEM_EDESC}', RATE = '{model.RATE}', UNIT = '{model.UNIT}', ITEM_CATEGORY = '{model.CATEGORY}',
                        MODIFIED_BY = '{userInfo.User_id}', MODIFIED_DATE=SYSDATE WHERE ITEM_ID= '{model.ITEM_ID}' AND COMPANY_CODE= '{userInfo.company_code}'";
            }
            else
            {
                model.ITEM_ID = _objectEntity.SqlQuery<int>("SELECT NVL(MAX(ITEM_ID),1000)+1 FROM DIST_COMP_ITEM_MASTER").FirstOrDefault();
                Query1 = $@"INSERT INTO  DIST_COMP_ITEM_MASTER (ITEM_ID,ITEM_EDESC,RATE,UNIT,ITEM_CATEGORY,CREATED_BY,CREATED_DATE,COMPANY_CODE,BRANCH_CODE,DELETED_FLAG) 
                         VALUES('{model.ITEM_ID}', '{model.ITEM_EDESC}', '{model.RATE}', '{model.UNIT}','{model.CATEGORY}','{userInfo.User_id}', SYSDATE, '{userInfo.company_code}', '{userInfo.branch_code}', 'N')";
            }
            var insertroot = _objectEntity.ExecuteSqlCommand(Query1);
            return "200";
        }

        //Delete CompItem
        public string DeleteCompItem(int Id, User userInfo)
        {
            var sqlquery = $@"DELETE FROM DIST_COMP_ITEM_MASTER WHERE ITEM_ID='{Id}' AND COMPANY_CODE= '{userInfo.company_code}'";
            _objectEntity.ExecuteSqlCommand(sqlquery);
            return "success";
        }

        public string SaveGroupMap(List<GroupMapModel> model, User userInfo)
        {
            var result = "200";
            using (var trans = _objectEntity.Database.BeginTransaction())
            {
                try
                {
                    //clear all map Items
                    var delRows = _objectEntity.ExecuteSqlCommand($"DELETE FROM DIST_GROUP_MAPPING WHERE COMPANY_CODE='{userInfo.company_code}'");

                    //insert all map items
                    foreach (var item in model)
                    {
                        foreach (var group in item.MAPPED_GROUPS)
                        {
                            var Query = $@"INSERT INTO DIST_GROUP_MAPPING(GROUPID, MAPPED_GROUPID, COMPANY_CODE, CREATED_BY, CREATED_DATE) 
                                    VALUES ('{item.GROUP_CODE}','{group}','{userInfo.company_code}','{userInfo.User_id}',SYSDATE)";
                            var Rows = _objectEntity.ExecuteSqlCommand(Query);
                        }
                    }
                    trans.Commit();
                }
                catch (Exception ex)
                {
                    trans.Rollback();
                    result = "500";
                }
            }
            return result;
        }

        public List<GroupMapModel> GetGroupMap(User userInfo)
        {
            var query = $@"SELECT GROUPID GROUP_CODE,WM_CONCAT(MAPPED_GROUPID) MAPPED_GROUP_CODES
                        FROM DIST_GROUP_MAPPING
                        --WHERE COMPANY_CODE = '{userInfo.company_code}'
                        GROUP BY GROUPID";
            var data = _objectEntity.SqlQuery<GroupMapModel>(query).ToList();

            //change the string to list
            foreach (var item in data)
                item.MAPPED_GROUPS = item.MAPPED_GROUP_CODES.Split(',').ToList();

            return data;
        }
        public string SaveDistUserMap(List<UserMapModel> model, User userInfo)
        {
            var result = string.Empty;
            using (var trans = _objectEntity.Database.BeginTransaction())
            {
                try
                {
                    //clear all map Items
                    var delRows = _objectEntity.ExecuteSqlCommand($"DELETE FROM DIST_SYNERGY_EMPLOYEE_MAP WHERE COMPANY_CODE='{userInfo.company_code}'");

                    //insert all map items
                    foreach (var item in model)
                    {
                        foreach (var group in item.MAPPED_USERS)
                        {
                            var Query = $@"INSERT INTO DIST_SYNERGY_EMPLOYEE_MAP(LOGIN_SP_CODE, DIST_SP_CODE, COMPANY_CODE, CREATED_BY, CREATED_DATE) 
                                    VALUES ('{item.LOGIN_SP_CODE}','{group}','{userInfo.company_code}','{userInfo.User_id}',SYSDATE)";
                            var Rows = _objectEntity.ExecuteSqlCommand(Query);
                        }

                        var distULC = $@"'{string.Join("','", item.MAPPED_USERS)}'";
                        var getUserIdQry = $@"SELECT TO_CHAR(USER_NO) FROM SC_APPLICATION_USERS WHERE EMPLOYEE_CODE = '{item.LOGIN_SP_CODE}'";
                        var userId = _objectEntity.SqlQuery<string>(getUserIdQry).First();
                        DistUserMappingInTextFilt(userInfo, userId, distULC);
                    }
                    trans.Commit();
                    result = "Success";
                }
                catch (Exception ex)
                {
                    trans.Rollback();
                    throw ex;
                }
            }
            return result;
        }
        public void DistUserMappingInTextFilt(User user, string loginUserCode, string distUserLoginCode)
        {
            var path = HttpContext.Current.Server.MapPath(@"~/App_Data/SpCode_" + loginUserCode + "_" + user.company_code + ".txt");
            if (!System.IO.File.Exists(path))
            {
                using (var tw = new System.IO.StreamWriter(path, true))
                {
                    tw.WriteLine(distUserLoginCode);
                    tw.Flush();
                }
            }
            else
            {
                using (var tw = new System.IO.StreamWriter(path))
                {
                    tw.WriteLine(distUserLoginCode);
                    tw.Flush();
                }
            }
        }
        public List<UserMapModel> GetDistUserMap(User userInfo)
        {
            var query = $@"SELECT LOGIN_SP_CODE LOGIN_SP_CODE,WM_CONCAT(DIST_SP_CODE) DIST_SP_CODES
                        FROM DIST_SYNERGY_EMPLOYEE_MAP
                        WHERE COMPANY_CODE = '{userInfo.company_code}'
                        GROUP BY LOGIN_SP_CODE";
            var data = _objectEntity.SqlQuery<UserMapModel>(query).ToList();

            //change the string to list
            foreach (var item in data)
                item.MAPPED_USERS = item.DIST_SP_CODES.Split(',').ToList();

            return data;
        }
        public string deleteUser(UserMapModel model, User userInfo)
        {
            var sqlquery = $@"DELETE FROM DIST_SYNERGY_EMPLOYEE_MAP WHERE LOGIN_SP_CODE='{model.LOGIN_SP_CODE}' AND COMPANY_CODE= '{userInfo.company_code}'";
            _objectEntity.ExecuteSqlCommand(sqlquery);
            return "success";
        }


        #region ResellerExcelSave






        public ResellerListModel GetResellerSaveObj(ResellerListModel model, User userInfo)
        {
            model.Distributor_Name = (model.Distributor_Name == null) ? "" : model.Distributor_Name;
            var AreaName = $"SELECT DISTINCT TO_CHAR(AREA_CODE) FROM DIST_AREA_MASTER WHERE UPPER(AREA_NAME) = TRIM(UPPER('{model.AREA_Name}')) OR AREA_CODE LIKE '{model.AREA_Name}' AND COMPANY_CODE = '{userInfo.company_code}' AND DELETED_FLAG= 'N'";
            var OutletType = $"SELECT DISTINCT TYPE_ID FROM DIST_OUTLET_TYPE WHERE UPPER(TYPE_EDESC) = TRIM(UPPER('{model.OUTLET_TYPE}')) OR TYPE_ID LIKE '{model.OUTLET_TYPE}' AND COMPANY_CODE = '{userInfo.company_code}' AND DELETED_FLAG= 'N'";
            var OutletSubType = $"SELECT DISTINCT SUBTYPE_ID FROM DIST_OUTLET_SUBTYPE WHERE UPPER(SUBTYPE_EDESC) = TRIM(UPPER('{model.OUTLET_SUBTYPE}')) OR SUBTYPE_ID LIKE '{model.OUTLET_SUBTYPE}' AND COMPANY_CODE = '{userInfo.company_code}' AND DELETED_FLAG= 'N'";
            var groupName = $"SELECT DISTINCT GROUPID FROM DIST_GROUP_MASTER WHERE UPPER(GROUP_EDESC) = TRIM(UPPER('{model.GROUP_NAME}')) OR GROUPID LIKE '{model.GROUP_NAME}' AND COMPANY_CODE = '{userInfo.company_code}' AND DELETED_FLAG= 'N'";
            var DistributorName = $"SELECT DISTINCT TO_CHAR(CUSTOMER_CODE) AS CODE FROM SA_CUSTOMER_SETUP WHERE UPPER(CUSTOMER_EDESC) IN (TRIM(UPPER('{model.Distributor_Name.Replace(",", "')),TRIM(UPPER('") }'))) OR CUSTOMER_CODE IN (TRIM(UPPER('{model.Distributor_Name.Replace(",", "')),TRIM(UPPER('") }'))) AND COMPANY_CODE = '{userInfo.company_code}' AND DELETED_FLAG= 'N'";

            model.AREA_Code = _objectEntity.SqlQuery<string>(AreaName).FirstOrDefault();
            model.OUTLET_TYPE_ID = _objectEntity.SqlQuery<int>(OutletType).FirstOrDefault();
            model.OUTLET_SUBTYPE_ID = _objectEntity.SqlQuery<int>(OutletSubType).FirstOrDefault();
            model.GROUP_ID = _objectEntity.SqlQuery<int>(groupName).FirstOrDefault();
            var distributorCode = _objectEntity.SqlQuery<CustomerModel>(DistributorName).ToList();
            model.Distributors.AddRange(distributorCode);
            return model;
        }

        public DistributorListModel GetDistributorSaveObj(DistributorListModel model, User userInfo)
        {
            var AreaName = $"SELECT DISTINCT TO_CHAR(AREA_CODE) FROM DIST_AREA_MASTER WHERE UPPER(AREA_NAME) = TRIM(UPPER('{model.AREA_Name}')) OR AREA_CODE LIKE '{model.AREA_Name}' AND COMPANY_CODE = '{userInfo.company_code}' AND DELETED_FLAG= 'N'";

            var groupName = $"SELECT DISTINCT GROUPID FROM DIST_GROUP_MASTER WHERE UPPER(GROUP_EDESC) = TRIM(UPPER('{model.GROUP_EDESC}')) OR GROUPID LIKE '{model.GROUP_EDESC}' AND COMPANY_CODE = '{userInfo.company_code}' AND DELETED_FLAG= 'N'";

            model.AREA_Code = _objectEntity.SqlQuery<string>(AreaName).FirstOrDefault();
            model.GROUPID = _objectEntity.SqlQuery<int>(groupName).FirstOrDefault();
            return model;
        }

        public CompItemModel GetCompItemMapSaveObj(CompItemModel model, User userInfo)
        {
            model.COMP_ITEM_EDESC = (model.COMP_ITEM_EDESC == null) ? "" : model.COMP_ITEM_EDESC;
            var ItemCode = $"SELECT DISTINCT TO_NUMBER(ITEM_CODE) FROM IP_ITEM_MASTER_SETUP WHERE UPPER(ITEM_EDESC) = TRIM(UPPER('{model.ITEM_EDESC}')) OR ITEM_CODE LIKE '{model.ITEM_EDESC}' AND COMPANY_CODE = '{userInfo.company_code}' AND DELETED_FLAG= 'N'";
            var CompItemMaps = $"SELECT DISTINCT TO_CHAR(ITEM_ID) AS CODE FROM DIST_COMP_ITEM_MASTER WHERE UPPER(ITEM_EDESC) IN (TRIM(UPPER('{model.COMP_ITEM_EDESC.Replace(",", "')),TRIM(UPPER('") }'))) OR TO_CHAR(ITEM_ID) IN(TRIM(UPPER('{model.COMP_ITEM_EDESC.Replace(",", "')),TRIM(UPPER('") }'))) AND COMPANY_CODE = '{userInfo.company_code}' AND DELETED_FLAG= 'N'";
            var itemCode = _objectEntity.SqlQuery<int>(ItemCode).FirstOrDefault();
            model.ITEM_CODE = Convert.ToInt32(itemCode);
            model.COMP_ITEMS.AddRange(_objectEntity.SqlQuery<string>(CompItemMaps).ToList());
            return model;
        }



        #endregion

        public RouteListModel GetRouteSaveObj(RouteGroup model, User userInfo)
        {
            var temp = model.Data.ToList();
            var areaTemp = temp.Select(x => x.AREA).Distinct().ToList();
            var AreaQuery = $"SELECT AREA_CODE FROM DIST_AREA_MASTER WHERE UPPER(AREA_NAME) IN (UPPER(TRIM('{String.Join("')),UPPER(TRIM('", areaTemp)}'))) OR AREA_CODE IN (UPPER(TRIM('{String.Join("')),UPPER(TRIM('", areaTemp)}')))";

            var result = new RouteListModel
            {
                ROUTE_NAME = temp[0].ROUTE_NAME,
                ROUTE_TYPE = temp[0].ROUTE_TYPE,
                AREA_CODE = _objectEntity.SqlQuery<string>(AreaQuery).ToList()
            };

            var entities = new List<RouteEntityModel>();
            var sn = 1;
            foreach (var item in temp)
            {
                var query = "";
                if (item.ENTITY_TYPE == "D")
                    query = $"SELECT TO_CHAR(CUSTOMER_CODE) FROM SA_CUSTOMER_SETUP WHERE CUSTOMER_EDESC = '{item.ENTITY}'  OR CUSTOMER_CODE LIKE '{item.ENTITY}'  AND COMPANY_CODE='{userInfo.company_code}' AND DELETED_FLAG='N'";
                if (item.ENTITY_TYPE == "R")
                    query = $"SELECT TO_CHAR(RESELLER_CODE) FROM DIST_RESELLER_MASTER WHERE RESELLER_NAME = '{item.ENTITY}' OR RESELLER_CODE LIKE '{item.ENTITY}' AND COMPANY_CODE='{userInfo.company_code}' AND DELETED_FLAG='N'";
                if (item.ENTITY_TYPE == "P")
                    query = $"SELECT PARTY_TYPE_CODE FROM IP_PARTY_TYPE_CODE WHERE PARTY_TYPE_EDESC = '{item.ENTITY}' OR PARTY_TYPE_CODE LIKE '{item.ENTITY}'  AND COMPANY_CODE='{userInfo.company_code}' AND DELETED_FLAG='N'";
                if (item.ENTITY_TYPE == "H")
                    query = $"SELECT TO_CHAR(CODE) FROM BRD_OTHER_ENTITY WHERE DESCRIPTION = '{item.ENTITY}' OR CODE LIKE '{item.ENTITY}' AND COMPANY_CODE = '{userInfo.company_code}' AND DELETED_FLAG = 'N'";

                entities.Add(new RouteEntityModel
                {
                    ENTITY_Code = _objectEntity.SqlQuery<string>(query).FirstOrDefault(),
                    ENTITY_TYPE = item.ENTITY_TYPE,
                    ORDER_NO = sn.ToString()
                });
                sn++;
            }
            result.RouteEntityModel = entities;
            return result;
        }

        //Employee
        public List<CompanyModel> GetCompany(User userInfo)
        {
            var result = _objectEntity.SqlQuery<CompanyModel>($"SELECT COMPANY_EDESC ,COMPANY_CODE  FROM COMPANY_SETUP WHERE DELETED_FLAG='N'").ToList();
            return result;
        }

        public List<BranchModel> GetBranch(string COMPANY_CODE, User userInfo)
        {
            if (string.IsNullOrWhiteSpace(COMPANY_CODE))
            {
                var result = _objectEntity.SqlQuery<BranchModel>($"SELECT BRANCH_EDESC BranchName,BRANCH_CODE BranchCode FROM FA_BRANCH_SETUP WHERE DELETED_FLAG= 'N' AND GROUP_SKU_FLAG = 'I'").ToList();
                return result;
            }
            else
            {
                var result = _objectEntity.SqlQuery<BranchModel>($"SELECT BRANCH_EDESC BranchName,BRANCH_CODE BranchCode,COMPANY_CODE FROM FA_BRANCH_SETUP WHERE DELETED_FLAG= 'N' AND GROUP_SKU_FLAG = 'I' AND COMPANY_CODE IN ('{COMPANY_CODE}')").ToList();
                return result;
            }
        }

        public List<EmployeeSetupModel> GetEmployee(User userInfo)
        {
            var query = string.Empty;
            query = $@"SELECT HES.EMPLOYEE_CODE,HES.EMPLOYEE_EDESC,HES.EPERMANENT_ADDRESS1,
                        WM_CONCAT(DISTINCT CS.COMPANY_EDESC) COMPANY_EDESC,
                        WM_CONCAT(DISTINCT FBS.BRANCH_EDESC) BRANCH_EDESC,
                        WM_CONCAT(DISTINCT  CS.COMPANY_CODE) COMPANY_CODES,
                        WM_CONCAT(DISTINCT FBS.BRANCH_CODE) BRANCH_CODES
            FROM HR_EMPLOYEE_SETUP HES
            INNER JOIN SC_APPLICATION_USERS SAU ON HES.EMPLOYEE_CODE = SAU.EMPLOYEE_CODE
            INNER JOIN SC_COMPANY_CONTROL SCC ON SAU.USER_NO =  SCC.USER_NO
            INNER JOIN COMPANY_SETUP CS ON CS.COMPANY_CODE = SCC.COMPANY_CODE
            INNER JOIN SC_BRANCH_CONTROL SBC ON SAU.USER_NO = SBC.USER_NO
            INNER JOIN FA_BRANCH_SETUP FBS ON FBS.BRANCH_CODE = SBC.BRANCH_CODE
            WHERE HES.GROUP_SKU_FLAG = 'I'
                        AND HES.DELETED_FLAG = 'N'
                        AND SCC.DELETED_FLAG = 'N'
                        AND SBC.DELETED_FLAG = 'N'
            GROUP BY HES.EMPLOYEE_CODE,HES.EMPLOYEE_EDESC,HES.EPERMANENT_ADDRESS1";

            var result = _objectEntity.SqlQuery<EmployeeSetupModel>(query).ToList();
            return result;
        }

        public string CreateEmployee(EmployeeSetupModel model, User userInfo)
        {
            var result = "";
            using (var trans = _objectEntity.Database.BeginTransaction())
            {
                try
                {
                    var insertroot = 0;
                    var query = string.Empty;
                    if (string.IsNullOrWhiteSpace(model.EMPLOYEE_CODE))
                    {
                        model.EMPLOYEE_CODE = _objectEntity.SqlQuery<int>("SELECT NVL(MAX(TO_NUMBER(EMPLOYEE_CODE)),1000)+1 FROM HR_EMPLOYEE_SETUP").FirstOrDefault().ToString();
                        query = $@"INSERT INTO HR_EMPLOYEE_SETUP(EMPLOYEE_CODE,EMPLOYEE_EDESC,EMPLOYEE_NDESC,GROUP_SKU_FLAG,EPERMANENT_ADDRESS1,COMPANY_CODE,BRANCH_CODE,CREATED_BY,CREATED_DATE,LOCK_FLAG,OVERTIME_APPLICABLE,DELETED_FLAG )
                               VALUES('{model.EMPLOYEE_CODE}','{model.EMPLOYEE_EDESC}','{model.EMPLOYEE_EDESC}','I','{model.EPERMANENT_ADDRESS1}','{userInfo.company_code}','{userInfo.branch_code}','{userInfo.User_id}',SYSDATE,'N','N','N') ";
                        model.USER_NO = _objectEntity.SqlQuery<int>("SELECT (MAX(USER_NO) )+1 FROM SC_APPLICATION_USERS").FirstOrDefault();
                        var query1 = $@"INSERT INTO SC_APPLICATION_USERS(  USER_NO, PRE_USER_NO,GROUP_SKU_FLAG, LOGIN_CODE,LOGIN_EDESC,LOGIN_NDESC,PASSWORD,
                                 EMPLOYEE_CODE,COMPANY_CODE,CREATED_BY,CREATED_DATE,
                                 ABBR_CODE,DELETED_FLAG,GLOBAL_MASTER_EDIT_FLAG,DEFAULT_CONSOLIDATE)
                                 VALUES( '{model.USER_NO}','3','I','{model.EMPLOYEE_EDESC}','{model.EMPLOYEE_EDESC}','{model.EMPLOYEE_EDESC}','{model.EMPLOYEE_EDESC}',
                                  '{model.EMPLOYEE_CODE}','{userInfo.company_code}','{userInfo.User_id}',SYSDATE,'SBM','N','N','N')";
                        insertroot = _objectEntity.ExecuteSqlCommand(query1);
                    }
                    else
                    {
                        query = $@" UPDATE HR_EMPLOYEE_SETUP SET EMPLOYEE_EDESC='{model.EMPLOYEE_EDESC}',EMPLOYEE_NDESC='{model.EMPLOYEE_EDESC}',EPERMANENT_ADDRESS1 = '{model.EPERMANENT_ADDRESS1}'
                        WHERE EMPLOYEE_CODE='{model.EMPLOYEE_CODE}' AND COMPANY_CODE = '{userInfo.company_code}' AND BRANCH_CODE = '{userInfo.branch_code}' AND DELETED_FLAG='N'";
                        var userNo = _objectEntity.SqlQuery<int>($"SELECT USER_NO FROM SC_APPLICATION_USERS WHERE EMPLOYEE_CODE = '{model.EMPLOYEE_CODE}' AND GROUP_SKU_FLAG = 'I' AND COMPANY_CODE = '{userInfo.company_code}' AND ROWNUM = 1").FirstOrDefault();
                        var deletedRows = _objectEntity.ExecuteSqlCommand($"DELETE FROM SC_COMPANY_CONTROL WHERE USER_NO = '{userNo}'");
                        deletedRows = _objectEntity.ExecuteSqlCommand($"DELETE FROM SC_BRANCH_CONTROL WHERE USER_NO = '{userNo}'");
                        model.USER_NO = userNo;
                    }
                    insertroot = _objectEntity.ExecuteSqlCommand(query);

                    foreach (var company in model.COMPANY_CODE)
                    {
                        var query2 = $@"INSERT INTO SC_COMPANY_CONTROL( USER_NO,COMPANY_CODE,ACCESS_FLAG,CREATED_BY,CREATED_DATE,DELETED_FLAG)
                                    VALUES( '{model.USER_NO}','{company}','Y','{userInfo.User_id}',SYSDATE,'N')";
                        insertroot = _objectEntity.ExecuteSqlCommand(query2);
                    }
                    foreach (var branch in model.BRANCH_CODE)
                    {
                        var company = branch.Substring(0, 2);
                        var query3 = $@"INSERT INTO SC_BRANCH_CONTROL( USER_NO,BRANCH_CODE,ACCESS_FLAG,COMPANY_CODE,CREATED_BY,CREATED_DATE,DELETED_FLAG)
                                       VALUES( '{model.USER_NO}','{branch}','Y','{company}','{userInfo.User_id}',SYSDATE,'N')";
                        insertroot = _objectEntity.ExecuteSqlCommand(query3);
                    }

                    trans.Commit();
                    result = "200";
                }
                catch (Exception ex)
                {
                    trans.Rollback();
                    result = "500";
                }
            }
            return result;
        }

        //Delete Employee
        public string DeleteEmployee(int Id, User userInfo)
        {
            var result = "";
            using (var tran = _objectEntity.Database.BeginTransaction())
            {
                try
                {
                    var sqlquery = $@"UPDATE HR_EMPLOYEE_SETUP SET DELETED_FLAG='Y' WHERE EMPLOYEE_CODE='{Id}' AND COMPANY_CODE= '{userInfo.company_code}'";
                    _objectEntity.ExecuteSqlCommand(sqlquery);
                    var userNo = _objectEntity.SqlQuery<int>($"SELECT USER_NO FROM SC_APPLICATION_USERS WHERE EMPLOYEE_CODE = '{Id}' AND GROUP_SKU_FLAG = 'I' AND COMPANY_CODE = '{userInfo.company_code}' AND ROWNUM = 1").FirstOrDefault();
                    var deletedRows = _objectEntity.ExecuteSqlCommand($"UPDATE SC_COMPANY_CONTROL SET DELETED_FLAG='Y' WHERE USER_NO = '{userNo}'");
                    deletedRows = _objectEntity.ExecuteSqlCommand($"UPDATE SC_BRANCH_CONTROL SET DELETED_FLAG='Y' WHERE USER_NO = '{userNo}'");
                    deletedRows = _objectEntity.ExecuteSqlCommand($"UPDATE SC_APPLICATION_USERS SET DELETED_FLAG='Y' WHERE EMPLOYEE_CODE = '{Id}' AND GROUP_SKU_FLAG = 'I' AND COMPANY_CODE = '{userInfo.company_code}'");
                    tran.Commit();
                    result = "200";
                }
                catch (Exception ex)
                {
                    tran.Rollback();
                    result = "500";
                }
            }
            return result;
        }

        public List<CustomerModel> GetMuCodes()
        {
            var data = _objectEntity.SqlQuery<CustomerModel>("SELECT DISTINCT MU_CODE CODE,MU_EDESC NAME FROM IP_MU_CODE WHERE DELETED_FLAG = 'N'").ToList();
            return data;
        }

        public List<CategoryModel> GetCategoryCodes()
        {
            var data = _objectEntity.SqlQuery<CategoryModel>("SELECT DISTINCT CATEGORY_CODE ,CATEGORY_EDESC  FROM IP_CATEGORY_CODE WHERE DELETED_FLAG = 'N'").ToList();
            return data;
        }

        public List<ItemModel> GetItems(User userInfo)
        {
            var query = $@"SELECT IMS.ITEM_CODE,IMS.ITEM_EDESC,IMS.CATEGORY_CODE,IMS.INDEX_MU_CODE MU_CODE,ISS.BRAND_NAME,IRS.SALES_RATE,IRS.APPLY_DATE
                    FROM IP_ITEM_MASTER_SETUP IMS 
                    LEFT JOIN IP_ITEM_SPEC_SETUP ISS ON IMS.ITEM_CODE = ISS.ITEM_CODE AND IMS.COMPANY_CODE = ISS.COMPANY_CODE
                    LEFT JOIN (SELECT A.ITEM_CODE, A.APPLY_DATE, B.SALES_RATE, B.COMPANY_CODE
                                                  FROM (SELECT ITEM_CODE, COMPANY_CODE, MAX(APP_DATE) APPLY_DATE 
                                                    FROM IP_ITEM_RATE_APPLICAT_SETUP
                                                    WHERE COMPANY_CODE = '{userInfo.company_code}'
                                                    AND BRANCH_CODE = '{userInfo.branch_code}'
                                                    GROUP BY ITEM_CODE, COMPANY_CODE) A
                                                  INNER JOIN IP_ITEM_RATE_APPLICAT_SETUP B
                                                    ON B.ITEM_CODE = A.ITEM_CODE
                                                    AND B.APP_DATE = A.APPLY_DATE
                                                    AND B.COMPANY_CODE = '{userInfo.company_code}'
                                                    AND B.BRANCH_CODE = '{userInfo.branch_code}')
                            IRS ON IMS.ITEM_CODE = IRS.ITEM_CODE AND IMS.COMPANY_CODE = IRS.COMPANY_CODE
                    WHERE IMS.DELETED_FLAG = 'N'
                    AND GROUP_SKU_FLAG = 'I'
                    AND IMS.COMPANY_CODE = '{userInfo.company_code}'
                    ORDER BY IMS.ITEM_CODE";
            var data = _objectEntity.SqlQuery<ItemModel>(query).ToList();
            return data;
        }
        public List<ItemModel> GetItemsList(User userInfo)
        {
            string item_filter_condition = "";
            try
            {
                var url = System.Web.HttpContext.Current.Request.Url.AbsoluteUri;
                string xmlpath = HttpContext.Current.Server.MapPath("~/Areas/NeoERP.Distribution/ProductCondition.xml");
                var xml = XDocument.Load(xmlpath);
                var condition_query = from c in xml.Root.Descendants("Vendor")
                                      where (string)c.Attribute("ModuleName") == "Distribution"
                                      select c.Element("ConditionQuery").Value;
                var result = condition_query.FirstOrDefault();
                if (result != null)
                {
                    item_filter_condition = result;
                }
            }
            catch (Exception)
            {
                item_filter_condition = "";
            }
            var query = $@"SELECT IMS.ITEM_CODE,IMS.ITEM_EDESC,IMS.CATEGORY_CODE,IMS.INDEX_MU_CODE MU_CODE,ISS.BRAND_NAME,TO_CHAR(NVL(IRS.SALES_RATE,0)) SALES_RATE
                FROM IP_ITEM_MASTER_SETUP IMS 
                LEFT JOIN IP_ITEM_SPEC_SETUP ISS ON IMS.ITEM_CODE = ISS.ITEM_CODE AND IMS.COMPANY_CODE = ISS.COMPANY_CODE
                LEFT JOIN (SELECT A.ITEM_CODE, A.APPLY_DATE, B.SALES_RATE, B.COMPANY_CODE
                                              FROM (SELECT ITEM_CODE, COMPANY_CODE, MAX(APP_DATE) APPLY_DATE 
                                                FROM IP_ITEM_RATE_APPLICAT_SETUP
                                                WHERE COMPANY_CODE = '{userInfo.company_code}'
                                                AND BRANCH_CODE = '{userInfo.branch_code}'
                                                GROUP BY ITEM_CODE, COMPANY_CODE) A
                                              INNER JOIN IP_ITEM_RATE_APPLICAT_SETUP B
                                                ON B.ITEM_CODE = A.ITEM_CODE
                                                AND B.APP_DATE = A.APPLY_DATE
                                                AND B.COMPANY_CODE = '{userInfo.company_code}'
                                                AND B.BRANCH_CODE = '{userInfo.branch_code}') IRS ON IMS.ITEM_CODE = IRS.ITEM_CODE AND IMS.COMPANY_CODE = IRS.COMPANY_CODE
                WHERE 1 = 1 {item_filter_condition}
                AND GROUP_SKU_FLAG = 'I'
                AND IMS.COMPANY_CODE = '{userInfo.company_code}'
                ORDER BY IMS.ITEM_CODE";
            var items = _objectEntity.SqlQuery<ItemModel>(query).ToList();
            return items;
        }
        public int SaveItem(ItemModel model, User userInfo)
        {
            var result = 0;
            using (var trans = _objectEntity.Database.BeginTransaction())
            {
                try
                {
                    var row = 0;
                    if (string.IsNullOrWhiteSpace(model.ITEM_CODE))
                    {
                        model.ITEM_CODE = _objectEntity.SqlQuery<string>($"SELECT TO_CHAR(NVL(MAX(TO_NUMBER(ITEM_CODE)),0)+1) CODE FROM IP_ITEM_MASTER_SETUP WHERE COMPANY_CODE = '{userInfo.company_code}'").FirstOrDefault();
                        string ItemInsert = $@"INSERT INTO IP_ITEM_MASTER_SETUP (ITEM_CODE,ITEM_EDESC, CATEGORY_CODE,INDEX_MU_CODE,MASTER_ITEM_CODE,PRE_ITEM_CODE,GROUP_SKU_FLAG,COMPANY_CODE,BRANCH_CODE,DELETED_FLAG,CREATED_BY,CREATED_DATE)
                        VALUES ('{model.ITEM_CODE}','{model.ITEM_EDESC}','{model.CATEGORY_CODE}','{model.MU_CODE}','01.01','01','I','{userInfo.company_code}','{userInfo.branch_code}','N','{userInfo.User_id}',SYSDATE)";
                        row = _objectEntity.ExecuteSqlCommand(ItemInsert);

                        string BrandInsert = $@"INSERT INTO IP_ITEM_SPEC_SETUP (ITEM_CODE,PART_NUMBER,BRAND_NAME,DELETED_FLAG,COMPANY_CODE,CREATED_BY,CREATED_DATE)
                        VALUES ('{model.ITEM_CODE}','0','{model.BRAND_NAME}','N','{userInfo.company_code}','{userInfo.User_id}',SYSDATE)";
                        row = _objectEntity.ExecuteSqlCommand(BrandInsert);
                    }
                    else
                    {
                        var updateQuery = $@"UPDATE IP_ITEM_MASTER_SETUP SET ITEM_EDESC = '{model.ITEM_EDESC}',CATEGORY_CODE = '{model.CATEGORY_CODE}',INDEX_MU_CODE = '{model.MU_CODE}',
                        MODIFY_DATE = SYSDATE,MODIFY_BY = '{userInfo.User_id}'
                        WHERE ITEM_CODE = '{model.ITEM_CODE}' AND COMPANY_CODE = '{userInfo.company_code}' AND BRANCH_CODE = '{userInfo.branch_code}'";
                        row = _objectEntity.ExecuteSqlCommand(updateQuery);

                        string brdUpdate = $@"UPDATE IP_ITEM_SPEC_SETUP SET BRAND_NAME = '{model.BRAND_NAME}', MODIFY_DATE = SYSDATE, MODIFY_BY = '{userInfo.User_id}'
                        WHERE ITEM_CODE = '{model.ITEM_CODE}' AND COMPANY_CODE = '{userInfo.company_code}'";
                        row = _objectEntity.ExecuteSqlCommand(brdUpdate);
                    }

                    string RateInsert = $@"INSERT INTO IP_ITEM_RATE_APPLICAT_SETUP (ITEM_CODE,APP_DATE,SALES_RATE,DELETED_FLAG,COMPANY_CODE,BRANCH_CODE,CREATED_BY,CREATED_DATE,CURRENCY_CODE,EXCHANGE_RATE,MU_CODE)
                    VALUES ('{model.ITEM_CODE}',SYSDATE,'{model.SALES_RATE}','N','{userInfo.company_code}','{userInfo.branch_code}','{userInfo.User_id}',SYSDATE,'NRS',1,'{model.MU_CODE}')";
                    row = _objectEntity.ExecuteSqlCommand(RateInsert);
                    trans.Commit();
                    result = 200; //success
                }
                catch (Exception ex)
                {
                    trans.Rollback();
                }
            }
            return result;
        }

        public List<WEB_QUESTION_ASNWER> GetWebQuestionAnswerList(User userInfo)
        {
            var query = $@" SELECT * FROM (SELECT DQM.QUESTION,DQM.SET_CODE,WM_CONCAT(DQD.ANSWERS)ANSWERS,DQM.QA_TYPE from DIST_QA_MASTER DQM
 LEFT JOIN DIST_QA_DETAIL  DQD ON DQM.QA_CODE = DQD.QA_CODE AND DQM.COMPANY_CODE=DQD.COMPANY_CODE
 GROUP BY DQM.QUESTION,DQM.SET_CODE,DQM.QA_TYPE)T
 INNER JOIN DIST_QA_SET DQS ON DQS.SET_CODE=T.SET_CODE 
 WHERE DQS.QA_TYPE='w' AND COMPANY_CODE='{userInfo.company_code}'";
            var data = _objectEntity.SqlQuery<WEB_QUESTION_ASNWER>(query).ToList();
            return data;
        }

        public List<ResellerDistributorModel> GetDistributorResellerList(User userInfo)
        {
            try
            {
                var query = $@" SELECT * FROM (SELECT A.DISTRIBUTOR_CODE CUSTOMER_CODE, B.CUSTOMER_EDESC CUSTOMER_EDESC   
                                    FROM DIST_DISTRIBUTOR_MASTER A, SA_CUSTOMER_SETUP B
                                    WHERE A.DISTRIBUTOR_CODE = B.CUSTOMER_CODE
                                         AND A.COMPANY_CODE = B.COMPANY_CODE
                                         AND A.COMPANY_CODE = '{userInfo.company_code}'
                                         AND B.DELETED_FLAG = A.DELETED_FLAG 
                                         AND A.DELETED_FLAG ='N'
                                         AND A.ACTIVE = 'Y'
                                UNION ALL
                                SELECT RESELLER_CODE CUSTOMER_CODE,RESELLER_NAME CUSTOMER_EDESC FROM DIST_RESELLER_MASTER
                                    WHERE COMPANY_CODE='{userInfo.company_code}' AND ACTIVE ='Y')
                                    ORDER BY CUSTOMER_EDESC";
                var result = this._objectEntity.SqlQuery<ResellerDistributorModel>(query).ToList();
                return result;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }



        public List<DDL_TEMPLATE> GetMessageTemplateList(User userInfo)
        {
            try
            {
                var final_data = new List<DDL_TEMPLATE>();
                var query = $@"select SCHEDULAR_TEMPLATE from WEB_SCHEDULARMAIL_QUEUE";
                var result = this._objectEntity.SqlQuery<string>(query).ToList();
                var response = result.Where(x => x.Contains(".html")).ToList();
                foreach (var item in response)
                {
                    var obj = new DDL_TEMPLATE
                    {
                        NAME = item.Split('.')[0],
                        ID = item
                    };
                    final_data.Add(obj);
                }
                return final_data;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
        public List<CustomerModel> GetCustomerByDealer(User userInfo)
        {
            var result = new List<CustomerModel>();
            var query1 = $@"SELECT  PARTY_TYPE_CODE FROM FA_SUB_LEDGER_DEALER_MAP
            WHERE CUSTOMER_CODE = '{userInfo.DistributerNo}'";
            var partyTypeCode = this._objectEntity.SqlQuery<string>(query1).FirstOrDefault();
            if (!string.IsNullOrEmpty(partyTypeCode))
            {
                //var query = $@"SELECT SCS.CUSTOMER_EDESC Name, SCS.CUSTOMER_CODE Code FROM SA_CUSTOMER_SETUP SCS INNER JOIN FA_SUB_LEDGER_DEALER_MAP FSLDM ON FSLDM.CUSTOMER_CODE = SCS.CUSTOMER_CODE AND SCS.COMPANY_CODE= FSLDM.COMPANY_CODE WHERE SCS.DELETED_FLAG='N' AND SCS.COMPANY_CODE='{userInfo.company_code}'AND  FSLDM.PARTY_TYPE_CODE='{userInfo.User_id}'";
                var query = $@"SELECT SCS.CUSTOMER_EDESC Name, SCS.CUSTOMER_CODE Code FROM SA_CUSTOMER_SETUP SCS INNER JOIN FA_SUB_LEDGER_DEALER_MAP FSLDM ON FSLDM.CUSTOMER_CODE = SCS.CUSTOMER_CODE AND SCS.COMPANY_CODE= FSLDM.COMPANY_CODE WHERE SCS.DELETED_FLAG='N' AND SCS.COMPANY_CODE='{userInfo.company_code}'AND  FSLDM.PARTY_TYPE_CODE='{partyTypeCode}'";
                result = this._objectEntity.SqlQuery<CustomerModel>(query).ToList();
            }


            return result;
        }

        public string AddScheme(SchemeModel model, User userInfo)
        {
            
            using (var trans = _objectEntity.Database.BeginTransaction())
            {
                try
                {
                    if (model.Action == "Update" || model.Action == "Approve and Save"  || model.Action == "Check and Save")
                    {
                        if(model.OfferType=="GIFT")
                        {
                            string deleteGiftItem = $@"DELETE DIST_SCHEME_GIFT_ITEMS WHERE SCHEME_ID='{model.SchemeID}'";
                            _objectEntity.ExecuteSqlCommand(deleteGiftItem);

                        }
                        
                        string deleteSchemeItem = $@"DELETE DIST_SCHEME_ITEMS WHERE SCHEME_ID='{model.SchemeID}'";
                        _objectEntity.ExecuteSqlCommand(deleteSchemeItem);
                        
                        string deleteSchemeMapping = $@"DELETE DIST_SCHEME_RULE_MAPPING WHERE SCHEME_ID='{model.SchemeID}'";
                        _objectEntity.ExecuteSqlCommand(deleteSchemeMapping);

                        string deleteSchemeEntity = $@"DELETE DIST_SCHEME_ENTITY_MAPPING WHERE SCHEME_ID='{model.SchemeID}'";
                        _objectEntity.ExecuteSqlCommand(deleteSchemeEntity);


                        string deleteScheme = $@"DELETE DIST_SCHEME WHERE SCHEME_ID='{model.SchemeID}'";
                        _objectEntity.ExecuteSqlCommand(deleteScheme);
                    }
                     
                        var row = 0;
                        var schemeid = 0.00;
                        string IDFetch = "select SCHEME_ID_SEQ.nextval from dual";
                        schemeid = _objectEntity.SqlQuery<double>(IDFetch).FirstOrDefault();

                        string SCHEMEINSERT = $@"INSERT INTO DIST_SCHEME (SCHEME_ID, SCHEME_NAME,   OFFER_TYPE ,  START_DATE, END_DATE, SP_CODE, COMPANY_CODE, BRANCH_CODE, DELETED_FLAG)
                        VALUES ('{schemeid}','{model.SchemeName}','{model.OfferType}', TO_DATE('{model.StartDate.ToShortDateString()}','MM/dd/yyyy'),TO_DATE('{model.EndDate.ToShortDateString()}','MM/dd/yyyy'), '{model.SP_CODE}', '{userInfo.company_code}','{userInfo.branch_code}', 'N')";
                        row = _objectEntity.ExecuteSqlCommand(SCHEMEINSERT);

                        foreach (var area in model.AreaCode)
                        {
                        var AreaId = 0.00;
                        string AreaIDFetch = "select AREA_ID_SEQ.nextval from dual";
                        AreaId = _objectEntity.SqlQuery<double>(AreaIDFetch).FirstOrDefault();

                        string SCHEMEAREAINSERT = $@"Insert into DIST_SCHEME_AREA_MAPPING  (SCHEME_AREA_ID, SCHEME_ID, AREA_CODE)
                            Values('{AreaId}', '{schemeid}', '{area}')";
                        row = _objectEntity.ExecuteSqlCommand(SCHEMEAREAINSERT);

                        }
                        foreach (var item in model.ItemCode)
                        {
                            var SchemeItemId = 0.00;
                            string ItemIDFetch = "select SCHEME_ITEM_ID_SEQ.nextval from dual";
                            SchemeItemId = _objectEntity.SqlQuery<double>(ItemIDFetch).FirstOrDefault();

                            string SCHEMEITEMINSERT = $@"Insert into DIST_SCHEME_ITEMS (SCHEME_ITEM_ID, SCHEME_ID, ITEM_CODE)
                            Values('{SchemeItemId}', '{schemeid}', '{item}')";
                            row = _objectEntity.ExecuteSqlCommand(SCHEMEITEMINSERT);
                        }

                        foreach(var customer in model.CustomerCode)
                        {
                        var CustId = 0.00;
                        string CustIDFetch = "select ENTITY_ID_SEQ.nextval from dual";
                        CustId = _objectEntity.SqlQuery<double>(CustIDFetch).FirstOrDefault();

                        string SCHEMECUSTINSERT = $@"Insert into DIST_SCHEME_ENTITY_MAPPING (ENTITY_ID , SCHEME_ID, ENTITY_TYPE , ENTITY_CODE)
                            Values('{CustId}', '{schemeid}', 'D', '{customer}')";
                        row = _objectEntity.ExecuteSqlCommand(SCHEMECUSTINSERT);

                        }
                    
                        foreach(var reseller in model.ResellerCode)
                        {
                        var ResellerId = 0.00;
                        string ResellerIDFetch = "select ENTITY_ID_SEQ.nextval from dual";
                        ResellerId = _objectEntity.SqlQuery<double>(ResellerIDFetch).FirstOrDefault();

                        string SCHEMERESELLERINSERT = $@"Insert into DIST_SCHEME_ENTITY_MAPPING (ENTITY_ID , SCHEME_ID, ENTITY_TYPE, ENTITY_CODE )
                            Values('{ResellerId}', '{schemeid}', 'R', '{reseller}')";
                        row = _objectEntity.ExecuteSqlCommand(SCHEMERESELLERINSERT);

                        }
                    if (model.OfferType == "GIFT")
                    {

                        foreach (var dt in model.SchemeDetails)
                        {
                            var RuleId = 0.00;
                            string RuleIDFetch = "select SCHEME_RULE_ID_SEQ.nextval from dual";
                            RuleId = _objectEntity.SqlQuery<double>(RuleIDFetch).FirstOrDefault();

                            string DETAILSINSERT = $@"INSERT INTO DIST_SCHEME_RULE_MAPPING (RULE_ID, SCHEME_ID, MAX_VALUE, MIN_VALUE, GIFT_QTY)
                            VALUES('{RuleId}','{schemeid}', '{dt.Max_Value}', '{dt.Min_Value}', '{dt.GiftQty}')";
                            row = _objectEntity.ExecuteSqlCommand(DETAILSINSERT);

                            foreach (var item in dt.GiftItemCode)
                            {
                                string GIFTITEMINSERT = $@"INSERT INTO DIST_SCHEME_GIFT_ITEMS (RULE_ID, GIFT_ITEM_CODE)
                                VALUES ('{RuleId}', '{item}')";
                                row = _objectEntity.ExecuteSqlCommand(GIFTITEMINSERT);
                            }
                        }


                    }
                    else
                    {
                        foreach (var dt in model.SchemeDetails)
                        {
                            var RuleId = 0.00;
                            dt.DiscountType = dt.DiscountType == "VALUE" ? model.OfferType : dt.DiscountType;
                            string RuleIDFetch = "select SCHEME_RULE_ID_SEQ.nextval from dual";
                            RuleId = _objectEntity.SqlQuery<double>(RuleIDFetch).FirstOrDefault();

                            string DETAILSINSERT = $@"INSERT INTO DIST_SCHEME_RULE_MAPPING (RULE_ID, SCHEME_ID, MAX_VALUE, MIN_VALUE, DISCOUNT, DISCOUNT_TYPE )
                            VALUES('{RuleId}','{schemeid}', '{dt.Max_Value}', '{dt.Min_Value}', '{dt.Discount}', '{ dt.DiscountType}')";
                            row = _objectEntity.ExecuteSqlCommand(DETAILSINSERT);

                        }
                    }

                    _objectEntity.SaveChanges();
                    trans.Commit();
                    bool success=false;
                    if (model.Action == "Approve and Save")
                    {
                         success=ApproveScheme(schemeid.ToString(), "A");
                        if (success)
                        {
                            return "Success";
                        }
                        else
                        {
                            return "Error while approving";
                        }
                    }
                    else if(model.Action =="Check and Save")
                    {
                        success = ApproveScheme(schemeid.ToString(), "C");
                        if (success)
                        {
                            return "Success";
                        }
                        else
                        {
                            return "Error while approving";
                        }
                    }
                    return "Success";
                   
                }

                catch (Exception ex)
                {
                    trans.Rollback();
                    return "Error:"+ex;
                }
                
            }
        }


        public List<SchemeModel> GetAllScheme(User userInfo)
        {
            var result = new List<SchemeModel>();               
                //var query = $@"select ds.SCHEME_ID as SchemeID ,ds.SCHEME_NAME as SchemeName ,ds.AREA_CODE as AreaCode, da.AREA_NAME as AreaName,ds.MAX_VALUE as Max_Value ,ds.MIN_VALUE as Min_Value , ds.OFFER_TYPE as OfferType, ds.DISCOUNT ,ds.DISCOUNT_TYPE as DiscountType    from DIST_SCHEME ds,DIST_AREA_MASTER da where ds.AREA_CODE=da.AREA_CODE and  ds.COMPANY_CODE={userInfo.company_code} and ds.BRANCH_CODE={userInfo.branch_code} and ds.DELETED_FLAG='N'";
                var query = $@"select ds.SCHEME_ID as SchemeID ,ds.SCHEME_NAME as SchemeName , ds.Start_Date as StartDate, ds.End_Date as EndDate, ds.SP_CODE ,dl.Full_Name as Employee_Name,  ds.OFFER_TYPE as OfferType, ds.APPROVE_FLAG as Status, ds.CHECKED_FLAG as CheckedStatus from DIST_SCHEME ds,DIST_LOGIN_USER dl where ds.SP_CODE=dl.SP_CODE and  ds.COMPANY_CODE={userInfo.company_code} and ds.BRANCH_CODE={userInfo.branch_code} and ds.DELETED_FLAG='N'";
                result = this._objectEntity.SqlQuery<SchemeModel>(query).ToList();
            
            return result;
        }

        public List<ItemModel> GetSchemeItem(string Id, User userInfo)
        {
            var result = new List<ItemModel>();
            var query = $@"select sc.Item_code, it.Item_Edesc from DIST_SCHEME_ITEMS sc, IP_ITEM_MASTER_SETUP it where sc.ITEM_CODE=it.ITEM_CODE and sc.SCHEME_ID={Id} and it.COMPANY_CODE={userInfo.company_code} and it.BRANCH_CODE={userInfo.branch_code}";
            result = this._objectEntity.SqlQuery<ItemModel>(query).ToList();

            return result;
            
        }
        public List<SchemeGiftModel> GetGiftItem(string Id, User userInfo)
        {
            var Ruleresult = new List<SchemeGiftModel>();
            var Itemresult = new List<ItemModel>();
            //var query = $@"select it.Item_code, it.Item_Edesc from DIST_SCHEME_GIFT_ITEMS scg, IP_ITEM_MASTER_SETUP it where scg.GIFT_ITEM_CODE=it.ITEM_CODE and scg.SCHEME_ID={Id} and it.COMPANY_CODE={userInfo.company_code}  and  it.BRANCH_CODE={userInfo.branch_code}";
            var query = $@"select distinct sr.Rule_ID, sr.Max_Value, sr.Min_Value, sr.Gift_QTY from DIST_SCHEME_GIFT_ITEMS scg, DIST_SCHEME_RULE_MAPPING sr where scg.RULE_ID=sr.RULE_ID and sr.SCHEME_ID={Id}";
            Ruleresult = this._objectEntity.SqlQuery<SchemeGiftModel>(query).ToList();
            foreach (var rule in Ruleresult)
            {
                var query2 = $@"select distinct it.Item_code, it.Item_EDESC from DIST_SCHEME_GIFT_ITEMS scg, IP_ITEM_MASTER_SETUP it where scg.GIFT_ITEM_CODE=it.ITEM_CODE and scg.RULE_ID={rule.Rule_ID} and it.COMPANY_CODE={userInfo.company_code} and it.BRANCH_CODE={userInfo.branch_code}";
                Itemresult = this._objectEntity.SqlQuery<ItemModel>(query2).ToList();
                foreach(var item in Itemresult)
                {
                    rule.ItemCode.Add(item.ITEM_CODE);
                    rule.ItemName = rule.ItemName + ", " + item.ITEM_EDESC;
                }

                rule.ItemName = rule.ItemName.Trim(',');


            }
            return Ruleresult;
            
        }
        
        public List<SchemeDetailModel> GetOtherItem(string Id, User userInfo)
        {
            var result = new List<SchemeDetailModel>();
            var query = $@"select distinct sr.Rule_ID, sr.Max_Value, sr.Min_Value, sr.Discount, sr.Discount_Type as DiscountType from DIST_SCHEME_RULE_MAPPING sr where sr.SCHEME_ID={Id}";
            result = this._objectEntity.SqlQuery<SchemeDetailModel>(query).ToList();

            return result;

        }

        public int DeleteScheme(string Id)
        {
            var result = 0;
            string updateScheme = $@"update DIST_SCHEME set Deleted_Flag='Y'  WHERE SCHEME_ID='{Id}'";
            _objectEntity.ExecuteSqlCommand(updateScheme);
            result = 400;
            return result;

        }

        public List<EmployeeAreaModel> GetEmployeesandRoute(User userInfo)
        {
            var companyCode = userInfo.company_code;

            
            string getEmployees = $@"SELECT DISTINCT SPM.SP_CODE SP_CODE,ES.EMPLOYEE_CODE EMPLOYEE_CODE,ES.EMPLOYEE_EDESC || ' ('||ES.EMPLOYEE_CODE||')' EMPLOYEE_EDESC,
                    ES.EMPLOYEE_NDESC EMPLOYEE_NDESC,ES.GROUP_SKU_FLAG GROUP_SKU_FLAG,ES.MASTER_EMPLOYEE_CODE MASTER_EMPLOYEE_CODE,
                    ES.PRE_EMPLOYEE_CODE PRE_EMPLOYEE_CODE
                    FROM HR_EMPLOYEE_SETUP ES,DIST_SALESPERSON_MASTER SPM, DIST_LOGIN_USER LU
                    WHERE SPM.SP_CODE = LU.SP_CODE
                    AND SPM.COMPANY_CODE = LU.COMPANY_CODE
                    AND ES.DELETED_FLAG='N' AND SPM.ACTIVE='Y' AND LU.ACTIVE = 'Y'
                    AND SPM.SP_CODE=ES.EMPLOYEE_CODE 
                    AND ES.COMPANY_CODE = SPM.COMPANY_CODE
                    AND SPM.COMPANY_CODE = '{companyCode}'
                    ORDER BY LOWER(TRIM(ES.EMPLOYEE_EDESC || ' ('||ES.EMPLOYEE_CODE||')'))";
            var result = this._objectEntity.SqlQuery<EmployeeAreaModel>(getEmployees).ToList();

            foreach(var data in result)
            {
                string getRoute = $@"select AREA_CODE from DIST_USER_AREAS where SP_CODE='{data.EMPLOYEE_CODE}'";
                var areas = this._objectEntity.SqlQuery<string>(getRoute).ToList();
                data.AREA_CODE = areas;


            }


            return result;

        }


        public List<AreaModel> GetSchemeArea(string Id)
        {

            var result = new List<AreaModel>();
            var query = $@"select dm.AREA_CODE, dm.AREA_NAME from DIST_SCHEME_AREA_MAPPING da,DIST_AREA_MASTER dm where da.AREA_CODE=dm.AREA_CODE and SCHEME_ID={Id}";
            result = this._objectEntity.SqlQuery<AreaModel>(query).ToList();

            return result;

        } 
        
        public bool ApproveScheme(string Id, string Action)
        {
            bool result=false;
            if (Action == "A")
            {
                string updateScheme = $@"update DIST_SCHEME set Approve_Flag='Y'  WHERE SCHEME_ID='{Id}'";
                _objectEntity.ExecuteSqlCommand(updateScheme);
                result = true;

            }
            else if (Action=="R")
            {
                string updateScheme = $@"update DIST_SCHEME set Approve_Flag='N'  WHERE SCHEME_ID='{Id}'";
                _objectEntity.ExecuteSqlCommand(updateScheme);
                result = true;

            }
            else if(Action == "C")
            {
                string updateScheme = $@"update DIST_SCHEME set Checked_Flag='Y'  WHERE SCHEME_ID='{Id}'";
                _objectEntity.ExecuteSqlCommand(updateScheme);
                result = true;
            }
            else if(Action == "CR")
            {
                string updateScheme = $@"update DIST_SCHEME set Checked_Flag='N'  WHERE SCHEME_ID='{Id}'";
                _objectEntity.ExecuteSqlCommand(updateScheme);
                result = true;
            }
            return result;

        }
        


    }
}
