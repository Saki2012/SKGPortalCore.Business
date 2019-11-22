using System;
using System.Collections.Generic;
using System.Linq;
using SKGPortalCore.Data;
using SKGPortalCore.Lib;
using SKGPortalCore.Model;
using SKGPortalCore.Model.BillData;
using SKGPortalCore.Model.MasterData;
using SKGPortalCore.Model.SourceData;

namespace SKGPortalCore.Business.BillData
{
    public interface IBizReceiptInfoBill<T> where T : IImportSource
    {
        public void CheckData(T model);
    }
    public class BizReceiptInfoBill:IDisposable
    {
        protected MessageLog Message;
        protected ApplicationDbContext DataAccess;
        public BizReceiptInfoBill(MessageLog message, ApplicationDbContext dataAccess) { Message = message;DataAccess = dataAccess; }

        #region IDisposable Support
        private bool disposedValue = false; // 偵測多餘的呼叫

        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    // TODO: 處置 Managed 狀態 (Managed 物件)。
                }

                // TODO: 釋放 Unmanaged 資源 (Unmanaged 物件) 並覆寫下方的完成項。
                // TODO: 將大型欄位設為 null。

                disposedValue = true;
            }
        }

        // TODO: 僅當上方的 Dispose(bool disposing) 具有會釋放 Unmanaged 資源的程式碼時，才覆寫完成項。
        // ~BizReceiptInfoBill()
        // {
        //   // 請勿變更這個程式碼。請將清除程式碼放入上方的 Dispose(bool disposing) 中。
        //   Dispose(false);
        // }

        // 加入這個程式碼的目的在正確實作可處置的模式。
        public void Dispose()
        {
            // 請勿變更這個程式碼。請將清除程式碼放入上方的 Dispose(bool disposing) 中。
            Dispose(true);
            // TODO: 如果上方的完成項已被覆寫，即取消下行的註解狀態。
            // GC.SuppressFinalize(this);
        }
        #endregion
        /// <summary>
        /// 獲取客戶資料對應的費用(清算手續費、系統商手續費、Hitrust費用)
        /// </summary>
        /// <param name="detail"></param>
        /// <param name="chargePayType"></param>
        /// <param name="canalisType"></param>
        /// <param name="bankFee"></param>
        /// <param name="thirdFee"></param>
        /// <param name="hiTrustFee"></param>

    }
    public class BizReceiptInfoBillBANK : BizReceiptInfoBill, IBizReceiptInfoBill<ReceiptInfoBillBankModel>
    {
        public BizReceiptInfoBillBANK(MessageLog message, ApplicationDbContext dataAccess) : base(message, dataAccess) { }
        public void CheckData(ReceiptInfoBillBankModel model)
        {
            if (model.RealAccount.IsNullOrEmpty()) { Message.AddErrorMessage(MessageCode.Code1010, model.Id, ResxManage.GetDescription(model.RealAccount)); }
            else if (!model.RealAccount.IsNumberString()) { Message.AddErrorMessage(MessageCode.Code1009, model.Id, ResxManage.GetDescription(model.RealAccount), model.RealAccount); }
            string tradedate = $"{model.TradeDate.ToADDateFormat()} {model.TradeTime.Substring(0, 2)}:{model.TradeTime.Substring(2, 2)}:{model.TradeTime.Substring(4, 2)}";
            if (!DateTime.TryParse(tradedate, out _)) { Message.AddErrorMessage(MessageCode.Code1011, model.Id, ResxManage.GetDescription(model.TradeDate)); }
            //if (model.CompareCode.IsNullOrEmpty()) { Message.AddErrorMessage(MessageCode.Code1010, model.Id, ResxManage.GetDescription(model.CompareCode)); }
            //else if (!model.CompareCode.IsNumberString()) { Message.AddErrorMessage(MessageCode.Code1009, model.Id, ResxManage.GetDescription(model.CompareCode), model.CompareCode); }
            if (model.PN.CompareTo("+") != 0 && model.PN.CompareTo("-") != 0) { Message.AddErrorMessage(MessageCode.Code1012, model.Id); }
            if (model.Amount.IsNullOrEmpty()) { Message.AddErrorMessage(MessageCode.Code1010, model.Id, ResxManage.GetDescription(model.Amount)); }
            else if (!model.Amount.IsNumberString()) { Message.AddErrorMessage(MessageCode.Code1009, model.Id, ResxManage.GetDescription(model.Amount), model.Amount); }
            if (model.TradeChannel.IsNullOrEmpty()) { Message.AddErrorMessage(MessageCode.Code1010, model.Id, ResxManage.GetDescription(model.TradeChannel)); }
            if (model.Channel.IsNullOrEmpty()) { Message.AddErrorMessage(MessageCode.Code1010, model.Id, ResxManage.GetDescription(model.Channel)); }
            if (model.CustomerCode.IsNullOrEmpty()) { Message.AddErrorMessage(MessageCode.Code1010, model.Id, ResxManage.GetDescription(model.CustomerCode)); }
            if (model.Fee.IsNullOrEmpty()) { Message.AddErrorMessage(MessageCode.Code1010, model.Id, ResxManage.GetDescription(model.Fee)); }
            else if (!model.Fee.IsNumberString()) { Message.AddErrorMessage(MessageCode.Code1009, model.Id, ResxManage.GetDescription(model.Fee), model.Fee); }
        }
        public ReceiptBillSet GetReceiptBillSet(ReceiptInfoBillBankModel model)
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
                    RemitDate = tradeDate,
                    PayAmount = model.Amount.ToDecimal(),
                    BankCode = model.CompareCode.Trim(),
                    CompareCode = model.CompareCode.Trim(),
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
    public class BizReceiptInfoBillPOST : BizReceiptInfoBill, IBizReceiptInfoBill<ReceiptInfoBillPostModel>
    {
        private List<ChannelMapModel> ChannelMap;
        public BizReceiptInfoBillPOST(MessageLog message, ApplicationDbContext dataAccess) : base(message, dataAccess) { }
        public void CheckData(ReceiptInfoBillPostModel model)
        {
            ChannelMap = DataAccess.Set<ChannelMapModel>().ToList();
        }
        public ReceiptBillSet GetReceiptBillSet(ReceiptInfoBillPostModel model)
        {
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
                    RemitDate = model.TradeDate.ROCDateToCEDate(),
                    PayAmount = model.Amount.ToDecimal(),
                    BankCode = model.CompareCode,
                    CompareCode = model.CompareCode,
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
    public class BizReceiptInfoBillMARKET : BizReceiptInfoBill, IBizReceiptInfoBill<ReceiptInfoBillMarketModel>
    {
        public BizReceiptInfoBillMARKET(MessageLog message, ApplicationDbContext dataAccess) : base(message, dataAccess) { }
        public void CheckData(ReceiptInfoBillMarketModel model)
        {
        }
        public ReceiptBillSet GetReceiptBillSet(ReceiptInfoBillMarketModel model)
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
                    RemitDate = model.PayDate.ToDateTime(),
                    PayAmount = model.Barcode3.ToDecimal(),
                    BankCode = model.Barcode2,
                    CompareCode = model.Barcode2,
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
    public class BizReceiptInfoBillMARKETSPI : BizReceiptInfoBill, IBizReceiptInfoBill<ReceiptInfoBillMarketSPIModel>
    {
        public BizReceiptInfoBillMARKETSPI(MessageLog message, ApplicationDbContext dataAccess) : base(message, dataAccess) { }
        public void CheckData(ReceiptInfoBillMarketSPIModel model)
        {
        }
        public ReceiptBillSet GetReceiptBillSet(ReceiptInfoBillMarketSPIModel model)
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
                    RemitDate = model.PayDate.ToDateTime(),
                    PayAmount = model.Barcode3_Amount.ToDecimal(),
                    BankCode = model.Barcode3_CompareCode,
                    CompareCode = model.Barcode3_CompareCode,
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
    public class BizReceiptInfoBillFARM : BizReceiptInfoBill, IBizReceiptInfoBill<ReceiptInfoBillFarmModel>
    {
        public BizReceiptInfoBillFARM(MessageLog message, ApplicationDbContext dataAccess) : base(message, dataAccess) { }
        public void CheckData(ReceiptInfoBillFarmModel model)
        {


        }
        public ReceiptBillSet GetReceiptBillSet(ReceiptInfoBillFarmModel model)
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
                    RemitDate = model.PayDate.ToDateTime(),
                    PayAmount = model.Barcode3.ToDecimal(),
                    BankCode = model.Barcode2,
                    CompareCode = model.Barcode2,
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
}
