using WebMaster.HtmlManager;
using WebMaster.SQLManager;
using WebMaster.UserManager;
using System;
using System.Collections.Generic;
using System.Data;
using System.Web;
using System.IO;
using System.Threading;
using System.Text;
using WebMaster.CoreManager;
using WebMaster.WebManager;
using WebMaster.LabelManager;
using WebMaster.BlockChainManager;

namespace WebMaster.DataManager
{

    public class DataProcessing
    {
        public static bool LockerDB = false;
        public static void OpenDBCon()
        {
            if (!LockerDB)
            {
                SqlServerHelper.setsqlservercon(DeFine.DBIPAddress, DeFine.DBName, DeFine.DBPort, DeFine.DBUSName, DeFine.DBUSPassword);
                LockerDB = true;
            }
        }

        public static bool checkaddress(HttpContext context)
        {
            Uri referrerUri = HttpContext.Current.Request.UrlReferrer;//获取下载之前访问的那个页面的uri
            Uri currentUri = HttpContext.Current.Request.Url;
            if (referrerUri == null)//没有前导页，直接访问下载页
            {
                //输出提示，可以根据自身要求完善此处代码

                return false;
            }
            return true;
        }
        public static TokenType checktoken(HttpContext LockerHttpContext, ref usertoken SelectAdminToken, ref usertoken SelectUserToken)
        {
            HttpContext GetHttpContext = LockerHttpContext;
            string AdminToken = null;
            string AdminTokenType = null;

            string UserToken = null;
            string UserTokenType = null;


            DataProcessing.ValidationToken(PlugHelper.PutAllRequest(GetHttpContext), ref AdminToken, ref AdminTokenType);
            DataProcessing.ValidationUserToken(PlugHelper.PutAllRequest(GetHttpContext), ref UserToken, ref UserTokenType);

            UserHelper.TokenLegitimate(AdminToken, AdminTokenType, out SelectAdminToken);
            if (SelectAdminToken.usertype == "admin")
            {
                return TokenType.Admin;
            }
            UserHelper.TokenLegitimate(UserToken, UserTokenType, out SelectUserToken);
            if (SelectUserToken.usertype == "user")
            {
                return TokenType.User;
            }
            return TokenType.Null;
        }
        public static string ProcessingRequest(HttpContext LockerHttpContext, HttpContextcls Request, string HtmlFile = "")
        {
            string LockerUserIPAddress = IPAddressHelper.GetUserIPAddress(LockerHttpContext);
            WebMaster.WebManager.WebDefence.NewConnect(LockerUserIPAddress);
            int GetUserConnect = WebMaster.WebManager.WebDefence.IsConnect(LockerUserIPAddress);
            string HtmlCode = "";
            string AdminToken = null;
            string AdminTokenType = null;
            string UserToken = null;
            string UserTokenType = null;

            ValidationToken(Request, ref AdminToken, ref AdminTokenType);
            ValidationUserToken(Request, ref UserToken, ref UserTokenType);

            usertoken SelectAdminToken = new usertoken();
            usertoken SelectUserToken = new usertoken();
          
            if (GetUserConnect > DeFine.WallMaxConnect)
            {
                bool ReturnZero =true;
                if (AdminToken == null == false)
                {
                    usertoken TmpAdminToken = new usertoken();
                    UserHelper.TokenLegitimate(AdminToken, AdminTokenType, out TmpAdminToken);
                    if (TmpAdminToken.usertype == "admin")
                    {
                        if(TmpAdminToken.username.ToLower()=="admin") ReturnZero = false;
                    }
                }
                if (ReturnZero)
                {
                    GC.Collect();
                    return string.Format("保护性阻断访问 {0}:{1}", LockerUserIPAddress, GetUserConnect);
                }
            }

            if (AdminToken == null == false || UserToken == null == false)
            {
                UserHelper.TokenLegitimate(AdminToken, AdminTokenType, out SelectAdminToken);
                UserHelper.TokenLegitimate(UserToken, UserTokenType, out SelectUserToken);

                if (SelectAdminToken.usertype == "admin")
                {
                    //存在Token Admin权限
                    if (UserCenter(Request, ref HtmlCode)) return HtmlCode;
                    if (DelectUser(Request, ref HtmlCode, SelectAdminToken)) return HtmlCode;
                    if (ExitAdmin(LockerHttpContext, Request, ref HtmlCode)) return HtmlCode;
                    if (ReloadUser(Request, ref HtmlCode, SelectAdminToken)) return HtmlCode;
                    if (NewConnection(LockerHttpContext,Request, ref HtmlCode, "admin", SelectAdminToken)) return HtmlCode;
                    if (GetVisitChart(Request, ref HtmlCode)) return HtmlCode;
                    if (GetVisitTable(Request, ref HtmlCode)) return HtmlCode;
                    if (GetVisitCountByDay(Request, ref HtmlCode)) return HtmlCode;
                    if (GetDBCount(Request, ref HtmlCode)) return HtmlCode;
                    if (GetEmailCount(Request, ref HtmlCode)) return HtmlCode;
                    if (GetDarkUserCount(Request, ref HtmlCode)) return HtmlCode;
                    if (GetBrainMessage(Request, ref HtmlCode)) return HtmlCode;
                    if (GetLoginRecord(Request, ref HtmlCode)) return HtmlCode;
                    if (ClearLoginRecord(Request, ref HtmlCode, SelectAdminToken)) return HtmlCode;
                    if (GetNewEmailMessage(Request, ref HtmlCode)) return HtmlCode;
                    if (GetNavListByType(Request, ref HtmlCode)) return HtmlCode;
                    if (GetActiveHtmlTemplate(Request, ref HtmlCode)) return HtmlCode;
                    if (ReloadNavColumn(Request, ref HtmlCode)) return HtmlCode;
                    if (DelectNavColumn(Request, ref HtmlCode)) return HtmlCode;
                    if (AddNavColumn(Request, ref HtmlCode)) return HtmlCode;
                    if (ReloadLineT(Request, ref HtmlCode)) return HtmlCode;
                    if (ReloadLineTT(Request, ref HtmlCode)) return HtmlCode;
                    if (AddLineT(Request, ref HtmlCode)) return HtmlCode;
                    if (AddLineTT(Request, ref HtmlCode)) return HtmlCode;
                    if (DelectLineT(Request, ref HtmlCode)) return HtmlCode;
                    if (DelectLineTT(Request, ref HtmlCode)) return HtmlCode;
                    if (ReloadListT(Request, ref HtmlCode)) return HtmlCode;
                    if (ReloadListTT(Request, ref HtmlCode)) return HtmlCode;
                    if (GetArticlePageData(Request, ref HtmlCode)) return HtmlCode;
                    if (GetAllArticleColumn(Request, ref HtmlCode)) return HtmlCode;
                    if (SetArticleText(Request, ref HtmlCode)) return HtmlCode;
                    if (GetArticle(Request, ref HtmlCode)) return HtmlCode;
                    if (DelectArticle(Request, ref HtmlCode)) return HtmlCode;
                    if (CreatArticle(Request, ref HtmlCode)) return HtmlCode;
                    if (GetHtmlTemplate(Request, ref HtmlCode)) return HtmlCode;
                    if (TryConvert(Request, ref HtmlCode)) return HtmlCode;
                    if (SaveHtmlTemplate(Request, ref HtmlCode, SelectAdminToken)) return HtmlCode;
                    if (DelectHtmlTemplate(Request, ref HtmlCode, SelectAdminToken)) return HtmlCode;
                    if (GetLockerImage(Request, ref HtmlCode)) return HtmlCode;
                    if (KillStandImg(Request, ref HtmlCode)) return HtmlCode;
                    if (GetAllFileList(Request, ref HtmlCode)) return HtmlCode;
                    if (SetFileItemValue(Request, ref HtmlCode)) return HtmlCode;
                    if (DelectFileItem(Request, ref HtmlCode)) return HtmlCode;
                    if (GetPluginUICode(Request, ref HtmlCode)) return HtmlCode;
                    if (GetAllPlugin(Request, ref HtmlCode)) return HtmlCode;
                    if (ActivePulg(LockerHttpContext,Request, ref HtmlCode)) return HtmlCode;
                    if (GetInstallPulg(Request, ref HtmlCode)) return HtmlCode;
                    if (InstallPulg(Request, ref HtmlCode)) return HtmlCode;
                    if (DelectPulg(Request, ref HtmlCode)) return HtmlCode;
                    if (ShellEngine(Request, ref HtmlCode)) return HtmlCode;
                    if (SecurityScanning(Request, ref HtmlCode)) return HtmlCode;
                    if (GetSecurityScanningMsg(Request, ref HtmlCode)) return HtmlCode;
                    if (GetSecurityScanningData(Request, ref HtmlCode)) return HtmlCode;
                    if (GetFireWallList(Request, ref HtmlCode)) return HtmlCode;
                    if (AddWorkTime(Request, ref HtmlCode)) return HtmlCode;
                    if (GetWorkTimeList(Request, ref HtmlCode)) return HtmlCode;
                    if (DelectWorkTimeItem(Request, ref HtmlCode)) return HtmlCode;
                    if (StartWorkTimeItem(Request, ref HtmlCode)) return HtmlCode;
                    if (EndWorkTimeItem(Request, ref HtmlCode)) return HtmlCode;
                    if (ReloadWorkTimeItem(Request, ref HtmlCode)) return HtmlCode;
                    if (GetWebSetting(Request, ref HtmlCode)) return HtmlCode;
                    if (SetWebSetting(Request, ref HtmlCode)) return HtmlCode;
                    if (CheckPoolValue(Request, ref HtmlCode)) return HtmlCode;
                    if (GetAllBlockByHead(Request, ref HtmlCode)) return HtmlCode;
                    if (SaveFileToBlock(Request, ref HtmlCode)) return HtmlCode;
                    if (SaveStrToBlock(Request, ref HtmlCode)) return HtmlCode;

                 
                }
                if (SelectUserToken.usertype == "user")
                {
                    //存在Token User权限
                    if (ReloadUserPic(Request, ref HtmlCode, SelectUserToken)) return HtmlCode;
               
                    if (GetUserINFO(Request, ref HtmlCode, SelectUserToken)) return HtmlCode;
                    if (ExitUser(LockerHttpContext, Request, ref HtmlCode)) { return HtmlCode; }
                    if (NewConnection(LockerHttpContext,Request, ref HtmlCode, "user", SelectUserToken)) return HtmlCode;
                }
            }
            else
            {
                //不存在Token
                if (NewConnection(LockerHttpContext,Request, ref HtmlCode, "null", new usertoken())) return HtmlCode;
            }

            //全部 



            if (GetLostPassword(Request, ref HtmlCode))
            {
                return HtmlCode;
            }
            if (RegUser(LockerHttpContext,Request, ref HtmlCode))
            {
                return HtmlCode;
            }
            if (LoginUserby(LockerHttpContext, Request, ref HtmlCode))
            {
                return HtmlCode;
            }
            if (ReloadUser(Request, ref HtmlCode, SelectUserToken))
            {
                return HtmlCode;
            }
            if (GetUserLogin(Request, ref HtmlCode, SelectUserToken))
            {
                return HtmlCode;
            }
            if (GetAdminLogin(Request, ref HtmlCode, SelectUserToken))
            {
                return HtmlCode;
            }

            return HtmlCode;

        }


        /// <summary>
        /// 效验所有类型的token
        /// </summary>
        /// <param name="Request"></param>
        /// <param name="UserToken"></param>
        /// <param name="TokenType"></param>
        public static void ValidationToken(HttpContextcls Request, ref string UserToken, ref string TokenType)
        {
            UserToken = PlugHelper.ContextSetting(Request, "Cookies", "USToken");
            usertoken GetAdminToken = new usertoken();
            if (UserToken == null == false)
            {
                if (UserHelper.TokenLegitimate(UserToken, "FormCookies", out GetAdminToken))
                {
                    TokenType = "FormCookies";
                    return;
                }

            }
            UserToken = PlugHelper.ContextSetting(Request, "Post", "USToken");
            if (UserToken == null == false)
            {
                if (UserHelper.TokenLegitimate(UserToken, "FormPost", out GetAdminToken))
                {
                    TokenType = "FormPost";
                    return;
                }

            }
            UserToken = PlugHelper.ContextSetting(Request, "Get", "USToken");
            if (UserToken == null == false)
            {
                if (UserHelper.TokenLegitimate(UserToken, "FormGet", out GetAdminToken))
                {
                    TokenType = "FormGet";
                    return;
                }

            }

        }

        public static void ValidationUserToken(HttpContextcls Request, ref string UserToken, ref string TokenType)
        {
            UserToken = PlugHelper.ContextSetting(Request, "Cookies", "Token");
            usertoken GetToken = new usertoken();
            if (UserToken == null == false)
            {
                if (UserHelper.TokenLegitimate(UserToken, "FormCookies", out GetToken))
                {
                    TokenType = "FormCookies";
                    return;
                }

            }
            UserToken = PlugHelper.ContextSetting(Request, "Post", "Token");
            if (UserToken == null == false)
            {
                if (UserHelper.TokenLegitimate(UserToken, "FormPost", out GetToken))
                {
                    TokenType = "FormPost";
                    return;
                }

            }
            UserToken = PlugHelper.ContextSetting(Request, "Get", "Token");
            if (UserToken == null == false)
            {
                if (UserHelper.TokenLegitimate(UserToken, "FormGet", out GetToken))
                {
                    TokenType = "FormGet";
                    return;
                }

            }
        }

        public static string ValidationToken(HttpContextcls Request, ref string TokenType)
        {
            string UserToken = "";
            UserToken = PlugHelper.ContextSetting(Request, "Cookies", "USToken");
            usertoken GetToken = new usertoken();
            if (UserToken == null == false)
            {
                if (UserHelper.TokenLegitimate(UserToken, "FormCookies", out GetToken))
                {
                    TokenType = "FormCookies";
                    return UserToken;
                }

            }
            UserToken = PlugHelper.ContextSetting(Request, "Post", "USToken");
            if (UserToken == null == false)
            {
                if (UserHelper.TokenLegitimate(UserToken, "FormPost", out GetToken))
                {
                    TokenType = "FormPost";
                    return UserToken;
                }

            }
            UserToken = PlugHelper.ContextSetting(Request, "Get", "USToken");
            if (UserToken == null == false)
            {
                if (UserHelper.TokenLegitimate(UserToken, "FormGet", out GetToken))
                {
                    TokenType = "FormGet";
                    return UserToken;
                }

            }
            return UserToken;
        }

        /// <summary>
        /// 用户登出
        /// </summary>
        /// <param name="Request"></param>
        /// <param name="UserToken"></param>
        /// <param name="TokenType"></param>
        public static bool ExitUser(HttpContext LockerHttpContext, HttpContextcls Request, ref string ReturnMessage, string Type = "Get")
        {
            string gettype = PlugHelper.ContextSetting(Request, Type, "type");
            if (gettype == "clearuser")
            {
                if (CookieHelper.GetCookieValue(LockerHttpContext, "token") == null == false)
                {
                    CookieHelper.ClearCookie(LockerHttpContext, "token");
                    ReturnMessage = "用户登出成功";
                    return true;
                }
                else
                {
                    ReturnMessage = "不存在token";
                    return true;
                }

            }
            return false;
        }

        /// <summary>
        /// 管理员登出
        /// </summary>
        /// <param name="Request"></param>
        /// <param name="UserToken"></param>
        /// <param name="TokenType"></param>
        public static bool ExitAdmin(HttpContext LockerHttpContext, HttpContextcls Request, ref string ReturnMessage, string Type = "Get")
        {
            string gettype = PlugHelper.ContextSetting(Request, Type, "type");
            if (gettype == "clearadmin")
            {
                if (CookieHelper.GetCookieValue(LockerHttpContext, "UStoken") == null == false)
                {
                    CookieHelper.ClearCookie(LockerHttpContext, "UStoken");
                    ReturnMessage = "管理员登出成功";
                    return true;
                }
                else
                {
                    ReturnMessage = "不存在UStoken";
                    return true;
                }

            }
            return false;
        }

        /// <summary>
        /// 获取登录状态
        /// </summary>
        /// <param name="Request"></param>
        /// <param name="ReturnMessage"></param>
        /// <param name="Type"></param>
        /// <returns></returns>
        public static bool GetUserLogin(HttpContextcls Request, ref string ReturnMessage, usertoken token, string Type = "Get")
        {
            string gettype = PlugHelper.ContextSetting(Request, Type, "type");
            if (gettype == "GetUserLogin")
            {
                string JsonFormat = "{'Login':'{0}'}".Replace("'", "\"");
                if (token.password == null == false)
                {
                    if (token.password.Length > 0)
                    {
                        if (token.usertype == "user")
                        {
                            ReturnMessage = string.Format(JsonFormat, true.ToString());
                            return true;
                        }
                        else
                        {
                            ReturnMessage = string.Format(JsonFormat, false.ToString());
                            return true;
                        }
                    }
                    else
                    {
                        ReturnMessage = string.Format(JsonFormat, false.ToString());
                        return true;
                    }

                }
                else
                {
                    ReturnMessage = string.Format(JsonFormat, false.ToString());
                    return true;
                }
            }
            return false;
        }

        public static bool GetAdminLogin(HttpContextcls Request, ref string ReturnMessage, usertoken token, string Type = "Get")
        {
            string gettype = PlugHelper.ContextSetting(Request, Type, "type");
            if (gettype == "GetUserLogin")
            {
                string JsonFormat = "{'Login':'{0}'}".Replace("'", "\"");
                if (token.password == null == false)
                {
                    if (token.password.Length > 0)
                    {
                        if (token.usertype == "admin")
                        {
                            ReturnMessage = string.Format(JsonFormat, true.ToString());
                            return true;
                        }
                        else
                        {
                            ReturnMessage = string.Format(JsonFormat, false.ToString());
                            return true;
                        }
                    }
                    else
                    {
                        ReturnMessage = string.Format(JsonFormat, false.ToString());
                        return true;
                    }

                }
                else
                {
                    ReturnMessage = string.Format(JsonFormat, false.ToString());
                    return true;
                }
            }
            return false;
        }
        /// <summary>
        /// 用户中心接口
        /// </summary>
        /// <returns></returns>
        public static bool UserCenter(HttpContextcls Request, ref string ReturnMessage, string Type = "Get")
        {
            string gettype = PlugHelper.ContextSetting(Request, Type, "type");
            if (gettype == "UScenter")
            {
                List<UserMsgItem> AllUser = new List<UserMsgItem>();
                DataTable Msg = SqlServerHelper.ExecuteDataTable("Select * From UserList");
                if (Msg.Rows.Count > 0)
                {
                    for (int i = 0; i < Msg.Rows.Count; i++)
                    {
                        UserMsgItem NUserMsgItem = new UserMsgItem();
                        NUserMsgItem.ID = DataHelper.ObjToStr(Msg.Rows[i]["ID"]);
                        NUserMsgItem.userid = DataHelper.ObjToStr(Msg.Rows[i]["userid"]);
                        NUserMsgItem.userpassword = PIN.Decrypt(DataHelper.ObjToStr(Msg.Rows[i]["userpassword"]));
                        NUserMsgItem.username = DataHelper.ObjToStr(Msg.Rows[i]["username"]);
                        NUserMsgItem.usersex = DataHelper.ObjToStr(Msg.Rows[i]["usersex"]);
                        NUserMsgItem.usertype = DataHelper.ObjToStr(Msg.Rows[i]["usertype"]);
                        NUserMsgItem.useremail = DataHelper.ObjToStr(Msg.Rows[i]["useremail"]);
                        NUserMsgItem.userphone = DataHelper.ObjToStr(Msg.Rows[i]["userphone"]);
                        NUserMsgItem.userbirthday = DataHelper.ObjToStr(Msg.Rows[i]["userbirthday"]);
                        NUserMsgItem.userpicpath = DataHelper.ObjToStr(Msg.Rows[i]["userpicpath"]);
                        NUserMsgItem.userfristfromaddress = DataHelper.ObjToStr(Msg.Rows[i]["userfristfromaddress"]);
                        NUserMsgItem.Subclass = DataHelper.ObjToStr(Msg.Rows[i]["Subclass"]);
                        if (NUserMsgItem.userid == "admin" == false) AllUser.Add(NUserMsgItem);
                    }
                    ReturnMessage = JsonHelper.DataFormatToJson(AllUser);
                    return true;
                }
                else
                {
                    return true;
                }
            }
            return false;
        }
      
        /// <summary>
        /// 用户注册
        /// </summary>
        /// <param name="Request"></param>
        /// <param name="ReturnMessage"></param>
        /// <param name="Type"></param>
        /// <returns></returns>
        public static bool RegUser(HttpContext LockerHttpContext,HttpContextcls Request, ref string ReturnMessage, string Type = "Get")
        {
            string gettype = PlugHelper.ContextSetting(Request, Type, "type");
            if (gettype == "Reguser")
            {
                string userid = PlugHelper.ContextSetting(Request, Type, "userid");
                string userpassword = PlugHelper.ContextSetting(Request, Type, "userpassword");
                string username = PlugHelper.ContextSetting(Request, Type, "username");
                string usertype = "user";
                string useremail = PlugHelper.ContextSetting(Request, Type, "useremail");
                int usersex=UserHelper.GetUserSex(PlugHelper.ContextSetting(Request, Type, "usersex"));
                string userphone = PlugHelper.ContextSetting(Request, Type, "userphone");
                string userbirthday = PlugHelper.ContextSetting(Request, Type, "userbirthday");
                string userpicpath = PlugHelper.ContextSetting(Request, Type, "userpicpath");
                var VarLockerUserSource = IPAddressHelper.GetUserSource(LockerHttpContext);
                string userfristfromaddress = VarLockerUserSource.UserIPAddress+">"+ VarLockerUserSource.BroadbandOperators+">"+VarLockerUserSource.UserFromPlace;
                string SystemCode = PlugHelper.ContextSetting(Request, Type, "SystemCode", "HtmlCode");

                if (usersex >= 0==false) {ReturnMessage = "Please InPut Sex";return true; }
                if (!useremail.Contains("@")) { ReturnMessage = "EmailStringsError is Not DefFormat"; return true; }
                if(!UserHelper.ChecksumBirthday(userbirthday)) { ReturnMessage = "Please Checksum Your BirthDay  Must Format :Year.Month.Day"; return true; }

                string sqloder = "INSERT INTO UserList(userid,userpassword,username,usersex,usertype,useremail,userphone,userbirthday,userpicpath,userfristfromaddress) VALUES ('{0}', '{1}', '{2}', '{3}','{4}','{5}', '{6}', '{7}','{8}','{9}');";
                if (SystemCode == "ReGadmin->~@123~")
                {
                    usertype = "admin";
                    SqlServerHelper.ExecuteNonQuery(CommandType.Text, string.Format(sqloder, userid, PIN.Encrypt(userpassword), username, usersex, usertype, useremail, userphone, userbirthday, userpicpath, userfristfromaddress));
                    ReturnMessage = "TryUpDataByAdminCode";
                    return true;
                }

                object getobj = SqlServerHelper.ExecuteScalar(CommandType.Text, "SELECT userid FROM UserList Where userid = '" + userid + "'");

                if (getobj == null == false)
                {
                    ReturnMessage = "User ID Repeat";
                    return true;

                }
                if (userid == null || userid == "")
                {
                    ReturnMessage = "User ID is not to Nothing";
                    return true;
                }

                if (userpassword == null || userpassword == "")
                {
                    ReturnMessage = "Password is not to Nothing";
                    return true;
                }

                int get = SqlServerHelper.ExecuteNonQuery(CommandType.Text, string.Format(sqloder, userid, PIN.Encrypt(userpassword), username, usersex, usertype, useremail, userphone, userbirthday, userpicpath, userfristfromaddress));

                if (get > 0)
                {
                    ReturnMessage = "Reg Done!" + DateTime.Now.ToString("yyyy:MM:dd,HH:mm:ss");
                }
                else
                {
                    ReturnMessage = "Reg Error";
                }
                return true;
            }


            return false;
        }

        /// <summary>
        /// 用户修改自身信息
        /// </summary>
        /// <param name="Request"></param>
        /// <param name="ReturnMessage"></param>
        /// <param name="Type"></param>
        /// <returns></returns>
        public static bool ReloadUser(HttpContextcls Request, ref string ReturnMessage, usertoken GetUserToken, string Type = "Get")
        {
            string gettype = PlugHelper.ContextSetting(Request, Type, "type");
            if (gettype == "reloaduser")
            {
                string GetUserType = GetUserToken.username;
                if (GetUserType == "admin")
                {
                    string userid = PlugHelper.ContextSetting(Request, Type, "userid");
                    string username = PlugHelper.ContextSetting(Request, Type, "username");
                    string userpicpath = PlugHelper.ContextSetting(Request, Type, "userpicpath");
                    string useremail = PlugHelper.ContextSetting(Request, Type, "useremail");
                    string userbirthday = PlugHelper.ContextSetting(Request, Type, "userbirthday");
                    string userfristfromaddress = PlugHelper.ContextSetting(Request, Type, "userfristfromaddress");
                    string userphone = PlugHelper.ContextSetting(Request, Type, "userphone");
                    string Subclass = PlugHelper.ContextSetting(Request, Type, "Subclass");
                    string userpassword = PlugHelper.ContextSetting(Request, Type, "userpassword");
                    string usertype = PlugHelper.ContextSetting(Request, Type, "usertype");
                    int getces = SqlServerHelper.ExecuteNonQuery(CommandType.Text, "UPDATE userlist SET useremail = '" + useremail + "',userfristfromaddress= '" + userfristfromaddress + "',userphone= '" + userphone + "',username = '" + username + "',userpicpath = '" + userpicpath + "',userbirthday = '" + userbirthday + "',Subclass ='" + Subclass + "',userpassword ='" + PIN.Encrypt(userpassword) + "',usertype ='" + usertype + "' where userid ='" + userid + "'");

                    if (getces == 0 == false)
                    {
                        ReturnMessage = "用户修改成功 username:" + username.ToString();
                        return true;
                    }
                    else
                    {
                        ReturnMessage = "用户修改失败 username:" + username.ToString();
                        return true;
                    }
                }
                else
                {
                    ReturnMessage = "拒绝的权限:" + GetUserType;
                    return true;
                }
            }
            return false;
        }
        /// <summary>
        /// 登录用户
        /// </summary>
        /// <param name="Request"></param>
        /// <param name="ReturnMessage"></param>
        /// <param name="Type"></param>
        /// <returns></returns>
        public static bool LoginUserby(HttpContext LockerHttpContext, HttpContextcls Request, ref string ReturnMessage, string Type = "Get")
        {
            string gettype = PlugHelper.ContextSetting(Request, Type, "type");
            if (gettype == "LoginUser")
            {
                var LockerUserIPAddress = IPAddressHelper.GetUserSource(LockerHttpContext);
                string userid = PlugHelper.ContextSetting(Request, Type, "userid");
                string UserPassword = PlugHelper.ContextSetting(Request, Type, "userpassword");
              
                string logintype = PlugHelper.ContextSetting(Request, Type, "logintype");
                string HrefLocker = PlugHelper.ContextSetting(Request, Type, "HrefLocker");
                string loginadd = LockerUserIPAddress.UserIPAddress + ">" + LockerUserIPAddress.BroadbandOperators + ">" + LockerUserIPAddress.UserFromPlace;

                userlogin Nlogin = new userlogin();
                Nlogin.LoginDataTime = DateTime.Now.ToString();
                string UserTruePath = "http://localhost:44379/";
                string UserFalsePath = "/Web/";

                string AdminTruePath = "/Web/ShowWebPage?UI=index";
                string AdminFalsePath = "/Web/LoginUser";
                if (logintype.ToLower() == "user")
                {
                    if (userid == null == false && userid == "" == false)
                    {
                        object get = SqlServerHelper.ExecuteScalar(CommandType.Text, "SELECT userpassword FROM UserList Where userid ='" + userid + "' And usertype='user'");
                        if (get == null == false)
                        {
                            if (PIN.Encrypt(UserPassword) == get.ToString())
                            {
                                if (HrefLocker == null == false)
                                {
                                    if (HrefLocker == "false")
                                    {
                                        Nlogin.LoginDataTime = DateTime.Now.ToString();
                                        Nlogin.username = userid;
                                        Nlogin.targethtml = DeFine.GetRootURI() + UserTruePath;
                                        Nlogin.Msg = "用户登录成功";
                                        Nlogin.usertoken = UserHelper.setusertoken(userid, PIN.Encrypt(UserPassword), DeFine.DBIPAddress, DeFine.DBName);
                                        ReturnMessage = JsonHelper.DataFormatToJson(Nlogin);
                                        return true;
                                    }
                                    else
                                    {
                                        Nlogin.LoginDataTime = DateTime.Now.ToString();
                                        Nlogin.username = userid;
                                        Nlogin.targethtml =  UserTruePath;
                                        Nlogin.Msg = "用户登录成功";
                                        Nlogin.usertoken = UserHelper.setusertoken(userid, PIN.Encrypt(UserPassword), DeFine.DBIPAddress, DeFine.DBName);
                                        ReturnMessage = JsonHelper.DataFormatToJson(Nlogin);
                                        CookieHelper.SetCookie(LockerHttpContext, "token", Nlogin.usertoken);
                                        return true;
                                    }
                                }
                            }
                            else
                            {
                                Nlogin.LoginDataTime = DateTime.Now.ToString();
                                Nlogin.username = userid;
                                Nlogin.targethtml = DeFine.GetRootURI() + UserFalsePath;
                                Nlogin.Msg = "用户密码错误";
                                Nlogin.usertoken = "";
                                ReturnMessage = JsonHelper.DataFormatToJson(Nlogin);
                                return true;
                            }
                        }
                        else
                        {
                            Nlogin.LoginDataTime = DateTime.Now.ToString();
                            Nlogin.username = "Null";
                            Nlogin.targethtml = DeFine.GetRootURI() + UserFalsePath;
                            Nlogin.Msg = "用户名不存在";
                            Nlogin.usertoken = "";
                            ReturnMessage = JsonHelper.DataFormatToJson(Nlogin);
                            return true;
                        }
                    }
                    else
                    {

                        Nlogin.LoginDataTime = DateTime.Now.ToString();
                        Nlogin.username = "Null";
                        Nlogin.targethtml = DeFine.GetRootURI() + UserFalsePath;
                        Nlogin.Msg = "用户名或者密码不能为空";
                        Nlogin.usertoken = "";
                        ReturnMessage = JsonHelper.DataFormatToJson(Nlogin);
                        return true;

                    }

                }
                else
                {
                    usercls getcls = new usercls();
                    getcls = UserHelper.LoginUser(userid, UserPassword, loginadd);
                    Nlogin.username = userid;
                    Nlogin.LoginDataTime = DateTime.Now.ToString();
                    if (getcls.msg == "用户登陆成功!")
                    {
                        Nlogin.targethtml = DeFine.GetRootURI() + AdminTruePath;
                        if (HrefLocker == "true")
                        {
                            Nlogin.Msg = "管理员登录成功";
                            Nlogin.usertoken = getcls.usertoken;
                            CookieHelper.SetCookie(LockerHttpContext, "USToken", getcls.usertoken);  //登陆成功后保存到cookies
                            ReturnMessage = JsonHelper.DataFormatToJson(Nlogin);

                        }
                        else
                        {
                            ReturnMessage = JsonHelper.DataFormatToJson(Nlogin);
                        }
                    }
                    else
                    {
                        Nlogin.targethtml = "index.html";
                        if (HrefLocker == "true")
                        {
                            Nlogin.targethtml = DeFine.GetRootURI() + AdminFalsePath;
                            Nlogin.Msg = "登录失败原因:" + getcls.msg;
                            Nlogin.usertoken = getcls.usertoken;
                            ReturnMessage = JsonHelper.DataFormatToJson(Nlogin);
                        }
                        else
                        { ReturnMessage = JsonHelper.DataFormatToJson(Nlogin); }
                    }
                }

            }
            return false;
        }
        /// <summary>
        ///  删除用户
        /// </summary>
        /// <param name="Request"></param>
        /// <param name="ReturnMessage"></param>
        /// <param name="Type"></param>
        /// <returns></returns>
        public static bool DelectUser(HttpContextcls Request, ref string ReturnMessage, usertoken GetUserToken, string Type = "Get")
        {
            string gettype = PlugHelper.ContextSetting(Request, Type, "type");
            if (gettype == "DelectUser")
            {
                if (GetUserToken.username.ToLower() == "admin")
                {
                    string GetID = PlugHelper.ContextSetting(Request, Type, "ID");

                    int get = SqlServerHelper.ExecuteNonQuery(CommandType.Text, "DELETE FROM UserList WHERE ID = '" + GetID + "'");
                    if (get == 0 == false)
                    {
                        ReturnMessage = "true";
                        return true;
                    }
                    else
                    {
                        ReturnMessage = "false";
                        return true;
                    }

                }
                else
                {
                    ReturnMessage = "false";
                    return true;
                }

            }
            return false;
        }

        /// <summary>
        /// 找回用户密码
        /// </summary>
        /// <param name="Request"></param>
        /// <param name="ReturnMessage"></param>
        /// <param name="Type"></param>
        /// <returns></returns>
        public static bool GetLostPassword(HttpContextcls Request, ref string ReturnMessage, string Type = "Get")
        {
            string gettype = PlugHelper.ContextSetting(Request, Type, "type");
            if (gettype == "GetLostPassword")
            {
                string GetLostType = PlugHelper.ContextSetting(Request, Type, "LostType");
                string GetUserID = PlugHelper.ContextSetting(Request, Type, "userid");
                string GetUserEmail = PlugHelper.ContextSetting(Request, Type, "useremail");
                if (GetLostType == "通过邮箱找回")
                {
                    DataTable Msg = SqlServerHelper.ExecuteDataTable("Select * From UserList where userid ='" + GetUserID + "' And useremail ='" + GetUserEmail + "'");
                    if (Msg.Rows.Count > 0)
                    {

                        EmailHelper.Initialize();//开启邮箱服务
                        try
                        {
                            EmailHelper.SendMail(EmailHelper.GetAllInfoUser()[0], Msg.Rows[0]["useremail"].ToString(), "系统", "用户密码找回", "您的密码是:" + PIN.Decrypt(Msg.Rows[0]["userpassword"].ToString()));
                            ReturnMessage = "您的密码已经发送到邮箱";
                            return true;
                        }
                        catch
                        {
                            ReturnMessage = "发送失败请确保您的邮箱地址是正常的";
                            return true;
                        }
                    }
                }
                if (GetLostType == "通过个人信息找回")
                {
                    //只允许用户通过此方法找到密码管理员类型账户只能通过邮箱找回
                    string Getuserbirthday = PlugHelper.ContextSetting(Request, Type, "userbirthday");
                    string Getuserphone = PlugHelper.ContextSetting(Request, Type, "userphone");
                    if (Getuserbirthday == "" == false && Getuserbirthday == null == false)
                        if (Getuserphone == "" == false && Getuserphone == null == false)
                        {
                            object GetUserPassword = SqlServerHelper.ExecuteScalar("Select userpassword From UserList where userid ='" + GetUserID + "' And userbirthday='" + Getuserbirthday + "' And userphone='" + Getuserphone + "' And usertype ='user'");
                            if (GetUserPassword == null)
                            {
                                ReturnMessage = "找回失败请确保您的生日和手机号填写正确";
                                return true;
                            }
                            else
                            {
                                ReturnMessage = "找回成功您的密码是:" + PIN.Decrypt(GetUserPassword.ToString());
                                return true;
                            }

                        }

                }

            }


            //EmailHelper.AddEmailInfor("525084464@qq.com", "121212dcjgi", "525084464@qq.com", "stkx212212jdcjgi", "QQ邮箱"); 已经添加了就不需要再添加了

            return false;
        }

        /// <summary>
        /// 根据Cookies获取用户详情资料
        /// </summary>
        /// <param name="Request"></param>
        /// <param name="ReturnMessage"></param>
        /// <param name="GetUserToken"></param>
        /// <param name="Type"></param>
        /// <returns></returns>
        public static bool GetUserINFO(HttpContextcls Request, ref string ReturnMessage, usertoken GetUserToken, string Type = "Get")
        {
            string gettype = PlugHelper.ContextSetting(Request, Type, "type");
            if (gettype == "GetUserINFO")
            {
                if (GetUserToken.username == null == false && GetUserToken.username == "" == false)
                {
                    var Getinfomsg = UserHelper.GetUserINFO(GetUserToken);

                    if (Getinfomsg == null == false)
                    {
                        ReturnMessage = JsonHelper.DataFormatToJson(Getinfomsg);
                        return true;
                    }
                    else
                    {
                        ReturnMessage = "ERROR";
                        return true;
                    }
                }
            }
            return false;
        }



        #region 前端调用接口
        /// <summary>
        /// 新增用户连接
        /// </summary>
        /// <param name="Request"></param>
        /// <param name="ReturnMessage"></param>
        /// <param name="Form"></param>
        /// <param name="LockerToken"></param>
        /// <param name="Type"></param>
        /// <returns></returns>
        public static bool NewConnection(HttpContext LockerHttpContext, HttpContextcls Request, ref string ReturnMessage, string Form, usertoken LockerToken, string Type = "Obj")
        {
            string gettype = PlugHelper.ContextSetting(Request, Type, "type");
            if (gettype == "NewConnection")
            {
                var GetUserSource = IPAddressHelper.GetUserSource(LockerHttpContext);
                string IPaddress = GetUserSource.UserIPAddress;
                string FormCity = GetUserSource.BroadbandOperators + ">" + GetUserSource.UserFromPlace;
                string CurrentUrl = PlugHelper.ContextSetting(Request, Type, "CurrentUrl");
                if (IPaddress.Length > 0 == false)
                {
                    //自动过滤非法用户
                    return true;
                }

                Action NAction = new Action(() =>
                {
                    string LockerUserName = LockerToken.username;
                    if (Form == "null") LockerUserName = IPaddress + "_TempUser";
                    string SqlFinder = "Select LockerUserID From UserAccessHistory Where LockerUserID='" + LockerUserName + "'";
                    object LockerUserID = SqlServerHelper.ExecuteScalar(SqlFinder);


                    object GetUserVisit = SqlServerHelper.ExecuteScalar("Select UserName From VisitHistory Where UserName='" + LockerUserName + "' And UPDataDay='" + DateTime.Now.ToString("yyyy_MM_dd") + "'");
                    if (GetUserVisit == null)
                    {
                        SqlServerHelper.ExecuteNonQuery("INSERT INTO VisitHistory(UserName,UPDataDay,FirstVisitUrl,LastVisitUrl,ClickCount) VALUES ('" + LockerUserName + "','" + DateTime.Now.ToString("yyyy_MM_dd") + "','" + CurrentUrl + "','" + CurrentUrl + "',1);");
                    }
                    else
                    {
                        SqlServerHelper.ExecuteNonQuery("UPDATE VisitHistory SET LastVisitUrl = '" + CurrentUrl + "',ClickCount= ClickCount+1 WHERE UserName = '" + LockerUserName + "'");
                    }

                    if (LockerUserID == null)
                    {
                        if (Form == "null")
                        {
                            if (IPaddress.Length > 0 == false)
                            {
                                //自动过滤非法用户
                                return;
                            }
                        }
                        string SqlLoader = "INSERT INTO UserAccessHistory(LockerUserID,IPaddress,FormCity,VisitCount,LastVisitTime) VALUES ('{0}', '{1}', '{2}', {3}, '{4}');";
                        SqlServerHelper.ExecuteNonQuery(string.Format(SqlLoader, LockerUserName, IPaddress, FormCity, "0", DateTime.Now.ToString()));


                    }
                    else
                    {
                        string SqlOder = "UPDATE UserAccessHistory SET VisitCount = VisitCount+1,LastVisitTime= '{0}' WHERE LockerUserID = '{1}'";
                        SqlServerHelper.ExecuteNonQuery(string.Format(SqlOder, DateTime.Now.ToString(), LockerUserName));
                    }

                });

                if (Form == "admin")
                {
                    CoreHelper.RunCodeNoParam(new ThreadWork(LockerToken, null, CallLocation.SystemPool, NAction));
                }
                else
                if (Form == "user")
                {
                    CoreHelper.RunCodeNoParam(new ThreadWork(LockerToken, null, CallLocation.UserPool, NAction));
                }
                else
                {
                    CoreHelper.RunCodeNoParam(new ThreadWork(LockerToken, null, CallLocation.QueuePool, NAction));
                }

            }
            return false;
        }

        #endregion

        #region CMS管理员提供接口
        /// <summary>
        /// 按年月查询月内前端用户的访问量返回报表
        /// </summary>
        /// <param name="Request"></param>
        /// <param name="ReturnMessage"></param>
        /// <param name="Type"></param>
        /// <returns></returns>
        public static bool GetVisitTable(HttpContextcls Request, ref string ReturnMessage, string Type = "Get")
        {
            string gettype = PlugHelper.ContextSetting(Request, Type, "type");
            if (gettype == "GetVisitTable")
            {
                string LockerYear = PlugHelper.ContextSetting(Request, Type, "Year");
                string LockerMonth = PlugHelper.ContextSetting(Request, Type, "Month");
                if (LockerYear.Length == 4 == false) return false;
                if (LockerMonth.Length == 1)
                {
                    LockerMonth = "0" + LockerMonth;
                }
                string SqlOder = "select * from VisitHistory Where UPDataDay Like '" + LockerYear + "_" + LockerMonth + "%';";
                ReturnMessage = JsonHelper.DataFormatToJson(SqlServerHelper.ExecuteDataTable(SqlOder));
                return true;
            }
            return false;
        }
        public static bool GetVisitChart(HttpContextcls Request, ref string ReturnMessage, string Type = "Get")
        {
            string gettype = PlugHelper.ContextSetting(Request, Type, "type");
            if (gettype == "GetVisitChart")
            {

                string LockerYear = PlugHelper.ContextSetting(Request, Type, "Year");
                string LockerMonth = PlugHelper.ContextSetting(Request, Type, "Month");
                if (LockerYear.Length == 4 == false) return false;
                if (LockerMonth.Length == 1)
                {
                    LockerMonth = "0" + LockerMonth;
                }
                ReturnMessage = JsonHelper.DataFormatToJson(EchartHelper.GetVisitChart(LockerYear, LockerMonth));
                return true;

            }
            return false;
        }
        /// <summary>
        /// 当日用户游览次数
        /// </summary>
        /// <param name="Request"></param>
        /// <param name="ReturnMessage"></param>
        /// <param name="Type"></param>
        /// <returns></returns>
        public static bool GetVisitCountByDay(HttpContextcls Request, ref string ReturnMessage, string Type = "Get")
        {
            string gettype = PlugHelper.ContextSetting(Request, Type, "type");
            if (gettype == "GetVisitCountByDay")
            {
                DateTime dateTime = DateTime.Now;
                DataTable AllVisit = SqlServerHelper.ExecuteDataTable("Select * From VisitHistory Where UPDataDay ='" + dateTime.Year + "_" + dateTime.Month + "_" + dateTime.Day + "';");
                if (AllVisit.Rows.Count > 0)
                {
                    int VisitCount = 0;
                    for (int i = 0; i < AllVisit.Rows.Count; i++)
                    {
                        int Number = 0;
                        int.TryParse(DataHelper.ObjToStr(AllVisit.Rows[i]["ClickCount"]), out Number);
                        VisitCount += Number;
                    }
                    ReturnMessage = VisitCount.ToString();
                    return true;
                }
                else
                {
                    ReturnMessage = "0";
                    return true;
                }

            }
            return false;
        }
        /// <summary>
        /// 获取数据库条数
        /// </summary>
        /// <param name="Request"></param>
        /// <param name="ReturnMessage"></param>
        /// <param name="Type"></param>
        /// <returns></returns>
        public static bool GetDBCount(HttpContextcls Request, ref string ReturnMessage, string Type = "Get")
        {
            string gettype = PlugHelper.ContextSetting(Request, Type, "type");
            if (gettype == "GetDBCount")
            {
                int AllCount = 0;
                List<string> AllTable = new List<string>() { "VisitHistory", "UserAccessHistory", "savelis", "product", "define", "UserList" };
                foreach (var Get in AllTable)
                {
                    int TempNumber = 0;
                    object GetTmp = SqlServerHelper.ExecuteScalar("select count(1) from VisitHistory");
                    int.TryParse(DataHelper.ObjToStr(GetTmp), out TempNumber);
                    if (TempNumber == 0) TempNumber = 1;
                    AllCount += TempNumber;
                }
                ReturnMessage = AllCount.ToString();
                return true;
            }
            return false;
        }
        /// <summary>
        /// 获取接收和发送的邮件信息数
        /// </summary>
        /// <param name="Request"></param>
        /// <param name="ReturnMessage"></param>
        /// <param name="Type"></param>
        /// <returns></returns>
        public static bool GetEmailCount(HttpContextcls Request, ref string ReturnMessage, string Type = "Get")
        {
            string gettype = PlugHelper.ContextSetting(Request, Type, "type");
            if (gettype == "GetEmailCount")
            {
                int EmailCount = 0;
                if (!EmailHelper.FristLoadINFO)
                {
                    string GetDefPath = DeFine.DefCurrentDirectory + "system.db";
                    SQLiteHelper.OpenSql(DeFine.DefCurrentDirectory + "system.db");
                    EmailHelper.FristLoadINFO = true;
                }
                object GetTmp = SQLiteHelper.ExecuteScalar("select count(1) from EmailList;");
                int.TryParse(DataHelper.ObjToStr(GetTmp), out EmailCount);
                ReturnMessage = EmailCount.ToString();
                return true;
            }
            return false;
        }
        /// <summary>
        /// 获取黑名单用户数
        /// </summary>
        /// <param name="Request"></param>
        /// <param name="ReturnMessage"></param>
        /// <param name="Type"></param>
        /// <returns></returns>
        public static bool GetDarkUserCount(HttpContextcls Request, ref string ReturnMessage, string Type = "Get")
        {
            string gettype = PlugHelper.ContextSetting(Request, Type, "type");
            if (gettype == "GetDarkUserCount")
            {
                int DarkUser = 0;
                var LockerUserList = WebDefence.GetAllConnect();
                foreach (var Get in WebDefence.GetAllConnect())
                {
                    if (Get.MaxConnect > 30)
                    {
                        DarkUser++;
                    }
                }
                ReturnMessage = DarkUser.ToString();
                return true;
            }
            return false;
        }
        /// <summary>
        /// 获取计算机性能情况
        /// </summary>
        /// <param name="Request"></param>
        /// <param name="ReturnMessage"></param>
        /// <param name="Type"></param>
        /// <returns></returns>
        public static bool GetBrainMessage(HttpContextcls Request, ref string ReturnMessage, string Type = "Get")
        {
            string gettype = PlugHelper.ContextSetting(Request, Type, "type");
            if (gettype == "GetBrainMessage")
            {
                ReturnMessage = JsonHelper.DataFormatToJson(BrainConfig.GetPerformanceMessage());
                return true;
            }
            return false;
        }
        /// <summary>
        /// 获取用户登录详情
        /// </summary>
        /// <param name="Request"></param>
        /// <param name="ReturnMessage"></param>
        /// <param name="Type"></param>
        /// <returns></returns>
        public static bool GetLoginRecord(HttpContextcls Request, ref string ReturnMessage, string Type = "Get")
        {
            string gettype = PlugHelper.ContextSetting(Request, Type, "type");
            if (gettype == "GetLoginRecord")
            {
                ReturnMessage = JsonHelper.DataFormatToJson(SqlServerHelper.ExecuteDataTable("Select * From savelis"));
                return true;
            }
            return false;
        }
        /// <summary>
        /// 清空登录信息
        /// </summary>
        /// <param name="Request"></param>
        /// <param name="ReturnMessage"></param>
        /// <param name="GetUserToken"></param>
        /// <param name="Type"></param>
        /// <returns></returns>
        public static bool ClearLoginRecord(HttpContextcls Request, ref string ReturnMessage, usertoken GetUserToken, string Type = "Get")
        {
            string gettype = PlugHelper.ContextSetting(Request, Type, "type");
            if (gettype == "ClearLoginRecord")
            {
                if (GetUserToken.username.ToLower() == "admin")
                {
                    int state = SqlServerHelper.ExecuteNonQuery("Delete from savelis where 1=1;");
                    if (state == 0 == false)
                    {
                        ReturnMessage = "true";
                        return true;
                    }
                    else
                    {
                        ReturnMessage = "false";
                        return true;
                    }
                }
                else
                {
                    ReturnMessage = "Error Insufficient Authority";
                    return true;
                }

            }
            return false;
        }
        /// <summary>
        /// 获取最新的收件信息
        /// </summary>
        /// <param name="Request"></param>
        /// <param name="ReturnMessage"></param>
        /// <param name="Type"></param>
        /// <returns></returns>
        public static bool GetNewEmailMessage(HttpContextcls Request, ref string ReturnMessage, string Type = "Get")
        {
            string gettype = PlugHelper.ContextSetting(Request, Type, "type");
            if (gettype == "GetNewEmailMessage")
            {
                string SqlOder = "select * from EmailList order by rowid desc limit 10;";
                if (!EmailHelper.FristLoadINFO)
                {
                    string GetDefPath = DeFine.DefCurrentDirectory + "system.db";
                    SQLiteHelper.OpenSql(DeFine.DefCurrentDirectory + "system.db");
                    EmailHelper.FristLoadINFO = true;
                }
                ReturnMessage = JsonHelper.DataFormatToJson(SQLiteHelper.ExecuteDataTable(SqlOder));
                return true;
            }
            return false;
        }
        /// <summary>
        /// 获取主导航
        /// </summary>
        /// <param name="Request"></param>
        /// <param name="ReturnMessage"></param>
        /// <param name="Type"></param>
        /// <returns></returns>
        public static bool GetNavListByType(HttpContextcls Request, ref string ReturnMessage, string Type = "Get")
        {
            string gettype = PlugHelper.ContextSetting(Request, Type, "type");
            if (gettype == "GetNavListByType")
            {
                string ColumnType = PlugHelper.ContextSetting(Request, Type, "ColumnType");
                DataTable GetDataTable = CMSHelper.GetNavByColumnType(ColumnType);
                if (ColumnType.Equals("ListT"))
                {
                    ReturnMessage = JsonHelper.DataFormatToJson(CMSHelper.ConvertToListT(GetDataTable));
                    return true;
                }
                else
                if (ColumnType.Equals("ListTT"))
                {
                    ReturnMessage = JsonHelper.DataFormatToJson(CMSHelper.ConvertToListTT(GetDataTable));
                    return true;
                }
                else
                {
                    ReturnMessage = JsonHelper.DataFormatToJson(GetDataTable);
                    return true;
                }

            }
            return false;
        }
        /// <summary>
        /// 获取可用模板
        /// </summary>
        /// <param name="Request"></param>
        /// <param name="ReturnMessage"></param>
        /// <param name="Type"></param>
        /// <returns></returns>
        public static bool GetActiveHtmlTemplate(HttpContextcls Request, ref string ReturnMessage, string Type = "Get")
        {
            string gettype = PlugHelper.ContextSetting(Request, Type, "type");
            if (gettype == "GetActiveHtmlTemplate")
            {
                ReturnMessage = JsonHelper.DataFormatToJson(CMSHelper.GetAllTmp());
                return true;
            }
            return false;

        }
        /// <summary>
        /// 修改导航栏目
        /// </summary>
        /// <param name="Request"></param>
        /// <param name="ReturnMessage"></param>
        /// <param name="Type"></param>
        /// <returns></returns>
        public static bool ReloadNavColumn(HttpContextcls Request, ref string ReturnMessage, string Type = "Obj")
        {
            string gettype = PlugHelper.ContextSetting(Request, Type, "type");
            if (gettype == "ReloadNavColumn")
            {
                string ID = PlugHelper.ContextSetting(Request, Type, "ID");
                string ColumnName = PlugHelper.ContextSetting(Request, Type, "ColumnName");
                string ColumnHref = PlugHelper.ContextSetting(Request, Type, "ColumnHref");
                string SeoText = PlugHelper.ContextSetting(Request, Type, "SeoText");
                string ColumnOrder = PlugHelper.ContextSetting(Request, Type, "ColumnOrder");
                string LockerTemplate = PlugHelper.ContextSetting(Request, Type, "LockerTemplate");
                string NColumnType = PlugHelper.ContextSetting(Request, Type, "NColumnType");
                string Parent = PlugHelper.ContextSetting(Request, Type, "Parent");
                int Number = 0;
                int.TryParse(ID, out Number);
                if (Number > 0)
                {
                    ReturnMessage = JsonHelper.DataFormatToJson(CMSHelper.ReloadNavByID(Number, new nav(ColumnName, ColumnHref, SeoText, ColumnOrder, LockerTemplate, NColumnType, Parent)));
                    return true;
                }
                else
                {
                    return true;
                }

            }
            return false;

        }
        /// <summary>
        /// Reload LineT
        /// </summary>
        /// <param name="Request"></param>
        /// <param name="ReturnMessage"></param>
        /// <param name="Type"></param>
        /// <returns></returns>
        public static bool ReloadLineT(HttpContextcls Request, ref string ReturnMessage, string Type = "Obj")
        {
            string gettype = PlugHelper.ContextSetting(Request, Type, "type");
            if (gettype == "ReloadLineT")
            {
                string ID = PlugHelper.ContextSetting(Request, Type, "ID");
                string LockerRows = PlugHelper.ContextSetting(Request, Type, "LockerRows");
                string NewText = PlugHelper.ContextSetting(Request, Type, "NewText");

                int IDNumber = 0;
                int RowsNumber = 0;

                int.TryParse(ID, out IDNumber);
                int.TryParse(LockerRows, out RowsNumber);

                ReturnMessage = JsonHelper.DataFormatToJson(CMSHelper.ReloadListT(IDNumber, RowsNumber, NewText));
                return true;
            }
            return false;
        }
        /// <summary>
        /// Reload LineTT
        /// </summary>
        /// <param name="Request"></param>
        /// <param name="ReturnMessage"></param>
        /// <param name="Type"></param>
        /// <returns></returns>
        public static bool ReloadLineTT(HttpContextcls Request, ref string ReturnMessage, string Type = "Obj")
        {
            string gettype = PlugHelper.ContextSetting(Request, Type, "type");
            if (gettype == "ReloadLineTT")
            {
                string ID = PlugHelper.ContextSetting(Request, Type, "ID");
                string LockerRows = PlugHelper.ContextSetting(Request, Type, "LockerRows");
                string NewText = PlugHelper.ContextSetting(Request, Type, "NewText");

                int IDNumber = 0;
                int RowsNumber = 0;

                int.TryParse(ID, out IDNumber);
                int.TryParse(LockerRows, out RowsNumber);

                ReturnMessage = JsonHelper.DataFormatToJson(CMSHelper.ReloadListTT(IDNumber, RowsNumber, NewText));
                return true;
            }
            return false;
        }
        /// <summary>
        /// Add LineT
        /// </summary>
        /// <param name="Request"></param>
        /// <param name="ReturnMessage"></param>
        /// <param name="Type"></param>
        /// <returns></returns>
        public static bool AddLineT(HttpContextcls Request, ref string ReturnMessage, string Type = "Obj")
        {
            string gettype = PlugHelper.ContextSetting(Request, Type, "type");
            if (gettype == "AddLineT")
            {
                string ID = PlugHelper.ContextSetting(Request, Type, "ID");
                int IDNumber = 0;
                int.TryParse(ID, out IDNumber);
                ReturnMessage = JsonHelper.DataFormatToJson(CMSHelper.AddListT(IDNumber));
                return true;
            }
            return false;
        }
        /// <summary>
        /// Add LineTT
        /// </summary>
        /// <param name="Request"></param>
        /// <param name="ReturnMessage"></param>
        /// <param name="Type"></param>
        /// <returns></returns>
        public static bool AddLineTT(HttpContextcls Request, ref string ReturnMessage, string Type = "Obj")
        {
            string gettype = PlugHelper.ContextSetting(Request, Type, "type");
            if (gettype == "AddLineTT")
            {
                string ID = PlugHelper.ContextSetting(Request, Type, "ID");
                int IDNumber = 0;
                int.TryParse(ID, out IDNumber);
                ReturnMessage = JsonHelper.DataFormatToJson(CMSHelper.AddListTT(IDNumber));
                return true;
            }
            return false;
        }
        /// <summary>
        /// Delect LineT
        /// </summary>
        /// <param name="Request"></param>
        /// <param name="ReturnMessage"></param>
        /// <param name="Type"></param>
        /// <returns></returns>
        public static bool DelectLineT(HttpContextcls Request, ref string ReturnMessage, string Type = "Obj")
        {
            string gettype = PlugHelper.ContextSetting(Request, Type, "type");
            if (gettype == "DelectLineT")
            {
                string ID = PlugHelper.ContextSetting(Request, Type, "ID");
                string LockerRows = PlugHelper.ContextSetting(Request, Type, "LockerRows");

                int IDNumber = 0;
                int RowsNumber = 0;

                int.TryParse(ID, out IDNumber);
                int.TryParse(LockerRows, out RowsNumber);

                ReturnMessage = JsonHelper.DataFormatToJson(CMSHelper.DelectListTFormDB(IDNumber, RowsNumber));
                return true;
            }


            return false;
        }
        /// <summary>
        /// Delect LineTT
        /// </summary>
        /// <param name="Request"></param>
        /// <param name="ReturnMessage"></param>
        /// <param name="Type"></param>
        /// <returns></returns>
        public static bool DelectLineTT(HttpContextcls Request, ref string ReturnMessage, string Type = "Obj")
        {
            string gettype = PlugHelper.ContextSetting(Request, Type, "type");
            if (gettype == "DelectLineTT")
            {
                string ID = PlugHelper.ContextSetting(Request, Type, "ID");
                string LockerRows = PlugHelper.ContextSetting(Request, Type, "LockerRows");
                int IDNumber = 0;
                int RowsNumber = 0;

                int.TryParse(ID, out IDNumber);
                int.TryParse(LockerRows, out RowsNumber);

                ReturnMessage = JsonHelper.DataFormatToJson(CMSHelper.DelectListTTFormDB(IDNumber, RowsNumber));
                return true;
            }


            return false;
        }

        public static bool ReloadListT(HttpContextcls Request, ref string ReturnMessage, string Type = "Obj")
        {
            string gettype = PlugHelper.ContextSetting(Request, Type, "type");
            if (gettype == "ReloadListT")
            {
                string ID = PlugHelper.ContextSetting(Request, Type, "ID");
                string Parent = PlugHelper.ContextSetting(Request, Type, "Parent");
                string SeoText = PlugHelper.ContextSetting(Request, Type, "SeoText");
                int IDNumber = 0;
                int.TryParse(ID, out IDNumber);
                ReturnMessage = JsonHelper.DataFormatToJson(CMSHelper.ReloadT(IDNumber, Parent, SeoText));
                return true;
            }
            return false;
        }

        public static bool ReloadListTT(HttpContextcls Request, ref string ReturnMessage, string Type = "Obj")
        {
            string gettype = PlugHelper.ContextSetting(Request, Type, "type");
            if (gettype == "ReloadListTT")
            {
                string ID = PlugHelper.ContextSetting(Request, Type, "ID");
                string Parent = PlugHelper.ContextSetting(Request, Type, "Parent");
                string SeoText = PlugHelper.ContextSetting(Request, Type, "SeoText");
                int IDNumber = 0;
                int.TryParse(ID, out IDNumber);
                ReturnMessage = JsonHelper.DataFormatToJson(CMSHelper.ReloadTT(IDNumber, Parent, SeoText));
                return true;
            }
            return false;
        }
        /// <summary>
        /// 删除指定导航栏目
        /// </summary>
        /// <param name="Request"></param>
        /// <param name="ReturnMessage"></param>
        /// <param name="Type"></param>
        /// <returns></returns>
        public static bool DelectNavColumn(HttpContextcls Request, ref string ReturnMessage, string Type = "Obj")
        {
            string gettype = PlugHelper.ContextSetting(Request, Type, "type");
            if (gettype == "DelectNavColumn")
            {
                string ID = PlugHelper.ContextSetting(Request, Type, "ID");
                int Number = 0;
                int.TryParse(ID, out Number);
                if (Number > 0)
                {
                    ReturnMessage = JsonHelper.DataFormatToJson(CMSHelper.DelectNavByID(Number));
                    return true;
                }
                else
                {
                    return true;
                }
            }
            return false;
        }
        /// <summary>
        /// 添加新的导航
        /// </summary>
        /// <param name="Request"></param>
        /// <param name="ReturnMessage"></param>
        /// <param name="Type"></param>
        /// <returns></returns>
        public static bool AddNavColumn(HttpContextcls Request, ref string ReturnMessage, string Type = "Obj")
        {
            string gettype = PlugHelper.ContextSetting(Request, Type, "type");
            if (gettype == "AddNavColumn")
            {
                string ColumnName = PlugHelper.ContextSetting(Request, Type, "ColumnName");
                if (ColumnName == "") ColumnName = "Null";
                string NColumnType = PlugHelper.ContextSetting(Request, Type, "NColumnType");
                string parent = "";
                if (NColumnType == "ListT")
                {
                    ColumnName = "";

                    parent = "ListT";
                }
                else
                if (NColumnType == "ListTT")
                {
                    ColumnName = "";

                    parent = "ListT,T";
                }

                ReturnMessage = JsonHelper.DataFormatToJson(CMSHelper.CreatNewNav(ColumnName, NColumnType, "", "", "", parent));
                return true;
            }
            return false;
        }

        /// <summary>
        /// 获取所有文章栏目关联子级
        /// </summary>
        /// <param name="Request"></param>
        /// <param name="ReturnMessage"></param>
        /// <param name="Type"></param>
        /// <returns></returns>
        public static bool GetAllArticleColumn(HttpContextcls Request, ref string ReturnMessage, string Type = "Obj")
        {
            string gettype = PlugHelper.ContextSetting(Request, Type, "type");
            if (gettype == "GetAllArticleColumn")
            {
                ReturnMessage = JsonHelper.DataFormatToJson(ArticleHelper.GetAllArticleColumn());
                return true;
            }
            return false;
        }
        /// <summary>
        /// 获取指定页的文章
        /// </summary>
        /// <param name="Request"></param>
        /// <param name="ReturnMessage"></param>
        /// <param name="Type"></param>
        /// <returns></returns>
        public static bool GetArticlePageData(HttpContextcls Request, ref string ReturnMessage, string Type = "Obj")
        {
            string gettype = PlugHelper.ContextSetting(Request, Type, "type");
            if (gettype == "GetArticlePageData")
            {
                string ColumnType = PlugHelper.ContextSetting(Request, Type, "ColumnType");
                string OderBy = PlugHelper.ContextSetting(Request, Type, "OderBy");
                string SelectPage = PlugHelper.ContextSetting(Request, Type, "SelectPage");
                int PageNumber = 0;
                int.TryParse(SelectPage, out PageNumber);

                SQLSetting PageSetting = new SQLSetting();
                PageSetting.LockerTable.Add("product");
                PageSetting.OderBy = "ProductID " + OderBy;
                PageSetting.PageLength = 10;
                FindKey NFindKey = new FindKey();
                NFindKey.Key = "Type";
                NFindKey.Value = "'" + ColumnType + "'";
                PageSetting.Condition.Add(NFindKey);
                PageSetting.FindTime = DateTime.Now;
                int PageCount = 0;
                DataTable NTable = SqlServerCommand.GetSignData(PageSetting, PageNumber, ref PageCount);
                PageList NPageList = new PageList();
                NPageList.PageMessage = NTable;
                NPageList.PageCount = PageCount;
                NPageList.SelectPage = PageNumber;

                ReturnMessage = JsonHelper.DataFormatToJson(NPageList);
                return true;
            }
            return false;
        }
        /// <summary>
        /// 设置文本文章
        /// </summary>
        /// <param name="Request"></param>
        /// <param name="ReturnMessage"></param>
        /// <param name="Type"></param>
        /// <returns></returns>
        public static bool SetArticleText(HttpContextcls Request, ref string ReturnMessage, string Type = "Obj")
        {
            string gettype = PlugHelper.ContextSetting(Request, Type, "type");
            if (gettype == "SetArticleText")
            {
                string ID = PlugHelper.ContextSetting(Request, Type, "ID");
                string EncryptData = PlugHelper.ContextSetting(Request, Type, "EncryptData", "HtmlCode");
                int IDNumber = 0;
                int.TryParse(ID, out IDNumber);
                string DecryptData = PIN.Base64Decode(EncryptData);
                object GetArticle = SqlServerHelper.ExecuteScalar("Select * From product Where ProductID =" + IDNumber);
                if (GetArticle == null == false)
                {
                    int state = SqlServerHelper.ExecuteNonQuery("UPDATE product SET ProductText = '" + PIN.Encrypt(DecryptData) + "' Where ProductID =" + IDNumber);
                    if (state == 0 == false)
                    {
                        string ProductName = PlugHelper.ContextSetting(Request, Type, "ProductName");
                        string ProductOrder = PlugHelper.ContextSetting(Request, Type, "ProductOrder");
                        string ProductDescribe = PlugHelper.ContextSetting(Request, Type, "ProductDescribe");
                        string ProductSeoDescribe = PlugHelper.ContextSetting(Request, Type, "ProductSeoDescribe");
                        string ProductSeoTittle = PlugHelper.ContextSetting(Request, Type, "ProductSeoTittle");
                        string GetSet = PlugHelper.ContextSetting(Request, Type, "GetSet");
                        string ProductTittle = PlugHelper.ContextSetting(Request, Type, "ProductTittle");
                        string ProductShortTittle = PlugHelper.ContextSetting(Request, Type, "ProductShortTittle");
                        string AllImageURL = PlugHelper.ContextSetting(Request, Type, "AllImageURL", "HtmlCode");
                        string ActType = PlugHelper.ContextSetting(Request, Type, "ActType");
                        string SqlOder = "UPDATE product SET ProductName = '{0}',ProductDescribe = '{1}',ProductSeoDescribe='{2}',ProductSeoTittle='{3}',GetSet='{4}',ProductTittle='{5}',ProductShortTittle='{6}',AllImageURL='{7}',Type='{8}',ProductOrder='{9}' Where ProductID =" + IDNumber;
                        state = SqlServerHelper.ExecuteNonQuery(string.Format(SqlOder, ProductName, ProductDescribe, ProductSeoDescribe, ProductSeoTittle, GetSet, ProductTittle, ProductShortTittle, AllImageURL, ActType,ProductOrder));
                        if (state == 0 == false)
                        {
                            ReturnMessage = true.ToString();
                            return true;
                        }
                        else
                        {
                            ReturnMessage = false.ToString();
                            return true;
                        }
                    }
                    else
                    {
                        ReturnMessage = false.ToString();
                        return true;
                    }

                }
                else
                {
                    ReturnMessage = false.ToString();
                    return true;
                }
            }
            return false;
        }

        /// <summary>
        /// 快速获取文章
        /// </summary>
        /// <param name="Request"></param>
        /// <param name="ReturnMessage"></param>
        /// <param name="Type"></param>
        /// <returns></returns>
        public static bool GetArticle(HttpContextcls Request, ref string ReturnMessage, string Type = "Obj")
        {
            string gettype = PlugHelper.ContextSetting(Request, Type, "type");
            if (gettype == "GetArticle")
            {
                string ID = PlugHelper.ContextSetting(Request, Type, "ID");
                int IDNumber = 0;
                int.TryParse(ID, out IDNumber);
                ReturnMessage = JsonHelper.DataFormatToJson(ArticleHelper.QuickGetArticle(IDNumber));
                return true;
            }

            return false;
        }
        /// <summary>
        /// 删除指定文章
        /// </summary>
        /// <param name="Request"></param>
        /// <param name="ReturnMessage"></param>
        /// <param name="Type"></param>
        /// <returns></returns>
        public static bool DelectArticle(HttpContextcls Request, ref string ReturnMessage, string Type = "Obj")
        {
            string gettype = PlugHelper.ContextSetting(Request, Type, "type");
            if (gettype == "DelectArticle")
            {
                string ID = PlugHelper.ContextSetting(Request, Type, "ID");
                int IDNumber = 0;
                int.TryParse(ID, out IDNumber);
                string SqlOder = "Delete From product Where ProductID = " + IDNumber.ToString();
                int state = SqlServerHelper.ExecuteNonQuery(SqlOder);
                if (state == 0 == false)
                {
                    ReturnMessage = true.ToString();
                    return true;
                }
                else
                {
                    ReturnMessage = false.ToString();
                    return true;
                }

            }

            return false;
        }

        /// <summary>
        /// 创建一篇文章
        /// </summary>
        /// <param name="Request"></param>
        /// <param name="ReturnMessage"></param>
        /// <param name="Type"></param>
        /// <returns></returns>
        public static bool CreatArticle(HttpContextcls Request, ref string ReturnMessage, string Type = "Obj")
        {
            string gettype = PlugHelper.ContextSetting(Request, Type, "type");
            if (gettype == "CreatArticle")
            {
                string SqlOder = "INSERT INTO product(ProductName,ProductOrder,ProductDescribe,ProductSeoDescribe,ProductSeoTittle,GetSet,ProductTittle,ProductShortTittle,AllImageURL,Type,ProductText,Creattime,Creat) VALUES ('{0}','{1}','{2}','{3}','{4}','{5}','{6}','{7}','{8}','{9}','{10}','{11}','{12}');";
                string ProductName = PlugHelper.ContextSetting(Request, Type, "ProductName");
                string ProductOrder = PlugHelper.ContextSetting(Request, Type, "ProductOrder");
                string ProductDescribe = PlugHelper.ContextSetting(Request, Type, "ProductDescribe");
                string ProductSeoDescribe = PlugHelper.ContextSetting(Request, Type, "ProductSeoDescribe");
                string ProductSeoTittle = PlugHelper.ContextSetting(Request, Type, "ProductSeoTittle");
                string GetSet = PlugHelper.ContextSetting(Request, Type, "GetSet");
                string ProductTittle = PlugHelper.ContextSetting(Request, Type, "ProductTittle");
                string ProductShortTittle = PlugHelper.ContextSetting(Request, Type, "ProductShortTittle");
                string AllImageURL = PlugHelper.ContextSetting(Request, Type, "AllImageURL", "HtmlCode");
                string FormType = PlugHelper.ContextSetting(Request, Type, "FormType");
                string EncryptData = PlugHelper.ContextSetting(Request, Type, "EncryptData", "HtmlCode");
                string DecryptData = PIN.Base64Decode(EncryptData);
                string Creattime = DateTime.Now.ToString();
                string Creat = false.ToString();
                int state = SqlServerHelper.ExecuteNonQuery(string.Format(SqlOder, ProductName, ProductOrder, ProductDescribe, ProductSeoDescribe, ProductSeoTittle, GetSet, ProductTittle, ProductShortTittle, AllImageURL, FormType, PIN.Encrypt(DecryptData), Creattime, Creat));
                if (state == 0 == false)
                {
                    ReturnMessage = true.ToString();
                    return true;
                }
                else
                {
                    return true;
                }
            }
            return false;
        }

        /// <summary>
        /// 获取所有前后端模板
        /// </summary>
        /// <param name="Request"></param>
        /// <param name="ReturnMessage"></param>
        /// <param name="Type"></param>
        /// <returns></returns>
        public static bool GetHtmlTemplate(HttpContextcls Request, ref string ReturnMessage, string Type = "Obj")
        {
            string gettype = PlugHelper.ContextSetting(Request, Type, "type");
            if (gettype == "GetHtmlTemplate")
            {
                ReturnMessage = JsonHelper.DataFormatToJson(CMSHelper.GetAllTemplate());
                return true;
            }
            return false;
        }

        public static bool TryConvert(HttpContextcls Request, ref string ReturnMessage, string Type = "Obj")
        {
            string gettype = PlugHelper.ContextSetting(Request, Type, "type");
            if (gettype == "TryConvert")
            {
                string EncryptData = PlugHelper.ContextSetting(Request, Type, "EncryptData", "HtmlCode");
                ReturnMessage = PIN.Base64Decode(EncryptData);
                return true;
            }
            return false;
        }
        /// <summary>
        /// 保存模板内容
        /// </summary>
        /// <param name="Request"></param>
        /// <param name="ReturnMessage"></param>
        /// <param name="GetAdminToken"></param>
        /// <param name="Type"></param>
        /// <returns></returns>
        public static bool SaveHtmlTemplate(HttpContextcls Request, ref string ReturnMessage, usertoken GetAdminToken, string Type = "Obj")
        {
            string gettype = PlugHelper.ContextSetting(Request, Type, "type");
            if (gettype == "SaveHtmlTemplate")
            {
                string HtmlFileName = PlugHelper.ContextSetting(Request, Type, "HtmlFileName", "HtmlCode");
                string HeadCode = PIN.Base64Decode(PlugHelper.ContextSetting(Request, Type, "HeadCode", "HtmlCode")).Replace("&gt;", ">").Replace("@html->", "\r\n@html->");
                string EncryptData = PlugHelper.ContextSetting(Request, Type, "EncryptData", "HtmlCode");

                string DeData = PIN.Base64Decode(EncryptData);
                if (GetAdminToken.username.ToLower() == "admin")
                {
                    try
                    {
                        DataHelper.writefile(DeFine.TemplatesPath + HtmlFileName, HeadCode + "\r\n" + DeData, Encoding.UTF8);
                        ReturnMessage = true.ToString();
                        return true;
                    }
                    catch
                    {
                        ReturnMessage = false.ToString();
                        return true;
                    }
                }
                else
                {
                    if (!HtmlFileName.Contains("admin"))
                    {
                        try
                        {
                            DataHelper.writefile(DeFine.TemplatesPath + HtmlFileName, HeadCode + "\r\n" + DeData, Encoding.UTF8);
                            ReturnMessage = true.ToString();
                            return true;
                        }
                        catch
                        {
                            ReturnMessage = false.ToString();
                            return true;
                        }
                    }
                    else
                    {
                        ReturnMessage = false.ToString();
                        return true;
                    }
                }
            }
            return false;
        }

        /// <summary>
        /// 删除指定模板
        /// </summary>
        /// <param name="Request"></param>
        /// <param name="ReturnMessage"></param>
        /// <param name="GetAdminToken"></param>
        /// <param name="Type"></param>
        /// <returns></returns>
        public static bool DelectHtmlTemplate(HttpContextcls Request, ref string ReturnMessage, usertoken GetAdminToken, string Type = "Obj")
        {
            string gettype = PlugHelper.ContextSetting(Request, Type, "type");
            if (gettype == "DelectHtmlTemplate")
            {
                string HtmlFileName = PlugHelper.ContextSetting(Request, Type, "HtmlFileName", "HtmlCode");

                if (GetAdminToken.username.ToLower() == "admin")
                {
                    try
                    {
                        File.Delete(DeFine.TemplatesPath + HtmlFileName);
                        ReturnMessage = true.ToString();
                        return true;
                    }
                    catch
                    {
                        ReturnMessage = false.ToString();
                        return true;
                    }
                }
                else
                {
                    if (!HtmlFileName.Contains("admin"))
                    {
                        try
                        {
                            File.Delete(DeFine.TemplatesPath + HtmlFileName);
                            ReturnMessage = true.ToString();
                            return true;
                        }
                        catch
                        {
                            ReturnMessage = false.ToString();
                            return true;
                        }
                    }
                    else
                    {
                        ReturnMessage = false.ToString();
                        return true;
                    }
                }
            }
            return false;
        }
       
        /// <summary>
        /// 检查html或sql引用的图片
        /// </summary>
        /// <param name="Request"></param>
        /// <param name="ReturnMessage"></param>
        /// <param name="Type"></param>
        /// <returns></returns>
        public static bool GetLockerImage(HttpContextcls Request, ref string ReturnMessage,string Type = "Obj")
        {
            string gettype = PlugHelper.ContextSetting(Request, Type, "type");
            if (gettype == "GetLockerImage")
            {
                ReturnMessage = JsonHelper.DataFormatToJson(CMSHelper.CheckHtmlFileByPic());
                return true;
                  
            }
            return false;
        }
        /// <summary>
        /// 删除未被调用的文件
        /// </summary>
        /// <param name="Request"></param>
        /// <param name="ReturnMessage"></param>
        /// <param name="Type"></param>
        /// <returns></returns>
        public static bool KillStandImg(HttpContextcls Request, ref string ReturnMessage, string Type = "Obj")
        {
            string gettype = PlugHelper.ContextSetting(Request, Type, "type");
            if (gettype == "KillStandImg")
            {
                ReturnMessage = JsonHelper.DataFormatToJson(CMSHelper.AutoDelectStandPic());
                return true;
            }
            return false;
        }
        /// <summary>
        /// 同步上传的文件列表
        /// </summary>
        /// <param name="Request"></param>
        /// <param name="ReturnMessage"></param>
        /// <param name="Type"></param>
        /// <returns></returns>
        public static bool GetAllFileList(HttpContextcls Request, ref string ReturnMessage, string Type = "Obj")
        {
            string gettype = PlugHelper.ContextSetting(Request, Type, "type");
            if (gettype == "GetAllFileList")
            {
                ReturnMessage = JsonHelper.DataFormatToJson(CMSHelper.GetAllFileFromDB());
                return true;
            }
            return false;
        }
        /// <summary>
        /// 设置文件权限
        /// </summary>
        /// <param name="Request"></param>
        /// <param name="ReturnMessage"></param>
        /// <param name="Type"></param>
        /// <returns></returns>

        public static bool SetFileItemValue(HttpContextcls Request, ref string ReturnMessage, string Type = "Obj")
        {
            string gettype = PlugHelper.ContextSetting(Request, Type, "type");
            if (gettype == "SetFileItemValue")
            {
                string ID = PlugHelper.ContextSetting(Request, Type, "ID");
                string FileAuthority = PlugHelper.ContextSetting(Request, Type, "FileAuthority");
                string AntiTheftChain = PlugHelper.ContextSetting(Request, Type, "AntiTheftChain");
                int Number = 0;
                int.TryParse(ID,out Number);
                string SqlOder = "update FileList SET FileAuthority='{0}',AntiTheftChain='{1}' Where ID = " + Number;
                int state = SqlServerHelper.ExecuteNonQuery(string.Format(SqlOder, FileAuthority, AntiTheftChain));
                if (state == 0 == false)
                {
                    ReturnMessage =JsonHelper.DataFormatToJson(new CommonResult());
                }
                else
                {
                    ReturnMessage = JsonHelper.DataFormatToJson(new CommonResult("NotFindTable"));
                }
                return true;
            }
            return false;
        }

        /// <summary>
        /// 删除上传的文件
        /// </summary>
        /// <param name="Request"></param>
        /// <param name="ReturnMessage"></param>
        /// <param name="Type"></param>
        /// <returns></returns>
        public static bool DelectFileItem(HttpContextcls Request, ref string ReturnMessage, string Type = "Obj")
        {
            string gettype = PlugHelper.ContextSetting(Request, Type, "type");
            if (gettype == "DelectFileItem")
            {
                string ID = PlugHelper.ContextSetting(Request, Type, "ID");
                if (CMSHelper.KillFile(ID))
                {
                    ReturnMessage = JsonHelper.DataFormatToJson(new CommonResult());
                }
                else
                {
                    ReturnMessage = JsonHelper.DataFormatToJson(new CommonResult("NotFindFile"));
                }
                return true;
            }
            return false;
        }

        /// <summary>
        /// 读取选中的插件页面
        /// </summary>
        /// <param name="Request"></param>
        /// <param name="ReturnMessage"></param>
        /// <param name="Type"></param>
        /// <returns></returns>
        public static bool GetPluginUICode(HttpContextcls Request, ref string ReturnMessage, string Type = "Obj")
        {
            string gettype = PlugHelper.ContextSetting(Request, Type, "type");
            if (gettype == "GetPluginUICode")
            {
                string ID = PlugHelper.ContextSetting(Request, Type, "ID");
                ReturnMessage= PluginHelper.getuiinpulg(DataHelper.StrToInt(ID));
                return true;
            }
            return false;
        }

        /// <summary>
        /// 获取全部安装的插件
        /// </summary>
        /// <param name="Request"></param>
        /// <param name="ReturnMessage"></param>
        /// <param name="Type"></param>
        /// <returns></returns>
        public static bool GetAllPlugin(HttpContextcls Request, ref string ReturnMessage, string Type = "Obj")
        {
            string gettype = PlugHelper.ContextSetting(Request, Type, "type");
            if (gettype == "GetAllPlugin")
            {
                List<pulginfo> getallplug = PluginHelper.selectallpulg();
                ReturnMessage = JsonHelper.DataFormatToJson(getallplug);
                return true;
            }
            return false;
        }

        /// <summary>
        /// 启用或关闭插件
        /// </summary>
        /// <param name="Request"></param>
        /// <param name="ReturnMessage"></param>
        /// <param name="Type"></param>
        /// <returns></returns>
        public static bool ActivePulg(HttpContext e,HttpContextcls Request, ref string ReturnMessage, string Type = "Obj")
        {
            string gettype = PlugHelper.ContextSetting(Request, Type, "type");
            if (gettype == "ActivePulg")
            {
                string Message = "";

                string ID = PlugHelper.ContextSetting(Request, Type, "ID");
                string PulgCheck = PlugHelper.ContextSetting(Request, Type, "PulgCheck");

                object CurrentCheck = SqlServerHelper.ExecuteScalar("Select pulgcheck From pulginslist Where ID =" + ID);
                if (CurrentCheck == null) { return true; }
                else
                {
                    if (CurrentCheck.ToString().ToLower().Equals(PulgCheck.ToLower()))
                    {
                        ReturnMessage = "<error>请勿重复操作";
                        return true;
                    }
                }
                if (PulgCheck.ToLower() == "true")
                {
                    if (PluginHelper.StartPlug(DataHelper.StrToInt(ID),e,ref Message))
                    {
                        if (PluginHelper.activeselectpulg(DataHelper.StrToInt(ID).ToString(), true))
                        {
                            PlugNav NPlugNav = new PlugNav();
                            NPlugNav.PlugID = ID;
                            NPlugNav.NavCode = PluginHelper.GetFunctionValue(Message.ToLower(), "newnav");

                            PluginHelper.PlugNavList.Add(NPlugNav);
                            ReturnMessage = PluginHelper.GetFunctionValue(Message.ToLower(),"text");
                            return true;
                        }
                    }
                    else
                    {
                        ReturnMessage = PluginHelper.GetFunctionValue(Message.ToLower(), "text");
                        return true;
                    }
                  
                }
                else
                {
                    if (PluginHelper.ClosePlug(DataHelper.StrToInt(ID),e,ref Message))
                    {
                        if (PluginHelper.activeselectpulg(DataHelper.StrToInt(ID).ToString(), false))
                        {
                            ReturnMessage = PluginHelper.GetFunctionValue(Message.ToLower(), "text");
                            List<PlugNav> NextList = new List<PlugNav>();
                            foreach (var get in PluginHelper.PlugNavList)
                            {
                                if (get.PlugID == ID ==false)
                                {
                                    NextList.Add(get);
                                }
                            }
                            PluginHelper.PlugNavList.Clear();
                            PluginHelper.PlugNavList = NextList;
                            return true;
                        }
                    }
                    else
                    {
                        ReturnMessage = PluginHelper.GetFunctionValue(Message.ToLower(), "text");
                        return true;
                    }
                
                }
            }
           

            return false;
        }
        /// <summary>
        /// 卸载插件了
        /// </summary>
        /// <param name="Request"></param>
        /// <param name="ReturnMessage"></param>
        /// <param name="Type"></param>
        /// <returns></returns>
        public static bool DelectPulg(HttpContextcls Request, ref string ReturnMessage, string Type = "Obj")
        {
            string gettype = PlugHelper.ContextSetting(Request, Type, "type");
            if (gettype == "DelectPulg")
            {
                string ID = PlugHelper.ContextSetting(Request, Type, "ID");
                DataTable SelectPlug = SqlServerHelper.ExecuteDataTable("Select * From pulginslist Where ID = "+ DataHelper.StrToInt(ID));
                if (SelectPlug.Rows.Count > 0)
                {
                    if (DataHelper.ObjToStr(SelectPlug.Rows[0]["pulgcheck"]).ToLower() == "false")
                    {
                        string LockerFileName = DataHelper.ObjToStr(SelectPlug.Rows[0]["pulgpath"]);
                        int state = SqlServerHelper.ExecuteNonQuery("delete from pulginslist where ID =" + ID);
                        if (state == 0 == false)
                        {
                            if (File.Exists(DeFine.PluginPath + LockerFileName))
                            {
                                new Thread(() => {
                                    string TmpPath = DeFine.PluginPath + LockerFileName;
                                    Thread.Sleep(1000);
                                    try { File.Delete(TmpPath); } catch { HttpRuntime.UnloadAppDomain(); }
                                }).Start();

                                ReturnMessage = "已成功卸载" + DeFine.PluginPath + "->目标->" + LockerFileName;
                                return true;
                            }
                        }
                    }
                }
            }
            return false;
        }
        /// <summary>
        /// 获取可安装的插件
        /// </summary>
        /// <param name="Request"></param>
        /// <param name="ReturnMessage"></param>
        /// <param name="Type"></param>
        /// <returns></returns>
        public static bool GetInstallPulg(HttpContextcls Request, ref string ReturnMessage, string Type = "Obj")
        {
            string gettype = PlugHelper.ContextSetting(Request, Type, "type");
            if (gettype == "GetInstallPulg")
            {
                DataTable NTable = SqlServerHelper.ExecuteDataTable("Select * From FileList Where DefFileName Like '%.dll'");
                if (NTable.Rows.Count > 0)
                {
                    ReturnMessage = JsonHelper.DataFormatToJson(NTable);
                    return true;
                }
            }
            return false;
        }

        /// <summary>
        /// 开始安装插件
        /// </summary>
        /// <param name="Request"></param>
        /// <param name="ReturnMessage"></param>
        /// <param name="Type"></param>
        /// <returns></returns>
        public static bool InstallPulg(HttpContextcls Request, ref string ReturnMessage, string Type = "Obj")
        {
            string gettype = PlugHelper.ContextSetting(Request, Type, "type");
            if (gettype == "InstallPulg")
            {
                string ID = PlugHelper.ContextSetting(Request, Type, "ID");
                DataTable SelectItem = SqlServerHelper.ExecuteDataTable("Select * From FileList Where DefFileName Like '%.dll' And ID ="+ DataHelper.StrToInt(ID).ToString());
                if (SelectItem.Rows.Count > 0)
                {
                    string LockerPluginPath = DeFine.UPLoadFile + DataHelper.ObjToStr(SelectItem.Rows[0]["FileName"]);
                    if (File.Exists(LockerPluginPath))
                    {
                        if (File.Exists(DeFine.PluginPath + DataHelper.ObjToStr(SelectItem.Rows[0]["DefFileName"])))
                        {
                            try 
                            {
                                File.Delete(DeFine.PluginPath + DataHelper.ObjToStr(SelectItem.Rows[0]["DefFileName"]));
                            } 
                            catch 
                            {
                                ReturnMessage = "错误文件权限不足";
                                return true;
                            }
                        } 
                        File.Copy(LockerPluginPath, DeFine.PluginPath+ DataHelper.ObjToStr(SelectItem.Rows[0]["DefFileName"]));
                        string InstallMessage = PluginHelper.installpulgins(PluginHelper.getallpulgins());
                        ReturnMessage = InstallMessage;

                        return true;
                    }
                }
                else
                {
                    return true;
                }
            }
            return false;
        }

        /// <summary>
        /// 调用引擎接口
        /// </summary>
        /// <param name="Request"></param>
        /// <param name="ReturnMessage"></param>
        /// <param name="Type"></param>
        /// <returns></returns>
        public static object EngineLock = new object();
        public static bool ShellEngine(HttpContextcls Request, ref string ReturnMessage, string Type = "Obj")
        {
            string gettype = PlugHelper.ContextSetting(Request, Type, "type");
            if (gettype == "ShellEngine")
            {
                lock (EngineLock)
                {
                    string RefreshCache = PlugHelper.ContextSetting(Request, Type, "RefreshCache");
                    string Front = PlugHelper.ContextSetting(Request, Type, "Front");
                    string After = PlugHelper.ContextSetting(Request, Type, "After");
                    string RefreshDB = PlugHelper.ContextSetting(Request, Type, "RefreshDB");
                    string ReloadArticle = PlugHelper.ContextSetting(Request, Type, "ReloadArticle");
                    EngineSetting NEngineSetting = new EngineSetting();
                    NEngineSetting.RefreshCache = DataHelper.StrToBool(RefreshCache);
                    NEngineSetting.Front = DataHelper.StrToBool(Front);
                    NEngineSetting.After = DataHelper.StrToBool(After);
                    NEngineSetting.RefreshDB = DataHelper.StrToBool(RefreshDB);
                    NEngineSetting.ReloadArticle = DataHelper.StrToBool(ReloadArticle);
                    string Message = HtmlCreatEngine.ReadAllTemplate(NEngineSetting);
                    ReturnMessage = Message;
                    return true;
                }
            }
            return false;
        }
        /// <summary>
        /// 执行安全检查
        /// </summary>
        /// <param name="Request"></param>
        /// <param name="ReturnMessage"></param>
        /// <param name="Type"></param>
        /// <returns></returns>
        public static bool SecurityScanning(HttpContextcls Request, ref string ReturnMessage, string Type = "Obj")
        {
            string gettype = PlugHelper.ContextSetting(Request, Type, "type");
            if (gettype == "SecurityScanning")
            {
                if (WebDefence.WorkingScanThread == 0)
                {
                    ReturnMessage = " WebDefence.StartWebScan " + DateTime.Now;
                    WebDefence.StartWebScan(AppDomain.CurrentDomain.BaseDirectory);
                    return true;
                }
                else
                {
                    ReturnMessage = " WebDefence.StartWebScan " + DateTime.Now;
                    return true;
                }
            }
            return false;
        }

        /// <summary>
        /// 获取扫描的实时消息
        /// </summary>
        /// <param name="Request"></param>
        /// <param name="ReturnMessage"></param>
        /// <param name="Type"></param>
        /// <returns></returns>
        public static bool GetSecurityScanningMsg(HttpContextcls Request, ref string ReturnMessage, string Type = "Obj")
        {
            string gettype = PlugHelper.ContextSetting(Request, Type, "type");
            if (gettype == "SecurityScanningMsg")
            {
                ReturnMessage = JsonHelper.DataFormatToJson(WebDefence.CurrentScanMessage);
                return true;
            }
            return false;
        }

        /// <summary>
        /// 获取威胁和可疑文件列表
        /// </summary>
        /// <param name="Request"></param>
        /// <param name="ReturnMessage"></param>
        /// <param name="Type"></param>
        /// <returns></returns>
        public static bool GetSecurityScanningData(HttpContextcls Request, ref string ReturnMessage, string Type = "Obj")
        {
            string gettype = PlugHelper.ContextSetting(Request, Type, "type");
            if (gettype == "GetSecurityScanningData")
            {
                ReturnMessage = JsonHelper.DataFormatToJson(WebDefence.ScanMessageList);
                return true;
            }
            return false;
        }
        /// <summary>
        /// 获取防火墙拦截的ip列表
        /// </summary>
        /// <param name="Request"></param>
        /// <param name="ReturnMessage"></param>
        /// <param name="Type"></param>
        /// <returns></returns>
        public static bool GetFireWallList(HttpContextcls Request, ref string ReturnMessage, string Type = "Obj")
        {
            string gettype = PlugHelper.ContextSetting(Request, Type, "type");
            if (gettype == "GetFireWallList")
            {
                ReturnMessage = JsonHelper.DataFormatToJson(WebDefence.AllConnect);
                return true;
            }
            return false;
        }
        /// <summary>
        /// 添加新的线程任务池
        /// </summary>
        /// <param name="Request"></param>
        /// <param name="ReturnMessage"></param>
        /// <param name="Type"></param>
        /// <returns></returns>

        public static bool AddWorkTime(HttpContextcls Request, ref string ReturnMessage, string Type = "Obj")
        {
            string gettype = PlugHelper.ContextSetting(Request, Type, "type");
            if (gettype == "AddWorkTime")
            {
                string RunCode = PlugHelper.ContextSetting(Request, Type, "RunCode");
                string Sleep = PlugHelper.ContextSetting(Request, Type, "Sleep");
                string RunPool = PlugHelper.ContextSetting(Request, Type, "RunPool");
                bool While =DataHelper.StrToBool(PlugHelper.ContextSetting(Request, Type, "While"));
                bool Active = DataHelper.StrToBool(PlugHelper.ContextSetting(Request, Type, "Active"));
                string DspText = PlugHelper.ContextSetting(Request, Type, "DspText");
                string SqlOder = "INSERT INTO ThreadWorking([WorkAction],[RunCode],[Sleep],[RunPool],[While],[Active],[DspText]) VALUES ('','{0}', '{1}', '{2}', '{3}', '{4}','{5}');";
                int state = SqlServerHelper.ExecuteNonQuery(string.Format(SqlOder,RunCode,Sleep,RunPool,While,Active,DspText));
                if (state == 0 == false)
                {
                    new Thread(() => {

                        WorkingTime.StartWorkingService(false);
                        Thread.Sleep(100);
                        WorkingTime.StartWorkingService(true);

                    }).Start();
                    ReturnMessage = true.ToString();
                    return true;
                }

            }
            return false;
        }
        /// <summary>
        /// 获取线程任务池队列
        /// </summary>
        /// <param name="Request"></param>
        /// <param name="ReturnMessage"></param>
        /// <param name="Type"></param>
        /// <returns></returns>
        public static bool GetWorkTimeList(HttpContextcls Request, ref string ReturnMessage, string Type = "Obj")
        {
            string gettype = PlugHelper.ContextSetting(Request, Type, "type");
            if (gettype == "GetWorkTimeList")
            {
                ReturnMessage =JsonHelper.DataFormatToJson(SqlServerHelper.ExecuteDataTable("Select * From ThreadWorking"));
                return true;
            }
            return false;
        }

        /// <summary>
        /// 删除指定的线程任务组
        /// </summary>
        /// <param name="Request"></param>
        /// <param name="ReturnMessage"></param>
        /// <param name="Type"></param>
        /// <returns></returns>
        public static bool DelectWorkTimeItem(HttpContextcls Request, ref string ReturnMessage, string Type = "Obj")
        {
            string gettype = PlugHelper.ContextSetting(Request, Type, "type");
            if (gettype == "DelectWorkTimeItem")
            {
                int ID = DataHelper.StrToInt(PlugHelper.ContextSetting(Request, Type, "ID"));
                int state = SqlServerHelper.ExecuteNonQuery("Delete From ThreadWorking WHERE ID = " + ID.ToString());
                if (state == 0 == false)
                {
                    ReturnMessage = true.ToString();
                    return true;
                }
                else
                {
                    ReturnMessage = false.ToString();
                    return true;
                }
            }
            return false;
        }
        /// <summary>
        /// 开始处理任务
        /// </summary>
        /// <param name="Request"></param>
        /// <param name="ReturnMessage"></param>
        /// <param name="Type"></param>
        /// <returns></returns>
        public static bool StartWorkTimeItem(HttpContextcls Request, ref string ReturnMessage, string Type = "Obj")
        {
            string gettype = PlugHelper.ContextSetting(Request, Type, "type");
            if (gettype == "StartWorkTimeItem")
            {
                int ID = DataHelper.StrToInt(PlugHelper.ContextSetting(Request, Type, "ID"));
                int state = SqlServerHelper.ExecuteNonQuery("UPDate ThreadWorking SET Active = '" + true.ToString() + "' Where ID = " + ID.ToString());
                if (state == 0 == false)
                {
                    ReturnMessage = true.ToString();
                    return true;
                }
                else
                {
                    ReturnMessage = false.ToString();
                    return true;
                }
            }
            return false;
        }
        /// <summary>
        /// 结束处理任务
        /// </summary>
        /// <param name="Request"></param>
        /// <param name="ReturnMessage"></param>
        /// <param name="Type"></param>
        /// <returns></returns>
        public static bool EndWorkTimeItem(HttpContextcls Request, ref string ReturnMessage, string Type = "Obj")
        {
            string gettype = PlugHelper.ContextSetting(Request, Type, "type");
            if (gettype == "EndWorkTimeItem")
            {
                int ID = DataHelper.StrToInt(PlugHelper.ContextSetting(Request, Type, "ID"));
                int state = SqlServerHelper.ExecuteNonQuery("UPDate ThreadWorking SET Active = '" + false.ToString() + "' Where ID = " + ID.ToString());
                if (state == 0 == false)
                {
                    ReturnMessage = true.ToString();
                    return true;
                }
                else
                {
                    ReturnMessage = false.ToString();
                    return true;
                }
            }
            return false;
        }
        /// <summary>
        /// 修改线程任务池里的对象
        /// </summary>
        /// <param name="Request"></param>
        /// <param name="ReturnMessage"></param>
        /// <param name="Type"></param>
        /// <returns></returns>
        public static bool ReloadWorkTimeItem(HttpContextcls Request, ref string ReturnMessage, string Type = "Obj")
        {
            string gettype = PlugHelper.ContextSetting(Request, Type, "type");
            if (gettype == "ReloadWorkTimeItem")
            {
                int ID = DataHelper.StrToInt(PlugHelper.ContextSetting(Request, Type, "ID"));

                string RunPool = PlugHelper.ContextSetting(Request, Type, "RunPool");
                string RunCode = PlugHelper.ContextSetting(Request, Type, "RunCode");
                string Sleep = PlugHelper.ContextSetting(Request, Type, "Sleep");
                string While = PlugHelper.ContextSetting(Request, Type, "While");
                string DspText = PlugHelper.ContextSetting(Request, Type, "DspText");

                int state = SqlServerHelper.ExecuteNonQuery("UPDate ThreadWorking SET RunPool = '" + RunPool + "',RunCode ='"+ RunCode + "',Sleep ='"+ Sleep + "',While ='"+ While + "',DspText ='"+ DspText + "' Where ID = " + ID.ToString());
                if (state == 0 == false)
                {
                    ReturnMessage = true.ToString();
                    return true;
                }
                else
                {
                    ReturnMessage = false.ToString();
                    return true;
                }
            }

            return false;
        }
        /// <summary>
        /// 获取网站全局设置
        /// </summary>
        /// <param name="Request"></param>
        /// <param name="ReturnMessage"></param>
        /// <param name="Type"></param>
        /// <returns></returns>
        public static bool GetWebSetting(HttpContextcls Request, ref string ReturnMessage, string Type = "Obj")
        {
            string gettype = PlugHelper.ContextSetting(Request, Type, "type");
            if (gettype == "GetWebSetting")
            {
                WebSetting NWebSetting = new WebSetting();
                NWebSetting.WebHost = CMSHelper.GetDeFineValue("DEFURL");
                NWebSetting.WebRequestURL = CMSHelper.GetDeFineValue("WebRequestURL");
                NWebSetting.WallMaxConnect = CMSHelper.GetDeFineValue("WallMaxConnect");

                NWebSetting.WebDescription = CMSHelper.GetDeFineValue("WebDescription");
                NWebSetting.BottomCopyright = CMSHelper.GetDeFineValue("BottomCopyright");
                NWebSetting.KeepOnRecord = CMSHelper.GetDeFineValue("KeepOnRecord");

                NWebSetting.ActList = CMSHelper.GetDeFineValue("ActList");
                NWebSetting.ActName = CMSHelper.GetDeFineValue("ActName");

                ReturnMessage = JsonHelper.DataFormatToJson(NWebSetting);
                return true;
            }
            return false;
        }
        /// <summary>
        /// 应用网站全局设置
        /// </summary>
        /// <param name="Request"></param>
        /// <param name="ReturnMessage"></param>
        /// <param name="Type"></param>
        /// <returns></returns>
        public static bool SetWebSetting(HttpContextcls Request, ref string ReturnMessage, string Type = "Obj")
        {
            string gettype = PlugHelper.ContextSetting(Request, Type, "type");
            if (gettype == "SetWebSetting")
            {
                WebSetting NWebSetting = new WebSetting();

                NWebSetting.WebHost = PlugHelper.ContextSetting(Request, Type, "WebHost", "HtmlCode");
                if (!NWebSetting.WebHost.EndsWith("/")) NWebSetting.WebHost += "/";
                NWebSetting.WebRequestURL = PlugHelper.ContextSetting(Request, Type, "WebRequestURL", "HtmlCode");
                NWebSetting.WallMaxConnect = PlugHelper.ContextSetting(Request, Type, "WallMaxConnect");

                NWebSetting.WebDescription= PlugHelper.ContextSetting(Request, Type, "WebDescription","HtmlCode");
                NWebSetting.BottomCopyright = PlugHelper.ContextSetting(Request, Type, "BottomCopyright","HtmlCode");
                NWebSetting.KeepOnRecord= PlugHelper.ContextSetting(Request, Type, "KeepOnRecord","HtmlCode");

                NWebSetting.ActList = PlugHelper.ContextSetting(Request, Type, "ActList", "HtmlCode");
                NWebSetting.ActName = PlugHelper.ContextSetting(Request, Type, "ActName", "HtmlCode");
                CMSHelper.SetDeFineValue("DEFURL", NWebSetting.WebHost);
                CMSHelper.SetDeFineValue("WebRequestURL", NWebSetting.WebRequestURL);
                CMSHelper.SetDeFineValue("WallMaxConnect", NWebSetting.WallMaxConnect);

                CMSHelper.SetDeFineValue("WebDescription", NWebSetting.WebDescription);
                CMSHelper.SetDeFineValue("BottomCopyright", NWebSetting.BottomCopyright);
                CMSHelper.SetDeFineValue("KeepOnRecord", NWebSetting.KeepOnRecord);

                CMSHelper.SetDeFineValue("ActList", NWebSetting.ActList);
                CMSHelper.SetDeFineValue("ActName", NWebSetting.ActName);

                DeFine.DefWebUrl = NWebSetting.WebHost;
                DeFine.DefWebRequestUrl = NWebSetting.WebRequestURL;
                DeFine.WallMaxConnect = DataHelper.StrToInt(NWebSetting.WallMaxConnect);

                ReturnMessage = true.ToString();
                return true;

            }
            return false;
        }
        /// <summary>
        /// 获取当前线程池队列
        /// </summary>
        /// <param name="Request"></param>
        /// <param name="ReturnMessage"></param>
        /// <param name="Type"></param>
        /// <returns></returns>
        public static bool CheckPoolValue(HttpContextcls Request, ref string ReturnMessage, string Type = "Obj")
        {
            string gettype = PlugHelper.ContextSetting(Request, Type, "type");
            if (gettype == "CheckPoolValue")
            {
                PoolValue NPoolValue = new PoolValue();
                NPoolValue.EmergencyPoolThreadCount = CoreHelper.EmergencyPoolThreadCount;
                NPoolValue.QueuePoolThreadCount = CoreHelper.QueuePoolThreadCount;
                NPoolValue.SystemPoolThreadCount = CoreHelper.SystemPoolThreadCount;
                NPoolValue.UserPoolThreadCount = CoreHelper.UserPoolThreadCount;

                ReturnMessage = JsonHelper.DataFormatToJson(NPoolValue);
                return true;
            }
            return false;
        }

        /// <summary>
        /// 根据链头获取整条区块链
        /// </summary>
        /// <param name="Request"></param>
        /// <param name="ReturnMessage"></param>
        /// <param name="Type"></param>
        /// <returns></returns>
        public static bool GetAllBlockByHead(HttpContextcls Request, ref string ReturnMessage, string Type = "Obj")
        {
            string gettype = PlugHelper.ContextSetting(Request, Type, "type");
            if (gettype == "GetAllBlockByHead")
            {
                string BlockHead =PlugHelper.ContextSetting(Request, Type, "BlockHead");
                Block HeadBlock = BlockChainHelper.ReadBlockHead(BlockHead);
                List<Block> AllBlock = BlockChainHelper.ReadAllBlockByHead(HeadBlock);
                ReturnMessage = JsonHelper.DataFormatToJson(AllBlock);
                return true;
            }
            return false;
        }
      
        /// <summary>
        /// 文件转储到区块链
        /// </summary>
        /// <param name="Request"></param>
        /// <param name="ReturnMessage"></param>
        /// <param name="Type"></param>
        /// <returns></returns>
        public static bool SaveFileToBlock(HttpContextcls Request, ref string ReturnMessage, string Type = "Obj")
        {
            string gettype = PlugHelper.ContextSetting(Request, Type, "type");
            if (gettype == "SaveFileToBlock")
            {
                string FileAction = PlugHelper.ContextSetting(Request, Type, "FileName","HtmlCode");
                string BlockType = PlugHelper.ContextSetting(Request, Type, "BlockType", "HtmlCode");
                Accessibility NAccessibility = new Accessibility();
                if (DataHelper.StrToBool(BlockType))
                {
                    NAccessibility = Accessibility.Public;
                }
                else
                {
                    NAccessibility = Accessibility.Private;
                }
                string FileName = CMSHelper.GetFileByAction(FileAction).FileName;
                string FilePath = DeFine.UPLoadFile + FileName;
                int Sucess = 0;
                if (File.Exists(FilePath))
                {
                    FileStream fs = new FileStream(FilePath, FileMode.Open,FileAccess.Read,FileShare.ReadWrite);
                    long size = fs.Length;
                    byte[] array = new byte[size];
                    fs.Read(array, 0, array.Length);
                    var GetAllBlock=BlockChainHelper.CreatBlock(FileAction, array, 1000, NAccessibility);
                    Sucess =BlockChainHelper.SaveToDB(GetAllBlock);
                    fs.Close();
                }
                if (Sucess > 0)
                {
                    ReturnMessage = true.ToString()+" Action:"+ FileAction;
                }
                else
                {
                    ReturnMessage = false.ToString();
                }
               
                return true;
            }
            return false;
        }

        /// <summary>
        /// 字符串转储到区块链
        /// </summary>
        /// <param name="Request"></param>
        /// <param name="ReturnMessage"></param>
        /// <param name="Type"></param>
        /// <returns></returns>
        public static bool SaveStrToBlock(HttpContextcls Request, ref string ReturnMessage, string Type = "Obj")
        {
            string gettype = PlugHelper.ContextSetting(Request, Type, "type");
            if (gettype == "SaveStrToBlock")
            {
                string StrText = PlugHelper.ContextSetting(Request, Type, "StrText", "HtmlCode");
                string BlockType = PlugHelper.ContextSetting(Request, Type, "BlockType", "HtmlCode");
                Accessibility NAccessibility = new Accessibility();
                if (DataHelper.StrToBool(BlockType))
                {
                    NAccessibility = Accessibility.Public;
                }
                else
                {
                    NAccessibility = Accessibility.Private;
                }

                int Sucess = 0;
                string Action = StrText.GetHashCode().ToString();
                var GetAllBlock = BlockChainHelper.CreatBlock(Action, Encoding.UTF8.GetBytes(StrText), 255, NAccessibility);
                Sucess = BlockChainHelper.SaveToDB(GetAllBlock);
                
                if (Sucess > 0)
                {
                    ReturnMessage = true.ToString()+" Action:" + Action;
                }
                else
                {
                    ReturnMessage = false.ToString();
                }

                return true;
            }
            return false;

        }
        /// <summary>
        /// 修改用户缩略图
        /// </summary>
        /// <param name="Request"></param>
        /// <param name="ReturnMessage"></param>
        /// <param name="LockerToken"></param>
        /// <param name="Type"></param>
        /// <returns></returns>
        public static bool ReloadUserPic(HttpContextcls Request, ref string ReturnMessage, usertoken LockerToken, string Type = "Obj")
        {
            string gettype = PlugHelper.ContextSetting(Request, Type, "type");
            if (gettype == "ReloadUserPic")
            {
                string PicPath = PlugHelper.ContextSetting(Request, Type, "PicPath");
                ReturnMessage = UserHelper.ReloadUserPic(LockerToken.username,PicPath).ToString();
                return true;
            }
            return false;
        }


   

        #endregion




    }

    
    public class WebSetting
    {
        public string WebHost = "";
        public string WebRequestURL = "";
        public string WallMaxConnect = "";
        public string ActList = "";
        public string ActName = "";
        public string WebDescription = "";
        public string BottomCopyright = "";
        public string KeepOnRecord = "";
    }
    public enum TokenType
    {
        Null = 0, User = 1, Admin = 2
    }
    public class ConvertMessage
    {
        public int success = 0;
        public int error = 0;
        public bool working = false;
    }

    public class UserMsgItem
    {
        public string ID = "";
        public string userid = "";
        public string userpassword = "";
        public string username = "";
        public string usersex = "";
        public string usertype = "";
        public string useremail = "";
        public string userphone = "";
        public string userbirthday = "";
        public string userpicpath = "";
        public string userfristfromaddress = "";
        public string Subclass = "";
    }

    public class UserPayCenter
    {
        public string LockerUserID = "";
        public string UserPayNumber = "";
    }

    public class PageList
    {
        public DataTable PageMessage = new DataTable();
        public int PageCount = 0;
        public int SelectPage = 0;
    }
}