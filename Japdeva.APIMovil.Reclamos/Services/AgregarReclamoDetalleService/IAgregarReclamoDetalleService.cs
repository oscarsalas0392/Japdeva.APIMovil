using Japdeva.APIMovil.Reclamos.Models;

namespace Japdeva.APIMovil.Reclamos.Services.AgregarReclamoDetalleService
{
    /// <summary>
    /// Interfaz que define el contrato para el servicio de agregación de detalles de reclamos.
    /// Proporciona operaciones para añadir información detallada y seguimiento a reclamos existentes.
    /// </summary>
    public interface IAgregarReclamoDetalleService
    {
        /// <summary>
        /// Valor por defecto para el identificador de usuario interno.
        /// </summary>
        private const int VALOR_DEFECTO_ID_USUARIO_INTERNO = 0;

        /// <summary>
        /// Valor por defecto para el identificador de orden de proceso.
        /// </summary>
        private const int VALOR_DEFECTO_ID_ORDEN_PROCESO = 0;

        /// <summary>
        /// Valor por defecto para el descripcion.
        /// </summary>
        private const string VALOR_DEFECTO_DESCRIPCION = "";

        /// <summary>
        /// Agrega un nuevo detalle a un reclamo existente de forma asíncrona.
        /// Permite registrar información adicional, comentarios o actualizaciones sobre el estado del reclamo.
        /// </summary>
        /// <param name="traceId">Identificador único para rastreo de la operación.</param>
        /// <param name="idReclamo">Identificador del reclamo al cual se agregará el detalle.</param>
        /// <param name="descripcion">Descripción o comentario del detalle a agregar.</param>
        /// <param name="estadoDetalleReclamoModel">Estado inicial del detalle del reclamo. Valor por defecto: Pendiente.</param>
        /// <param name="idUsuarioInterno">Identificador del usuario interno que registra el detalle. Valor por defecto: 0.</param>
        /// <param name="idOrdenProceso">Identificador del orden de proceso asociado. Valor por defecto: 0.</param>
        Task AgregarReclamoDetalleAsync(
             string traceId,
             long idReclamo,
             string descripcion = VALOR_DEFECTO_DESCRIPCION,
             EstadoDetalleReclamoModel estadoDetalleReclamoModel = EstadoDetalleReclamoModel.Pendiente,
             int idUsuarioInterno = VALOR_DEFECTO_ID_USUARIO_INTERNO,
             int idOrdenProceso = VALOR_DEFECTO_ID_ORDEN_PROCESO);
    }
}
