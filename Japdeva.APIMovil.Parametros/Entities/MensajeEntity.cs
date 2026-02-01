using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Japdeva.APIMovil.Parametros.Entities
{
    /// <summary>
    /// Representa un mensaje asociado a una pantalla y usuario interno, incluyendo información de registro y estado.
    /// </summary>

    [Table("Tbl_Mensaje")]
    public class MensajeEntity
    {
        /// <summary>
        /// Obtiene o establece el identificador único del perfil.
        /// </summary>

        [Column("id")]
        [Key]
        public int Id { get; set; }

        /// <summary>
        /// Obtiene o establece la descripción del perfil.
        /// </summary>

        [Column("descripcion")]
        [MaxLength(300)]
        public string Descripcion { get; set; } = string.Empty;


        /// <summary>
        /// Obtiene o establece el identificador del tipo de mensaje asociado.
        /// </summary>
        [Column("idTipoMensaje")]
        public int IdTipoMensaje { get; set; }


        /// <summary>
        /// Obtiene o establece el identificador de la pantalla asociada al mensaje.
        /// </summary>
        [Column("idPantalla")]
        public int IdPantalla { get; set; }

        /// <summary>
        /// Obtiene o establece el identificador del usuario interno asociado al perfil.
        /// </summary>
        [Column("idUsuarioInterno")]
        public long? IdUsuarioInterno { get; set; }

        /// <summary>
        /// Obtiene o establece la fecha y hora de registro del detalle.
        /// </summary>
        [Column("fechaRegistro")]
        public DateTime FechaRegistro { get; set; }

        /// <summary>
        /// Obtiene o establece la fecha y hora de la última edición del detalle.
        /// </summary>
        [Column("fechaEdicion")]
        public DateTime? FechaEdicion { get; set; }

        /// <summary>
        /// Obtiene o establece si el perfil está activo.
        /// </summary>

        [Column("activo")]
        public bool Activo { get; set; }
    }
}
