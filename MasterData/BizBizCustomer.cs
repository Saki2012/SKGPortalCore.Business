using SKGPortalCore.Data;
using SKGPortalCore.Lib;
using SKGPortalCore.Model.MasterData;
using SKGPortalCore.Model.System;
using System.Linq;

namespace SKGPortalCore.Repository.SKGPortalCore.Business.MasterData
{
    internal static class BizBizCustomer
    {
        #region Public
        public static void CheckData(SysMessageLog message, BizCustomerSet set)
        {
            CheckVirtualAccountLength(message, set.BizCustomer);
            CheckBizCustType(message, set);
        }
        /// <summary>
        /// 設置資料
        /// </summary>
        /// <param name="set"></param>
        public static void SetData(BizCustomerSet set)
        {
            bool marketEnable = false, postEnable = false;
            set.BizCustomerFeeDetail.ForEach(row =>
            {
                if (row.ChannelType == ChannelGroupType.Market) marketEnable = true;
                if (row.ChannelType == ChannelGroupType.Post) postEnable = true;
                ResetRowPercent(row);
            });
            set.BizCustomer.MarketEnable = marketEnable;
            set.BizCustomer.PostEnable = postEnable;
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
            if (bizCustomer.VirtualAccount1 != VirtualAccount1.Empty) len += bizCustomer.BillTermLen;
            if (bizCustomer.VirtualAccount2 != VirtualAccount2.Empty) len += bizCustomer.PayerNoLen;
            if (bizCustomer.VirtualAccount3.In(VirtualAccount3.SeqPayEndDate, VirtualAccount3.SeqAmountPayEndDate)) len += 4;
            if (bizCustomer.VirtualAccount3 != VirtualAccount3.NoverifyCode) len += 1;
            if ((int)bizCustomer.VirtualAccountLen != len)
                message.AddCustErrorMessage(MessageCode.Code1007, (int)bizCustomer.VirtualAccountLen, len);
        }
        /// <summary>
        /// 檢查介紹商企業代號是否有選擇
        /// </summary>
        /// <param name="set"></param>
        private static void CheckBizCustType(SysMessageLog message, BizCustomerSet set)
        {
            if (set.BizCustomerFeeDetail.Any(p => p.ChannelType == ChannelGroupType.Hitrust))
            {
                if (set.BizCustomer.IntroCustomer.BizCustType != BizCustType.Hitrust || set.BizCustomer.IntroCustomerCode.IsNullOrEmpty())
                    message.AddCustErrorMessage(MessageCode.Code0001, ResxManage.GetDescription(set.BizCustomer.IntroCustomerCode));
            }
            if (set.BizCustomerFeeDetail.Any(p => p.BankFeeType == BankFeeType.TotalFee && p.Percent > 0m))
            {
                if (set.BizCustomer.IntroCustomer.BizCustType != BizCustType.Introducer || set.BizCustomer.IntroCustomerCode.IsNullOrEmpty())
                    message.AddCustErrorMessage(MessageCode.Code0001, ResxManage.GetDescription(set.BizCustomer.IntroCustomerCode));
            }
        }
        /// <summary>
        /// 若銀行手續費類型不為每筆總手續費時，分潤%設置為0
        /// </summary>
        /// <param name="row"></param>
        private static void ResetRowPercent(BizCustomerFeeDetailModel row)
        {
            if (row.BankFeeType != BankFeeType.TotalFee) row.Percent = 0;
        }
        #endregion
    }
}
