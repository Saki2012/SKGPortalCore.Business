using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using OfficeOpenXml;
using OfficeOpenXml.Table;
using pdftron.PDF;
using pdftron.SDF;
using SKGPortalCore.Data;
using SKGPortalCore.Lib;
using SKGPortalCore.Model.BillData;
using SKGPortalCore.Model.Enum;
using SKGPortalCore.Model.MasterData;
using SKGPortalCore.Repository.SKGPortalCore.Business.Func;

namespace SKGPortalCore.Repository.SKGPortalCore.Business.BillData
{
    /// <summary>
    /// 帳單-商業邏輯
    /// </summary>
    internal static class BizBill
    {
        #region Public
        /// <summary>
        /// 檢查資料
        /// </summary>
        /// <param name="set"></param>
        public static void CheckData(BillSet set, SysMessageLog Message, ApplicationDbContext DataAccess)
        {
            if (BizVirtualAccountCode.CheckBankCodeExist(DataAccess, set.Bill.VirtualAccountCode)) { Message.AddCustErrorMessage(MessageCode.Code1008, ResxManage.GetDescription(set.Bill.VirtualAccountCode), set.Bill.VirtualAccountCode); }
            if (CheckPayEndDate(set.Bill.PayEndDate, set.Bill.BizCustomer)) { Message.AddCustErrorMessage(MessageCode.Code0001, ResxManage.GetDescription(set.Bill.PayEndDate)); }
        }
        /// <summary>
        /// 設置資料
        /// </summary>
        /// <param name="set"></param>
        /// <param name="action"></param>
        public static void SetData(BillSet set, ApplicationDbContext DataAccess)
        {
            SetBillDetail(DataAccess, set.Bill, set.BillDetail);
            SetBillReceiptDetail(DataAccess, set.Bill, set.BillReceiptDetail);

            set.Bill.VirtualAccountCode = GetBankCode(DataAccess, set.Bill);
            set.Bill.CollectionTypeId = GetCollectionTypeId(DataAccess, set.Bill);
            if (!(set.Bill.BizCustomer.MarketEnable || set.Bill.BizCustomer.PostEnable)) set.Bill.PayEndDate = DateTime.MinValue;
        }
        /// <summary>
        /// 獲取銷帳狀態
        /// </summary>
        /// <param name="payAmount"></param>
        /// <param name="hasPayAmount"></param>
        /// <param name="receiptBillCount"></param>
        /// <returns></returns>
        public static PayStatus GetPayStatus(decimal payAmount, decimal hasPayAmount)
        {
            if (hasPayAmount == 0m) return PayStatus.Unpaid;
            else
            {
                if (payAmount > hasPayAmount) return PayStatus.UnderPaid;
                else if (payAmount == hasPayAmount) return PayStatus.PaidComplete;
                else return PayStatus.OverPaid;
            }
        }

        public static void ExportExcel(List<BillModel> bills)
        {
            using ExcelPackage excel = new ExcelPackage();
            var workSheet = excel.Workbook.Worksheets.Add("Sheet1");
            workSheet.Cells["A1"].LoadFromCollection(bills, true, TableStyles.Medium12);
            //...
            workSheet.Cells[$"A{bills.Count + 2}"].LoadFromCollection(bills, true, TableStyles.Medium12);
            excel.Save();
        }
        public static void ReadExcel()
        {
            using FileStream fs = new FileStream(@"C:\Read.xlsx", FileMode.Open, FileAccess.Read, FileShare.ReadWrite);
            using ExcelPackage excel = new ExcelPackage(fs);
            ExcelWorksheet sheet = excel.Workbook.Worksheets[1];//取得Sheet1
            int startRowNumber = sheet.Dimension.Start.Row;//起始列編號，從1算起
            int endRowNumber = sheet.Dimension.End.Row;//結束列編號，從1算起
            int startColumn = sheet.Dimension.Start.Column;//開始欄編號，從1算起
            int endColumn = sheet.Dimension.End.Column;//結束欄編號，從1算起
            bool isHeader = true;//有包含標題
            if (isHeader) startRowNumber += 1;
            for (int currentRow = startRowNumber; currentRow <= endRowNumber; currentRow++)
            {
                ExcelRange range = sheet.Cells[currentRow, startColumn, currentRow, endColumn];//抓出目前的Excel列
                if (!range.Any(c => !string.IsNullOrEmpty(c.Text)))//這是一個完全空白列(使用者用Delete鍵刪除動作)
                    continue;//略過此列
                //讀值
                string cellValue = sheet.Cells[currentRow, 1].Text;//讀取格式化過後的文字(讀取使用者看到的文字)
            }
        }
        public static void PrintBill(BillSet set)
        {
            using PDFDoc pdfdoc = new PDFDoc();
            pdftron.PDF.Convert.OfficeToPDF(pdfdoc, $"{ReportTemplate.TemplatePath}{ReportTemplate.BillTemplate}.docx", null);
            Page pg = pdfdoc.GetPage(1);
            ContentReplacer replacer = new ContentReplacer();
            //SetData();
            //foreach (string key in Dic.Keys) replacer.AddString(key, Dic[key]);
            replacer.Process(pg);
            pdfdoc.Save($"{ReportTemplate.TemplateOutputPath}{ReportTemplate.ReceiptTemplate}{ReportTemplate.Resx}.pdf", SDFDoc.SaveOptions.e_linearized);
        }
        #endregion

        #region Private

        #region  CheckData
        /// <summary>
        /// 檢查繳費截止日期是否未填
        /// </summary>
        /// <param name="payEndDate"></param>
        /// <param name="customer"></param>
        /// <returns></returns>
        private static bool CheckPayEndDate(DateTime payEndDate, BizCustomerModel customer)
        {
            return (customer.MarketEnable || customer.PostEnable) && (payEndDate == null || payEndDate == DateTime.MinValue);
        }
        #endregion

        #region SetDetail
        /// <summary>
        /// 
        /// </summary>
        /// <param name="DataAccess"></param>
        /// <param name="Bill"></param>
        /// <param name="BillDetail"></param>
        private static void SetBillDetail(ApplicationDbContext DataAccess, BillModel Bill, List<BillDetailModel> BillDetail)
        {
            Bill.PayAmount = 0m;
            BillDetail?.ForEach(row =>
            {
                Bill.PayAmount += row.PayAmount;
            });
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="DataAccess"></param>
        /// <param name="Bill"></param>
        /// <param name="BillReceiptDetail"></param>
        private static void SetBillReceiptDetail(ApplicationDbContext DataAccess, BillModel Bill, List<BillReceiptDetailModel> BillReceiptDetail)
        {
            Bill.HadPayAmount = 0m;
            BillReceiptDetail?.ForEach(row =>
            {
                Bill.HadPayAmount += row.ReceiptBill.PayAmount;
            });
            Bill.PayStatus = GetPayStatus(Bill.PayAmount, Bill.HadPayAmount);
        }
        #endregion

        #region GetData
        /// <summary>
        /// 獲取銀行銷帳編號
        /// </summary>
        /// <param name="bill"></param>
        /// <returns></returns>
        private static string GetBankCode(ApplicationDbContext DataAccess, BillModel bill)
        {
            string vir1 = string.Empty, vir2 = string.Empty, vir3 = string.Empty;
            if (null == bill.BizCustomer) return string.Empty;
            switch (bill.BizCustomer.VirtualAccount1)
            {
                case VirtualAccount1.BillTerm:
                    vir1 = bill.BillTerm.BillTermNo.PadLeft(bill.BizCustomer.BillTermLen, '0');
                    break;
            }
            switch (bill.BizCustomer.VirtualAccount2)
            {
                case VirtualAccount2.PayerNo:
                    vir2 = bill.Payer.PayerNo.PadLeft(bill.BizCustomer.PayerNoLen, '0');
                    break;
                case VirtualAccount2.Seq:
                    break;
            }
            switch (bill.BizCustomer.VirtualAccount3)
            {
                case VirtualAccount3.SeqPayEndDate:
                case VirtualAccount3.SeqAmountPayEndDate:
                    //西元年末碼(1碼)+天數(3碼)
                    vir3 = bill.PayEndDate.Year.ToString().Substring(3, 1) + bill.PayEndDate.DayOfYear.ToString().PadLeft(3, '0');
                    break;
            }
            string result = $"{bill.CustomerCode}{vir3}{vir1}{vir2}";
            return $"{result}{BizVirtualAccountCode.GetVirtualCheckCode(bill.BizCustomer.VirtualAccount3, result, bill.PayAmount)}";
        }
        /// <summary>
        /// 獲取帳單對應的超商代收類別
        /// todo：Check超商代收類別是否ok
        /// </summary>
        /// <param name="bill"></param>
        /// <returns></returns>
        private static string GetCollectionTypeId(ApplicationDbContext DataAccess, BillModel bill)
        {
            if (null == bill.BizCustomer) return string.Empty;
            List<string> coltypes = bill.BizCustomer.CollectionTypeIds.Split(',').ToList();
            List<string> channels = bill.BizCustomer.ChannelIds.Split(',').ToList();
            string collectionTypeId = DataAccess.Set<CollectionTypeDetailModel>()
                .Where(p => coltypes.Contains(p.CollectionTypeId) && channels.Contains(p.ChannelId) && p.SRange >= bill.PayAmount && p.ERange <= bill.PayAmount)
                .Select(p => p.CollectionTypeId).FirstOrDefault();
            return collectionTypeId;
        }
        #endregion

        #endregion
    }
}
