using System;
using System.Collections.Generic;
using System.Linq;
using SKGPortalCore.Data;
using SKGPortalCore.Lib;
using SKGPortalCore.Model;
using SKGPortalCore.Model.BillData;
using SKGPortalCore.Model.MasterData;
using SKGPortalCore.Model.SourceData;

namespace SKGPortalCore.Repository.SKGPortalCore.Business.BillData
{
    public static class BizReceiptInfoBillBANK
    {
        public static void CheckData(ReceiptInfoBillBankModel model, SysMessageLog Message)
        {
            if (model.RealAccount.IsNullOrEmpty()) { Message.AddCustErrorMessage(MessageCode.Code1010, model.Id, ResxManage.GetDescription(model.RealAccount)); }
            else if (!model.RealAccount.IsNumberString()) { Message.AddCustErrorMessage(MessageCode.Code1009, model.Id, ResxManage.GetDescription(model.RealAccount), model.RealAccount); }
            string tradedate = $"{model.TradeDate.ToADDateFormat()} {model.TradeTime.Substring(0, 2)}:{model.TradeTime.Substring(2, 2)}:{model.TradeTime.Substring(4, 2)}";
            if (!DateTime.TryParse(tradedate, out _)) { Message.AddCustErrorMessage(MessageCode.Code1011, model.Id, ResxManage.GetDescription(model.TradeDate)); }
            //if (model.CompareCode.IsNullOrEmpty()) { Message.AddErrorMessage(MessageCode.Code1010, model.Id, ResxManage.GetDescription(model.CompareCode)); }
            //else if (!model.CompareCode.IsNumberString()) { Message.AddErrorMessage(MessageCode.Code1009, model.Id, ResxManage.GetDescription(model.CompareCode), model.CompareCode); }
            if (model.PN.CompareTo("+") != 0 && model.PN.CompareTo("-") != 0) { Message.AddCustErrorMessage(MessageCode.Code1012, model.Id); }
            if (model.Amount.IsNullOrEmpty()) { Message.AddCustErrorMessage(MessageCode.Code1010, model.Id, ResxManage.GetDescription(model.Amount)); }
            else if (!model.Amount.IsNumberString()) { Message.AddCustErrorMessage(MessageCode.Code1009, model.Id, ResxManage.GetDescription(model.Amount), model.Amount); }
            if (model.TradeChannel.IsNullOrEmpty()) { Message.AddCustErrorMessage(MessageCode.Code1010, model.Id, ResxManage.GetDescription(model.TradeChannel)); }
            if (model.Channel.IsNullOrEmpty()) { Message.AddCustErrorMessage(MessageCode.Code1010, model.Id, ResxManage.GetDescription(model.Channel)); }
            if (model.CustomerCode.IsNullOrEmpty()) { Message.AddCustErrorMessage(MessageCode.Code1010, model.Id, ResxManage.GetDescription(model.CustomerCode)); }
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
                    //CustomerCode = bizCust?.BizCustomer.CustomerCode,
                    CollectionTypeId = "Bank999",
                    ChannelId = model.Channel,
                    TransDate = tradeDate,
                    TradeDate = tradeDate,
                    ExpectRemitDate = tradeDate,
                    PayAmount = model.Amount.ToDecimal(),
                    BankBarCode = model.CompareCode.Trim(),
                    //CompareCodeForCheck = compareCodeForCheck,
                    TransDataUnusual = TransDataUnusual.Normal,
                    //ChargePayType = chargePayType,
                    //ChannelFee = channelFee,//model.Fee.ToDecimal(),銀行帶過來的通路手續費暫時不管
                    //BankFee = bankFee,
                    //ThirdFee = thirdFee,
                    //HiTrustFee = hiTrustFee,
                    ImportBatchNo = model.ImportBatchNo,
                    Source = model.Source,
                }
            };
            return result;
        }
    }
    public static class BizReceiptInfoBillPOST
    {
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
                    //CustomerCode = bizCust?.BizCustomer.CustomerCode,
                    CollectionTypeId = model.CollectionType.Trim(),
                    ChannelId = ChannelMap.FirstOrDefault(p => p.TransCode == model.Channel)?.ChannelId,
                    TransDate = model.TradeDate.ROCDateToCEDate(),
                    TradeDate = model.TradeDate.ROCDateToCEDate(),
                    ExpectRemitDate = model.TradeDate.ROCDateToCEDate(),
                    PayAmount = model.Amount.ToDecimal(),
                    BankBarCode = model.CompareCode,
                    TransDataUnusual = TransDataUnusual.Normal,
                    //ChargePayType = chargePayType,
                    //ChannelFee = channelFee,//model.Fee.ToDecimal(),銀行帶過來的通路手續費暫時不管
                    //BankFee = bankFee,
                    //ThirdFee = thirdFee,
                    //HiTrustFee = hiTrustFee,
                    ImportBatchNo = model.ImportBatchNo,
                    Source = model.Source,
                }
            };
            return result;
        }
    }
    public static class BizReceiptInfoBillMARKET
    {
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
                    //CustomerCode = bizCust.BizCustomer.CustomerCode,
                    CollectionTypeId = model.CollectionType,
                    TransDate = model.PayDate.ToDateTime(),
                    TradeDate = model.PayDate.ToDateTime(),
                    ExpectRemitDate = model.PayDate.ToDateTime(),
                    PayAmount = model.Barcode3.ToDecimal(),
                    BankBarCode = model.Barcode2,
                    //CompareCodeForCheck = compareCodeForCheck,
                    TransDataUnusual = TransDataUnusual.Normal,
                    //ChargePayType = chargePayType,
                    //ChannelFee = channelFee,//model.Fee.ToDecimal(),銀行帶過來的通路手續費暫時不管
                    //BankFee = bankFee,
                    //ThirdFee = thirdFee,
                    //HiTrustFee = hiTrustFee,
                    ImportBatchNo = model.ImportBatchNo,
                    Source = model.Source,
                }
            };
            return result;
        }
    }
    public static class BizReceiptInfoBillMARKETSPI
    {
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
                    //CustomerCode = bizCust.BizCustomer.CustomerCode,
                    CollectionTypeId = model.ISC,
                    TransDate = model.TransDate.ToDateTime(),
                    TradeDate = model.PayDate.ToDateTime(),
                    ExpectRemitDate = model.PayDate.ToDateTime(),
                    PayAmount = model.Barcode3_Amount.ToDecimal(),
                    BankBarCode = model.Barcode3_CompareCode,
                    //CompareCodeForCheck = compareCodeForCheck,
                    TransDataUnusual = TransDataUnusual.Normal,
                    //ChargePayType = chargePayType,
                    //ChannelFee = channelFee,//model.Fee.ToDecimal(),銀行帶過來的通路手續費暫時不管
                    //BankFee = bankFee,
                    //ThirdFee = thirdFee,
                    //HiTrustFee = hiTrustFee,
                    ImportBatchNo = model.ImportBatchNo,
                    Source = model.Source,
                }
            };
            return result;
        }
    }
    public static class BizReceiptInfoBillFARM
    {
        public static void CheckData(ReceiptInfoBillFarmModel model)
        {


        }
        public static ReceiptBillSet GetReceiptBillSet(ReceiptInfoBillFarmModel model)
        {
            ReceiptBillSet result = new ReceiptBillSet()
            {
                ReceiptBill = new ReceiptBillModel()
                {
                    BillNo = "",
                    //CustomerCode = bizCust.BizCustomer.CustomerCode,
                    CollectionTypeId = model.CollectionType,
                    TransDate = model.PayDate.ToDateTime(),
                    TradeDate = model.PayDate.ToDateTime(),
                    ExpectRemitDate = model.PayDate.ToDateTime(),
                    PayAmount = model.Barcode3.ToDecimal(),
                    BankBarCode = model.Barcode2,
                    TransDataUnusual = TransDataUnusual.Normal,
                    //ChargePayType = chargePayType,
                    //ChannelFee = channelFee,//model.Fee.ToDecimal(),銀行帶過來的通路手續費暫時不管
                    //BankFee = bankFee,
                    //ThirdFee = thirdFee,
                    //HiTrustFee = hiTrustFee,
                    ImportBatchNo = model.ImportBatchNo,
                    Source = model.Source,
                }
            };
            return result;
        }
    }
}
