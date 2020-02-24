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
                if (!bizCustFeeDetail.FeeType.In(FeeType.ClearFee, FeeType.TotalFee, FeeType.HitrustFee))
                {
                    set.BizCustomerFeeDetail.Remove(bizCustFeeDetail);
                }
            }

        }

        public static void SetData(BizCustomerSet set)
        {
            SetVirtualAccountLen(set.BizCustomer);
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
            return;
            int len = bizCustomer.CustomerCode.Length;
            if (bizCustomer.VirtualAccount1 != VirtualAccount1.Empty) len += bizCustomer.BillTermLen;
            if (bizCustomer.VirtualAccount2 != VirtualAccount2.Empty) len += bizCustomer.PayerNoLen;
            if (bizCustomer.VirtualAccount3.In(VirtualAccount3.SeqPayEndDate, VirtualAccount3.SeqAmountPayEndDate)) len += 4;
            if (bizCustomer.VirtualAccount3 != VirtualAccount3.NoverifyCode) len += 1;
            //if (bizCustomer.VirtualAccountLen != len) { message.AddCustErrorMessage(MessageCode.Code1007, bizCustomer.VirtualAccountLen, len); }
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="bizCustomerModel"></param>
        private static void SetVirtualAccountLen(BizCustomerModel bizCustomerModel)
        {
            switch (bizCustomerModel.CustomerCode.Length)
            {
                case 3:
                    bizCustomerModel.VirtualAccountLen = VirtualAccountLen.Len14;
                    break;
                case 4:
                    bizCustomerModel.VirtualAccountLen = VirtualAccountLen.Len16;
                    break;
                case 6:
                    bizCustomerModel.VirtualAccountLen = VirtualAccountLen.Len13;
                    break;
            }
        }
        #endregion
    }
}
