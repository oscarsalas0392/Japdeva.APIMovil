using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Japdeva.APIMovil.Reclamos.Entities
{
    [Table("Tbl_DetalleReclamoHistorico")]
    public class DetalleReclamoHistoricoEntity
    {
        [Key]
        [Column("id")]
        public long Id { get; set; }

        [Column("idReclamo")]
        public long IdReclamo { get; set; }

        [Column("idOrdenProceso")]
        public int IdOrdenProceso { get; set; }

        [Column("idDevolucionProceso")]
        public int? IdDevolucionProceso { get; set; }

        [Column("idEstadoDetalleReclamo")]
        public int IdEstadoDetalleReclamo { get; set; }

        [Column("idUsuarioInterno")]
        public long IdUsuarioInterno { get; set; }

        [Column("descripcion")]
        [MaxLength(500)]
        public string Descripcion { get; set; } = string.Empty;

        [Column("fechaRegistro")]
        public DateTime FechaRegistro { get; set; }
    }
}
