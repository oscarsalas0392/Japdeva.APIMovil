namespace Japdeva.APIMovil.Usuarios.Models
{
    /// <summary>
    /// Modelo de solicitud para asignar un usuario a un departamento.
    /// </summary>
    public class AgregarDepartamentoUsuarioSolicitudModel
    {
        /// <summary>Obtiene o establece el identificador del usuario a asignar.</summary>
        public int IdUsuario { get; set; }

        /// <summary>Obtiene o establece el identificador del departamento destino.</summary>
        public int IdDepartamento { get; set; }

        /// <summary>Obtiene o establece el identificador del administrador que realiza la asignación.</summary>
        public int IdUsuarioAdministrador { get; set; }
    }
}
