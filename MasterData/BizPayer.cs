using SKGPortalCore.Data;
using SKGPortalCore.Lib;
using SKGPortalCore.Model.MasterData;
using System.Linq;
using System;
using System.ComponentModel;
using SKGPortalCore.Model.System;

namespace SKGPortalCore.Repository.SKGPortalCore.Business.MasterData
{
    internal static class BizPayer
    {
        #region Public
        //保存前
        public static void CheckData(SysMessageLog message, ApplicationDbContext dataAccess, PayerSet set)
        {
            CheckPayerNoLen(message, set.Payer);
            CheckPayerNoIsNotNum(message, set.Payer.PayerNo);
            CheckTermNoExist(message, dataAccess, set.Payer);
        }
        #endregion

        #region Private
        #region CheckData
        /// <summary>
        /// 檢查「繳款人編號」長度
        /// </summary>
        /// <param name="payer"></param>
        private static void CheckPayerNoLen(SysMessageLog message, PayerModel payer)
        {
            if (payer.PayerNo.Length != payer.BizCustomer.PayerNoLen)
                message.AddCustErrorMessage(MessageCode.Code1005, ResxManage.GetDescription<PayerModel>(p => p.PayerNo), payer.BizCustomer.PayerNoLen);
        }
        /// <summary>
        /// 檢查「繳款人編號」是否皆為數字
        /// </summary>
        /// <param name="payerNo"></param>
        /// <returns></returns>
        private static void CheckPayerNoIsNotNum(SysMessageLog message, string payerNo)
        {
            if (!payerNo.IsNumberString())
                message.AddCustErrorMessage(MessageCode.Code1006, ResxManage.GetDescription<PayerModel>(p => p.PayerNo));
        }

        /// <summary>
        /// 檢查期別編號是否重複
        /// </summary>
        /// <param name="dataAccess"></param>
        /// <param name="payer"></param>
        /// <returns></returns>
        private static void CheckTermNoExist(SysMessageLog message, ApplicationDbContext dataAccess, PayerModel payer)
        {
            if (dataAccess.Set<PayerModel>().Any(p => p.InternalId != payer.InternalId && p.CustomerCode == payer.CustomerCode
            && p.PayerNo == payer.PayerNo && (p.FormStatus == FormStatus.Saved || p.FormStatus == FormStatus.Approved)))
                message.AddCustErrorMessage(MessageCode.Code1008, ResxManage.GetDescription<PayerModel>(p => p.PayerNo), payer.PayerNo);
        }
        #endregion
        #endregion
    }
}
