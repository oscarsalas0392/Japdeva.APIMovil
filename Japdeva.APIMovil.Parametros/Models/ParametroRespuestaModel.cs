namespace Japdeva.APIMovil.Parametros.Models
{
    /// <summary>
    /// Modelo de respuesta para un parámetro del sistema.
    /// </summary>
    public class ParametroRespuestaModel
    {
        /// <summary>
        /// Obtiene o establece el identificador del parámetro.
        /// </summary>
        public int Id { get; set; }

        /// <summary>
        /// Obtiene o establece el nombre del parámetro.
        /// </summary>
        public string Nombre { get; set; } = string.Empty;

        /// <summary>
        /// Obtiene o establece el primer valor del parámetro. Para FAQ corresponde a la pregunta.
        /// </summary>
        public string Valor1 { get; set; } = string.Empty;

        /// <summary>
        /// Obtiene o establece el segundo valor del parámetro. Para FAQ corresponde a la respuesta.
        /// </summary>
        public string? Valor2 { get; set; }

        /// <summary>
        /// Obtiene o establece la descripción del parámetro.
        /// </summary>
        public string Descripcion { get; set; } = string.Empty;
    }
}
