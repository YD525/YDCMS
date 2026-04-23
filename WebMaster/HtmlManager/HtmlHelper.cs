using WebMaster.DataManager;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace WebMaster.HtmlManager
{
    public class HtmlHelper
    {
       

        /// <summary>
        /// 创建UE插件上传框
        /// </summary>
        /// <param name="plusname"></param>
        /// <param name="buttonname"></param>
        /// <param name="creatid"></param>
        /// <param name="buttonstyle"></param>
        /// <param name="imagestyle"></param>
        /// <param name="inuptstyle"></param>
        /// <returns></returns>
        public static ueinupt createfunctionueinupt(string buttonname, string creatid, string buttonstyle = "", string imagestyle = "", string inuptstyle = "")
        {
            ueinupt newinupt = new ueinupt();
            string newcrlf = "\r\n";
            newinupt.buttoncode = ("<input type='button' class='" + buttonstyle + "' onclick='upImage" + creatid + "();' value='" + buttonname + "'/>").Replace("'", "\"");
            newinupt.imagecode = ("<img id='img" + creatid + "' src='' class='" + imagestyle + "'/>").Replace("'", "\"");
            newinupt.inuptcode = ("<input type='text' class='" + inuptstyle + "' id='inupt" + creatid + "' value=''/>").Replace("'", "\"");
            newinupt.scriptcode += ("<script type='text/plain' id='con" + creatid + "'></script>").Replace("'", "\"") + newcrlf;
            string dqcode = "var con" + creatid + ";" + newcrlf;
            dqcode += "var obj" + creatid + ";" + newcrlf; ;
            string incode = "obj" + creatid + " = con" + creatid + ".getDialog('insertimage');" + newcrlf; ;
            incode += "obj" + creatid + ".render();" + newcrlf; ;
            incode += "obj" + creatid + ".open();" + newcrlf; ;
            dqcode += createjsfunction("upImage" + creatid, incode);
            //FUNCTION
            dqcode += "con" + creatid + " = new UE.ui.Editor();" + newcrlf;
            dqcode += "con" + creatid + ".render('" + "con" + creatid + "');" + newcrlf;
            dqcode += "con" + creatid + ".ready(function(){" + newcrlf;
            dqcode += "con" + creatid + ".setDisabled();" + newcrlf;
            dqcode += "con" + creatid + ".hide();" + newcrlf;
            dqcode += "con" + creatid + ".addListener('beforeInsertImage',function(t, arg){" + newcrlf;
            dqcode += "document.getElementById('inupt" + creatid + "').value = arg[0].src;" + newcrlf;
            dqcode += "document.getElementById('img" + creatid + "').src = arg[0].src;" + newcrlf;
            dqcode += "});" + newcrlf;
            dqcode += "});" + newcrlf;
            newinupt.scriptcode += getjsformat(dqcode);
            newinupt.allcode = newinupt.scriptcode + "\r\n" + newinupt.inuptcode + "\r\n" + newinupt.imagecode + "\r\n" + newinupt.buttoncode + "\r\n";
            newinupt.creatid = creatid;
            return newinupt;
        }
        /// <summary>
        /// 创建ue超级文本框
        /// </summary>
        /// <param name="code"></param>
        /// <param name="target"></param>
        /// <param name="id"></param>
        /// <param name="width"></param>
        /// <param name="height"></param>
        /// <returns></returns>
        public static string createtext(string code, string target, string id, int width, int height)
        {
            string plusbenti = "<textarea id='" + id + "' runat='server' type='text/plain' style='width: " + width + "%; height: " + height + "px;'></textarea>";
            string plusfoot = "<script>UE.getEditor('" + id + "', {initialFrameWidth: '100%'})</script>";
            code = code.Replace("</body>", "\r\n" + plusfoot + "\r\n" + "</body>");
            code = code.Replace(target, plusbenti);
            return code;
        }
        public static string installueditor(string code)
        {
            string plushead = "<script src='" + DeFine.GetRootURI() + "/ueditor/ueditor.config.js' type='text/javascript'></script>"
              + "\r\n" + "<script src='" + DeFine.GetRootURI() + "/ueditor/ueditor.all.min.js' type='text/javascript'></script>";
            code = code.Replace("</head>", "\r\n" + plushead + "\r\n" + "</head>");
            return code;
        }

        public static string installueditor()
        {
            string plushead = "<script src='" + DeFine.GetRootURI() + "/ueditor/ueditor.config.js' type='text/javascript'></script>"
              + "\r\n" + "<script src='" + DeFine.GetRootURI() + "/ueditor/ueditor.all.min.js' type='text/javascript'></script>";
            return plushead;
        }

        public static string createnewhtml(string tittle = "", string iconpath = "", string description = "", string keywords = "", string encoded = "", List<string> alllink = null, List<string> allmeta = null)
        {
            if (encoded == "") { encoded = "utf-8"; }
            string iconimage = "";
            if (iconpath == "") { iconimage = DeFine.GetRootURI() + "/admin/favicon.ico"; } else { iconimage = iconpath; }

            string newcrlf = "\r\n";
            string htmlcode = "<!DOCTYPE html>" + newcrlf + "<html>" + newcrlf;

            string defstyle = ("<style type='text/css'>").Replace("'", "\"") + newcrlf;
            defstyle += "body{margin:0;}" + newcrlf;
            defstyle += "</style>" + newcrlf;

            htmlcode += "<head>" + newcrlf;
            htmlcode += ("<meta http-equiv='content-type' content='text/html;charset=" + encoded + "'>").Replace("'", "\"") + newcrlf;
            htmlcode += "<title>" + tittle + "</title>" + newcrlf;
            htmlcode += ("<link rel='icon' href='" + iconimage + "' type='image/x-icon'/>").Replace("'", "\"") + newcrlf;
            htmlcode += ("<link rel='shortcut icon' href='" + iconimage + "' type='image/x-icon'/>").Replace("'", "\"") + newcrlf;
            htmlcode += ("<meta name='description' content='" + description + "'/>").Replace("'", "\"") + newcrlf;
            htmlcode += ("<meta name='keywords' content ='" + keywords + "'/>").Replace("'", "\"") + newcrlf;
            htmlcode += "{@HZC.alllink}" + newcrlf;
            htmlcode += "{@HZC.allmeta}" + newcrlf;
            htmlcode += defstyle + newcrlf;
            htmlcode += "</head>" + newcrlf;
            htmlcode += "<body>" + newcrlf;
            htmlcode += "</body>" + newcrlf;
            htmlcode += "</html>" + newcrlf;


            if (alllink == null == false)
            {
                string richtext = "";
                foreach (var getlink in alllink)
                {
                    richtext += getlink + "\r\n";
                }
                htmlcode = htmlcode.Replace("{@HZC.alllink}", richtext);
            }
            else
            {
                htmlcode = htmlcode.Replace("{@HZC.alllink}", "");
            }

            if (allmeta == null == false)
            {
                string richtext = "";
                foreach (var getmeta in allmeta)
                {
                    richtext += getmeta + "\r\n";
                }
                htmlcode = htmlcode.Replace("{@HZC.allmeta}", richtext);
            }
            else
            {
                htmlcode = htmlcode.Replace("{@HZC.allmeta}", "");
            }

            GC.Collect();

            return htmlcode;
        }
        public static string getjsformat(string instrcode)
        {
            return ("<script type='text/javascript'>\r\n" + instrcode + "</script>\r\n").Replace("'", "\"");

        }
        public static string createjsfunction(string functionname, string incode)
        {
            return "function " + functionname + "()" + "\r\n" + "{" + "\r\n" + incode + "\r\n" + "}\r\n";

        }

        #region html控制
        public static string begininhead(string htmlcode, string newcode)
        {
            return htmlcode.Replace("</head>", "\r\n" + newcode + "\r\n" + "</head>");
        }
        public static string begininstyle(string htmlcode, string newcode)
        {
            return htmlcode.Replace("</style>", "\r\n" + newcode + "\r\n" + "</style>");
        }
        public static string begininbody(string htmlcode, string newcode)
        {
            return htmlcode.Replace("</body>", "\r\n" + newcode + "\r\n" + "</body>");
        }
        public static string begininhtml(string htmlcode, string newcode)
        {
            return htmlcode.Replace("</html>", "\r\n" + newcode + "\r\n" + "</html>");
        }
        public static string begininlastcode(string htmlcode, string newcode)
        {
            return htmlcode + "\r\n" + newcode;
        }
        public static string indexincode(string htmlcode, string newcode)
        {
            return newcode + "\r\n" + htmlcode;
        }
        #endregion

        #region html创建标签了
        public static string createlink(string filepath = "", List<metalink> alltype = null)
        {
            string richtext = "";
            string autotext = "";
            if (filepath == "" == false)
            {
                autotext = ("href='" + filepath + "'").Replace("'", "\"");
                if (autotext.EndsWith(".css"))
                {
                    autotext += " rel='stylesheet' type='text/css'";
                }
            }
            foreach (var gettype in alltype)
            {
                richtext += (" " + gettype.functionname + "='" + gettype.functionvalue + "' ").Replace("'", "\"");
            }
            return string.Format("<link {0} {1}/>", richtext, autotext);
        }
        public static string createmeta(List<metatype> alltype)
        {
            string richtext = "";
            foreach (var gettype in alltype)
            {
                richtext += (" " + gettype.functionname + "='" + gettype.functionvalue + "' ").Replace("'", "\"");
            }
            return string.Format("<meta {0}/>", richtext);
        }
        #endregion
        public class metatype
        {
            public string functionname { get; set; }
            public string functionvalue { get; set; }

        }
        public class metalink : metatype
        {
            //addnewcode
        }
    }

    public struct ueinupt
    {
        public string imagecode { get; set; }
        public string buttoncode { get; set; }
        public string inuptcode { get; set; }
        public string scriptcode { get; set; }
        public string creatid { get; set; }
        public string allcode { get; set; }
    }
}