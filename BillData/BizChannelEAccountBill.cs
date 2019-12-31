using SKGPortalCore.Model.BillData;

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
                channelFee += s.ReceiptBill.ChargePayType == Model.ChargePayType.Deduction ? s.ReceiptBill.ChannelFee : 0m;
                count++;
            }
            set.ChannelEAccountBill.Amount = payAmount;
            set.ChannelEAccountBill.ExpectRemitAmount = payAmount - channelFee;
            set.ChannelEAccountBill.PayCount = count;
        }
        #endregion

        #region Private

        #endregion
    }
}
