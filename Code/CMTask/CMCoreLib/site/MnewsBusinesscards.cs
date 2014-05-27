 
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
    class MnewsBusinesscards : baseSite
    {
        protected SiteDataSet mDataSet = new SiteDataSet();
        private IWebElement main_table;
        private const int maxRows = 14, minRows = 12;

        internal MnewsBusinesscards(int taskId)
            : base(taskId, @"MnewsBusinesscards\errorHandlerLog\", "MnewsBusinesscards.log", @"MnewsBusinesscards\infoHandlerLog\", "MnewsBusinesscards.log", TSites.MnewsBusinesscards.ToString())
        {


        }


        public static string getBasePageUrl()
        {
             return "http://www.mnews.co.il/businesscards?categoryId=0&value=";
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
            string pageUrl="http://www.mnews.co.il/businesscards?page="+pageNum+"&value=&categoryId=0&categories=False";
            return driverUtils.NevigateToPage(myDriver.WebDriver, pageUrl);
            
        }
        private void writePageData()
        {

            mDataSet.filter();
            SitesBL.AddMnewsBusinesscardsPageTable(mDataSet.GetPageTable());
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
                    writePageData();
                }

                if (i == randomNumber)
                {
                    string url = myDriver.WebDriver.Url;
                    release(TTaskStatusType.Success.ToString());
                    if (TaskBL.AddTask(1, "GetPageData", CMLib.DateTimeSQLite(DateTime.Now), "0", TSites.MnewsBusinesscards.ToString(), curPage.ToString()))
                        return TTaskStatusType.Success;
                    return TTaskStatusType.Failed;

                }
                else
                {
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


        private bool getRandomRowData()
        {
            int rowNum = 0;
            try
            {
                string url = "";
                rowNum = mDataSet.getRandomOpenRowNum(out url);
                if (url == "")
                    return false;
                if (!driverUtils.NevigateToPage(myDriver.WebDriver,url))
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
                
                System.Collections.ObjectModel.ReadOnlyCollection<IWebElement> main = myDriver.WebDriver.FindElements(By.ClassName("companyText"));
                if (main.Count == 0)
                    return false;
                pageTableRow["Freetext"] = main[0].Text.Replace("\n", ", ").Replace("\r", String.Empty).Replace("\t", String.Empty);
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

                System.Collections.ObjectModel.ReadOnlyCollection<IWebElement> asakimPage = myDriver.WebDriver.FindElements(By.ClassName("asakimPage"));

                System.Collections.ObjectModel.ReadOnlyCollection<IWebElement> TableTrs = asakimPage[0].FindElements(By.ClassName("companyDetails"));
                foreach (IWebElement row in TableTrs)
                {
                    DataRow tableRow = mDataSet.GetNewRow();
                    System.Collections.ObjectModel.ReadOnlyCollection<IWebElement> nameText = row.FindElements(By.ClassName("nameText"));
                    if (nameText.Count == 0)
                        return false;
                    System.Collections.ObjectModel.ReadOnlyCollection<IWebElement> link = nameText[0].FindElements(By.XPath(".//a"));
                    if (link.Count == 0)
                        return false;

                    tableRow["Name"] = link[0].Text;
                    tableRow["url"] = link[0].GetAttribute("href");
                    System.Collections.ObjectModel.ReadOnlyCollection<IWebElement> text = nameText[0].FindElements(By.ClassName("text"));
                    tableRow["Description"] = text[0].Text.Replace("למידע נוסף לחצו כאן", "").Trim();



                    System.Collections.ObjectModel.ReadOnlyCollection<IWebElement> companyCommunication = row.FindElements(By.ClassName("companyCommunication"));
                    if(companyCommunication.Count>0)
                    {
                        System.Collections.ObjectModel.ReadOnlyCollection<IWebElement> label = companyCommunication[0].FindElements(By.ClassName("rowCommunication-icon"));
                        System.Collections.ObjectModel.ReadOnlyCollection<IWebElement> value = companyCommunication[0].FindElements(By.ClassName("rowCommunication-text"));
                        if (label.Count == value.Count)
                        {
                            for (int i = 0; i < label.Count; i++)
                            {
                                System.Collections.ObjectModel.ReadOnlyCollection<IWebElement> img = label[i].FindElements(By.XPath(".//img"));
                                if (img.Count > 0)
                                {
                                    string imgHref = img[0].GetAttribute("src");
                                    if (imgHref == "http://www.mnews.co.il/pub/mnews/images/iconPhone.png")
                                         tableRow["Phone"] = value[i].Text.Trim();
                                    else if (imgHref == "http://www.mnews.co.il/pub/mnews/images/iconFax.png")
                                         tableRow["Fax"] = value[i].Text.Trim();
                                    else if (imgHref == "http://www.mnews.co.il/pub/mnews/images/iconMail.png")
                                        tableRow["Mail"] = value[i].Text.Trim();
                                    else if (imgHref == "http://www.mnews.co.il/pub/mnews/images/mobile.png")
                                        tableRow["Mobile"] = value[i].Text.Trim();
                                    else if (imgHref == "http://www.mnews.co.il/pub/mnews/images/iconWeb.png")
                                        tableRow["SiteUrl"] = value[i].Text.Trim();
                                 }
                            
                            
                            }
                        }

                    }
                    tableRow[SiteDataSet.sInternalStatus] = SiteDataSet.sCloseStatus;
                    string InternalDownloadStatus = SiteDataSet.sWaitingInternalDownloadStatus;
                    tableRow[SiteDataSet.sInternalDownloadStatus] = InternalDownloadStatus;
                    tableRow[SiteDataSet.sTaskId] = taskId.ToString();
                    mDataSet.AddRow(tableRow);
                }
               
                taskTable = SitesBL.UpdateMnewsBusinesscardsTableRowsStatus(mDataSet.GetClonePageTable());
                mDataSet.setMnewsBusinesscardsTable(taskTable);
                return true;
            }
            catch (Exception ex)
            {

                return false;
            }

        }
 
        private string[] getPageTableCols()
        {
            string[] cols = { "Name", "Description", "Phone", "Mobile", "Fax", "Mail", "SiteUrl", "Freetext","url" };
            return cols;
        }
    }
}
