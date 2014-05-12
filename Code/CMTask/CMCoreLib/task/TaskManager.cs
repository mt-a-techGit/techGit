using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data;
using CMCore.site;
using ManageLogFile;
using BLL.BLL;
using OpenQA.Selenium;
using OpenQA.Selenium.Firefox;
using System.Threading;

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
 
        BLL.BLL.TaskBL tasksBL;

        public void getTask(string taskId)
        {

            BLL.BLL.TaskBL TaskBL = new BLL.BLL.TaskBL(errorLog, infoLog);
            DataTable tasks = tasksBL.getTask(taskId);
            if (tasks == null)
                return;
            for (int i = 0; i < tasks.Rows.Count; i++)
            {
                int Id = 0;
                string MinPage = "";
                int.TryParse(tasks.Rows[i]["Id"].ToString(), out Id);
                string ETaskSource = tasks.Rows[i]["ETaskSource"].ToString();
                string TaskName = tasks.Rows[i]["TaskName"].ToString();
                MinPage = tasks.Rows[i]["MinPage"].ToString();
                string TaskDateStr = tasks.Rows[i]["TaskDate"].ToString();
                DateTime TaskDate = DateTime.MinValue;
                DateTime.TryParse(TaskDateStr, out TaskDate);
                string CityName = tasks.Rows[i]["CityName"].ToString();
                ScheduledTask task = new ScheduledTask(Id, ETaskSource, TaskName, TaskDate, CityName, MinPage);
                monitorTasks(task);
            }

            
        }
  
        private void monitorTasks(ScheduledTask task)
        {
            LogFile errorLogTask = new LogFile(@"TaskManager\errorHandlerLog\", "TaskManager.log");
            LogFile infoLogTask = new LogFile(@"TaskManager\infoHandlerLog\", "TaskManager.log");
            baseSite SiteBase = null;
            TTaskStatusType downloadStatusType = TTaskStatusType.Failed;
            try
            {



                switch (task.ETaskSource)
                {
                    case "Winwin":
                        string url = "";
                    switch (task.TaskName)
                    {
                        case "GetPageData":
                            if (task.ETaskSource == "Winwin")
                            {
                                Winwin site = new Winwin(task.Id, task.ETaskSource, task.TaskDate, task.CityName);
                                SiteBase = site;
                                downloadStatusType = site.getPageData(task.MinPage);
                            }
                       break;
                    }
                    break;
                }
            }

            catch (Exception ex)
            {
                downloadStatusType = TTaskStatusType.Failed;
                errorLogTask.handleException(ex);
                errorLogTask.writeToLogFile("at DB GetTasks");
            }
           
        }

        

         

    }
}
