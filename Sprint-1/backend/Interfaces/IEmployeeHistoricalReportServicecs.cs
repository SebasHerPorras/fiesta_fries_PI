using backend.Models.Payroll;

namespace backend.Interfaces
{
    public interface IEmployeeHistoricalReportService
    {
        /// <summary>
        /// Genera el reporte histórico del empleado incluyendo la fila de totales.
        /// </summary>
        /// <param name="employeeId">ID del empleado.</param>
        /// <param name="startDate">Fecha inicial del filtro (opcional).</param>
        /// <param name="endDate">Fecha final del filtro (opcional).</param>
        /// <returns>Lista del reporte histórico del empleado.</returns>
        Task<IEnumerable<EmployeeHistoricalReportDto>> GenerateReportAsync(
            long employeeId,
            DateTime? startDate,
            DateTime? endDate);
    }
}

