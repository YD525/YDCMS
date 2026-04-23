using WebMaster.DataManager;
using WebMaster.LabelManager;
using WebMaster.SQLManager;
using WebMaster.UserManager;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Web;
using static WebMaster.SQLManager.SqlServerCommand;
using System.Threading;
using System.Windows.Documents;
using System.Data;
using WebMaster.CoreManager;
using Renci.SshNet.Messages;

namespace WebMaster.HtmlManager
{
    public class HtmlCreatEngine
    {
        public static List<TmpStyle> AllWebPage = new List<TmpStyle>();//所有网页存在内存中

        private static bool LockerTemplate = false;

        public static List<HtmlByThread> QueueHtml = new List<HtmlByThread>();
        public static List<ReplaceWait> WaitByHtml = new List<ReplaceWait>();//allNAVSTART
        public static List<ActList> WaitAllActList = new List<ActList>();//allLISTSTART

        public static List<typekey> AllRTTmp = new List<typekey>();
        public static List<HtmlByThread> QueueStaticHtmlTmp = new List<HtmlByThread>();


        public static void QuickSetFunction(List<string> InputCode, ref List<string> LoadCode, HtmlByThread LockerHtmlThread)
        {
            LoadCode.Clear();
            foreach (var GetLine in InputCode)
            {
                LoadCode.Add(QuickSetFunction(GetLine, LockerHtmlThread));
            }
        }
        public static string QuickSetFunction(string CodeLine, HtmlByThread LockerHtmlThread)
        {
            string NewLine = CodeLine;
            if (NewLine.Contains("(@_AutoFindCssPath_@)"))
            {
                string LockerStylePath = LockerHtmlThread.StylePath;
                if (!LockerStylePath.EndsWith("/"))
                {
                    LockerStylePath += "/";
                }
                NewLine = NewLine.Replace("(@_AutoFindCssPath_@)", LockerStylePath);
            }

            if (NewLine.Contains("(@_SystemSetting:DEFURL_@)"))
            {
                NewLine = NewLine.Replace("(@_SystemSetting:DEFURL_@)", DefWebHost);
            }

            if (NewLine.Contains("(@_WebRequestUrl_@)"))
            {
                NewLine = NewLine.Replace("(@_WebRequestUrl_@)", DefWebHost + "/Web/RequestManager");
            }

            //(@_RequestUrl_@)
            return NewLine;
        }
        public static string DefWebHost = "";
        public static int LockerCreatEngine = 0;

        public static List<WebMaster.FileInformation> CurrentRTList = new List<WebMaster.FileInformation>();
        public static List<WebMaster.FileInformation> CurrentHtmList = new List<WebMaster.FileInformation>();


        public static List<WebMaster.FileInformation> SelectFileList(bool After, bool Front)
        {
            List<WebMaster.FileInformation> HtmList = new List<WebMaster.FileInformation>();
            if (!After && !Front)
            {
                return HtmList;
            }
            else
            if (After && Front)
            {
                return CurrentHtmList;
            }

            if (!After)
            {
                foreach (var Get in CurrentHtmList)
                {
                    string GetFileName = Get.FilePath;
                    if (GetFileName.Contains(@"\"))
                    {
                        GetFileName = GetFileName.Substring(GetFileName.LastIndexOf(@"\"));
                    }
                    if (!GetFileName.ToLower().StartsWith("admin_"))
                    {
                        HtmList.Add(Get);
                    }
                }
            }
            else
            if (!Front)
            {
                foreach (var Get in CurrentHtmList)
                {
                    string GetFileName = Get.FilePath;
                    if (GetFileName.Contains(@"\"))
                    {
                        GetFileName = GetFileName.Substring(GetFileName.LastIndexOf(@"\"));
                    }
                    if (!GetFileName.ToLower().StartsWith("user_"))
                    {
                        HtmList.Add(Get);
                    }
                }
            }
            return HtmList;
        }

        public static int DeleteTemplateOldFile()
        {
            int DeleteSucess = 0;
            string GetFilePath = (AppDomain.CurrentDomain.BaseDirectory + @"\article\").Replace(@"\\",@"\");

            var GetFileList = DataHelper.getallfile(GetFilePath, "", new List<string> { ".html" });
            foreach (var GetHtml in GetFileList)
            {
                try { 

                if (File.Exists(GetHtml.FilePath))
                {
                    FileInfo GetInFo = new FileInfo(GetHtml.FilePath);
                    GetInFo.Delete();
                }

                }
                catch { }
            }
            return DeleteSucess;
        }

        public static List<string> ProtectDirectory = new List<string>() { "AllModule", "App_Data", "App_Start", "bin", "ckplayer", "Content", "Controllers", "DataFile", "dist", "FileList", "FileUpload", "fonts", "HtmlTemplate", "lib", "Models", "node_modules", "obj", "Properties", "pulgins", "Scripts", "ueditor", "Views", "x64", "x86","ArticleList" };
        public static string ReadAllTemplate(EngineSetting Setting)
        {
            if (LockerCreatEngine > 0) return "Engine Working Please Wait";

            string Message = "";
            Message += "Start Engine Success" + "\r\n";

            DeleteTemplateOldFile();

            if (Setting.ReloadArticle)
            {
                int Files = DeleteTemplateOldFile();
                GC.Collect();
                Message += string.Format("OutPut All Article Reload Count :{0}\r\n", Files);
            }

            if (Setting.RefreshDB)
            {
                Initialization();

                ReleaseCache();
                DeleteTemplateOldFile();
                Message += "Initialization DB Data List\r\n";
            }
            if (Setting.RefreshCache)
            {
                CurrentRTList.Clear();
                CurrentHtmList.Clear();
                CurrentRTList = DataHelper.GetAllFileCode(DeFine.TemplatesPath, "", new List<string> { ".RT" });
                CurrentHtmList = DataHelper.GetAllFileCode(DeFine.TemplatesPath, "", new List<string> { ".htm" });
                ReadAllTemplate
                (
                CurrentRTList,
                SelectFileList(Setting.After, Setting.Front),
                true
                );
                Message += "Start Cache Clear Service\r\n";
            }
            else
            {
                ReadAllTemplate
                   (
                   CurrentRTList,
                   SelectFileList(Setting.After, Setting.Front),
                   true
                   );
            }

            Message += "new (" + (CurrentHtmList.Count + CurrentRTList.Count).ToString() + ") Files Has been Creat\r\n";

            return Message;
        }
        public static void ReadAllTemplate(string Source, bool Refresh = false)
        {
            if (LockerCreatEngine > 0) return;
            CurrentRTList.Clear();
            CurrentHtmList.Clear();
            CurrentRTList = DataHelper.GetAllFileCode(Source, "", new List<string> { ".RT" });
            CurrentHtmList = DataHelper.GetAllFileCode(Source, "", new List<string> { ".htm" });
            ReadAllTemplate
                (
                CurrentRTList,
                CurrentHtmList,
                Refresh
                );
        }
        public static object LockerProcess = new object();
        public static List<ActionToHref> AllAction = new List<ActionToHref>();
        public static void ReadAllTemplate(List<WebMaster.FileInformation> RTList, List<WebMaster.FileInformation> HtmList, bool Refresh = false)
        {
            lock (LockerProcess)
            {
                if (Directory.Exists(@"\wwwroot\article\"))
                {
                    foreach (var GetFile in DataHelper.getallfile(@"\wwwroot\article\", "", new List<string> { ".html" }))
                    {
                        File.Delete(GetFile.FilePath);
                    }
                }

                if (LockerCreatEngine > 0) return;
                new Thread(() =>
                {
                    LockerCreatEngine++;

                    int state = SqlServerHelper.ExecuteNonQuery("UPDATE product SET Creat = 'False' WHERE Creat = 'True'");

                    AllAction.Clear();
                    SignHelper.AllSign.Remove("Nav");
                    SignHelper.AllSign.Remove("Act");
                    SignHelper.AllSign.Remove("ActList");

                    SignHelper.AllSign.Remove("ActionPath");
                    foreach (var Get in HtmList)
                    {
                        foreach (var GetLine in Get.FileCode)
                        {
                            if (GetLine.StartsWith("@->"))
                            {
                                try
                                {

                                    ActionToHref NActionToHref = new ActionToHref();
                                    NActionToHref.FilePath = Get.FilePath;
                                    foreach (var GetFunction in GetLine.Trim().Substring("@->".Length).Split(';'))
                                    {
                                        if (GetFunction.Contains("="))
                                        {
                                            if (GetFunction.Split('=')[0].ToLower() == "TmpName".ToLower())
                                            {
                                                NActionToHref.Action = GetFunction.Split('=')[1];
                                            }
                                            if (GetFunction.Split('=')[0].ToLower() == "FileSource".ToLower())
                                            {
                                                string DefUrl = DeFine.DefWebUrl;
                                                if (!DefUrl.EndsWith("/")) DefUrl = DefUrl + "/";
                                                string GetActionPath = (GetFunction.Split('=')[1].Replace("AutoCreat", Get.FileName.Split('_')[1].Split('.')[0] + ".html")).Replace(@"\", "/");
                                                if (GetActionPath.StartsWith("/")) GetActionPath = GetActionPath.Substring(1, GetActionPath.Length - 1);
                                                NActionToHref.Href = DefUrl + GetActionPath;
                                            }
                                        }
                                    }
                                    if (NActionToHref.Action.ToLower() == "auto")
                                    {
                                        if (Get.FileName.Contains("_"))
                                        {
                                            NActionToHref.Action = Get.FileName.Split('_')[1].Split('.')[0];
                                        }
                                        else
                                        {
                                            NActionToHref.Action = Get.FileName.Split('.')[0];
                                        }
                                    }
                                    if (Get.FileName.ToLower().StartsWith("user_")) AllAction.Add(NActionToHref);

                                    break;

                                }
                                catch { }

                            }
                        }
                    }

                    Initialization();

                    QueueHtml.Clear();
                    WaitByHtml.Clear();
                    WaitAllActList.Clear();

                    //LockerTemplate = false;//CES
                    if (Refresh)
                    {
                        LockerTemplate = false;
                    }
                    if (!LockerTemplate)
                    {
                        DefWebHost = CMSHelper.GetDeFineValue("DEFURL");

                        AllRTTmp.Clear();
                        AllWebPage.Clear();
                        AllSafe.Clear();
                        LockerTemplate = true;
                        var GetRTList = RTList;
                        foreach (var get in GetRTList)
                        {
                            typekey Nkey = new typekey();
                            Nkey.key = get.FileName;
                            Nkey.value = DataHelper.ListToSrt(get.FileCode);
                            AllRTTmp.Add(Nkey);
                        }


                        var GetFileList = HtmList;
                        foreach (var get in GetFileList)
                        {
                            HtmlByThread Nhtml = new HtmlByThread();
                            string FileName = get.FileName;
                            string HtmlFileName = "";
                            string HtmlAction = "";
                            string HtmlPath = "";
                            HtmlType HtmlShowType = HtmlType.Dynamic;
                            string StylePath = "";
                            string TmpType = "";
                            string TemplateType = "";
                            if (get.FileName.Contains("_"))
                            {
                                TemplateType = get.FileName.Split('_')[0];
                                HtmlFileName = get.FileName.Split('_')[1].Split('.')[0];
                                TmpType = get.FileName.Split('_')[0];
                            }
                            List<string> InuptCode = new List<string>();
                            List<string> FileCode = new List<string>();
                            FileCode = get.FileCode;
                            bool ListenHtmlStart = false;
                            bool HtmlCodeStart = false;
                            foreach (var lv2get in FileCode)
                            {

                                if (!HtmlCodeStart)
                                {
                                    if (GetConfigSetting(lv2get, ref HtmlAction, ref HtmlPath, ref HtmlShowType, ref StylePath, HtmlFileName, TmpType))
                                    {
                                        InuptCode.Add("<!--CreatTime is " + DateTime.Now + " Action is " + HtmlAction + " -->");
                                        ListenHtmlStart = true;
                                    }
                                    if (ListenHtmlStart)
                                    {
                                        if (GetHtmlCode(lv2get))
                                        {
                                            HtmlCodeStart = true;
                                        }
                                    }
                                }
                                else
                                {
                                    if (lv2get.Trim().Length > 0)
                                    {
                                        string GetNewLine = MainCore.InputLine(lv2get, HtmlAction);
                                        if (GetNewLine == null == false)
                                        {
                                            InuptCode.Add(GetNewLine);
                                        }
                                    }
                                }
                            }
                            if (HtmlPath.ToLower().EndsWith(@"\autocreat"))
                            {
                                HtmlPath = HtmlPath.Replace("autocreat", HtmlFileName + ".html");
                            }
                            Nhtml.TemplateType = TemplateType;
                            Nhtml.HtmlFileName = HtmlFileName;
                            Nhtml.HtmlAction = HtmlAction;
                            Nhtml.HtmlPath = HtmlPath;
                            Nhtml.HtmlShowType = HtmlShowType;
                            Nhtml.StylePath = StylePath;
                            QuickSetFunction(InuptCode, ref Nhtml.AllCode, Nhtml);
                            QueueHtml.Add(Nhtml);
                        }



                        List<HtmlByThread> NAllWebPage = new List<HtmlByThread>();

                        for (int i = 0; i < QueueHtml.Count; i++)
                        {
                            HtmlByThread Ntmpstyle = new HtmlByThread();

                            Ntmpstyle = QueueHtml[i];

                            string RichTextBox = DataHelper.ListToSrt(QueueHtml[i].AllCode);

                            foreach (var GetReplace in WaitByHtml)
                            {
                                RichTextBox = RichTextBox.Replace(GetReplace.key, GetReplace.value);
                            }

                            //开始编译列表标签
                            foreach (var FristAct in WaitAllActList)
                            {
                                if (QueueHtml[i].HtmlAction == FristAct.ActionName)
                                {
                                    RichTextBox = RichTextBox.Replace(FristAct.ReplaceCode, FristAct.LockerCode.Replace("{#INSTRList}", "<div class='fyauto'><a>上一页</a>" + "1/" + WaitAllActList.Count) + "<a href='" + GetActListById(QueueHtml[i].HtmlAction) + "'>下一页</a></div>");
                                }
                            }
                            Ntmpstyle.AllCode.Clear();
                            Ntmpstyle.AllCode.AddRange(DataHelper.StrToList(RichTextBox));
                            NAllWebPage.Add(Ntmpstyle);
                        }
                        QueueHtml.Clear();
                        QueueHtml.AddRange(NAllWebPage);
                        NAllWebPage.Clear();
                        //开始编译导航标签



                        foreach (var lv2get in QueueHtml)
                        {
                            string StringFormat = "{@#code+=" + lv2get.HtmlFileName + "}";

                            for (int GetWebPage = 0; GetWebPage < QueueHtml.Count; GetWebPage++)
                            {

                                string RichText = DataHelper.ListToSrt(QueueHtml[GetWebPage].AllCode);
                                if (RichText.Contains(StringFormat))
                                {
                                    if (lv2get.TemplateType == QueueHtml[GetWebPage].TemplateType)
                                    {
                                        RichText = RichText.Replace(StringFormat, DataHelper.ListToSrt(lv2get.AllCode));
                                        QueueHtml[GetWebPage].AllCode = DataHelper.StrToList(RichText);
                                    }

                                }
                            }
                        }
                        SignHelper.Initialization("");//进行二次扫描
                        SignHelper.AllSign.Add("Act");
                        SignHelper.AllSign.Add("Nav");
                        SignHelper.AllSign.Add("ActList");

                        SignHelper.AllSign.Add("ActionPath");
                        List<HtmlByThread> CopyQueueHtml = new List<HtmlByThread>();
                        CopyQueueHtml.AddRange(QueueHtml);

                        foreach (var Queue in CopyQueueHtml)
                        {
                            HtmlByThread CurrentHtml = Queue;
                            SignHelper.Initialization(Queue.HtmlAction);
                            List<string> TmpList = new List<string>();
                            foreach (var Get in Queue.AllCode)
                            {
                                string GetNewLine = MainCore.InputLine(Get, Queue.HtmlAction);

                                if (GetNewLine == null == false)
                                {
                                    if (GetNewLine.Contains("(@_CurrentActionName_@)"))
                                    {
                                        if (Queue.HtmlShowType == HtmlType.Static)
                                        {
                                            DataTable NTable = new DataTable();
                                            NTable = SqlServerHelper.ExecuteDataTable("Select * From nav");
                                            for (int lv1 = 0; lv1 < NTable.Rows.Count; lv1++)
                                            {
                                                if (DataHelper.ObjToStr(NTable.Rows[lv1]["ColumnHref"]).Equals(Queue.HtmlAction))
                                                {
                                                    GetNewLine = GetNewLine.Replace("(@_CurrentActionName_@)", DataHelper.ObjToStr(NTable.Rows[lv1]["ColumnName"]));
                                                    if (GetNewLine.Contains("(@_CurrentSeoText_@)"))
                                                    {
                                                        GetNewLine = GetNewLine.Replace("(@_CurrentSeoText_@)", DataHelper.ObjToStr(NTable.Rows[lv1]["SeoText"]));
                                                    }
                                                    break;
                                                }
                                            }
                                        }

                                    }
                                    TmpList.Add(GetNewLine);
                                }
                            }
                            SignHelper.WriteAllPage(DataHelper.ListToSrt(TmpList));
                            QueueHtml.Remove(CurrentHtml);
                            HtmlByThread NextHtml = CurrentHtml;
                            NextHtml.AllCode = TmpList;
                            QueueHtml.Add(NextHtml);

                            if (Queue.HtmlShowType == HtmlType.Dynamic)
                            {
                                //动态
                                AllWebPage.Add(new TmpStyle(Queue.HtmlAction, Queue.StylePath, TmpList));
                            }
                            else
                            {
                                if (Queue.HtmlShowType == HtmlType.Static)
                                {
                                    //静态
                                    AllWebPage.Add(new TmpStyle(Queue.HtmlFileName, Queue.HtmlAction, Queue.HtmlPath, Queue.StylePath, TmpList));
                                }

                            }
                        }


                        //全部处理完毕后过滤掉静态页面
                        SignHelper.WriteAllAct();


                        List<TmpStyle> GWebPage = new List<TmpStyle>();

                        foreach (var getNpage in AllWebPage)
                        {
                            if (getNpage.HtmlShowType == HtmlType.Dynamic)
                            {
                                GWebPage.Add(getNpage);
                            }
                        }
                        AllWebPage.Clear();
                        AllWebPage.AddRange(GWebPage);
                        QueueStaticHtmlTmp.Clear();
                        foreach (var GetPage in QueueHtml)
                        {
                            if (GetPage.HtmlShowType == HtmlType.Static)
                            {
                                QueueStaticHtmlTmp.Add(GetPage);
                            }
                        }
                        QueueHtml.Clear();
                    }
                    LockerCreatEngine--;
                }).Start();
            }
          


        }

        /// <summary>
        /// 获取自定义标记
        /// </summary>
        /// <param name="stylename"></param>
        /// <returns></returns>
        public static string QuickGetRT(string stylename)
        {
            foreach (var get in AllRTTmp)
            {
                if (stylename + ".RT" == get.key)
                {
                    return get.value.Replace("<@GetDefUrl>", QuickFind("SystemSetting:DEFURL").htmlcode);
                }
            }
            return "";
        }
        public static string ShowWebPage(string WebAction, HttpContext Context)
        {

            if (AllWebPage.Count == 0)//自动刷新样式
            {
                // HtmlCreatEngine.ReadAllTemplate(DeFine.TemplatesPath, true);
            }
            string PlugFormat = "";

            while (HtmlCreatEngine.LockerCreatEngine > 0) { Thread.Sleep(100); }

            string InputHtml = "";
            foreach (var get in AllWebPage)
            {
                if (get.HtmlAction == WebAction)
                {
                    InputHtml =DataHelper.ListToSrt(get.AllCode);
                }
                if (get.HtmlAction == "plugmain")
                {
                    PlugFormat= DataHelper.ListToSrt(get.AllCode);
                }
            }

            foreach (var SafeGet in AllSafe)
            {
                if (SafeGet.WebAction == WebAction)
                {
                    if (SafeGet.ThisType == SafeType.VisitLock)
                    {
                        string AdminTokenType = "";
                        string AdminToken = DataProcessing.ValidationToken(PlugHelper.PutAllRequest(Context), ref AdminTokenType);
                        usertoken SelectAdminToken = new usertoken();
                        UserHelper.TokenLegitimate(AdminToken, AdminTokenType, out SelectAdminToken);
                        if (SelectAdminToken.password.Length > 0 && SelectAdminToken.username.Length > 0)
                        {
                            InputHtml = InputHtml.Replace("{@fromToken:username}", SelectAdminToken.username);
                        }
                        else
                        {
                            return "禁止访问原因没有登录管理员或者UStoken过期";
                        }
                    }
                }

            }
            string GetReturnCode = PluginHelper.PutAllUI(WebAction, InputHtml, Context);
            if (GetReturnCode == null)
            {
                string PlugNavList = "";
                foreach (var get in PluginHelper.PlugNavList)
                {
                    PlugNavList += get.NavCode + "\r\n";
                }

                return InputHtml.Replace("<!--@ENDNAV-->", PlugNavList + "\r\n" + "<!--@ENDNAV-->");
            }
            else
            {
                string PlugNavList = "";
                foreach (var get in PluginHelper.PlugNavList)
                {
                    PlugNavList += get.NavCode + "\r\n";
                }

                if (GetReturnCode.Contains("PlugStartFromUI->"))
                {
                    return PlugFormat.Replace("<!--@ENDNAV-->", PlugNavList + "\r\n" + "<!--@ENDNAV-->").Replace("(@_pluguicodestart_@)", GetReturnCode.Replace("PlugStartFromUI->", ""));
                }
                else
                {
                    return GetReturnCode;
                }
            }
        }
        public static string GetActListById(string HtmlAction, int id = 0)
        {
            foreach (var autoget in WaitAllActList)
            {
                if (autoget.ActionName == HtmlAction)
                {
                    return autoget.AllAct[id].TargetPath;
                }
            }
            return "NotFind";
        }





        public static List<SafeCls> AllSafe = new List<SafeCls>();
        public static bool GetConfigSetting(string Message, ref string HtmlAction, ref string HtmlPath, ref HtmlType HtmlShowType, ref string StylePath, string FileName, string TmpType)
        {
            if (Message.Trim().StartsWith("@->"))
            {
                Message = Message.Trim().Substring(Message.IndexOf("@->") + "@->".Length);
                string SelectAction = "";
                List<string> AllOder = new List<string>();
                if (Message.Contains(";")) { AllOder.AddRange(Message.Split(';').ToList()); }
                foreach (var get in AllOder)
                {
                    if (get.Contains("="))
                    {
                        string Function = get.Split('=')[0].ToLower();
                        string Value = get.Split('=')[1].ToLower();

                        if (Function == "creatpath")
                        {
                            if (Value.ToLower() == "true")
                            {
                                HtmlShowType = HtmlType.Static;
                            }
                            if (Value.ToLower() == "false")
                            {
                                HtmlShowType = HtmlType.Dynamic;
                            }
                            if (Value.ToLower() == "null")
                            {
                                HtmlShowType = HtmlType.Null;
                            }
                        }
                        if (Function == "filesource")
                        {
                            HtmlPath = Value;
                        }
                        if (Function == "tmpname")
                        {
                            SelectAction = Value;
                            HtmlAction = Value;

                            if (HtmlAction == "auto")
                            {
                                HtmlAction = FileName;
                                SelectAction = FileName;
                            }
                        }
                        if (Function == "stylepath")
                        {
                            if (Value == "autosearch" == false)
                            {
                                StylePath = Value;
                            }
                            else
                            {
                                StylePath = "../HtmlTemplate" + "/" + "AllStyle" + "/" + TmpType;
                            }
                        }
                    }
                }

                SafeCls Safe = new SafeCls();
                if (Message.ToLower().Contains("AdminLocking".ToLower()))
                {
                    Safe.ThisType = SafeType.VisitLock;
                    Safe.WebAction = SelectAction;
                    AllSafe.Add(Safe);
                }
                return true;
            }
            return false;
        }

        public static string HtmlCodeInupt = "";
        public static bool GetHtmlCode(string Line)
        {
            if (Line.Replace(" ", "") == "@html->")
            {
                return true;
            }
            else
            {
                return false;
            }
        }
        /// <summary>
        /// 初始化引擎
        /// </summary>
        public static void Initialization()
        {
            LabelHelper.Initialization("");
            SignHelper.Initialization("");
        }
        public static void CreatInsert(string LockerTableName, DataTable Table)
        {
            new Thread(() =>
            {

                Thread.Sleep(100);

                string InsertFormat = "INSERT INTO {0}({1}) VALUES ({2});";
                string AllParam = "";
                string AllValue = "";

                foreach (DataRow Line in Table.Rows)
                {
                    AllParam = "";
                    AllValue = "";
                    foreach (DataColumn item in Line.Table.Columns)
                    {
                        if (item.ColumnName.ToLower() == "id" == false)
                            if (item.ColumnName.ToLower() == "ProductID" == false)
                            {
                                AllParam += "[" + item.ColumnName + "],";
                                AllValue += "'" + Line[item.ColumnName] + "',";
                            }
                    }
                    if (AllParam.EndsWith(","))
                    {
                        AllParam = AllParam.Substring(0, AllParam.Length - 1);
                    }
                    if (AllValue.EndsWith(","))
                    {
                        AllValue = AllValue.Substring(0, AllValue.Length - 1);
                    }
                    int State = SqlServerHelper.ExecuteNonQuery(string.Format(InsertFormat, LockerTableName, AllParam, AllValue));

                    if (State == 0 == false)
                    {

                    }
                    else
                    { 
                    //CreatError
                    }
                }
            }).Start();
        }
        public static int ReleaseCache()
        {
            int Succes = 0;

            if (SqlServerHelper.ExecuteNonQuery("DBCC FREEPROCCACHE") == 0 == false) Succes++;
            if (SqlServerHelper.ExecuteNonQuery("DBCC FREESESSIONCACHE") == 0 == false) Succes++;
            if (SqlServerHelper.ExecuteNonQuery("DBCC FREESYSTEMCACHE('All')") == 0 == false) Succes++;
            if (SqlServerHelper.ExecuteNonQuery("DBCC DROPCLEANBUFFERS") == 0 == false) Succes++;

            DataTable NTable = SqlServerHelper.ExecuteDataTable("SELECT * FROM INFORMATION_SCHEMA.TABLES");
            if (NTable.Rows.Count > 0)
            {
                for (int i = 0; i < NTable.Rows.Count; i++)
                {
                    string LockerTableName = DataHelper.ObjToStr(NTable.Rows[i]["TABLE_NAME"]);
                    if (LockerTableName.ToLower() == "FileList".ToLower())
                    {
                        Succes += FormatDB(LockerTableName);
                    }
                    if (LockerTableName.ToLower() == "UserList".ToLower())
                    {
                        Succes += FormatDB(LockerTableName);
                    }
                    if (LockerTableName.ToLower() == "product".ToLower())
                    {
                        Succes += FormatDB(LockerTableName);
                    }
                    if (LockerTableName.ToLower() == "pulginslist".ToLower())
                    {
                        Succes += FormatDB(LockerTableName);
                    }
                    if (LockerTableName.ToLower() == "nav".ToLower())
                    {
                        Succes += FormatDB(LockerTableName);
                    }
                    if (LockerTableName.ToLower() == "ThreatLibrary".ToLower())
                    {
                        Succes += FormatDB(LockerTableName);
                    }
                    if (LockerTableName.ToLower() == "ThreadWorking".ToLower())
                    {
                        Succes += FormatDB(LockerTableName);
                    }
                }
            }
            return Succes;
        }
        public static int FormatDB(string TableName)
        {
            int Succes = 0;
            string ClearRows = "truncate table {0};";
            DataTable OLDData = SqlServerHelper.ExecuteDataTable("Select * From " + TableName);
            if (SqlServerHelper.ExecuteNonQuery(string.Format(ClearRows, TableName)) == 0 == false)
            {
                Succes++;
            }
            CreatInsert(TableName, OLDData);
            return Succes;
        }

    }
    public enum SafeType { VisitLock = 0, Null = 1 }
    public enum HtmlType { Static = 0, Dynamic = 1, Null = 1 }

    public class EngineSetting
    {
        public bool RefreshCache { get; set; }
        public bool Front { get; set; }
        public bool After { get; set; }
        public bool RefreshDB { get; set; }
        public bool ReloadArticle { get; set; }

        public bool ListFromSetting = false;

        public List<WebMaster.FileInformation> RTList = new List<WebMaster.FileInformation>();

        public List<WebMaster.FileInformation> HtmList = new List<WebMaster.FileInformation>();
    }
    public class SafeCls
    {
        public SafeType ThisType = SafeType.Null;
        public string WebAction = "";
    }
    public class HtmlTypeCls
    {
        public string HtmlFileName { get; set; }
        public string HtmlAction { get; set; }
        public string TemplateType = "";
        public string CreatPath = "";
        public string HtmlPath = "";
        public string LockerType = "";
        public string HeadCode = "";
        public string CallCount = "";
        public string CodeText = "";
        public HtmlType HtmlShowType = HtmlType.Dynamic;
        public string StylePath { get; set; }

        public List<string> AllCode = new List<string>();
    }
    public class TmpStyle : HtmlTypeCls
    {
        public TmpStyle()
        {



        }



        //动态网页支持
        public TmpStyle(string HtmlAction, string StylePath, List<string> AllCode, HtmlType HtmlShowType = HtmlType.Dynamic)
        {
            if (HtmlShowType == HtmlType.Dynamic)
            {
                this.HtmlAction = HtmlAction;
                this.StylePath = StylePath;

                string GetHtmlInupt = DataHelper.ListToSrt(AllCode);

                System.Text.RegularExpressions.MatchCollection getjg = System.Text.RegularExpressions.Regex.Matches(GetHtmlInupt, @"{@admin:HrefTarget:(.*?)}");
                //处理Action连接
                if (getjg.Count > 0)
                {
                    for (int i = 0; i < getjg.Count; i++)
                    {
                        string GetActionValue = getjg[i].Value;
                        string GetHref = GetActionValue.Substring("{@admin:HrefTarget:".Length, GetActionValue.Length - "{@admin:HrefTarget:".Length - 1);
                        GetHtmlInupt = GetHtmlInupt.Replace(GetActionValue, DeFine.GetRootURI() + "/Web/ShowWebPage?UI=" + GetHref);
                    }
                }
                this.AllCode = DataHelper.StrToList(GetHtmlInupt);
                this.HtmlShowType = HtmlShowType;
            }
        }
        //静态网页支持
        public TmpStyle(string HtmlFileName, string HtmlAction, string HtmlPath, string StylePath, List<string> AllCode, HtmlType HtmlShowType = HtmlType.Static)
        {
            if (HtmlShowType == HtmlType.Static)
            {
                this.HtmlFileName = HtmlFileName;
                this.HtmlAction = HtmlAction;
                if (HtmlPath.Contains(":"))
                {
                    this.HtmlPath = HtmlPath;
                }
                else
                {
                    this.HtmlPath = DeFine.DefCurrentDirectory + HtmlPath;
                }
                string GetDirectory = this.HtmlPath.Substring(0, this.HtmlPath.LastIndexOf(@"\"));
                string GetFileName = this.HtmlPath.Substring(GetDirectory.Length + 1);
                if (GetFileName.Trim() == "")
                {
                    HtmlPath += HtmlFileName + ".html";
                }
                this.StylePath = StylePath;
                this.AllCode = AllCode;
                this.HtmlShowType = HtmlShowType;
                //处理静态文件
                if (Directory.Exists(GetDirectory))
                {

                }
                else
                {
                    Directory.CreateDirectory(GetDirectory);
                }

                DataHelper.writefile(GetDirectory + @"\" + GetFileName, DataHelper.ListToSrt(AllCode), Encoding.UTF8);

            }
        }

    }

    public struct ReplaceWait
    {
        public string key { get; set; }
        public string value { get; set; }
        public ReplaceWait(string key, string value)
        {
            this.key = key;
            this.value = value;
        }
    }

    public class ActionToHref
    {
        public string Action = "";
        public string Href = "";
        public string FilePath = "";
    }
    public class HtmlByThread : HtmlTypeCls
    {


    }
}