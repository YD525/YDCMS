using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using WebMaster.UserManager;

namespace WebMaster.CoreManager
{
    public class CoreHelper
    {
        public static int QueuePoolThreadCount = 0;

        public static int UserPoolThreadCount = 0;

        public static int SystemPoolThreadCount = 0;

        public static int EmergencyPoolThreadCount = 0;

        public static Queue<ThreadWork> QueuePool = new Queue<ThreadWork>();
        public static Queue<ThreadWork> UserPool = new Queue<ThreadWork>();
        private static Queue<ThreadWork> SystemPool = new Queue<ThreadWork>();
        public static Queue<ThreadWork> EmergencyPool = new Queue<ThreadWork>();

        public static int MaxQueuePoolCount = 2;
        public static int MaxUserPoolCount = 3;
        public static int MaxSystemPoolCount = 5;
        public static int MaxEmergencyPoolCount = 10;

        public static int QueuePoolSleep = 300;
        public static int UserPoolSleep = 200;
        public static int SystemPoolSleep = 100;
        public static int EmergencyPoolSleep = 10;

        public static int CurrentThreadCount = 0;
        public static int CurrentBigCall = 0;

        public delegate object GetQueue(ref Queue<ThreadWork> List);
        /// <summary>
        /// 执行但无回显
        /// </summary>
        public static void RunCodeNoParam(ThreadWork WorkItem)
        {
            new Thread(() =>
            {

                CurrentThreadCount++;

            }).Start();
         
            try
            {
                if (WorkItem.SqlOder == null) WorkItem.SqlOder = "";
                if (WorkItem.DepositPool == CallLocation.NewThread)
                {
                    new Thread(() =>
                    {
                        if (WorkItem.LockerThreadAction == null == false) WorkItem.LockerThreadAction.Invoke();
                        if (WorkItem.SqlOder.Length > 0) SqlServerHelper.ExecuteNonQuery(WorkItem.SqlOder);
                    }).Start();

                    return;
                }

                if (WorkItem.DepositPool == CallLocation.EmergencyPool)
                {
                    EmergencyPool.Enqueue(WorkItem);
                }
                else
                if (WorkItem.DepositPool == CallLocation.QueuePool)
                {
                    QueuePool.Enqueue(WorkItem);
                }
                else
                if (WorkItem.DepositPool == CallLocation.UserPool)
                {
                    UserPool.Enqueue(WorkItem);
                }
                else
                if (WorkItem.DepositPool == CallLocation.SystemPool)
                {
                    SystemPool.Enqueue(WorkItem);
                }

            }
            catch
            {
                GC.Collect();
            }
        }

        public static bool LockerCoreService = false;


        public static void ThreadCoreService(bool check)
        {
            if (check)
            {
                if (!LockerCoreService)
                {
                    LockerCoreService = true;
                    GetQueue GetQueueItem = new GetQueue(GetQueueWorkItem);
                    new Thread(() =>
                    {
                        while (LockerCoreService)
                        {
                            Thread.Sleep(3000);
                            try 
                            { 
                            if (CurrentBigCall < CurrentThreadCount)
                            {
                                CurrentBigCall = CurrentThreadCount;
                            }
                            CurrentThreadCount = 0;
                            }
                            catch { }
                        }
                    }).Start();
                    new Thread(() =>
                    {

                        while (LockerCoreService)
                        {
                            //Is Def
                            if (QueuePool.Count > 0)
                            {
                                if (QueuePoolThreadCount > MaxQueuePoolCount)
                                {
                                    Thread.Sleep(QueuePoolSleep * 3);
                                }
                                else
                                {
                                        new Thread(() =>
                                        {
                                        QueuePoolThreadCount++;
                                        Thread.Sleep(QueuePoolSleep);
                                        object GetReturn = GetQueueItem.Invoke(ref QueuePool);
                                        if (GetReturn == null == false)
                                        {
                                            ThreadWork GetWork = (ThreadWork)GetReturn;
                                            if (GetWork.LockerThreadAction == null == false) GetWork.LockerThreadAction.Invoke();
                                            if (GetWork.SqlOder == null == false)
                                                if (GetWork.SqlOder.Length > 0) SqlServerHelper.ExecuteNonQuery(GetWork.SqlOder);
                                        }
                                        QueuePoolThreadCount--;
                                        }).Start();
                                }
                            }
                            else
                            {
                                Thread.Sleep(QueuePoolSleep * 3);
                            }

                        }

                    }).Start();


                    new Thread(() =>
                    {

                        while (LockerCoreService)
                        {

                            //IsUser
                            if (UserPool.Count > 0)
                            {
                                if (UserPoolThreadCount > MaxUserPoolCount)
                                {
                                    Thread.Sleep(UserPoolSleep * 3);
                                }
                                else
                                {

                                        new Thread(() =>
                                        {
                                        UserPoolThreadCount++;
                                        Thread.Sleep(UserPoolSleep);
                                        object GetReturn = GetQueueItem.Invoke(ref UserPool);
                                        if (GetReturn == null == false)
                                        {
                                            ThreadWork GetWork = (ThreadWork)GetReturn;
                                            if (GetWork.LockerThreadAction == null == false) GetWork.LockerThreadAction.Invoke();
                                            if (GetWork.SqlOder == null == false)
                                                if (GetWork.SqlOder.Length > 0) SqlServerHelper.ExecuteNonQuery(GetWork.SqlOder);
                                        }
                                        UserPoolThreadCount--;
                                        }).Start();

                                }
                            }
                            else
                            {
                                Thread.Sleep(UserPoolSleep * 3);
                            }
                        }

                    }).Start();

                    new Thread(() =>
                    {

                        while (LockerCoreService)
                        {
                            //IsSystem
                            if (SystemPool.Count > 0)
                            {
                                if (SystemPoolThreadCount > MaxSystemPoolCount)
                                {
                                    Thread.Sleep(SystemPoolSleep * 3);
                                }
                                else
                                {
                                    new Thread(() =>
                                    {
                                    SystemPoolThreadCount++;
                                    Thread.Sleep(SystemPoolSleep);
                                    object GetReturn = GetQueueItem.Invoke(ref SystemPool);
                                    if (GetReturn == null == false)
                                    {
                                        ThreadWork GetWork = (ThreadWork)GetReturn;
                                        if (GetWork.LockerThreadAction == null == false) GetWork.LockerThreadAction.Invoke();
                                        if (GetWork.SqlOder == null == false)
                                            if (GetWork.SqlOder.Length > 0) SqlServerHelper.ExecuteNonQuery(GetWork.SqlOder);
                                    }
                                    SystemPoolThreadCount--;
                                    }).Start();
                                }
                            }
                            else
                            {
                                Thread.Sleep(SystemPoolSleep * 3);
                            }
                        }

                    }).Start();

                    new Thread(() =>
                    {

                        while (LockerCoreService)
                        {
                            //IsEmergency
                            if (EmergencyPool.Count > 0)
                            {
                                if (EmergencyPoolThreadCount > MaxEmergencyPoolCount)
                                {
                                    Thread.Sleep(EmergencyPoolSleep * 3);
                                }
                                else
                                {
                                    new Thread(() =>
                                    {
                                    EmergencyPoolThreadCount++;
                                    Thread.Sleep(SystemPoolSleep);
                                    object GetReturn = GetQueueItem.Invoke(ref EmergencyPool);
                                    if (GetReturn == null == false)
                                    {
                                        ThreadWork GetWork = (ThreadWork)GetReturn;
                                        if (GetWork.LockerThreadAction == null == false) GetWork.LockerThreadAction.Invoke();
                                        if (GetWork.SqlOder == null == false)
                                            if (GetWork.SqlOder.Length > 0) SqlServerHelper.ExecuteNonQuery(GetWork.SqlOder);
                                    }
                                    EmergencyPoolThreadCount--;
                                    }).Start();
                                }
                            }
                            else
                            {
                                Thread.Sleep(SystemPoolSleep * 3);
                            }
                        }

                    }).Start();
                }
            }
            else
            {
                LockerCoreService = false;
            }

        }


        public static object GetQueueWorkItem(ref Queue<ThreadWork> List)
        {
            if (List.Count > 0)
            {
                try
                {
                    return List.Dequeue();
                }
                catch
                {
                    return new ThreadWork();
                }
            }
            return null;
        }
    }


    public class PoolValue
    {
        public int QueuePoolThreadCount = 0;

        public int UserPoolThreadCount = 0;

        public int SystemPoolThreadCount = 0;

        public int EmergencyPoolThreadCount = 0;
    }
    public enum CallLocation { QueuePool = 0, UserPool = 1, SystemPool = 2, EmergencyPool = 3, NewThread = 4 }
    public struct ThreadWork
    {
        public Action LockerThreadAction;
        public string LockerCallUserId;
        public CallLocation DepositPool;
        public DateTime CallTime;
        public string SqlOder;
        public ThreadWork(usertoken LockerUserToken, string SqlOder, CallLocation DepositPool = CallLocation.QueuePool, Action LockerAction = null)
        {
            this.LockerCallUserId = LockerUserToken.username;
            this.LockerThreadAction = LockerAction;
            this.DepositPool = DepositPool;
            this.SqlOder = SqlOder;
            this.CallTime = DateTime.Now;
        }

        public ThreadWork(string LockerID, string SqlOder, CallLocation DepositPool = CallLocation.QueuePool, Action LockerAction = null)
        {
            this.LockerCallUserId = LockerID;
            this.LockerThreadAction = LockerAction;
            this.DepositPool = DepositPool;
            this.SqlOder = SqlOder;
            this.CallTime = DateTime.Now;
        }
    }
}
