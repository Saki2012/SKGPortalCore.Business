using SKGPortalCore.Data;
using SKGPortalCore.Lib;
using SKGPortalCore.Model.Enum;
using SKGPortalCore.Model.MasterData;
using System.Linq;

namespace SKGPortalCore.Repository.SKGPortalCore.Business.MasterData
{
    internal static class BizBizCustomer
    {
        #region Public
        public static void CheckData(SysMessageLog message, BizCustomerSet set)
        {
            if (!CheckVirtualAccountLength(message, set.BizCustomer, out int len)) { message.AddCustErrorMessage(MessageCode.Code1007, (int)set.BizCustomer.VirtualAccountLen, len); }
            CheckBizCustType(set);
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
        private static bool CheckVirtualAccountLength(SysMessageLog message, BizCustomerModel bizCustomer, out int len)
        {
            len = bizCustomer.CustomerCode.Length;
            return true;
            if (bizCustomer.VirtualAccount1 != VirtualAccount1.Empty) len += bizCustomer.BillTermLen;
            if (bizCustomer.VirtualAccount2 != VirtualAccount2.Empty) len += bizCustomer.PayerNoLen;
            if (bizCustomer.VirtualAccount3.In(VirtualAccount3.SeqPayEndDate, VirtualAccount3.SeqAmountPayEndDate)) len += 4;
            if (bizCustomer.VirtualAccount3 != VirtualAccount3.NoverifyCode) len += 1;
            return (int)bizCustomer.VirtualAccountLen != len;
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
        /// <summary>
        /// 檢查介紹商企業代號是否有選擇
        /// </summary>
        /// <param name="set"></param>
        private static void CheckBizCustType(BizCustomerSet set)
        {
            if (set.BizCustomerFeeDetail.Any(p => p.ChannelType == ChannelGroupType.Hitrust))
            {
                if (set.BizCustomer.IntroCustomer.BizCustType != BizCustType.Hitrust) return;//error
            }
            if (set.BizCustomerFeeDetail.Any(p => p.BankFeeType == BankFeeType.TotalFee && p.Percent > 0m))
            {
                if (set.BizCustomer.IntroCustomer.BizCustType != BizCustType.Introducer) return;//error
            }
        }
        #endregion
    }
}
