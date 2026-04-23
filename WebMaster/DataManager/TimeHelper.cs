using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WebMaster.DataManager
{
   public class TimeHelper
    {

        /// <summary>
        ///  时间戳[10|13]转为C#格式时间       
        /// </summary>
        public static DateTime StampToDateTime(string stamp)
        {
            if (stamp.Length != 10 || stamp.Length != 13) return DateTime.Now;
            try
            {
                DateTime StartDateTime = TimeZoneInfo.ConvertTime(new DateTime(1970, 1, 1), TimeZoneInfo.FindSystemTimeZoneById("China Standard Time"));
                if (stamp.Length == 10) StartDateTime.AddSeconds(long.Parse(stamp));
                return StartDateTime.AddMilliseconds(long.Parse(stamp));
            }
            catch (Exception)
            {
                return DateTime.Now;
            }
        }



        /// <summary>
        /// 获取时间戳
        /// </summary>
        /// <returns></returns>
        public static string DateTimeToStamp(System.DateTime time, int length = 13)
        {
            long ts = ConvertDateTimeTolong(time);
            return ts.ToString().Substring(0, length);
        }
    


        /// <summary>
        /// DateTime时间格式转换为Unix时间戳格式
        /// </summary>
        private static long ConvertDateTimeTolong(DateTime datetime)
        {

            try
            {
                return datetime.Ticks - TimeZoneInfo.ConvertTime(new DateTime(1970, 1, 1), TimeZoneInfo.FindSystemTimeZoneById("China Standard Time")).Ticks;
            }
            catch (Exception)
            {
                return 0;
            }
        }

    }
}
