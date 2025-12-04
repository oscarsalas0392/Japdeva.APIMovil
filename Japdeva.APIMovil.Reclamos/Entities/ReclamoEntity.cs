using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Japdeva.APIMovil.Reclamos.Entities
{
    [Table("Tbl_Reclamo")]
    public class ReclamoEntity
    {
        [Key]
        [Column("id")]
        public long Id { get; set; }

        [Column("idEstadoReclamo")]
        public int IdEstadoReclamo { get; set; }

        [Column("titulo")]
        [MaxLength(100)]
        public string Titulo { get; set; } = string.Empty;

        [Column("descripcion")]
        [MaxLength(100)]
        public string Descripcion { get; set; } = string.Empty;

        [Column("fechaRegistro")]
        public DateTime FechaRegistro { get; set; }

        [Column("idUsuarioExterno")]
        public long IdUsuarioExterno { get; set; }

    }
}
