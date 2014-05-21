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
                string commandText = @"UPDATE tasks SET CurrentState=(SELECT Id FROM TaskStatus WHERE (Status = '" + TaskStatus + "')), LastInUse = datetime()	WHERE Id = " + TaskId;

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

        public bool AddTask(int Priority, string TaskName, string TaskDate, string City, string ETaskSource)
        {
            try
            {
                StringBuilder commandText = new StringBuilder(" ");
                commandText.Append(" INSERT INTO Tasks(CurrentState, Priority, TaskType, LastInUse, TaskDate,City)");
                commandText.Append("  SELECT  (SELECT  Id from TaskStatus WHERE Status='Waiting') AS CurrentState," + Priority + " AS Priority, ");
                commandText.Append("  (SELECT  Id from TaskTypes WHERE TaskName='" + TaskName + "'  AND ETaskSource=(SELECT Id FROM ETaskSource WHERE Name='" + ETaskSource + "' ) ) AS TaskTypes, ");
                commandText.Append(" NULL AS LastInUse ,  '" + TaskDate + "',(SELECT Id FROM Cities WHERE CityName='" + City + "')");
                return helper.Load(commandText.ToString().Trim(), "");
            }
            catch (Exception ex)
            {
                errorLog.handleException(ex);
                errorLog.writeToLogFile("at AddTask  " + ex.StackTrace);
                return false;
            }
        }

        public bool AddTask(int Priority, string TaskName, string TaskDate, string City, string ETaskSource, string MinPage)
        {
            try
            {
                StringBuilder commandText = new StringBuilder(" ");
                commandText.Append(" INSERT INTO Tasks(CurrentState, Priority, TaskType, LastInUse, TaskDate,City,MinPage)");
                commandText.Append("  SELECT  (SELECT  Id from TaskStatus WHERE Status='Waiting') AS CurrentState," + Priority + " AS Priority, ");
                commandText.Append("  (SELECT  Id from TaskTypes WHERE TaskName='" + TaskName + "'  AND ETaskSource=(SELECT Id FROM ETaskSource WHERE Name='" + ETaskSource + "' ) ) AS TaskTypes, ");
                commandText.Append(" NULL AS LastInUse ,  '" + TaskDate + "',(SELECT Id FROM Cities WHERE CityName='" + City + "'),'" + MinPage + "'");
                return helper.Load(commandText.ToString().Trim(), "");
            }
            catch (Exception ex)
            {
                errorLog.handleException(ex);
                errorLog.writeToLogFile("at AddTask  " + ex.StackTrace);
                return false;
            }
        }

        public bool AddTaskByTaskId(int Priority, int taskId, string MinPage)
        {
            try
            {
                StringBuilder commandText = new StringBuilder(" ");
                commandText.Append(" INSERT INTO Tasks(CurrentState, Priority, TaskType, LastInUse, TaskDate,City,MinPage)");
                commandText.Append("  SELECT  (SELECT  Id from TaskStatus WHERE Status='Waiting') AS CurrentState, ");
                commandText.Append("  Priority,TaskType,null,TaskDate,City," + MinPage + " From Tasks WHERE Tasks.Id="+taskId.ToString());
                return helper.Load(commandText.ToString().Trim(), "");
            }
            catch (Exception ex)
            {
                errorLog.handleException(ex);
                errorLog.writeToLogFile("at AddTaskByTaskId  " + ex.StackTrace);
                return false;
            }
        }

        public DataTable getTask(  string TaskId )
        {
            try
            {
                StringBuilder commandText = new StringBuilder("  ");
                commandText.Append(" SELECT   Tasks.Id, TaskName,Name as ETaskSource, TaskDate, CityName, TaskType AS CurType, MinPage ");
                commandText.Append(" FROM Tasks INNER JOIN ");
                commandText.Append(" TaskTypes ON Tasks.TaskType = TaskTypes.Id INNER JOIN ");
                commandText.Append(" ETaskSource ON TaskTypes.ETaskSource = ETaskSource.Id LEFT JOIN");
                commandText.Append(" Cities ON Cities.Id = Tasks.City ");
                commandText.Append(" WHERE   Tasks.Id=" + TaskId + ";");
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
