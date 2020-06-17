using SKGPortalCore.Core.DB;
using SKGPortalCore.Core.Libary;
using SKGPortalCore.Core.LibEnum;
using SKGPortalCore.Model.BillData;
using SKGPortalCore.Model.Report;
using System.Collections.Generic;
using System.Linq;

namespace SKGPortalCore.Repository.SKGPortalCore.Business.Report
{
    internal static class BizBillRpt
    {
        /// <summary>
        /// 獲取帳單繳費進度報表
        /// </summary>
        public static List<BillPayProgressRptModel> GetBillPayProgressRpt(ApplicationDbContext dataAccess, string customerCode, string billTermId)
        {
            List<BillPayProgressRptModel> result = new List<BillPayProgressRptModel>();
            result.AddRange(dataAccess.Set<BillModel>().Where(p =>
            (customerCode.IsNullOrEmpty() || p.CustomerCode.Equals(customerCode)) &&
            (billTermId.IsNullOrEmpty() || p.BillTermId.Equals(billTermId)) &&
            p.PayStatus == PayStatus.Unpaid
            ).Select(p => new BillPayProgressRptModel
            {
                BillNo = p.BillNo,
                PayEndDate = p.PayEndDate,
                PayerId = p.Payer.PayerId,
                PayerName = p.Payer.PayerName,
                PayerType = ResxManage.GetDescription(p.Payer.PayerType),
                VirtualAccountCode = p.VirtualAccountCode,
                PayStatus = ResxManage.GetDescription(p.PayStatus),
                PayAmount = p.PayAmount,
                HadPayAmount = p.HadPayAmount,
            }));
            result.AddRange(dataAccess.Set<BillReceiptDetailModel>().Where(p =>
            (customerCode.IsNullOrEmpty() || p.Bill.CustomerCode.Equals(customerCode)) &&
            (billTermId.IsNullOrEmpty() || p.Bill.BillTermId.Equals(billTermId))
            ).Select(p => new BillPayProgressRptModel
            {
                BillNo = p.BillNo,
                PayEndDate = p.Bill.PayEndDate,
                PayerId = p.Bill.Payer.PayerId,
                PayerName = p.Bill.Payer.PayerName,
                PayerType = ResxManage.GetDescription(p.Bill.Payer.PayerType),
                VirtualAccountCode = p.Bill.VirtualAccountCode,
                PayStatus = ResxManage.GetDescription(p.Bill.PayStatus),
                PayAmount = p.Bill.PayAmount,
                HadPayAmount = p.Bill.HadPayAmount,
                TradeDate = p.ReceiptBill.TradeDate,
                ExpectRemitDate = p.ReceiptBill.ExpectRemitDate,
                ChannelId = p.ReceiptBill.Channel.ChannelId,
                ChannelName = p.ReceiptBill.Channel.ChannelName,
            }));
            return result;
        }
    }
}
