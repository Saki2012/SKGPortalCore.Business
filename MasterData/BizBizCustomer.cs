using SKGPortalCore.Lib;
using SKGPortalCore.Model;
using SKGPortalCore.Model.MasterData;

namespace SKGPortalCore.Repository.SKGPortalCore.Business.MasterData
{
    internal static class BizBizCustomer
    {
        #region Public
        public static void CheckData(BizCustomerSet set)
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
