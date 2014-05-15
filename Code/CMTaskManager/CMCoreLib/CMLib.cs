using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using CMCore.task;
using System.Data;
using ManageLogFile;
using OpenQA.Selenium;
using System.Collections.ObjectModel;

namespace CMCore
{
    public class CMLib
    {
        public static string DateTimeSQLite(DateTime datetime)
        {
            return datetime.ToString("yyyy-MM-dd");
        }

        public void start()
        {
            TaskManager TM = TaskManager.Instance;
            while (true)
            {
                TM.updateTasks();
                System.Threading.Thread.Sleep(20000);
            }
        }
    }
}










