using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Japdeva.APIMovil.Reclamos.Entities
{
    /// <summary>
    /// Entidad que representa un documento de usuario asociado a un reclamo.
    /// Almacena los archivos o documentos proporcionados por usuarios externos como evidencia o soporte del reclamo.
    /// </summary>
    [Table("Tbl_DocumentoUsuario")]
    public class DocumentoUsuarioEntity
    {
        /// <summary>
        /// Obtiene o establece el identificador único del documento de usuario.
        /// </summary>
        [Column("id")]
        [Key]
        public long Id { get; set; }

        /// <summary>
        /// Obtiene o establece el identificador del reclamo al cual pertenece el documento.
        /// </summary>
        [Column("idReclamo")]
        public long IdReclamo { get; set; }

        /// <summary>
        /// Obtiene o establece el nombre del documento proporcionado por el usuario.
        /// </summary>
        [Column("nombreDocumento")]
        [MinLength(1)]
        [Required]
        public string NombreDocumento { get; set; } = string.Empty;

        /// <summary>
        /// Obtiene o establece el contenido o referencia del documento.
        /// Puede contener la ruta del archivo, contenido codificado o identificador del documento.
        /// </summary>
        [Column("documento")]
        [MinLength(1)]
        [Required]
        public string Documento { get; set; } = string.Empty;

        /// <summary>
        /// Obtiene o establece la fecha y hora de registro del documento.
        /// </summary>
        [Column("fechaRegistro")]
        public DateTime FechaRegistro { get; set; }

    }
}
