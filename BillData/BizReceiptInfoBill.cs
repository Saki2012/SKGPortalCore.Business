﻿using System;
using System.Collections.Generic;
using System.Text;
using SKGPortalCore.Data;
using SKGPortalCore.Lib;
using SKGPortalCore.Model;
using SKGPortalCore.Model.BillData;
using SKGPortalCore.Model.MasterData;

using System.Linq;

namespace SKGPortalCore.Business.BillData
{
    public interface IBizReceiptInfoBill<T> where T : IReceiptInfoBill
    {
        public void CheckData(T model);
        public ReceiptBillSet GetReceiptBillSet(T model, BizCustomerSet bizCust, ChargePayType chargePayType, decimal channelFee, string compareCodeForCheck);
    }
    public class BizReceiptInfoBill : BizBase
    {
        public BizReceiptInfoBill(MessageLog message) : base(message) { }
        /// <summary>
        /// 獲取客戶資料對應的費用(清算手續費、系統商手續費、Hitrust費用)
        /// </summary>
        /// <param name="detail"></param>
        /// <param name="chargePayType"></param>
        /// <param name="canalisType"></param>
        /// <param name="bankFee"></param>
        /// <param name="thirdFee"></param>
        /// <param name="hiTrustFee"></param>
        private protected void GetBizCustFee(List<BizCustFeeDetailModel> detail, decimal channelFee, ChargePayType chargePayType, CanalisType canalisType, out decimal bankFee, out decimal thirdFee, out decimal hiTrustFee)
        {
            bankFee = 0; thirdFee = 0; hiTrustFee = 0;
            if (!LibData.HasData(detail)) return;
            BizCustFeeDetailModel model = detail.FirstOrDefault(p => p.ChannelType == canalisType && (p.FeeType == FeeType.ClearFee || p.FeeType == FeeType.TotalFee));
            hiTrustFee = detail.FirstOrDefault(p => p.ChannelType == canalisType && p.FeeType == FeeType.HitrustFee).Fee;
            if (null != model)
            {
                switch (model.FeeType)
                {
                    case FeeType.ClearFee:
                        {
                            bankFee = model.Fee;
                        }
                        break;
                    case FeeType.TotalFee:
                        {
                            switch (chargePayType)
                            {
                                case ChargePayType.Deduction:
                                    {
                                        thirdFee = BizReceiptBill.FeeDeduct(model.Fee, channelFee, model.Percent);
                                        bankFee = model.Fee - thirdFee;
                                    }
                                    break;
                                case ChargePayType.Increase:
                                    {
                                        thirdFee = BizReceiptBill.FeePlus(model.Fee, model.Percent);
                                        bankFee = model.Fee - thirdFee;
                                    }
                                    break;
                            }
                            break;
                        }
                }
            }
        }
    }
    public class BizReceiptInfoBillBANK : BizReceiptInfoBill, IBizReceiptInfoBill<ReceiptInfoBillBankModel>
    {
        public BizReceiptInfoBillBANK(MessageLog message) : base(message) { }
        public void CheckData(ReceiptInfoBillBankModel model)
        {
            if (model.RealAccount.IsNullOrEmpty()) { Message.AddErrorMessage(MessageCode.Code1010, model.Id, ResxManage.GetDescription(model.RealAccount)); }
            else if (!model.RealAccount.IsNumberString()) { Message.AddErrorMessage(MessageCode.Code1009, model.Id, ResxManage.GetDescription(model.RealAccount), model.RealAccount); }
            string tradedate = $"{model.TradeDate.ToADDateFormat()} {model.TradeTime.Substring(0, 2)}:{model.TradeTime.Substring(2, 2)}:{model.TradeTime.Substring(4, 2)}";
            if (!DateTime.TryParse(tradedate, out _)) { Message.AddErrorMessage(MessageCode.Code1011, model.Id, ResxManage.GetDescription(model.TradeDate)); }
            if (model.CompareCode.IsNullOrEmpty()) { Message.AddErrorMessage(MessageCode.Code1010, model.Id, ResxManage.GetDescription(model.CompareCode)); }
            else if (!model.CompareCode.IsNumberString()) { Message.AddErrorMessage(MessageCode.Code1009, model.Id, ResxManage.GetDescription(model.CompareCode), model.CompareCode); }
            if (model.PN.CompareTo("+") != 0 && model.PN.CompareTo("-") != 0) { Message.AddErrorMessage(MessageCode.Code1012, model.Id); }
            if (model.Amount.IsNullOrEmpty()) { Message.AddErrorMessage(MessageCode.Code1010, model.Id, ResxManage.GetDescription(model.Amount)); }
            else if (!model.Amount.IsNumberString()) { Message.AddErrorMessage(MessageCode.Code1009, model.Id, ResxManage.GetDescription(model.Amount), model.Amount); }
            if (model.TradeChannel.IsNullOrEmpty()) { Message.AddErrorMessage(MessageCode.Code1010, model.Id, ResxManage.GetDescription(model.TradeChannel)); }
            if (model.Channel.IsNullOrEmpty()) { Message.AddErrorMessage(MessageCode.Code1010, model.Id, ResxManage.GetDescription(model.Channel)); }
            if (model.CustomerCode.IsNullOrEmpty()) { Message.AddErrorMessage(MessageCode.Code1010, model.Id, ResxManage.GetDescription(model.CustomerCode)); }
            if (model.Fee.IsNullOrEmpty()) { Message.AddErrorMessage(MessageCode.Code1010, model.Id, ResxManage.GetDescription(model.Fee)); }
            else if (!model.Fee.IsNumberString()) { Message.AddErrorMessage(MessageCode.Code1009, model.Id, ResxManage.GetDescription(model.Fee), model.Fee); }
        }
        public ReceiptBillSet GetReceiptBillSet(ReceiptInfoBillBankModel model, BizCustomerSet bizCust, ChargePayType chargePayType, decimal channelFee, string compareCodeForCheck)
        {
            DateTime.TryParse($"{model.TradeDate.ToADDateFormat()} {model.TradeTime.Substring(0, 2)}:{model.TradeTime.Substring(2, 2)}:{model.TradeTime.Substring(4, 2)}", out DateTime tradeDate);
            GetBizCustFee(bizCust.BizCustFeeDetail, channelFee, chargePayType, CanalisType.Bank, out decimal bankFee, out decimal thirdFee, out decimal hiTrustFee);
            ReceiptBillSet result = new ReceiptBillSet()
            {
                ReceiptBill = new ReceiptBillModel()
                {
                    BillNo = "",
                    CustomerCode = bizCust.BizCustomer.CustomerCode,
                    CollectionTypeId = "Bank999",
                    ChannelId = model.Channel,
                    TransDate = tradeDate,
                    TradeDate = tradeDate,
                    RemitDate = tradeDate,
                    PayAmount = model.Amount.ToDecimal(),
                    BankCode = model.CompareCode,
                    CompareCode = model.CompareCode,
                    CompareCodeForCheck = compareCodeForCheck,
                    TransDataUnusual = TransDataUnusual.Normal,
                    ChargePayType = chargePayType,
                    ChannelFee = channelFee,//model.Fee.ToDecimal(),銀行帶過來的通路手續費暫時不管
                    BankFee = bankFee,
                    ThirdFee = thirdFee,
                    HiTrustFee = hiTrustFee,
                    ImportBatchNo = model.ImportBatchNo,
                    Source = model.Source,
                }
            };
            return result;
        }
    }
    public class BizReceiptInfoBillPOST : BizReceiptInfoBill, IBizReceiptInfoBill<ReceiptInfoBillPostModel>
    {
        public BizReceiptInfoBillPOST(MessageLog message) : base(message) { }
        public void CheckData(ReceiptInfoBillPostModel model)
        {
        }
        public ReceiptBillSet GetReceiptBillSet(ReceiptInfoBillPostModel model, BizCustomerSet bizCust, ChargePayType chargePayType, decimal channelFee, string compareCodeForCheck)
        {
            GetBizCustFee(bizCust.BizCustFeeDetail, channelFee, chargePayType, CanalisType.Post, out decimal bankFee, out decimal thirdFee, out decimal hiTrustFee);
            ReceiptBillSet result = new ReceiptBillSet()
            {
                ReceiptBill = new ReceiptBillModel()
                {
                    BillNo = "",
                    CustomerCode = bizCust.BizCustomer.CustomerCode,
                    CollectionTypeId = model.CollectionType.Trim(),
                    ChannelId = model.Channel,
                    TransDate = model.TradeDate.ToDateTime(),
                    TradeDate = model.TradeDate.ToDateTime(),
                    RemitDate = model.TradeDate.ToDateTime(),
                    PayAmount = model.Amount.ToDecimal(),
                    BankCode = model.CompareCode,
                    CompareCode = model.CompareCode,
                    CompareCodeForCheck = compareCodeForCheck,
                    TransDataUnusual = TransDataUnusual.Normal,
                    ChargePayType = chargePayType,
                    ChannelFee = channelFee,//model.Fee.ToDecimal(),銀行帶過來的通路手續費暫時不管
                    BankFee = bankFee,
                    ThirdFee = thirdFee,
                    HiTrustFee = hiTrustFee,
                    ImportBatchNo = model.ImportBatchNo,
                    Source = model.Source,
                }
            };
            return result;
        }
    }
    public class BizReceiptInfoBillMARKET : BizReceiptInfoBill, IBizReceiptInfoBill<ReceiptInfoBillMarketModel>
    {
        public BizReceiptInfoBillMARKET(MessageLog message) : base(message) { }
        public void CheckData(ReceiptInfoBillMarketModel model)
        {
        }
        public ReceiptBillSet GetReceiptBillSet(ReceiptInfoBillMarketModel model, BizCustomerSet bizCust, ChargePayType chargePayType, decimal channelFee, string compareCodeForCheck)
        {
            GetBizCustFee(bizCust.BizCustFeeDetail, channelFee, chargePayType, CanalisType.Market, out decimal bankFee, out decimal thirdFee, out decimal hiTrustFee);
            ReceiptBillSet result = new ReceiptBillSet()
            {
                ReceiptBill = new ReceiptBillModel()
                {
                    BillNo = "",
                    CustomerCode = bizCust.BizCustomer.CustomerCode,
                    CollectionTypeId = model.CollectionType,
                    TransDate = model.PayDate.ToDateTime(),
                    TradeDate = model.PayDate.ToDateTime(),
                    RemitDate = model.PayDate.ToDateTime(),
                    PayAmount = model.Barcode3.ToDecimal(),
                    BankCode = model.Barcode2,
                    CompareCode = model.Barcode2,
                    CompareCodeForCheck = compareCodeForCheck,
                    TransDataUnusual = TransDataUnusual.Normal,
                    ChargePayType = chargePayType,
                    ChannelFee = channelFee,//model.Fee.ToDecimal(),銀行帶過來的通路手續費暫時不管
                    BankFee = bankFee,
                    ThirdFee = thirdFee,
                    HiTrustFee = hiTrustFee,
                    ImportBatchNo = model.ImportBatchNo,
                    Source = model.Source,
                }
            };
            return result;
        }
    }
    public class BizReceiptInfoBillMARKETSPI : BizReceiptInfoBill, IBizReceiptInfoBill<ReceiptInfoBillMarketSPIModel>
    {
        public BizReceiptInfoBillMARKETSPI(MessageLog message) : base(message) { }
        public void CheckData(ReceiptInfoBillMarketSPIModel model)
        {
        }
        public ReceiptBillSet GetReceiptBillSet(ReceiptInfoBillMarketSPIModel model, BizCustomerSet bizCust, ChargePayType chargePayType, decimal channelFee, string compareCodeForCheck)
        {
            GetBizCustFee(bizCust.BizCustFeeDetail, channelFee, chargePayType, CanalisType.Market, out decimal bankFee, out decimal thirdFee, out decimal hiTrustFee);
            ReceiptBillSet result = new ReceiptBillSet()
            {
                ReceiptBill = new ReceiptBillModel()
                {
                    BillNo = "",
                    CustomerCode = bizCust.BizCustomer.CustomerCode,
                    CollectionTypeId = model.ISC,
                    TransDate = model.TransDate.ToDateTime(),
                    TradeDate = model.PayDate.ToDateTime(),
                    RemitDate = model.PayDate.ToDateTime(),
                    PayAmount = model.Barcode3_Amount.ToDecimal(),
                    BankCode = model.Barcode3_CompareCode,
                    CompareCode = model.Barcode3_CompareCode,
                    CompareCodeForCheck = compareCodeForCheck,
                    TransDataUnusual = TransDataUnusual.Normal,
                    ChargePayType = chargePayType,
                    ChannelFee = channelFee,//model.Fee.ToDecimal(),銀行帶過來的通路手續費暫時不管
                    BankFee = bankFee,
                    ThirdFee = thirdFee,
                    HiTrustFee = hiTrustFee,
                    ImportBatchNo = model.ImportBatchNo,
                    Source = model.Source,
                }
            };
            return result;
        }
    }
    public class BizReceiptInfoBillFARM : BizReceiptInfoBill, IBizReceiptInfoBill<ReceiptInfoBillFarmModel>
    {
        public BizReceiptInfoBillFARM(MessageLog message) : base(message) { }
        public void CheckData(ReceiptInfoBillFarmModel model)
        {


        }
        public ReceiptBillSet GetReceiptBillSet(ReceiptInfoBillFarmModel model, BizCustomerSet bizCust, ChargePayType chargePayType, decimal channelFee, string compareCodeForCheck)
        {
            GetBizCustFee(bizCust.BizCustFeeDetail, channelFee, chargePayType, CanalisType.Farm, out decimal bankFee, out decimal thirdFee, out decimal hiTrustFee);
            ReceiptBillSet result = new ReceiptBillSet()
            {
                ReceiptBill = new ReceiptBillModel()
                {
                    BillNo = "",
                    CustomerCode = bizCust.BizCustomer.CustomerCode,
                    CollectionTypeId = model.CollectionType,
                    TransDate = model.PayDate.ToDateTime(),
                    TradeDate = model.PayDate.ToDateTime(),
                    RemitDate = model.PayDate.ToDateTime(),
                    PayAmount = model.Barcode3.ToDecimal(),
                    BankCode = model.Barcode2,
                    CompareCode = model.Barcode2,
                    CompareCodeForCheck = compareCodeForCheck,
                    TransDataUnusual = TransDataUnusual.Normal,
                    ChargePayType = chargePayType,
                    ChannelFee = channelFee,//model.Fee.ToDecimal(),銀行帶過來的通路手續費暫時不管
                    BankFee = bankFee,
                    ThirdFee = thirdFee,
                    HiTrustFee = hiTrustFee,
                    ImportBatchNo = model.ImportBatchNo,
                    Source = model.Source,
                }
            };
            return result;
        }
    }
}
