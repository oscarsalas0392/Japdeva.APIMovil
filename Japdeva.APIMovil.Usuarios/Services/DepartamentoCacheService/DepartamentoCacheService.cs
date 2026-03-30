using Japdeva.APIMovil.Common.Extensions;
using Japdeva.APIMovil.Common.Repositories.ConsultarListaRepository;
using Japdeva.APIMovil.Usuarios.Entities;

namespace Japdeva.APIMovil.Usuarios.Services.DepartamentoCacheService
{
    /// <summary>
    /// Servicio de caché para los departamentos de la organización.
    /// Mantiene en memoria todos los departamentos activos para consulta sin acceso a base de datos.
    /// </summary>
    public class DepartamentoCacheService : IDepartamentoCacheService
    {
        private readonly ILogger<DepartamentoCacheService> _logger;
        private readonly IServiceProvider _serviceProvider;
        private readonly List<DepartamentoEntity> _departamentosCache = new List<DepartamentoEntity>();

        private const int PAGINA_INICIAL = 1;

        /// <summary>
        /// Inicializa el servicio de caché de departamentos.
        /// </summary>
        /// <param name="logger">Logger para registro de operaciones.</param>
        /// <param name="serviceProvider">Proveedor de servicios para resolución de dependencias.</param>
        public DepartamentoCacheService(ILogger<DepartamentoCacheService> logger, IServiceProvider serviceProvider)
        {
            this._logger = logger;
            this._serviceProvider = serviceProvider;
        }

        /// <summary>
        /// Carga o recarga todos los departamentos activos desde la base de datos.
        /// </summary>
        /// <param name="traceId">Identificador de trazabilidad.</param>
        public async Task LlenarCacheDepartamentosAsync(string traceId)
        {
            string nombreMetodo = this.ObtenerNombreMetodo();
            try
            {
                this._logger.Inicio(traceId, nombreMetodo);
                List<DepartamentoEntity> lista = new List<DepartamentoEntity>();
                using var scope = this._serviceProvider.CreateScope();
                var repositorio = scope.ServiceProvider.GetRequiredService<IConsultarListaRepository>();
                int paginaActual = PAGINA_INICIAL;
                int totalPaginas = PAGINA_INICIAL;
                do
                {
                    var respuesta = await repositorio.ConsultarListaAsync<DepartamentoEntity>(traceId, paginaActual);
                    totalPaginas = respuesta.CantidadPaginas;
                    paginaActual++;
                    lista.AddRange(respuesta.Lista);
                }
                while (paginaActual <= totalPaginas);
                lock (this._departamentosCache)
                {
                    this._departamentosCache.Clear();
                    this._departamentosCache.AddRange(lista);
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
        /// Retorna todos los departamentos activos almacenados en caché.
        /// </summary>
        /// <param name="traceId">Identificador de trazabilidad.</param>
        /// <returns>Lista de departamentos activos.</returns>
        public List<DepartamentoEntity> ObtenerTodos(string traceId)
        {
            string nombreMetodo = this.ObtenerNombreMetodo();
            try
            {
                this._logger.Inicio(traceId, nombreMetodo);
                return this._departamentosCache.Where(d => d.Activo).ToList();
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
        /// Retorna el departamento activo con el identificador indicado, o null si no existe.
        /// </summary>
        /// <param name="traceId">Identificador de trazabilidad.</param>
        /// <param name="id">Identificador del departamento.</param>
        /// <returns>El departamento encontrado o null.</returns>
        public DepartamentoEntity? ObtenerPorId(string traceId, int id)
        {
            string nombreMetodo = this.ObtenerNombreMetodo();
            try
            {
                this._logger.Inicio(traceId, nombreMetodo);
                return this._departamentosCache.FirstOrDefault(d => d.Id == id && d.Activo);
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
