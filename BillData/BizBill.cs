using Microsoft.EntityFrameworkCore;
using SKGPortalCore.Data;
using SKGPortalCore.Lib;
using SKGPortalCore.Model;
using SKGPortalCore.Model.MasterData.OperateSystem;
using SKGPortalCore.Models.BillData;
using System.Collections.Generic;
using System.Linq;
namespace SKGPortalCore.Business.BillData
{
    /// <summary>
    /// 帳單-商業邏輯
    /// </summary>
    public class BizBill : BizBase
    {
        #region Const
        private const string str1 = "ABCDEFGHIJKLMNOPQRSTUVWXYZ+%-. $/1234567890";
        private const string str2 = "1234567891234567892345678912678901234567890";
        #endregion

        #region Construct
        public BizBill(MessageLog message, ApplicationDbContext dataAccess = null, IUserModel user = null) : base(message, dataAccess, user) { }
        #endregion

        #region Public
        //保存前
        public void CheckData(BillSet set)
        {
            ConditionRequireFields(set.Bill);
            ConditionRequireFields(set.BillDetail);
            CheckBankCodeExist(set.Bill);
        }
        public void SetData(BillSet set)
        {
            set.Bill.PayAmount = 0m;
            SetBarCode(set.Bill);
            foreach (var detail in set.BillDetail)
            {
                set.Bill.PayAmount += detail.PayAmount;
            }
            set.Bill.HasPayAmount = 0m;
            foreach (var detail in set.BillReceiptDetail)
            {
                set.Bill.HasPayAmount += detail.ReceiptBill.PayAmount;
            }
            set.Bill.PayStatus = GetPayStatus(set.Bill.PayAmount, set.Bill.HasPayAmount);
        }
        #endregion

        #region Private
        /// <summary>
        /// 
        /// </summary>
        /// <param name="bill"></param>
        private void SetBarCode(BillModel bill)
        {
            bill.BankCode = GetBankCode(bill);
            bill.BankBarCode = GetBankBarCode(bill);
            bill.MarketBarCode1 = GetMarketBarCode1(bill);
            bill.MarketBarCode2 = GetMarketBarCode2(bill);
            bill.MarketBarCode3 = GetMarketBarCode3(bill);
            //bill.BankCode = GetMarketCompareCode(bill);
            bill.PostBarCode1 = GetPostBarCode1(bill);
            bill.PostBarCode2 = GetPostBarCode2(bill);
            bill.PostBarCode3 = GetPostBarCode3(bill);
            //bill.PostCompareCode = GetPostCompareCode(bill);
        }
        /// 設置銀行銷帳編號
        /// </summary>
        /// <returns></returns>
        private string GetBankCode(BillModel bill)
        {
            string vir1 = string.Empty, vir2 = string.Empty, vir3 = string.Empty;
            //if(VirtualAccount1.BillTermId)
            vir1 = bill.BillTermId.PadLeft(bill.Customer.BillTermLen, '0');

            //if(VirtualAccount2.PayerNo)
            vir2 = bill.PayerId.PadLeft(bill.Customer.PayerNoLen, '0');
            //else if(VirtualAccount2.Seq)
            //vir2=//序號

            //if(VirtualAccount3.SeqPayEndDate||VirtualAccount3.SeqAmountPayEndDate)
            vir3 = $"{bill.PayEndDate.Year}{bill.PayEndDate.DayOfYear.ToString().PadLeft(3, '0')}";

            //bill.BankCode = $"{vir3}{vir1}{vir2}";
            return $"{vir3}{vir1}{vir2}";
        }
        /// <summary>
        /// 設置銀行條碼
        /// </summary>
        /// <param name="bill"></param>
        /// <returns></returns>
        private string GetBankBarCode(BillModel bill)
        {
            VirtualAccount3 x = VirtualAccount3.NoverifyCode;
            string result;
            switch (x)
            {
                case VirtualAccount3.NoverifyCode:
                    result = bill.BankCode;
                    break;
                case VirtualAccount3.Seq:
                case VirtualAccount3.SeqPayEndDate:
                    result = bill.BankCode + GetBankCheckCodeSeq(bill.BankCode);
                    break;
                case VirtualAccount3.SeqAmount:
                case VirtualAccount3.SeqAmountPayEndDate:
                    result = bill.BankCode + GetBankCheckCodeSeqAmount(bill.BankCode, bill.PayAmount.ToInt32());
                    break;
                default:
                    result = string.Empty;
                    break;
            }
            //bill.BankBarCode = result;
            return result;
        }
        /// <summary>
        /// 設置超商條碼1
        /// 代收期限(民國年yymmdd)(6碼) + 代收項目 (3碼)
        /// </summary>
        /// <returns></returns>
        private string GetMarketBarCode1(BillModel bill)
        {
            bill.MarketBarCode1 = bill.PayEndDate.ToROCDate().Substring(1) + "代收類別名稱(6V1)";
            return "";
        }
        /// <summary>
        /// 設置超商條碼2
        /// 銀行條碼(16碼，靠左補零)
        /// </summary>
        /// <returns></returns>
        private string GetMarketBarCode2(BillModel bill)
        {
            return bill.BankBarCode.PadLeft(16, '0');
        }
        /// <summary>
        /// 設置超商條碼3
        /// 應繳年月(4碼) + 檢碼(2) + 應繳金額(9碼，靠左補零)
        /// </summary>
        /// <returns></returns>
        private string GetMarketBarCode3(BillModel bill)
        {
            bill.MarketBarCode3 = bill.MarketBarCode1.Substring(0, 4) +
                GetMarketCheckCode(bill.MarketBarCode1, bill.MarketBarCode2, bill.MarketBarCode1.Substring(0, 4), bill.PayAmount.ToInt32())
                + bill.PayAmount.ToString("D").PadLeft(9, '0');
            return "";
        }
        /// <summary>
        /// 設置超商檢查碼(待考慮)
        /// </summary>
        /// <returns></returns>
        private string GetMarketCompareCode(BillModel bill)
        {
            return bill.MarketBarCode2;
        }
        /// <summary>
        /// 設置郵局條碼1
        /// </summary>
        /// <returns></returns>
        private string GetPostBarCode1(BillModel bill)
        {
            return string.Empty;
            //return bill.POSTAccount;
        }
        /// <summary>
        /// 設置郵局條碼2
        /// </summary>
        /// <returns></returns>
        private string GetPostBarCode2(BillModel bill)
        {
            string tmpPostBarCode2 = bill.PayEndDate.ToROCDate() + bill.BankBarCode.PadLeft(16, '0');
            tmpPostBarCode2 += GetPostCheckCode(bill.PostBarCode1, tmpPostBarCode2, bill.PostBarCode3);
            return tmpPostBarCode2;
        }
        /// <summary>
        /// 設置郵局條碼3
        /// </summary>
        /// <returns></returns>
        private string GetPostBarCode3(BillModel bill)
        {
            return bill.PayAmount.ToString().PadLeft(8, '0');
        }
        /// <summary>
        /// 設置郵局檢查碼(待考慮)
        /// </summary>
        /// <returns></returns>
        private string GetPostCompareCode(BillModel bill)
        {
            return bill.BankBarCode.PadLeft(16, '0');
        }
        /// <summary>
        /// 檢查銷帳編號是否已存在
        /// </summary>
        /// <param name="bill"></param>
        /// <returns></returns>
        private bool CheckBankCodeExist(BillModel bill)
        {
            BillModel result = DataAccess.Bill.FirstOrDefault(p =>
              p.BillNo != bill.BillNo
              && p.BankCode == bill.BankCode
              && (p.FormStatus == FormStatus.Saved || p.FormStatus == FormStatus.Approved));
            return null != result;
        }
        /// <summary>
        /// 檢查欄位是否必填(表頭)
        /// </summary>
        /// <param name="masterModel">服務申請書主檔</param>
        private void ConditionRequireFields(BillModel master)
        {

        }
        /// <summary>
        /// 檢查欄位是否必填(表身)
        /// </summary>
        /// <param name="masterModel">服務申請書主檔</param>
        private void ConditionRequireFields(List<BillDetailModel> detail)
        {
        }
        /// <summary>
        /// 銀行條碼檢碼(無金額檢算)
        /// </summary>
        /// <param name="bankcode"></param>
        /// <returns></returns>
        private string GetBankCheckCodeSeq(string bankcode)
        {
            int tv = 0;
            //扣除檢碼長度，故-1
            switch (bankcode.Length)
            {
                case 12:
                    {
                        foreach (char ch in bankcode)
                            tv += ch.ToInt32();
                        tv = tv * 13 % 11;
                    }
                    break;
                case 13:
                    {
                        int[] arrtmp14 = { 10, 0, 9, 8, 7, 6, 5, 4, 3, 2, 3, 5, 7 };
                        for (int i = 0; i < arrtmp14.Length; i++)
                            tv += bankcode[i].ToInt32() * arrtmp14[i];
                        tv %= 11;
                    }
                    break;
                case 15:
                    {
                        int[] arrtmp16 = { 10, 0, 9, 8, 7, 6, 5, 4, 3, 2, 3, 5, 7, 9, 7 };
                        for (int i = 0; i < arrtmp16.Length; i++)
                            tv += bankcode[i].ToInt32() * arrtmp16[i];
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
        private string GetBankCheckCodeSeqAmount(string bankcode, int amount)
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
                            tv += code[i].ToInt32() * arrtmp14[i];
                    }
                    break;
                case 15:
                    {
                        int[] arrtmp16 = { 1, 2, 3, 4, 5, 6, 7, 8, 9, 2, 4, 6, 8, 1, 3, 5, 7, 9, 7, 5, 3, 1, 2 };
                        string code = amount.ToString().PadLeft(8, '0') + bankcode;
                        for (int i = 0; i < arrtmp16.Length; i++)
                            tv += code[i].ToInt32() * arrtmp16[i];
                    }
                    break;
            }

            tv = 11 - tv % 11;
            switch (tv)
            {
                case 10:
                    return "0";
                case 11:
                    return "1";
                default:
                    return tv.ToString()[0].ToString();
            }
        }
        /// <summary>
        /// 超商檢碼
        /// </summary>
        /// <param name="barcode1"></param>
        /// <param name="payNo"></param>
        /// <param name="barDetailDate"></param>
        /// <param name="amount"></param>
        /// <returns></returns>
        private string GetMarketCheckCode(string barcode1, string payNo, string barDetailDate, int amount)
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
        private void GetMarketEncode(ref int cal1, ref int cal2, string bar)
        {
            int len = bar.Length;
            for (int i = 0; i < len; i++)
            {
                if (i % 2 == 0)
                    cal1 += MarketEncode(bar.Substring(i, 1));
                else
                    cal2 += MarketEncode(bar.Substring(i, 1));
            }
        }
        /// <summary>
        /// 超商加密碼
        /// </summary>
        /// <param name="idx"></param>
        /// <returns></returns>
        private int MarketEncode(string idx)
        {
            return int.Parse(str2.Substring(str1.IndexOf(idx), 1));
        }
        /// <summary>
        /// 郵局檢碼
        /// 第一段奇數+第二段奇數+第三段奇數共20碼相加 % 11
        /// </summary>
        /// <param name="postBarCode1"></param>
        /// <param name="tmp"></param>
        /// <param name="postBarCode3"></param>
        /// <returns></returns>
        private char GetPostCheckCode(string postBarCode1, string tmpPostBarCode2, string postBarCode3)
        {
            int tv = (GetSingleNum(postBarCode1) + GetSingleNum(tmpPostBarCode2) + GetSingleNum(postBarCode3)) % 11;
            return tv.ToString().Length == 2 ? '0' : tv.ToString()[0];
        }
        /// <summary>
        /// 取奇數位數值總和
        /// </summary>
        /// <param name="code"></param>
        /// <returns></returns>
        private int GetSingleNum(string code)
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
        private PayStatus GetPayStatus(decimal payAmount, decimal hasPayAmount)
        {
            if (hasPayAmount == 0m) return PayStatus.Unpaid;
            if (payAmount > hasPayAmount)
                return PayStatus.UnderPaid;
            else if (payAmount == hasPayAmount)
                return PayStatus.PaidComplete;
            else
                return PayStatus.OverPaid;
        }
        #endregion

        #region DataAccess
        #endregion
    }
}
