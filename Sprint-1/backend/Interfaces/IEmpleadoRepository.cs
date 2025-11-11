using System.Collections.Generic;
using backend.Models;

namespace backend.Interfaces
{
    public interface IEmpleadoRepository
    {
        List<EmpleadoModel> GetEmpleadosPorEmpresa(long cedulaEmpresa);
    }
}