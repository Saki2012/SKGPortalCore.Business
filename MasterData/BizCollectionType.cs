using System.Collections.Generic;
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
            CheckIsOverlap(message, set.CollectionTypeDetail);
            CheckChannelVerifyPeriod(message, set);
        }
        /// <summary>
        /// 設置資料
        /// </summary>
        /// <param name="set"></param>
        public static void SetData(CollectionTypeSet set)
        {
            SetCollectionTypeDetail(set.CollectionTypeDetail);
            RemoveNotUsedPeriod(set);
        }
        #endregion

        #region Private

        #region  CheckData
        /// <summary>
        /// 檢查收款區間是否重疊
        /// </summary>
        /// <param name="detail"></param>
        /// <returns></returns>
        private static void CheckIsOverlap(SysMessageLog message, List<CollectionTypeDetailModel> detail)
        {
            List<CollectionTypeDetailModel> dt = detail.Where(p => detail.Where(
                    q => p.RowId != q.RowId && p.ChannelId == q.ChannelId &&
                    (p.SRange >= q.SRange && p.SRange <= q.ERange || p.ERange >= q.SRange && p.ERange <= q.ERange)
                    ).Any()).ToList();
            string channelName = LibData.Merge(",", false, dt?.Select(p => p.Channel.ChannelName).Distinct().ToArray());
            if (!channelName.IsNullOrEmpty()) message.AddCustErrorMessage(MessageCode.Code1013, channelName);
        }
        /// <summary>
        /// 檢查是否有通路尚未填寫核銷規則
        /// </summary>
        /// <returns></returns>
        private static void CheckChannelVerifyPeriod(SysMessageLog message, CollectionTypeSet set)
        {
            Dictionary<string, string> detail = set.CollectionTypeDetail.Select(p => p.Channel).Distinct().ToDictionary(p => p.ChannelId, q => q.ChannelName);
            Dictionary<string, CollectionTypeVerifyPeriodModel> period = set.CollectionTypeVerifyPeriod.ToDictionary(p => p.ChannelId, q => q);
            List<string> detailExpectChannels = detail.Keys.Except(period.Keys).ToList();
            string[] notWriteChannels = detail?.Where(p => p.Key.In(detailExpectChannels.ToArray())).Select(p => p.Value).Distinct().ToArray();
            string channelName = LibData.Merge(",", false, notWriteChannels);
            if (!channelName.IsNullOrEmpty()) message.AddCustErrorMessage(MessageCode.Code1014, channelName);
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
                row.ChannelTotalFee = row.ChannelFee + row.ChannelFeedBackFee + row.ChannelRebateFee;
            });
        }
        /// <summary>
        /// 移除核銷週期未在費率明細裡出現過的代收通路項目
        /// </summary>
        /// <param name="set"></param>
        private static void RemoveNotUsedPeriod(CollectionTypeSet set)
        {
            List<string> detail = set.CollectionTypeDetail.Select(p => p.ChannelId).Distinct().ToList();
            Dictionary<string, CollectionTypeVerifyPeriodModel> period = set.CollectionTypeVerifyPeriod.ToDictionary(p => p.ChannelId, q => q);
            List<string> periodExpectChannels = period.Keys.Except(detail).ToList();
            List<CollectionTypeVerifyPeriodModel> removePeriods = period.Where(p => p.Key.In(periodExpectChannels.ToArray())).Select(p => p.Value).ToList();
            removePeriods.ForEach(row => set.CollectionTypeVerifyPeriod.Remove(row));
        }
        #endregion

        #endregion
    }
}
