using backend.Models.Payroll;
using iText.Kernel.Pdf;
using iText.Layout;
using iText.Layout.Element;
using iText.Layout.Properties;
using iText.Kernel.Font;
using iText.IO.Font.Constants;
using Microsoft.Extensions.Logging;

namespace backend.Services
{
    public class PayrollPdfService
    {
        private readonly ILogger<PayrollPdfService> _logger;

        public PayrollPdfService(ILogger<PayrollPdfService> logger)
        {
            _logger = logger;
        }

        public Task<byte[]> GeneratePayrollPdfAsync(PayrollFullReport report)
        {
            try
            {
                _logger.LogInformation("?? INICIO - Generando PDF para planilla {PayrollId}", report?.Header?.PayrollId);

                // VALIDAR REPORT
                if (report == null)
                {
                    _logger.LogError("? ERROR: Report es NULL");
                    throw new ArgumentNullException(nameof(report), "Report no puede ser null");
                }

                if (report.Header == null)
                {
                    _logger.LogError("? ERROR: Report.Header es NULL");
                    throw new ArgumentNullException(nameof(report.Header), "Header no puede ser null");
                }

                _logger.LogInformation("? Report válido - PayrollId: {PayrollId}, Empresa: {Empresa}", 
                    report.Header.PayrollId, report.Header.NombreEmpresa);

                var ms = new MemoryStream();
                _logger.LogInformation("? MemoryStream creado");
                
                try
                {
                    _logger.LogInformation("?? Creando PdfWriter...");
                    var writer = new PdfWriter(ms);
                    writer.SetCloseStream(false);
                    _logger.LogInformation("? PdfWriter creado");

                    _logger.LogInformation("?? Creando PdfDocument...");
                    var pdf = new PdfDocument(writer);
                    _logger.LogInformation("? PdfDocument creado");

                    _logger.LogInformation("?? Creando Document...");
                    var document = new Document(pdf);
                    _logger.LogInformation("? Document creado");

                    // Usar fuente con soporte Unicode
                    _logger.LogInformation("?? Creando fuentes...");
                    var boldFont = PdfFontFactory.CreateFont(StandardFonts.HELVETICA_BOLD);
                    var normalFont = PdfFontFactory.CreateFont(StandardFonts.HELVETICA);
                    _logger.LogInformation("? Fuentes creadas");

                    // ENCABEZADO
                    _logger.LogInformation("?? Agregando encabezado...");
                    document.Add(new Paragraph("REPORTE DE PLANILLA")
                        .SetFont(boldFont)
                        .SetFontSize(20)
                        .SetTextAlignment(TextAlignment.CENTER));

                    document.Add(new Paragraph($"Empresa: {report.Header.NombreEmpresa ?? "N/A"}")
                        .SetFontSize(14)
                        .SetFont(normalFont));
                    document.Add(new Paragraph($"Empleador: {report.Header.NombreEmpleador ?? "N/A"}")
                        .SetFontSize(12)
                        .SetFont(normalFont));
                    document.Add(new Paragraph($"Período: {report.Header.PeriodDate:yyyy-MM-dd}")
                        .SetFontSize(12)
                        .SetFont(normalFont));
                    document.Add(new Paragraph($"Frecuencia: {report.Header.FrecuenciaPago ?? "N/A"} (Día {report.Header.DiaPago})")
                        .SetFontSize(12)
                        .SetFont(normalFont));

                    document.Add(new Paragraph("\n"));
                    _logger.LogInformation("? Encabezado agregado");

                    // TABLA DE RESUMEN
                    _logger.LogInformation("?? Creando tabla de resumen...");
                    var summaryTable = new Table(2);
                    summaryTable.AddHeaderCell(new Cell().Add(new Paragraph("Concepto").SetFont(boldFont)));
                    summaryTable.AddHeaderCell(new Cell().Add(new Paragraph("Monto (CRC)").SetFont(boldFont)));

                    AddTableRow(summaryTable, "Salario Bruto Total", report.Header.TotalGrossSalary, normalFont);
                    AddTableRow(summaryTable, "Deducciones Empleado", report.Header.TotalEmployeeDeductions, normalFont);
                    AddTableRow(summaryTable, "Deducciones Empleador", report.Header.TotalEmployerDeductions, normalFont);
                    AddTableRow(summaryTable, "Beneficios", report.Header.TotalBenefits, normalFont);
                    AddTableRow(summaryTable, "Salario Neto", report.Header.TotalNetSalary, normalFont);
                    AddTableRow(summaryTable, "COSTO TOTAL EMPLEADOR", report.Header.TotalEmployerCost, boldFont);

                    document.Add(summaryTable);
                    document.Add(new Paragraph("\n"));
                    _logger.LogInformation("? Tabla de resumen agregada");

                    // DEDUCCIONES EMPLEADOR
                    _logger.LogInformation("?? Procesando deducciones empleador ({Count} registros)...", report.EmployerCharges?.Count ?? 0);
                    if (report.EmployerCharges != null && report.EmployerCharges.Any())
                    {
                        document.Add(new Paragraph("CARGAS PATRONALES")
                            .SetFont(boldFont)
                            .SetFontSize(14));

                        var chargesTable = new Table(3);
                        chargesTable.AddHeaderCell(new Cell().Add(new Paragraph("Concepto").SetFont(boldFont)));
                        chargesTable.AddHeaderCell(new Cell().Add(new Paragraph("Porcentaje").SetFont(boldFont)));
                        chargesTable.AddHeaderCell(new Cell().Add(new Paragraph("Monto (CRC)").SetFont(boldFont)));

                        foreach (var charge in report.EmployerCharges)
                        {
                            chargesTable.AddCell(new Cell().Add(new Paragraph(charge.ChargeName ?? "N/A").SetFont(normalFont)));
                            chargesTable.AddCell(new Cell().Add(new Paragraph($"{charge.PercentageDisplay:N2}%").SetFont(normalFont)));
                            chargesTable.AddCell(new Cell().Add(new Paragraph($"{charge.TotalAmount:N2}").SetFont(normalFont)));
                        }

                        document.Add(chargesTable);
                        document.Add(new Paragraph("\n"));
                        _logger.LogInformation("? Deducciones empleador agregadas");
                    }
                    else
                    {
                        _logger.LogWarning("?? No hay deducciones de empleador");
                    }

                    // DEDUCCIONES EMPLEADO
                    _logger.LogInformation("?? Procesando deducciones empleado ({Count} registros)...", report.EmployeeDeductions?.Count ?? 0);
                    if (report.EmployeeDeductions != null && report.EmployeeDeductions.Any())
                    {
                        document.Add(new Paragraph("DEDUCCIONES EMPLEADO")
                            .SetFont(boldFont)
                            .SetFontSize(14));

                        var deductionsTable = new Table(3);
                        deductionsTable.AddHeaderCell(new Cell().Add(new Paragraph("Concepto").SetFont(boldFont)));
                        deductionsTable.AddHeaderCell(new Cell().Add(new Paragraph("Porcentaje").SetFont(boldFont)));
                        deductionsTable.AddHeaderCell(new Cell().Add(new Paragraph("Monto (CRC)").SetFont(boldFont)));

                        foreach (var deduction in report.EmployeeDeductions)
                        {
                            deductionsTable.AddCell(new Cell().Add(new Paragraph(deduction.DeductionName ?? "N/A").SetFont(normalFont)));
                            deductionsTable.AddCell(new Cell().Add(new Paragraph($"{deduction.PercentageDisplay:N2}%").SetFont(normalFont)));
                            deductionsTable.AddCell(new Cell().Add(new Paragraph($"{deduction.TotalAmount:N2}").SetFont(normalFont)));
                        }

                        document.Add(deductionsTable);
                        document.Add(new Paragraph("\n"));
                        _logger.LogInformation("? Deducciones empleado agregadas");
                    }
                    else
                    {
                        _logger.LogWarning("?? No hay deducciones de empleado");
                    }

                    // BENEFICIOS
                    _logger.LogInformation("?? Procesando beneficios ({Count} registros)...", report.Benefits?.Count ?? 0);
                    if (report.Benefits != null && report.Benefits.Any())
                    {
                        document.Add(new Paragraph("BENEFICIOS")
                            .SetFont(boldFont)
                            .SetFontSize(14));

                        var benefitsTable = new Table(3);
                        benefitsTable.AddHeaderCell(new Cell().Add(new Paragraph("Nombre").SetFont(boldFont)));
                        benefitsTable.AddHeaderCell(new Cell().Add(new Paragraph("Tipo").SetFont(boldFont)));
                        benefitsTable.AddHeaderCell(new Cell().Add(new Paragraph("Monto (CRC)").SetFont(boldFont)));

                        foreach (var benefit in report.Benefits)
                        {
                            benefitsTable.AddCell(new Cell().Add(new Paragraph(benefit.BenefitName ?? "N/A").SetFont(normalFont)));
                            benefitsTable.AddCell(new Cell().Add(new Paragraph(benefit.BenefitType ?? "N/A").SetFont(normalFont)));
                            benefitsTable.AddCell(new Cell().Add(new Paragraph($"{benefit.TotalAmount:N2}").SetFont(normalFont)));
                        }

                        document.Add(benefitsTable);
                        _logger.LogInformation("? Beneficios agregados");
                    }
                    else
                    {
                        _logger.LogWarning("?? No hay beneficios");
                    }

                    _logger.LogInformation("?? Cerrando documento...");
                    document.Close();
                    _logger.LogInformation("? Documento cerrado");

                    var bytes = ms.ToArray();
                    _logger.LogInformation("? PDF generado exitosamente - Planilla: {PayrollId}, Tamaño: {Size} bytes", 
                        report.Header.PayrollId, bytes.Length);

                    return Task.FromResult(bytes);
                }
                finally
                {
                    _logger.LogInformation("?? Limpiando MemoryStream...");
                    ms.Dispose();
                    _logger.LogInformation("? MemoryStream disposed");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "? ERROR CRÍTICO generando PDF para planilla {PayrollId}. Tipo: {ExceptionType}, Mensaje: {Message}", 
                    report?.Header?.PayrollId ?? 0, 
                    ex.GetType().Name,
                    ex.Message);
                
                if (ex.InnerException != null)
                {
                    _logger.LogError("? Inner Exception: {InnerType} - {InnerMessage}", 
                        ex.InnerException.GetType().Name,
                        ex.InnerException.Message);
                }

                throw;
            }
        }

        public Task<byte[]> GeneratePayrollEmployeePdfAsync(PayrollEmployeeReport report)
        {
            try
            {
                _logger.LogInformation("Inicio - Generando PDF reporte por empleado - Payroll: {PayrollId}, Employee: {EmployeeId}",
                    report?.Header?.CompanyId, report?.Header?.EmployeeId);

                if (report == null) throw new ArgumentNullException(nameof(report));
                if (report.Header == null) throw new ArgumentNullException(nameof(report.Header));

                var ms = new MemoryStream();

                try
                {
                    var writer = new PdfWriter(ms);
                    writer.SetCloseStream(false);
                    var pdf = new PdfDocument(writer);
                    var document = new Document(pdf);

                    var boldFont = PdfFontFactory.CreateFont(StandardFonts.HELVETICA_BOLD);
                    var normalFont = PdfFontFactory.CreateFont(StandardFonts.HELVETICA);

                    document.Add(new Paragraph("REPORTE POR EMPLEADO")
                        .SetFont(boldFont)
                        .SetFontSize(18)
                        .SetTextAlignment(TextAlignment.CENTER));

                    document.Add(new Paragraph($"Empresa: {report.Header.NombreEmpresa ?? "N/A"}").SetFont(normalFont));
                    document.Add(new Paragraph($"Empleado: {report.Header.NombreEmpleado ?? "N/A"}").SetFont(normalFont));
                    document.Add(new Paragraph($"Tipo: {report.Header.TipoEmpleado ?? "N/A"}").SetFont(normalFont));
                    document.Add(new Paragraph($"Fecha Pago: {report.Header.FechaPago:yyyy-MM-dd}").SetFont(normalFont));
                    document.Add(new Paragraph($"Salario Bruto: CRC {report.Header.SalarioBruto:N2}").SetFont(normalFont));
                    document.Add(new Paragraph("\n"));

                    // Deducciones
                    if (report.EmployeeDeductions != null && report.EmployeeDeductions.Any())
                    {
                        document.Add(new Paragraph("DEDUCCIONES OBLIGATORIAS").SetFont(boldFont).SetFontSize(14));
                        var dedTable = new Table(3);
                        dedTable.AddHeaderCell(new Cell().Add(new Paragraph("Concepto").SetFont(boldFont)));
                        dedTable.AddHeaderCell(new Cell().Add(new Paragraph("Porcentaje").SetFont(boldFont)));
                        dedTable.AddHeaderCell(new Cell().Add(new Paragraph("Monto (CRC)").SetFont(boldFont)));

                        foreach (var d in report.EmployeeDeductions)
                        {
                            dedTable.AddCell(new Cell().Add(new Paragraph(d.DeductionName ?? "N/A").SetFont(normalFont)));
                            dedTable.AddCell(new Cell().Add(new Paragraph($"{d.Percentage:N2}%").SetFont(normalFont)));
                            dedTable.AddCell(new Cell().Add(new Paragraph($"{d.DeductionAmount:N2}").SetFont(normalFont)));
                        }

                        document.Add(dedTable);
                        document.Add(new Paragraph($"\nTotal deducciones obligatorias: CRC {report.TotalEmployeeDeductions:N2}").SetFont(boldFont));
                        document.Add(new Paragraph("\n"));
                    }

                    // Beneficios
                    if (report.EmployerBenefits != null && report.EmployerBenefits.Any())
                    {
                        document.Add(new Paragraph("BENEFICIOS / DEDUCCIONES VOLUNTARIAS").SetFont(boldFont).SetFontSize(14));
                        var benTable = new Table(3);
                        benTable.AddHeaderCell(new Cell().Add(new Paragraph("Nombre").SetFont(boldFont)));
                        benTable.AddHeaderCell(new Cell().Add(new Paragraph("Tipo").SetFont(boldFont)));
                        benTable.AddHeaderCell(new Cell().Add(new Paragraph("Monto (CRC)").SetFont(boldFont)));

                        foreach (var b in report.EmployerBenefits)
                        {
                            benTable.AddCell(new Cell().Add(new Paragraph(b.BenefitName ?? "N/A").SetFont(normalFont)));
                            benTable.AddCell(new Cell().Add(new Paragraph(b.BenefitType ?? "N/A").SetFont(normalFont)));
                            benTable.AddCell(new Cell().Add(new Paragraph($"{b.BenefitAmount:N2}").SetFont(normalFont)));
                        }

                        document.Add(benTable);
                        document.Add(new Paragraph($"\nTotal beneficios voluntarios: CRC {report.TotalEmployerBenefits:N2}").SetFont(boldFont));
                        document.Add(new Paragraph("\n"));
                    }

                    // Totales finales
                    document.Add(new Paragraph("TOTALES FINALES").SetFont(boldFont).SetFontSize(14));
                    var totalsTable = new Table(2);
                    totalsTable.AddCell(new Cell().Add(new Paragraph("Total Deducciones Obligatorias").SetFont(normalFont)));
                    totalsTable.AddCell(new Cell().Add(new Paragraph($"{report.Totals.TotalDeduccionesObligatorias:N2}").SetFont(normalFont)));
                    totalsTable.AddCell(new Cell().Add(new Paragraph("Total Beneficios Voluntarios").SetFont(normalFont)));
                    totalsTable.AddCell(new Cell().Add(new Paragraph($"{report.Totals.TotalBeneficiosVoluntarios:N2}").SetFont(normalFont)));
                    totalsTable.AddCell(new Cell().Add(new Paragraph("Total Deducciones").SetFont(normalFont)));
                    totalsTable.AddCell(new Cell().Add(new Paragraph($"{report.Totals.TotalDeducciones:N2}").SetFont(normalFont)));
                    totalsTable.AddCell(new Cell().Add(new Paragraph("Pago Neto").SetFont(boldFont)));
                    totalsTable.AddCell(new Cell().Add(new Paragraph($"CRC {report.Totals.PagoNeto:N2}").SetFont(boldFont)));

                    document.Add(totalsTable);

                    document.Close();

                    var bytes = ms.ToArray();
                    return Task.FromResult(bytes);
                }
                finally
                {
                    ms.Dispose();
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error generando PDF reporte por empleado");
                throw;
            }
        }

        private void AddTableRow(Table table, string label, decimal value, PdfFont font)
        {
            try
            {
                var cell1 = new Cell().Add(new Paragraph(label ?? "N/A").SetFont(font));
                var cell2 = new Cell().Add(new Paragraph($"CRC {value:N2}").SetFont(font));

                table.AddCell(cell1);
                table.AddCell(cell2);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "? Error agregando fila a tabla: {Label}, {Value}", label, value);
                throw;
            }
        }
    }
}