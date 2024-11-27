using NeoERP.QuotationManagement.Service.Models;
using NeoERP.QuotationManagement.Service.Interface;
using System;
using Newtonsoft.Json.Linq;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using NeoErp.Core;
using NeoErp.Core.Caching;
using System.Data.SqlClient;
using NeoErp.Core.Models;
using NeoErp.Core.Models.Log4NetLoggin;
using System.Web.Http;
using NeoErp.Data;
using NeoErp.Core.Services.CommonSetting;
using System.Web;
using System.IO;

namespace NeoERP.QuotationManagement.Controllers.Api
{
    public class QuotationApiController : ApiController
    {
        private const string QM = "Quotation Management";
        private IQuotationRepo _quotRepo;
        private IDbContext _dbContext;
        private IWorkContext _workContext;
        private ICacheManager _cacheManager;
        private NeoErpCoreEntity _objectEntity;
        private readonly ILogErp _logErp;
        private ISettingService _settingService;
        private DefaultValueForLog _defaultValueForLog;
        public QuotationApiController(IQuotationRepo _IQuotRepo, IDbContext dbContext, IWorkContext workContext, ICacheManager cacheManager, NeoErpCoreEntity objectEntity)
        {
            this._quotRepo = _IQuotRepo;
            this._dbContext = dbContext;
            this._objectEntity = objectEntity;
            this._workContext = workContext;
            this._cacheManager = cacheManager;
            this._defaultValueForLog = new DefaultValueForLog(this._workContext);
            this._logErp = new LogErp(this, _defaultValueForLog.LogUser, _defaultValueForLog.LogCompany, _defaultValueForLog.LogBranch, _defaultValueForLog.LogTypeCode, _defaultValueForLog.LogModule, _defaultValueForLog.FormCode);
        }
        [HttpGet]
        public List<Products> ItemDetails()
        {
            List<Products> itemDeatils = _quotRepo.GetAllProducts();
            return itemDeatils;
        }

        public List<Quotation_setup> getTenderNo()
        {
            List<Quotation_setup> quotations = _quotRepo.GetQuotationId();
            return quotations;
        }
        public List<Quotation_setup> getTenderId(string tenderNo)
        {
            List<Quotation_setup> quotation = _quotRepo.GetTenderId(tenderNo);
            return quotation;
        }
        [HttpPost]
        public HttpResponseMessage deleteQuotationId(string tenderNo)
        {
            try
            {
                bool isDeleted = _quotRepo.deleteQuotationId(tenderNo);
                if (isDeleted)
                {
                    return Request.CreateResponse(HttpStatusCode.OK, new { MESSAGE = "Quotation deleted successfully!!", STATUS_CODE = (int)HttpStatusCode.OK });
                }
                else
                {
                    // Handle case where project was not posted successfully
                    return Request.CreateResponse(HttpStatusCode.InternalServerError, new { MESSAGE = "Failed to delete project!!", STATUS_CODE = (int)HttpStatusCode.InternalServerError });
                }
            }
            catch (Exception ex)
            {
                // Handle any exceptions that occurred during the operation
                return Request.CreateResponse(HttpStatusCode.InternalServerError, new { MESSAGE = ex.Message, STATUS_CODE = (int)HttpStatusCode.InternalServerError });
            }
        }
        [HttpPost]
        public IHttpActionResult SaveItemData(Quotation_setup formData)
        {
            try
            {
                bool isPosted = _quotRepo.InsertQuotationData(formData);
                if (isPosted)
                {
                    return Ok(new { success = true, message = "Quotation data saved successfully." });
                }
                else
                {
                    return Ok(new { success = true, message = "Failed to save Quotation data." });
                }
            }
            catch (Exception ex)
            {
                return InternalServerError(ex);
            }
        }
        public List<Quotation_setup> ListAllTenders()
        {
            List<Quotation_setup> response = new List<Quotation_setup>();
            try
            {
                response = _quotRepo.ListAllTenders();
                return response;

            }
            catch (Exception ex)
            {
                throw new Exception(ex.StackTrace);
            }
        }
        public List<Quotation_setup> GetQuotationById(string tenderNo)
        {
            List<Quotation_setup> response = new List<Quotation_setup>();
            try
            {
                response = _quotRepo.GetQuotationById(tenderNo);
                return response;

            }
            catch (Exception ex)
            {
                throw new Exception(ex.StackTrace);
            }
        }
        [HttpPost]
        public HttpResponseMessage updateItemsById(string tenderNo,string id)
        {
            try
            {
                bool isDeleted = _quotRepo.updateItemsById(tenderNo,id);
                if (isDeleted)
                {
                    return Request.CreateResponse(HttpStatusCode.OK, new { MESSAGE = "Product deleted successfully!!", STATUS_CODE = (int)HttpStatusCode.OK });
                }
                else
                {
                    // Handle case where project was not posted successfully
                    return Request.CreateResponse(HttpStatusCode.InternalServerError, new { MESSAGE = "Failed to delete product!!", STATUS_CODE = (int)HttpStatusCode.InternalServerError });
                }
            }
            catch (Exception ex)
            {
                // Handle any exceptions that occurred during the operation
                return Request.CreateResponse(HttpStatusCode.InternalServerError, new { MESSAGE = ex.Message, STATUS_CODE = (int)HttpStatusCode.InternalServerError });
            }
        }
        public List<Quotation_Details> ListQuotationDetails()
        {
            List<Quotation_Details> response = new List<Quotation_Details>();
            try
            {
                response = _quotRepo.ListQuotationDetails();
                return response;

            }
            catch (Exception ex)
            {
                throw new Exception(ex.StackTrace);
            }
        }
        [HttpGet]
        public List<Quotation_Details> QuotationDetailsById(string quotationNo,string tenderNo)
        {
            List<Quotation_Details> response = new List<Quotation_Details>();
            try
            {
                response = _quotRepo.QuotationDetailsById(quotationNo, tenderNo);
                return response;

            }
            catch (Exception ex)
            {
                throw new Exception(ex.StackTrace);
            }
        }
        [HttpGet]
        public List<Quotation_Details> QuotationDetailsId(string quotationNo, string tenderNo)
        {
            List<Quotation_Details> response = new List<Quotation_Details>();
            try
            {
                response = _quotRepo.QuotationDetailsId(quotationNo, tenderNo);
                return response;

            }
            catch (Exception ex)
            {
                throw new Exception(ex.StackTrace);
            }
        }
        public List<SummaryReport> TendersItemWise()
        {
            List<SummaryReport> response = new List<SummaryReport>();
            try
            {
                response = _quotRepo.TendersItemWise();
                return response;

            }
            catch (Exception ex)
            {
                throw new Exception(ex.StackTrace);
            }
        }
        [HttpGet]
        public List<Quotation> ItemDetailsByTender(string tenderNo)
        {
            List<Quotation> response = new List<Quotation>();
            try
            {
                 response = _quotRepo.ItemDetailsTenderNo(tenderNo);
                return response;
            }
            catch (Exception ex)
            {
                throw new Exception(ex.StackTrace);
            }
        }
        [HttpPost]
        public HttpResponseMessage updateQuotationStatus(string quotationNo, string status)
        {
            try
            {
                bool isDeleted=false;
                if (status == "AP")
                {
                     isDeleted = _quotRepo.acceptQuotation(quotationNo, status);
                }
                else if(status=="R")
                {
                     isDeleted = _quotRepo.rejectQuotation(quotationNo, status);
                }
                if (isDeleted)
                {
                    return Request.CreateResponse(HttpStatusCode.OK, new { MESSAGE = "Quotation !!", STATUS_CODE = (int)HttpStatusCode.OK });
                }
                else
                {
                    // Handle case where project was not posted successfully
                    return Request.CreateResponse(HttpStatusCode.InternalServerError, new { MESSAGE = "Quotation !!", STATUS_CODE = (int)HttpStatusCode.InternalServerError });
                }
            }
            catch (Exception ex)
            {
                // Handle any exceptions that occurred during the operation
                return Request.CreateResponse(HttpStatusCode.InternalServerError, new { MESSAGE = ex.Message, STATUS_CODE = (int)HttpStatusCode.InternalServerError });
            }
        }
        [HttpPost]
        public IHttpActionResult saveTender(Tender formData)
        {
            try
            {
                bool isPosted = _quotRepo.InsertTenderData(formData);
                if (isPosted)
                {
                    return Ok(new { success = true, message = "Tender data saved successfully." });
                }
                else
                {
                    return Ok(new { success = true, message = "Failed to save Quotation Tender." });
                }
            }
            catch (Exception ex)
            {
                return InternalServerError(ex);
            }
        }
        [HttpGet]
        public List<Tender> TenderDetails()
        {
            List<Tender> response = new List<Tender>();
            try
            {
                response = _quotRepo.getTenderDetails();
                return response;
            }
            catch(Exception ex) {
                throw new Exception(ex.StackTrace);
            }
        }
        [HttpPost]
        public HttpResponseMessage deleteTenderId(string tenderNo)
        {
            try
            {
                bool isDeleted = _quotRepo.deleteTenderId(tenderNo);
                if (isDeleted)
                {
                    return Request.CreateResponse(HttpStatusCode.OK, new { MESSAGE = "Tender No. deleted successfully!!", STATUS_CODE = (int)HttpStatusCode.OK });
                }
                else
                {
                    // Handle case where project was not posted successfully
                    return Request.CreateResponse(HttpStatusCode.InternalServerError, new { MESSAGE = "Failed to delete Tender No.!!", STATUS_CODE = (int)HttpStatusCode.InternalServerError });
                }
            }
            catch (Exception ex)
            {
                // Handle any exceptions that occurred during the operation
                return Request.CreateResponse(HttpStatusCode.InternalServerError, new { MESSAGE = ex.Message, STATUS_CODE = (int)HttpStatusCode.InternalServerError });
            }
        }
        [HttpGet]
        public List<Tender> getTenderById(string tenderNo)
        {
            List<Tender> response = new List<Tender>();
            try
            {
                response = _quotRepo.getTenderById(tenderNo);
                return response;
            }
            catch (Exception ex)
            {
                throw new Exception(ex.StackTrace);
            }
        }
        public List<QuotationCount> GetQuotationCount()
        {
            List<QuotationCount> response = new List<QuotationCount>();
            try
            {
                response = _quotRepo.GetQuotationCount();
                return response;

            }
            catch (Exception ex)
            {
                throw new Exception(ex.StackTrace);
            }
        }
        public List<Quotation_Details> QuotationById(string quotationNo, string tenderNo)
        {
            List<Quotation_Details> response = new List<Quotation_Details>();
            try
            {
                response = _quotRepo.QuotationDetailsById(quotationNo, tenderNo);
                return response;

            }
            catch (Exception ex)
            {
                throw new Exception(ex.StackTrace);
            }
        }
    }
}
