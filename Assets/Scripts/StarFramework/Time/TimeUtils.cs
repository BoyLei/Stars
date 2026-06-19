////////////////////////////////////////////////////////////////////
//                            _ooOoo_                             //
//                           o8888888o                            //
//                           88" . "88                            //
//                           (| ^_^ |)                            //
//                           O\  =  /O                            //
//                        ____/`---'\____                         //
//                      .'  \\|     |//  `.                       //
//                     /  \\|||  :  |||//  \                      //
//                    /  _||||| -:- |||||-  \                     //
//                    |   | \\\  -  /// |   |                     //
//                    | \_|  ''\---/''  |   |                     //
//                    \  .-\__  `-`  ___/-. /                     //
//                  ___`. .'  /--.--\  `. . ___                   //
//                ."" '<  `.___\_<|>_/___.'  >'"".                //
//              | | :  `- \`.;`\ _ /`;.`/ - ` : | |               //
//              \  \ `-.   \_ __\ /__ _/   .-` /  /               //
//        ========`-.____`-.___\_____/___.-`____.-'========       //
//                             `=---='                            //
//        ^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^      //
//            佛祖保佑       无BUG        不修改                   //
////////////////////////////////////////////////////////////////////
/*
* 描述：
* 工程 ：StarProject
*/

using StarProject.Service.LocalData;
using StarProject.Service.Time;
using System;
using System.Text;

namespace SGF.Time
{
    ///【1970】+ 【Gmt处理】
    ///【客户端时刻】
    ///【服务器时刻】= 【服务器时间】 + 【LagTime（Detal)】
    ///TODO：lag平均值，给服务器提前发送请求攻击指令，GetSystemTime？
    [XLua.LuaCallCSharp]
    public class TimeUtils
    {
        public static float FixedDeltaTime = UnityEngine.Time.fixedDeltaTime * 1000;
        //unit
        readonly static DateTime DateTime_1970_01_01_00_00_00 = new(1970, 1, 1);
        //北京Gmt
        readonly static DateTime DateTime_1970_01_01_08_00_00 = new(1970, 1, 1, 8, 0, 0);

        #region 服务器时间

        /// <summary>
        /// 【客户端时刻-gmt】
        /// </summary>
        public static DateTime ClientUtcNow { get { return TimeZone.CurrentTimeZone.ToLocalTime(DateTime.UtcNow); } }

        private static StringBuilder sb = new();
        private static readonly object lockObject = new();

        private static string GetTiemStr(DateTime dateTime)
        {
            return sb.Clear().Append(dateTime).Append(":").Append(dateTime.Millisecond).ToString();

        }
        public static string ClientUtcNowStr { get { return GetTiemStr(ClientUtcNow); } }

        /// <summary>
        /// 【服务器时刻-gmt】
        /// </summary>
        public static DateTime ServerNow { get { return GetServerTime(); } }

        /// <summary>
        /// 服务器当前的 时间的 天数
        /// </summary> 
        public static int ServerNowDay { get { return ServerNow.Day; } }

        /// <summary>
        /// 【服务器时刻-gmt-毫秒】
        /// </summary>
        public static long ServerNowStampMilli { get { return DateTimeToStampMilli(ServerNow); } }

        public static long ClientNowStampMilli { get { return DateTimeToStampMilli(ClientUtcNow); } }


        //LagTime（当服务器发送服务器当前时间）：正数为客户端时间在未来，1校准时间，2加速，3重要在预估网络延迟为+100ms的时候属正常波动，
        //整数说明是服务器提前指令
        //负数是服务器在过去，他本要提前的说法并没达到，说明服务器处理的数据延后了（服务器处理问题，网络波动太卡了）：之后要做平均值
        public static double LagMillTime = 0;
        // 平均的ping 值
        public static int AveragePing = 0;
        public static int SinglePing => AveragePing / 2;

        public static void OnServerTimeSync(long millisTime)
        {
            //客户端服务器都应该基于GMT来做
            DateTime serverDT = TimeZone.CurrentTimeZone.ToLocalTime(DateTime_1970_01_01_00_00_00).AddMilliseconds(millisTime);
            LagMillTime = (serverDT - ClientUtcNow).TotalMilliseconds;
        }


        //当前服务器时间
        public static DateTime GetServerTime()
        {
            var serverBaseTime = TimeManager.Instance.ServerSyncTimeStamp;

            // 如果 服务器时间取不到, 那就取客户端本地时间 为服务器时间
            if (serverBaseTime == 0)
            {
                return ClientUtcNow;
            }

            var dateTime1 = TimeManager.Instance.GetServerTime();

            return dateTime1;
        }

        // 时间戳 转换为时间 毫秒
        public static DateTime MilliStampToDateTime(string timeStamp)
        {
            DateTime startTime = TimeZone.CurrentTimeZone.ToLocalTime(new DateTime(1970, 1, 1));
            long mTime = long.Parse(timeStamp + "0000");
            //long mTime = long.Parse(timeStamp);
            TimeSpan toNow = new(mTime);
            //Debug.Log("\n 当前时间为：" + startTime.Add(toNow).ToString("yyyy/MM/dd HH:mm:ss:ffff"));
            //Debug.Log("\n 当前时间为：" + startTime.Add(toNow).ToString("yyyy/MM/dd HH:mm:ss"));
            return startTime.Add(toNow);
        }

        // 时间转时间戳  毫秒
        public static long DateTimeToStampMilli(DateTime now)
        {
            //Unix 2 Gmt //北京是时间纪元的8.am //1970-0-0~1970-8-0
            DateTime current_GMT_ORIStartTime = TimeZone.CurrentTimeZone.ToLocalTime(new DateTime(1970, 1, 1)); // 当地时区 //服务器也要取时区一样的 //时区对时区保持一致
            long timeStamp = (long)(now - current_GMT_ORIStartTime).TotalMilliseconds; // 相差毫秒数 //gmt服务器（延迟时间） - gmt纪元时间 = 延迟时间 【服务器时间就叫通信标准时间，没人关心你客户端时间】
                                                                                       //Gmt标准通讯时间 - Gmt纪元时间 = 通讯时间戳 
                                                                                       //long timeStamp = (long)(now - startTime).TotalSeconds; // 相差毫秒数
                                                                                       //NumberToTextConverter.ToText((now - current_GMT_ORIStartTime).TotalMilliseconds);
            return timeStamp;
        }


        /// <summary>  
        /// 时间戳Timestamp转换成日期  
        /// </summary>  
        /// <param name="timeStamp"></param>  
        /// <returns></returns>  
        public static DateTime GetDateTime(int timeStamp)
        {
            DateTime dtStart = TimeZone.CurrentTimeZone.ToLocalTime(new DateTime(1970, 1, 1));
            long lTime = (long)timeStamp * 10000000;
            TimeSpan toNow = new(lTime);
            DateTime targetDt = dtStart.Add(toNow);
            return targetDt;
        }

        /// <summary>  
        /// 时间戳Timestamp转换成日期  
        /// </summary>  
        /// <param name="timeStamp"></param>  
        /// <returns></returns>  
        public static DateTime GetDateTime(string timeStamp)
        {
            DateTime dtStart = TimeZone.CurrentTimeZone.ToLocalTime(new DateTime(1970, 1, 1));
            long lTime = long.Parse(timeStamp + "0000000");
            TimeSpan toNow = new(lTime);
            DateTime targetDt = dtStart.Add(toNow);
            return targetDt;
        }

        public static long DateTime2Stamp(DateTime now)
        {
            System.DateTime startTime = TimeZone.CurrentTimeZone.ToLocalTime(new System.DateTime(1970, 1, 1)); // 当地时区
            long timeStamp = (long)(now - startTime).TotalSeconds; // 相差秒数
            return timeStamp;
        }

        public static DateTime Stamp2DataTime(long stamp)
        {
            System.DateTime startTime = TimeZone.CurrentTimeZone.ToLocalTime(new System.DateTime(1970, 1, 1)); // 当地时区
            DateTime dt = startTime.AddSeconds(stamp);
            return dt;
        }

        public static DateTime JavaTimeStampToDateTime(double javaTimeStamp)
        {
            // Java timestamp is milliseconds past epoch
            System.DateTime dtDateTime = new(1970, 1, 1, 0, 0, 0, 0, System.DateTimeKind.Utc);
            dtDateTime = dtDateTime.AddMilliseconds(javaTimeStamp).ToLocalTime();
            return dtDateTime;
        }

        public static long Server2Stamp(string dateTime)
        {
            long ret = 0;
            string[] bufBig = dateTime.Split('T');

            string[] bufDate = bufBig[0].Split('-');

            string[] bufTime = bufBig[1].Split(':');

            string[] second = bufTime[2].Split('.');
            DateTime date = new(int.Parse(bufDate[0]), int.Parse(bufDate[1]), int.Parse(bufDate[2]), int.Parse(bufTime[0]), int.Parse(bufTime[1]), int.Parse(second[0]));

            ret = DateTime2Stamp(date);
            return ret;
        }
        #endregion

        //--------------------客户端时间----------------两份就是我们没必要修改用户android时间

        //TODO: dl
        //等待接入服务器时间后，重新封装
        public static double GetServerTimeNow()
        {

            return ServerNow.Subtract(DateTime_1970_01_01_08_00_00).TotalMilliseconds;
        }

        //自 1970 年 1 月 1 日午夜 12:00:00 经过的毫秒数
        public static double GetTotalMilliseconds()
        {
            DateTime nowtime = DateTime.Now.ToLocalTime();
            return nowtime.Subtract(DateTime_1970_01_01_08_00_00).TotalMilliseconds;
        }

        public static double GetTotalSeconds()
        {
            DateTime nowtime = DateTime.Now.ToLocalTime();
            return nowtime.Subtract(DateTime_1970_01_01_08_00_00).TotalSeconds;
        }


        public static TimeSpan GetTimeSpanSince1970()
        {
            return DateTime.Now.Subtract(DateTime_1970_01_01_08_00_00);
        }

        public static string FormatShowTime(ulong timeInSec)
        {
            string _text = "";
            ulong showTime;
            if ((timeInSec / 86400) > 0)
            {
                showTime = timeInSec / 86400;
                _text = showTime.ToString() + "天";
            }
            else if ((timeInSec / 3600) > 0)
            {
                showTime = timeInSec / 3600;
                _text = showTime.ToString() + "小时";
            }
            else if ((timeInSec / 60) > 0)
            {
                showTime = timeInSec / 60;
                _text = showTime.ToString() + "分钟";
            }
            else
            {
                // 对1分钟进行特殊处理, 30秒 和 1分30秒 均为1分钟
                _text = "1分钟";
            }
            return _text;
        }

        public static string FormatShowTimeHM(double javaTimeStamp = 0, string symbol = ":")
        {
            string _text = "";

            if (javaTimeStamp == 0)
            {
                javaTimeStamp = ServerNowStampMilli;
            }

            DateTime dateTime = JavaTimeStampToDateTime(javaTimeStamp);

            _text += dateTime.Hour;
            _text += symbol;
            _text += dateTime.Minute >= 10 ? dateTime.Minute.ToString() : ("0" + dateTime.Minute.ToString());

            return _text;
        }


        public static ulong getDay(ulong _time)
        {
            return _time / 86400;
        }

        public static ulong getHour(ulong _time)
        {
            return _time % 86400 / 3600;
        }

        public static ulong getMinute(ulong _time)
        {
            return _time % 3600 / 60;
        }

        public static ulong getSecond(ulong _time)
        {
            return _time % 60;
        }

        //--------------------------------------------------------------------------------
        public const uint OnDaySecond = 24 * 60 * 60;
        public const uint OnHourSecond = 60 * 60;


        public static string GetTimeString(string format, long seconds)
        {
            string label = format;
            long ms = seconds * 1000;
            int s = (int)seconds;
            int m = s / 60;
            int h = m / 60;
            int d = h / 24;

            string t = "";
            //处理天
            if (label.IndexOf("%dd") >= 0)
            {
                t = d >= 10 ? d.ToString() : ("0" + d.ToString());
                label = label.Replace("%dd", t);
                h = h % 24;
            }
            else if (label.IndexOf("%d") >= 0)
            {
                label = label.Replace("%d", d.ToString());
                h = h % 24;
            }

            //处理小时
            if (label.IndexOf("%hh") >= 0)
            {
                t = h >= 10 ? h.ToString() : ("0" + h.ToString());
                label = label.Replace("%hh", t);
                m = m % 60;
            }
            else if (label.IndexOf("%h") >= 0)
            {
                label = label.Replace("%h", h.ToString());
                m = m % 60;
            }

            //处理分
            if (label.IndexOf("%mm") >= 0)
            {
                t = m >= 10 ? m.ToString() : ("0" + m.ToString());
                label = label.Replace("%mm", t);
                s = s % 60;
            }
            else if (label.IndexOf("%m") >= 0)
            {
                label = label.Replace("%m", m.ToString());
                s = s % 60;
            }

            //处理秒
            if (label.IndexOf("%ss") >= 0)
            {
                t = s >= 10 ? s.ToString() : ("0" + s.ToString());
                label = label.Replace("%ss", t);
                ms = ms % 1000;
            }
            else if (label.IndexOf("%s") >= 0)
            {
                label = label.Replace("%s", s.ToString());
                ms = ms % 1000;
            }

            //处理毫秒
            if (label.IndexOf("ms") >= 0)
            {
                t = ms.ToString();
                label = label.Replace("%ms", t);
            }

            return label;
        }

        /// <summary>
        /// 如果不显示天, 显示hh就会显示所有小时, 但是又怕有地方需要这样, 因此这里新开一下
        /// </summary>
        /// <param name="format"></param>
        /// <param name="seconds"></param>
        /// <returns></returns>
        public static string GetTimeStringV2(string format, long seconds)
        {
            string label = format;
            long ms = seconds * 1000;
            int s = (int)seconds;
            int m = s / 60;
            int h = m / 60;
            int d = h / 24;

            string t = "";
            //处理天
            if (label.IndexOf("%dd") >= 0)
            {
                t = d >= 10 ? d.ToString() : ("0" + d.ToString());
                label = label.Replace("%dd", t);
            }
            else if (label.IndexOf("%d") >= 0)
            {
                label = label.Replace("%d", d.ToString());
            }
            h = h % 24;

            //处理小时
            if (label.IndexOf("%hh") >= 0)
            {
                t = h >= 10 ? h.ToString() : ("0" + h.ToString());
                label = label.Replace("%hh", t);
            }
            else if (label.IndexOf("%h") >= 0)
            {
                label = label.Replace("%h", h.ToString());
            }
            m = m % 60;

            //处理分
            if (label.IndexOf("%mm") >= 0)
            {
                t = m >= 10 ? m.ToString() : ("0" + m.ToString());
                label = label.Replace("%mm", t);
            }
            else if (label.IndexOf("%m") >= 0)
            {
                label = label.Replace("%m", m.ToString());
            }
            s = s % 60;

            //处理秒
            if (label.IndexOf("%ss") >= 0)
            {
                t = s >= 10 ? s.ToString() : ("0" + s.ToString());
                label = label.Replace("%ss", t);
            }
            else if (label.IndexOf("%s") >= 0)
            {
                label = label.Replace("%s", s.ToString());
            }
            ms = ms % 1000;

            //处理毫秒
            if (label.IndexOf("ms") >= 0)
            {
                t = ms.ToString();
                label = label.Replace("%ms", t);
            }

            return label;
        }


        public static uint GetUnixTime()
        {
            DateTime startTime = TimeZone.CurrentTimeZone.ToLocalTime(new System.DateTime(1970, 1, 1, 0, 0, 0, 0));

            DateTime nowTime = DateTime.Now;

            uint unixTime = (uint)(Math.Round((nowTime - startTime).TotalMilliseconds, MidpointRounding.AwayFromZero) / 1000);
            return unixTime;
        }

        // 
        public static uint GetUnixTime(DateTime time)
        {
            DateTime startTime = TimeZone.CurrentTimeZone.ToLocalTime(new System.DateTime(1970, 1, 1, 0, 0, 0, 0));

            uint unixTime = (uint)(Math.Round((time - startTime).TotalMilliseconds, MidpointRounding.AwayFromZero) / 1000);
            return unixTime;
        }

        public static DateTime GetLocalTime(uint timeStamp)
        {
            DateTime dtStart = TimeZone.CurrentTimeZone.ToLocalTime(new DateTime(1970, 1, 1));
            long lTime = long.Parse(timeStamp + "0000000");
            TimeSpan toNow = new(lTime);
            DateTime dtResult = dtStart.Add(toNow);
            return dtResult;
        }


        private static uint DAY_PER_YEAR = 365;
        private static uint DAY_PER_MONTH = 30;
        private static uint DAY_PER_WEEK = 7;
        public static string DateStringFromNow(DateTime dt)
        {
            TimeSpan span = DateTime.Now - dt;

            double year = span.TotalDays / DAY_PER_YEAR;
            double month = span.TotalDays / DAY_PER_MONTH;
            double week = span.TotalDays / DAY_PER_WEEK;

            if (year > 1)
            {
                return string.Format("{0}年前", (int)Math.Floor(year));
            }
            else if (month > 1)
            {
                return string.Format("{0}个月前", (int)Math.Floor(month));
            }
            else if (week > 1)
            {
                return string.Format("{0}周前", (int)Math.Floor(week));
            }
            else if (span.TotalDays > 1)
            {
                return string.Format("{0}天前", (int)Math.Floor(span.TotalDays));
            }
            else if (span.TotalHours > 1)
            {
                return string.Format("{0}小时前", (int)Math.Floor(span.TotalHours));
            }
            else if (span.TotalMinutes > 1)
            {
                return string.Format("{0}分钟前", (int)Math.Floor(span.TotalMinutes));
            }
            else
            {
                return
                    //string.Format("{0}秒前", (int)Math.Floor(span.TotalSeconds));
                    "刚才";
            }
        }

        public static string TimeLogString()
        {
            return $"T: {ClientUtcNow} {SGF.Time.TimeUtils.ClientUtcNow.Millisecond + "ms"}";
        }

        /// <summary>
        /// 判断一个时间 是否是 未成年 假日时间
        /// </summary>
        public static bool IsChildHoliday(DateTime dataTime)
        {
            if (dataTime.Hour < 20 || dataTime.Hour >= 21)
            {
                return false;
            }

            string day = dataTime.ToString("MM-dd-yy");
            UnityEngine.Debug.Log($"[sdk] 检查当前 服务器时间: {day}");

            return LocalDataManager.Instance.IsHoliday(day);
        }

        /// <summary>
        /// 是否是 服务器 当前的 假日时间
        /// </summary>
        public static bool IsServerNowHoliday()
        {
            UnityEngine.Debug.Log($"[sdk] 检查当前 服务器时间: {ServerNow} 是否是节假日: {IsChildHoliday(ServerNow)} , 客户端时间: {ClientUtcNow} ");
            // return IsChildHoliday(new DateTime(2024,1,1,20,10,0));
            // return IsChildHoliday(ServerNow) || true;
            return IsChildHoliday(ServerNow);
        }

        /// <summary>
        /// 得到当前的 服务器天数， 需要考虑刷新时间;
        ///     
        /// note:
        ///     比如 定义的 刷新时间是 早上五点,那需要过了 5点 才算进入 新的一天
        /// </summary>
        public static int GetCurServerDay()
        {
            /// <summary>
            /// 当前时间
            /// </summary>
            return ServerNow.Day;
        }

    }
}
