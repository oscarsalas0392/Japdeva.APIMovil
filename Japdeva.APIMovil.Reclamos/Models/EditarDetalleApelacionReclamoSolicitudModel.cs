namespace Japdeva.APIMovil.Reclamos.Models
{
    /// <summary>
    /// Modelo de solicitud para editar el detalle de una apelación de reclamo.
    /// </summary>
    public class EditarDetalleApelacionReclamoSolicitudModel
    {
        /// <summary>
        /// Obtiene o establece el identificador único del detalle de apelación a editar.
        /// </summary>
        public long IdDetalleApelacionReclamo { get; set; }

        /// <summary>
        /// Obtiene o establece el identificador del estado del detalle de la apelación.
        /// </summary>
        public int IdEstadoDetalleReclamo { get; set; }

        /// <summary>
        /// Obtiene o establece el identificador del siguiente nivel de proceso.
        /// Indica la etapa a la que avanzará la apelación tras la acción actual.
        /// </summary>
        public int? IdNivelSiguienteProceso { get; set; }

        /// <summary>
        /// Obtiene o establece el identificador del usuario interno asignado al detalle.
        /// </summary>
        public long? IdUsuarioInterno { get; set; }

        /// <summary>
        /// Obtiene o establece la descripción o comentarios adicionales del detalle.
        /// </summary>
        public string Descripcion { get; set; } = string.Empty;

        /// <summary>
        /// Obtiene o establece la descripción de la resolución aplicada a la apelación.
        /// </summary>
        public string DescripcionResolucion { get; set; } = string.Empty;
    }
}
