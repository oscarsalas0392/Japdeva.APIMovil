
namespace Japdeva.APIMovil.Colas.Models
{
    /// <summary>
    /// Modelo para consultar mensajes en cola con paginación.
    /// </summary>
    public class ConsultarMensajesModel
    {
        private const int NUMERO_PAGINA_PREDETERMINADO = 1;
        private const int TAMANO_PAGINA_PREDETERMINADO = 10;
        private const string CAMPO_ORDEN_PREDETERMINADO = "FechaCreacion";
        /// <summary>
        /// Obtiene o establece el nombre de la cola a consultar.
        /// </summary>
        public string? NombreCola { get; set; }

        /// <summary>
        /// Obtiene o establece el estado de los mensajes a consultar.
        /// </summary>
        public EstadoMensajeModel? Estado { get; set; }

        /// <summary>
        /// Obtiene o establece el tipo de mensaje a consultar.
        /// </summary>
        public string? TipoMensaje { get; set; }

        /// <summary>
        /// Obtiene o establece el número de página para paginación.
        /// </summary>
        public int NumeroPagina { get; set; } = NUMERO_PAGINA_PREDETERMINADO;

        /// <summary>
        /// Obtiene o establece el tamaño de página para paginación.
        /// </summary>
        public int TamanoPagina { get; set; } = TAMANO_PAGINA_PREDETERMINADO;

        /// <summary>
        /// Obtiene o establece la fecha de inicio para filtro por rango.
        /// </summary>
        public DateTime? FechaInicio { get; set; }

        /// <summary>
        /// Obtiene o establece la fecha de fin para filtro por rango.
        /// </summary>
        public DateTime? FechaFin { get; set; }

        /// <summary>
        /// Obtiene o establece el campo por el cual ordenar los resultados.
        /// </summary>
        public string OrdenarPor { get; set; } = CAMPO_ORDEN_PREDETERMINADO;

        /// <summary>
        /// Obtiene o establece si el ordenamiento es descendente.
        /// </summary>
        public bool OrdenDescendente { get; set; } = true;
    }
}