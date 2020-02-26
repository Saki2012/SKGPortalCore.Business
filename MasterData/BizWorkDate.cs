using System;
using System.IO;
using System.Net;
using System.Text;
using SKGPortalCore.Model.MasterData;

namespace SKGPortalCore.Repository.SKGPortalCore.Business.MasterData
{
    internal class BizWorkDate
    {
        /// <summary>
        /// 同步政府行政機關辦公日曆表
        /// </summary>
        public WorkDateModel[] SyncWorkDate()
        {
            string url = "http://data.ntpc.gov.tw/api/v1/rest/datastore/382000000A-000077-002";
            WebRequest request = WebRequest.Create(url);
            HttpWebResponse response = request.GetResponse() as HttpWebResponse;
            Stream responseStream = response.GetResponseStream();
            using StreamReader reader = new StreamReader(responseStream, Encoding.GetEncoding("utf-8"));
            string srcString = reader.ReadToEnd();
            HolidayOpenData jsonData = Newtonsoft.Json.JsonConvert
                .DeserializeObject<HolidayOpenData>(srcString);
            foreach (WorkDateModel holiday in jsonData.result.records)
            {
                Console.WriteLine($"Date: {holiday.Date}, IsHoliday: {holiday.IsWorkDate}, Category: {holiday.HolidayCategory}");
            }
            return jsonData.result.records;
        }
    }
}
