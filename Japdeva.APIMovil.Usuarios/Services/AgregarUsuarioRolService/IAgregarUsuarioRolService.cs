namespace Japdeva.APIMovil.Usuarios.Services.AgregarUsuarioRolService
{
    /// <summary>
    /// Contrato para el servicio de asignación de roles a usuarios.
    /// </summary>
    public interface IAgregarUsuarioRolService
    {
        /// <summary>
        /// Asigna un rol a un usuario.
        /// </summary>
        /// <param name="traceId">Identificador de trazabilidad.</param>
        /// <param name="idUsuario">Identificador del usuario.</param>
        /// <param name="idRol">Identificador del rol a asignar.</param>
        Task AgregarUsuarioRolAsync(string traceId, int idUsuario, int idRol);
    }
}
