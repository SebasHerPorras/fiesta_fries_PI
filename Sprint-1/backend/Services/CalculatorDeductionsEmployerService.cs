using backend.Models;

namespace backend.Services
{
    public class CalculatorDeductionsEmployerService
    {
        public ResultadoDeduccionesEmpleadorDto CalcularDeduccionesEmpleador(
            EmployeeCalculationDto empleado,
            List<EmployerSocialSecurityContributions> cargasSociales)
        {
            if (empleado == null)
                throw new ArgumentException("Los datos del empleado son requeridos");

            if (empleado.SalarioBruto <= 0)
                throw new ArgumentException("El salario bruto debe ser mayor a cero");

            if (cargasSociales == null || !cargasSociales.Any())
            {
                throw new ArgumentException("La lista de cargas sociales es requerida y no puede estar vacia");
            }

            var deducciones = new List<DeductionEmployerDto>();
            decimal totalDeducciones = 0;
            decimal porcentajeTotalEmpleador = 0;

            foreach (var cargaSocial in cargasSociales)
            {
                var monto = Math.Round(empleado.SalarioBruto * cargaSocial.Percentage, 2);

                deducciones.Add(new DeductionEmployerDto
                {
                    Nombre = cargaSocial.Name,
                    Porcentaje = Math.Round(cargaSocial.Percentage * 100, 4),
                    Monto = monto
                });

                totalDeducciones += monto;
                porcentajeTotalEmpleador += cargaSocial.Percentage;

            }

            var resultado = new ResultadoDeduccionesEmpleadorDto
            {
                NombreEmpleado = empleado.NombreEmpleado,
                SalarioBruto = empleado.SalarioBruto,
                DeduccionesEmpleador = deducciones,
                TotalDeduccionesEmpleador = Math.Round(totalDeducciones, 2),
                PorcentajeTotalEmpleador = Math.Round(porcentajeTotalEmpleador * 100, 4)
            };
            return resultado;
        }
    }
}
