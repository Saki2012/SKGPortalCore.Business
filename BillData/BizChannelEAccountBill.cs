using SKGPortalCore.Data;
using SKGPortalCore.Model.BillData;
using SKGPortalCore.Model.System;

namespace SKGPortalCore.Repository.SKGPortalCore.Business.BillData
{
    internal static class BizChannelEAccountBill
    {
        #region Public
        /// <summary>
        /// 設置欄位
        /// </summary>
        /// <param name="set"></param>
        /// <param name="action"></param>
        public static void SetData(ChannelEAccountBillSet set)
        {
            int count = 0;
            decimal payAmount = 0, channelFee = 0;
            foreach (ChannelEAccountBillDetailModel s in set.ChannelEAccountBillDetail)
            {
                payAmount += s.ReceiptBill.PayAmount;
                channelFee += s.ReceiptBill.ChargePayType == ChargePayType.Deduction ? s.ReceiptBill.ChannelFee : 0m;
                count++;
            }
            set.ChannelEAccountBill.Amount = payAmount;
            set.ChannelEAccountBill.ExpectRemitAmount = payAmount - channelFee;
            set.ChannelEAccountBill.PayCount = count;
        }
        /// <summary>
        /// 過帳資料
        /// </summary>
        public static void PostingData(ChannelEAccountBillSet set, ApplicationDbContext dataAccess)
        {
            PostingReceiptBill(set, dataAccess);
        }
        #endregion

        #region Private
        /// <summary>
        /// 過帳收款單
        /// </summary>
        private static void PostingReceiptBill(ChannelEAccountBillSet set, ApplicationDbContext dataAccess)
        {
            foreach (var c in set.ChannelEAccountBillDetail)
            {
                c.ReceiptBill.ChannelEAccountBillNo = set.ChannelEAccountBill.BillNo;
                dataAccess.Update(c.ReceiptBill);
            }
        }
        #endregion
    }
}
