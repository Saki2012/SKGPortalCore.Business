using SKGPortalCore.Data;
using SKGPortalCore.Lib;
using SKGPortalCore.Model.MasterData;

namespace SKGPortalCore.Repository.SKGPortalCore.Business.MasterData
{
    internal static class BizBillTerm
    {
        #region Public
        /// <summary>
        /// 檢查欄位
        /// </summary>
        /// <param name="set"></param>
        public static void CheckData(ApplicationDbContext dataAccess, SysMessageLog Message, BillTermSet set)
        {
            if (!CheckTermNoLen(set.BillTerm)) { Message.AddCustErrorMessage(MessageCode.Code1005, ResxManage.GetDescription(set.BillTerm.BillTermNo), set.BillTerm.BizCustomer.BillTermLen); }
            if (CheckTermNoExist(dataAccess, set.BillTerm)) { Message.AddCustErrorMessage(MessageCode.Code1008, ResxManage.GetDescription(set.BillTerm.BillTermNo), set.BillTerm.BillTermNo); }
            if (!CheckTermNo(set.BillTerm)) { Message.AddCustErrorMessage(MessageCode.Code1006, ResxManage.GetDescription(set.BillTerm)); }
        }
        #endregion

        #region Private
        /// <summary>
        /// 檢查期別編號長度
        /// </summary>
        /// <param name="billTerm"></param>
        /// <returns></returns>
        private static bool CheckTermNoLen(BillTermModel billTerm)
        {
            return billTerm.BizCustomer.BillTermLen == billTerm.BillTermNo.Length;
        }
        /// <summary>
        /// 檢查期別編號是否重複
        /// </summary>
        /// <param name="dataAccess"></param>
        /// <param name="billTerm"></param>
        /// <returns></returns>
        private static bool CheckTermNoExist(ApplicationDbContext dataAccess, BillTermModel billTerm)
        {
            return dataAccess.Set<BillTermModel>().Where("BillTermId<>{0} And CustomerCode={1} And BillTermNo={2} And FormStatus In(0,1)", billTerm.BillTermId, billTerm.CustomerCode, billTerm.BillTermNo).Any();
        }
        /// <summary>
        /// 檢查「期別編號」是否為數字
        /// </summary>
        /// <param name="payer"></param>
        private static bool CheckTermNo(BillTermModel billTerm)
        {
            return billTerm.BillTermNo.IsNumberString();
        }
        #endregion
    }
}
