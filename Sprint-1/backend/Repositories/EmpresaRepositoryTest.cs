using backend.Interfaces;
using backend.Repositories;
using System;
using System.Collections.Generic;

public class EmpresaRepositoryTests : IEmpresaRepositoryD
{
    public List<DateTime> FechasPago = new();
    public decimal Deducciones = 0;
    public decimal Beneficios = 0;
    public decimal Salarios = 0;

    public List<DateTime> GetUltimasFechasPago(long cedulaJuridica, DateTime fechaLimite)
    {
        return FechasPago
            .Where(f => f.Date <= fechaLimite.Date)
            .OrderByDescending(f => f)
            .Take(3)
            .ToList();
    }

    public decimal getEmployerDeductions(long id, DateTime fecha)
    {
        return Deducciones;
    }

    public decimal GetBeneficiosEmpresa(long cedula)
    {
        return Beneficios;
    }

    public decimal GetTotalSalarios(long cedula)
    {
        return Salarios;
    }
}
