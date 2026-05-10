using System.Linq.Expressions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;
using Japdeva.APIMovil.Common.Extensions;
using Japdeva.APIMovil.Common.Models;
using Japdeva.APIMovil.Common.Repositories.ConsultarListaRepository;
using Japdeva.APIMovil.Common.Repositories.ConsultarRepository;
using Japdeva.APIMovil.Usuarios.Entities;
using Japdeva.APIMovil.Usuarios.Models;

namespace Japdeva.APIMovil.Usuarios.Services.ObtenerUsuariosService
{
    /// <summary>
    /// Servicio para la obtención de usuarios en el sistema.
    /// </summary>
    public class ObtenerUsuariosService : IObtenerUsuariosService
    {
        private readonly ILogger<ObtenerUsuariosService> _logger;
        private readonly IServiceProvider _serviceProvider;
        private const string MENSAJE_USUARIO_NO_ENCONTRADO = "Usuario no encontrado.";
        private const int PAGINA_PREDETERMINADA = 1;

        /// <summary>
        /// Inicializa una nueva instancia de la clase ObtenerUsuariosService.
        /// </summary>
        /// <param name="logger">Logger para registro de eventos.</param>
        /// <param name="serviceProvider">Proveedor de servicios para resolver dependencias.</param>
        public ObtenerUsuariosService(ILogger<ObtenerUsuariosService> logger, IServiceProvider serviceProvider)
        {
            this._logger = logger;
            this._serviceProvider = serviceProvider ?? throw new ArgumentNullException(nameof(serviceProvider));
        }

        /// <summary>
        /// Obtiene todos los usuarios activos.
        /// </summary>
        /// <param name="traceId">Identificador de trazabilidad.</param>
        /// <returns>Resultado con la lista de usuarios.</returns>
        public async Task<IActionResult> ObtenerTodosLosUsuariosAsync(string traceId)
        {
            string nombreMetodo = this.ObtenerNombreMetodo();
            try
            {
                this._logger.Inicio(traceId, nombreMetodo);
                using var scope = this._serviceProvider.CreateScope();
                var consultarListaRepository = scope.ServiceProvider.GetRequiredService<IConsultarListaRepository>();
                Expression<Func<UsuarioEntity, bool>> filtro = u => u.Activo;
                var usuariosRespuesta = await consultarListaRepository.ConsultarListaAsync<UsuarioEntity>(traceId, PAGINA_PREDETERMINADA, filtro);
                var listaUsuarios = new List<UsuarioRespuestaModel>();
                foreach (var usuario in usuariosRespuesta.Lista)
                {
                    var usuarioModelo = new UsuarioRespuestaModel();
                    usuarioModelo.Id = usuario.Id;
                    usuarioModelo.Identificacion = usuario.Identificacion;
                    usuarioModelo.IdTipoCedula = usuario.IdTipoCedula;
                    usuarioModelo.Nombre = usuario.Nombre;
                    usuarioModelo.Apellidos = usuario.Apellidos;
                    usuarioModelo.Correo = usuario.Correo;
                    usuarioModelo.Telefono = usuario.Telefono;
                    usuarioModelo.FechaNacimiento = usuario.FechaNacimiento;
                    usuarioModelo.FechaRegistro = usuario.FechaRegistro;
                    usuarioModelo.FechaEdicion = usuario.FechaEdicion;
                    usuarioModelo.Activo = usuario.Activo;
                    listaUsuarios.Add(usuarioModelo);
                }
                var resultado = new RespuestaListaModel<UsuarioRespuestaModel>();
                resultado.TotalRegistros = usuariosRespuesta.TotalRegistros;
                resultado.CantidadPaginas = usuariosRespuesta.CantidadPaginas;
                resultado.PaginaActual = usuariosRespuesta.PaginaActual;
                resultado.Lista = listaUsuarios;
                return new OkObjectResult(resultado);
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
        /// Obtiene un usuario por su identificador.
        /// </summary>
        /// <param name="traceId">Identificador de trazabilidad.</param>
        /// <param name="id">Identificador del usuario a obtener.</param>
        /// <returns>Resultado con el usuario encontrado.</returns>
        public async Task<IActionResult> ObtenerUsuarioPorIdAsync(string traceId, int id)
        {
            string nombreMetodo = this.ObtenerNombreMetodo();
            try
            {
                this._logger.Inicio(traceId, nombreMetodo);
                using var scope = this._serviceProvider.CreateScope();
                var consultarRepository = scope.ServiceProvider.GetRequiredService<IConsultarRepository>();
                var usuario = await consultarRepository.ConsultarAsync<UsuarioEntity>(traceId, usuario=> usuario.Id == id);
                if (usuario is null) throw new KeyNotFoundException(MENSAJE_USUARIO_NO_ENCONTRADO);
                var respuesta = new UsuarioRespuestaModel();
                respuesta.Id = usuario.Id;
                respuesta.Identificacion = usuario.Identificacion;
                respuesta.IdTipoCedula = usuario.IdTipoCedula;
                respuesta.Nombre = usuario.Nombre;
                respuesta.Apellidos = usuario.Apellidos;
                respuesta.Correo = usuario.Correo;
                respuesta.Telefono = usuario.Telefono;
                respuesta.FechaNacimiento = usuario.FechaNacimiento;
                respuesta.FechaRegistro = usuario.FechaRegistro;
                respuesta.FechaEdicion = usuario.FechaEdicion;
                respuesta.Activo = usuario.Activo;
                return new OkObjectResult(respuesta);
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
        /// Obtiene un usuario activo por su número de identificación (cédula).
        /// </summary>
        /// <param name="traceId">Identificador de trazabilidad.</param>
        /// <param name="identificacion">Número de identificación del usuario a buscar.</param>
        /// <returns>Resultado con el usuario encontrado.</returns>
        public async Task<IActionResult> ObtenerUsuarioPorIdentificacionAsync(string traceId, string identificacion)
        {
            string nombreMetodo = this.ObtenerNombreMetodo();
            try
            {
                this._logger.Inicio(traceId, nombreMetodo);
                using var scope = this._serviceProvider.CreateScope();
                var consultarRepository = scope.ServiceProvider.GetRequiredService<IConsultarRepository>();
                var usuario = await consultarRepository.ConsultarAsync<UsuarioEntity>(traceId, u => u.Identificacion == identificacion && u.Activo);
                if (usuario is null) throw new KeyNotFoundException(MENSAJE_USUARIO_NO_ENCONTRADO);
                var respuesta = new UsuarioRespuestaModel();
                respuesta.Id = usuario.Id;
                respuesta.Identificacion = usuario.Identificacion;
                respuesta.IdTipoCedula = usuario.IdTipoCedula;
                respuesta.Nombre = usuario.Nombre;
                respuesta.Apellidos = usuario.Apellidos;
                respuesta.Correo = usuario.Correo;
                respuesta.Telefono = usuario.Telefono;
                respuesta.FechaNacimiento = usuario.FechaNacimiento;
                respuesta.FechaRegistro = usuario.FechaRegistro;
                respuesta.FechaEdicion = usuario.FechaEdicion;
                respuesta.Activo = usuario.Activo;
                return new OkObjectResult(respuesta);
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
