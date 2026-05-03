namespace Japdeva.APIMovil.Reclamos.Models
{
    /// <summary>
    /// Modelo para editar el detalle de un reclamo, incluyendo información relevante como el estado, usuario interno y descripción.
    /// </summary>
    public class EditarDetalleReclamoSolicitudModel
    {
        /// <summary>
        /// Obtiene o establece el identificador único del detalle del reclamo.
        /// </summary>
        public long IdDetalleReclamo { get; set; }

        /// <summary>
        /// Obtiene o establece el identificador del estado del detalle del reclamo.
        /// Define el estado actual en el que se encuentra el detalle.
        /// </summary>
        public int IdEstadoDetalleReclamo { get; set; }


        /// <summary>
        /// Obtiene o establece el identificador del nivel del siguiente proceso asociado al detalle del reclamo.
        /// Indica el flujo o etapa a la que avanzará el reclamo tras la acción actual.
        /// </summary>
        public int? IdNivelSiguienteProceso { get; set; }

        /// <summary>
        /// Obtiene o establece el identificador del usuario interno asignado al detalle.
        /// Puede ser nulo si no hay usuario asignado.
        /// </summary>
        public long? IdUsuarioInterno { get; set; }

        /// <summary>
        /// Obtiene o establece la descripción o comentarios adicionales del detalle del reclamo.
        /// </summary>
        public string Descripcion { get; set; } = string.Empty;

        /// <summary>
        /// Obtiene o establece la descripción de la resolución asociada al detalle del reclamo.
        /// </summary>
        public string DescripcionResolucion { get; set; } = string.Empty;


    }
}
