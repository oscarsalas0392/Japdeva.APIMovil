namespace Japdeva.APIMovil.Reclamos.Models
{
    /// <summary>
    /// Representa un archivo adjunto a una solicitud, incluyendo su nombre y contenido codificado en base64.
    /// </summary>
    public class ArchivoSolicitudModel
    {
        /// <summary>
        /// Obtiene o establece el nombre del archivo.
        /// </summary>
  
        public string NombreArchivo { get; set; } = string.Empty;

        /// <summary>
        /// Obtiene o establece el contenido del archivo codificado en base64.
        /// </summary>
        public string ContenidoArchivo { get; set; } = string.Empty;
    }
}
