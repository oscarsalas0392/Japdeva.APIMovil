namespace Japdeva.APIMovil.Reclamos.Models
{
    /// <summary>
    /// Modelo que representa la respuesta de un reclamo, incluyendo información relevante como estado, usuario y departamento.
    /// </summary>
    public class ReclamoRespuestaModel
    {
        /// <summary>
        /// Identificador único del reclamo.
        /// </summary>
        public long Id { get; set; }

        /// <summary>
        /// Título del reclamo.
        /// </summary>
        public string Titulo { get; set; } = string.Empty;

        /// <summary>
        /// Descripción detallada del reclamo.
        /// </summary>
        public string Descripcion { get; set; } = string.Empty;

        /// <summary>
        /// Identificador del estado actual del reclamo.
        /// </summary>
        public int IdEstadoReclamo { get; set; }

        /// <summary>
        /// Identificador del usuario externo que realizó el reclamo.
        /// </summary>
        public long IdUsuarioExterno { get; set; }

        /// <summary>
        /// Fecha en que se registró el reclamo.
        /// </summary>
        public DateTime FechaRegistro { get; set; }

        /// <summary>
        /// Identificador del departamento actual encargado del reclamo.
        /// </summary>
        public long IdDepartamentoActual { get; set; }

        /// <summary>
        /// Descripción del departamento actual encargado del reclamo.
        /// </summary>
        public string DescripcionDepartamento { get; set; } = string.Empty;
    }
}
