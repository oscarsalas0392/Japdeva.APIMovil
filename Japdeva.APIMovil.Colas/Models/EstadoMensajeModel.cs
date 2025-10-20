namespace Japdeva.APIMovil.Colas.Models
{
    /// <summary>
    /// Enumeración de los posibles estados de un mensaje en cola.
    /// </summary>
    public enum EstadoMensajeModel
    {
        /// <summary>
        /// Mensaje pendiente de procesamiento.
        /// </summary>
        Pendiente = 1,

        /// <summary>
        /// Mensaje en proceso de ejecución.
        /// </summary>
        EnProceso = 2,

        /// <summary>
        /// Mensaje procesado exitosamente.
        /// </summary>
        Procesado = 3,

        /// <summary>
        /// Mensaje falló en el procesamiento.
        /// </summary>
        Fallido = 4,

        /// <summary>
        /// Mensaje cancelado manualmente.
        /// </summary>
        Cancelado = 5,

    }
}