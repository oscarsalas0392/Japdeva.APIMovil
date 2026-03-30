using Japdeva.APIMovil.Common.ColasGrpc;
using Japdeva.APIMovil.Common.Models;

namespace Japdeva.APIMovil.Common.Services.ColasGrpcClientMapperService
{
    /// <summary>
    /// Contrato para convertir mensajes protobuf al modelo de dominio ColaMensajeModel.
    /// </summary>
    public interface IColasGrpcClientMapperService
    {
        /// <summary>
        /// Convierte un mensaje protobuf al modelo de dominio ColaMensajeModel.
        /// </summary>
        /// <param name="traceId">Identificador de trazabilidad.</param>
        /// <param name="respuesta">Mensaje protobuf recibido del servidor gRPC.</param>
        /// <returns>Modelo de dominio mapeado.</returns>
        ColaMensajeModel ConvertirMapAModelo(string traceId, MensajeResponse respuesta);
    }
}
