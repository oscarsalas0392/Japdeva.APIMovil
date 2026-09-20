using Japdeva.APIMovil.Common.Extensions;
using Japdeva.APIMovil.Common.Repositories.ActualizarRepository;
using Japdeva.APIMovil.Common.Repositories.ConsultarRepository;
using Japdeva.APIMovil.Reclamos.Entities;
using Japdeva.APIMovil.Reclamos.Services.EstadoReclamoCacheService;

namespace Japdeva.APIMovil.Reclamos.Services.EditarReclamoService
{
    /// <summary>
    /// Servicio para editar un reclamo existente, permitiendo actualizar su descripción de resolución y estado.
    /// </summary>
    public class EditarReclamoService : IEditarReclamoService
    {
        private readonly ILogger<EditarReclamoService> _logger;
        private readonly IServiceProvider _serviceProvider;
        private readonly IEstadoReclamoCacheService _estadoReclamoCacheService;

        private const string MENSAJE_ERROR_RECLAMO_NO_ENCONTRADO = "El reclamo con Id {0} no fue encontrado.";
        private const string MENSAJE_ERROR_ESTADO_RECLAMO_NO_ENCONTRADO = "El estado del reclamo con Id {0} no fue encontrado.";

        /// <summary>
        /// Inicializa una nueva instancia de la clase <see cref="EditarReclamoService"/>.
        /// </summary>
        /// <param name="logger">Instancia del registrador de logs.</param>
        /// <param name="serviceProvider">Proveedor de servicios para la obtención de dependencias.</param>
        /// <param name="estadoReclamoCacheService">Servicio de caché de estados de reclamo.</param>
        public EditarReclamoService(
            ILogger<EditarReclamoService> logger,
            IServiceProvider serviceProvider,
            IEstadoReclamoCacheService estadoReclamoCacheService)
        {
            this._logger = logger;
            this._serviceProvider = serviceProvider;
            this._estadoReclamoCacheService = estadoReclamoCacheService;
        }

        /// <summary>
        /// Edita un reclamo existente actualizando su descripción de resolución y estado.
        /// </summary>
        /// <param name="traceId">Identificador de traza para el seguimiento de la operación.</param>
        /// <param name="idReclamo">Identificador único del reclamo a editar.</param>
        /// <param name="descripcionResolucion">Nueva descripción de la resolución del reclamo.</param>
        /// <param name="idEstadoReclamo">Nuevo identificador del estado del reclamo.</param>
        /// <exception cref="Exception">Se lanza si el reclamo o el estado no son encontrados.</exception>
        public async Task EditarReclamoAsync(string traceId, long idReclamo, string descripcionResolucion, int idEstadoReclamo)
        {
            string nombreMetodo = this.ObtenerNombreMetodo();
            try
            {
                this._logger.Inicio(traceId, nombreMetodo);

                var estadoReclamo = this._estadoReclamoCacheService.ObtenerEstadoReclamo(traceId, idEstadoReclamo);
                if (estadoReclamo is null) throw new Exception(string.Format(MENSAJE_ERROR_ESTADO_RECLAMO_NO_ENCONTRADO, idEstadoReclamo));

                using var scope = this._serviceProvider.CreateScope();
                var consultarRepository = scope.ServiceProvider.GetRequiredService<IConsultarRepository>();
                var actualizarRepository = scope.ServiceProvider.GetRequiredService<IActualizarRepository>();

                var reclamo = await consultarRepository.ConsultarAsync<ReclamoEntity>(traceId, x => x.Id == idReclamo);
                if (reclamo is null) throw new Exception(string.Format(MENSAJE_ERROR_RECLAMO_NO_ENCONTRADO, idReclamo));

                reclamo.DescripcionResolucion = descripcionResolucion;
                reclamo.IdEstadoReclamo = estadoReclamo.Id;
                await actualizarRepository.ActualizarAsync<ReclamoEntity>(traceId, reclamo);
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
