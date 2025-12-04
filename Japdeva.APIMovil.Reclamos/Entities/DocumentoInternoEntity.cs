using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Japdeva.APIMovil.Reclamos.Entities
{
    [Table("Tbl_DocumentoInterno")]
    public class DocumentoInternoEntity
    {
        [Key]
        [Column("id")]
        public long Id { get; set; }

        [Column("idDetalleReclamo")]
        public long IdDetalleReclamo { get; set; }

        [Column("documento")]
        [MinLength(1)]
        public string Documento { get; set; } = string.Empty;

        [Column("fechaRegistro")]
        public DateTime FechaRegistro { get; set; }
    }
}
