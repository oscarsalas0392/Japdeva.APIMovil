using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Japdeva.APIMovil.Reclamos.Entities
{
    [Table("Tbl_EstadoDetalleReclamoOrdenProceso")]
    public class EstadoDetalleReclamoOrdenProcesoEntity
    {
        [Column("id")]
        [Key]
        public int Id { get; set; }

        [Column("idOrdenProceso")]
        public int IdOrdenProceso { get; set; } 

        [Column("idEstadoDetalleReclamo")]
        public int IdEstadoDetalleReclamo { get; set; }

        [Column("obligatorioDescripcion")]
        public bool ObligatorioDescripcion { get; set; }

        [Column("obligatorioDocumentoInterno")]
        public bool ObligatorioDocumentoInterno { get; set; }

        [Column("idUsuarioInterno")]
        public long IdUsuarioInterno { get; set; }

        [Column("fechaRegistro")]
        public DateTime FechaRegistro { get; set; }

        [Column("activo")]
        public bool Activo { get; set; }
    }
}
