using System.Text.RegularExpressions;
using Microsoft.AspNetCore.Mvc;
using Japdeva.APIMovil.Common.Extensions;
using Japdeva.APIMovil.Common.Repositories.ActualizarRepository;
using Japdeva.APIMovil.Common.Repositories.ConsultarRepository;
using Japdeva.APIMovil.Usuarios.Entities;
using Japdeva.APIMovil.Usuarios.Models;
using Japdeva.APIMovil.Usuarios.Services.NotificarContrasenaTemporalService;

namespace Japdeva.APIMovil.Usuarios.Services.OlvidarContrasenaService
{
    /// <summary>
    /// Servicio para la recuperación de contraseña mediante generación de clave temporal.
    /// </summary>
    public class OlvidarContrasenaService : IOlvidarContrasenaService
    {
        private readonly ILogger<OlvidarContrasenaService> _logger;
        private readonly IServiceProvider _serviceProvider;
        private readonly INotificarContrasenaTemporalService _notificarContrasenaTemporalService;
        private const string MENSAJE_CORREO_REQUERIDO = "El correo es requerido.";
        private const string MENSAJE_CORREO_INVALIDO = "El formato del correo no es válido.";
        private const string PATRON_CORREO = @"^[^@\s]+@[^@\s]+\.[^@\s]+$";
        private const string CARACTERES_CONTRASENA = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789";
        private const int LONGITUD_CONTRASENA_TEMPORAL = 10;
        private const int INDICE_INICIAL = 0;
        private const int HORAS_VALIDEZ_CONTRASENA_TEMPORAL = 24;

        /// <summary>
        /// Inicializa una nueva instancia de OlvidarContrasenaService.
        /// </summary>
        /// <param name="logger">Logger para registro de eventos.</param>
        /// <param name="serviceProvider">Proveedor de servicios para resolver dependencias.</param>
        /// <param name="notificarContrasenaTemporalService">Servicio para enviar la contraseña temporal por correo.</param>
        public OlvidarContrasenaService(
            ILogger<OlvidarContrasenaService> logger,
            IServiceProvider serviceProvider,
            INotificarContrasenaTemporalService notificarContrasenaTemporalService)
        {
            this._logger = logger;
            this._serviceProvider = serviceProvider ?? throw new ArgumentNullException(nameof(serviceProvider));
            this._notificarContrasenaTemporalService = notificarContrasenaTemporalService;
        }

        /// <summary>
        /// Genera una contraseña temporal, la persiste y dispara en segundo plano el envío del correo.
        /// </summary>
        /// <param name="traceId">Identificador de trazabilidad.</param>
        /// <param name="solicitud">Datos de la solicitud con el correo del usuario.</param>
        /// <returns>Resultado de la operación.</returns>
        public async Task<IActionResult> OlvidarContrasenaAsync(string traceId, OlvidarContrasenaSolicitudModel solicitud)
        {
            string nombreMetodo = this.ObtenerNombreMetodo();
            try
            {
                this._logger.Inicio(traceId, nombreMetodo);
                if (solicitud is null) throw new ArgumentNullException(nameof(solicitud));
                if (string.IsNullOrEmpty(solicitud.Correo)) throw new ArgumentException(MENSAJE_CORREO_REQUERIDO);
                if (!Regex.IsMatch(solicitud.Correo, PATRON_CORREO)) throw new ArgumentException(MENSAJE_CORREO_INVALIDO);

                using var scope = this._serviceProvider.CreateScope();
                var consultarRepository = scope.ServiceProvider.GetRequiredService<IConsultarRepository>();
                var actualizarRepository = scope.ServiceProvider.GetRequiredService<IActualizarRepository>();

                var usuario = await consultarRepository.ConsultarAsync<UsuarioEntity>(traceId, u => u.Correo == solicitud.Correo && u.Activo);

                if (usuario is not null)
                {
                    string contrasenaTemporal = this.GenerarContrasenaTemporal(traceId);
                    usuario.Contrasena = contrasenaTemporal;
                    usuario.FechaExpiracionContrasena = DateTime.UtcNow.AddHours(HORAS_VALIDEZ_CONTRASENA_TEMPORAL);
                    usuario.FechaEdicion = DateTime.UtcNow;

                    await actualizarRepository.ActualizarAsync<UsuarioEntity>(traceId, usuario);

                    _ = Task.Run(() => this._notificarContrasenaTemporalService.NotificarAsync(traceId, usuario.Nombre, usuario.Correo, contrasenaTemporal));
                }

                return new OkResult();
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
        /// Genera una contraseña temporal aleatoria de longitud fija.
        /// </summary>
        /// <param name="traceId">Identificador de trazabilidad.</param>
        /// <returns>Contraseña temporal generada.</returns>
        public string GenerarContrasenaTemporal(string traceId)
        {
            string nombreMetodo = this.ObtenerNombreMetodo();
            try
            {
                this._logger.Inicio(traceId, nombreMetodo);
                char[] contrasena = new char[LONGITUD_CONTRASENA_TEMPORAL];
                for (int i = INDICE_INICIAL; i < LONGITUD_CONTRASENA_TEMPORAL; i++)
                {
                    contrasena[i] = CARACTERES_CONTRASENA[Random.Shared.Next(CARACTERES_CONTRASENA.Length)];
                }
                return new string(contrasena);
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
