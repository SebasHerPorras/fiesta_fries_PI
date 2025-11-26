using System;
using System.Collections.Generic;

namespace backend.Interfaces
{
    public interface IEmployerDashboardService
    {
        List<DateTime>? GetUltimasFechasPago(long cedulaJuridica, DateTime fechaLimite);

        decimal GetPlanillaCosto(long id, DateTime fecha);
    }
}