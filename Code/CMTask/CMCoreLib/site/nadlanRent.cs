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
    class nadlanRent : baseSite
    {
        public const string mainTableId = "savedAddsTable";
        private IWebElement main_table;
        private const int minRows = 7, maxRows = 12;
        private DateTime pageDate = DateTime.MinValue;
             private int compareDate = -2;
        
        internal nadlanRent(int taskId, string ETaskSource, DateTime pageDate)
            : base(taskId, @"nadlanRent\errorHandlerLog\", "nadlanRent.log", @"nadlanRent\infoHandlerLog\", "nadlanRent.log", ETaskSource)
        {
            
            this.pageDate = pageDate;
            
        }

        public static string getPageUrl(string pageNum)
        {
            return "http://www.ad.co.il/nadlanrent?pageindex="+pageNum;
        }

     
        public static string getBasePageUrl()
        {
            return "http://www.ad.co.il/nadlanrent";
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
            string[] cols = { "Type", "Source","EntrenceDate", "Size", "Veranda", "Rooms", "City", "Floor", "Address","Region", "Neighborhood", "Phone1", "Name", "Price", "phone2", "FreeText","RowDate", "DownloadStatus","TaskId" };
            return cols;
        }

        
        private TTaskStatusType getBasisTable()
        {
            try
            {
                System.Collections.ObjectModel.ReadOnlyCollection<IWebElement> table = myDriver.WebDriver.FindElements(By.ClassName("nadlan-thumbnail-container"));
                System.Collections.ObjectModel.ReadOnlyCollection<IWebElement> lines = table[0].FindElements(By.ClassName("col-sm-6"));
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
                    string entrenceDate = lines[i].GetAttribute("data-enterdate");
                    string address = lines[i].GetAttribute("data-address");
                    string areasize = lines[i].GetAttribute("data-areasize");
                    string veranda = lines[i].GetAttribute("data-verandas");
                    string rooms = lines[i].GetAttribute("data-rooms");
                    string city = lines[i].GetAttribute("data-city");
                    string floor = lines[i].GetAttribute("data-floor");
                    string created = lines[i].GetAttribute("data-created");
                    string curDate = driverUtils.ExecuteScript(myDriver.WebDriver, "var myDate = new Date(" + created + "  );" + "return myDate.toGMTString()");
                    if (curDate == "")
                        return TTaskStatusType.Failed;
                    DateTime tmpDate = DateTime.MinValue;
                    if (!DateTime.TryParse(curDate, out tmpDate))
                        return TTaskStatusType.Failed;
                    string hood = lines[i].GetAttribute("data-hood");
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
                    string contact = lines[i].GetAttribute("data-contact");
                    string price = lines[i].GetAttribute("data-price");
                    string area = lines[i].GetAttribute("data-salearea");
                    string type = lines[i].GetAttribute("data-saletype");
                    taskTable.Rows.Add(type, "Yad2", entrenceDate, areasize, veranda, rooms, city, floor,address, area, hood, phone1, contact, price, phone2, desc, tmpDate.ToShortDateString(), "Success", taskId);
                } 
                SitesBL.addNadlanRentTable(taskTable);
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
                    if (page == 1707)
                        return TTaskStatusType.Success;
                    base.TaskBL.updateTaskMinPage(taskId, page.ToString());
                    //updatetaskMinPage()
                    url = getPageUrl(page.ToString());
                }
                else
                    return stat;

            }
            if (TaskBL.AddTask(1, "GetPageData", CMLib.DateTimeSQLite(pageDate), "", TSites.NadlanRent.ToString(), page.ToString()))
                return TTaskStatusType.Success;
            return TTaskStatusType.Failed;
        }

     
        private bool initCity(string cityName)
        {
            
            try
            {
                System.Collections.ObjectModel.ReadOnlyCollection<IWebElement> filters = myDriver.WebDriver.FindElements(By.ClassName("filtermore"));
                filters[1].Click();
                driverUtils.Sleep(5000, 7000);
                System.Collections.ObjectModel.ReadOnlyCollection<IWebElement> fancybox = myDriver.WebDriver.FindElements(By.ClassName("fancybox-skin"));
                System.Collections.ObjectModel.ReadOnlyCollection<IWebElement> citiesul = fancybox[0].FindElements(By.XPath(".//ul"));

                System.Collections.ObjectModel.ReadOnlyCollection<IWebElement> cities = citiesul[0].FindElements(By.XPath(".//li"));

                for (int i = 0; i < cities.Count; i++)
                {
                    if (cities[i].Text.Contains(cityName))
                    {
                        System.Collections.ObjectModel.ReadOnlyCollection<IWebElement> cityLink = cities[i].FindElements(By.XPath(".//a"));
                        cityLink[0].Click();
                        return true;
                    }
                }
                return false;
            }
            catch (Exception ex)
            {
                errorLog.handleException(ex);
                errorLog.writeToLogFile("at initCity  " + ex.StackTrace);
                return false;
            }
        }
   
     }

}
