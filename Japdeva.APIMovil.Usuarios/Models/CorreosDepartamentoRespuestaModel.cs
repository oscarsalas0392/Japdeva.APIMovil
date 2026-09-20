namespace Japdeva.APIMovil.Usuarios.Models
{
    /// <summary>
    /// Modelo que representa los correos electrónicos de los usuarios asignados a un departamento.
    /// </summary>
    public class CorreosDepartamentoRespuestaModel
    {
        /// <summary>Lista de correos electrónicos activos del departamento.</summary>
        public List<string> Correos { get; set; } = [];
    }
}
