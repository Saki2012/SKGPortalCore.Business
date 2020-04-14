using System;
using System.Collections.Generic;
using System.Linq;
using SKGPortalCore.Lib;
using SKGPortalCore.Model.BillData;
using SKGPortalCore.Model.MasterData;
using SKGPortalCore.Model.SourceData;

namespace SKGPortalCore.Repository.SKGPortalCore.Business.BillData
{
    public static class BizReceiptInfo
    {
        //銀行
        public static ReceiptBillSet GetReceiptBillSet(ReceiptInfoBillBankModel model)
        {
            DateTime.TryParse($"{model.TradeDate.ToADDateFormat()} {model.TradeTime.Substring(0, 2)}:{model.TradeTime.Substring(2, 2)}:{model.TradeTime.Substring(4, 2)}", out DateTime tradeDate);
            ReceiptBillSet result = new ReceiptBillSet()
            {
                ReceiptBill = new ReceiptBillModel()
                {
                    BillNo = "",
                    CollectionTypeId = SystemCP.BankCollectionTypeId,
                    ChannelId = model.Channel,
                    TransDate = tradeDate,
                    TradeDate = tradeDate,
                    ExpectRemitDate = tradeDate,
                    PayAmount = model.Amount.ToDecimal(),
                    VirtualAccountCode = model.CompareCode,
                    ImportBatchNo = model.ImportBatchNo,
                    Source = model.Source,
                }
            };
            return result;
        }
        //郵局
        public static ReceiptBillSet GetReceiptBillSet(ReceiptInfoBillPostModel model, List<ChannelMapModel> channelMap)
        {
            ReceiptBillSet result = new ReceiptBillSet()
            {
                ReceiptBill = new ReceiptBillModel()
                {
                    BillNo = "",
                    CollectionTypeId = model.CollectionType.Trim().Trim(),
                    ChannelId = channelMap.FirstOrDefault(p => p.TransCode == model.Channel)?.ChannelId,
                    TransDate = model.TradeDate.ROCDateToCEDate().AddDays(1),
                    TradeDate = model.TradeDate.ROCDateToCEDate(),
                    PayAmount = model.Amount.ToDecimal(),
                    VirtualAccountCode = model.CompareCode,
                    ImportBatchNo = model.ImportBatchNo,
                    Source = model.Source,
                }
            };
            return result;
        }
        //超商
        public static ReceiptBillSet GetReceiptBillSet(ReceiptInfoBillMarketModel model, List<ChannelMapModel> channelMap)
        {
            ReceiptBillSet result = new ReceiptBillSet()
            {
                ReceiptBill = new ReceiptBillModel()
                {
                    BillNo = "",
                    CollectionTypeId = model.CollectionType.Trim(),
                    ChannelId = channelMap.FirstOrDefault(p => p.TransCode == model.Channel.Trim())?.ChannelId,
                    TransDate = model.AccountingDay.ToDateTime(),
                    TradeDate = model.PayDate.ToDateTime(),
                    PayAmount = model.Barcode3.ToDecimal(),
                    VirtualAccountCode = model.Barcode2,
                    ImportBatchNo = model.ImportBatchNo,
                    Source = model.Source,
                }
            };
            return result;
        }
        //超商-產險
        public static ReceiptBillSet GetReceiptBillSet(ReceiptInfoBillMarketSPIModel model, List<ChannelMapModel> channelMap)
        {
            ReceiptBillSet result = new ReceiptBillSet()
            {
                ReceiptBill = new ReceiptBillModel()
                {
                    BillNo = "",
                    CollectionTypeId = model.CollectionType.Trim(),
                    ChannelId = channelMap.FirstOrDefault(p => p.TransCode == model.Channel.Trim())?.ChannelId,
                    TransDate = model.TransDate.ToDateTime(),
                    TradeDate = model.PayDate.ToDateTime(),
                    PayAmount = model.Barcode3_Amount.ToDecimal(),
                    VirtualAccountCode = model.Barcode2,
                    ImportBatchNo = model.ImportBatchNo,
                    Source = model.Source,
                }
            };
            return result;
        }
    }
}
