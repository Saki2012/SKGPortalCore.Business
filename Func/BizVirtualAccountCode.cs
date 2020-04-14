using SKGPortalCore.Data;
using SKGPortalCore.Lib;
using SKGPortalCore.Model.System;
using SKGPortalCore.Model.SystemTable;
using System;
using System.Collections.Generic;
using System.Text;

namespace SKGPortalCore.Repository.SKGPortalCore.Business.Func
{
    /// <summary>
    /// 虛擬帳號
    /// </summary>
    public static class BizVirtualAccountCode
    {
        #region Const
        private const string martCrypt1 = "ABCDEFGHIJKLMNOPQRSTUVWXYZ+%-. $/1234567890";
        private const string martCrypt2 = "1234567891234567892345678912678901234567890";
        #endregion

        #region Public
        /// <summary>
        /// 檢查銷帳編號是否已存在
        /// </summary>
        /// <param name="dataAccess"></param>
        /// <param name="virtualAccountCode">虛擬帳號</param>
        /// <returns></returns>
        public static bool CheckBankCodeExist(ApplicationDbContext dataAccess, string virtualAccountCode, out VirtualAccountCodeModel virtualAccount)
        {
            virtualAccount = dataAccess.Set<VirtualAccountCodeModel>().Find(virtualAccountCode);
            return null != virtualAccount;
        }

        public static void AddVirtualAccountCode(ApplicationDbContext dataAccess, string sourceProgId, string sourceBillNo, string virtualAccountCode)
        {
            VirtualAccountCodeModel virtualAccount = new VirtualAccountCodeModel() { SrcProgId = sourceProgId, SrcBillNo = sourceBillNo, VirtualAccountCode = virtualAccountCode, };
            dataAccess.Set<VirtualAccountCodeModel>().Add(virtualAccount);
        }

        public static void DelVirtualAccountCode(ApplicationDbContext dataAccess, string virtualAccountCode)
        {
            dataAccess.Set<VirtualAccountCodeModel>().Remove(dataAccess.Set<VirtualAccountCodeModel>().Find(virtualAccountCode));
        }

        #region 銀行條碼規則
        /// <summary>
        /// 獲取虛擬帳號檢碼
        /// </summary>
        /// <returns></returns>
        public static string GetVirtualCheckCode(VirtualAccount3 virtualAccount3, string bankCode, decimal payAmount = 0)
        {
            switch (virtualAccount3)
            {
                case VirtualAccount3.Seq:
                case VirtualAccount3.SeqPayEndDate:
                    return GetBankCheckCodeSeq(bankCode);
                case VirtualAccount3.SeqAmount:
                case VirtualAccount3.SeqAmountPayEndDate:
                    return GetBankCheckCodeSeqAmount(bankCode, payAmount > 0 ? payAmount : 0);
                default:
                    return string.Empty;
            }
        }
        #endregion

        #region 超商條碼規則
        /// <summary>
        /// 獲取超商條碼1
        /// 代收期限(民國年yymmdd)(6碼) + 代收項目 (3碼)
        /// Ex:105/01/23 --> 050123 
        /// </summary>
        /// <param name="payEndDate">代收期限</param>
        /// <param name="collectionTypeId">代收項目</param>
        /// <returns>超商條碼1：代收期限+代收項目</returns>
        public static string GetMarketBarCode1(DateTime payEndDate, string collectionTypeId)
        {
            return payEndDate.ToROCDate().Substring(1) + collectionTypeId;
        }
        /// <summary>
        /// 獲取超商條碼1
        /// 代收期限(民國年yymmdd)(6碼) + 代收項目 (3碼)
        /// Ex:105/01/23 --> 050123
        /// </summary>
        /// <param name="payEndDate">代收期限</param>
        /// <param name="collectionTypeId">代收項目</param>
        /// <returns>超商條碼1：代收期限+代收項目</returns>
        public static string GetMarketBarCode1(string payEndDate, string collectionTypeId)
        {
            string result = payEndDate + collectionTypeId;
            return result;
        }
        /// <summary>
        /// 獲取超商條碼2
        /// 虛擬帳號(16碼，右靠左補零)
        /// </summary>
        /// <param name="virtualAccountCode">虛擬帳號</param>
        /// <returns>超商條碼2</returns>
        public static string GetMarketBarCode2(string virtualAccountCode)
        {
            return virtualAccountCode.PadLeft(16, '0');
        }
        /// <summary>
        /// 獲取超商條碼3
        /// 應繳年月(4碼) + 檢碼(2) + 應繳金額(9碼，靠左補零)
        /// Ex:105/01/23 --> 0501
        /// </summary>
        /// <returns></returns>
        public static string GetMarketBarCode3(string marketBarCode1, string marketBarCode2, decimal payAmount)
        {
            string strPayEndDate = marketBarCode1.Substring(0, 4),
                   strPayAmount = payAmount.ToString("D9");
            string checkCode = GetMarketCheckCode(marketBarCode1, marketBarCode2, strPayAmount);
            return $"{strPayEndDate}{checkCode}{strPayAmount}";
        }
        #endregion

        #region 郵局條碼規則
        /// <summary>
        /// 獲取郵局條碼1
        /// </summary>
        /// <returns></returns>
        public static string GetPostBarCode1 { get { return SystemCP.PostCollectionTypeId; } }
        /// <summary>
        /// 獲取郵局條碼2
        /// </summary>
        /// <returns></returns>
        public static string GetPostBarCode2(string payEndDate, string virtualAccountCode, decimal payAmount)
        {
            string tmpPostBarCode2 = payEndDate + virtualAccountCode.PadLeft(16, '0');
            tmpPostBarCode2 += GetPostCheckCode(GetPostBarCode1, tmpPostBarCode2, GetPostBarCode3(payAmount));
            return tmpPostBarCode2;
        }
        /// <summary>
        /// 獲取郵局條碼3
        /// </summary>
        /// <returns></returns>
        public static string GetPostBarCode3(decimal payAmount)
        {
            return payAmount.ToString("D8");
        }
        #endregion

        #endregion

        #region Private
        #region 銀行條碼規則
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
        private static string GetBankCheckCodeSeqAmount(string bankcode, decimal amount)
        {
            string strPayAmount = amount.ToString("D8");
            int tv = 0;
            //扣除檢碼長度，故-1
            switch (bankcode.Length)
            {
                case 12:
                case 13:
                    {
                        int[] arrtmp14 = { 1, 2, 3, 4, 5, 6, 7, 8, 9, 2, 4, 6, 8, 1, 3, 5, 7, 9, 7, 5, 3 };
                        string code = $"{strPayAmount}{bankcode.PadLeft(13, '0')}";
                        for (int i = 0; i < arrtmp14.Length; i++)
                        {
                            tv += code[i].ToInt32() * arrtmp14[i];
                        }
                    }
                    break;
                case 15:
                    {
                        int[] arrtmp16 = { 1, 2, 3, 4, 5, 6, 7, 8, 9, 2, 4, 6, 8, 1, 3, 5, 7, 9, 7, 5, 3, 1, 2 };
                        string code = $"{strPayAmount}{bankcode}";
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
                _ => tv.ToString().Substring(0, 1),
            };
        }
        #endregion
        #region 超商條碼規則
        /// <summary>
        /// 超商檢碼
        /// </summary>
        /// <param name="barcode1"></param>
        /// <param name="payNo"></param>
        /// <param name="barDetailDate"></param>
        /// <param name="amount"></param>
        /// <returns></returns>
        private static string GetMarketCheckCode(string barcode1, string payNo, string amount)
        {
            string bar1 = barcode1, bar2 = payNo.PadLeft(16, '0'), bar3 = $"{barcode1.Substring(0, 4)}{amount}", result = string.Empty;
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
        #endregion
        #region 郵局條碼規則
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
        #endregion
        #endregion
    }
}
