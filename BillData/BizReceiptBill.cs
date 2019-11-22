using System;
using System.Collections.Generic;
using System.Linq;
using SKGPortalCore.Data;
using SKGPortalCore.Lib;
using SKGPortalCore.Model;
using SKGPortalCore.Model.BillData;
using SKGPortalCore.Model.MasterData;

namespace SKGPortalCore.Repository.SKGPortalCore.Business.BillData
{
    /// <summary>
    /// 收款單-商業邏輯
    /// </summary>
     internal static class BizReceiptBill
    {
        #region Public
        /// <summary>
        /// 設置欄位值
        /// </summary>
        /// <param name="set"></param>
        /// <param name="action"></param>
        public static void SetData(ApplicationDbContext DataAccess, ReceiptBillSet set, ChannelVerifyPeriodModel periodModel, FuncAction action)
        {
            set.ReceiptBill.ToBillNo = GetBillNo(DataAccess, set.ReceiptBill.CompareCodeForCheck);
            if (action == FuncAction.Create)//未來若有修改RemitDate的情況，需進行差異調整
                set.ReceiptBill.RemitDate = GetRemitDate(periodModel, set.ReceiptBill);
        }
        /// <summary>
        /// 設置欄位值
        /// </summary>
        /// <param name="set"></param>
        /// <param name="compareCodeForCheck"></param>
        /// <param name="chargePayType"></param>
        /// <param name="channelFee"></param>
        public static void SetData(ReceiptBillSet set, List<BizCustomerFeeDetailModel> bizCustFee, string compareCodeForCheck, ChargePayType chargePayType, decimal channelFee)
        {
            GetBizCustFee(bizCustFee, channelFee, chargePayType, CanalisType.Bank, out decimal bankFee, out decimal thirdFee, out decimal hiTrustFee);
            set.ReceiptBill.BankFee = bankFee;
            set.ReceiptBill.ThirdFee = thirdFee;
            set.ReceiptBill.HiTrustFee = hiTrustFee;

            set.ReceiptBill.ChannelFee = channelFee;
            set.ReceiptBill.CompareCodeForCheck = compareCodeForCheck;
            set.ReceiptBill.ChargePayType = chargePayType;
        }
        /// <summary>
        /// 獲取預計匯款日
        /// </summary>
        /// <param name="model"></param>
        /// <param name="periodModel"></param>
        /// <returns></returns>
        public static DateTime GetRemitDate(ChannelVerifyPeriodModel periodModel, ReceiptBillModel model)
        {
            switch (periodModel?.PayPeriodType)
            {
                case PayPeriodType.NDay:
                    if (model.Channel.ChannelType == CanalisType.Market)
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
        public static ChannelEAccountBillSet CreateChannelEAccountBill(ReceiptBillModel model)
        {
            return new ChannelEAccountBillSet()
            {
                ChannelEAccountBill = new ChannelEAccountBillModel()
                {
                    ChannelId = model.ChannelId,
                    CollectionTypeId = model.CollectionTypeId,
                    ExpectRemitDate = model.RemitDate,
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
        public static string GetBillNo(ApplicationDbContext DataAccess, string compareCodeForCheck)
        {
            string bill = DataAccess.Set<BillModel>().Where(p => p.CompareCodeForCheck == compareCodeForCheck &&
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
        public static decimal FeeDeduct(decimal totalFee, decimal channelFee, decimal splitting)
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
        public static decimal FeePlus(decimal totalFee, decimal splitting)
        {
            return Math.Ceiling(totalFee * ((100m - splitting) / 100m));
        }
        #endregion

        #region Private
        private static void GetBizCustFee(List<BizCustomerFeeDetailModel> detail, decimal channelFee, ChargePayType chargePayType, CanalisType canalisType, out decimal bankFee, out decimal thirdFee, out decimal hiTrustFee)
        {
            bankFee = 0; thirdFee = 0; hiTrustFee = 0;
            if (!detail.HasData()) return;
            BizCustomerFeeDetailModel model = detail.FirstOrDefault(p => p.ChannelType == canalisType && p.FeeType == FeeType.HitrustFee);
            hiTrustFee = null == model ? 0 : model.Fee;
            model = detail.FirstOrDefault(p => p.ChannelType == canalisType && (p.FeeType == FeeType.ClearFee || p.FeeType == FeeType.TotalFee));
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
