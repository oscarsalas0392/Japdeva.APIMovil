using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;
using Japdeva.APIMovil.Common.Extensions;
using Japdeva.APIMovil.Common.Repositories.ActualizarRepository;
using Japdeva.APIMovil.Common.Repositories.ConsultarRepository;
using Japdeva.APIMovil.Usuarios.Entities;
using Japdeva.APIMovil.Usuarios.Models;

namespace Japdeva.APIMovil.Usuarios.Services.ActualizarContrasenaUsuarioService
{
    /// <summary>
    /// Servicio para actualizar la contraseña de un usuario validando la contraseña anterior.
    /// </summary>
    public class ActualizarContrasenaUsuarioService : IActualizarContrasenaUsuarioService
    {
        private readonly ILogger<ActualizarContrasenaUsuarioService> _logger;
        private readonly IServiceProvider _serviceProvider;
        private const string MENSAJE_USUARIO_NO_ENCONTRADO = "Usuario no encontrado.";
        private const string MENSAJE_CONTRASENA_ANTERIOR_REQUERIDA = "La contraseña anterior es requerida.";
        private const string MENSAJE_CONTRASENA_NUEVA_REQUERIDA = "La contraseña nueva es requerida.";
        private const string MENSAJE_CONTRASENA_ANTERIOR_INCORRECTA = "La contraseña anterior es incorrecta.";

        /// <summary>
        /// Inicializa una nueva instancia de ActualizarContrasenaUsuarioService.
        /// </summary>
        /// <param name="logger">Logger para registro de eventos.</param>
        /// <param name="serviceProvider">Proveedor de servicios para resolver dependencias.</param>
        public ActualizarContrasenaUsuarioService(ILogger<ActualizarContrasenaUsuarioService> logger, IServiceProvider serviceProvider)
        {
            this._logger = logger;
            this._serviceProvider = serviceProvider ?? throw new ArgumentNullException(nameof(serviceProvider));
        }

        /// <summary>
        /// Actualiza la contraseña de un usuario validando la contraseña anterior.
        /// </summary>
        /// <param name="traceId">Identificador de trazabilidad.</param>
        /// <param name="solicitud">Datos de la solicitud con la contraseña anterior y la nueva.</param>
        /// <returns>Resultado de la operación.</returns>
        public async Task<IActionResult> ActualizarContrasenaAsync(string traceId, ActualizarContrasenaUsuarioSolicitudModel solicitud)
        {
            string nombreMetodo = this.ObtenerNombreMetodo();
            try
            {
                this._logger.Inicio(traceId, nombreMetodo);
                if (solicitud is null) throw new ArgumentNullException(nameof(solicitud));
                if (string.IsNullOrEmpty(solicitud.ContrasenaAnterior)) throw new ArgumentException(MENSAJE_CONTRASENA_ANTERIOR_REQUERIDA);
                if (string.IsNullOrEmpty(solicitud.ContrasenaNueva)) throw new ArgumentException(MENSAJE_CONTRASENA_NUEVA_REQUERIDA);

                using var scope = this._serviceProvider.CreateScope();
                var consultarRepository = scope.ServiceProvider.GetRequiredService<IConsultarRepository>();
                var actualizarRepository = scope.ServiceProvider.GetRequiredService<IActualizarRepository>();

                var usuario = await consultarRepository.ConsultarAsync<UsuarioEntity>(traceId, u => u.Id == solicitud.Id && u.Activo);
                if (usuario is null) throw new KeyNotFoundException(MENSAJE_USUARIO_NO_ENCONTRADO);

                if (usuario.Contrasena != solicitud.ContrasenaAnterior)
                    throw new UnauthorizedAccessException(MENSAJE_CONTRASENA_ANTERIOR_INCORRECTA);

                usuario.Contrasena = solicitud.ContrasenaNueva;
                usuario.FechaExpiracionContrasena = null;
                usuario.FechaEdicion = DateTime.UtcNow;
                await actualizarRepository.ActualizarAsync<UsuarioEntity>(traceId, usuario);

                return new OkResult();
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
