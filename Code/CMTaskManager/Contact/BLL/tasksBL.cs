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
    public class TaskBL
    {
        private dBHelper helper = null;

        // Determin the ConnectionString
        string connectionString = dBFunctions.ConnectionStringSQLite;
        private LogFile errorLog;
        private LogFile infoLog;

        public TaskBL(LogFile errorLog, LogFile infoLog)
        {
            helper = new dBHelper(connectionString);
            this.errorLog = errorLog;
            this.infoLog = infoLog;
        }

        public bool updateTaskMinPage(int taskId, string MinPage)
        {
            try
            {
                StringBuilder commandText = new StringBuilder(" ");
                commandText.Append(" UPDATE Tasks SET MinPage='" + MinPage + "' WHERE Id=" + taskId.ToString());
                return (helper.Load(commandText.ToString(), ""));

            }
            catch (Exception ex)
            {
                errorLog.handleException(ex);
                errorLog.writeToLogFile("at updateTaskMinPage  " + ex.StackTrace);
                return false;
            }
        }

        public bool UpdateTaskStatus(int TaskId, string TaskStatus)
        {
            try
            {

                // Determin the DataAdapter = CommandText + Connection
                string commandText = @"UPDATE tasks SET CurrentState=(SELECT Id FROM TaskStatus WHERE (Status = '" + TaskStatus + "'))	WHERE Id = " + TaskId;

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

        private string DateTimeSQLite(DateTime datetime)
        {
            string dd = DateTime.Now.ToString("yyyy-MM-dd");
            return dd;
        }

       
        public DataTable getTasksStatus( string TasksIds)
        {
            try
            {
                StringBuilder commandText = new StringBuilder("");
                commandText.Append("   SELECT   Tasks.Id, Tasks.CurrentState FROM Tasks  WHERE  Tasks.Id in (" + TasksIds + ")");
                 
                if (helper.Load(commandText.ToString(), "") == true)
                    return helper.DataSet.Tables[0];
                return null;
            }
            catch (Exception ex)
            {
                errorLog.handleException(ex);
                errorLog.writeToLogFile("at DB GetTasks");
                return null;
            }
        }

        public DataTable getTasks(int maxTasks)
        {
            try
            {
                StringBuilder commandText = new StringBuilder("");
                commandText.Append(" CREATE TEMP TABLE TasksTable(Id, TaskName, ETaskSource, TaskDate, City, TaskType,MinPage) ; ");
                commandText.Append(" INSERT INTO TasksTable (Id, TaskName, ETaskSource, TaskDate, City, TaskType ,MinPage) ");
                commandText.Append(" SELECT   Tasks.Id, TaskName,Name as ETaskSource, TaskDate, CityName, TaskType AS CurType, MinPage ");
                commandText.Append(" FROM Tasks INNER JOIN ");
                commandText.Append(" TaskTypes ON Tasks.TaskType = TaskTypes.Id INNER JOIN ");
                commandText.Append(" ETaskSource ON TaskTypes.ETaskSource = ETaskSource.Id LEFT JOIN");
                commandText.Append(" Cities ON Cities.Id = Tasks.City ");

                commandText.Append(" WHERE   CurrentState = (SELECT Id FROM TaskStatus WHERE (Status = 'Waiting') ) ");
                
                commandText.Append(" AND (SELECT COUNT(Id) FROM Tasks WHERE CurType = TaskType AND   CurrentState = (SELECT Id FROM TaskStatus WHERE (Status = 'Process'))  ) <  "+ maxTasks);
                commandText.Append(" ORDER BY Priority,RANDOM()  LIMIT 1; ");
                commandText.Append(" UPDATE tasks SET CurrentState=(SELECT Id FROM TaskStatus WHERE (Status = 'Process')) ");
                commandText.Append(" WHERE  id in(SELECT id FROM TasksTable); ");
                commandText.Append(" UPDATE tasks SET CurrentState=(SELECT Id FROM TaskStatus WHERE (Status = 'Waiting')) ");
                commandText.Append(" WHERE  (  CurrentState=(SELECT Id FROM TaskStatus WHERE (Status = 'Failed')) ); ");
                commandText.Append(" SELECT * FROM TasksTable; ");
                commandText.Append(" DROP TABLE TasksTable; ");
                commandText.Append(" INSERT INTO Tasks(CurrentState, Priority, TaskType, LastInUse, TaskDate,City)  ");
                commandText.Append(" SELECT (SELECT Id FROM TaskStatus WHERE (Status = 'Waiting') ),1,(SELECT  Id from TaskTypes WHERE TaskName='GetPageData'  AND ETaskSource=(SELECT Id FROM ETaskSource WHERE Name='Winwin' )) AS CurType,");
                commandText.Append(" NULL AS LastInUse ,  date('now','-1 day'),Id From Cities  ");
                commandText.Append(" WHERE 0=(SELECT COUNT (Id) FROM Tasks WHERE TaskDate=(SELECT date('now','-1 day') ) AND TaskType = CurType) ; ");

                if (helper.Load(commandText.ToString(), "") == true)
                    return helper.DataSet.Tables[0];
                return null;
            }
            catch (Exception ex)
            {
                errorLog.handleException(ex);
                errorLog.writeToLogFile("at DB GetTasks");
                return null;
            }
        }

    }
}
