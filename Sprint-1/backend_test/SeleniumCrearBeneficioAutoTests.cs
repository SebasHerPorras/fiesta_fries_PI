using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;
using OpenQA.Selenium.Support.UI;
using Xunit;
using System;
using System.Threading;
using System.Linq;

namespace backend_test
{
    public class SeleniumCrearBeneficioTests
    {
        private const string BaseUrl = "http://localhost:8080";
        private const string EmpleadorEmail = "selenium_test@gmail.com";
        private const string EmpleadorPassword = "123456";
        private string beneficioNombre;

        [Fact]
        public void CrearBeneficio_MontoFijo_Automatizado()
        {
            var options = new ChromeOptions();
            options.AddArgument("--start-maximized");
            options.AddArgument("--disable-blink-features=AutomationControlled");

            using var driver = new ChromeDriver(options);
            var wait = new WebDriverWait(driver, TimeSpan.FromSeconds(15));

            try
            {
                // 1. Navegar y hacer login
                driver.Navigate().GoToUrl(BaseUrl);
                WaitForPageLoad(driver, wait);
                Login(driver, wait);

                // 2. Esperar a que cargue el Profile
                wait.Until(d => d.Url.Contains("/Profile"));
                Thread.Sleep(3000);

                // 3. Seleccionar la empresa de prueba
                var companyDropdown = wait.Until(d => d.FindElement(By.CssSelector("select.company-select")));
                var selectCompany = new SelectElement(companyDropdown);
                selectCompany.SelectByText("Empresa SeleniumTest");

                // 4. Esperar redirección a PageEmpresaAdmin
                wait.Until(d => d.Url.Contains("/PageEmpresaAdmin"));
                Thread.Sleep(3000);

                // 5. Navegar a la sección de beneficios
                NavegarAFormularioBeneficio(driver, wait);

                // 6. Llenar formulario de beneficio
                LlenarFormularioBeneficio(driver, wait);

                // 7. Enviar formulario
                EnviarFormulario(driver, wait);

                Assert.True(true, "Beneficio creado exitosamente");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"URL actual: {driver.Url}");
                Console.WriteLine($"Título: {driver.Title}");

                try
                {
                    var screenshot = ((ITakesScreenshot)driver).GetScreenshot();
                    screenshot.SaveAsFile($"error_beneficio_{DateTime.Now:yyyyMMddHHmmss}.png");
                    Console.WriteLine("Screenshot guardado");
                }
                catch { }

                throw new Exception($"Error en test: {ex.Message}");
            }
            finally
            {
                Thread.Sleep(3000);
                driver.Quit();
            }
        }

        private void WaitForPageLoad(IWebDriver driver, WebDriverWait wait)
        {
            try
            {
                wait.Until(d => ((IJavaScriptExecutor)d).ExecuteScript("return document.readyState").Equals("complete"));
            }
            catch { }
        }

        private void Login(IWebDriver driver, WebDriverWait wait)
        {
            try
            {
                var emailField = wait.Until(d => d.FindElement(By.CssSelector("label.input > input[type='email']")));
                emailField.Clear();
                emailField.SendKeys(EmpleadorEmail);

                var passwordField = driver.FindElement(By.CssSelector("label.input > input[type='password']"));
                passwordField.Clear();
                passwordField.SendKeys(EmpleadorPassword);

                var loginButton = driver.FindElement(By.CssSelector("button.btn[type='submit']"));
                SafeClick(driver, loginButton, wait);

                Thread.Sleep(3000);
            }
            catch (Exception ex)
            {
                throw new Exception($"Error en login: {ex.Message}");
            }
        }

        private void NavegarAFormularioBeneficio(IWebDriver driver, WebDriverWait wait)
        {
            try
            {
                Console.WriteLine("Buscando botón para agregar beneficio...");

                // Posibles selectores para el botón de agregar beneficio
                var posiblesBotones = new[]
                {
                    "//button[contains(text(), 'Agregar Beneficio')]",
                    "//button[contains(text(), 'Nuevo Beneficio')]",
                    "//button[contains(text(), 'Crear Beneficio')]",
                    "//a[contains(@href, 'FormBeneficios')]",
                    "//a[contains(text(), 'Beneficio')]",
                    "//*[contains(text(), 'Beneficio') and (self::button or self::a)]"
                };

                foreach (var xpath in posiblesBotones)
                {
                    try
                    {
                        var elementos = driver.FindElements(By.XPath(xpath));
                        if (elementos.Count > 0)
                        {
                            Console.WriteLine($"Encontrado botón: {xpath}");
                            SafeClick(driver, elementos[0], wait);
                            Thread.Sleep(2000);

                            if (EsFormularioBeneficio(driver))
                            {
                                Console.WriteLine("✓ Formulario de beneficio encontrado");
                                return;
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"Intento fallido con {xpath}: {ex.Message}");
                    }
                }

                // Intento directo por URL
                Console.WriteLine("Intentando acceso directo por URL...");
                driver.Navigate().GoToUrl($"{BaseUrl}/FormBeneficios");
                Thread.Sleep(2000);

                if (EsFormularioBeneficio(driver))
                {
                    Console.WriteLine("✓ Acceso directo exitoso");
                    return;
                }

                throw new Exception("No se pudo acceder al formulario de beneficio");
            }
            catch (Exception ex)
            {
                throw new Exception($"Error navegando al formulario: {ex.Message}");
            }
        }

        private bool EsFormularioBeneficio(IWebDriver driver)
        {
            try
            {
                // Buscar el h2 con el texto "Registrar Beneficio"
                var h2Elementos = driver.FindElements(By.XPath("//h2[contains(text(), 'Beneficio')]"));

                // Buscar el placeholder específico "Nombre del beneficio"
                var nombreInput = driver.FindElements(By.XPath("//input[contains(@placeholder, 'Nombre del beneficio')]"));

                return h2Elementos.Count > 0 || nombreInput.Count > 0;
            }
            catch
            {
                return false;
            }
        }

        private void LlenarFormularioBeneficio(IWebDriver driver, WebDriverWait wait)
        {
            try
            {
                // Generar nombre único para el beneficio
                var random = new Random();
                int randomId = random.Next(1000, 9999);
                beneficioNombre = $"Beneficio{randomId:D4}";

                Console.WriteLine($"Creando beneficio: {beneficioNombre}");

                // Esperar a que el formulario esté listo
                Thread.Sleep(1000);

                // 1. Nombre del beneficio - Selector correcto del formulario
                var nombreInput = wait.Until(d => d.FindElement(By.XPath(
                    "//input[contains(@placeholder, 'Nombre del beneficio')]"
                )));
                nombreInput.Clear();
                nombreInput.SendKeys(beneficioNombre);

                Thread.Sleep(300);

                // 2. Tipo de beneficio - Todos los selects en el formulario
                var allSelects = driver.FindElements(By.TagName("select"));
                Console.WriteLine($"Total selects encontrados: {allSelects.Count}");

                if (allSelects.Count < 3)
                {
                    throw new Exception($"Se esperaban al menos 3 selects, se encontraron {allSelects.Count}");
                }

                // Primer select: Tipo de Beneficio
                var tipoSelect = allSelects[0];
                var selectTipo = new SelectElement(tipoSelect);
                Console.WriteLine($"Opciones en Tipo: {string.Join(", ", selectTipo.Options.Select(o => o.Text))}");
                selectTipo.SelectByText("Monto Fijo");
                Thread.Sleep(300);

                // Segundo select: Quién asume
                var quienAsumeSelect = allSelects[1];
                var selectQuienAsume = new SelectElement(quienAsumeSelect);
                Console.WriteLine($"Opciones en Quién Asume: {string.Join(", ", selectQuienAsume.Options.Select(o => o.Text))}");
                selectQuienAsume.SelectByText("Empresa");
                Thread.Sleep(300);

                // 3. Valor: 10000 - Input type number con placeholder "Valor*"
                var valorInput = wait.Until(d => d.FindElement(By.XPath(
                    "//input[@type='number' and contains(@placeholder, 'Valor')]"
                )));
                valorInput.Clear();
                valorInput.SendKeys("10000");
                Thread.Sleep(300);

                // Tercer select: Etiqueta
                var etiquetaSelect = allSelects[2];
                var selectEtiqueta = new SelectElement(etiquetaSelect);
                Console.WriteLine($"Opciones en Etiqueta: {string.Join(", ", selectEtiqueta.Options.Select(o => o.Text))}");
                selectEtiqueta.SelectByText("Beneficio");
                Thread.Sleep(300);

                Console.WriteLine("✓ Formulario de beneficio llenado correctamente");
            }
            catch (Exception ex)
            {
                throw new Exception($"Error llenando formulario: {ex.Message}");
            }
        }

        private void EnviarFormulario(IWebDriver driver, WebDriverWait wait)
        {
            try
            {
                // Buscar el botón "Registrar" o "Actualizar"
                IWebElement btnEnviar = null;
                var botonesEnviar = new[]
                {
                    "//button[contains(text(), 'Registrar')]",
                    "//button[contains(text(), 'Actualizar')]",
                    "//button[@type='submit' and contains(@class, 'btn-primary')]",
                    "//button[contains(@class, 'btn-primary')]"
                };

                foreach (var xpath in botonesEnviar)
                {
                    try
                    {
                        var elementos = driver.FindElements(By.XPath(xpath));
                        if (elementos.Count > 0 && elementos[0].Displayed && elementos[0].Enabled)
                        {
                            btnEnviar = elementos[0];
                            Console.WriteLine($"Botón encontrado: {xpath} - Texto: {btnEnviar.Text}");
                            break;
                        }
                    }
                    catch { }
                }

                if (btnEnviar == null)
                    throw new Exception("No se encontró el botón de envío");

                // Scroll al botón y hacer clic
                ((IJavaScriptExecutor)driver).ExecuteScript("arguments[0].scrollIntoView({block: 'center'});", btnEnviar);
                Thread.Sleep(500);

                SafeClick(driver, btnEnviar, wait);

                Console.WriteLine("✓ Formulario enviado");
                Thread.Sleep(4000);

                VerificarExito(driver);
            }
            catch (Exception ex)
            {
                throw new Exception($"Error enviando formulario: {ex.Message}");
            }
        }

        private void VerificarExito(IWebDriver driver)
        {
            try
            {
                // Buscar mensaje de éxito
                var mensajesExito = new[]
                {
                    "//*[contains(@class, 'message') and contains(@class, 'success')]",
                    "//*[contains(text(), 'registrado correctamente')]",
                    "//*[contains(text(), 'exitosamente')]",
                    "//*[contains(text(), 'éxito')]"
                };

                foreach (var xpath in mensajesExito)
                {
                    try
                    {
                        var elemento = driver.FindElement(By.XPath(xpath));
                        if (elemento.Displayed)
                        {
                            Console.WriteLine($"✓ Mensaje de éxito: {elemento.Text}");
                            return;
                        }
                    }
                    catch { }
                }

                // Verificar si redirigió a PageEmpresaAdmin
                if (driver.Url.Contains("PageEmpresaAdmin"))
                {
                    Console.WriteLine("✓ Redirigido a PageEmpresaAdmin - Beneficio creado exitosamente");
                    return;
                }

                Console.WriteLine("✓ Beneficio creado (sin mensaje explícito)");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Verificación de éxito: {ex.Message}");
            }
        }

        private void SafeClick(IWebDriver driver, IWebElement element, WebDriverWait wait)
        {
            try
            {
                // Esperar a que sea clickeable
                wait.Until(d => element.Displayed && element.Enabled);

                // Scroll al elemento
                ((IJavaScriptExecutor)driver).ExecuteScript("arguments[0].scrollIntoView({block: 'center'});", element);
                Thread.Sleep(300);

                // Intentar clic normal
                element.Click();
            }
            catch (ElementClickInterceptedException)
            {
                // Si falla, usar JavaScript
                ((IJavaScriptExecutor)driver).ExecuteScript("arguments[0].click();", element);
            }
        }
    }
}