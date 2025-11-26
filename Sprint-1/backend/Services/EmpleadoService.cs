using backend.Interfaces.Services;
using backend.Models;
using backend.Repositories;
using Dapper;
using iText.Commons.Bouncycastle.Asn1.X509;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http.HttpResults;
using System;
using System.Collections.Generic;
using Microsoft.Data.SqlClient;
using System.Drawing;
using static iText.StyledXmlParser.Jsoup.Select.Evaluator;
using static System.Runtime.InteropServices.JavaScript.JSType;
using Microsoft.Extensions.Logging;

namespace backend.Services
{
    public class EmpleadoService : IEmployeeService
    {
        private readonly string _connectionString;
        private readonly PersonService _personService;
        private readonly PersonRepository _personRepository;
        private readonly EmpleadoRepository _empleadoService;

        public EmpleadoService()
        {
            var builder = WebApplication.CreateBuilder();
            _connectionString = builder.Configuration.GetConnectionString("UserContext")
                ?? throw new InvalidOperationException("Connection string 'UserContext' not found.");

            _personService = new PersonService();
            _personRepository = new PersonRepository();
            _empleadoService = new EmpleadoRepository(); 
        }

        // Reutiliza PersonService: si no existe la persona la crea (PersonService crea user si hace falta)
        public EmpleadoModel CreateEmpleadoWithPersonaAndUser(EmpleadoCreateRequest req)
        {
            try
            {
                // 1) comprobar si existe la persona por su id (identidad num�rica)
                var existingPersona = _personService.GetByIdentity(req.personaId);

                PersonModel personaUsed;
                if (existingPersona != null)
                {
                    personaUsed = existingPersona;
                    Console.WriteLine($"Persona existente encontrada. Id persona: {personaUsed.id}, user: {personaUsed.uniqueUser}");
                }
                else
                {
                    // Construir PersonModel desde el request y delegar la creaci�n a PersonService
                    var personToCreate = new PersonModel
                    {
                        id = req.personaId,
                        firstName = req.firstName,
                        secondName = req.secondName,
                        birthdate = req.birthdate,
                        direction = req.direction,
                        personalPhone = req.personalPhone,
                        homePhone = req.homePhone,
                        email = req.userEmail, // usar email para crear user dentro de PersonService
                        personType = req.personType ?? "Empleado",
                        uniqueUser = Guid.Empty
                    };

                    personaUsed = _personService.Insert(personToCreate);
                    Console.WriteLine($"Persona creada por PersonService: id={personaUsed.id}, uniqueUser={personaUsed.uniqueUser}");
                }

                // 2) insertar Empleado usando el id de la persona
                using var connection = new SqlConnection(_connectionString);
                connection.Open();
                var empleadoD = new EmpleadoModel
                {
                    id = personaUsed.id,
                    position = req.position,
                    employmentType = req.employmentType,
                    salary = req.salary,
                    hireDate = req.hireDate,
                    department = req.departament,
                    idCompny = req.idCompny
                };

                const string insertEmpleado = @"INSERT INTO [Fiesta_Fries_DB].[Empleado] (id, position, employmentType,salary,hireDate,department,idCompny) VALUES (@id, @position, @employmentType, @salary,@hireDate,@department,@idCompny)";
                connection.Execute(insertEmpleado, empleadoD);
                Console.WriteLine($"Empleado creado para persona id={personaUsed.id}");

                return empleadoD;
            
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error en CreateEmpleadoWithPersonaAndUser: " + ex);
                throw;
            }


        }

        public List<EmpleadoListDto> GetByEmpresa(long cedulaJuridica)
        {
            var empleadoRepository = new EmpleadoRepository();
            return empleadoRepository.GetByEmpresa(cedulaJuridica);
        }

        public async Task<decimal> GetSalarioBrutoAsync(int cedulaEmpleado)
        {
            using var connection = new SqlConnection(_connectionString);
            const string query = "SELECT salary FROM [Fiesta_Fries_DB].[Empleado] WHERE id = @Cedula";
            var salario = await connection.ExecuteScalarAsync<int?>(query, new { Cedula = cedulaEmpleado });
            return salario ?? 0;
        }

        public List<EmployeeCalculationDto> GetEmployeeCalculationDtos(long cedulaJuridica, DateTime fechaInicio, DateTime fechaFin)
        {
            try
            {
                var resultado = _empleadoService.GetEmployeesForPayroll(cedulaJuridica, fechaInicio, fechaFin);
                return resultado;
            }
            catch (Exception ex)
            {
                return new List<EmployeeCalculationDto>();
            }
        }

        public List<EmployeeDeductionsByPayrollModel> GetEmployeePayrollData(int id, DateTime date)
        {
            
            List < EmployeeDeductionsByPayrollModel> data = this._empleadoService.GetEmployeePayrollData(id, date);

            if(data == null)
            {
                data = new List<EmployeeDeductionsByPayrollModel> ();
            }

            return data;
        }

        public DateTime GetHireDate(int id)
        {
            DateTime hireDate = this._empleadoService.GetHireDate(id);

            return hireDate;
        }
        public int GetSalaryAmount(int id)
        {
            int salary = this._empleadoService.GetSalaryAmount(id);

            return salary;
        }

        public EmployeeDashboardDataModelcs? GetDashboardData(int id, DateTime date)
        {
            int employeeSalary = this.GetSalaryAmount(id);

             List<EmployeeDeductionsByPayrollModel> payrollData = this.GetEmployeePayrollData(id, date);

            decimal deductionsAmount = this.DeductionAmount(payrollData);

            decimal netsalary = employeeSalary - deductionsAmount;

            decimal retained_percentage = 100 - ((netsalary / employeeSalary) * 100);

            EmployeeDashboardDataModelcs data = this.BuildPayrollData(employeeSalary, deductionsAmount, netsalary, retained_percentage);

            return data;

        }

       public EmployeeDashboardDataModelcs BuildPayrollData(int employeeSalary, decimal deductionsAmount, decimal netsalary, decimal retained_percentage)
        {
            EmployeeDashboardDataModelcs data = new EmployeeDashboardDataModelcs();

            data.NetSalary = netsalary;
            data.CrudSalary = employeeSalary;
            data.TotalDeductions = deductionsAmount;
            data.ReteinedPercentage = retained_percentage;

            return data;
        }

        public decimal DeductionAmount(List<EmployeeDeductionsByPayrollModel> data)
        {
            decimal deductions = 0;

            foreach(var item in data)
            {
                deductions += item.DeductionAmount;
            }

            return deductions;
        }

        public DateTime? GetLastPayRoll(int id)
        {
            DateTime? lastPayroll = this._empleadoService.GetLastPayroll(id);

            return lastPayroll;
        }

        public async Task<bool> UpdateEmpleadoAsync(int id, EmpleadoUpdateDto dto)
        {
            var empleado = _empleadoService.GetById(id);
            if (empleado == null)
                return false;

            var persona = _personRepository.GetByIdentity(id);
            if (persona == null)
                return false;

            persona.firstName = dto.FirstName;
            persona.secondName = dto.SecondName;
            persona.direction = dto.Direction;
            persona.personalPhone = dto.PersonalPhone;
            persona.homePhone = dto.HomePhone;

            empleado.position = dto.Position;
            empleado.department = dto.Department;
            empleado.salary = dto.Salary;

            _personRepository.Update(persona);
            await _empleadoService.UpdateAsync(empleado);

            return true;
        }
        public async Task<EmpleadoUpdateDto?> GetEmpleadoPersonaByIdAsync(int id)
        {
            return await _empleadoService.GetEmpleadoPersonaByIdAsync(id);
        }

        public byte[] GenerateDashBoardChart(int id, DateTime date)
        {
            List<EmployeeDeductionsByPayrollModel> items = this.GetEmployeePayrollData(id, date);

            int width = 1000;  
            int height = 600;

            using Bitmap bmp = new(width, height);
            using Graphics g = Graphics.FromImage(bmp);

            g.Clear(Color.White);
            g.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;
            g.TextRenderingHint = System.Drawing.Text.TextRenderingHint.AntiAlias;

           
            Font titleFont = new Font("Arial", 14, FontStyle.Bold);
            Font valueFont = new Font("Arial", 10, FontStyle.Bold);
            Font labelFont = new Font("Arial", 9);

            Brush textBrush = Brushes.Black;

            
            g.DrawString("Desglose de Deducciones Salariales", titleFont, textBrush, 50, 20);

            Brush[] colors =
            {
              Brushes.SteelBlue,
              Brushes.MediumSeaGreen,
              Brushes.Salmon,
              Brushes.Goldenrod,
              Brushes.MediumPurple,
              Brushes.LightCoral,
              Brushes.LightSeaGreen,
              Brushes.Plum
            };

            int barWidth = 70;
            int spacing = 30;
            int startX = 120;  
            int startY = 80;
            int chartHeight = height - startY - 100;

            if (items.Count == 0)
            {
                // Manejar caso sin datos
                g.DrawString("No hay datos de deducciones disponibles", titleFont, Brushes.Gray, width / 2 - 150, height / 2);
            }
            else
            {
                decimal maxValue = items.Max(i => i.DeductionAmount);
                float scale = chartHeight / (float)maxValue;

                int colorIndex = 0;
                int x = startX;

                foreach (var item in items)
                {
                    var color = colors[colorIndex % colors.Length];
                    colorIndex++;

                    float barHeight = (float)item.DeductionAmount * scale;
                    int barY = startY + chartHeight - (int)barHeight;

                    g.FillRectangle(color, x, barY, barWidth, barHeight);
                    g.DrawRectangle(Pens.Black, x, barY, barWidth, barHeight);

                    string amountText = "₡" + item.DeductionAmount.ToString("N0");
                    SizeF textSize = g.MeasureString(amountText, valueFont);

                    float textX = x + (barWidth - textSize.Width) / 2;
                    float textY = barY - textSize.Height - 5;

                    if (textY < 10) textY = barY + barHeight + 5;

                    g.DrawString(amountText, valueFont, textBrush, textX, textY);

                    if (item.Percentage != null)
                    {
                        string percentText = item.Percentage.Value.ToString("0.##") + "%";
                        SizeF percentSize = g.MeasureString(percentText, labelFont);

                        float percentX = x + (barWidth - percentSize.Width) / 2;
                        float percentY = textY - percentSize.Height - 3;

                        if (percentY < 10)
                        {
                            percentY = barY + barHeight + 20;
                        }

                        g.DrawString(percentText, labelFont, textBrush, percentX, percentY);
                    }

                    string deductionName = WrapText(item.DeductionName, 15); 
                    SizeF nameSize = g.MeasureString(deductionName, labelFont);

                    System.Drawing.Drawing2D.Matrix originalTransform = g.Transform;
                    g.TranslateTransform(x + barWidth / 2, startY + chartHeight + 40); 
                    g.RotateTransform(-45); 
                    g.DrawString(deductionName, labelFont, textBrush, -nameSize.Width / 2, 0);
                    g.Transform = originalTransform;

                    x += barWidth + spacing;
                }

                DrawYAxis(g, startX - 10, startY, chartHeight, maxValue, valueFont, items);
            }

            using MemoryStream ms = new MemoryStream();
            bmp.Save(ms, System.Drawing.Imaging.ImageFormat.Png);
            return ms.ToArray();
        }

        private string WrapText(string text, int maxLength)
        {
            if (text.Length <= maxLength) return text;

            string[] words = text.Split(' ');
            string result = "";
            string currentLine = "";

            foreach (string word in words)
            {
                if ((currentLine + word).Length > maxLength)
                {
                    result += currentLine.Trim() + "\n";
                    currentLine = word + " ";
                }
                else
                {
                    currentLine += word + " ";
                }
            }

            result += currentLine.Trim();
            return result;
        }

        private void DrawYAxis(Graphics g, int x, int y, int height, decimal maxValue, Font font, List<EmployeeDeductionsByPayrollModel> items)
        {
            int numberOfIntervals = 5;
            Brush axisBrush = Brushes.Black;
            Pen axisPen = new Pen(axisBrush, 1);

           
            g.DrawLine(axisPen, x, y, x, y + height);

            for (int i = 0; i <= numberOfIntervals; i++)
            {
                decimal value = maxValue * i / numberOfIntervals;
                int yPos = y + height - (int)(height * i / numberOfIntervals);

                string valueText = "₡" + value.ToString("N0");
                SizeF textSize = g.MeasureString(valueText, font);

                g.DrawLine(axisPen, x - 5, yPos, x, yPos);

                g.DrawString(valueText, font, axisBrush, x - textSize.Width - 5, yPos - textSize.Height / 2);

                g.DrawLine(new Pen(Color.LightGray, 1), x, yPos, x + (items.Count * (70 + 30)) + 50, yPos);
            }
        }

    }
}
