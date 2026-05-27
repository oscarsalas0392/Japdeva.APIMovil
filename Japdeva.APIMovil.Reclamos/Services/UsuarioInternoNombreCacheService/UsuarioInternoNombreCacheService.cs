using System.Text.Json;
using Japdeva.APIMovil.Common.Extensions;
using Japdeva.APIMovil.Common.Repositories.ConsultarListaRepository;
using Japdeva.APIMovil.Common.Services.ColaRpcService;
using Japdeva.APIMovil.Reclamos.Entities;
using Japdeva.APIMovil.Reclamos.Models;

namespace Japdeva.APIMovil.Reclamos.Services.UsuarioInternoNombreCacheService
{
    /// <summary>
    /// Caché en memoria de nombres de usuarios internos asignados a detalles de reclamo.
    /// Carga los identificadores desde la base de datos y resuelve los nombres vía RPC.
    /// El caché se refresca periódicamente desde el servicio de parámetros en segundo plano.
    /// </summary>
    public class UsuarioInternoNombreCacheService : IUsuarioInternoNombreCacheService
    {
        private readonly ILogger<UsuarioInternoNombreCacheService> _logger;
        private readonly IServiceProvider _serviceProvider;
        private readonly IColaRpcService _colaRpcService;
        private readonly Dictionary<long, string> _cache = [];

        private const int PAGINA_INICIAL = 1;
        private const int LISTA_VACIA = 0;
        private const int TIMEOUT_SEGUNDOS = 5;
        private const string COLA_OBTENER_USUARIO = "ObtenerUsuario";
        private const string COLA_RESPUESTA = "Respuesta";

        /// <summary>
        /// Inicializa una nueva instancia de la clase <see cref="UsuarioInternoNombreCacheService"/>.
        /// </summary>
        /// <param name="logger">Logger para registro de eventos.</param>
        /// <param name="serviceProvider">Proveedor de servicios para resolución de dependencias con scope.</param>
        /// <param name="colaRpcService">Servicio RPC para obtener datos de usuario desde la cola.</param>
        public UsuarioInternoNombreCacheService(
            ILogger<UsuarioInternoNombreCacheService> logger,
            IServiceProvider serviceProvider,
            IColaRpcService colaRpcService)
        {
            this._logger = logger;
            this._serviceProvider = serviceProvider;
            this._colaRpcService = colaRpcService;
        }

        /// <summary>
        /// Carga el caché con los nombres de todos los usuarios internos asignados
        /// a detalles de reclamo, consultando la base de datos y resolviendo nombres vía RPC.
        /// Invocado periódicamente por el servicio de parámetros en segundo plano.
        /// </summary>
        /// <param name="traceId">Identificador de traza para el seguimiento de la operación.</param>
        public async Task LlenarCacheUsuarioInternoNombreAsync(string traceId)
        {
            string nombreMetodo = this.ObtenerNombreMetodo();
            try
            {
                this._logger.Inicio(traceId, nombreMetodo);

                List<long> idsUsuarios = await this.ObtenerIdsDesdeDetallesAsync(traceId);
                if (idsUsuarios.Count == LISTA_VACIA) return;

                Dictionary<long, string> nombres = await this.ConsultarNombresRpcAsync(traceId, idsUsuarios);

                lock (this._cache)
                {
                    this._cache.Clear();
                    foreach (KeyValuePair<long, string> par in nombres)
                        this._cache[par.Key] = par.Value;
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
        /// Obtiene el nombre completo de un usuario interno desde el caché en memoria.
        /// </summary>
        /// <param name="traceId">Identificador de traza para el seguimiento de la operación.</param>
        /// <param name="idUsuario">Identificador del usuario interno a buscar.</param>
        /// <returns>El nombre completo del usuario, o null si no está en caché.</returns>
        public string? ObtenerNombrePorId(string traceId, long idUsuario)
        {
            string nombreMetodo = this.ObtenerNombreMetodo();
            try
            {
                this._logger.Inicio(traceId, nombreMetodo);
                this._cache.TryGetValue(idUsuario, out string? nombre);
                return nombre;
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
        /// Consulta todas las páginas de detalles de reclamo y retorna los identificadores
        /// únicos de usuarios internos asignados.
        /// </summary>
        /// <param name="traceId">Identificador de traza para el seguimiento de la operación.</param>
        /// <returns>Lista de identificadores únicos de usuarios internos.</returns>
        public async Task<List<long>> ObtenerIdsDesdeDetallesAsync(string traceId)
        {
            string nombreMetodo = this.ObtenerNombreMetodo();
            try
            {
                this._logger.Inicio(traceId, nombreMetodo);

                using var scope = this._serviceProvider.CreateScope();
                var consultarListaRepository = scope.ServiceProvider.GetRequiredService<IConsultarListaRepository>();

                List<long> idsUsuarios = [];
                int paginaActual = PAGINA_INICIAL;
                int totalPaginas = PAGINA_INICIAL;

                do
                {
                    var pagina = await consultarListaRepository.ConsultarListaAsync<DetalleReclamoEntity>(
                        traceId, paginaActual, d => d.IdUsuarioInterno.HasValue);

                    IEnumerable<long> ids = pagina.Lista
                        .Where(d => d.IdUsuarioInterno.HasValue)
                        .Select(d => d.IdUsuarioInterno!.Value);
                    idsUsuarios.AddRange(ids);

                    totalPaginas = pagina.CantidadPaginas;
                    paginaActual++;
                }
                while (paginaActual <= totalPaginas);

                return idsUsuarios.Distinct().ToList();
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
        /// Realiza las llamadas RPC en paralelo para los identificadores indicados
        /// y retorna los nombres resueltos exitosamente.
        /// </summary>
        /// <param name="traceId">Identificador de traza para el seguimiento de la operación.</param>
        /// <param name="idsUsuarios">Identificadores de los usuarios a consultar vía RPC.</param>
        /// <returns>Diccionario de idUsuario → nombre completo de los usuarios resueltos.</returns>
        public async Task<Dictionary<long, string>> ConsultarNombresRpcAsync(string traceId, List<long> idsUsuarios)
        {
            string nombreMetodo = this.ObtenerNombreMetodo();
            Dictionary<long, string> resultado = [];
            try
            {
                this._logger.Inicio(traceId, nombreMetodo);

                using CancellationTokenSource cts = new CancellationTokenSource(TimeSpan.FromSeconds(TIMEOUT_SEGUNDOS));

                var tareas = idsUsuarios.Select(idUsuario => new
                {
                    IdUsuario = idUsuario,
                    Tarea = this._colaRpcService.EnviarYEsperarRespuestaAsync(
                        traceId, COLA_OBTENER_USUARIO, COLA_RESPUESTA, idUsuario.ToString(), cts.Token),
                }).ToList();

                try
                {
                    await Task.WhenAll(tareas.Select(t => t.Tarea));
                }
                catch (Exception)
                {
                    // Algunas tareas fallaron; se procesan las exitosas en el siguiente bloque
                }

                foreach (var item in tareas)
                {
                    if (!item.Tarea.IsCompletedSuccessfully) continue;
                    try
                    {
                        var mensaje = item.Tarea.Result;
                        if (mensaje is null) continue;

                        UsuarioDatosRespuestaModel? usuario = JsonSerializer.Deserialize<UsuarioDatosRespuestaModel>(mensaje.Contenido);
                        if (usuario is null) continue;

                        resultado[item.IdUsuario] = $"{usuario.Nombre} {usuario.Apellidos}".Trim();
                    }
                    catch (Exception)
                    {
                        // Ignorar errores de deserialización por usuario
                    }
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

            return resultado;
        }
    }
}
