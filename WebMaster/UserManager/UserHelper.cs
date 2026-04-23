using WebMaster.DataManager;
using WebMaster.SQLManager;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text.RegularExpressions;
using System.Web;
using WebMaster.HtmlManager;

namespace WebMaster.UserManager
{
    public class UserHelper
    {


        /// <summary>
        /// token解析
        /// </summary>
        /// <param name="token"></param>
        /// <returns></returns>
        public static usertoken tokenjx(string token)
        {
            usertoken sltoken = new usertoken();
            string[] alltype = token.Split('>');
            string thistime = "";
            string username = "";
            string password = "";
            string coredb = "";
            string coredbname = "";
            if (alltype.Length >= 5)
            {
                username = alltype[0];
                password = alltype[1];
                thistime = alltype[2];
                coredb = alltype[3];
                coredbname = alltype[4];
            }
            sltoken.username = username;
            sltoken.password = password;
            sltoken.thistime = thistime;
            sltoken.coredb = coredb;
            sltoken.coredbname = coredbname;
            return sltoken;
        }

        /// <summary>
        /// 验证用户token
        /// </summary>
        /// <param name="token"></param>
        /// <returns></returns>
        public static bool TokenLegitimateByUser(string token)
        {
            //SELECT * FROM userlist Where username
            string GetNmae = PIN.AESDecrypt(PIN.Base64Decode(token));
            object get = SqlServerHelper.ExecuteScalar(CommandType.Text, "SELECT * FROM UserList Where username ='" + GetNmae + "'");
            if (get == null == false)
            { return true; }
            else { return false; }
        }
        /// <summary>
        /// 生成token高强度DES加密
        /// </summary>
        /// <param name="username"></param>
        /// <param name="userpassword"></param>
        /// <returns></returns>
        public static string setusertoken(string username, string userpassword, string dblocker, string dbname)
        {
            DateTime dqtime = DateTime.Now;
            string getthistime = dqtime.ToString("yyyy-MM-dd^HH:mm:ss");
            string usertoken = PIN.Encrypt(username + ">" + userpassword + ">" + getthistime + ">" + dblocker + ">" + dbname);
            //if (alltoken.Count < 5)
            //{
            //    alltoken.Add(usertoken);
            //}
            //else {
            //    usertoken = "";
            //}

            return usertoken;
        }


        /// <summary>
        /// 验证Token是否合法
        /// </summary>
        /// <param name="Token"></param>
        /// <returns></returns>
        public static bool TokenLegitimate(string Token, string Type, out usertoken ustoken)
        {

            if (Type == "FormGet")
            {
                ustoken = tokenjx(PIN.Decrypt(Token));
            }
            else
            {
                ustoken = tokenjx(PIN.Decrypt(Token));
            }

            string SqlFormat = "select usertype from UserList where userpassword='{0}' AND userid='{1}'";
            object Legitimate = SqlServerHelper.ExecuteScalar(CommandType.Text, string.Format(SqlFormat, ustoken.password, ustoken.username));
            if (Legitimate == null)
            {
                return false;
            }
            else
            {
                ustoken.usertype = Legitimate.ToString();
                return true;

            }
        }

        /// <summary>
        /// 用户登录验证
        /// </summary>
        /// <param name="UserName"></param>
        /// <param name="UserPassword"></param>
        /// <param name="IPAddress"></param>
        /// <param name="SelectDB"></param>
        /// <param name="DBName"></param>
        /// <returns></returns>
        public static usercls LoginUser(string userid, string UserPassword, string IPAddress, string SelectDB, string DBName)
        {
            usercls getcls = new usercls();
            string retext = "";
            string UserId = userid;
            string PinPassword = PIN.Encrypt(UserPassword);
            object gettype = "";
            string sqloder = "select userpassword from UserList where userid='" + userid + "' And usertype='admin'";
            object getpassword = SqlServerHelper.ExecuteScalar(CommandType.Text, string.Format(sqloder));
            if (getpassword == null)
            {
                retext = "用户名不存在!";
            }
            else
            {
                if (getpassword.ToString() == PinPassword)
                {
                    retext = "用户登陆成功!";
                    gettype = SqlServerHelper.ExecuteScalar(CommandType.Text, string.Format("select usertype from UserList where userid='" + userid + "'")).ToString();

                }
                else { retext = "密码输入错误!"; }
            }
            getcls.lockdb = SelectDB;
            getcls.lockname = DBName;
            getcls.username = userid;
            getcls.msg = retext;
            getcls.logintime = DateTime.Now.ToString("yyyy-MM-dd^hh:mm:ss");
            getcls.usertoken = setusertoken(userid, PinPassword, SelectDB, DBName);
            getcls.usertype = gettype.ToString();

            string lockday = DateTime.Now.ToString("yyyy-MM-dd");
            object getuserlogin = SqlServerHelper.ExecuteScalar(CommandType.Text, string.Format("select username from savelis where username='" + userid + "'AND lockday='" + lockday + "'"));
            string sqloderrt = string.Format("INSERT INTO savelis(username, msg, starttime, lasttime,ipaddress,lockday)VALUES('{0}','{1}','{2}','{3}','{4}','{5}')", StringHelper.FormatSpecialSymbolsClear(userid), getcls.msg, getcls.logintime, "", IPAddress, lockday);
            if (getuserlogin == null)
            {
                SqlServerHelper.ExecuteNonQuery(CommandType.Text, sqloderrt);
            }

            return getcls;
        }
        public static usercls LoginUser(string UserName, string UserPassword, string IPAddress)
        {
            return LoginUser(UserName, UserPassword, IPAddress, DeFine.DBIPAddress, DeFine.DBName);
        }

        /// <summary>
        /// 根据TOKEN解析用户数据
        /// </summary>
        /// <param name="GetUserToken"></param>
        /// <returns></returns>
        public static UserINFO GetUserINFO(usertoken GetUserToken)
        {
            if (GetUserToken.username == null == false && GetUserToken.username == "" == false)
            {
                UserINFO NUserINFO = new UserINFO();
                DataTable Msg = SqlServerHelper.ExecuteDataTable("Select * From UserList where userid= '" + GetUserToken.username + "'");
                if (Msg.Rows.Count > 0)
                {

                    NUserINFO.userid = DataHelper.ObjToStr(Msg.Rows[0]["userid"]);
                    NUserINFO.username = DataHelper.ObjToStr(Msg.Rows[0]["username"]);
                    NUserINFO.usersex = DataHelper.StrToInt(DataHelper.ObjToStr(Msg.Rows[0]["usersex"]));
                    NUserINFO.userbirthday = DataHelper.ObjToStr(Msg.Rows[0]["userbirthday"]);
                    NUserINFO.userpicpath = DataHelper.ObjToStr(Msg.Rows[0]["userpicpath"]);
                    NUserINFO.useremail = DataHelper.ObjToStr(Msg.Rows[0]["useremail"]);
                    NUserINFO.usertype = DataHelper.ObjToStr(Msg.Rows[0]["usertype"]);
                    NUserINFO.userphone = DataHelper.ObjToStr(Msg.Rows[0]["userphone"]);
                    NUserINFO.userfristfromaddress = DataHelper.ObjToStr(Msg.Rows[0]["userfristfromaddress"]);
                    NUserINFO.Subclass = DataHelper.ObjToStr(Msg.Rows[0]["Subclass"]);
                    return NUserINFO;
                }
                else
                {
                    return null;
                }
            }
            return null;
        }

        public static UserINFO GetUserINFO(string userid)
        {
            if (userid == "" == false)
            {
                if (userid == null == false)
                {
                    UserINFO NUserINFO = new UserINFO();
                    DataTable Msg = SqlServerHelper.ExecuteDataTable("Select * From UserList where userid= '" + userid + "'");
                    if (Msg.Rows.Count > 0)
                    {
                        NUserINFO.userid = DataHelper.ObjToStr(Msg.Rows[0]["userid"]);
                        NUserINFO.username = DataHelper.ObjToStr(Msg.Rows[0]["username"]);
                        NUserINFO.usersex = DataHelper.StrToInt(DataHelper.ObjToStr(Msg.Rows[0]["usersex"]));
                        NUserINFO.userbirthday = DataHelper.ObjToStr(Msg.Rows[0]["userbirthday"]);
                        NUserINFO.userpicpath = DataHelper.ObjToStr(Msg.Rows[0]["userpicpath"]);
                        NUserINFO.useremail = DataHelper.ObjToStr(Msg.Rows[0]["useremail"]);
                        NUserINFO.usertype = DataHelper.ObjToStr(Msg.Rows[0]["usertype"]);
                        NUserINFO.userphone = DataHelper.ObjToStr(Msg.Rows[0]["userphone"]);
                        NUserINFO.userfristfromaddress = DataHelper.ObjToStr(Msg.Rows[0]["userfristfromaddress"]);
                        NUserINFO.Subclass = DataHelper.ObjToStr(Msg.Rows[0]["Subclass"]);
                        return NUserINFO;
                    }
                    else
                    {
                        return null;
                    }
                }
            }
            return null;
        }

        public static List<string> Boy = new List<string>() { "Boy", "Male", "男" };

        public static List<string> Girl = new List<string>() { "Girl", "Female", "女" };

        public static int GetUserSex(string str)
        {
            foreach (var Get in Boy)
            {
                if (str.Trim().ToLower().StartsWith(Get.ToLower()))
                {
                    return 1;
                }

            }

            foreach (var Get in Girl)
            {
                if (str.Trim().ToLower().StartsWith(Get.ToLower()))
                {
                    return 0;
                }
            }

            return -1;

        }

        /// <summary>
        /// 修改用户缩略图
        /// </summary>
        /// <param name="UserID"></param>
        /// <param name="userpicpath"></param>
        /// <returns></returns>
        public static bool ReloadUserPic(string UserID,string userpicpath)
        {
            string sqloder = "UPDATE UserList Set userpicpath ='" + userpicpath + "' Where UserID ='" + UserID + "';";
            int state = SqlServerHelper.ExecuteNonQuery(sqloder);
            if (state == 0 == false)
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        /// <summary>
        /// 通过字符串获取用户生日和年龄
        /// </summary>
        /// <param name="Birthday"></param>
        /// <returns></returns>

        public static UserAge ConvertUserBirthday(string Birthday)
        {

            string[] AllParma = Birthday.Split('.');
            int Year = 0;
            int Month = 0;
            int Day = 0;
            int Finds = 0;
            foreach (var GetItem in AllParma)
            {
                if (GetItem.Trim().Length > 0)
                {
                    Finds++;
                    if (Finds == 1)
                    {
                        Year = DataHelper.StrToInt(GetItem);
                    }
                    if (Finds == 2)
                    {
                        Month = DataHelper.StrToInt(GetItem);
                    }
                    if (Finds == 3)
                    {
                        Day = DataHelper.StrToInt(GetItem);
                    }
                }
            }
            return new UserAge(Year, Month, Day);
        }
        /// <summary>
        /// 验证字符串是否符合格式
        /// </summary>
        /// <param name="Birthday"></param>
        /// <returns></returns>
        public static bool ChecksumBirthday(string Birthday)
        {
            string[] AllParma = Birthday.Split('.');
            int Finds = 0;
            foreach (var GetItem in AllParma)
            {
                if (GetItem.Trim().Length > 0)
                {
                    int Number = 0;
                    int.TryParse(GetItem, out Number);
                    if (Number > 0)
                    {
                        Finds++;
                        if (Finds == 1)
                        {
                            if (Number > 999)
                            {
                                //is year
                            }
                            else
                            {
                                return false;
                            }
                        }
                        if (Finds == 2)
                        {
                            if (Number == 0 == false && Number <= 12)
                            {
                                //is Month
                            }
                            else
                            {
                                return false;
                            }
                        }
                        if (Finds == 3)
                        {
                            if (Number == 0 == false && Number <= 31)
                            {
                                //is Day
                            }
                            else
                            {
                                return false;
                            }

                        }
                    }

                }
            }
            if (Finds == 3)
            {
                return true;
            }
            else
            {
                return false;
            }
        }


    }
    public class UserVisit
    {
        public string WebPage;
        public string LastVisitUrl;
        public int VisitCount;
        public UserVisit(string WebPage, string LastVisitUrl, int VisitCount)
        {
            this.WebPage = WebPage;
            this.LastVisitUrl = LastVisitUrl;
            this.VisitCount = VisitCount;
        }
    }
    public struct userlogin
    {
        public string username { get; set; }

        public string usertoken { get; set; }

        public string targethtml { get; set; }

        public string Msg { get; set; }

        public string LoginDataTime { get; set; }
    }
    public struct usercls
    {
        public string username { get; set; }
        public string usertoken { get; set; }
        public string logintime { get; set; }
        public string usertype { get; set; }
        public string msg { get; set; }
        public string lockdb { get; set; }
        public string lockname { get; set; }
    }
    //用户token信息结构体
    public struct usertoken
    {
        public string usertype { get; set; }
        public string username { get; set; }
        public string password { get; set; }
        public string thistime { get; set; }
        public string coredb { get; set; }
        public string coredbname { get; set; }
    }

    public class UserAge
    {
        public int Year = 0;
        public int Month = 0;
        public int Day = 0;

        public int Age = 0;
        public UserAge(int Year, int Month, int Day)
        {
            this.Age = DateTime.Now.Year - Year;

            this.Year = Year;
            this.Month = Month;
            this.Day = Day;
        }
    }
}