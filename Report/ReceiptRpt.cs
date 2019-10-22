using System.Collections.Generic;
using pdftron.PDF;
using pdftron.SDF;
using Convert = pdftron.PDF.Convert;

namespace SKGPortalCore.Business.Report
{
    public class ReceiptRpt
    {

        private readonly Dictionary<string, string> Dic = new Dictionary<string, string>()
        {
            {"RealAccountName","" },
            {"TradeYear","" },
            {"TradeMonth","" },
            {"PrintDateCHN","" },
        };

        public void ExportPdf()
        {
            using PDFDoc pdfdoc = new PDFDoc();
            Convert.OfficeToPDF(pdfdoc, $"{ReportTemplate.TemplatePath}{ReportTemplate.ReceiptTemplate}.docx", null);
            Page pg = pdfdoc.GetPage(1);
            ContentReplacer replacer = new ContentReplacer();
            SetData();
            foreach (string key in Dic.Keys) replacer.AddString(key, Dic[key]);
            replacer.Process(pg);
            pdfdoc.Save($"{ReportTemplate.TemplateOutputPath}{ReportTemplate.ReceiptTemplate}{ReportTemplate.Resx}.pdf", SDFDoc.SaveOptions.e_linearized);
        }

        public void SetData() 
        {
            Dic["RealAccountName"] = string.Empty;
        }
    }
}
