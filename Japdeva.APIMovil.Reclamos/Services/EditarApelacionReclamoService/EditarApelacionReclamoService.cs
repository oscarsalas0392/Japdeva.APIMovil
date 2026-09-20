using Japdeva.APIMovil.Common.Extensions;
using Japdeva.APIMovil.Common.Repositories.ActualizarRepository;
using Japdeva.APIMovil.Common.Repositories.ConsultarRepository;
using Japdeva.APIMovil.Reclamos.Entities;

namespace Japdeva.APIMovil.Reclamos.Services.EditarApelacionReclamoService
{
    /// <summary>
    /// Servicio para editar una apelación existente, actualizando su descripción de resolución y estado.
    /// </summary>
    public class EditarApelacionReclamoService : IEditarApelacionReclamoService
    {
        private readonly ILogger<EditarApelacionReclamoService> _logger;
        private readonly IServiceProvider _serviceProvider;

        private const string MENSAJE_ERROR_APELACION_NO_ENCONTRADA = "La apelación con Id {0} no fue encontrada.";
        private const string MENSAJE_ERROR_ESTADO_RECLAMO_NO_ENCONTRADO = "El estado del reclamo con Id {0} no fue encontrado.";
        private const bool ESTADO_RECLAMO_ACTIVO = true;

        /// <summary>
        /// Inicializa una nueva instancia de la clase <see cref="EditarApelacionReclamoService"/>.
        /// </summary>
        public EditarApelacionReclamoService(
            ILogger<EditarApelacionReclamoService> logger,
            IServiceProvider serviceProvider)
        {
            this._logger = logger;
            this._serviceProvider = serviceProvider;
        }

        /// <summary>
        /// Edita una apelación existente actualizando su descripción de resolución y estado.
        /// </summary>
        /// <param name="traceId">Identificador único para rastreo de la operación.</param>
        /// <param name="idApelacionReclamo">Identificador único de la apelación a editar.</param>
        /// <param name="descripcionResolucion">Nueva descripción de la resolución.</param>
        /// <param name="idEstadoReclamo">Nuevo identificador del estado.</param>
        public async Task EditarApelacionReclamoAsync(string traceId, long idApelacionReclamo, string descripcionResolucion, int idEstadoReclamo)
        {
            string nombreMetodo = this.ObtenerNombreMetodo();
            try
            {
                this._logger.Inicio(traceId, nombreMetodo);
                using var scope = this._serviceProvider.CreateScope();
                var consultarRepository = scope.ServiceProvider.GetRequiredService<IConsultarRepository>();
                var actualizarRepository = scope.ServiceProvider.GetRequiredService<IActualizarRepository>();

                var apelacion = await consultarRepository.ConsultarAsync<ApelacionReclamoEntity>(traceId, x => x.Id == idApelacionReclamo);
                if (apelacion is null) throw new Exception(string.Format(MENSAJE_ERROR_APELACION_NO_ENCONTRADA, idApelacionReclamo));

                var estadoReclamo = await consultarRepository.ConsultarAsync<EstadoReclamoEntity>(traceId, x => x.Id == idEstadoReclamo && x.Activo == ESTADO_RECLAMO_ACTIVO);
                if (estadoReclamo is null) throw new Exception(string.Format(MENSAJE_ERROR_ESTADO_RECLAMO_NO_ENCONTRADO, idEstadoReclamo));

                apelacion.DescripcionResolucion = descripcionResolucion;
                apelacion.IdEstadoReclamo = estadoReclamo.Id;
                await actualizarRepository.ActualizarAsync<ApelacionReclamoEntity>(traceId, apelacion);
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
