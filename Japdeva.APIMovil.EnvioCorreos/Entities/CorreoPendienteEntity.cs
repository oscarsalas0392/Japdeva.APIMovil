using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Japdeva.APIMovil.EnvioCorreos.Entities
{
    /// <summary>
    /// Entidad que representa un correo electrónico pendiente de envío.
    /// </summary>
    [Table("Tbl_CorreoPendiente")]
    public class CorreoPendienteEntity
    {
        /// <summary>Obtiene o establece el identificador único del registro.</summary>
        [Key]
        [Column("id")]
        public long Id { get; set; }

        /// <summary>Obtiene o establece el correo electrónico del destinatario.</summary>
        [Required]
        [MaxLength(200)]
        [Column("destinatario")]
        public string Destinatario { get; set; } = string.Empty;

        /// <summary>Obtiene o establece el asunto del correo.</summary>
        [Required]
        [MaxLength(500)]
        [Column("asunto")]
        public string Asunto { get; set; } = string.Empty;

        /// <summary>Obtiene o establece el cuerpo del correo en formato HTML.</summary>
        [Required]
        [Column("cuerpo")]
        public string Cuerpo { get; set; } = string.Empty;

        /// <summary>Obtiene o establece el número de intentos de envío realizados.</summary>
        [Column("intentos")]
        public int Intentos { get; set; } = 0;

        /// <summary>Obtiene o establece el mensaje del último error ocurrido durante el envío.</summary>
        [Column("ultimoError")]
        public string? UltimoError { get; set; }

        /// <summary>Obtiene o establece si el cuerpo del correo está en formato HTML.</summary>
        [Column("esCuerpoHtml")]
        public bool EsCuerpoHtml { get; set; } = true;

        /// <summary>Obtiene o establece si el correo fue enviado exitosamente.</summary>
        [Column("enviado")]
        public bool Enviado { get; set; } = false;

        /// <summary>Obtiene o establece la fecha de creación del registro.</summary>
        [Column("fechaRegistro")]
        public DateTime FechaRegistro { get; set; } = DateTime.UtcNow;

        /// <summary>Obtiene o establece la fecha de última modificación del registro.</summary>
        [Column("fechaEdicion")]
        public DateTime? FechaEdicion { get; set; }
    }
}
