using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Japdeva.APIMovil.Reclamos.Entities
{
    [Table("Tbl_DevolucionProceso")]
    public class DevolucionProcesoEntity
    {
        [Column("id")]
        [Key]
        public int Id { get; set; }

        [Column("idOrdenProceso")]
        public int IdOrdenProceso { get; set; }

        [Column("idOrdenDevolucionProceso")]
        public int OrdenDevolucionProceso { get; set; }

        [Column("idUsuarioInterno")]
        public long IdUsuarioInterno { get; set; }

        [Column("fechaRegistro")]
        public DateTime FechaRegistro { get; set; }

        [Column("activo")]
        public bool Activo { get; set; }
    }
}
