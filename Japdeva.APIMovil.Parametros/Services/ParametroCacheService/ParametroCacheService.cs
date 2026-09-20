using Japdeva.APIMovil.Common.Extensions;
using Japdeva.APIMovil.Common.Repositories.ConsultarListaRepository;
using Japdeva.APIMovil.Parametros.Entities;

namespace Japdeva.APIMovil.Parametros.Services.ParametroCacheService
{
    /// <summary>
    /// Servicio para gestionar la caché de parámetros del sistema en memoria, obteniendo los datos
    /// desde el repositorio y permitiendo su acceso eficiente.
    /// </summary>
    public class ParametroCacheService : IParametroCacheService
    {
        private readonly ILogger<ParametroCacheService> _logger;
        private readonly IServiceProvider _serviceProvider;
        private readonly List<ParametroEntity> _parametroCache = new List<ParametroEntity>();

        private const int PAGINA_INICIAL = 1;

        /// <summary>
        /// Inicializa una nueva instancia de la clase <see cref="ParametroCacheService"/>.
        /// </summary>
        /// <param name="logger">Instancia del registrador de logs.</param>
        /// <param name="serviceProvider">Proveedor de servicios para la obtención de dependencias.</param>
        public ParametroCacheService(ILogger<ParametroCacheService> logger, IServiceProvider serviceProvider)
        {
            this._logger = logger;
            this._serviceProvider = serviceProvider;
        }

        /// <summary>
        /// Llena la caché de parámetros obteniendo la información desde el repositorio.
        /// </summary>
        /// <param name="traceId">Identificador de traza para el seguimiento de la operación.</param>
        public async Task LlenarCacheParametrosAsync(string traceId)
        {
            string nombreMetodo = this.ObtenerNombreMetodo();
            try
            {
                this._logger.Inicio(traceId, nombreMetodo);
                List<ParametroEntity> listaParametros = new List<ParametroEntity>();
                using var scope = this._serviceProvider.CreateScope();
                var consultarListaEntity = scope.ServiceProvider.GetRequiredService<IConsultarListaRepository>();
                int paginaActual = PAGINA_INICIAL;
                int totalPaginas = PAGINA_INICIAL;

                do
                {
                    var respuestaLista = await consultarListaEntity.ConsultarListaAsync<ParametroEntity>(traceId, paginaActual);
                    totalPaginas = respuestaLista.CantidadPaginas;
                    paginaActual++;
                    listaParametros.AddRange(respuestaLista.Lista);
                }
                while (paginaActual <= totalPaginas);

                lock (this._parametroCache)
                {
                    this._parametroCache.Clear();
                    this._parametroCache.AddRange(listaParametros);
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
        /// Obtiene un parámetro activo por su nombre.
        /// </summary>
        /// <param name="traceId">Identificador de traza para el seguimiento de la operación.</param>
        /// <param name="nombre">Nombre del parámetro a obtener.</param>
        /// <returns>El parámetro si existe y está activo, null en caso contrario.</returns>
        public ParametroEntity? ObtenerParametroPorNombre(string traceId, string nombre)
        {
            string nombreMetodo = this.ObtenerNombreMetodo();
            try
            {
                this._logger.Inicio(traceId, nombreMetodo);
                return this._parametroCache.FirstOrDefault(p => p.Nombre.Equals(nombre, StringComparison.OrdinalIgnoreCase) && p.Activo);
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
