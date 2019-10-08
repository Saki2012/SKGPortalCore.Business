using System.Collections.Generic;
using System.Linq;
using SKGPortalCore.Data;
using SKGPortalCore.Model.MasterData;
using SKGPortalCore.Model.MasterData.OperateSystem;

namespace SKGPortalCore.Business.MasterData
{
    /// <summary>
    /// 代收類別-商業邏輯
    /// </summary>
    public class BizCollectionType : BizBase
    {
        #region Construct
        public BizCollectionType(MessageLog message, ApplicationDbContext dataAccess = null, IUserModel user = null) : base(message, dataAccess, user) { }
        #endregion

        #region Public
        public void CheckData(CollectionTypeSet set)
        {

            if (CheckIsOverlap(set.CollectionTypeDetail))
            {
                return;//報錯 重複
            }

            foreach (CollectionTypeDetailModel detail in set.CollectionTypeDetail)
            {
                CheckSERange(detail);
            }
        }
        #endregion

        #region Private
        /// <summary>
        /// 檢查收款區間(起)是否大於(迄)
        /// </summary>
        /// <param name="detail"></param>
        /// <returns></returns>
        private bool CheckSERange(CollectionTypeDetailModel detail)
        {
            return detail.SRange > detail.ERange;
        }
        /// <summary>
        /// 檢查收款區間是否重疊
        /// </summary>
        /// <param name="detail"></param>
        /// <returns></returns>
        private bool CheckIsOverlap(List<CollectionTypeDetailModel> detail)
        {
            return detail.Where(p => detail.Where(
                    q => p.RowId != q.RowId && (p.SRange >= q.SRange && p.SRange <= q.ERange
                    || p.ERange >= q.SRange && p.ERange <= q.ERange)).Any()).Count() > 0;
        }
        #endregion
    }
}
