using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data;

using CMCore.site;
using ManageLogFile;

namespace CMCore.data
{
    public class SiteDataSet
    {
        private LogFile errorLog = new LogFile(@"SiteDataSet\errorHandlerLog\", "SiteDataSet.log");
        private LogFile infoLog = new LogFile(@"SiteDataSet\infoHandlerLog\", "SiteDataSet.log");

        protected DataTable mPageTable;
        public const string sInternalStatus = "ds_status";
        public const string sInternalDownloadStatus = "ds_downloadStatus";
        public const string sTaskId = "ds_taskId";
        public const string sCloseStatus = "close";
        public const string sOpenStatus = "open";
        public const string sWaitingInternalDownloadStatus = "Waiting";
        public const string sSsuccessInternalDownloadStatus = "Success";
        public const string sFailedInternalDownloadStatus = "Failed";
        public const string sRunningInternalDownloadStatus = "Running";
        internal SiteDataSet()
        {
        }

        public int Count { get { return mPageTable.Rows.Count; } }

        internal string GetStatus(DateTime date)
        {
            for (int i = 0; i < mPageTable.Rows.Count; i++)
            {
                if (mPageTable.Rows[i][SiteDataSet.sInternalDownloadStatus].ToString() != SiteDataSet.sSsuccessInternalDownloadStatus)
                    return SiteDataSet.sFailedInternalDownloadStatus;
            }
            return SiteDataSet.sSsuccessInternalDownloadStatus;
        }

        internal int dateRowsCount(DateTime date,out bool isTaskFinish)
        {
            DateTime minRowDate = DateTime.MinValue;
            DateTime maxRowDate = DateTime.MaxValue;
        
            int count = 0;
            for (int i = 0; i < mPageTable.Rows.Count; i++)
            {
               
                DateTime rowDate = DateTime.MinValue;
                DateTime.TryParse(mPageTable.Rows[i]["rowDate"].ToString(), out rowDate);
                
                if (minRowDate == DateTime.MinValue || rowDate.CompareTo(minRowDate) == -1)
                    minRowDate = rowDate;
                 if (maxRowDate == DateTime.MaxValue || rowDate.CompareTo(maxRowDate) == 1)
                    maxRowDate = rowDate;
            
                if (rowDate == date)
                    count++;
            }

            if(maxRowDate.CompareTo(date)==-1 || (maxRowDate.CompareTo(date)==1 && minRowDate.CompareTo(date)==-1)  )
                isTaskFinish=true;
              else
            isTaskFinish=false;
            return count;
        }

        

        internal int filterDate(DateTime date)
        {
            if (mPageTable == null)
                return 0;
            for (int i = 0; i < mPageTable.Rows.Count; i++)
            {

                if (mPageTable.Rows[i][sInternalDownloadStatus].ToString() != sSsuccessInternalDownloadStatus)
                {
                    mPageTable.Rows.RemoveAt(i);
                    i--;
                }
            }
            return mPageTable.Rows.Count;
        }

        internal DataTable GetPageTable()
        {
            return mPageTable;
        }

        internal DataTable GetClonePageTable()
        {
            DataTable dt = mPageTable.Copy();
            return dt;
        }

        internal void Init(string[] tableCols)
        {
            CreatePageTable(tableCols);
        }

        internal void CreatePageTable(string[] tableCols)
        {
            mPageTable = new DataTable();
            DataTable workTable = new DataTable();
            foreach (string col in tableCols)
                 mPageTable.Columns.Add(col, typeof(string));
            mPageTable.Columns.Add(sInternalStatus, typeof(string));
            mPageTable.Columns.Add(sInternalDownloadStatus, typeof(string));
            mPageTable.Columns.Add(sTaskId, typeof(string));
        }

        internal void AddEntry(int rowNum, string status, string downloadStatus)
        {
            //TODO this is incorrect status is set from the outside!
            mPageTable.Rows[rowNum][sInternalStatus] = status;
            mPageTable.Rows[rowNum][sInternalDownloadStatus] = downloadStatus;
        }

        internal int getRandomOpenRowNum(int maxVal, DateTime date)
        {
            Random random = new Random();
            int rowNum = random.Next(0, maxVal);
            DateTime rowDate = DateTime.MinValue;
            DateTime.TryParse(mPageTable.Rows[rowNum]["rowDate"].ToString(), out rowDate);
            while (rowDate.ToShortDateString() != date.ToShortDateString() || mPageTable.Rows[rowNum][sInternalStatus].ToString() == sOpenStatus || mPageTable.Rows[rowNum][sInternalDownloadStatus].ToString() != sWaitingInternalDownloadStatus)
            {
                rowNum = random.Next(0, maxVal);
                DateTime.TryParse(mPageTable.Rows[rowNum]["rowDate"].ToString(), out rowDate);
            }
            return rowNum;
        }

        internal DataRow GetRow(int rowNum)
        {
            return mPageTable.Rows[rowNum];
        }

        internal void SetDownloadStatusForRow(int rowNum, string status)
        {
            mPageTable.Rows[rowNum][sInternalDownloadStatus] = status;
        }

        internal DataRow GetNewRow()
        {
            return mPageTable.NewRow();
        }

        internal void setTable(DataTable newTable)
        {
            try
            {
                for (int j = 0; j < mPageTable.Rows.Count; j++)
                {
                    mPageTable.Rows[j][sInternalDownloadStatus] = "Waiting";
                    for (int i = 0; i < newTable.Rows.Count; i++)
                    {
                        bool isRow = true;
                        if (newTable.Rows[i]["Type"].ToString() == mPageTable.Rows[j]["Type"].ToString() &&
                            newTable.Rows[i]["Price"].ToString() == mPageTable.Rows[j]["Price"].ToString() &&
                            newTable.Rows[i]["Rooms"].ToString() == mPageTable.Rows[j]["Rooms"].ToString() &&
                            newTable.Rows[i]["EntrenceDate"].ToString() == mPageTable.Rows[j]["EntrenceDate"].ToString() &&
                            newTable.Rows[i]["Type"].ToString() == mPageTable.Rows[j]["Type"].ToString() &&
                            newTable.Rows[i]["Address"].ToString() == mPageTable.Rows[j]["Address"].ToString() &&
                            newTable.Rows[i]["countRows"].ToString()!="0" )
                        { 
                            mPageTable.Rows[j][sInternalDownloadStatus] = "Done";
                            break;
                        }
                    }
                }
            } 
            catch(Exception ex)
            {
                errorLog.handleException(ex);
                errorLog.writeToLogFile("at AddTask  " + ex.StackTrace);
            }
         }

        internal void AddRow(DataRow tableRow)
        {
            mPageTable.Rows.Add(tableRow);
        }
        internal int getRandomOpenRowNum(int maxVal)
        {
            Random random = new Random();
            int rowNum = random.Next(0, maxVal);

            while (mPageTable.Rows[rowNum][sInternalStatus].ToString() == sOpenStatus || mPageTable.Rows[rowNum][sInternalDownloadStatus].ToString() != sWaitingInternalDownloadStatus)
                rowNum = random.Next(0, maxVal);
            return rowNum;
        }

        internal void setHomelessClassesTable(DataTable newTable)
        {
            try
            {
                for (int j = 0; j < mPageTable.Rows.Count; j++)
                {
                    mPageTable.Rows[j][sInternalDownloadStatus] = "Waiting";
                    for (int i = 0; i < newTable.Rows.Count; i++)
                    {
                        if (newTable.Rows[i]["Type"].ToString() == mPageTable.Rows[j]["Type"].ToString() &&
                            newTable.Rows[i]["Category"].ToString() == mPageTable.Rows[j]["Category"].ToString() &&
                            newTable.Rows[i]["Area"].ToString() == mPageTable.Rows[j]["Area"].ToString() &&
                            newTable.Rows[i]["CourseName"].ToString() == mPageTable.Rows[j]["CourseName"].ToString() &&
                            newTable.Rows[i]["countRows"].ToString() != "0"
                           )
                        {
                            mPageTable.Rows[j][sInternalDownloadStatus] = "Done";
                            break;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                errorLog.handleException(ex);
                errorLog.writeToLogFile("at AddTask  " + ex.StackTrace);
            }
        }

        internal void setWinwinVehicleTable(DataTable newTable)
        {
            try
            {
                for (int j = 0; j < mPageTable.Rows.Count; j++)
                {
                    mPageTable.Rows[j][sInternalDownloadStatus] = "Waiting";
                    for (int i = 0; i < newTable.Rows.Count; i++)
                    {

                        if (newTable.Rows[i]["Type"].ToString() == mPageTable.Rows[j]["Type"].ToString() &&
                            newTable.Rows[i]["EngineCapacity"].ToString() == mPageTable.Rows[j]["EngineCapacity"].ToString() &&
                            newTable.Rows[i]["Year"].ToString() == mPageTable.Rows[j]["Year"].ToString() &&
                            newTable.Rows[i]["PrevOwnersNum"].ToString() == mPageTable.Rows[j]["PrevOwnersNum"].ToString() &&
                            newTable.Rows[i]["Price"].ToString() == mPageTable.Rows[j]["Price"].ToString() &&
                            newTable.Rows[i]["Area"].ToString() == mPageTable.Rows[j]["Area"].ToString() &&
                            newTable.Rows[i]["RowDate"].ToString() == mPageTable.Rows[j]["RowDate"].ToString() &&
                            newTable.Rows[i]["countRows"].ToString() != "0")
                        {
                            mPageTable.Rows[j][sInternalDownloadStatus] = "Done";
                            break;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                errorLog.handleException(ex);
                errorLog.writeToLogFile("at AddTask  " + ex.StackTrace);
            }
        }


        internal int getRandomOpenRowNum(out string url)
        {
            Random random = new Random();
            int rowNum = random.Next(0, mPageTable.Rows.Count);

            while (mPageTable.Rows[rowNum][sInternalStatus].ToString() == sOpenStatus || mPageTable.Rows[rowNum][sInternalDownloadStatus].ToString() != sWaitingInternalDownloadStatus)
                rowNum = random.Next(0, mPageTable.Rows.Count);
            url = mPageTable.Rows[rowNum]["url"].ToString();
            return rowNum;
        }

        internal void setHomelessVehicleTable(DataTable newTable)
        {
            try
            {
                for (int j = 0; j < mPageTable.Rows.Count; j++)
                {
                    mPageTable.Rows[j][sInternalDownloadStatus] = "Waiting";
                    for (int i = 0; i < newTable.Rows.Count; i++)
                    {
                        if (newTable.Rows[i]["Type"].ToString() == mPageTable.Rows[j]["Type"].ToString() &&
                            newTable.Rows[i]["EngineCapacity"].ToString() == mPageTable.Rows[j]["EngineCapacity"].ToString() &&
                            newTable.Rows[i]["Year"].ToString() == mPageTable.Rows[j]["Year"].ToString() &&
                            newTable.Rows[i]["PrevOwnersNum"].ToString() == mPageTable.Rows[j]["PrevOwnersNum"].ToString() &&
                            newTable.Rows[i]["Price"].ToString() == mPageTable.Rows[j]["Price"].ToString() &&
                            newTable.Rows[i]["Area"].ToString() == mPageTable.Rows[j]["Area"].ToString() &&
                            newTable.Rows[i]["RowDate"].ToString() == mPageTable.Rows[j]["RowDate"].ToString() &&
                            newTable.Rows[i]["countRows"].ToString() != "0"
                           )
                        {
                            mPageTable.Rows[j][sInternalDownloadStatus] = "Done";
                            break;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                errorLog.handleException(ex);
                errorLog.writeToLogFile("at AddTask  " + ex.StackTrace);
            }
        }

        internal void setHomelessTable(DataTable newTable)
        {
            try
            {
                for (int j = 0; j < mPageTable.Rows.Count; j++)
                {
                    mPageTable.Rows[j][sInternalDownloadStatus] = "Waiting";
                    for (int i = 0; i < newTable.Rows.Count; i++)
                    {
                        if (newTable.Rows[i]["Type"].ToString() == mPageTable.Rows[j]["Type"].ToString() &&
                            newTable.Rows[i]["Rooms"].ToString() == mPageTable.Rows[j]["Rooms"].ToString() &&
                            newTable.Rows[i]["Price"].ToString() == mPageTable.Rows[j]["Price"].ToString() &&
                            newTable.Rows[i]["Address"].ToString() == mPageTable.Rows[j]["Address"].ToString() &&
                            newTable.Rows[i]["City"].ToString() == mPageTable.Rows[j]["City"].ToString() &&
                            newTable.Rows[i]["rowDate"].ToString() == mPageTable.Rows[j]["rowDate"].ToString() &&
                            newTable.Rows[i]["EntrenceDate"].ToString() == mPageTable.Rows[j]["EntrenceDate"].ToString() &&
                            newTable.Rows[i]["countRows"].ToString() != "0")
                        {
                            mPageTable.Rows[j][sInternalDownloadStatus] = "Done";
                            break;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                errorLog.handleException(ex);
                errorLog.writeToLogFile("at setHomelessTable  " + ex.StackTrace);
            }
        }


        internal void setWinwinProfessionalTable(DataTable newTable)
        {
            try
            {
                for (int j = 0; j < mPageTable.Rows.Count; j++)
                {
                    mPageTable.Rows[j][sInternalDownloadStatus] = "Waiting";
                    for (int i = 0; i < newTable.Rows.Count; i++)
                    {

                        if (newTable.Rows[i]["BusinessName"].ToString() == mPageTable.Rows[j]["BusinessName"].ToString() &&
                            newTable.Rows[i]["Profession"].ToString() == mPageTable.Rows[j]["Profession"].ToString() &&
                            newTable.Rows[i]["SubCategory"].ToString() == mPageTable.Rows[j]["SubCategory"].ToString() &&
                            newTable.Rows[i]["Area"].ToString() == mPageTable.Rows[j]["Area"].ToString() &&
                            newTable.Rows[i]["RowDate"].ToString() == mPageTable.Rows[j]["RowDate"].ToString() &&
                            newTable.Rows[i]["countRows"].ToString() != "0")
                        {
                            mPageTable.Rows[j][sInternalDownloadStatus] = "Done";
                            break;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                errorLog.handleException(ex);
                errorLog.writeToLogFile("at AddTask  " + ex.StackTrace);
            }
        }

        internal int filter()
        {
            if (mPageTable == null)
                return 0;
            for (int i = 0; i < mPageTable.Rows.Count; i++)
            {

                if (mPageTable.Rows[i][sInternalDownloadStatus].ToString() != sSsuccessInternalDownloadStatus)
                {
                    mPageTable.Rows.RemoveAt(i);
                    i--;
                }
            }
            return mPageTable.Rows.Count;
        }
        internal bool IsTableFinish()
        {
            if (mPageTable.Columns.Contains("downloadStatus"))
                mPageTable.Columns["downloadStatus"].ColumnName = "ds_downloadStatus";

            for (int i = 0; i < mPageTable.Rows.Count; i++)
            {
                if (mPageTable.Rows[i][sInternalDownloadStatus].ToString() != sSsuccessInternalDownloadStatus && mPageTable.Rows[i][sInternalDownloadStatus].ToString() != "Done")
                    return false;

            }
            return true;
        }
        internal bool IsTableFinish(DateTime date)
        {
            if (mPageTable.Columns.Contains("downloadStatus"))
                mPageTable.Columns["downloadStatus"].ColumnName = "ds_downloadStatus";
              
            for (int i = 0; i < mPageTable.Rows.Count; i++)
            {
                DateTime rowDate = DateTime.MinValue;
                DateTime.TryParse(mPageTable.Rows[i]["rowDate"].ToString(), out rowDate);
                if (rowDate == date && mPageTable.Rows[i][sInternalDownloadStatus].ToString() != sSsuccessInternalDownloadStatus && mPageTable.Rows[i][sInternalDownloadStatus].ToString() != "Done")
                      return false;
                   
            }
            return true;
        }
    }
}
