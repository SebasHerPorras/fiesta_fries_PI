using backend.Models;

namespace backend.Services
{
    public class CalculatorDeductionsEmployerService
    {
        // Tasas patronales Costa Rica - Empleador
        private readonly Dictionary<string, decimal> _tasasEmpleador = new()
        {
            { "CCSS Salud", 0.0925m },                    // 9.25%
            { "CCSS Pensiones (IVM)", 0.0525m },          // 5.25%
            { "INA", 0.005m },                            // 0.50%
            { "ASFA", 0.0025m },                          // 0.25%
            { "Banco Popular", 0.0025m },                 // 0.25%
            { "Fondo de Capitalización Laboral (FCL)", 0.03m }  // 3.00%
        };

        public ResultadoDeduccionesEmpleadorDto CalcularDeduccionesEmpleador(EmployeeCalculationDto empleado)
        {
            if (empleado == null)
                throw new ArgumentException("Los datos del empleado son requeridos");

            if (empleado.SalarioBruto <= 0)
                throw new ArgumentException("El salario bruto debe ser mayor a cero");

            var deducciones = new List<DeductionEmployerDto>();
            decimal totalDeducciones = 0;

            foreach (var tasa in _tasasEmpleador)
            {
                var monto = Math.Round(empleado.SalarioBruto * tasa.Value, 2);
                
                deducciones.Add(new DeductionEmployerDto
                {
                    Nombre = tasa.Key,
                    Porcentaje = tasa.Value * 100, // Convertir a porcentaje
                    Monto = monto
                });

                totalDeducciones += monto;
            }

            return new ResultadoDeduccionesEmpleadorDto
            {
                NombreEmpleado = empleado.NombreEmpleado,
                SalarioBruto = empleado.SalarioBruto,
                DeduccionesEmpleador = deducciones,
                TotalDeduccionesEmpleador = Math.Round(totalDeducciones, 2),
                PorcentajeTotalEmpleador = Math.Round(_tasasEmpleador.Values.Sum() * 100, 2) // 18.50%
            };
        }
    }
}