namespace Japdeva.APIMovil.Reclamos.Models
{
    /// <summary>
    /// Modelo de solicitud para asignar un detalle de reclamo a un usuario interno.
    /// Al asignar, el detalle pasa a estado En Proceso y queda bloqueado para otros usuarios.
    /// </summary>
    public class AsignarDetalleReclamoSolicitudModel
    {
        /// <summary>
        /// Obtiene o establece el identificador único del detalle del reclamo a asignar.
        /// </summary>
        public long IdDetalleReclamo { get; set; }

        /// <summary>
        /// Obtiene o establece el identificador del usuario interno que tomará el caso.
        /// </summary>
        public long IdUsuarioInterno { get; set; }
    }
}
