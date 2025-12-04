using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Japdeva.APIMovil.Reclamos.Entities
{
    [Table("Tbl_DocumentoUsuario")]
    public class DocumentoUsuarioEntity
    {
        [Key]
        [Column("id")]
        public long Id { get; set; }

        [Column("idReclamo")]
        public long IdReclamo { get; set; }

        [Column("documento")]
        [MinLength(1)]
        [Required]
        public string Documento { get; set; } = string.Empty;   

        [Column("fechaRegistro")]
        public DateTime FechaRegistro { get; set; }
    }
}
