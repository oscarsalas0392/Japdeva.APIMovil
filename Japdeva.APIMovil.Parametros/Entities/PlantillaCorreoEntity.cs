using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Japdeva.APIMovil.Parametros.Entities
{
    /// <summary>
    /// Representa una plantilla de correo electrónico utilizada para la generación de mensajes automatizados.
    /// </summary>
    [Table("Tbl_PlantillaCorreo")]
    public class PlantillaCorreoEntity
    {
        /// <summary>
        /// Obtiene o establece el identificador único de la plantilla de correo.
        /// </summary>
        [Column("id")]
        [Key]
        public int Id { get; set; }

        /// <summary>
        /// Obtiene o establece el contenido de la plantilla de correo.
        /// </summary>
        [Column("Plantilla")]
        public string Plantilla { get; set; } = string.Empty;

        /// <summary>
        /// Obtiene o establece el identificador del usuario interno asociado a la plantilla.
        /// </summary>
        [Column("idUsuarioInterno")]
        public long? IdUsuarioInterno { get; set; }

        /// <summary>
        /// Obtiene o establece la fecha y hora de registro de la plantilla.
        /// </summary>
        [Column("fechaRegistro")]
        public DateTime FechaRegistro { get; set; }

        /// <summary>
        /// Obtiene o establece la fecha y hora de la última edición de la plantilla.
        /// </summary>
        [Column("fechaEdicion")]
        public DateTime? FechaEdicion { get; set; }

        /// <summary>
        /// Obtiene o establece si la plantilla está activa.
        /// </summary>
        [Column("activo")]
        public bool Activo { get; set; }
    }
}
