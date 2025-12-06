namespace Japdeva.APIMovil.Reclamos.Models
{
    /// <summary>
    /// Modelo de solicitud para la creación de un nuevo reclamo en el sistema.
    /// Contiene toda la información necesaria proporcionada por el usuario para registrar un reclamo.
    /// </summary>
    public class AgregarReclamoSolicitudModel
    {
        /// <summary>
        /// Obtiene o establece el identificador único de la solicitud.
        /// </summary>
        public long Id { get; set; }

        /// <summary>
        /// Obtiene o establece el título descriptivo del reclamo.
        /// Resumen breve que identifica el reclamo.
        /// </summary>
        public string Titulo { get; set; } = string.Empty;

        /// <summary>
        /// Obtiene o establece la descripción detallada del reclamo.
        /// Explicación completa del problema, situación o solicitud del usuario.
        /// </summary>
        public string Descripcion { get; set; } = string.Empty;

        /// <summary>
        /// Obtiene o establece el identificador del usuario externo que crea el reclamo.
        /// Referencia al usuario que origina la solicitud.
        /// </summary>
        public long IdUsuarioExterno { get; set; }

        /// <summary>
        /// Obtiene o establece la lista de documentos asociados al reclamo.
        /// Archivos, imágenes o documentos de soporte proporcionados como evidencia.
        /// </summary>
        public List<string> ListaDocumentos { get; set; } = new List<string>();
    }
}
