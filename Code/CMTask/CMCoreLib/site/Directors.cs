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
    class Directors : baseSite
    {
        protected SiteDataSet mDataSet = new SiteDataSet();
        public const string mainTableId = "ctl00_ContentMain_gridCompanies_GridView1";
        private IWebElement main_table;
        private const int maxRows = 14, minRows = 12;

        internal Directors(int taskId)
            : base(taskId, @"Directors\errorHandlerLog\", "Directors.log", @"Directors\infoHandlerLog\", "Directors.log", TSites.Directors.ToString())
        {

        }

        public static string getBasePageUrl()
        {
            return "http://directors.dundb.co.il/Companies.aspx";
        }

        public static string getPageUrl(int pageNum)
        {
            return "http://directors.dundb.co.il/Companies.aspx?ct=3&page=" + pageNum.ToString();
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

        private TTaskStatusType getSitePageData(string MinPage)
        {
            int curPage = 0;
            string pageUrl = getBasePageUrl();
            if (MinPage != "" && MinPage != "0")
            {
                int.TryParse(MinPage, out curPage);
                pageUrl = getPageUrl(curPage);
            }

            if (!driverUtils.NevigateToPage(myDriver.WebDriver, pageUrl))
            {
                release(TTaskStatusType.Failed.ToString());
                return TTaskStatusType.DriverError;
            }

            return getMainTableData(curPage);
        }

        
        private void writePageData()
        {
            mDataSet.filter();
            //SitesBL.AddDirectorsPageTable(mDataSet.GetPageTable());
        }

        private TTaskStatusType getMainTableData(int curPage)
        {
            int cur = 1;
            Random random = new Random();
            int randomNumber = random.Next(minRows, maxRows);
            int i = 0;

            while (true)
            {
                if (!getBasisTable())
                {
                    release(TTaskStatusType.Failed.ToString());
                    return TTaskStatusType.Failed;
                }
                bool IsTableFinish = false;
                driverUtils.CloseOtherWindows(myDriver.WebDriver);
                for (; i < randomNumber; i++)
                {
                    IsTableFinish = mDataSet.IsTableFinish();
                    if (IsTableFinish)
                        break;
                    if (!getRandomRowData())
                    {

                        release(TTaskStatusType.Failed.ToString());
                        return TTaskStatusType.Failed; ;
                    }
                }

                if (i == randomNumber)
                {
                    string url = myDriver.WebDriver.Url;
                    release(TTaskStatusType.Success.ToString());
                    if (TaskBL.AddTask(1, "GetPageData", CMLib.DateTimeSQLite(DateTime.Now), "0", TSites.Directors.ToString(), curPage.ToString()))
                        return TTaskStatusType.Success;
                    return TTaskStatusType.Failed;

                }
                else
                {
                    curPage++;
                    TaskBL.updateTaskMinPage(taskId, curPage.ToString());
                    if (!driverUtils.NevigateToPage(myDriver.WebDriver, getPageUrl(curPage)))
                    {
                        release(TTaskStatusType.Failed.ToString());
                        return TTaskStatusType.Failed;
                    }
                }
            }
        }


        private bool getRandomRowData()
        {
            int rowNum = 0;
            try
            {
                string url = "";
                rowNum = mDataSet.getRandomOpenRowNum(out url);
                if (url == "")
                    return false;
                if (!driverUtils.NevigateToPage(myDriver.WebDriver, url))
                    return false;
                bool status = getRowData(mDataSet.GetRow(rowNum));

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

        private bool getRowData(DataRow pageTableRow)
        {
            try
            {
                IWebElement main = myDriver.WebDriver.FindElement(By.Id("detailsTbl"));
                if (main == null)
                    return false;
                System.Collections.ObjectModel.ReadOnlyCollection<IWebElement> TableTrs = main.FindElements(By.XPath(".//tr"));
                foreach (IWebElement row in TableTrs)
                {

                    if (row.Text.IndexOf("שם לועזי:") > -1)
                        pageTableRow["ForeignName"] = row.Text.Replace("שם לועזי:", "").Trim();
                    else if (row.Text.IndexOf("כתובת:") > -1)
                        pageTableRow["Address"] = row.Text.Replace("כתובת:", "").Trim();
                    else if (row.Text.IndexOf("טלפון:") > -1)
                        pageTableRow["Phone"] = row.Text.Replace("טלפון:", "").Trim();
                    else if (row.Text.IndexOf("פקס:") > -1)
                        pageTableRow["Fax"] = row.Text.Replace("פקס:", "").Trim();
                    else if (row.Text.IndexOf("אימייל:") > -1)
                        pageTableRow["Mail"] = row.Text.Replace("אימייל:", "").Trim();
                    else if (row.Text.IndexOf("אינטרנט:") > -1)
                        pageTableRow["SiteAddress"] = row.Text.Replace("אינטרנט:", "").Trim();
                    else if (row.Text.IndexOf("מעמד משפטי:") > -1)
                        pageTableRow["LegalStatus"] = row.Text.Replace("מעמד משפטי:", "").Trim();
                    else if (row.Text.IndexOf("מספר רישום:") > -1)
                        pageTableRow["RegisterNumber"] = row.Text.Replace("מספר רישום:", "").Trim();
                    else if (row.Text.IndexOf("שנת יסוד:") > -1)
                        pageTableRow["EstablishedYear"] = row.Text.Replace("שנת יסוד:", "").Trim();
                    else if (row.Text.IndexOf("תחום עיסוק:") > -1)
                        pageTableRow["Domain"] = row.Text.Replace("תחום עיסוק:", "").Trim();

                }

                main = myDriver.WebDriver.FindElement(By.Id("ctl00_ContentMain_gridDirectors_GridView1"));
                if (main == null)
                    return false;
                TableTrs = main.FindElements(By.XPath(".//tr"));
                bool firstRow = true;
                DataTable PageTable = new DataTable();
                PageTable.Columns.Add("Name", typeof(string));
                PageTable.Columns.Add("Url", typeof(string));
                PageTable.Columns.Add("JobTitle", typeof(string));
                PageTable.Columns.Add("DownloadStatus", typeof(string));
                foreach (IWebElement row in TableTrs)
                {
                    if (firstRow)
                    {
                        firstRow = false;
                        continue;
                    }
                    System.Collections.ObjectModel.ReadOnlyCollection<IWebElement> RowTds = row.FindElements(By.XPath(".//td"));
                    string Name = RowTds[1].Text;
                    string JobTitle = RowTds[2].Text;
                    System.Collections.ObjectModel.ReadOnlyCollection<IWebElement> Link = row.FindElements(By.XPath(".//a"));
                    string DetailsUrl = Link[0].GetAttribute("href");
                    PageTable.Rows.Add(Name, DetailsUrl, JobTitle, "Waiting");
                }
                pageTableRow["ds_downloadStatus"] = "Success";


                base.SitesBL.AddDirectorsRow(pageTableRow, PageTable);

                return true;
            }
            catch (Exception ex)
            {
                errorLog.handleException(ex);
                errorLog.writeToLogFile("at getRowData  " + ex.StackTrace);
                return false;
            }
        }


        private bool getBasisTable()
        {
            try
            {
                mDataSet.CreatePageTable(getPageTableCols());
                DataTable taskTable = new DataTable();
                main_table = myDriver.WebDriver.FindElement(By.Id(mainTableId));
                System.Collections.ObjectModel.ReadOnlyCollection<IWebElement> TableTrs = main_table.FindElements(By.XPath(".//tr"));
                bool firstRow = true;
                foreach (IWebElement row in TableTrs)
                {
                    if (firstRow)
                    {
                        firstRow = false;
                        continue;
                    }
                    DataRow tableRow = mDataSet.GetNewRow();
                    System.Collections.ObjectModel.ReadOnlyCollection<IWebElement> RowTds = row.FindElements(By.XPath(".//td"));
                    if (RowTds.Count < 4)
                        return false;
                    tableRow["CompanyName"] = RowTds[1].Text;
                    tableRow["Categorization"] = RowTds[2].Text;
                    System.Collections.ObjectModel.ReadOnlyCollection<IWebElement> Link = row.FindElements(By.XPath(".//a"));
                    tableRow["url"] = Link[0].GetAttribute("href");
                    tableRow[SiteDataSet.sInternalStatus] = SiteDataSet.sCloseStatus;
                    string InternalDownloadStatus = SiteDataSet.sWaitingInternalDownloadStatus;
                    tableRow[SiteDataSet.sInternalDownloadStatus] = InternalDownloadStatus;
                    tableRow[SiteDataSet.sTaskId] = taskId.ToString();
                    mDataSet.AddRow(tableRow);
                }

                taskTable = SitesBL.UpdateDirectorsTableRowsStatus(mDataSet.GetClonePageTable());
                mDataSet.setDirectorsTable(taskTable);
                return true;
            }
            catch (Exception ex)
            {

                return false;
            }

        }

        private string[] getPageTableCols()
        {
            string[] cols = { "CompanyName", "url", "Categorization", "ForeignName", "Address", "Phone", "Fax", "Mail", "SiteAddress", "LegalStatus", "RegisterNumber", "EstablishedYear", "Domain" };
            return cols;
        }
    }
}
