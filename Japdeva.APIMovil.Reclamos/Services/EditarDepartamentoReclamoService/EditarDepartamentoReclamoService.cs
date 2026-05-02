using Japdeva.APIMovil.Common.Extensions;
using Japdeva.APIMovil.Common.Repositories.ActualizarRepository;
using Japdeva.APIMovil.Common.Repositories.ConsultarRepository;
using Japdeva.APIMovil.Reclamos.Entities;

namespace Japdeva.APIMovil.Reclamos.Services.EditarDepartamentoReclamoService
{
    /// <summary>
    /// Servicio para editar el departamento asociado a un reclamo.
    /// </summary>
    public class EditarDepartamentoReclamoService : IEditarDepartamentoReclamoService
    {
        private readonly ILogger<EditarDepartamentoReclamoService> _logger;
        private readonly IServiceProvider _serviceProvider;
        private const string MENSAJE_ERROR_DEPARTAMENTO_RECLAMO_NO_ENCONTRADO = "El reclamo con Id {0} no fue encontrado.";
        /// <summary>
        /// Inicializa una nueva instancia de la clase <see cref="EditarDepartamentoReclamoService"/>.
        /// </summary>
        public EditarDepartamentoReclamoService(
            ILogger<EditarDepartamentoReclamoService> logger,
            IServiceProvider serviceProvider)
        {
            this._logger = logger;
            this._serviceProvider = serviceProvider;
        }

        /// <summary>
        /// Edita el departamento asociado a un reclamo existente.
        /// </summary>
        /// <param name="traceId">Identificador de la traza para el registro de logs.</param>
        /// <param name="idReclamo">Identificador del reclamo a modificar.</param>
        /// <param name="idDepartamentoReclamo">Identificador del nuevo departamento a asociar al reclamo.</param>
        /// <exception cref="Exception">Se lanza si el reclamo no es encontrado.</exception>
        public async Task EditarDepartamentoReclamoAsync(string traceId, long idReclamo, long idDepartamentoReclamo)
        {
            string nombreMetodo = this.ObtenerNombreMetodo();
            try
            {
                this._logger.Inicio(traceId, nombreMetodo);
                using var scope = this._serviceProvider.CreateScope();
                var consultarRepository = scope.ServiceProvider.GetRequiredService<IConsultarRepository>();
                var actualizarRepository = scope.ServiceProvider.GetRequiredService<IActualizarRepository>();
                var reclamo = await consultarRepository.ConsultarAsync<ReclamoEntity>(traceId, x => x.Id == idReclamo);
                if (reclamo is null) throw new Exception(string.Format(MENSAJE_ERROR_DEPARTAMENTO_RECLAMO_NO_ENCONTRADO, idReclamo));

                reclamo.IdDepartamentoActual = idDepartamentoReclamo;
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
