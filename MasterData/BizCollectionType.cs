﻿using System.Collections.Generic;
using System.Linq;
using SKGPortalCore.Data;
using SKGPortalCore.Lib;
using SKGPortalCore.Model.MasterData;

namespace SKGPortalCore.Repository.SKGPortalCore.Business.MasterData
{
    /// <summary>
    /// 代收類別-商業邏輯
    /// </summary>
    internal static class BizCollectionType
    {
        #region Public
        /// <summary>
        /// 檢查資料
        /// </summary>
        /// <param name="set"></param>
        /// <param name="message"></param>
        public static void CheckData(CollectionTypeSet set, SysMessageLog message)
        {
            if (CheckIsOverlap(set.CollectionTypeDetail, out string channelName)) { message.AddCustErrorMessage(MessageCode.Code1013, channelName); }
        }
        /// <summary>
        /// 設置資料
        /// </summary>
        /// <param name="set"></param>
        public static void SetData(CollectionTypeSet set)
        {
            SetCollectionTypeDetail(set.CollectionTypeDetail);
        }
        #endregion

        #region Private

        #region  CheckData
        /// <summary>
        /// 檢查收款區間是否重疊
        /// </summary>
        /// <param name="detail"></param>
        /// <returns></returns>
        private static bool CheckIsOverlap(List<CollectionTypeDetailModel> detail, out string channelName)
        {
            CollectionTypeDetailModel dt = detail.Where(p => detail.Where(
                     q => p.RowId != q.RowId && p.ChannelId == q.ChannelId &&
                     (p.SRange >= q.SRange && p.SRange <= q.ERange || p.ERange >= q.SRange && p.ERange <= q.ERange)
                     ).Any()).FirstOrDefault();
            channelName = string.Empty; channelName = dt?.Channel.ChannelName;
            return null == dt;
        }
        #endregion

        #region SetDetail
        /// <summary>
        /// 
        /// </summary>
        /// <param name="DataAccess"></param>
        /// <param name="Bill"></param>
        /// <param name="BillReceiptDetail"></param>
        private static void SetCollectionTypeDetail(List<CollectionTypeDetailModel> collectionTypeDetail)
        {
            collectionTypeDetail?.ForEach(row =>
            {
                row.ChannelTotalFee = row.ChannelFee + row.BankFee + row.BankBackFee;
            });
        }
        #endregion

        #endregion
    }
}
