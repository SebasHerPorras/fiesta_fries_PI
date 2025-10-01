namespace backend.Models
{
    public class EmpleadoListDto
    {
        public int Cedula { get; set; }
        public string Nombre { get; set; } = string.Empty;
        public int Edad { get; set; }
        public string Correo { get; set; } = string.Empty;
        public string Departamento { get; set; } = string.Empty;
        public string TipoContrato { get; set; } = string.Empty;
    }
}