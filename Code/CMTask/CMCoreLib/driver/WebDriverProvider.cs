using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using CMCore.webtools;
using CMCore.site;

namespace CMCore.webdriver
{
    public class WebDriverProvider
    {
        private ProxyProvider mProxyProvider;

        private static object syncRoot = new Object();
        private static volatile WebDriverProvider instance;

        public static WebDriverProvider Instance
        {
            get
            {
                if (instance == null)
                {
                    lock (syncRoot)
                    {
                        if (instance == null)
                            instance = new WebDriverProvider();
                    }
                }
                return instance;
            }
        }

        private WebDriverProvider()
        {
            mProxyProvider = new ProxyProvider();
            mProxyProvider.ResetLiveProxyList();
        }

        public void releaseDriver(string httpProxy)
        {
             mProxyProvider.releaseDProxy(httpProxy);
        }
 
        public Driver getAvaiableDriver(string ETaskSource)
        {
            string httpProxy = mProxyProvider.GetNextLiveProxy(ETaskSource);
            if (httpProxy == null || httpProxy == "")
                return null;
            Driver newDriver = new Driver(httpProxy);
            return newDriver;
        }
    }
}
