using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Management;
using System.Net;
using System.Net.Sockets;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using WebMaster.CoreManager;

namespace WebMaster
{
    public class BrainConfig
    {


        public const string automessage = "computertype:{7}" + "\r\n" + "CPU:type:{0} core:{1} max:{2}" + "\r\n" + "GPU:type:{3} gpuram:{4}" + "\r\n" + "OSTYPE:{5} RAM MAX:{6}" + "\r\n" + "ip:{9} vln:ipaddress:{8} MAC:{10}";
        public static Tuple<string, string> xinjg;
        public static Tuple<string, string> gtgpu;
        public static long ramget;//获取内存大小
        public static string wintype;
        public static int coresum;
        public static string gtgpuram;
        public static string allram;
        public static string compnm;
        public static string userip;
        public static string alluserip;
        public static string wkMAC;
        public static void autogetuserlisttype()
        {
            xinjg = GetCPU();//获取cpu核心数
            gtgpu = GetVideoController();//获取显卡信息
            ramget = Convert.ToInt64(Convert.ToInt64(GetPhisicalMemory()) / 1024 / 1024);//获取内存大小
            wintype = GetOS_Version();
            coresum = GetCPU_Count();
            gtgpuram = Convert.ToInt32(Convert.ToInt32(gtgpu.Item2) / 1024 / 1024).ToString() + "MB";
            allram = "RAM" + ramget.ToString() + "MB";
            compnm = getcomputerRT();
            userip = GetIpAddressArt();
            alluserip = ALLGetIpAddressArt().Replace("来自", " ");
            wkMAC = GetMacAddress();

        }
        public static string Areporterror(string robot_id, string source, string lineno, string colno, string version, string errormsg)
        {

            string cwmsg = "";

            string[] ipallrt = alluserip.Split('：');
            string autotj = "version=" + version + "&device_name=" + compnm + "&system_version=" + getorsystype() + "&connection_type=" + ipallrt[0] + "&operator=" + ipallrt[1] + "&device_type=" + xinjg.Item1 + "&robot_id=" + robot_id;

            cwmsg = string.Format(automessage, xinjg.Item1, coresum.ToString(), xinjg.Item2, gtgpu.Item1, gtgpuram, wintype, allram, compnm, userip, alluserip, wkMAC);
            autotj = autotj + "&msg=" + cwmsg + "errormsg:" + errormsg;
            return postservice(autotj);

        }
        public static string getorsystype()//获取系统位数
        {
            bool type;
            type = Environment.Is64BitOperatingSystem;
            if (type == true) { return "64"; } else { return "32"; }
        }
        public static string postservice(string data)
        {


            var url = "http://robots.huajuanyun.com/api/error/addErrorLog";
            //通过抓包工具，可以看到是这个格式
            var postData = data;

            HttpWebRequest req = (HttpWebRequest)WebRequest.Create(url);
            req.Method = "POST";
            req.Timeout = 30000;
            req.AllowAutoRedirect = false;
            req.ContentType = "application/x-www-form-urlencoded";
            req.KeepAlive = true;

            byte[] postBytes = Encoding.UTF8.GetBytes(postData);
            req.ContentLength = postBytes.Length;
            Stream postDataStream = req.GetRequestStream();
            postDataStream.Write(postBytes, 0, postBytes.Length);
            postDataStream.Close();

            HttpWebResponse resp = (HttpWebResponse)req.GetResponse();
            string encoding = resp.ContentEncoding; if (encoding == null || encoding.Length < 1)
            {
                encoding = "UTF-8";
            }

            StreamReader reader = new StreamReader(resp.GetResponseStream(), Encoding.GetEncoding(encoding));
            string retstring = reader.ReadToEnd().Replace(@"\", "").Replace(@"\\", "").Replace("\"", "").Replace(",", ""); //111
            return retstring;
        }

        public static Tuple<string, string> GetCPU() //获取处理器型号
        {
            Tuple<string, string> result = null;
            try
            {
                string str = string.Empty;
                ManagementClass mcCPU = new ManagementClass(WindowsAPIType.Win32_Processor.ToString());
                ManagementObjectCollection mocCPU = mcCPU.GetInstances();
                foreach (ManagementObject m in mocCPU)
                {
                    string name = m[WindowsAPIKeys.Name.ToString()].ToString();
                    string[] parts = name.Split('@');
                    result = new Tuple<string, string>(parts[0].Split('-')[0] + "处理器", parts[1]);
                    break;
                }

            }
            catch
            {

            }
            return result;
        }

        public static int GetCPU_Count() //获取cpu核心数
        {
            int str = 0;
            try
            {
                int coreCount = 0;
                foreach (var item in new System.Management.ManagementObjectSearcher("Select * from " +
                WindowsAPIType.Win32_Processor.ToString()).Get())
                {
                    coreCount += int.Parse(item[WindowsAPIKeys.NumberOfCores.ToString()].ToString());
                }
                if (coreCount == 2)
                {
                    return 0;
                }
                str = coreCount;
            }
            catch
            {

            }
            return str;
        }



        public static string GetPhisicalMemory() //获取内存大小
        {
            ManagementObjectSearcher searcher = new ManagementObjectSearcher();   //用于查询一些如系统信息的管理对象 
            searcher.Query = new SelectQuery(WindowsAPIType.Win32_PhysicalMemory.ToString(), "",
 new string[] { WindowsAPIKeys.Capacity.ToString() });//设置查询条件 
            ManagementObjectCollection collection = searcher.Get();   //获取内存容量 
            ManagementObjectCollection.ManagementObjectEnumerator em = collection.GetEnumerator();

            long capacity = 0;
            while (em.MoveNext())
            {
                ManagementBaseObject baseObj = em.Current;
                if (baseObj.Properties[WindowsAPIKeys.Capacity.ToString()].Value != null)
                {
                    try
                    {
                        capacity += long.Parse(baseObj.Properties[WindowsAPIKeys.Capacity.ToString()].Value.ToString());
                    }
                    catch
                    {
                        return "查询失败";
                    }
                }
            }
            return capacity.ToString();
        }

        public static Tuple<string, string> GetVideoController() //获取显卡型号显存大小
        {
            Tuple<string, string> result = null;
            try
            {

                ManagementClass hardDisk = new ManagementClass(WindowsAPIType.Win32_VideoController.ToString());
                ManagementObjectCollection hardDiskC = hardDisk.GetInstances();
                foreach (ManagementObject m in hardDiskC)
                {
                    result = new Tuple<string, string>(m[WindowsAPIKeys.VideoProcessor.ToString()].ToString().Replace("Family", ""), m[WindowsAPIKeys.AdapterRAM.ToString()].ToString());
                    break;
                }
            }
            catch
            {

            }
            return result;
        }

        public static string getcomputerRT()
        { //获取电脑型号
            string shebeixinghao = "nothing";
            ManagementClass mc = new ManagementClass("Win32_ComputerSystem");
            //collection to store all management objects
            ManagementObjectCollection moc = mc.GetInstances();
            if (moc.Count != 0)
            {
                foreach (ManagementObject mo in mc.GetInstances())
                {
                    // display general system information
                    shebeixinghao = mo["Manufacturer"].ToString();

                }
            }
            return shebeixinghao;
        }

        public static string GetIpAddressArt() //获取内网ip地址
        {
            try
            {
                string HostName = Dns.GetHostName(); //得到主机名
                IPHostEntry IpEntry = Dns.GetHostEntry(HostName);
                for (int i = 0; i < IpEntry.AddressList.Length; i++)
                {
                    //从IP地址列表中筛选出IPv4类型的IP地址
                    //AddressFamily.InterNetwork表示此IP为IPv4,
                    //AddressFamily.InterNetworkV6表示此地址为IPv6类型
                    if (IpEntry.AddressList[i].AddressFamily == AddressFamily.InterNetwork)
                    {
                        return IpEntry.AddressList[i].ToString();
                    }
                }
                return "";
            }
            catch (Exception ex)
            {
                return ex.Message;
            }
        }
        public static string ALLGetIpAddressArt() //获取公网ip地址
        {
            string strUrl = "http://2019.ip138.com/ic.asp"; //获得IP的网址了
            Uri uri = new Uri(strUrl);
            WebRequest wr = WebRequest.Create(uri);
            Stream s = wr.GetResponse().GetResponseStream();
            StreamReader sr = new StreamReader(s, Encoding.Default);
            string all = sr.ReadToEnd(); //读取网站的数据
            return RELKO(all, "您的IP是：", "</center>").Replace(" ", "");
        }

        public static string GetOS_Version() //获取系统版本
        {
            string str = "Windows 10";
            try
            {
                string hdId = string.Empty;
                ManagementClass hardDisk = new ManagementClass(WindowsAPIType.Win32_OperatingSystem.ToString());
                ManagementObjectCollection hardDiskC = hardDisk.GetInstances();
                foreach (ManagementObject m in hardDiskC)
                {
                    str = m[WindowsAPIKeys.Name.ToString()].ToString().Split('|')[0].Replace("Microsoft", "");
                    break;
                }
            }
            catch
            {

            }
            return str;
        }


        public static string GetMacAddress() //获取MAC地址
        {
            try
            {
                string mac = "";
                ManagementClass mc = new ManagementClass("Win32_NetworkAdapterConfiguration");
                ManagementObjectCollection moc = mc.GetInstances();
                foreach (ManagementObject mo in moc)
                {
                    if ((bool)mo["IPEnabled"] == true)
                    {
                        mac = mo["MacAddress"].ToString();
                        break;
                    }
                }
                moc = null;
                mc = null;
                return mac;
            }
            catch
            {
                return "unknow";
            }
            finally
            {
            }

        }

        /// <summary>
        /// 获取剩余空间
        /// </summary>
        /// <param name="str_HardDiskName"></param>
        /// <returns></returns>
        public static long GetHardDiskFreeSpace(string str_HardDiskName)
        {
            long freeSpace = new long();
            str_HardDiskName = str_HardDiskName + ":\\";
            System.IO.DriveInfo[] drives = System.IO.DriveInfo.GetDrives();
            foreach (System.IO.DriveInfo drive in drives)
            {
                if (drive.Name == str_HardDiskName)
                {
                    freeSpace = drive.TotalFreeSpace;
                }
            }
            return freeSpace;
        }
        /// <summary>
        /// 获取总空间
        /// </summary>
        /// <param name="str_HardDiskName"></param>
        /// <returns></returns>
        public static long GetHardDiskSpace(string str_HardDiskName)
        {
            long totalSize = new long();
            str_HardDiskName = str_HardDiskName + ":\\";
            System.IO.DriveInfo[] drives = System.IO.DriveInfo.GetDrives();
            foreach (System.IO.DriveInfo drive in drives)
            {
                if (drive.Name == str_HardDiskName)
                {
                    totalSize = drive.TotalSize;
                }
            }
            return totalSize;
        }

        /// <summary>
        /// 将B转换为TB
        /// </summary>
        /// <param name="b"></param>
        /// <returns></returns>
        public static string GetTB(long b)
        {
            for (int i = 0; i < 4; i++)
            {
                b /= 1024;
            }
            return b.ToString();
        }

        /// <summary>
        /// 将B转换为GB
        /// </summary>
        /// <param name="b"></param>
        /// <returns></returns>
        public static string GetGB(long b)
        {
            for (int i = 0; i < 3; i++)
            {
                b /= 1024;
            }
            return b.ToString();
        }

        /// <summary>
        /// 将B转换为MB
        /// </summary>
        /// <param name="b"></param>
        /// <returns></returns>
        public static string GetMB(long b)
        {
            for (int i = 0; i < 2; i++)
            {
                b /= 1024;
            }
            return b.ToString();
        }



        public static bool LockerPerformance = false;
        public static Performance ThisPerformance = new Performance();
        public static object LockerGetPerformanceMessage = new object();

        public static void StartListenService(bool check)
        {
            lock (LockerGetPerformanceMessage)
            {
                if (check == true)
                {
                    if (!LockerPerformance)
                    {
                        LockerPerformance = true;
                        new Thread(() =>
                        {
                            while (LockerPerformance)
                            {


                                long HardDiskSpace = GetHardDiskSpace("C");
                                long HardDiskFreeSpace = GetHardDiskFreeSpace("C");
                                ConfigHelper NConfigHelper = new ConfigHelper();
                                Performance NPerformance = new Performance();
                                NPerformance.ThreadCount = CoreHelper.CurrentBigCall;
                                NPerformance.CpuUsage = NConfigHelper.GetCPUUsage();
                                NPerformance.FreeRam = NConfigHelper.GetRamUsage();
                                NPerformance.SpaceUsage = GetMB(HardDiskSpace) + "/" + GetMB(HardDiskSpace - HardDiskFreeSpace);
                                ThisPerformance = NPerformance;
                                Thread.Sleep(1000);
                            }
                        }).Start();
                    }
                }
                else
                {
                    LockerPerformance = false;
                }
            }
        }
        public static Performance GetPerformanceMessage()
        {
            return ThisPerformance;
        }

    

        public static string RELKO(string text, string str1, string str2)
        {
            try
            {


                int iukA = text.IndexOf(str1) + str1.Length;
                int yukA = text.IndexOf(str2);
                GC.Collect();
                return text.Substring(iukA, yukA - iukA);


            }
            catch { return "error"; }

        }


    }
    [StructLayout(LayoutKind.Sequential)]
    public struct CPU_INFO
    {
        public uint dwOemId;
        public uint dwPageSize;
        public uint lpMinimumApplicationAddress;
        public uint lpMaximumApplicationAddress;
        public uint dwActiveProcessorMask;
        public uint dwNumberOfProcessors;
        public uint dwProcessorType;
        public uint dwAllocationGranularity;
        public uint dwProcessorLevel;
        public uint dwProcessorRevision;
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct SYSTEMTIME_INFO
    {
        public ushort wYear;
        public ushort wMonth;
        public ushort wDayOfWeek;
        public ushort wDay;
        public ushort wHour;
        public ushort wMinute;
        public ushort wSecond;
        public ushort wMilliseconds;
    }


    [StructLayout(LayoutKind.Sequential)]
    public struct MEMORYSTATUSEX
    {
        public uint dwLength;
        public uint dwMemoryLoad;
        public ulong ullTotalPhys;
        public ulong ullAvailPhys;      //可用物理内存
        public ulong ullTotalPageFile;
        public ulong ullAvailPageFile;
        public ulong ullTotalVirtual;
        public ulong ullAvailVirtual;
        public ulong ullAvailExtendedVirtual;
    }

    public class ConfigHelper
    {
        [DllImport("kernel32")]
        public static extern void GetSystemDirectory(StringBuilder
    SysDir, int count);
        [DllImport("kernel32")]
        public static extern void GetSystemInfo(ref CPU_INFO cpuinfo);

        [DllImport("kernel32")]
        public static extern void GetSystemTime(ref SYSTEMTIME_INFO stinfo);


        [DllImport("kernel32.dll")]
        public static extern void GlobalMemoryStatusEx(ref MEMORYSTATUSEX stat);

        public ConfigHelper()
        {
            //// 初始化计数器
            //counters = new PerformanceCounter[System.Environment.ProcessorCount];
            //for (int i = 0; i < counters.Length; i++)
            //{
            //    counters[i] = new PerformanceCounter("Processor", "% Processor Time", i.ToString());
            //    counters[i].NextValue(); // 这里是为了获得CPU占用率的值
            //}
        }
        public int UsingProcess()
        {
            int interval = 1000;
            var prevCpuTime = TimeSpan.Zero;
            Process myproceexe = Process.GetCurrentProcess();
            prevCpuTime = myproceexe.TotalProcessorTime;
            Thread.Sleep(interval);
         
            var value = ((myproceexe.TotalProcessorTime - prevCpuTime).TotalMilliseconds / interval) / Environment.ProcessorCount * 100;
           
        
            return Convert.ToInt32(value);
        }

        // 返回所有核心的CPU的占用率的值
        public int GetCPUUsage()
        {
            return UsingProcess();
        }
        public int GetRamUsage()
        {
            MEMORYSTATUSEX memInfo = new MEMORYSTATUSEX();
            memInfo.dwLength = (uint)Marshal.SizeOf(typeof(MEMORYSTATUSEX));
            GlobalMemoryStatusEx(ref memInfo);

            return Convert.ToInt32(memInfo.ullAvailPhys / 1024 / 1024);
        }
    }
    public class Performance
    {
        public int ThreadCount = 0;
        public int CpuUsage = 0;
        public int FreeRam = 0;
        public string SpaceUsage = "";
    }
    public struct Memory
    {
        public string UsewlMemory { get; set; }
        public string wlMemory { get; set; }
        public string UsexnMemory { get; set; }
        public string xnMemory { get; set; }
    }
    public enum WindowsAPIType
    {
        /// <summary>
        /// 内存
        /// </summary>
        Win32_PhysicalMemory,
        /// <summary>
        /// cpu
        /// </summary>
        Win32_Processor,
        /// <summary>
        /// 硬盘
        /// </summary>
        win32_DiskDrive,
        /// <summary>
        /// 电脑型号
        /// </summary>
        Win32_ComputerSystemProduct,
        /// <summary>
        /// 分辨率
        /// </summary>
        Win32_DesktopMonitor,
        /// <summary>
        /// 显卡
        /// </summary>
        Win32_VideoController,
        /// <summary>
        /// 操作系统
        /// </summary>
        Win32_OperatingSystem

    }
    public enum WindowsAPIKeys
    {
        /// <summary>
        /// 名称
        /// </summary>
        Name,
        /// <summary>
        /// 显卡芯片
        /// </summary>
        VideoProcessor,
        /// <summary>
        /// 显存大小
        /// </summary>
        AdapterRAM,
        /// <summary>
        /// 分辨率宽
        /// </summary>
        ScreenWidth,
        /// <summary>
        /// 分辨率高
        /// </summary>
        ScreenHeight,
        /// <summary>
        /// 电脑型号
        /// </summary>
        Version,
        /// <summary>
        /// 硬盘容量
        /// </summary>
        Size,
        /// <summary>
        /// 内存容量
        /// </summary>
        Capacity,
        /// <summary>
        /// cpu核心数
        /// </summary>
        NumberOfCores
    }

}
