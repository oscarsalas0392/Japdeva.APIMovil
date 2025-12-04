using Japdeva.APIMovil.Reclamos.Entities;

namespace Japdeva.APIMovil.Reclamos.Services.DevolucionProcesoCacheService
{
    public interface IDevolucionProcesoCacheService
    {
        Task LlenarCacheDevolucionProcesoAsync(string traceId);
        DevolucionProcesoEntity? ObtenerDevolucionProceso(string traceId, int idDevolucionProceso);
    }
}
