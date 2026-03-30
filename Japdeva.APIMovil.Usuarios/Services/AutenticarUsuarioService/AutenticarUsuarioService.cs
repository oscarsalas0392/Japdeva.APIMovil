using Microsoft.AspNetCore.Mvc;
using Japdeva.APIMovil.Common.Extensions;
using Japdeva.APIMovil.Common.Models;
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
        private readonly IConsultarRepository _consultarRepository;
        private const string MENSAJE_AUTENTICACION_EXITOSA = "Autenticación exitosa.";
        private const string MENSAJE_CREDENCIALES_INVALIDAS = "Credenciales inválidas.";
        private const bool EXITO = true;
        private const bool ERROR = false;

        /// <summary>
        /// Inicializa una nueva instancia de AutenticarUsuarioService.
        /// </summary>
        /// <param name="logger">Logger para registro de eventos.</param>
        /// <param name="consultarRepository">Repositorio para consultar entidades.</param>
        public AutenticarUsuarioService(ILogger<AutenticarUsuarioService> logger, IConsultarRepository consultarRepository)
        {
            this._logger = logger;
            this._consultarRepository = consultarRepository ?? throw new ArgumentNullException(nameof(consultarRepository));
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

                var usuario = await this._consultarRepository.ConsultarAsync<UsuarioEntity>(
                    traceId, u => u.Correo == solicitud.Correo && u.Activo);

                if (usuario is null || usuario.Contrasena != solicitud.Contrasena)
                    return new UnauthorizedObjectResult(new RespuestaModel { Mensaje = MENSAJE_CREDENCIALES_INVALIDAS, Exito = ERROR });

                var respuesta = new AutenticarUsuarioRespuestaModel
                {
                    Id = usuario.Id,
                    Nombre = usuario.Nombre,
                    Apellidos = usuario.Apellidos,
                    Correo = usuario.Correo,
                    Identificacion = usuario.Identificacion
                };

                return new OkObjectResult(new RespuestaModel { Mensaje = MENSAJE_AUTENTICACION_EXITOSA, Exito = EXITO, Datos = respuesta });
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
