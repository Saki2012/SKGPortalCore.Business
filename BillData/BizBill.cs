using SKGPortalCore.Data;
using SKGPortalCore.Lib;
using SKGPortalCore.Model.BillData;
using SKGPortalCore.Model.MasterData;
using SKGPortalCore.Model.Report;
using SKGPortalCore.Model.System;
using SKGPortalCore.Repository.SKGPortalCore.Business.Func;
using System;
using System.Collections.Generic;
using System.Linq;

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
        public static void CheckData(BillSet set, SysMessageLog message, ApplicationDbContext dataAccess)
        {
            if (BizVirtualAccountCode.CheckBankCodeExist(dataAccess, set.Bill.VirtualAccountCode, out _))
            { message.AddCustErrorMessage(MessageCode.Code1008, ResxManage.GetDescription<BillModel>(p => p.VirtualAccountCode), set.Bill.VirtualAccountCode); }
            CalcTotalPayAmount(set);
            CheckPayEndDate(message, set.Bill);
            CheckCollectionTypeId(message, dataAccess, set.Bill);
        }
        /// <summary>
        /// 設置資料
        /// </summary>
        /// <param name="set"></param>
        /// <param name="action"></param>
        public static void SetData(BillSet set, string progId, ApplicationDbContext dataAccess)
        {
            //SetBillDetail(set.Bill, set.BillDetail);
            SetBillReceiptDetail(set.Bill, set.BillReceiptDetail);
            SetBankCode(set.Bill);
            BizVirtualAccountCode.AddVirtualAccountCode(dataAccess, progId, set.Bill.BillNo, set.Bill.VirtualAccountCode);
            ResetPayEndDateAndCollectionType(set.Bill);
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
            if (hasPayAmount == decimal.Zero) return PayStatus.Unpaid;
            else
            {
                if (payAmount > hasPayAmount) return PayStatus.UnderPaid;
                else if (payAmount == hasPayAmount) return PayStatus.PaidComplete;
                else return PayStatus.OverPaid;
            }
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
        private static void CheckPayEndDate(SysMessageLog message, BillModel bill)
        {
            if ((bill.BizCustomer.MarketEnable || bill.BizCustomer.PostEnable) && (bill.PayEndDate == null || bill.PayEndDate.Equals(DateTime.MinValue)))
                message.AddCustErrorMessage(MessageCode.Code0001, ResxManage.GetDescription<BillModel>(p => p.PayEndDate));
        }
        /// <summary>
        /// 檢查超商代收項目
        /// </summary>
        /// <param name="payEndDate"></param>
        /// <param name="customer"></param>
        /// <returns></returns>
        private static void CheckCollectionTypeId(SysMessageLog message, ApplicationDbContext dataAccess, BillModel bill)
        {
            if (bill.BizCustomer.MarketEnable)
            {
                if (bill.CollectionTypeId.IsNullOrEmpty())
                    message.AddCustErrorMessage(MessageCode.Code0001, ResxManage.GetDescription<BillModel>(p => p.CollectionTypeId));
                else
                {
                    if (!bill.BizCustomer.CollectionTypeIds.Split(',').Contains(bill.CollectionTypeId))
                        message.AddCustErrorMessage(MessageCode.Code1015, bill.CollectionTypeId);
                    else
                    {
                        CollectionTypeDetailModel colDet = dataAccess.Set<CollectionTypeDetailModel>().FirstOrDefault(p => p.CollectionTypeId.Equals(bill.CollectionTypeId) && p.SRange <= bill.PayAmount && p.ERange >= bill.PayAmount);
                        if (null == colDet)
                            message.AddCustErrorMessage(MessageCode.Code1016, bill.CollectionTypeId);
                    }
                }
            }
        }
        #endregion

        #region SetData
        /// <summary>
        /// 獲取銀行銷帳編號
        /// </summary>
        /// <param name="bill"></param>
        /// <returns></returns>
        private static void SetBankCode(BillModel bill)
        {
            string vir1 = string.Empty, vir2 = string.Empty, vir3 = string.Empty;
            if (null == bill.BizCustomer)
            {
                bill.VirtualAccountCode = string.Empty;
                return;
            }
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
            bill.VirtualAccountCode = $"{result}{BizVirtualAccountCode.GetVirtualCheckCode(bill.BizCustomer.VirtualAccount3, result, bill.PayAmount)}".PadLeft(16, '0');
        }
        /// <summary>
        /// 若未啟用超商/郵局通路時，重置繳費截止日
        ///   未啟用超商時，重置代收類別
        /// </summary>
        /// <param name="bill"></param>
        private static void ResetPayEndDateAndCollectionType(BillModel bill)
        {
            if (!(bill.BizCustomer.MarketEnable || bill.BizCustomer.PostEnable)) bill.PayEndDate = DateTime.MinValue;
            if (!bill.BizCustomer.MarketEnable) bill.CollectionTypeId = null;
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="DataAccess"></param>
        /// <param name="Bill"></param>
        /// <param name="BillDetail"></param>
        //private static void SetBillDetail( List<BillDetailModel> BillDetail)
        //{
        //    BillDetail?.ForEach(row =>
        //    {
        //    });
        //}
        /// <summary>
        /// 
        /// </summary>
        /// <param name="DataAccess"></param>
        /// <param name="Bill"></param>
        /// <param name="BillReceiptDetail"></param>
        private static void SetBillReceiptDetail(BillModel Bill, List<BillReceiptDetailModel> BillReceiptDetail)
        {
            Bill.HadPayAmount = 0m;
            BillReceiptDetail?.ForEach(row =>
            {
                Bill.HadPayAmount += row.ReceiptBill.PayAmount;
            });
            Bill.PayStatus = GetPayStatus(Bill.PayAmount, Bill.HadPayAmount);
        }
        /// <summary>
        /// 彙總應繳金額
        /// </summary>
        /// <param name="set"></param>
        private static void CalcTotalPayAmount(BillSet set)
        {
            set.Bill.PayAmount = set.BillDetail.Sum(row => row.PayAmount);
        }
        #endregion

        #endregion
    }
}
