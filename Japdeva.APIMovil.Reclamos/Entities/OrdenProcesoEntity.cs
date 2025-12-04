using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Japdeva.APIMovil.Reclamos.Entities
{
    [Table("Tbl_OrdenProceso")]
    public class OrdenProcesoEntity
    {
        [Column("id")]
        [Key]
        public int Id { get; set; }

        [Column("departamento")]
        public int Departamento { get; set; }

        [Column("orden")]
        public int Orden { get; set; }

        [Column("idUsuarioInterno")]
        public long IdUsuarioInterno { get; set; }

        [Column("fechaRegistro")]
        public DateTime FechaRegistro { get; set; }

        [Column("activo")]
        public bool Activo { get; set; }

    }
}
