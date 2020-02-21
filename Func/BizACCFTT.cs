using System.Collections.Generic;
using System.Linq;
using SKGPortalCore.Lib;
using SKGPortalCore.Model.Enum;
using SKGPortalCore.Model.MasterData;
using SKGPortalCore.Model.MasterData.OperateSystem;
using SKGPortalCore.Model.SourceData;

namespace SKGPortalCore.Repository.SKGPortalCore.Business.Func
{
    public static class BizACCFTT
    {
        #region Public
        /// <summary>
        /// 服務申請書對應客戶資料
        /// </summary>
        /// <param name="accftt"></param>
        public static CustomerSet SetCustomer(ACCFTT accftt, CustomerSet customerSet)
        {
            if (null == customerSet) customerSet = new CustomerSet();
            if (null == customerSet.Customer) customerSet.Customer = new CustomerModel();

            string custId = accftt.IDCODE.TrimStart('0');
            customerSet.Customer.CustomerId = custId;
            customerSet.Customer.CustomerName = accftt.CUSTNAME;
            customerSet.Customer.DeptId = accftt.APPBECODE;
            //customerSet.Customer.BillTermLen = 3;//默認三碼
            //customerSet.Customer.PayerNoLen = 6;//默認六碼
            customerSet.Customer.IsSysCust = false;
            return customerSet;
        }
        /// <summary>
        /// 服務申請書對應商戶資料
        /// </summary>
        /// <param name="accftt"></param>
        public static BizCustomerSet SetBizCustomer(ACCFTT accftt, BizCustomerSet bizCustomerSet)
        {
            if (null == bizCustomerSet)
            {
                bizCustomerSet = new BizCustomerSet();
            }

            if (null == bizCustomerSet.BizCustomer)
            {
                bizCustomerSet.BizCustomer = new BizCustomerModel();
            }

            if (null == bizCustomerSet.BizCustomerFeeDetail)
            {
                bizCustomerSet.BizCustomerFeeDetail = new List<BizCustomerFeeDetailModel>();
            }

            string custId = accftt.IDCODE.TrimStart('0'); string custCode = accftt.KEYNO.Trim();
            bizCustomerSet.BizCustomer.CustomerId = custId;
            bizCustomerSet.BizCustomer.CustomerCode = custCode;
            bizCustomerSet.BizCustomer.AccountDeptId = accftt.BRCODE;
            bizCustomerSet.BizCustomer.RealAccount = accftt.ACCIDNO;
            bizCustomerSet.BizCustomer.VirtualAccountLen = (10 + custCode.Length).ToByte();
            bizCustomerSet.BizCustomer.ChannelIds = GetChannel(accftt);
            bizCustomerSet.BizCustomer.CollectionTypeIds = GetCollectionType(accftt);
#if DEBUG
            bizCustomerSet.BizCustomer.VirtualAccount1 = VirtualAccount1.BillTerm;
            bizCustomerSet.BizCustomer.VirtualAccount2 = VirtualAccount2.PayerNo;
#endif
            bizCustomerSet.BizCustomer.VirtualAccount3 = GetVirtualAccount3(accftt);
            bizCustomerSet.BizCustomer.AccountStatus = AccountStatus.Unable;
            bizCustomerSet.BizCustomer.EntrustCustId = accftt.CUSTID;
            bizCustomerSet.BizCustomer.ImportBatchNo = accftt.ImportBatchNo;
            bizCustomerSet.BizCustomer.Source = accftt.Src;

            int rowId = bizCustomerSet.BizCustomerFeeDetail.OrderBy(p => -p.RowId).Select(p => p.RowId).FirstOrDefault();
            bizCustomerSet.BizCustomerFeeDetail.ForEach(p => p.RowState = RowState.Delete);
            if (!accftt.ACTFEE.ToInt32().IsNullOrEmpty())
            {
                bizCustomerSet.BizCustomerFeeDetail.Add(new BizCustomerFeeDetailModel()
                {
                    CustomerCode = custCode,
                    RowId = ++rowId,
                    RowState = RowState.Insert,
                    ChannelType = ChannelGroupType.Market,
                    FeeType = FeeType.ClearFee,
                    Fee = accftt.ACTFEE.ToDecimal(),
                    Percent = 0m
                });
            }

            if (!accftt.ACTFEEPT.ToInt32().IsNullOrEmpty())
            {
                bizCustomerSet.BizCustomerFeeDetail.Add(new BizCustomerFeeDetailModel()
                {
                    CustomerCode = custCode,
                    RowId = ++rowId,
                    RowState = RowState.Insert,
                    ChannelType = ChannelGroupType.Post,
                    FeeType = FeeType.ClearFee,
                    Fee = accftt.ACTFEEPT.ToDecimal(),
                    Percent = 0m,
                });
            }

            if (!accftt.HIFLAG.IsNullOrEmpty())
            {
                bizCustomerSet.BizCustomer.HiTrustFlag = (HiTrustFlag)accftt.HIFLAG.ToByte();
            }

            if (!accftt.HIFARE.IsNullOrEmpty())
            {
                bizCustomerSet.BizCustomerFeeDetail.Add(new BizCustomerFeeDetailModel()
                {
                    CustomerCode = custCode,
                    RowId = ++rowId,
                    RowState = RowState.Insert,
                    //ChannelType = ChannelGroupType.HiTrust,
                    FeeType = FeeType.IntroducerFee,
                    Fee = accftt.HIFARE.ToDecimal(),
                    Percent = 0m,
                });
            }
            //銀行-每筆總手續費
            if (!accftt.ACTFEEBEFT.IsNullOrEmpty())
            {
                bizCustomerSet.BizCustomerFeeDetail.Add(new BizCustomerFeeDetailModel()
                {
                    CustomerCode = custCode,
                    RowId = ++rowId,
                    RowState = RowState.Insert,
                    ChannelType = ChannelGroupType.Bank,
                    FeeType = FeeType.TotalFee,
                    Fee = accftt.ACTFEEBEFT.ToDecimal(),
                    Percent = accftt.SHAREBEFTPERCENT.ToDecimal(),
                });
            }
            //超商-每筆總手續費
            if (!accftt.ACTFEEMART.IsNullOrEmpty())
            {
                bizCustomerSet.BizCustomerFeeDetail.Add(new BizCustomerFeeDetailModel()
                {
                    CustomerCode = custCode,
                    RowId = ++rowId,
                    RowState = RowState.Insert,
                    ChannelType = ChannelGroupType.Market,
                    FeeType = FeeType.TotalFee,
                    Fee = accftt.ACTFEEMART.ToDecimal(),
                    Percent = accftt.ACTPERCENT.ToDecimal(),
                });
            }
            //郵局-每筆總手續費
            if (!accftt.ACTFEEPOST.IsNullOrEmpty())
            {
                bizCustomerSet.BizCustomerFeeDetail.Add(new BizCustomerFeeDetailModel()
                {
                    CustomerCode = custCode,
                    RowId = ++rowId,
                    RowState = RowState.Insert,
                    ChannelType = ChannelGroupType.Post,
                    FeeType = FeeType.TotalFee,
                    Fee = accftt.ACTFEEPOST.ToDecimal(),
                    Percent = accftt.POSTPERCENT.ToDecimal(),
                });
            }
            //農金-清算手續費
            if (!accftt.AGRIFEE.IsNullOrEmpty())
            {
                bizCustomerSet.BizCustomerFeeDetail.Add(new BizCustomerFeeDetailModel()
                {
                    CustomerCode = custCode,
                    RowId = ++rowId,
                    RowState = RowState.Insert,
                    ChannelType = ChannelGroupType.Market,
                    FeeType = FeeType.ClearFee,
                    Fee = accftt.AGRIFEE.ToDecimal(),
                    Percent = 0,
                });
            }

            return bizCustomerSet;
        }
        /// <summary>
        /// 新增登入帳戶
        /// </summary>
        /// <param name="accftt"></param>
        /// <returns></returns>
        public static CustUserSet AddAdminAccount(ACCFTT accftt)
        {
            return new CustUserSet()
            {
                User = new CustUserModel()
                {
                    KeyId = $"{accftt.IDCODE.TrimStart('0')},admin",
                    CustomerId = accftt.IDCODE.TrimStart('0'),
                    UserId = "admin",
                    UserName = "管理員",
                    AccountStatus = AccountStatus.Unable,
                },
                UserRoles = new List<CustUserRoleModel>()
                {
                    new CustUserRoleModel() { KeyId = $"{accftt.IDCODE.TrimStart('0')},admin", RoleId = "FrontEndAdmin" }
                }
            };
        }
        #endregion

        #region Private
        /// <summary>
        /// 獲取啟用通路
        /// </summary>
        /// <param name="accftt"></param>
        /// <returns></returns>
        private static string GetChannel(ACCFTT accftt)
        {
            List<string> channels = new List<string>();
            switch (accftt.CHANNEL)
            {
                case "9":
                    {
                        channels.Add(ChannelValue.Cash);
                        channels.Add(ChannelValue.ATM);
                        channels.Add(ChannelValue.Remit);
                        channels.Add(ChannelValue.ACH);
                        channels.Add(ChannelValue.ACHForPay);
                        channels.Add(ChannelValue.CTBC);
                        if (accftt.POSTFLAG == "1")
                        {
                            channels.Add(ChannelValue.Post);
                        }

                        if (accftt.RSTORE1 == "1")
                        {
                            channels.Add(ChannelValue.Mart711);
                        }

                        if (accftt.RSTORE2 == "1")
                        {
                            channels.Add(ChannelValue.MartFamily);
                        }

                        if (accftt.RSTORE3 == "1")
                        {
                            channels.Add(ChannelValue.MartOK);
                        }

                        if (accftt.RSTORE4 == "1")
                        {
                            channels.Add(ChannelValue.MartLIFE);
                        }
                    }
                    break;
                case "0":
                    {
                        channels.Add(ChannelValue.Cash);
                        channels.Add(ChannelValue.Remit);
                        channels.Add(ChannelValue.ACH);
                        channels.Add(ChannelValue.ACHForPay);
                        channels.Add(ChannelValue.CTBC);
                        if (accftt.POSTFLAG == "1")
                        {
                            channels.Add(ChannelValue.Post);
                        }

                        if (accftt.RSTORE1 == "1")
                        {
                            channels.Add(ChannelValue.Mart711);
                        }

                        if (accftt.RSTORE2 == "1")
                        {
                            channels.Add(ChannelValue.MartFamily);
                        }

                        if (accftt.RSTORE3 == "1")
                        {
                            channels.Add(ChannelValue.MartOK);
                        }

                        if (accftt.RSTORE4 == "1")
                        {
                            channels.Add(ChannelValue.MartLIFE);
                        }
                    }
                    break;
                case "1":
                    {
                        channels.Add(ChannelValue.ATM);
                        channels.Add(ChannelValue.Remit);
                        channels.Add(ChannelValue.ACH);
                        channels.Add(ChannelValue.ACHForPay);
                        channels.Add(ChannelValue.CTBC);
                        if (accftt.POSTFLAG == "1")
                        {
                            channels.Add(ChannelValue.Post);
                        }

                        if (accftt.RSTORE1 == "1")
                        {
                            channels.Add(ChannelValue.Mart711);
                        }

                        if (accftt.RSTORE2 == "1")
                        {
                            channels.Add(ChannelValue.MartFamily);
                        }

                        if (accftt.RSTORE3 == "1")
                        {
                            channels.Add(ChannelValue.MartOK);
                        }

                        if (accftt.RSTORE4 == "1")
                        {
                            channels.Add(ChannelValue.MartLIFE);
                        }
                    }
                    break;
                case "2":
                    {
                        channels.Add(ChannelValue.Cash);
                        if (accftt.RSTORE1 == "1")
                        {
                            channels.Add(ChannelValue.Mart711);
                        }

                        if (accftt.RSTORE2 == "1")
                        {
                            channels.Add(ChannelValue.MartFamily);
                        }

                        if (accftt.RSTORE3 == "1")
                        {
                            channels.Add(ChannelValue.MartOK);
                        }

                        if (accftt.RSTORE4 == "1")
                        {
                            channels.Add(ChannelValue.MartLIFE);
                        }
                    }
                    break;
                case "3":
                    {
                        channels.Add(ChannelValue.Remit);
                    }
                    break;
                case "4":
                    {
                        if (accftt.POSTFLAG == "1")
                        {
                            channels.Add(ChannelValue.Post);
                        }

                        if (accftt.RSTORE1 == "1")
                        {
                            channels.Add(ChannelValue.Mart711);
                        }

                        if (accftt.RSTORE2 == "1")
                        {
                            channels.Add(ChannelValue.MartFamily);
                        }

                        if (accftt.RSTORE3 == "1")
                        {
                            channels.Add(ChannelValue.MartOK);
                        }

                        if (accftt.RSTORE4 == "1")
                        {
                            channels.Add(ChannelValue.MartLIFE);
                        }
                    }
                    break;
            }
            if (accftt.AUTOFLAG == "1")
            {
                channels.Add(ChannelValue.Deduct_Server);
            }

            channels.Add(ChannelValue.Deduct_Client);
            if (accftt.EBFLAG == "1")
            {
                channels.Add(ChannelValue.EB);
            }

            if (accftt.CTBCFLAG == "1")
            {
                channels.Add(ChannelValue.CTBC);
            }

            if (accftt.AGRIFLAG == "1")
            {
                channels.Add(ChannelValue.Farm);
            }

            channels.Sort();
            return LibData.Merge(",", false, channels.ToArray());
        }
        /// <summary>
        /// 獲取啟用代收類別
        /// </summary>
        /// <param name="accftt"></param>
        /// <returns></returns>
        private static string GetCollectionType(ACCFTT accftt)
        {
            List<string> collections = new List<string>();
            if (!accftt.RECVITEM1.IsNullOrEmpty())
            {
                collections.Add(accftt.RECVITEM1);
            }

            if (!accftt.RECVITEM2.IsNullOrEmpty())
            {
                collections.Add(accftt.RECVITEM2);
            }

            if (!accftt.RECVITEM3.IsNullOrEmpty())
            {
                collections.Add(accftt.RECVITEM3);
            }

            if (!accftt.RECVITEM4.IsNullOrEmpty())
            {
                collections.Add(accftt.RECVITEM4);
            }

            if (!accftt.RECVITEM5.IsNullOrEmpty())
            {
                collections.Add(accftt.RECVITEM5);
            }

            collections.Sort();
            return LibData.Merge(",", false, collections.ToArray());
        }
        /// <summary>
        /// 獲取自組銷帳編號3
        /// </summary>
        /// <param name="accftt"></param>
        /// <returns></returns>
        private static VirtualAccount3 GetVirtualAccount3(ACCFTT accftt)
        {
            if (accftt.CHKNUMFLAG == "0" || accftt.CHKNUMFLAG == "N")
                return VirtualAccount3.NoverifyCode;
            else
                if (accftt.CHKAMTFLAG == "Y")
                    if (accftt.DUETERM == "1")
                        return VirtualAccount3.SeqAmountPayEndDate;
                    else
                        return VirtualAccount3.SeqAmount;
                else
                    if (accftt.DUETERM == "1")
                        return VirtualAccount3.SeqPayEndDate;
                    else
                        return VirtualAccount3.Seq;
        }
        #endregion
    }
}
