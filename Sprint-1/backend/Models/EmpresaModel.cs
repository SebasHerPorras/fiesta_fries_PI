public class EmpresaModel
{
    public long CedulaJuridica { get; set; }
    public string Nombre { get; set; }
    public string? DireccionEspecifica { get; set; }
    public int? Telefono { get; set; }
    public int NoMaxBeneficios { get; set; }
    public string FrecuenciaPago { get; set; }
    public int DiaPago { get; set; }
    public int DueñoEmpresa { get; set; } 
}