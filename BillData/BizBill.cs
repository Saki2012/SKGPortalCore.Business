﻿using System.Collections.Generic;
using System.IO;
using System.Linq;
using OfficeOpenXml;
using OfficeOpenXml.Table;
using pdftron.PDF;
using pdftron.SDF;
using SKGPortalCore.Data;
using SKGPortalCore.Lib;
using SKGPortalCore.Model;
using SKGPortalCore.Model.BillData;
using SKGPortalCore.Model.MasterData;
using SKGPortalCore.Repository.SKGPortalCore.Business.Func;

namespace SKGPortalCore.Repository.SKGPortalCore.Business.BillData
{
    /// <summary>
    /// 帳單-商業邏輯
    /// </summary>
    internal static class BizBill
    {
        #region Const
        private const string martCrypt1 = "ABCDEFGHIJKLMNOPQRSTUVWXYZ+%-. $/1234567890";
        private const string martCrypt2 = "1234567891234567892345678912678901234567890";
        #endregion

        #region Public
        /// <summary>
        /// 檢查資料
        /// </summary>
        /// <param name="set"></param>
        public static void CheckData(BillSet set, MessageLog Message, ApplicationDbContext DataAccess)
        {
            if (CheckBankCodeExist(DataAccess, set.Bill)) { Message.AddErrorMessage(MessageCode.Code1008, ResxManage.GetDescription(set.Bill.BankBarCode), set.Bill.BankBarCode); }
        }
        /// <summary>
        /// 設置資料
        /// </summary>
        /// <param name="set"></param>
        /// <param name="action"></param>
        public static void SetData(BillSet set, ApplicationDbContext DataAccess)
        {
            set.Bill.PayAmount = 0m;
            if (set.BillDetail.HasData())
            {
                foreach (BillDetailModel detail in set.BillDetail)
                {
                    set.Bill.PayAmount += detail.PayAmount;
                }
            }
            set.Bill.HadPayAmount = 0m;
            if (set.BillReceiptDetail.HasData())
            {
                foreach (BillReceiptDetailModel detail in set.BillReceiptDetail)
                {
                    set.Bill.HadPayAmount += detail.ReceiptBill.PayAmount;
                }
            }
            set.Bill.PayStatus = GetPayStatus(set.Bill.PayAmount, set.Bill.HadPayAmount);
            SetBarCode(DataAccess, set.Bill);
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
            Convert.OfficeToPDF(pdfdoc, $"{ReportTemplate.TemplatePath}{ReportTemplate.BillTemplate}.docx", null);
            Page pg = pdfdoc.GetPage(1);
            ContentReplacer replacer = new ContentReplacer();
            //SetData();
            //foreach (string key in Dic.Keys) replacer.AddString(key, Dic[key]);
            replacer.Process(pg);
            pdfdoc.Save($"{ReportTemplate.TemplateOutputPath}{ReportTemplate.ReceiptTemplate}{ReportTemplate.Resx}.pdf", SDFDoc.SaveOptions.e_linearized);
        }
        #endregion

        #region Private
        /// <summary>
        /// 設置條碼編碼
        /// </summary>
        /// <param name="bill"></param>
        private static void SetBarCode(ApplicationDbContext DataAccess, BillModel bill)
        {
            //銀行：
            bill.BankBarCode = GetBankCode(DataAccess, bill);
            //超商：
            string collectionTypeId = GetCollectionTypeId(DataAccess, bill);
            bill.MarketBarCode1 = !collectionTypeId.IsNullOrEmpty() ? GetMarketBarCode1(bill, collectionTypeId) : string.Empty;
            bill.MarketBarCode2 = !collectionTypeId.IsNullOrEmpty() ? GetMarketBarCode2(bill) : string.Empty;
            bill.MarketBarCode3 = !collectionTypeId.IsNullOrEmpty() ? GetMarketBarCode3(bill) : string.Empty;
            //郵局：應判斷Channel是否包含郵局類型的通路?
            bill.PostBarCode1 = bill.PayAmount > 0 ? GetPostBarCode1() : string.Empty;
            bill.PostBarCode3 = bill.PayAmount > 0 ? GetPostBarCode3(bill) : string.Empty;
            bill.PostBarCode2 = bill.PayAmount > 0 ? GetPostBarCode2(bill) : string.Empty;
        }
        /// <summary>
        /// 獲取銀行銷帳編號
        /// </summary>
        /// <param name="bill"></param>
        /// <returns></returns>
        private static string GetBankCode(ApplicationDbContext DataAccess, BillModel bill)
        {
            string vir1 = string.Empty, vir2 = string.Empty, vir3 = string.Empty;
            switch (bill.BizCustomer.VirtualAccount1)
            {
                case VirtualAccount1.BillTerm:
                    vir1 = bill.BillTerm.BillTermNo.PadLeft(bill.Customer.BillTermLen, '0');
                    break;
            }
            switch (bill.BizCustomer.VirtualAccount2)
            {
                case VirtualAccount2.PayerNo:
                    vir2 = bill.Payer.PayerNo.PadLeft(bill.Customer.PayerNoLen, '0');
                    break;
                case VirtualAccount2.Seq:
                    BillTermModel billTerm = DataAccess.Find<BillTermModel>(bill.BillTermId);
                    vir2 = (++billTerm.Seq).ToString().PadLeft(bill.Customer.PayerNoLen, '0');
                    DataAccess.Update(billTerm);
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
            switch (bill.BizCustomer.VirtualAccount3)
            {
                case VirtualAccount3.NoverifyCode:
                    return result;
                case VirtualAccount3.Seq:
                case VirtualAccount3.SeqPayEndDate:
                    return result + GetBankCheckCodeSeq(result);
                case VirtualAccount3.SeqAmount:
                case VirtualAccount3.SeqAmountPayEndDate:
                    return result + GetBankCheckCodeSeqAmount(result, bill.PayAmount > 0 ? bill.PayAmount.ToInt32() : 0);
                default:
                    return result;
            }
        }
        /// <summary>
        /// 獲取超商條碼1
        /// 代收期限(民國年yymmdd)(6碼) + 代收項目 (3碼)
        /// </summary>
        /// <returns></returns>
        private static string GetMarketBarCode1(BillModel bill, string collectionTypeId)
        {
            return bill.PayEndDate.ToROCDate().Substring(1) + collectionTypeId;
        }
        /// <summary>
        /// 獲取超商條碼2
        /// 銀行條碼(16碼，靠左補零)
        /// </summary>
        /// <returns></returns>
        private static string GetMarketBarCode2(BillModel bill)
        {
            return bill.BankBarCode.PadLeft(16, '0');
        }
        /// <summary>
        /// 獲取超商條碼3
        /// 應繳年月(4碼) + 檢碼(2) + 應繳金額(9碼，靠左補零)
        /// </summary>
        /// <returns></returns>
        private static string GetMarketBarCode3(BillModel bill)
        {
            return bill.MarketBarCode1.Substring(0, 4)
                + GetMarketCheckCode(bill.MarketBarCode1, bill.MarketBarCode2, bill.MarketBarCode1.Substring(0, 4), bill.PayAmount.ToInt32())
                + bill.PayAmount.ToInt32().ToString().PadLeft(9, '0');
        }
        /// <summary>
        /// 獲取郵局條碼1
        /// </summary>
        /// <returns></returns>
        private static string GetPostBarCode1()
        {
            return "50084884";
        }
        /// <summary>
        /// 獲取郵局條碼2
        /// </summary>
        /// <returns></returns>
        private static string GetPostBarCode2(BillModel bill)
        {
            string tmpPostBarCode2 = bill.PayEndDate.ToROCDate() + bill.BankBarCode.PadLeft(16, '0');
            tmpPostBarCode2 += GetPostCheckCode(bill.PostBarCode1, tmpPostBarCode2, bill.PostBarCode3);
            return tmpPostBarCode2;
        }
        /// <summary>
        /// 獲取郵局條碼3
        /// </summary>
        /// <returns></returns>
        private static string GetPostBarCode3(BillModel bill)
        {
            return bill.PayAmount.ToInt32().ToString().PadLeft(8, '0');
        }
        /// <summary>
        /// 檢查銷帳編號是否已存在
        /// </summary>
        /// <param name="bill"></param>
        /// <returns></returns>
        private static bool CheckBankCodeExist(ApplicationDbContext DataAccess, BillModel bill)
        {
            BillModel result = DataAccess.Set<BillModel>().FirstOrDefault(p =>
             p.BillNo != bill.BillNo && p.BankBarCode == bill.BankBarCode && (p.FormStatus == FormStatus.Saved || p.FormStatus == FormStatus.Approved));
            return null != result;
        }
        /// <summary>
        /// 銀行條碼檢碼(無金額檢算)
        /// </summary>
        /// <param name="bankcode"></param>
        /// <returns></returns>
        private static string GetBankCheckCodeSeq(string bankcode)
        {
            int tv = 0;
            //扣除檢碼長度，故-1
            switch (bankcode.Length)
            {
                case 12:
                    {
                        foreach (char ch in bankcode)
                        {
                            tv += ch.ToInt32();
                        }

                        tv = tv * 13 % 11;
                    }
                    break;
                case 13:
                    {
                        int[] arrtmp14 = { 10, 0, 9, 8, 7, 6, 5, 4, 3, 2, 3, 5, 7 };
                        for (int i = 0; i < arrtmp14.Length; i++)
                        {
                            tv += bankcode[i].ToInt32() * arrtmp14[i];
                        }

                        tv %= 11;
                    }
                    break;
                case 15:
                    {
                        int[] arrtmp16 = { 10, 0, 9, 8, 7, 6, 5, 4, 3, 2, 3, 5, 7, 9, 7 };
                        for (int i = 0; i < arrtmp16.Length; i++)
                        {
                            tv += bankcode[i].ToInt32() * arrtmp16[i];
                        }

                        tv %= 11;
                    }
                    break;
            }
            return tv.ToString().Length == 2 ? "0" : tv.ToString()[0].ToString();
        }
        /// <summary>
        /// 銀行條碼檢碼(含金額檢算)
        /// </summary>
        /// <param name="bankcode"></param>
        /// <returns></returns>
        private static string GetBankCheckCodeSeqAmount(string bankcode, int amount)
        {
            int tv = 0;
            //扣除檢碼長度，故-1
            switch (bankcode.Length)
            {
                case 12:
                case 13:
                    {
                        int[] arrtmp14 = { 1, 2, 3, 4, 5, 6, 7, 8, 9, 2, 4, 6, 8, 1, 3, 5, 7, 9, 7, 5, 3 };
                        string code = amount.ToString().PadLeft(8, '0') + bankcode.PadLeft(13, '0');
                        for (int i = 0; i < arrtmp14.Length; i++)
                        {
                            tv += code[i].ToInt32() * arrtmp14[i];
                        }
                    }
                    break;
                case 15:
                    {
                        int[] arrtmp16 = { 1, 2, 3, 4, 5, 6, 7, 8, 9, 2, 4, 6, 8, 1, 3, 5, 7, 9, 7, 5, 3, 1, 2 };
                        string code = amount.ToString().PadLeft(8, '0') + bankcode;
                        for (int i = 0; i < arrtmp16.Length; i++)
                        {
                            tv += code[i].ToInt32() * arrtmp16[i];
                        }
                    }
                    break;
            }

            tv = 11 - tv % 11;
            return tv switch
            {
                10 => "0",
                11 => "1",
                _ => tv.ToString()[0].ToString(),
            };
        }
        /// <summary>
        /// 超商檢碼
        /// </summary>
        /// <param name="barcode1"></param>
        /// <param name="payNo"></param>
        /// <param name="barDetailDate"></param>
        /// <param name="amount"></param>
        /// <returns></returns>
        private static string GetMarketCheckCode(string barcode1, string payNo, string barDetailDate, int amount)
        {
            string bar1 = barcode1, bar2 = payNo.PadLeft(16, '0'), bar3 = barDetailDate + amount.ToString().PadLeft(9, '0'), result = string.Empty;
            int cal1 = 0, cal2 = 0;
            GetMarketEncode(ref cal1, ref cal2, bar1);
            GetMarketEncode(ref cal1, ref cal2, bar2);
            GetMarketEncode(ref cal1, ref cal2, bar3);
            result += (cal1 % 11) switch
            {
                0 => "A",
                10 => "B",
                _ => (cal1 % 11).ToString(),
            };
            result += (cal2 % 11) switch
            {
                0 => "X",
                10 => "Y",
                _ => (cal2 % 11).ToString(),
            };
            return result;
        }
        /// <summary>
        /// 獲取超商加密碼
        /// </summary>
        /// <param name="cal1"></param>
        /// <param name="cal2"></param>
        /// <param name="bar"></param>
        private static void GetMarketEncode(ref int cal1, ref int cal2, string bar)
        {
            int len = bar.Length;
            for (int i = 0; i < len; i++)
                if (i % 2 == 0)
                    cal1 += MarketEncode(bar.Substring(i, 1));
                else
                    cal2 += MarketEncode(bar.Substring(i, 1));
        }
        /// <summary>
        /// 超商加密碼
        /// </summary>
        /// <param name="idx"></param>
        /// <returns></returns>
        private static int MarketEncode(string idx)
        {
            return int.Parse(martCrypt2.Substring(martCrypt1.IndexOf(idx), 1));
        }
        /// <summary>
        /// 郵局檢碼
        /// 第一段奇數+第二段奇數+第三段奇數共20碼相加 % 11
        /// </summary>
        /// <param name="postBarCode1"></param>
        /// <param name="tmp"></param>
        /// <param name="postBarCode3"></param>
        /// <returns></returns>
        private static char GetPostCheckCode(string postBarCode1, string tmpPostBarCode2, string postBarCode3)
        {
            int tv = (GetSingleNum(postBarCode1) + GetSingleNum(tmpPostBarCode2) + GetSingleNum(postBarCode3)) % 11;
            return tv.ToString().Length == 2 ? '0' : tv.ToString()[0];
        }
        /// <summary>
        /// 取奇數位數值總和
        /// </summary>
        /// <param name="code"></param>
        /// <returns></returns>
        private static int GetSingleNum(string code)
        {
            int result = 0;
            for (int i = 0; i < code.Length; i += 2)
                result += code[i].ToInt32();
            return result;
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
            if (hasPayAmount == 0m)
            {
                return PayStatus.Unpaid;
            }
            if (payAmount > hasPayAmount)
            {
                return PayStatus.UnderPaid;
            }
            else if (payAmount == hasPayAmount)
            {
                return PayStatus.PaidComplete;
            }
            else
            {
                return PayStatus.OverPaid;
            }
        }
        /// <summary>
        /// 獲取帳單對應的超商代收類別
        /// </summary>
        /// <param name="bill"></param>
        /// <returns></returns>
        private static string GetCollectionTypeId(ApplicationDbContext DataAccess, BillModel bill)
        {
            List<string> coltypes = bill.BizCustomer.CollectionTypeIds.Split(',').ToList();
            List<string> channels = bill.BizCustomer.ChannelIds.Split(',').ToList();
            List<string> colTypeIds = DataAccess.Set<CollectionTypeDetailModel>()
                .Where(p => coltypes.Contains(p.CollectionTypeId) && channels.Contains(p.ChannelId) && p.SRange >= bill.PayAmount && p.ERange <= bill.PayAmount)
                .Select(p => p.CollectionTypeId).ToList();
            return colTypeIds.Count > 0 ? colTypeIds[0] : string.Empty;
        }
        #endregion
    }
}
