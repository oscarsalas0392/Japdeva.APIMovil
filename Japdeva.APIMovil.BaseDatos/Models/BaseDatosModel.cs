namespace Japdeva.APIMovil.BaseDatos.Models
{
    public class BaseDatosModel
    {
        public string Nombre { get; set; } = string.Empty;
        public bool Ejecutar { get; set; }
        public bool RecrearBaseDatos { get; set; }
    }
}
