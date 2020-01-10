using SKGPortalCore.Data;
using SKGPortalCore.Lib;
using SKGPortalCore.Model.MasterData;

namespace SKGPortalCore.Repository.SKGPortalCore.Business.MasterData
{
    internal static class BizPayer
    {
        #region Public
        //保存前
        public static void CheckData(SysMessageLog Message, PayerSet set)
        {
            CheckPayerNo(Message, set.Payer);
        }
        #endregion

        #region Private
        /// <summary>
        /// 檢查「繳款人編號」
        /// </summary>
        /// <param name="payer"></param>
        private static void CheckPayerNo(SysMessageLog Message, PayerModel payer)
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
