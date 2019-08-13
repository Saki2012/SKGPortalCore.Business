using SKGPortalCore.Model.MasterData;
using System;
using System.IO;
using System.Net;
using System.Text;

namespace SKGPortalCore.Business.MasterData
{
    public class BizWorkDate
    {
        /// <summary>
        /// 同步政府行政機關辦公日曆表
        /// </summary>
        public WorkDateModel[] SyncWorkDate()
        {
            string url = "http://data.ntpc.gov.tw/api/v1/rest/datastore/382000000A-000077-002";
            var request = WebRequest.Create(url);
            var response = request.GetResponse() as HttpWebResponse;
            var responseStream = response.GetResponseStream();
            using StreamReader reader = new StreamReader(responseStream, Encoding.GetEncoding("utf-8"));
            var srcString = reader.ReadToEnd();
            var jsonData = Newtonsoft.Json.JsonConvert
                .DeserializeObject<HolidayOpenData>(srcString);
            foreach (var holiday in jsonData.result.records)
            {
                Console.WriteLine($"Date: {holiday.Date}, IsHoliday: {holiday.IsHoliday}, Category: {holiday.HolidayCategory}");
            }
            return jsonData.result.records;
        }
    }
}
