using backend.Handlers.backend.Repositories;
using backend.Models;
using backend.Services;
using System.Drawing;

namespace backend.Services
{
    public class EmpresaService
    {
        private readonly EmpresaRepository _empresaRepository;

        public EmpresaService()
        {
            _empresaRepository = new EmpresaRepository();
        }

        public string CreateEmpresa(EmpresaModel empresa)
        {
            try
            {
                Console.WriteLine("=== EMPRESA SERVICE ===");
                Console.WriteLine($"Cédula: {empresa.CedulaJuridica}");
                Console.WriteLine($"DueñoEmpresa: {empresa.DueñoEmpresa}");

                var result = _empresaRepository.CreateEmpresa(empresa);

                if (string.IsNullOrEmpty(result))
                {
                    Console.WriteLine("SERVICE: Empresa creada exitosamente");
                    return string.Empty; 
                }
                else
                {
                    Console.WriteLine($"SERVICE: Error - {result}");
                    return result; 
                }
            }
            catch (Exception ex)
            {
                var errorMessage = $"Error en service: {ex.Message}";
                Console.WriteLine($"SERVICE EXCEPTION: {errorMessage}");
                return errorMessage;
            }
        }

        public List<EmpresaModel> GetEmpresas()
        {
            return _empresaRepository.GetEmpresas();
        }

        public List<EmpresaModel> GetEmpresasByOwner(int ownerId)
        {
            try
            {
                return _empresaRepository.GetByOwner(ownerId);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[ERROR] Service GetEmpresasByOwner: {ex.Message}");
                return new List<EmpresaModel>();
            }
        }

        public EmpresaModel GetEmpresaById(int id)
        {
            try
            {
                return _empresaRepository.GetById(id);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[ERROR] Service GetEmpresaById: {ex.Message}");
                return null;
            }
        }

        public EmpresaModel GetEmpresaByEmployeeUserId(string userId)
        {
            try
            {
                return _empresaRepository.GetByEmployeeUserId(userId);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[ERROR] Service GetEmpresaByEmployeeUserId: {ex.Message}");
                return null;
            }
        }

        public EmpresaModel GetEmpresaByCedula(long cedulaJuridica)
        {
            return _empresaRepository.GetByCedula(cedulaJuridica);
        }

        public void UpdateEmpresa(EmpresaModel empresa)
        {
            _empresaRepository.Update(empresa);
        }

        public List<EmploymentTypeCountModel>? GetEmployeersCountbyRol(long id)
        {
            return this._empresaRepository.GetEmpleadosCountByRoll(id);
        }

        public byte[] GenerateEmploymentTypeChart(long companyId)
        {
            var rawData = GetEmployeersCountbyRol(companyId);

            // Debug
            for (int i = 0; i < rawData.Count; i++)
            {
                Console.WriteLine(rawData[i].EmploymentType);
                Console.WriteLine(rawData[i].CantidadEmpleados);
            }

            if (rawData == null || !rawData.Any())
            {
                return GenerateNoDataImage();
            }

            var normalizedData = NormalizeEmploymentData(rawData);

            if (normalizedData.All(item => item.CantidadEmpleados == 0))
            {
                return GenerateNoDataImage();
            }

            int width = 900;
            int height = 600;

            using Bitmap bmp = new(width, height);
            using Graphics g = Graphics.FromImage(bmp);

            g.Clear(Color.White);
            g.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;
            g.TextRenderingHint = System.Drawing.Text.TextRenderingHint.AntiAlias;

            Font titleFont = new Font("Arial", 16, FontStyle.Bold);
            Font valueFont = new Font("Arial", 12, FontStyle.Bold);
            Font labelFont = new Font("Arial", 10);

            g.DrawString("Distribución de Empleados por Tipo", titleFont, Brushes.Black, 50, 20);

            Brush[] colors = { Brushes.SteelBlue, Brushes.MediumSeaGreen, Brushes.Goldenrod };

            int barWidth = 100;
            int spacing = 60;
            int startX = 120;
            int startY = 100;
            int chartHeight = 400;

            int maxValue = normalizedData.Max(i => i.CantidadEmpleados);
            float scale = chartHeight / (float)maxValue;

            int x = startX;

            for (int i = 0; i < normalizedData.Count; i++)
            {
                var item = normalizedData[i];
                var color = colors[i % colors.Length];

                float barHeight = (float)item.CantidadEmpleados * scale;
                int barY = startY + chartHeight - (int)barHeight; 

                g.FillRectangle(color, x, barY, barWidth, barHeight);
                g.DrawRectangle(Pens.Black, x, barY, barWidth, barHeight);

                string countText = item.CantidadEmpleados.ToString();
                g.DrawString(countText, valueFont, Brushes.Black, x + 10, barY - 25);

                string typeName = item.EmploymentType;
                g.DrawString(typeName, labelFont, Brushes.Black, x, startY + chartHeight + 10);

                x += barWidth + spacing;
            }
            using MemoryStream ms = new MemoryStream();
            bmp.Save(ms, System.Drawing.Imaging.ImageFormat.Png);
            return ms.ToArray();
        }

        private byte[] GenerateNoDataImage()
        {
            int width = 600;
            int height = 400;

            using Bitmap bmp = new(width, height);
            using Graphics g = Graphics.FromImage(bmp);

            g.Clear(Color.White);
            g.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;
            g.TextRenderingHint = System.Drawing.Text.TextRenderingHint.AntiAlias;

            Font titleFont = new Font("Arial", 18, FontStyle.Bold);
            Font messageFont = new Font("Arial", 12);

            Brush textBrush = Brushes.DarkGray;
            Brush iconBrush = Brushes.LightGray;

            g.FillEllipse(iconBrush, width / 2 - 60, 80, 120, 120);
            g.DrawEllipse(Pens.Gray, width / 2 - 60, 80, 120, 120);

            g.DrawLine(new Pen(Color.Gray, 2), width / 2 - 40, 140, width / 2 - 10, 100);
            g.DrawLine(new Pen(Color.Gray, 2), width / 2 - 10, 100, width / 2 + 20, 160);
            g.DrawLine(new Pen(Color.Gray, 2), width / 2 + 20, 160, width / 2 + 40, 120);

            string title = "No hay datos disponibles";
            string message = "No se encontraron registros de empleados para generar el gráfico.";

            SizeF titleSize = g.MeasureString(title, titleFont);
            SizeF messageSize = g.MeasureString(message, messageFont);

            g.DrawString(title, titleFont, textBrush,
                        width / 2 - titleSize.Width / 2,
                        220);

            g.DrawString(message, messageFont, textBrush,
                        width / 2 - messageSize.Width / 2,
                        260);

            using MemoryStream ms = new MemoryStream();
            bmp.Save(ms, System.Drawing.Imaging.ImageFormat.Png);
            return ms.ToArray();
        }

        private List<EmploymentTypeCountModel> NormalizeEmploymentData(List<EmploymentTypeCountModel> rawData)
        {

            if (rawData == null)
                return new List<EmploymentTypeCountModel>();


            var allTypes = new List<string> { "Tiempo Completo", "Por Horas", "Medio Tiempo" };

            var normalizedList = new List<EmploymentTypeCountModel>();

            foreach (var type in allTypes)
            {
                var existingItem = rawData.FirstOrDefault(item =>
                    item.EmploymentType != null &&
                    item.EmploymentType.Equals(type, StringComparison.OrdinalIgnoreCase));

                if (existingItem != null)
                {
                    normalizedList.Add(existingItem);
                }
                else
                {

                    normalizedList.Add(new EmploymentTypeCountModel
                    {
                        EmploymentType = type,
                        CantidadEmpleados = 0
                    });
                }
            }

            return normalizedList;
        }

        public List<DateTime>? GetUltimasFechasPago(long cedulaJuridica, DateTime fechaLimite)
        {
            return this._empresaRepository.GetUltimasFechasPago(cedulaJuridica, fechaLimite);
        }

        public decimal GetPlanillaCosto(long id, DateTime fecha)
        {
            decimal result = 0;

            result += this._empresaRepository.getEmployerDeductions(id, fecha);
            result += this._empresaRepository.GetBeneficiosEmpresa(id);
            result += this._empresaRepository.GetTotalSalarios(id);

            return result;
        }

        public byte[] CreateChart(long id, DateTime fecha)
        {
            try
            {


                decimal deducciones = _empresaRepository.getEmployerDeductions(id, fecha);
                decimal beneficios = _empresaRepository.GetBeneficiosEmpresa(id);
                decimal salarios = _empresaRepository.GetTotalSalarios(id);
                decimal total = deducciones + beneficios + salarios;

                
                if (total == 0)
                {
                    return GenerateNoDataPieChart();
                }

                
                double porcentajeDeducciones = (double)(deducciones * 100 / total);
                double porcentajeBeneficios = (double)(beneficios * 100 / total);
                double porcentajeSalarios = (double)(salarios * 100 / total);

                
                
                int width = 800;
                int height = 600;

                using Bitmap bmp = new(width, height);
                using Graphics g = Graphics.FromImage(bmp);

                g.Clear(Color.White);
                g.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;
                g.TextRenderingHint = System.Drawing.Text.TextRenderingHint.AntiAlias;

                
                Font titleFont = new Font("Segoe UI", 16, FontStyle.Bold);
                Font labelFont = new Font("Segoe UI", 10, FontStyle.Regular);
                Font percentageFont = new Font("Segoe UI", 9, FontStyle.Bold);

                
                Color[] colors =
                {
                 Color.FromArgb(74, 124, 226),   
                 Color.FromArgb(46, 196, 182),   
                 Color.FromArgb(249, 168, 37)    
                };

                string[] labels =
                {
                "Deducciones",
                "Beneficios",
                 "Salarios"
                };

                double[] percentages = { porcentajeDeducciones, porcentajeBeneficios, porcentajeSalarios };
                decimal[] values = { deducciones, beneficios, salarios };

                
                g.DrawString("Distribución de Costos de Planilla", titleFont, Brushes.Black, 50, 20);

                
                int pieDiameter = 300;
                int pieX = 100;
                int pieY = 100;

               
                float startAngle = 0;

                for (int i = 0; i < percentages.Length; i++)
                {
                    float sweepAngle = (float)(percentages[i] * 3.6); 

                    using (var brush = new SolidBrush(colors[i]))
                    {
                        g.FillPie(brush, pieX, pieY, pieDiameter, pieDiameter, startAngle, sweepAngle);
                    }

                    
                    g.DrawPie(Pens.Black, pieX, pieY, pieDiameter, pieDiameter, startAngle, sweepAngle);

                    startAngle += sweepAngle;
                }

                
                int legendX = pieX + pieDiameter + 50;
                int legendY = pieY;

                g.DrawString("Desglose:", labelFont, Brushes.Black, legendX, legendY);

                for (int i = 0; i < labels.Length; i++)
                {
                    int itemY = legendY + 30 + (i * 60);

                    g.FillRectangle(new SolidBrush(colors[i]), legendX, itemY, 20, 20);
                    g.DrawRectangle(Pens.Black, legendX, itemY, 20, 20);

                    
                    string legendText = $"{labels[i]}: {percentages[i]:0.0}%";
                    g.DrawString(legendText, labelFont, Brushes.Black, legendX + 30, itemY);

                    
                    string valueText = $"¢{values[i]:N2}";
                    g.DrawString(valueText, labelFont, Brushes.DarkGray, legendX + 30, itemY + 18);
                }

                
                string totalText = $"Total: ¢{total:N2}";
                SizeF totalSize = g.MeasureString(totalText, titleFont);
                g.DrawString(totalText, titleFont, Brushes.DarkBlue,
                            pieX + (pieDiameter - totalSize.Width) / 2,
                            pieY + pieDiameter + 20);

                using MemoryStream ms = new MemoryStream();
                bmp.Save(ms, System.Drawing.Imaging.ImageFormat.Png);

                Console.WriteLine("Gráfico de pastel generado exitosamente");
                return ms.ToArray();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error generando gráfico de pastel: {ex.Message}");
                return GenerateErrorPieChart(ex.Message);
            }
        }

        private byte[] GenerateNoDataPieChart()
        {
            int width = 600;
            int height = 400;

            using Bitmap bmp = new(width, height);
            using Graphics g = Graphics.FromImage(bmp);

            g.Clear(Color.White);

            Font titleFont = new Font("Segoe UI", 16, FontStyle.Bold);
            Font messageFont = new Font("Segoe UI", 12);

            g.DrawString("No hay datos de costos", titleFont, Brushes.Gray, 50, 50);
            g.DrawString("No se encontraron datos para generar el gráfico", messageFont, Brushes.DarkGray, 50, 90);

            g.DrawEllipse(new Pen(Color.LightGray, 2), 200, 150, 200, 200);

            using MemoryStream ms = new MemoryStream();
            bmp.Save(ms, System.Drawing.Imaging.ImageFormat.Png);
            return ms.ToArray();
        }

        private byte[] GenerateErrorPieChart(string errorMessage)
        {
            int width = 600;
            int height = 400;

            using Bitmap bmp = new(width, height);
            using Graphics g = Graphics.FromImage(bmp);

            g.Clear(Color.FromArgb(255, 240, 240));

            Font titleFont = new Font("Segoe UI", 16, FontStyle.Bold);
            Font messageFont = new Font("Segoe UI", 11);

            g.DrawString("Error en Gráfico de Pastel", titleFont, Brushes.DarkRed, 50, 50);
            g.DrawString(errorMessage, messageFont, Brushes.DarkRed, 50, 90);

            using MemoryStream ms = new MemoryStream();
            bmp.Save(ms, System.Drawing.Imaging.ImageFormat.Png);
            return ms.ToArray();
        }
    }
}