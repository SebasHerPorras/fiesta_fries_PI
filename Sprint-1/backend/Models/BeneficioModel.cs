namespace backend.Models
{
    public class BeneficioModel
    {
        public int IdBeneficio { get; set; }
        public long CedulaJuridica { get; set; }
        public string Nombre { get; set; }
        public string Tipo { get; set; }
        public string QuienAsume { get; set; }
        public decimal Valor { get; set; }
        public string Etiqueta { get; set; }
    }
}
