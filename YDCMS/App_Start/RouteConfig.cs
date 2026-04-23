using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Web;
using System.Web.Mvc;
using System.Web.Routing;
using WebMaster;
using WebMaster.CoreManager;
using WebMaster.DataManager;
using WebMaster.HtmlManager;
using WebMaster.WebManager;

namespace WebManager
{
  
    public class RouteConfig
    {
        public static void RegisterRoutes(RouteCollection routes)
        {
            routes.IgnoreRoute("{resource}.axd/{*pathInfo}");

            DataProcessing.OpenDBCon();

            PluginHelper.initialization();
            SettingHelper.initialization();


            WorkingTime.StartWorkingService(true);//启动定期任务处理服务

            BrainConfig.StartListenService(false);//启动性能监听服务
            CoreHelper.ThreadCoreService(true);//启动线程池服务
            WebDefence.StartDefenceService(true);//启动网站防御服务
           
            HtmlCreatEngine.ReadAllTemplate(DeFine.TemplatesPath, true);
           

            routes.MapRoute(
                name: "Default",
                url: "{controller}/{action}/{id}",
                defaults: new { controller = "Web", action = "Index", id = UrlParameter.Optional }
            );

           
        }
    }
}
