using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using OpenQA.Selenium;
using OpenQA.Selenium.Remote;
using OpenQA.Selenium.Support.UI;
using OpenQA.Selenium.Chrome;
using OpenQA.Selenium.Firefox;
using System.IO;
using OpenQA.Selenium.IE;
using Microsoft.Win32;
using System.Data;

namespace CMCore.webdriver
{
    public class Driver
    {
        public IWebDriver WebDriver = null;
        public string httpProxy = "";
        public string status = "Busy";
        public DateTime lastUsed = DateTime.MinValue;
        public int grade = 0;

        public Driver(string httpProxy)
        {
            this.httpProxy = httpProxy;
            getdriver();
        }

        private Driver getdriver()
        {
            status = "Busy";
            lastUsed = DateTime.Now;
            WebDriver = CMCore.site.driverUtils.getNewWebDriver(httpProxy);
            return this;
        }

        public void releaseDriver()
        {
            status = "Available";
            lastUsed = DateTime.Now;
            if (WebDriver != null)
                WebDriver.Quit();
            WebDriver = null;
        }

    }
}
