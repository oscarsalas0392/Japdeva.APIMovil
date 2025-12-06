using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Japdeva.APIMovil.Reclamos.Entities
{
    /// <summary>
    /// Entidad que representa el histórico de documentos internos en el sistema de reclamos.
    /// Mantiene un registro de auditoría de todos los cambios, modificaciones y versiones
    /// de los documentos internos generados durante el procesamiento de reclamos.
    /// </summary>
    [Table("Tbl_DocumentoInternoHistorico")]
    public class DocumentoInternoHistoricoEntity
    {
        /// <summary>
        /// Obtiene o establece el identificador único del registro histórico de documento interno.
        /// </summary>
        [Column("id")]
        [Key]
        public long Id { get; set; }

        /// <summary>
        /// Obtiene o establece el identificador del reclamo al cual pertenecía el documento interno histórico.
        /// Mantiene la referencia al reclamo principal para trazabilidad completa.
        /// </summary>
        [Column("idReclamo")]
        public long IdReclamo { get; set; }

        /// <summary>
        /// Obtiene o establece el contenido o referencia del documento interno en el momento del registro histórico.
        /// Captura el estado del documento tal como estaba cuando se creó este registro de auditoría.
        /// </summary>
        [Column("documento")]
        [MaxLength(4000)]
        [Required]
        public string Documento { get; set; } = string.Empty;

        /// <summary>
        /// Obtiene o establece la fecha y hora de registro de este cambio histórico.
        /// Timestamp exacto de cuando se registró esta versión del documento para auditoría.
        /// </summary>
        [Column("fechaRegistro")]
        public DateTime FechaRegistro { get; set; }

        /// <summary>
        /// Obtiene o establece el identificador del usuario interno que realizó la acción registrada.
        /// Permite auditoría completa de quién modificó o actualizó el documento interno.
        /// </summary>
        [Column("idUsuarioInterno")]
        public long IdUsuarioInterno { get; set; }
    }
}
