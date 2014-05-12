using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ManageLogFile;
using System.Data;
using BLL.Y2TableAdapters;

namespace BLL
{
    public class Y2BL
    {
          
        private LogFile errorLog;
        private LogFile infoLog;

        public Y2BL(LogFile errorLog, LogFile infoLog) { 
             
            this.errorLog= errorLog;
            this.infoLog = infoLog;
        }
 
        public DataTable getTaskTable(int taskId)
        {
            try
            {
                taskTableTableAdapter adapter = new taskTableTableAdapter();
                return adapter.GetData(taskId);
            }
            catch (Exception ex)
            {
                errorLog.handleException(ex);
                errorLog.writeToLogFile("at DB getYad2Basic");
                return null;
            }
        }
       
    }
}
