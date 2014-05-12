using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using CMCore.webdriver;
using OpenQA.Selenium;
using OpenQA.Selenium.Support.UI;
using OpenQA.Selenium.Interactions;

namespace CMCore.site
{

   

    class baseSiteCommon
    {
        public static bool NevigateToPage(IWebDriver myDriver, string url)
        {
            try
            {
                myDriver.Navigate().GoToUrl(url);
                IWait<IWebDriver> wait = new OpenQA.Selenium.Support.UI.WebDriverWait(myDriver, TimeSpan.FromSeconds(180.00));
                wait.Until(driver1 => ((IJavaScriptExecutor)myDriver).ExecuteScript("return document.readyState").Equals("complete"));
                return true;
            }
            catch (Exception ex)
            {
                throw ex;
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
