using Japdeva.APIMovil.Reclamos.Entities;

namespace Japdeva.APIMovil.Reclamos.Services.EstadoDetalleReclamoCacheService
{
    public interface IEstadoDetalleReclamoCacheService
    {
        Task LlenarCacheEstadoDetalleReclamoAsync(string traceId);
        EstadoDetalleReclamoEntity? ObtenerEstadoDetalleReclamo(string traceId, int idEstadoDetalleReclamo);
    }
}
