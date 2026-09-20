namespace Japdeva.APIMovil.Reclamos.Models
{
    /// <summary>
    /// Enumeración que define los posibles estados de los detalles de reclamo en el sistema.
    /// Representa las diferentes etapas por las que puede pasar un detalle específico durante el proceso de atención.
    /// </summary>
    public enum EstadoDetalleReclamoModel
    {
        /// <summary>
        /// Estado inicial del detalle. Indica que el detalle ha sido registrado pero está pendiente de procesamiento.
        /// </summary>
        Pendiente = 1,

        /// <summary>
        /// Estado que indica que el detalle está siendo procesado o en revisión.
        /// </summary>
        EnProceso = 2,

        /// <summary>
        /// Estado que indica que el detalle ha sido completado exitosamente.
        /// </summary>
        Completado = 3,

        /// <summary>
        /// Estado que indica que el detalle ha sido rechazado o no puede ser procesado.
        /// </summary>
        Rechazado = 4,

        /// <summary>
        /// Estado que indica que el detalle ha sido devuelto a otro departamento.
        /// </summary>
        Devuelto = 5
    }
}
