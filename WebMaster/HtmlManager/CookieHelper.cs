using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web;

namespace WebMaster
{
    //cookies  数据保存在用户端
    public class CookieHelper
    {
        /// <summary>
        /// 清除指定Cookie
        /// </summary>
        /// <param name="cookiename">cookiename</param>
        public static void ClearCookie(HttpContext LockerHttpContext, string cookiename)
        {
            HttpCookie cookie = LockerHttpContext.Request.Cookies[cookiename];
            if (cookie != null)
            {
                cookie.Expires = DateTime.Now.AddYears(-3);
                LockerHttpContext.Response.Cookies.Add(cookie);
            }
        }
        /// <summary>
        /// 获取指定Cookie值
        /// </summary>
        /// <param name="cookiename">cookiename</param>
        /// <returns></returns>
        public static string GetCookieValue(HttpContext LockerHttpContext,string cookiename)
        {

            try
            {
                HttpCookie cookie = LockerHttpContext.Request.Cookies[cookiename];
                string str = string.Empty;


                if (cookie != null)
                {
                    str = cookie.Value;
                }

                return str;
            }
            catch
            {
                return "";
            }

        }
        /// <summary>
        /// 添加一个Cookie（24小时过期）
        /// </summary>
        /// <param name="cookiename"></param>
        /// <param name="cookievalue"></param>
        public static void SetCookie(HttpContext LockerHttpContext,string cookiename, string cookievalue)
        {
            SetCookie(LockerHttpContext,cookiename, cookievalue, DateTime.Now.AddDays(1.0));
        }
        /// <summary>
        /// 添加一个Cookie
        /// </summary>
        /// <param name="cookiename">cookie名</param>
        /// <param name="cookievalue">cookie值</param>
        /// <param name="expires">过期时间 DateTime</param>
        public static void SetCookie(HttpContext LockerHttpContext,string cookiename, string cookievalue, DateTime expires)
        {
            HttpCookie cookie = new HttpCookie(cookiename)
            {
                Value = cookievalue,
                Expires = expires
            };
            LockerHttpContext.Response.Cookies.Add(cookie);
        }

    }
}
