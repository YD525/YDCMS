using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Web;

namespace WebMaster.DataManager
{
    public class StringHelper
    {
        /// <summary>
        /// 正则删除字符串所有符号
        /// </summary>
        /// <param name="message"></param>
        /// <returns></returns>
        public static string FormatSpecialSymbolsClear(string message)
        {
           return Regex.Replace(message, @"[^a-zA-Z0-9_\u4e00-\u9fa5\' ']", "");
        }
    }
}