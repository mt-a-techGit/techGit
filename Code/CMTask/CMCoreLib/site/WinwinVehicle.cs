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
    class WinwinVehicle : baseSite
    {
        public const string mainTableId = "savedAddsTable";
        private IWebElement main_table;
        private const int minRows = 7, maxRows = 12;
        private string city = "";
        private int compareDate = -2;
        private DateTime pageDate = DateTime.MinValue;
      
        internal WinwinVehicle(int taskId, DateTime pageDate,string city )
            : base(taskId, @"WinwinVehicle\errorHandlerLog\", "WinwinVehicle.log", @"WinwinVehicle\infoHandlerLog\", "WinwinVehicle.log", TSites.WinwinVehicle.ToString())
        {
            this.pageDate = pageDate;
            this.city = city;
        }
 
        

        public   string getBasePageUrl()
        {
            if (city == "ירושלים")
                return "http://www.winwin.co.il/Cars/Search/SearchResults/CarPage.aspx?search=eaec151e206931536a77eb277a3b1bf6";
            else if (city == "תל אביב יפו")
                return "http://www.winwin.co.il/Cars/Search/SearchResults/CarPage.aspx?search=58705be295d4ae60b888692a69503b90";
            else if (city == "חולון")
                return "http://www.winwin.co.il/Cars/Search/SearchResults/CarPage.aspx?search=8a5929cfda3e0636837504fa0977243c";
            else if (city == "פתח תקווה")
                return "http://www.winwin.co.il/Cars/Search/SearchResults/CarPage.aspx?search=c7d9ef8f835b6b24badf4a1d6cf3bb64";
            else if (city == "רמת השרון")
                return "http://www.winwin.co.il/Cars/Search/SearchResults/CarPage.aspx?search=85e71baa4b37ef9c0c8008537dd93b95";
            else return "";
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
            string[] cols = { "Type", "EngineCapacity", "Year", "PrevOwnersNum", "Gear","Price", "Area", "RowDate", "Manufacturer", "Model", "SubModel","CarEngine", "Ownership", "Odometer", "Agency","FreeText", "Name", "Phone1", "Phone2","City" };
            return cols;
        }

        private TTaskStatusType getSitePageData(string MinPage)
        {
            int curPage = 0;
            int.TryParse(MinPage, out curPage);
            string Url = MinPage;

            if (MinPage == "" || MinPage == "0")
            {
                curPage = 1;
                Url = getBasePageUrl();
                if (!driverUtils.NevigateToPage(myDriver.WebDriver, Url))
                {
                    release(TTaskStatusType.Failed.ToString());
                    return TTaskStatusType.DriverError;
                }
            }
            else
            {
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
            if (city == "ירושלים")
                return driverUtils.NevigateToPage(myDriver.WebDriver, "http://www.winwin.co.il/Cars/Search/SearchResults/CarPage.aspx?PageNumberBottom=" + pageNum + "&PageNumberTop=" + pageNum + "&search=eaec151e206931536a77eb277a3b1bf6");
            else if (city == "תל אביב יפו")
                return driverUtils.NevigateToPage(myDriver.WebDriver, "http://www.winwin.co.il/Cars/Search/SearchResults/CarPage.aspx?PageNumberBottom=" + pageNum + "&PageNumberTop=" + pageNum + "&search=58705be295d4ae60b888692a69503b90");
            else if (city == "חולון")
                return driverUtils.NevigateToPage(myDriver.WebDriver, "http://www.winwin.co.il/Cars/Search/SearchResults/CarPage.aspx?PageNumberBottom=" + pageNum + "&PageNumberTop=" + pageNum + "&search=8a5929cfda3e0636837504fa0977243c");
            else if (city == "פתח תקווה")
                return driverUtils.NevigateToPage(myDriver.WebDriver, "http://www.winwin.co.il/Cars/Search/SearchResults/CarPage.aspx?PageNumberBottom=" + pageNum + "&PageNumberTop=" + pageNum + "&search=c7d9ef8f835b6b24badf4a1d6cf3bb64");

            else if (city == "רמת השרון")
                return driverUtils.NevigateToPage(myDriver.WebDriver, "http://www.winwin.co.il/Cars/Search/SearchResults/CarPage.aspx?PageNumberBottom=" + pageNum + "&PageNumberTop=" + pageNum + "&search=85e71baa4b37ef9c0c8008537dd93b95");
            else
                return false;
       }

        private TTaskStatusType getMainTableData(int curPage)
        {
            int cur = 1;

            Random random = new Random();
            int randomNumber = random.Next(minRows, maxRows);
            int i = 0;
            bool isTaskFinish = false;

            while (true)
            {   driverUtils.CloseOtherWindows(myDriver.WebDriver);
                List<IWebElement> mainTableRows = getBasisTable();
                
                if (mainTableRows == null)
                {
                    release(TTaskStatusType.Failed.ToString());
                    return TTaskStatusType.Failed;
                }

                bool IsTableFinish = false;
                int dateRowsCount = mDataSet.dateRowsCount(pageDate, out isTaskFinish);
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
                       if (!writePageData())
                           return TTaskStatusType.Failed; 
                       driverUtils.screeshot(myDriver.WebDriver, "..//screenShot//taskName" + taskId + "_" + DateTime.Now.Hour.ToString() + DateTime.Now.Minute.ToString() + DateTime.Now.Second.ToString() + ".jpg");
                   }
                   if (i == randomNumber)
                   {
                       string url = myDriver.WebDriver.Url;
                       release(TTaskStatusType.Success.ToString());
                       if (TaskBL.AddTask(1, "GetPageData", CMLib.DateTimeSQLite(pageDate), city, TSites.WinwinVehicle.ToString(), curPage.ToString()))
                           return TTaskStatusType.Success;
                       return TTaskStatusType.Failed;
                   }
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
                curPage++;
                TaskBL.updateTaskMinPage(taskId, curPage.ToString());
                if (!goToPage(curPage))
                {
                    release(TTaskStatusType.Failed.ToString());
                     return TTaskStatusType.Failed;
                }
            }
        }

        private bool writePageData()
        {
            mDataSet.filter();
           return  SitesBL.AddWinwinVehiclePageTable(mDataSet.GetPageTable());
        }

        private bool getRandomRowData(List<IWebElement> mainTableRows)
        {
            int rowNum = 0;
            try
            {
                myDriver.WebDriver.SwitchTo().DefaultContent();
                rowNum = mDataSet.getRandomOpenRowNum(mainTableRows.Count,pageDate);
                IWebElement rowElement = clickRow(mainTableRows, rowNum);
                if (rowElement == null)
                    return false;
                driverUtils.Sleep(7000, 10000);
                
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
                    string rowText = lines[i].Text;


                    if (rowText == "")
                        continue;
                    if (rowText.IndexOf("יצרן:") > -1)
                    {
                        pageTableRow["Manufacturer"] = rowText.Replace("יצרן:", "").Trim();

                    }
                    else if (rowText.IndexOf("תת-דגם:") > -1)
                    {
                        pageTableRow["SubModel"] = rowText.Replace("תת-דגם:", "").Trim();
                    }
                    else if (rowText.IndexOf("דגם:") > -1)
                    {
                        pageTableRow["Model"] = rowText.Replace("דגם:", "").Trim();
                    }
                   
                    else if (rowText.IndexOf("מנוע:") > -1)
                    {
                        pageTableRow["CarEngine"] = rowText.Replace("מנוע:", "").Trim();
                    }
                    else if (rowText.IndexOf("בעלות") > -1)
                    {
                        pageTableRow["Ownership"] = rowText.Replace("בעלות:", "").Trim();
                    }
                    else if (rowText.IndexOf("ק מ:") > -1)
                    {
                        pageTableRow["Odometer"] = rowText.Replace("ק מ:", "").Trim();
                    }
                    else if (rowText.IndexOf("בעלות") > -1)
                    {
                        pageTableRow["Ownership"] = rowText.Replace("בעלות:", "").Trim();
                    }
                    
                        
                    else if (rowText.IndexOf("טלפון:") > -1)
                    {

                        System.Collections.ObjectModel.ReadOnlyCollection<IWebElement> phoneLink = lines[i].FindElements(By.XPath(".//a"));
                        if (phoneLink.Count == 0)
                            return false;
                        phoneLink[0].Click();
                        driverUtils.Sleep(1000, 2000);
                        rowText = lines[i].Text;
                        if (rowText.Contains("לחץ כאן"))
                            return false;
                        string myString = rowText.Replace("\r\n", string.Empty);
                        myString = myString.Replace("טלפון 1:", string.Empty);
                        myString = myString.Replace("טלפון:", string.Empty);
                        pageTableRow["phone1"] = myString;

                    }
                    else if (rowText.IndexOf("טלפון 2:") > -1)
                    {
                        string myString = rowText.Replace("\r\n", string.Empty);
                        myString = myString.Replace("טלפון 2:", string.Empty);
                        pageTableRow["phone2"] = myString;

                    }
                    else if (rowText.IndexOf("שם:") > -1)
                    {
                        string myString = rowText.Replace("\r\n", string.Empty);
                        myString = myString.Replace("שם:", string.Empty);
                        pageTableRow["name"] = myString;
                    }
                    else if (rowText.IndexOf("עיר:") > -1)
                    {
                        string myString = rowText.Replace("\r\n", string.Empty);
                        myString = myString.Replace("עיר:", string.Empty);
                        pageTableRow["city"] = myString;
                    }
                }

                System.Collections.ObjectModel.ReadOnlyCollection<IWebElement> freeTextlines = rowElement.FindElements(By.ClassName("ExNoteInfo"));
                if(freeTextlines.Count>0)
                    pageTableRow["FreeText"] = freeTextlines[0].Text.Replace("'"," ").Replace('"',' ');
                

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
                 myDriver.WebDriver.SwitchTo().DefaultContent();
                string id = mainTableRows[rowNum].GetAttribute("id");
                string detailsId = id.Remove(id.IndexOf("Open"), 4);
                IWebElement WebElement = myDriver.WebDriver.FindElement(By.Id(detailsId));
                return WebElement;
            }
            catch (Exception ex)
            {
                errorLog.handleException(ex);
                errorLog.writeToLogFile("at clickRow  " + ex.StackTrace);
                return null;
            }
        }

        private void initPageTable(List<IWebElement> mainTableRows, string Type)
        {
            try
            {
               
                foreach (IWebElement row in mainTableRows)
                {
                    DataRow tableRow = mDataSet.GetNewRow();
                    initPageRow(row, tableRow);
                    tableRow[SiteDataSet.sInternalStatus] = SiteDataSet.sCloseStatus;
                    if (Type == "True")
                        tableRow["Agency"] = "True";
                    string InternalDownloadStatus = SiteDataSet.sWaitingInternalDownloadStatus;
                    tableRow[SiteDataSet.sInternalDownloadStatus] = InternalDownloadStatus;
                    tableRow[SiteDataSet.sTaskId] = taskId.ToString();
                    mDataSet.AddRow(tableRow);
                }

               

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
                    tableRow[ind] = rowsTd[i].Text.Replace("'", "''").Trim();
                    ind++;
                }
            }
        }

        private List<IWebElement> getBasisTable()
        {
            try
            {
                myDriver.WebDriver.SwitchTo().DefaultContent();
                //main_table = myDriver.WebDriver.FindElement(By.Id(mainTableId));

                System.Collections.ObjectModel.ReadOnlyCollection<IWebElement> Tables = myDriver.WebDriver.FindElements(By.Id(mainTableId));


                mDataSet.CreatePageTable(getPageTableCols());
                DataTable taskTable = new DataTable();
                List<IWebElement> mainTableRows = new List<IWebElement>();
                getMainTableRows(Tables[0], mainTableRows);
                initPageTable(mainTableRows, "True");
                if (mainTableRows == null)
                    return null;
                taskTable = SitesBL.UpdateWinwinVehicleTableRowsStatus(mDataSet.GetClonePageTable());
                if (taskTable == null)
                    return null;
                mDataSet.setWinwinVehicleTable(taskTable);

                return mainTableRows;
            }
            catch (Exception ex)
            {
                errorLog.handleException(ex);
                errorLog.writeToLogFile("at getBasisTable  " + ex.StackTrace);
                return null;
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

        private void getMainTableRows(IWebElement table, List<IWebElement> mainTableTrs)
        {
            compareDate = -2;
            try
            {
                System.Collections.ObjectModel.ReadOnlyCollection<IWebElement> TableTrs = table.FindElements(By.XPath(".//tr"));
               foreach (IWebElement curTr in TableTrs)
                {
                    string className = curTr.GetAttribute("class");
                    if (className == "paid" || className == "TitleData")
                    {
                        System.Collections.ObjectModel.ReadOnlyCollection<IWebElement> TableTds = curTr.FindElements(By.XPath(".//td"));
                        mainTableTrs.Add(curTr);
                        string dd = TableTds[10].Text;
                        DateTime rowDate = DateTime.MaxValue;
                        DateTime.TryParse(dd, out rowDate);
                        if (compareDate != 0)
                            compareDate = comparePageDate(rowDate);
                    }
                   
                }
                return  ;
            }
            catch (Exception ex)
            {
                errorLog.handleException(ex);
                errorLog.writeToLogFile("at getMainTableRows  " + ex.StackTrace);
                return  ;
            }
        }

    }
}

