using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web;
using WebMaster.WebManager;

namespace WebMaster
{
    public class PlugHelper
    {
        /// <summary>
        /// 转换成结构体便于处理
        /// </summary>
        /// <param name="context"></param>
        /// <returns></returns>
        public static HttpContextcls PutAllRequest(HttpContext Context)
        {
            HttpContextcls newcls = new HttpContextcls();
            if (Context == null) return new HttpContextcls();
            for (int i = 0; i < Context.Request.Params.Count; i++)
            {
                typekey newkey = new typekey();
                if (Context.Request.Params.Keys[i] == null == false)
                {
                    newkey.key = Context.Request.Params.Keys[i].ToString();
                }
                else { newkey.key = ""; }
                if (Context.Request.Params[i] == null == false)
                {
                    newkey.value = Context.Request.Params[i].ToString();
                }
                else
                {
                    newkey.value = "";
                }
                newcls.allParams.Add(newkey);
            }

            for (int i = 0; i < Context.Request.Form.Count; i++)
            {
                typekey newkey = new typekey();
                if (Context.Request.Form.Keys[i] == null == false)
                {
                    newkey.key = Context.Request.Form.Keys[i].ToString();
                }
                else { newkey.key = ""; }
                if (Context.Request.Form[i] == null == false)
                {
                    newkey.value = Context.Request.Form[i].ToString();
                }
                else
                {
                    newkey.value = "";
                }
                newcls.allForm.Add(newkey);
            }

            for (int i = 0; i < Context.Request.QueryString.Count; i++)
            {
                typekey newkey = new typekey();
                if (Context.Request.QueryString.Keys[i] == null == false)
                {
                    newkey.key = Context.Request.QueryString.Keys[i].ToString();
                }
                else { newkey.key = ""; }
                if (Context.Request.QueryString[i] == null == false)
                {
                    newkey.value = Context.Request.QueryString[i].ToString();
                }
                else
                {
                    newkey.value = "";
                }
                newcls.allQueryString.Add(newkey);
            }

            for (int i = 0; i < Context.Request.Cookies.Count; i++)
            {
                typekey newkey = new typekey();
                if (Context.Request.Cookies.Keys[i] == null == false)
                {
                    newkey.key = Context.Request.Cookies.Keys[i].ToString();
                }
                else { newkey.key = ""; }
                if (Context.Request.Cookies[Context.Request.Cookies.Keys[i].ToString()].Value == null == false)
                {
                    newkey.value = Context.Request.Cookies[Context.Request.Cookies.Keys[i].ToString()].Value;
                }
                newcls.allCookies.Add(newkey);
            }

            return newcls;
        }


        /// <summary>
        /// 取httpcontext指定参数值
        /// </summary>
        /// <param name="obj"></param>
        /// <param name="type"></param>
        /// <param name="key"></param>
        /// <returns></returns>
        public static string ContextSetting(HttpContextcls obj, string type, string key,string DefFormat= "HtmlEncode")
        {
            if (type.ToLower() == "obj")
            {
                foreach (var get in obj.allParams)
                {
                    if (get.key.ToLower() == key.ToLower())
                    {
                        return WebDefence.InuptValueByNoSQLOder(get.value,DefFormat);
                    }
                }
            }
            else
            if (type.ToLower() == "get")
            {
                foreach (var get in obj.allQueryString)
                {
                    if (get.key.ToLower() == key.ToLower())
                    {
                        return WebDefence.InuptValueByNoSQLOder(get.value,DefFormat);
                    }
                }
            }
            else
            if (type.ToLower() == "post")
            {
                foreach (var get in obj.allForm)
                {
                    if (get.key.ToLower() == key.ToLower())
                    {
                        return WebDefence.InuptValueByNoSQLOder(get.value,DefFormat);
                    }
                }
            }
            else
            if (type.ToLower() == "cookies")
            {
                foreach (var get in obj.allCookies)
                {
                    if (get.key.ToLower() == key.ToLower())
                    {
                        return WebDefence.InuptValueByNoSQLOder(get.value,DefFormat);
                    }
                }
            }
            
            return null;

        }
    }


    public class HttpContextcls
    {
        public List<typekey> allParams = new List<typekey>();//所有参数包括getpost

        public List<typekey> allForm = new List<typekey>();//所有post参数

        public List<typekey> allQueryString = new List<typekey>();//所有get参数

        public List<typekey> allCookies = new List<typekey>();//所有cookies信息
    }

    public struct typekey
    {
        public string key { get; set; }
        public string value { get; set; }
    }
}
