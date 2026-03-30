using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Japdeva.APIMovil.EnvioCorreos.Entities
{
    /// <summary>
    /// Entidad que representa el histórico de correos enviados o con intentos agotados.
    /// </summary>
    [Table("Tbl_CorreoHistorico")]
    public class CorreoHistoricoEntity
    {
        /// <summary>Obtiene o establece el identificador único del registro histórico.</summary>
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

        /// <summary>Obtiene o establece el total de intentos realizados.</summary>
        [Column("intentos")]
        public int Intentos { get; set; }

        /// <summary>Obtiene o establece el mensaje del último error ocurrido.</summary>
        [Column("ultimoError")]
        public string? UltimoError { get; set; }

        /// <summary>Obtiene o establece si el cuerpo del correo está en formato HTML.</summary>
        [Column("esCuerpoHtml")]
        public bool EsCuerpoHtml { get; set; }

        /// <summary>Obtiene o establece si el correo fue enviado exitosamente.</summary>
        [Column("enviado")]
        public bool Enviado { get; set; }

        /// <summary>Obtiene o establece la fecha de creación del registro original.</summary>
        [Column("fechaRegistro")]
        public DateTime FechaRegistro { get; set; }

        /// <summary>Obtiene o establece la fecha de última modificación del registro original.</summary>
        [Column("fechaEdicion")]
        public DateTime? FechaEdicion { get; set; }

        /// <summary>Obtiene o establece la fecha en que el registro fue movido al histórico.</summary>
        [Column("fechaMovimiento")]
        public DateTime FechaMovimiento { get; set; } = DateTime.UtcNow;
    }
}
