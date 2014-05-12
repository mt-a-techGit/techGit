using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data;
using ManageLogFile;
using BLL.BLL;
using OpenQA.Selenium;
using System.Threading;
using System.Diagnostics;
using System.IO;

namespace CMCore.task
{
    public enum TTaskStatusType
    {
        Success,
        Failed,
        DriverError,
        UnKnownError,
        SiteBlock
    }

    public class TaskManager
    {
        private LogFile errorLog = new LogFile(@"TaskManager\errorHandlerLog\", "TaskManager.log");
        private LogFile infoLog = new LogFile(@"TaskManager\infoHandlerLog\", "TaskManager.log");
        private static object syncRoot = new Object();
        private static volatile TaskManager instance;

        public static TaskManager Instance
        {
            get
            {
                if (instance == null)
                {
                    lock (syncRoot)
                    {
                        if (instance == null)
                            instance = new TaskManager();
                    }
                }
                return instance;
            }
        }

        private TaskManager()
        {
            tasksBL = new TaskBL(errorLog, infoLog);

        }

        int maxTasksNum =4;
        BLL.BLL.TaskBL tasksBL;

        public void updateTasks()
        {

            BLL.BLL.TaskBL TaskBL = new BLL.BLL.TaskBL(errorLog, infoLog);

            lock (this)
            {
                Path.
                DataTable tasks = tasksBL.getTasks(maxTasksNum);
                if (tasks == null)
                    return;
                for (int i = 0; i < tasks.Rows.Count; i++)
                {
                    int Id = 0;
                    int.TryParse(tasks.Rows[i]["Id"].ToString(), out Id);
                    Process ffmpeg = new Process();
                    ffmpeg.StartInfo.FileName =@"C:\Users\MyPcUser\Desktop\CMNew\CM2\CMTask\CM\bin\Debug\CM.exe";
                    ffmpeg.StartInfo.Arguments = tasks.Rows[i]["Id"].ToString();
                    ffmpeg.Start();
                }
           }
        }




      
    
       

    }
}
