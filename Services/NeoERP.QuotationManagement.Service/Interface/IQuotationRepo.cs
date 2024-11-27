using NeoErp.Core.Domain;
using NeoErp.Core.Models;
using System.Collections.Generic;
using NeoErp.Core.Models.CustomModels;
using NeoERP.QuotationManagement.Service.Models;
using System.Net.Http;

namespace NeoERP.QuotationManagement.Service.Interface
{
    public interface IQuotationRepo
    {
        List<Products> GetAllProducts();
        List<Company> GetCompany();
        List<Quotation_setup> GetQuotationId();
        bool InsertQuotationData(Quotation_setup data);
        List<Quotation_setup> GetTenderId(string tenderNo);
        List<Quotation_setup> ListAllTenders();
        List<Quotation_Details> ListQuotationDetails();
        bool deleteQuotationId(string tenderNo);
        List<Quotation_setup> GetQuotationById(string tenderNo);
        bool updateItemsById(string tenderNo,string id);
        List<Quotation_Details> QuotationDetailsById(string quotationNo,string tenderNo);
        List<Quotation_Details> QuotationDetailsId(string quotationNo, string tenderNo);

        List<SummaryReport> TendersItemWise();
        List<Quotation> ItemDetailsTenderNo(string tenderNo);
        bool acceptQuotation(string quotationNo, string status);
        bool rejectQuotation(string quotationNo, string status);
        bool InsertTenderData(Tender data);
        List<Tender> getTenderDetails();
        bool deleteTenderId(string id);
        List<Tender> getTenderById(string id);
        List<QuotationCount> GetQuotationCount();


    }
}
