using backend.Interfaces;
using backend.Models;
using backend.Repositories;
using System;
using System.Collections.Generic;

namespace backend.Services
{
    public class EmployeeBenefitService : IEmployeeBenefitService
    {
        private readonly EmployeeBenefitRepository _repository;

        public EmployeeBenefitService(EmployeeBenefitRepository repository)
        {
            _repository = repository ?? throw new ArgumentNullException(nameof(repository));
        }

        public Task<List<int>> GetSelectedBenefitIdsAsync(int employeeId)
        {
            if (employeeId <= 0) throw new ArgumentException("employeeId debe ser mayor a 0", nameof(employeeId));
            return _repository.GetSelectedBenefitIdsAsync(employeeId);
        }

        public Task<bool> CanEmployeeSelectBenefitAsync(int employeeId, int benefitId)
        {
            if (employeeId <= 0 || benefitId <= 0) return Task.FromResult(false);
            return _repository.CanEmployeeSelectBenefitAsync(employeeId, benefitId);
        }

        public async Task<bool> SaveSelectionAsync(EmployeeBenefit entity)
        {
            if (entity == null) throw new ArgumentNullException(nameof(entity));
            if (entity.EmployeeId <= 0 || entity.BenefitId <= 0) throw new ArgumentException("EmployeeId y BenefitId requeridos");

            var can = await _repository.CanEmployeeSelectBenefitAsync(entity.EmployeeId, entity.BenefitId);
            if (!can) return false;

            // 2. Obtener datos canónicos del beneficio
            var benefit = await _repository.GetBeneficioByIdAsync(entity.BenefitId);
            if (benefit == null)
            {
                return false;
            }

            entity.ApiName = benefit.Nombre;
            entity.BenefitValue = benefit.Valor;
            entity.BenefitType = benefit.Tipo;

            if (entity.BenefitValue.HasValue && entity.BenefitValue.Value < 0)
            {
                throw new InvalidOperationException("BenefitValue no puede ser negativo");
            }

            var saved = await _repository.SaveSelectionAsync(entity);
            return saved;
        }

    }
}
