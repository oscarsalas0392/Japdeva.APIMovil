using Microsoft.Extensions.Logging;
using Japdeva.APIMovil.Common.ColasGrpc;
using Japdeva.APIMovil.Common.Extensions;
using Japdeva.APIMovil.Common.Models;

namespace Japdeva.APIMovil.Common.Services.ColasGrpcClientMapperService
{
    /// <summary>
    /// Servicio auxiliar del cliente gRPC de Colas.
    /// Convierte mensajes protobuf al modelo de dominio ColaMensajeModel.
    /// </summary>
    public class ColasGrpcClientMapperService : IColasGrpcClientMapperService
    {
        private readonly ILogger<ColasGrpcClientMapperService> _logger;

        /// <summary>
        /// Inicializa el servicio mapper con el logger de la aplicación.
        /// </summary>
        /// <param name="logger">Logger para registro de operaciones.</param>
        public ColasGrpcClientMapperService(ILogger<ColasGrpcClientMapperService> logger)
        {
            this._logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        /// <summary>
        /// Convierte un mensaje protobuf al modelo de dominio ColaMensajeModel.
        /// </summary>
        /// <param name="traceId">Identificador de trazabilidad.</param>
        /// <param name="respuesta">Mensaje protobuf recibido del servidor gRPC.</param>
        /// <returns>Modelo de dominio mapeado.</returns>
        public ColaMensajeModel ConvertirMapAModelo(string traceId, MensajeResponse respuesta)
        {
            string nombreMetodo = this.ObtenerNombreMetodo();
            try
            {
                this._logger.Inicio(traceId, nombreMetodo);
                ColaMensajeModel colaMensajeModel = new ColaMensajeModel
                {
                    Id = respuesta.Id,
                    IdRpc = respuesta.IdRpc,
                    ColaId = respuesta.Cola,
                    TraceId = respuesta.TraceId,
                    Contenido = respuesta.Mensaje,
                    Estado = respuesta.Estado,
                    MetaDatos = respuesta.MetaDatos,
                    TraceIdDiferente = respuesta.TraceIdDiferente
                };
                return colaMensajeModel;
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
