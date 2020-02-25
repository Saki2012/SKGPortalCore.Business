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
            if (CheckPayerNoLen(set.Payer)) { Message.AddCustErrorMessage(MessageCode.Code1005, ResxManage.GetDescription(set.Payer.PayerNo), set.Payer.BizCustomer.PayerNoLen); }
            if (CheckPayerNoIsNotNum(set.Payer.PayerNo)) { Message.AddCustErrorMessage(MessageCode.Code1006, ResxManage.GetDescription(set.Payer.PayerNo)); }
        }
        #endregion

        #region Private
        /// <summary>
        /// 檢查「繳款人編號」長度
        /// </summary>
        /// <param name="payer"></param>
        private static bool CheckPayerNoLen(PayerModel payer)
        {
            return payer.PayerNo.Length != payer.BizCustomer.PayerNoLen;
        }
        /// <summary>
        /// 檢查「繳款人編號」是否皆為數字
        /// </summary>
        /// <param name="payerNo"></param>
        /// <returns></returns>
        private static bool CheckPayerNoIsNotNum(string payerNo)
        {
            return !payerNo.IsNumberString();
        }
        #endregion
    }
}
