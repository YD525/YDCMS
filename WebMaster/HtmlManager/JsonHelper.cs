using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;


namespace WebMaster.HtmlManager
{
    public class JsonHelper
    {
        public static string DataFormatToJson(object value)
        {
            return JsonConvert.SerializeObject(value);
        }

    }


    public class UserINFO
    {
        public string userid = "";
        public string username = "";
        public int usersex = -1;
        public string userbirthday = "";
        public string userphone = "";
        public string userpicpath = "";
        public string useremail = "";
        public string usertype = "";
        public string userfristfromaddress = "";
        public string Subclass = "";
    }
}