﻿using SKGPortalCore.Lib;
using SKGPortalCore.Model.MasterData;

namespace SKGPortalCore.Business.MasterData
{
    public class BizBizCustomer
    {
        #region Public
        public void LoopAction(BizCustomerSet set)
        {

            foreach (BizCustomerFeeDetailModel  bizCustFeeDetail in set.BizCustomerFeeDetail)
            {

            }
        }
        #endregion

        #region Private
        private void CheckChannel(string channelIds)
        {
            if (channelIds.IsNullOrEmpty()) return;
            channelIds.Split(',');
        }

        private void CheckCollectionType(string collectionTypeIds)
        {
            if (collectionTypeIds.IsNullOrEmpty()) return;

        }
        #endregion
    }
}
