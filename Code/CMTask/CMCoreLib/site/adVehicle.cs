 
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;
using OpenQA.Selenium.Support.UI;
using System.Data;
using OpenQA.Selenium.Remote;
using OpenQA.Selenium.Firefox;
using System.IO;
using CMCore.webdriver;
using CMCore.data;
using CMCore.task;
using System.Collections.ObjectModel;
using BLL.BLL;
  
namespace CMCore.site
{
    class adVehicle : baseSite
    {
        public const string mainTableId = "savedAddsTable";
        private IWebElement main_table;
        private const int minRows = 7, maxRows = 12;
        private DateTime pageDate = DateTime.MinValue;
             private int compareDate = -2;
        
        internal adVehicle(int taskId, string ETaskSource, DateTime pageDate)
            : base(taskId, @"adVehicle\errorHandlerLog\", "adVehicle.log", @"adVehicle\infoHandlerLog\", "adVehicle.log", ETaskSource)
        {
            
            this.pageDate = pageDate;
            
        }

        public static string getPageUrl(string pageNum)
        {
            return "http://www.ad.co.il/car?pageindex=" + pageNum;
        }

     
        public static string getBasePageUrl()
        {
            return "http://www.ad.co.il/car";
        }

        public TTaskStatusType getPageData(string curPage)
        {
            if (myDriver == null)
                return TTaskStatusType.DriverError;
            TTaskStatusType downloadStatusType = getSitePageData(curPage);
            release(downloadStatusType.ToString());
            return downloadStatusType;
        }
 
        private string[] getPageTableCols()
        {
            string[] cols = { "Source", "Manufacture","Model", "Year", "Hand", "Odometer", "Gear", "CarEngine", "EngineCapacity","Ownership", "Area", "City","Price", "Name", "Phone1", "Phone2","FreeText","RowDate", "DownloadStatus","TaskId" };
            return cols;
        }

        
        private TTaskStatusType getBasisTable()
        {
            try
            {
                System.Collections.ObjectModel.ReadOnlyCollection<IWebElement> container = myDriver.WebDriver.FindElements(By.ClassName("sec-thumbnail-container"));

                System.Collections.ObjectModel.ReadOnlyCollection<IWebElement> lines = container[0].FindElements(By.ClassName("col-sm-6"));
                if (lines.Count == 0)
                    return TTaskStatusType.Failed;
                DataTable taskTable = new DataTable();
                string[] cols = getPageTableCols();
                foreach (string col in cols)
                    taskTable.Columns.Add(col, typeof(string));
                myDriver.WebDriver.SwitchTo().DefaultContent();
                for (int i = 0; i < lines.Count; i++)
                {
                    string source = lines[i].GetAttribute("data-src");
                    if (source == "1")
                        source = "Yad2";
                    else if (source == "2")
                        source = "Winwin";
                    else if (source == "3")
                        source = "Homeless";
                    else
                        source = "";
                    string Manufacture = lines[i].GetAttribute("data-man");
                    string Model = lines[i].GetAttribute("data-model");
                    string Year = lines[i].GetAttribute("data-year");
                    string Hand = lines[i].GetAttribute("data-hand");
                    string Odometer = lines[i].GetAttribute("data-km");
                    string Gear = lines[i].GetAttribute("data-trans");
                    string CarEngine = lines[i].GetAttribute("data-enginetype");
                    string EngineCapacity = lines[i].GetAttribute("data-enginevol");
                    string created = lines[i].GetAttribute("data-created");

                    string curDate = driverUtils.ExecuteScript(myDriver.WebDriver, "var myDate = new Date(" + created + "  );" + "return myDate.toGMTString()");
                    if (curDate == "")
                        return TTaskStatusType.Failed;
                    DateTime tmpDate = DateTime.MinValue;
                    if (!DateTime.TryParse(curDate, out tmpDate))
                        return TTaskStatusType.Failed;
                    string Ownership = lines[i].GetAttribute("data-currholder");
                    string Area = lines[i].GetAttribute("data-salearea");
                    string City = lines[i].GetAttribute("data-city");
                    string Price = lines[i].GetAttribute("data-price");
                    string Name = lines[i].GetAttribute("data-contact");
                    string phone = lines[i].GetAttribute("data-phone");
                    string phone1 = "";
                    string phone2 = "";
                    if (phone.IndexOf('/') > 0)
                    {
                        string[] phones = phone.Split('/');
                        phone1 = phones[0];
                        phone2 = phones[1];
                    }
                    else
                    {
                        phone1 = phone;
                        phone2 = "";
                    }
                    string desc = lines[i].GetAttribute("data-desc").Replace("'", "''");


                    taskTable.Rows.Add(source, Manufacture, Model, Year, Hand, Odometer, Gear, CarEngine, EngineCapacity, Ownership, Area, City, Price, Name, phone1, phone2, desc, tmpDate.ToShortDateString(), "Success", taskId);
                }
                   
                  
                    
                    SitesBL.addAdVehicleTable(taskTable);
                return TTaskStatusType.Success;
            }
            catch (Exception ex)
            {
                errorLog.handleException(ex);
                errorLog.writeToLogFile("at getBasisTable  " + ex.StackTrace);
                return TTaskStatusType.Failed;
            }
        }

   
    
        private TTaskStatusType getSitePageData(string curPage)
        {
            string url=getBasePageUrl();
            int page = 1;
            if (curPage != "")
            {
                 url = getPageUrl(curPage);
                 int.TryParse(curPage, out page);
            }
                  
            for (int i = 0; i < 5;i++ )
            {
                if (!driverUtils.NevigateToPage(myDriver.WebDriver, url))
                {
                    release(TTaskStatusType.DriverError.ToString());
                    return TTaskStatusType.DriverError;
                }
                TTaskStatusType stat = getBasisTable();
                if (stat == TTaskStatusType.Success)
                {
                    page++;
                    if (page == 2331)
                        return TTaskStatusType.Success;
                    base.TaskBL.updateTaskMinPage(taskId, page.ToString());
                    //updatetaskMinPage()
                    url = getPageUrl(page.ToString());
                }
                else
                    return stat;

            }
            if (TaskBL.AddTask(1, "GetPageData", CMLib.DateTimeSQLite(pageDate), "", TSites.AdVehicle.ToString(), page.ToString()))
                return TTaskStatusType.Success;
            return TTaskStatusType.Failed;
        }

     
       
   
     }

}

