using System.Linq.Expressions;
using Microsoft.AspNetCore.Mvc;
using Japdeva.APIMovil.Common.Extensions;
using Japdeva.APIMovil.Common.Models;
using Japdeva.APIMovil.Common.Repositories.AgregarRepository;
using Japdeva.APIMovil.Common.Repositories.ConsultarRepository;
using Japdeva.APIMovil.Usuarios.Entities;
using Japdeva.APIMovil.Usuarios.Models;

namespace Japdeva.APIMovil.Usuarios.Services.AgregarUsuarioService
{
    /// <summary>
    /// Servicio para el registro de nuevos usuarios en el sistema.
    /// </summary>
    public class AgregarUsuarioService : IAgregarUsuarioService
    {
        private readonly ILogger<AgregarUsuarioService> _logger;
        private readonly IAgregarRepository _agregarRepository;
        private readonly IConsultarRepository _consultarRepository;
        private const string MENSAJE_USUARIO_EXISTE = "Ya existe un usuario registrado con esa identificación.";
        private const string NOMBRE_ACCION_OBTENER = "ObtenerUsuarioPorId";
        private const string NOMBRE_CONTROLADOR = "Usuario";
        private const bool EXITO = true;
        private const bool ERROR = false;

        /// <summary>
        /// Inicializa una nueva instancia de AgregarUsuarioService.
        /// </summary>
        /// <param name="logger">Logger para registro de eventos.</param>
        /// <param name="agregarRepository">Repositorio para agregar entidades.</param>
        /// <param name="consultarRepository">Repositorio para consultar entidades.</param>
        public AgregarUsuarioService(ILogger<AgregarUsuarioService> logger, IAgregarRepository agregarRepository, IConsultarRepository consultarRepository)
        {
            this._logger = logger;
            this._agregarRepository = agregarRepository ?? throw new ArgumentNullException(nameof(agregarRepository));
            this._consultarRepository = consultarRepository ?? throw new ArgumentNullException(nameof(consultarRepository));
        }

        /// <summary>
        /// Registra un nuevo usuario en el sistema.
        /// </summary>
        /// <param name="traceId">Identificador de trazabilidad.</param>
        /// <param name="solicitud">Datos del usuario a registrar.</param>
        /// <returns>Resultado de la operación de registro.</returns>
        public async Task<IActionResult> AgregarUsuarioAsync(string traceId, AgregarUsuarioSolicitudModel solicitud)
        {
            string nombreMetodo = this.ObtenerNombreMetodo();
            try
            {
                this._logger.Inicio(traceId, nombreMetodo);

                if (solicitud is null) throw new ArgumentNullException(nameof(solicitud));

                Expression<Func<UsuarioEntity, bool>> filtro = u => u.Identificacion == solicitud.Identificacion;
                var usuarioExistente = await this._consultarRepository.ConsultarAsync<UsuarioEntity>(traceId, filtro);
                if (usuarioExistente is not null)
                    return new BadRequestObjectResult(new RespuestaModel { Mensaje = MENSAJE_USUARIO_EXISTE, Exito = ERROR });

                var nuevoUsuario = new UsuarioEntity();
                nuevoUsuario.Identificacion = solicitud.Identificacion;
                nuevoUsuario.IdTipoCedula = solicitud.IdTipoCedula;
                nuevoUsuario.Nombre = solicitud.Nombre;
                nuevoUsuario.Apellidos = solicitud.Apellidos;
                nuevoUsuario.Correo = solicitud.Correo;
                nuevoUsuario.Contrasena = solicitud.Contrasena;
                nuevoUsuario.FechaRegistro = DateTime.UtcNow;
                nuevoUsuario.Activo = EXITO;

                await this._agregarRepository.AgregarAsync<UsuarioEntity>(traceId, nuevoUsuario);

                var respuesta = new UsuarioRespuestaModel();
                respuesta.Id = nuevoUsuario.Id;
                respuesta.Identificacion = nuevoUsuario.Identificacion;
                respuesta.IdTipoCedula = nuevoUsuario.IdTipoCedula;
                respuesta.Nombre = nuevoUsuario.Nombre;
                respuesta.Apellidos = nuevoUsuario.Apellidos;
                respuesta.Correo = nuevoUsuario.Correo;
                respuesta.FechaRegistro = nuevoUsuario.FechaRegistro;
                respuesta.Activo = nuevoUsuario.Activo;

                return new CreatedAtActionResult(NOMBRE_ACCION_OBTENER, NOMBRE_CONTROLADOR, new { id = nuevoUsuario.Id }, respuesta);
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
