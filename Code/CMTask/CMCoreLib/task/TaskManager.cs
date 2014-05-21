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
                    case "Yad2":
                    case "Winwin":
                    case "Homeless":
                    case "WinwinProfessional":
                    case "HomelessClasses":
                    case "HomelessWorkshop":
                    case "HomelessCourses":
                    case "HomelessLecture":
                    case "HomelessPrivateClasses":
                    case "HomelessMeetings":
                    case "HomelessVehicle":
                    case "WinwinVehicle":
                    case "NadlanRent":
                    case "AdVehicle":
                    case "NadlanSale":
                    case "Freelancerim":
                        
                        switch (task.TaskName)
                        {
                            case "GetPageData":

                                if (task.ETaskSource == "Yad2")
                                {
                                    Y2 site = new Y2(task.Id, task.ETaskSource, task.TaskDate,task.CityName);
                                    downloadStatusType = site.getPageData(task.MinPage);
                                }
                                else if (task.ETaskSource == "Freelancerim")
                                {
                                    Freelancerim site = new Freelancerim(task.Id);
                                    downloadStatusType = site.getPageData(task.MinPage);
                                }
                                else if (task.ETaskSource == "NadlanSale")
                                {
                                    NadlanSale site = new NadlanSale(task.Id, task.ETaskSource, task.TaskDate);
                                    downloadStatusType = site.getPageData(task.MinPage);
                                }
                                else if (task.ETaskSource == "AdVehicle")
                                {
                                    adVehicle site = new adVehicle(task.Id, task.ETaskSource, task.TaskDate);
                                    downloadStatusType = site.getPageData(task.MinPage);
                                }
                                else if (task.ETaskSource == "NadlanRent")
                                {
                                    nadlanRent site = new nadlanRent(task.Id, task.ETaskSource, task.TaskDate);
                                    downloadStatusType = site.getPageData(task.MinPage);
                                }
                                else if (task.ETaskSource == "Homeless")
                                {
                                    Homeless site = new Homeless(task.Id, task.ETaskSource, task.TaskDate);
                                    downloadStatusType = site.getPageData(task.MinPage);
                                }
                                else if (task.ETaskSource == "WinwinVehicle")
                                {
                                    WinwinVehicle site = new WinwinVehicle(task.Id, task.TaskDate,task.CityName);
                                    downloadStatusType = site.getPageData(task.MinPage);
                                }
                                else if (task.ETaskSource == "HomelessVehicle")
                                {
                                    HomelessVehicle site = new HomelessVehicle(task.Id, task.ETaskSource, task.TaskDate);
                                    downloadStatusType = site.getPageData(task.MinPage);
                                }
                                else if (task.ETaskSource == "WinwinProfessional")
                                {
                                    WinwinProfessional site = new WinwinProfessional(task.Id);
                                    SiteBase = site;
                                    downloadStatusType = site.getPageData(task.MinPage);
                                }
                                else if (task.ETaskSource == "Winwin")
                                {
                                    Winwin site = new Winwin(task.Id, task.ETaskSource, task.TaskDate, task.CityName);
                                    SiteBase = site;
                                    downloadStatusType = site.getPageData(task.MinPage);
                                }
                                else if (task.ETaskSource == "HomelessClasses")
                                {
                                    HomelessClasses site = new HomelessClasses(task.Id);
                                    SiteBase = site;
                                    downloadStatusType = site.getPageData(task.MinPage, "Classes");

                                }
                                else if (task.ETaskSource == "HomelessCourses")
                                {
                                    HomelessClasses site = new HomelessClasses(task.Id);
                                    SiteBase = site;
                                    downloadStatusType = site.getPageData(task.MinPage, "Courses");

                                }
                                else if (task.ETaskSource == "HomelessWorkshop")
                                {
                                    HomelessClasses site = new HomelessClasses(task.Id);
                                    SiteBase = site;
                                    downloadStatusType = site.getPageData(task.MinPage, "Workshop");

                                }
                                else if (task.ETaskSource == "HomelessLecture")
                                {
                                    HomelessClasses site = new HomelessClasses(task.Id);
                                    SiteBase = site;
                                    downloadStatusType = site.getPageData(task.MinPage, "Lecture");

                                }
                                else if (task.ETaskSource == "HomelessPrivateClasses")
                                {
                                    HomelessClasses site = new HomelessClasses(task.Id);
                                    SiteBase = site;
                                    downloadStatusType = site.getPageData(task.MinPage, "PrivateClasses");

                                }
                                else if (task.ETaskSource == "HomelessMeetings")
                                {
                                    HomelessClasses site = new HomelessClasses(task.Id);
                                    SiteBase = site;
                                    downloadStatusType = site.getPageData(task.MinPage, "Meetings");

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
