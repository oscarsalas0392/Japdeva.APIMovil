using Microsoft.Extensions.Logging;
using Japdeva.APIMovil.Colas.Entities;
using Japdeva.APIMovil.Common.Extensions;
using Japdeva.APIMovil.Common.Repositories.ConsultarListaRepository;
using Japdeva.APIMovil.Common.Repositories.ConsultarRepository;

namespace Japdeva.APIMovil.Colas.Services.ColaService
{
    /// <summary>
    /// Servicio para gestionar colas en el sistema.
    /// </summary>
    public class ColaService : IColaService
    {
        private readonly ILogger<ColaService> _logger;
        private readonly IServiceProvider _serviceProvider;
        private readonly List<ColaEntity> _colasCache = new List<ColaEntity>();
        private const int PAGINA_INICIAL = 1;
        private const bool ESTADO_ACTIVO = true;

        /// <summary>
        /// Inicializa una nueva instancia de la clase ColaService.
        /// </summary>
        /// <param name="serviceProvider">Proveedor de servicios para inyección de dependencias.</param>
        /// <param name="logger">Instancia de logger para registrar eventos.</param>
        public ColaService(IServiceProvider serviceProvider, ILogger<ColaService> logger)
        {
            _logger = logger;
            _serviceProvider = serviceProvider;
        }

        /// <summary>
        /// Llena el caché de colas con datos activos desde el repositorio.
        /// </summary>
        /// <param name="traceId">Identificador de trazabilidad para el seguimiento de la operación.</param>
        public async Task LlenarCacheColasAsync(string traceId)
        {
            string nombreMetodo = this.ObtenerNombreMetodo();
            try
            {
                using var scope = this._serviceProvider.CreateScope();
                var consultarListaRepository = scope.ServiceProvider.GetRequiredService<IConsultarListaRepository>(); 
                this._logger.Inicio(traceId, nombreMetodo);
                var colas = await consultarListaRepository.ConsultarListaAsync<ColaEntity>(
                    traceId, PAGINA_INICIAL, c => c.Activo == ESTADO_ACTIVO);

                if (colas is null || !colas.Lista.Any()) return;

                lock (this._colasCache)
                {
                    this._colasCache.Clear();
                    this._colasCache.AddRange(colas.Lista);
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
        /// Obtiene una cola específica por su identificador desde el caché.
        /// </summary>
        /// <param name="traceId">Identificador de trazabilidad para el seguimiento de la operación.</param>
        /// <param name="nombreCola">Nombre de la cola a buscar.</param>
        /// <returns>La entidad de cola encontrada o null si no existe.</returns>
        public async Task<ColaEntity?> ObtenerColaPorNombreAsync(string traceId, string nombreCola)
        {
            string nombreMetodo = this.ObtenerNombreMetodo();
            try
            {
                ColaEntity? cola = null;
                this._logger.Inicio(traceId, nombreMetodo);

                lock (this._colasCache)
                {
                    cola = this._colasCache.FirstOrDefault(c => c.Nombre == nombreCola);
                }

                if (cola is null)
                {
                    using var scope = this._serviceProvider.CreateScope();
                    var consultarRepository = scope.ServiceProvider.GetRequiredService<IConsultarRepository>();
                    cola = await consultarRepository.ConsultarAsync<ColaEntity>(
                        traceId,
                        c => c.Nombre == nombreCola && c.Activo);
                }

                return cola;

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
        /// Cuenta el número total de colas activas en el sistema.
        /// </summary>
        /// <param name="traceId">Identificador de trazabilidad para el seguimiento de la operación.</param>
        /// <returns>El número total de colas activas.</returns>
        public async Task<int> ContarColasActivasAsync(string traceId)
        {
            string nombreMetodo = this.ObtenerNombreMetodo();
            try
            {
                this._logger.Inicio(traceId, nombreMetodo);

                using var scope = this._serviceProvider.CreateScope();
                var consultarRepository = scope.ServiceProvider.GetRequiredService<IConsultarRepository>();
                return await consultarRepository.ContarAsync<ColaEntity>(
                    traceId,
                    c => c.Activo);

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