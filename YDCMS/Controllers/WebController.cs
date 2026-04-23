using Microsoft.Ajax.Utilities;
using System;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;
using WebMaster;
using WebMaster.BlockChainManager;
using WebMaster.DataManager;
using WebMaster.HtmlManager;
using WebMaster.UserManager;
using Block = WebMaster.BlockChainManager.Block;

namespace WebManager.Controllers
{
    public class WebController : Controller
    {

        /// <summary>
        /// 接收所有请求
        /// </summary>
        /// <returns></returns>
        public async Task<ActionResult> RequestManager()
        {
            GC.Collect();
            GC.SuppressFinalize(this);
            ActionResult NActionResult = null;
            string GetReturnCode = null;
            await Task.Run(() =>
            {
                HttpContext GetHttpContext = this.HttpContext.ApplicationInstance.Context;
                GetReturnCode = DataProcessing.ProcessingRequest(GetHttpContext, PlugHelper.PutAllRequest(GetHttpContext));
                string PulgMessage = PluginHelper.PutAllRequest(GetHttpContext);
                if (PulgMessage == null == false)
                {
                    GetReturnCode += "\r\n" + PulgMessage;
                }
            });
            NActionResult = Content(GetReturnCode);
            return NActionResult;
        }

        [HttpGet]
        public ActionResult ShowWebPage(string UI)
        {
            return Content(HtmlCreatEngine.ShowWebPage(UI, this.HttpContext.ApplicationInstance.Context));
        }



        [HttpGet]
        public ActionResult FileStream(string act)
        {
            FileList SelectFile = CMSHelper.GetFileByAction(act);
            if (SelectFile.FileAuthority.ToLower() == "all")
            {
            }
            else
            {
                usertoken SelectAdminToken = new usertoken();
                usertoken SelectUserToken = new usertoken();
                TokenType LockerType = new TokenType();
                LockerType = DataProcessing.checktoken(this.HttpContext.ApplicationInstance.Context, ref SelectAdminToken, ref SelectUserToken);
                if (LockerType == TokenType.User)
                {
                    if (!SelectFile.FileAuthority.ToLower().Contains("user"))
                    {
                        return Content("");
                    }
                }

                if (LockerType == TokenType.Admin)
                {
                    if (!SelectFile.FileAuthority.ToLower().Contains("admin"))
                    {
                        return Content("");
                    }

                }
        
                if (LockerType == TokenType.Null)
                {
                    return Content("");
                }

            }
            switch (SelectFile.FileType)
            {
                case "ImageType":
                    if (!DataProcessing.checkaddress(this.HttpContext.ApplicationInstance.Context))
                    {
                        if (SelectFile.AntiTheftChain == false.ToString())
                        {
                        }
                        else
                        {
                            return Content("");
                        }
                    }

                    Response.ContentType = "image/JPEG";
                    Response.TransmitFile(DeFine.UPLoadFile + SelectFile.FileName);

                    break;
                case "FileType":
                    if (!DataProcessing.checkaddress(this.HttpContext.ApplicationInstance.Context))
                    {
                        if (SelectFile.AntiTheftChain == false.ToString())
                        {
                        }
                        else
                        {
                            return Content("");
                        }
                    }

                    Response.Clear();
                    string fileName = HttpUtility.UrlEncode(System.Text.Encoding.UTF8.GetBytes(SelectFile.DefFileName));//(这里是你要的文件名称)
                    Response.AppendHeader("Content-Disposition", "attachment;filename=" + fileName);
                    Response.ContentType = "application/octet-stream";
                    Response.TransmitFile(DeFine.UPLoadFile + SelectFile.FileName);

                    break;
                case "obj":
                    if (!DataProcessing.checkaddress(this.HttpContext.ApplicationInstance.Context))
                    {
                        if (SelectFile.AntiTheftChain == false.ToString())
                        {
                        }
                        else
                        {
                            return Content("");
                        }
                    }

                    Response.Clear();
                    string ObjName = HttpUtility.UrlEncode(System.Text.Encoding.UTF8.GetBytes(SelectFile.DefFileName));//(这里是你要的文件名称)
                    Response.AppendHeader("Content-Disposition", "attachment;filename=" + ObjName);
                    Response.ContentType = "application/octet-stream";
                    Response.TransmitFile(DeFine.UPLoadFile + SelectFile.FileName);

                    break;
                case "VideoType":
                    if (!DataProcessing.checkaddress(this.HttpContext.ApplicationInstance.Context))
                    {
                        if (SelectFile.AntiTheftChain == false.ToString())
                        {
                        }
                        else
                        {
                            return Content("");
                        }
                    }

                    Response.ContentType = "video/mp4";
                    Response.AddHeader("content-disposition", "attachment;filename=interception.exe");
                    Response.Clear();
                    Response.TransmitFile(DeFine.UPLoadFile + SelectFile.FileName);

                    break;
            }





            return Content("");
        }

        [HttpGet]
        public ActionResult GetBlock(string BlockID,string GetType)
        {
            Block HeadBlock = BlockChainHelper.ReadBlockHead(BlockID);
            List<Block> AllBlock = BlockChainHelper.ReadAllBlockByHead(HeadBlock);
            if (HeadBlock.Access == Accessibility.Private)
            {
                usertoken SelectAdminToken = new usertoken();
                usertoken SelectUserToken = new usertoken();
                TokenType LockerType = new TokenType();
                LockerType = DataProcessing.checktoken(this.HttpContext.ApplicationInstance.Context, ref SelectAdminToken, ref SelectUserToken);
                if (LockerType == TokenType.Admin)
                {

                }
                else
                {
                    return Content("");
                }
            }
           
            if (GetType == "String")
            {
                return Content(BlockChainHelper.PutToString(AllBlock));
            }
            else
            {
                Response.AppendHeader("Content-Disposition", "attachment;filename=" + CMSHelper.GetFileByAction(HeadBlock.BlockID).DefFileName);
                Response.ContentType = "application/octet-stream";
                Response.BinaryWrite(BlockChainHelper.PutToFile(AllBlock));
            }
            return Content("");
        }
        public ActionResult LoginUser()
        {
            return View();
        }



      
    }
}