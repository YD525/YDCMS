using System;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using System.Web;
using WebMaster.HtmlManager;
using WebMaster.UserManager;

namespace WebMaster.WebManager
{
    public class WebDefence
    {

        [System.Runtime.InteropServices.DllImportAttribute("kernel32.dll", EntryPoint = "SetProcessWorkingSetSize", ExactSpelling = true, CharSet =
        System.Runtime.InteropServices.CharSet.Ansi, SetLastError = true)]
        private static extern int SetProcessWorkingSetSize(IntPtr process, int minimumWorkingSetSize, int maximumWorkingSetSize);
        private static bool FristDefenceServiceLock = false;

        public static List<UserConnect> AllConnect = new List<UserConnect>();


        public static void StartDefenceService(bool check)
        {
            if (check)
            {
                if (!FristDefenceServiceLock)
                {
                    FristDefenceServiceLock = check;
                    new Thread(() =>
                    {
                        while (FristDefenceServiceLock)
                        {
                            Thread.Sleep(300);
                            for (int i = 0; i < AllConnect.Count; i++)
                            {
                                if (AllConnect[i].MaxConnect > 0)
                                {
                                    AllConnect[i].MaxConnect--;
                                }
                            }
                        }
                    }).Start();

                    new Thread(() =>
                    {
                        while (FristDefenceServiceLock)
                        {
                            Thread.Sleep(5000);
                            RAMClean();
                        }
                    }).Start();
                }
            }
            else
            {
                FristDefenceServiceLock = check;
            }

        }
        public static object LockerConnect = new object();
        public static void NewConnect(string IpAddress)
        {
            lock (LockerConnect)
            {
                bool NewUser = true;
                foreach (var get in AllConnect)
                {
                    if (get.UserIP == null == false)
                    {
                        if (get.UserIP == IpAddress)
                        {
                            NewUser = false;
                        }
                    }
                }
                if (!NewUser)
                {
                    for (int i = 0; i < AllConnect.Count; i++)
                    {
                        if (AllConnect[i].UserIP == IpAddress)
                        {
                            if (AllConnect[i].MaxConnect<99999)AllConnect[i].MaxConnect++;
                        }
                    }
                }
                else
                {
                    AllConnect.Add(new UserConnect(IpAddress, 0));
                }
            }
        }

        public static void RAMClean()
        {
            try
            {
                if (Environment.OSVersion.Platform == PlatformID.Win32NT)
                {
                    SetProcessWorkingSetSize(System.Diagnostics.Process.GetCurrentProcess().Handle, -1, -1);
                }
            }
            catch { }
        }
        public static int IsConnect(string IpAddress)
        {
            foreach (var get in AllConnect)
            {
                if (get.UserIP == IpAddress)
                {
                    return get.MaxConnect;
                }
            }
            return 0;
        }

        public static List<UserConnect> GetAllConnect()
        {
            return AllConnect;
        }

        /// <summary>
        /// 判断参数里是否存在sql指令
        /// </summary>
        /// <param name="InText"></param>
        /// <returns></returns>
        public static bool SqlFilter(string InText)
        {
            string word = "and|exec|insert|select|delete|update|chr|mid|master|or|truncate|char|declare|join|cmd";
            if (InText == null)
                return false;
            foreach (string i in word.Split('|'))
            {
                if ((InText.ToLower().IndexOf(i + " ") > -1) || (InText.ToLower().IndexOf(" " + i) > -1))
                {
                    return true;
                }
            }
            return false;
        }

        public static bool CheckSignature(string FilePath)
        {
            try
            {
                X509Certificate cert = X509Certificate.CreateFromSignedFile(FilePath);
                if (cert == null) return false;
                return true;
            }
            catch
            {
                return false;
            }
        }


        public static ScanActionMessage CurrentScanMessage = new ScanActionMessage();

        private static object StartWebLocker = new object();
        public static List<ScanMessage> ScanMessageList = new List<ScanMessage>();
        public static int WorkingScanThread = 0;

        public static string ReadFile(string path)
        {
            try {
                string allcode = "";
                FileStream fs = new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.ReadWrite);
                StreamReader sr = new StreamReader(fs, System.Text.Encoding.UTF8);
                allcode = sr.ReadToEnd();
                fs.Close();
                sr.Close();
                sr.Dispose();
                return allcode;
            }
            catch 
            {
                return "";
            }
        }
        /// <summary>
        /// 开始网站安全扫描
        /// </summary>
        /// <param name="ParentPath"></param>
        public static void StartWebScan(string ParentPath)
        {
            new Thread(() =>
            {
                lock (StartWebLocker)
                {
                    try
                    {
                        WorkingScanThread++;
                        ScanMessageList.Clear();
                        DataTable Inspire = SqlServerHelper.ExecuteDataTable("Select * From ThreatLibrary Where MD5 =''");

                        string DeadChain = "please check filepath: {0} url:{1} code:{2} is DeadChain";
                        string ThreatFile = "Dangerous find malicious files filepath:{0} Typeis:{1}";
                        string External = "Links to external sites :{1} code:{2} filepath:{0}";

                        string GetDefUrl = CMSHelper.GetDeFineValue("DEFURL");
                        List<FileInformation> FileList = DataHelper.getallfile(ParentPath, "", null);
                        decimal ScanItem = 0;
                        decimal AllCount = (decimal)FileList.Count;

                        foreach (var FileItem in FileList)
                        {
                            ScanMessage NScanMessage = new ScanMessage();
                            ScanItem++;
                            NScanMessage.FilePath = FileItem.FilePath;
                            NScanMessage.FileName = FileItem.FileName;
                            NScanMessage.FileMD5 = GetFileMD5(FileItem.FilePath);
                            ScanActionMessage NScanActionMessage = new ScanActionMessage();
                            NScanActionMessage.CurrentPath = FileItem.FilePath;
                            NScanActionMessage.CurrentFileMD5 = NScanMessage.FileMD5;
                            decimal ScanRate = decimal.Parse((ScanItem / AllCount).ToString("0.000"));
                            var LockerRate = Math.Round(ScanRate, 2);
                            NScanActionMessage.CurrentScanRate = Convert.ToInt32((LockerRate * 100)).ToString();
                            NScanActionMessage.IsOver = false.ToString();
                            CurrentScanMessage = NScanActionMessage;
                            if (NScanMessage.FileMD5.Replace(" ","").Length > 0)
                            {
                                DataTable NTable = SqlServerHelper.ExecuteDataTable("Select * From ThreatLibrary Where MD5 ='" + NScanMessage.FileMD5 + "'");
                                if (NTable.Rows.Count > 0)
                                {
                                    NScanMessage.ThreatLevel += 5;
                                    NScanMessage.Message += "检测威胁:" + string.Format(ThreatFile, NScanMessage.FilePath, DataHelper.ObjToStr(NTable.Rows[0]["ThreatName"])) + "\r\n";
                                }
                            }
                      
                            bool CheckCode = false;
                            if (NScanMessage.FilePath.ToLower().EndsWith(".htm")) CheckCode = true;
                            if (NScanMessage.FilePath.ToLower().EndsWith(".rt")) CheckCode = true;
                            if (NScanMessage.FilePath.ToLower().EndsWith(".cof")) CheckCode = true;

                            string AllCode = ""; 
                            if (!NScanMessage.FilePath.ToLower().EndsWith(".dll"))
                            {
                                if (!NScanMessage.FilePath.ToLower().EndsWith(".pdb"))
                                {
                                    if (!NScanMessage.FilePath.ToLower().EndsWith(".xml"))
                                    AllCode = ReadFile(FileItem.FilePath);
                                }
                            }
                            string CheckFileCode = AllCode.ToLower().Replace(" ", "").Replace("\r\n","").Replace("\n","").Replace(" ","");
                            for (int i = 0; i < Inspire.Rows.Count; i++)
                            {
                                int Satisfy = 0;

                                int MaxRows = DataHelper.StrToInt(DataHelper.ObjToStr(Inspire.Rows[i]["MaxRows"]));
                                string Suffix = DataHelper.ObjToStr(Inspire.Rows[i]["Suffix"]);
                                string MaliciousString = DataHelper.ObjToStr(Inspire.Rows[i]["MaliciousString"]);
                                bool IsShell = DataHelper.StrToBool(DataHelper.ObjToStr(Inspire.Rows[i]["IsShell"]));
                                bool DocumentSignature = DataHelper.StrToBool(DataHelper.ObjToStr(Inspire.Rows[i]["DocumentSignature"]));
                                int SatisfyCount = DataHelper.StrToInt(DataHelper.ObjToStr(Inspire.Rows[i]["SatisfyCount"]));
                                string ThreatName = DataHelper.ObjToStr(Inspire.Rows[i]["ThreatName"]);

                                if (MaxRows == 0 == false)
                                {
                                    if (AllCode.Split(new char[2] { '\r', '\n' }).Length < MaxRows)
                                    {
                                        Satisfy++;
                                    }
                                }
                                if (Suffix == "" == false)
                                {
                                    if (NScanMessage.FileName.ToLower().EndsWith(Suffix.ToLower()))
                                    {
                                        Satisfy++;
                                    }
                                }
                                if (MaliciousString == "" == false)
                                {
                                    if (CheckFileCode.Contains(MaliciousString.ToLower().Replace(" ","")))
                                    {
                                        Satisfy++;
                                    }
                                }
                                if (IsShell)
                                {
                                    string GetName = FileItem.FileName;
                                    if (GetName.Contains(".")) GetName = GetName.Split('.')[0];
                                    if (Process.GetProcessesByName(GetName).Length > 0)
                                    {
                                        Satisfy++;
                                    }
                                }
                                if (DocumentSignature)
                                {
                                    if (!CheckSignature(FileItem.FilePath))
                                    {
                                        Satisfy++;
                                    }
                                }
                                if (SatisfyCount == 0 == false)
                                {
                                    if (Satisfy >= SatisfyCount)
                                    {
                                        NScanMessage.ThreatLevel += 5;
                                        NScanMessage.Message +="启发式引擎:"+ string.Format(ThreatFile, NScanMessage.FilePath, ThreatName) + "\r\n";
                                        break;
                                    }
                                }
                            }
                            if (CheckCode)
                            {
                                Regex reg = new Regex(@"(http|https)://(?<Url>[^(:|\""']*)");
                                MatchCollection mc = reg.Matches(AllCode);
                                foreach (Match m in mc)
                                {
                                    string GetHref = m.Groups["Url"].Value;
                                    if (!GetHref.Trim().StartsWith(GetDefUrl))
                                    {
                                        if (!GetHref.ToLower().Contains("www.w3.org"))
                                        {
                                            if (!CheckUrlVisit(GetHref))
                                            {
                                                //死链
                                                NScanMessage.ThreatLevel++;
                                                NScanMessage.Message +="行为优化引擎:"+ string.Format(DeadChain, NScanMessage.FilePath, GetHref, m.Value) + "\r\n";
                                            }
                                            else
                                            {
                                                //外链
                                                NScanMessage.Message +="行为优化引擎:"+ string.Format(External, NScanMessage.FilePath, GetHref, m.Value) + "\r\n";
                                            }
                                        }
                                    }

                                }
                            }
                            if (NScanMessage.Message.Length > 0)
                            {
                                ScanMessageList.Add(NScanMessage);
                            }
                        }
                        WorkingScanThread--;
                        GC.Collect();
                        CurrentScanMessage = new ScanActionMessage();
                    }
                    catch
                    {
                        WorkingScanThread--;
                        GC.Collect();
                        CurrentScanMessage = new ScanActionMessage();
                    }
                }
            }).Start();
        }

        public static bool CheckUrlVisit(string url)
        {
            try
            {
                HttpWebRequest req = (HttpWebRequest)WebRequest.Create(url);
                HttpWebResponse resp = (HttpWebResponse)req.GetResponse();
                if (resp.StatusCode == HttpStatusCode.OK)
                {
                    resp.Close();
                    return true;
                }
            }
            catch
            {
                return false;
            }
            return false;
        }

        public static string GetFileMD5(string strFileFullPath)
        {
            System.IO.FileStream fst = null;
            try
            {
                fst = new System.IO.FileStream(strFileFullPath, System.IO.FileMode.Open);
                System.Security.Cryptography.MD5 md5 = new System.Security.Cryptography.MD5CryptoServiceProvider();
                byte[] data = md5.ComputeHash(fst);
                StringBuilder sBuilder = new StringBuilder();
                for (int i = 0; i < data.Length; i++)
                {
                    sBuilder.Append(data[i].ToString("x2"));
                }
                fst.Close();
                return sBuilder.ToString().ToLower();
            }
            catch (System.Exception ex)
            {
                if (fst != null)
                    fst.Close();
                return "";
            }
            finally
            {
            }
        }


        /// <summary>
        /// 防止渗透注入
        /// </summary>
        /// <param name="str"></param>
        /// <param name="format"></param>
        /// <returns></returns>

        public static string InuptValueByNoSQLOder(string str, string format)
        {
            if (SqlFilter(str))
            {
                string TryRePair = str.ToLower().Replace("and", "").Replace("exec", "").Replace("insert", "").Replace("delete", "").Replace("update", "").Replace("chr", "").Replace("=", "");
                if (!SqlFilter(TryRePair)) return TryRePair;
                return "";
            }
            else
            {
                if (format == "HtmlCode")
                {
                    return str;
                }

                if (format == "HtmlEncode")
                {
                    return HttpUtility.HtmlEncode(str);
                }

                return "Nothing";
            }
        }

    }

    public class UserConnect
    {
        public string UserIP = "";
        public int MaxConnect = 0;
        public UserConnect()
        {

        }

        public UserConnect(string UserIP, int MaxConnect)
        {
            this.UserIP = UserIP;
            this.MaxConnect = MaxConnect;
        }
    }

    public class ScanMessage
    {
        public string FileName = "";
        public string FilePath = "";
        public string FileMD5 = "";
        public string Message = "";
        public int ThreatLevel = 0;
    }

    public class ScanActionMessage
    {
        public string CurrentPath = "";
        public string CurrentFileMD5 = "";
        public string CurrentScanRate = "";

        public DateTime StartTime = DateTime.Now;
        public string IsOver = "True";
    }
}
