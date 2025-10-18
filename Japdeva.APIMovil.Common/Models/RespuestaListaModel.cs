namespace Japdeva.APIMovil.Common.Models
{
    /// <summary>
    /// Modelo de respuesta para listas paginadas de tipo T
    /// </summary>
    /// <typeparam name="T">Tipo de datos en la lista</typeparam>
    public class RespuestaListaModel<T>
    {
        /// <summary>
        /// Número total de registros
        /// </summary>
        public int TotalRegistros { get; set; }
        
        /// <summary>
        /// Cantidad total de páginas
        /// </summary>
        public int CantidadPaginas { get; set; }
        
        /// <summary>
        /// Número de la página actual
        /// </summary>
        public int PaginaActual { get; set; }
        
        /// <summary>
        /// Lista de elementos de tipo T
        /// </summary>
        public List<T> Lista { get; set; } = new List<T>();

    }
}