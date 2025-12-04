using Japdeva.APIMovil.Reclamos.Entities;

namespace Japdeva.APIMovil.Reclamos.Services.EstadoReclamoCacheService
{
    public interface IEstadoReclamoCacheService
    {
        Task LlenarCacheEstadoReclamoAsync(string traceId);
        EstadoReclamoEntity? ObtenerEstadoReclamo(string traceId, int idEstadoReclamo);
    }
}
