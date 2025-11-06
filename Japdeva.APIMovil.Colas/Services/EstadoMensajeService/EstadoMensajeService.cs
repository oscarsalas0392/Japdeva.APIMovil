using Microsoft.Extensions.Logging;
using Japdeva.APIMovil.Colas.Entities;
using Japdeva.APIMovil.Common.Extensions;
using Japdeva.APIMovil.Common.Repositories.ConsultarListaRepository;

namespace Japdeva.APIMovil.Colas.Services.EstadoMensajeService
{
    /// <summary>
    /// Servicio para gestionar estados de mensajes en colas.
    /// </summary>
    public class EstadoMensajeService : IEstadoMensajeService
    {
        private readonly ILogger<EstadoMensajeService> _logger;
        private readonly IServiceProvider _serviceProvider;
        private readonly List<EstadoMensajeEntity> _estadosCache = new List<EstadoMensajeEntity>();
        private const int PAGINA_INICIAL = 1;
        private const bool ESTADO_ACTIVO = true;

        /// <summary>
        /// Inicializa una nueva instancia de la clase EstadoMensajeService.
        /// </summary>
        /// <param name="serviceProvider">Proveedor de servicios para resolver dependencias.</param>
        /// <param name="logger">Instancia de logger para registrar eventos.</param>
        public EstadoMensajeService(IServiceProvider serviceProvider, ILogger<EstadoMensajeService> logger)
        {
            _logger = logger;
            _serviceProvider = serviceProvider;
        }

        /// <summary>
        /// Llena el caché de estados de mensajes con datos activos desde el repositorio.
        /// </summary>
        /// <param name="traceId">Identificador de trazabilidad para el seguimiento de la operación.</param>
        public async Task LlenarCacheEstadosMensajeAsync(string traceId)
        {
            string nombreMetodo = this.ObtenerNombreMetodo();
            try
            {
                this._logger.Inicio(traceId, nombreMetodo);

                using var scope = this._serviceProvider.CreateScope();
                var consultarListaRepository = scope.ServiceProvider.GetRequiredService<IConsultarListaRepository>();
                var estadosMensaje = await consultarListaRepository.ConsultarListaAsync<EstadoMensajeEntity>(
                    traceId, PAGINA_INICIAL, e => e.Activo == ESTADO_ACTIVO);

                if (estadosMensaje is null || !estadosMensaje.Lista.Any()) return;

                lock (this._estadosCache)
                {
                    this._estadosCache.Clear();
                    this._estadosCache.AddRange(estadosMensaje.Lista);
                }
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

        /// <summary>
        /// Obtiene un estado de mensaje específico por su identificador desde el caché.
        /// </summary>
        /// <param name="traceId">Identificador de trazabilidad para el seguimiento de la operación.</param>
        /// <param name="id">Identificador único del estado de mensaje a buscar.</param>
        /// <returns>La entidad de estado de mensaje encontrada o null si no existe.</returns>
        public EstadoMensajeEntity? ObtenerEstadoMensajePorId(string traceId, int id)
        {
            string nombreMetodo = this.ObtenerNombreMetodo();
            try
            {
                this._logger.Inicio(traceId, nombreMetodo);

                lock (this._estadosCache)
                {
                    return this._estadosCache.FirstOrDefault(e => e.Id == id);
                }
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