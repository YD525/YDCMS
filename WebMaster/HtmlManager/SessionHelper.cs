using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web;

namespace WebMaster
{
    public class SessionHelper
    {
        public static object GetAllSession(string name)
        {
            return HttpContext.Current.Session;
        }
        public static object GetSession(string name)
        {
            return HttpContext.Current.Session[name];
        }
        public static void SetSession(string name, object val)
        {
            HttpContext.Current.Session[name] = val;
        }

        public static void AddSession(string name, object val)
        {
            HttpContext.Current.Session.Remove(name);
            HttpContext.Current.Session.Add(name, val);
        }
      
        public static void CleanSession(string name)
        {
            HttpContext.Current.Session.Remove(name);
        }
    }
}
