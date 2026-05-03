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
        /// Obtiene o establece el valor del parámetro.
        /// </summary>
        public string Valor { get; set; } = string.Empty;

        /// <summary>
        /// Obtiene o establece la descripción del parámetro.
        /// </summary>
        public string Descripcion { get; set; } = string.Empty;
    }
}
