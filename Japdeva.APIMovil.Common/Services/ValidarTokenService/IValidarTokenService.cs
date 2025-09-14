using System.Security.Claims;
using Microsoft.IdentityModel.Tokens;

namespace Japdeva.APIMovil.Common.Services
{
    /// <summary>
    /// Define los métodos para la validación de tokens JWT en la aplicación.
    /// </summary>
    public interface IValidarTokenService
    {
        /// <summary>
        /// Valida un token JWT y extrae los claims si es válido.
        /// </summary>
        /// <param name="traceId">Identificador único para rastreo de la operación.</param>
        /// <param name="token">Token JWT a validar.</param>
        /// <param name="issuer">Emisor esperado del token JWT.</param>
        /// <param name="audience">Audiencia esperada del token JWT.</param>
        /// <param name="claveSecreta">Clave secreta para validar la firma del token.</param>
        /// <returns>Colección de claims si el token es válido, null si no es válido.</returns>
        IEnumerable<Claim>? ValidarToken(string traceId, string token, string issuer, string audience, string claveSecreta);

        /// <summary>
        /// Obtiene la clave de seguridad utilizada para validar los tokens.
        /// </summary>
        /// <param name="traceId">Identificador único para rastreo de la operación.</param>
        /// <param name="claveSecreta">Clave secreta en formato string.</param>
        /// <returns>Clave de seguridad simétrica.</returns>
        SymmetricSecurityKey ObtenerClaveSeguridad(string traceId, string claveSecreta);
    }
}
