namespace Japdeva.APIMovil.Parametros.Models
{
    /// <summary>
    /// Enumeración que define los tipos de plantillas de correo disponibles en el sistema.
    /// </summary>
    public enum TipoPlantillaCorreoModel
    {
        /// <summary>
        /// Plantilla para notificar al usuario el envío de una contraseña temporal.
        /// </summary>
        CambioContrasena = 1,

        /// <summary>
        /// Plantilla para notificar al usuario externo el registro exitoso de su reclamo.
        /// </summary>
        NuevoReclamo = 2,

        /// <summary>
        /// Plantilla para notificar internamente al departamento sobre un nuevo reclamo ingresado.
        /// </summary>
        NuevoReclamoDepartamento = 3,

        /// <summary>
        /// Plantilla para notificar al usuario la resolución de su reclamo.
        /// Aplica tanto para finalización como para rechazo, distinguidos por la variable {{ESTADO}}.
        /// </summary>
        FinalizacionProcesoReclamo = 4,

        /// <summary>
        /// Alias de <see cref="FinalizacionProcesoReclamo"/>. Usa la misma plantilla con {{ESTADO}} = Rechazado.
        /// </summary>
        RechazoReclamo = 4
    }
}
