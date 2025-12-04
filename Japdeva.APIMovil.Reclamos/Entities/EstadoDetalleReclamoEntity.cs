using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Japdeva.APIMovil.Reclamos.Entities
{
    [Table ("Tbl_EstadoDetalleReclamo")]
    public class EstadoDetalleReclamoEntity
    {
        [Column("id")]
        [Key]
        public int Id { get; set; }

        [Column("descripcion")]
        [MaxLength(100)]
        [MinLength(1)]
        [Required]
        public string Descripcion { get; set; } = string.Empty;

        [Column("continuaProceso")]
        public bool ContinuaProceso { get; set; }

        [Column("rechazaProceso")]
        public bool RechazaProceso { get; set; }

        [Column("devolucionProceso")]
        public bool DevolucionProceso { get; set; }

        [Column("finalizarProceso")]
        public bool FinalizarProceso { get; set; }

        [Column("idUsuarioInterno")]
        public long IdUsuarioInterno { get; set; }

        [Column("fechaRegistro")]
        public DateTime FechaRegistro { get; set; }

        [Column("activo")]
        public bool Activo { get; set; }

    }
}
