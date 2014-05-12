using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data;
using OpenQA.Selenium;
using CMCore.site;
using System.Collections.ObjectModel;
using BLL;
using ManageLogFile;
using BLL.BLL;

namespace CMCore.webtools
{
    public class ProxyProvider
    {
        private const string sOnline = "online";
        private const string sOffLine = "offline";
        private const string sHttpProxy = "httpProxy";
        private const string sLastConnectionStatus = "lastConnectionStatus";
        private const string sStatus = "status";
        private const string sBusy = "Busy";
        private LogFile errorLog;
        private LogFile infoLog;
        private ProxyBL blProxie;

        public ProxyProvider()
        {
            errorLog = new LogFile(@"ProxyProvider\errorHandlerLog\", "ProxyProvider.log");
            infoLog = new LogFile(@"ProxyProvider\infoHandlerLog\", "ProxyProvider.log");
            blProxie = new ProxyBL(errorLog, infoLog);
        }

        public void ResetLiveProxyList()
        {
            //validate proxy list
            //mCurrentIndex = 0;
        }

        public void releaseDProxy(string HttpProxy)
        {
            blProxie.SetProxyStatus("Success", HttpProxy);
        }

        public string GetNextLiveProxy(string ETaskSource)
        {
            lock (this)
            {
                ProxyBL blProxie = new ProxyBL(errorLog, infoLog);
                return blProxie.GetNextLiveProxy(ETaskSource);
            }
        }

    }
}
