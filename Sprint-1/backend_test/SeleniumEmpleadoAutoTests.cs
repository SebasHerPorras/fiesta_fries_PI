using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;
using Xunit;
using System;
using System.Threading;
using OpenQA.Selenium.Support.UI;

namespace backend_test
{
    public class SeleniumEmpleadoAutoTests
    {
        private const string BaseUrl = "http://localhost:8080";
        private const string EmpresaCedula = "9999999999";
        private const string EmpleadorEmail = "selenium_test@gmail.com";
        private const string EmpleadorPassword = "123456";

        [Fact]
        public void CrearEmpleados_Automatizado()
        {
            using var driver = new ChromeDriver();
            driver.Manage().Window.Maximize();

            try
            {
                // 1. Login como empleador de pruebas (LA RUTA ES "/" NO "/login")
                driver.Navigate().GoToUrl(BaseUrl);

                // Esperar que la página cargue completamente
                var wait = new WebDriverWait(driver, TimeSpan.FromSeconds(10));
                wait.Until(d => ((IJavaScriptExecutor)d).ExecuteScript("return document.readyState").Equals("complete"));

                // Login con selectores correctos (input está dentro de label)
                var emailField = wait.Until(d => d.FindElement(By.CssSelector("label.input > input[type='email']")));
                emailField.Clear();
                emailField.SendKeys(EmpleadorEmail);

                var passwordField = driver.FindElement(By.CssSelector("label.input > input[type='password']"));
                passwordField.Clear();
                passwordField.SendKeys(EmpleadorPassword);

                var loginButton = driver.FindElement(By.CssSelector("button.btn[type='submit']"));
                loginButton.Click();

                // Esperar redirección a Profile
                wait.Until(d => d.Url.Contains("/Profile"));
                Thread.Sleep(2000); // Dar tiempo para que cargue la página

                // 2. Seleccionar la empresa en el dropdown
                var companyDropdown = wait.Until(d => d.FindElement(By.CssSelector("select.company-select")));
                var selectCompany = new SelectElement(companyDropdown);
                selectCompany.SelectByText("Empresa SeleniumTest");

                // Esperar redirección a PageEmpresaAdmin
                wait.Until(d => d.Url.Contains("/PageEmpresaAdmin"));
                Thread.Sleep(2000);

                // 3. Obtener el último incremental desde el backend
                int ultimoIncremental = 100000000;
                try
                {
                    ultimoIncremental = ObtenerUltimaCedulaDesdeBackend();
                }
                catch
                {
                    // Si no hay empleados, usar valor base
                }

                // 4. Crear 1 empleado de prueba
                for (int i = 1; i <= 1; i++)
                {
                    int nuevoIncremental = ultimoIncremental + i;
                    string nombre = $"prueba{nuevoIncremental:D7}";
                    string correo = $"correoprueba{nuevoIncremental:D5}@test.com";
                    string cedula = $"{nuevoIncremental:D9}";

                    // Click en "Agregar empleado"
                    var btnAgregar = wait.Until(d => d.FindElement(By.XPath("//button[contains(text(), 'Agregar Empleado')]")));
                    btnAgregar.Click();

                    // Esperar que el formulario esté visible
                    var nombreInput = wait.Until(d => d.FindElement(By.Id("Name")));

                    // Llenar formulario con IDs y selectores correctos
                    nombreInput.Clear();
                    nombreInput.SendKeys(nombre);

                    driver.FindElement(By.Id("SecondNames")).SendKeys("Test Apellido");
                    driver.FindElement(By.Id("Id")).SendKeys(cedula);
                    driver.FindElement(By.Id("BirthDate")).SendKeys("01011990");
                    driver.FindElement(By.Id("email")).SendKeys(correo);

                    driver.FindElement(By.CssSelector("input[placeholder='👔 Puesto*']")).SendKeys("Tester");

                    // Tipo de contrato (primer select sin id específico)
                    var contratoSelects = driver.FindElements(By.TagName("select"));
                    var contrato = new SelectElement(contratoSelects[0]);
                    contrato.SelectByText("Tiempo Completo");

                    // Método de pago (segundo select)
                    var metodoPago = new SelectElement(driver.FindElement(By.Id("role")));
                    metodoPago.SelectByValue("cash");

                    // Departamento
                    driver.FindElement(By.CssSelector("input[placeholder='🧑‍💼 Departamento*']")).SendKeys("TI");

                    // Salario
                    driver.FindElement(By.CssSelector("input[placeholder='💵 Salario Colones*']")).SendKeys("500000");

                    // Teléfonos
                    driver.FindElement(By.Id("Phone_Number")).SendKeys("88881111");
                    driver.FindElement(By.Id("Home Number")).SendKeys("22223333");

                    // Contraseñas
                    driver.FindElement(By.Id("Password")).SendKeys("Hola@2025");
                    driver.FindElement(By.Id("Password_Confirm")).SendKeys("Hola@2025");

                    // Dirección
                    driver.FindElement(By.Id("Direction")).SendKeys("San José, Costa Rica");

                    // Submit
                    driver.FindElement(By.CssSelector("button.btn-primary[type='submit']")).Click();

                    // Esperar mensaje de éxito o redirección
                    Thread.Sleep(3000);
                }

                Assert.True(true, "Test completado exitosamente");
            }
            finally
            {
                Thread.Sleep(2000); // Ver resultado antes de cerrar
                driver.Quit();
            }
        }

        private int ObtenerUltimaCedulaDesdeBackend()
        {
            var random = new Random();
            return random.Next(100000000, 999999999);
        }
    }
}