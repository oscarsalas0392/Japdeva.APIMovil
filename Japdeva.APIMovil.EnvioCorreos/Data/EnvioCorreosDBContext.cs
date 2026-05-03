using Microsoft.EntityFrameworkCore;
using Japdeva.APIMovil.EnvioCorreos.Entities;

namespace Japdeva.APIMovil.EnvioCorreos.Data
{
    /// <summary>
    /// Contexto de base de datos del microservicio de Envío de Correos.
    /// </summary>
    public class EnvioCorreosDBContext : DbContext
    {
        /// <summary>
        /// Inicializa una nueva instancia de EnvioCorreosDBContext.
        /// </summary>
        /// <param name="options">Opciones de configuración del contexto.</param>
        public EnvioCorreosDBContext(DbContextOptions<EnvioCorreosDBContext> options) : base(options)
        {
        }

        /// <summary>Correos pendientes de envío.</summary>
        public DbSet<CorreoPendienteEntity> CorreosPendientes { get; set; } = null!;

        /// <summary>Histórico de correos enviados o con intentos agotados.</summary>
        public DbSet<CorreoHistoricoEntity> CorreosHistorico { get; set; } = null!;
    }
}
