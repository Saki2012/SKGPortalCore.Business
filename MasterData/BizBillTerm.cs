using System;
using System.Collections.Generic;
using System.Text;
using SKGPortalCore.Data;
using SKGPortalCore.Lib;
using SKGPortalCore.Model;
using SKGPortalCore.Model.MasterData;
using SKGPortalCore.Model.MasterData.OperateSystem;

namespace SKGPortalCore.Business.MasterData
{
    public class BizBillTerm : BizBase
    {
        #region Construct
        public BizBillTerm(MessageLog message, ApplicationDbContext dataAccess = null, IUserModel user = null) : base(message, dataAccess, user) { }
        #endregion

        #region Public
        /// <summary>
        /// 檢查欄位
        /// </summary>
        /// <param name="set"></param>
        public void CheckData(BillTermSet set)
        {
            if (!CheckTermNoLen(set.BillTerm)) { Message.AddErrorMessage(MessageCode.Code1005, ResxManage.GetDescription(set.BillTerm.BillTermNo), set.BillTerm.BizCustomer.Customer.BillTermLen); }
        }
        #endregion

        #region Private
        private bool CheckTermNoLen(BillTermModel billTerm)
        {
            return billTerm.BizCustomer.Customer.BillTermLen == billTerm.BillTermNo.Length;
        }
        #endregion
    }
}
