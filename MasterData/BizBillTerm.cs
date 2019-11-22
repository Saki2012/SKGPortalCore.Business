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
        public static void CheckData(MessageLog Message, BillTermSet set)
        {
            if (!CheckTermNoLen(set.BillTerm)) { Message.AddErrorMessage(MessageCode.Code1005, ResxManage.GetDescription(set.BillTerm.BillTermNo), set.BillTerm.BizCustomer.Customer.BillTermLen); }
        }
        #endregion

        #region Private
        private static bool CheckTermNoLen(BillTermModel billTerm)
        {
            return billTerm.BizCustomer.Customer.BillTermLen == billTerm.BillTermNo.Length;
        }
        #endregion
    }
}
