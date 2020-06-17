using SKGPortalCore.Core.DB;
using SKGPortalCore.Core.Libary;
using SKGPortalCore.Model.BillData;
using SKGPortalCore.Model.Report;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;

namespace SKGPortalCore.Repository.SKGPortalCore.Business.Report
{
    internal static class BizReceiptBillRpt
    {
        #region Public
        /// <summary>
        /// 獲取無帳單收款資料報表
        /// </summary>
        /// <param name="dataAccess"></param>
        public static List<NoBillReceiptRptModel> NoBillReceiptRpt(ApplicationDbContext dataAccess, string customerCode)
        {
            List<NoBillReceiptRptModel> result = new List<NoBillReceiptRptModel>();
            result.AddRange(
            dataAccess.Set<ReceiptBillModel>().Where(p =>
            (customerCode.IsNullOrEmpty() || p.CustomerCode.Equals(customerCode))
            ).Select(p => new NoBillReceiptRptModel()
            {
                BillNo = p.BillNo,
                VirtualAccountCode = p.VirtualAccountCode,
                TradeDate = p.TradeDate,
                ExpectRemitDate = p.ExpectRemitDate,
                ChannelId = p.Channel.ChannelId,
                ChannelName = p.Channel.ChannelName,
                HadPayAmount = p.PayAmount
            }));
            return result;
        }
        /// <summary>
        /// 獲取手續費報表
        /// </summary>
        /// <param name="dataAccess"></param>
        public static DataTable GetChannelTotalFeeRpt(ApplicationDbContext dataAccess, string customerId)
        {
            List<ChannelTotalFeeRptModel> lst = new List<ChannelTotalFeeRptModel>();
            lst.AddRange(dataAccess.Set<ReceiptBillModel>().Where(p =>
            (customerId.IsNullOrEmpty() || p.BizCustomer.CustomerId.Equals(customerId))
            ).
                Select(p => new ChannelTotalFeeRptModel()
                {
                    CustomerName = p.BizCustomer.Customer.CustomerName,
                    RealAccount = p.BizCustomer.RealAccount,
                    CustomerCode = p.CustomerCode,
                    ChannelId = p.Channel.ChannelId,
                    ChannelName = p.Channel.ChannelName,
                    TotalFee = p.TotalFee,
                }));
            DataTable result = CreateDynamicDataTable(lst);
            Dictionary<string, DataRow> dic = new Dictionary<string, DataRow>();
            DataRow row;
            result.BeginLoadData();
            foreach (var data in lst)
            {
                string key = data.CustomerCode;
                if (!dic.ContainsKey(key))
                {
                    row = result.NewRow();
                    result.Rows.Add(row);
                    row[nameof(data.CustomerCode)] = data.CustomerCode;
                    row[nameof(data.RealAccount)] = data.RealAccount;
                    row[nameof(data.CustomerName)] = data.CustomerName;
                    row[nameof(data.TotalFee)] = decimal.Zero;
                    dic.Add(key, row);
                }
                row = dic[key];
                string dynamicColName = $"{nameof(data.ChannelId)}_{data.ChannelId}";
                row[dynamicColName] = row[dynamicColName].ToDecimal() + data.TotalFee;
                row[nameof(data.TotalFee)] = row[nameof(data.TotalFee)].ToDecimal() + data.TotalFee;
            }
            result.EndLoadData();
            return result;
        }
        /// <summary>
        /// 獲取收款明細報表
        /// (舊：銷帳明細資料查詢)
        /// </summary>
        /// <param name="dataAccess"></param>
        public static List<ReceiptRptModel> GetReceiptRpt
        (ApplicationDbContext dataAccess, string customerId, string customerCode, string[] channelIds, DateTime beginDate, DateTime endDate)
        {
            List<ReceiptRptModel> result = new List<ReceiptRptModel>();
            result.AddRange(
            dataAccess.Set<ReceiptBillModel>().Where(p =>
                (customerId.IsNullOrEmpty() || p.BizCustomer.CustomerId.Equals(customerId)) &&
                (customerCode.IsNullOrEmpty() || p.CustomerCode.Equals(customerCode)) &&
                (channelIds.Length == 0 || channelIds.Contains(p.ChannelId)) &&
                (p.TradeDate >= beginDate) &&
                (p.TradeDate <= endDate)
            ).Select(p => new ReceiptRptModel
            {
                BillNo = p.BillNo,
                TradeDate = p.TradeDate,
                TransDate = p.TransDate,
                AccountDeptId = p.BizCustomer.AccountDeptId,
                HadPayAmount = p.PayAmount,
                Fee = p.TotalFee,
                IncomeAmount = p.PayAmount - p.TotalFee,
                VirtualAccountCode = p.VirtualAccountCode,
                ChannelId = p.Channel.ChannelId,
                ChannelName = p.Channel.ChannelName,
            }));
            return result;
        }
        /// <summary>
        /// 獲取總收款報表-客戶別
        /// </summary>
        public static List<TotalReceiptRpt> GetTotalReceipt_Customer(ApplicationDbContext dataAccess, DateTime tradeDate)
        {
            List<TotalReceiptRpt> result = new List<TotalReceiptRpt>();
            result.AddRange(dataAccess.Set<ReceiptBillModel>().Where(p =>
           tradeDate.GetFirstDate() <= p.TradeDate && tradeDate.GetLastDate() >= p.TradeDate
            ).Select(p => new TotalReceiptRpt
            {
                CustomerId = p.BizCustomer.CustomerId,
                CustomerName = p.BizCustomer.Customer.CustomerName,
                AccountDeptId = p.BizCustomer.AccountDeptId,
                DeptName = p.BizCustomer.AccountDept.DeptName,
                CreateTime = p.BizCustomer.CreateTime,
                TradeDate = p.TradeDate,
                PayAmount = p.PayAmount,
                ChannelTotalFee = p.ChannelTotalFee,
            }));
            SumListA(result);
            return result;
        }
        /// <summary>
        /// 獲取總收款報表-通路別
        /// </summary>
        public static DataTable GetTotalReceipt_Channel(ApplicationDbContext dataAccess, DateTime tradeDate)
        {
            List<TotalReceiptRpt> lst = new List<TotalReceiptRpt>();
            lst.AddRange(dataAccess.Set<ReceiptBillModel>().Where(p =>
           tradeDate.GetFirstDate() <= p.TradeDate && tradeDate.GetLastDate() >= p.TradeDate
            ).Select(p => new TotalReceiptRpt
            {
                CustomerId = p.BizCustomer.CustomerId,
                CustomerName = p.BizCustomer.Customer.CustomerName,
                AccountDeptId = p.BizCustomer.AccountDeptId,
                DeptName = p.BizCustomer.AccountDept.DeptName,
                CreateTime = p.BizCustomer.CreateTime,
                ChannelId = p.Channel.ChannelId,
                ChannelName = p.Channel.ChannelName,
                TradeDate = p.TradeDate,
                PayAmount = p.PayAmount,
                ChannelTotalFee = p.ChannelTotalFee,
            }));
            DataTable result = CreateDynamicDataTable(lst);

            Dictionary<string, DataRow> dic = new Dictionary<string, DataRow>();
            DataRow row;
            result.BeginLoadData();
            foreach (var data in lst)
            {
                string key = data.CustomerId;
                if (!dic.ContainsKey(key))
                {
                    row = result.NewRow();
                    result.Rows.Add(row);
                    row[nameof(data.CustomerId)] = data.CustomerId;
                    row[nameof(data.CustomerName)] = data.CustomerName;
                    row[nameof(data.AccountDeptId)] = data.AccountDeptId;
                    row[nameof(data.DeptName)] = data.DeptName;
                    row[nameof(data.CreateTime)] = data.CreateTime;
                    row[nameof(data.TradeDate)] = data.TradeDate;
                    row[nameof(data.TotalCount)] = decimal.Zero;
                    row[nameof(data.PayAmount)] = decimal.Zero;
                    row[nameof(data.ChannelTotalFee)] = decimal.Zero;
                    dic.Add(key, row);
                }
                row = dic[key];

                string dynamicColNameA = $"{nameof(data.ChannelId)}_{data.ChannelId}_Count";
                row[dynamicColNameA] = row[dynamicColNameA].ToDecimal() + data.TotalCount;
                row[nameof(data.TotalCount)] = row[nameof(data.TotalCount)].ToDecimal() + data.TotalCount;

                string dynamicColNameB = $"{nameof(data.ChannelId)}_{data.ChannelId}_Amount";
                row[dynamicColNameB] = row[dynamicColNameB].ToDecimal() + data.PayAmount;
                row[nameof(data.PayAmount)] = row[nameof(data.PayAmount)].ToDecimal() + data.PayAmount;

                string dynamicColNameC = $"{nameof(data.ChannelId)}_{data.ChannelId}_Fee";
                row[dynamicColNameB] = row[dynamicColNameB].ToDecimal() + data.ChannelTotalFee;
                row[nameof(data.ChannelTotalFee)] = row[nameof(data.ChannelTotalFee)].ToDecimal() + data.ChannelTotalFee;
            }
            result.EndLoadData();

            lst.SumListData((x, y) => new { x.ChannelId, y.TotalCount });

            return result;
        }
        #endregion

        #region Private
        /// <summary>
        /// 
        /// </summary>
        /// <param name="lst"></param>
        private static void SumListA(List<TotalReceiptRpt> lst)
        {
            Dictionary<string, TotalReceiptRpt> dic = new Dictionary<string, TotalReceiptRpt>();
            string key;
            for (int i = lst.Count - 1; i >= 0; i--)
            {
                var data = lst[i];
                key = data.CustomerId;
                if (!dic.ContainsKey(key))
                {
                    data.TotalCount++;
                    dic.Add(key, data);
                }
                else
                {
                    dic[key].TotalCount++;
                    dic[key].PayAmount += data.PayAmount;
                    dic[key].ChannelTotalFee += data.ChannelTotalFee;
                    lst.Remove(data);
                }
            }
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="lst"></param>
        /// <returns></returns>
        private static DataTable CreateDynamicDataTable(List<ChannelTotalFeeRptModel> lst)
        {
            lst.Sort((x, y) => { return x.ChannelId.CompareTo(y.ChannelId); });
            DataTable result = new DataTable();
            string propName;
            foreach (var prop in typeof(ChannelTotalFeeRptModel).GetProperties())
            {
                propName = prop.Name;
                switch (propName)
                {
                    case nameof(ChannelTotalFeeRptModel.ChannelId):
                    case nameof(ChannelTotalFeeRptModel.ChannelName):
                        break;
                    default:
                        {
                            DataColumn col = new DataColumn(propName)
                            { Caption = ResxManage.GetDescription(prop) };
                            result.Columns.Add(col);
                        }
                        break;
                }
            }
            foreach (var m in lst)
            {
                string dynamicColName = $"{nameof(m.ChannelId)}_{m.ChannelId}";
                if (!result.Columns.Contains(dynamicColName))
                {
                    DataColumn col = new DataColumn(dynamicColName)
                    { Caption = m.ChannelName };
                    result.Columns.Add(col);
                }
            }
            return result;
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="lst"></param>
        /// <returns></returns>
        private static DataTable CreateDynamicDataTable(List<TotalReceiptRpt> lst)
        {
            lst.Sort((x, y) => { return x.ChannelId.CompareTo(y.ChannelId); });
            DataTable result = new DataTable();
            string propName;
            foreach (var prop in typeof(TotalReceiptRpt).GetProperties())
            {
                propName = prop.Name;
                switch (propName)
                {
                    case nameof(TotalReceiptRpt.ChannelId):
                    case nameof(TotalReceiptRpt.ChannelName):
                        break;
                    default:
                        {
                            DataColumn col = new DataColumn(propName)
                            { Caption = ResxManage.GetDescription(prop) };
                            result.Columns.Add(col);
                        }
                        break;
                }
            }
            foreach (var m in lst)
            {
                string dynamicColName = $"{nameof(m.ChannelId)}_{m.ChannelId}_Count";
                if (!result.Columns.Contains(dynamicColName))
                {
                    DataColumn col = new DataColumn(dynamicColName)
                    { Caption = m.ChannelName };
                    result.Columns.Add(col);
                }
                dynamicColName = $"{nameof(m.ChannelId)}_{m.ChannelId}_Amount";
                if (!result.Columns.Contains(dynamicColName))
                {
                    DataColumn col = new DataColumn(dynamicColName)
                    { Caption = m.ChannelName };
                    result.Columns.Add(col);
                }

                dynamicColName = $"{nameof(m.ChannelId)}_{m.ChannelId}_Fee";
                if (!result.Columns.Contains(dynamicColName))
                {
                    DataColumn col = new DataColumn(dynamicColName)
                    { Caption = m.ChannelName };
                    result.Columns.Add(col);
                }
            }
            return result;
        }
        #endregion
    }
}
