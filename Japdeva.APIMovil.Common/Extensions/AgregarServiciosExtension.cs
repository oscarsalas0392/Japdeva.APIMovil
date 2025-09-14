using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Tokens;
using Ocelot.DependencyInjection;
using Japdeva.APIMovil.Common.Services;
using Japdeva.APIMovil.Common.Services.DesencriptarService;
using Japdeva.APIMovil.Common.Services.EncriptarHelperService;
using Japdeva.APIMovil.Common.Services.EncriptarService;

namespace Japdeva.APIMovil.Common.Extensions
{
    /// <summary>
    /// Proporciona métodos de extensión para agregar servicios a la aplicación.
    /// </summary>
    public static class AgregarServiciosExtension
    {

        private const string JWT_ISSUER_ENV = "ISSUER";
        private const string JWT_AUDIENCE_ENV = "AUDIENCE";
        private const string JWT_CLAVE_SECRETA_ENV = "CLAVE_SECRETA";
        private const string MENSAJE_ERROR_JWT_ISSUER = "ISSUER no configurado";
        private const string MENSAJE_ERROR_JWT_AUDIENCE = "AUDIENCE no configurado";
        private const string MENSAJE_ERROR_JWT_CLAVE = "CLAVE_SECRETA no configurado";

        /// <summary>
        /// Agrega los servicios necesarios para los microservicios a la aplicación.
        /// </summary>
        public static WebApplicationBuilder AgregarServiciosMicroservicios(this WebApplicationBuilder builder)
        {
            try
            {
                if (builder is null) throw new ArgumentNullException(nameof(builder));
                builder.AgregarLog4Net();
                builder.Services.AddControllers();
                builder.Services.AddOpenApi();
                string issuer = Environment.GetEnvironmentVariable(JWT_ISSUER_ENV) ?? throw new InvalidOperationException(MENSAJE_ERROR_JWT_ISSUER);
                string audience = Environment.GetEnvironmentVariable(JWT_AUDIENCE_ENV) ?? throw new InvalidOperationException(MENSAJE_ERROR_JWT_AUDIENCE);
                string claveSecreta = Environment.GetEnvironmentVariable(JWT_CLAVE_SECRETA_ENV) ?? throw new InvalidOperationException(MENSAJE_ERROR_JWT_CLAVE);

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

        /// <summary>
        /// Agrega los servicios necesarios para el gateway a la aplicación.
        /// </summary>
        public static WebApplicationBuilder AgregarServiciosGateway(this WebApplicationBuilder builder)
        {
            try
            {
                if (builder is null) throw new ArgumentNullException(nameof(builder));
                builder.AgregarLog4Net();
                builder.Services.AddControllers();
                builder.Services.AddOpenApi();
                builder.Services.AddSingleton<IGenerarTokenService, GenerarTokenService>();
                builder.Services.AddSingleton<IValidarTokenService, ValidarTokenService>();
                builder.Services.AddSingleton<IEncriptarService, EncriptarService>();
                builder.Services.AddSingleton<IDesencriptarService, DesencriptarService>();
                builder.Services.AddSingleton<IEncriptarHelperService, EncriptarHelperService>();
                builder.Services.AddOcelot(builder.Configuration);
                return builder;
            }
            catch (Exception)
            {
                throw;
            }
        }
    }
}
