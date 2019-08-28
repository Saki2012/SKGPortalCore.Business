using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using SKGPortalCore.Data;
using SKGPortalCore.Lib;
using SKGPortalCore.Model;
using SKGPortalCore.Model.MasterData;
using SKGPortalCore.Model.SourceData;

namespace SKGPortalCore.Business.Func
{
    public class BizACCFTT : BizBase
    {
        public BizACCFTT(MessageLog message, ApplicationDbContext db) : base(message, db) { }

        #region Private
        /// <summary>
        /// 
        /// </summary>
        /// <param name="data"></param>
        private void SyncData(List<BizCustomerSet> bizCustomerSets, List<CustomerSet> customerSets, List<ACCFTT> datas)
        {
            foreach (ACCFTT data in datas)
            {
                using var transaction = DataAccess.Database.BeginTransaction();
                try
                {
                    switch (data.APPLYSTAT.ToInt32())
                    {
                        case 0:
                            {
                                SetCustomer(data, new CustomerSet());
                                SetBizCustomer(data, new BizCustomerSet());
                                //throw new Exception();
                            }
                            break;
                        case 1:
                        case 9:
                            {
                                BizCustomerSet set = bizCustomerSets.Where(p => p.BizCustomer.CustomerId == data.IDCODE && p.BizCustomer.CustomerCode == data.KEYNO).FirstOrDefault();
                                if (null != set) set.BizCustomer.AccountStatus = AccountStatus.Unable;
                            }
                            break;
                    }
                    transaction.Commit();
                }
                catch (Exception ex)
                {
                    transaction.Rollback();
                    throw ex;
                }
            }
            //BizCustomerRepository bizCustomerRepository = new BizCustomerRepository();
            //bizCustomerRepository.Create(bizCustomerSets[0]);
            //CustomerRepository customerRepository = new CustomerRepository();
            //customerRepository.Create(customerSets[0]);
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="data"></param>
        /// <param name="customerSet"></param>
        private void SetCustomer(ACCFTT data, CustomerSet customerSet)
        {
            CustomerModel cust = customerSet.Customer;
            cust.CustomerId = data.IDCODE;
            cust.CustomerName = data.CUSTNAME;
            cust.DeptId = data.APPBECODE;
            cust.BillTermLen = 3;
            cust.PayerNoLen = 6;
            cust.IsSysCust = false;

            //CustomerRepository rep = new CustomerRepository(DataAccess);
            //rep.Create(customerSet);
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="data"></param>
        /// <param name="bizCustomerSet"></param>
        private void SetBizCustomer(ACCFTT data, BizCustomerSet bizCustomerSet)
        {
            BizCustomerModel bizCust = bizCustomerSet.BizCustomer;

            bizCust.CustomerId = data.IDCODE;
            bizCust.CustomerCode = data.KEYNO;
            bizCust.AccountDeptId = data.BRCODE;
            bizCust.RealAccount = data.ACCIDNO;
            bizCust.VirtualAccountLen = (10 + data.KEYNO.Length).ToByte();
            bizCust.ChannelIds = GetChannel(data);
            bizCust.CollectionTypeIds = GetCollectionType(data);
            bizCust.AccountStatus = AccountStatus.Unable;
            bizCust.VirtualAccount3 = GetVirtualAccount3(data);
            bizCust.EntrustCustId = data.CUSTID;


            List<BizCustomerFeeDetailModel > bizCustDetail = bizCustomerSet.BizCustomerFeeDetail;

            if (!data.ACTFEE.ToInt32().IsNullOrEmpty())
                bizCustDetail.Add(new BizCustomerFeeDetailModel ()
                {
                    CustomerCode = bizCust.CustomerCode,
                    ChannelType = CanalisType.Market,
                    FeeType = FeeType.ClearFee,
                    Fee = data.ACTFEE.ToDecimal(),
                    Percent = 0m
                });
            if (!data.ACTFEEPT.ToInt32().IsNullOrEmpty())
                bizCustDetail.Add(new BizCustomerFeeDetailModel ()
                {
                    CustomerCode = bizCust.CustomerCode,
                    ChannelType = CanalisType.Post,
                    FeeType = FeeType.ClearFee,
                    Fee = data.ACTFEEPT.ToDecimal(),
                    Percent = 0m,
                });
            if (!data.HIFLAG.IsNullOrEmpty())
                bizCust.HiTrustFlag = (HiTrustFlag)data.HIFLAG.ToByte();
            if (!data.HIFARE.IsNullOrEmpty())
                bizCustDetail.Add(new BizCustomerFeeDetailModel ()
                {
                    CustomerCode = bizCust.CustomerCode,
                    ChannelType = CanalisType.HiTrust,
                    FeeType = FeeType.HitrustFee,
                    Fee = data.HIFARE.ToDecimal(),
                    Percent = 0m,
                });
            //銀行-每筆總手續費
            if (!data.ACTFEEBEFT.IsNullOrEmpty())
                bizCustDetail.Add(new BizCustomerFeeDetailModel ()
                {
                    CustomerCode = bizCust.CustomerCode,
                    ChannelType = CanalisType.Bank,
                    FeeType = FeeType.TotalFee,
                    Fee = data.ACTFEEBEFT.ToDecimal(),
                    Percent = data.SHAREBEFTPERCENT.ToDecimal(),
                });
            //超商-每筆總手續費
            if (!data.ACTFEEMART.IsNullOrEmpty())
                bizCustDetail.Add(new BizCustomerFeeDetailModel ()
                {
                    CustomerCode = bizCust.CustomerCode,
                    ChannelType = CanalisType.Market,
                    FeeType = FeeType.TotalFee,
                    Fee = data.ACTFEEMART.ToDecimal(),
                    Percent = data.ACTPERCENT.ToDecimal(),
                });
            //郵局-每筆總手續費
            if (!data.ACTFEEPOST.IsNullOrEmpty())
                bizCustDetail.Add(new BizCustomerFeeDetailModel ()
                {
                    CustomerCode = bizCust.CustomerCode,
                    ChannelType = CanalisType.Post,
                    FeeType = FeeType.TotalFee,
                    Fee = data.ACTFEEPOST.ToDecimal(),
                    Percent = data.POSTPERCENT.ToDecimal(),
                });
            //農金-清算手續費
            if (!data.AGRIFEE.IsNullOrEmpty())
                bizCustDetail.Add(new BizCustomerFeeDetailModel ()
                {
                    CustomerCode = bizCust.CustomerCode,
                    ChannelType = CanalisType.Farm,
                    FeeType = FeeType.ClearFee,
                    Fee = data.AGRIFEE.ToDecimal(),
                    Percent = 0,
                });

            //BizCustomerRepository rep = new BizCustomerRepository(DataAccess);
            //rep.Create(bizCustomerSet);
        }
        /// <summary>
        /// 同步客戶Admin密碼
        /// </summary>
        private void SyncPasuwado()
        {
            List<string> customerids = new List<string>();
            /*
            --Update
            Select * From Customer A
            Inner Join CustomerAdminPaswado B On A.CustomerId=B.CustomerId
            Inner Join CustUserModel C On A.CustomerId=C.CustomerId And C.UserId='Admin'
            --Insert
            Select * From Customer A
            --Inner Join CustomerAdminPaswado B On A.CustomerId=B.CustomerId
            Left Join CustUserModel C On A.CustomerId=C.CustomerId And C.UserId='Admin'
            Where C.KeyId Is Null
             */
        }
        /// <summary>
        /// 獲取啟用通路
        /// </summary>
        /// <param name="data"></param>
        /// <returns></returns>
        private string GetChannel(ACCFTT data)
        {
            List<string> channels = new List<string>();
            switch (data.CHANNEL)
            {
                case "9":
                    {
                        channels.Add(ChannelValue.Cash);
                        channels.Add(ChannelValue.ATM);
                        channels.Add(ChannelValue.Remit);
                        channels.Add(ChannelValue.ACH);
                        channels.Add(ChannelValue.ACHForPay);
                        channels.Add(ChannelValue.CTBC);
                        if (data.POSTFLAG == "1") channels.Add(ChannelValue.Post);
                        if (data.RSTORE1 == "1") channels.Add(ChannelValue.Mart711);
                        if (data.RSTORE2 == "1") channels.Add(ChannelValue.MartFML);
                        if (data.RSTORE3 == "1") channels.Add(ChannelValue.MartOK);
                        if (data.RSTORE4 == "1") channels.Add(ChannelValue.MartLIFE);
                    }
                    break;
                case "0":
                    {
                        channels.Add(ChannelValue.Cash);
                        channels.Add(ChannelValue.Remit);
                        channels.Add(ChannelValue.ACH);
                        channels.Add(ChannelValue.ACHForPay);
                        channels.Add(ChannelValue.CTBC);
                        if (data.POSTFLAG == "1") channels.Add(ChannelValue.Post);
                        if (data.RSTORE1 == "1") channels.Add(ChannelValue.Mart711);
                        if (data.RSTORE2 == "1") channels.Add(ChannelValue.MartFML);
                        if (data.RSTORE3 == "1") channels.Add(ChannelValue.MartOK);
                        if (data.RSTORE4 == "1") channels.Add(ChannelValue.MartLIFE);
                    }
                    break;
                case "1":
                    {
                        channels.Add(ChannelValue.ATM);
                        channels.Add(ChannelValue.Remit);
                        channels.Add(ChannelValue.ACH);
                        channels.Add(ChannelValue.ACHForPay);
                        channels.Add(ChannelValue.CTBC);
                        if (data.POSTFLAG == "1") channels.Add(ChannelValue.Post);
                        if (data.RSTORE1 == "1") channels.Add(ChannelValue.Mart711);
                        if (data.RSTORE2 == "1") channels.Add(ChannelValue.MartFML);
                        if (data.RSTORE3 == "1") channels.Add(ChannelValue.MartOK);
                        if (data.RSTORE4 == "1") channels.Add(ChannelValue.MartLIFE);
                    }
                    break;
                case "2":
                    {
                        channels.Add(ChannelValue.Cash);
                        if (data.RSTORE1 == "1") channels.Add(ChannelValue.Mart711);
                        if (data.RSTORE2 == "1") channels.Add(ChannelValue.MartFML);
                        if (data.RSTORE3 == "1") channels.Add(ChannelValue.MartOK);
                        if (data.RSTORE4 == "1") channels.Add(ChannelValue.MartLIFE);
                    }
                    break;
                case "3":
                    {
                        channels.Add(ChannelValue.Remit);
                    }
                    break;
                case "4":
                    {
                        if (data.POSTFLAG == "1") channels.Add(ChannelValue.Post);
                        if (data.RSTORE1 == "1") channels.Add(ChannelValue.Mart711);
                        if (data.RSTORE2 == "1") channels.Add(ChannelValue.MartFML);
                        if (data.RSTORE3 == "1") channels.Add(ChannelValue.MartOK);
                        if (data.RSTORE4 == "1") channels.Add(ChannelValue.MartLIFE);
                    }
                    break;
            }
            if (data.AUTOFLAG == "1") channels.Add(ChannelValue.Deduct_Server); channels.Add(ChannelValue.Deduct_Client);
            if (data.EBFLAG == "1") channels.Add(ChannelValue.EB);
            if (data.CTBCFLAG == "1") channels.Add(ChannelValue.CTBC);
            if (data.AGRIFLAG == "1") channels.Add(ChannelValue.Farm);
            channels.Sort();
            return LibData.Merge(",", false, channels.ToArray());
        }
        /// <summary>
        /// 獲取啟用代收類別
        /// </summary>
        /// <param name="data"></param>
        /// <returns></returns>
        private string GetCollectionType(ACCFTT data)
        {
            List<string> collections = new List<string>();
            if (!data.RECVITEM1.IsNullOrEmpty()) collections.Add(data.RECVITEM1);
            if (!data.RECVITEM2.IsNullOrEmpty()) collections.Add(data.RECVITEM2);
            if (!data.RECVITEM3.IsNullOrEmpty()) collections.Add(data.RECVITEM3);
            if (!data.RECVITEM4.IsNullOrEmpty()) collections.Add(data.RECVITEM4);
            if (!data.RECVITEM5.IsNullOrEmpty()) collections.Add(data.RECVITEM5);
            collections.Sort();
            return LibData.Merge(",", false, collections.ToArray());
        }
        /// <summary>
        /// 獲取自組銷帳編號3
        /// </summary>
        /// <param name="data"></param>
        /// <returns></returns>
        private VirtualAccount3 GetVirtualAccount3(ACCFTT data)
        {
            if (data.CHKNUMFLAG == "0" || data.CHKNUMFLAG == "N")
                return VirtualAccount3.NoverifyCode;
            else
            {
                if (data.CHKAMTFLAG == "Y")
                {
                    if (data.DUETERM == "1")
                        return VirtualAccount3.SeqAmountPayEndDate;
                    else
                        return VirtualAccount3.SeqAmount;
                }
                else
                {
                    if (data.DUETERM == "1")
                        return VirtualAccount3.SeqPayEndDate;
                    else
                        return VirtualAccount3.Seq;
                }
            }
        }
        #endregion
    }
}
