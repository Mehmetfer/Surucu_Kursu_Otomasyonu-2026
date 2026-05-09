using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;
using System;
using System.Threading;

namespace Kolera_Mtsk.Mebbis
{
    public class MebbisSeleniumEngine
    {
        private ChromeDriver _driver;

        public void Start()
        {
            var options = new ChromeOptions();
            options.AddArgument("--start-maximized");
            options.AddArgument("--disable-notifications");

            _driver = new ChromeDriver(options);
        }

        public void Login(string username, string password)
        {
            _driver.Navigate().GoToUrl("https://mebbis.meb.gov.tr/default.aspx?NoSession");

            Thread.Sleep(2000);

            var inputs = _driver.FindElements(By.TagName("input"));

            foreach (var el in inputs)
            {
                var type = el.GetAttribute("type");

                if (type == "text")
                    el.SendKeys(username);

                if (type == "password")
                    el.SendKeys(password);
            }

            foreach (var el in inputs)
            {
                if (el.GetAttribute("type") == "submit" ||
                    el.GetAttribute("type") == "button")
                {
                    el.Click();
                    break;
                }
            }

            Thread.Sleep(3000);
        }

        public void OpenMtskPage()
        {
            _driver.Navigate().GoToUrl("https://mebbisyd.meb.gov.tr/SKT/skt00001.aspx");
            Thread.Sleep(2000);
        }

        public void ClickKurumAdayKayit()
        {
            var element = _driver.FindElement(By.CssSelector("td[title='Kurum Aday Kayıt İşlemleri']"));
            element.Click();
        }

        public void Stop()
        {
            _driver.Quit();
        }
    }
}