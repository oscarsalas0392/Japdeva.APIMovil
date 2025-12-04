using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Japdeva.APIMovil.Reclamos.Entities
{
    [Table("Tbl_EstadoReclamo")]
    public class EstadoReclamoEntity
    {
        [Key]
        [Column("id")]
        public int Id { get; set; }

        [Column("descripcion")]
        [MaxLength(100)]
        [MinLength(1)]
        [Required]
        public string Descripcion { get; set; } = string.Empty;

        [Column("idUsuarioInterno")]
        public long IdUsuarioInterno { get; set; }

        [Column("fechaRegistro")]
        public DateTime FechaRegistro { get; set; }

        [Column("activo")]
        public bool Activo { get; set; }

    }
}
