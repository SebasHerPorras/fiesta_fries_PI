using backend.Interfaces;
using backend.Interfaces.Services;
using backend.Interfaces.Strategies;
using backend.Models;
using backend.Models.Common;
using backend.Models.Payroll;
using Microsoft.Extensions.Logging;
using System.Linq;
using System.Collections.Generic;

namespace backend.Services
{
    public class PayrollPeriodService : IPayrollPeriodService
    {
        private readonly IPayrollRepository _payrollRepository;
        private readonly IEmpresaRepository _empresaRepository;
        private readonly ILogger<PayrollPeriodService> _logger;
        private readonly IEnumerable<IPeriodCalculator> _periodCalculators;

        public PayrollPeriodService(
            IPayrollRepository payrollRepository,
            IEmpresaRepository empresaRepository,
            ILogger<PayrollPeriodService> logger,
            IEnumerable<IPeriodCalculator> periodCalculators)
        {
            _payrollRepository = payrollRepository ?? throw new ArgumentNullException(nameof(payrollRepository));
            _empresaRepository = empresaRepository ?? throw new ArgumentNullException(nameof(empresaRepository));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _periodCalculators = periodCalculators ?? throw new ArgumentNullException(nameof(periodCalculators));
        }

        public async Task<PayrollPeriod> CalculateNextPeriodAsync(string companyId)
        {
            _logger.LogInformation("Calculating next payroll period for company: {CompanyId}", companyId);
            ValidateCompanyId(companyId);

            try
            {
                var company = await GetCompanyAsync(companyId);
                var lastPayroll = await GetLastPayrollAsync(companyId);

                var baseDate = CalculateBaseDate(lastPayroll, company);
                var nextPeriod = CalculatePeriodByFrequency(company, baseDate);

                await ValidatePeriodNotInDistantFuture(nextPeriod);
                await MarkProcessedStatusAsync(companyId, new List<PayrollPeriod> { nextPeriod });

                _logger.LogInformation("Next period calculated: {Description}", nextPeriod.Description);
                return nextPeriod;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error calculating next period for company {CompanyId}", companyId);
                throw;
            }
        }

        public async Task<PayrollPeriod> CalculateNextPendingPeriodAsync(string companyId)
        {
            ValidateCompanyId(companyId);

            try
            {
                var company = await GetCompanyAsync(companyId);
                var overduePeriods = await GetOverduePeriodsAsync(companyId);

                if (overduePeriods.Any())
                {
                    var oldestOverdue = overduePeriods.OrderBy(p => p.StartDate).First();
                    _logger.LogInformation("Returning oldest overdue period: {Description}", oldestOverdue.Description);
                    return oldestOverdue;
                }

                _logger.LogInformation("No overdue periods found for company {CompanyId}, calculating next normal period", companyId);
                return await CalculateNextPeriodAsync(companyId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error calculating next pending period for company {CompanyId}", companyId);
                throw;
            }
        }

        public async Task<List<PayrollPeriod>> GetOverduePeriodsAsync(string companyId)
        {
            ValidateCompanyId(companyId);

            try
            {
                var company = await GetCompanyAsync(companyId);
                var searchRange = await GetSearchDateRangeAsync(companyId, company);

                var searchResult = await GeneratePeriodsInRangeAsync(company, searchRange.Start, searchRange.End);
                var processedPeriods = await MarkProcessedStatusAsync(companyId, searchResult.Periods);

                var overduePeriods = FilterOverduePeriods(processedPeriods);

                LogSearchResults(companyId, searchResult, overduePeriods.Count);

                return overduePeriods;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting overdue periods for company {CompanyId}", companyId);
                throw;
            }
        }

        public async Task<List<PayrollPeriod>> GetPendingPeriodsAsync(string companyId, int months = 6)
        {
            ValidateCompanyId(companyId);

            try
            {
                var company = await GetCompanyAsync(companyId);
                var startDate = company.FechaCreacion;
                var endDate = DateTime.Now.AddMonths(months);

                var searchResult = await GeneratePeriodsInRangeAsync(company, startDate, endDate);
                var processedPeriods = await MarkProcessedStatusAsync(companyId, searchResult.Periods);

                return processedPeriods
                    .Where(p => !p.IsProcessed)
                    .OrderBy(p => p.StartDate)
                    .ToList();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting pending periods for company {CompanyId}", companyId);
                throw;
            }
        }


        private async Task<List<PayrollPeriod>> GetAllPeriodsAsync(string companyId, int months = 12)
        {
            ValidateCompanyId(companyId);

            try
            {
                var company = await GetCompanyAsync(companyId);
                var startDate = company.FechaCreacion;
                var endDate = DateTime.Now.AddMonths(months);

                var searchResult = await GeneratePeriodsInRangeAsync(company, startDate, endDate);
                var processedPeriods = await MarkProcessedStatusAsync(companyId, searchResult.Periods);

                // NO filtrar por IsProcessed - devolver todos los períodos
                return processedPeriods
                    .OrderBy(p => p.StartDate)
                    .ToList();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting all periods for company {CompanyId}", companyId);
                throw;
            }
        }

        public async Task<PayrollPeriod> CalculateCurrentPeriodAsync(string companyId)
        {
            ValidateCompanyId(companyId);
            var company = await GetCompanyAsync(companyId);
            return CalculatePeriodByFrequency(company, DateTime.Now);
        }

    
        public async Task<PayrollPeriod?> ResolvePayrollPeriodAsync(string companyId, DateTime periodDate, bool allowProcessed = false)
        {
            ValidateCompanyId(companyId);

            var periods = new List<PayrollPeriod>();

            try
            {
                // Usar GetAllPeriodsAsync para incluir períodos procesados en la búsqueda
                var allPeriods = await GetAllPeriodsAsync(companyId, 12);
                if (allPeriods != null && allPeriods.Any()) periods.AddRange(allPeriods);
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Failed to retrieve all periods for company {CompanyId}", companyId);
            }

            try
            {
                var overdue = await GetOverduePeriodsAsync(companyId);
                if (overdue != null && overdue.Any()) periods.AddRange(overdue);
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Failed to retrieve overdue periods for company {CompanyId}", companyId);
            }

            try
            {
                var current = await CalculateCurrentPeriodAsync(companyId);
                if (current != null) periods.Add(current);
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Failed to calculate current period for company {CompanyId}", companyId);
            }

            try
            {
                var next = await CalculateNextPendingPeriodAsync(companyId);
                if (next != null) periods.Add(next);
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Failed to calculate next pending period for company {CompanyId}", companyId);
            }

            if (!periods.Any())
            {
                _logger.LogInformation("No candidate periods found for company {CompanyId}", companyId);
                return null;
            }

            var deduped = periods
                .GroupBy(p => p.StartDate.Date)
                .Select(g => g.OrderBy(p => p.IsProcessed ? 1 : 0).First())
                .ToList();

            // Búsqueda exacta: Períodos que contengan la fecha especificada
            var matches = deduped
                .Where(p => periodDate.Date >= p.StartDate.Date && periodDate.Date <= p.EndDate.Date)
                .OrderBy(p => p.StartDate)
                .ToList();

            if (matches.Any())
            {
                // Siempre devolver el período que contiene la fecha exacta
                // La validación de si está procesado se hace en ProcessPayrollAsync
                var chosen = matches.First();
                
                if (chosen.IsProcessed && !allowProcessed)
                {
                    _logger.LogWarning("Period {Description} ({Start} - {End}) for date {Date} (company {CompanyId}) is already processed", 
                        chosen.Description, chosen.StartDate.ToString("yyyy-MM-dd"), chosen.EndDate.ToString("yyyy-MM-dd"), 
                        periodDate.ToString("yyyy-MM-dd"), companyId);
                }
                else
                {
                    _logger.LogDebug("Resolved period {Description} ({Start} - {End}) for date {Date} (company {CompanyId})",
                        chosen.Description, chosen.StartDate.ToString("yyyy-MM-dd"), chosen.EndDate.ToString("yyyy-MM-dd"), 
                        periodDate.ToString("yyyy-MM-dd"), companyId);
                }
                
                return chosen;
            }

            // FALLBACK: Solo si NO hay período exacto, buscar alternativas
            _logger.LogWarning("No exact period found containing date {Date} for company {CompanyId}, searching for alternatives", 
                periodDate.ToString("yyyy-MM-dd"), companyId);

            // Si no hay un periodo que contenga la fecha busca el próximo periodo no procesado
            var future = deduped
                .Where(p => p.StartDate.Date > periodDate.Date)
                .Where(p => allowProcessed || !p.IsProcessed)
                .OrderBy(p => p.StartDate)
                .FirstOrDefault();

            if (future != null)
            {
                _logger.LogDebug("Selected next future period {Description} ({Start} - {End}) for date {Date}", future.Description, future.StartDate.ToString("yyyy-MM-dd"), future.EndDate.ToString("yyyy-MM-dd"), periodDate.ToString("yyyy-MM-dd"));
                return future;
            }

            // De lo contrario selecciona el periodo anterior no procesado más cercano
            var previous = deduped
                .Where(p => p.EndDate.Date < periodDate.Date)
                .Where(p => allowProcessed || !p.IsProcessed)
                .OrderByDescending(p => p.StartDate)
                .FirstOrDefault();

            if (previous != null)
            {
                _logger.LogDebug("Selected previous period {Description} ({Start} - {End}) for date {Date}", previous.Description, previous.StartDate.ToString("yyyy-MM-dd"), previous.EndDate.ToString("yyyy-MM-dd"), periodDate.ToString("yyyy-MM-dd"));
                return previous;
            }

            _logger.LogInformation("No matching (respecting allowProcessed={AllowProcessed}) period found for date {Date} in company {CompanyId}", allowProcessed, periodDate.ToString("yyyy-MM-dd"), companyId);
            return null;
        }

        public async Task<bool> IsPeriodProcessedAsync(string companyId, DateTime period)
        {
            var payrolls = await _payrollRepository.GetPayrollsByCompanyAsync(companyId);
            return payrolls?.Any(p => p.PeriodDate.Date == period.Date) == true;
        }

        #region Private Methods

        private void ValidateCompanyId(string companyId)
        {
            if (string.IsNullOrWhiteSpace(companyId))
                throw new ArgumentException("Company ID cannot be null or empty", nameof(companyId));
        }

        private async Task<EmpresaModel> GetCompanyAsync(string companyId)
        {
            long legalId = long.Parse(companyId);
            var company = _empresaRepository.GetByCedulaJuridica(legalId);

            if (company == null)
                throw new ArgumentException($"Company with legal ID {companyId} not found");

            EnsureValidCreationDate(company, companyId);
            LogCompanyDetails(company, companyId);

            return company;
        }

        private void EnsureValidCreationDate(EmpresaModel company, string companyId)
        {
            if (company.FechaCreacion == DateTime.MinValue)
            {
                _logger.LogWarning("Company {CompanyId} has no creation date, using default", companyId);
                company.FechaCreacion = DateTime.Now.AddYears(-1);
            }
        }

        private void LogCompanyDetails(EmpresaModel company, string companyId)
        {
            _logger.LogDebug(
                "Company found: {CompanyName} (Created: {CreationDate}, Frequency: {Frequency}, Payment Day: {Day})",
                company.Nombre, company.FechaCreacion.ToString("yyyy-MM-dd"),
                company.FrecuenciaPago, company.DiaPago);
        }

        private async Task<Payroll> GetLastPayrollAsync(string companyId)
        {
            return await _payrollRepository.GetLatestPayrollAsync(companyId);
        }

        // Decide desde dónde calcular el próximo periodo
        private DateTime CalculateBaseDate(Payroll lastPayroll, EmpresaModel company)
        {
            if (lastPayroll != null)
            {
                _logger.LogDebug("Using last payroll date: {LastPayrollDate}",
                    lastPayroll.PeriodDate.ToString("yyyy-MM-dd"));
                return lastPayroll.PeriodDate;
            }
            var creationDate = company.FechaCreacion;
            var oneYearAgo = DateTime.Now.AddYears(-1);
            var baseDate = creationDate < oneYearAgo ? oneYearAgo : creationDate;

            _logger.LogDebug(
                "No previous payrolls. Using base date: {BaseDate} (Company created: {CreationDate})",
                baseDate.ToString("yyyy-MM-dd"), creationDate.ToString("yyyy-MM-dd"));

            return baseDate;
        }

        private PayrollPeriod CalculatePeriodByFrequency(EmpresaModel company, DateTime baseDate)
        {
            var frecuencia = company.FrecuenciaPago?.ToLower();

            var calculator = _periodCalculators.FirstOrDefault(c => c.CanHandle(frecuencia));

            if (calculator == null)
                throw new ArgumentException($"Unsupported payment frequency: {company.FrecuenciaPago}");

            return calculator.CalculatePeriod(baseDate, company.DiaPago);
        }

        private async Task<DateRange> GetSearchDateRangeAsync(string companyId, EmpresaModel company)
        {
            var lastPayroll = await GetLastPayrollAsync(companyId);
            var startDate = lastPayroll?.PeriodDate ?? company.FechaCreacion;
            var endDate = DateTime.Now.AddMonths(1);

            return new DateRange(startDate, endDate);
        }

        private async Task<PeriodSearchResult> GeneratePeriodsInRangeAsync(
            EmpresaModel company, DateTime startDate, DateTime endDate)
        {
            var periods = new List<PayrollPeriod>();
            var currentDate = startDate;
            var searchLimitReached = false;

            while (currentDate < endDate && !searchLimitReached)
            {
                var period = CalculatePeriodByFrequency(company, currentDate);

                if (!PeriodExists(periods, period))
                {
                    periods.Add(period);
                }

                currentDate = period.EndDate.AddDays(1);
                searchLimitReached = ExceedsMaxSearchDuration(startDate, currentDate);
            }

            return new PeriodSearchResult
            {
                Periods = periods,
                TotalGenerated = periods.Count,
                SearchLimitReached = searchLimitReached
            };
        }

        private bool PeriodExists(List<PayrollPeriod> periods, PayrollPeriod newPeriod)
        {
            return periods.Any(p => p.StartDate == newPeriod.StartDate);
        }

        private bool ExceedsMaxSearchDuration(DateTime startDate, DateTime currentDate)
        {
            return currentDate > startDate.AddYears(2);
        }

        private async Task<List<PayrollPeriod>> MarkProcessedStatusAsync(string companyId, List<PayrollPeriod> periods)
        {
            var tasks = periods.Select(async period =>
            {
                period.IsProcessed = await IsPeriodProcessedAsync(companyId, period.StartDate);
                return period;
            });

            return (await Task.WhenAll(tasks)).ToList();
        }

        private List<PayrollPeriod> FilterOverduePeriods(List<PayrollPeriod> periods)
        {
            var currentDate = DateTime.Now;

            return periods
                .Where(p => p.EndDate < currentDate && !p.IsProcessed)
                .OrderBy(p => p.StartDate)
                .ToList();
        }

        private void LogSearchResults(string companyId, PeriodSearchResult result, int overdueCount)
        {
            _logger.LogInformation(
                "Period search completed for company {CompanyId}. Generated: {Generated}, Overdue: {Overdue}, LimitReached: {LimitReached}",
                companyId, result.TotalGenerated, overdueCount, result.SearchLimitReached);
        }

        private async Task ValidatePeriodNotInDistantFuture(PayrollPeriod period)
        {
            if (period.StartDate > DateTime.Now.AddMonths(2))
            {
                _logger.LogWarning("Calculated period is too far in future: {StartDate}", period.StartDate);
                throw new InvalidOperationException("Cannot process periods more than 2 months in advance");
            }
        }

        #endregion
    }
}