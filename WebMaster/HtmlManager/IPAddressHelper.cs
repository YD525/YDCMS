using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Web;
using WebMaster.DataManager;

namespace WebMaster.HtmlManager
{
    public class IPAddressHelper
    {
        
        public static string GetUserIPAddress(HttpContext LockerHttpContext)
        {
            string strresult = string.Empty;
            strresult = LockerHttpContext.Request.ServerVariables["HTTP_X_FORWARDED_FOR"];
            if (strresult != null && strresult != string.Empty)
            {
               
                if (strresult.IndexOf(".") == -1)    
                {
                    strresult = null;
                }
                else
                {
                    if (strresult.IndexOf(".") != -1)
                    {
                           
                        strresult = strresult.Replace(" ", "").Replace("'", "");
                        string[] strarrtemparyip = strresult.Split(",;".ToCharArray());

                        for (int i = 0; i < strarrtemparyip.Length; i++)
                        {
                            if (IsIPAddress(strarrtemparyip[i]) && strarrtemparyip[i].Substring(0, 3) != "10." && strarrtemparyip[i].Substring(0, 7) != "192.168" && strarrtemparyip[i].Substring(0, 7) != "172.16.")
                            {
                                return strarrtemparyip[i];  
                            }
                        }
                    }
                    else if (IsIPAddress(strresult))    
                    {
                        return strresult;
                    }
                    else
                    {
                        strresult = null;      
                    }
                }
            }
            string strIpAddress = (LockerHttpContext.Request.ServerVariables["HTTP_X_FORWARDED_FOR"] != null && LockerHttpContext.Request.ServerVariables["HTTP_X_FORWARDED_FOR"] != string.Empty ? LockerHttpContext.Request.ServerVariables["HTTP_X_FORWARDED_FOR"] : LockerHttpContext.Request.ServerVariables["REMOTE_ADDR"]);

            if (null == strresult || strresult == string.Empty)
            {
                strresult = LockerHttpContext.Request.ServerVariables["REMOTE_ADDR"];
            }
            if (strresult == null || strresult == string.Empty)
            {
                strresult = LockerHttpContext.Request.UserHostAddress;
            }

            return strresult;
        }
       
        private static bool IsIPAddress(string strIp)
        {
            if (strIp == null || strIp == string.Empty || strIp.Length < 7 || strIp.Length > 15)
            {
                return false;
            }

            string strRegformat = @"^d{1,3}[.]d{1,3}[.]d{1,3}[.]d{1,3}___FCKpd___0quot";

            Regex regex = new Regex(strRegformat, RegexOptions.IgnoreCase);

            return regex.IsMatch(strIp);
        }
        public static UserSource GetUserSource(HttpContext LockerHttpContext)
        {
            UserSource NUserSource = new UserSource();
            string UserIPAddress = GetUserIPAddress(LockerHttpContext);
            NUserSource.UserIPAddress = UserIPAddress;
            try {
                IPLocation ip = GetUserGeographicLocation(UserIPAddress);
                NUserSource.BroadbandOperators = ip.ISP;
                NUserSource.UserFromPlace = ip.Area;
            } catch { }
            return NUserSource;
        }

        public static IPLocation GetUserGeographicLocation(string IPAddress)
        { 
        return new QQWry(DeFine.SystemFilePath + "qqwry.dat").SearchIPLocation(IPAddress);
        }

        public class UserSource
        {
            public string UserIPAddress = "";
            public string BroadbandOperators = "";
            public string UserFromPlace = "";
        }

    }


    // <summary>

    /// IP对象类，该对象包含所属国家和地区

    /// </summary>

    public class IPLocation

    {

        public string ISP { get; set; }

        public string Area { get; set; }

    }


    /// <summary>

    /// 读取QQ纯真IP数据库 

    /// </summary>    

    public class QQWry

    {

        //第一种模式 

        private const byte REDIRECT_MODE_1 = 0x01;



        //第二种模式 

        private const byte REDIRECT_MODE_2 = 0x02;



        //每条记录长度 

        private const int IP_RECORD_LENGTH = 7;



        //数据库文件 

        private FileStream ipFile = null;



        private const string unISP = "未知ISP";

        private const string unArea = "未知地区";



        //索引开始位置 

        private long ipBegin;



        //索引结束位置 

        private long ipEnd;



        //IP地址对象 

        private IPLocation loc;



        //存储文本内容 

        private byte[] buf;



        //存储3字节 

        private byte[] b3;



        //存储4字节 

        private byte[] b4;



 

        public QQWry(string ipfile)
        {
            buf = new byte[100];

            b3 = new byte[3];

            b4 = new byte[4];

            lock (this)
            {
                GC.Collect();

                ipFile = new System.IO.FileStream(ipfile, System.IO.FileMode.Open, System.IO.FileAccess.Read, FileShare.ReadWrite); ;

                ipBegin = ReadLong4(0);

                ipEnd = ReadLong4(4);

                loc = new IPLocation();
            }

        }
      


        /// <summary> 

        /// 搜索IP地址搜索 

        /// </summary> 

        /// <param name="ip"></param> 

        /// <returns></returns> 

        public IPLocation SearchIPLocation(string ip)

        {

            //将字符IP转换为字节 

            string[] ipSp = ip.Split('.');

            if (ipSp.Length != 4)

            {

                ip = "127.0.0.1";

                ipSp = ip.Split('.');

            }

            byte[] IP = new byte[4];

            for (int i = 0; i < IP.Length; i++)

            {

                IP[i] = (byte)(Int32.Parse(ipSp[i]) & 0xFF);

            }



            IPLocation local = null;

            long offset = LocateIP(IP);



            if (offset != -1)

            {

                local = getIPLocation(offset);

            }



            if (local == null)

            {

                local = new IPLocation();

                local.Area = unArea;

                local.ISP = unISP;

            }
            ipFile.Close();
            ipFile.Dispose();
            return local;

        }



        /// <summary> 

        /// 取得具体信息 

        /// </summary> 

        /// <param name="offset"></param> 

        /// <returns></returns> 

        private IPLocation getIPLocation(long offset)

        {

            ipFile.Position = offset + 4;

            //读取第一个字节判断是否是标志字节 

            byte one = (byte)ipFile.ReadByte();

            if (one == REDIRECT_MODE_1)

            {

                //第一种模式 

                //读取国家偏移 

                long countryOffset = ReadLong3();

                //转至偏移处 

                ipFile.Position = countryOffset;

                //再次检查标志字节 

                byte b = (byte)ipFile.ReadByte();

                if (b == REDIRECT_MODE_2)

                {

                    loc.Area = ReadString(ReadLong3());

                    ipFile.Position = countryOffset + 4;

                }

                else

                    loc.Area = ReadString(countryOffset);



                //读取运营商标志

                loc.ISP = ReadArea(ipFile.Position);



            }

            else if (one == REDIRECT_MODE_2)

            {

                //第二种模式 

                loc.Area = ReadString(ReadLong3());

                loc.ISP = ReadArea(offset + 8);

            }

            else

            {

                //普通模式 

                loc.Area = ReadString(--ipFile.Position);

                loc.ISP = ReadString(ipFile.Position);

            }

            ipFile.Close();

            return loc;

        }



        /// <summary> 

        /// 读取地区名称 

        /// </summary> 

        /// <param name="offset"></param> 

        /// <returns></returns> 

        private string ReadArea(long offset)

        {

            ipFile.Position = offset;

            byte one = (byte)ipFile.ReadByte();

            if (one == REDIRECT_MODE_1 || one == REDIRECT_MODE_2)

            {

                long areaOffset = ReadLong3(offset + 1);

                return (areaOffset == 0) ? unArea : ReadString(areaOffset);

            }

            else

            {

                return ReadString(offset);

            }

        }



        /// <summary> 

        /// 读取字符串 

        /// </summary> 

        /// <param name="offset"></param> 

        /// <returns></returns> 

        private string ReadString(long offset)

        {

            ipFile.Position = offset;

            int i = 0;

            for (i = 0, buf[i] = (byte)ipFile.ReadByte(); buf[i] != (byte)(0); buf[++i] = (byte)ipFile.ReadByte()) ;

            return (i > 0) ? Encoding.Default.GetString(buf, 0, i) : "";

        }



        /// <summary> 

        /// 查找IP地址所在的绝对偏移量 

        /// </summary> 

        /// <param name="ip"></param> 

        /// <returns></returns> 

        private long LocateIP(byte[] ip)

        {

            long m = 0;

            int r;



            //比较第一个IP项 

            ReadIP(ipBegin, b4);

            r = CompareIP(ip, b4);

            if (r == 0)

                return ipBegin;

            else if (r < 0)

                return -1;

            //开始二分搜索 

            for (long i = ipBegin, j = ipEnd; i < j;)

            {

                m = this.GetMiddleOffset(i, j);

                ReadIP(m, b4);

                r = CompareIP(ip, b4);

                if (r > 0)

                    i = m;

                else if (r < 0)

                {

                    if (m == j)

                    {

                        m = j -= IP_RECORD_LENGTH;

                    }

                    else

                    {

                        j = m;

                    }

                }

                else

                    return ReadLong3(m + 4);

            }

            m = ReadLong3(m + 4);

            ReadIP(m, b4);

            return (CompareIP(ip, b4) <= 0) ? m : -1;

        }



        /// <summary> 

        /// 从当前位置读取四字节,此四字节是IP地址 

        /// </summary> 

        /// <param name="offset"></param> 

        /// <param name="ip"></param> 

        private void ReadIP(long offset, byte[] ip)

        {

            ipFile.Position = offset;

            ipFile.Read(ip, 0, ip.Length);

            byte tmp = ip[0];

            ip[0] = ip[3];

            ip[3] = tmp;

            tmp = ip[1];

            ip[1] = ip[2];

            ip[2] = tmp;

        }



        /// <summary> 

        /// 比较IP地址是否相同 

        /// </summary> 

        /// <param name="ip"></param> 

        /// <param name="beginIP"></param> 

        /// <returns>0:相等,1:ip大于beginIP,-1:小于</returns> 

        private int CompareIP(byte[] ip, byte[] beginIP)

        {

            for (int i = 0; i < 4; i++)

            {

                int r = CompareByte(ip[i], beginIP[i]);

                if (r != 0)

                    return r;

            }

            return 0;

        }



        /// <summary> 

        /// 比较两个字节是否相等 

        /// </summary> 

        /// <param name="bsrc"></param> 

        /// <param name="bdst"></param> 

        /// <returns></returns> 

        private int CompareByte(byte bsrc, byte bdst)

        {

            if ((bsrc & 0xFF) > (bdst & 0xFF))

                return 1;

            else if ((bsrc ^ bdst) == 0)

                return 0;

            else

                return -1;

        }



        /// <summary> 

        /// 从当前位置读取4字节,转换为长整型 

        /// </summary> 

        /// <param name="offset"></param> 

        /// <returns></returns> 

        private long ReadLong4(long offset)

        {

            long ret = 0;

            ipFile.Position = offset;

            ret |= (ipFile.ReadByte() & 0xFF);

            ret |= ((ipFile.ReadByte() << 8) & 0xFF00);

            ret |= ((ipFile.ReadByte() << 16) & 0xFF0000);

            ret |= ((ipFile.ReadByte() << 24) & 0xFF000000);



            return ret;

        }



        /// <summary> 

        /// 根据当前位置,读取3字节 

        /// </summary> 

        /// <param name="offset"></param> 

        /// <returns></returns> 

        private long ReadLong3(long offset)

        {

            long ret = 0;

            ipFile.Position = offset;

            ret |= (ipFile.ReadByte() & 0xFF);

            ret |= ((ipFile.ReadByte() << 8) & 0xFF00);

            ret |= ((ipFile.ReadByte() << 16) & 0xFF0000);

            return ret;

        }



        /// <summary> 

        /// 从当前位置读取3字节 

        /// </summary> 

        /// <returns></returns> 

        private long ReadLong3()

        {

            long ret = 0;

            ret |= (ipFile.ReadByte() & 0xFF);

            ret |= ((ipFile.ReadByte() << 8) & 0xFF00);

            ret |= ((ipFile.ReadByte() << 16) & 0xFF0000);

            return ret;

        }



        /// <summary> 

        /// 取得begin和end中间的偏移 

        /// </summary> 

        /// <param name="begin"></param> 

        /// <param name="end"></param> 

        /// <returns></returns> 

        private long GetMiddleOffset(long begin, long end)

        {

            long records = (end - begin) / IP_RECORD_LENGTH;

            records >>= 1;

            if (records == 0)

                records = 1;

            return begin + records * IP_RECORD_LENGTH;

        }

    }
}
