using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;
using Xunit;
using System;
using System.Threading;
using OpenQA.Selenium.Support.UI;

namespace backend_test
{
    public class SeleniumRegistrarHorasTests
    {
        private const string BaseUrl = "http://localhost:8080";

        private const string EmpleadoEmail = "ddoodle444@gmail.com";
        private const string EmpleadoPassword = "Ignacio10@";

        [Fact]
        public void RegistrarHoras_Empleado()
        {
            using var driver = new ChromeDriver();
            driver.Manage().Window.Maximize();

            try
            {
                WebDriverWait wait = new(driver, TimeSpan.FromSeconds(10));

                driver.Navigate().GoToUrl(BaseUrl);

                wait.Until(d => ((IJavaScriptExecutor)d)
                    .ExecuteScript("return document.readyState")
                    .Equals("complete"));


                var emailField = wait.Until(d =>
                    d.FindElement(By.CssSelector("label.input > input[type='email']")));
                emailField.SendKeys(EmpleadoEmail);

                var passwordField = driver.FindElement(By.CssSelector("label.input > input[type='password']"));
                passwordField.SendKeys(EmpleadoPassword);

                var loginBtn = driver.FindElement(By.CssSelector("button.btn[type='submit']"));
                loginBtn.Click();

               
                wait.Until(d => d.Url.Contains("/Profile"));
                Thread.Sleep(1500);

                
                var registrarHorasBtn = wait.Until(d =>
                    d.FindElement(By.LinkText("Registrar Horas")));
                registrarHorasBtn.Click();

                
                wait.Until(d => d.Url.Contains("/RegisterHoras"));
                Thread.Sleep(1500);

                
                var inputSemana = wait.Until(d => d.FindElement(By.Id("semana")));
                inputSemana.SendKeys("2025-W10");
                Thread.Sleep(800);

                var daySelect = wait.Until(d => d.FindElement(By.Id("role")));
                var selectDia = new SelectElement(daySelect);
                selectDia.SelectByText("Lunes");
                Thread.Sleep(800);

                wait.Until(d =>
                {
                    var options = d.FindElements(By.CssSelector("#hoursCount option"));
                    return options.Count > 1;   
                });

                var hoursSelect = wait.Until(d => d.FindElement(By.Id("hoursCount")));
                var selectHoras = new SelectElement(hoursSelect);
                selectHoras.SelectByValue("1");
                Thread.Sleep(800);

                var submitBtn = wait.Until(d =>
                    d.FindElement(By.CssSelector("button.btn-primary[type='submit']")));
                submitBtn.Click();

                Thread.Sleep(2000);

                bool successMessageFound = false;

                try
                {
                    var msg = driver.FindElement(By.CssSelector(".message"));
                    successMessageFound = msg.Displayed && msg.Text.Contains("éxito");
                }
                catch { }

                Assert.True(successMessageFound, "No se encontró el mensaje de éxito.");

            }
            finally
            {
                Thread.Sleep(2000);
                driver.Quit();
            }
        }
    }
}
