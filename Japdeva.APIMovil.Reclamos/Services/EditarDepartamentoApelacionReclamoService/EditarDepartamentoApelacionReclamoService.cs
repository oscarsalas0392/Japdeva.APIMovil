using Japdeva.APIMovil.Common.Extensions;
using Japdeva.APIMovil.Common.Repositories.ActualizarRepository;
using Japdeva.APIMovil.Common.Repositories.ConsultarRepository;
using Japdeva.APIMovil.Reclamos.Entities;

namespace Japdeva.APIMovil.Reclamos.Services.EditarDepartamentoApelacionReclamoService
{
    /// <summary>
    /// Servicio para actualizar el departamento asignado a una apelación de reclamo.
    /// </summary>
    public class EditarDepartamentoApelacionReclamoService : IEditarDepartamentoApelacionReclamoService
    {
        private readonly ILogger<EditarDepartamentoApelacionReclamoService> _logger;
        private readonly IServiceProvider _serviceProvider;

        private const string MENSAJE_ERROR_APELACION_NO_ENCONTRADA = "La apelación con Id {0} no fue encontrada.";

        /// <summary>
        /// Inicializa una nueva instancia de la clase <see cref="EditarDepartamentoApelacionReclamoService"/>.
        /// </summary>
        public EditarDepartamentoApelacionReclamoService(
            ILogger<EditarDepartamentoApelacionReclamoService> logger,
            IServiceProvider serviceProvider)
        {
            this._logger = logger;
            this._serviceProvider = serviceProvider;
        }

        /// <summary>
        /// Actualiza el departamento actualmente asignado a una apelación.
        /// </summary>
        public async Task EditarDepartamentoApelacionReclamoAsync(string traceId, long idApelacionReclamo, long idDepartamento)
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

                apelacion.IdDepartamentoActual = idDepartamento;
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
