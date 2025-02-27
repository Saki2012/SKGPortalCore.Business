﻿using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using SKGPortalCore.Core;
using SKGPortalCore.Core.DB;
using SKGPortalCore.Core.Libary;
using SKGPortalCore.Core.LibEnum;
using SKGPortalCore.Core.Model.User;
using SKGPortalCore.Interface.IRepository.Import;
using SKGPortalCore.Model.MasterData;
using SKGPortalCore.Model.SourceData;
using SKGPortalCore.Repository.BillData;
using SKGPortalCore.Repository.MasterData;
using SKGPortalCore.Repository.SKGPortalCore.Business.BillData;

namespace SKGPortalCore.Repository.SKGPortalCore.Business.Import
{
    /// <summary>
    /// 資訊流導入-超商
    /// </summary>
    public sealed class ReceiptInfoImportMARKET : IImportData
    {
        #region Property
        /// <summary>
        /// 資訊流長度(byte)
        /// </summary>
        private const int StrLen = 120;
        /// <summary>
        /// 
        /// </summary>
        public ApplicationDbContext DataAccess { get; }
        /// <summary>
        /// 
        /// </summary>
        public SysMessageLog Message { get; }
        /// <summary>
        /// 檔案名稱
        /// </summary>
        private const string FileName = "SKG_MART";
        /// <summary>
        /// 原檔案存放位置
        /// </summary>
        private const string SrcPath = @"D:\iBankRoot\Ftp_SKGPortalCore\TransactionListDaily\";
        /// <summary>
        /// 成功檔案存放位置
        /// </summary>
        private const string SuccessPath = @"D:\iBankRoot\Ftp_SKGPortalCore\SuccessFolder\TransactionListDaily\";
        /// <summary>
        /// 失敗檔案存放位置
        /// </summary>
        private const string FailPath = @"D:\iBankRoot\Ftp_SKGPortalCore\ErrorFolder\TransactionListDaily\";
        /// <summary>
        /// 原資料
        /// </summary>
        private string SrcFile => $"{SrcPath}{FileName}.{DateTime.Now.ToString("yyyyMMdd", CultureInfo.InvariantCulture)}";
        /// <summary>
        /// 成功資料
        /// </summary>
        private string SuccessFile => $"{SuccessPath}{FileName}.{DateTime.Now.ToString("yyyyMMdd", CultureInfo.InvariantCulture)}{LibData.GenRandomString(3)}";
        /// <summary>
        /// 失敗資料
        /// </summary>
        private string FailFile => $"{FailPath}{FileName}.{DateTime.Now.ToString("yyyyMMdd", CultureInfo.InvariantCulture)}{LibData.GenRandomString(3)}";
        #endregion

        #region Construct
        public ReceiptInfoImportMARKET(ApplicationDbContext dataAccess, SysMessageLog messageLog = null)
        {
            DataAccess = dataAccess;
            Message = messageLog ?? new SysMessageLog(SystemOperator.SysOperator);
            Directory.CreateDirectory(SrcPath);
            Directory.CreateDirectory(SuccessPath);
            Directory.CreateDirectory(FailPath);
        }
        #endregion

        #region Implement
        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        Dictionary<int, string> IImportData.ReadFile()
        {
            Dictionary<int, string> result = new Dictionary<int, string>();
            int line = 0; string strRow;
            using StreamReader sr = new StreamReader(SrcFile, Encoding.GetEncoding(950));
            while (sr.Peek() > 0)
            {
                strRow = sr.ReadLine();
                line++;
                if (0 == strRow.Length)
                {
                    continue;
                }

                if (StrLen != strRow.ByteLen()) { /*第N行 Error:長度不符*/}
                switch (strRow.ByteSubString(0, 1))
                {
                    case "1"://表頭、檢查是否今日，並且Count歸零
                        break;
                    case "2"://明細
                        result.Add(line, strRow);
                        break;
                    case "3"://表尾、檢查是否筆數正確
                        break;
                }
            }
            return result;
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="sources"></param>
        /// <returns></returns>
        IList IImportData.AnalyzeFile(Dictionary<int, string> sources)
        {
            List<dynamic> result = new List<dynamic>();
            DateTime now = DateTime.Now;
            string importBatchNo = $"MART{now.ToString("yyyyMMddhhmmss")}";
            foreach (int line in sources.Keys)
            {
                if (LibData.ByteSubString(sources[line], 9, 3) == "I0O")
                    result.Add(new ReceiptInfoBillMarketSPIModel() { Id = line, Source = sources[line], ImportBatchNo = importBatchNo });
                else
                    result.Add(new ReceiptInfoBillMarketModel() { Id = line, Source = sources[line], ImportBatchNo = importBatchNo });
            }
            return result;
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="modelSources"></param>
        void IImportData.CreateData(IList modelSources)
        {
            List<dynamic> models = modelSources as List<dynamic>;
            using BizCustomerRepository bizCustRepo = new BizCustomerRepository(DataAccess) { Message = Message };
            using ReceiptBillRepository repo = new ReceiptBillRepository(DataAccess) { User = SystemOperator.SysOperator };
            List<ChannelMapModel> channelMap = DataAccess.Set<ChannelMapModel>().ToList();
            models.ForEach(model =>
            {
                if (model is ReceiptInfoBillMarketModel)
                {
                    repo.Create(BizReceiptInfo.GetReceiptBillSet(model, channelMap));
                }
                else if (model is ReceiptInfoBillMarketSPIModel)
                {
                    repo.Create(BizReceiptInfo.GetReceiptBillSet(model, channelMap));
                }
            });
            repo.CommitData(FuncAction.Create);
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="isSuccess"></param>
        void IImportData.MoveToOverFolder(bool isSuccess)
        {
            if (File.Exists(SrcFile))
            {
                string file;
                do
                {
                    file = isSuccess ? SuccessFile : FailFile;
                } while (File.Exists(file));
                File.Move(SrcFile, file);
            }
        }
        #endregion
    }
}
