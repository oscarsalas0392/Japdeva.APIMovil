using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Japdeva.APIMovil.Reclamos.Entities
{
    /// <summary>
    /// Entidad que representa un detalle o seguimiento de una apelación de reclamo.
    /// Almacena los comentarios, cambios de estado y acciones realizadas durante
    /// el proceso de atención de la apelación.
    /// </summary>
    [Table("Tbl_DetalleApelacionReclamo")]
    public class DetalleApelacionReclamoEntity
    {
        /// <summary>
        /// Obtiene o establece el identificador único del detalle de apelación.
        /// </summary>
        [Column("id")]
        [Key]
        public long Id { get; set; }

        /// <summary>
        /// Obtiene o establece el identificador de la apelación a la cual pertenece este detalle.
        /// </summary>
        [Column("idApelacionReclamo")]
        public long IdApelacionReclamo { get; set; }

        /// <summary>
        /// Obtiene o establece el identificador del nivel de proceso asociado al detalle.
        /// Define la etapa del flujo de atención de la apelación.
        /// </summary>
        [Column("idNivelProceso")]
        public int IdNivelProceso { get; set; }

        /// <summary>
        /// Obtiene o establece el identificador del departamento asociado al detalle.
        /// </summary>
        [Column("idDepartamento")]
        public long IdDepartamento { get; set; }

        /// <summary>
        /// Obtiene o establece el identificador del estado del detalle de apelación.
        /// </summary>
        [Column("idEstadoDetalleReclamo")]
        public int IdEstadoDetalleReclamo { get; set; }

        /// <summary>
        /// Obtiene o establece el identificador del usuario interno que registró el detalle.
        /// </summary>
        [Column("idUsuarioInterno")]
        public long? IdUsuarioInterno { get; set; }

        /// <summary>
        /// Obtiene o establece la descripción o comentario del detalle de la apelación.
        /// </summary>
        [Column("descripcion")]
        [MaxLength(1000)]
        public string Descripcion { get; set; } = string.Empty;

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
    }
}
