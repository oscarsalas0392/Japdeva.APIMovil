using Microsoft.AspNetCore.Mvc;
using Japdeva.APIMovil.Common.Extensions;
using Japdeva.APIMovil.Common.Repositories.ActualizarRepository;
using Japdeva.APIMovil.Common.Repositories.ConsultarRepository;
using Japdeva.APIMovil.Reclamos.Entities;
using Japdeva.APIMovil.Reclamos.Models;

namespace Japdeva.APIMovil.Reclamos.Services.AsignarDetalleReclamoService
{
    /// <summary>
    /// Servicio que asigna un detalle de reclamo a un usuario interno y lo pasa a estado En Proceso.
    /// Solo permite la asignación si el detalle está en estado Pendiente, garantizando
    /// que un caso no pueda ser tomado por más de un usuario.
    /// </summary>
    public class AsignarDetalleReclamoService : IAsignarDetalleReclamoService
    {
        private readonly ILogger<AsignarDetalleReclamoService> _logger;
        private readonly IServiceProvider _serviceProvider;

        private const int ESTADO_PENDIENTE = 1;
        private const int ESTADO_EN_PROCESO = 2;
        private const string MENSAJE_ERROR_DETALLE_NO_ENCONTRADO = "El detalle de reclamo con Id {0} no fue encontrado.";
        private const string MENSAJE_ERROR_YA_ASIGNADO = "El detalle de reclamo con Id {0} ya fue asignado y no está en estado Pendiente.";
        private const string MENSAJE_ERROR_EN_HISTORICO = "No se puede asignar el detalle del reclamo con Id {0} porque el reclamo está en histórico.";

        /// <summary>
        /// Inicializa una nueva instancia de la clase <see cref="AsignarDetalleReclamoService"/>.
        /// </summary>
        /// <param name="logger">Logger para registro de eventos.</param>
        /// <param name="serviceProvider">Proveedor de servicios para resolución de dependencias con scope.</param>
        public AsignarDetalleReclamoService(
            ILogger<AsignarDetalleReclamoService> logger,
            IServiceProvider serviceProvider)
        {
            this._logger = logger;
            this._serviceProvider = serviceProvider;
        }

        /// <summary>
        /// Asigna el detalle de reclamo al usuario interno, cambia el estado a En Proceso
        /// y actualiza el estado del reclamo padre si estaba Pendiente.
        /// </summary>
        /// <param name="traceId">Identificador de traza.</param>
        /// <param name="solicitud">Datos de la asignación.</param>
        /// <returns>OkResult si la operación fue exitosa.</returns>
        public async Task<IActionResult> AsignarDetalleReclamoAsync(string traceId, AsignarDetalleReclamoSolicitudModel solicitud)
        {
            string nombreMetodo = this.ObtenerNombreMetodo();
            using var scope = this._serviceProvider.CreateScope();
            try
            {
                this._logger.Inicio(traceId, nombreMetodo);

                var consultarRepository = scope.ServiceProvider.GetRequiredService<IConsultarRepository>();
                var actualizarRepository = scope.ServiceProvider.GetRequiredService<IActualizarRepository>();

                var detalle = await consultarRepository.ConsultarAsync<DetalleReclamoEntity>(
                    traceId, d => d.Id == solicitud.IdDetalleReclamo);

                if (detalle is null)
                    throw new KeyNotFoundException(string.Format(MENSAJE_ERROR_DETALLE_NO_ENCONTRADO, solicitud.IdDetalleReclamo));

                if (detalle.IdEstadoDetalleReclamo != ESTADO_PENDIENTE)
                    throw new InvalidOperationException(string.Format(MENSAJE_ERROR_YA_ASIGNADO, solicitud.IdDetalleReclamo));

                var reclamo = await consultarRepository.ConsultarAsync<ReclamoEntity>(
                    traceId, r => r.Id == detalle.IdReclamo);

                if (reclamo is not null && reclamo.EstaEnHistorico)
                    throw new InvalidOperationException(string.Format(MENSAJE_ERROR_EN_HISTORICO, solicitud.IdDetalleReclamo));

                detalle.IdUsuarioInterno = solicitud.IdUsuarioInterno;
                detalle.IdEstadoDetalleReclamo = ESTADO_EN_PROCESO;
                detalle.FechaEdicion = DateTime.UtcNow;

                await actualizarRepository.ActualizarAsync<DetalleReclamoEntity>(traceId, detalle);

                if (reclamo is not null && reclamo.IdEstadoReclamo == ESTADO_PENDIENTE)
                {
                    reclamo.IdEstadoReclamo = ESTADO_EN_PROCESO;
                    await actualizarRepository.ActualizarAsync<ReclamoEntity>(traceId, reclamo);
                }

                return new OkResult();
            }
            catch (Exception ex)
            {
                this._logger.Error(traceId, nombreMetodo, ex);
                throw;
            }
            finally
            {
                this._logger.Fin(traceId, nombreMetodo);
            }
        }
    }
}
