using SKGPortalCore.Core;
using SKGPortalCore.Core.DB;
using SKGPortalCore.Core.Libary;
using SKGPortalCore.Model.BillData;
using SKGPortalCore.Repository.SKGPortalCore.Business.Func;

namespace SKGPortalCore.Repository.SKGPortalCore.Business.BillData
{
    /// <summary>
    /// 入金機-商業邏輯
    /// </summary>
    internal static class BizDepositBill
    {
        #region Public
        /// <summary>
        /// 檢查資料
        /// </summary>
        /// <param name="set"></param>
        public static void CheckData(DepositBillSet set, SysMessageLog message, ApplicationDbContext dataAccess)
        {
            if (BizVirtualAccountCode.CheckBankCodeExist(dataAccess, set.DepositBill.VirtualAccountCode, out _))
            { message.AddCustErrorMessage(MessageCode.Code1008, ResxManage.GetDescription<DepositBillModel>(p => p.VirtualAccountCode), set.DepositBill.VirtualAccountCode); }
        }
        /// <summary>
        /// 設置資料
        /// </summary>
        /// <param name="set"></param>
        /// <param name="action"></param>
        public static void SetData(DepositBillSet set,string progId,ApplicationDbContext dataAccess)
        {
            BizVirtualAccountCode.AddVirtualAccountCode(dataAccess, progId, set.DepositBill.BillNo, set.DepositBill.VirtualAccountCode);
        }
        #endregion

        #region Private

        #region  CheckData

        #endregion

        #region SetData

        #endregion

        #endregion
    }
}
