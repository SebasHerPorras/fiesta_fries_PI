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
            var result = string.Empty;

            try
            {
                Console.WriteLine("=== EMPRESA SERVICE ===");
                Console.WriteLine($"Recibiendo empresa - DueñoEmpresa: {empresa.DueñoEmpresa}");
                Console.WriteLine($"Cédula: {empresa.CedulaJuridica}");

                var isCreated = _empresaRepository.CreateEmpresa(empresa);

                if (!isCreated)
                {
                    result = "Error al crear la empresa en el repositorio";
                    Console.WriteLine($"Error: {result}");
                }
                else
                {
                    Console.WriteLine("Empresa creada exitosamente en el repositorio");
                }
            }
            catch (Exception ex)
            {
                result = $"Error creando empresa: {ex.Message}";
                Console.WriteLine($"Excepción en service: {ex.Message}");
                Console.WriteLine($"Stack trace: {ex.StackTrace}");
            }

            return result;
        }

        public List<EmpresaModel> GetEmpresas()
        {
            return _empresaRepository.GetEmpresas();
        }
    }
}