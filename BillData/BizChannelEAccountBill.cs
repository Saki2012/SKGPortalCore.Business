﻿using SKGPortalCore.Core.DB;
using SKGPortalCore.Core.LibEnum;
using SKGPortalCore.Model.BillData;

namespace SKGPortalCore.Repository.SKGPortalCore.Business.BillData
{
    internal static class BizChannelEAccountBill
    {
        #region Public
        /// <summary>
        /// 設置欄位
        /// </summary>
        /// <param name="set"></param>
        /// <param name="action"></param>
        public static void SetData(ChannelEAccountBillSet set)
        {
            int count = 0;
            decimal payAmount = 0, channelFee = 0;
            set.ChannelEAccountBillDetail.ForEach(row =>
            {
                payAmount += row.ReceiptBill.PayAmount;
                channelFee += row.ReceiptBill.ChargePayType.Equals(ChargePayType.Deduction) ? row.ReceiptBill.ChannelFee : 0m;
                count++;
            });
            set.ChannelEAccountBill.Amount = payAmount;
            set.ChannelEAccountBill.ChannelFee = channelFee;
            set.ChannelEAccountBill.ExpectRemitAmount = payAmount - channelFee;
            set.ChannelEAccountBill.TotalCount = count;
        }
        /// <summary>
        /// 過帳資料
        /// </summary>
        public static void PostingData(ChannelEAccountBillSet set, ApplicationDbContext dataAccess)
        {
        }
        #endregion

        #region Private

        #region SetData

        #endregion

        #region PostingData

        #endregion

        #endregion
    }
}
