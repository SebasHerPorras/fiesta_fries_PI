using backend.Models;
using backend.Interfaces;
using System.Text.Json;

namespace backend.Services
{
    public class PrivateInsuranceService : IPrivateInsuranceService
    {
        private readonly HttpClient _httpClient;
        private readonly string _baseUrl;
        private readonly string _token;

        public PrivateInsuranceService(HttpClient httpClient, IConfiguration configuration)
        {
            _httpClient = httpClient;
            _baseUrl = configuration["ExternalApis:SeguroPrivado:BaseUrl"]
                ?? "https://external-api-seguro-medico-dxfuffcjajcuf7cg.southcentralus-01.azurewebsites.net";
            _token = configuration["ExternalApis:SeguroPrivado:Token"] ?? "12345";
            
            // Configurar authorization header
            if (!_httpClient.DefaultRequestHeaders.Contains("Authorization"))
            {
                _httpClient.DefaultRequestHeaders.Add("Authorization", _token);
            }
        }

        public async Task<ExternalApiResponse> CalculatePremiumAsync(PrivateInsuranceRequest request)
        {
            try
            {
                var edad = request.Age > 0 ? request.Age : CalcularEdad(request.BirthDate);
                var url = $"{_baseUrl}/SeguroPrivado/seguro-privado?edad={edad}&dependientes={request.DependentsCount}";
                
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
                Console.WriteLine($"HTTP Error calling PrivateInsurance API: {ex.Message}");
                return new ExternalApiResponse();
            }
            catch (JsonException ex)
            {
                Console.WriteLine($"JSON Error parsing PrivateInsurance response: {ex.Message}");
                return new ExternalApiResponse();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"General Error calling PrivateInsurance API: {ex.Message}");
                return new ExternalApiResponse();
            }
        }

        private int CalcularEdad(DateTime fechaNacimiento)
        {
            var today = DateTime.Today;
            var age = today.Year - fechaNacimiento.Year;
            if (fechaNacimiento.Date > today.AddYears(-age))
                age--;
            return age;
        }
    }
}