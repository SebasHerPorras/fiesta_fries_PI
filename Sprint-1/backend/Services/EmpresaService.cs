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

                if (string.IsNullOrEmpty(result))
                {
                    Console.WriteLine("SERVICE: Empresa creada exitosamente");
                    return string.Empty; 
                }
                else
                {
                    Console.WriteLine($"SERVICE: Error - {result}");
                    return result; 
                }
            }
            catch (Exception ex)
            {
                var errorMessage = $"Error en service: {ex.Message}";
                Console.WriteLine($"SERVICE EXCEPTION: {errorMessage}");
                return errorMessage;
            }
        }

        public List<EmpresaModel> GetEmpresas()
        {
            return _empresaRepository.GetEmpresas();
        }

        public List<EmpresaModel> GetEmpresasByOwner(int ownerId)
        {
            try
            {
                return _empresaRepository.GetByOwner(ownerId);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[ERROR] Service GetEmpresasByOwner: {ex.Message}");
                return new List<EmpresaModel>();
            }
        }

        public EmpresaModel GetEmpresaById(int id)
        {
            try
            {
                return _empresaRepository.GetById(id);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[ERROR] Service GetEmpresaById: {ex.Message}");
                return null;
            }
        }

        public EmpresaModel GetEmpresaByEmployeeUserId(string userId)
        {
            try
            {
                return _empresaRepository.GetByEmployeeUserId(userId);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[ERROR] Service GetEmpresaByEmployeeUserId: {ex.Message}");
                return null;
            }
        }
    }
}