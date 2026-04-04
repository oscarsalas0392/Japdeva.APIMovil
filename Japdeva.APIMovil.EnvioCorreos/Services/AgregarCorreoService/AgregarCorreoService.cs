using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;
using Japdeva.APIMovil.Common.Extensions;
using Japdeva.APIMovil.Common.Models;
using Japdeva.APIMovil.Common.Repositories.AgregarRepository;
using Japdeva.APIMovil.EnvioCorreos.Entities;
using Japdeva.APIMovil.EnvioCorreos.Models;

namespace Japdeva.APIMovil.EnvioCorreos.Services.AgregarCorreoService
{
    /// <summary>
    /// Servicio para el registro de correos electrónicos en la cola de envío.
    /// </summary>
    public class AgregarCorreoService : IAgregarCorreoService
    {
        private readonly ILogger<AgregarCorreoService> _logger;
        private readonly IServiceProvider _serviceProvider;
        private const string MENSAJE_CORREO_REGISTRADO = "Correo registrado en la cola de envío correctamente.";
        private const int INTENTOS_INICIALES = 0;
        private const bool EXITO = true;
        private const bool ENVIADO_INICIAL = false;

        /// <summary>
        /// Inicializa una nueva instancia de AgregarCorreoService.
        /// </summary>
        /// <param name="logger">Logger para registro de eventos.</param>
        /// <param name="serviceProvider">Proveedor de servicios para resolver dependencias.</param>
        public AgregarCorreoService(ILogger<AgregarCorreoService> logger, IServiceProvider serviceProvider)
        {
            this._logger = logger;
            this._serviceProvider = serviceProvider ?? throw new ArgumentNullException(nameof(serviceProvider));
        }

        /// <summary>
        /// Registra un nuevo correo en la cola de envío.
        /// </summary>
        /// <param name="traceId">Identificador de trazabilidad.</param>
        /// <param name="solicitud">Datos del correo a registrar.</param>
        /// <returns>Resultado de la operación de registro.</returns>
        public async Task<IActionResult> AgregarCorreoAsync(string traceId, AgregarCorreoSolicitudModel solicitud)
        {
            string nombreMetodo = this.ObtenerNombreMetodo();
            try
            {
                this._logger.Inicio(traceId, nombreMetodo);

                if (solicitud is null) throw new ArgumentNullException(nameof(solicitud));

                using var scope = this._serviceProvider.CreateScope();
                var agregarRepository = scope.ServiceProvider.GetRequiredService<IAgregarRepository>();
                var nuevoCorreo = new CorreoPendienteEntity();
                nuevoCorreo.Destinatario = solicitud.Destinatario;
                nuevoCorreo.Asunto = solicitud.Asunto;
                nuevoCorreo.Cuerpo = solicitud.Cuerpo;
                nuevoCorreo.EsCuerpoHtml = solicitud.EsCuerpoHtml;
                nuevoCorreo.Intentos = INTENTOS_INICIALES;
                nuevoCorreo.Enviado = ENVIADO_INICIAL;
                nuevoCorreo.FechaRegistro = DateTime.UtcNow;

                await agregarRepository.AgregarAsync<CorreoPendienteEntity>(traceId, nuevoCorreo);

                var respuesta = new CorreoRespuestaModel();
                respuesta.Id = nuevoCorreo.Id;
                respuesta.Destinatario = nuevoCorreo.Destinatario;
                respuesta.Asunto = nuevoCorreo.Asunto;
                respuesta.Intentos = nuevoCorreo.Intentos;
                respuesta.Enviado = nuevoCorreo.Enviado;
                respuesta.FechaRegistro = nuevoCorreo.FechaRegistro;

                return new OkObjectResult(new RespuestaModel { Mensaje = MENSAJE_CORREO_REGISTRADO, Exito = EXITO, Datos = respuesta });
            }
            catch (ArgumentException ex)
            {
                this._logger.Error(traceId, nombreMetodo, ex);
                throw;
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
