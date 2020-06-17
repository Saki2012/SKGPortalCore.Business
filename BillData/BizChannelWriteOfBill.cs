using System;
using System.Collections.Generic;
using SKGPortalCore.Core.Libary;
using SKGPortalCore.Model.BillData;

namespace SKGPortalCore.Repository.SKGPortalCore.Business.BillData
{
    /// <summary>
    /// 通路帳款核銷單-商業邏輯
    /// </summary>
    internal static class BizChannelWriteOfBill
    {
        #region Public
        public static void CheckData(ChannelWriteOfBillSet set)
        {
            if (CompareData(set.ChannelWriteOfDetail, set.CashFlowWriteOfDetail) != 0m) {/*核銷金額不一致，請確認！*/ }
        }
        #endregion

        #region Private
        private static decimal CompareData(List<ChannelWriteOfDetailModel> channelWriteOfDetail, List<CashFlowWriteOfDetailModel> cashFlowWriteOfDetail)
        {
            decimal val = 0m;
            RecComparison<ChannelWriteOfDetailModel, CashFlowWriteOfDetailModel> rc = new RecComparison<ChannelWriteOfDetailModel, CashFlowWriteOfDetailModel>(channelWriteOfDetail, cashFlowWriteOfDetail);
            rc.Master.Sort(new Comparison<ChannelWriteOfDetailModel>((x, y) =>
          {
              int result = x.ChannelEAccountBill.ChannelId.CompareTo(y.ChannelEAccountBill.ChannelId);
              if (result == 0)
              {
                  result = x.ChannelEAccountBill.CollectionTypeId.CompareTo(y.ChannelEAccountBill.CollectionTypeId);
              }
              else
              {
                  return result;
              }
              return result;
          }));
            rc.Detail.Sort(new Comparison<CashFlowWriteOfDetailModel>((x, y) =>
           {
               int result = x.CashFlowBill.ChannelId.CompareTo(y.CashFlowBill.ChannelId);
               if (result == 0)
               {
                   result = x.CashFlowBill.CollectionTypeId.CompareTo(y.CashFlowBill.CollectionTypeId);
               }
               else
               {
                   return result;
               }
               return result;
           }));
            rc.CompareFunc = new Func<ChannelWriteOfDetailModel, CashFlowWriteOfDetailModel, int>((x, y) =>
             {
                 int result = x.ChannelEAccountBill.ChannelId.CompareTo(y.CashFlowBill.ChannelId);
                 if (result == 0)
                 {
                     result = x.ChannelEAccountBill.CollectionTypeId.CompareTo(y.CashFlowBill.CollectionTypeId);
                 }
                 else
                 {
                     return result;
                 }
                 return result;
             });

            if (rc.Enable)
            {
                while (!rc.IsEof)
                {
                    rc.BackToBookMark();
                    val += rc.CurrentRow.ChannelEAccountBill.ExpectRemitAmount;
                    while (rc.Compare())
                    {
                        rc.SetBookMark();
                        val -= rc.DetailRow.CashFlowBill.BillNo.ToDecimal();
                        rc.DetailMoveNext();
                    }
                    rc.MoveNext();
                }
            }

            return val;
        }
        #endregion
    }
}
