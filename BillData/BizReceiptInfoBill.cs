using System;
using System.Collections.Generic;
using System.Linq;
using SKGPortalCore.Data;
using SKGPortalCore.Lib;
using SKGPortalCore.Model.BillData;
using SKGPortalCore.Model.Enum;
using SKGPortalCore.Model.MasterData;
using SKGPortalCore.Model.SourceData;

namespace SKGPortalCore.Repository.SKGPortalCore.Business.BillData
{
    public static class BizReceiptInfo
    {
        //銀行
        public static void CheckData(ReceiptInfoBillBankModel model, SysMessageLog Message)
        {
            if (model.RealAccount.IsNullOrEmpty()) { Message.AddCustErrorMessage(MessageCode.Code1010, model.Id, ResxManage.GetDescription(model.RealAccount)); }
            else if (!model.RealAccount.IsNumberString()) { Message.AddCustErrorMessage(MessageCode.Code1009, model.Id, ResxManage.GetDescription(model.RealAccount), model.RealAccount); }
            string tradedate = $"{model.TradeDate.ToADDateFormat()} {model.TradeTime.Substring(0, 2)}:{model.TradeTime.Substring(2, 2)}:{model.TradeTime.Substring(4, 2)}";
            if (!DateTime.TryParse(tradedate, out _)) { Message.AddCustErrorMessage(MessageCode.Code1011, model.Id, ResxManage.GetDescription(model.TradeDate)); }
            if (model.PN.CompareTo("+") != 0 && model.PN.CompareTo("-") != 0) { Message.AddCustErrorMessage(MessageCode.Code1012, model.Id); }
            if (model.Amount.IsNullOrEmpty()) { Message.AddCustErrorMessage(MessageCode.Code1010, model.Id, ResxManage.GetDescription(model.Amount)); }
            else if (!model.Amount.IsNumberString()) { Message.AddCustErrorMessage(MessageCode.Code1009, model.Id, ResxManage.GetDescription(model.Amount), model.Amount); }
            if (model.TradeChannel.IsNullOrEmpty()) { Message.AddCustErrorMessage(MessageCode.Code1010, model.Id, ResxManage.GetDescription(model.TradeChannel)); }
            if (model.Channel.IsNullOrEmpty()) { Message.AddCustErrorMessage(MessageCode.Code1010, model.Id, ResxManage.GetDescription(model.Channel)); }
            if (model.Fee.IsNullOrEmpty()) { Message.AddCustErrorMessage(MessageCode.Code1010, model.Id, ResxManage.GetDescription(model.Fee)); }
            else if (!model.Fee.IsNumberString()) { Message.AddCustErrorMessage(MessageCode.Code1009, model.Id, ResxManage.GetDescription(model.Fee), model.Fee); }
        }
        public static ReceiptBillSet GetReceiptBillSet(ReceiptInfoBillBankModel model)
        {
            DateTime.TryParse($"{model.TradeDate.ToADDateFormat()} {model.TradeTime.Substring(0, 2)}:{model.TradeTime.Substring(2, 2)}:{model.TradeTime.Substring(4, 2)}", out DateTime tradeDate);
            ReceiptBillSet result = new ReceiptBillSet()
            {
                ReceiptBill = new ReceiptBillModel()
                {
                    BillNo = "",
                    CollectionTypeId = ConstParameter.BankCollectionTypeId,
                    ChannelId = model.Channel,
                    TransDate = tradeDate,
                    TradeDate = tradeDate,
                    ExpectRemitDate = tradeDate,
                    PayAmount = model.Amount.ToDecimal(),
                    BankBarCode = model.CompareCode.Trim(),
                    ImportBatchNo = model.ImportBatchNo,
                    Source = model.Source,
                }
            };
            return result;
        }
        //郵局
        public static void CheckData(ReceiptInfoBillPostModel model)
        {
        }
        public static ReceiptBillSet GetReceiptBillSet(ReceiptInfoBillPostModel model, ApplicationDbContext DataAccess)
        {
            List<ChannelMapModel> ChannelMap = DataAccess.Set<ChannelMapModel>().ToList();
            ReceiptBillSet result = new ReceiptBillSet()
            {
                ReceiptBill = new ReceiptBillModel()
                {
                    BillNo = "",
                    CollectionTypeId = model.CollectionType.Trim(),
                    ChannelId = ChannelMap.FirstOrDefault(p => p.TransCode == model.Channel)?.ChannelId,
                    TransDate = model.TradeDate.ROCDateToCEDate(),
                    TradeDate = model.TradeDate.ROCDateToCEDate(),
                    PayAmount = model.Amount.ToDecimal(),
                    BankBarCode = model.CompareCode,
                    ImportBatchNo = model.ImportBatchNo,
                    Source = model.Source,
                }
            };
            return result;
        }
        //超商
        public static void CheckData(ReceiptInfoBillMarketModel model)
        {
        }
        public static ReceiptBillSet GetReceiptBillSet(ReceiptInfoBillMarketModel model)
        {
            ReceiptBillSet result = new ReceiptBillSet()
            {
                ReceiptBill = new ReceiptBillModel()
                {
                    BillNo = "",
                    CollectionTypeId = model.CollectionType,
                    TransDate = model.PayDate.ToDateTime(),
                    TradeDate = model.PayDate.ToDateTime(),
                    ExpectRemitDate = model.PayDate.ToDateTime(),
                    PayAmount = model.Barcode3.ToDecimal(),
                    BankBarCode = model.Barcode2,
                    ImportBatchNo = model.ImportBatchNo,
                    Source = model.Source,
                }
            };
            return result;
        }
        //超商-產險
        public static void CheckData(ReceiptInfoBillMarketSPIModel model)
        {
        }
        public static ReceiptBillSet GetReceiptBillSet(ReceiptInfoBillMarketSPIModel model)
        {
            ReceiptBillSet result = new ReceiptBillSet()
            {
                ReceiptBill = new ReceiptBillModel()
                {
                    BillNo = "",
                    CollectionTypeId = model.ISC,
                    TransDate = model.TransDate.ToDateTime(),
                    TradeDate = model.PayDate.ToDateTime(),
                    ExpectRemitDate = model.PayDate.ToDateTime(),
                    PayAmount = model.Barcode3_Amount.ToDecimal(),
                    BankBarCode = model.Barcode3_CompareCode,
                    ImportBatchNo = model.ImportBatchNo,
                    Source = model.Source,
                }
            };
            return result;
        }
    }
}
