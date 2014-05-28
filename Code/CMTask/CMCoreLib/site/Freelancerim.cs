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
    class Freelancerim : baseSite
    {
        protected SiteDataSet mDataSet = new SiteDataSet();
        public const string mainTableId = "SiteContent";
        private IWebElement main_table;
        private const int maxRows = 14, minRows = 12;

        internal Freelancerim(int taskId)
            : base(taskId, @"Freelancerim\errorHandlerLog\", "Freelancerim.log", @"Freelancerim\infoHandlerLog\", "Freelancerim.log", TSites.Freelancerim.ToString())
        {

        }

        public static string getBasePageUrl()
        {
            return "http://www.freelancerim.co.il/Freelancers/";
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
            int curPage = 1;
        
            if (!driverUtils.NevigateToPage(myDriver.WebDriver, getBasePageUrl()))
            {
                release(TTaskStatusType.Failed.ToString());
                return TTaskStatusType.DriverError;
            }
            if (MinPage != "" && MinPage != "0")
            {
                int.TryParse(MinPage, out curPage);
                if (!goToPage(curPage))
                {
                    release(TTaskStatusType.Failed.ToString());
                    return TTaskStatusType.DriverError;
                }
            }
            return getMainTableData(curPage);
        }

        private bool goToPage(int pageNum)
        {
           if( driverUtils.ExecuteScript(myDriver.WebDriver, "FilterItemsByPage("+pageNum+"); return false;")=="Failed")
                return false;
           return true;
        }
        private bool writePageData()
        {
            mDataSet.filter();
            return SitesBL.AddFreelancerimPageTable(mDataSet.GetPageTable());
        }

        private TTaskStatusType getMainTableData(int curPage)
        {
            int cur = 1;
            Random random = new Random();
            int randomNumber = random.Next(minRows, maxRows);
            int i = 0;
             
            while (true)
            {
                if(!  getBasisTable())
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
                        return TTaskStatusType.Failed;;
                    }
                }
                if (i > 0)
                {
                    if(!writePageData())
                        return TTaskStatusType.Failed; 
                }

                if (i == randomNumber)
                {
                    string url = myDriver.WebDriver.Url;
                    release(TTaskStatusType.Success.ToString());
                    if (TaskBL.AddTask(1, "GetPageData", CMLib.DateTimeSQLite(DateTime.Now), "0", TSites.Freelancerim.ToString(), curPage.ToString()))
                        return TTaskStatusType.Success;
                    return TTaskStatusType.Failed;

                }
                else
                {
                    curPage++;
                    TaskBL.updateTaskMinPage(taskId, curPage.ToString());
                    if (!driverUtils.NevigateToPage(myDriver.WebDriver, getBasePageUrl()))
                    {
                        release(TTaskStatusType.Failed.ToString());
                        return TTaskStatusType.DriverError;
                    }
                    if (!goToPage(curPage))
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
                for (int i = 0; i < 2; i++)
                {
                    if (!driverUtils.NevigateToPage(myDriver.WebDriver, url))
                        return false;
                    bool status = getRowData(mDataSet.GetRow(rowNum));
                    if (status)
                    {
                        mDataSet.SetDownloadStatusForRow(rowNum, SiteDataSet.sSsuccessInternalDownloadStatus);
                        return true;
                    }
                }
                mDataSet.SetDownloadStatusForRow(rowNum, SiteDataSet.sFailedInternalDownloadStatus);
                return false;
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
                System.Collections.ObjectModel.ReadOnlyCollection<IWebElement> main = myDriver.WebDriver.FindElements(By.Id("UserProfileDataDiv"));
                if (main.Count == 0)
                    return false;

                System.Collections.ObjectModel.ReadOnlyCollection<IWebElement> RightInnerDiv = main[0].FindElements(By.ClassName("UserProfileDataRightInnerDiv"));
                System.Collections.ObjectModel.ReadOnlyCollection<IWebElement> RightInnerDivlines = RightInnerDiv[0].FindElements(By.ClassName("UserProfileDetailsDiv"));
                for (int i = 0; i < RightInnerDivlines.Count; i++)
                {

                    if (RightInnerDivlines[i].Text.IndexOf("פעיל באתר מ -") > -1)
                        pageTableRow["ActiveDate"] = RightInnerDivlines[i].Text.Replace("פעיל באתר מ -", "").Trim();
                    else if (RightInnerDivlines[i].Text.IndexOf("אזורי פעילות:") > -1)
                        pageTableRow["Areas"] = RightInnerDivlines[i].Text.Replace("אזורי פעילות:", "").Trim();

                }
                System.Collections.ObjectModel.ReadOnlyCollection<IWebElement> UserProfileSpecialty = main[0].FindElements(By.ClassName("UserProfileSpecialtyDiv"));
                pageTableRow["ProfileSpecialty"] = UserProfileSpecialty[0].Text.Replace("'", "").Trim();
                pageTableRow["SubProfileSpecialty"] = "";
                for (int i = 1; i < UserProfileSpecialty.Count; i++)
                {
                    if (i > 1)
                        pageTableRow["SubProfileSpecialty"] += ", ";
                    pageTableRow["SubProfileSpecialty"] += UserProfileSpecialty[i].Text.Replace("'", "").Trim();
                }
                System.Collections.ObjectModel.ReadOnlyCollection<IWebElement> UserProfileAboutText = main[0].FindElements(By.ClassName("UserProfileAboutText"));
                if (UserProfileAboutText.Count > 0)
                    pageTableRow["FreeText"] = UserProfileAboutText[0].Text.Replace("'", "").Trim();
                IWebElement UserProfileContactPhone = myDriver.WebDriver.FindElement(By.Id("UserProfileContactPhone"));
                driverUtils.ExecuteScript(myDriver.WebDriver, "document.getElementById('UserProfileContactPhone').style.display = 'block'; return true ");
                pageTableRow["Phone"] = UserProfileContactPhone.Text.Trim();
                if (UserProfileContactPhone.Text.Trim() == "לא נקבע")
                    pageTableRow["Phone"] = "";
                IWebElement UserProfileContactFax = myDriver.WebDriver.FindElement(By.Id("UserProfileContactFax"));
                driverUtils.ExecuteScript(myDriver.WebDriver, "document.getElementById('UserProfileContactFax').style.display = 'block'; return true ");

                pageTableRow["Fax"] = UserProfileContactFax.Text.Trim();
                if (UserProfileContactFax.Text.Trim() == "לא נקבע")
                    pageTableRow["Fax"] = "";
                IWebElement UserProfileContactEmail = myDriver.WebDriver.FindElement(By.Id("UserProfileContactEmail"));
                driverUtils.ExecuteScript(myDriver.WebDriver, "document.getElementById('UserProfileContactEmail').style.display = 'block'; return true");

                pageTableRow["Mail"] = UserProfileContactEmail.Text.Trim();
                if (UserProfileContactEmail.Text.Trim() == "לא נקבע")
                    pageTableRow["Mail"] = "";
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
                driverUtils.Sleep(12000, 14000);
                main_table = myDriver.WebDriver.FindElement(By.Id(mainTableId));
                System.Collections.ObjectModel.ReadOnlyCollection<IWebElement> TableTrs = main_table.FindElements(By.ClassName("FreelancerProViewInnerDiv"));
                foreach (IWebElement row in TableTrs)
                {
                    DataRow tableRow = mDataSet.GetNewRow();
                    System.Collections.ObjectModel.ReadOnlyCollection<IWebElement> ProViewName = row.FindElements(By.ClassName("FreelancerProViewName"));
                    if (ProViewName.Count == 0)
                        return false;
                    tableRow["url"] = ProViewName[0].GetAttribute("href");
                    tableRow["Name"] = ProViewName[0].Text;
                    if (ProViewName.Count >1)
                        tableRow["UserProfileCompany"] = ProViewName[1].Text;
                    tableRow[SiteDataSet.sInternalStatus] = SiteDataSet.sCloseStatus;
                    string InternalDownloadStatus = SiteDataSet.sWaitingInternalDownloadStatus;
                    tableRow[SiteDataSet.sInternalDownloadStatus] = InternalDownloadStatus;
                    tableRow[SiteDataSet.sTaskId] = taskId.ToString();
                    mDataSet.AddRow(tableRow);
                }
               
                taskTable = SitesBL.UpdateFreelancerimTableRowsStatus(mDataSet.GetClonePageTable());
                mDataSet.setFreelancerimTable(taskTable);
                return true;
            }
            catch (Exception ex)
            {

                return false;
            }

        }
     
        private string[] getPageTableCols()
        {
            string[] cols = { "Category", "url", "Name", "UserProfileCompany", "ActiveDate", "Areas", "ProfileSpecialty", "SubProfileSpecialty", "FreeText", "Phone","Fax","Mail" };
            return cols;
        }
    }
}
