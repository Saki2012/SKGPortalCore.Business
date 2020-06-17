using SKGPortalCore.Core;
using SKGPortalCore.Core.Libary;
using SKGPortalCore.Core.LibEnum;
using SKGPortalCore.Model.MasterData;
using System.Collections.Generic;
using System.Linq;

namespace SKGPortalCore.Repository.SKGPortalCore.Business.MasterData
{
    internal static class BizBizCustomer
    {
        #region Public
        /// <summary>
        /// 檢查資料
        /// </summary>
        /// <param name="message"></param>
        /// <param name="set"></param>
        public static void CheckData(SysMessageLog message, BizCustomerSet set)
        {
            CheckVirtualAccountLength(message, set.BizCustomer);
            CheckBizCustType(message, set);
            CheckIntroduceType(message, set.BizCustomerFeeDetail);
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
                if (row.ChannelGroupType == ChannelGroupType.Market) marketEnable = true;
                if (row.ChannelGroupType == ChannelGroupType.Post) postEnable = true;
                ResetRowPercent(row);
            });
            set.BizCustomer.MarketEnable = marketEnable;
            set.BizCustomer.PostEnable = postEnable;
        }
        #endregion

        #region Private
        #region CheckData
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
            if (set.BizCustomerFeeDetail.Any(p => p.BankFeeType == BankFeeType.Hitrust_ClearFee_CurMonth || p.BankFeeType == BankFeeType.Hitrust_ClearFee_NextMonth))
            {
                if (set.BizCustomer.IntroCustomerCode.IsNullOrEmpty() || set.BizCustomer.IntroCustomer.BizCustType != BizCustType.Hitrust)
                    message.AddCustErrorMessage(MessageCode.Code1019, ResxManage.GetDescription(BizCustType.Hitrust));
            }
            if (set.BizCustomerFeeDetail.Any(p => p.BankFeeType == BankFeeType.TotalFee && p.IntroPercent > 0m))
            {
                if (set.BizCustomer.IntroCustomerCode.IsNullOrEmpty() || set.BizCustomer.IntroCustomer.BizCustType != BizCustType.Introducer)
                    message.AddCustErrorMessage(MessageCode.Code1019, ResxManage.GetDescription(BizCustType.Introducer));
            }
        }
        /// <summary>
        /// 檢查介紹商與Hitrust商是否衝突
        /// </summary>
        /// <param name="message"></param>
        /// <param name="detail"></param>
        private static void CheckIntroduceType(SysMessageLog message, List<BizCustomerFeeDetailModel> detail)
        {
            bool introducer = false, hitrust = false;
            detail.ForEach(p =>
            {
                if (p.BankFeeType == BankFeeType.TotalFee) introducer = true;
                if (p.BankFeeType == BankFeeType.Hitrust_ClearFee_CurMonth || p.BankFeeType == BankFeeType.Hitrust_ClearFee_NextMonth) hitrust = true;
            });
            if (introducer && hitrust)
                message.AddCustErrorMessage(MessageCode.Code1018);
        }

        #endregion
        #region SetData
        /// <summary>
        /// 若銀行手續費類型不為每筆總手續費時，分潤%設置為0
        /// </summary>
        /// <param name="row"></param>
        private static void ResetRowPercent(BizCustomerFeeDetailModel row)
        {
            if (row.BankFeeType != BankFeeType.TotalFee) row.IntroPercent = 0;
        }
        #endregion
        #endregion
    }
}
