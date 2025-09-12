using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.Extensions.Logging;
using Microsoft.IdentityModel.Tokens;
using Japdeva.APIMovil.Common.Extensions;

namespace Japdeva.APIMovil.Common.Services
{
    /// <summary>
    /// Servicio para la generación de tokens JWT que implementa las funcionalidades
    /// de creación de tokens de acceso con roles específicos.
    /// </summary>
    public class GenerarTokenService : IGenerarTokenService
    {
        private readonly ILogger<GenerarTokenService> _logger;
        private const string ALGORITMO_SEGURIDAD = SecurityAlgorithms.HmacSha256;
        private const int TIEMPO_EXPIRACION_DEFAULT = 60;

        /// <summary>
        /// Inicializa una nueva instancia del servicio de generación de tokens.
        /// </summary>
        /// <param name="logger">Logger para registro de eventos.</param>
        /// <exception cref="ArgumentNullException">Se lanza cuando el logger es null.</exception>
        public GenerarTokenService(ILogger<GenerarTokenService> logger)
        {
            _logger = logger;
        }

        /// <summary>
        /// Genera un token JWT para el usuario especificado con el rol proporcionado.
        /// </summary>
        /// <param name="traceId">Identificador único para rastreo de la operación.</param>
        /// <param name="issuer">Emisor del token JWT.</param>
        /// <param name="audience">Audiencia del token JWT.</param>
        /// <param name="claveSecreta">Clave secreta para firmar el token.</param>
        /// <param name="role">Rol del usuario que se incluirá en el token.</param>
        /// <returns>Token JWT como string.</returns>
        /// <exception cref="ArgumentNullException">Se lanza cuando algún parámetro es null.</exception>
        public string GenerarToken(string traceId, string issuer, string audience, string claveSecreta, string role)
        {
            string nombreMetodo = this.ObtenerNombreMetodo();
            try
            {
                this._logger.Inicio(traceId, nombreMetodo);
                if (traceId is null) throw new ArgumentNullException(nameof(traceId));
                if (issuer is null) throw new ArgumentNullException(nameof(issuer));
                if (audience is null) throw new ArgumentNullException(nameof(audience));
                if (claveSecreta is null) throw new ArgumentNullException(nameof(claveSecreta));
                if (role is null) throw new ArgumentNullException(nameof(role));

                List<Claim> claimsValidos = [new Claim(ClaimTypes.Role, role)];
                int tiempoExpiracion = TIEMPO_EXPIRACION_DEFAULT;

                SymmetricSecurityKey claveSeguridad = ObtenerClaveSeguridad(traceId, claveSecreta);
                SigningCredentials credencialesSignado = new SigningCredentials(claveSeguridad, ALGORITMO_SEGURIDAD);

                DateTime fechaExpiracion = DateTime.UtcNow.AddMinutes(tiempoExpiracion);

                SecurityTokenDescriptor tokenDescriptor = new SecurityTokenDescriptor();
                tokenDescriptor.Subject = new ClaimsIdentity(claimsValidos);
                tokenDescriptor.Expires = fechaExpiracion;
                tokenDescriptor.Issuer = issuer;
                tokenDescriptor.Audience = audience;
                tokenDescriptor.SigningCredentials = credencialesSignado;
            

                JwtSecurityTokenHandler tokenHandler = new JwtSecurityTokenHandler();
                SecurityToken token = tokenHandler.CreateToken(tokenDescriptor);
                string tokenString = tokenHandler.WriteToken(token);

                return tokenString;
            }
            catch (Exception ex)
            {
                this._logger.Error(traceId, nombreMetodo, ex);
                throw;
            }
            finally
            {
                this._logger.Fin(traceId, nombreMetodo);
            }
        }

        /// <summary>
        /// Obtiene la clave de seguridad utilizada para firmar los tokens.
        /// </summary>
        /// <param name="traceId">Identificador único para rastreo de la operación.</param>
        /// <param name="claveSecreta">Clave secreta en formato string.</param>
        /// <returns>Clave de seguridad simétrica.</returns>
        /// <exception cref="ArgumentNullException">Se lanza cuando claveSecreta es null.</exception>
        public SymmetricSecurityKey ObtenerClaveSeguridad(string traceId, string claveSecreta)
        {
            string nombreMetodo = this.ObtenerNombreMetodo();
            try
            {
                this._logger.Inicio(traceId, nombreMetodo);
                if (traceId is null) throw new ArgumentNullException(nameof(traceId));
                if (claveSecreta is null) throw new ArgumentNullException(nameof(claveSecreta));
                byte[] bytesClaveSecreta = Encoding.UTF8.GetBytes(claveSecreta);
                return new SymmetricSecurityKey(bytesClaveSecreta);
            }
            catch (Exception ex)
            {
                this._logger.Error(traceId, nombreMetodo, ex);
                throw;
            }
            finally
            {
                this._logger.Fin(traceId, nombreMetodo);
            }
        }
    }
}
