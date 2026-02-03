namespace Japdeva.APIMovil.Parametros.Models
{
    /// <summary>
    /// Enumeración que define los tipos de plantillas de correo disponibles en el sistema.
    /// </summary>
    public enum TipoPlantillaCorreoModel
    {
        /// <summary>
        /// Plantilla para notificar la creación de un nuevo reclamo.
        /// </summary>
        NuevoReclamo = 1,
        NuevoReclamoDepartamento = 2,
        FinalizacionProcesoReclamo=3,
        RechazoReclamo=4,
        CambioContrasena=5


    }
}
