namespace Japdeva.APIMovil.Reclamos.Models
{
    /// <summary>
    /// Modelo que representa los correos electrónicos de los usuarios de un departamento, obtenidos desde la cola.
    /// </summary>
    public class CorreosDepartamentoRespuestaModel
    {
        /// <summary>Lista de correos electrónicos activos del departamento.</summary>
        public List<string> Correos { get; set; } = [];
    }
}
