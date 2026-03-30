using Microsoft.AspNetCore.Mvc;
using Japdeva.APIMovil.Common.Extensions;
using Japdeva.APIMovil.Common.Models;
using Japdeva.APIMovil.Usuarios.Models;
using Japdeva.APIMovil.Usuarios.Services.TipoCedulaCacheService;

namespace Japdeva.APIMovil.Usuarios.Services.ObtenerTiposCedulaService
{
    /// <summary>
    /// Servicio para consultar los tipos de cédula desde caché.
    /// </summary>
    public class ObtenerTiposCedulaService : IObtenerTiposCedulaService
    {
        private readonly ILogger<ObtenerTiposCedulaService> _logger;
        private readonly ITipoCedulaCacheService _tipoCedulaCacheService;
        private const string MENSAJE_TIPO_CEDULA_NO_ENCONTRADO = "Tipo de cédula no encontrado.";
        private const string MENSAJE_TIPO_CEDULA_OBTENIDO = "Tipo de cédula obtenido correctamente.";
        private const bool EXITO = true;
        private const bool ERROR = false;

        /// <summary>
        /// Inicializa una nueva instancia de ObtenerTiposCedulaService.
        /// </summary>
        /// <param name="logger">Logger para registro de eventos.</param>
        /// <param name="tipoCedulaCacheService">Servicio de caché de tipos de cédula.</param>
        public ObtenerTiposCedulaService(ILogger<ObtenerTiposCedulaService> logger, ITipoCedulaCacheService tipoCedulaCacheService)
        {
            this._logger = logger;
            this._tipoCedulaCacheService = tipoCedulaCacheService;
        }

        /// <summary>
        /// Retorna todos los tipos de cédula activos almacenados en caché.
        /// </summary>
        /// <param name="traceId">Identificador de trazabilidad.</param>
        /// <returns>Lista de tipos de cédula activos.</returns>
        public async Task<IActionResult> ObtenerTodosLosTiposCedulaAsync(string traceId)
        {
            string nombreMetodo = this.ObtenerNombreMetodo();
            try
            {
                this._logger.Inicio(traceId, nombreMetodo);
                var tipos = this._tipoCedulaCacheService.ObtenerTodos(traceId);
                var lista = new List<TipoCedulaRespuestaModel>();
                foreach (var tipo in tipos)
                {
                    var modelo = new TipoCedulaRespuestaModel();
                    modelo.Id = tipo.Id;
                    modelo.Tipo = tipo.Tipo;
                    modelo.Formato = tipo.Formato;
                    modelo.Activo = tipo.Activo;
                    lista.Add(modelo);
                }
                return await Task.FromResult(new OkObjectResult(lista) as IActionResult);
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

        /// <summary>
        /// Retorna el tipo de cédula activo con el identificador indicado.
        /// </summary>
        /// <param name="traceId">Identificador de trazabilidad.</param>
        /// <param name="id">Identificador del tipo de cédula.</param>
        /// <returns>Resultado con el tipo de cédula encontrado.</returns>
        public async Task<IActionResult> ObtenerTipoCedulaPorIdAsync(string traceId, int id)
        {
            string nombreMetodo = this.ObtenerNombreMetodo();
            IActionResult resultado;
            try
            {
                this._logger.Inicio(traceId, nombreMetodo);
                var tipo = this._tipoCedulaCacheService.ObtenerPorId(traceId, id);
                if (tipo is null)
                {
                    resultado = new NotFoundObjectResult(new RespuestaModel { Mensaje = MENSAJE_TIPO_CEDULA_NO_ENCONTRADO, Exito = ERROR });
                }
                else
                {
                    var respuesta = new TipoCedulaRespuestaModel();
                    respuesta.Id = tipo.Id;
                    respuesta.Tipo = tipo.Tipo;
                    respuesta.Formato = tipo.Formato;
                    respuesta.Activo = tipo.Activo;
                    resultado = new OkObjectResult(new RespuestaModel { Mensaje = MENSAJE_TIPO_CEDULA_OBTENIDO, Exito = EXITO, Datos = respuesta });
                }
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
            return await Task.FromResult(resultado);
        }
    }
}
