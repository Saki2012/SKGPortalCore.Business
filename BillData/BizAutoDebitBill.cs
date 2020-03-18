using SKGPortalCore.Data;
using SKGPortalCore.Lib;
using SKGPortalCore.Model.BillData;
using SKGPortalCore.Model.MasterData;
using SKGPortalCore.Model.System;
using SKGPortalCore.Repository.SKGPortalCore.Business.Func;
using System;
using System.Collections.Generic;
using System.Text;

namespace SKGPortalCore.Repository.SKGPortalCore.Business.BillData
{
    /// <summary>
    /// 約定扣款單-商業邏輯
    /// </summary>
    internal static class BizAutoDebitBill
    {
        #region Public
        /// <summary>
        /// 檢查資料
        /// </summary>
        /// <param name="set"></param>
        public static void CheckData(AutoDebitBillSet set, SysMessageLog message, ApplicationDbContext dataAccess)
        {
            if (BizVirtualAccountCode.CheckBankCodeExist(dataAccess, set.AutoDebitBill.VirtualAccountCode, out _))
            { message.AddCustErrorMessage(MessageCode.Code1008, ResxManage.GetDescription<AutoDebitBillModel>(p => p.VirtualAccountCode), set.AutoDebitBill.VirtualAccountCode); }
            CheckPayerType(message, set.AutoDebitBill.Payer);
        }
        /// <summary>
        /// 設置資料
        /// </summary>
        /// <param name="set"></param>
        /// <param name="action"></param>
        public static void SetData(AutoDebitBillSet set,string progId,ApplicationDbContext dataAccess)
        {
            BizVirtualAccountCode.AddVirtualAccountCode(dataAccess, progId, set.AutoDebitBill.BillNo, set.AutoDebitBill.VirtualAccountCode);
            SetBankCode(set.AutoDebitBill);
        }
        #endregion

        #region Private

        #region  CheckData
        /// <summary>
        /// 檢查繳款人是否為約定扣款帳號
        /// </summary>
        /// <param name="message"></param>
        /// <param name="payer"></param>
        private static void CheckPayerType(SysMessageLog message, PayerModel payer)
        {
            if (payer.PayerType != PayerType.Account)
                message.AddCustErrorMessage(MessageCode.Code1017, payer.PayerName, ResxManage.GetDescription(PayerType.Account));
        }
        #endregion

        #region SetData
        /// <summary>
        /// 獲取銀行銷帳編號
        /// </summary>
        /// <param name="bill"></param>
        /// <returns></returns>
        private static void SetBankCode(AutoDebitBillModel bill)
        {
            string vir1 = string.Empty, vir2 = string.Empty, vir3 = string.Empty;
            //if (null == bill.BizCustomer)
            //{
            //    bill.VirtualAccountCode = string.Empty;
            //    return;
            //}
            //switch (bill.BizCustomer.VirtualAccount1)
            //{
            //    case VirtualAccount1.BillTerm:
            //        vir1 = bill.BillTerm.BillTermNo.PadLeft(bill.BizCustomer.BillTermLen, '0');
            //        break;
            //}
            //switch (bill.BizCustomer.VirtualAccount2)
            //{
            //    case VirtualAccount2.PayerNo:
            //        vir2 = bill.Payer.PayerNo.PadLeft(bill.BizCustomer.PayerNoLen, '0');
            //        break;
            //    case VirtualAccount2.Seq:
            //        break;
            //}
            //switch (bill.BizCustomer.VirtualAccount3)
            //{
            //    case VirtualAccount3.SeqPayEndDate:
            //    case VirtualAccount3.SeqAmountPayEndDate:
            //        //西元年末碼(1碼)+天數(3碼)
            //        vir3 = bill.PayEndDate.Year.ToString().Substring(3, 1) + bill.PayEndDate.DayOfYear.ToString().PadLeft(3, '0');
            //        break;
            //}
            string result = $"{bill.CustomerCode}{vir3}{vir1}{vir2}";
            bill.VirtualAccountCode = $"{result}{BizVirtualAccountCode.GetVirtualCheckCode(bill.BizCustomer.VirtualAccount3, result, bill.PayAmount)}".PadLeft(16, '0');
        }
        #endregion

        #endregion
    }
}
