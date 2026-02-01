using Microsoft.EntityFrameworkCore;
using Japdeva.APIMovil.Parametros.Entities;

namespace Japdeva.APIMovil.Parametros.Data
{
    /// <summary>
    /// Representa el contexto de base de datos para los parámetros de la aplicación.
    /// </summary>
    public class ParametroDbContext : DbContext
    {
        /// <summary>
        /// Inicializa una nueva instancia de la clase <see cref="ParametroDbContext"/> con las opciones especificadas.
        /// </summary>
        /// <param name="options">Opciones para este contexto.</param>
        public ParametroDbContext(DbContextOptions<ParametroDbContext> options)
            : base(options)
        {
        }

        /// <summary>
        /// Obtiene o establece la colección de mensajes en la base de datos.
        /// </summary>
        public DbSet<MensajeEntity> Mensajes { get; set; } = null!;

        /// <summary>
        /// Obtiene o establece la colección de menús en la base de datos.
        /// </summary>
        public DbSet<MenuEntity> Menus { get; set; } = null!;

        /// <summary>
        /// Obtiene o establece la colección de menús por perfil en la base de datos.
        /// </summary>
        public DbSet<MenuPerfilEntity> MenuPefiles { get; set; } = null!;

        /// <summary>
        /// Obtiene o establece la colección de pantallas en la base de datos.
        /// </summary>
        public DbSet<PantallaEntity> Pantallas { get; set; } = null!;

        /// <summary>
        /// Obtiene o establece la colección de tipos de mensajes en la base de datos.
        /// </summary>
        public DbSet<TipoMensajeEntity> TiposMensajes { get; set; } = null!;

        /// <summary>
        /// Obtiene o establece la colección de plantillas de correos en la base de datos.
        /// </summary>
        public DbSet<PlantillaCorreoEntity> PlantillaCorreos { get; set; } = null!;
    }
}
