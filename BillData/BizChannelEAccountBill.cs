using SKGPortalCore.Data;
using SKGPortalCore.Model.BillData;

namespace SKGPortalCore.Business.BillData
{
    public class BizChannelEAccountBill : BizBase
    {
        #region Construct
        public BizChannelEAccountBill(MessageLog message) : base(message) { }
        #endregion

        #region Public
        /// <summary>
        /// 設置欄位
        /// </summary>
        /// <param name="set"></param>
        /// <param name="action"></param>
        public void SetData(ChannelEAccountBillSet set)
        {
            int count = 0;
            decimal payAmount = 0, fee = 0;
            foreach (var s in set.ChannelEAccountBillDetail)
            {
                payAmount += s.ReceiptBill.PayAmount;
                fee += s.ReceiptBill.ChannelFee;
                count++;
            }
            set.ChannelEAccountBill.Amount = payAmount;
            set.ChannelEAccountBill.ExpectRemitAmount = payAmount - fee;
            set.ChannelEAccountBill.PayCount = count;
        }
        #endregion

        #region Private
        
        #endregion
    }
}
