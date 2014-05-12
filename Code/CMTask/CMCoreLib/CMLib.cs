using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using CMCore.webdriver;
using CMCore.site;
using CMCore.task;
using System.Data;
using ManageLogFile;
using OpenQA.Selenium;
using System.Collections.ObjectModel;
using CMCore.webtools;
using OpenQA.Selenium.Firefox;


namespace CMCore
{
    public class CMLib
    {
        public static string DateTimeSQLite(DateTime datetime)
        {
            return datetime.ToString("yyyy-MM-dd");
        }

        public void start(string taskId)
        {
            TaskManager TM = TaskManager.Instance;
            TM.getTask(taskId);
               
            
        }
    }
}










