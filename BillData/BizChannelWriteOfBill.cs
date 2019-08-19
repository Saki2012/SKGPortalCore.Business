using SKGPortalCore.Data;
using SKGPortalCore.Lib;
using SKGPortalCore.Model.BillData;
using System.Collections.Generic;

namespace SKGPortalCore.Business.BillData
{
    /// <summary>
    /// 通路帳款核銷單-商業邏輯
    /// </summary>
    public class BizChannelWriteOfBill : BizBase
    {
        #region Public
        public BizChannelWriteOfBill(MessageLog message) : base(message) { }
        #endregion

        #region Private
        private void CompareData(List<ChannelWriteOfDetailModel> channelWriteOfDetail, List<CashFlowWriteOfDetailModel> cashFlowWriteOfDetail)
        {
            RecComparison<ChannelWriteOfDetailModel, CashFlowWriteOfDetailModel> rc = new RecComparison<ChannelWriteOfDetailModel, CashFlowWriteOfDetailModel>(channelWriteOfDetail, cashFlowWriteOfDetail, "BillNo,RowId", "BillNo,RowId");
            if (rc.Enable)
                while (!rc.IsEof)
                {
                    rc.BackToBookMark();
                    while (rc.Compare())
                    {
                        rc.SetBookMark();
                        rc.CurrentRow.Value += rc.DetailRow.Value;
                        rc.DetailMoveNext();
                    }
                    rc.MoveNext();
                }
        }
        #endregion
    }
}
