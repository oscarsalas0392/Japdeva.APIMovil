using Microsoft.EntityFrameworkCore;
using Japdeva.APIMovil.Usuarios.Entities;

namespace Japdeva.APIMovil.Usuarios.Data
{
    /// <summary>
    /// Contexto de base de datos del microservicio de Usuarios.
    /// Expone todas las entidades del dominio de usuarios.
    /// </summary>
    public class UsuariosDBContext : DbContext
    {
        /// <summary>
        /// Inicializa una nueva instancia de UsuariosDBContext.
        /// </summary>
        /// <param name="options">Opciones de configuración del contexto.</param>
        public UsuariosDBContext(DbContextOptions<UsuariosDBContext> options) : base(options)
        {
        }

        /// <summary>Conjunto de usuarios registrados.</summary>
        public DbSet<UsuarioEntity> Usuarios { get; set; }

        /// <summary>Conjunto de roles disponibles en el sistema.</summary>
        public DbSet<RolEntity> Roles { get; set; }

        /// <summary>Conjunto de tipos de cédula disponibles.</summary>
        public DbSet<TipoCedulaEntity> TiposCedula { get; set; }

        /// <summary>Conjunto de departamentos de la organización.</summary>
        public DbSet<DepartamentoEntity> Departamentos { get; set; }

        /// <summary>Conjunto de asignaciones de roles a usuarios.</summary>
        public DbSet<UsuarioRolEntity> UsuariosRoles { get; set; }

        /// <summary>Conjunto de asignaciones de usuarios a departamentos.</summary>
        public DbSet<DepartamentoUsuarioEntity> DepartamentosUsuarios { get; set; }
    }
}
