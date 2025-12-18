namespace Japdeva.APIMovil.Reclamos.Models
{
    /// <summary>
    /// Modelo para agregar documentos internos a un reclamo, incluyendo el identificador del reclamo y la lista de documentos asociados.
    /// </summary>
    public class AgregarDocumentoInternoSolicitudModel
    {
        /// <summary>
        /// Identificador del detalle reclamo asociado a la solicitud.
        /// </summary>
        public long IdDetalleReclamo { get; set; }

        /// <summary>
        /// Lista de documentos asociados a la solicitud.
        /// </summary>
        public List<ArchivoSolicitudModel> ListaDocumentos { get; set; } = new();
    }
}
