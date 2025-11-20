using backend.Interfaces;
using backend.Models;
using System.Globalization;
using System.Text.Json;
using Microsoft.Extensions.Logging;

namespace backend.Services
{
    public class SolidarityAssociationService : ISolidarityAssociationService
    {
        private readonly HttpClient _httpClient;
        private readonly string _baseUrl;
        private readonly string _token;
        private readonly ILogger<SolidarityAssociationService> _logger;

        public SolidarityAssociationService(HttpClient httpClient, IConfiguration configuration, ILogger<SolidarityAssociationService> logger)
        {
            _httpClient = httpClient;
            _logger = logger;
            _baseUrl = configuration["ExternalApis:AsociacionSolidarista:BaseUrl"]
                ?? "https://external-api-asociacion-f2gbh5crcgezgucr.southcentralus-01.azurewebsites.net";
            _token = configuration["ExternalApis:AsociacionSolidarista:Token"] ?? "ImparablesToken2025";
            
            // Configurar authorization header como Bearer token
            if (!_httpClient.DefaultRequestHeaders.Contains("Authorization"))
            {
                _httpClient.DefaultRequestHeaders.Add("Authorization", $"Bearer {_token}");
            }
            
            // Configurar Accept header
            if (!_httpClient.DefaultRequestHeaders.Contains("Accept"))
            {
                _httpClient.DefaultRequestHeaders.Add("Accept", "text/plain");
            }
        }

        public async Task<ExternalApiResponse> CalculateContributionAsync(SolidarityAssociationRequest request)
        {
            try
            {
                var formattedSalary = request.GrossSalary.ToString("F2", CultureInfo.InvariantCulture);

                var url = $"{_baseUrl}/api/asociacionsolidarista/aporte-empleado?cedulaEmpresa={request.CompanyLegalId}&salarioBruto={formattedSalary}";

                _logger.LogInformation("=== DIAGNÓSTICO API ASOCIACIÓN SOLIDARISTA ===");
                _logger.LogInformation("URL: {Url}", url);
                _logger.LogInformation("Salario original: {GrossSalary}, Salario formateado: {FormattedSalary}", request.GrossSalary, formattedSalary);

                var response = await _httpClient.GetAsync(url);
                response.EnsureSuccessStatusCode();

                var jsonContent = await response.Content.ReadAsStringAsync();
                C_logger.LogDebug("RESPUESTA CRUDA: {JsonContent}", jsonContent);

                try
                {
                    using JsonDocument doc = JsonDocument.Parse(jsonContent);
                    var root = doc.RootElement;
                    _logger.LogDebug("ESTRUCTURA JSON: {JsonStructure}", JsonSerializer.Serialize(root, new JsonSerializerOptions { WriteIndented = true }));
                }
                catch (Exception jsonEx)
                {
                    logger.LogWarning(jsonEx, "Error parseando JSON para diagnóstico");
                }

                var apiResponse = JsonSerializer.Deserialize<ExternalApiResponse>(jsonContent, new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                });

                _logger.LogDebug("API Response después de deserializar - Deductions count: {DeductionsCount}", apiResponse?.Deductions?.Count ?? 0);

                if (apiResponse?.Deductions != null)
                {
                    foreach (var deduction in apiResponse.Deductions)
                    {
                         _logger.LogDebug("Deducción - Tipo: {DeductionType}, Monto: {DeductionAmount}", deduction.Type, deduction.Amount);
                    }
                }

                if (apiResponse?.Deductions != null)
                {
                    foreach (var deduction in apiResponse.Deductions)
                    {
                        if (deduction.Amount > request.GrossSalary)
                        {
                            _logger.LogWarning(
                                "Valor anormal detectado - Deducción: {DeductionAmount} > Salario: {GrossSalary}. " +
                                "Calculando valor conservador (1% del salario)", deduction.Amount, request.GrossSalary);
                            
                            deduction.Amount = Math.Round(request.GrossSalary * 0.01m, 2);
                            
                            _logger.LogInformation("Nuevo valor de deducción: {NewDeductionAmount}", deduction.Amount);
                        }
                    }
                }

                return apiResponse ?? new ExternalApiResponse();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error en API Asociación Solidarista");
                return new ExternalApiResponse();
            }
        }
    }
}
