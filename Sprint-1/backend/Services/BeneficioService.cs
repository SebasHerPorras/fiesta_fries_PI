using backend.Models;
using backend.Handlers.backend.Repositories;

namespace backend.Handlers.backend.Services
{
    public class BeneficioService
    {
        private readonly BeneficioRepository _beneficioRepository;

        public BeneficioService()
        {
            _beneficioRepository = new BeneficioRepository();
        }

        public string CreateBeneficio(BeneficioModel beneficio)
        {
            // Validaciones
            if (beneficio == null)
                return "Los datos del beneficio son requeridos";

            if (string.IsNullOrWhiteSpace(beneficio.Nombre))
                return "El nombre del beneficio es requerido";

            if (beneficio.Valor < 0)
                return "El valor no puede ser negativo";

            // Validaciones:
            var tiposPermitidos = new[] { "Monto Fijo", "Porcentual", "API" };
            if (!tiposPermitidos.Contains(beneficio.Tipo))
                return "Tipo de beneficio no válido";

            var quienAsumePermitido = new[] { "Empresa", "Empleado", "Ambos" };
            if (!quienAsumePermitido.Contains(beneficio.QuienAsume))
                return "Valor no válido para 'Quien Asume'";

            var etiquetasPermitidas = new[] { "Beneficio", "Deducción" };
            if (!etiquetasPermitidas.Contains(beneficio.Etiqueta))
                return "Etiqueta no válida";

            return _beneficioRepository.CreateBeneficio(beneficio);
        }

        public string UpdateBeneficio(int id, BeneficioModel beneficio)
        {
          try
          {
            var repo = new BeneficioRepository();
            var beneficioExistente = repo.GetById(id);
            if (beneficioExistente == null)
                return "Beneficio no encontrado";

            // Actualizar
            beneficioExistente.Nombre = beneficio.Nombre;
            beneficioExistente.Tipo = beneficio.Tipo;
            beneficioExistente.QuienAsume = beneficio.QuienAsume;
            beneficioExistente.Valor = beneficio.Valor;
            beneficioExistente.Etiqueta = beneficio.Etiqueta;

            var actualizado = repo.Update(beneficioExistente);
            return actualizado ? "" : "No se pudo actualizar el beneficio";
          }
          catch (Exception ex)
          {
              Console.WriteLine($"[ERROR] UpdateBeneficio Service: {ex.Message}");
              return "Error interno al actualizar beneficio";
          }
        }


        public List<BeneficioModel> GetAll()
        {
            return _beneficioRepository.GetAll();
        }

        public List<BeneficioModel> GetByEmpresa(long cedulaJuridica)
        {
            return _beneficioRepository.GetByEmpresa(cedulaJuridica);
        }

        public BeneficioModel GetById(int id)
        {
            return _beneficioRepository.GetById(id);
        }

    }
}