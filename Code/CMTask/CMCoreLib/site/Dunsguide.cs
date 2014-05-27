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
    class Dunsguide : baseSite
    {
        protected SiteDataSet mDataSet = new SiteDataSet();
        public const string mainTableId = "no-more-tables";
        private IWebElement main_table;
        private const int maxRows = 14, minRows = 12;
        private string Category = "";
        internal Dunsguide(int taskId, string Category)
            : base(taskId, @"Dunsguide\errorHandlerLog\", "Dunsguide.log", @"Dunsguide\infoHandlerLog\", "Dunsguide.log", TSites.Dunsguide.ToString())
        {
            this.Category = Category;
        }






        public TTaskStatusType getPageData(string MinPage, string baseUrl)
        {
            try
            {
                if (myDriver == null)
                    return TTaskStatusType.DriverError;
                TTaskStatusType downloadStatusType = getSitePageData(MinPage, baseUrl);
                release(downloadStatusType.ToString());
                return downloadStatusType;
            }
            catch (Exception ex)
            {
                release(TTaskStatusType.Failed.ToString());
                return TTaskStatusType.Failed;
            }
        }

        private TTaskStatusType getSitePageData(string MinPage, string baseUrl)
        {
            int curPage = 0;
            string url = baseUrl;
            if (MinPage != "" && MinPage != "0")
            {
                int.TryParse(MinPage, out curPage);
                if (curPage == 0)
                    return TTaskStatusType.Failed;
                url = getPageUrl(curPage, baseUrl);

            }


            if (!driverUtils.NevigateToPage(myDriver.WebDriver, url))
            {
                release(TTaskStatusType.Failed.ToString());
                return TTaskStatusType.DriverError;
            }
            return getMainTableData(baseUrl, curPage);
        }



        private string getPageUrl(int curPage, string basePage)
        {

            string tmp = basePage.Substring(0, basePage.Length - 5) + "-G" + (curPage).ToString() + ".aspx";

            return tmp;
        }



        private void writePageData()
        {
            mDataSet.filter();
            SitesBL.AddDunsguidePageTable(mDataSet.GetPageTable());
        }

        private TTaskStatusType getMainTableData(string baseUrl, int curPage)
        {
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
                        writePageData();
                        release(TTaskStatusType.Failed.ToString());
                        return TTaskStatusType.Failed; ;
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
                    if (TaskBL.AddTask(1, "GetPageData", CMLib.DateTimeSQLite(DateTime.Now), "0", TSites.Dunsguide.ToString(), myDriver.WebDriver.Url))
                        return TTaskStatusType.Success;
                    return TTaskStatusType.Failed;

                }
                else
                {
                    if (curPage == 0)
                        curPage = 2;
                    else
                        curPage++;
                    TaskBL.updateDunsguideTypeTaskMinPage(Category, curPage.ToString());
                    if (!driverUtils.NevigateToPage(myDriver.WebDriver, getPageUrl(curPage, baseUrl)))
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

                System.Collections.ObjectModel.ReadOnlyCollection<IWebElement> comp_yes_right = myDriver.WebDriver.FindElements(By.ClassName("comp_yes_right"));
                if (comp_yes_right.Count > 0)
                {
                    System.Collections.ObjectModel.ReadOnlyCollection<IWebElement> comp_yes_rightTable = comp_yes_right[0].FindElements(By.XPath(".//table"));
                    if (comp_yes_rightTable.Count == 0)
                        return false;
                    System.Collections.ObjectModel.ReadOnlyCollection<IWebElement> comp_yes_rightTrs = comp_yes_rightTable[0].FindElements(By.XPath(".//tr"));
                    if (comp_yes_rightTrs.Count > 1)
                    {

                        System.Collections.ObjectModel.ReadOnlyCollection<IWebElement> comp_yes_rightTds = comp_yes_rightTrs[0].FindElements(By.XPath(".//td"));
                        for (int i = 0; i < comp_yes_rightTds.Count; i++)
                        {
                            if (comp_yes_rightTds[i].Text.IndexOf("טלפון") > -1)
                                pageTableRow["Phone"] = comp_yes_rightTds[i].Text.Replace("טלפון", "").Trim();
                            else if (comp_yes_rightTds[i].Text.IndexOf("פקס") > -1)
                                pageTableRow["Fax"] = comp_yes_rightTds[i].Text.Replace("פקס:", "").Trim();
                            else if (comp_yes_rightTds[i].Text.IndexOf("אינטרנט") > -1)
                                pageTableRow["SiteUrl"] = comp_yes_rightTds[i].Text.Replace("אינטרנט", "").Trim();
                            else if (comp_yes_rightTds[i].Text.IndexOf("דואר אלקטרוני") > -1)
                                pageTableRow["Mail"] = comp_yes_rightTds[i].Text.Replace("אימייל:", "").Trim();

                        }

                    }
                    if (comp_yes_rightTrs.Count > 2)
                    {
                        string[] lines = comp_yes_rightTrs[2].Text.Split('\n');
                        for (int i = 0; i < lines.Length; i++)
                        {
                            if (lines[i].IndexOf("אופי פעילות:") > -1)
                                pageTableRow["TypeOfActivity"] = lines[i].Replace("אופי פעילות:", "").Replace("'", "''").Trim();
                            else if (lines[i].IndexOf("תחומים:") > -1)
                                pageTableRow["FieldWork"] = lines[i].Replace("תחומים:", "").Trim();
                            else if (lines[i].IndexOf("סיווגים:") > -1)
                                pageTableRow["Classifications"] = lines[i].Replace("סיווגים:", "").Trim();
                        }
                    }
                }
                System.Collections.ObjectModel.ReadOnlyCollection<IWebElement> no_comp_left = myDriver.WebDriver.FindElements(By.ClassName("no_comp_left"));
                if (no_comp_left.Count > 0)
                {
                    System.Collections.ObjectModel.ReadOnlyCollection<IWebElement> CompanyDetails_add = no_comp_left[0].FindElements(By.ClassName("CompanyDetails_add"));
                    if (CompanyDetails_add.Count > 0)
                    {
                        pageTableRow["Address"] = CompanyDetails_add[0].Text.Replace("כתובת:", "").Replace("'", "''").Trim();
                        //todo תא דואר: split
                    }
                    System.Collections.ObjectModel.ReadOnlyCollection<IWebElement> spanPhone = myDriver.WebDriver.FindElements(By.Id("spanPhone"));
                    if (spanPhone.Count > 0)
                    {
                        if (driverUtils.ExecuteScript(myDriver.WebDriver, "document.getElementById('spanPhone').style.display = 'block'; return true ") == "Failed")
                            return false;
                        pageTableRow["Phone"] = spanPhone[0].Text;

                    }
                    string[] lines = no_comp_left[0].Text.Split('\n');
                    for (int i = 0; i < lines.Length; i++)
                    {
                        lines[i] = lines[i].Trim();
                        if (lines[i] == "")
                            continue;
                        if (lines[i].IndexOf("שם באנגלית:") > -1)
                            pageTableRow["ForeignName"] = lines[i].Replace("שם באנגלית:", "").Trim();
                        if (lines[i].IndexOf("שם לועזי:") > -1)
                            pageTableRow["ForeignName"] = lines[i].Replace("שם לועזי:", "").Trim();

                        else if (lines[i].IndexOf("ידועה בשם:") > -1)
                            pageTableRow["SecName"] = lines[i].Replace("ידועה בשם:", "").Trim();
                        else if (lines[i].IndexOf("שם נוסף:") > -1)
                            pageTableRow["SecName"] = lines[i].Replace("שם נוסף:", "").Trim();

                        else if (lines[i].IndexOf("מספר רשום:") > -1)
                            pageTableRow["RegisteredNumber"] = lines[i].Replace("מספר רשום:", "").Trim();
                        else if (lines[i].IndexOf("כתובת:") > -1)
                            pageTableRow["Address"] = lines[i].Replace("כתובת:", "").Trim();
                        //todo zipCode
                        else if (lines[i].IndexOf("שנת יסוד:") > -1)
                            pageTableRow["EstablishedYear"] = lines[i].Replace("שנת יסוד:", "").Trim();
                        else if (lines[i].IndexOf("שנת רישום:") > -1)
                            pageTableRow["RegistrationYear"] = lines[i].Replace("שנת רישום:", "").Trim();
                        else if (lines[i].IndexOf("הכנסות:") > -1)
                        {
                            pageTableRow["Income"] = lines[i].Replace("הכנסות:", "").Trim();
                            if (pageTableRow["Income"].ToString().IndexOf("מקרא") > -1)
                                pageTableRow["Income"] = "";
                        }

                        else if (lines[i].IndexOf("חברת אם:") > -1)
                            pageTableRow["ParentCompany"] = lines[i].Replace("חברת אם:", "").Trim();

                        else if (lines[i].IndexOf("מעמד משפטי:") > -1)
                            pageTableRow["LegalStatus"] = lines[i].Replace("מעמד משפטי:", "").Trim();
                        else if (lines[i].IndexOf("תא דואר:") > -1)
                            pageTableRow["PostOfficeBox"] = lines[i].Replace("תא דואר:", "").Trim();


                        else if (lines[i].IndexOf("תחום עיסוק") > -1)
                        {
                            string tmp = "";
                            i++;
                            while (i < lines.Length)
                            {
                                if (lines[i].IndexOf(":") > -1)
                                {
                                    i--;
                                    break;
                                }
                                lines[i] = lines[i].Trim();
                                if (lines[i] == "")
                                    break;
                                if (tmp != "")
                                    tmp += ",";
                                tmp += lines[i];
                                i++;
                            }
                            pageTableRow["TypeOfActivity"] = tmp;
                        }
                        else if (lines[i].IndexOf("מועסקים") > -1)
                        {
                            pageTableRow["NumberOfEmployees"] = lines[i].Replace("מועסקים:", "").Trim();
                            if (pageTableRow["NumberOfEmployees"].ToString().IndexOf("מקרא") > -1)
                                pageTableRow["NumberOfEmployees"] = "";
                        }
                        else if (lines[i].IndexOf("מנהלים") > -1)
                        {
                            string tmp = "";
                            i++;
                            while (i < lines.Length)
                            {
                                if (lines[i] == "")
                                    break;
                                lines[i] = lines[i].Trim();
                                if (lines[i].IndexOf(":") > -1)
                                {
                                    i--;
                                    break;
                                }

                                if (lines[i].IndexOf("לרשימת הדירקטורים") > -1)
                                    break;
                                if (tmp != "")
                                    tmp += ",";
                                tmp += lines[i];
                                i++;
                            }

                            pageTableRow["Directors"] = tmp;
                        }
                        else if (lines[i].IndexOf("חברות בנות") > -1)
                        {
                            string tmp = "";
                            i++;
                            while (i < lines.Length)
                            {
                                if (lines[i] == "")
                                    break;
                                lines[i] = lines[i].Trim();
                                if (lines[i].IndexOf(":") > -1)
                                {
                                    i--;
                                    break;
                                }

                                if (lines[i].IndexOf("לרשימת הדירקטורים") > -1)
                                    break;
                                if (tmp != "")
                                    tmp += ",";
                                tmp += lines[i];
                                i++;
                            }

                            pageTableRow["Subsidiary "] = tmp;
                        }
                    }


                    //System.Collections.ObjectModel.ReadOnlyCollection<IWebElement> comp_no_comp_left = no_comp_left[0].FindElements(By.XPath(".//table"));
                    //if(comp_no_comp_left.Count==0)
                    //    return false;
                    //System.Collections.ObjectModel.ReadOnlyCollection<IWebElement> no_comp_leftTrs = comp_no_comp_left[0].FindElements(By.XPath(".//tr"));




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


        private bool getBasisTable()
        {
            try
            {
                mDataSet.CreatePageTable(getPageTableCols());
                DataTable taskTable = new DataTable();
                main_table = myDriver.WebDriver.FindElement(By.Id(mainTableId));
                if (main_table == null)
                    return false;
                System.Collections.ObjectModel.ReadOnlyCollection<IWebElement> TableTrs = main_table.FindElements(By.ClassName("tr_yes"));

                foreach (IWebElement row in TableTrs)
                {

                    DataRow tableRow = mDataSet.GetNewRow();
                    System.Collections.ObjectModel.ReadOnlyCollection<IWebElement> CompanyLink = row.FindElements(By.ClassName("resultsCompanyNoName"));
                    if (CompanyLink.Count == 0)
                        return false;
                    tableRow["Name"] = CompanyLink[0].Text;
                    tableRow["Category"] = Category;
                    tableRow["url"] = CompanyLink[0].GetAttribute("href");
                    tableRow[SiteDataSet.sInternalStatus] = SiteDataSet.sCloseStatus;
                    string InternalDownloadStatus = SiteDataSet.sWaitingInternalDownloadStatus;
                    tableRow[SiteDataSet.sInternalDownloadStatus] = InternalDownloadStatus;
                    tableRow[SiteDataSet.sTaskId] = taskId.ToString();
                    mDataSet.AddRow(tableRow);
                }
                System.Collections.ObjectModel.ReadOnlyCollection<IWebElement> TableTr_no = main_table.FindElements(By.ClassName("tr_no"));

                foreach (IWebElement row in TableTr_no)
                {
                    System.Collections.ObjectModel.ReadOnlyCollection<IWebElement> TableTd_no = row.FindElements(By.XPath(".//td"));
                    if (TableTd_no.Count == 0)
                        return false;
                    DataRow tableRow = mDataSet.GetNewRow();
                    System.Collections.ObjectModel.ReadOnlyCollection<IWebElement> CompanyLink = TableTd_no[1].FindElements(By.XPath(".//a"));

                    if (CompanyLink.Count == 0)
                        return false;
                    tableRow["Name"] = CompanyLink[0].Text;
                    tableRow["Category"] = Category;
                    tableRow["url"] = CompanyLink[0].GetAttribute("href");
                    tableRow[SiteDataSet.sInternalStatus] = SiteDataSet.sCloseStatus;
                    string InternalDownloadStatus = SiteDataSet.sWaitingInternalDownloadStatus;
                    tableRow[SiteDataSet.sInternalDownloadStatus] = InternalDownloadStatus;
                    tableRow[SiteDataSet.sTaskId] = taskId.ToString();
                    mDataSet.AddRow(tableRow);
                }
                taskTable = SitesBL.UpdateDunsguideTableRowsStatus(mDataSet.GetClonePageTable());
                mDataSet.setDunsguideTable(taskTable);
                return true;
            }
            catch (Exception ex)
            {

                return false;
            }

        }

        private string[] getPageTableCols()
        {
            string[] cols = { "Category", "url", "Name", "SecName", "ForeignName", "Address", "Phone", "Fax", "Mail", "SiteUrl", "TypeOfActivity", "FieldWork", "Classifications", "ZipCode", "NumberOfEmployees", "Income", "RegisteredNumber", "EstablishedYear", "RegistrationYear", "LegalStatus", "ParentCompany", "PostOfficeBox", "Directors", "Subsidiary" };
            return cols;
        }
    }
}




