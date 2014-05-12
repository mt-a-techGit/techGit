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
using System.Collections.ObjectModel;
using CMCore.webdriver;
using CMCore.data;
using CMCore.driver;
using CMCore.task;
using BLL.BLL;

namespace CMCore.site
{
    public class Y2 : baseSite
    {
        private const int maxRows = 7, minRows = 5;
        internal Y2(int taskId,string ETaskSource, DateTime pageDate, string city)
            : base(taskId, @"Y2\errorHandlerLog\", "Y2.log", @"Y2\infoHandlerLog\", "Y2.log", ETaskSource)
        {
            this.pageDate = pageDate;
            this.city = city;
        }
        private int compareDate = -2;

        private string city = "";
        private IWebElement main_table;
        DateTime pageDate = DateTime.Now;
        public const string mainTableId = "main_table";

        private string[] getPageTableCols()
        {
            string[] cols = { "type", "area", "address", "price", "rooms", "entrenceDate", "floor", "rowDate", "city", "neighborhood", "address2", "size", "freeText", "name", "phone1", "phone2", "municipalRate" };
            return cols;
        }
        public TTaskStatusType getPageData(string MinPage)
        {
            if (myDriver == null)
                return TTaskStatusType.DriverError;
            TTaskStatusType downloadStatusType = getSitePageData(MinPage);
            release(downloadStatusType.ToString());
            return downloadStatusType;
        }

        private bool initCity(string cityName)
        {
             
            try
            {
                IWebElement spnCity = myDriver.WebDriver.FindElement(By.Id("City"));
               
                for (int i = 0; i < cityName.Length; i++)
                {
                    spnCity.SendKeys(cityName[i].ToString());
                    driverUtils.Sleep(500, 1000);
                }
                driverUtils.Sleep(3000, 7000);
                IWebElement SearchButton = myDriver.WebDriver.FindElement(By.Id("SearchButton"));
                System.Collections.ObjectModel.ReadOnlyCollection<IWebElement> SearchBut = SearchButton.FindElements(By.ClassName("submit"));
                SearchBut[0].Click();
                return true;
            }
            catch (Exception ex)
            {
                errorLog.handleException(ex);
                errorLog.writeToLogFile("at initCity  " + ex.StackTrace);
                return false;
            }
        }

        private TTaskStatusType getSitePageData(string MinPage)
        {
            TTaskStatusType status = TTaskStatusType.Failed;
            bool nav = false;
            if (MinPage != "")
            {
                if (!driverUtils.NevigateToPage(myDriver.WebDriver, MinPage))
                {
                    release(TTaskStatusType.Failed.ToString());
                    return TTaskStatusType.Failed;
                }

            }
            else
            {
                if (!driverUtils.NevigateToPage(myDriver.WebDriver, getBasePageUrl()) || !initCity(city))
                {
                    release(TTaskStatusType.Failed.ToString());
                    return TTaskStatusType.Failed;
                }

            }
            return   getMainTableData(MinPage);
        
        }
        int from = 1, to = 1, cur = 1;
        private TTaskStatusType getMainTableData(string MinPage)
        {
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
                    for (; i < randomNumber; i++)
                    {
                        IsTableFinish = mDataSet.IsTableFinish(pageDate);
                        if (IsTableFinish)
                            break;
                        if (!getRandomRowData(mainTableRows))
                        {
                            if (i > 0)
                            {
                                 writePageData();
                                 driverUtils.screeshot(myDriver.WebDriver, "..//screenShot//taskName" + taskId + "_" + DateTime.Now.Hour.ToString() + DateTime.Now.Minute.ToString() + DateTime.Now.Second.ToString() + ".jpg");
                            }
                        }    
                        release(TTaskStatusType.Failed.ToString());
                        return TTaskStatusType.Failed;
                    }
                    if (i > 0)
                    {
                        writePageData();
                        driverUtils.screeshot(myDriver.WebDriver, "..//screenShot//taskName" + taskId + "_" + DateTime.Now.Hour.ToString() + DateTime.Now.Minute.ToString() + DateTime.Now.Second.ToString() + ".jpg");

                    }
                    if (i == randomNumber)
                    {
                        string url = myDriver.WebDriver.Url;
                        TaskBL.AddTask(1, "GetPageData", CMLib.DateTimeSQLite(pageDate), city, TSites.Yad2.ToString(), url);
                        release(TTaskStatusType.Success.ToString());
                        return TTaskStatusType.Success;
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
                if (!goToNextPage())
                {
                    release(TTaskStatusType.Failed.ToString());
                    return TTaskStatusType.Failed;
                }
                TaskBL.updateTaskMinPage(taskId, myDriver.WebDriver.Url);
                }
        }


        private List<IWebElement> getBasisTable()
        {
            try
            {
                mDataSet.CreatePageTable(getPageTableCols());
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

        private void initPageTable(List<IWebElement> mainTableRows)
        {
            try
            {
                mDataSet.CreatePageTable(getPageTableCols());
                SitesBL SitesBL = new SitesBL(errorLog, infoLog);
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

                taskTable = SitesBL.UpdateYad2TableRowsStatus(mDataSet.GetClonePageTable());
                mDataSet.setTable(taskTable);

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
                driverUtils.CloseOtherWindows(myDriver.WebDriver);
                myDriver.WebDriver.SwitchTo().DefaultContent();
                rowNum = mDataSet.getRandomOpenRowNum(mainTableRows.Count, pageDate);
                IWebElement rowElement = clickRow(mainTableRows, rowNum);
                if (rowElement == null)
                    return false;
                driverUtils.Sleep(1000, 2000);
                // TODO this need farther work, getRowData should not fill the Data struct...
                //driverUtils.CloseOtherWindows(myDriver.WebDriver);
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

        private bool goToNextPage()
        {
            try
            {
                ReadOnlyCollection<IWebElement> nextElements = myDriver.WebDriver.FindElements(By.ClassName("next"));
                for (int i = 0; i < nextElements.Count; i++)
                {
                    if (nextElements[i].TagName == "a")
                    {
                        nextElements[i].Click();
                        driverUtils.Sleep(10000, 13000);
                        return true;
                    }
                }
                return false;
            }
            catch (Exception ex)
            { return false; }
           
        }

        private bool goToPageRelative(int pageNum)
        {
            try
            {
                System.Collections.ObjectModel.ReadOnlyCollection<IWebElement> pagination = myDriver.WebDriver.FindElements(By.ClassName("pages"));
                System.Collections.ObjectModel.ReadOnlyCollection<IWebElement> paginationLi = pagination[0].FindElements(By.XPath(".//a"));
                System.Collections.ObjectModel.ReadOnlyCollection<IWebElement> current = pagination[0].FindElements(By.ClassName("active"));

                int cur = 0;
                if (current.Count == 0)
                    return false;
                int.TryParse(current[0].Text, out cur);
                for (int i = 0; i < paginationLi.Count; i++)
                {
                    int page = 0;
                    int.TryParse(paginationLi[i].Text, out page);
                    if (pageNum + cur == page)
                    {
                        paginationLi[i].Click();
                        driverUtils.Sleep(10000, 13000);
                        driverUtils.CloseOtherWindows(myDriver.WebDriver);
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

        private bool goToPage(int pageNum)
        {
            try
            {
                System.Collections.ObjectModel.ReadOnlyCollection<IWebElement> pagination = myDriver.WebDriver.FindElements(By.ClassName("pages"));
                System.Collections.ObjectModel.ReadOnlyCollection<IWebElement> paginationLi = pagination[0].FindElements(By.XPath(".//a"));
                for (int i = 0; i < paginationLi.Count; i++)
                {
                    int page = 0;
                    int.TryParse(paginationLi[i].Text, out page);
                    if (pageNum == page)
                    {
                        paginationLi[i].Click();
                        driverUtils.Sleep(10000, 15000);
                        driverUtils.CloseOtherWindows(myDriver.WebDriver);
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

        private IWebElement clickRow(List<IWebElement> mainTableRows, int rowNum)
        {

            try
            {
                ReadOnlyCollection<IWebElement> prevMainTabletd = mainTableRows[rowNum].FindElements(By.XPath(".//td"));
                ReadOnlyCollection<IWebElement> prevMainTableFrames = main_table.FindElements(By.XPath(".//iframe"));
                prevMainTabletd[12].Click();
                driverUtils.Sleep(5000, 6000);
                driverUtils.CloseOtherWindows(myDriver.WebDriver);
                mDataSet.AddEntry(rowNum, SiteDataSet.sOpenStatus, SiteDataSet.sRunningInternalDownloadStatus);
                ReadOnlyCollection<IWebElement> CurMainTableFrames = main_table.FindElements(By.XPath(".//iframe"));
                return getRowDetailsElement(prevMainTableFrames, CurMainTableFrames);
            }
            catch (Exception ex)
            {
                errorLog.handleException(ex);
                errorLog.writeToLogFile("at clickRow  " + ex.StackTrace);
                return null;
            }
        }

        private IWebElement getRowDetailsElement(System.Collections.ObjectModel.ReadOnlyCollection<IWebElement> prevMainTableFrames, System.Collections.ObjectModel.ReadOnlyCollection<IWebElement> CurMainTableFrames)
        {
            if (prevMainTableFrames.Count == CurMainTableFrames.Count - 1)
            {
                for (int i = 0; i < CurMainTableFrames.Count; i++)
                {
                    bool exists = false;
                    for (int j = 0; j < prevMainTableFrames.Count; j++)
                    {
                        if (CurMainTableFrames[i].Equals(prevMainTableFrames[j]))
                        {
                            exists = true;
                            break;
                        }
                    }
                    if (!exists)
                        return CurMainTableFrames[i];
                }
            }
            return null;
        }

        private bool getRowData(DataRow pageTableRow, IWebElement rowElement)
        {
            try
            {
                myDriver.WebDriver.SwitchTo().DefaultContent();
                int phoneInd = 0;
                string id = rowElement.GetAttribute("id");
                myDriver.WebDriver.SwitchTo().Frame(id);
                if (id == "captch_frame")
                {
                    siteException ex = new siteException("captch", TTaskStatusType.SiteBlock);
                    throw ex;
                }
                System.Collections.ObjectModel.ReadOnlyCollection<IWebElement> frameTDs = myDriver.WebDriver.FindElements(By.XPath(".//td"));
                string lastText = "";
                if (frameTDs.Count == 0)
                    return false;
                foreach (IWebElement curTd in frameTDs)
                {

                    if (lastText.Length < 30 && lastText.IndexOf("טלפון") > -1)
                    {
                        IWebElement baseRow = driverUtils.GetParent(curTd);
                        var phoneLink = baseRow.FindElements(By.XPath(".//a"));
                        if (phoneLink.Count > 0)
                        {
                            driverUtils.Sleep(5000, 6000);
                            phoneLink[0].Click();
                            
                            break;
                        }
                    }
                    lastText = curTd.Text;
                }
                lastText = "";
                myDriver.WebDriver.SwitchTo().DefaultContent();
                myDriver.WebDriver.SwitchTo().Frame(id);

                frameTDs = myDriver.WebDriver.FindElements(By.XPath(".//td"));
                string ss = "";
                if (frameTDs.Count > 0)
                {
                    ss = frameTDs[0].Text;
                    string[] lines = ss.Split('\n');
                    for (int i = 0; i < lines.Length; i++)
                        lines[i] = lines[i].Trim();

                    for (int i = 0; i < lines.Length; i++)
                    {

                        if (lines[i].IndexOf("ישוב") > -1)
                        {
                            lines[i] = lines[i].Replace("ישוב", "");
                            lines[i] = lines[i].Replace(":", "");
                            pageTableRow["city"] = lines[i].Trim();
                        }

                        else if (lines[i].IndexOf("שכונה") > -1 && lines[i].IndexOf("לצפייה בנתוני") == -1)
                        {
                            lines[i] = lines[i].Replace("שכונה", "");
                            lines[i] = lines[i].Replace(":", "");
                            pageTableRow["neighborhood"] = lines[i].Trim();
                        }
                        else if (lines[i].IndexOf("כתובת:") > -1)
                        {
                            lines[i] = lines[i].Replace("כתובת", "");
                            lines[i] = lines[i].Replace(":", "");
                            pageTableRow["address2"] = lines[i].Trim();
                        }
                        else if (lines[i].IndexOf("גודל") > -1)
                        {
                            lines[i] = lines[i].Replace("גודל", "");
                            lines[i] = lines[i].Replace("\"", "");
                            lines[i] = lines[i].Replace("במר", "");
                            lines[i] = lines[i].Replace(":", "");
                            pageTableRow["size"] = lines[i].Trim();
                        }
                        else if (lines[i].IndexOf("איש קשר") > -1)
                        {
                            lines[i] = lines[i].Replace("איש קשר", "");
                            lines[i] = lines[i].Replace(":", "");
                            pageTableRow["name"] = lines[i].Trim();
                        }
                        else if (lines[i].IndexOf("תוספות") > -1)
                        {
                            lines[i] = lines[i].Replace("תוספות", "");
                            lines[i] = lines[i].Replace(":", "");
                            pageTableRow["freeText"] = lines[i].Trim();
                        }
                        else if (lines[i].IndexOf("ארנונה") > -1)
                        {
                            pageTableRow["municipalRate"] = lines[i].Trim();
                        }
                        else if (lines[i].IndexOf("טלפון 1") > -1)
                        {
                            lines[i] = lines[i].Replace("טלפון 1", "");
                            lines[i] = lines[i].Replace(":", "");

                            pageTableRow["phone1"] = lines[i].Trim();
                        }
                        else if (lines[i].IndexOf("טלפון 2") > -1)
                        {
                            lines[i] = lines[i].Replace("טלפון 2", "");
                            lines[i] = lines[i].Replace(":", "");

                            pageTableRow["phone2"] = lines[i].Trim();
                        }
                    }
                }
                return true;
            }
            catch (Exception ex)
            {
                errorLog.handleException(ex);
                errorLog.writeToLogFile("at initPaggetRowDataeTable  " + ex.StackTrace);
                return false;
            }
        }

        private void initPageRow(IWebElement row, DataRow tableRow)
        {
            var rowsTd = row.FindElements(By.XPath("td"));

            int[] tdVal = { 4, 6, 8, 10, 12, 14, 16, 20 };
            int ind = 0;
            for (int i = 0; i < tdVal.Length; i++)
            {
                tableRow[ind] = rowsTd[tdVal[i]].Text;
                ind++;
            }
        }

        private List<IWebElement> getMainTableRows()
        {
            try
            {
                System.Collections.ObjectModel.ReadOnlyCollection<IWebElement> mainTableTds = main_table.FindElements(By.XPath(".//td"));
                System.Collections.ObjectModel.ReadOnlyCollection<IWebElement> mainTableTr = main_table.FindElements(By.XPath(".//tr"));

                List<IWebElement> mainTableTrs = new List<IWebElement>();
                IWebElement lastRow = null;
                foreach (IWebElement curTr in mainTableTr)
                {
                    string display = curTr.GetAttribute("id");
                    string cname = curTr.GetAttribute("class");
                    if (curTr.Text!="" && cname != "Info" && display != "")
                            mainTableTrs.Add(curTr);
                        
                    
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


        public static string getBasePageUrl()
        {
            return " http://www.yad2.co.il/Nadlan/rent.php?";
        }

        private void writePageData()
        {
            SitesBL SitesBL = new SitesBL(errorLog, infoLog);
            mDataSet.filterDate(pageDate);
            SitesBL.AddYad2PageTable(mDataSet.GetPageTable());
        }

        private string setNextTaskUrl()
        {
            string nextUrl = "";
            return nextUrl;
        }
    }
}

