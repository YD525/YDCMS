using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading;
using System.Web;
using WebMaster.CoreManager;
using WebMaster.DataManager;
using WebMaster.HtmlManager;

namespace WebMaster
{
    public class WorkingTime
    {
        public static int LockerWorkingTimeThread = 0;
        public static bool CloseAllWorkingTimeThread = true;
        private static bool WorkingLocker = false;

        public static void StartWorkingService(bool check)
        {
            if (check)
            {
                if (!WorkingLocker)
                {
                    WorkingLocker = true;
                    new Thread(() => {

                        while (LockerWorkingTimeThread > 0)
                        {
                            CloseAllWorkingTimeThread = true;
                            Thread.Sleep(1000);
                        }
                       
                        CloseAllWorkingTimeThread = false;
                        
                        DataTable NTable = SqlServerHelper.ExecuteDataTable("Select * From ThreadWorking");
                        for (int i = 0; i < NTable.Rows.Count; i++)
                        {
                            int ID =DataHelper.StrToInt(DataHelper.ObjToStr(NTable.Rows[i]["ID"]));
                            string RunPool =DataHelper.ObjToStr(NTable.Rows[i]["RunPool"]);
                            long tick = DateTime.Now.Ticks;

                            string WorkAction= ID.ToString() + "_" + (new Random((int)(tick & 0xffffffffL) | (int)(tick >> 32)).Next(100000,999999)).ToString();
                            int state = SqlServerHelper.ExecuteNonQuery("UPDate ThreadWorking SET WorkAction = '"+ WorkAction + "' Where ID = "+ ID.ToString());
                            if (state==0==false)
                            {

                                Action CreatAction = new Action(() => {

                                    WorkingTime.LockerWorkingTimeThread++;
                                    bool ISWhile = true;
                                    string CurrentWorkAction = WorkAction;

                                    while (!WorkingTime.CloseAllWorkingTimeThread)
                                    {
                                        if (ISWhile)
                                        {
                                            DataTable GetTable = SqlServerHelper.ExecuteDataTable("Select * From ThreadWorking Where WorkAction ='" + CurrentWorkAction+"'");
                                            if (GetTable.Rows.Count > 0)
                                            {
                                                int GetSleep = DataHelper.StrToInt(DataHelper.ObjToStr(GetTable.Rows[0]["Sleep"]));

                                                bool Active = DataHelper.StrToBool(DataHelper.ObjToStr(GetTable.Rows[0]["Active"]));
                                                if (Active)
                                                {
                                                    for (int ir = 0; ir < GetSleep; ir++)
                                                    {
                                                        Thread.Sleep(1000);
                                                    }

                                                    string RunCode = DataHelper.ObjToStr(GetTable.Rows[0]["RunCode"]);

                                                    if (RunCode.Contains("ReadAllTemplate;"))
                                                    {
                                                        HtmlCreatEngine.ReadAllTemplate(DeFine.TemplatesPath, true);
                                                    }

                                                    bool While = DataHelper.StrToBool(DataHelper.ObjToStr(GetTable.Rows[0]["While"]));
                                                    if (!While)
                                                    {
                                                        ISWhile = false;
                                                    }
                                                }
                                                else
                                                {
                                                    Thread.Sleep(1000);
                                                }
                                            }
                                            else { break; }
                                        }
                                        else
                                        {
                                            Thread.Sleep(1000);
                                            object While = SqlServerHelper.ExecuteScalar("Select While From ThreadWorking Where WorkAction ='"+ CurrentWorkAction + "'");
                                            if (While == null) 
                                            { 
                                                break;
                                            }
                                            else
                                            {
                                                if (DataHelper.StrToBool(While.ToString()))
                                                {
                                                    ISWhile = true;
                                                }
                                                else
                                                {
                                                    ISWhile = false;
                                                }
                                            }
                                        }
                                    }
                                    WorkingTime.LockerWorkingTimeThread--;
                                });

                                if (RunPool.ToLower().Contains("UserPool".ToLower()))
                                {
                                    CoreHelper.RunCodeNoParam(new ThreadWork("User", "", CallLocation.UserPool, CreatAction));
                                }
                                if (RunPool.ToLower().Contains("SystemPool".ToLower()))
                                {
                                    CoreHelper.RunCodeNoParam(new ThreadWork("System", "", CallLocation.SystemPool, CreatAction));
                                }
                                if (RunPool.ToLower().Contains("EmergencyPool".ToLower()))
                                {
                                    CoreHelper.RunCodeNoParam(new ThreadWork("Emergency", "", CallLocation.EmergencyPool, CreatAction));
                                }
                                if (RunPool.ToLower().Contains("QueuePool".ToLower()))
                                {
                                    CoreHelper.RunCodeNoParam(new ThreadWork("Queue", "", CallLocation.QueuePool, CreatAction));
                                }

                            }
                          
                        }
                    }).Start();
                }
            }
            if (!check)
            {
                WorkingLocker = false;
                CloseAllWorkingTimeThread = true;
            }
                
        }


        
    }
}