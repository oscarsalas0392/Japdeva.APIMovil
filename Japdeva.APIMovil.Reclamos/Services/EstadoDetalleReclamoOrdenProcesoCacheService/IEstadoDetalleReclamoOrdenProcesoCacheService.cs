using Japdeva.APIMovil.Reclamos.Entities;

namespace Japdeva.APIMovil.Reclamos.Services.EstadoDetalleReclamoOrdenProcesoCacheService
{
    public interface IEstadoDetalleReclamoOrdenProcesoCacheService
    {
        Task LlenarCacheEstadoDetalleReclamoOrdenProcesoAsync(string traceId);
        EstadoDetalleReclamoOrdenProcesoEntity? ObtenerEstadoDetalleReclamoOrdenProceso(string traceId, int idEstadoDetalleReclamo);
    }
}
