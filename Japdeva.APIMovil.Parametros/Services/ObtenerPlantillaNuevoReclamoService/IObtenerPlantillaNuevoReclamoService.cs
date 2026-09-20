namespace Japdeva.APIMovil.Parametros.Services.ObtenerPlantillaNuevoReclamoService
{
    /// <summary>
    /// Servicio para obtener la plantilla de correo de nuevo reclamo con los datos personalizados.
    /// </summary>
    public interface IObtenerPlantillaNuevoReclamoService
    {
        /// <summary>
        /// Obtiene la plantilla de correo para nuevo reclamo, reemplazando el marcador @idReclamo por el valor proporcionado.
        /// </summary>
        /// <param name="traceId">Identificador de traza para el seguimiento de la operación.</param>
        /// <param name="idReclamo">Identificador del reclamo que se usará para personalizar la plantilla.</param>
        /// <returns>La plantilla de correo personalizada con el ID del reclamo.</returns>
        string ObtenerPlantillaNuevoReclamo(string traceId, long idReclamo);
    }
}
