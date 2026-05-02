using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Japdeva.APIMovil.Reclamos.Entities
{
    /// <summary>
    /// Entidad que representa el histórico de documentos de usuario en el sistema de reclamos.
    /// Mantiene un registro de auditoría de todos los cambios y versiones de los documentos
    /// proporcionados por usuarios externos como evidencia o soporte de sus reclamos.
    /// </summary>
    [Table("Tbl_DocumentoUsuarioHistorico")]
    public class DocumentoUsuarioHistoricoEntity
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
