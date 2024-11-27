using System;
using System.Collections.Generic;
using System.Linq;
using PagedList;
using System.Web.Mvc;
using NeoErp.Models;
using NeoErp.Models.Common;
using NeoErp.Services.LogService;
using NeoErp.Core;
using NeoErp.Core.Services;
using NeoErp.Core.Domain;
using NeoErp.Core.Caching;
using NeoErp.Core.Plugins;
using System.Xml.Linq;
using System.Xml;
using System.Configuration;
using NeoErp.Services;
using NeoErp.Data;
using NeoErp.Models.QueryBuilder;
using NeoErp.Core.Services.CommonSetting;
using NeoErp.Core.Models.Log4NetLoggin;

namespace NeoErp.Controllers
{
    public class HomeController : Controller
    {

        private IWorkContext _workContext;
        private IAuthenticationService _authenticationService;
        private IMenuModel _menuService;
        private readonly ICacheManager _cacheManager;
        private IPluginFinder _pluginFinder;
        private IQueryBuilder _queryService;
        private IDbContext _dbContext;
        private ISettingService _settingService;
        private ILogViewer _logViewer;
        private ILogErp _logErp;
        // public const string Men= "";
        private string dealerPortalLogin = ConfigurationManager.AppSettings["DealerPortalLogin"];

        public HomeController(IWorkContext workContext, IAuthenticationService authenticationService, IMenuModel menuService, ICacheManager cacheManager, IPluginFinder pluginFinder, IQueryBuilder queryService, IDbContext dbContext, ISettingService settingService, ILogViewer logViewer, ILogErp logErp)
        {
            this._workContext = workContext;
            this._authenticationService = authenticationService;
            this._menuService = menuService;
            this._cacheManager = cacheManager;
            this._pluginFinder = pluginFinder;
            this._queryService = queryService;
            this._dbContext = dbContext;
            this._settingService = settingService;
            this._logViewer = logViewer;
            this._logErp = new LogErp(this);

        }

        public ActionResult LogIn()
        {
            Session.Clear();
            if (dealerPortalLogin == "true")
            {
                return Redirect("/distribution/distributor/login");
            }
            else
            {
                return View();
            }
        }

        public ActionResult LogInNew()
        {
            ViewBag.error = false;

            return View();
        }
        [HttpPost]
        [ValidateInput(false)]
        public ActionResult LogIn(User model)
        {
            try
            {
                ViewBag.error = false;
                if (string.IsNullOrEmpty(model.company_name))
                {
                    ViewBag.error = true;
                    ViewBag.Message = "Please Select Company.";
                    return View();
                }
                if (string.IsNullOrEmpty(model.branch_code))
                {
                    ViewBag.error = true;
                    ViewBag.Message = "Please Select Branch.";
                    return View();
                }
                var result = _authenticationService.SignIn(model.UserName, model.Password, model.company_name, model.branch_code).ToList();
                _logErp.InfoInFile($@"User authenticated for username : {model.UserName}  password: {model.Password} company:{model.company_name} and branch: {model.branch_name}");
                if (result.Count > 0)
                {
                    User userData = result.First();
                    userData.Branch = userData.branch_code;
                    userData.company_code = userData.Company;
                    _logErp.InfoInFile($@"User Data Contains {userData.branch_code} as branch code and {userData.company_code} as company code");

                    if (result.Count > 0 && userData.Company != null && userData.company_code != null && userData.Branch != null && userData.branch_code != null)
                    {
                        _workContext.CurrentUserinformation = userData;
                        _authenticationService.SignIn(userData, true);
                        _logErp.WarnInDB("authentication service . signin successfull using : " + userData.UserName + " " + userData.Password);
                        try
                        {
                            var filename = $"{_workContext.CurrentUserinformation.User_id.ToString()}{_workContext.CurrentUserinformation.company_code}";

                            var setting = _settingService.LoadSetting<UserDashboardSetting>(filename);
                            if (setting != null)
                            {

                                if (!string.IsNullOrEmpty(setting.DefaultPath))
                                    return Redirect(setting.DefaultPath);
                                return RedirectToAction("GlobalDashboard", "main");
                            }
                        }
                        catch (Exception ex)
                        {
                            _logErp.ErrorInDB("Error while authenticating : " + ex.Message + ex.StackTrace);
                            return RedirectToAction("GlobalDashboard", "main");
                        }

                        return RedirectToAction("GlobalDashboard", "main");

                    }
                    else
                    {
                        _logErp.ErrorInDB("Failed to load company or branch");
                        ViewBag.error = true;
                        ViewBag.Message = "Failed to load company or branch.";
                        return View();
                    }
                }

                else
                {
                    _logErp.ErrorInDB("Username or password is incorrect");
                    ViewBag.error = true;
                    ViewBag.Message = "Username or password is incorrect.";
                    return View();
                }
            }
            catch (Exception ex)
            {
                _logErp.ErrorInDB("Error while authenticating : " + ex.Message + ex.StackTrace);
                ViewBag.error = true;
                ViewBag.Message = ex.Message;
                return View();
            }
        }


        [HttpPost]
        [ValidateInput(false)]
        public ActionResult LogInMobile(User model)
        {
            model.company_name = "01";
            model.branch_code = "01.01";
            var result = _authenticationService.SignIn(model.UserName, model.Password, model.company_name, model.branch_code).ToList();
            if (result.Count > 0)
            {
                User userData = result.First();
                userData.Branch = userData.branch_code;
                userData.company_code = userData.Company;

                if (result.Count > 0 && userData.Company != null && userData.company_code != null && userData.Branch != null && userData.branch_code != null)
                {
                    _workContext.CurrentUserinformation = userData;
                    _authenticationService.SignIn(userData, true);
                    return Json("success");
                }
                else
                {
                    ViewBag.Message = "Failed to load company or branch.";
                    return Json("error");
                }
            }

            else
            {
                ViewBag.Message = "Username or password is incorrect.";
                return Json("error");
            }
        }

        public ActionResult PluginList()
        {
            var pluginDescriptors = _pluginFinder.GetPluginDescriptors(LoadPluginsMode.All, 0).ToList();
            return View(pluginDescriptors);
        }




        public ActionResult Uninstall(string systemname)
        {
            try
            {
                //get plugin system name
                string systemName = systemname;
                var pluginDescriptor = _pluginFinder.GetPluginDescriptorBySystemName(systemName, LoadPluginsMode.All);
                if (pluginDescriptor == null)
                    //No plugin found with the specified id
                    return RedirectToAction("List");

                //check whether plugin is installed
                if (!pluginDescriptor.Installed)
                    return RedirectToAction("List");
                //uninstall plugin
                pluginDescriptor.Instance().Uninstall();
                //restart application
                NeoErp.Core.WebHelper helper = new Core.WebHelper(HttpContext);
                helper.RestartAppDomain(true, "/Main/DashBoard");
            }
            catch (Exception exc)
            {

            }

            return RedirectToAction("List");
        }

        public ActionResult install(string systemname)
        {
            try
            {
                var pluginDescriptor = _pluginFinder.GetPluginDescriptorBySystemName(systemname, LoadPluginsMode.All);
                if (pluginDescriptor == null)
                    //No plugin found with the specified id
                    return RedirectToAction("List");

                //check whether plugin is not installed
                if (pluginDescriptor.Installed)
                    return RedirectToAction("List");

                //install plugin
                pluginDescriptor.Instance().Install();
                NeoErp.Core.WebHelper helper = new Core.WebHelper(HttpContext);
                helper.RestartAppDomain(true, "/Main/DashBoard");

            }
            catch (Exception exc)
            {

            }

            return RedirectToAction("List");
        }

        public ActionResult LogOut(LoginModel model)
        {
            Session.Clear();
            return RedirectToLocal("LogIn");
        }


        private ActionResult RedirectToLocal(string returnUrl)
        {
            _authenticationService.SignOut();
            return RedirectToAction("LogIn", "Home");
        }
        [AcceptVerbs(HttpVerbs.Get)]
        public ActionResult ListCompany(string UserName)
        {
            IEnumerable<User> data = _authenticationService.GetCompanyList(UserName);
            return Json(data, JsonRequestBehavior.AllowGet);


        }
        [AcceptVerbs(HttpVerbs.Get)]
        public ActionResult ListBranch(string UserName)
        {
            IEnumerable<BranchInfo> data = _authenticationService.GetCompanyBranchList(UserName);
            return Json(data, JsonRequestBehavior.AllowGet);


        }

        [AcceptVerbs(HttpVerbs.Get)]
        public ActionResult CompanyBranchList(string UserName, string Password)
        {
            IEnumerable<CompanyBranchModel> data = _authenticationService.GetAllCompanyBranch(UserName, Password);
            return Json(data, JsonRequestBehavior.AllowGet);
        }

        [AcceptVerbs(HttpVerbs.Get)]
        public ActionResult ListCompanyBranch(List<string> company_code, string UserName)
        {
            var Company = company_code.FirstOrDefault();
            if (Company == null)
                return null;

            if (Company == "null")
                return null;

            if (string.IsNullOrEmpty(Company))
                return null;



            IEnumerable<BranchInfo> data = _authenticationService.GetBranchControlList(Company, UserName);
            return Json(data, JsonRequestBehavior.AllowGet);
        }
        [HttpPost]
        public bool UpdateCompanyCode(string companyCode)
        {
            User userData = _workContext.CurrentUserinformation;
            userData.company_code = companyCode;
            _workContext.CurrentUserinformation = userData;
            _authenticationService.UpdateAuthenticatedCustomer(userData);
            return true;
        }
        //xml 
        [HttpPost]
        public void SaveReportSetting(string reportName, string columnName)
        {

            string path = Server.MapPath("~/App_Data/UserReportSetting.xml");
            XmlWriterSettings xmlWriterSettings = new XmlWriterSettings();
            xmlWriterSettings.Indent = true;
            xmlWriterSettings.NewLineOnAttributes = true;

            if (System.IO.File.Exists(path) == false)
            {
                using (XmlWriter writer = XmlWriter.Create(path, xmlWriterSettings))
                {
                    writer.WriteStartDocument();
                    writer.WriteStartElement("UserReportSetup");
                    //generate user node
                    writer.WriteStartElement("User");
                    writer.WriteAttributeString("ID", User.Identity.Name.ToUpper());
                    writer.WriteAttributeString("Name", User.Identity.Name.ToUpper());

                    //generate report file node
                    writer.WriteStartElement("Report");
                    writer.WriteAttributeString("ID", reportName);
                    writer.WriteAttributeString("Name", reportName);

                    //Insert hidden column name
                    writer.WriteElementString("hiddenColumnName", columnName);
                    writer.WriteEndElement();
                    writer.WriteEndElement();
                    writer.WriteEndElement();
                    writer.WriteEndDocument();

                }
            }
            else // file exist case
            {

                XDocument xDocument = XDocument.Load(path);
                XElement userReportPresent = xDocument.Descendants("UserReportSetup")
                    .Descendants("User").Where(x => (string)x.Attribute("ID") == User.Identity.Name.ToUpper())
                    .Descendants("Report").Where(x => (string)x.Attribute("ID") == reportName).FirstOrDefault();
                if (userReportPresent != null) //update old value
                {
                    //for hidden column
                    try
                    {
                        userReportPresent.Elements("hiddenColumnName").Single().Value = columnName;
                    }
                    catch
                    {
                        userReportPresent.Add(new XElement("hiddenColumnName", columnName));
                    }
                }
                else //generate new node
                {
                    var userNode = xDocument.Element("UserReportSetup").Descendants("User").Where(x => (string)x.Attribute("ID") == User.Identity.Name.ToUpper());
                    XElement root;
                    if (userNode.Count() > 0)
                    {
                        root = userNode.FirstOrDefault();
                        root.Add(new XElement("Report",
                                   new XAttribute("ID", reportName),
                                   new XAttribute("Name", reportName),
                                   new XElement("hiddenColumnName", columnName)
                                   ));
                    }
                    else
                    {
                        root = xDocument.Element("UserReportSetup");
                        //add user node
                        root.Add(new XElement("User",
                                 new XAttribute("ID", User.Identity.Name.ToUpper()),
                                 new XAttribute("Name", User.Identity.Name.ToUpper()),
                                 new XElement("Report",
                                new XAttribute("ID", reportName),
                                new XAttribute("Name", reportName),
                                new XElement("hiddenColumnName", columnName)
                                )));
                    }
                }
                xDocument.Save(path);
            }

        }
        [HttpPost]
        public void SaveReportConfigSetting(ReportSettingModel model)
        {
            string path = Server.MapPath("~/App_Data/UserReportSetting.xml");
            XmlWriterSettings xmlWriterSettings = new XmlWriterSettings();
            xmlWriterSettings.Indent = true;
            xmlWriterSettings.NewLineOnAttributes = true;

            if (System.IO.File.Exists(path) == false) //if not exist case
            {
                using (XmlWriter writer = XmlWriter.Create(path, xmlWriterSettings))
                {
                    writer.WriteStartDocument();
                    writer.WriteStartElement("UserReportSetup");
                    //generate user node
                    writer.WriteStartElement("User");
                    writer.WriteAttributeString("ID", User.Identity.Name.ToUpper());
                    writer.WriteAttributeString("Name", User.Identity.Name.ToUpper());

                    //generate report file node
                    writer.WriteStartElement("Report");
                    writer.WriteAttributeString("ID", model.reportName);
                    writer.WriteAttributeString("Name", model.reportName);




                    //save field to file                   
                    foreach (System.Reflection.PropertyInfo prop in typeof(ReportSettingModel).GetProperties())
                    {
                        var value = prop.GetValue(model);
                        if (value != null)
                        {
                            if (value.ToString().Contains("System.Collections.Generic.List`1"))
                            {
                                var list = value as List<string>;
                                if (list != null && list.Count() > 0)
                                {
                                    string listValue = string.Join(",", list);
                                    writer.WriteElementString(prop.Name, listValue);
                                }
                            }
                            else
                                writer.WriteElementString(prop.Name, value.ToString());
                        }
                    }

                    writer.WriteEndElement();
                    writer.WriteEndDocument();
                }
            }
            else // file exist case
            {

                XDocument xDocument = XDocument.Load(path);
                XElement userReportPresent = xDocument.Descendants("UserReportSetup")
                    .Descendants("User").Where(x => (string)x.Attribute("ID") == User.Identity.Name.ToUpper())
                    .Descendants("Report").Where(x => (string)x.Attribute("ID") == model.reportName).FirstOrDefault();
                if (userReportPresent != null)//delete old node
                {
                    userReportPresent.Remove();
                }
                //generate new node 

                var userNode = xDocument.Element("UserReportSetup").Descendants("User").Where(x => (string)x.Attribute("ID") == User.Identity.Name.ToUpper());
                XElement root;
                if (userNode.Count() > 0)
                {
                    root = userNode.FirstOrDefault();
                    root.Add(new XElement("Report",
                               new XAttribute("ID", model.reportName),
                               new XAttribute("Name", model.reportName)
                               ));
                }
                else
                {
                    root = xDocument.Element("UserReportSetup");

                    //add user node
                    root.Add(new XElement("User",
                                 new XAttribute("ID", User.Identity.Name.ToUpper()),
                                 new XAttribute("Name", User.Identity.Name.ToUpper()),
                                 new XElement("Report",
                                new XAttribute("ID", model.reportName),
                                new XAttribute("Name", model.reportName)
                                )));
                }

                // var rootUser = root.Descendants("User").Where(x => (string)x.Attribute("ID") == User.Identity.Name.ToUpper()).FirstOrDefault();

                XElement rootUserReport = xDocument.Descendants("UserReportSetup")
                .Descendants("User").Where(x => (string)x.Attribute("ID") == User.Identity.Name.ToUpper())
                .Descendants("Report").Where(x => (string)x.Attribute("ID") == model.reportName).FirstOrDefault();


                //save field to file                   
                foreach (System.Reflection.PropertyInfo prop in typeof(ReportSettingModel).GetProperties())
                {
                    var value = prop.GetValue(model, null);
                    if (value != null)
                    {
                        if (value.ToString().Contains("System.Collections.Generic.List`1"))
                        {
                            var list = value as List<string>;
                            if (list != null && list.Count() > 0)
                            {
                                string listValue = string.Join(",", list);
                                rootUserReport.Add(new XElement(prop.Name, listValue));
                            }
                        }
                        else
                            rootUserReport.Add(new XElement(prop.Name, value.ToString()));
                    }
                }


                xDocument.Save(path);
            }

        }

        T GetObject<T>(IDictionary<string, object> dict)
        {
            Type type = typeof(T);
            var obj = Activator.CreateInstance(type);

            foreach (var kv in dict)
            {
                type.GetProperty(kv.Key).SetValue(obj, kv.Value);
            }
            return (T)obj;
        }

        [HttpPost]
        public JsonResult GetReportSetting(string reportName)
        {
            try
            {
                string xmlpath = Server.MapPath("~/App_Data/UserReportSetting.xml");
                var xml = XDocument.Load(xmlpath);
                var result = xml.Descendants("UserReportSetup")
                    .Descendants("User").Where(x => (string)x.Attribute("ID") == User.Identity.Name.ToUpper())
                    .Descendants("Report").Where(x => (string)x.Attribute("ID") == reportName).FirstOrDefault();

                dynamic dynamicModel = new System.Dynamic.ExpandoObject();
                foreach (var xe in result.Descendants())
                {
                    var value = xe.Value;
                    string[] excludeList = { "Amount", "Quantity", "Rate", "Range" };
                    if (xe.Name.LocalName.Contains("Filter") && !excludeList.Any(s => xe.Name.LocalName.Contains(s)) && xe.Value != null)
                        ((IDictionary<String, object>)dynamicModel)[xe.Name.LocalName] = xe.Value.Split(',').ToList();
                    else
                        ((IDictionary<String, object>)dynamicModel)[xe.Name.LocalName] = xe.Value;
                }

                ReportSettingModel reportSettingModel = GetObject<ReportSettingModel>(dynamicModel);
                return Json(reportSettingModel, JsonRequestBehavior.AllowGet);
            }
            catch (Exception e)
            {
                ReportSettingModel reportSettingModel = new ReportSettingModel();
                return Json(reportSettingModel, JsonRequestBehavior.AllowGet);
            }

        }


        [HttpPost]
        public int DeleteFavouriteMenu(string menuName)
        {
            string path = Server.MapPath("~/App_Data/UserReportSetting.xml");
            int status = 0;
            if (System.IO.File.Exists(path))
            {
                XDocument xDocument = XDocument.Load(path);
                XElement userReportPresent = xDocument.Descendants("UserReportSetup")
                    .Descendants("User").Where(x => (string)x.Attribute("ID") == User.Identity.Name.ToUpper())
                    .Descendants("Report").Where(x => (string)x.Attribute("ID") == menuName).FirstOrDefault();
                if (userReportPresent != null)
                {
                    userReportPresent.Remove();
                    xDocument.Save(path);
                    status = 200;
                }
                else
                    status = 304; //not modified
            }
            else
                status = 404;

            return status;
        }

        [HttpGet]
        public JsonResult GetFavroiteMenus(string moduleCode = "01")
        {
            try
            {
                string xmlpath = Server.MapPath("~/App_Data/UserReportSetting.xml");
                var xml = XDocument.Load(xmlpath);
                var result = xml.Descendants("UserReportSetup")
                    .Descendants("User").Where(x => (string)x.Attribute("ID") == User.Identity.Name.ToUpper())
                    .Descendants("Report").Where(x => (string)x.Element("isFavroite") == "true" && (string)x.Element("moduleCode") == moduleCode);


                string json = Newtonsoft.Json.JsonConvert.SerializeObject(result.Reverse().ToList(), Newtonsoft.Json.Formatting.Indented).Replace('@', 'R');
                return Json(json, JsonRequestBehavior.AllowGet);
            }
            catch (Exception e)
            {

                return Json(null, JsonRequestBehavior.AllowGet);
            }

        }


        [HttpPost]
        public JsonResult GetFiscalYearSetting()
        {
            try
            {
                var connStr = ConfigurationManager.ConnectionStrings["NeoErpCoreEntity"].ConnectionString.Split(';');
                var index = Array.FindIndex(connStr, x => x.Contains("USER ID"));
                var userName = connStr[index].Substring(connStr[index].IndexOf("=") + 1).TrimEnd('"');

                string xmlpath = Server.MapPath("~/App_Data/FiscalYearDBSetup.xml");
                var xml = XDocument.Load(xmlpath);
                var result = xml.Descendants("DBSetup").Descendants("Company").Where(x => (string)x.Attribute("ID") == userName);

                IList<FiscalYearDBModel> model = new List<FiscalYearDBModel>();
                foreach (var item in result.Descendants("FiscalYear"))
                {
                    string fiscalYearName = item.Attributes("Name").FirstOrDefault().Value;
                    var dbName = item.Elements("DBName").FirstOrDefault().Value;
                    model.Add(new FiscalYearDBModel() { DBName = dbName, FiscalYear = fiscalYearName });
                }

                return new JsonResult { Data = model };
            }
            catch (Exception e)
            {
                return null;
            }

        }

        public ActionResult DashboardAlerts()
        {
            var alerts = _queryService.NotificationList();
            var result = new List<NotificationBuilderModel>();
            if (alerts.Count > 0)
            {
                foreach (var alert in alerts)
                {
                    if (alert.NotificationResult == "Scalar")
                    {
                        var a = _dbContext.SqlQuery<double>(alert.SqlQuery).FirstOrDefault();
                        if (a > alert.MaxResult || a < alert.MinResult)
                        {
                            alert.ResultValue = a.ToString();
                            result.Add(alert);
                        }
                    }
                    else
                    {
                        var a = _dbContext.SqlQuery<string>(alert.SqlQuery).FirstOrDefault();
                        alert.ResultValue = a;
                        if (!string.IsNullOrWhiteSpace(a))
                            result.Add(alert);
                    }
                }
                return View(result);
            }

            return View(result);
        }

        //Method to view log
        public ActionResult LogView(string sortOn, string orderBy, string pSortOn, string keyWord, int? page)
        {
            #region LogFromFile

            int recordSize = 10;
            if (!page.HasValue)
            {
                page = 1;
                if (string.IsNullOrWhiteSpace(orderBy) || orderBy.Equals("asc")) orderBy = "asc";
                else orderBy = "desc";
            }
            if (!string.IsNullOrWhiteSpace(sortOn) && !sortOn.Equals(pSortOn, StringComparison.CurrentCultureIgnoreCase)) orderBy = "asc";
            ViewBag.OrderBy = orderBy;
            ViewBag.SortOn = sortOn;
            ViewBag.KeyWord = keyWord;

            DataClasses dataClasses = new DataClasses();
            var list = dataClasses.GetLogFiles().AsQueryable();

            switch (sortOn)
            {
                case "FileName":
                    if (orderBy.Equals("desc"))
                    {
                        list = list.OrderByDescending(P => P.FileName);
                    }
                    else
                    {
                        list = list.OrderBy(p => p.FileName);
                    }
                    break;
                case "CreatedDate":
                    if (orderBy.Equals("desc"))
                    {
                        list = list.OrderByDescending(T => T.CreatedDate);
                    }
                    else
                    {
                        list = list.OrderBy(T => T.CreatedDate);
                    }
                    break;
                default:
                    list = list.OrderBy(p => p.FileId);
                    break;
            }
            if (!string.IsNullOrWhiteSpace(keyWord))
            {
                list = list.Where(f => f.FileName.StartsWith(keyWord));
            }
            var finalList = list.ToPagedList(page.Value, recordSize);

            #endregion

            #region LogFromDB

            // var result = _logViewer.GetLogForCurrentUserAndCompany();
            var result = _logViewer.GetAllLog();
            var result1 = result.ToPagedList(page.Value, recordSize);
            ViewBag.Result = result1;

            #endregion

            List<SelectListItem> subModule = new List<SelectListItem>();
            subModule.Add(new SelectListItem { Text = "Financial Accounting", Value = "1" });
            subModule.Add(new SelectListItem { Text = "Inventory n Purcurement", Value = "2" });
            subModule.Add(new SelectListItem { Text = "Production Management", Value = "3" });
            subModule.Add(new SelectListItem { Text = "Sales n Revenue", Value = "4" });
            subModule.Add(new SelectListItem { Text = "Human Resource Management", Value = "5" });
            ViewBag.LogSubModule = subModule;


            List<SelectListItem> module = new List<SelectListItem>();
            module.Add(new SelectListItem { Text = ProjectModule.BusinessIntelligent.ToString(), Value = ((int)ProjectModule.BusinessIntelligent).ToString() });
            module.Add(new SelectListItem { Text = ProjectModule.DocumentTemplate.ToString(), Value = ((int)ProjectModule.DocumentTemplate).ToString() });
            //ViewBag.LogModule = module;

            //Dictionary<string, string> moduleCollection = (Dictionary<string, string>)Session["ModuleCollection"];
            //foreach(var mod in moduleCollection)
            //{
            //    module.Add(new SelectListItem { Text = mod.Key, Value = mod.Value });
            //}
            ViewBag.LogModule = module;

            return View(finalList);
        }


        public ActionResult DeleteLog(DateTime fromDate, DateTime toDate, string module, string subModule)
        {
            try
            {
                _logViewer.DeleteLogUsingFilter(fromDate, toDate, module, subModule);
                var model = new { Status = true, Message = "Deleted Successfully" };
                return Json(model, JsonRequestBehavior.AllowGet);
                //return Json(new { Status = true, Message = "Deleted successfully",JsonRequestBehavior.AllowGet });
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public ActionResult DeleteAllLog()
        {
            try
            {
                _logViewer.DeleteAllLog();
                var model = new { Status = true, Message = "Deleted Successfully" };
                return Json(model, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                throw ex;
            }

        }



        public ActionResult DownloadLog(string sortOn, string orderBy, string pSortOn, string keyword, int? page)
        {
            {
                int recordsPerPage = 10;
                if (!page.HasValue)
                {
                    page = 1;
                    if (string.IsNullOrWhiteSpace(orderBy) || orderBy.Equals("asc"))
                    {
                        orderBy = "desc";
                    }
                    else
                    {
                        orderBy = "asc";
                    }
                }

                if (!string.IsNullOrWhiteSpace(sortOn) && !sortOn.Equals(pSortOn, StringComparison.CurrentCultureIgnoreCase))
                {
                    orderBy = "asc";
                }

                ViewBag.OrderBy = orderBy;
                ViewBag.SortOn = sortOn;
                ViewBag.Keyword = keyword;

                DataClasses ob = new DataClasses();
                var list = ob.GetFiles().AsQueryable();

                switch (sortOn)
                {
                    case "FileName":
                        if (orderBy.Equals("desc"))
                        {
                            list = list.OrderByDescending(P => P.FileName);
                        }
                        else
                        {
                            list = list.OrderBy(p => p.FileName);
                        }
                        break;
                    default:
                        list = list.OrderBy(p => p.FileId);
                        break;
                }
                if (!string.IsNullOrWhiteSpace(keyword))
                {
                    list = list.Where(f => f.FileName.Contains(keyword));
                }
                var finalList = list.ToPagedList(page.Value, recordsPerPage);
                return View(finalList);
            }
        }

        public FileResult Download(string id, bool isLog)
        {
            DataClasses objData = new DataClasses();
            int fid = Convert.ToInt32(id);
            string fileName = "";
            string contentType = "log/txt";
            if (isLog)
            {
                var logFile = objData.GetLogFiles();
                fileName = (from f in logFile
                            where f.FileId == fid
                            select f.FilePath).First();
                return File(fileName, contentType, "LogFromLog.txt");

            }
            else
            {

                var files = objData.GetFiles();
                fileName = (from f in files
                            where f.FileId == fid
                            select f.FilePath).First();

                return File(fileName, contentType, "Log.txt");
            }
        }

        public ActionResult DashboardConfig()
        {
            return View();
        }
        public ActionResult Quotation(string qo)
        {
            string quotationId = qo;
            return View();
        }
    }

    public enum ProjectModule
    {
        BusinessIntelligent = 01,
        DocumentTemplate = 06,
    }
}
