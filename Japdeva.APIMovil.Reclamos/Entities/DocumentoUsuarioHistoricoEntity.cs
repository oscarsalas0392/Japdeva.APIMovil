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
        /// Obtiene o establece el identificador único del registro histórico de documento de usuario.
        /// </summary>
        [Column("id")]
        [Key]
        public long Id { get; set; }

        /// <summary>
        /// Obtiene o establece el identificador del reclamo al cual pertenecía el documento de usuario histórico.
        /// Mantiene la relación con el reclamo original para trazabilidad completa del historial.
        /// </summary>
        [Column("idReclamo")]
        public long IdReclamo { get; set; }

        /// <summary>
        /// Obtiene o establece el contenido o referencia del documento de usuario en el momento del registro histórico.
        /// Preserva el estado exacto del documento tal como estaba cuando se creó este registro de auditoría.
        /// </summary>
        [Column("documento")]
        [MaxLength(4000)]
        [Required]
        public string Documento { get; set; } = string.Empty;

        /// <summary>
        /// Obtiene o establece la fecha y hora de registro de este cambio histórico.
        /// Timestamp preciso de cuando se registró esta versión del documento para auditoría temporal.
        /// </summary>
        [Column("fechaRegistro")]
        public DateTime FechaRegistro { get; set; }

        /// <summary>
        /// Obtiene o establece el identificador del usuario interno que realizó la acción sobre el documento.
        /// Permite auditoría de qué personal interno procesó o modificó el documento del usuario.
        /// </summary>
        [Column("idUsuarioInterno")]
        public long IdUsuarioInterno { get; set; }
    }
}
