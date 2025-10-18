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
      
        /// <summary>
        /// Agrega la configuración de autenticación JWT usando variables de entorno.
        /// </summary>
        /// <param name="builder">El builder de la aplicación web</param>
        /// <returns>El builder de la aplicación web configurado</returns>
        /// <exception cref="ArgumentNullException">Cuando el builder es nulo</exception>
        /// <exception cref="InvalidOperationException">Cuando las variables de entorno no están configuradas</exception>
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
                      options.TokenValidationParameters.ValidateIssuer = true;
                      options.TokenValidationParameters.ValidateAudience = true;
                      options.TokenValidationParameters.ValidateLifetime = true;
                      options.TokenValidationParameters.ValidateIssuerSigningKey = true;
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