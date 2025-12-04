using Japdeva.APIMovil.Reclamos.Entities;

namespace Japdeva.APIMovil.Reclamos.Services.OrdenProcesoCacheService
{
    public interface IOrdenProcesoCacheService
    {
        Task LlenarCacheOrdenProcesoAsync(string traceId);
        OrdenProcesoEntity? ObtenerOrdenProceso(string traceId, int idOrdenProceso);
    }
}
