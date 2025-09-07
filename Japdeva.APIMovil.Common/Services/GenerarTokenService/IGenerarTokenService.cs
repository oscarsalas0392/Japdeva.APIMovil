using Microsoft.IdentityModel.Tokens;

namespace Japdeva.APIMovil.Common.Services
{
    /// <summary>
    /// Define los métodos para generar tokens JWT y obtener claves de seguridad.
    /// </summary>
        public interface IGenerarTokenService
        {
        /// <summary>
        /// Genera un token JWT para el usuario especificado con el rol proporcionado.
        /// </summary>
        /// <param name="traceId">Identificador único para rastreo de la operación.</param>
        /// <param name="issuer">Emisor del token JWT.</param>
        /// <param name="audience">Audiencia del token JWT.</param>
        /// <param name="claveSecreta">Clave secreta para firmar el token.</param>
        /// <param name="role">Rol del usuario que se incluirá en el token.</param>
        /// <returns>Token JWT como string.</returns>
        string GenerarToken(string traceId, string issuer, string audience, string claveSecreta, string role);

        /// <summary>
        /// Obtiene la clave de seguridad utilizada para firmar los tokens.
        /// </summary>
        /// <param name="traceId">Identificador único para rastreo de la operación.</param>
        /// <param name="claveSecreta">Clave secreta en formato string.</param>
        /// <returns>Clave de seguridad simétrica.</returns>
        SymmetricSecurityKey ObtenerClaveSeguridad(string traceId, string claveSecreta);
    }
}
