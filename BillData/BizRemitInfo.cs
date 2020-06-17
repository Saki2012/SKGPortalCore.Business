using System;
using SKGPortalCore.Core.Libary;
using SKGPortalCore.Model.BillData;
using SKGPortalCore.Model.SourceData;

namespace SKGPortalCore.Repository.SKGPortalCore.Business.BillData
{
    public static class BizRemitInfo
    {
        #region Public
        public static CashFlowBillSet GetCashFlowBillSet(RemitInfoModel model)
        {
            return new CashFlowBillSet()
            {
                CashFlowBill = new CashFlowBillModel()
                {
                    BillNo = "",
                    RemitTime = Convert.ToDateTime(model.RemitDate + model.RemitTime),
                    ChannelId = model.Channel,
                    CollectionTypeId = model.CollectionType,
                    Amount = model.Amount.ToDecimal(),
                    ImportBatchNo = model.ImportBatchNo,
                    Source = model.Source
                }
            };
        }
        #endregion
    }
}
