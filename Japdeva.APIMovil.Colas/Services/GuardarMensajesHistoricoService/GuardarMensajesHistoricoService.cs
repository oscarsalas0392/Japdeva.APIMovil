using Microsoft.Extensions.Logging;
using Japdeva.APIMovil.Colas.Entities;
using Japdeva.APIMovil.Colas.Models;
using Japdeva.APIMovil.Common.Extensions;
using Japdeva.APIMovil.Common.Repositories.AgregarRepository;
using Japdeva.APIMovil.Common.Repositories.ConsultarListaRepository;   
using Japdeva.APIMovil.Common.Repositories.EliminarRepository;

namespace Japdeva.APIMovil.Colas.Services.GuardarMensajesHistoricoService
{
    /// <summary>
    /// Servicio para gestionar el histórico de mensajes en colas del sistema.
    /// </summary>
    public class GuardarMensajesHistoricoService : IGuardarMensajesHistoricoService
    {

        private readonly ILogger<GuardarMensajesHistoricoService> _logger;
        private readonly IServiceProvider _serviceProvider;
        private const int PAGINA_INICIAL = 1;
        /// <summary>
        /// Inicializa una nueva instancia de la clase GuardarMensajesHistoricoService.
        /// </summary>
        /// <param name="serviceProvider">Proveedor de servicios para la inyección de dependencias.</param>
        /// <param name="logger">Instancia de logger para registrar eventos.</param>
        public GuardarMensajesHistoricoService(
            IServiceProvider serviceProvider,
            ILogger<GuardarMensajesHistoricoService> logger)
        {
            this._logger = logger;
            this._serviceProvider = serviceProvider;
        }

        /// <summary>
        /// Mueve mensajes procesados y cancelados al histórico para mejorar el rendimiento.
        /// </summary>
        /// <param name="traceId">Identificador de trazabilidad para el seguimiento de la operación.</param>
        public async Task MoverMensajesAHistoricoAsync(string traceId)
        {
            string nombreMetodo = this.ObtenerNombreMetodo();
            try
            {
                this._logger.Inicio(traceId, nombreMetodo);
                List<MensajeColaHistoricoEntity> mensajesHistorico = new List<MensajeColaHistoricoEntity>();

                using var scope = this._serviceProvider.CreateScope();
                var consultarListaRepository = scope.ServiceProvider.GetRequiredService<IConsultarListaRepository>();
                var agregarRepository = scope.ServiceProvider.GetRequiredService<IAgregarRepository>();
                var eliminarRepository = scope.ServiceProvider.GetRequiredService<IEliminarRepository>();
                int pagina = PAGINA_INICIAL;
                var mensajes = await consultarListaRepository.ConsultarListaAsync<MensajeColaEntity>(
                    traceId, pagina, mensaje => mensaje.EstadoId == (int)EstadoMensajeModel.Procesado ||
                                              mensaje.EstadoId == (int)EstadoMensajeModel.Cancelado);

                if (mensajes is null || !mensajes.Lista.Any()) return;

                foreach (var mensaje in mensajes.Lista)
                {
                    MensajeColaHistoricoEntity mensajeColaHistoricoEntity = new MensajeColaHistoricoEntity();
                    mensajeColaHistoricoEntity.IdMensajeCola = mensaje.Id;
                    mensajeColaHistoricoEntity.ColaId = mensaje.ColaId;
                    mensajeColaHistoricoEntity.ContadorReintentos = mensaje.ContadorReintentos;
                    mensajeColaHistoricoEntity.ContenidoMensaje = mensaje.ContenidoMensaje;
                    mensajeColaHistoricoEntity.EstadoId = mensaje.EstadoId;
                    mensajeColaHistoricoEntity.FechaEdicion = mensaje.FechaEdicion;
                    mensajeColaHistoricoEntity.FechaRegistro = mensaje.FechaRegistro;
                    mensajeColaHistoricoEntity.MensajeError = mensaje.MensajeError;
                    mensajeColaHistoricoEntity.PrioridadId = mensaje.PrioridadId;
                    mensajeColaHistoricoEntity.Metadatos = mensaje.Metadatos;
                    mensajeColaHistoricoEntity.TraceId = mensaje.TraceId;
                    mensajeColaHistoricoEntity.FechaArchivado = DateTime.UtcNow;
                    mensajesHistorico.Add(mensajeColaHistoricoEntity);
                }

                await agregarRepository.AgregarVariosAsync<MensajeColaHistoricoEntity>(traceId, mensajesHistorico);
                await eliminarRepository.EliminarAsync<MensajeColaEntity>(traceId, mensajes.Lista);
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