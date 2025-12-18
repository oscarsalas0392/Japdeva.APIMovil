namespace Japdeva.APIMovil.Reclamos.Models
{
    /// <summary>
    /// Enumeración que define los posibles estados de un reclamo en el sistema.
    /// Representa el ciclo de vida y las diferentes etapas por las que puede pasar un reclamo.
    /// </summary>
    public enum EstadoReclamoModel
    {
        /// <summary>
        /// Estado inicial del reclamo. Indica que el reclamo ha sido registrado pero aún no ha sido procesado.
        /// </summary>
        Pendiente = 1,

        /// <summary>
        /// Estado que indica que el reclamo está siendo procesado o atendido por el personal interno.
        /// </summary>
        EnProceso = 2,

        /// <summary>
        /// Estado que indica que el reclamo ha sido resuelto satisfactoriamente.
        /// </summary>
        Completado = 3,

        /// <summary>
        /// Estado  que indica que el reclamo ha sido rechazado.
        /// </summary>
        Rechazado = 4
    }
}
