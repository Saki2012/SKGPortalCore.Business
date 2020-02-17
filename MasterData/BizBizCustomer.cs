using SKGPortalCore.Data;
using SKGPortalCore.Lib;
using SKGPortalCore.Model.Enum;
using SKGPortalCore.Model.MasterData;

namespace SKGPortalCore.Repository.SKGPortalCore.Business.MasterData
{
    internal static class BizBizCustomer
    {
        #region Public
        public static void CheckData(SysMessageLog message, BizCustomerSet set)
        {
            CheckVirtualAccountLength(message, set.BizCustomer);
            foreach (BizCustomerFeeDetailModel bizCustFeeDetail in set.BizCustomerFeeDetail)
            {
                if (!bizCustFeeDetail.FeeType.In(FeeType.ClearFee, FeeType.TotalFee, FeeType.IntroducerFee))
                {
                    set.BizCustomerFeeDetail.Remove(bizCustFeeDetail);
                }
            }

        }
        #endregion

        #region Private
        /// <summary>
        /// 檢查銷帳編號長度
        /// </summary>
        /// <param name="message"></param>
        /// <param name="bizCustomer"></param>
        private static void CheckVirtualAccountLength(SysMessageLog message, BizCustomerModel bizCustomer)
        {
            int len = bizCustomer.CustomerCode.Length;
            if (bizCustomer.VirtualAccount1 != VirtualAccount1.Empty) len += bizCustomer.Customer.BillTermLen;
            if (bizCustomer.VirtualAccount2 != VirtualAccount2.Empty) len += bizCustomer.Customer.PayerNoLen;
            if (bizCustomer.VirtualAccount3.In(VirtualAccount3.SeqPayEndDate, VirtualAccount3.SeqAmountPayEndDate)) len += 4;
            if (bizCustomer.VirtualAccount3 != VirtualAccount3.NoverifyCode) len += 1;
            if (bizCustomer.VirtualAccountLen != len) { message.AddCustErrorMessage(MessageCode.Code1007, bizCustomer.VirtualAccountLen, len); }
        }
        #endregion
    }
}
