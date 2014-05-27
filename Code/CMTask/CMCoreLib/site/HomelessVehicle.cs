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
using System.Net.Sockets;


namespace CMCore.site
{
   
    class HomelessVehicle : baseSite
    {
        protected SiteDataSet mDataSet = new SiteDataSet();
        ProxyBL blProxie;
        public const string mainTableId = "mainresults";
        private IWebElement main_table;
        private const int maxRows = 14, minRows = 8;
        private DateTime pageDate = DateTime.MinValue;
       
        private int compareDate = -2;
         
        internal HomelessVehicle(int taskId, string ETaskSource, DateTime pageDate)
            : base(taskId, @"HomelessVehicle\errorHandlerLog\", "HomelessVehicle.log", @"HomelessVehicle\infoHandlerLog\", "HomelessVehicle.log", ETaskSource)
        {
                this.pageDate = pageDate;
        }
 
        public static string getBasePageUrl()
        {
            return "http://www.homeless.co.il/private/4";
        }

        public TTaskStatusType getPageData(string MinPage)
        {
            try
            { 
            if (myDriver == null)
                return TTaskStatusType.DriverError;
            TTaskStatusType downloadStatusType = getSitePageData(MinPage);
            release(downloadStatusType.ToString());
            return downloadStatusType;
            }
            catch (Exception ex)
            {
                release(TTaskStatusType.Failed.ToString());
                return TTaskStatusType.Failed;
            }
        }
  
        private string[] getPageTableCols()
        {
            string[] cols = { "Type", "EngineCapacity", "Year", "PrevOwnersNum", "Price", "Area", "RowDate", "CarEngine", "Gear", "Odometer", "Agency", "FreeText", "Name", "Phone1", "Phone2" };
            return cols;
        }

        private bool getRowData(DataRow pageTableRow, string Id)
        {
            try
            {
                driverUtils.Sleep(5000, 6000);
                myDriver.WebDriver.SwitchTo().DefaultContent();
                myDriver.WebDriver.SwitchTo().Frame(Id);
                System.Collections.ObjectModel.ReadOnlyCollection<IWebElement> details = myDriver.WebDriver.FindElements(By.ClassName("details"));
                System.Collections.ObjectModel.ReadOnlyCollection<IWebElement> lines = details[0].FindElements(By.XPath(".//div"));
                 

                for (int i = lines.Count-1; i  >=0 ; i--)
                {
                    if (lines[i].Text == "" || lines[i].Text.Length > 150)
                        continue;
                    if (lines[i].Text.IndexOf("סוג מנוע:") > -1)
                    {
                        
                        string myString = lines[i].Text.Replace("סוג מנוע:", string.Empty);
                        pageTableRow["CarEngine"] = myString;
                    }
                    else if (lines[i].Text.IndexOf("ת. הילוכים:") > -1)
                    {
                         
                        pageTableRow["Gear"] = lines[i].Text.Replace("ת. הילוכים:", "").Trim();
                    }
                    else if (lines[i].Text.IndexOf("רכב מסוכנות") > -1)
                    {

                        pageTableRow["Agency"] = "True";
                    }
                    else if (lines[i].Text.Replace("'", "''").IndexOf("קילומטראז :") > -1)
                    {
                       
                        string myString = lines[i].Text.Replace("'", "''").Replace("קילומטראז :", string.Empty).Trim();
                        pageTableRow["Odometer"] = myString;

                    }
                    else if (lines[i].Text.Replace("'", "''").IndexOf("קילומטראז :") > -1)
                    {
                  
                        string myString = lines[i].Text.Replace("'", "''").Replace("קילומטראז :", string.Empty).Trim();
                        pageTableRow["Odometer"] = myString;

                    }
                    if (lines[i].Text.IndexOf("איש קשר:") > -1)
                    {
                      
                        string myString = lines[i].Text.Replace("'", "''").Replace("\r\n", string.Empty).Trim();
                        myString = myString.Replace("איש קשר:", string.Empty).Trim();
                        pageTableRow["name"] = myString;
                    }
                    else if (lines[i].Text.IndexOf("טלפון 1:") > -1)
                    {
                        
                        string myString = lines[i].Text.Replace("\r\n", string.Empty);
                        myString = myString.Replace("טלפון 1:", string.Empty).Trim();
                   
                        pageTableRow["phone1"] = myString;

                    }
                    else if (lines[i].Text.IndexOf("טלפון 2:") > -1)
                    {
                       
                        string myString = lines[i].Text.Replace("\r\n", string.Empty);
                        myString = myString.Replace("טלפון 2:", string.Empty);
                        pageTableRow["phone2"] = myString;

                    }
                  
                  
                }
                System.Collections.ObjectModel.ReadOnlyCollection<IWebElement> freeTextlines = myDriver.WebDriver.FindElements(By.ClassName("remarks"));
                if (freeTextlines.Count > 0)
                    pageTableRow["freeText"] = freeTextlines[0].Text.Replace("'", "''");

                return true;
            }
            catch (Exception ex)
            {
                errorLog.handleException(ex);
                errorLog.writeToLogFile("at getRowData  " + ex.StackTrace);
                return false;
            }
        }
 
        private List<IWebElement> getBasisTable()
        {
            try
            {
                myDriver.WebDriver.SwitchTo().DefaultContent();
                System.Collections.ObjectModel.ReadOnlyCollection<IWebElement> fancybox = myDriver.WebDriver.FindElements(By.ClassName("fancybox-item"));
                if (fancybox.Count > 0)
                    fancybox[0].Click();
                main_table = myDriver.WebDriver.FindElement(By.Id(mainTableId));
                List<IWebElement> mainTableRows = getMainTableRows();
                if (mainTableRows == null)
                    return null;
                if (!initPageTable(mainTableRows))
                    return null;
                return mainTableRows;
            }
            catch (Exception ex)
            {
                errorLog.handleException(ex);
                errorLog.writeToLogFile("at getBasisTable  " + ex.StackTrace);
                return null;
            }
        }

        private bool initPageTable(List<IWebElement> mainTableRows)
        {
            try
            {
                mDataSet.CreatePageTable(getPageTableCols());
                DataTable taskTable = new DataTable();
                foreach (IWebElement row in mainTableRows)
                {
                    DataRow tableRow = mDataSet.GetNewRow();
                    initPageRow(row, tableRow);
                    tableRow[SiteDataSet.sInternalStatus] = SiteDataSet.sCloseStatus;
                    string InternalDownloadStatus = SiteDataSet.sWaitingInternalDownloadStatus;
                    tableRow[SiteDataSet.sInternalDownloadStatus] = InternalDownloadStatus;
                    tableRow[SiteDataSet.sTaskId] = taskId.ToString();
                    mDataSet.AddRow(tableRow);
                }
              
                    taskTable = SitesBL.UpdateHomelessVehicleTableRowsStatus(mDataSet.GetClonePageTable());
                    if (taskTable == null)
                        return false;
                    mDataSet.setHomelessVehicleTable(taskTable);
                    return true;
               
            }
            catch (Exception ex)
            {
                errorLog.handleException(ex);
                errorLog.writeToLogFile("at initPageTable  " + ex.StackTrace);
                throw ex;
            }
        }

       
        private bool getRandomRowData(List<IWebElement> mainTableRows)
        {
            int rowNum = 0;
            try
            {
                myDriver.WebDriver.SwitchTo().DefaultContent();
                rowNum = mDataSet.getRandomOpenRowNum(mainTableRows.Count, pageDate);
                string frameId = clickRow(mainTableRows, rowNum);
                if (frameId == string.Empty)
                    return false;
                bool status = getRowData(mDataSet.GetRow(rowNum), frameId);
                if (status)
                    mDataSet.SetDownloadStatusForRow(rowNum, SiteDataSet.sSsuccessInternalDownloadStatus);
                else
                    mDataSet.SetDownloadStatusForRow(rowNum, SiteDataSet.sFailedInternalDownloadStatus);
                return status;
            }
            catch (Exception ex)
            {
                errorLog.handleException(ex);
                errorLog.writeToLogFile("at getRandomRowData rowNum=" + rowNum + "  " + ex.StackTrace);
                return false;
            }
        }
        private TTaskStatusType getSitePageData(string MinPage)
        {
            string Url = MinPage;
            if (MinPage == "" ||  MinPage =="0")
                Url=getBasePageUrl();
             
            if (!driverUtils.NevigateToPage(myDriver.WebDriver,Url ))
            {
                release(TTaskStatusType.Failed.ToString());
                return TTaskStatusType.Failed;
            }
   
            return getMainTableData(MinPage);
        }

        private TTaskStatusType findFirstPage(string MinPage)
        {
            
                if (!driverUtils.NevigateToPage(myDriver.WebDriver, MinPage))
                {
                    release(TTaskStatusType.Failed.ToString());
                    return TTaskStatusType.Failed;
                }

                return TTaskStatusType.Success;
        }

        private TTaskStatusType getMainTableData(string newMinPage)
        {
            try
            {
                int from = 1 + 1, to = 1, cur = 1;
                if (newMinPage != "")
                {
                    string tmp = newMinPage.Replace("http://www.homeless.co.il/private/", "");
                    int.TryParse(tmp, out cur);
                }
                bool begin = false, end = false;
                Random random = new Random();
                int randomNumber = random.Next(minRows, maxRows);
                int i = 0;
                bool isTaskFinish = false;

                while (true)
                {

                    List<IWebElement> mainTableRows = getBasisTable();
                    driverUtils.CloseOtherWindows(myDriver.WebDriver);
                    if (mainTableRows == null)
                    {
                        release(TTaskStatusType.Failed.ToString());
                        return TTaskStatusType.Failed;
                    }
                    int dateRowsCount = mDataSet.dateRowsCount(pageDate, out isTaskFinish);
                    bool IsTableFinish = false;
                    if (dateRowsCount > 0)
                    {
                        begin = true;
                       

                        for (; i < randomNumber; i++)
                        {
                            myDriver.WebDriver.SwitchTo().DefaultContent();
                            IsTableFinish = mDataSet.IsTableFinish(pageDate);
                            if (IsTableFinish)
                                break;
                            if (!getRandomRowData(mainTableRows))
                            {
                                writePageData();
                                driverUtils.screeshot(myDriver.WebDriver, "..//screenShot//taskName" + taskId + "_" + DateTime.Now.Hour.ToString() + DateTime.Now.Minute.ToString() + DateTime.Now.Second.ToString() + ".jpg");
                                release(TTaskStatusType.Failed.ToString());
                                return TTaskStatusType.Failed;
                            }
                        }
                        if (i > 0)
                        {
                            if (!writePageData())
                                return TTaskStatusType.Failed; 
                            driverUtils.screeshot(myDriver.WebDriver, "..//screenShot//taskName" + taskId + "_" + DateTime.Now.Hour.ToString() + DateTime.Now.Minute.ToString() + DateTime.Now.Second.ToString() + ".jpg");
                        }
                        if (i == randomNumber)
                        {

                            if (TaskBL.AddTask(1, "GetPageData", CMLib.DateTimeSQLite(pageDate), "", TSites.HomelessVehicle.ToString(), "http://www.homeless.co.il/private/" + cur.ToString()))
                                return TTaskStatusType.Success;
                            return TTaskStatusType.Failed;

                        }
                         
                    }
                    else
                    {
                        myDriver.WebDriver.SwitchTo().DefaultContent();
                        System.Collections.ObjectModel.ReadOnlyCollection<IWebElement> fancybox = myDriver.WebDriver.FindElements(By.ClassName("fancybox-item"));
                        if (fancybox.Count > 0)
                        {
                            fancybox[0].Click();
                            continue;
                        }
                        else
                        {
                            if (compareDate == -2)
                                return TTaskStatusType.Failed;
                            if (compareDate == 1)
                            {
                                release(TTaskStatusType.Success.ToString());
                                return TTaskStatusType.Success;
                            }
                        }
                        
                    }
                    cur++;
                    TaskBL.updateTaskMinPage(taskId, "http://www.homeless.co.il/private/" + cur.ToString());
                    if (!goToPage(cur))
                        return TTaskStatusType.Failed;
                }
            }
            catch (Exception ex)
            { 
                return TTaskStatusType.Failed; 
            }
        }

        private bool goToPage(int pageNum)
        {

            try
            {
                driverUtils.NevigateToPage(myDriver.WebDriver, "http://www.homeless.co.il/private/" + pageNum);

                return true; ;
            }
            catch (Exception ex)
            {
                errorLog.handleException(ex);
                errorLog.writeToLogFile("at goToPage  " + ex.StackTrace);
                return false;
            }
        }

        private int comparePageDate(DateTime date)
        {
            try
            {
                if (date == null || pageDate == null)
                    return -2;

                return pageDate.CompareTo(date);
            }
            catch (Exception ex)
            {
                errorLog.handleException(ex);
                errorLog.writeToLogFile("at comparePageDate  " + ex.StackTrace);

                return -2;
            }

        }

        private bool writePageData()
        {
            mDataSet.filterDate(pageDate);
            return  SitesBL.AddHomelessVehiclePageTable(mDataSet.GetPageTable());
        }

    
     
        private void initPageRow(IWebElement row, DataRow tableRow)
        {
            var rowsTd = row.FindElements(By.XPath("td"));
            int ind = 0;
            for (int i = 0; i < rowsTd.Count; i++)
            {
                if (i!=0 && i!=10&& i!=8 && i!=7)
                {
                    tableRow[ind] = rowsTd[i].Text.Replace("'", "''");
                    ind++;
                }
            }
         }

        private List<IWebElement> getMainTableRows()
        {
            compareDate = -2;
            try
            {
                System.Collections.ObjectModel.ReadOnlyCollection<IWebElement> TableTrs = main_table.FindElements(By.XPath(".//tr"));
                List<IWebElement> mainTableTrs = new List<IWebElement>();
                int ind = 0;
                foreach (IWebElement curTr in TableTrs)
                {
                    ind++;
                    if (ind == 1)
                        continue;
                    System.Collections.ObjectModel.ReadOnlyCollection<IWebElement> TableTds = curTr.FindElements(By.XPath(".//td"));
                    if (TableTds.Count<10)
                        continue;
                    
                    string dd = TableTds[9].Text;
                    DateTime rowDate = DateTime.MaxValue;
                    if (!DateTime.TryParse(dd, out rowDate))
                        return null;
                    mainTableTrs.Add(curTr);
                    if (compareDate != 0)
                          compareDate = comparePageDate(rowDate);
                }
                return mainTableTrs;
            }
            catch (Exception ex)
            {
                errorLog.handleException(ex);
                errorLog.writeToLogFile("at getMainTableRows  " + ex.StackTrace);
                return null;
            }
        }




   
      
     
        private void goToNextPage()
        {
            ReadOnlyCollection<IWebElement> nextElements = myDriver.WebDriver.FindElements(By.ClassName("pagingArrowRight"));
            if (nextElements.Count > 0)
            {
                nextElements[0].Click();
                driverUtils.Sleep(20000, 27000);
            }
        }

        private string clickRow(List<IWebElement> mainTableRows, int rowNum)
        {
            try
            {
                System.Collections.ObjectModel.ReadOnlyCollection<IWebElement> rowTd = mainTableRows[rowNum].FindElements(By.XPath(".//td"));
                if (rowTd == null || rowTd.Count < 11)
                    return string.Empty;
                rowTd[4].Click();
                driverUtils.Sleep(5000, 6000);
                mDataSet.AddEntry(rowNum, SiteDataSet.sOpenStatus, SiteDataSet.sRunningInternalDownloadStatus);
                string id = mainTableRows[rowNum].GetAttribute("id");
                string detailsId = id.Replace("ad_", "addetails_");
                IWebElement WebElement = main_table.FindElement(By.Id(detailsId));
                if (WebElement != null)
                    return detailsId;
                return string.Empty;
            }
            catch (Exception ex)
            {
                errorLog.handleException(ex);
                errorLog.writeToLogFile("at clickRow  " + ex.StackTrace);
                return string.Empty;
            }
        }
    }

}

