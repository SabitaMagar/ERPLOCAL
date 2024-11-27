using NeoErp.Core.Caching;
using NeoErp.Core.Models.CustomModels;
using NeoErp.Core.Services;
using NeoErp.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace NeoErp.Models.Settings
{
    public class MenuSettingsModel : IMenuSettings
    {
        private ICacheManager _cacheManager;
        private IDbContext _dbContext;

        public MenuSettingsModel(ICacheManager cacheManager, IDbContext dbContext)
        {
            this._cacheManager = cacheManager;
            this._dbContext = dbContext;
        }

        public List<MenuSettingsEntities> GetMenuForSettings()
        {
            string MenuSql = "SELECT mm.MENU_NO, mm.MENU_EDESC, mm.MENU_OBJECT_NAME, mm.MODULE_CODE,ms.MODULE_EDESC, mm.FULL_PATH, mm.VIRTUAL_PATH, mm.GROUP_SKU_FLAG,mm.PRE_MENU_NO,mm.ICON_PATH, mm.COMPANY_CODE,mm.MODULE_ABBR,mm.COLOR,mm.DESCRIPTION FROM WEB_MENU_MANAGEMENT mm INNER JOIN WEB_MODULE_SETUP ms ON mm.MODULE_CODE = ms.MODULE_CODE";
            return _dbContext.SqlQuery<MenuSettingsEntities>(MenuSql).ToList();
        }
        public string UpdateMenuForSettings()
        {
            string MenuSql = "UPDATE WEB_MENU_MANAGEMENT SET MENU_NO=";
            var num = 0;
            try
            {
                num = this._dbContext.ExecuteSqlCommand(MenuSql);
            }
            catch (Exception ex)
            {
                throw ex;
            }
            return num.ToString();
        }

        public string CreateMenuForSettings()
        {
            string MenuSql = "INSERT INTO WEB_MENU_MANAGEMENT VALUES()";
            var num = 0;
            try
            {
                num = this._dbContext.ExecuteSqlCommand(MenuSql);
            }
            catch (Exception ex)
            {
                throw ex;
            }
            return num.ToString();
        }

        public string DeleteMenuForSettings()
        {
            string checkSql = "SELECT count(*) from web_menu_control where menu_no =";
            string MenuSql = "DELETE FROM WEB_MENU_MANAGEMENT WHERE MENU_NO = ";
            var num = 0;
            try
            {
                num = this._dbContext.ExecuteSqlCommand(MenuSql);
            }
            catch (Exception ex)
            {
                throw ex;
            }
            return num.ToString();
        }

        public MenuSettingsEntities GetMenuForSettings(string menuNo)
        {
            var entities = new MenuSettingsEntities();
            try
            {
                var a = new List<MenuSettingsEntities>();
                string MenuSql = "SELECT MENU_NO, MENU_EDESC, MENU_OBJECT_NAME, MODULE_CODE, FULL_PATH, VIRTUAL_PATH, GROUP_SKU_FLAG,PRE_MENU_NO, COMPANY_CODE FROM WEB_MENU_MANAGEMENT WHERE MENU_NO = " + menuNo + "";
                MenuSql = MenuSql.Replace("\"", "'");
                //var a = this._dbContext.ExecuteSqlCommand(MenuSql);
                //entities = this._dbContext.SqlQuery<MenuSettingsEntities>(MenuSql).FirstOrDefault();
                //string MenuSql = "SELECT  MENU_NO,MENU_EDESC,MENU_OBJECT_NAME,MODULE_CODE,COMPANY_CODE FROM WEB_MENU_MANAGEMENT WHERE MENU_NO ='" + menuNo + "'";
                a = _dbContext.SqlQuery<MenuSettingsEntities>(MenuSql).ToList();

                //a = _dbContext.SqlQuery<MenuSettingsEntities>(MenuSql).ToList();
                entities = a.FirstOrDefault();
            }
            catch (Exception ex)
            {
                throw ex;
            }
            return entities;

        }

        public string UpdateMenuForSettings(MenuSettingsEntities menu)
        {
            var num = 0;
            try
            {
                string MenuSql = "UPDATE WEB_MENU_MANAGEMENT SET MENU_EDESC ='" + menu.MENU_EDESC + "', MENU_OBJECT_NAME ='" + menu.MENU_OBJECT_NAME + "', MODULE_CODE ='" + menu.MODULE_CODE + "', VIRTUAL_PATH ='" + menu.VIRTUAL_PATH + "', FULL_PATH ='" + menu.FULL_PATH + "', ICON_PATH ='" + menu.ICON_PATH + "', PRE_MENU_NO='" + menu.PRE_MENU_NO + "',MODULE_ABBR='" + menu.MODULE_ABBR + "',COLOR='" + menu.COLOR + "',DESCRIPTION='" + menu.DESCRIPTION + "' WHERE MENU_NO='" + menu.MENU_NO + "'";
                //MenuSql = MenuSql.Replace("\"", "'");
                num = this._dbContext.ExecuteSqlCommand(MenuSql);
            }
            catch (Exception ex)
            {
                throw ex;
            }
            return num.ToString();
        }

        public string CreateMenuForSettings(MenuSettingsEntities menu)
        {
            // Menu No generate
            var menuNos = "";
            var num = 0;
            menuNos = GetMenuNo(menu.PRE_MENU_NO);
            if (menuNos != null || menuNos != "")
            {
                var menuNumber = Convert.ToInt32(menuNos) + 1;
                menuNos = menu.PRE_MENU_NO == "00" ? ((menuNumber).ToString()).PadLeft(2, '0') : menu.PRE_MENU_NO + "." + ((menuNumber).ToString()).PadLeft(2, '0');
            }
            else
            {
                TestMenu tsm = new TestMenu();
                var menuNumber = Convert.ToInt32(tsm.MENU_NO) + 1;
                menuNos = ((menuNumber).ToString()).PadLeft(2, '0');
            }
            //var SYSDATE = "15/9/2016";           
            menu.COMPANY_CODE = "01";
            if (menu.GROUP_SKU_FLAG == "G")
            {
                menu.FULL_PATH = "/" + menu.MENU_EDESC;
            }
            string MenuSql = "INSERT INTO WEB_MENU_MANAGEMENT (MENU_NO,MENU_EDESC, MENU_OBJECT_NAME, MODULE_CODE, FULL_PATH, VIRTUAL_PATH, ICON_PATH, PRE_MENU_NO, COMPANY_CODE, GROUP_SKU_FLAG, CREATED_BY, CREATED_DATE,MODULE_ABBR,COLOR,DESCRIPTION)" +
                " VALUES('" + menuNos + "','" + menu.MENU_EDESC + "','" + menu.MENU_OBJECT_NAME + "','" + menu.MODULE_CODE + "','" + menu.FULL_PATH + "','" + menu.VIRTUAL_PATH + "','" + menu.ICON_PATH + "','" + menu.PRE_MENU_NO + "','" + menu.COMPANY_CODE + "','" + menu.GROUP_SKU_FLAG + "','" + menu.MODULE_CODE + "',SYSDATE,'" + menu.MODULE_ABBR + "','" + menu.COLOR + "','" + menu.DESCRIPTION + "')";
            try
            {
                num = this._dbContext.ExecuteSqlCommand(MenuSql);
            }
            catch (Exception ex)
            {
                throw ex;
            }
            return menuNos.ToString();
        }

        public string GetMenuNo(string preMenuNo)
        {
            var query = "SELECT MAX(MENU_NO)as MENU_NO FROM WEB_MENU_MANAGEMENT WHERE PRE_MENU_NO='" + preMenuNo + "' order by MENU_NO desc";
            //var MENUNO = new TestMenu();
            var menuNo = "";
            try
            {
                menuNo = _dbContext.SqlQuery<string>(query).FirstOrDefault();
                if (menuNo.Contains('.'))
                    menuNo = menuNo.Substring(menuNo.IndexOf('.') + 1);
                //if (MENUNO.MENU_NO == 0)
                //{
                //    MENUNO.MENU_NO = 0;
                //}
                // menuNo = MENUNO.MENU_NO.ToString();
            }
            catch (Exception ex)
            {
               // throw ex;
            }
            return menuNo;
        }
        public class TestMenu
        {
            public string MENU_NO { get; set; } = "0";
        }

        public string DeleteMenuForSettings(string MenuNo)
        {
            var num = 0;
            var count = 0;
            try
            {
                string checkSql = "SELECT COUNT(*)COUNT FROM WEB_MENU_CONTROL WHERE MENU_NO =" + MenuNo + "";
                checkSql = checkSql.Replace("\"", "'");
                count = _dbContext.SqlQuery<int>(checkSql).First();
                if (count > 0)
                {
                    return "count";
                }
                else
                {
                    string MenuSql = "DELETE FROM WEB_MENU_MANAGEMENT WHERE MENU_NO =" + MenuNo + "";
                    MenuSql = MenuSql.Replace("\"", "'");
                    num = this._dbContext.ExecuteSqlCommand(MenuSql);
                }

            }
            catch (Exception ex)
            {
                throw ex;
            }
            return num.ToString();
        }
        public List<ModuleEntities> GetModuleForMenu()
        {
            string MenuSql = "SELECT DISTINCT MODULE_CODE,MODULE_EDESC FROM WEB_MODULE_SETUP";
            List<ModuleEntities> moduleList;
            try
            {
                moduleList = this._dbContext.SqlQuery<ModuleEntities>(MenuSql).ToList();
            }
            catch (Exception ex)
            {
                throw ex;
            }
            return moduleList;
        }

        public List<MenuSettingsEntities> GetPreMenu()
        {
            string MenuSql = "SELECT MENU_NO, PRE_MENU_NO, LPAD(' ', 2*level-1) || SYS_CONNECT_BY_PATH(MENU_EDESC,' / ') MENU_EDESC, GROUP_SKU_FLAG FROM WEB_MENU_MANAGEMENT"
                            + " START WITH MENU_NO IN (SELECT MENU_NO FROM WEB_MENU_MANAGEMENT WHERE PRE_MENU_NO = '00') CONNECT BY PRIOR MENU_NO = PRE_MENU_NO";
            List<MenuSettingsEntities> moduleList;
            try
            {
                moduleList = this._dbContext.SqlQuery<MenuSettingsEntities>(MenuSql).Where(x => x.GROUP_SKU_FLAG == "G").ToList();
            }
            catch (Exception ex)
            {
                throw ex;
            }
            return moduleList;
        }
        public List<MenuSetupModel> MenuListAllNodes()
        {
            string query = @"SELECT LEVEL,INITCAP(mm.MENU_EDESC)MENU_EDESC,mm.MENU_NO as MENU_NO,
            mm.GROUP_SKU_FLAG,mm.MENU_NO as MASTER_MENU_CODE ,mm.PRE_MENU_NO, LEVEL, mm.FULL_PATH, mm.VIRTUAL_PATH, mm.MODULE_CODE,mm.ORDERBY
            FROM WEB_MENU_MANAGEMENT mm
            WHERE
            LEVEL = 1
            START WITH PRE_MENU_NO = '00'
            CONNECT BY PRIOR MENU_NO = PRE_MENU_NO
            ORDER SIBLINGS BY ORDERBY";
            //string query = @"SELECT LEVEL,INITCAP(CS.CUSTOMER_EDESC)MENU_EDESC, CS.CUSTOMER_CODE as MENU_NO,
            //CS.GROUP_SKU_FLAG,CS.MASTER_CUSTOMER_CODE as MASTER_MENU_CODE ,CS.PRE_CUSTOMER_CODE as PRE_MENU_NO,  LEVEL
            //FROM SA_CUSTOMER_SETUP CS
            //WHERE 
            //LEVEL = 1 AND ROWNUM <=2
            //START WITH PRE_CUSTOMER_CODE = '00'
            //CONNECT BY PRIOR MASTER_CUSTOMER_CODE = PRE_CUSTOMER_CODE
            //ORDER SIBLINGS BY CUSTOMER_EDESC";
            var menuListNodes = _dbContext.SqlQuery<MenuSetupModel>(query).ToList();
            return menuListNodes;
        }

        public List<MenuSetupModel> GetMenuListByMenuNo(string level, string masterCode)
        {
            string query = string.Format(@"SELECT LEVEL,INITCAP(MENU_EDESC)MENU_EDESC,MENU_NO,
            GROUP_SKU_FLAG,MENU_NO as MASTER_MENU_CODE ,PRE_MENU_NO, LEVEL, FULL_PATH, VIRTUAL_PATH, MODULE_CODE, ORDERBY
            FROM WEB_MENU_MANAGEMENT 
            WHERE LEVEL = {0}
            START WITH PRE_MENU_NO = '{1}'
            CONNECT BY PRIOR MENU_NO = PRE_MENU_NO
            ORDER SIBLINGS BY ORDERBY", level.ToString(), masterCode.ToString());

            //string query = string.Format(@"SELECT LEVEL,INITCAP(CS.CUSTOMER_EDESC) MENU_EDESC,GROUP_SKU_FLAG,
            //CUSTOMER_CODE as MENU_NO, CS.MASTER_CUSTOMER_CODE as MASTER_MENU_CODE, CS.PRE_CUSTOMER_CODE as PRE_MENU_NO, CS.BRANCH_CODE
            //FROM SA_CUSTOMER_SETUP CS
            //WHERE CS.DELETED_FLAG = 'N'
            //AND CS.COMPANY_CODE = '01'
            //AND LEVEL = {0}
            //START WITH PRE_CUSTOMER_CODE = '{1}'
            //CONNECT BY PRIOR MASTER_CUSTOMER_CODE = PRE_CUSTOMER_CODE
            //ORDER SIBLINGS BY CUSTOMER_EDESC", level.ToString(), masterCode.ToString());
            var menuListNodes = _dbContext.SqlQuery<MenuSetupModel>(query).ToList();
            return menuListNodes;
        }

        public List<SammyEntity> GetMenuRouting(string moduleCode)
        {
            var query = $"SELECT VIRTUAL_PATH, FULL_PATH FROM WEB_MENU_MANAGEMENT WHERE MODULE_CODE ='{moduleCode}'";
            List<SammyEntity> menuControlList;
            try
            {
                menuControlList = _dbContext.SqlQuery<SammyEntity>(query).ToList();
            }
            catch (Exception ex)
            {

                throw ex;
            }
            return menuControlList;
        }
        public string getChangeMenuOrder(List<MenuOrderModels> modal)
        {
            var num = 0;
            for (int i = 0; i < modal.Count(); i++)
            {
                string MenuSql = $@"UPDATE WEB_MENU_MANAGEMENT SET ORDERBY = '{modal[i].ORDER}' WHERE MENU_NO= '{modal[i].MENU_NO}'";
                try
                {
                    num = this._dbContext.ExecuteSqlCommand(MenuSql);
                }
                catch (Exception ex)
                {
                    throw ex;
                }
            }
            return num.ToString();
        }
    }
}