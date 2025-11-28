using System;
using System.Collections.Generic;
using backend.Interfaces;

public class EmployerDashboardServiceTest : IEmployerDashboardService
{
    private readonly IEmpresaRepositoryD _repo;

    public EmployerDashboardServiceTest(IEmpresaRepositoryD repo)
    {
        _repo = repo;
    }

    public List<DateTime>? GetUltimasFechasPago(long cedulaJuridica, DateTime fechaLimite)
    {
        return _repo.GetUltimasFechasPago(cedulaJuridica, fechaLimite);
    }

    public decimal GetPlanillaCosto(long id, DateTime fecha)
    {
        decimal result = 0;

        result += _repo.getEmployerDeductions(id, fecha);
        result += _repo.GetBeneficiosEmpresa(id);
        result += _repo.GetTotalSalarios(id);

        return result;
    }
}
