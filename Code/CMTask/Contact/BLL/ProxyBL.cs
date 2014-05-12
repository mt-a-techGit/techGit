using BLL.Class;
using ManageLogFile;
using System;
using System.Collections.Generic;
using System.Data;
using System.Globalization;
using System.Linq;
using System.Text;

namespace BLL.BLL
{
    public class ProxyBL
    {
        private dBHelper helper = null;
        private LogFile errorLog = new LogFile(@"TaskManager\errorHandlerLog\", "TaskManager.log");
        private LogFile infoLog = new LogFile(@"TaskManager\infoHandlerLog\", "TaskManager.log");

        // Determin the ConnectionString
        string connectionString = dBFunctions.ConnectionStringSQLite;

        public ProxyBL(LogFile errorLog, LogFile infoLog)
        {
            helper = new dBHelper(connectionString);
            this.errorLog = errorLog;
            this.infoLog = infoLog;
        }

        public bool SetProxyStatus(string status, string httpProxy)
        {

            try
            {

                // Determin the DataAdapter = CommandText + Connection
                string commandText = @"UPDATE Proxies SET Status = (SELECT Id FROM ProxiesStatus WHERE (StatusName = '" + status + "')) WHERE HttpProxy = '" + httpProxy +"'";
                // Make a new object
                helper = new dBHelper(connectionString);

                // Load the data
                if (helper.Load(commandText, "") == true)
                    return true;
                return false;
            }
            catch (Exception ex)
            {
                errorLog.handleException(ex);
                errorLog.writeToLogFile("at DB UpdateTaskStatus");
                return false;
            }
        }

        private bool UpdateTaskStatus(int TaskId)
        {
            try
            {
                string commandText = @"UPDATE tasks SET CurrentState=(SELECT Id FROM TaskStatus WHERE (Status = @TaskStatus)), LastInUse = datetime()	WHERE Id = " + TaskId;
                helper = new dBHelper(connectionString);
                if (helper.Load(commandText, "") == true)
                    return true;
                return false;
            }
            catch (Exception ex)
            {
                errorLog.handleException(ex);
                errorLog.writeToLogFile("at DB UpdateTaskStatus");
                return false;
            }
        }

        private string DateTimeSQLite(DateTime datetime)
        {
            string dd = DateTime.Now.ToString("yyyy-MM-dd");
            return dd;
        }

        public bool UpdateDTProxies(DataTable proxiesTable)
        {
            try
            {
                StringBuilder commandText = new StringBuilder(" CREATE TEMP TABLE ProxiesTable(HttpProxy,Status ,Country , LastInUse );");
                commandText.Append("INSERT INTO ProxiesTable (HttpProxy, Status,Country ,LastInUse ) ");
                for (int i = 0; i < proxiesTable.Rows.Count; i++)
                {
                    if (i > 0)
                        commandText.Append(" UNION");
                    string ss = @" SELECT '" + proxiesTable.Rows[i]["HttpProxy"].ToString() + "','" + proxiesTable.Rows[i]["Status"].ToString() + "','" + proxiesTable.Rows[i]["Country"].ToString() + "' ,'" + DateTimeSQLite(DateTime.Now) + "'";
                    commandText.Append(ss);
                }
                commandText.Append(";");
                commandText.Append(" INSERT INTO Proxies (HttpProxy,Status,Country,LastInUse )  ");
                commandText.Append(" SELECT HttpProxy,Status ,Country, LastInUse from ProxiesTable  ");
                commandText.Append(" WHERE Status='Success' AND HttpProxy not in (select HttpProxy from Proxies);   ");
                commandText.Append(" DELETE FROM Proxies WHERE HttpProxy IN (");
                commandText.Append(" SELECT  HttpProxy FROM ProxiesTable WHERE ProxiesTable.Status='Failed' )  ;");
                commandText.Append(" UPDATE Proxies SET Status=(SELECT Id FROM ProxiesStatus WHERE (StatusName = 'Success'))	");
                commandText.Append(" WHERE HttpProxy IN (SELECT HttpProxy FROM ProxiesTable WHERE ProxiesTable.Status='Success');    ");
                commandText.Append(" INSERT INTO Tasks(CurrentState, Priority, TaskType, LastInUse, TaskDate,City) ");
                commandText.Append("   SELECT  (SELECT  Id from TaskStatus WHERE Status='Waiting') AS CurrentState,1 AS Priority, ");
                commandText.Append("   (SELECT  Id from TaskTypes WHERE TaskName='TestProxy'  AND ETaskSource=(SELECT Id FROM ETaskSource WHERE Name='ProxyManager' ) ) AS TaskTypes, ");
                commandText.Append("   NULL AS LastInUse , date('now'),NULL");
                commandText.Append(" WHERE (SELECT COUNT (Id) FROM proxies WHERE Status=(SELECT  Id FROM ProxiesStatus WHERE StatusName='Unassigned')) > 0 ");
                if (helper.Load(commandText.ToString(), "") == true)
                    return true;
                return false;
        
            }
            catch (Exception ex)
            {
                errorLog.handleException(ex);
                errorLog.writeToLogFile("at DB UpdateDTProxies");
                return false;
            }
        }

        public bool AddTaskGetProxiesListParams(int taskId, string date)
        {
            try
            {
                StringBuilder commandText = new StringBuilder("	INSERT INTO TaskGetProxiesListParams(TaskId, PageDate ) VALUES (@TaskId, @PageDate )");

                if (helper.Load(commandText.ToString(), "") == true)
                    return true;
                return false;
            }
            catch (Exception ex)
            {
                errorLog.handleException(ex);
                errorLog.writeToLogFile("at DB AddTaskGetProxiesListParams");
        
                return false;
            }
        }

        public bool AddProxies(DataTable proxies)
        {
            try
            {
                StringBuilder commandText = new StringBuilder("");
                commandText.Append(" CREATE TEMP TABLE proxiesTable(HttpProxy,Country,Status);");
                commandText.Append(" INSERT INTO proxiesTable (HttpProxy,Country,Status) ");
                for (int i = 0; i < proxies.Rows.Count; i++)
                {
                    if (i > 0)
                        commandText.Append(" UNION");
                    commandText.Append(@" SELECT '" + proxies.Rows[i]["HttpProxy"].ToString() + "','" + proxies.Rows[i]["Country"].ToString() + "','" + proxies.Rows[i]["Status"].ToString() + "'");

                }
                commandText.Append("  ;                                                                                                          ");

                commandText.Append("  INSERT INTO Proxies(HttpProxy, Status, Country)                                                            ");
                commandText.Append("         SELECT   HttpProxy,(SELECT Id FROM ProxiesStatus WHERE (StatusName = 'Unassigned')),Country         ");
                commandText.Append("         FROM proxiesTable                                                                                   ");
                commandText.Append("         WHERE proxiesTable.HttpProxy NOT IN ( SELECT HttpProxy FROM Proxies)     ;                           ");
                commandText.Append(" UPDATE Proxies SET Status= (SELECT Id FROM ProxiesStatus WHERE (StatusName = 'Unassigned') )                ");
                commandText.Append("         WHERE Proxies.HttpProxy IN ( SELECT HttpProxy FROM proxiesTable)                     ;               ");
                commandText.Append(" DROP TABLE proxiesTable;                  ;               ");
                if (helper.Load(commandText.ToString(), "") == true)
                    return true;
                return false;
            }
            catch (Exception ex)
            {
                errorLog.handleException(ex);
                errorLog.writeToLogFile("AddProxies");
                return false;
            }
        }

        public bool AddSiteProxiesConnection(int ProxyId, string SiteName, DateTime ConnectionTime, string status, string httpProxy)
        {
            try
            {
                StringBuilder commandText = new StringBuilder("	  INSERT INTO SiteProxiesConnection(ProxyId, ETaskSource,ConnectionTime, Status )");
                if (ProxyId > 0)
                    commandText.Append(" VALUES (" + ProxyId + ", (SELECT Id FROM ETaskSource WHERE (Name = '" + SiteName + "')),  datetime()	, ");
                else
                    commandText.Append(" VALUES ( (SELECT Id FROM Proxies WHERE HttpProxy='" + httpProxy + "'), (SELECT Id FROM ETaskSource WHERE (Name = '" + SiteName + "')),  datetime()	, ");
                commandText.Append(" (SELECT Id FROM SiteProxiesConnectionStatus WHERE Status = '" + status + "' ) ) ");
                if (helper.Load(commandText.ToString(), "") == true)
                    return true;
                return false;
            }
            catch (Exception ex)
            {
                errorLog.handleException(ex);
                errorLog.writeToLogFile("AddSiteProxiesConnection");
                return false;
            }
        }

        public string GetNextLiveProxy(string ETaskSource)
        {
            try
            {
                StringBuilder commandText = new StringBuilder("");

                commandText.Append("   CREATE TEMP TABLE _Variables(HttpProxy TEXT  ); ");
                commandText.Append("   INSERT INTO _Variables (HttpProxy)  ");
              
                commandText.Append("  SELECT      Proxies.HttpProxy ");
                commandText.Append("  FROM        Proxies INNER JOIN ");
                commandText.Append(" ProxiesStatus ON Proxies.Status = ProxiesStatus.Id  LEFT JOIN ( ");
                commandText.Append(" SELECT ProxyId,count (ProxyId) AS CountProxy FROM ( ");
                commandText.Append(" SELECT ProxyId, SiteProxiesConnection.ConnectionTime, SiteProxiesConnectionStatus.Status,Name ");
                commandText.Append(" FROM SiteProxiesConnection INNER JOIN ");
                commandText.Append(" SiteProxiesConnectionStatus ON SiteProxiesConnection.Status = SiteProxiesConnectionStatus.Id INNER JOIN ");
                commandText.Append(" ETaskSource ON SiteProxiesConnection.ETaskSource = ETaskSource.Id ");
                if (ETaskSource != "")
                    commandText.Append(" WHERE ( ETaskSource.Name = '" + ETaskSource + "') ");
                commandText.Append(" GROUP BY ProxyId ");
                commandText.Append(" ORDER BY SiteProxiesConnection.ConnectionTime DESC LIMIT 3 ) AS SiteProxies ");
                commandText.Append(" WHERE SiteProxies.Status='Failed' ");
                commandText.Append(" ) AS CountProxy ON CountProxy.ProxyId=Proxies.[id] LEFT JOIN ( ");
                commandText.Append(" SELECT    ProxyId,count(SiteProxiesConnection.ProxyId) AS BlockProxies ");
                commandText.Append(" FROM SiteProxiesConnection INNER JOIN ");
                commandText.Append(" ETaskSource ON SiteProxiesConnection.ETaskSource = ETaskSource.Id ");
                commandText.Append(" WHERE    (ETaskSource.Name = '" + ETaskSource + "') AND (Status =  'Block') ");
                commandText.Append(" GROUP BY ProxyId ) AS BlockProxies ON BlockProxies.ProxyId=Proxies.id ");
                commandText.Append("  WHERE (ProxiesStatus.StatusName =  'Success') AND (CountProxy is null OR CountProxy<3) AND (BlockProxies is null OR BlockProxies=0)  ");
                commandText.Append("  ORDER BY RANDOM() LIMIT 1;");
                commandText.Append("  SELECT * FROM _Variables;");
                commandText.Append("  UPDATE Proxies SET  LastInUse=(SELECT datetime('now')), Status=(SELECT id FROM ProxiesStatus WHERE StatusName='InUse') WHERE HttpProxy IN(SELECT HttpProxy FROM _Variables );");
                commandText.Append("  DROP TABLE _Variables;");
                if (helper.Load(commandText.ToString(), "") == true)
                    return helper.DataSet.Tables[0].Rows[0][0].ToString();
                return string.Empty;
            }
            catch (Exception ex)
            {
                errorLog.handleException(ex);
                errorLog.writeToLogFile("GetNextLiveProxy");
                return string.Empty;
            }
        }

        public DataTable GetProxies(string status, string country, int top)
        {
            try
            {
                StringBuilder commandText = new StringBuilder(" SELECT Proxies.*, ProxiesStatus.StatusName");
                commandText.Append(" FROM Proxies INNER JOIN");
                commandText.Append(" ProxiesStatus ON Proxies.Status = ProxiesStatus.Id");
                commandText.Append(" WHERE (ProxiesStatus.StatusName = '" + status + "') ");
                if (country != "")
                    commandText.Append("  AND ( Proxies.Country = '" + country + "' )");
                commandText.Append(" LIMIT " + top);

                if (helper.Load(commandText.ToString(), "") == true)
                    return helper.DataSet.Tables[0];

                return null;
            }
            catch (Exception ex)
            {
                errorLog.handleException(ex);
                errorLog.writeToLogFile("at GetProxies  " + ex.StackTrace);
                return null;
            }
        }



    }
}
