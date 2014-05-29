 
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
    class Zimmer : baseSite
    {
        protected SiteDataSet mDataSet = new SiteDataSet();
        public const string mainTableId = "searchResults",from="2004",to="2014";
        private IWebElement main_table;
        private const int maxRows = 14, minRows = 12;
        private string Category = "";
        internal Zimmer(int taskId)
            : base(taskId, @"Zimmer\errorHandlerLog\", "Zimmer.log", @"Zimmer\infoHandlerLog\", "Zimmer.log", TSites.Zimmer.ToString())
        {
           
        }
    
         public static string getBasePageUrl()
        {
            return "http://www.zimmer.co.il/";
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
            int pageNum =0;
            string url=getBasePageUrl();
            if (MinPage != "" && MinPage != "0")
                int.TryParse(MinPage, out pageNum);
               
            if (!driverUtils.NevigateToPage(myDriver.WebDriver, url))
            {
                release(TTaskStatusType.Failed.ToString());
                return TTaskStatusType.DriverError;
            }
            try
            {



                mDataSet.CreatePageTable(getPageTableCols());
                System.Collections.ObjectModel.ReadOnlyCollection<IWebElement> selects = myDriver.WebDriver.FindElements(By.XPath(".//select"));
                for (int i = 0; i < selects.Count; i++)
                {

                    if (selects[i].GetAttribute("name") == "Sreg")
                    {
                        System.Collections.ObjectModel.ReadOnlyCollection<IWebElement> selectOptions = selects[i].FindElements(By.XPath(".//option"));
                        selectOptions[pageNum].Click();
                        driverUtils.Sleep(2000, 2500);
                        IWebElement row = selects[i].FindElement(By.XPath("..")).FindElement(By.XPath(".."));

                        System.Collections.ObjectModel.ReadOnlyCollection<IWebElement> BtnSrc = row.FindElements(By.XPath(".//input"));
                        BtnSrc[0].Click();
                        driverUtils.Sleep(2000, 2500);
                        break;
                    }
                }
                int cnt = 0;
                System.Collections.ObjectModel.ReadOnlyCollection<IWebElement> formBtnDiv = myDriver.WebDriver.FindElements(By.XPath(".//table"));
                for (int i = 0; i < formBtnDiv.Count; i++)
                {
                    if (formBtnDiv[i].GetAttribute("cellspacing") == "3" && formBtnDiv[i].GetAttribute("width") == "631")
                    {
                        cnt++;
                         string city = "";
                        System.Collections.ObjectModel.ReadOnlyCollection<IWebElement> tableTrs = formBtnDiv[i].FindElements(By.XPath(".//tr"));
                        for (int j = 0; j < tableTrs.Count; j++)
                        {
                            if (tableTrs[j].Text == "")
                                continue;
                           
                            if (j == 0)
                            {
                                System.Collections.ObjectModel.ReadOnlyCollection<IWebElement> tableTds = tableTrs[j].FindElements(By.XPath(".//td"));
                                city = tableTds[2].Text.Replace(": מפה", "").Replace("צימרים ב", "").Replace("צימר ב", "").Trim();
                            }
                            if (j > 2)
                            {
                                DataRow tableRow = mDataSet.GetNewRow();

                                System.Collections.ObjectModel.ReadOnlyCollection<IWebElement> tableTds = tableTrs[j].FindElements(By.XPath(".//td"));
                                tableRow["Name"] = tableTds[0].Text.Replace("'", "''").Trim();
                                tableRow["City"] = city;
                                tableRow["UnitNumber"] = tableTds[1].Text.Replace("'", "''").Trim();
                                tableRow["Type"] = tableTds[2].Text.Replace("'", "''").Trim();
                                tableRow["Phone"] = tableTds[3].Text.Replace("'", "''").Trim();
                                tableRow["Mobile"] = tableTds[4].Text.Replace("'", "''").Trim();
                                tableRow[SiteDataSet.sInternalStatus] = SiteDataSet.sCloseStatus;
                                tableRow[SiteDataSet.sInternalDownloadStatus] = SiteDataSet.sSsuccessInternalDownloadStatus;
                                tableRow[SiteDataSet.sTaskId] = taskId.ToString();
                                mDataSet.AddRow(tableRow);
                            }
                            //for (int k = 0; k < tableTds.Count; k++)
                            //{
                            //    string text = tableTds[k].Text;
                            //}
                        }

                    }


                }
                if (SitesBL.AddZimmerPageTable(mDataSet.GetPageTable()) == false)
                    return TTaskStatusType.Failed;

                pageNum++;
                if (pageNum < 3)
                {
                    if (TaskBL.AddTask(1, "GetPageData", CMLib.DateTimeSQLite(DateTime.Now), "", TSites.Zimmer.ToString(), pageNum.ToString()))
                        return TTaskStatusType.Success;
                    return TTaskStatusType.Failed;
                }
                return TTaskStatusType.Success;
            }
            catch (Exception ex)
            {
                errorLog.handleException(ex);
                errorLog.writeToLogFile("at getBasisTable  " + ex.StackTrace);
               return TTaskStatusType.Failed;
            }
        }
 
     
        private string[] getPageTableCols()
        {
            string[] cols = { "City", "Name","UnitNumber", "Type","Phone", "Mobile"};
            return cols;
        }
    }
}



 
