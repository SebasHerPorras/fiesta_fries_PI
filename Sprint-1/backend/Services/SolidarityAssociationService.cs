using backend.Interfaces;
using backend.Models;
using System.Globalization;
using System.Text.Json;

namespace backend.Services
{
    public class SolidarityAssociationService : ISolidarityAssociationService
    {
        private readonly HttpClient _httpClient;
        private readonly string _baseUrl;
        private readonly string _token;

        public SolidarityAssociationService(HttpClient httpClient, IConfiguration configuration)
        {
            _httpClient = httpClient;
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

                Console.WriteLine("=== DIAGNÓSTICO API ASOCIACIÓN SOLIDARISTA ===");
                Console.WriteLine("URL: " + url);
                Console.WriteLine("Salario original: " + request.GrossSalary);
                Console.WriteLine("Salario formateado: " + formattedSalary);

                var response = await _httpClient.GetAsync(url);
                response.EnsureSuccessStatusCode();

                var jsonContent = await response.Content.ReadAsStringAsync();
                Console.WriteLine("RESPUESTA CRUDA: " + jsonContent);

                try
                {
                    using JsonDocument doc = JsonDocument.Parse(jsonContent);
                    var root = doc.RootElement;
                    Console.WriteLine("ESTRUCTURA JSON:");
                    Console.WriteLine(JsonSerializer.Serialize(root, new JsonSerializerOptions { WriteIndented = true }));
                }
                catch (Exception jsonEx)
                {
                    Console.WriteLine("Error parseando JSON: " + jsonEx.Message);
                }

                var apiResponse = JsonSerializer.Deserialize<ExternalApiResponse>(jsonContent, new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                });

                Console.WriteLine("API Response después de deserializar:");
                Console.WriteLine("Deductions count: " + (apiResponse?.Deductions?.Count ?? 0));

                if (apiResponse?.Deductions != null)
                {
                    foreach (var deduction in apiResponse.Deductions)
                    {
                        Console.WriteLine("Deducción - Tipo: " + deduction.Type + ", Monto: " + deduction.Amount);
                    }
                }

                if (apiResponse?.Deductions != null)
                {
                    foreach (var deduction in apiResponse.Deductions)
                    {
                        if (deduction.Amount > request.GrossSalary)
                        {
                            Console.WriteLine("VALOR ANORMAL: " + deduction.Amount + " > salario " + request.GrossSalary);
                            Console.WriteLine("Calculando valor conservador (1% del salario)");
                            deduction.Amount = Math.Round(request.GrossSalary * 0.01m, 2);
                            Console.WriteLine("Nuevo valor: " + deduction.Amount);
                        }
                    }
                }

                return apiResponse ?? new ExternalApiResponse();
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error en API Asociación Solidarista: " + ex.Message);
                return new ExternalApiResponse();
            }
        }
    }
}
