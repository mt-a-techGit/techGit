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

    class Homeless : baseSite
    {
        protected SiteDataSet mDataSet = new SiteDataSet();
        public const string mainTableId = "mainresults";
        private IWebElement main_table;
        private const int maxRows = 14, minRows = 8;
        private DateTime pageDate = DateTime.MinValue;

        private int compareDate = -2;

        internal Homeless(int taskId, string ETaskSource, DateTime pageDate)
            : base(taskId, @"Homeless\errorHandlerLog\", "Homeless.log", @"Homeless\infoHandlerLog\", "Homeless.log", ETaskSource)
        {

            this.pageDate = pageDate;

        }

        public static string getPageUrl()
        {
            return "http://www.homeless.co.il/rent/50";
        }

        public static string getBasePageUrl()
        {
            return "http://www.homeless.co.il/rent/50";
        }

        public TTaskStatusType getPageData(string MinPage)
        {
            try
            {
                if (myDriver == null)
                    return TTaskStatusType.DriverError;
                TTaskStatusType downloadStatusType = getSitePageData(MinPage);
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
            string[] cols = { "type", "rooms", "floor", "price", "address", "city", "entrenceDate", "rowDate", "freeText", "region", "address2", "size", "area", "name", "phone1", "phone2" };
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
                int found = 0;

                for (int i = lines.Count - 1; i >= 0; i--)
                {
                    if (lines[i].Text == "" || lines[i].Text.Length > 150)
                        continue;
                    if (lines[i].Text.IndexOf("איש קשר:") > -1)
                    {
                        found++;
                        string myString = lines[i].Text.Replace("'", "''").Replace("\r\n", string.Empty);
                        myString = myString.Replace("איש קשר:", string.Empty);
                        pageTableRow["name"] = myString;
                    }
                    else if (lines[i].Text.IndexOf("כתובת:") > -1)
                    {
                        found++;
                        pageTableRow["address2"] = lines[i].Text.Replace(":", "").Replace("כתובת", "").Trim();
                    }
                    else if (lines[i].Text.IndexOf("טלפון 1:") > -1)
                    {
                        found++;
                        string myString = lines[i].Text.Replace("\r\n", string.Empty);
                        myString = myString.Replace("טלפון 1:", string.Empty);
                        myString = myString.Replace("טלפון:", string.Empty);
                        pageTableRow["phone1"] = myString;

                    }
                    else if (lines[i].Text.IndexOf("טלפון 2:") > -1)
                    {
                        found++;
                        string myString = lines[i].Text.Replace("\r\n", string.Empty);
                        myString = myString.Replace("טלפון 2:", string.Empty);
                        pageTableRow["phone2"] = myString;

                    }

                    else if (lines[i].Text.Replace('"', '@').IndexOf("מ@ר") > -1)
                    {
                        found++;
                        pageTableRow["area"] = lines[i].Text.Replace(":", " ").Trim();
                    }
                    else if (lines[i].Text.IndexOf("שכונה/ישוב") > -1)
                    {
                        found++;
                        pageTableRow["region"] = lines[i].Text.Replace("שכונה/ישוב", "").Replace(":", "").Trim();
                    }
                    if (found == 6)
                        break;
                }
                System.Collections.ObjectModel.ReadOnlyCollection<IWebElement> freeTextlines = myDriver.WebDriver.FindElements(By.ClassName("remarks"));
                if (freeTextlines.Count > 0)
                    pageTableRow["freeText"] = freeTextlines[0].Text.Replace("'", " ").Replace('"', ' ');

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
                    taskTable = SitesBL.UpdateHomelessTableRowsStatus(mDataSet.GetClonePageTable());
                    mDataSet.setHomelessTable(taskTable);
                }
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

            if (MinPage == "" || MinPage == "0")
                Url = getBasePageUrl();

            if (!driverUtils.NevigateToPage(myDriver.WebDriver, Url))
            {
                release(TTaskStatusType.Failed.ToString());
                return TTaskStatusType.Failed;
            }

            return getMainTableData(MinPage);
        }

        private TTaskStatusType getMainTableData(string newMinPage)
        {
            try
            {
                int from = 1 + 1, to = 1, cur = 1;
                if (newMinPage != "")
                {
                    string tmp = newMinPage.Replace("http://www.homeless.co.il/rent/", "");
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
                            release(TTaskStatusType.Success.ToString());
                            if (TaskBL.AddTask(1, "GetPageData", CMLib.DateTimeSQLite(pageDate), "", TSites.Homeless.ToString(), "http://www.homeless.co.il/rent/" + cur.ToString()))
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
                    cur++;
                    TaskBL.updateTaskMinPage(taskId, "http://www.homeless.co.il/rent/" + cur.ToString());
                    if (!goToPage(cur))
                    {
                        release(TTaskStatusType.Failed.ToString());
                        return TTaskStatusType.Failed; }
                }
            }
            catch (Exception ex)
            {
                release(TTaskStatusType.Failed.ToString());
                return TTaskStatusType.Failed;
            }
        }

        private bool goToPage(int pageNum)
        {

            try
            {
                driverUtils.NevigateToPage(myDriver.WebDriver, "http://www.homeless.co.il/rent/" + pageNum);

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

        private void writePageData()
        {
            mDataSet.filterDate(pageDate);
            SitesBL.AddHomelessPageTable(mDataSet.GetPageTable());
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
                    if (TableTds.Count < 10)
                        continue;

                    string dd = TableTds[10].Text;
                    DateTime rowDate = DateTime.MaxValue;
                    DateTime.TryParse(dd, out rowDate);
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
                if (rowTd == null || rowTd.Count < 12)
                    return string.Empty;
                rowTd[4].Click();
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
