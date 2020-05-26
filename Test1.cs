using NUnit.Framework;
using Microsoft.Extensions.Configuration;
using OpenQA.Selenium;
using OpenQA.Selenium.Support.UI;
using WaitHelpers = SeleniumExtras.WaitHelpers;
using OpenQA.Selenium.Chrome;
using System;
using System.IO;


namespace FotoWare
{
    public class Test1
    {
        IWebDriver driver;

        IConfigurationRoot PageProperties, TestConfig;

        // Screenshot Path
        string path = Path.Combine(
                   Environment.GetFolderPath(
                       Environment.SpecialFolder.Desktop
                       ),
                   "Screenshot",
                   DateTime.Now.ToString("dd-MM-yyyy HH-mm-ss")
               );


        [SetUp]
        public void Setup()
        {
            ChromeOptions options = new ChromeOptions();
            options.AddArgument("headless");
            driver = new ChromeDriver(options);
            driver.Manage().Window.Maximize();

            // PageProperties.json file contains Page Element Selectors
            PageProperties = new ConfigurationBuilder()
                .AddJsonFile("PageProperties.json")
                .Build();

            // TestConfig.json contains Test Configuration Data
            TestConfig = new ConfigurationBuilder()
                .AddJsonFile("TestConfig.json")
                .Build();

            // Create Directory for Screenshot Path
            if (!Directory.Exists(path))
            {
                Directory.CreateDirectory(path);
            }
        }


        [TearDown]
        public void TearDown()
        {
            driver.Close();
        }


        [Test]
        public void SeleniumTest()
        {
            WebDriverWait wait = new WebDriverWait(
                driver, TimeSpan.FromSeconds(120)
                );

            // Go to login url
            driver.Url = TestConfig["login_url"];
            TestContext.Progress.WriteLine("Starting TEST 1... ");
            TestContext.Progress.WriteLine("Navigating to: " + driver.Url);

            wait.Until(
                WaitHelpers.
                ExpectedConditions.
                ElementToBeClickable(
                    By.CssSelector(
                        PageProperties["login_form_css"]
                        )
                    )
                );

            TestContext.Progress.WriteLine("Login Form Loaded");

            // Enter Username
            driver.PageSource.Contains(PageProperties["username_css"]);
            IWebElement username = driver.FindElement(
                By.CssSelector(
                    PageProperties["username_css"]
                    )
                );
            username.SendKeys(TestConfig["username"]);
            TestContext.Progress.WriteLine("Entered Username.");

            // Enter Password
            driver.PageSource.Contains(PageProperties["password_css"]);
            IWebElement password = driver.FindElement(
                By.CssSelector(
                    PageProperties["password_css"]
                    )
                );
            password.SendKeys(TestConfig["password"]);
            TestContext.Progress.WriteLine("Entered Password.");

            // Click Login
            driver.PageSource.Contains(PageProperties["login_button_css"]);
            IWebElement loginButton = driver.FindElement(
                By.CssSelector(
                    PageProperties["login_button_css"]
                    )
                );
            loginButton.Click();
            TestContext.Progress.WriteLine("Clicked Login Button.");

            // Click Archive
            IWebElement archive = wait.Until(
                WaitHelpers
                .ExpectedConditions
                .ElementToBeClickable(
                    By.CssSelector(
                        PageProperties["archive_css"]
                        )
                    )
                );
            archive.Click();
            TestContext.Progress.WriteLine("Clicked on Archive.");

            // Enter Search Keyword
            IWebElement searchInput = wait.Until(
                WaitHelpers
                .ExpectedConditions
                .ElementToBeClickable(
                    By.Id(
                        PageProperties["search_input_id"]
                        )
                    )
                );
            searchInput.SendKeys(TestConfig["keyword"]);
            searchInput.SendKeys(Keys.Enter);
            TestContext.Progress.WriteLine("Entered Search Keyword.");

            wait.Until(WaitHelpers.ExpectedConditions
                .UrlContains(TestConfig["keyword"]));

            //Save screenshot
            Screenshot image = ((ITakesScreenshot)driver).GetScreenshot();
            image.SaveAsFile(
                path+"/test.png",
                OpenQA.Selenium.ScreenshotImageFormat.Png
                );
            TestContext.Progress.WriteLine("Saved Screenshot.");
        }
    }
}