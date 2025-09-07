using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.Extensions.Logging;
using Microsoft.IdentityModel.Tokens;
using Japdeva.APIMovil.Common.Extensions;

namespace Japdeva.APIMovil.Common.Services
{
    /// <summary>
    /// Servicio para la validación de tokens JWT que implementa las funcionalidades
    /// de verificación y extracción de claims de tokens de acceso.
    /// </summary>
    public class ValidarTokenService : IValidarTokenService
    {
        private readonly ILogger<ValidarTokenService> _logger;

        /// <summary>
        /// Inicializa una nueva instancia del servicio de validación de tokens.
        /// </summary>
        /// <param name="logger">Logger para registro de eventos.</param>
        /// <exception cref="ArgumentNullException">Se lanza cuando el logger es null.</exception>
        public ValidarTokenService(ILogger<ValidarTokenService> logger)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        /// <summary>
        /// Valida un token JWT y extrae los claims si es válido.
        /// </summary>
        /// <param name="traceId">Identificador único para rastreo de la operación.</param>
        /// <param name="token">Token JWT a validar.</param>
        /// <param name="issuer">Emisor esperado del token JWT.</param>
        /// <param name="audience">Audiencia esperada del token JWT.</param>
        /// <param name="claveSecreta">Clave secreta para validar la firma del token.</param>
        /// <returns>Colección de claims si el token es válido, null si no es válido.</returns>
        /// <exception cref="ArgumentNullException">Se lanza cuando algún parámetro requerido es null.</exception>
        /// <exception cref="SecurityTokenException">Se lanza cuando el token no es válido.</exception>
        public IEnumerable<Claim>? ValidarToken(string traceId, string token, string issuer, string audience, string claveSecreta)
        {
            string nombreMetodo = nameof(ValidarToken);
            try
            {
                if (traceId is null) throw new ArgumentNullException(nameof(traceId));
                if (issuer is null) throw new ArgumentNullException(nameof(issuer));
                if (audience is null) throw new ArgumentNullException(nameof(audience));
                if (claveSecreta is null) throw new ArgumentNullException(nameof(claveSecreta));

                _logger.Inicio(traceId, nombreMetodo);

                if (string.IsNullOrEmpty(token)) throw new ArgumentNullException(nameof(token));

                JwtSecurityTokenHandler tokenHandler = new JwtSecurityTokenHandler();
                SymmetricSecurityKey claveSeguridad = ObtenerClaveSeguridad(traceId, claveSecreta);
                TokenValidationParameters parametrosValidacion = new TokenValidationParameters
                {
                    ValidateIssuer = true,
                    ValidateAudience = true,
                    ValidateLifetime = true,
                    ValidateIssuerSigningKey = true,
                    ValidIssuer = issuer,
                    ValidAudience = audience,
                    IssuerSigningKey = claveSeguridad,
                    ClockSkew = TimeSpan.Zero
                };

                ClaimsPrincipal principal = tokenHandler.ValidateToken(token, parametrosValidacion, out _);
                IEnumerable<Claim> claims = principal.Claims;
                return claims;
            }
            catch (SecurityTokenException ex)
            {
                _logger.Error(traceId, nombreMetodo, ex);
                throw;
            }
            catch (Exception ex)
            {
                _logger.Error(traceId, nombreMetodo, ex);
                throw;
            }
            finally
            {
                _logger.Fin(traceId, nombreMetodo);
            }
        }

        /// <summary>
        /// Obtiene la clave de seguridad utilizada para validar los tokens.
        /// </summary>
        /// <param name="traceId">Identificador único para rastreo de la operación.</param>
        /// <param name="claveSecreta">Clave secreta en formato string.</param>
        /// <returns>Clave de seguridad simétrica.</returns>
        /// <exception cref="ArgumentNullException">Se lanza cuando claveSecreta es null.</exception>
        public SymmetricSecurityKey ObtenerClaveSeguridad(string traceId, string claveSecreta)
        {
            string nombreMetodo = nameof(ObtenerClaveSeguridad);
            try
            {
                if (traceId is null) throw new ArgumentNullException(nameof(traceId));
                if (claveSecreta is null) throw new ArgumentNullException(nameof(claveSecreta));

                _logger.Inicio(traceId, nombreMetodo);
                byte[] bytesClaveSecreta = Encoding.UTF8.GetBytes(claveSecreta);
                return new SymmetricSecurityKey(bytesClaveSecreta);
            }
            catch (Exception ex)
            {
                _logger.Error(traceId, nombreMetodo, ex);
                throw;
            }
            finally
            {
                _logger.Fin(traceId, nombreMetodo);
            }
        }
    }
}
