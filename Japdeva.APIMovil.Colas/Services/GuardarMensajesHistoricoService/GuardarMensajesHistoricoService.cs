using Microsoft.Extensions.Logging;
using Japdeva.APIMovil.Colas.Entities;
using Japdeva.APIMovil.Colas.Models;
using Japdeva.APIMovil.Common.Extensions;
using Japdeva.APIMovil.Common.Repositories.AgregarRepository;
using Japdeva.APIMovil.Common.Repositories.ConsultarListaRepository;
using Japdeva.APIMovil.Common.Repositories.ConsultarRepository;
using Japdeva.APIMovil.Common.Repositories.EliminarRepository;

namespace Japdeva.APIMovil.Colas.Services.GuardarMensajesHistoricoService
{
    /// <summary>
    /// Servicio para gestionar el histórico de mensajes en colas del sistema.
    /// </summary>
    public class GuardarMensajesHistoricoService : IGuardarMensajesHistoricoService
    {
        private readonly IConsultarListaRepository _consultarListaRepository;
        private readonly IConsultarRepository _consultarRepository;
        private readonly IAgregarRepository _agregarRepository;
        private readonly IEliminarRepository _eliminarRepository;
        private readonly ILogger<GuardarMensajesHistoricoService> _logger;
        private const int PAGINA_INICIAL = 1;

        /// <summary>
        /// Inicializa una nueva instancia de la clase GuardarMensajesHistoricoService.
        /// </summary>
        /// <param name="consultarListaRepository">Repositorio para consultas de listas de datos.</param>
        /// <param name="consultarRepository">Repositorio para consultas individuales.</param>
        /// <param name="agregarRepository"></param>
        /// <param name="eliminarRepository"></param>
        /// <param name="logger">Instancia de logger para registrar eventos.</param>
        public GuardarMensajesHistoricoService(
            IConsultarListaRepository consultarListaRepository,
            IConsultarRepository consultarRepository,
            IAgregarRepository agregarRepository,
            IEliminarRepository eliminarRepository,
            ILogger<GuardarMensajesHistoricoService> logger)
        {
            this._consultarListaRepository = consultarListaRepository;
            this._consultarRepository = consultarRepository;
            this._agregarRepository = agregarRepository;
            this._eliminarRepository = eliminarRepository;
            this._logger = logger;
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

                int pagina = PAGINA_INICIAL;
                var mensajes = await this._consultarListaRepository.ConsultarListaAsync<MensajeColaEntity>(
                    traceId, pagina, mensaje => mensaje.EstadoId == (int)EstadoMensajeModel.Procesado ||
                                              mensaje.EstadoId == (int)EstadoMensajeModel.Cancelado);

                if (mensajes is null || !mensajes.Lista.Any()) return;

                foreach (var mensaje in mensajes.Lista)
                {
                    MensajeColaHistoricoEntity mensajeColaHistoricoEntity = new MensajeColaHistoricoEntity();
                    mensajeColaHistoricoEntity.Id = mensaje.Id;
                    mensajeColaHistoricoEntity.ColaId = mensaje.ColaId;
                    mensajeColaHistoricoEntity.ContadorReintentos = mensaje.ContadorReintentos;
                    mensajeColaHistoricoEntity.ContenidoMensaje = mensaje.ContenidoMensaje;
                    mensajeColaHistoricoEntity.EstadoId = mensaje.EstadoId;
                    mensajeColaHistoricoEntity.FechaEdicion = mensaje.FechaEdicion;
                    mensajeColaHistoricoEntity.FechaRegistro = mensaje.FechaRegistro;
                    mensajeColaHistoricoEntity.MaximoReintentos = mensaje.MaximoReintentos;
                    mensajeColaHistoricoEntity.MensajeError = mensaje.MensajeError;
                    mensajeColaHistoricoEntity.Prioridad = mensaje.Prioridad;
                    mensajeColaHistoricoEntity.Metadatos = mensaje.Metadatos;
                    mensajeColaHistoricoEntity.ProximoReintento = mensaje.ProximoReintento;
                    mensajeColaHistoricoEntity.TraceId = mensaje.TraceId;
                    mensajesHistorico.Add(mensajeColaHistoricoEntity);
                }
                
                await this._agregarRepository.AgregarVariosAsync<MensajeColaHistoricoEntity>(traceId, mensajesHistorico);
                await this._eliminarRepository.EliminarAsync<MensajeColaEntity>(traceId, mensajes.Lista);
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