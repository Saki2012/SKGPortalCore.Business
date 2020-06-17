using SKGPortalCore.Core;
using SKGPortalCore.Core.DB;
using SKGPortalCore.Core.Libary;
using SKGPortalCore.Core.LibEnum;
using SKGPortalCore.Model.MasterData;
using System.Linq;

namespace SKGPortalCore.Repository.SKGPortalCore.Business.MasterData
{
    internal static class BizBillTerm
    {
        #region Public
        /// <summary>
        /// 檢查欄位
        /// </summary>
        /// <param name="set"></param>
        public static void CheckData(ApplicationDbContext dataAccess, SysMessageLog message, BillTermSet set)
        {
            CheckTermNo(message, set.BillTerm);
            CheckTermNoLen(message, set.BillTerm);
            CheckTermNoExist(message, dataAccess, set.BillTerm);
        }
        #endregion

        #region Private
        /// <summary>
        /// 檢查期別編號長度
        /// </summary>
        /// <param name="billTerm"></param>
        /// <returns></returns>
        private static void CheckTermNoLen(SysMessageLog message, BillTermModel billTerm)
        {
            if (billTerm.BizCustomer.BillTermLen != billTerm.BillTermNo.Length)
                message.AddCustErrorMessage(MessageCode.Code1005, ResxManage.GetDescription<BillTermModel>(p => p.BillTermNo), billTerm.BizCustomer.BillTermLen);
        }
        /// <summary>
        /// 檢查期別編號是否重複
        /// </summary>
        /// <param name="dataAccess"></param>
        /// <param name="billTerm"></param>
        /// <returns></returns>
        private static void CheckTermNoExist(SysMessageLog message, ApplicationDbContext dataAccess, BillTermModel billTerm)
        {
            if (dataAccess.Set<BillTermModel>().Any(p => p.InternalId != billTerm.InternalId && p.CustomerCode == billTerm.CustomerCode
            && p.BillTermNo == billTerm.BillTermNo && (p.FormStatus == FormStatus.Saved || p.FormStatus == FormStatus.Approved)))
                message.AddCustErrorMessage(MessageCode.Code1008, ResxManage.GetDescription<BillTermModel>(p => p.BillTermNo), billTerm.BillTermNo);
        }
        /// <summary>
        /// 檢查「期別編號」是否為數字
        /// </summary>
        /// <param name="payer"></param>
        private static void CheckTermNo(SysMessageLog message, BillTermModel billTerm)
        {
            if (!billTerm.BillTermNo.IsNumberString())
                message.AddCustErrorMessage(MessageCode.Code1006, ResxManage.GetDescription<BillTermModel>(p => p.BillTermNo));
        }
        #endregion
    }
}
