using WebMaster.DataManager;
using WebMaster.LabelManager;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Web;

namespace WebMaster.SQLManager
{
    public class SqlServerCommand
    {
        public static int AutoCreatNewFunction(string TableName, string FunctionName, string Value="")
        {
            string sqloder = "SELECT GetSet FROM DeFine where TableName='{0}'";
            object GetOder = SqlServerHelper.ExecuteScalar(string.Format(sqloder, TableName));
            string OderTmp = "";
            string RichText = "";
            string LockerTableName = "";
            if (GetOder == null) return 0;
            OderTmp = GetOder.ToString();
            if (GetOder.ToString().Contains(FunctionName))
            {
                if (OderTmp.Contains(">"))
                {
                    if (OderTmp.Contains(";"))
                    {
                        if (Value == "" == false)
                        { 
                        foreach (var get in OderTmp.Split(';'))
                        {
                            if (get.Split('>')[0] == FunctionName == false)
                            {
                                RichText += get.Split('>')[0] + ">" + get.Split('>')[1] + ";";
                            }
                            else
                            {
                                RichText += get.Split('>')[0] + ">" + Value + ";";
                            }
                        }
                        return SqlServerHelper.ExecuteNonQuery("update define set GetSet='" + RichText + "'" + " where TableName ='" + TableName + "'");
                        }
                    }
                }
                return 0;
            }
            else
            {
                LockerTableName = TableName;
                return SqlServerHelper.ExecuteNonQuery("update define set GetSet='" + (OderTmp + FunctionName + ">" + Value + ";") + "'" + " where TableName ='" + LockerTableName + "'");
            }

        }

        public static ReturnCommandValues QuickFind(string Oder)
        {
            ReturnCommandValues NValues = new ReturnCommandValues();
            string function = "";
            string key = "";
            if (Oder.Contains(":"))
            {
                NValues.function = function = Oder.Split(':')[0];
                NValues.key = key = Oder.Split(':')[1];
            }

            string sqloder = "SELECT * FROM DeFine";
            DataTable alldata = SqlServerHelper.ExecuteDataTable(sqloder);
            if (alldata.Rows == null == false)
            {
                if (alldata.Rows.Count > 0)
                {
                    string TableName = "";
                    string TableValue = "";
                    for (int i = 0; i < alldata.Rows.Count; i++)
                    {
                        TableName = alldata.Rows[0]["TableName"].ToString();
                        TableValue = alldata.Rows[0]["GetSet"].ToString();
                        if (TableName == function)
                        {
                            if (TableValue.Contains(";"))
                            {
                                foreach (var get in TableValue.Split(';'))
                                {
                                    if (get.Contains(">"))
                                    {
                                        string type = get.Split('>')[0];
                                        string value = get.Split('>')[1];
                                        if (type.ToLower() == key.ToLower())
                                        {
                                            NValues.htmlcode = value;
                                            return NValues;
                                        }

                                    }
                                }

                            }
                        }
                    }

                }
            }
            NValues.htmlcode = "NotFindValues";
            return NValues;
        }

        public static ReturnCommandValues QuickFind(string AllCommand, string Key)
        {
            ReturnCommandValues NValues = new ReturnCommandValues();
            NValues.function = "NOFunction";
            NValues.key = Key;
            NValues.htmlcode = "";
            if (AllCommand.Contains(";"))
            {
                foreach (var get in AllCommand.Split(';'))
                {
                    if (get.Contains(">"))
                    {
                        string type = get.Split('>')[0];
                        string value = get.Split('>')[1];
                        if (type.ToLower() == Key.ToLower())
                        {
                            NValues.htmlcode = value;
                            return NValues;
                        }

                    }
                }

            }
            return NValues;
        }
        public static bool QuickSet(string Function, string key, string value)
        {
            string sqloder = "SELECT * FROM DeFine Where TableName='" + StringHelper.FormatSpecialSymbolsClear(Function) + "'";
            DataTable alldata = SqlServerHelper.ExecuteDataTable(sqloder);
            string TableValue = "";
            if (alldata.Rows == null == false)
            {
                if (alldata.Rows.Count > 0)
                {
                    TableValue = alldata.Rows[0]["GetSet"].ToString();
                    List<TypeKey> alltype = new List<TypeKey>();
                    foreach (var get in TableValue.Split(';'))
                    {
                        if (get.Contains(">"))
                        {
                            TypeKey onetype = new TypeKey();
                            onetype.key = get.Split('>')[0];
                            onetype.value = get.Split('>')[1];

                            alltype.Add(onetype);
                        }
                    }
                    bool IsNewFunction = true;
                    for (int i = 0; i < alltype.Count; i++)
                    {
                        if (alltype[i].key == key)
                        {
                            alltype[i].value = value;
                            IsNewFunction = false;
                        }
                    }
                    if (IsNewFunction)
                    {
                        TypeKey onetype = new TypeKey();
                        onetype.key = key;
                        onetype.value = value;
                        alltype.Add(onetype);
                    }
                    if (QuickSet(Function, alltype) >= 1)
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

        public static int QuickSet(string Function, List<TypeKey> alltype)
        {
            string autosql = "UPDATE DeFine SET GetSet='{0}' where TableName='{1}'";
            string CommandLine = "";
            foreach (var get in alltype)
            {
                CommandLine += get.key + ">" + get.value + ";";
            }
            return SqlServerHelper.ExecuteNonQuery(CommandType.Text, string.Format(autosql, CommandLine, StringHelper.FormatSpecialSymbolsClear(Function)));
        }

        public static string GetSignData(SQLSetting Setting, string LockerCode)
        {
            string ReturnMessage = "";
            if (Setting.LockerTable.Count > 0)
            {
                List<TypeKey> RtCommand = new List<TypeKey>();
                List<TypeKey> AllGetSetFind = new List<TypeKey>();
                List<string> ALLGetParams = new List<string>();
                bool istrue = false;
                foreach (var TabGet in Setting.LockerTable)
                {
                    istrue = true;

                    string SqlOder = "SELECT * FROM {0} ";

                    SqlOder = string.Format(SqlOder, TabGet);

                    if (Setting.Condition.Count > 0)
                    {
                        SqlOder += "Where ";
                        foreach (var ConGet in Setting.Condition)
                        {
                            if (!ConGet.Key.StartsWith("_"))
                            {
                                SqlOder += ConGet.Key + " = " + string.Format("'{0}'", ConGet.Value) + " AND ";
                            }
                            else
                            {
                                AllGetSetFind.Add(new TypeKey(ConGet.Key, ConGet.Value));
                            }
                        }
                        SqlOder = SqlOder.Substring(0, SqlOder.Length - " AND ".Length);
                    }

                    if (Setting.OderBy == "" == false)
                    {
                        SqlOder += " ORDER BY " + Setting.OderBy;
                    }

                    SqlOder += ";";
                    ALLGetParams = Setting.GetParams;


                    DataTable NTable = SqlServerHelper.ExecuteDataTable(SqlOder);
                    List<string> TNameLis = new List<string>();
                    List<object> TValueLis = new List<object>();
                    int RowSum = 0;
                    foreach (DataRow GetNTable in NTable.Rows)
                    {
                        if (Setting.MaxRow > 0)
                        {
                            if (RowSum >= Setting.MaxRow + Setting.StartRow) { break; }
                        }
                        if (RowSum >= Setting.StartRow)
                        {
                            TNameLis.Clear();
                            TValueLis.Clear();
                            DataColumnCollection TName = GetNTable.Table.Columns;
                            foreach (DataColumn GetTName in TName)
                            {
                                TNameLis.Add(GetTName.Caption);
                            }
                            TValueLis.AddRange(GetNTable.ItemArray.ToList());
                            RtCommand.Clear();
                            foreach (var GetParams in ALLGetParams)
                            {
                                for (int i = 0; i < TNameLis.Count; i++)
                                {
                                    string GetNTableName = TNameLis[i];
                                    string GetNTableValue = "";
                                    if (TValueLis[i] == null == false)
                                    {
                                        GetNTableValue = TValueLis[i].ToString();
                                    }
                                    if (GetNTableName == "GetSet")
                                    {
                                        int findgetset = 0;
                                        foreach (var OGetSet in AllGetSetFind)
                                        {

                                            if (QuickFind(GetNTableValue, OGetSet.key).htmlcode == OGetSet.value == false)
                                            {
                                                istrue = false;
                                            }
                                            else { findgetset++; }
                                        }
                                        if (findgetset == AllGetSetFind.Count == false)
                                        {
                                            istrue = false;
                                        }
                                    }
                                    if (GetNTableName == GetParams)
                                    {
                                        RtCommand.Add(new TypeKey(GetNTableName, GetNTableValue));
                                    }
                                }
                            }

                            if (!istrue)
                            {

                            }
                            else
                            {
                                if (RtCommand.Count <= 0) { istrue = false; }
                            }
                            if (istrue)
                            {
                                string NextTmpLine = LockerCode;
                                foreach (var GetCommand in RtCommand)
                                {
                                    NextTmpLine = NextTmpLine.Replace("(&" + GetCommand.key + ")", GetCommand.value);
                                }
                                ReturnMessage += NextTmpLine + "\r\n";
                            }
                        }
                        RowSum++;
                    }
                }
            }
            return ReturnMessage;
        }

        public static ActList GetSignData(SQLSetting Setting, string LockerCode, string TargetPath, string ReplaceCode, string ActionName)
        {
            bool instrlist = false;
            if (LockerCode.Contains("{#INSTRList}"))
            {
                instrlist = true;
            }
            ActList ReturnAct = new ActList();
            string ReturnMessage = "";
            List<TypeKey> RtCommand = new List<TypeKey>();
            List<string> ALLGetParams = new List<string>();
            bool istrue = false;
            string sqloder = "select * from ( select * , ROW_NUMBER() OVER(Order by {0}) AS RowId from {1} {2}) as newList where RowId between {3} and {4};";
            string WHEREINSTR = "";
            List<TypeKey> AllGetSetFind = new List<TypeKey>();
            if (Setting.Condition.Count > 0)
            {
                WHEREINSTR += "Where ";
                foreach (var ConGet in Setting.Condition)
                {
                    if (!ConGet.Key.StartsWith("_"))
                    {
                        WHEREINSTR += ConGet.Key + " = " + ConGet.Value + " AND ";
                    }
                    else
                    {
                        AllGetSetFind.Add(new TypeKey(ConGet.Key, ConGet.Value));
                    }
                }
                WHEREINSTR = WHEREINSTR.Substring(0, WHEREINSTR.Length - " AND ".Length);
            }
            string NOderBy = Setting.OderBy.Trim();
            if (NOderBy == "")
            {
                NOderBy = "ProductID";
            }
            string Nsqloder = "";
            ALLGetParams = Setting.GetParams;
            int SettingPage = 0;
            if (Setting.Page == "auto")
            {
                object GeTtmp = SqlServerHelper.ExecuteScalar(CommandType.Text, "select count(*) from " + Setting.LockerTable[0]);
                if (GeTtmp == null == false)
                {
                    SettingPage = int.Parse(GeTtmp.ToString()) / Setting.PageLength;
                    double getpage = int.Parse(GeTtmp.ToString()) % Setting.PageLength;
                    if (getpage > 0)
                    {
                        SettingPage++;
                    }
                }

            }
            else
            {
                SettingPage = int.Parse(Setting.Page);
            }
            for (int pages = 0; pages < SettingPage;)
            {
                Nsqloder = sqloder;
                pages++;
                ReturnMessage = "";
                int NextPages = (pages - 1) * Setting.PageLength;
                if (NextPages == 0)
                {

                }
                else
                {
                    NextPages++;
                }
                Nsqloder = string.Format(Nsqloder, NOderBy, Setting.LockerTable[0], WHEREINSTR, NextPages, pages * Setting.PageLength);
                DataTable NTable = SqlServerHelper.ExecuteDataTable(Nsqloder);
                List<string> TNameLis = new List<string>();
                List<object> TValueLis = new List<object>();

                foreach (DataRow GetNTable in NTable.Rows)
                {
                    istrue = true;
                    TNameLis.Clear();
                    TValueLis.Clear();
                    DataColumnCollection TName = GetNTable.Table.Columns;
                    foreach (DataColumn GetTName in TName)
                    {
                        TNameLis.Add(GetTName.Caption);
                    }
                    TValueLis.AddRange(GetNTable.ItemArray.ToList());
                    RtCommand.Clear();
                    foreach (var GetParams in ALLGetParams)
                    {
                        for (int i = 0; i < TNameLis.Count; i++)
                        {
                            string GetNTableName = TNameLis[i];
                            string GetNTableValue = "";
                            if (TValueLis[i] == null == false)
                            {
                                GetNTableValue = TValueLis[i].ToString();
                            }
                            if (GetNTableName == "GetSet")
                            {
                                int findgetset = 0;
                                foreach (var OGetSet in AllGetSetFind)
                                {

                                    if (QuickFind(GetNTableValue, OGetSet.key).htmlcode == OGetSet.value == false)
                                    {
                                        istrue = false;
                                    }
                                    else { findgetset++; }
                                }
                                if (findgetset == AllGetSetFind.Count == false)
                                {
                                    istrue = false;
                                }

                            }
                            if (GetNTableName == GetParams)
                            {
                                RtCommand.Add(new TypeKey(GetNTableName, GetNTableValue));
                            }
                        }
                    }

                    if (!istrue)
                    {

                    }
                    else
                    {
                        if (RtCommand.Count <= 0) { istrue = false; }
                    }
                    if (istrue)
                    {
                        string NextTmpLine = LockerCode.Replace("{#INSTRList}", "");
                        foreach (var GetCommand in RtCommand)
                        {
                            NextTmpLine = NextTmpLine.Replace("(&" + GetCommand.key + ")", GetCommand.value);
                        }
                        ReturnMessage += NextTmpLine + "\r\n";
                    }
                }

                if (pages - 1 == 0 == false)
                {
                    if (instrlist)
                    {
                        ReturnMessage += "{#INSTRList}" + "\r\n";
                    }
                    RtKey NActFrist = new RtKey();
                    NActFrist.ActionName = ActionName;
                    NActFrist.LockerCode = ReturnMessage;
                    NActFrist.ReplaceCode = ReplaceCode;
                    NActFrist.TargetPath = ReturnFormatTargetPath(TargetPath, pages.ToString());
                    NActFrist.lockerid = pages;
                    ReturnAct.AllAct.Add(NActFrist);

                    ReturnMessage = "";
                }
                else
                {
                    if (instrlist)
                    {
                        ReturnMessage += "{#INSTRList}" + "\r\n";
                    }
                    ReturnAct.ActionName = ActionName;
                    ReturnAct.LockerCode = ReturnMessage;
                    ReturnAct.ReplaceCode = ReplaceCode;
                    ReturnAct.TargetPath = ReturnFormatTargetPath(TargetPath, pages.ToString());
                    ReturnAct.lockerid = pages;

                    ReturnMessage = "";
                }
            }
            return ReturnAct;
        }

        public static DataTable GetSignData(SQLSetting Setting,int SelectPage,ref int PageCount)
        {
            string sqloder = "select * from ( select * , ROW_NUMBER() OVER(Order by {0}) AS RowId from {1} {2}) as newList where RowId between {3} and {4};";
            string WHEREINSTR = "";
            List<TypeKey> AllGetSetFind = new List<TypeKey>();
            if (Setting.Condition.Count > 0)
            {
                WHEREINSTR += "Where ";
                foreach (var ConGet in Setting.Condition)
                {
                    if (!ConGet.Key.StartsWith("_"))
                    {
                        WHEREINSTR += ConGet.Key + " = " + ConGet.Value + " AND ";
                    }
                    else
                    {
                        AllGetSetFind.Add(new TypeKey(ConGet.Key, ConGet.Value));
                    }
                }
                WHEREINSTR = WHEREINSTR.Substring(0, WHEREINSTR.Length - " AND ".Length);
            }
            string NOderBy = Setting.OderBy.Trim();
            if (NOderBy == "")
            {
                NOderBy = "ProductID";
            }
            string Nsqloder = "";
            int SettingPage = 0;
            if (Setting.Page == "auto")
            {
                object GeTtmp = SqlServerHelper.ExecuteScalar(CommandType.Text, "select count(*) from " + Setting.LockerTable[0] + " "+ WHEREINSTR);
                if (GeTtmp == null == false)
                {
                    SettingPage = int.Parse(GeTtmp.ToString()) / Setting.PageLength;
                    double getpage = int.Parse(GeTtmp.ToString()) % Setting.PageLength;
                    if (getpage > 0)
                    {
                        SettingPage++;
                    }
                }

            }
            else
            {
                SettingPage = int.Parse(Setting.Page);
            }
                 Nsqloder = sqloder;
                int NextPages = (SelectPage - 1) * Setting.PageLength;
                if (NextPages == 0)
                {

                }
                else
                {
                    NextPages++;
                }
                Nsqloder = string.Format(Nsqloder, NOderBy, Setting.LockerTable[0], WHEREINSTR, NextPages, SelectPage * Setting.PageLength);
                DataTable NTable = SqlServerHelper.ExecuteDataTable(Nsqloder);
                PageCount = SettingPage;
            return NTable;
        }
        public static string ReturnFormatTargetPath(string TargetPath, string Pages)
        {
            string NTargetPath = TargetPath;
            if (NTargetPath.EndsWith(@"\")) { } else { NTargetPath += @"\"; }
            return NTargetPath + "Act_" + new Random().Next(100000, 999999) + "_" + new Random().Next(300000, 9999999) + "_" + Pages + ".html";
        }
        public struct ReturnCommandValues
        {
            public string htmlcode { get; set; }
            public string function { get; set; }
            public string key { get; set; }
        }

        public class TypeKey
        {
            public string key { get; set; }
            public string value { get; set; }

            public TypeKey()
            {

            }

            public TypeKey(string key, string value)
            {
                this.key = key;
                this.value = value;
            }
        }

        public class RtKey
        {
            public string TargetPath = "";

            public string LockerCode = "";

            public string ReplaceCode = "";

            public string ActionName = "";

            public int lockerid = 0;
        }

        public class ActList : RtKey
        {
            public List<RtKey> AllAct = new List<RtKey>();
        }
    }
}