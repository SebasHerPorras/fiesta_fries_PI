using backend.Handlers.backend.Repositories;
using backend.Models;
using backend.Services;

namespace backend.Services
{
    public class EmpresaService
    {
        private readonly EmpresaRepository _empresaRepository;

        public EmpresaService()
        {
            _empresaRepository = new EmpresaRepository();
        }

        public string CreateEmpresa(EmpresaModel empresa)
        {
            try
            {
                Console.WriteLine("=== EMPRESA SERVICE ===");
                Console.WriteLine($"Cédula: {empresa.CedulaJuridica}");
                Console.WriteLine($"DueñoEmpresa: {empresa.DueñoEmpresa}");

                var result = _empresaRepository.CreateEmpresa(empresa);

                if (result == "EMPRESA_CREADA_EXITOSAMENTE")
                {
                    Console.WriteLine("Empresa creada exitosamente");
                    return "La empresa se ha registrado correctamente."; 
                }
                else
                {
                    Console.WriteLine($"Error: {result}");
                    return result; 
                }
            }
            catch (Exception ex)
            {
                var errorMessage = $"Error en service: {ex.Message}";
                Console.WriteLine($"{errorMessage}");
                return errorMessage;
            }
        }

        public List<EmpresaModel> GetEmpresas()
        {
            return _empresaRepository.GetEmpresas();
        }
    }
}