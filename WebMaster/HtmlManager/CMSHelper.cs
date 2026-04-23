using Renci.SshNet.Messages;
using Renci.SshNet.Security;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Dynamic;
using System.IO;
using System.Linq;
using System.Reflection.Emit;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows.Shapes;
using WebMaster.DataManager;

namespace WebMaster.HtmlManager
{
    public class CMSHelper
    {
        /// <summary>
        /// 创建1级导航
        /// </summary>
        /// <param name="ColumnName"></param>
        /// <param name="ColumnHref"></param>
        /// <param name="ColumnOrder"></param>
        /// <param name="LockerTemplate"></param>
        /// <param name="ColumnType"></param>
        /// <returns></returns>
        public static bool CreatNewNav(string ColumnName, string ColumnType = "DefNav", string ColumnHref = "", string ColumnOrder = "", string LockerTemplate = "Auto", string Parent = "")
        {
            string SqlOder = "INSERT INTO nav(ColumnName,ColumnHref,ColumnOrder,LockerTemplate,ColumnType,Parent) VALUES ('{0}', '{1}', '{2}', '{3}','{4}','{5}');";
            int state = SqlServerHelper.ExecuteNonQuery(string.Format(SqlOder, ColumnName, ColumnHref, ColumnOrder, LockerTemplate, ColumnType, Parent));
            if (state == 0 == false) return true;
            return false;
        }

        /// <summary>
        /// 删除1级导航
        /// </summary>
        /// <param name="ID"></param>
        /// <returns></returns>
        public static bool DelectNavByID(int ID)
        {
            string SqlOder = "Delete From nav Where ID = " + ID;
            int state = SqlServerHelper.ExecuteNonQuery(SqlOder);
            if (state == 0 == false) return true;
            return false;
        }

        /// <summary>
        /// 获取指定类型的导航
        /// </summary>
        /// <param name="ColumnType"></param>
        /// <returns></returns>
        public static DataTable GetNavByColumnType(string ColumnType)
        {
            DataTable NDataTable = SqlServerHelper.ExecuteDataTable("Select * From nav Where ColumnType ='" + ColumnType + "'");
            return NDataTable;
        }
        public static bool DelectListTFormDB(int ID, int LockerRows)
        {
            List<LineT> GetList = ConvertToListT(SqlServerHelper.ExecuteDataTable("Select * From nav Where ID =" + ID.ToString() + " And ColumnType ='" + "ListT" + "'"));

            LineT SelectLineT = null;
            if (GetList.Count > 0) SelectLineT = GetList[0];
            if (SelectLineT == null == false)
            {
                SelectLineT.AllValue.RemoveAt(LockerRows);
                SelectLineT.AllHref.RemoveAt(LockerRows);
                nav GetNav = ClassLineToNav(SelectLineT);
                if (GetNav == null == false)
                {
                    string SqlOder = "UPDATE nav SET ColumnName = '{0}', ColumnHref = '{1}',ColumnOrder = '{2}',LockerTemplate = '{3}',ColumnType = '{4}',Parent = '{5}' Where ID = " + ID.ToString() + " And ColumnType ='" + "ListT" + "'";
                    int state = SqlServerHelper.ExecuteNonQuery(string.Format(SqlOder, GetNav.ColumnName, GetNav.ColumnHref, GetNav.ColumnOrder, GetNav.LockerTemplate, GetNav.ColumnType, GetNav.Parent));
                }
                return true;
            }

            return false;
        }
        public static bool DelectListTTFormDB(int ID, int LockerRows)
        {
            List<LineTT> GetList = ConvertToListTT(SqlServerHelper.ExecuteDataTable("Select * From nav Where ID =" + ID.ToString() + " And ColumnType ='" + "ListTT" + "'"));

            LineTT SelectLineTT = null;
            if (GetList.Count > 0) SelectLineTT = GetList[0];
            if (SelectLineTT == null == false)
            {
                SelectLineTT.AllValue.RemoveAt(LockerRows);
                SelectLineTT.AllHref.RemoveAt(LockerRows);
                nav GetNav = ClassLineToNav(SelectLineTT);
                if (GetNav == null == false)
                {
                    string SqlOder = "UPDATE nav SET ColumnName = '{0}', ColumnHref = '{1}',ColumnOrder = '{2}',LockerTemplate = '{3}',ColumnType = '{4}',Parent = '{5}' Where ID = " + ID.ToString() + " And ColumnType ='" + "ListTT" + "'";
                    int state = SqlServerHelper.ExecuteNonQuery(string.Format(SqlOder, GetNav.ColumnName, GetNav.ColumnHref, GetNav.ColumnOrder, GetNav.LockerTemplate, GetNav.ColumnType, GetNav.Parent));
                }
                return true;
            }

            return false;
        }
        public static bool ReloadListT(int ID, int LockerRows, string NewText)
        {
            NewText = NewText.Replace("&gt;", ">").Replace(" ", "");
            if (NewText.Length > 0)
            {
                List<LineT> GetList = ConvertToListT(SqlServerHelper.ExecuteDataTable("Select * From nav Where ID =" + ID.ToString() + " And ColumnType ='" + "ListT" + "'"));
                if (GetList.Count > 0)
                {
                    if (NewText.Contains(">"))
                    {
                        GetList[0].AllValue[LockerRows] = NewText.Split('>')[0].Replace(";", "");
                        GetList[0].AllHref[LockerRows] = NewText.Split('>')[1].Replace(";", "");
                    }

                    if (GetList == null == false)
                    {
                        nav GetNav = ClassLineToNav(GetList[0]);
                        if (GetNav == null == false)
                        {
                            string SqlOder = "UPDATE nav SET ColumnName = '{0}', ColumnHref = '{1}' Where ID = " + ID.ToString() + " And ColumnType ='" + "ListT" + "'";
                            int state = SqlServerHelper.ExecuteNonQuery(string.Format(SqlOder, GetNav.ColumnName, GetNav.ColumnHref));
                        }
                        return true;
                    }
                }
            }
            return false;
        }
        public static bool ReloadListTT(int ID, int LockerRows, string NewText)
        {
            NewText = NewText.Replace("&gt;", ">").Replace(" ", "");
            if (NewText.Length > 0)
            {
                List<LineTT> GetList = ConvertToListTT(SqlServerHelper.ExecuteDataTable("Select * From nav Where ID =" + ID.ToString() + " And ColumnType ='" + "ListTT" + "'"));
                if (GetList.Count > 0)
                {
                    if (NewText.Contains(">"))
                    {

                        GetList[0].AllValue[LockerRows].Key = NewText.Split('>')[0].Replace(";", "");
                        GetList[0].AllValue[LockerRows].Value = NewText.Split('>')[1].Replace(";", "");
                        try
                        {
                            GetList[0].AllHref[LockerRows] = NewText.Split('>')[2];
                        }
                        catch { }
                    }

                    if (GetList == null == false)
                    {
                        nav GetNav = ClassLineToNav(GetList[0]);
                        if (GetNav == null == false)
                        {
                            string SqlOder = "UPDATE nav SET ColumnName = '{0}', ColumnHref = '{1}'  Where ID = " + ID.ToString() + " And ColumnType ='" + "ListTT" + "'";
                            int state = SqlServerHelper.ExecuteNonQuery(string.Format(SqlOder, GetNav.ColumnName, GetNav.ColumnHref));
                        }
                        return true;
                    }
                }
            }
            return false;
        }
        public static bool ReloadT(int ID, string Parent, string SeoText)
        {
            string SqlOder = "UPDATE nav SET  Parent = '{0}',SeoText = '{1}' Where ID = " + ID.ToString() + " And ColumnType ='" + "ListT" + "'";
            int state = SqlServerHelper.ExecuteNonQuery(string.Format(SqlOder, Parent, SeoText));
            if (state == 0 == false)
            {
                return true;
            }
            return false;
        }

        public static bool ReloadTT(int ID, string Parent, string SeoText)
        {
            string SqlOder = "UPDATE nav SET  Parent = '{0}',SeoText = '{1}' Where ID = " + ID.ToString() + " And ColumnType ='" + "ListTT" + "'";
            int state = SqlServerHelper.ExecuteNonQuery(string.Format(SqlOder, Parent, SeoText));
            if (state == 0 == false)
            {
                return true;
            }
            return false;
        }
        public static bool AddListT(int ID)
        {
            List<LineT> GetList = ConvertToListT(SqlServerHelper.ExecuteDataTable("Select * From nav Where ID =" + ID.ToString() + " And ColumnType ='" + "ListT" + "'"));
            if (GetList.Count > 0)
            {
                GetList[0].AllValue.Add("Value");
                GetList[0].AllHref.Add("Src");
                if (GetList == null == false)
                {
                    nav GetNav = ClassLineToNav(GetList[0]);
                    if (GetNav == null == false)
                    {
                        string SqlOder = "UPDATE nav SET ColumnName = '{0}', ColumnHref = '{1}',ColumnOrder = '{2}',LockerTemplate = '{3}',ColumnType = '{4}',Parent = '{5}' Where ID = " + ID.ToString() + " And ColumnType ='" + "ListT" + "'";
                        int state = SqlServerHelper.ExecuteNonQuery(string.Format(SqlOder, GetNav.ColumnName, GetNav.ColumnHref, GetNav.ColumnOrder, GetNav.LockerTemplate, GetNav.ColumnType, GetNav.Parent));
                    }
                    return true;
                }
            }
            return false;
        }
        public static bool AddListTT(int ID)
        {
            List<LineTT> GetList = ConvertToListTT(SqlServerHelper.ExecuteDataTable("Select * From nav Where ID =" + ID.ToString() + " And ColumnType ='" + "ListTT" + "'"));
            if (GetList.Count > 0)
            {
                KeyValue NKeyValue = new KeyValue();
                NKeyValue.Key = "Key";
                NKeyValue.Value = "Value";
                GetList[0].AllValue.Add(NKeyValue);
                GetList[0].AllHref.Add("Src");
                if (GetList == null == false)
                {
                    nav GetNav = ClassLineToNav(GetList[0]);
                    if (GetNav == null == false)
                    {
                        string SqlOder = "UPDATE nav SET ColumnName = '{0}', ColumnHref = '{1}',ColumnOrder = '{2}',LockerTemplate = '{3}',ColumnType = '{4}',Parent = '{5}' Where ID = " + ID.ToString() + " And ColumnType ='" + "ListTT" + "'";
                        int state = SqlServerHelper.ExecuteNonQuery(string.Format(SqlOder, GetNav.ColumnName, GetNav.ColumnHref, GetNav.ColumnOrder, GetNav.LockerTemplate, GetNav.ColumnType, GetNav.Parent));
                    }
                    return true;
                }
            }
            return false;
        }

        /// <summary>
        /// 解析ListT结构体
        /// </summary>
        /// <param name="Message"></param>
        /// <returns></returns>
        public static List<LineT> ConvertToListT(DataTable Message)
        {
            List<LineT> NListT = new List<LineT>();
            if (Message.Rows.Count > 0)
            {
                for (int i = 0; i < Message.Rows.Count; i++)
                {
                    int ID = Convert.ToInt32(DataHelper.ObjToStr(Message.Rows[i]["ID"]));
                    string ColumnName = DataHelper.ObjToStr(Message.Rows[i]["ColumnName"]);
                    string ColumnHref = DataHelper.ObjToStr(Message.Rows[i]["ColumnHref"]);
                    string SeoText = DataHelper.ObjToStr(Message.Rows[i]["SeoText"]);
                    string ColumnOrder = DataHelper.ObjToStr(Message.Rows[i]["ColumnOrder"]);
                    string LockerTemplate = DataHelper.ObjToStr(Message.Rows[i]["LockerTemplate"]);
                    string ColumnType = DataHelper.ObjToStr(Message.Rows[i]["ColumnType"]);
                    string Parent = DataHelper.ObjToStr(Message.Rows[i]["Parent"]);

                    LineT NLineT = new LineT();

                    List<string> AllValue = new List<string>();
                    List<string> AllHref = new List<string>();


                    int AllRows = 0;

                    foreach (var GetValue in ColumnName.Split(';'))
                    {
                        if (GetValue.Trim().Length > 0)
                        {
                            AllRows++;
                            AllValue.Add(GetValue);
                        }
                    }
                    foreach (var GetHref in ColumnHref.Split(';'))
                    {
                        if (GetHref.Trim().Length > 0)
                        {
                            AllRows--;
                            AllHref.Add(GetHref);
                        }
                    }
                    if (AllRows == 0)
                    {
                        NLineT.ID = ID;
                        NLineT.AllValue = AllValue;
                        NLineT.AllHref = AllHref;
                        NLineT.SeoText = SeoText;
                        NLineT.DefCode = ColumnName;
                        NLineT.ColumnOrder = ColumnOrder;
                        NLineT.LockerTemplate = LockerTemplate;
                        NLineT.ColumnType = ColumnType;
                        NLineT.Parent = Parent;
                        NListT.Add(NLineT);
                    }


                }

                return NListT;
            }

            return new List<LineT>();
        }

        /// <summary>
        /// 解析ListTT结构体
        /// </summary>
        /// <param name="Message"></param>
        /// <returns></returns>
        public static List<LineTT> ConvertToListTT(DataTable Message)
        {
            List<LineTT> NListTT = new List<LineTT>();
            if (Message.Rows.Count > 0)
            {
                for (int i = 0; i < Message.Rows.Count; i++)
                {
                    int ID = Convert.ToInt32(DataHelper.ObjToStr(Message.Rows[i]["ID"]));
                    string ColumnName = DataHelper.ObjToStr(Message.Rows[i]["ColumnName"]);
                    string ColumnHref = DataHelper.ObjToStr(Message.Rows[i]["ColumnHref"]);
                    string SeoText = DataHelper.ObjToStr(Message.Rows[i]["SeoText"]);
                    string ColumnOrder = DataHelper.ObjToStr(Message.Rows[i]["ColumnOrder"]);
                    string LockerTemplate = DataHelper.ObjToStr(Message.Rows[i]["LockerTemplate"]);
                    string ColumnType = DataHelper.ObjToStr(Message.Rows[i]["ColumnType"]);
                    string Parent = DataHelper.ObjToStr(Message.Rows[i]["Parent"]);

                    LineTT NLineTT = new LineTT();

                    List<KeyValue> AllValue = new List<KeyValue>();
                    List<string> AllHref = new List<string>();


                    int AllRows = 0;

                    foreach (var GetValue in ColumnName.Split(';'))
                    {
                        if (GetValue.Trim().Length > 0)
                        {
                            if (GetValue.Contains(">"))
                            {
                                KeyValue NKeyValue = new KeyValue();
                                NKeyValue.Key = GetValue.Split('>')[0];
                                NKeyValue.Value = GetValue.Split('>')[1];
                                AllValue.Add(NKeyValue);

                            }
                            AllRows++;

                        }
                    }
                    foreach (var GetHref in ColumnHref.Split(';'))
                    {
                        if (GetHref.Trim().Length > 0)
                        {
                            AllRows--;
                            AllHref.Add(GetHref);
                        }
                    }
                    if (AllRows == 0)
                    {
                        NLineTT.ID = ID;
                        NLineTT.AllValue = AllValue;
                        NLineTT.AllHref = AllHref;
                        NLineTT.SeoText = SeoText;
                        NLineTT.DefCode = ColumnName;
                        NLineTT.ColumnOrder = ColumnOrder;
                        NLineTT.LockerTemplate = LockerTemplate;
                        NLineTT.ColumnType = ColumnType;
                        NLineTT.Parent = Parent;
                        NListTT.Add(NLineTT);
                    }


                }
                return NListTT;
            }

            return new List<LineTT>();
        }


        public static nav ClassLineToNav(object LockerItem)
        {
            if (LockerItem is LineT)
            {
                LineT GetItem = (LineT)LockerItem;

                int ID = GetItem.ID;
                string ColumnName = "";
                foreach (var Get in GetItem.AllValue)
                {
                    ColumnName += Get + ";";
                }

                string ColumnHref = "";
                foreach (var Get in GetItem.AllHref)
                {
                    ColumnHref += Get + ";";
                }
                string ColumnOrder = GetItem.ColumnOrder;
                string LockerTemplate = GetItem.LockerTemplate;
                string ColumnType = GetItem.ColumnType;
                string Parent = GetItem.Parent;
                string SeoText = GetItem.SeoText;
                return new nav(ID.ToString(), ColumnName, ColumnHref, SeoText, ColumnOrder, LockerTemplate, ColumnType, Parent);
            }
            else
            if (LockerItem is LineTT)
            {
                LineTT GetItem = (LineTT)LockerItem;

                int ID = GetItem.ID;
                string ColumnName = "";
                foreach (var Get in GetItem.AllValue)
                {
                    ColumnName += Get.Key + ">" + Get.Value + ";";
                }

                string ColumnHref = "";
                foreach (var Get in GetItem.AllHref)
                {
                    ColumnHref += Get + ";";
                }
                string ColumnOrder = GetItem.ColumnOrder;
                string LockerTemplate = GetItem.LockerTemplate;
                string ColumnType = GetItem.ColumnType;
                string Parent = GetItem.Parent;
                string SeoText = GetItem.SeoText;
                return new nav(ID.ToString(), ColumnName, ColumnHref, SeoText, ColumnOrder, LockerTemplate, ColumnType, Parent);
            }

            return null;
        }
        /// <summary>
        /// 修改指定导航的内容
        /// </summary>
        /// <param name="ID"></param>
        /// <param name="Item"></param>
        /// <returns></returns>
        public static bool ReloadNavByID(int ID, nav Item)
        {
            string SqlOder = "UPDATE nav SET ColumnName = '{0}', ColumnHref = '{1}',SeoText= '{2}', ColumnOrder = '{3}',LockerTemplate = '{4}',ColumnType = '{5}',Parent = '{6}' Where ID = " + ID.ToString();
            int state = SqlServerHelper.ExecuteNonQuery(string.Format(SqlOder, Item.ColumnName, Item.ColumnHref, Item.SeoText, Item.ColumnOrder, Item.LockerTemplate, Item.ColumnType, Item.Parent));
            if (state == 0 == false) return true;
            return false;
        }

        /// <summary>
        /// 获取DeFine的值
        /// </summary>
        /// <param name="FunctionName"></param>
        /// <param name="TableName"></param>
        /// <returns></returns>
        public static string GetDeFineValue(string FunctionName, string TableName = "CMSSetting")
        {
            object GetSet = SqlServerHelper.ExecuteScalar(string.Format("Select GetSet From define Where TableName ='{0}'", TableName));
            if (GetSet == null) return null;

            string[] GetCommand = GetSet.ToString().Split(';');
            foreach (var GetItem in GetCommand)
            {
                if (GetItem.Contains(">"))
                {
                    string Name = GetItem.Split('>')[0];
                    string Value = GetItem.Split('>')[1];
                    if (Name.Equals(FunctionName)) return Value;
                }
            }
            return null;
        }
        /// <summary>
        /// 设置DeFine的值
        /// </summary>
        /// <param name="FunctionName"></param>
        /// <param name="SetValue"></param>
        /// <param name="TableName"></param>
        /// <returns></returns>
        public static DeFineMessage SetDeFineValue(string FunctionName, string SetValue, string TableName = "CMSSetting")
        {
            string SqlOder = "UPDATE define SET GetSet ='{0}' Where TableName ='" + TableName + "'";
            object GetSet = SqlServerHelper.ExecuteScalar(string.Format("Select GetSet From define Where TableName ='{0}'", TableName));
            if (GetSet == null) return DeFineMessage.Null;
            bool Finder = false;
            List<string> GetCommand = new List<string>();
            GetCommand = GetSet.ToString().Split(';').ToList();

            for (int i = 0; i < GetCommand.Count; i++)
            {
                if (GetCommand[i].Contains(">"))
                {
                    string Name = GetCommand[i].Split('>')[0];
                    if (Name.Equals(FunctionName))
                    {
                        if (SetValue == null == false)
                        {
                            GetCommand[i] = FunctionName + ">" + SetValue;
                            Finder = true;
                            break;
                        }
                        else
                        {
                            GetCommand[i] = "";
                            SqlServerHelper.ExecuteNonQuery(string.Format(SqlOder, DeFineCommandToString(GetCommand.ToArray())));
                            return DeFineMessage.Delect;
                        }

                    }
                }
            }

            if (Finder)
            {
                SqlServerHelper.ExecuteNonQuery(string.Format(SqlOder, DeFineCommandToString(GetCommand.ToArray())));
                return DeFineMessage.Reload;
            }
            else
            {
                GetCommand.Add(FunctionName + ">" + SetValue);
                SqlServerHelper.ExecuteNonQuery(string.Format(SqlOder, DeFineCommandToString(GetCommand.ToArray())));
                return DeFineMessage.New;
            }
        }

        /// <summary>
        /// 遍历所有模板文件
        /// </summary>
        /// <param name="TmpLeve"></param>
        public static List<HtmlTemplate> GetAllTmp(string TmpLeve = "user", bool ShowCode = false)
        {
            List<HtmlTemplate> AllHtmlTemplate = new List<HtmlTemplate>();
            foreach (var Get in HtmlCreatEngine.QueueStaticHtmlTmp)
            {
                if (Get.TemplateType.ToLower().Equals(TmpLeve.ToLower()))
                {
                    HtmlTemplate NHtmlTemplate = new HtmlTemplate();
                    NHtmlTemplate.HtmlAction = Get.HtmlAction;
                    NHtmlTemplate.HtmlFileName = Get.HtmlFileName;
                    NHtmlTemplate.HtmlPath = Get.HtmlPath;
                    NHtmlTemplate.HtmlShowType = Get.HtmlShowType.ToString();
                    NHtmlTemplate.StylePath = Get.StylePath;
                    NHtmlTemplate.TemplateType = Get.TemplateType;
                    if (ShowCode)
                    {
                        foreach (var GetLine in Get.AllCode)
                        {
                            NHtmlTemplate.AllCode += GetLine.Trim() + "\r\n";
                        }
                    }
                    AllHtmlTemplate.Add(NHtmlTemplate);
                }

            }
            foreach (var Get in HtmlCreatEngine.AllWebPage)
            {
                if (Get.TemplateType.ToLower().Equals(TmpLeve.ToLower()))
                {
                    HtmlTemplate NHtmlTemplate = new HtmlTemplate();
                    NHtmlTemplate.HtmlAction = Get.HtmlAction;
                    NHtmlTemplate.HtmlFileName = Get.HtmlFileName;
                    NHtmlTemplate.HtmlPath = Get.HtmlPath;
                    NHtmlTemplate.HtmlShowType = Get.HtmlShowType.ToString();
                    NHtmlTemplate.StylePath = Get.StylePath;
                    NHtmlTemplate.TemplateType = Get.TemplateType;
                    if (ShowCode)
                    {
                        foreach (var GetLine in Get.AllCode)
                        {
                            NHtmlTemplate.AllCode += GetLine.Trim() + "\r\n";
                        }
                    }
                    AllHtmlTemplate.Add(NHtmlTemplate);
                }

            }
            return AllHtmlTemplate;
        }

        /// <summary>
        /// 返回所有可用的模板
        /// </summary>
        /// <returns></returns>

        public static List<HtmlTypeCls> GetAllTemplate()
        {
            List<HtmlTypeCls> AllTemplate = new List<HtmlTypeCls>();
            List<KeyValue> LockerTemplate = new List<KeyValue>();


            var GetFileList = DataHelper.getallfile(DeFine.TemplatesPath, "", new List<string> { ".htm" });

            foreach (var get in GetFileList)
            {
                HtmlTypeCls Nhtml = new HtmlTypeCls();

                Nhtml.HtmlFileName = get.FileName;
                if (get.FileName.Contains("_"))
                {
                    Nhtml.LockerType = get.FileName.Split('_')[0];
                    Nhtml.HtmlAction = get.FileName.Split('_')[1].Split('.')[0];
                    Nhtml.CallCount = "0";

                    Nhtml.HtmlPath = get.FilePath;
                    Nhtml.CreatPath = get.FilePath.Replace(".htm", ".html").Replace(DeFine.TemplatesPath, CMSHelper.GetDeFineValue("DEFURL")).Replace(@"\", "/");
                    bool LockerCode = false;
                    foreach (var Get in DataHelper.readfile(get.FilePath))
                    {
                        if (LockerCode)
                        {
                            if (Get.Contains("@#code+="))
                            {
                                foreach (var lv2 in GetFileList)
                                {
                                    if (Get.Contains(lv2.FileName.Split('_')[1].Split('.')[0]))
                                    {
                                        bool Frist = true;
                                        for (int ir = 0; ir < LockerTemplate.Count; ir++)
                                        {
                                            if (LockerTemplate[ir].Key == lv2.FileName.Split('_')[1].Split('.')[0])
                                            {
                                                Frist = false;
                                                int NextNumber = int.Parse(LockerTemplate[ir].Value) + 1;
                                                LockerTemplate[ir].Value = NextNumber.ToString();
                                            }
                                        }
                                        if (Frist) LockerTemplate.Add(new KeyValue(lv2.FileName.Split('_')[1].Split('.')[0], "1"));
                                    }
                                }
                            }

                            Nhtml.CodeText += Get + "\r\n";
                        }
                        else
                        {
                            Nhtml.HeadCode += Get;
                        }
                        if (Get.Contains("@html->"))
                        {
                            LockerCode = true;
                        }
                    }
                    AllTemplate.Add(Nhtml);
                }
            }

            for (int i = 0; i < AllTemplate.Count; i++)
            {
                foreach (var get in LockerTemplate)
                {
                    if (AllTemplate[i].HtmlAction == get.Key)
                    {
                        AllTemplate[i].CallCount = get.Value;
                    }
                }
            }
            return AllTemplate;
        }

        /// <summary>
        /// 返回图片集合以及调用详情
        /// </summary>
        /// <returns></returns>
        public static ImageList CheckHtmlFileByPic()
        {
            ImageList NImageList = new ImageList();
            foreach (var GetHtmlItem in GetAllTmp("user", true))
            {
                HtmlFileCall NHtmlFileCall = new HtmlFileCall();

                NHtmlFileCall.HtmlFileName = GetHtmlItem.HtmlPath.Substring(GetHtmlItem.HtmlPath.LastIndexOf(@"\") + 1);
                NHtmlFileCall.HtmlFilePath = GetHtmlItem.HtmlPath;

                NHtmlFileCall.PicPath.AddRange(GetHtmlImageUrlList(GetHtmlItem.AllCode).ToList());
                NImageList.LockerHtmlFile.Add(NHtmlFileCall);
            }

            List<string> AllImage = new List<string>();
            DirectoryAllFiles.FileList.Clear();
            List<FileInformation> list = DirectoryAllFiles.GetAllFiles(new System.IO.DirectoryInfo(AppDomain.CurrentDomain.BaseDirectory + @"ueditor\net\upload\image\"));

            foreach (var item in list)
            {
                AllImage.Add(item.FilePath);
            }
            foreach (var Get in AllImage)
            {
                bool IsStand = true;
                string picname = Get.Substring(Get.LastIndexOf(@"\") + 1);
                object ProductID = SqlServerHelper.ExecuteScalar("Select ProductName From product Where AllImageURL like '%" + ("/" + picname) + "%'");
                if (ProductID == null)
                {
                    foreach (var Lv2Get in NImageList.LockerHtmlFile)
                    {

                        foreach (var Lv3Get in Lv2Get.PicPath)
                        {
                            if (Lv3Get.Contains(("/" + picname)))
                            {
                                IsStand = false;
                            }
                        }
                    }
                    if (IsStand)
                    {
                        NImageList.StandPicPath.Add(Get);
                    }
                }
                else
                {
                    bool FristItem = true;
                    for (int i = 0; i < NImageList.LockerHtmlFile.Count; i++)
                    {
                        if (NImageList.LockerHtmlFile[i].HtmlFileName == ProductID.ToString())
                        {
                            if (NImageList.LockerHtmlFile[i].HtmlFilePath == "Form SQL")
                            {
                                FristItem = false;
                                if (Get.Contains(@"\ueditor\"))
                                {
                                    if (!NImageList.LockerHtmlFile[i].PicPath.Contains(Get.Substring(Get.IndexOf(@"\ueditor\")).Replace(@"\", "/")))
                                    {
                                        NImageList.LockerHtmlFile[i].PicPath.Add(Get);
                                    }
                                }
                                else
                                {
                                    NImageList.LockerHtmlFile[i].PicPath.Add(Get);
                                }
                            }
                            else
                            {
                                FristItem = false;

                                NImageList.LockerHtmlFile[i].PicPath.Add(Get);
                            }

                        }
                    }
                    if (FristItem)
                    {
                        HtmlFileCall NHtmlFileCall = new HtmlFileCall();
                        NHtmlFileCall.HtmlFilePath = "Form SQL";
                        NHtmlFileCall.HtmlFileName = ProductID.ToString();
                        string NewWebPath = Get;
                        if (NewWebPath.Contains(@"\ueditor\"))
                        {
                            NewWebPath = NewWebPath.Substring(NewWebPath.IndexOf(@"\ueditor\"));
                        }
                        NHtmlFileCall.PicPath.Add(NewWebPath.Replace(@"\", "/"));
                        NImageList.LockerHtmlFile.Add(NHtmlFileCall);
                    }
                }
            }
            return NImageList;
        }

        /// <summary>
        /// 删除所有没有用到的图片返回删除数量
        /// </summary>
        /// <returns></returns>
        public static int AutoDelectStandPic()
        {
            int DelectFiles = 0;
            ImageList NImageList = new ImageList();
            foreach (var GetHtmlItem in GetAllTmp("user", true))
            {
                HtmlFileCall NHtmlFileCall = new HtmlFileCall();

                NHtmlFileCall.HtmlFileName = GetHtmlItem.HtmlPath.Substring(GetHtmlItem.HtmlPath.LastIndexOf(@"\") + 1);
                NHtmlFileCall.HtmlFilePath = GetHtmlItem.HtmlPath;

                NHtmlFileCall.PicPath.AddRange(GetHtmlImageUrlList(GetHtmlItem.AllCode).ToList());
                NImageList.LockerHtmlFile.Add(NHtmlFileCall);
            }

            List<string> AllImage = new List<string>();
            List<FileInformation> list = DirectoryAllFiles.GetAllFiles(new System.IO.DirectoryInfo(AppDomain.CurrentDomain.BaseDirectory + @"ueditor\net\upload\image\"));

            foreach (var item in list)
            {
                AllImage.Add(item.FilePath);
            }
            foreach (var Get in AllImage)
            {
                bool IsStand = true;
                string picname = Get.Substring(Get.LastIndexOf(@"\") + 1);
                object ProductID = SqlServerHelper.ExecuteScalar("Select ProductName From product Where AllImageURL like '%" + ("/" + picname) + "%'");
                if (ProductID == null)
                {
                    foreach (var Lv2Get in NImageList.LockerHtmlFile)
                    {

                        foreach (var Lv3Get in Lv2Get.PicPath)
                        {
                            if (Lv3Get.Contains(("/" + picname)))
                            {
                                IsStand = false;
                            }
                        }
                    }
                    if (IsStand)
                    {
                        NImageList.StandPicPath.Add(Get);
                    }
                }
                else
                {
                    bool FristItem = true;
                    for (int i = 0; i < NImageList.LockerHtmlFile.Count; i++)
                    {
                        if (NImageList.LockerHtmlFile[i].HtmlFileName == ProductID.ToString())
                        {
                            FristItem = false;
                            NImageList.LockerHtmlFile[i].PicPath.Add(Get);
                        }
                    }
                    if (FristItem)
                    {
                        HtmlFileCall NHtmlFileCall = new HtmlFileCall();
                        NHtmlFileCall.HtmlFilePath = "Form SQL";
                        NHtmlFileCall.HtmlFileName = ProductID.ToString();
                        NHtmlFileCall.PicPath.Add(Get);
                        NImageList.LockerHtmlFile.Add(NHtmlFileCall);
                    }
                }
            }
            foreach (var DelectItem in NImageList.StandPicPath)
            {
                if (DelectItem.Contains(@"ueditor\net\upload\image\"))
                {
                    if (File.Exists(DelectItem))
                    {
                        File.Delete(DelectItem);
                        DelectFiles++;
                    }
                }
            }
            return DelectFiles;
        }
        public static string DeFineCommandToString(string[] Command)
        {
            string RichText = "";
            foreach (var Item in Command)
            {
                if (Item.Trim().Length > 0)
                {
                    RichText += Item + ";";
                }
            }
            return RichText;
        }
        public static string[] GetHtmlImageUrlList(string sHtmlText)
        {

            Regex regImg = new Regex(@"<img\b[^<>]*?\bsrc[\s\t\r\n]*=[\s\t\r\n]*[""']?[\s\t\r\n]*(?<imgUrl>[^\s\t\r\n""'<>]*)[^<>]*?/?[\s\t\r\n]*>", RegexOptions.IgnoreCase);
            MatchCollection matches = regImg.Matches(sHtmlText);
            int i = 0;
            string[] sUrlList = new string[matches.Count];
            foreach (Match match in matches)
                sUrlList[i++] = match.Groups["imgUrl"].Value;
            return sUrlList;
        }

        private static List<string> ImageType = new List<string>() { ".jpg", ".png", ".bmp", ".tif", ".gif", ".pcx", ".tga" };
        private static List<string> VideoType = new List<string>() { ".mp4", ".mp3", ".flv", ".avi", ".rmvb", ".mpg", ".mov", ".mkv" };
        private static List<string> FileType = new List<string>() { ".rar", ".zip", ".7z", ".cab", ".arj", ".lzh", ".tar", ".gz", ".ace", ".jar" };
        public static bool NewFile(string FileName, string LockerUserID)
        {
            if (FileName.Contains("_"))
            {
                string DefFileName = FileName.Split('_')[1];

                if (DefFileName.Contains("_"))
                {
                    DefFileName = DefFileName.Split('_')[0];
                }

                string ThisType = "";
                string Suffix = "";
                if (DefFileName.Contains("."))
                {
                    Suffix = "." + DefFileName.Split('.')[1];
                }
                if (ThisType == "")
                {
                    foreach (var get in ImageType)
                    {
                        if (get.ToLower().Equals(Suffix.ToLower()))
                        {
                            ThisType = "ImageType";
                            break;
                        }
                    }
                }
                if (ThisType == "")
                {
                    foreach (var get in VideoType)
                    {
                        if (get.ToLower().Equals(Suffix.ToLower()))
                        {
                            ThisType = "VideoType";
                            break;
                        }
                    }
                }
                if (ThisType == "")
                {
                    foreach (var get in FileType)
                    {
                        if (get.ToLower().Equals(Suffix.ToLower()))
                        {
                            ThisType = "FileType";
                            break;
                        }
                    }
                }
                if (ThisType == "") ThisType = "obj";
                string FileSize = BrainConfig.GetMB(new FileInfo(DeFine.UPLoadFile + FileName).Length) + "MB";
                string FileAction = TimeHelper.DateTimeToStamp(DateTime.Now) + new Random().Next(10, 99);
                object SelectDefFileName = SqlServerHelper.ExecuteScalar("Select DefFileName From FileList Where DefFileName='" + DefFileName + "'");
                if (SelectDefFileName == null)
                {
                    string SqlOder = "INSERT INTO FileList(FileName,FileType,FileSize,FileAuthority,UPLoadByUser,FileAction,AntiTheftChain,DefFileName) VALUES ('{0}','{1}','{2}','{3}','{4}','{5}','{6}','{7}');";
                    int state = SqlServerHelper.ExecuteNonQuery(string.Format(SqlOder, FileName, ThisType, FileSize, "ALL", LockerUserID, FileAction, false.ToString(), DefFileName));
                    if (state == 0 == false)
                    {
                        return true;
                    }
                    else
                    {
                        return false;
                    }
                }
                else
                {
                    return false;
                }
            }
            else
            {
                return false;
            }
        }

        public static FileList GetFileByAction(string Action)
        {
            DataTable NTabel = SqlServerHelper.ExecuteDataTable("Select * From FileList Where FileAction ='"+Action+"'");
            if (NTabel.Rows.Count > 0)
            {
                try
                {
                    FileList NFileList = new FileList();
                    NFileList.ID = int.Parse(DataHelper.ObjToStr(NTabel.Rows[0]["ID"]));
                    NFileList.FileName = DataHelper.ObjToStr(NTabel.Rows[0]["FileName"]);
                    NFileList.FileType = DataHelper.ObjToStr(NTabel.Rows[0]["FileType"]);
                    NFileList.FileSize = DataHelper.ObjToStr(NTabel.Rows[0]["FileSize"]);
                    NFileList.FileAuthority = DataHelper.ObjToStr(NTabel.Rows[0]["FileAuthority"]);
                    NFileList.UPLoadByUser = DataHelper.ObjToStr(NTabel.Rows[0]["UPLoadByUser"]);
                    NFileList.FileAction = DataHelper.ObjToStr(NTabel.Rows[0]["FileAction"]);
                    NFileList.AntiTheftChain = DataHelper.ObjToStr(NTabel.Rows[0]["AntiTheftChain"]);
                    NFileList.DefFileName= DataHelper.ObjToStr(NTabel.Rows[0]["DefFileName"]);
                    return NFileList;
                }
                catch
                {
                    return new FileList();
                }
            }
            return new FileList();
        }

        public static DataTable GetAllFileFromDB()
        {
            return SqlServerHelper.ExecuteDataTable("Select * From FileList");
        }
        public static bool KillFile(string ID)
        {
            int Number = 0;
            int.TryParse(ID,out Number);
            string SqlOder = "Select * From FileList Where ID=" + Number.ToString();
            DataTable NTable = SqlServerHelper.ExecuteDataTable(SqlOder);
            if (NTable.Rows.Count > 0)
            {
                string GetFileName = DataHelper.ObjToStr(NTable.Rows[0]["FileName"]);
                if (File.Exists(DeFine.UPLoadFile + GetFileName))
                {
                    File.Delete(DeFine.UPLoadFile + GetFileName);
                    int state = SqlServerHelper.ExecuteNonQuery("delete from FileList Where ID = " + Number.ToString());
                    if (state == 0 == false)
                    {
                        return true;
                    }
                    else
                    {
                        return false;
                    }
                }
            }
         
            return false;
        }
    }

    public class DirectoryAllFiles
    {
        public static List<FileInformation> FileList = new List<FileInformation>();
        public static List<FileInformation> GetAllFiles(DirectoryInfo dir)
        {
            FileInfo[] allFile = dir.GetFiles();
            foreach (FileInfo fi in allFile)
            {
                FileList.Add(new FileInformation { FileName = fi.Name, FilePath = fi.FullName });
            }
            DirectoryInfo[] allDir = dir.GetDirectories();
            foreach (DirectoryInfo d in allDir)
            {
                GetAllFiles(d);
            }
            return FileList;
        }
    }

    public class FileInformation
    {
        public string FileName { get; set; }
        public string FilePath { get; set; }
    }
    public class HtmlFileCall
    {
        public string HtmlFileName = "";
        public string HtmlFilePath = "";
        public List<string> PicPath = new List<string>();
    }

    public class ImageList
    {
        public List<HtmlFileCall> LockerHtmlFile = new List<HtmlFileCall>();
        public List<string> StandPicPath = new List<string>();
    }

    public class HtmlTemplate
    {
        public string HtmlFileName = "";
        public string HtmlAction = "";
        public string TemplateType = "";
        public string HtmlPath = "";
        public string HtmlShowType = "";
        public string StylePath = "";
        public string AllCode = "";
    }


    public enum DeFineMessage
    {
        Null = 0,
        New = 1,
        Reload = 2,
        Delect = 3,
        Error = 4
    }




    public class KeyValue
    {
        public string Key = "";
        public string Value = "";
        public KeyValue(string Key, string Value)
        {
            this.Key = Key;
            this.Value = Value;
        }

        public KeyValue()
        {
        }
    }
    public class LineT
    {
        public int ID = 0;
        public List<string> AllValue = new List<string>();
        public List<string> AllHref = new List<string>();
        public string SeoText = "";
        public string DefCode = "";
        public string ColumnOrder = "";
        public string LockerTemplate = "";
        public string ColumnType = "";
        public string Parent = "";
    }

    public class LineTT
    {
        public int ID = 0;
        public List<KeyValue> AllValue = new List<KeyValue>();
        public List<string> AllHref = new List<string>();
        public string SeoText = "";
        public string DefCode = "";
        public string ColumnOrder = "";
        public string LockerTemplate = "";
        public string ColumnType = "";
        public string Parent = "";
    }
    public class FileList
    {
        public int ID = 0;
        public string FileName = "";
        public string FileType = "";
        public string FileSize = "";
        public string FileAuthority = "";
        public string UPLoadByUser = "";
        public string FileAction = "";
        public string AntiTheftChain = "";
        public string DefFileName = "";
    }

    public class nav
    {
        public string ID = "";
        public string ColumnName = "";
        public string ColumnHref = "";
        public string SeoText = "";
        public string ColumnOrder = "";
        public string LockerTemplate = "";
        public string ColumnType = "DefNav";
        public string Parent = "";

        public nav()
        {
        }
        public nav(string ColumnName, string ColumnHref, string SeoText, string ColumnOrder, string LockerTemplate, string ColumnType, string Parent)
        {
            this.ColumnName = ColumnName;
            this.ColumnHref = ColumnHref;
            this.SeoText = SeoText;
            this.ColumnOrder = ColumnOrder;
            this.LockerTemplate = LockerTemplate;
            this.ColumnType = ColumnType;
            this.Parent = Parent;
        }

        public nav(string ID, string ColumnName, string ColumnHref, string SeoText, string ColumnOrder, string LockerTemplate, string ColumnType, string Parent)
        {
            this.ID = ID;
            this.ColumnName = ColumnName;
            this.ColumnHref = ColumnHref;
            this.SeoText = SeoText;
            this.ColumnOrder = ColumnOrder;
            this.LockerTemplate = LockerTemplate;
            this.ColumnType = ColumnType;
            this.Parent = Parent;
        }
    }
}
