using WebMaster.HtmlManager;
using WebMaster.SQLManager;
using System;
using System.Collections.Generic;
using System.Linq;
using static WebMaster.SQLManager.SqlServerCommand;
using System.Text.RegularExpressions;
using System.Data;
using WebMaster.DataManager;
using System.Text;
using System.Web.UI.WebControls;

namespace WebMaster.LabelManager
{
    public class SignHelper
    {
        public static string CurrentHtmlAction = "";
        public static void Initialization(string HtmlAction)
        {
            CurrentHtmlAction = HtmlAction;
        }

        public static bool ActionDifferent(string HtmlAction)
        {
            if (CurrentHtmlAction == HtmlAction)
            {

                return true; ;
            }
            else
            {
                Initialization(HtmlAction);
                return false;
            }

        }

        public static bool OverWrite = true;


        public static List<string> AllSign = new List<string>() { "InputValue", "ActionPath", "Obj", "Nav", "QuickGet", "ListT", "ListTT", "Act", "ActList" };

        public static Queue<WaitAct> AllActList = new Queue<WaitAct>();//AllActList


        public static SignCls CurrentSignCls = new SignCls();
        public static List<string> CurrentCode = new List<string>();
        /// <summary>
        /// 处理字符串
        /// </summary>
        /// <param name="LockerLine"></param>
        public static List<string> ProcessingString(string LockerLine)
        {

            List<string> TmpStrList = new List<string>();
            if (OverWrite)
            {
                //处理OverWrite
                bool IsSign = false;
                Regex NRegex = new Regex(@"(.*?){@#Start(.*?)}(.*?)");
                MatchCollection SignList = NRegex.Matches(LockerLine);
                foreach (Match Item in SignList)
                {
                    foreach (var get in AllSign)
                    {
                        string LockerItemValue = Item.Value;

                        if (LockerItemValue.Contains(get))
                        {
                            IsSign = true;
                            CurrentSignCls = GetSignType(LockerItemValue, get);
                            OverWrite = GetSignEnd(LockerLine, ref CurrentSignCls);
                        }
                    }
                }
                if (!IsSign)
                {
                    if (CurrentCode.Count > 0)
                    {
                        CurrentCode.Add(LockerLine);
                        TmpStrList.AddRange(CurrentCode);

                        CurrentCode.Clear();
                        return CurrentCode;
                    }
                    else
                    {
                        return new List<string>() { LockerLine };
                    }

                }
                else
                {
                    TmpStrList.AddRange(CurrentCode);
                    CurrentCode.Clear();
                    if (CurrentCode.Count > 0)
                    {
                        return TmpStrList;
                    }
                    else
                    {
                        return new List<string>();
                    }
                }




            }
            else
            {
                OverWrite = GetSignEnd(LockerLine, ref CurrentSignCls);
                if (OverWrite)
                {
                    TmpStrList.AddRange(CurrentCode);
                    CurrentCode.Clear();
                    return TmpStrList;
                }
                else
                {
                    return new List<string>();
                }
            }
        }
        public static SignCls GetSignType(string LockerLine, string LockerSignName)
        {
            SignCls NSignCls = new SignCls();
            NSignCls.SignName = LockerSignName;
            NSignCls.StartCode = LockerLine.Substring(LockerLine.IndexOf("{@#Start" + LockerSignName) + ("{@#Start" + LockerSignName).Length);
            NSignCls.StartCode = NSignCls.StartCode.Substring(0, NSignCls.StartCode.LastIndexOf("}"));
            NSignCls.EndCode = "{/#End" + LockerSignName + "}";
            if (NSignCls.StartCode.Contains("&"))
            {
                foreach (var GetItem in NSignCls.StartCode.Split('&'))
                {
                    SettingItem NSettingItem = new SettingItem();
                    string CurrentKey = "";
                    if (GetItem.Contains(":"))
                    {
                        CurrentKey = GetItem.Split(':')[0];
                        NSettingItem.Key = CurrentKey;
                        if (GetItem.Split(':')[1].Contains(";"))
                        {
                            foreach (var Lv2Get in GetItem.Split(':')[1].Split(';'))
                            {
                                if (Lv2Get.Trim().Length > 0) NSettingItem.AllValue.Add(Lv2Get);
                            }
                        }
                        else
                        {
                            if (GetItem.Split(':')[1].Length > 0) NSettingItem.AllValue.Add(GetItem.Split(':')[1]);
                        }
                    }
                    if (CurrentKey.Trim().Length > 0) NSignCls.AllSetting.Add(NSettingItem);
                }
            }

            return NSignCls;
        }

        public static bool GetSignEnd(string LockerLine, ref SignCls LockerSignCls)
        {
            if (LockerLine.Replace(" ", "").Contains(LockerSignCls.EndCode))
            {
                if (LockerLine.Contains(LockerSignCls.StartCode))
                {
                    string GetLine = LockerLine.Substring(LockerLine.IndexOf(LockerSignCls.StartCode + "}") + (LockerSignCls.StartCode + "}").Length);
                    GetLine = GetLine.Substring(0, GetLine.LastIndexOf(LockerSignCls.EndCode));
                    LockerSignCls.SignCode.Add(GetLine);

                    ProcessingSign(LockerSignCls);
                    return true;
                }
                else
                {
                    string LostStr = LockerLine.Replace(LockerSignCls.EndCode, "");
                    if (LostStr.Replace(" ", "").Length > 0)
                    {
                        LockerSignCls.SignCode.Add(LostStr);
                        ProcessingSign(LockerSignCls);
                        return true;
                    }
                    else
                    {
                        ProcessingSign(LockerSignCls);
                        return true;
                    }
                }
            }
            else
            {
                if (LockerLine.Contains(LockerSignCls.StartCode))
                {
                    string GetLine = LockerLine.Substring(LockerLine.IndexOf(LockerSignCls.StartCode + "}") + (LockerSignCls.StartCode + "}").Length);
                    if (GetLine.Replace(" ", "").Length > 0)
                    {
                        LockerSignCls.SignCode.Add(GetLine);
                        return false;
                    }
                    else
                    {
                        return false;
                    }
                }
                else
                {
                    LockerSignCls.SignCode.Add(LockerLine);
                    return false;
                }
            }
        }

        public static string ActListFormat(int ID)
        {
            string ActList = CMSHelper.GetDeFineValue("ActList");
            ActList = ActList.Replace("(ID)", ID.ToString());
            ActList = ActList.Replace("(TimeStamp)", TimeHelper.DateTimeToStamp(DateTime.Now));
            ActList = ActList.Replace("(TIME:dd)", DateTime.Now.Day.ToString());
            ActList = ActList.Replace("(TIME:yyyy)", DateTime.Now.Year.ToString());
            ActList = ActList.Replace("(TIME:MM)", DateTime.Now.Month.ToString());
            foreach (var Item in ActList.Split(')'))
            {
                string GetItem = Item;
                if (GetItem.Trim().Length > 0)
                {
                    if (GetItem.Contains("(Random:"))
                    {
                        GetItem = GetItem.Substring(GetItem.IndexOf("(Random:"));
                        string Get = GetItem.Replace("(Random:", "");
                        if (Get.ToLower().Contains("x"))
                        {
                            string NextChar = "";
                            for (int i = 0; i < Get.Length; i++)
                            {
                                NextChar += RandomChar();
                            }
                            ActList = ActList.Replace("(Random:" + Get + ")", NextChar);
                        }
                        else
                        {
                            if (Get.Contains("-"))
                            {
                                long tick = DateTime.Now.Ticks;
                                ActList = ActList.Replace("(Random:" + Get.Split('-')[0] + "-" + Get.Split('-')[1] + ")", new Random((int)(tick & 0xffffffffL) | (int)(tick >> 32)).Next(DataHelper.StrToInt(Get.Split('-')[0]), DataHelper.StrToInt(Get.Split('-')[1])).ToString());
                            }
                        }
                    }
                }
            }
            return ActList;
        }
        public static void WriteAllAct()
        {
            while (AllActList.Count > 0)
            {
                WaitAct Get = AllActList.Dequeue();
                WriteAllAct(Get.WritePath, Get.NCreatOver, Get.Config, Get.StyleAction);
            }
        }

        public static void WriteAllAct(string WritePath,CreatOver NCreatOver,DataRow Config,string StyleAction)
        {
            HtmlByThread LockerStyle = new HtmlByThread();
            int ID = 0;
            foreach (var GetStyle in HtmlCreatEngine.QueueHtml)
            {
                if (GetStyle.HtmlAction == StyleAction)
                {
                    LockerStyle = GetStyle;
                }
            }
            string GetStyleCode = DataHelper.ListToSrt(LockerStyle.AllCode);
            string ArticleCode = "";
            bool IsArticle = false;
            if (GetStyleCode.Contains("{@#article}"))
            {
                if (GetStyleCode.Contains("{/#article}"))
                {
                    IsArticle = true;
                    ArticleCode = BrainConfig.RELKO(GetStyleCode, "{@#article}", "{/#article}");
                    GetStyleCode = GetStyleCode.Replace(("{@#article}" + ArticleCode + "{/#article}"), "(###>StartActCode)");
                }
            }
            if (IsArticle)
            {
                foreach (DataColumn col in Config.Table.Columns)
                {
                    if (col.ColumnName.ToLower() == "ProductID".ToLower())
                    {
                        ID = DataHelper.StrToInt(DataHelper.ObjToStr(Config[col.ColumnName]));
                     
                    }
                    if (col.ColumnName.ToLower() == "ProductText".ToLower())
                    {
                        string GetText = DataHelper.ObjToStr(Config[col.ColumnName]);

                        ArticleCode = ArticleCode.Replace(string.Format("(&{0})", col.ColumnName), PIN.Decrypt(GetText));
                    }
                    else
                    {
                        ArticleCode = ArticleCode.Replace(string.Format("(&{0})", col.ColumnName), DataHelper.ObjToStr(Config[col.ColumnName]));
                    }
                }

                GetStyleCode = GetStyleCode.Replace("(###>StartActCode)", ArticleCode);

                int state = SqlServerHelper.ExecuteNonQuery("UPDATE product SET Creat = 'True',Creattime='" + DateTime.Now.ToString() + "',Createpath='" + NCreatOver.FilePath + "',ProductHref='" + NCreatOver.WebPath + "' WHERE ProductID = '" + ID.ToString() + "'");
                if (state == 0 == false)
                {
                    DataHelper.writefile(WritePath, GetStyleCode, Encoding.UTF8);
                }
            }
        }

        public static CreatOver CreatAct(DataRow Config, string StyleAction)
        {
            CreatOver NCreatOver = new CreatOver();
            string GetActName = CMSHelper.GetDeFineValue("ActName");
            int ID = 0;
            foreach (DataColumn col in Config.Table.Columns)
            {
                if (col.ColumnName.ToLower() == "ProductID".ToLower())
                {
                    ID = DataHelper.StrToInt(DataHelper.ObjToStr(Config[col.ColumnName]));

                }
            }

            DataTable NTable = SqlServerHelper.ExecuteDataTable("Select * From product Where ProductID =" + ID.ToString() + " And Creat ='True'");
            if (NTable.Rows.Count > 0)
            {
                NCreatOver.CreatTime = DateTime.Parse(DataHelper.ObjToStr(NTable.Rows[0]["Creattime"]));
                NCreatOver.FilePath = DataHelper.ObjToStr(NTable.Rows[0]["Createpath"]);
                NCreatOver.WebPath = DataHelper.ObjToStr(NTable.Rows[0]["ProductHref"]);

                return NCreatOver;
            }
            else
            {
                int State = SqlServerHelper.ExecuteNonQuery("Update product Set Creat ='True' Where ProductID=" + ID.ToString());
                if (State == 0 == false)
                {
                    string WritePath = (AppDomain.CurrentDomain.BaseDirectory + @"article\");
                    WritePath = WritePath.Substring(0, WritePath.LastIndexOf(@"\"));
                    if (!WritePath.EndsWith(@"\")) WritePath += @"\";

                    WritePath = WritePath + ID + ".html";
                    NCreatOver.CreatTime = DateTime.Now;
                    NCreatOver.FilePath = WritePath;
                    NCreatOver.WebPath = NCreatOver.FilePath.Replace(AppDomain.CurrentDomain.BaseDirectory, DeFine.DefWebUrl).Replace(@"\", "/");
                    AllActList.Enqueue(new WaitAct(WritePath, NCreatOver, Config, StyleAction));

                }
                return NCreatOver;
            }

          
          
        }

        public static int GetRandSeed()
        {
            byte[] bytes = new byte[8];
            System.Security.Cryptography.RNGCryptoServiceProvider rng = new System.Security.Cryptography.RNGCryptoServiceProvider();
            rng.GetBytes(bytes);
            return BitConverter.ToInt32(bytes, 0);
        }
        public static string RandomChar()
        {
            string[] AllChar = { "a", "b", "c", "d", "e", "f", "g", "h", "i", "j", "k", "l", "m", "n", "o", "p", "q", "r", "s", "t", "u", "v", "w", "x", "y", "z", "A", "B", "C", "D", "E", "F", "G", "H", "I", "J", "K", "L", "M", "N", "O", "P", "Q", "R", "S", "T", "U", "V", "W", "X", "Y", "Z" };

            Random NRandom = new Random(GetRandSeed());
            return AllChar[NRandom.Next(0, AllChar.Length)];
        }

        public static string NoHTML(string Htmlstring)
        {
            Htmlstring = Regex.Replace(Htmlstring, @"<script[^>]*?>.*?</script>", "", RegexOptions.IgnoreCase);
            Htmlstring = Regex.Replace(Htmlstring, @"<(.[^>]*)>", "", RegexOptions.IgnoreCase);
            Htmlstring = Regex.Replace(Htmlstring, @"([\r\n])[\s]+", "", RegexOptions.IgnoreCase);
            Htmlstring = Regex.Replace(Htmlstring, @"-->", "", RegexOptions.IgnoreCase);
            Htmlstring = Regex.Replace(Htmlstring, @"<!--.*", "", RegexOptions.IgnoreCase);
            Htmlstring = Regex.Replace(Htmlstring, @"&(quot|#34);", "\"", RegexOptions.IgnoreCase);
            Htmlstring = Regex.Replace(Htmlstring, @"&(amp|#38);", "&", RegexOptions.IgnoreCase);
            Htmlstring = Regex.Replace(Htmlstring, @"&(lt|#60);", "<", RegexOptions.IgnoreCase);
            Htmlstring = Regex.Replace(Htmlstring, @"&(gt|#62);", ">", RegexOptions.IgnoreCase);
            Htmlstring = Regex.Replace(Htmlstring, @"&(nbsp|#160);", "   ", RegexOptions.IgnoreCase);
            Htmlstring = Regex.Replace(Htmlstring, @"&(iexcl|#161);", "\xa1", RegexOptions.IgnoreCase);
            Htmlstring = Regex.Replace(Htmlstring, @"&(cent|#162);", "\xa2", RegexOptions.IgnoreCase);
            Htmlstring = Regex.Replace(Htmlstring, @"&(pound|#163);", "\xa3", RegexOptions.IgnoreCase);
            Htmlstring = Regex.Replace(Htmlstring, @"&(copy|#169);", "\xa9", RegexOptions.IgnoreCase);
            Htmlstring = Regex.Replace(Htmlstring, @"&#(\d+);", "", RegexOptions.IgnoreCase);
            Htmlstring.Replace("<", "");
            Htmlstring.Replace(">", "");
            Htmlstring.Replace("\r\n", "");
            return Htmlstring;
        }

        public static int GetNavIdByHref(string Href)
        {
            DataTable NTable = SqlServerHelper.ExecuteDataTable("Select * From Nav");
            for (int i = 0; i < NTable.Rows.Count; i++)
            {
                if (DataHelper.ObjToStr(NTable.Rows[i]["ColumnHref"]).Equals(Href))
                { 
                  return DataHelper.StrToInt(DataHelper.ObjToStr(NTable.Rows[i]["ID"]));
                }
            }
            return 0;
        }

        public static void GetActionPath(SignCls LockerSignCls)
        {
            string RichText = "";
            string GetAction = CurrentHtmlAction;



            string SqlOder = "Select * From nav Where ID ='" + GetNavIdByHref(GetAction) + "'";
            DataTable NTable= SqlServerHelper.ExecuteDataTable(SqlOder);
            List<ActionList> AllAction = new List<ActionList>();
            if (NTable.Rows.Count > 0)
            {

       
                string LockerColumnName = DataHelper.ObjToStr(NTable.Rows[0]["ColumnName"]);
                string LockerColumnType = DataHelper.ObjToStr(NTable.Rows[0]["ColumnType"]);
                string LockerColumnHref = DataHelper.ObjToStr(NTable.Rows[0]["ColumnHref"]);

                ActionList NActionList;



                if (LockerColumnType == "Lv2Nav")
                {
                    NActionList = new ActionList();
                    NActionList.ActionName = LockerColumnName;
                    NActionList.ActionPath = GetActionHref(LockerColumnHref);

                    NActionList.FromID = DataHelper.StrToInt(DataHelper.ObjToStr(NTable.Rows[0]["ID"]));

                    string Parent = DataHelper.ObjToStr(NTable.Rows[0]["Parent"]);


                    AllAction.Add(NActionList);

                    if (Parent.Length > 0)
                    {
                        if (Parent.Contains(")"))
                        {
                            foreach (var ActionItem in Parent.Split(')'))
                            {
                                string TempAction = ActionItem;
                                if (ActionItem.StartsWith("("))
                                {
                                    TempAction = TempAction.Substring("(".Length);
                                }

                                    DataTable MainTable = SqlServerHelper.ExecuteDataTable("Select * From nav Where ColumnType ='DefNav' And ColumnName='"+ TempAction + "'");
                                    if (MainTable.Rows.Count > 0)
                                    {
                                        ActionList Lv2ActionList = new ActionList();
                                        Lv2ActionList.ActionName = DataHelper.ObjToStr(MainTable.Rows[0]["ColumnName"]);
                                        Lv2ActionList.ActionPath = GetActionHref(DataHelper.ObjToStr(MainTable.Rows[0]["ColumnHref"]));
                                        Lv2ActionList.FromID = DataHelper.StrToInt(DataHelper.ObjToStr(MainTable.Rows[0]["ID"]));
                                        AllAction.Add(Lv2ActionList);
                                    }
                              
                            }
                        }
                    }
                  
                }
                else
                if (LockerColumnType == "DefNav")
                {
                    NActionList = new ActionList();
                    NActionList.ActionName = LockerColumnName;
                    NActionList.ActionPath = GetActionHref(LockerColumnHref);
                    NActionList.FromID = DataHelper.StrToInt(DataHelper.ObjToStr(NTable.Rows[0]["ID"]));
                    AllAction.Add(NActionList);
                }
                else
                { 
                
                }
                string SplictChar = "";
                foreach (var Parma in LockerSignCls.AllSetting)
                {
                    if (Parma.Key == "ActionSplictChar")
                    {
                        if(Parma.AllValue.Count>0) SplictChar = Parma.AllValue[0];
                    }
                }

                string NewCode = DataHelper.ListToSrt(LockerSignCls.SignCode);
                for (int i = AllAction.Count-1; i >= 0; i--)
                {
                    RichText += NewCode.Replace("(&ActionPath)", AllAction[i].ActionPath).Replace("(&ActionName)", AllAction[i].ActionName + SplictChar) + "\r\n";
                }
                CurrentCode.AddRange(DataHelper.StrToList(RichText));
            }
            else
            {
                CurrentCode.AddRange(DataHelper.StrToList(""));
                return;
            }
        
        }


        public static void GetInputValue(SignCls LockerSignCls)
        {
            string RichText = "";
            RichText= DataHelper.ListToSrt(LockerSignCls.SignCode);
            bool Finds = false;
            foreach (var Parma in LockerSignCls.AllSetting)
            {
                if (Parma.Key == "ParmaList")
                {
                    if (Parma.AllValue.Count > 0) Finds = true;
                    foreach (var GetValue in Parma.AllValue)
                    {
                        string GetItemValue = CMSHelper.GetDeFineValue(GetValue);
                        if (GetItemValue == null) Finds = false;
                        RichText = RichText.Replace("(&"+ GetValue + ")", GetItemValue);
                    }
                }
            }
            if (Finds)
            {
                CurrentCode.AddRange(DataHelper.StrToList(RichText));
            }
            else
            {
                CurrentCode.AddRange(DataHelper.StrToList(""));
            }
        }

        public static void GetObjSign(SignCls LockerSignCls)
        {
            string AutoSqlOder = "Select {0} {1} From {2} ";
            string Params = "";
            List<string> ParamsList = new List<string>();
            List<string> DeCode = new List<string>();
            string TableName = "";
            string Where = "";
            string OrderBy = "";
            int StartRow = 0;
            int MaxRow = 0;

            foreach (var Get in LockerSignCls.AllSetting)
            {
                if (Get.Key.ToLower() == "lockertable")
                {
                    if (Get.AllValue.Count > 0) TableName = Get.AllValue[0];
                }
                if (Get.Key.ToLower() == "orderby")
                {
                    if (Get.AllValue.Count > 0) OrderBy = Get.AllValue[0];
                }
                if (Get.Key.ToLower() == "startrow")
                {
                    if (Get.AllValue.Count > 0)
                    {
                        StartRow = DataHelper.StrToInt(Get.AllValue[0]);
                    }
                }
                if (Get.Key.ToLower() == "maxrow")
                {
                    if (Get.AllValue.Count > 0)
                    {
                        MaxRow = DataHelper.StrToInt(Get.AllValue[0]);
                    }
                }
                if (Get.Key.ToLower() == "where")
                {
                    foreach (var GetItem in Get.AllValue)
                    {
                        Where += " " + GetItem + " And";
                    }
                }
                if (Get.Key.ToLower() == "params")
                {
                    foreach (var GetItem in Get.AllValue)
                    {
                        if (GetItem.StartsWith("Decode_"))
                        {
                            Params += GetItem.Trim().Substring("Decode_".Length) + ",";
                            ParamsList.Add(GetItem.Trim().Substring("Decode_".Length));
                            DeCode.Add(GetItem.Trim().Substring("Decode_".Length));
                        }
                        else
                        {
                            Params += GetItem + ",";
                            ParamsList.Add(GetItem);
                        }

                    }
                }
            }


            if (Where.EndsWith("And"))
            {
                Where = Where.Substring(0, Where.Length - "And".Length);
            }
            if (Params.EndsWith(","))
            {
                Params = Params.Substring(0, Params.Length - 1);
            }
            if (OrderBy.Length > 0)
            {
                OrderBy = " Order by " + OrderBy;
            }
            if (Params.Length > 0 == false)
            {
                Params = "*";
            }


            if (Where.Length > 0)
            {
                Where = " Where " + Where;
            }
            DataTable NTable = null;
            try
            {
                if (MaxRow > 0)
                {
                    NTable = SqlServerHelper.ExecuteDataTable(string.Format(AutoSqlOder, "Top " + MaxRow.ToString(), Params, TableName) + Where + OrderBy);
                }
                else
                {
                    NTable = SqlServerHelper.ExecuteDataTable(string.Format(AutoSqlOder, "", Params, TableName) + Where + OrderBy);
                }
            }
            catch
            {
                CurrentCode.Add("Error Not Find Table Please Check Your InFo");
            }
            if (NTable.Rows.Count > 0)
            {

                for (int i = 0; i < NTable.Rows.Count; i++)
                {
                    bool Loading = true;
                    if (StartRow == 0 == false)
                    {
                        if (i < StartRow) Loading = false;
                    }
                    if (Loading)
                    {
                        string TmpCodeFormat = DataHelper.ListToSrt(LockerSignCls.SignCode);
                        if (ParamsList.Count > 0)
                        {
                            foreach (var Get in ParamsList)
                            {
                                try
                                {
                                    if (DeCode.Contains(Get))
                                    {
                                        string Value = DataHelper.ObjToStr(NTable.Rows[i][Get]);
                                        TmpCodeFormat = TmpCodeFormat.Replace(string.Format("(&{0})", Get), PIN.Decrypt(Value));
                                    }
                                    else
                                    {
                                        string Value = DataHelper.ObjToStr(NTable.Rows[i][Get]);
                                        TmpCodeFormat = TmpCodeFormat.Replace(string.Format("(&{0})", Get), Value);
                                    }
                                }
                                catch
                                {
                                    TmpCodeFormat = TmpCodeFormat.Replace(string.Format("(&{0})", Get), "(&NotFind:" + Get + ")");
                                }
                            }
                            CurrentCode.AddRange(DataHelper.StrToList(TmpCodeFormat));
                        }
                        else
                        {
                            DataRow Row = NTable.Rows[i];
                            foreach (DataColumn col in NTable.Rows[i].Table.Columns)
                            {
                                TmpCodeFormat = TmpCodeFormat.Replace(string.Format("(&{0})", col.ColumnName), DataHelper.ObjToStr(NTable.Rows[i][col]));
                            }
                            CurrentCode.AddRange(DataHelper.StrToList(TmpCodeFormat));
                        }
                    }
                }
            }
        }
        public static void GetNavSign(SignCls LockerSignCls)
        {
            string AutoSqlOder = "Select {0} {1} From {2} ";
            string Params = "";
            List<string> ParamsList = new List<string>();
            string Where = "ColumnType = 'DefNav' And Parent = '' And";
            string OrderBy = "";
            int StartRow = 0;
            int MaxRow = 0;
            string AutoSelect = "";
            string LockerHtmlAction = "";

            foreach (var Get in LockerSignCls.AllSetting)
            {
                if (Get.Key.ToLower() == "autoselect")
                {
                    if (Get.AllValue.Count > 0)
                    {
                        AutoSelect = Get.AllValue[0];
                        AutoSelect = AutoSelect.Replace("'", "\"");
                    }

                }
                if (Get.Key.ToLower() == "orderby")
                {
                    if (Get.AllValue.Count > 0) OrderBy = Get.AllValue[0];
                }
                if (Get.Key.ToLower() == "startrow")
                {
                    if (Get.AllValue.Count > 0)
                    {
                        StartRow = DataHelper.StrToInt(Get.AllValue[0]);
                    }
                }
                if (Get.Key.ToLower() == "maxrow")
                {
                    if (Get.AllValue.Count > 0)
                    {
                        MaxRow = DataHelper.StrToInt(Get.AllValue[0]);
                    }
                }
                if (Get.Key.ToLower() == "where")
                {
                    foreach (var GetItem in Get.AllValue)
                    {
                        Where += " " + GetItem + " And";
                    }
                }
                if (Get.Key.ToLower() == "params")
                {
                    foreach (var GetItem in Get.AllValue)
                    {
                        Params += GetItem + ",";
                        ParamsList.Add(GetItem);
                    }
                }
            }

            if (Where.EndsWith("And"))
            {
                Where = Where.Substring(0, Where.Length - "And".Length);
            }
            if (Params.EndsWith(","))
            {
                Params = Params.Substring(0, Params.Length - 1);
            }
            if (OrderBy.Length > 0)
            {
                OrderBy = " Order by " + OrderBy;
            }
            if (Params.Length > 0 == false)
            {
                Params = "*";
            }


            if (Where.Length > 0)
            {
                Where = " Where " + Where;
            }
            DataTable NTable = null;
            try
            {
                if (MaxRow > 0)
                {
                    NTable = SqlServerHelper.ExecuteDataTable(string.Format(AutoSqlOder, "Top " + MaxRow.ToString(), Params, "nav") + Where + OrderBy);
                }
                else
                {
                    NTable = SqlServerHelper.ExecuteDataTable(string.Format(AutoSqlOder, "", Params, "nav") + Where + OrderBy);
                }
            }
            catch
            {
                CurrentCode.Add("Error Not Find Table Please Check Your InFo");
            }
            if (NTable == null) return;
            if (NTable.Rows.Count > 0)
            {

                for (int i = 0; i < NTable.Rows.Count; i++)
                {
                    bool Loading = true;
                    if (StartRow == 0 == false)
                    {
                        if (i < StartRow) Loading = false;
                    }
                    if (Loading)
                    {
                        string TmpCodeFormat = DataHelper.ListToSrt(LockerSignCls.SignCode);
                        string GetColumnName = DataHelper.ObjToStr(NTable.Rows[i]["ColumnName"]);
                        string Lv2TmpCode = "";
                        string Lv2ListCode = "";
                        string StartCode = "";
                        string EndCode = "";
                        string Lv2OrderBy = "";
                        int Max = 0;
                        int Start = 0;

                        DataTable Lv2Column = null;

                        if (TmpCodeFormat.Contains("{@#ChildrenList"))
                        {

                            Regex NRegex = new Regex(@"(.*?){@#ChildrenList(.*?)}(.*?)");
                            MatchCollection SignList = NRegex.Matches(TmpCodeFormat);
                            foreach (Match Item in SignList)
                            {
                                StartCode = Item.Value;

                                string GetCurrentSetting = Item.Value.Substring(Item.Value.IndexOf("{@#ChildrenList") + "{@#ChildrenList".Length);
                                GetCurrentSetting = GetCurrentSetting.Substring(0, GetCurrentSetting.LastIndexOf("}"));
                                if (GetCurrentSetting.Contains("&"))
                                {
                                    foreach (var Get in GetCurrentSetting.Split('&'))
                                    {
                                        string GetType = Get.Replace(";", "");
                                        if (GetType.Contains(":"))
                                        {
                                            if (GetType.Split(':')[0].ToLower() == "maxrow")
                                            {
                                                Max = DataHelper.StrToInt(GetType.Split(':')[1]);
                                            }

                                            if (GetType.Split(':')[0].ToLower() == "startrow")
                                            {
                                                Start = DataHelper.StrToInt(GetType.Split(':')[1]);
                                            }
                                            if (GetType.Split(':')[0].ToLower() == "orderby")
                                            {
                                                Lv2OrderBy = " Order by " + GetType.Split(':')[1];
                                            }
                                        }
                                    }
                                }
                                EndCode = "{/#ChildrenList}";

                            }
                            Lv2TmpCode = BrainConfig.RELKO(TmpCodeFormat, StartCode, EndCode);
                            if (Max == 0)
                            {
                                Lv2Column = SqlServerHelper.ExecuteDataTable("Select * From nav Where ColumnType = 'Lv2Nav' And Parent LIKE '%(" + GetColumnName + ")%' " + Lv2OrderBy);
                            }
                            else
                            {
                                Lv2Column = SqlServerHelper.ExecuteDataTable("Select Top " + Max.ToString() + " * From nav Where ColumnType = 'Lv2Nav' And Parent LIKE '%(" + GetColumnName + ")%' " + Lv2OrderBy);
                            }

                        }
                        if (Lv2Column == null == false)
                        {
                            if (Lv2Column.Rows.Count > 0)
                            {
                                if (TmpCodeFormat.Contains("{@#AutoChildrenHide}"))
                                {
                                    TmpCodeFormat = TmpCodeFormat.Replace("{@#AutoChildrenHide}", "");
                                    TmpCodeFormat = TmpCodeFormat.Replace("{/#AutoChildrenHide}", "");
                                }

                                for (int ir = 0; ir < Lv2Column.Rows.Count; ir++)
                                {
                                    string NextTmp = Lv2TmpCode;
                                    int RowNumber = 0;
                                    foreach (DataColumn col in Lv2Column.Rows[ir].Table.Columns)
                                    {
                                        RowNumber++;
                                        bool LoadingLv2 = true;
                                        if (Start == 0 == false)
                                        {
                                            if (RowNumber < Start) LoadingLv2 = false;
                                        }
                                        if (LoadingLv2)
                                        {
                                            if (col.ColumnName.ToLower() == "ColumnHref".ToLower())
                                            {
                                                NextTmp = NextTmp.Replace(string.Format("(&{0})", col.ColumnName), GetActionHref(DataHelper.ObjToStr(Lv2Column.Rows[ir][col])));
                                            }
                                            else
                                            {
                                                NextTmp = NextTmp.Replace(string.Format("(&{0})", col.ColumnName), DataHelper.ObjToStr(Lv2Column.Rows[ir][col]));
                                            }
                                        }
                                    }
                                    Lv2ListCode += NextTmp + "\r\n";
                                }
                                TmpCodeFormat = TmpCodeFormat.Replace(Lv2TmpCode, Lv2ListCode);
                                TmpCodeFormat = TmpCodeFormat.Replace(StartCode, "").Replace(EndCode, "");
                            }
                            else
                            {
                                TmpCodeFormat = TmpCodeFormat.Replace(Lv2TmpCode, "");
                                TmpCodeFormat = TmpCodeFormat.Replace(StartCode, "").Replace(EndCode, "");
                                if (TmpCodeFormat.Contains("{@#AutoChildrenHide}"))
                                {
                                    TmpCodeFormat = TmpCodeFormat.Replace(BrainConfig.RELKO(TmpCodeFormat, "{@#AutoChildrenHide}", "{/#AutoChildrenHide}"), "");
                                    TmpCodeFormat = TmpCodeFormat.Replace("{@#AutoChildrenHide}", "");
                                    TmpCodeFormat = TmpCodeFormat.Replace("{/#AutoChildrenHide}", "");
                                }
                            }

                        }
              


                        if (ParamsList.Count > 0)
                        {
                            foreach (var Get in ParamsList)
                            {
                                try
                                {
                                    string Value = DataHelper.ObjToStr(NTable.Rows[i][Get]);



                                    if (Get.ToLower() == "ColumnHref".ToLower())
                                    {
                                        if (Value.Contains("/"))
                                        {
                                            LockerHtmlAction = LockerHtmlAction.Substring(Value.LastIndexOf("/"));
                                        }
                                        else
                                        {
                                            LockerHtmlAction = Value;
                                        }
                                            LockerHtmlAction = LockerHtmlAction.ToLower().Replace(".html", "");

                                        if (CurrentHtmlAction.Contains(LockerHtmlAction))
                                        {
                                            TmpCodeFormat = TmpCodeFormat.Replace("(&autoselect)", AutoSelect);
                                        }
                                        else
                                        {
                                            TmpCodeFormat = TmpCodeFormat.Replace("(&autoselect)","");
                                        }
                                    }
                                     
                                    TmpCodeFormat = TmpCodeFormat.Replace("(&Select)", ("ID =''").Replace("'", "\""));
                                    
                                    if (Get.ToLower() == "ColumnHref".ToLower())
                                    {
                                        TmpCodeFormat = TmpCodeFormat.Replace(string.Format("(&{0})", Get), GetActionHref(Value));
                                    }
                                    else
                                    {
                                        TmpCodeFormat = TmpCodeFormat.Replace(string.Format("(&{0})", Get), Value);
                                    }


                                }
                                catch
                                {
                                    TmpCodeFormat = TmpCodeFormat.Replace(string.Format("(&{0})", Get), "(&NotFind:" + Get + ")");
                                }
                            }
                            CurrentCode.AddRange(DataHelper.StrToList(TmpCodeFormat));
                        }
                        else
                        {
                            DataRow Row = NTable.Rows[i];
                            foreach (DataColumn col in NTable.Rows[i].Table.Columns)
                            {
                              
                               TmpCodeFormat = TmpCodeFormat.Replace("(&Select)", ("ID =''").Replace("'", "\""));

                                if (col.ColumnName.ToLower() == "ColumnHref".ToLower())
                                {
                                    if (DataHelper.ObjToStr(NTable.Rows[i][col]).Contains("/"))
                                    {
                                        LockerHtmlAction = LockerHtmlAction.Substring(DataHelper.ObjToStr(NTable.Rows[i][col]).LastIndexOf("/"));
                                    }
                                    else
                                    {
                                        LockerHtmlAction = DataHelper.ObjToStr(NTable.Rows[i][col]);
                                    }
                                    LockerHtmlAction = LockerHtmlAction.ToLower().Replace(".html", "");


                                    if (LockerHtmlAction.Length > 0)
                                    {
                                        if (CurrentHtmlAction.Contains(LockerHtmlAction))
                                        {
                                            TmpCodeFormat = TmpCodeFormat.Replace("(&autoselect)", AutoSelect);
                                        }
                                        else
                                        {
                                            TmpCodeFormat = TmpCodeFormat.Replace("(&autoselect)", "");
                                        }
                                    }

                                }


                                if (col.ColumnName.ToLower() == "ColumnHref".ToLower())
                                {
                                    TmpCodeFormat = TmpCodeFormat.Replace(string.Format("(&{0})", col.ColumnName), GetActionHref(DataHelper.ObjToStr(NTable.Rows[i][col])));
                                }
                                else
                                {
                                    TmpCodeFormat = TmpCodeFormat.Replace(string.Format("(&{0})", col.ColumnName), DataHelper.ObjToStr(NTable.Rows[i][col]));
                                }
                            }

                           
                            CurrentCode.AddRange(DataHelper.StrToList(TmpCodeFormat));
                        }
                    }
                }
            }
        }
        public static void GetQuickGetSign(SignCls LockerSignCls)
        {
            foreach (var Get in LockerSignCls.AllSetting)
            {
                if (Get.Key.ToLower() == "lockertable")
                {
                    if (Get.AllValue.Count > 0)
                    {
                        CurrentCode.AddRange(DataHelper.StrToList(DataHelper.ListToSrt(LockerSignCls.SignCode).Replace("{&INPUTJSON}", JsonHelper.DataFormatToJson(SqlServerHelper.ExecuteDataTable("Select * From " + Get.AllValue[0])))));
                    }
                }
            }
            CurrentCode.Add("JsonError");
        }
        public static void GetListTSign(SignCls LockerSignCls)
        {
            string LockerParent = "";
            foreach (var Get in LockerSignCls.AllSetting)
            {
                if (Get.Key == "lockertable")
                {
                    if (Get.AllValue.Count > 0) LockerParent = Get.AllValue[0];
                }
            }
            DataTable NTable = SqlServerHelper.ExecuteDataTable("Select * From nav Where Parent='" + LockerParent + "' And ColumnType='ListT'");
            if (NTable.Rows.Count > 0)
            {
                string RichText = "";
                for (int i = 0; i < NTable.Rows.Count; i++)
                {
                    List<string> KeyList = DataHelper.ObjToStr(NTable.Rows[i]["ColumnName"]).Split(';').ToList();
                    List<string> SrcList = DataHelper.ObjToStr(NTable.Rows[i]["ColumnHref"]).Split(';').ToList();
                    for (int ir = 0; ir < KeyList.Count; ir++)
                    {
                        if (KeyList[ir].Length > 0)
                        {
                            if (SrcList.Count >= ir)
                            {
                                RichText += DataHelper.ListToSrt(LockerSignCls.SignCode).Replace("(&Key)", KeyList[ir]).Replace("(&Src)", SrcList[ir]) + "\r\n";
                            }
                        }
                    }
                }
                CurrentCode.AddRange(DataHelper.StrToList(RichText));
            }
        }
        public static void GetListTTSign(SignCls LockerSignCls)
        {
            string LockerParent = "";
            foreach (var Get in LockerSignCls.AllSetting)
            {
                if (Get.Key == "lockertable")
                {
                    if (Get.AllValue.Count > 0) LockerParent = Get.AllValue[0];
                }
            }
            DataTable NTable = SqlServerHelper.ExecuteDataTable("Select * From nav Where Parent='" + LockerParent + "' And ColumnType='ListTT'");
            if (NTable.Rows.Count > 0)
            {
                string RichText = "";
                for (int i = 0; i < NTable.Rows.Count; i++)
                {
                    List<string> TypeKeyList = DataHelper.ObjToStr(NTable.Rows[i]["ColumnName"]).Split(';').ToList();
                    List<string> SrcList = DataHelper.ObjToStr(NTable.Rows[i]["ColumnHref"]).Split(';').ToList();
                    for (int ir = 0; ir < TypeKeyList.Count; ir++)
                    {
                        if (SrcList.Count >= ir)
                        {
                            string TypeKey = TypeKeyList[ir];
                            if (TypeKey.Contains(">"))
                            {
                                RichText += DataHelper.ListToSrt(LockerSignCls.SignCode).Replace("(&Key)", TypeKey.Split('>')[0]).Replace("(&Value)", TypeKey.Split('>')[1]).Replace("(&Src)", SrcList[ir]) + "\r\n";
                            }
                        }
                    }
                }
                CurrentCode.AddRange(DataHelper.StrToList(RichText));
            }
        }

        public static HtmlByThread GetActionCode(string Action)
        { 
            foreach(var Get in HtmlCreatEngine.QueueHtml)
            {
                if (Get.HtmlAction == Action)
                {
                    return Get;
                }
            }
            return new HtmlByThread();
        }

        public static void GetActSign(SignCls LockerSignCls)
        {
            string OrderBy = "";
            int StartRow = 0;
            int MaxRow = 0;
            string Where = " And ";
            string ActType = "";
            List<string> MaxSize = new List<string>();
            foreach (var Get in LockerSignCls.AllSetting)
            {
                if (Get.Key.ToLower() == "orderby")
                {
                    if (Get.AllValue.Count > 0) OrderBy = Get.AllValue[0];
                }
                if (Get.Key.ToLower() == "startrow")
                {
                    if (Get.AllValue.Count > 0)
                    {
                        StartRow = DataHelper.StrToInt(Get.AllValue[0]);
                    }
                }
                if (Get.Key.ToLower() == "maxrow")
                {
                    if (Get.AllValue.Count > 0)
                    {
                        MaxRow = DataHelper.StrToInt(Get.AllValue[0]);
                    }
                }
                if (Get.Key.ToLower() == "where")
                {
                    foreach (var GetItem in Get.AllValue)
                    {
                        Where += " " + GetItem + " And";
                    }
                }
                if (Get.Key.ToLower() == "maxsize")
                {
                    foreach (var GetItem in Get.AllValue)
                    {
                        MaxSize.Add(GetItem);
                    }
                }
                if (Get.Key.ToLower() == "type")
                {
                    foreach (var GetItem in Get.AllValue)
                    {
                        if (GetItem.ToLower() == "auto")
                        {
                            ActType = SignHelper.CurrentHtmlAction;
                            string FullActType = DataHelper.ObjToStr(SqlServerHelper.ExecuteScalar("Select ColumnName From nav Where ColumnHref='" + ActType + "'"));
                            ActType = FullActType;
                        }
                        else
                        {
                            if (GetItem.Length > 0)
                            {
                                ActType = GetItem;
                            }
                        }
                    }
                }
            }
            if (Where.EndsWith("And"))
            {
                Where = Where.Substring(0, Where.Length - "And".Length);
            }
            if (OrderBy.Length > 0)
            {
                OrderBy = " Order by " + OrderBy;
            }
            string AutoTop = "";
            if (MaxRow > 0)
            {
                AutoTop = " Top " + MaxRow.ToString();
            }

            DataTable NTable = null;
            if (Where == " And ")
            {
                NTable = SqlServerHelper.ExecuteDataTable("Select " + AutoTop + " * From product Where Type='" + ActType + "'" + OrderBy);
            }
            else
            {
                NTable = SqlServerHelper.ExecuteDataTable("Select " + AutoTop + " * From product Where Type='" + ActType + "' " + Where + OrderBy);
            }
            if (NTable.Rows.Count > 0)
            {
                object LockerStyleTmp = SqlServerHelper.ExecuteScalar("Select LockerTemplate From Nav Where ColumnName ='" + ActType + "'");
                string RichText = "";
                for (int i = 0; i < NTable.Rows.Count; i++)
                {
                    if (i >= StartRow)
                    {
                        DataRow GetRow = NTable.Rows[i];
                        CreatOver CreatItem = CreatAct(GetRow, DataHelper.ObjToStr(LockerStyleTmp));

                        string TmpString = DataHelper.ListToSrt(LockerSignCls.SignCode);
                        foreach (DataColumn col in NTable.Rows[i].Table.Columns)
                        {
                            string ProductText = "";
                            if (col.ColumnName == "ProductText")
                            {
                                ProductText = NoHTML(PIN.Decrypt(DataHelper.ObjToStr(NTable.Rows[i][col])));
                            }

                            string LockerName = "";
                            int LockerLength = 0;
                            string Value = "";
                            foreach (var Get in MaxSize)
                            {
                                if (Get.Contains(">"))
                                {
                                    if (Get.Split('>')[0].ToLower() == col.ColumnName.ToLower())
                                    {
                                        LockerName = Get.Split('>')[0];
                                        LockerLength = DataHelper.StrToInt(Get.Split('>')[1]);
                                    }
                                }
                            }
                            if (col.ColumnName.ToLower() == "ProductText".ToLower())
                            {
                                Value = ProductText;
                            }
                            else
                            {
                                Value = DataHelper.ObjToStr(NTable.Rows[i][col]);
                            }
                            if (col.ColumnName.ToLower() == "ProductHref".ToLower())
                            {
                                Value = CreatItem.WebPath;
                            }
                            if (col.ColumnName.ToLower() == "Createpath".ToLower())
                            {
                                Value = CreatItem.FilePath;
                            }
                            if (col.ColumnName.ToLower() == "Creattime".ToLower())
                            {
                                Value = CreatItem.CreatTime.ToString();
                            }

                            LockerName = col.ColumnName;
                            if (LockerLength > 0)
                            {
                                if (Value.Length > LockerLength)
                                {
                                    Value = Value.Substring(0, LockerLength);
                                }
                            }
                            TmpString = TmpString.Replace(string.Format("(&{0})", LockerName), Value) + "\r\n";
                        }
                        RichText += TmpString + "\r\n";
                    }
                }
                CurrentCode.AddRange(DataHelper.StrToList(RichText));
            }

        }


        public static List<StartPage> AllPageCode = new List<StartPage>();

        public static StartPage NextPage(int PageCount, SignCls LockerSignCls, bool Creat = true)
        {
            string GetActList = CMSHelper.GetDeFineValue("ActList");
            GetActList = GetActList.Replace("(ID)", (PageCount - 1).ToString());
            GetActList = GetActList.Replace("(TimeStamp)", TimeHelper.DateTimeToStamp(DateTime.Now));
            GetActList = GetActList.Replace("(TIME:dd)", DateTime.Now.Day.ToString());
            GetActList = GetActList.Replace("(TIME:yyyy)", DateTime.Now.Year.ToString());
            GetActList = GetActList.Replace("(TIME:MM)", DateTime.Now.Month.ToString());
            GetActList = GetActList.Replace("(Action)", CurrentHtmlAction);
            foreach (var Item in GetActList.Split(')'))
            {
                string GetItem = Item;
                if (GetItem.Trim().Length > 0)
                {
                    if (GetItem.Contains("(Random:"))
                    {
                        GetItem = GetItem.Substring(GetItem.IndexOf("(Random:"));
                        string Get = GetItem.Replace("(Random:", "");
                        if (Get.ToLower().Contains("x"))
                        {
                            string NextChar = "";
                            for (int i = 0; i < Get.Length; i++)
                            {
                                NextChar += RandomChar();
                            }
                            GetActList = GetActList.Replace("(Random:" + Get + ")", NextChar);
                        }
                        else
                        {
                            if (Get.Contains("-"))
                            {
                                long tick = DateTime.Now.Ticks;
                                GetActList = GetActList.Replace("(Random:" + Get.Split('-')[0] + "-" + Get.Split('-')[1] + ")", new Random((int)(tick & 0xffffffffL) | (int)(tick >> 32)).Next(DataHelper.StrToInt(Get.Split('-')[0]), DataHelper.StrToInt(Get.Split('-')[1])).ToString());
                            }
                        }
                    }
                }
            }

            StartPage NStartPage = new StartPage();
            NStartPage.Page = PageCount;
            NStartPage.PageAction = CurrentHtmlAction;
            NStartPage.LockerSignCls = LockerSignCls;
            NStartPage.PageName = GetActList;
            NStartPage.SavePath = DeFine.ArticleSavePath + NStartPage.PageName+".html";
            string GetWebUrl = DeFine.DefWebUrl;
            if (!GetWebUrl.EndsWith("/")) GetWebUrl += "/";
            NStartPage.WebPath = (GetWebUrl + (DeFine.ArticleSavePath.Substring(AppDomain.CurrentDomain.BaseDirectory.Length)) + NStartPage.PageName).Replace(@"\", "/") + ".html";
            if (!Creat)
            {
                AllPageCode.Add(NStartPage);
            }
            return NStartPage;
        }

        public static void WriteAllPage(string ActPageCode)
        {
            for (int i = 0; i < AllPageCode.Count; i++)
            {
                string NextPageWebPath = "";
                string OldPageWebPath = "";
                if (i == 0)
                {
                    OldPageWebPath = GetActionHref(AllPageCode[i].PageAction);
                }
                else
                {
                    OldPageWebPath = AllPageCode[i - 1].WebPath;
                }
                if ((i + 1) == AllPageCode.Count)
                {
                    NextPageWebPath = "";
                }
                else
                {
                    NextPageWebPath = AllPageCode[i + 1].WebPath;
                }

                string TableName = "";
                string Where = "";
                string OrderBy = "";
                int StartRow = 0;
                int MaxRow = 0;
                int page = 0;
                List<string> MaxSize = new List<string>();
                string ActType = "";

                foreach (var Get in AllPageCode[i].LockerSignCls.AllSetting)
                {
                    if (Get.Key.ToLower() == "maxsize")
                    {
                        foreach (var GetItem in Get.AllValue)
                        {
                            MaxSize.Add(GetItem);
                        }
                    }
                    if (Get.Key.ToLower() == "page")
                    {
                        if (Get.AllValue.Count > 0) page = DataHelper.StrToInt(Get.AllValue[0]);
                    }
                    if (Get.Key.ToLower() == "lockertable")
                    {
                        if (Get.AllValue.Count > 0) TableName = Get.AllValue[0];
                    }
                    if (Get.Key.ToLower() == "orderby")
                    {
                        if (Get.AllValue.Count > 0) OrderBy = Get.AllValue[0];
                    }
                    if (Get.Key.ToLower() == "startrow")
                    {
                        if (Get.AllValue.Count > 0)
                        {
                            StartRow = DataHelper.StrToInt(Get.AllValue[0]);
                        }
                    }
                    if (Get.Key.ToLower() == "maxrow")
                    {
                        if (Get.AllValue.Count > 0)
                        {
                            MaxRow = DataHelper.StrToInt(Get.AllValue[0]);
                        }
                    }
                    if (Get.Key.ToLower() == "type")
                    {
                        foreach (var GetItem in Get.AllValue)
                        {
                            if (GetItem.ToLower() == "auto")
                            {
                                ActType = SignHelper.CurrentHtmlAction;
                                string FullActType = DataHelper.ObjToStr(SqlServerHelper.ExecuteScalar("Select ColumnName From nav Where ColumnHref='" + ActType + "'"));
                                ActType = FullActType;
                            }
                            else
                            {
                                if (GetItem.Length > 0)
                                {
                                    ActType = GetItem;
                                }
                            }
                        }
                    }
                }


                if (Where.EndsWith("And"))
                {
                    Where = Where.Substring(0, Where.Length - "And".Length);
                }
                if (OrderBy.Length > 0)
                {
                    OrderBy = " Order by " + OrderBy;
                }

                SQLSetting PageSetting = new SQLSetting();
                PageSetting.LockerTable.Add("product");
                PageSetting.OderBy = OrderBy;
                PageSetting.PageLength = page;
                FindKey NFindKey = new FindKey();
                NFindKey.Key = "Type";
                NFindKey.Value = "'" + ActType + "'";
                PageSetting.Condition.Add(NFindKey);
                PageSetting.FindTime = DateTime.Now;
                PageSetting.StartRow = StartRow;
                PageSetting.MaxRow = MaxRow;

                int PageCount = 0;

                DataTable NTable = SqlServerCommand.GetSignData(PageSetting, AllPageCode[i].Page, ref PageCount);


                if (NTable.Rows.Count > 0)
                {
                    object LockerStyleTmp = SqlServerHelper.ExecuteScalar("Select LockerTemplate From Nav Where ColumnName ='" + ActType + "'");
                    string PageCode = "";
                    string RichText = "";
                    string TmpString = DataHelper.ListToSrt(AllPageCode[i].LockerSignCls.SignCode);
                    PageCode = BrainConfig.RELKO(TmpString, "{@#ActListCon}", "{/#ActListCon}");
                    for (int ir = 0; ir < NTable.Rows.Count; ir++)
                    {
                        TmpString = DataHelper.ListToSrt(AllPageCode[i].LockerSignCls.SignCode);
                        DataRow GetRow = NTable.Rows[ir];
                        CreatOver CreatItem = CreatAct(GetRow, DataHelper.ObjToStr(LockerStyleTmp));
                        foreach (DataColumn col in NTable.Rows[ir].Table.Columns)
                        {
                            string ProductText = "";
                            if (col.ColumnName == "ProductText")
                            {
                                ProductText = NoHTML(PIN.Decrypt(DataHelper.ObjToStr(NTable.Rows[ir][col])));
                            }

                            string LockerName = "";
                            int LockerLength = 0;
                            string Value = "";
                            foreach (var Get in MaxSize)
                            {
                                if (Get.Contains(">"))
                                {
                                    if (Get.Split('>')[0].ToLower() == col.ColumnName.ToLower())
                                    {
                                        LockerName = Get.Split('>')[0];
                                        LockerLength = DataHelper.StrToInt(Get.Split('>')[1]);
                                    }
                                }
                            }
                            if (col.ColumnName.ToLower() == "ProductText".ToLower())
                            {
                                Value = ProductText;
                            }
                            else
                            {
                                Value = DataHelper.ObjToStr(NTable.Rows[ir][col]);
                            }
                            if (col.ColumnName.ToLower() == "ProductHref".ToLower())
                            {
                                Value = CreatItem.WebPath;
                            }
                            if (col.ColumnName.ToLower() == "Createpath".ToLower())
                            {
                                Value = CreatItem.FilePath;
                            }
                            if (col.ColumnName.ToLower() == "Creattime".ToLower())
                            {
                                Value = CreatItem.CreatTime.ToString();
                            }

                            LockerName = col.ColumnName;
                            if (LockerLength > 0)
                            {
                                if (Value.Length > LockerLength)
                                {
                                    Value = Value.Substring(0, LockerLength);
                                }
                            }
                            TmpString = TmpString.Replace(string.Format("(&{0})", LockerName), Value);
                        }

                        TmpString = TmpString.Replace("{@#ActListCon}" + PageCode + "{/#ActListCon}", "");
                        RichText += TmpString + "\r\n";
                    }
                    PageCode = PageCode.Replace("(@previouspage)", OldPageWebPath);
                    PageCode = PageCode.Replace("(@CurrentPage)", (AllPageCode[i].Page).ToString());
                    PageCode = PageCode.Replace("(@MaxPage)", PageCount.ToString());
                    PageCode = PageCode.Replace("(@nextpage)", NextPageWebPath);
                    RichText += PageCode + "\r\n";
                    string WriteText = ActPageCode.Replace(BrainConfig.RELKO(ActPageCode, "<!--@StartListCode-->\r\n", "<!--@EndListCode-->\r\n"), RichText);
                    DataHelper.writefile(AllPageCode[i].SavePath,WriteText, Encoding.UTF8);
                    //True
                }

            }
            AllPageCode.Clear();
        }

        public static void GetActListSign(SignCls LockerSignCls)
        {
            string TableName = "";
            string Where = "";
            string OrderBy = "";
            int StartRow = 0;
            int MaxRow = 0;
            int page = 0;
            List<string> MaxSize = new List<string>();
            string ActType = "";
            foreach (var Get in LockerSignCls.AllSetting)
            {
                if (Get.Key.ToLower() == "maxsize")
                {
                    foreach (var GetItem in Get.AllValue)
                    {
                        MaxSize.Add(GetItem);
                    }
                }
                if (Get.Key.ToLower() == "page")
                {
                    if (Get.AllValue.Count > 0) page = DataHelper.StrToInt(Get.AllValue[0]);
                }
                if (Get.Key.ToLower() == "lockertable")
                {
                    if (Get.AllValue.Count > 0) TableName = Get.AllValue[0];
                }
                if (Get.Key.ToLower() == "orderby")
                {
                    if (Get.AllValue.Count > 0) OrderBy = Get.AllValue[0];
                }
                if (Get.Key.ToLower() == "startrow")
                {
                    if (Get.AllValue.Count > 0)
                    {
                        StartRow = DataHelper.StrToInt(Get.AllValue[0]);
                    }
                }
                if (Get.Key.ToLower() == "maxrow")
                {
                    if (Get.AllValue.Count > 0)
                    {
                        MaxRow = DataHelper.StrToInt(Get.AllValue[0]);
                    }
                }
                if (Get.Key.ToLower() == "type")
                {
                    foreach (var GetItem in Get.AllValue)
                    {
                        if (GetItem.ToLower() == "auto")
                        {
                            ActType = SignHelper.CurrentHtmlAction;
                            string FullActType = DataHelper.ObjToStr(SqlServerHelper.ExecuteScalar("Select ColumnName From nav Where ColumnHref='" + ActType + "'"));
                            ActType = FullActType;
                        }
                        else
                        {
                            if (GetItem.Length > 0)
                            {
                                ActType = GetItem;
                            }
                        }
                    }
                }
            }


            if (Where.EndsWith("And"))
            {
                Where = Where.Substring(0, Where.Length - "And".Length);
            }
            if (OrderBy.Length > 0)
            {
                OrderBy = " Order by " + OrderBy;
            }


            SQLSetting PageSetting = new SQLSetting();
            PageSetting.LockerTable.Add("product");
            PageSetting.OderBy = OrderBy;
            PageSetting.PageLength = page;
            FindKey NFindKey = new FindKey();
            NFindKey.Key = "Type";
            NFindKey.Value = "'" + ActType + "'";
            PageSetting.Condition.Add(NFindKey);
            PageSetting.FindTime = DateTime.Now;
            PageSetting.StartRow = StartRow;
            PageSetting.MaxRow = MaxRow;

            int PageCount = 0;

            DataTable NTable = SqlServerCommand.GetSignData(PageSetting, 1, ref PageCount);

            if (NTable.Rows.Count > 0)
            {
                object LockerStyleTmp = SqlServerHelper.ExecuteScalar("Select LockerTemplate From Nav Where ColumnName ='" + ActType + "'");
                string RichText = "";
                string PageCode = "";
                string TmpString = DataHelper.ListToSrt(LockerSignCls.SignCode);

                PageCode = BrainConfig.RELKO(TmpString, "{@#ActListCon}", "{/#ActListCon}");
                for (int i = 0; i < NTable.Rows.Count; i++)
                {
                    TmpString = DataHelper.ListToSrt(LockerSignCls.SignCode);
                    DataRow GetRow = NTable.Rows[i];
                    CreatOver CreatItem = CreatAct(GetRow, DataHelper.ObjToStr(LockerStyleTmp));
                    foreach (DataColumn col in NTable.Rows[i].Table.Columns)
                    {
                        string ProductText = "";
                        if (col.ColumnName == "ProductText")
                        {
                            ProductText = NoHTML(PIN.Decrypt(DataHelper.ObjToStr(NTable.Rows[i][col])));
                        }

                        string LockerName = "";
                        int LockerLength = 0;
                        string Value = "";
                        foreach (var Get in MaxSize)
                        {
                            if (Get.Contains(">"))
                            {
                                if (Get.Split('>')[0].ToLower() == col.ColumnName.ToLower())
                                {
                                    LockerName = Get.Split('>')[0];
                                    LockerLength = DataHelper.StrToInt(Get.Split('>')[1]);
                                }
                            }
                        }
                        if (col.ColumnName.ToLower() == "ProductText".ToLower())
                        {
                            Value = ProductText;
                        }
                        else
                        {
                            Value = DataHelper.ObjToStr(NTable.Rows[i][col]);
                        }
                        if (col.ColumnName.ToLower() == "ProductHref".ToLower())
                        {
                            Value = CreatItem.WebPath;
                        }
                        if (col.ColumnName.ToLower() == "Createpath".ToLower())
                        {
                            Value = CreatItem.FilePath;
                        }
                        if (col.ColumnName.ToLower() == "Creattime".ToLower())
                        {
                            Value = CreatItem.CreatTime.ToString();
                        }

                        LockerName = col.ColumnName;
                        if (LockerLength > 0)
                        {
                            if (Value.Length > LockerLength)
                            {
                                Value = Value.Substring(0, LockerLength);
                            }
                        }
                        TmpString = TmpString.Replace(string.Format("(&{0})", LockerName), Value);
                    }

                    TmpString = TmpString.Replace("{@#ActListCon}" + PageCode + "{/#ActListCon}", "");
                    RichText += TmpString + "\r\n";
                }
                PageCode = PageCode.Replace("(@previouspage)", GetActionHref(CurrentHtmlAction));
                PageCode = PageCode.Replace("(@CurrentPage)", "1");
                PageCode = PageCode.Replace("(@MaxPage)", PageCount.ToString());
          
                
                for (int i = 0; i < PageCount; i++)
                {
                    if (i > 0)
                    {
                        NextPage(i + 1, LockerSignCls,false);
                    }
                }
                if (AllPageCode.Count > 0)
                {
                    PageCode = PageCode.Replace("(@nextpage)", AllPageCode[0].WebPath);
                }
                else
                {
                    PageCode = PageCode.Replace("(@nextpage)", "");
                }
              
                RichText += PageCode + "\r\n";
                CurrentCode.AddRange(DataHelper.StrToList("<!--@StartListCode-->\r\n" + RichText + "<!--@EndListCode-->\r\n"));
            }

          
        }
        public static string GetActionHref(string Action)
        {
            if (Action.Replace(" ", "").StartsWith("http://"))
            {
                return Action;
            }
            else
            if (Action.Replace(" ", "").StartsWith("https://"))
            {
                return Action;
            }
            else
            {
                foreach (var GetItem in HtmlCreatEngine.AllAction)
                {
                    if (GetItem.Action.ToLower() == Action.ToLower())
                    {
                        return GetItem.Href;
                    }
                }
                return Action;
            }
        }
        public static void ProcessingSign(SignCls LockerSignCls)
        {
            switch (LockerSignCls.SignName)
            {
                case "InputValue"://处理DeFine标签
                    GetInputValue(LockerSignCls);
                    break;
                case "ActionPath"://处理文章位置标签
                    GetActionPath(LockerSignCls);
                    break;
                case "Obj"://处理万能标签
                    GetObjSign(LockerSignCls);
                    break;
                case "Nav"://处理导航标签
                    GetNavSign(LockerSignCls);
                    break;
                case "QuickGet"://处理快速查询标签
                    GetQuickGetSign(LockerSignCls);
                    break;
                case "ListT"://处理自定义泛型
                    GetListTSign(LockerSignCls);
                    break;
                case "ListTT"://处理自定义泛型,泛型
                    GetListTTSign(LockerSignCls);
                    break;
                case "Act":
                    GetActSign(LockerSignCls);//预编译文章标签
                    break;
                case "ActList":
                    GetActListSign(LockerSignCls);//预编译文章列表标签
                    break;
            }
        }


        public static void GetStartNav(string StartNavInfoLine, string StartNavCodeLine, string ReplaceCode, string SignType = "StartNav")
        {
            string NReplaceCode = ReplaceCode;
            string NStartNavCodeLine = StartNavCodeLine;
            SQLSetting GetSetting = GetSQLConfig(StartNavInfoLine, ref NReplaceCode, SignType);

            NStartNavCodeLine = GetSignData(GetSetting, NStartNavCodeLine);

            HtmlCreatEngine.WaitByHtml.Add(new ReplaceWait(NReplaceCode, NStartNavCodeLine));
        }

        public static void GetStartList(string StartListInfoLine, string StartListCodeLine, string ReplaceCode, string ActionName = "", string SignType = "StartList")
        {
            string NReplaceCode = ReplaceCode;
            string NStartListCodeLine = StartListCodeLine;
            SQLSetting GetSetting = GetSQLConfig(StartListInfoLine, ref NReplaceCode, SignType);
            var get = GetSignData(GetSetting, NStartListCodeLine, "", NReplaceCode, ActionName);
            HtmlCreatEngine.WaitAllActList.Add(get);
        }
        public static SQLSetting GetSQLConfig(string StartNavInfoLine, ref string ReplaceCode, string SignType)
        {
            SQLSetting NSqlSetting = new SQLSetting();
            ReplaceCode = ReplaceCode.Substring(0, ReplaceCode.Length - 1);
            List<string> allcommand = new List<string>();
            allcommand = StartNavInfoLine.Split('&').ToList();
            foreach (var get in allcommand)
            {
                string Nget = "";

                if (get.ToLower().StartsWith("lockertable:"))
                {
                    Nget = get.Substring("lockertable:".Length);

                    if (Nget.Contains(";"))
                    {
                        foreach (var lv2get in Nget.Split(';'))
                        {
                            if (lv2get == "" == false)
                            {
                                NSqlSetting.LockerTable.Add(lv2get);
                            }
                        }
                    }
                    else
                    {
                        NSqlSetting.LockerTable.Add(Nget.Trim());
                    }

                }

                if (get.ToLower().StartsWith("params:"))
                {
                    Nget = get.Substring("params:".Length);
                    if (Nget.Contains(";"))
                    {
                        foreach (var lv2get in Nget.Split(';'))
                        {
                            if (lv2get == "" == false)
                            {
                                NSqlSetting.GetParams.Add(lv2get);
                            }
                        }
                    }
                }

                if (get.ToLower().StartsWith("lockercol:"))
                {
                    Nget = get.Substring("lockercol:".Length);
                    if (Nget.Contains(";"))
                    {
                        foreach (var lv2get in Nget.Split(';'))
                        {
                            if (lv2get.Contains("="))
                            {
                                FindKey Nkey = new FindKey();
                                Nkey.Key = lv2get.Split('=')[0].Trim();
                                Nkey.Value = lv2get.Split('=')[1].Trim();
                                NSqlSetting.Condition.Add(Nkey);
                            }
                        }
                    }
                }

                if (get.ToLower().StartsWith("maxrow="))
                {
                    Nget = get.Substring("maxrow=".Length);
                    if (Nget.Replace(";", "").Trim() == "auto" == false)
                    {
                        NSqlSetting.MaxRow = int.Parse(Nget.Replace(";", "").Trim());
                    }
                    else
                    {
                        NSqlSetting.MaxRow = 0;
                    }

                }
                if (get.ToLower().StartsWith("minrow="))
                {
                    Nget = get.Substring("minrow=".Length);
                    NSqlSetting.MinRow = int.Parse(Nget.Replace(";", "").Trim());
                }
                if (get.ToLower().StartsWith("startrow="))
                {
                    Nget = get.Substring("startrow=".Length);
                    NSqlSetting.StartRow = int.Parse(Nget.Replace(";", "").Trim());
                }
                if (get.ToLower().StartsWith("oderby:"))
                {
                    Nget = get.Substring("oderby:".Length);
                    NSqlSetting.OderBy = Nget.Replace(";", "");
                }
                if (get.ToLower().StartsWith("page="))
                {
                    Nget = get.Substring("page=".Length);
                    NSqlSetting.Page = Nget.Replace(";", "").Trim();
                }
                if (get.ToLower().StartsWith("pagelength="))
                {
                    Nget = get.Substring("pagelength=".Length);
                    NSqlSetting.PageLength = int.Parse(Nget.Replace(";", "").Trim());
                }

            }
            NSqlSetting.SignType = SignType;
            NSqlSetting.FindTime = DateTime.Now;

            string NReplaceCode = ReplaceCode.Substring(0, ReplaceCode.Length - 1);

            return NSqlSetting;
        }
    }
    public class WaitAct
    {
        public string WritePath = "";
        public CreatOver NCreatOver = new CreatOver();
        public DataRow Config = null;
        public string StyleAction = "";

        public WaitAct(string WritePath, CreatOver NCreatOver, DataRow Config, string StyleAction)
        {
            this.WritePath = WritePath;
            this.NCreatOver = NCreatOver;
            this.Config = Config;
            this.StyleAction = StyleAction;
        }
    }
    public class ActionList
    {
        public string ActionPath { get; set; }
        public string ActionName { get; set; }
        public int FromID { get; set; }
    
    }

    public class SignCls
    {
        public string StartCode = "";

        public List<SettingItem> AllSetting = new List<SettingItem>();

        public string SignName = "";

        public List<string> SignCode = new List<string>();

        public string EndCode = "";
    }
    public class StartPage
    {
        public int Page = 0;
        public string PageAction = "";
        public SignCls LockerSignCls = new SignCls();
        public string SavePath = "";
        public string WebPath = "";
        public string PageName = "";
    }
    public class CreatOver
    {
        public DateTime CreatTime = DateTime.Now;
        public string FilePath = "";
        public string WebPath = "";
    }
    public class SettingItem
    {
        public string Key = "";
        public List<string> AllValue = new List<string>();
    }
    public struct FindKey
    {
        public string Key { get; set; }
        public string Value { get; set; }
    }
    public class SQLSetting
    {
        public List<string> LockerTable = new List<string>();//要查询的表名
        public List<string> GetParams = new List<string>();//要查询的所有列
        public List<FindKey> Condition = new List<FindKey>();//查询时的附加条件
        public int MaxRow = 0;//枚举的最大行数
        public int MinRow = 0;//最小的行数->弃用
        public int StartRow = 0;//跳过的行数
        public string OderBy = "";//排序的方式
        public string SignType = "";//标识类型
        public string Page = "auto";//分页方式
        public int PageLength = 0;//一页有多少个数据
        public DateTime FindTime = new DateTime();//查询的时间
    }
}