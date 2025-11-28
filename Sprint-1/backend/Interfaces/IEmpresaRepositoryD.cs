using System;
using System.Collections.Generic;

namespace backend.Interfaces
{
    public interface IEmpresaRepositoryD
    {
        List<DateTime> GetUltimasFechasPago(long cedulaJuridica, DateTime fechaLimite);

        decimal getEmployerDeductions(long id, DateTime fecha);

        decimal GetBeneficiosEmpresa(long cedula);

        decimal GetTotalSalarios(long cedula);
    }
}