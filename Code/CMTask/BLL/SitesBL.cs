using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data;
using ManageLogFile;
using BLL.SitesTableAdapters;

namespace BLL
{
    public class SitesBL
    {
        QueriesTableAdapter queriesTableAdapter = new QueriesTableAdapter();
        private LogFile errorLog;
        private LogFile infoLog;

        public SitesBL(LogFile errorLog, LogFile infoLog)
        { 
            this.errorLog= errorLog;
            this.infoLog = infoLog;
        }

        public DataTable UpdateYad2TableRowsStatus(DataTable pageTable)
        {
            try
            {
                if (pageTable.Columns.Contains("ds_status"))
                    pageTable.Columns.Remove("ds_status");
                UpdateYad2TableRowsStatusTableAdapter TableAdapter = new UpdateYad2TableRowsStatusTableAdapter();
                return TableAdapter.GetData(pageTable);
            }
            catch (Exception ex)
            {
                errorLog.handleException(ex);
                errorLog.writeToLogFile("at DB UpdateYad2TableRowsStatus");
                return null;
            }
        }
        
        public DataTable UpdateWinwinTableRowsStatus(DataTable pageTable)
        {
            try
            {
                if (pageTable.Columns.Contains("ds_status"))
                    pageTable.Columns.Remove("ds_status");
                UpdateWinwinTableRowsStatusTableAdapter TableAdapter = new UpdateWinwinTableRowsStatusTableAdapter();
                return TableAdapter.GetData(pageTable);
            }
            catch (Exception ex)
            {
                errorLog.handleException(ex);
                errorLog.writeToLogFile("at DB UpdateWinwinTableRowsStatus");
                return null;
            }
        }

        public void AddYad2PageTable(DataTable pageTable)
        {
            try
            {
                if (pageTable.Columns.Contains("ds_status"))
                    pageTable.Columns.Remove("ds_status");
                 queriesTableAdapter.AddYad2PageTable(pageTable);
            }
            catch (Exception ex)
            {
                errorLog.handleException(ex);
                errorLog.writeToLogFile("at DB AddYad2PageTable");
                return ;
            }
        }
 
        public void AddWinwinPageTable(DataTable pageTable)
        {
            try
            {
                if (pageTable.Columns.Contains("ds_status"))
                    pageTable.Columns.Remove("ds_status");
                queriesTableAdapter.AddWinwinPageTable(pageTable);
            }
            catch (Exception ex)
            {
                errorLog.handleException(ex);
                errorLog.writeToLogFile("at DB addPageTable");
                return ;
            }
        }
    }
}
