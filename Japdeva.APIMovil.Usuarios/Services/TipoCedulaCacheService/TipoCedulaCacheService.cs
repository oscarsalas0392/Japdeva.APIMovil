using Japdeva.APIMovil.Common.Extensions;
using Japdeva.APIMovil.Common.Repositories.ConsultarListaRepository;
using Japdeva.APIMovil.Usuarios.Entities;

namespace Japdeva.APIMovil.Usuarios.Services.TipoCedulaCacheService
{
    /// <summary>
    /// Servicio de caché para los tipos de cédula disponibles en el sistema.
    /// Mantiene en memoria todos los tipos activos para consulta sin acceso a base de datos.
    /// </summary>
    public class TipoCedulaCacheService : ITipoCedulaCacheService
    {
        private readonly ILogger<TipoCedulaCacheService> _logger;
        private readonly IServiceProvider _serviceProvider;
        private readonly List<TipoCedulaEntity> _tiposCedulaCache = new List<TipoCedulaEntity>();

        private const int PAGINA_INICIAL = 1;

        /// <summary>
        /// Inicializa el servicio de caché de tipos de cédula.
        /// </summary>
        /// <param name="logger">Logger para registro de operaciones.</param>
        /// <param name="serviceProvider">Proveedor de servicios para resolución de dependencias.</param>
        public TipoCedulaCacheService(ILogger<TipoCedulaCacheService> logger, IServiceProvider serviceProvider)
        {
            this._logger = logger;
            this._serviceProvider = serviceProvider;
        }

        /// <summary>
        /// Carga o recarga todos los tipos de cédula activos desde la base de datos.
        /// </summary>
        /// <param name="traceId">Identificador de trazabilidad.</param>
        public async Task LlenarCacheTiposCedulaAsync(string traceId)
        {
            string nombreMetodo = this.ObtenerNombreMetodo();
            try
            {
                this._logger.Inicio(traceId, nombreMetodo);
                List<TipoCedulaEntity> lista = new List<TipoCedulaEntity>();
                using var scope = this._serviceProvider.CreateScope();
                var repositorio = scope.ServiceProvider.GetRequiredService<IConsultarListaRepository>();
                int paginaActual = PAGINA_INICIAL;
                int totalPaginas = PAGINA_INICIAL;
                do
                {
                    var respuesta = await repositorio.ConsultarListaAsync<TipoCedulaEntity>(traceId, paginaActual);
                    totalPaginas = respuesta.CantidadPaginas;
                    paginaActual++;
                    lista.AddRange(respuesta.Lista);
                }
                while (paginaActual <= totalPaginas);
                lock (this._tiposCedulaCache)
                {
                    this._tiposCedulaCache.Clear();
                    this._tiposCedulaCache.AddRange(lista);
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
        /// Retorna todos los tipos de cédula activos almacenados en caché.
        /// </summary>
        /// <param name="traceId">Identificador de trazabilidad.</param>
        /// <returns>Lista de tipos de cédula activos.</returns>
        public List<TipoCedulaEntity> ObtenerTodos(string traceId)
        {
            string nombreMetodo = this.ObtenerNombreMetodo();
            try
            {
                this._logger.Inicio(traceId, nombreMetodo);
                return this._tiposCedulaCache.Where(t => t.Activo).ToList();
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
        /// Retorna el tipo de cédula activo con el identificador indicado, o null si no existe.
        /// </summary>
        /// <param name="traceId">Identificador de trazabilidad.</param>
        /// <param name="id">Identificador del tipo de cédula.</param>
        /// <returns>El tipo de cédula encontrado o null.</returns>
        public TipoCedulaEntity? ObtenerPorId(string traceId, int id)
        {
            string nombreMetodo = this.ObtenerNombreMetodo();
            try
            {
                this._logger.Inicio(traceId, nombreMetodo);
                return this._tiposCedulaCache.FirstOrDefault(t => t.Id == id && t.Activo);
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
