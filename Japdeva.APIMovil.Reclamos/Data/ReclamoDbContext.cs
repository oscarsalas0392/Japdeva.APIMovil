using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using Japdeva.APIMovil.Reclamos.Entities;

namespace Japdeva.APIMovil.Reclamos.Data
{
    /// <summary>
    /// Contexto de Entity Framework para el módulo de Reclamos.
    /// Proporciona acceso a las entidades relacionadas con la gestión de reclamos, sus detalles,
    /// documentos, estados y configuraciones del proceso de atención.
    /// </summary>
    public class ReclamoDbContext : DbContext
    {
        /// <summary>
        /// Inicializa una nueva instancia del contexto de datos de Reclamos.
        /// </summary>
        /// <param name="options">Opciones de configuración para el contexto de Entity Framework.</param>
        public ReclamoDbContext(DbContextOptions<ReclamoDbContext> options) : base(options)
        {
        }

        /// <summary>
        /// Configura el modelo de Entity Framework Core aplicando una conversión global
        /// para todas las propiedades de tipo <see cref="DateTime"/>.
        /// Esta configuración es especialmente importante al trabajar con PostgreSQL y
        /// columnas del tipo <c>timestamp with time zone</c>, ya que el proveedor Npgsql
        /// solo admite valores en UTC y produce excepciones cuando se utilizan fechas con
        /// <see cref="DateTimeKind.Local"/> o <see cref="DateTimeKind.Unspecified"/>.
        /// </summary>

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            foreach (var tipoEntidad in modelBuilder.Model.GetEntityTypes())
            {
                foreach (var propiedad in tipoEntidad.GetProperties())
                {
                    if (propiedad.ClrType == typeof(DateTime))
                    {
                        propiedad.SetValueConverter(
                            new ValueConverter<DateTime, DateTime>(
                                v => v.Kind == DateTimeKind.Utc ? v : v.ToUniversalTime(),
                                v => DateTime.SpecifyKind(v, DateTimeKind.Utc)
                            )
                        );
                    }
                }
            }
        }


        /// <summary>
        /// Obtiene o establece el conjunto de entidades de reclamos.
        /// Representa la tabla principal de reclamos registrados por usuarios externos.
        /// </summary>
        public DbSet<ReclamoEntity> Reclamos { get; set; } = null!;

        /// <summary>
        /// Obtiene o establece el conjunto de entidades de detalles de reclamos.
        /// Representa la información de seguimiento y detalles adicionales de los reclamos.
        /// </summary>
        public DbSet<DetalleReclamoEntity> DetalleReclamos { get; set; } = null!;

        /// <summary>
        /// Obtiene o establece el conjunto de entidades de documentos de usuario.
        /// Representa los archivos y documentos asociados a los reclamos por parte de usuarios externos.
        /// </summary>
        public DbSet<DocumentoUsuarioEntity> DocumentosUsuario { get; set; } = null!;

        /// <summary>
        /// Obtiene o establece el conjunto de entidades de documentos internos.
        /// Representa los documentos generados internamente durante el proceso de atención.
        /// </summary>
        public DbSet<DocumentoInternoEntity> DocumentosInternos { get; set; } = null!;

        /// <summary>
        /// Obtiene o establece el conjunto de entidades de estados de reclamos.
        /// Representa el catálogo de estados posibles de los reclamos.
        /// </summary>
        public DbSet<EstadoReclamoEntity> EstadosReclamos { get; set; } = null!;

        /// <summary>
        /// Obtiene o establece el conjunto de entidades de estados de detalle de reclamos.
        /// Representa el catálogo de estados posibles de los detalles de reclamos.
        /// </summary>
        public DbSet<EstadoDetalleReclamoEntity> EstadosDetalleReclamos { get; set; } = null!;

        /// <summary>
        /// Obtiene o establece el conjunto de entidades de procesos.
        /// Representa el catálogo de tipos de proceso: Reclamo y Apelación.
        /// </summary>
        public DbSet<ProcesoEntity> Procesos { get; set; } = null!;

        /// <summary>
        /// Obtiene o establece el conjunto de entidades de niveles de proceso.
        /// Representa la configuración de los flujos y secuencias de atención de reclamos.
        /// </summary>
        public DbSet<NivelProcesoEntity> NivelesProcesos { get; set; } = null!;


        /// <summary>
        /// Obtiene o establece el conjunto de entidades de órdenes de niveles de proceso.
        /// Representa la relación jerárquica y de secuencia entre los diferentes niveles del proceso de atención de reclamos.
        /// </summary>
        public DbSet<OrdenNivelProcesoEntity> OrdenesNivelesProcesos { get; set; } = null!;


        /// <summary>
        /// Obtiene o establece el conjunto de entidades de estado detalle reclamo orden proceso.
        /// Representa la relación entre estados de detalle y órdenes de proceso.
        /// </summary>
        public DbSet<EstadoDetalleReclamoNivelProcesoEntity> EstadoDetalleReclamoOrdenProcesos { get; set; } = null!;

        /// <summary>
        /// Obtiene o establece el conjunto de entidades de apelaciones de reclamos.
        /// Representa las apelaciones presentadas por usuarios externos ante resoluciones de reclamos.
        /// </summary>
        public DbSet<ApelacionReclamoEntity> ApelacionesReclamos { get; set; } = null!;

        /// <summary>
        /// Obtiene o establece el conjunto de entidades de detalles de apelaciones de reclamos.
        /// Representa el seguimiento y las acciones realizadas durante el proceso de apelación.
        /// </summary>
        public DbSet<DetalleApelacionReclamoEntity> DetalleApelacionesReclamos { get; set; } = null!;

        /// <summary>
        /// Obtiene o establece el conjunto de entidades históricas de detalles de reclamos.
        /// Mantiene el registro histórico de cambios en los detalles de reclamos.
        /// </summary>
        public DbSet<DetalleReclamoHistoricoEntity> DetalleReclamoHistoricos { get; set; } = null!;

        /// <summary>
        /// Obtiene o establece el conjunto de entidades históricas de documentos de usuario.
        /// Mantiene el registro histórico de cambios en los documentos de usuario.
        /// </summary>
        public DbSet<DocumentoUsuarioHistoricoEntity> DocumentoUsuarioHistoricos { get; set; } = null!;

        /// <summary>
        /// Obtiene o establece el conjunto de entidades históricas de documentos internos.
        /// Mantiene el registro histórico de cambios en los documentos internos.
        /// </summary>
        public DbSet<DocumentoInternoHistoricoEntity> DocumentoInternoHistoricos { get; set; } = null!;
    }
}
