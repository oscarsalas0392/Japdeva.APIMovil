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
        private readonly string _tamanioPagina = Environment.GetEnvironmentVariable("TAMANIO_PAGINA_LISTA") ?? TAMANIO_PAGINA_DEFECTO;
        private const string ERROR_PAGINA_MENOR_QUE_CERO = "El número de página debe ser mayor que cero.";
        private const string ERROR_TAMANIO_PAGINA_INVALIDO = "El tamaño de página configurado no es válido.";
        private const int PAGINA_MINIMA = 0;
        private const string TAMANIO_PAGINA_DEFECTO = "50";
        private const int AJUSTE_PAGINA_BASE_CERO = 1;

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
        /// <returns>Una tarea que representa la operación asíncrona que contiene la respuesta con la lista paginada de entidades</returns>
        public async Task<RespuestaListaModel<T>> ConsultarListaAsync<T>(string traceId, int pagina, Expression<Func<T, bool>>? filtro = null) where T : class
        {
            string nombreMetodo = this.ObtenerNombreMetodo();
            try
            {
                this._logger.Inicio(traceId, nombreMetodo);
                RespuestaListaModel<T> respuesta = new RespuestaListaModel<T>();
                int tamanioPagina;

                if (pagina <= PAGINA_MINIMA)
                {
                    throw new ArgumentOutOfRangeException(nameof(pagina), ERROR_PAGINA_MENOR_QUE_CERO);
                }

                if (int.TryParse(this._tamanioPagina, out tamanioPagina))
                {
                    throw new Exception(ERROR_TAMANIO_PAGINA_INVALIDO);
                }

                IQueryable<T> query = this._dbContext.Set<T>().AsNoTracking();

                if (filtro is not null)
                {
                    query = query.Where(filtro);
                }

                int totalRegistros = await query.CountAsync();
                var resultados = await query
                    .Skip((pagina - AJUSTE_PAGINA_BASE_CERO) * tamanioPagina)
                    .Take(tamanioPagina)
                    .ToListAsync();

                int totalPaginas = (int)Math.Ceiling((double)totalRegistros / tamanioPagina);

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
