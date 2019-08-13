using SKGPortalCore.Model.BillData;

namespace SKGPortalCore.Business.BillData
{
    public class BizChannelEAccountBill : BizBase
    {
        #region Construct
        public BizChannelEAccountBill() : base() { }
        #endregion

        #region Public
        /// <summary>
        /// 保存前時機
        /// </summary>
        /// <param name="set"></param>
        public void BeforeUpdate(ChannelEAccountBillSet set)
        {
            set.ChannelEAccountBill.ExpectRemitAmount = 0m;
            foreach (var detail in set.ChannelEAccountBillDetail)
            {
                set.ChannelEAccountBill.ExpectRemitAmount += detail.ReceiptBill.PayAmount;
            }
        }
        /// <summary>
        /// 保存後時機
        /// </summary>
        /// <param name="set"></param>
        public void AfterUpdate(ChannelEAccountBillSet set) { }
        #endregion

        #region Private
        #endregion
    }
}
