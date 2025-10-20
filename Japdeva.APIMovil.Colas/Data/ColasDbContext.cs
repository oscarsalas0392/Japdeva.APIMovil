using Microsoft.EntityFrameworkCore;
using Japdeva.APIMovil.Colas.Entities;

namespace Japdeva.APIMovil.Colas.Data
{
    /// <summary>
    /// Contexto de la base de datos para el sistema de colas.
    /// </summary>
    public class ColasDbContext : DbContext
    {
        /// <summary>
        /// Inicializa una nueva instancia de la clase ColasDbContext.
        /// </summary>
        /// <param name="options">Las opciones de configuración para el contexto de la base de datos.</param>
        public ColasDbContext(DbContextOptions<ColasDbContext> options) : base(options)
        {
        }

        /// <summary>
        /// Conjunto de entidades de mensajes en cola en la base de datos.
        /// </summary>
        public DbSet<MensajeColaEntity> MensajesCola { get; set; }
        /// <summary>
        /// Conjunto de entidades de estado de colas en la base de datos.
        /// </summary>
        public DbSet<EstadoColaEntity> EstadoColas { get; set; } 

        /// <summary>
        /// Conjunto de entidades de estados de mensaje en la base de datos.
        /// </summary>
        public DbSet<EstadoMensajeEntity> EstadosMensaje { get; set; } 

        /// <summary>
        /// Conjunto de entidades de colas en la base de datos.
        /// </summary>
        public DbSet<ColaEntity> Colas { get; set; } 

        /// <summary>
        /// Conjunto de entidades de prioridades en la base de datos.
        /// </summary>
        public DbSet<PrioridadEntity> Prioridades { get; set; } 

    }
}