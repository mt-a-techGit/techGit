using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using BLL.ProxyTableAdapters;
using System.Data;
using ManageLogFile;

namespace BLL
{
    public class ProxyBL
    {
        private LogFile errorLog;
        private LogFile infoLog;

        public ProxyBL(LogFile errorLog, LogFile infoLog)
        {
            this.errorLog = errorLog;
            this.infoLog = infoLog;
        }

        public int AddSiteProxiesConnection(int ProxyId, string SiteName, DateTime ConnectionTime, string status, string httpProxy)
        {
            try
            {
                QueriesTableAdapter adapter = new QueriesTableAdapter();
                object tt = adapter.AddSiteProxiesConnection(ProxyId, SiteName, status, httpProxy);
                return int.Parse(tt.ToString());
            }
            catch (Exception ex)
            {
                errorLog.handleException(ex);
                errorLog.writeToLogFile("AddProxies");
                return 0;
            }
        }
 
        public void UpdateDTProxies(DataTable Proxies)
        {
            try
            {
                QueriesTableAdapter adapter = new QueriesTableAdapter();
                adapter.UpdateDTProxies(Proxies);
            }
            catch (Exception ex)
            {
                errorLog.handleException(ex);
                errorLog.writeToLogFile("setProxYStatus");
            }
        }

        public int SetProxyStatus(string status, string httpProxy)
        {
            try
            {
                QueriesTableAdapter adapter = new QueriesTableAdapter();
                object tt = adapter.SetProxyStatus(httpProxy, status);
                return int.Parse(tt.ToString());
            }
            catch (Exception ex)
            {
                errorLog.handleException(ex);
                errorLog.writeToLogFile("setProxYStatus");
                return 0;
            }
        }

        public int AddProxies(DataTable proxies)
        {
            try
            {
                QueriesTableAdapter adapter = new QueriesTableAdapter();
                return (int)adapter.AddProxies(proxies);
            }
            catch (Exception ex)
            {
                errorLog.handleException(ex);
                errorLog.writeToLogFile("AddProxies");
                return 0;
            }
        }

        public string GetNextLiveProxy(string ETaskSource)
        {
            try
            {
                QueriesTableAdapter adapter = new QueriesTableAdapter();
                return adapter.GetNextLiveProxy(ETaskSource).ToString();
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
                GetProxiesTableAdapter adapter = new GetProxiesTableAdapter();
                return adapter.GetData(status, country, top);
            }
            catch (Exception ex)
            {
                errorLog.handleException(ex);
                errorLog.writeToLogFile("GetProxies");
                return null;
            }
        }

    }
}
