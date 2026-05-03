namespace Japdeva.APIMovil.Reclamos.Models
{
    /// <summary>
    /// Modelo de solicitud para la creación de una apelación sobre un reclamo resuelto.
    /// Contiene la información proporcionada por el usuario para registrar la apelación.
    /// </summary>
    public class AgregarApelacionReclamoSolicitudModel
    {
        /// <summary>
        /// Obtiene o establece el identificador del reclamo sobre el cual se presenta la apelación.
        /// </summary>
        public long IdReclamo { get; set; }

        /// <summary>
        /// Obtiene o establece el título descriptivo de la apelación.
        /// </summary>
        public string Titulo { get; set; } = string.Empty;

        /// <summary>
        /// Obtiene o establece la descripción con los argumentos de la apelación.
        /// </summary>
        public string Descripcion { get; set; } = string.Empty;

        /// <summary>
        /// Obtiene o establece el identificador del usuario externo que presenta la apelación.
        /// </summary>
        public long IdUsuarioExterno { get; set; }

        /// <summary>
        /// Obtiene o establece la lista de documentos de soporte asociados a la apelación.
        /// </summary>
        public List<ArchivoSolicitudModel> ListaDocumentos { get; set; } = new List<ArchivoSolicitudModel>();
    }
}
