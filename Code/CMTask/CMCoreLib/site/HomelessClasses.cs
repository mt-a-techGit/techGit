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
    class HomelessClasses : baseSite
    {
        protected SiteDataSet mDataSet = new SiteDataSet();
        public const string mainTableId = "indexwrap";
        private IWebElement main_table;
        private const int maxRows = 14, minRows = 12;

        internal HomelessClasses(int taskId)
            : base(taskId, @"HomelessClasses\errorHandlerLog\", "HomelessClasses.log", @"HomelessClasses\infoHandlerLog\", "HomelessClasses.log", TSites.HomelessClasses.ToString())
        {


        }


        public static string getBasePageUrl(string type)
        {
             switch (type)
                        {
                 case "Classes":
                     return "http://www.homeless.co.il/courses/inumber3=2";
                 case "Courses":
                     return "http://www.homeless.co.il/courses/inumber3=1";
                 case "Workshop":
                     return "http://www.homeless.co.il/courses/inumber3=3";
                 case "Lecture":
                     return "http://www.homeless.co.il/courses/inumber3=4";
                 case "PrivateClasses":
                     return "http://www.homeless.co.il/courses/inumber3=6";
                 case "Meetings":
                     return "http://www.homeless.co.il/courses/inumber3=5";
                    

             }
             return "";
        }

        public TTaskStatusType getPageData(string MinPage,string type)
        {
            try
            {
                if (myDriver == null)
                    return TTaskStatusType.DriverError;
                TTaskStatusType downloadStatusType = getSitePageData(MinPage,type);
                return downloadStatusType;
            }
            catch (Exception ex)
            {
                release(TTaskStatusType.Failed.ToString());
                return TTaskStatusType.Failed;
            }
        }

        private TTaskStatusType getSitePageData(string MinPage,string type)
        {
            int curPage = 0;
            int.TryParse(MinPage, out curPage);
            string Url = MinPage;

            if (MinPage == "" || MinPage == "0")
                Url = getBasePageUrl(type);

            if (!driverUtils.NevigateToPage(myDriver.WebDriver, Url))
            {
                release(TTaskStatusType.Failed.ToString());
                return TTaskStatusType.DriverError;
            }
            return getMainTableData(curPage,type);
        }

        private bool goToPage(int pageNum,string type)
        {
            switch (type)
            {
                case "Classes":
                    return driverUtils.NevigateToPage(myDriver.WebDriver, "http://www.homeless.co.il/courses/inumber3=2/" + pageNum);
                case "Courses":
                    return driverUtils.NevigateToPage(myDriver.WebDriver, "http://www.homeless.co.il/courses/inumber3=1/" + pageNum);
                case "Workshop":
                    return driverUtils.NevigateToPage(myDriver.WebDriver, "http://www.homeless.co.il/courses/inumber3=3/" + pageNum);
                case "Lecture":
                    return driverUtils.NevigateToPage(myDriver.WebDriver, "http://www.homeless.co.il/courses/inumber3=4/" + pageNum);
                case "PrivateClasses":
                    return driverUtils.NevigateToPage(myDriver.WebDriver, "http://www.homeless.co.il/courses/inumber3=6/" + pageNum);
                case "Meetings":
                    return driverUtils.NevigateToPage(myDriver.WebDriver, "http://www.homeless.co.il/courses/inumber3=5/" + pageNum);
                   
            }
            return false;
        }
        private void writePageData()
        {
            mDataSet.filter();
            SitesBL.AddHomelessClassesPageTable(mDataSet.GetPageTable());
        }

        private TTaskStatusType getMainTableData(int curPage,string type)
        {
            int cur = 1;

            Random random = new Random();
            int randomNumber = random.Next(minRows, maxRows);
            int i = 0;
            driverUtils.Sleep(20000, 22000);
            bool isTaskFinish = false;
            System.Collections.ObjectModel.ReadOnlyCollection<IWebElement> fancybox = myDriver.WebDriver.FindElements(By.ClassName("fancybox-item"));
            if (fancybox.Count > 0)
                fancybox[0].Click();
            while (true)
            {
                if(!  getBasisTable(type))
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
                    if (TaskBL.AddTask(1, "GetPageData", CMLib.DateTimeSQLite(DateTime.Now), "0", TSites.WinwinProfessional.ToString(), curPage.ToString()))
                        return TTaskStatusType.Success;
                    return TTaskStatusType.Failed;

                }
                else
                {
               
                    if (curPage == 0)
                        curPage = 2;
                    else
                        curPage++;
                    TaskBL.updateTaskMinPage(taskId, curPage.ToString());

                    if (!goToPage(curPage,type))
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
                System.Collections.ObjectModel.ReadOnlyCollection<IWebElement> main = myDriver.WebDriver.FindElements(By.Id("addetails"));
                if (main.Count == 0)
                    return false;
                System.Collections.ObjectModel.ReadOnlyCollection<IWebElement> lines = main[0].FindElements(By.ClassName("details_row"));

                 
                for (int i = 0; i < lines.Count; i++)
                {
                    System.Collections.ObjectModel.ReadOnlyCollection<IWebElement> tds = lines[i].FindElements(By.XPath(".//td") );
                    for (int j = 0; j < tds.Count-1; j++)
                    {
                        if (tds[j].Text.IndexOf("כתובת") > -1)
                        {
                            pageTableRow["address"] = tds[j + 1].Text.Trim();

                        }
                        else
                            if (tds[j].Text.IndexOf("טלפון:") > -1)
                            {
                                pageTableRow["phone"] = tds[j + 1].Text.Trim();

                            }
                        else if (tds[j].Text.IndexOf("איש קשר:") > -1)
                        {
                            pageTableRow["name"] = tds[j + 1].Text.Trim();

                        }
                        

                    }
                    
                    
                }
                System.Collections.ObjectModel.ReadOnlyCollection<IWebElement> hlPrice = myDriver.WebDriver.FindElements(By.ClassName("hlPrice"));
                if (hlPrice.Count == 1)
                {
                    pageTableRow["price"] = hlPrice[0].Text;
                }
                System.Collections.ObjectModel.ReadOnlyCollection<IWebElement> sepDesc = myDriver.WebDriver.FindElements(By.ClassName("sepDesc"));
                for (int i = 0; i < sepDesc.Count; i++)
                {
                    if (sepDesc[i].Text.IndexOf("תיאור הפעילות:") > -1)
                    {
                       IWebElement freeText= driverUtils.GetParent(sepDesc[i]);
                       pageTableRow["freeText"] = freeText.Text.Replace("תיאור הפעילות:","");
                    }
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

    
        private bool getBasisTable(string type)
        {
            try
            {
                System.Collections.ObjectModel.ReadOnlyCollection<IWebElement> fancybox = myDriver.WebDriver.FindElements(By.ClassName("fancybox-item"));
                if (fancybox.Count > 0)
                    fancybox[0].Click();
                mDataSet.CreatePageTable(getPageTableCols());
                DataTable taskTable = new DataTable();
                main_table = myDriver.WebDriver.FindElement(By.Id(mainTableId));
                System.Collections.ObjectModel.ReadOnlyCollection<IWebElement> TableTrs = main_table.FindElements(By.ClassName("searchresult"));
                foreach (IWebElement row in TableTrs)
                {
                    DataRow tableRow = mDataSet.GetNewRow();
                    System.Collections.ObjectModel.ReadOnlyCollection<IWebElement> title = row.FindElements(By.ClassName("title"));
                    System.Collections.ObjectModel.ReadOnlyCollection<IWebElement> location = row.FindElements(By.ClassName("location"));

                    System.Collections.ObjectModel.ReadOnlyCollection<IWebElement> label = location[0].FindElements(By.ClassName("label"));
                    System.Collections.ObjectModel.ReadOnlyCollection<IWebElement> value = location[0].FindElements(By.ClassName("value"));
                    if (label.Count == 0 || label.Count != value.Count)
                        return false;
                    for (int ind = 0; ind < label.Count; ind++)
                    {
                        tableRow["type"] = type;
                        if (label[ind].Text == "אזור:")
                        {
                            tableRow["area"] = value[ind].Text;

                        }
                        else
                            if (label[ind].Text == "כתובת:")
                            {
                                tableRow["address"] = value[ind].Text;

                            }
                            else
                                if (label[ind].Text == "קטגוריה:")
                                {
                                    tableRow["category"] = value[ind].Text;

                                }



                    }
                    if (title.Count > 0)
                    {
                        tableRow["courseName"] = title[0].Text;
                        System.Collections.ObjectModel.ReadOnlyCollection<IWebElement> link = title[0].FindElements(By.XPath(".//a"));
                        if (link.Count > 0)
                            tableRow["url"] = link[0].GetAttribute("href"); ;
                    }


                    tableRow[SiteDataSet.sInternalStatus] = SiteDataSet.sCloseStatus;
                    string InternalDownloadStatus = SiteDataSet.sWaitingInternalDownloadStatus;
                    tableRow[SiteDataSet.sInternalDownloadStatus] = InternalDownloadStatus;
                    tableRow[SiteDataSet.sTaskId] = taskId.ToString();
                    mDataSet.AddRow(tableRow);
                }
               
                taskTable = SitesBL.UpdateHomelessClassesTableRowsStatus(mDataSet.GetClonePageTable());
                mDataSet.setHomelessClassesTable(taskTable);
                return true;
            }
            catch (Exception ex)
            {

                return false;
            }

        }


        private string[] getPageTableCols()
        {
            string[] cols = { "type", "category", "courseName", "area", "address", "url", "department", "phone", "name", "price", "freeText" };
            return cols;
        }
    }
}
