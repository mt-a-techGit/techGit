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
    class DirectorsMembers : baseSite
    {
        protected SiteDataSet mDataSet = new SiteDataSet();
        public const string mainTableId = "ctl00_ContentMain_gridCompanies_GridView1";
        private IWebElement main_table;
        private const int maxRows = 14, minRows = 12;

        internal DirectorsMembers(int taskId)
            : base(taskId, @"DirectorsMembers\errorHandlerLog\", "DirectorsMembers.log", @"DirectorsMembers\infoHandlerLog\", "DirectorsMembers.log", TSites.DirectorsMembers.ToString())
        {

        }

        public void getPageData(string MinPage)
        {
            try
            {
                if (myDriver == null)
                    return;
                getMainTableData();
                release("Failed");
            }
            catch (Exception ex)
            {
                release(TTaskStatusType.Failed.ToString());
            }
        }



        private void getMainTableData()
        {
            try
            {
                DataTable DTDirectorsMembers = SitesBL.GetDirectorsMembers();

                for (int i = 0; i < DTDirectorsMembers.Rows.Count; i++)
                {
                    DataTable PageTable = new DataTable();
                    PageTable.Columns.Add("Name", typeof(string));
                    PageTable.Columns.Add("Url", typeof(string));
                    PageTable.Columns.Add("BirthDate", typeof(string));
                    PageTable.Columns.Add("Education", typeof(string));
                    PageTable.Columns.Add("JobDescription", typeof(string));
                    PageTable.Columns.Add("DownloadStatus", typeof(string));

                    if (!driverUtils.NevigateToPage(myDriver.WebDriver, DTDirectorsMembers.Rows[i]["url"].ToString()))
                        return;
                    IWebElement main = myDriver.WebDriver.FindElement(By.Id("c_content"));
                    System.Collections.ObjectModel.ReadOnlyCollection<IWebElement> table = main.FindElements(By.XPath(".//table"));
                    System.Collections.ObjectModel.ReadOnlyCollection<IWebElement> TableTrs = table[1].FindElements(By.XPath(".//tr"));
                    string Name = "", Url = "", BirthDate = "", Education = "", JobDescription = "";
                    foreach (IWebElement row in TableTrs)
                    {

                        if (row.Text.IndexOf("שנת לידה:") > -1)
                            BirthDate = row.Text.Replace("שנת לידה:", "").Trim();
                        else if (row.Text.IndexOf("השכלה:") > -1)
                            Education = row.Text.Replace("השכלה:", "").Trim();
                        else if (row.Text.IndexOf("תפקיד נוכחי:") > -1)
                            JobDescription = row.Text.Replace("תפקיד נוכחי:", "").Trim();


                    }
                    PageTable.Rows.Add(DTDirectorsMembers.Rows[i]["Name"], DTDirectorsMembers.Rows[i]["Url"], BirthDate, Education, JobDescription, "Success");

                    DataTable CompaniesTable = new DataTable();
                    CompaniesTable.Columns.Add("CompanyName", typeof(string));
                    CompaniesTable.Columns.Add("Url", typeof(string));
                    CompaniesTable.Columns.Add("Categorization", typeof(string));
                    CompaniesTable.Columns.Add("JobTitle", typeof(string));
                    CompaniesTable.Columns.Add("DownloadStatus", typeof(string));
                    IWebElement CompaniesMainTable = myDriver.WebDriver.FindElement(By.Id("ctl00_ContentMain_gridCompanies_GridView1"));
                    System.Collections.ObjectModel.ReadOnlyCollection<IWebElement> CompaniesTrs = CompaniesMainTable.FindElements(By.XPath(".//tr"));
                    bool firstRow = true;
                    string CompanyName = "", CompaniesUrl = "", Categorization = "", JobTitle = "";
                    foreach (IWebElement DirectorsMembersrow in CompaniesTrs)
                    {
                        if (firstRow)
                        {
                            firstRow = false;
                            continue;
                        }
                        System.Collections.ObjectModel.ReadOnlyCollection<IWebElement> RowTds = DirectorsMembersrow.FindElements(By.XPath(".//td"));
                        if (RowTds.Count < 3)
                            continue;
                        CompanyName = RowTds[1].Text;
                        Categorization = RowTds[2].Text;
                        System.Collections.ObjectModel.ReadOnlyCollection<IWebElement> Link = DirectorsMembersrow.FindElements(By.XPath(".//a"));
                        CompaniesUrl = Link[0].GetAttribute("href");
                        JobTitle = RowTds[3].Text;
                        CompaniesTable.Rows.Add(CompanyName, CompaniesUrl, Categorization, JobTitle, "Waiting");
                    }
                    SitesBL.addCompaniesTable(PageTable, CompaniesTable);

                }
            }
            catch (Exception ex)
            {
                errorLog.handleException(ex);
                errorLog.writeToLogFile("at getMainTableData  " + ex.StackTrace);
                return;
            }
        }
    }
}
