
namespace Japdeva.APIMovil.Colas.Services.GuardarMensajesHistoricoService
{
    /// <summary>
    /// Interfaz para el servicio de gestión de histórico de mensajes en colas.
    /// </summary>
    public interface IGuardarMensajesHistoricoService
    {
        /// <summary>
        /// Mueve mensajes procesados y cancelados al histórico para mejorar el rendimiento.
        /// </summary>
        /// <param name="traceId">Identificador de trazabilidad para el seguimiento de la operación.</param>
        /// <returns>Número de mensajes movidos al histórico.</returns>
        Task MoverMensajesAHistoricoAsync(string traceId);

    }
}