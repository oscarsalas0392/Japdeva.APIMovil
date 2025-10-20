using System.ComponentModel.DataAnnotations;

namespace Japdeva.APIMovil.Colas.Models
{
    /// <summary>
    /// Modelo para actualizar mensajes fallidos en el sistema de colas.
    /// </summary>
    public class ActualizarMensajeFallidoModel
    {
        private const string MENSAJE_ERROR_ID_REQUERIDO = "El identificador del mensaje es requerido.";
        private const string MENSAJE_ERROR_ID_MAYOR_CERO = "El identificador debe ser mayor a cero.";
        private const string MENSAJE_ERROR_TRACEID_REQUERIDO = "El TraceId es requerido.";
        private const string MENSAJE_ERROR_TRACEID_LONGITUD = "El TraceId no puede exceder 100 caracteres.";
        private const string MENSAJE_ERROR_DESCRIPCION_REQUERIDA = "La descripción del error es requerida.";
        private const string MENSAJE_ERROR_DESCRIPCION_LONGITUD = "La descripción del error no puede exceder 500 caracteres.";
        private const int VALOR_MINIMO_ID = 1;
        private const int LONGITUD_MAXIMA_TRACEID = 100;
        private const int LONGITUD_MAXIMA_DESCRIPCION = 500;

        /// <summary>
        /// Identificador único del mensaje a actualizar.
        /// </summary>
        [Required(ErrorMessage = MENSAJE_ERROR_ID_REQUERIDO)]
        [Range(VALOR_MINIMO_ID, int.MaxValue, ErrorMessage = MENSAJE_ERROR_ID_MAYOR_CERO)]
        public int Id { get; set; }

        /// <summary>
        /// Identificador de trazabilidad del mensaje original.
        /// </summary>
        [Required(ErrorMessage = MENSAJE_ERROR_TRACEID_REQUERIDO)]
        [StringLength(LONGITUD_MAXIMA_TRACEID, ErrorMessage = MENSAJE_ERROR_TRACEID_LONGITUD)]
        public string TraceId { get; set; } = string.Empty;

        /// <summary>
        /// Descripción del error que causó el fallo.
        /// </summary>
        [Required(ErrorMessage = MENSAJE_ERROR_DESCRIPCION_REQUERIDA)]
        [StringLength(LONGITUD_MAXIMA_DESCRIPCION, ErrorMessage = MENSAJE_ERROR_DESCRIPCION_LONGITUD)]
        public string DescripcionError { get; set; } = string.Empty;

        /// <summary>
        /// Indica si se debe incrementar el contador de reintentos.
        /// </summary>
        public bool IncrementarReintentos { get; set; } = true;
    }
}