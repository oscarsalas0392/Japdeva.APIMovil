using Japdeva.APIMovil.Reclamos.Entities;
using Microsoft.EntityFrameworkCore;

namespace Japdeva.APIMovil.Reclamos.Data
{
    public class ReclamoDbContext : DbContext
    {
        public ReclamoDbContext(DbContextOptions<ReclamoDbContext> options) : base(options)
        {
        }
        public DbSet<DetalleReclamoEntity> DetalleReclamos { get; set; }
        public DbSet<DetalleReclamoHistoricoEntity> DetalleReclamoHistoricos { get; set; }
        public DbSet<DevolucionProcesoEntity> DevolucionProcesos { get; set; }
        public DbSet<DocumentoInternoEntity> DocumentoInternos { get; set; }
        public DbSet<DocumentoInternoHistoricoEntity> DocumentoInternoHistoricos { get; set; }
        public DbSet<DocumentoUsuarioEntity> DocumentoUsuarios { get; set; }
        public DbSet<DocumentoUsuarioHistoricoEntity> DocumentoUsuarioHistoricos { get; set; }   
        public DbSet<EstadoDetalleReclamoEntity> EstadoDetalleReclamos { get; set;}
        public DbSet<EstadoDetalleReclamoOrdenProcesoEntity> EstadoDetalleReclamoOrdenProcesos { get; set; }
        public DbSet<EstadoReclamoEntity> EstadoReclamos { get; set; }
        public DbSet<OrdenProcesoEntity> OrdenProcesos { get; set; }
        public DbSet<ReclamoEntity> Reclamos { get; set; }
    }
}
