using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data;
using BLL.TasksTableAdapters;
using ManageLogFile;

namespace BLL
{
    public class TasksBL
    {

        private LogFile errorLog;
        private LogFile infoLog;

        public TasksBL(LogFile errorLog, LogFile infoLog)
        {
            this.errorLog = errorLog;
            this.infoLog = infoLog;
        }

        public void AddTaskGetPageData(int Priority, string taskSource, string date)
        {

            try
            {
                QueriesTableAdapter adapter = new QueriesTableAdapter();
                adapter.AddTaskGetPageData(Priority, taskSource, date);
            }
            catch (Exception ex)
            {
                errorLog.handleException(ex);
                errorLog.writeToLogFile("at DB getTasks");
                return;
            }
        }

        public int AddTask(int Priority, int taskType)
        {
            try
            {
                QueriesTableAdapter adapter = new QueriesTableAdapter();
                 object tt = adapter.AddTask(Priority,1, taskType);
                return int.Parse(tt.ToString());

            }
            catch (Exception ex)
            {
                errorLog.handleException(ex);
                errorLog.writeToLogFile("at DB getTasks");
                return 0;
            }
        }

        public void UpdateTaskGetPageDataParams(int taskId, int PageNo, int LinesNo)
        {
            try
            {
                QueriesTableAdapter adapter = new QueriesTableAdapter();
                adapter.UpdateTaskGetPageDataParams(taskId, PageNo, LinesNo);
            }
            catch (Exception ex)
            {
                errorLog.handleException(ex);
                errorLog.writeToLogFile("at DB UpdateTaskStatus");
                return;
            }
        }
 
        public void UpdateTaskStatus(string status, int taskId)
        {
            try
            {
                QueriesTableAdapter adapter = new QueriesTableAdapter();
                adapter.UpdateTaskStatus(taskId, status);
            }
            catch (Exception ex)
            {
                errorLog.handleException(ex);
                errorLog.writeToLogFile("at DB UpdateTaskStatus");
                return;
            }
        }
 
        public DataTable getTasks(int maxTasks, string TaskName, string ETaskSource)
        {
            try
            {
                GetTasksTableAdapter adapter = new GetTasksTableAdapter();
                return adapter.GetData(maxTasks, TaskName, ETaskSource);
            }
            catch (Exception ex)
            {
                errorLog.handleException(ex);
                errorLog.writeToLogFile("at DB getTasks");
                return null;
            }
        }

        public DataTable GetTaskGetPageDataParams(int taskId)
        {
            try
            {
                GetTaskGetPageDataParamsTableAdapter adapter = new GetTaskGetPageDataParamsTableAdapter();
                return adapter.GetData(taskId);
            }
            catch (Exception ex)
            {
                errorLog.handleException(ex);
                errorLog.writeToLogFile("at DB GetTaskYad2GetPageDataParams");
                return null;
            }
        }

    }
}
