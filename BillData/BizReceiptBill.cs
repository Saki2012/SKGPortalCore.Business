using SKGPortalCore.Lib;
using SKGPortalCore.Model;
using SKGPortalCore.Model.BillData;
using SKGPortalCore.Model.MasterData;
using System;
using System.Collections.Generic;

namespace SKGPortalCore.Business.BillData
{
    /// <summary>
    /// 收款單-商業邏輯
    /// </summary>
    public class BizReceiptBill : BizBase
    {
        #region Property
        private const string I0O = "I0O";
        #endregion

        #region Construct
        public BizReceiptBill() : base() { }
        #endregion

        #region Public
        /// <summary>
        /// 計算 每筆總手續費之「清算手續費」(內扣)
        /// (每筆總手續費-通路手續費) * (100-分潤%)/100。
        /// </summary>
        /// <param name="totalFee"></param>
        /// <param name="channelFee"></param>
        /// <param name="splitting"></param>
        /// <returns></returns>
        public static decimal FeeDeduct(decimal totalFee, decimal channelFee, decimal splitting)
        {
            return Math.Ceiling((totalFee - channelFee) * ((100m - splitting) / 100m));
        }
        /// <summary>
        /// 計算 每筆總手續費之「清算手續費」(外加)
        /// 每筆總手續費 * (100-分潤%)/100。
        /// </summary>
        /// <param name="totalFee"></param>
        /// <param name="splitting"></param>
        /// <returns></returns>
        public static decimal FeePlus(decimal totalFee, decimal splitting)
        {
            return Math.Ceiling(totalFee * ((100m - splitting) / 100m));
        }
        #endregion

        #region ImportClass
        /// <summary>
        /// 繳款資訊單-超商
        /// </summary>
        public class ReceiptBillImportMarket : IReceiptBillImport
        {
            #region Property
            private const int len = 120;
            private List<ChannelMapModel> Channel = new List<ChannelMapModel>();//{ get; set; }
            private List<CollectionTypeModel> CollectionType = new List<CollectionTypeModel>();//{ get; set; }
            #endregion

            #region Interface Implementation 
            List<ReceiptBillSet> IReceiptBillImport.DataAnalyze(List<string> fileContent)
            {
                List<ReceiptBillSet> receiptBills = new List<ReceiptBillSet>();
                List<ReceiptInfoBillMarketModel> receiptInfos = new List<ReceiptInfoBillMarketModel>();
                if (null == fileContent) return receiptBills;
                for (int i = 0; i < fileContent.Count; i++)
                {
                    int rowNo = i + 1;
                    string str = fileContent[i];
                    if (str?.Length == 0) continue;
                    if (len != str.ByteLen()) continue;// Log.AddErrorMessage(MessageCode.Code1003,rowNo,len);
                    bool isI0O = str.ByteSubString(9, 8).Trim().ToUpper().CompareTo(I0O) == 0;
                    switch (str[0])
                    {
                        case '1':
                            GetHeaderData(rowNo, isI0O, str);
                            break;
                        case '2':
                            {
                                ReceiptInfoBillMarketModel data = GetDetailData(isI0O, str);
                                CheckDetailData(rowNo, data);
                                receiptInfos.Add(data);
                                ACCFTT applyForm = AnalyzeApplyForm(CanalisType.Market, data.CompareCode);
                                string bankCode = AnalyzeBankCode(CanalisType.Market, applyForm, data.CompareCode);
                                ReceiptBillSet set = GetReceiptBillSet(applyForm, data);
                                if (null != set) receiptBills.Add(set);
                            }
                            break;
                        case '3':
                            GetEndData(rowNo, isI0O, str);
                            break;
                    }
                }
                ImportInfoData(receiptInfos);
                return receiptBills;
            }
            #endregion

            #region Private
            /// <summary>
            /// 
            /// </summary>
            /// <param name="isI0O"></param>
            /// <param name="str"></param>
            private void GetHeaderData(int rowNo, bool isI0O, string str)
            {
                HeaderData data = new HeaderData();
                if (isI0O)
                {
                    data.Channel = str.ByteSubString(1, 8).Trim();
                    data.CollectionType = str.ByteSubString(9, 8).Trim();
                    data.TransDate = str.ByteSubString(20, 8).Trim();
                }
                else
                {
                    data.Channel = str.ByteSubString(9, 8).Trim();
                    data.CollectionType = str.ByteSubString(1, 8).Trim(); ;
                    data.TransDate = str.ByteSubString(21, 8).Trim();
                }
                CheckHeaderData(rowNo, data);
            }
            /// <summary>
            /// 檢查表頭資料
            /// </summary>
            /// <param name="data"></param>
            private void CheckHeaderData(int rowNo, HeaderData data)
            {
                if (!Channel.Exists(p => p.TransCode == data.Channel)) { throw new Exception("第{0}行：交易代號{1}不存在於「交易代號與平台通路代號關聯表」"); }
                if (!CollectionType.Exists(p => p.CollectionTypeId == data.CollectionType)) { throw new Exception("第{0}行：代收類別{1}不存在於「代收類別資料」"); }
                if (!DateTime.TryParse(data.TransDate.ToADDateFormat(), out DateTime result)) { throw new Exception("第{0}行：入/扣款日期不為有效日期"); }
            }
            /// <summary>
            /// 解析「繳款資訊單-超商」資料
            /// </summary>
            /// <param name="isI0O"></param>
            /// <param name="str"></param>
            private ReceiptInfoBillMarketModel GetDetailData(bool isI0O, string str)
            {
                ReceiptInfoBillMarketModel data = new ReceiptInfoBillMarketModel();
                if (isI0O)
                {
                    data.Channel = str.ByteSubString(1, 8).Trim();
                    data.CollectionType = str.ByteSubString(9, 8).Trim();
                    data.TransDate = str.ByteSubString(17, 8).Trim();
                    data.TradeDate = str.ByteSubString(25, 8).Trim();
                    data.CompareCode = str.ByteSubString(33, 16).Trim();
                    data.Amount = str.ByteSubString(55, 9).Trim();
                    data.Branch = str.ByteSubString(82, 6).Trim();
                }
                else
                {
                    data.Channel = str.ByteSubString(9, 8).Trim();
                    data.CollectionType = str.ByteSubString(1, 8).Trim();
                    data.TransDate = str.ByteSubString(44, 8).Trim();
                    data.TradeDate = str.ByteSubString(52, 8).Trim();
                    data.CompareCode = str.ByteSubString(69, 20).Trim();
                    data.Amount = str.ByteSubString(89, 15).Trim();
                    data.Branch = str.ByteSubString(17, 8).Trim();
                }
                data.Source = str;
                return data;
            }
            /// <summary>
            /// 檢查明細資料
            /// </summary>
            /// <param name="rowNo"></param>
            /// <param name="data"></param>
            private void CheckDetailData(int rowNo, ReceiptInfoBillMarketModel data)
            {
                if (!Channel.Exists(p => p.TransCode == data.Channel)) { throw new Exception("第{0}行：交易代號{1}不存在於「交易代號與平台通路代號關聯表」"); }
                if (!CollectionType.Exists(p => p.CollectionTypeId == data.CollectionType)) { throw new Exception("第{0}行：代收類別{1}不存在於「代收類別資料」"); }
                if (data.Branch.IsNullOrEmpty()) { throw new Exception("第{0}行：代收機構不可為空白");/*??*/ }
                if (!DateTime.TryParse(data.TransDate.ToADDateFormat(), out DateTime transDate)) { throw new Exception("第{0}行：門市會計日不為有效日期"); }
                if (!DateTime.TryParse(data.TradeDate.ToADDateFormat(), out DateTime tradeDate)) { throw new Exception("第{0}行：顧客繳費日不為有效日期"); }
                if (16 != data.CompareCode.Length) { throw new Exception("第{0}行：銷帳編號不為16碼"); }
                if (!decimal.TryParse(data.Amount, out decimal amount)) { throw new Exception("第{0}行：代收金額不為有效數字"); }
            }
            /// <summary>
            /// 
            /// </summary>
            /// <param name="isI0O"></param>
            /// <param name="str"></param>
            private void GetEndData(int rowNo, bool isI0O, string str)
            {
                EndData data = new EndData();
                if (isI0O)
                {
                    data.TotalAmount = str.ByteSubString(29, 14).Trim();
                    data.Count = str.ByteSubString(43, 10).Trim();
                }
                else
                {
                    data.TotalAmount = str.ByteSubString(28, 14).Trim();
                    data.Count = str.ByteSubString(44, 10).Trim();
                }
                CheckEndData(rowNo, data);
            }
            /// <summary>
            /// 檢查表尾資料
            /// </summary>
            /// <param name="rowNo"></param>
            /// <param name="data"></param>
            private void CheckEndData(int rowNo, EndData data)
            {
                if (!decimal.TryParse(data.TotalAmount, out decimal totalAmount)) { throw new Exception("第{0}行：代收總金額不為有效數字"); }
                if (!int.TryParse(data.Count, out int count)) { throw new Exception("第{0}行：代收總筆數不為有效數字"); }
            }
            /// <summary>
            /// 
            /// </summary>
            /// <returns></returns>
            private ReceiptBillSet GetReceiptBillSet(ACCFTT applyForm, ReceiptInfoBillMarketModel data)
            {

                ReceiptBillSet set = new ReceiptBillSet()
                {
                    ReceiptBill = new ReceiptBillModel()
                    {
                        CollectionTypeId = data.CollectionType,
                        CollectionTypeDetailRowId = 0,
                        TransDate = data.TransDate.ToDateTime(),
                        TradeDate = data.TradeDate.ToDateTime(),
                        CompareCode = data.CompareCode,
                        PayAmount = data.Amount.ToDecimal(),
                        //CustomerId = applyForm?.CustomerId,
                        //CustomerCode = applyForm?.CustomerCode,
                        BankCode = "",
                        RemitDate = DateTime.Now,
                        CreateStaff = data.CreateStaff,
                        CreateTime = data.CreateTime,
                        ModifyStaff = data.ModifyStaff,
                        ModifyTime = data.ModifyTime,
                        Source = "ReceiptInfoBillMarketModel",
                        SourceId = data.Id
                    }
                };
                return set;
            }
            /// <summary>
            /// 
            /// </summary>
            /// <param name="receiptInfos"></param>
            private void ImportInfoData(List<ReceiptInfoBillMarketModel> receiptInfos)
            {
                throw new NotImplementedException();
            }
            /// <summary>
            /// 
            /// </summary>
            private class HeaderData
            {
                /// <summary>
                /// 通路
                /// </summary>
                public string Channel { get; set; }
                /// <summary>
                /// 代收類別
                /// </summary>
                public string CollectionType { get; set; }
                /// <summary>
                /// 入/扣款日期
                /// </summary>
                public string TransDate { get; set; }
            }
            /// <summary>
            /// 
            /// </summary>
            private class EndData
            {
                /// <summary>
                /// 代收總金額
                /// </summary>
                public string TotalAmount { get; set; }
                /// <summary>
                /// 代收總筆數
                /// </summary>
                public string Count { get; set; }
            }
            #endregion
        }
        /// <summary>
        /// 繳款資訊單-郵局
        /// </summary>
        public class ReceiptBillImportPost : IReceiptBillImport
        {
            #region Property
            private const int len = 110;
            #endregion

            #region Interface Implementation 
            List<ReceiptBillSet> IReceiptBillImport.DataAnalyze(List<string> fileContent)
            {
                List<ReceiptBillSet> receiptBills = new List<ReceiptBillSet>();
                List<ReceiptInfoBillPostModel> receiptInfos = new List<ReceiptInfoBillPostModel>();
                if (null == fileContent) return receiptBills;
                for (int i = 0; i < fileContent.Count; i++)
                {
                    int rowNo = i + 1;
                    string str = fileContent[i];
                    if (str?.Length == 0) continue;
                    if (len != str.ByteLen()) throw new Exception();
                    string f = str.ByteSubString(0, 1).Trim();
                    if (f == "H" || f == "T") continue;
                    ReceiptInfoBillPostModel data = GetDetailData(str);
                    CheckDetailData(rowNo, data);
                    receiptInfos.Add(data);
                    ACCFTT applyForm = AnalyzeApplyForm(CanalisType.Market, data.CompareCode);
                    string bankCode = AnalyzeBankCode(CanalisType.Market, applyForm, data.CompareCode);
                    ReceiptBillSet set = GetReceiptBillSet(applyForm, data);
                    if (null != set) receiptBills.Add(set);
                }
                ImportInfoData(receiptInfos);
                return receiptBills;
            }
            #endregion

            #region Private
            /// <summary>
            /// 
            /// </summary>
            /// <param name="isI0O"></param>
            /// <param name="str"></param>
            private ReceiptInfoBillPostModel GetDetailData(string str)
            {
                ReceiptInfoBillPostModel data = new ReceiptInfoBillPostModel
                {
                    CollectionType = str.ByteSubString(0, 8).Trim(),
                    TransDate = str.ByteSubString(8, 7).Trim(),
                    TradeDate = str.ByteSubString(8, 7).Trim(),
                    Branch = str.ByteSubString(15, 6).Trim(),
                    Channel = str.ByteSubString(21, 4).Trim(),
                    PN = str.ByteSubString(32, 1)[0],
                    Amount = str.ByteSubString(33, 11).Trim(),//BarCode3
                    CompareCode = str.ByteSubString(44, 24).Trim(),
                };
                return data;
            }
            private void CheckDetailData(int rowNo, ReceiptInfoBillPostModel data)
            {
                //if (!Channel.Exists(p => p.TransCode == data.Channel)) { throw new Exception("第{0}行：交易代號{1}不存在於「交易代號與平台通路代號關聯表」"); }
                //if (!CollectionType.Exists(p => p.CollectionTypeId == data.CollectionType)) { throw new Exception("第{0}行：代收類別{1}不存在於「代收類別資料」"); }
                if (data.Branch.IsNullOrEmpty()) { throw new Exception("第{0}行：代收機構不可為空白");/*??*/ }
                if (!DateTime.TryParse(data.TradeDate.ToADDateFormat(), out DateTime tradeDate)) { throw new Exception("第{0}行：顧客繳費日不為有效日期"); }
                //if (data.CompareCode.Length) { throw new Exception("第{0}行：銷帳編號不為16碼"); }
                if (!decimal.TryParse(data.Amount, out decimal amount)) { throw new Exception("第{0}行：代收金額不為有效數字"); }
                if (data.PN != '+' || data.PN != '-') { throw new Exception("第{0}行：存提別不為'+'或'-'"); }

            }
            private ReceiptBillSet GetReceiptBillSet(ACCFTT applyForm, ReceiptInfoBillPostModel data)
            {
                throw new NotImplementedException();
            }
            private void ImportInfoData(List<ReceiptInfoBillPostModel> receiptInfos)
            {
                throw new NotImplementedException();
            }
            #endregion
        }
        /// <summary>
        /// 繳款資訊單-銀行
        /// </summary>
        public class ReceiptBillImportBank : IReceiptBillImport
        {
            #region Property
            private const int len = 128;
            #endregion

            #region Interface Implementation 
            List<ReceiptBillSet> IReceiptBillImport.DataAnalyze(List<string> fileContent)
            {
                List<ReceiptBillSet> receiptBills = new List<ReceiptBillSet>();
                List<ReceiptInfoBillBankModel> receiptInfos = new List<ReceiptInfoBillBankModel>();
                if (null == fileContent) return receiptBills;
                for (int i = 0; i < fileContent.Count; i++)
                {
                    int rowNo = i + 1;
                    string str = fileContent[i];
                    if (str?.Length == 0) continue;
                    if (len != str.ByteLen()) throw new Exception();
                    ReceiptInfoBillBankModel data = GetDetailData(str);
                    CheckDetailData(rowNo, data);
                    receiptInfos.Add(data);
                    ACCFTT applyForm = AnalyzeApplyForm(CanalisType.Market, data.CompareCode);
                    string bankCode = AnalyzeBankCode(CanalisType.Market, applyForm, data.CompareCode);
                    ReceiptBillSet set = GetReceiptBillSet(applyForm, data);
                    if (null != set) receiptBills.Add(set);
                }
                ImportInfoData(receiptInfos);
                return receiptBills;
            }
            #endregion

            #region Private
            private ReceiptInfoBillBankModel GetDetailData(string str)
            {
                ReceiptInfoBillBankModel data = new ReceiptInfoBillBankModel()
                {
                    CollectionType = "Bank999",
                    TradeDate = str.ByteSubString(13, 27),
                    TransDate = str.ByteSubString(13, 27),
                    CompareCode = str.ByteSubString(27, 16),
                    Amount = str.ByteSubString(43, 11).ToDecimal(),
                    Branch = str.ByteSubString(64, 4).Trim(),
                    TradeCode = str.ByteSubString(68, 2).Trim(),
                    Channel = str.ByteSubString(70, 2),
                };
                return data;
            }
            private void CheckDetailData(int rowNo, ReceiptInfoBillBankModel data)
            {
                throw new NotImplementedException();
            }
            private void ImportInfoData(List<ReceiptInfoBillBankModel> receiptInfos)
            {
                throw new NotImplementedException();
            }
            private ReceiptBillSet GetReceiptBillSet(ACCFTT applyForm, ReceiptInfoBillBankModel data)
            {
                throw new NotImplementedException();
            }
            #endregion
        }
        /// <summary>
        /// 繳款資訊單-農金
        /// </summary>
        public class ReceiptBillImportFarm : IReceiptBillImport
        {
            #region Property
            private const int len = 120;
            #endregion

            #region Interface Implementation 
            List<ReceiptBillSet> IReceiptBillImport.DataAnalyze(List<string> fileContent)
            {
                List<ReceiptBillSet> receiptBills = new List<ReceiptBillSet>();
                List<ReceiptInfoBillFarmModel> receiptInfos = new List<ReceiptInfoBillFarmModel>();
                if (null == fileContent) return receiptBills;
                for (int i = 0; i < fileContent.Count; i++)
                {
                    int rowNo = i + 1;
                    string str = fileContent[i];
                    if (str?.Length == 0) continue;
                    if (len != str.ByteLen()) throw new Exception();
                    bool isI0O = str.ByteSubString(9, 8).Trim().ToUpper().CompareTo(I0O) == 0;
                    switch (str[0])
                    {
                        case '1':
                            GetHeaderData(isI0O, str);
                            break;
                        case '2':
                            {
                                ReceiptInfoBillFarmModel data = GetDetailData(isI0O, str);
                                CheckDetailData(rowNo, data);
                                receiptInfos.Add(data);
                                ACCFTT applyForm = AnalyzeApplyForm(CanalisType.Market, data.CompareCode);
                                string bankCode = AnalyzeBankCode(CanalisType.Market, applyForm, data.CompareCode);
                                ReceiptBillSet set = GetReceiptBillSet(applyForm, data);
                                if (null != set) receiptBills.Add(set);
                            }
                            break;
                        case '3':
                            GetEndData(isI0O, str);
                            break;
                    }
                }
                return receiptBills;
            }


            #endregion

            #region Private
            /// <summary>
            /// 
            /// </summary>
            /// <param name="isI0O"></param>
            /// <param name="str"></param>
            private void GetHeaderData(bool isI0O, string str)
            {
                HeaderData data = new HeaderData();
                if (isI0O)
                {
                    data.Channel = str.ByteSubString(1, 8).Trim();//待Mapping
                    data.CollectionType = str.ByteSubString(9, 8).Trim();
                    data.TransDate = Convert.ToDateTime(str.ByteSubString(20, 8).Trim());
                }
                else
                {
                    data.Channel = str.ByteSubString(9, 8).Trim();//待Mapping
                    data.CollectionType = str.ByteSubString(1, 8).Trim(); ;
                    data.TransDate = Convert.ToDateTime(str.ByteSubString(21, 8).Trim());
                }
            }
            /// <summary>
            /// 
            /// </summary>
            /// <param name="isI0O"></param>
            /// <param name="str"></param>
            private ReceiptInfoBillFarmModel GetDetailData(bool isI0O, string str)
            {
                ReceiptInfoBillFarmModel data = new ReceiptInfoBillFarmModel();
                if (isI0O)
                {
                    data.Channel = str.ByteSubString(1, 8).Trim();//待Mapping
                    data.CollectionType = str.ByteSubString(9, 8).Trim();
                    data.TransDate = str.ByteSubString(17, 8).Trim();
                    data.TradeDate = str.ByteSubString(25, 8).Trim();
                    data.CompareCode = str.ByteSubString(33, 16).Trim();
                    data.Amount = str.ByteSubString(55, 9).Trim();
                    data.Branch = str.ByteSubString(82, 6).Trim();
                }
                else
                {
                    data.Channel = str.ByteSubString(9, 8).Trim();//待Mapping
                    data.CollectionType = str.ByteSubString(1, 8).Trim();
                    data.TransDate = str.ByteSubString(44, 8).Trim();
                    data.TradeDate = str.ByteSubString(52, 8).Trim();
                    data.CompareCode = str.ByteSubString(69, 20).Trim();//BarCode2
                    data.Amount = str.ByteSubString(89, 15).Trim();//BarCode3
                    data.Branch = str.ByteSubString(17, 8).Trim();
                }
                return data;
            }
            private ReceiptBillSet GetReceiptBillSet(ACCFTT applyForm, ReceiptInfoBillFarmModel data)
            {
                throw new NotImplementedException();
            }
            private void CheckDetailData(int rowNo, ReceiptInfoBillFarmModel data)
            {
                throw new NotImplementedException();
            }
            /// <summary>
            /// 
            /// </summary>
            /// <param name="isI0O"></param>
            /// <param name="str"></param>
            private void GetEndData(bool isI0O, string str)
            {
                EndData data = new EndData();
                if (isI0O)
                {
                    data.TotalAmount = str.ByteSubString(29, 14).Trim().ToDecimal();
                    data.Count = str.ByteSubString(43, 10).Trim().ToInt32();
                }
                else
                {
                    data.TotalAmount = str.ByteSubString(28, 14).Trim().ToDecimal();
                    data.Count = str.ByteSubString(44, 10).Trim().ToInt32();
                }
            }
            private class HeaderData
            {
                /// <summary>
                /// 通路
                /// </summary>
                public string Channel { get; set; }
                /// <summary>
                /// 代收類別
                /// </summary>
                public string CollectionType { get; set; }
                /// <summary>
                /// 入/扣款日期
                /// </summary>
                public DateTime TransDate { get; set; }
            }
            private class EndData
            {
                /// <summary>
                /// 代收總金額
                /// </summary>
                public decimal TotalAmount { get; set; }
                /// <summary>
                /// 代收總筆數
                /// </summary>
                public int Count { get; set; }
            }
            #endregion
        }
        /// <summary>
        /// 解析Code對應的服務申請書(CustomerCode)
        /// </summary>
        /// <param name="compareCode"></param>
        /// <returns></returns>
        private static ACCFTT AnalyzeApplyForm(CanalisType channelType, string compareCode)
        {
            List<ACCFTT> model = new List<ACCFTT>();
            switch (channelType)
            {
                case CanalisType.Post:
                    compareCode = compareCode.Substring(7).TrimStart('0');
                    break;
                case CanalisType.Farm:
                case CanalisType.Market:
                    compareCode = compareCode.TrimStart('0');
                    break;
            }
            //List<ACCFTT> m = model.Where(p => p.CustomerCode == compareCode.Substring(0, 6)).ToList();
            //if (m?.Count == 0)
            //{
            //    m = model.Where(p => p.CustomerCode == compareCode.Substring(0, 4)).ToList();
            //    if (m?.Count == 0)
            //    {
            //        m = model.Where(p => p.CustomerCode == compareCode.Substring(0, 3)).ToList();
            //    }
            //}
            //if (m?.Count > 0)
            //    return m[0];
            return null;
        }
        /// <summary>
        /// 解析銷帳編號
        /// </summary>
        /// <param name="type"></param>
        /// <param name="compareCode"></param>
        private static string AnalyzeBankCode(CanalisType type, ACCFTT applyForm, string compareCode)
        {
            string bankCode = compareCode;
            switch (type)
            {
                case CanalisType.Bank:
                    {
                        //if (null != applyForm && applyForm.VirtualAccount3 != VirtualAccount3.NoverifyCode)
                        //    bankCode = compareCode.Substring(0, compareCode.Length - 1);
                    }
                    break;
                case CanalisType.Post:
                    {
                        if (null == applyForm)
                            bankCode = compareCode.Substring(7, 16);
                        else
                        {
                            //if (applyForm.VirtualAccount3 == VirtualAccount3.NoverifyCode)
                            //    bankCode = compareCode.Substring(7, 16).Trim('0').Substring(0, applyForm.VirtualAccountLen);
                            //else
                            //    bankCode = compareCode.Substring(7, 16).Trim('0').Substring(0, applyForm.VirtualAccountLen - 1);
                        }
                    }
                    break;
                case CanalisType.Farm:
                case CanalisType.Market:
                    {
                        if (null != applyForm)
                        {
                            //if (applyForm.VirtualAccount3 == VirtualAccount3.NoverifyCode)
                            //    bankCode = compareCode.TrimStart('0').Substring(0, applyForm.VirtualAccountLen);
                            //else
                            //    bankCode = compareCode.TrimStart('0').Substring(0, applyForm.VirtualAccountLen - 1);
                        }
                    }
                    break;
            }
            return bankCode;
        }
        /// <summary>
        /// 計算預計匯款日
        /// </summary>
        /// <param name="receiptBillSet"></param>
        private static void SetExpectRemitDate(ReceiptBillSet receiptBillSet)
        {


        }
        private static decimal GetChannelFee(string channelId, string collectionTypeId, decimal amount)
        {
            return 0;
        }
        private static decimal GetBankFee(string customerId, string customerCode, CanalisType channelType, FeeType feeType)
        {
            switch (feeType)
            {
                case FeeType.ClearFee:
                    break;
                case FeeType.TotalFee:
                    break;
                default:
                    return 0;
            }
            return 0;
        }
        private static decimal GetIntroducerFee(string customerId, string customerCode)
        {
            return 0;
        }
        private static decimal GetHiTrustFee(string customerId, string customerCode)
        {
            return 0;
        }
        #endregion
    }

    /// <summary>
    /// 繳款資訊單導入介面
    /// </summary>
    public interface IReceiptBillImport
    {
        /// <summary>
        /// 解析通路資訊流
        /// </summary>
        /// <returns></returns>
        public List<ReceiptBillSet> DataAnalyze(List<string> fileContent);
    }
}
