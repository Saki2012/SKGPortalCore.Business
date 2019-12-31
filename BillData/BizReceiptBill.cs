using System;
using System.Collections.Generic;
using System.Linq;
using SKGPortalCore.Data;
using SKGPortalCore.Lib;
using SKGPortalCore.Model;
using SKGPortalCore.Model.BillData;
using SKGPortalCore.Model.MasterData;
using SKGPortalCore.Model.MasterData.OperateSystem;
using SKGPortalCore.Repository.BillData;
using SKGPortalCore.Repository.MasterData;

namespace SKGPortalCore.Repository.SKGPortalCore.Business.BillData
{
    /// <summary>
    /// 收款單-商業邏輯
    /// </summary>
    internal static class BizReceiptBill
    {
        #region Public
        /// <summary>
        /// 檢查資料
        /// </summary>
        /// <param name="set"></param>
        /// <param name="message"></param>
        public static void CheckData(ReceiptBillSet set, MessageLog message)
        {


        }
        /// <summary>
        /// 設置欄位值
        /// </summary>
        /// <param name="set"></param>
        /// <param name="action"></param>
        public static void SetData(ReceiptBillSet set, ApplicationDbContext dataAccess, IUserModel user, FuncAction action,
            Dictionary<string, BizCustomerSet> bizCustSetDic, Dictionary<string, CollectionTypeSet> colSetDic, Dictionary<string, ChannelVerifyPeriodModel> periodDic)
        {
            BizCustomerSet bizCustomerSet = GetBizCustomerSet(bizCustSetDic, dataAccess, set.ReceiptBill.BankBarCode);
            GetCollectionTypeSet(dataAccess, colSetDic, set.ReceiptBill.CollectionTypeId, set.ReceiptBill.ChannelId, set.ReceiptBill.PayAmount, out ChargePayType chargePayType, out decimal channelFee);
            ChannelVerifyPeriodModel period = GetChannelVerifyPeriod(dataAccess, periodDic, set.ReceiptBill.CollectionTypeId, set.ReceiptBill.ChannelId);

            set.ReceiptBill.ToBillNo = GetBillNo(dataAccess, set.ReceiptBill.BankBarCode);
            if (action == FuncAction.Create)
            {
                set.ReceiptBill.ExpectRemitDate = GetRemitDate(period, set.ReceiptBill);
                InsertBillReceiptDetail(dataAccess, set.ReceiptBill, set.ReceiptBill.ToBillNo);
                InsertChannelEAccount(dataAccess, user, set);
            }
            GetBizCustFee(bizCustomerSet.BizCustomerFeeDetail, channelFee, chargePayType, ChannelGroupType.Bank, out decimal bankFee, out decimal thirdFee, out decimal hiTrustFee);
            set.ReceiptBill.BankFee = bankFee;
            set.ReceiptBill.ThirdFee = thirdFee;
            set.ReceiptBill.ChannelFee = channelFee;
            set.ReceiptBill.ChargePayType = chargePayType;
        }
        #endregion

        #region Private
        /// <summary>
        /// 
        /// </summary>
        /// <param name="detail"></param>
        /// <param name="channelFee"></param>
        /// <param name="chargePayType"></param>
        /// <param name="channelGroupType"></param>
        /// <param name="bankFee"></param>
        /// <param name="thirdFee"></param>
        /// <param name="hiTrustFee"></param>
        private static void GetBizCustFee(List<BizCustomerFeeDetailModel> detail, decimal channelFee, ChargePayType chargePayType, ChannelGroupType channelGroupType, out decimal bankFee, out decimal thirdFee, out decimal hiTrustFee)
        {
            bankFee = 0; thirdFee = 0; hiTrustFee = 0;
            if (!detail.HasData()) return;
            BizCustomerFeeDetailModel model = detail.FirstOrDefault(p => p.ChannelType == channelGroupType && p.FeeType == FeeType.IntroducerFee);
            hiTrustFee = null == model ? 0 : model.Fee;
            model = detail.FirstOrDefault(p => p.ChannelType == channelGroupType && (p.FeeType == FeeType.ClearFee || p.FeeType == FeeType.TotalFee));
            if (null != model)
            {
                switch (model.FeeType)
                {
                    case FeeType.ClearFee:
                        {
                            bankFee = model.Fee;
                        }
                        break;
                    case FeeType.TotalFee:
                        {
                            switch (chargePayType)
                            {
                                case ChargePayType.Deduction:
                                    {
                                        thirdFee = FeeDeduct(model.Fee, channelFee, model.Percent);
                                        bankFee = model.Fee - thirdFee;
                                    }
                                    break;
                                case ChargePayType.Increase:
                                    {
                                        thirdFee = FeePlus(model.Fee, model.Percent);
                                        bankFee = model.Fee - thirdFee;
                                    }
                                    break;
                            }
                            break;
                        }
                }
            }
        }
        /// <summary>
        /// 獲取月結的預計匯款日
        /// </summary>
        /// <returns></returns>
        private static DateTime GetMonthlyTime()
        {
            return DateTime.MinValue;
        }
        /// <summary>
        /// 獲取旬結的預計匯款日
        /// </summary>
        /// <param name="i"></param>
        /// <returns></returns>
        private static DateTime GetTenDayTime(int i)
        {
            return i switch
            {
                1 => GetEarlyMonthTime(),
                2 => GetMidMonthTime(),
                3 => GetLateMonthTime(),
                _ => DateTime.MinValue,
            };
        }
        /// <summary>
        /// 獲取上旬結的預計匯款日
        /// </summary>
        /// <returns></returns>
        private static DateTime GetEarlyMonthTime() { return DateTime.MinValue; }
        /// <summary>
        /// 獲取中旬結的預計匯款日
        /// </summary>
        /// <returns></returns>
        private static DateTime GetMidMonthTime() { return DateTime.MinValue; }
        /// <summary>
        /// 獲取下旬結的預計匯款日
        /// </summary>
        /// <returns></returns>
        private static DateTime GetLateMonthTime() { return DateTime.MinValue; }
        /// <summary>
        /// 獲取週結的預計匯款日
        /// </summary>
        /// <returns></returns>
        private static DateTime GetWeekTime(DateTime transDate)
        {
            if (transDate.DayOfWeek != DayOfWeek.Sunday)
            {

            }
            else
            {

            }
            return DateTime.MinValue;
        }
        /// <summary>
        /// 獲取一般日結的預計匯款日
        /// </summary>
        /// <returns></returns>
        private static DateTime GetDayTime()
        {
            return DateTime.MinValue;
        }
        /// <summary>
        /// 獲取超商日結的預計匯款日
        /// </summary>
        /// <param name="channelId"></param>
        /// <returns></returns>
        private static DateTime GetMarketTime(string channelId)
        {
            switch (channelId)
            {
                case "01"://7-11
                case "02"://全家
                    return GetMarketTime_711_Family();
                case "03"://OK
                    return GetMarketTime_OK();
                case "04"://萊爾富
                    return GetMarketTime_Hilife();
                default:
                    return DateTime.MinValue;
            }
        }
        /// <summary>
        /// 獲取預計匯款日
        /// </summary>
        /// <param name="model"></param>
        /// <param name="periodModel"></param>
        /// <returns></returns>
        private static DateTime GetRemitDate(ChannelVerifyPeriodModel periodModel, ReceiptBillModel model)
        {
            switch (periodModel?.PayPeriodType)
            {
                case PayPeriodType.NDay:
                    if (model.Channel.ChannelGroupType == ChannelGroupType.Market)
                        return GetMarketTime(model.ChannelId);
                    else
                        return GetDayTime();
                case PayPeriodType.Week:
                    return GetWeekTime(DateTime.Now);
                case PayPeriodType.TenDay:
                    return GetTenDayTime(10);
                case PayPeriodType.Month:
                    return GetMonthlyTime();
            }
            return DateTime.MinValue;
        }
        /// <summary>
        /// 生成電子通路帳簿
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        private static ChannelEAccountBillSet CreateChannelEAccountBill(ReceiptBillModel model)
        {
            return new ChannelEAccountBillSet()
            {
                ChannelEAccountBill = new ChannelEAccountBillModel()
                {
                    ChannelId = model.ChannelId,
                    CollectionTypeId = model.CollectionTypeId,
                    ExpectRemitDate = model.ExpectRemitDate,
                    PostponeDays = 0,
                },
                ChannelEAccountBillDetail = new List<ChannelEAccountBillDetailModel>()
                {
                    new ChannelEAccountBillDetailModel()
                    {
                        ReceiptBillNo= model.BillNo,
                    }
                }
            };
        }
        /// <summary>
        /// 獲取對應的帳單編號
        /// </summary>
        /// <param name="set"></param>
        /// <returns></returns>
        private static string GetBillNo(ApplicationDbContext DataAccess, string bankBarCode)
        {
            string bill = DataAccess.Set<BillModel>().Where(p => p.BankBarCode == bankBarCode &&
             (p.FormStatus == FormStatus.Saved || p.FormStatus == FormStatus.Approved)).OrderByDescending(p => p.CreateTime).Select(p => p.BillNo).FirstOrDefault();
            return bill;
        }
        /// <summary>
        /// 計算 每筆總手續費之「系統商手續費」(內扣)
        /// (每筆總手續費-通路手續費) * (100-分潤%)/100。
        /// </summary>
        /// <param name="totalFee"></param>
        /// <param name="channelFee"></param>
        /// <param name="splitting"></param>
        /// <returns></returns>
        private static decimal FeeDeduct(decimal totalFee, decimal channelFee, decimal splitting)
        {
            return Math.Ceiling((totalFee - channelFee) * ((100m - splitting) / 100m));
        }
        /// <summary>
        /// 計算 每筆總手續費之「系統商手續費」(外加)
        /// 每筆總手續費 * (100-分潤%)/100。
        /// </summary>
        /// <param name="totalFee"></param>
        /// <param name="splitting"></param>
        /// <returns></returns>
        private static decimal FeePlus(decimal totalFee, decimal splitting)
        {
            return Math.Ceiling(totalFee * ((100m - splitting) / 100m));
        }
        /// <summary>
        /// 根據銷帳編號獲取商戶資料
        /// </summary>
        /// <param name="compareCode"></param>
        /// <param name="compareCodeForCheck"></param>
        /// <returns></returns>
        private static BizCustomerSet GetBizCustomerSet(Dictionary<string, BizCustomerSet> BizCustSetDic, ApplicationDbContext dataAccess, string compareCode)
        {
            BizCustomerSet bizCust = null;
            string custCode6 = compareCode.Substring(0, 6),
                   custCode4 = compareCode.Substring(0, 4),
                   custCode3 = compareCode.Substring(0, 3);
            if (null == bizCust && BizCustSetDic.ContainsKey(custCode6)) bizCust = BizCustSetDic[custCode6];
            if (null == bizCust && BizCustSetDic.ContainsKey(custCode4)) bizCust = BizCustSetDic[custCode4];
            if (null == bizCust && BizCustSetDic.ContainsKey(custCode3)) bizCust = BizCustSetDic[custCode3];
            if (null == bizCust)
            {
                using BizCustomerRepository biz = new BizCustomerRepository(dataAccess);
                bizCust = biz.QueryData(new object[] { custCode6 });
                if (null != bizCust) BizCustSetDic.Add(custCode6, bizCust);
                if (null == bizCust) bizCust = biz.QueryData(new object[] { custCode4 });
                if (null != bizCust) BizCustSetDic.Add(custCode4, bizCust);
                if (null == bizCust) bizCust = biz.QueryData(new object[] { custCode3 });
                if (null != bizCust) BizCustSetDic.Add(custCode3, bizCust);
                if (null == bizCust) return null;
            }
            return bizCust;
        }
        /// <summary>
        /// 獲取代收類別
        /// </summary>
        /// <param name="DataAccess"></param>
        /// <param name="collectionTypeId"></param>
        /// <param name="channelId"></param>
        /// <param name="amount"></param>
        /// <param name="chargePayType"></param>
        /// <param name="channelFee"></param>
        private static void GetCollectionTypeSet(ApplicationDbContext dataAccess, Dictionary<string, CollectionTypeSet> colSetDic, string collectionTypeId, string channelId, decimal amount, out ChargePayType chargePayType, out decimal channelFee)
        {
            CollectionTypeSet colSet = null;
            channelFee = 0;
            chargePayType = ChargePayType.Deduction;
            if (!colSetDic.ContainsKey(collectionTypeId))
            {
                using CollectionTypeRepository colRepo = new CollectionTypeRepository(dataAccess);
                colSet = colRepo.QueryData(new object[] { collectionTypeId });
                colSetDic.Add(collectionTypeId, colSet);
            }
            colSet = colSetDic[collectionTypeId];
            if (null == colSet) return;
            chargePayType = colSet.CollectionType.ChargePayType;
            CollectionTypeDetailModel c = colSet.CollectionTypeDetail.FirstOrDefault(p => p.CollectionTypeId == collectionTypeId && p.ChannelId == channelId && p.SRange <= amount && p.ERange >= amount);
            if (null != c) channelFee = c.Fee;
        }
        /// <summary>
        /// 獲取預計匯款
        /// </summary>
        /// <param name="collectionTypeId"></param>
        /// <param name="channelId"></param>
        private static ChannelVerifyPeriodModel GetChannelVerifyPeriod(ApplicationDbContext dataAccess, Dictionary<string, ChannelVerifyPeriodModel> periodDic, string collectionTypeId, string channelId)
        {
            string pk = $"{collectionTypeId},{channelId}";
            ChannelVerifyPeriodModel periodModel = null;
            if (!periodDic.ContainsKey(pk))
            {
                periodModel = dataAccess.Set<ChannelVerifyPeriodModel>().FirstOrDefault(p => p.ChannelId == channelId && p.CollectionTypeId == collectionTypeId);
                periodDic.Add(pk, periodModel);
            }
            periodModel = periodDic[pk];
            return periodModel;
        }
        /// <summary>
        /// 插入帳單收款明細
        /// </summary>
        /// <param name="receiptBillNo"></param>
        /// <param name="billNo"></param>
        private static void InsertBillReceiptDetail(ApplicationDbContext dataAccess, ReceiptBillModel receipt, string billNo)
        {
            if (billNo.IsNullOrEmpty()) return;
            //using BillRepository rep = new BillRepository(DataAccess) { User = User };
            //BillSet billSet = rep.QueryData(new object[] { billNo });
            //if (null == billSet) { /*add Message:查無帳單*/ return; }
            //billSet.BillReceiptDetail.Add(new BillReceiptDetailModel() { BillNo = billNo, ReceiptBill = receipt, ReceiptBillNo = receipt.BillNo, RowState = RowState.Insert });
            //rep.Update(billSet);
            BillModel bill = dataAccess.Find<BillModel>(billNo);
            BillReceiptDetailModel dt = new BillReceiptDetailModel() { BillNo = billNo, ReceiptBill = receipt, ReceiptBillNo = receipt.BillNo, RowState = RowState.Insert };
            dataAccess.Add(dt);
            bill.HadPayAmount += receipt.PayAmount;
            bill.PayStatus = BizBill.GetPayStatus(bill.PayAmount, bill.HadPayAmount);
            dataAccess.Update(bill);
        }
        /// <summary>
        /// 插入通路電子帳簿
        /// </summary>
        private static void InsertChannelEAccount(ApplicationDbContext dataAccess, IUserModel user, ReceiptBillSet set)
        {
            if (set.ReceiptBill.ExpectRemitDate == DateTime.MinValue) return;
            using ChannelEAccountBillRepository repo = new ChannelEAccountBillRepository(dataAccess) { User = user };
            var channelEAccount = dataAccess.Set<ChannelEAccountBillModel>().FirstOrDefault(p => p.CollectionTypeId == set.ReceiptBill.CollectionTypeId && p.ExpectRemitDate == set.ReceiptBill.ExpectRemitDate);
            if (null == channelEAccount)
            {
                ChannelEAccountBillSet accountSet = CreateChannelEAccountBill(set.ReceiptBill);
                repo.Create(accountSet);
            }
            else
            {
                ChannelEAccountBillSet accountSet = repo.QueryData(new object[] { channelEAccount.BillNo });
                if (dataAccess.Set<ChannelEAccountBillDetailModel>().Where(p => p.ReceiptBillNo == set.ReceiptBill.BillNo).Count() == 0)
                    accountSet.ChannelEAccountBillDetail.Add(new ChannelEAccountBillDetailModel() { BillNo = accountSet.ChannelEAccountBill.BillNo, ReceiptBillNo = set.ReceiptBill.BillNo, RowState = RowState.Insert });
                repo.Update(accountSet);
            }
        }
        /// <summary>
        /// 
        /// Ex:T+3為例 
        /// -------------------------
        /// |資料傳輸日|入帳，撥款日|
        /// -------------------------
        /// |星期一　　|星期四　　　|
        /// -------------------------
        /// |星期二　　|(本週)星期五|
        /// -------------------------
        /// |星期三　　|(本週)星期五|
        /// -------------------------
        /// |星期四　　|(本週)星期五|
        /// -------------------------
        /// |星期五　　|星期一　　　|
        /// -------------------------
        /// |星期六　　|星期二　　　|
        /// -------------------------
        /// |星期日　　|星期三　　　|
        /// -------------------------
        /// </summary>
        /// <returns></returns>
        private static DateTime GetMarketTime_711_Family()
        {
            return DateTime.Now;
        }
        /// <summary>
        /// 
        /// Ex:T+3為例
        /// -------------------------
        /// |資料傳輸日|入帳，撥款日|
        /// -------------------------
        /// |星期一　　|星期四　　　|
        /// -------------------------
        /// |星期二　　|星期五　　　|
        /// -------------------------
        /// |星期三　　|(下週)星期一|
        /// -------------------------
        /// |星期四　　|(下週)星期一|
        /// -------------------------
        /// |星期五　　|(下週)星期一|
        /// -------------------------
        /// |星期六　　|星期二　　　|
        /// -------------------------
        /// |星期日　　|星期三　　　|
        /// -------------------------
        /// </summary>
        /// <returns></returns>
        private static DateTime GetMarketTime_OK()
        {
            return DateTime.Now;
        }
        /// <summary>
        /// 
        /// Ex:T+3為例 
        /// -------------------------
        /// |資料傳輸日|入帳，撥款日|
        /// -------------------------
        /// |星期一　　|星期四　　　|
        /// -------------------------
        /// |星期二　　|星期五　　　|
        /// -------------------------
        /// |星期三　　|星期一　　　|
        /// -------------------------
        /// |星期四　　|星期二　　　|
        /// -------------------------
        /// |星期五　　|星期三　　　|
        /// -------------------------
        /// |星期六　　|(下週)星期一|
        /// -------------------------
        /// |星期日　　|(下週)星期一|
        /// -------------------------
        /// </summary>
        /// <returns></returns>
        private static DateTime GetMarketTime_Hilife()
        {
            return DateTime.Now;
        }
        #endregion
    }
}
