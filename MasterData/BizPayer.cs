using SKGPortalCore.Data;
using SKGPortalCore.Lib;
using SKGPortalCore.Model.MasterData;

namespace SKGPortalCore.Business.MasterData
{
    public class BizPayer : BizBase
    {
        #region Construct
        public BizPayer(MessageLog message) : base(message) { }
        #endregion

        #region Public
        //保存前
        public void CheckData(PayerSet set)
        {
            CheckPayerNo(set.Payer);
        }
        #endregion

        #region Private
        /// <summary>
        /// 檢查「繳款人編號」
        /// </summary>
        /// <param name="payer"></param>
        private void CheckPayerNo(PayerModel payer)
        {
            if (payer.PayerNo.Length != payer.Customer.PayerNoLen)
            {
                Message.AddErrorMessage(MessageCode.Code1005, ResxManage.GetDescription(payer.PayerNo), payer.Customer.PayerNoLen);
            }

            if (payer.PayerNo.IsNumberString())
            {
                Message.AddErrorMessage(MessageCode.Code1006, ResxManage.GetDescription(payer.PayerNo));
            }
        }
        #endregion
    }
}
