using System;
using System.Collections.Generic;
using System.Text;
using SKGPortalCore.Data;
using SKGPortalCore.Lib;
using SKGPortalCore.Model.BillData;

namespace SKGPortalCore.Business.BillData
{
    public class BizRemitInfo : BizBase
    {
        public BizRemitInfo(MessageLog message) : base(message) { }

        public CashFlowBillSet GetCashFlowBillSet(RemitInfoModel model)
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
    }
}
