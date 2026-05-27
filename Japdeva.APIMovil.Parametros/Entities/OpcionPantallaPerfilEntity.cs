using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Japdeva.APIMovil.Parametros.Entities
{
    /// <summary>
    /// Representa la entidad que asocia una opción de pantalla con un perfil de usuario.
    /// </summary>
    [Table("Tbl_OpcionPantallaPerfil")]
    public class OpcionPantallaPerfilEntity
    {
        /// <summary>
        /// Obtiene o establece el identificador único de la asociación.
        /// </summary>
        [Column("id")]
        [Key]
        public int Id { get; set; }

        /// <summary>
        /// Obtiene o establece el identificador de la opción de pantalla.
        /// </summary>
        [Column("idOpcionPantalla")]
        public int IdOpcionPantalla { get; set; }

        /// <summary>
        /// Obtiene o establece el identificador del perfil asociado.
        /// </summary>
        [Column("idPerfil")]
        public int IdPerfil { get; set; }

        /// <summary>
        /// Obtiene o establece el identificador del usuario interno que registró la asociación.
        /// </summary>
        [Column("idUsuarioInterno")]
        public long? IdUsuarioInterno { get; set; }

        /// <summary>
        /// Obtiene o establece la fecha y hora de registro.
        /// </summary>
        [Column("fechaRegistro")]
        public DateTime FechaRegistro { get; set; }

        /// <summary>
        /// Obtiene o establece la fecha y hora de la última edición.
        /// </summary>
        [Column("fechaEdicion")]
        public DateTime? FechaEdicion { get; set; }

        /// <summary>
        /// Obtiene o establece si la asociación está activa.
        /// </summary>
        [Column("activo")]
        public bool Activo { get; set; }
    }
}
