using SKGPortalCore.Data;
using SKGPortalCore.Lib;
using SKGPortalCore.Model;
using SKGPortalCore.Model.MasterData;
using SKGPortalCore.Model.MasterData.OperateSystem;

namespace SKGPortalCore.Business.MasterData
{
    public class BizBizCustomer : BizBase
    {
        #region Construct
        public BizBizCustomer(MessageLog message, ApplicationDbContext dataAccess = null, IUserModel user = null) : base(message, dataAccess, user) { }
        #endregion

        #region Public
        public void CheckData(BizCustomerSet set)
        {
            foreach (BizCustomerFeeDetailModel bizCustFeeDetail in set.BizCustomerFeeDetail)
            {
                if (!bizCustFeeDetail.FeeType.In(FeeType.ClearFee, FeeType.TotalFee, FeeType.HitrustFee))
                {
                    set.BizCustomerFeeDetail.Remove(bizCustFeeDetail);
                }
            }
        }
        #endregion

        #region Private

        #endregion
    }
}
