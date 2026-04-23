using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using WebMaster.UserManager;

namespace WebMaster.DataManager
{
   public class PluginHelper
    {
        public static List<PlugNav> PlugNavList = new List<PlugNav>();
        public static string PutAllRequest(HttpContext context)
        {
            usertoken SelectAdminToken = new usertoken();
            usertoken SelectUserToken = new usertoken();
            TokenType NTokenType =DataProcessing.checktoken(context,ref SelectAdminToken,ref SelectUserToken);
            string main = "loginweb";
            List<pulginfo> getpulglis = selectactivepulg();
            string richtext = null;
            foreach (var getdq in getpulglis)
            {
                SqlCon Con = new SqlCon();
                Con.serveraddress = PIN.AESDecrypt(DeFine.DBIPAddress);
                Con.dbname = DeFine.DBName;
                Con.port = DeFine.DBPort;
                Con.username = PIN.AESDecrypt(DeFine.DBUSName);
                Con.password = PIN.AESDecrypt(DeFine.DBUSPassword);
                var dqcall =PlugCall(getdq.pulgpath, main, new object[] { context,NTokenType, SelectAdminToken, SelectUserToken, Con});
                if (dqcall.plugcode == null == false)
                {
                    richtext += dqcall.plugcode;
                }
            }
            return richtext;
        }
        public static string PutAllUI(string UI,string UICode,HttpContext context)
        {
            usertoken SelectAdminToken = new usertoken();
            usertoken SelectUserToken = new usertoken();
            TokenType NTokenType = DataProcessing.checktoken(context, ref SelectAdminToken, ref SelectUserToken);
            string main = "ShowWebPage";
            List<pulginfo> getpulglis = selectactivepulg();
            string richtext = null;
            foreach (var getdq in getpulglis)
            {
                SqlCon Con = new SqlCon();
                Con.serveraddress = PIN.AESDecrypt(DeFine.DBIPAddress);
                Con.dbname = DeFine.DBName;
                Con.port = DeFine.DBPort;
                Con.username = PIN.AESDecrypt(DeFine.DBUSName);
                Con.password = PIN.AESDecrypt(DeFine.DBUSPassword);
                var dqcall = PlugCall(getdq.pulgpath, main, new object[] { context,UI,UICode, NTokenType, SelectAdminToken, SelectUserToken, Con });
                if (dqcall.plugcode == null == false)
                {
                    richtext += dqcall.plugcode;
                }
            }
            return richtext;
        }


        public static bool StartPlug(int ID,HttpContext context,ref string message)
        {
            usertoken SelectAdminToken = new usertoken();
            usertoken SelectUserToken = new usertoken();
            TokenType NTokenType = DataProcessing.checktoken(context, ref SelectAdminToken, ref SelectUserToken);
            string main = "LoadPulg";
            List<pulginfo> getpulglis = selectallpulg();
            string richtext = null;
            foreach (var getdq in getpulglis)
            {
                if (getdq.lockerid == ID.ToString())
                {
                   
                    SqlCon Con = new SqlCon();
                    Con.serveraddress = PIN.AESDecrypt(DeFine.DBIPAddress);
                    Con.dbname = DeFine.DBName;
                    Con.port = DeFine.DBPort;
                    Con.username = PIN.AESDecrypt(DeFine.DBUSName);
                    Con.password = PIN.AESDecrypt(DeFine.DBUSPassword);
                    plugloader dqcall = new plugloader();
                    if (context == null)
                    {
                        dqcall = PlugCall(getdq.pulgpath, main, new object[] { "initialization", SelectAdminToken, SelectUserToken, Con });
                    }
                    else
                    {
                        dqcall = PlugCall(getdq.pulgpath, main, new object[] { NTokenType, SelectAdminToken, SelectUserToken, Con });
                    }
                   
                    if (dqcall.plugcode == null == false)
                    {
                        richtext += dqcall.plugcode;
                        message = richtext;
                    }
                }
            }
            if (message.ToLower().StartsWith("<error>"))
            {
                return false;
            }
            else
            {
                return true;
            }
           
        }

        public static bool ClosePlug(int ID,HttpContext context,ref string message)
        {
            usertoken SelectAdminToken = new usertoken();
            usertoken SelectUserToken = new usertoken();
            TokenType NTokenType = DataProcessing.checktoken(context, ref SelectAdminToken, ref SelectUserToken);
            string main = "UNLoadPulg";
            List<pulginfo> getpulglis = selectallpulg();
            string richtext = null;
            foreach (var getdq in getpulglis)
            {
                if (getdq.lockerid == ID.ToString())
                {
                    SqlCon Con = new SqlCon();
                    Con.serveraddress = PIN.AESDecrypt(DeFine.DBIPAddress);
                    Con.dbname = DeFine.DBName;
                    Con.port = DeFine.DBPort;
                    Con.username = PIN.AESDecrypt(DeFine.DBUSName);
                    Con.password = PIN.AESDecrypt(DeFine.DBUSPassword);
                    var dqcall = PlugCall(getdq.pulgpath, main, new object[] { NTokenType, SelectAdminToken, SelectUserToken, Con });
                    if (dqcall.plugcode == null == false)
                    {
                        richtext += dqcall.plugcode;
                        message = richtext;
                    }
                }
            }
            if (message.ToLower().StartsWith("<error>"))
            {
                return false;
            }
            else
            {
                return true;
            }

        }


        public static plugloader PlugCall(string path, string functionname, object []oder)
        {
            System.Reflection.Assembly ass;
            Type type;
            ass = System.Reflection.Assembly.LoadFile(path);
            type = ass.GetType("corehelper.plugin");//必须使用名称空间+类名称
            System.Reflection.MethodInfo method = type.GetMethod(functionname);//方法的名称
            method = type.GetMethod(functionname);//方法的名称
            string sreturn = (string)method.Invoke(null,oder); //静态方法的调用

            plugloader newoder = new plugloader();
            newoder.plugpath = path;
            newoder.plugcode = sreturn;
            return newoder;

        }

        public static plugloader PluginTmpCall(string path, string functionname, string filename, string iscode)
        {
            System.Reflection.Assembly ass;
            Type type;
            ass = System.Reflection.Assembly.LoadFile(path);
            type = ass.GetType("corehelper.plugin");//必须使用名称空间+类名称
            System.Reflection.MethodInfo method = type.GetMethod(functionname);//方法的名称
            method = type.GetMethod(functionname);//方法的名称
            string sreturn = (string)method.Invoke(null, new string[] { filename, iscode }); //静态方法的调用

            plugloader newoder = new plugloader();
            newoder.plugpath = path;
            newoder.plugcode = sreturn;
            return newoder;

        }

        public static plugloader pluginloader(string path, string callname, string[] allcommand)
        {
            object returntx = "";
            System.Reflection.Assembly ass;
            Type type;
            object obj;
            try
            {
                ass = System.Reflection.Assembly.LoadFile(path);//要绝对路径
                type = ass.GetType("corehelper.plugin");
                System.Reflection.MethodInfo method;
                obj = ass.CreateInstance("corehelper.plugin");

                if (allcommand.Length == 0)
                {
                    method = type.GetMethod(callname);
                    returntx = method.Invoke(null, null);
                }
                else
                {
                    method = type.GetMethod(callname);
                    returntx = method.Invoke(null, allcommand);
                }
                method = null;
            }
            catch (Exception ex)
            {

            }
            finally
            {
                ass = null;
                type = null;
                obj = null;
            }
            plugloader newoder = new plugloader();
            newoder.plugpath = path;
            newoder.plugcode = returntx;
            return newoder;
        }
        public static void initialization()
        {
            PluginHelper.PlugNavList.Clear();
            DataTable NTable = SqlServerHelper.ExecuteDataTable("Select * From pulginslist Where pulgcheck = 'True';");
            if (NTable.Rows.Count > 0)
            {
                for (int i = 0; i < NTable.Rows.Count; i++)
                {
                    string ID = DataHelper.ObjToStr(NTable.Rows[i]["ID"]);
                    string Message = "";
                    HttpContext Con = null;
                    if (PluginHelper.StartPlug(DataHelper.StrToInt(ID), Con, ref Message))
                    {
                        if (PluginHelper.activeselectpulg(DataHelper.StrToInt(ID).ToString(), true))
                        {
                            PlugNav NPlugNav = new PlugNav();
                            NPlugNav.PlugID = ID;
                            NPlugNav.NavCode = PluginHelper.GetFunctionValue(Message.ToLower(), "newnav");

                            PluginHelper.PlugNavList.Add(NPlugNav);
                        }
                    }
                }
            }
        }
        public static string GetFunctionValue(string Command,string Value)
        {
            string NextLine = "";
            string LockerValue = "<-@" + Value + "@->";
            if (Command.Contains(LockerValue))
            {
                NextLine = Command.Substring(Command.IndexOf(LockerValue) + LockerValue.Length);
                if (NextLine.Contains("<-@"))
                {
                    NextLine = NextLine.Substring(0, NextLine.IndexOf("<-@"));
                }
            }
            return NextLine;
        }
        public static List<plugloader> trycallplugin(string pathlocker, string typename, List<string> allcommand)
        {
            List<plugloader> newobjlis = new List<plugloader>();


            plugloader dqobj = pluginloader(pathlocker, typename, allcommand.ToArray());
            newobjlis.Add(dqobj);


            return newobjlis;


        }
        private static rttype IsAllPropertyNull<T>(T fac)
        {
            rttype zztype = new rttype();
            T ts = fac;
            PropertyInfo[] pi = ts.GetType().GetProperties();
            foreach (PropertyInfo p in pi)
            {
                object clsname = p.Name;
                object pvalue = p.GetValue(fac, null);


                if (pvalue is bool)
                {
                    if (clsname.ToString() == "rewrite")
                    {
                        zztype.rewrite = (bool)pvalue;
                    }
                }
                if (pvalue is string)
                {
                    if (clsname.ToString() == "fjmessage")
                    {
                        zztype.fjmessage = (string)pvalue;
                    }
                    if (clsname.ToString() == "plugintype")
                    {
                        zztype.plugintype = (string)pvalue;
                    }
                    if (clsname.ToString() == "html")
                    {
                        zztype.html = (string)pvalue;
                    }
                }

            }
            return zztype;
        }
        /// <summary>
        /// 获取所有配置信息
        /// </summary>
        /// <returns></returns>
        public static List<plugconfig> getallplugconfig(string pathlocker)
        {

            List<plugconfig> atureturn = new List<plugconfig>();
            List<quickgetcf> dqallcom = new List<quickgetcf>();
            List<string> allcon = new List<string>();
            List<plugloader> allcommand = new List<plugloader>();
            allcommand.AddRange(trycallplugin(pathlocker, "getname", allcon));
            allcommand.AddRange(trycallplugin(pathlocker, "getversion", allcon));
            allcommand.AddRange(trycallplugin(pathlocker, "getabout", allcon));
            allcommand.AddRange(trycallplugin(pathlocker, "getimage", allcon));
            allcommand.AddRange(trycallplugin(pathlocker, "getupdataurl", allcon));
            allcommand.AddRange(trycallplugin(pathlocker, "getshelluicode", allcon));

            foreach (var getdx in allcommand)
            {
                rttype dqget = new rttype();
                dqget = IsAllPropertyNull(getdx.plugcode);
                quickgetcf newsl = new quickgetcf();
                newsl.fjmessage = dqget.fjmessage;
                newsl.html = dqget.html;
                newsl.plugintype = dqget.plugintype;
                newsl.plugpath = getdx.plugpath;
                dqallcom.Add(newsl);
            }

            plugconfig fig = new plugconfig();
            foreach (var czcom in dqallcom)
            {
                fig.plugpath = czcom.plugpath;
                if (czcom.fjmessage == "name")
                {
                    fig.name = czcom.html;
                }
                if (czcom.fjmessage == "version")
                {
                    fig.version = czcom.html;
                }
                if (czcom.fjmessage == "about")
                {
                    fig.about = czcom.html;
                }
                if (czcom.fjmessage == "image")
                {
                    fig.image = czcom.html;
                }
                if (czcom.fjmessage == "updataurl")
                {
                    fig.updataurl = czcom.html;
                }
                if (czcom.fjmessage == "shelluicode")
                {
                    fig.shelluicode = czcom.html;
                }
            }
            atureturn.Add(fig);

            return atureturn;
        }

        public static string getcofrttext(List<plugconfig> e, string plugpath)
        {
            plugconfig getnewcof = new plugconfig();
            foreach (var c in e)
            {
                if (c.plugpath == plugpath)
                {
                    getnewcof = c;
                }

            }
            if (getnewcof.plugpath == "" == false)
            {
                string getplugpath = "插件引用地址:" + getnewcof.plugpath + "\r\n\r\n";
                string formattext = "插件名称:{0}\r\n插件版本:{1}\r\n版本描述:{2}\r\n缩略图:{3}\r\n插件更新地址:{4}\r\n插件界面代码:{5}\r\n";
                return getplugpath + string.Format(formattext, getnewcof.name, getnewcof.version, getnewcof.about, getnewcof.image, getnewcof.updataurl, getnewcof.shelluicode);
            }


            return "notfindplugs";
        }

        public static pulgtype getcofrttype(List<plugconfig> e, string plugpath)
        {
            plugconfig getnewcof = new plugconfig();
            pulgtype newrt = new pulgtype();
            foreach (var c in e)
            {
                if (c.plugpath == plugpath)
                {
                    getnewcof = c;
                }

            }
            if (getnewcof.plugpath == "" == false)
            {
                newrt.pulgname = getnewcof.name;
                newrt.pulgversion = getnewcof.version;
                newrt.pulgdsp = getnewcof.about;
                newrt.pulgimg = getnewcof.image;
                newrt.updatasrc = getnewcof.updataurl;
                newrt.uicode = getnewcof.shelluicode;
            }
            return newrt;
        }

        //结构体转byte数组
        public static byte[] StructToBytes(object structObj)
        {
            //得到结构体的大小
            int size = Marshal.SizeOf(structObj);
            //创建byte数组
            byte[] bytes = new byte[size];
            //分配结构体大小的内存空间
            IntPtr structPtr = Marshal.AllocHGlobal(size);
            //将结构体拷到分配好的内存空间
            Marshal.StructureToPtr(structObj, structPtr, false);
            //从内存空间拷到byte数组
            Marshal.Copy(structPtr, bytes, 0, size);
            //释放内存空间
            Marshal.FreeHGlobal(structPtr);
            //返回byte数组
            return bytes;
        }

        //byte数组转结构体:
        public static object BytesToStuct(byte[] bytes, Type type)
        {
            //得到结构体的大小
            int size = Marshal.SizeOf(type);
            //byte数组长度小于结构体的大小
            if (size > bytes.Length)
            {
                //返回空
                return null;
            }
            //分配结构体大小的内存空间
            IntPtr structPtr = Marshal.AllocHGlobal(size);
            //将byte数组拷到分配好的内存空间
            Marshal.Copy(bytes, 0, structPtr, size);
            //将内存空间转换为目标结构体
            object obj = Marshal.PtrToStructure(structPtr, type);
            //释放内存空间
            Marshal.FreeHGlobal(structPtr);
            //返回结构体
            return obj;
        }

        /// <summary>
        /// 查询所有插件
        /// </summary>
        /// <returns></returns>
        public static List<string> getallpulgins()
        {
            List<string> allfile = new List<string>();
            List<string> filetype = new List<string>();
            filetype.Add(".dll");
            string defpulginspath = DeFine.PluginPath;
            List<FileInformation> obj =DataHelper.getallfile(defpulginspath, defpulginspath, filetype);

            foreach (FileInformation getdqline in obj)
            {
                allfile.Add(getdqline.FilePath);
            }
            return allfile;
        }

        /// <summary>
        /// 部署你的插件
        /// </summary>
        /// <param name="elist"></param>
        /// <returns></returns>
        public static string installpulgins(List<string> elist)
        {
            string rtmessage = "";
            string sqlcommand = "INSERT INTO pulginslist(pulgpath,pulgmessage,pulgname,pulgversion,pulgdsp,pulgimg,updatasrc,pulgcheck)VALUES('{0}','{1}','{2}','{3}','{4}','{5}','{6}','{7}')";
            string defpulginspath = DeFine.PluginPath;


            if (elist.Count > 0 == false) { return "notfindpulgins"; }
            foreach (var get in elist)
            {
                string allpath = defpulginspath + get;
                try
                {
                    if (SqlServerHelper.ExecuteScalar("SELECT * from pulginslist WHERE pulgpath='" + get + "'") == null)//防止重复安装
                    {
                        var plugtype = getallplugconfig(allpath);
                        string dqmessage =getcofrttext(plugtype, allpath) + "\r\n";
                        pulgtype gettype = getcofrttype(plugtype, allpath);
                        rtmessage += dqmessage;
                        string sqloder = string.Format(sqlcommand, get, DateTime.Now.ToString("yyyy-MM-dd"), gettype.pulgname, gettype.pulgversion, gettype.pulgdsp, gettype.pulgimg, gettype.updatasrc,false.ToString());
                        int getinstall = SqlServerHelper.ExecuteNonQuery(sqloder);
                        rtmessage += "插件安装状态:" + getinstall.ToString() + "\r\n";
                    }
                }
                catch { }
            }
            return rtmessage;
        }

        /// <summary>
        /// 取激活的插件列表
        /// </summary>
        /// <returns></returns>
        public static List<pulginfo> selectactivepulg()
        {
            string defpulginspath = DeFine.PluginPath;
            List<pulginfo> elist = new List<pulginfo>();
            try
            {
                DataTable ntablis = new DataTable();
                string sqloder = "SELECT * FROM pulginslist WHERE pulgcheck='" + true.ToString() + "'";
                ntablis =SqlServerHelper.ExecuteDataTable(sqloder);
                if (ntablis.Rows.Count > 0)
                {
                    for (int i = 0; i < ntablis.Rows.Count; i++)
                    {
                        pulginfo ninfo = new pulginfo();
                        string plugpath = ntablis.Rows[i]["pulgpath"].ToString();
                        string allpath = defpulginspath + plugpath;
                        ninfo.pulgpath = allpath;
                        ninfo.pulgmessage = ntablis.Rows[i]["pulgmessage"].ToString();
                        ninfo.pulgname = ntablis.Rows[i]["pulgname"].ToString();
                        ninfo.pulgversion = ntablis.Rows[i]["pulgversion"].ToString();
                        ninfo.pulgdsp = ntablis.Rows[i]["pulgdsp"].ToString();
                        ninfo.pulgimg = ntablis.Rows[i]["pulgimg"].ToString();
                        ninfo.updatasrc = ntablis.Rows[i]["updatasrc"].ToString();
                        ninfo.pulgcheck = ntablis.Rows[i]["pulgcheck"].ToString();
                        elist.Add(ninfo);
                    }
                }
            }
            catch { }
            return elist;
        }
        // <summary>
        /// 取所有插件列表
        /// </summary>
        /// <returns></returns>
        public static List<pulginfo> selectallpulg()
        {
            string defpulginspath = DeFine.PluginPath;
            List<pulginfo> elist = new List<pulginfo>();
            DataTable ntablis = new DataTable();
            string sqloder = "SELECT * FROM pulginslist";
            ntablis =SqlServerHelper.ExecuteDataTable(sqloder);
            if (ntablis.Rows.Count > 0)
            {
                for (int i = 0; i < ntablis.Rows.Count; i++)
                {
                    pulginfo ninfo = new pulginfo();
                    string plugpath = ntablis.Rows[i]["pulgpath"].ToString();
                    string allpath = defpulginspath + plugpath;
                    ninfo.pulgpath = allpath;
                    ninfo.pulgmessage = ntablis.Rows[i]["pulgmessage"].ToString();
                    ninfo.pulgname = ntablis.Rows[i]["pulgname"].ToString();
                    ninfo.pulgversion = ntablis.Rows[i]["pulgversion"].ToString();
                    ninfo.pulgdsp = ntablis.Rows[i]["pulgdsp"].ToString();
                    ninfo.pulgimg = ntablis.Rows[i]["pulgimg"].ToString();
                    ninfo.updatasrc = ntablis.Rows[i]["updatasrc"].ToString();
                    ninfo.pulgcheck = ntablis.Rows[i]["pulgcheck"].ToString();
                    ninfo.lockerid = ntablis.Rows[i]["ID"].ToString();
                    elist.Add(ninfo);
                }
            }
            return elist;
        }
        /// <summary>
        /// 设置插件激活状态返回设置成功和失败
        /// </summary>
        /// <param name="pulgpath"></param>
        /// <returns></returns>
        public static bool activeselectpulg(string ID, bool check)
        {
            if (SqlServerHelper.ExecuteScalar("SELECT * from pulginslist WHERE ID='" + ID + "'") == null == false)
            {
                string sqloder = "UPDATE pulginslist SET pulgcheck='" + check.ToString() + "' WHERE ID='" + ID + "'";
                SqlServerHelper.ExecuteNonQuery(sqloder);
                return true;
            }
            return false;
        }

        /// <summary>
        /// 获取插件界面代码
        /// </summary>
        /// <param name="plugid"></param>
        /// <returns></returns>

        public static string getuiinpulg(int ID)
        {
            string defpulginspath = DeFine.PluginPath;
            DataTable ntablis = new DataTable();
            string sqloder = "SELECT * FROM pulginslist WHERE ID='" + ID + "'";
            ntablis =SqlServerHelper.ExecuteDataTable(sqloder);
            if (ntablis.Rows.Count > 0)
            {
                string LockerPath = defpulginspath + DataHelper.ObjToStr(ntablis.Rows[0]["pulgpath"]);
                var plugtype = getallplugconfig(LockerPath);
                return getcofrttype(plugtype, DataHelper.ObjToStr(LockerPath)).uicode;
            }
            return "";
        }
    }

    public class PlugNav
    {
        public string PlugID = "";
        public string NavCode = "";
    }
    public struct SqlCon
    {
        public string serveraddress { get; set; }
        public string dbname { get; set; }
        public int port { get; set; }
        public string username { get; set; }
        public string password { get; set; }
    }
    public class rttype
    {
        public string fjmessage { get; set; }//如=notoken则调用接口不需要验证token允许前端调用接口
        public string plugintype { get; set; }
        public bool rewrite { get; set; }
        public string html { get; set; }
    }
    public struct plugconfig
    {
        public string name { get; set; }
        public string version { get; set; }
        public string about { get; set; }
        public string image { get; set; }
        public string updataurl { get; set; }
        public string shelluicode { get; set; }
        public string plugpath { get; set; }
        public bool install { get; set; }
    }

    public struct plugloader
    {
        public string plugpath { get; set; }
        public object plugcode { get; set; }
    }
    public struct pulgtype
    {
        public string pulgname { get; set; }
        public string pulgversion { get; set; }
        public string pulgdsp { get; set; }
        public string pulgimg { get; set; }
        public string updatasrc { get; set; }
        public string uicode { get; set; }

    }

    public struct pulginfo
    {
        public string pulgpath { get; set; }
        public string pulgmessage { get; set; }
        public string pulgname { get; set; }
        public string pulgversion { get; set; }
        public string pulgdsp { get; set; }
        public string pulgimg { get; set; }
        public string updatasrc { get; set; }
        public string uicode { get; set; }
        public string pulgcheck { get; set; }
        public string lockerid { get; set; }
    }

    public struct quickgetcf
    {
        public string html { get; set; }
        public string fjmessage { get; set; }
        public string plugintype { get; set; }
        public string plugpath { get; set; }
        public string plugdsp { get; set; }

    }

    [StructLayoutAttribute(LayoutKind.Sequential, CharSet = CharSet.Ansi, Pack = 1)]
    struct TestStruct
    {
        public int c;

        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 256)]
        public string str;

        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 6)]
        public int[] test;
    }
}
