using System;
using System.Collections.Generic;
using System.Text;
using SKGPortalCore.Data;
using SKGPortalCore.Model.BillData;

namespace SKGPortalCore.Business.BillData
{
   public class BizReceiptInfoBill : BizBase
    {
        public BizBank Bank { get; }
        public BizPost Post { get; }
        public BizMarket Market { get; }
        public BizMarketSPI MarketSPI { get; }
        public BizFarm Farm { get; }
        public BizReceiptInfoBill(MessageLog message) : base(message) 
        {
            Bank = new BizBank() { Message = message };
            Post = new BizPost() { Message = message };
            Market = new BizMarket() { Message = message };
            MarketSPI = new BizMarketSPI() { Message = message };
            Farm = new BizFarm() { Message = message };
        }

        public class BizBank
        {
            public MessageLog Message { get; set; }
            public void CheckData(ReceiptInfoBillBankModel model)
            {
                

            }
            public ReceiptBillSet GetReceiptBillSet(ReceiptInfoBillBankModel model)
            {

                return null;
            }
        }

        public class BizPost
        {
            public MessageLog Message { get; set; }
            public void CheckData(ReceiptInfoBillPostModel model)
            {


            }
            public ReceiptBillSet GetReceiptBillSet(ReceiptInfoBillPostModel model)
            {

                return null;
            }
        }

        public class BizMarket
        {
            public MessageLog Message { get; set; }
            public void CheckData(ReceiptInfoBillMarketModel model)
            {


            }
            public ReceiptBillSet GetReceiptBillSet(ReceiptInfoBillMarketModel model)
            {

                return null;
            }
        }

        public class BizMarketSPI
        {
            public MessageLog Message { get; set; }
            public void CheckData(ReceiptInfoBillMarketSPIModel model)
            {


            }
            public ReceiptBillSet GetReceiptBillSet(ReceiptInfoBillMarketSPIModel model)
            {

                return null;
            }
        }
        public class BizFarm
        {
            public MessageLog Message { get; set; }
            public void CheckData(ReceiptInfoBillFarmModel model)
            {


            }
            public ReceiptBillSet GetReceiptBillSet(ReceiptInfoBillFarmModel model)
            {

                return null;
            }
        }
    }
}
