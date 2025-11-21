using backend.Interfaces;
using backend.Models;
using System.Globalization;
using System.Text.Json;
using Microsoft.Extensions.Logging;

namespace backend.Services
{
    public class VoluntaryPensionsService : IVoluntaryPensionsService
    {
        private readonly HttpClient _httpClient;
        private readonly string _baseUrl;
        private readonly ILogger<VoluntaryPensionsService> _logger;

        public VoluntaryPensionsService(HttpClient httpClient, IConfiguration configuration, ILogger<VoluntaryPensionsService> logger)
        {
            _httpClient = httpClient;
            _logger = logger;
            _baseUrl = configuration["ExternalApis:PensionesVoluntarias:BaseUrl"]
                ?? "https://external-api-pension-fpducreydfagbzhc.southcentralus-01.azurewebsites.net";
        }

        public async Task<ExternalApiResponse> CalculatePremiumAsync(VoluntaryPensionsRequest request)
        {
            try
            {
                var formattedSalary = request.GrossSalary.ToString("F2", CultureInfo.InvariantCulture);

                var url = $"{_baseUrl}/?planType={request.PlanType}&grossSalary={formattedSalary}";

                _logger.LogInformation("Llamando API Pensiones: {Url}", url);

                var response = await _httpClient.GetAsync(url);
                response.EnsureSuccessStatusCode();

                var jsonContent = await response.Content.ReadAsStringAsync();
                 _logger.LogDebug("Respuesta API Pensiones: {JsonContent}", jsonContent);

                var apiResponse = JsonSerializer.Deserialize<ExternalApiResponse>(jsonContent, new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                });

                if (apiResponse == null)
                {
                    _logger.LogWarning("La API de Pensiones Voluntarias retornó una respuesta nula");
                    return new ExternalApiResponse();
                }
                _logger.LogDebug("API Pensiones procesada exitosamente - Deducciones: {DeductionsCount}", apiResponse.Deductions?.Count ?? 0);

                return apiResponse;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error llamando API Pensiones Voluntarias");
                return new ExternalApiResponse();
            }
        }
    }
}
