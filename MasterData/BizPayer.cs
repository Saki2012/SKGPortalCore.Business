using SKGPortalCore.Model.MasterData;

namespace SKGPortalCore.Business.MasterData
{
    public class BizPayer : BizBase
    {
        #region Construct
        public BizPayer() : base() { }
        #endregion

        #region Public
        /// <summary>
        /// 保存前時機
        /// </summary>
        /// <param name="payerSet"></param>
        public void BeforeUpdate(PayerSet payerSet) { }
        /// <summary>
        /// 保存後時機
        /// </summary>
        /// <param name="payerSet"></param>
        public void AfterUpdate(PayerSet payerSet) { }
        /// <summary>
        /// 匯入繳款人(Excel匯入)
        /// </summary>
        public void ImportExcelPayer() { }
        #endregion

        #region Private

        #endregion

    }
}
