using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Japdeva.APIMovil.Reclamos.Entities
{
    /// <summary>
    /// Entidad que representa una apelación presentada sobre un reclamo resuelto.
    /// Almacena la información principal de la apelación iniciada por el usuario externo
    /// cuando no está conforme con la resolución emitida en el proceso de Reclamo.
    /// </summary>
    [Table("Tbl_ApelacionReclamo")]
    public class ApelacionReclamoEntity
    {
        /// <summary>
        /// Obtiene o establece el identificador único de la apelación.
        /// </summary>
        [Column("id")]
        [Key]
        public long Id { get; set; }

        /// <summary>
        /// Obtiene o establece el identificador del reclamo sobre el cual se presenta la apelación.
        /// </summary>
        [Column("idReclamo")]
        [Required]
        public long IdReclamo { get; set; }

        /// <summary>
        /// Obtiene o establece el título descriptivo de la apelación.
        /// </summary>
        [Column("titulo")]
        [MaxLength(200)]
        [MinLength(1)]
        [Required]
        public string Titulo { get; set; } = string.Empty;

        /// <summary>
        /// Obtiene o establece la descripción detallada con los argumentos de la apelación.
        /// </summary>
        [Column("descripcion")]
        [MaxLength(1000)]
        [MinLength(1)]
        [Required]
        public string Descripcion { get; set; } = string.Empty;

        /// <summary>
        /// Obtiene o establece el identificador del estado actual de la apelación.
        /// </summary>
        [Column("idEstadoReclamo")]
        [Required]
        public int IdEstadoReclamo { get; set; }

        /// <summary>
        /// Obtiene o establece el identificador del usuario externo que presentó la apelación.
        /// </summary>
        [Column("idUsuarioExterno")]
        public long IdUsuarioExterno { get; set; }

        /// <summary>
        /// Obtiene o establece la fecha y hora de registro de la apelación.
        /// </summary>
        [Column("fechaRegistro")]
        [Required]
        public DateTime FechaRegistro { get; set; }

        /// <summary>
        /// Obtiene o establece el identificador del departamento que atiende actualmente la apelación.
        /// </summary>
        [Column("idDepartamentoActual")]
        [Required]
        public long IdDepartamentoActual { get; set; }

        /// <summary>
        /// Obtiene o establece la descripción de la resolución emitida para la apelación.
        /// </summary>
        [Column("descripcionResolucion")]
        [MaxLength(200)]
        public string DescripcionResolucion { get; set; } = string.Empty;

    }
}
