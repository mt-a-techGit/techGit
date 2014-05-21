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
    class Winwin : baseSite
    {
        public const string mainTableId = "savedAddsTable";
        private IWebElement main_table;
        private const int minRows = 7, maxRows = 12;
        private DateTime pageDate = DateTime.MinValue;
        private string city = "";
        private int compareDate = -2;
        
        internal Winwin(int taskId, string ETaskSource, DateTime pageDate, string city)
            : base(taskId, @"Winwin\errorHandlerLog\", "Winwin.log", @"Winwin\infoHandlerLog\", "Winwin.log", ETaskSource)
        {
         
            this.pageDate = pageDate;
            this.city = city;
        }

        public static string getPageUrl()
        {
            return "http://www.winwin.co.il/RealEstate/ForRent/RealEstatePage.aspx";
        }

        
        public static string getBasePageUrl()
        {
            return "http://www.winwin.co.il/RealEstate/ForRent/RealEstatePage.aspx";
        }

        public TTaskStatusType getPageData(string MinPage)
        {
            if (myDriver == null)
                return TTaskStatusType.DriverError;
            TTaskStatusType downloadStatusType = getSitePageData(MinPage);
            release(downloadStatusType.ToString());
            return downloadStatusType;
        }

        private string[] getPageTableCols()
        {
            string[] cols = { "type", "rooms", "city", "address", "entrenceDate", "price", "rowDate", "region", "neighborhood", "address2", "rooms2", "floor", "area", "name", "phone1", "phone2", "freeText" };
            return cols;
        }

        private void writePageData()
        {
            mDataSet.filterDate(pageDate);
            SitesBL.AddWinwinPageTable(mDataSet.GetPageTable());
        }

        private List<IWebElement> getBasisTable()
        {
            try
            {
                myDriver.WebDriver.SwitchTo().DefaultContent();
                main_table = myDriver.WebDriver.FindElement(By.Id(mainTableId));
                List<IWebElement> mainTableRows = getMainTableRows();
                if (mainTableRows == null)
                    return null;
                initPageTable(mainTableRows);
                return mainTableRows;
            }
            catch (Exception ex)
            {
                errorLog.handleException(ex);
                errorLog.writeToLogFile("at getBasisTable  " + ex.StackTrace);
                return null;
            }
        }

        private bool getRowData(DataRow pageTableRow, IWebElement rowElement)
        {
            try
            {
                System.Collections.ObjectModel.ReadOnlyCollection<IWebElement> lines = rowElement.FindElements(By.ClassName("Row"));

                for (int i = 0; i < lines.Count; i++)
                {
                    if (lines[i].Text.IndexOf("תת אזור") > -1)
                    {
                        pageTableRow["region"] = lines[i].Text.Replace("תת אזור", "");
                        pageTableRow["region"] = pageTableRow["region"].ToString().Replace(":", "");
                        pageTableRow["region"] = pageTableRow["region"].ToString().Trim();
                    }
                    else if (lines[i].Text.IndexOf("שם:") > -1)
                    {
                        string myString = lines[i].Text.Replace("\r\n", string.Empty);
                        myString = myString.Replace("שם:", string.Empty);
                        pageTableRow["name"] = myString;
                    }

                    else if (lines[i].Text.IndexOf("טלפון:") > -1)
                    {

                        System.Collections.ObjectModel.ReadOnlyCollection<IWebElement> phoneLink = lines[i].FindElements(By.XPath(".//a"));
                        if (phoneLink.Count == 0)
                            return false;
                        phoneLink[0].Click();
                        if (lines[i].Text.Contains( "לחץ כאן"))
                            return false;
                        string myString = lines[i].Text.Replace("\r\n", string.Empty);
                        myString = myString.Replace("טלפון 1:", string.Empty);
                        myString = myString.Replace("טלפון:", string.Empty);
                        pageTableRow["phone1"] = myString;

                    }
                    else if (lines[i].Text.IndexOf("טלפון 2:") > -1)
                    {
                        string myString = lines[i].Text.Replace("\r\n", string.Empty);
                        myString = myString.Replace("טלפון 2:", string.Empty);
                        pageTableRow["phone2"] = myString;

                    }
                    else if (lines[i].Text.IndexOf("שכונה") > -1)
                    {
                        pageTableRow["neighborhood"] = lines[i].Text.Replace("שכונה", "");
                        pageTableRow["neighborhood"] = pageTableRow["neighborhood"].ToString().Replace(":", "");
                        pageTableRow["neighborhood"] = pageTableRow["neighborhood"].ToString().Trim();
                    }
                    else if (lines[i].Text.IndexOf("רחוב:") > -1)
                    {
                        pageTableRow["address2"] = lines[i].Text.Replace("רחוב", "");
                        pageTableRow["address2"] = pageTableRow["address2"].ToString().Replace(":", "");
                        pageTableRow["address2"] = pageTableRow["address2"].ToString().Trim();
                    }
                    else if (lines[i].Text.IndexOf("קומה:") > -1)
                    {
                        pageTableRow["floor"] = lines[i].Text.Replace("קומה", "");
                        pageTableRow["floor"] = pageTableRow["floor"].ToString().Replace(":", "");
                        pageTableRow["floor"] = pageTableRow["floor"].ToString().Trim();
                    }
                    else if (lines[i].Text.IndexOf("שטח:") > -1)
                    {
                        pageTableRow["area"] = lines[i].Text.Replace("שטח", "");
                        pageTableRow["area"] = pageTableRow["area"].ToString().Replace(":", "");
                        pageTableRow["area"] = pageTableRow["area"].ToString().Trim();
                    }

                }
                System.Collections.ObjectModel.ReadOnlyCollection<IWebElement> freeTextlines = rowElement.FindElements(By.ClassName("NoteInfo"));
                for (int i = 0; i < freeTextlines.Count; i++)
                {
                    pageTableRow["freeText"] = freeTextlines[i].Text;
                }

                return true;
            }
            catch (Exception ex)
            {
                errorLog.handleException(ex);
                errorLog.writeToLogFile("at getRowData  " + ex.StackTrace);
                return false;
            }
        }

        private bool getRandomRowData(List<IWebElement> mainTableRows)
        {
            int rowNum = 0;
            try
            {
                myDriver.WebDriver.SwitchTo().DefaultContent();
                rowNum = mDataSet.getRandomOpenRowNum(mainTableRows.Count, pageDate);
                IWebElement rowElement = clickRow(mainTableRows, rowNum);
                if (rowElement == null)
                    return false;
                driverUtils.Sleep(7000, 10000);
                // TODO this need farther work, getRowData should not fill the Data struct...
                bool status = getRowData(mDataSet.GetRow(rowNum), rowElement);
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
            if (MinPage != "")
            {
                if (!driverUtils.NevigateToPage(myDriver.WebDriver, MinPage))
                {
                    release(TTaskStatusType.DriverError.ToString());
                    return TTaskStatusType.DriverError;
                }

            }
            else
            {
                if (!driverUtils.NevigateToPage(myDriver.WebDriver, getBasePageUrl()) || !initCity(city))
                {
                    release(TTaskStatusType.DriverError.ToString());
                    return TTaskStatusType.DriverError;
                }
                
            }
            
            return getMainTableData(MinPage);
        }
        bool curChange = false;
      
        private TTaskStatusType getMainTableData(string MinPage)
        {
            Random random = new Random();
            int randomNumber = random.Next(minRows, maxRows);
            int i = 0;
            bool isTaskFinish = false;

            while (true)
            { 
                if(!driverUtils.CloseOtherWindows(myDriver.WebDriver))
                    return TTaskStatusType.Failed;
                List<IWebElement> mainTableRows = getBasisTable();
                if (mainTableRows == null)
                {
                     release(TTaskStatusType.Failed.ToString());
                    return TTaskStatusType.Failed;
                }
              
                int dateRowsCount = mDataSet.dateRowsCount(pageDate, out isTaskFinish);
                bool IsTableFinish = false;
                if (dateRowsCount > 0)
                {
                    for (; i < randomNumber; i++)
                    {
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
                        writePageData();
                        driverUtils.screeshot(myDriver.WebDriver, "..//screenShot//taskName" + taskId + "_" + DateTime.Now.Hour.ToString() + DateTime.Now.Minute.ToString() + DateTime.Now.Second.ToString() + ".jpg");

                    }

                    if (i == randomNumber)
                    {
                        string url = myDriver.WebDriver.Url;
                        release(TTaskStatusType.Success.ToString());
                        if (TaskBL.AddTask(1, "GetPageData", CMLib.DateTimeSQLite(pageDate), city, TSites.Winwin.ToString(), url))
                            return TTaskStatusType.Success;
                        return TTaskStatusType.Failed;
                    }
                }
                else
                {
                    if (compareDate == -2)
                    {
                        release(TTaskStatusType.Failed.ToString());
                        return TTaskStatusType.Failed;
                    }

                    if (compareDate == 1)
                    {
                        release(TTaskStatusType.Success.ToString());
                        return TTaskStatusType.Success;
                    }
                
                }
       
                System.Collections.ObjectModel.ReadOnlyCollection<IWebElement> pagination = myDriver.WebDriver.FindElements(By.ClassName("pagination"));
                System.Collections.ObjectModel.ReadOnlyCollection<IWebElement> paginationLi = pagination[0].FindElements(By.XPath(".//li"));
                if (paginationLi.Count == 1)
                {
                    release(TTaskStatusType.Success.ToString());
                    return TTaskStatusType.Success;
                }
                if (!goToNextPage())
                {
                    release(TTaskStatusType.Failed.ToString());
                    return TTaskStatusType.Failed;
                }
                TaskBL.updateTaskMinPage(taskId, myDriver.WebDriver.Url);
      
            }
        }

       

        private string getUrlByPageNum(int pageNum)
        {
            return " http://www.winwin.co.il/RealEstate/ForRent/RealEstatePage.aspx?PageNumberBottom=" + pageNum + "&PageNumberTop=" + pageNum + "&search=63d2ac72bee6b7115ebfb90df5e3314b    ";
        }

        //private bool goToNextPage()
        //{
        //    try
        //    {
        //        System.Collections.ObjectModel.ReadOnlyCollection<IWebElement> pagination = myDriver.WebDriver.FindElements(By.ClassName("pagination"));
        //        System.Collections.ObjectModel.ReadOnlyCollection<IWebElement> current = pagination[0].FindElements(By.ClassName("current"));
        //        int cur = 0;
        //        if (current.Count == 0)
        //            return false;
        //        int.TryParse(current[0].Text, out cur);
        //        System.Collections.ObjectModel.ReadOnlyCollection<IWebElement> paginationLi = pagination[0].FindElements(By.XPath(".//li"));
        //        for (int i = 0; i < paginationLi.Count; i++)
        //        {
        //            int page = 0;
        //            int.TryParse(paginationLi[i].Text.Replace('/', ' '), out page);
        //            if (cur+1 == page)
        //            {
        //                System.Collections.ObjectModel.ReadOnlyCollection<IWebElement> paginationLink = paginationLi[i].FindElements(By.XPath(".//a"));
        //                paginationLink[0].Click();
        //                driverUtils.Sleep(30000, 35000);
        //                driverUtils.CloseOtherWindows(myDriver.WebDriver);
        //                return true;
        //            }
        //        }
        //        return false;
        //    }
        //    catch (Exception ex)
        //    {
        //        errorLog.handleException(ex);
        //        errorLog.writeToLogFile("at goToPage  " + ex.StackTrace);
        //        return false;
        //    }
        //}


        private bool goToPage(int pageNum)
        {
            try
            {
                System.Collections.ObjectModel.ReadOnlyCollection<IWebElement> pagination = myDriver.WebDriver.FindElements(By.ClassName("pagination"));
                System.Collections.ObjectModel.ReadOnlyCollection<IWebElement> paginationLi = pagination[0].FindElements(By.XPath(".//li"));
                for (int i = 0; i < paginationLi.Count; i++)
                {
                    int page = 0;
                    int.TryParse(paginationLi[i].Text.Replace('/', ' '), out page);
                    if (pageNum == page)
                    {
                        System.Collections.ObjectModel.ReadOnlyCollection<IWebElement> paginationLink = paginationLi[i].FindElements(By.XPath(".//a"));
                        paginationLink[0].Click();
                        driverUtils.Sleep(10000, 15000);
                        return true;
                    }
                }
                return false;
            }
            catch (Exception ex)
            {
                errorLog.handleException(ex);
                errorLog.writeToLogFile("at goToPage  " + ex.StackTrace);
                return false;
            }
        }

        private void initPageRow(IWebElement row, DataRow tableRow)
        {
            var rowsTd = row.FindElements(By.XPath("td"));
            int ind = 0;
            for (int i = 0; i < rowsTd.Count; i++)
            {
                if (rowsTd[i].Text.Trim() != "")
                {
                    tableRow[ind] = rowsTd[i].Text.Replace("'", " ").Replace('"', ' ');
                    ind++;
                }
            }
        }

        private bool initCity(string cityName)
        {
            //string cityName = "רמת גן";
            try
            {
                IWebElement spnCity = myDriver.WebDriver.FindElement(By.Id("spnCityNotExistMsg"));
                IWebElement spnCityText = driverUtils.GetParent(spnCity);
                System.Collections.ObjectModel.ReadOnlyCollection<IWebElement> TableTrs = spnCityText.FindElements(By.ClassName("cyanborder"));

                for (int i = 0; i < cityName.Length; i++)
                {
                    TableTrs[0].SendKeys(cityName[i].ToString());
                    driverUtils.Sleep(500, 1000);
                }
                System.Collections.ObjectModel.ReadOnlyCollection<IWebElement> SearchButton = myDriver.WebDriver.FindElements(By.ClassName("SearchButton"));
                SearchButton[0].Click();
                return true;
            }
            catch (Exception ex)
            {
                errorLog.handleException(ex);
                errorLog.writeToLogFile("at initCity  " + ex.StackTrace);

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

        private List<IWebElement> getMainTableRows()
        {
            compareDate = -2;
            try
            {
                System.Collections.ObjectModel.ReadOnlyCollection<IWebElement> TableTrs = main_table.FindElements(By.XPath(".//tr"));
                List<IWebElement> mainTableTrs = new List<IWebElement>();
                foreach (IWebElement curTr in TableTrs)
                {
                    string className = curTr.GetAttribute("class");
                    if (className == "paid" || className == "TitleData")
                    {
                        System.Collections.ObjectModel.ReadOnlyCollection<IWebElement> TableTds = curTr.FindElements(By.XPath(".//td"));
                        string dd = TableTds[10].Text;
                        System.Collections.ObjectModel.ReadOnlyCollection<IWebElement> img = TableTds[1].FindElements(By.XPath(".//img"));
                        if(img.Count==0)
                        { 
                            DateTime rowDate = DateTime.MaxValue;
                            DateTime.TryParse(dd, out rowDate);
                            mainTableTrs.Add(curTr);
                            if (compareDate != 0)
                                compareDate = comparePageDate(rowDate) ;
                        }
                       

                    }
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

        private void initPageTable(List<IWebElement> mainTableRows)
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
                if (compareDate == 0)
                {
                    taskTable = SitesBL.UpdateWinwinTableRowsStatus(mDataSet.GetClonePageTable());
                    mDataSet.setTable(taskTable);
                }
            }
            catch (Exception ex)
            {
                errorLog.handleException(ex);
                errorLog.writeToLogFile("at initPageTable  " + ex.StackTrace);
                throw ex;
            }
        }

        private bool goToNextPage()
        {
            try
            {
                ReadOnlyCollection<IWebElement> nextElements = myDriver.WebDriver.FindElements(By.ClassName("pagingArrowRight"));
                if (nextElements.Count > 0)
                {
                    nextElements[0].Click();
                    if (!driverUtils.CloseOtherWindows(myDriver.WebDriver))
                        return false;
                    driverUtils.Sleep(5000, 7000);

                    if (myDriver.WebDriver.FindElement(By.Id(mainTableId)) == null)
                        return false;
                    return true;
                }
                else
                    return false;
            }
            catch (Exception ex)
            { return false; }
           
        }

        private IWebElement clickRow(List<IWebElement> mainTableRows, int rowNum)
        {
            try
            {
                mainTableRows[rowNum].Click();
                mDataSet.AddEntry(rowNum, SiteDataSet.sOpenStatus, SiteDataSet.sRunningInternalDownloadStatus);
                string id = mainTableRows[rowNum].GetAttribute("id");
                string detailsId = id.Remove(id.IndexOf("Open"), 4);
                IWebElement WebElement = main_table.FindElement(By.Id(detailsId));
                return WebElement;
            }
            catch (Exception ex)
            {
                errorLog.handleException(ex);
                errorLog.writeToLogFile("at clickRow  " + ex.StackTrace);
                return null;
            }
        }
    }

}
