
using LumiSoft.Net.Mail;
using LumiSoft.Net.POP3.Client;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Mail;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using System.Web.Security;
using WebMaster.DataManager;
using WebMaster.SQLManager;

namespace WebMaster.HtmlManager
{
    public class EmailHelper
    {



        private static List<POP3_Client> AllPopClient = new List<POP3_Client>();
        private static List<SmtpClient> AllSmtpClient = new List<SmtpClient>();

        public static bool FristLoadINFO = false;
        public static void Initialize()
        {
            if (!FristLoadINFO)
            {
                string CesGet = DeFine.DefCurrentDirectory + "system.db";
                SQLiteHelper.OpenSql(DeFine.DefCurrentDirectory + "system.db");
                FristLoadINFO = true;
            }
                
        }
      


        /// <summary>
        /// 添加邮件配置信息
        /// </summary>
        /// <param name="stmpusername"></param>
        /// <param name="stmppassword"></param>
        /// <param name="popusername"></param>
        /// <param name="poppassword"></param>
        /// <param name="type"></param>
        /// <param name="Lockrview"></param>
        /// <returns></returns>
        public static bool AddEmailInfor(string stmpusername, string stmppassword, string popusername, string poppassword, string type)
        {
            EmailInfo NInfo = new EmailInfo();
            NInfo.SmtpUserName = stmpusername;
            NInfo.SmtpPassword = stmppassword;
            NInfo.PoPUserName = popusername;
            NInfo.PoPPassword = poppassword;
            NInfo.EmailType = type;

            if (type == "QQ邮箱")
            {
                NInfo.SmtpHost = "smtp.qq.com";
                NInfo.SmtpPort = 587;
                NInfo.SmtpUseSsl = true;
                NInfo.PoPHost = "pop.qq.com";
                NInfo.PoPPort = 995;
                NInfo.pop3UseSsl = true;
            }
            if (type == "163邮箱")
            {
                NInfo.SmtpHost = "smtp.163.com";
                NInfo.SmtpPort = 465;
                NInfo.SmtpUseSsl = true;
                NInfo.PoPHost = "pop.163.com";
                NInfo.PoPPort = 995;
                NInfo.pop3UseSsl = true;
            }


            bool getinfo = AddEmailInfor(NInfo);
            if (getinfo)
            {
            return true;
            }
            else 
            {
            return false;
            }
        }



        /// <summary>
        /// 添加邮件信息方法重写
        /// </summary>
        /// <param name="NInfo"></param>
        /// <returns></returns>
        public static bool AddEmailInfor(EmailInfo NInfo)
        {

            bool iserror = false;
            POP3_Client Npop = POPConnect(NInfo.PoPHost, NInfo.PoPPort, NInfo.pop3UseSsl, NInfo.PoPUserName, NInfo.PoPPassword);
            if (Npop == null == false)
            {
                if (!Npop.IsConnected)
                {
                    iserror = true;
                }
            }
            else
            {
                iserror = true;
            }

            SmtpClient NSmtp = SmtpConnect(NInfo.SmtpHost, NInfo.SmtpPort, NInfo.SmtpUseSsl, NInfo.SmtpUserName, NInfo.SmtpPassword);

            if (NSmtp == null)
            {
                iserror = true;
            }
            if (iserror) { return false; }

            string sqloder = "INSERT INTO MailInfoList(EmailType,SmtpHost,SmtpPort,SmtpUseSSL,SmtpUserName,SmtpPassword,PoPHost,PoPPort,PoP3UseSSL,PoPUserName,PoPPassword) VALUES ('{0}','{1}','{2}','{3}','{4}','{5}','{6}','{7}','{8}','{9}','{10}')";
            int get = SQLiteHelper.ExecuteNonQuery(string.Format(sqloder, NInfo.EmailType, NInfo.SmtpHost, NInfo.SmtpPort, NInfo.SmtpUseSsl, NInfo.SmtpUserName, NInfo.SmtpPassword, NInfo.PoPHost, NInfo.PoPPort, NInfo.pop3UseSsl, NInfo.PoPUserName, NInfo.PoPPassword));
            if (get == 0 == false)
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        /// <summary>
        /// pop连接
        /// </summary>
        /// <param name="pop3Server"></param>
        /// <param name="pop3Port"></param>
        /// <param name="pop3UseSsl"></param>
        /// <param name="username"></param>
        /// <param name="password"></param>
        /// <returns></returns>

        public static POP3_Client POPConnect(string pop3Server, int pop3Port, bool pop3UseSsl, string username, string password)
        {
            POP3_Client pop3 = new POP3_Client();
            try
            {
                //与Pop3服务器建立连接
                pop3.Connect(pop3Server, pop3Port, pop3UseSsl);
                //验证身份
                pop3.Login(username, password);

                return pop3;
            }
            catch { return null; }
        }

        /// <summary>
        /// smtp连接
        /// </summary>
        /// <param name="stmpserver"></param>
        /// <param name="stmpport"></param>
        /// <param name="username"></param>
        /// <param name="password"></param>
        /// <returns></returns>
        public static SmtpClient SmtpConnect(string stmpserver, int stmpport, bool stmpUseSsl, string username, string password)
        {
            NetworkCredential myCredentials = new NetworkCredential(username, password);
            SmtpClient SmtpClient = new SmtpClient();
            try
            {
                SmtpClient.Credentials = myCredentials;
                SmtpClient.Port = stmpport;
                SmtpClient.Host = stmpserver;
                SmtpClient.EnableSsl = stmpUseSsl;
                return SmtpClient;
            }
            catch { return null; }
        }




        /// <summary>
        /// 取指定EmailInfo
        /// </summary>
        /// <param name="type"></param>
        /// <returns></returns>
        public static List<EmailInfo> GetSQLMailInfo(string type)
        {
            List<EmailInfo> allemailinfo = new List<EmailInfo>();
            DataTable get = SQLiteHelper.ExecuteDataTable("Select rowid, * From MailInfoList Where EmailType = '" + type + "'");
            for (int i = 0; i < get.Rows.Count; i++)
            {
                EmailInfo NInfo = new EmailInfo();
                NInfo.ID = int.Parse(get.Rows[i]["rowid"].ToString());
                NInfo.EmailType = get.Rows[i]["EmailType"].ToString();
                NInfo.SmtpHost = get.Rows[i]["SmtpHost"].ToString();
                NInfo.SmtpPort = int.Parse(get.Rows[i]["SmtpPort"].ToString());
                NInfo.SmtpUserName = get.Rows[i]["SmtpUserName"].ToString();
                NInfo.SmtpPassword = get.Rows[i]["SmtpPassword"].ToString();
                NInfo.PoPHost = get.Rows[i]["PoPHost"].ToString();
                NInfo.PoPPort = int.Parse(get.Rows[i]["PoPPort"].ToString());
                NInfo.pop3UseSsl = bool.Parse(get.Rows[i]["PoP3UseSSL"].ToString());
                NInfo.PoPUserName = get.Rows[i]["PoPUserName"].ToString();
                NInfo.PoPPassword = get.Rows[i]["PoPPassword"].ToString();
                allemailinfo.Add(NInfo);
            }
            return allemailinfo;

        }
        public static bool ServiceLocker = false;
        public static void EmailService(bool check)
        {
            if (ServiceLocker == check) { return; }
            ServiceLocker = check;
            if (ServiceLocker)
            {

                new Thread(() =>
                {
                    while (true)
                    {
                        Thread.Sleep(1000);
                        try
                        {
                            DataTable GetInfoList = SQLiteHelper.ExecuteDataTable("Select rowid,* From MailInfoList");
                            for (int i = 0; i < GetInfoList.Rows.Count; i++)
                            {
                                Thread.Sleep(1000);
                                string EmailAddress = GetInfoList.Rows[i]["SmtpUserName"].ToString();
                                string EmailType = GetInfoList.Rows[i]["EmailType"].ToString();
                                string PoPHost = GetInfoList.Rows[i]["PoPHost"].ToString();
                                string PoPPort = GetInfoList.Rows[i]["PoPPort"].ToString();
                                string PoP3UseSSL = GetInfoList.Rows[i]["PoP3UseSSL"].ToString();
                                string PoPUserName = GetInfoList.Rows[i]["PoPUserName"].ToString();
                                string PoPPassword = GetInfoList.Rows[i]["PoPPassword"].ToString();
                                var GetAllMails = GetEmails(POPConnect(PoPHost, int.Parse(PoPPort), bool.Parse(PoP3UseSSL), PoPUserName, PoPPassword));
                                if (GetAllMails.Count > 0)
                                {
                                    foreach (var get in GetAllMails)
                                    {
                                        DataTable GetEmailList = SQLiteHelper.ExecuteDataTable("Select rowid,* From EmailList Where EmailID='" + get.MailID + "'");
                                        if (GetEmailList.Rows.Count > 0 == false)
                                        {
                                            //ContentDisposition = {Content-Disposition: attachment; filename="compatibility_matrix.xml"
                                            List<FileCls> AllFile = new List<FileCls>();
                                            foreach (var FileItems in get.MailBody.Attachments)
                                            {
                                                string GetPath = DeFine.DefCurrentDirectory;
                                                FileCls NFile = new FileCls();
                                                NFile.FileName = FileItems.ContentDisposition.Param_FileName;
                                                FileItems.DataToFile(GetPath + @"\EmailFile\" + NFile.FileName);
                                                NFile.FilePath = GetPath + @"\EmailFile\" + NFile.FileName;
                                                AllFile.Add(NFile);
                                                GC.Collect();
                                            }
                                            string GetBodyText = get.MailBody.BodyText;
                                            string GetBodyHtmlText = get.MailBody.BodyHtmlText;
                                            string AUTOText = "收件";

                                            string PFget = FindGarbage(get);
                                            if (PFget.Contains("垃圾"))
                                            {
                                                AUTOText = PFget.Split(':')[0];
                                            }

                                            if (GetBodyText == null)
                                            {
                                                GetBodyText = GetBodyHtmlText;
                                            }
                                            string sqlloader = "INSERT INTO EmailList(EmailID,EmailAddress,TargetAddress,Tittle,RichText,EmailText,EmailFile,EmailType,EmailTextType,State) VALUES('{0}', '{1}', '{2}', '{3}', '{4}', '{5}', '{6}', '{7}', '{8}', '{9}')";
                                            SQLiteHelper.ExecuteNonQuery(string.Format(sqlloader, get.MailID, get.MailBody.From[0], EmailAddress, get.MailBody.Subject, Encrypt(GetBodyText), Encrypt(GetBodyHtmlText), Encrypt(JsonConvert.SerializeObject(AllFile)), EmailType, PFget, AUTOText));
                                            string GetInupt = "";
                                            if (get.MailBody.BodyText == null == false)
                                            {
                                                if (get.MailBody.BodyText.Replace("\r\n", "").Length > 80)
                                                {
                                                    GetInupt = get.MailBody.BodyText.Replace("\r\n", "").Substring(0, 80) + "...";
                                                }
                                                else
                                                {
                                                    GetInupt = get.MailBody.BodyText.Replace("\r\n", "");
                                                }
                                            }
                                            
                                        }

                                    }
                                }
                            }
                        }
                        catch
                        {

                        }
                    }
                }).Start();

            }


        }
        /// <summary>
        /// 删除指定邮箱配置
        /// </summary>
        /// <param name="rowid"></param>
        /// <returns></returns>

        public static int DelectMailInfo(string rowid)
        {
            return SQLiteHelper.ExecuteNonQuery("Delete from MailInfoList where rowid=" + rowid);
        }

        public static string Encrypt(string Text, string sKey = "yueding")
        {
            try
            {
                DESCryptoServiceProvider descryptoServiceProvider = new DESCryptoServiceProvider();
                byte[] bytes = Encoding.Default.GetBytes(Text);
                descryptoServiceProvider.Key = Encoding.ASCII.GetBytes(FormsAuthentication.HashPasswordForStoringInConfigFile(sKey, "md5").Substring(0, 8));
                descryptoServiceProvider.IV = Encoding.ASCII.GetBytes(FormsAuthentication.HashPasswordForStoringInConfigFile(sKey, "md5").Substring(0, 8));
                MemoryStream memoryStream = new MemoryStream();
                CryptoStream cryptoStream = new CryptoStream(memoryStream, descryptoServiceProvider.CreateEncryptor(), CryptoStreamMode.Write);
                cryptoStream.Write(bytes, 0, bytes.Length);
                cryptoStream.FlushFinalBlock();
                StringBuilder stringBuilder = new StringBuilder();
                foreach (byte b in memoryStream.ToArray())
                {
                    stringBuilder.AppendFormat("{0:X2}", b);
                }
                return stringBuilder.ToString();
            }
            catch { return ""; }
        }

        public static string[] AllTextKey = new string[] { "支持我们", "投票", "现在购买", "广告", "购买", "立即获得", "赌博", "博彩", "双色球", "投票", "赚钱", "秒赚", "上亿" };
        /// <summary>
        /// 综合评分判断邮件是否是垃圾邮件
        /// </summary>
        /// <param name="Nitem"></param>
        /// <returns></returns>
        public static string FindGarbage(UserEmail Nitem)
        {
            UserEmail item = new UserEmail();
            item = Nitem;
            int MailLeve = 100;
            if (item.MailBody.BodyText == null)
            {
                MailLeve = MailLeve - 10;
            }
            if (item.MailBody.BodyText == "")
            {
                MailLeve = MailLeve - 10;
                if (item.MailBody.BodyHtmlText == null == false)
                {
                    if (item.MailBody.BodyHtmlText == "" == false)
                    {
                    }
                    else { MailLeve = MailLeve - 50; }
                }
                else { MailLeve = MailLeve - 50; }
            }
            else
            {
                if (item.MailBody.BodyText.Length > 55)
                {
                    MailLeve = MailLeve + 10;
                }
            }

            if (item.MailBody.BodyHtmlText == null)
            {
                MailLeve = MailLeve - 10;

            }
            if (item.MailBody.BodyHtmlText == "")
            {
                MailLeve = MailLeve - 10;
            }
            else
            {
                if (item.MailBody.BodyHtmlText.Length > 170)
                {
                    MailLeve = MailLeve + 20;
                }
            }


            foreach (var get in AllTextKey)
            {
                if (item.MailBody.BodyHtmlText.Contains(get))
                {
                    MailLeve = MailLeve - 30;
                }
            }


            string strTest = item.MailBody.BodyHtmlText;
            Regex reg = new Regex(@"(?'all'(?'start'.).*?)\k'start'");
            while (strTest != (strTest = reg.Replace(strTest, "${all}"))) { }
            string nextstr = strTest.ToLower().Replace("\r\n", "").Replace("\r", "").Replace(" ", "").Replace("</div>", "").Replace("<div>", "");
            if (nextstr.Length < 100)
            {
                MailLeve = MailLeve - 70;
            }
            if (MailLeve > 80 == false)
            {
                return "接收的垃圾邮件:" + MailLeve.ToString();
            }

            return MailLeve.ToString();
        }

        public static string Decrypt(string Text, string sKey = "yueding")
        {
            try
            {
                DESCryptoServiceProvider descryptoServiceProvider = new DESCryptoServiceProvider();
                int num = Text.Length / 2;
                byte[] array = new byte[num];
                for (int i = 0; i < num; i++)
                {
                    int num2 = Convert.ToInt32(Text.Substring(i * 2, 2), 16);
                    array[i] = (byte)num2;
                }
                descryptoServiceProvider.Key = Encoding.ASCII.GetBytes(FormsAuthentication.HashPasswordForStoringInConfigFile(sKey, "md5").Substring(0, 8));
                descryptoServiceProvider.IV = Encoding.ASCII.GetBytes(FormsAuthentication.HashPasswordForStoringInConfigFile(sKey, "md5").Substring(0, 8));
                MemoryStream memoryStream = new MemoryStream();
                CryptoStream cryptoStream = new CryptoStream(memoryStream, descryptoServiceProvider.CreateDecryptor(), CryptoStreamMode.Write);
                cryptoStream.Write(array, 0, array.Length);
                cryptoStream.FlushFinalBlock();
                return Encoding.Default.GetString(memoryStream.ToArray());
            }
            catch { return ""; }
        }


        /// <summary>
        /// 取邮件
        /// </summary>
        /// <param name="pop3"></param>
        /// <returns></returns>
        public static List<UserEmail> GetEmails(POP3_Client pop3)
        {
            List<UserEmail> result = new List<UserEmail>();
            try
            {
                //获取邮件信息列表

                POP3_ClientMessageCollection infos = pop3.Messages;
                foreach (POP3_ClientMessage info in infos)
                {
                    UserEmail NEmail = new UserEmail();

                    byte[] bytes = info.MessageToByte();
                    Mail_Message MailBody = Mail_Message.ParseFromByte(bytes);
                    NEmail.MailBody = MailBody;
                    NEmail.MailID = info.UID;
                    result.Add(NEmail);
                }
            }
            catch (Exception ex)
            {

            }
            return result;
        }

        /// <summary>
        /// 取所有邮件信息
        /// </summary>
        /// <param name="TargetAddress"></param>
        /// <param name="EmailType"></param>
        /// <param name="State"></param>
        public static List<MailMessage> GetSQLEmailMessage(string TargetAddress, string EmailType, string State)
        {
            List<MailMessage> AllMessage = new List<MailMessage>();

            string AutoWhere = "";
            if (TargetAddress == "" == false)
            {
                AutoWhere += " TargetAddress='" + TargetAddress + "' And";
            }
            if (EmailType == "" == false)
            {
                AutoWhere += " EmailType='" + EmailType + "' And";
            }
            if (State == "" == false)
            {
                AutoWhere += " State='" + State + "' And";
            }
            if (AutoWhere.EndsWith("And"))
            {
                AutoWhere = AutoWhere.Substring(0, AutoWhere.Length - 3);
            }
            if (AutoWhere == "" == false)
            {
                AutoWhere = "Where " + AutoWhere;
            }
            DataTable GetMailList = SQLiteHelper.ExecuteDataTable("Select rowid,* From EmailList " + AutoWhere);
            for (int i = 0; i < GetMailList.Rows.Count; i++)
            {
                MailMessage NMessage = new MailMessage();
                NMessage.ID = int.Parse(GetMailList.Rows[i]["rowid"].ToString());
                NMessage.EmailAddress = GetMailList.Rows[i]["EmailAddress"].ToString();
                NMessage.TargetAddress = GetMailList.Rows[i]["TargetAddress"].ToString();
                NMessage.Tittle = GetMailList.Rows[i]["Tittle"].ToString();
                NMessage.RichText = Decrypt(GetMailList.Rows[i]["RichText"].ToString());
                NMessage.EmailText = Decrypt(GetMailList.Rows[i]["EmailText"].ToString());
                NMessage.EmailFile = JsonConvert.DeserializeObject<List<FileCls>>(Decrypt(GetMailList.Rows[i]["EmailFile"].ToString()));
                NMessage.EmailType = GetMailList.Rows[i]["EmailType"].ToString();
                NMessage.EmailTextType = GetMailList.Rows[i]["EmailTextType"].ToString();
                NMessage.State = GetMailList.Rows[i]["State"].ToString();
                AllMessage.Add(NMessage);
            }
            return AllMessage;
        }



        public static string[] GetAllEmailType()
        {
            List<string> Get = new List<string>();
            DataTable GetMailList = SQLiteHelper.ExecuteDataTable("Select rowid,* From EmailList");
            for (int i = 0; i < GetMailList.Rows.Count; i++)
            {
                string GetType = GetMailList.Rows[i]["EmailType"].ToString();
                if (!Get.Contains(GetType))
                {
                    if (GetType == null == false)
                        if (GetType == "" == false)
                            Get.Add(GetType);
                }
            }
            return Get.ToArray();
        }

        public static string[] GetAllInfoUser()
        {
            List<string> Get = new List<string>();
            DataTable GetMailList = SQLiteHelper.ExecuteDataTable("Select rowid,* From MailInfoList");
            for (int i = 0; i < GetMailList.Rows.Count; i++)
            {
                string GetType = GetMailList.Rows[i]["SmtpUserName"].ToString();
                if (!Get.Contains(GetType))
                {
                    if (GetType == null == false)
                        if (GetType == "" == false)
                            Get.Add(GetType);
                }
            }
            return Get.ToArray();
        }

        public static string[] GetAllEmailState()
        {
            List<string> Get = new List<string>();
            DataTable GetMailList = SQLiteHelper.ExecuteDataTable("Select rowid,* From EmailList");
            for (int i = 0; i < GetMailList.Rows.Count; i++)
            {
                string GetType = GetMailList.Rows[i]["State"].ToString();
                if (!Get.Contains(GetType))
                {
                    if (GetType == null == false)
                        if (GetType == "" == false)
                            Get.Add(GetType);
                }
            }
            return Get.ToArray();
        }

        /// <summary>
        /// 字节流转字符串
        /// </summary>
        /// <param name="item"></param>
        /// <returns></returns>
        public static string BytesToSqlStr(byte[] item)
        {

            string GetByte = "";
            foreach (var get in item)
            {
                GetByte += get.ToString() + "_";
            }
            GetByte = GetByte.Substring(0, GetByte.Length - 1);
            return GetByte;

        }
        /// <summary>
        /// 字符串转字节流
        /// </summary>
        /// <param name="item"></param>
        /// <returns></returns>
        public static byte[] SqlStrToBytes(string item)
        {
            try
            {
                byte[] GetData = new byte[item.Split('_').Length];
                for (int Dataitem = 0; Dataitem < GetData.Length; Dataitem++)
                {
                    GetData[Dataitem] = Convert.ToByte(item.Split('_')[Dataitem]);
                }
                return GetData;
            }
            catch { return new byte[1]; }
        }


        /// <summary>
        /// 发送邮件了
        /// </summary>
        /// <param name="UseSmtpNmae"></param>
        /// <param name="TargetAddress"></param>
        /// <param name="MailName"></param>
        /// <param name="Tittle"></param>
        /// <param name="Text"></param>
        /// <returns></returns>
        public static bool SendMail(string UseSmtpName, string TargetAddress, string MailName, string Tittle, string Text)
        {
            DataTable GetMailList = SQLiteHelper.ExecuteDataTable("Select rowid,* From MailInfoList Where SmtpUserName ='" + UseSmtpName + "'");
            if (GetMailList.Rows.Count > 0)
            {
                try
                {
                    System.Net.Mail.SmtpClient client = new System.Net.Mail.SmtpClient();
                    client.Host = GetMailList.Rows[0]["SmtpHost"].ToString();//使用163的SMTP服务器发送邮件
                    client.Port = int.Parse(GetMailList.Rows[0]["SmtpPort"].ToString());
                    string smtpssl = GetMailList.Rows[0]["SmtpUseSSL"].ToString();
                    client.EnableSsl = bool.Parse(smtpssl);
                    client.UseDefaultCredentials = true;
                    client.DeliveryMethod = System.Net.Mail.SmtpDeliveryMethod.Network;
                    client.Credentials = new System.Net.NetworkCredential(GetMailList.Rows[0]["SmtpUserName"].ToString(), GetMailList.Rows[0]["SmtpPassword"].ToString());
                    System.Net.Mail.MailMessage Message = new System.Net.Mail.MailMessage();
                    Message.From = new System.Net.Mail.MailAddress(UseSmtpName);
                    Message.To.Add(TargetAddress);
                    Message.Subject = Tittle;
                    Message.Body = Text;
                    Message.SubjectEncoding = System.Text.Encoding.UTF8;
                    Message.BodyEncoding = System.Text.Encoding.UTF8;
                    Message.Priority = System.Net.Mail.MailPriority.High;
                    Message.IsBodyHtml = true;
                    client.Send(Message);
                    string sqlloader = "INSERT INTO EmailList(EmailID,EmailAddress,TargetAddress,Tittle,RichText,EmailText,EmailFile,EmailType,EmailTextType,State) VALUES('{0}', '{1}', '{2}', '{3}', '{4}', '{5}', '{6}', '{7}', '{8}', '{9}')";
                    SQLiteHelper.ExecuteNonQuery(string.Format(sqlloader, "auto", UseSmtpName, TargetAddress, Tittle, Encrypt(Text), Encrypt(Text), "", GetMailList.Rows[0]["EmailType"].ToString(), "发送成功", "发件"));
                }
                catch
                {
                    string sqlloader = "INSERT INTO EmailList(EmailID,EmailAddress,TargetAddress,Tittle,RichText,EmailText,EmailFile,EmailType,EmailTextType,State) VALUES('{0}', '{1}', '{2}', '{3}', '{4}', '{5}', '{6}', '{7}', '{8}', '{9}')";
                    SQLiteHelper.ExecuteNonQuery(string.Format(sqlloader, "auto", UseSmtpName, TargetAddress, Tittle, Encrypt(Text), Encrypt(Text), "", GetMailList.Rows[0]["EmailType"].ToString(), "发送失败", "发件"));
                }


                //附件部分
                //Attachment attach = new Attachment(filename);
                //ContentDisposition disposition = attach.ContentDisposition;
                //disposition.CreationDate = System.IO.File.GetCreationTime(filename);
                //disposition.ModificationDate = System.IO.File.GetLastWriteTime(filename);
                //disposition.ReadDate = System.IO.File.GetLastAccessTime(filename);
                //Mail.Attachments.Add(attach);

                return true;
            }

            return false;
        }

    }


    public struct MailMessage
    {
        public int ID { get; set; }

        public string EmailAddress { get; set; }

        public string TargetAddress { get; set; }

        public string Tittle { get; set; }

        public string RichText { get; set; }

        public string EmailText { get; set; }

        public List<FileCls> EmailFile { get; set; }

        public string EmailType { get; set; }

        public string EmailTextType { get; set; }

        public string State { get; set; }
    }
    public struct FileCls
    {
        public string FilePath { get; set; }
        public string FileName { get; set; }
    }
    public struct UserEmail
    {
        public Mail_Message MailBody { get; set; }
        public string MailID { get; set; }
    }

    public class EmailInfo
    {
        public int ID { get; set; }
        public string SmtpHost { get; set; }

        public int SmtpPort { get; set; }

        public bool SmtpUseSsl { get; set; }

        public string SmtpUserName { get; set; }

        public string SmtpPassword { get; set; }

        public string EmailType { get; set; }

        public string PoPHost { get; set; }

        public int PoPPort { get; set; }

        public bool pop3UseSsl { get; set; }

        public string PoPUserName { get; set; }

        public string PoPPassword { get; set; }
    }

  
}
