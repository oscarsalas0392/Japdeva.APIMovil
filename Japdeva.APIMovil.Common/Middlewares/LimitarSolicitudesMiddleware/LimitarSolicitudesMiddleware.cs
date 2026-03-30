using System.Collections.Concurrent;
using System.Net;
using System.Threading.RateLimiting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Japdeva.APIMovil.Common.Extensions;
using Japdeva.APIMovil.Common.Models;

namespace Japdeva.APIMovil.Common.Middlewares
{
    /// <summary>
    /// Middleware que limita el número de solicitudes por IP en rutas configuradas.
    /// </summary>
    public class LimitarSolicitudesMiddleware : ILimitarSolicitudesMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<LimitarSolicitudesMiddleware> _logger;
        private readonly string _rutaLimitada = Environment.GetEnvironmentVariable(RUTA_AUTENTICAR_ENV) ?? string.Empty;
        private readonly ConcurrentDictionary<string, RateLimiter> _limitadores = new();
        private const string RUTA_AUTENTICAR_ENV = "RUTA_AUTENTICAR";
        private const string MENSAJE_LIMITE_EXCEDIDO = "Ha excedido el límite de intentos. Intente más tarde.";
        private const string IP_DESCONOCIDA = "desconocida";
        private const string TRACE_ID = "SYSTEM";
        private const char SEPARADOR_RUTA = '/';
        private const int LIMITE_SOLICITUDES = 5;
        private const int VENTANA_MINUTOS = 1;
        private const int COLA_LIMITE = 0;
        private const bool EXITO = false;

        /// <summary>
        /// Inicializa una nueva instancia del middleware de limitación de solicitudes.
        /// </summary>
        /// <param name="next">Siguiente middleware en el pipeline.</param>
        /// <param name="logger">Logger para registrar información.</param>
        public LimitarSolicitudesMiddleware(RequestDelegate next, ILogger<LimitarSolicitudesMiddleware> logger)
        {
            this._next = next;
            this._logger = logger;
        }

        /// <summary>
        /// Método de invocación del middleware que aplica el límite de solicitudes.
        /// </summary>
        /// <param name="context">Contexto HTTP de la solicitud.</param>
        public async Task InvokeAsync(HttpContext context)
        {
            string nombreMetodo = this.ObtenerNombreMetodo();
            try
            {
                this._logger.Inicio(TRACE_ID, nombreMetodo);
                string[] segmentos = context.Request.Path.ToString().Split(SEPARADOR_RUTA);
                bool esRutaLimitada = !string.IsNullOrEmpty(this._rutaLimitada)
                    && segmentos.Any(s => s.Equals(this._rutaLimitada, StringComparison.OrdinalIgnoreCase));
                if (esRutaLimitada)
                {
                    string ip = context.Connection.RemoteIpAddress?.ToString() ?? IP_DESCONOCIDA;
                    RateLimiter limitador = this._limitadores.GetOrAdd(ip, _ => this.CrearLimitador());
                    using RateLimitLease lease = await limitador.AcquireAsync();
                    if (!lease.IsAcquired)
                    {
                        context.Response.StatusCode = (int)HttpStatusCode.TooManyRequests;
                        RespuestaModel respuesta = new RespuestaModel();
                        respuesta.Identificador = context.TraceIdentifier;
                        respuesta.Exito = EXITO;
                        respuesta.Mensaje = MENSAJE_LIMITE_EXCEDIDO;
                        await context.Response.WriteAsJsonAsync(respuesta);
                        return;
                    }
                }
                await this._next(context);
            }
            catch (Exception ex)
            {
                this._logger.Error(TRACE_ID, nombreMetodo, ex);
                throw;
            }
            finally
            {
                this._logger.Fin(TRACE_ID, nombreMetodo);
            }
        }

        /// <summary>
        /// Crea un nuevo limitador de tasa de ventana fija para una IP determinada.
        /// </summary>
        /// <returns>Instancia de <see cref="RateLimiter"/> configurada.</returns>
        public RateLimiter CrearLimitador()
        {
            string nombreMetodo = this.ObtenerNombreMetodo();
            RateLimiter resultado;
            try
            {
                this._logger.Inicio(TRACE_ID, nombreMetodo);
                FixedWindowRateLimiterOptions opciones = new FixedWindowRateLimiterOptions();
                opciones.PermitLimit = LIMITE_SOLICITUDES;
                opciones.Window = TimeSpan.FromMinutes(VENTANA_MINUTOS);
                opciones.QueueProcessingOrder = QueueProcessingOrder.OldestFirst;
                opciones.QueueLimit = COLA_LIMITE;
                resultado = new FixedWindowRateLimiter(opciones);
            }
            catch (Exception ex)
            {
                this._logger.Error(TRACE_ID, nombreMetodo, ex);
                throw;
            }
            finally
            {
                this._logger.Fin(TRACE_ID, nombreMetodo);
            }
            return resultado;
        }
    }
}
