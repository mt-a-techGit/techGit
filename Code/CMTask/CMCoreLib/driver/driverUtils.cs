using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using CMCore.webdriver;
using OpenQA.Selenium;
using OpenQA.Selenium.Support.UI;
using OpenQA.Selenium.Interactions;
using OpenQA.Selenium.Remote;
using OpenQA.Selenium.Firefox;
using System.IO;
using System.Drawing.Imaging;
using System.Collections.ObjectModel;
using OpenQA.Selenium.Chrome;

namespace CMCore.site
{

    public static class driverUtils
    {
        public static bool NevigateToPage(IWebDriver myDriver, string url)
        {
            try
            {
                myDriver.Navigate().GoToUrl(url);
                //IWait<IWebDriver> wait = new OpenQA.Selenium.Support.UI.WebDriverWait(myDriver, TimeSpan.FromSeconds(80.00));
                //wait.Until(driver1 => ((IJavaScriptExecutor)myDriver).ExecuteScript("return document.readyState").Equals("complete"));
                Sleep(10000, 15000);
               // CloseOtherWindows(myDriver);
                return true;
            }
            catch (Exception ex)
            {
                return false;
            }

        }

        public static void screeshot(IWebDriver webDriver, string ImgPath)
        {
            try
            {
                Screenshot ss = ((ITakesScreenshot)webDriver).GetScreenshot();
                string screenshot = ss.AsBase64EncodedString;
                byte[] screenshotAsByteArray = ss.AsByteArray;
                SaveFile(screenshotAsByteArray, ImgPath);
            }
            catch (Exception ex) { }
        }

        public static bool CloseOtherWindows(IWebDriver webDriver)
        {

            try
            {
                ReadOnlyCollection<string> WindowHandles = webDriver.WindowHandles;
                string CurrentWindowHandle = webDriver.CurrentWindowHandle;
                foreach (string Window in WindowHandles)
                {

                    if (webDriver.CurrentWindowHandle != Window)
                    {
                        webDriver.SwitchTo().Window(Window);
                        webDriver.Close();
                    }

                }
                webDriver.SwitchTo().Window(CurrentWindowHandle);
                return true;
            }
            catch (Exception ex) { return false; }
            
        }

        private static void SaveFile(Byte[] fileBytes, string fileName)
        {
            FileStream fileStream = new FileStream(fileName, FileMode.Create, FileAccess.ReadWrite);
            fileStream.Write(fileBytes, 0, fileBytes.Length);
            fileStream.Close();
        }

        public static IWebDriver getNewWebDriver(string httpProxy)
        {
            try
            {
                Proxy proxy = new Proxy();
                proxy.HttpProxy = httpProxy;
                proxy.IsAutoDetect = false;
                FirefoxProfileManager profileManager = new FirefoxProfileManager();
                FirefoxProfile profile = profileManager.GetProfile("selenium");
                profile.SetProxyPreferences(proxy);
                IWebDriver driver = new FirefoxDriver(profile);
                return driver;
            }
            catch (Exception ex)
            {
                if (ex.Message.Contains("Unable to bind to locking port"))
                {
                    System.Diagnostics.Process[] pp = System.Diagnostics.Process.GetProcessesByName("firefox");
                    for (int i = 0; i < pp.Length; i++)
                    {
                        pp[i].Kill();

                    }
                }
                return null;
            }
        }

        public static IWebDriver getNewWebDriver()
        {
            IWebDriver webDriver = new FirefoxDriver();
            return webDriver;
        }

        public static string ExecuteScript(IWebDriver driver, string scriptValue)
        {
            try
            {
                var t = ((IJavaScriptExecutor)driver).ExecuteScript(scriptValue);
                return t.ToString();
            }
            catch (Exception ex)
            {
                return "";
            }
        }

        public static void refreshPage(IWebDriver driver)
        {
            Actions actions = new Actions(driver);
            actions.KeyDown(Keys.Control).SendKeys(Keys.F5).Perform();
        }

        public static void Sleep(int min, int max)
        {
            Random random = new Random();
            int randomNumber = random.Next(min, max);
            System.Threading.Thread.Sleep(randomNumber);
        }

        public static IWebElement GetParent(IWebElement e)
        {
            return e.FindElement(By.XPath(".."));
        }

    }
}
