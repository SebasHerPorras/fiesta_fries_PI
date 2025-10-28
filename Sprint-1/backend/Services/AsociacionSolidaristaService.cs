using backend.Models;
using backend.Interfaces;
using System.Text.Json;

namespace backend.Services
{
    public class AsociacionSolidaristaService : ISolidarityAssociationService
    {
        private readonly HttpClient _httpClient;
        private readonly string _baseUrl;
        private readonly string _token;

        public AsociacionSolidaristaService(HttpClient httpClient, IConfiguration configuration)
        {
            _httpClient = httpClient;
            _baseUrl = configuration["ExternalApis:AsociacionSolidarista:BaseUrl"]
                ?? "https://external-api-asociacion-f2gbh5crcgezgucr.southcentralus-01.azurewebsites.net";
            
            _token = configuration["ExternalApis:AsociacionSolidarista:Token"] ?? "ImparablesToken2025";
            
            if (!_httpClient.DefaultRequestHeaders.Contains("Authorization"))
            {
                _httpClient.DefaultRequestHeaders.Add("Authorization", $"Bearer {_token}");
            }

            if (!_httpClient.DefaultRequestHeaders.Contains("Accept"))
            {
                _httpClient.DefaultRequestHeaders.Add("Accept", "text/plain");
            }
        }

        public async Task<ExternalApiResponse> CalculateContributionAsync(SolidarityAssociationRequest request)
        {
            try
            {

                var url = $"{_baseUrl}/api/asociacionsolidarista/aporte-empleado?cedulaEmpresa={request.CompanyLegalId}&salarioBruto={request.GrossSalary}";
                
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
                Console.WriteLine($"HTTP Error calling SolidarityAssociation API: {ex.Message}");
                return new ExternalApiResponse();
            }
            catch (JsonException ex)
            {
                Console.WriteLine($"JSON Error parsing SolidarityAssociation response: {ex.Message}");
                return new ExternalApiResponse();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"General Error calling SolidarityAssociation API: {ex.Message}");
                return new ExternalApiResponse();
            }
        }
    }
}