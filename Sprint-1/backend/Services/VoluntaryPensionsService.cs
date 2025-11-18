using backend.Interfaces;
using backend.Models;
using System.Globalization;
using System.Text.Json;

namespace backend.Services
{
    public class VoluntaryPensionsService : IVoluntaryPensionsService
    {
        private readonly HttpClient _httpClient;
        private readonly string _baseUrl;

        public VoluntaryPensionsService(HttpClient httpClient, IConfiguration configuration)
        {
            _httpClient = httpClient;
            _baseUrl = configuration["ExternalApis:PensionesVoluntarias:BaseUrl"]
                ?? "https://external-api-pension-fpducreydfagbzhc.southcentralus-01.azurewebsites.net";
        }

        public async Task<ExternalApiResponse> CalculatePremiumAsync(VoluntaryPensionsRequest request)
        {
            try
            {
                var formattedSalary = request.GrossSalary.ToString("F2", CultureInfo.InvariantCulture);

                var url = $"{_baseUrl}/?planType={request.PlanType}&grossSalary={formattedSalary}";

                Console.WriteLine("Llamando API Pensiones: " + url);

                var response = await _httpClient.GetAsync(url);
                response.EnsureSuccessStatusCode();

                var jsonContent = await response.Content.ReadAsStringAsync();
                Console.WriteLine("Respuesta API Pensiones: " + jsonContent);

                var apiResponse = JsonSerializer.Deserialize<ExternalApiResponse>(jsonContent, new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                });

                if (apiResponse == null)
                {
                    return new ExternalApiResponse();
                }

                return apiResponse;
            }
            catch (Exception)
            {
                Console.WriteLine("Error llamando API Pensiones Voluntarias");
                return new ExternalApiResponse();
            }
        }
    }
}