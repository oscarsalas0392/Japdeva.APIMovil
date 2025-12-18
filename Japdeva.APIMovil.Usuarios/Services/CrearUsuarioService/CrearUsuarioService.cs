using System.Linq.Expressions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Japdeva.APIMovil.Common.Extensions;
using Japdeva.APIMovil.Common.Models;
using Japdeva.APIMovil.Common.Repositories.AgregarRepository;
using Japdeva.APIMovil.Common.Repositories.ConsultarRepository;
using Japdeva.APIMovil.Usuarios.Entities;
using Japdeva.APIMovil.Usuarios.Models;


namespace Japdeva.APIMovil.Usuarios.Services.CrearUsuarioService
{
    /// <summary>
    /// Servicio para la creación de usuarios en el sistema.
    /// </summary>
    public class CrearUsuarioService : ICrearUsuarioService
    {
        private readonly ILogger<CrearUsuarioService> _logger;
        private readonly IAgregarRepository _agregarRepository;
        private readonly IConsultarRepository _consultarRepository;
        private const string MENSAJE_USUARIO_EXISTE = "El usuario con este correo ya existe.";
        private const string NOMBRE_ACCION_OBTENER = "ObtenerUsuarioPorId";
        private const string NOMBRE_CONTROLADOR = "Usuario";
        private const bool EXITO = true;
        private const bool ERROR = false;


        /// <summary>
        /// Inicializa una nueva instancia de la clase CrearUsuarioService.
        /// </summary>
        /// <param name="logger">Logger para registro de eventos.</param>
        /// <param name="agregarRepository">Repositorio para agregar entidades.</param>
        /// <param name="consultarRepository">Repositorio para consultar entidades.</param>
        public CrearUsuarioService(ILogger<CrearUsuarioService> logger, IAgregarRepository agregarRepository, IConsultarRepository consultarRepository)
        {
            this._logger = logger;
            this._agregarRepository = agregarRepository ?? throw new ArgumentNullException(nameof(agregarRepository));
            this._consultarRepository = consultarRepository ?? throw new ArgumentNullException(nameof(consultarRepository));
        }

        /// <summary>
        /// Crea un nuevo usuario.
        /// </summary>
        /// <param name="traceId">Identificador de trazabilidad</param>
        /// <param name="solicitud">Datos del usuario a crear</param>
        /// <returns>Resultado de la operación de creación</returns>
        public async Task<IActionResult> CrearUsuarioAsync(string traceId, CrearUsuarioSolicitudModel solicitud)
        {
            string nombreMetodo = this.ObtenerNombreMetodo();
            try
            {
                this._logger.Inicio(traceId, nombreMetodo);

                if (solicitud is null) throw new ArgumentNullException(nameof(solicitud));

                Expression<Func<UsuarioEntity, bool>> filtro = u => u.Correo == solicitud.Correo;
                var usuarioExistente = await this._consultarRepository.ConsultarAsync<UsuarioEntity>(traceId, filtro);
                if (usuarioExistente is not null)
                    return new BadRequestObjectResult(new RespuestaModel 
                    { 
                        Mensaje = MENSAJE_USUARIO_EXISTE, 
                        Exito = ERROR 
                    });

                var nuevoUsuario = new UsuarioEntity();
                nuevoUsuario.Nombre = solicitud.Nombre;
                nuevoUsuario.Correo = solicitud.Correo;
                nuevoUsuario.Telefono = solicitud.Telefono;
                nuevoUsuario.FechaCreacion = DateTime.UtcNow;
                nuevoUsuario.Activo = EXITO;

                await this._agregarRepository.AgregarAsync<UsuarioEntity>(traceId, nuevoUsuario);

                var respuesta = new UsuarioRespuestaModel();
                respuesta.Id = nuevoUsuario.Id;
                respuesta.Nombre = nuevoUsuario.Nombre;
                respuesta.Correo = nuevoUsuario.Correo;
                respuesta.Telefono = nuevoUsuario.Telefono;
                respuesta.FechaCreacion = nuevoUsuario.FechaCreacion;
                respuesta.FechaActualizacion = nuevoUsuario.FechaActualizacion;
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
