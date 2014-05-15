using BLL.BLL;
using CMCore.data;
using CMCore.task;
using OpenQA.Selenium;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;

namespace CMCore.site
{
    class WinwinProfessional : baseSite
    {
        public const string mainTableId = "savedAddsTable";
        private IWebElement main_table;
        private const int minRows = 7, maxRows = 12;
        private string city = "";
        private int compareDate = -2;

        internal WinwinProfessional(int taskId)
            : base(taskId, @"WinwinProfessional\errorHandlerLog\", "WinwinProfessional.log", @"WinwinProfessional\infoHandlerLog\", "WinwinProfessional.log", TSites.WinwinProfessional.ToString())
        {

        }

        public static string getPageUrl()
        {
            return "http://www.winwin.co.il/Specialists/SpecialistPage.aspx";
        }

        public static string getBasePageUrl()
        {
            return "http://www.winwin.co.il/Specialists/SpecialistPage.aspx";
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
            string[] cols = { "businessName", "profession", "subCategory", "area", "rowDate", "businessName2", "profession2", "subCategory2", "area2", "isMobile", "freeText", "name", "phone1", "phone2", "city" };
            return cols;
        }

        private TTaskStatusType getSitePageData(string MinPage)
        {
            int curPage = 0;
            int.TryParse(MinPage, out curPage);
            string Url = MinPage;
            if (MinPage == "" || MinPage == "0")
            {
                Url = getBasePageUrl();

                if (!driverUtils.NevigateToPage(myDriver.WebDriver, Url))
                {
                    release(TTaskStatusType.Failed.ToString());
                    return TTaskStatusType.DriverError;
                }
            }
            else goToPage(curPage);
            return getMainTableData(curPage);
         
        }

        private bool goToPage(int pageNum)
        {
            return driverUtils.NevigateToPage(myDriver.WebDriver, "http://www.winwin.co.il/Specialists/Search/SearchResults/SpecialistPage.aspx?PageNumberTop=" + pageNum);
        }

        private TTaskStatusType getMainTableData(int curPage)
        {
            int cur = 1;

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

                bool IsTableFinish = false;


                for (; i < randomNumber; i++)
                {
                    IsTableFinish = mDataSet.IsTableFinish();
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
                    if (TaskBL.AddTask(1, "GetPageData", CMLib.DateTimeSQLite(DateTime.Now), city, TSites.WinwinProfessional.ToString(), curPage.ToString()))
                        return TTaskStatusType.Success;
                    return TTaskStatusType.Failed;

                }
                else
                {
                    if (curPage == 54)
                        return TTaskStatusType.Success;

                    curPage++;
                    TaskBL.updateTaskMinPage(taskId, curPage.ToString());

                    if (!goToPage(curPage))
                    {
                        release(TTaskStatusType.Failed.ToString());
                        return TTaskStatusType.Failed;
                    }
                }
            }
        }

        private void writePageData()
        {
            mDataSet.filter();
            SitesBL.AddWinwinProfessionalPageTable(mDataSet.GetPageTable());
        }

        private bool getRandomRowData(List<IWebElement> mainTableRows)
        {
            int rowNum = 0;
            try
            {
                myDriver.WebDriver.SwitchTo().DefaultContent();
                rowNum = mDataSet.getRandomOpenRowNum(mainTableRows.Count);
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

        private bool getRowData(DataRow pageTableRow, IWebElement rowElement)
        {
            try
            {
                System.Collections.ObjectModel.ReadOnlyCollection<IWebElement> lines = rowElement.FindElements(By.ClassName("Row"));
                for (int i = 0; i < lines.Count; i++)
                {
                    if (lines[i].Text == "")
                        continue;
                    if (lines[i].Text.IndexOf("שם העסק") > -1)
                    {
                        pageTableRow["businessName2"] = lines[i].Text.Replace("שם העסק:", "").Trim();

                    }
                    else if (lines[i].Text.IndexOf("מקצוע") > -1)
                    {
                        pageTableRow["profession2"] = lines[i].Text.Replace("מקצוע:", "").Trim();
                    }
                    else if (lines[i].Text.IndexOf("תת קטגוריה") > -1)
                    {
                        pageTableRow["subCategory2"] = lines[i].Text.Replace("תת קטגוריה", "").Replace(":", "").Trim();
                    }
                    else if (lines[i].Text.IndexOf("אזור שירות") > -1)
                    {
                        pageTableRow["area2"] = lines[i].Text.Replace("אזור שירות:", "").Trim();
                    }
                    else if (lines[i].Text.IndexOf("מגיע ללקוח") > -1)
                    {
                        pageTableRow["isMobile"] = lines[i].Text.Replace("מגיע ללקוח:", "").Trim();
                    }

                    else if (lines[i].Text.IndexOf("טלפון:") > -1)
                    {

                        System.Collections.ObjectModel.ReadOnlyCollection<IWebElement> phoneLink = lines[i].FindElements(By.XPath(".//a"));
                        if (phoneLink.Count == 0)
                            return false;
                        phoneLink[0].Click();
                        if (lines[i].Text.Contains("לחץ כאן"))
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
                    else if (lines[i].Text.IndexOf("שם:") > -1)
                    {
                        string myString = lines[i].Text.Replace("\r\n", string.Empty);
                        myString = myString.Replace("שם:", string.Empty);
                        pageTableRow["name"] = myString;
                    }
                    else if (lines[i].Text.IndexOf("עיר:") > -1)
                    {
                        string myString = lines[i].Text.Replace("\r\n", string.Empty);
                        myString = myString.Replace("עיר:", string.Empty);
                        pageTableRow["city"] = myString;
                    }
                }

                System.Collections.ObjectModel.ReadOnlyCollection<IWebElement> freeTextlines = rowElement.FindElements(By.ClassName("ExNoteInfo"));
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



        private IWebElement clickRow(List<IWebElement> mainTableRows, int rowNum)
        {
            try
            {
                mainTableRows[rowNum].Click();
                mDataSet.AddEntry(rowNum, SiteDataSet.sOpenStatus, SiteDataSet.sRunningInternalDownloadStatus);
                string id = mainTableRows[rowNum].GetAttribute("id");
                string detailsId = id.Remove(id.IndexOf("Open"), 4) + "Div";
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

                taskTable = SitesBL.UpdateWinwinProfessionalTableRowsStatus(mDataSet.GetClonePageTable());
                mDataSet.setWinwinProfessionalTable(taskTable);

            }
            catch (Exception ex)
            {
                errorLog.handleException(ex);
                errorLog.writeToLogFile("at initPageTable  " + ex.StackTrace);
                throw ex;
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
                    tableRow[ind] = rowsTd[i].Text.Replace("'", " ").Replace('"', ' ').Trim();
                    ind++;
                }
            }
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
                        mainTableTrs.Add(curTr);
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


    }
}
