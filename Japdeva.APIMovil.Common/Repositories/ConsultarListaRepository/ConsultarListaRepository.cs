using System;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Japdeva.APIMovil.Common.Extensions;
using Japdeva.APIMovil.Common.Models;

namespace Japdeva.APIMovil.Common.Repositories.ConsultarListaRepository
{
    /// <summary>
    /// Repositorio para consultar listas de entidades con paginación y filtros.
    /// </summary>
    /// <typeparam name="T">Tipo de entidad a consultar</typeparam>
    public class ConsultarListaRepository : IConsultarListaRepository
    {
        private readonly DbContext _dbContext;
        private readonly ILogger<ConsultarListaRepository> _logger;
        private const string ERROR_PAGINA_MENOR_QUE_CERO = "El número de página debe ser mayor que cero.";
        private const int PAGINA_MINIMA = 0;
        private const int TAMANIO_PAGINA = 50;
        
        /// <summary>
        /// Constructor para el repositorio de consulta de listas.
        /// </summary>
        /// <param name="logger">Logger para registro de eventos</param>
        /// <param name="dbContext">Contexto de base de datos</param>
        public ConsultarListaRepository(ILogger<ConsultarListaRepository> logger, DbContext dbContext)
        {
            this._logger = logger;
            this._dbContext = dbContext;
        }


        /// <summary>
        /// Consulta una lista paginada de entidades que cumplen con el filtro especificado.
        /// </summary>
        /// <param name="traceId">Identificador de trazabilidad</param>
        /// <param name="pagina">Número de página a consultar (1-based)</param>
        /// <param name="filtro">Expresión de filtro para la consulta</param>
        /// <returns>Modelo de respuesta con la lista paginada y metadatos</returns>
        public async Task<RespuestaListaModel<T>> ConsultarListaAsync<T>(string traceId, int pagina, Expression<Func<T, bool>>? filtro = null) where T : class
        {
            string nombreMetodo = this.ObtenerNombreMetodo();
            try
            {
                this._logger.Inicio(traceId, nombreMetodo);
                RespuestaListaModel<T> respuesta = new RespuestaListaModel<T>();

                if (pagina <= PAGINA_MINIMA)
                {
                    throw new ArgumentOutOfRangeException(nameof(pagina), ERROR_PAGINA_MENOR_QUE_CERO);
                }

                IQueryable<T> query = this._dbContext.Set<T>();

                if (filtro is not null)
                {
                    query = query.Where(filtro);
                }

                int totalRegistros = await query.CountAsync();
                int totalPaginas = (int)Math.Ceiling((double)totalRegistros / TAMANIO_PAGINA);
                var resultados = await query
                    .Skip((pagina - 1) * TAMANIO_PAGINA)
                    .Take(TAMANIO_PAGINA)
                    .ToListAsync();

                respuesta.Lista = resultados;
                respuesta.TotalRegistros = totalRegistros;
                respuesta.CantidadPaginas = totalPaginas;
                respuesta.PaginaActual = pagina;

                return respuesta;
            }
            catch (Exception ex)
            {
                this._logger.Error(traceId, nombreMetodo, ex);
                throw;
            }
            finally
            {
                this._logger.Fin(traceId, nombreMetodo);
            }
        }
    }
}
