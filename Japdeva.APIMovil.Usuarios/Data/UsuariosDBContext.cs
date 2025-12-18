using Microsoft.EntityFrameworkCore;
using Japdeva.APIMovil.Usuarios.Entities;

namespace Japdeva.APIMovil.Usuarios.Data
{
    /// <summary>
    /// Contexto de la base de datos para el sistema de usuarios.
    /// </summary>
    public class UsuariosDBContext : DbContext
    {
        /// <summary>
        /// Inicializa una nueva instancia de la clase UsuariosDBContext.
        /// </summary>
        /// <param name="options">Las opciones de configuración para el contexto de la base de datos.</param>
        public UsuariosDBContext(DbContextOptions<UsuariosDBContext> options) : base(options)
        {
        }

        /// <summary>
        /// Conjunto de entidades de usuarios en la base de datos.
        /// </summary>
        public DbSet<UsuarioEntity> Usuarios { get; set; }
    }
}
