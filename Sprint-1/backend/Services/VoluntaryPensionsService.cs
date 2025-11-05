using backend.Models;
using backend.Interfaces;
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
                var url = $"{_baseUrl}/?planType={request.PlanType}&grossSalary={request.GrossSalary}";
                
                var response = await _httpClient.GetAsync(url);
                response.EnsureSuccessStatusCode();
                
                var jsonContent = await response.Content.ReadAsStringAsync();
                var apiResponse = JsonSerializer.Deserialize<ExternalApiResponse>(jsonContent, new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                });

                return apiResponse ?? new ExternalApiResponse();
            }
            catch (HttpRequestException ex)
            {
                Console.WriteLine($"HTTP Error calling VoluntaryPensions API: {ex.Message}");
                return new ExternalApiResponse();
            }
            catch (JsonException ex)
            {
                Console.WriteLine($"JSON Error parsing VoluntaryPensions response: {ex.Message}");
                return new ExternalApiResponse();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"General Error calling VoluntaryPensions API: {ex.Message}");
                return new ExternalApiResponse();
            }
        }
    }
}