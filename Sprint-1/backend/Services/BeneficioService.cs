using backend.Models;
using backend.Handlers.backend.Repositories;
using backend.Interfaces;
using backend.Repositories;
using System;
using System.Collections.Generic;
using Microsoft.Data.SqlClient;

namespace backend.Services
{
    public class BeneficioService : IBeneficioService
    {
        private readonly BeneficioRepository _beneficioRepository;
        private readonly PayrollReportRepository _payrollReportRepository; // Para borrados lógicos
        private readonly ILogger<BeneficioService> _logger;

        public BeneficioService(BeneficioRepository beneficioRepository,
            PayrollReportRepository payrollReportRepository,
            ILogger<BeneficioService> logger)
        {
            _beneficioRepository = beneficioRepository;
            _payrollReportRepository = payrollReportRepository;
            _logger = logger;
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
            var beneficioExistente = _beneficioRepository.GetById(id);
            if (beneficioExistente == null)
                return "Beneficio no encontrado";

            // Actualizar
            beneficioExistente.Nombre = beneficio.Nombre;
            beneficioExistente.Tipo = beneficio.Tipo;
            beneficioExistente.QuienAsume = beneficio.QuienAsume;
            beneficioExistente.Valor = beneficio.Valor;
            beneficioExistente.Etiqueta = beneficio.Etiqueta;

            var actualizado = _beneficioRepository.Update(beneficioExistente);
            return actualizado ? "" : "No se pudo actualizar el beneficio";
          }
          catch (Exception ex)
          {
                _logger.LogError(ex, "Error Update Beneficio Service: {BenefitId}", id);
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

        public string DeleteBeneficio(int benefitId)
        {
            try
            {
                _logger.LogInformation("Starting deletion process for benefit {BenefitId}", benefitId);

                bool existsInPayroll = _beneficioRepository.ExistsInEmployerBenefitDeductions(benefitId);

                if (!existsInPayroll)
                {
                    _beneficioRepository.PhysicalDeletion(benefitId);
                    _logger.LogInformation("Physical deletion applied for benefit {BenefitId}", benefitId);
                    return "Physical deletion applied";
                }
                else
                {
                    DateTime? lastPeriod = _payrollReportRepository.GetLastPeriodAsync(benefitId).Result;

                    _beneficioRepository.LogicalDeletion(benefitId, lastPeriod);

                    _logger.LogInformation("Logical deletion applied for benefit {BenefitId}, last period {LastPeriod}", benefitId, lastPeriod);
                    return "Logical deletion applied";
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting benefit {BenefitId}", benefitId);
                return "Internal error deleting benefit";
            }
        }

    }
}