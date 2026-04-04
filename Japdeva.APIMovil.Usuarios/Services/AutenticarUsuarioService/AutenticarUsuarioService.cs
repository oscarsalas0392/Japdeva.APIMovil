using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;
using Japdeva.APIMovil.Common.Extensions;
using Japdeva.APIMovil.Common.Repositories.ConsultarRepository;
using Japdeva.APIMovil.Usuarios.Entities;
using Japdeva.APIMovil.Usuarios.Models;

namespace Japdeva.APIMovil.Usuarios.Services.AutenticarUsuarioService
{
    /// <summary>
    /// Servicio para autenticar usuarios verificando sus credenciales contra la base de datos.
    /// </summary>
    public class AutenticarUsuarioService : IAutenticarUsuarioService
    {
        private readonly ILogger<AutenticarUsuarioService> _logger;
        private readonly IServiceProvider _serviceProvider;
        private const string MENSAJE_CREDENCIALES_INVALIDAS = "Credenciales inválidas.";


        /// <summary>
        /// Inicializa una nueva instancia de AutenticarUsuarioService.
        /// </summary>
        /// <param name="logger">Logger para registro de eventos.</param>
        /// <param name="serviceProvider">Proveedor de servicios para resolver dependencias.</param>
        public AutenticarUsuarioService(ILogger<AutenticarUsuarioService> logger, IServiceProvider serviceProvider)
        {
            this._logger = logger;
            this._serviceProvider = serviceProvider ?? throw new ArgumentNullException(nameof(serviceProvider));
        }

        /// <summary>
        /// Verifica las credenciales del usuario y devuelve sus datos si son válidas.
        /// </summary>
        /// <param name="traceId">Identificador de trazabilidad.</param>
        /// <param name="solicitud">Credenciales del usuario a autenticar.</param>
        /// <returns>Datos del usuario autenticado o respuesta 401 si las credenciales son inválidas.</returns>
        public async Task<IActionResult> AutenticarAsync(string traceId, AutenticarUsuarioSolicitudModel solicitud)
        {
            string nombreMetodo = this.ObtenerNombreMetodo();
            try
            {
                this._logger.Inicio(traceId, nombreMetodo);
                if (solicitud is null) throw new ArgumentNullException(nameof(solicitud));
                using var scope = this._serviceProvider.CreateScope();
                var consultarRepository = scope.ServiceProvider.GetRequiredService<IConsultarRepository>();
                var usuario = await consultarRepository.ConsultarAsync<UsuarioEntity>(
                    traceId, u => u.Correo == solicitud.Correo && u.Activo);
                if (usuario is null || usuario.Contrasena != solicitud.Contrasena) throw new UnauthorizedAccessException(MENSAJE_CREDENCIALES_INVALIDAS);
                var respuesta = new AutenticarUsuarioRespuestaModel();
                respuesta.Id = usuario.Id;
                respuesta.Nombre = usuario.Nombre;
                respuesta.Apellidos = usuario.Apellidos;
                respuesta.Correo = usuario.Correo;
                respuesta.Identificacion = usuario.Identificacion;
                return new OkObjectResult(respuesta);
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
