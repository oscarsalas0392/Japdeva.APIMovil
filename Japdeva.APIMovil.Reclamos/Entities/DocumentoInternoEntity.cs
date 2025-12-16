using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Japdeva.APIMovil.Reclamos.Entities
{
    /// <summary>
    /// Entidad que representa un documento interno asociado a un reclamo.
    /// Almacena documentos, reportes y archivos generados internamente por el personal
    /// durante el procesamiento y resolución de reclamos.
    /// </summary>
    [Table("Tbl_DocumentoInterno")]
    public class DocumentoInternoEntity
    {
        /// <summary>
        /// Obtiene o establece el identificador único del documento interno.
        /// </summary>
        [Column("id")]
        [Key]
        public long Id { get; set; }

        /// <summary>
        /// Obtiene o establece el identificador del detalle reclamo al cual pertenece el documento interno.
        /// Establece la relación con el reclamo principal al que se adjunta este documento.
        /// </summary>
        [Column("idReclamo")]
        public long IdDetalleReclamo { get; set; }

        /// <summary>
        /// Obtiene o establece el nombre del documento interno.
        /// Describe el título o denominación del archivo adjunto al reclamo.
        /// </summary>
        [Column("nombreDocumento")]
        [MinLength(1)]
        [Required]
        public string NombreDocumento { get; set; } = string.Empty;


        /// <summary>
        /// Obtiene o establece el contenido o referencia del documento interno.
        /// Puede contener la ruta del archivo, contenido codificado, URL o identificador del documento.
        /// </summary>
        [Column("documento")]
        [MinLength(1)]
        [Required]
        public string Documento { get; set; } = string.Empty;

        /// <summary>
        /// Obtiene o establece la fecha y hora de registro del documento interno.
        /// Timestamp de cuando el documento fue creado o adjuntado al reclamo.
        /// </summary>
        [Column("fechaRegistro")]
        public DateTime FechaRegistro { get; set; }

        /// <summary>
        /// Obtiene o establece el identificador del usuario interno que creó o adjuntó el documento.
        /// Permite auditoría de quién generó o añadió el documento interno al reclamo.
        /// </summary>
        [Column("idUsuarioInterno")]
        public long IdUsuarioInterno { get; set; }

        /// <summary>
        /// Obtiene o establece el indicador de si el documento interno está activo en el sistema.
        /// Permite soft-delete de documentos sin eliminarlos físicamente de la base de datos.
        /// </summary>
        [Column("activo")]
        public bool Activo { get; set; }
    }
}
