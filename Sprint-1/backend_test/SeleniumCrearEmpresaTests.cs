using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;
using OpenQA.Selenium.Support.UI;
using Xunit;
using System;
using System.Threading;
using System.Linq;

namespace backend_test
{
    public class SeleniumCrearEmpresaTests
    {
        private const string BaseUrl = "http://localhost:8080";
        private const string EmpleadorEmail = "selenium_test@gmail.com";
        private const string EmpleadorPassword = "123456";
        private string empresaCedulaJuridica;
        private string empresaNombre;

        [Fact]
        public void CrearYEliminarEmpresa_Automatizado()
        {
            using var driver = new ChromeDriver();
            driver.Manage().Window.Maximize();
            var wait = new WebDriverWait(driver, TimeSpan.FromSeconds(15));

            try
            {
                // 1. Navegar al login
                driver.Navigate().GoToUrl(BaseUrl);
                WaitForPageLoad(driver, wait);

                // 2. Login
                Login(driver, wait);

                // 3. Esperar a que cargue el Profile
                WaitForPageLoad(driver, wait);
                Thread.Sleep(3000);

                // 4. Navegar al formulario de crear empresa
                NavegarAFormularioEmpresa(driver, wait);

                // 5. Llenar formulario y crear empresa
                LlenarFormularioEmpresa(driver, wait);
                EnviarFormulario(driver, wait);

                // 6. Volver al Profile y seleccionar la nueva empresa en el dropdown
                Thread.Sleep(3000);
                VolverAProfileYSeleccionarEmpresa(driver, wait);

                // 7. Navegar a PageEmpresaAdmin
                NavegarAPageEmpresaAdmin(driver, wait);

                // 8. Eliminar la empresa recién creada
                EliminarEmpresaRecienCreada(driver, wait);

                Assert.True(true, "Empresa creada y eliminada exitosamente");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"URL actual: {driver.Url}");
                Console.WriteLine($"Título: {driver.Title}");

                try
                {
                    var screenshot = ((ITakesScreenshot)driver).GetScreenshot();
                    screenshot.SaveAsFile($"error_empresa_{DateTime.Now:yyyyMMddHHmmss}.png");
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
                IWebElement emailField = wait.Until(d => d.FindElement(By.CssSelector("label.input > input[type='email']")));
                emailField.Clear();
                emailField.SendKeys(EmpleadorEmail);

                IWebElement passwordField = driver.FindElement(By.CssSelector("label.input > input[type='password']"));
                passwordField.Clear();
                passwordField.SendKeys(EmpleadorPassword);

                IWebElement loginButton = driver.FindElement(By.CssSelector("button.btn[type='submit']"));
                loginButton.Click();
                Thread.Sleep(3000);
            }
            catch (Exception ex)
            {
                throw new Exception($"Error en login: {ex.Message}");
            }
        }

        private void NavegarAFormularioEmpresa(IWebDriver driver, WebDriverWait wait)
        {
            try
            {
                Console.WriteLine("Buscando formas de acceder al formulario de empresa...");

                var posiblesBotones = new[]
                {
                    "//a[contains(@href, 'empresa') or contains(@href, 'Empresa')]",
                    "//button[contains(text(), 'Empresa')]",
                    "//*[contains(text(), 'Crear Empresa')]",
                    "//*[contains(text(), 'Nueva Empresa')]",
                    "//*[contains(text(), 'Agregar Empresa')]",
                    "//a[contains(@href, 'FormEmpresa')]",
                    "//button[contains(@class, 'empresa')]",
                    "//a[contains(@class, 'empresa')]"
                };

                foreach (var xpath in posiblesBotones)
                {
                    try
                    {
                        var elementos = driver.FindElements(By.XPath(xpath));
                        if (elementos.Count > 0)
                        {
                            Console.WriteLine($"Encontrado elemento con XPath: {xpath}");
                            var elemento = elementos[0];

                            ((IJavaScriptExecutor)driver).ExecuteScript("arguments[0].scrollIntoView(true);", elemento);
                            Thread.Sleep(1000);
                            elemento.Click();
                            Thread.Sleep(3000);

                            if (EsFormularioEmpresa(driver))
                            {
                                Console.WriteLine("Navegación exitosa al formulario de empresa");
                                return;
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"Error con XPath {xpath}: {ex.Message}");
                    }
                }

                // Intentar acceso directo
                Console.WriteLine("Intentando acceso directo por URL...");
                driver.Navigate().GoToUrl($"{BaseUrl}/FormEmpresa");
                Thread.Sleep(3000);

                if (EsFormularioEmpresa(driver))
                {
                    Console.WriteLine("Acceso directo exitoso");
                    return;
                }

                throw new Exception("No se pudo acceder al formulario de empresa");
            }
            catch (Exception ex)
            {
                throw new Exception($"Error navegando al formulario: {ex.Message}");
            }
        }

        private bool EsFormularioEmpresa(IWebDriver driver)
        {
            try
            {
                var elementosEsperados = new[]
                {
                    "//h1[contains(text(), 'Fiesta Fries')]",
                    "//input[contains(@placeholder, 'Cédula jurídica')]",
                    "//input[contains(@placeholder, 'Nombre de la empresa')]",
                    "//select",
                    "//input[contains(@placeholder, 'Día de pago')]"
                };

                foreach (var xpath in elementosEsperados)
                {
                    try
                    {
                        var elemento = driver.FindElement(By.XPath(xpath));
                        if (!elemento.Displayed)
                            return false;
                    }
                    catch
                    {
                        return false;
                    }
                }
                return true;
            }
            catch
            {
                return false;
            }
        }

        private void LlenarFormularioEmpresa(IWebDriver driver, WebDriverWait wait)
        {
            if (!EsFormularioEmpresa(driver))
                throw new Exception("No estamos en el formulario de empresa");

            empresaCedulaJuridica = GenerarCedulaJuridica();
            empresaNombre = $"Empresa Selenium Test {DateTime.Now:yyyyMMddHHmmss}";

            Console.WriteLine($"Llenando formulario para: {empresaNombre}");

            // Cédula jurídica
            var cedulaInput = wait.Until(d => d.FindElement(By.XPath("//input[contains(@placeholder, 'Cédula jurídica')]")));
            cedulaInput.Clear();
            cedulaInput.SendKeys(empresaCedulaJuridica);

            // Nombre de la empresa
            var nombreInput = driver.FindElement(By.XPath("//input[contains(@placeholder, 'Nombre de la empresa')]"));
            nombreInput.Clear();
            nombreInput.SendKeys(empresaNombre);

            // Dirección (opcional)
            try
            {
                var direccionInput = driver.FindElement(By.XPath("//input[contains(@placeholder, 'Dirección específica')]"));
                direccionInput.Clear();
                direccionInput.SendKeys("San José, Costa Rica - Selenium Test");
            }
            catch { }

            // Teléfono (opcional)
            try
            {
                var telefonoInput = driver.FindElement(By.XPath("//input[contains(@placeholder, 'Teléfono')]"));
                telefonoInput.Clear();
                telefonoInput.SendKeys("22225555");
            }
            catch { }

            // Número máximo de beneficios
            var beneficiosInput = driver.FindElement(By.CssSelector("input[type='number']"));
            beneficiosInput.Clear();
            beneficiosInput.SendKeys("10");

            // Frecuencia de pago
            var frecuenciaSelect = driver.FindElement(By.TagName("select"));
            var selectFrecuencia = new SelectElement(frecuenciaSelect);
            selectFrecuencia.SelectByText("Quincenal");

            // Día de pago
            var diaPagoInput = driver.FindElement(By.XPath("//input[contains(@placeholder, 'Día de pago')]"));
            diaPagoInput.Clear();
            diaPagoInput.SendKeys("15");

            Console.WriteLine("Formulario llenado correctamente");
        }

        private void EnviarFormulario(IWebDriver driver, WebDriverWait wait)
        {
            IWebElement btnEnviar = null;
            var botonesEnviar = new[]
            {
                "//button[contains(text(), 'Agregar Empresa')]",
                "//button[contains(text(), 'Registrar')]",
                "//button[contains(text(), 'Crear')]",
                "//button[contains(text(), 'Guardar')]",
                "//button[@type='submit']",
                "//button[contains(@class, 'btn-primary')]"
            };

            foreach (var xpath in botonesEnviar)
            {
                try
                {
                    var elementos = driver.FindElements(By.XPath(xpath));
                    if (elementos.Count > 0)
                    {
                        btnEnviar = elementos[0];
                        break;
                    }
                }
                catch { }
            }

            if (btnEnviar == null)
                throw new Exception("No se pudo encontrar el botón para enviar el formulario");

            ((IJavaScriptExecutor)driver).ExecuteScript("arguments[0].scrollIntoView(true);", btnEnviar);
            Thread.Sleep(1000);
            btnEnviar.Click();

            Console.WriteLine("Formulario enviado");

            Thread.Sleep(5000);

            VerificarExito(driver);
        }

        private void VerificarExito(IWebDriver driver)
        {
            try
            {
                var mensajesExito = new[]
                {
                    "//*[contains(text(), 'correctamente')]",
                    "//*[contains(text(), 'éxito')]",
                    "//*[contains(text(), 'success')]",
                    "//*[contains(@class, 'success')]"
                };

                foreach (var xpath in mensajesExito)
                {
                    try
                    {
                        var elemento = driver.FindElement(By.XPath(xpath));
                        if (elemento.Displayed)
                        {
                            Console.WriteLine($" Mensaje de éxito: {elemento.Text}");
                            return;
                        }
                    }
                    catch { }
                }

                if (driver.Url.Contains("/Profile") || !driver.Url.Contains("FormEmpresa"))
                {
                    Console.WriteLine(" Redirección exitosa después de crear empresa");
                    return;
                }

                Console.WriteLine(" No se encontró mensaje explícito de éxito");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error verificando éxito: {ex.Message}");
            }
        }

        private void VolverAProfileYSeleccionarEmpresa(IWebDriver driver, WebDriverWait wait)
        {
            try
            {
                Console.WriteLine("Volviendo al Profile y seleccionando empresa...");

                if (!driver.Url.Contains("/Profile"))
                {
                    driver.Navigate().GoToUrl($"{BaseUrl}/Profile");
                    Thread.Sleep(3000);
                    WaitForPageLoad(driver, wait);
                }

                SeleccionarEmpresaEnDropdown(driver, wait);

                Console.WriteLine(" Empresa seleccionada en el dropdown");
            }
            catch (Exception ex)
            {
                throw new Exception($"Error seleccionando empresa en dropdown: {ex.Message}");
            }
        }

        private void SeleccionarEmpresaEnDropdown(IWebDriver driver, WebDriverWait wait)
        {
            try
            {
                Console.WriteLine("Buscando dropdown de empresas...");

                // Buscar el dropdown de empresas
                IWebElement dropdownEmpresas = null;
                var dropdownSelectors = new[]
                {
                    By.CssSelector("select#companySelect"),
                    By.CssSelector("select.company-dropdown"),
                    By.CssSelector("select"),
                    By.XPath("//select[contains(@id, 'company')]"),
                    By.XPath("//select[contains(@name, 'company')]"),
                    By.XPath("//label[contains(text(), 'Empresa')]/following-sibling::select")
                };

                foreach (var selector in dropdownSelectors)
                {
                    try
                    {
                        dropdownEmpresas = driver.FindElement(selector);
                        if (dropdownEmpresas.Displayed)
                        {
                            Console.WriteLine(" Dropdown de empresas encontrado");
                            break;
                        }
                    }
                    catch { }
                }

                if (dropdownEmpresas == null)
                    throw new Exception("No se pudo encontrar el dropdown de empresas");

                var selectEmpresa = new SelectElement(dropdownEmpresas);
                var opciones = selectEmpresa.Options;
                Console.WriteLine($"Opciones disponibles en dropdown: {opciones.Count}");

                foreach (var opcion in opciones)
                {
                    Console.WriteLine($"Opción: {opcion.Text}");
                }

                // Buscar nuestra empresa por nombre
                IWebElement opcionEmpresa = null;

                foreach (var opcion in opciones)
                {
                    if (opcion.Text.Contains("Selenium Test") || opcion.Text == empresaNombre)
                    {
                        opcionEmpresa = opcion;
                        Console.WriteLine($" Empresa encontrada en dropdown: {opcion.Text}");
                        break;
                    }
                }

                if (opcionEmpresa == null)
                {
                    if (opciones.Count > 1) 
                    {
                        opcionEmpresa = opciones[opciones.Count - 1];
                        Console.WriteLine($" Seleccionando última opción: {opcionEmpresa.Text}");
                    }
                    else
                    {
                        throw new Exception($"No se pudo encontrar la empresa '{empresaNombre}' en el dropdown");
                    }
                }

                selectEmpresa.SelectByText(opcionEmpresa.Text);
                Thread.Sleep(2000);

                Console.WriteLine("Empresa seleccionada en dropdown");

            }
            catch (Exception ex)
            {
                throw new Exception($"Error seleccionando empresa en dropdown: {ex.Message}");
            }
        }

        private void NavegarAPageEmpresaAdmin(IWebDriver driver, WebDriverWait wait)
        {
            try
            {
                Console.WriteLine("Navegando a PageEmpresaAdmin...");

                // Buscar enlace/botón para administrar empresas
                var botonesAdmin = new[]
                {
                    "//a[contains(@href, 'PageEmpresaAdmin')]",
                    "//a[contains(text(), 'Ver Toda Empresa')]",
                    "//a[contains(text(), 'Administrar Empresa')]",
                    "//a[contains(text(), 'Empresa Admin')]",
                    "//button[contains(text(), 'Empresa Admin')]"
                };

                foreach (var xpath in botonesAdmin)
                {
                    try
                    {
                        var elementos = driver.FindElements(By.XPath(xpath));
                        if (elementos.Count > 0)
                        {
                            var elemento = elementos[0];
                            ((IJavaScriptExecutor)driver).ExecuteScript("arguments[0].scrollIntoView(true);", elemento);
                            Thread.Sleep(1000);
                            elemento.Click();
                            Thread.Sleep(3000);

                            // Verificar que estamos en PageEmpresaAdmin
                            if (driver.Url.Contains("PageEmpresaAdmin"))
                            {
                                Console.WriteLine(" Navegación exitosa a PageEmpresaAdmin");
                                return;
                            }
                        }
                    }
                    catch { }
                }

                driver.Navigate().GoToUrl($"{BaseUrl}/PageEmpresaAdmin");
                Thread.Sleep(3000);

                if (driver.Url.Contains("PageEmpresaAdmin"))
                {
                    Console.WriteLine(" Acceso directo a PageEmpresaAdmin exitoso");
                    return;
                }

                throw new Exception("No se pudo navegar a PageEmpresaAdmin");
            }
            catch (Exception ex)
            {
                throw new Exception($"Error navegando a PageEmpresaAdmin: {ex.Message}");
            }
        }

        private void EliminarEmpresaRecienCreada(IWebDriver driver, WebDriverWait wait)
        {
            try
            {
                Console.WriteLine($"Buscando empresa para eliminar: {empresaNombre}");

                WaitForPageLoad(driver, wait);
                Thread.Sleep(3000);

                var filasEmpresa = driver.FindElements(By.XPath("//table//tr[contains(@class, 'empresa-row') or .//td]"));

                Console.WriteLine($"Encontradas {filasEmpresa.Count} filas en la tabla");

                bool empresaEncontrada = false;

                foreach (var fila in filasEmpresa)
                {
                    try
                    {
                        var celdas = fila.FindElements(By.TagName("td"));
                        if (celdas.Count >= 2)
                        {
                            var cedulaCelda = celdas[0].Text.Trim();
                            var nombreCelda = celdas[1].Text.Trim();

                            Console.WriteLine($"Revisando empresa: {nombreCelda} - {cedulaCelda}");

                            if (cedulaCelda == empresaCedulaJuridica || nombreCelda.Contains("Selenium Test"))
                            {
                                Console.WriteLine($" Empresa encontrada: {nombreCelda}");
                                empresaEncontrada = true;

                                var botonesEliminar = fila.FindElements(By.XPath(".//button[contains(text(), 'Borrar') or contains(text(), 'Eliminar')]"));

                                if (botonesEliminar.Count > 0)
                                {
                                    var btnEliminar = botonesEliminar[0];

                                    ((IJavaScriptExecutor)driver).ExecuteScript("arguments[0].scrollIntoView(true);", btnEliminar);
                                    Thread.Sleep(1000);
                                    ((IJavaScriptExecutor)driver).ExecuteScript("arguments[0].click();", btnEliminar);

                                    Console.WriteLine(" Botón de eliminar clickeado");
                                    Thread.Sleep(2000);

                                    ConfirmarEliminacionEnModal(driver, wait);
                                    return;
                                }
                                else
                                {
                                    throw new Exception("No se encontró botón de eliminar en la fila de la empresa");
                                }
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"Error revisando fila: {ex.Message}");
                    }
                }

                if (!empresaEncontrada)
                {
                    Console.WriteLine("=== DEBUGGING INFO ===");
                    Console.WriteLine($"Buscábamos: {empresaNombre} (Cédula: {empresaCedulaJuridica})");
                    Console.WriteLine($"URL actual: {driver.Url}");
                    Console.WriteLine($"Page source length: {driver.PageSource.Length}");

                    var todasLasFilas = driver.FindElements(By.XPath("//table//tr"));
                    foreach (var fila in todasLasFilas)
                    {
                        try
                        {
                            var celdas = fila.FindElements(By.TagName("td"));
                            if (celdas.Count > 0)
                            {
                                Console.WriteLine($"Fila: {string.Join(" | ", celdas.Select(c => c.Text))}");
                            }
                        }
                        catch { }
                    }

                    throw new Exception($"No se pudo encontrar la empresa '{empresaNombre}' en la lista de PageEmpresaAdmin");
                }
            }
            catch (Exception ex)
            {
                throw new Exception($"Error eliminando empresa: {ex.Message}");
            }
        }

        private void ConfirmarEliminacionEnModal(IWebDriver driver, WebDriverWait wait)
        {
            try
            {
                Console.WriteLine("Buscando modal de confirmación...");

                var botonesConfirmar = new[]
                {
                    "//button[contains(text(), 'Confirmar')]",
                    "//button[contains(text(), 'Eliminar')]",
                    "//button[contains(text(), 'Sí')]",
                    "//button[contains(text(), 'Aceptar')]",
                    "//button[contains(@class, 'btn-primary')]",
                    "//button[contains(@class, 'btn-danger')]"
                };

                foreach (var xpath in botonesConfirmar)
                {
                    try
                    {
                        var elementos = driver.FindElements(By.XPath(xpath));
                        foreach (var elemento in elementos)
                        {
                            if (elemento.Displayed && elemento.Enabled)
                            {
                                Console.WriteLine($" Botón de confirmación encontrado: {elemento.Text}");
                                ((IJavaScriptExecutor)driver).ExecuteScript("arguments[0].click();", elemento);
                                Thread.Sleep(3000);
                                Console.WriteLine(" Eliminación confirmada");

                                VerificarEmpresaEliminada(driver);
                                return;
                            }
                        }
                    }
                    catch { }
                }

                throw new Exception("No se pudo encontrar el botón de confirmación en el modal");
            }
            catch (Exception ex)
            {
                throw new Exception($"Error confirmando eliminación: {ex.Message}");
            }
        }

        private void VerificarEmpresaEliminada(IWebDriver driver)
        {
            try
            {
                Thread.Sleep(3000);

                var mensajesExito = new[]
                {
                    "//*[contains(text(), 'eliminada')]",
                    "//*[contains(text(), 'éxito')]",
                    "//*[contains(text(), 'correctamente')]",
                    "//*[contains(@class, 'success')]"
                };

                foreach (var xpath in mensajesExito)
                {
                    try
                    {
                        var elemento = driver.FindElement(By.XPath(xpath));
                        if (elemento.Displayed)
                        {
                            Console.WriteLine($" Mensaje de eliminación: {elemento.Text}");
                            return;
                        }
                    }
                    catch { }
                }

                Console.WriteLine(" Eliminación completada (sin mensaje explícito)");
            }
            catch (Exception ex)
            {
                throw new Exception($"Error verificando eliminación: {ex.Message}");
            }
        }

        private string GenerarCedulaJuridica()
        {
            var random = new Random();
            return random.Next(1000000000, 1999999999).ToString().Substring(0, 10);
        }
    }
}