using System;
using System.Collections.Generic;
using System.Linq;
using SKGPortalCore.Data;
using SKGPortalCore.Lib;
using SKGPortalCore.Model.BillData;
using SKGPortalCore.Model.Enum;
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
        public static void CheckData(ReceiptBillSet set, SysMessageLog message)
        {

        }
        /// <summary>
        /// 設置欄位值
        /// </summary>
        /// <param name="set"></param>
        /// <param name="action"></param>
        public static void SetData(ReceiptBillSet set, ApplicationDbContext dataAccess, Dictionary<string, BizCustomerSet> bizCustSetDic, Dictionary<string, CollectionTypeSet> colSetDic, Dictionary<DateTime, bool> workDic)
        {
            BizCustomerSet bizCustomerSet = GetBizCustomerSet(dataAccess, bizCustSetDic, set.ReceiptBill.BankBarCode);
            CollectionTypeSet collectionTypeSet = GetCollectionTypeSet(dataAccess, colSetDic, set.ReceiptBill.CollectionTypeId);
            SetBizCustomer(set.ReceiptBill, bizCustomerSet.BizCustomer);
            SetBillNo(dataAccess, set.ReceiptBill);
            SetFee(set.ReceiptBill, bizCustomerSet, collectionTypeSet);
            SetRemitDate(set.ReceiptBill, collectionTypeSet, workDic);
            SetErrMessage(set.ReceiptBill);
        }
        /// <summary>
        /// 過帳資料
        /// </summary>
        public static void PostingData(ApplicationDbContext dataAccess, IUserModel user, FuncAction action, ReceiptBillSet oldData, ReceiptBillSet newData)
        {
            PostingBillReceiptDetail(dataAccess, newData.ReceiptBill, newData.ReceiptBill.ToBillNo);
            PostingChannelEAccount(dataAccess, user, newData);
        }
        #endregion

        #region Private
        /// <summary>
        /// 根據銷帳編號獲取商戶資料
        /// </summary>
        /// <param name="compareCode"></param>
        /// <param name="compareCodeForCheck"></param>
        /// <returns></returns>
        private static BizCustomerSet GetBizCustomerSet(ApplicationDbContext dataAccess, Dictionary<string, BizCustomerSet> BizCustSetDic, string compareCode)
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
        private static CollectionTypeSet GetCollectionTypeSet(ApplicationDbContext dataAccess, Dictionary<string, CollectionTypeSet> colSetDic, string collectionTypeId)
        {
            CollectionTypeSet colSet;
            if (!colSetDic.ContainsKey(collectionTypeId))
            {
                using CollectionTypeRepository colRepo = new CollectionTypeRepository(dataAccess);
                colSetDic.Add(collectionTypeId, colRepo.QueryData(new object[] { collectionTypeId }));
            }
            colSet = colSetDic[collectionTypeId];
            return colSet;

        }
        /// <summary>
        /// 設置企業編號
        /// </summary>
        /// <param name="receiptBill"></param>
        /// <param name="bizCustomerSet"></param>
        private static void SetBizCustomer(ReceiptBillModel receiptBill, BizCustomerModel bizCustomer)
        {
            receiptBill.Customer = bizCustomer;
            receiptBill.CustomerCode = bizCustomer?.CustomerCode;
        }
        /// <summary>
        /// 設置對應的帳單編號
        /// </summary>
        /// <param name="set"></param>
        /// <returns></returns>
        private static void SetBillNo(ApplicationDbContext DataAccess, ReceiptBillModel receiptBill)
        {
            receiptBill.ToBillNo = DataAccess.Set<BillModel>().Where(p => p.VirtualAccountCode == receiptBill.BankBarCode && (p.FormStatus == FormStatus.Saved || p.FormStatus == FormStatus.Approved)).OrderByDescending(p => p.CreateTime).Select(p => p.BillNo).FirstOrDefault();
        }
        /// <summary>
        /// 設置費用
        /// </summary>
        private static void SetFee(ReceiptBillModel receiptBill, BizCustomerSet bizCustomerSet, CollectionTypeSet collectionTypeSet)
        {
            CollectionTypeDetailModel collectionTypeDetailModel = collectionTypeSet.CollectionTypeDetail.FirstOrDefault(p => p.ChannelId == receiptBill.ChannelId && (p.SRange <= receiptBill.PayAmount && p.ERange >= receiptBill.PayAmount));
            BizCustomerFeeDetailModel bizCustomerFeeDetailModel = bizCustomerSet.BizCustomerFeeDetail.FirstOrDefault(p => p.ChannelType == receiptBill.Channel.ChannelGroupType);
            BizCustomerFeeDetailModel hiTrust = bizCustomerSet.BizCustomerFeeDetail.FirstOrDefault(p => p.ChannelType == ChannelGroupType.Hitrust);
            receiptBill.ChargePayType = collectionTypeSet.CollectionType.ChargePayType;
            receiptBill.FeeType = bizCustomerFeeDetailModel.BankFeeType;
            if (receiptBill.FeeType == BankFeeType.TotalFee)
            {
                switch (receiptBill.CollectionType.ChargePayType)
                {
                    case ChargePayType.Deduction:
                        {
                            receiptBill.ThirdFee = FeeDeduct(bizCustomerFeeDetailModel.Fee, collectionTypeDetailModel.ChannelTotalFee, bizCustomerFeeDetailModel.Percent);
                            receiptBill.BankFee = bizCustomerFeeDetailModel.Fee - receiptBill.ThirdFee;
                        }
                        break;
                    case ChargePayType.Increase:
                        {
                            receiptBill.ThirdFee = FeePlus(bizCustomerFeeDetailModel.Fee, bizCustomerFeeDetailModel.Percent);
                            receiptBill.BankFee = bizCustomerFeeDetailModel.Fee - receiptBill.ThirdFee;
                        }
                        break;
                }
            }
            else
            {
                receiptBill.BankFee = bizCustomerFeeDetailModel.Fee;
                receiptBill.ThirdFee = 0m;
            }
            receiptBill.ThirdFee = null != hiTrust ? hiTrust.Fee : receiptBill.ThirdFee;
            receiptBill.ChannelFeedBackFee = collectionTypeDetailModel.ChannelFeedBackFee;
            receiptBill.ChannelRebateFee = collectionTypeDetailModel.ChannelRebateFee;
            receiptBill.ChannelFee = collectionTypeDetailModel.ChannelFee;
            receiptBill.ChannelTotalFee = collectionTypeDetailModel.ChannelTotalFee;
            receiptBill.TotalFee = receiptBill.FeeType == BankFeeType.TotalFee ? bizCustomerFeeDetailModel.Fee : collectionTypeDetailModel.ChannelTotalFee;
        }
        /// <summary>
        /// 計算 每筆總手續費之「系統商手續費」(內扣)
        /// (每筆總手續費-通路總手續費) * (100-分潤%)/100。
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
        /// 設置預計匯款日
        /// </summary>
        /// <param name="model"></param>
        /// <param name="periodModel"></param>
        /// <returns></returns>
        private static void SetRemitDate(ReceiptBillModel receiptBill, CollectionTypeSet collectionTypeSet, Dictionary<DateTime, bool> workDic)
        {
            DateTime expectRemitDate = DateTime.MinValue;
            if (!receiptBill.Channel.ChannelGroupType.In(ChannelGroupType.Bank, ChannelGroupType.Self))
            {
                CollectionTypeVerifyPeriodModel period = collectionTypeSet.CollectionTypeVerifyPeriod.Where(p => p.ChannelId == receiptBill.ChannelId).FirstOrDefault();

                switch (period?.PayPeriodType)
                {
                    case PayPeriodType.NDay_A: expectRemitDate = GetNDay_A(workDic, receiptBill.TransDate); break;
                    case PayPeriodType.NDay_B: expectRemitDate = GetNDay_B(workDic, receiptBill.TransDate); break;
                    case PayPeriodType.NDay_C: expectRemitDate = GetNDay_C(workDic, receiptBill.TransDate); break;
                    case PayPeriodType.Week:   expectRemitDate = GetWeekTime(workDic, receiptBill.TransDate); break;
                    case PayPeriodType.TenDay: expectRemitDate = GetTenDayTime(workDic, receiptBill.TransDate); break;
                }
            }
            receiptBill.ExpectRemitDate = expectRemitDate;
        }
        /// <summary>
        /// 7-11、全家、郵局、農金、亞太
        /// 入帳日：入帳日為非營業日時，會與前一個傳輸日一同匯款
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
        private static DateTime GetNDay_A(Dictionary<DateTime, bool> workDic, DateTime transDate)
        {
            DateTime result = transDate.AddDays(3);
            if (!workDic[result]) result = LibData.GetWorkDate(workDic, result, -1);
            return result;
        }
        /// <summary>
        /// OK
        /// 入帳日：入帳日為非營業日時，往後遞延下一個營業日
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
        private static DateTime GetNDay_B(Dictionary<DateTime, bool> workDic, DateTime transDate)
        {
            return LibData.GetWorkDate(workDic, transDate.AddDays(3), 0);
        }
        /// <summary>
        /// 萊爾富
        /// 入帳日：
        /// 1. 傳輸日為非營業日時，金流會併在後一個營業日的入帳日一起匯款
        /// 2. 入帳日為非營業日時，往後遞延下一個營業日
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
        /// |星期六　　|(下週)星期四|
        /// -------------------------
        /// |星期日　　|(下週)星期四|
        /// -------------------------
        /// </summary>
        /// <returns></returns>
        private static DateTime GetNDay_C(Dictionary<DateTime, bool> workDic, DateTime transDate)
        {
            return !workDic[transDate] ?
                LibData.GetWorkDate(workDic, LibData.GetWorkDate(workDic, transDate, 0).AddDays(3), 0) :
                LibData.GetWorkDate(workDic, LibData.GetWorkDate(workDic, transDate.AddDays(3), 0), 0);
        }
        /// <summary>
        /// 獲取週結的預計匯款日
        /// </summary>
        /// <returns></returns>
        private static DateTime GetWeekTime(Dictionary<DateTime, bool> workDic, DateTime transDate)
        {
            return LibData.GetWorkDate(workDic, transDate.AddDays((transDate.DayOfWeek != DayOfWeek.Sunday ? 7 : 0) + DayOfWeek.Wednesday - transDate.DayOfWeek), 0); ;
        }
        /// <summary>
        /// 獲取旬結的預計匯款日
        /// </summary>
        /// <param name="i"></param>
        /// <returns></returns>
        private static DateTime GetTenDayTime(Dictionary<DateTime, bool> workDic, DateTime transDate)
        {
            if (transDate.Day < 11)
                return LibData.GetWorkDate(workDic, new DateTime(transDate.Year, transDate.Month, 15), 0);
            else if (transDate.Day < 21)
                return LibData.GetWorkDate(workDic, new DateTime(transDate.Year, transDate.Month, 25), 0);
            else
                return LibData.GetWorkDate(workDic, new DateTime(transDate.AddMonths(1).Year, transDate.AddMonths(1).Month, 5), 0);
        }
        /// <summary>
        /// 檢查，並設置異常訊息
        /// </summary>
        /// <param name="receiptBill"></param>
        private static void SetErrMessage(ReceiptBillModel receiptBill)
        {
            receiptBill.ErrMessage = "";
            receiptBill.IsErrData = !receiptBill.ErrMessage.IsNullOrEmpty();
        }
        /// <summary>
        /// 過帳帳單收款明細
        /// </summary>
        /// <param name="receiptBillNo"></param>
        /// <param name="billNo"></param>
        private static void PostingBillReceiptDetail(ApplicationDbContext dataAccess, ReceiptBillModel receipt, string billNo)
        {
            if (billNo.IsNullOrEmpty()) return;
            BillModel bill = dataAccess.Find<BillModel>(billNo);
            BillReceiptDetailModel dt = new BillReceiptDetailModel() { BillNo = billNo, ReceiptBill = receipt, ReceiptBillNo = receipt.BillNo, RowState = RowState.Insert };
            dataAccess.Add(dt);
            bill.HadPayAmount += receipt.PayAmount;
            bill.PayStatus = BizBill.GetPayStatus(bill.PayAmount, bill.HadPayAmount);
            dataAccess.Update(bill);
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
        /// 過帳通路電子帳簿
        /// </summary>
        private static void PostingChannelEAccount(ApplicationDbContext dataAccess, IUserModel user, ReceiptBillSet set)
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
        #endregion
    }
}
