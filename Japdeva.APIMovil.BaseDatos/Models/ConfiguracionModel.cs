namespace Japdeva.APIMovil.BaseDatos.Models
{
    public class ConfiguracionModel
    {
        public string Servidor { get; set; } = string.Empty;
        public int Puerto { get; set; }
        public string UsuarioPostgres { get; set; } = string.Empty;
        public bool RecrearTodasBasesDatos { get; set; }
        public List<string> OrdenCarpetas { get; set; } = [];
        public List<BaseDatosModel> BaseDatos { get; set; } = [];
    }
}
