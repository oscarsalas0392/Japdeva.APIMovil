
namespace Japdeva.APIMovil.Common.Models
{
    /// <summary>
    /// Modelo de respuesta genérico para las API.
    /// </summary>
    public class RespuestaModel
    {
        /// <summary>
        /// Obtiene o establece el estado de la respuesta.
        /// </summary>
        public bool Exito { get; set; }

        /// <summary>
        /// Obtiene o establece el mensaje de la respuesta.
        /// </summary>
        public string Mensaje { get; set; } = string.Empty;

        /// <summary>
        /// Obtiene o establece los datos de la respuesta.
        /// </summary>
        public object? Datos { get; set; }
    }
}
