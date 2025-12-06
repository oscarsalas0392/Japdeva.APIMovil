using System;
using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Tokens;

namespace Japdeva.APIMovil.Common.Extensions
{
    /// <summary>
    /// Proporciona métodos de extensión para configurar la autenticación JWT.
    /// </summary>
    public static class JwtAuthenticationExtension
    {
        private const string JWT_ISSUER_ENV = "ISSUER";
        private const string JWT_AUDIENCE_ENV = "AUDIENCE";
        private const string JWT_CLAVE_SECRETA_ENV = "CLAVE_SECRETA";
        private const string MENSAJE_ERROR_JWT_ISSUER = "ISSUER no configurado";
        private const string MENSAJE_ERROR_JWT_AUDIENCE = "AUDIENCE no configurado";
        private const string MENSAJE_ERROR_JWT_CLAVE = "CLAVE_SECRETA no configurado";
        
        private const bool VALIDAR_ISSUER = true;
        private const bool VALIDAR_AUDIENCE = true;
        private const bool VALIDAR_LIFETIME = true;
        private const bool VALIDAR_ISSUER_SIGNING_KEY = true;
      
        /// <summary>
        /// Agrega la configuración de autenticación JWT usando variables de entorno.
        /// </summary>
        /// <param name="builder">El builder de la aplicación web</param>
        /// <returns>El builder de la aplicación web configurado</returns>
        public static WebApplicationBuilder AddJwtAuthentication(this WebApplicationBuilder builder)
        {
            try
            {
                if (builder is null) throw new ArgumentNullException(nameof(builder));

                string issuer = Environment.GetEnvironmentVariable(JWT_ISSUER_ENV)
                    ?? throw new InvalidOperationException(MENSAJE_ERROR_JWT_ISSUER);
                string audience = Environment.GetEnvironmentVariable(JWT_AUDIENCE_ENV)
                    ?? throw new InvalidOperationException(MENSAJE_ERROR_JWT_AUDIENCE);
                string claveSecreta = Environment.GetEnvironmentVariable(JWT_CLAVE_SECRETA_ENV)
                    ?? throw new InvalidOperationException(MENSAJE_ERROR_JWT_CLAVE);
        
                builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
                  .AddJwtBearer(options =>
                  {
                      options.TokenValidationParameters = new TokenValidationParameters();
                      options.TokenValidationParameters.ValidateIssuer = VALIDAR_ISSUER;
                      options.TokenValidationParameters.ValidateAudience = VALIDAR_AUDIENCE;
                      options.TokenValidationParameters.ValidateLifetime = VALIDAR_LIFETIME;
                      options.TokenValidationParameters.ValidateIssuerSigningKey = VALIDAR_ISSUER_SIGNING_KEY;
                      options.TokenValidationParameters.ValidIssuer = issuer;
                      options.TokenValidationParameters.ValidAudience = audience;
                      options.TokenValidationParameters.IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(claveSecreta));
                  });


                return builder;
            }
            catch (Exception)
            {
                throw;
            }
        }     
    }
}