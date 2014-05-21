using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data;
using OpenQA.Selenium;
using OpenQA.Selenium.Support.UI;

using CMCore.webdriver;
using CMCore.data;
using ManageLogFile;
using CMCore.driver;
using CMCore.task;
 
using BLL.BLL;

namespace CMCore.site
{
    public enum TSites
    {
        Yad2,
        Easy,
        Winwin,
        TestDeviceAgent,
        Homeless,
        WinwinProfessional,
        HomelessClasses,
        WinwinVehicle,
        HomelessVehicle,
        NadlanRent,
        AdVehicle,
        NadlanSale,
        Freelancerim
    }

    public abstract class baseSite
    {
        public class siteException : Exception
        {
            public TTaskStatusType siteDownloadStatusType { get; set; }

            public siteException(string message, TTaskStatusType pageDataException)
                : base(message)
            {

                this.siteDownloadStatusType = siteDownloadStatusType;
            }
        }
        protected ProxyBL blProxie;
        protected int sMinWaitForLine = 70000;
        protected int sMaxWaitForLine = 150000;
        protected int sMinWaitForActionInLine = 20000;
        protected int sMaxWaitForActionInLine = 60000;
        protected int taskId;
        protected SiteDataSet mDataSet = new SiteDataSet();
        protected Driver myDriver;
        private WebDriverProvider DriverProvider = WebDriverProvider.Instance;
        protected LogFile errorLog;
        protected LogFile infoLog;
        protected TaskBL TaskBL;
        protected SitesBL SitesBL; 
        internal baseSite(int taskId, string errorLogPath, string errorLogName, string infoLogPath, string infoLogName, string ETaskSource)
        {
            this.taskId = taskId;
            initDriver(ETaskSource);
            initLogs(errorLogPath, errorLogName, infoLogPath, infoLogName);
            TaskBL = new TaskBL(errorLog, infoLog);
            blProxie = new ProxyBL(errorLog, infoLog);
            SitesBL = new SitesBL(errorLog, infoLog);
        }

        private void initDriver(string ETaskSource)
        {
            // todo get driver by profile
            this.myDriver = DriverProvider.getAvaiableDriver(ETaskSource);
        }

        private void initLogs(string errorLogPath, string errorLogName, string infoLogPath, string infoLogName)
        {
            errorLog = new LogFile(errorLogPath, errorLogName);
            infoLog = new LogFile(infoLogPath, infoLogName);
        }

        

        protected void release(string Status)
        {
            string stat = "Failed";
                if (Status == "Success")
                    stat = "Success";
                TaskBL.UpdateTaskStatus(taskId, stat);
            if (myDriver.WebDriver != null)
            {
                myDriver.WebDriver.Quit();
                blProxie.SetProxyStatus("Success", myDriver.httpProxy);
                
            }
        }

    }
}
