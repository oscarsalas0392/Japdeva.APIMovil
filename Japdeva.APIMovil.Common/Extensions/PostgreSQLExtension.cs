using System;
using Microsoft.AspNetCore.Builder;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Npgsql.EntityFrameworkCore.PostgreSQL;

namespace Japdeva.APIMovil.Common.Extensions
{
    /// <summary>
    /// Proporciona métodos de extensión para configurar la conexión a PostgreSQL.
    /// </summary>
    public static class PostgreSQLExtension
    {
        private const string POSTGRESQL_CONNECTION_STRING_ENV = "CONNECTION_STRING";
        private const string MENSAJE_ERROR_CONNECTION_STRING = "CONNECTION_STRING no configurado";
        private const int MAX_RETRY_COUNT = 3;
        private const int MAX_RETRY_DELAY_SECONDS = 30;
        private const int COMMAND_TIMEOUT_SECONDS = 30;

        /// <summary>
        /// Agrega la configuración de PostgreSQL al contenedor de servicios.
        /// </summary>
        /// <param name="builder">El builder de la aplicación web</param>
        /// <returns>El builder de la aplicación web configurado</returns>
        /// <exception cref="ArgumentNullException">Cuando el builder es nulo</exception>
        /// <exception cref="InvalidOperationException">Cuando la cadena de conexión no está configurada</exception>
        public static WebApplicationBuilder AgregarPostgreSQL<TDbContext>(this WebApplicationBuilder builder)
            where TDbContext : DbContext
        {
            try
            {
                if (builder is null) throw new ArgumentNullException(nameof(builder));

                string connectionString = Environment.GetEnvironmentVariable(POSTGRESQL_CONNECTION_STRING_ENV)
                    ?? throw new InvalidOperationException(MENSAJE_ERROR_CONNECTION_STRING);

                builder.Services.AddDbContext<TDbContext>(options =>
                {
                    options.UseNpgsql(connectionString, npgsqlOptions =>
                    {
                        npgsqlOptions.EnableRetryOnFailure(
                            maxRetryCount: MAX_RETRY_COUNT,
                            maxRetryDelay: TimeSpan.FromSeconds(MAX_RETRY_DELAY_SECONDS),
                            errorCodesToAdd: null);
                        npgsqlOptions.CommandTimeout(COMMAND_TIMEOUT_SECONDS);
                    });
                    
                    if (builder.Environment.IsDevelopment())
                    {
                        options.EnableSensitiveDataLogging();
                        options.EnableDetailedErrors();
                    }
                });

                // Registrar también como DbContext base para los repositorios
                builder.Services.AddScoped<DbContext>(provider => provider.GetRequiredService<TDbContext>());
                return builder;
            }
            catch (Exception)
            {
                throw;
            }
        }
    }
}