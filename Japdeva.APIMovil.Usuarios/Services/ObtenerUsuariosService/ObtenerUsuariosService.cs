using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Japdeva.APIMovil.Common.Extensions;
using Japdeva.APIMovil.Common.Models;
using Japdeva.APIMovil.Common.Repositories.ConsultarListaRepository;
using Japdeva.APIMovil.Common.Repositories.ConsultarRepository;
using Japdeva.APIMovil.Usuarios.Entities;
using Japdeva.APIMovil.Usuarios.Models;


namespace Japdeva.APIMovil.Usuarios.Services.ObtenerUsuariosService
{
    /// <summary>
    /// Servicio para la obtenci�n de usuarios en el sistema.
    /// </summary>
    public class ObtenerUsuariosService : IObtenerUsuariosService
    {
        private readonly ILogger<ObtenerUsuariosService> _logger;
        private readonly IConsultarRepository _consultarRepository;
        private readonly IConsultarListaRepository _consultarListaRepository;
        private const string MENSAJE_USUARIO_NO_ENCONTRADO = "Usuario no encontrado.";
        private const string MENSAJE_USUARIO_OBTENIDO = "Usuario obtenido correctamente.";
        private const int PAGINA_PREDETERMINADA = 1;
        private const bool EXITO = true;
        private const bool ERROR = false;


        /// <summary>
        /// Inicializa una nueva instancia de la clase ObtenerUsuariosService.
        /// </summary>
        /// <param name="logger">Logger para registro de eventos.</param>
        /// <param name="consultarRepository">Repositorio para consultar una entidad.</param>
        /// <param name="consultarListaRepository">Repositorio para consultar una lista de entidades.</param>
        public ObtenerUsuariosService(ILogger<ObtenerUsuariosService> logger, IConsultarRepository consultarRepository, IConsultarListaRepository consultarListaRepository)
        {
            this._logger = logger;
            this._consultarRepository = consultarRepository ?? throw new ArgumentNullException(nameof(consultarRepository));
            this._consultarListaRepository = consultarListaRepository 
                ?? throw new ArgumentNullException(nameof(consultarListaRepository));
        }

        /// <summary>
        /// Obtiene todos los usuarios activos.
        /// </summary>
        /// <param name="traceId">Identificador de trazabilidad</param>
        /// <returns>Resultado con la lista de usuarios</returns>
        public async Task<IActionResult> ObtenerTodosLosUsuariosAsync(string traceId)
        {
            string nombreMetodo = this.ObtenerNombreMetodo();
            try
            {
                this._logger.Inicio(traceId, nombreMetodo);

                Expression<Func<UsuarioEntity, bool>> filtro = u => u.Activo;
                var usuariosRespuesta = await this._consultarListaRepository.ConsultarListaAsync<UsuarioEntity>(
                    traceId, 
                    PAGINA_PREDETERMINADA, 
                    filtro);

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

        /// <summary>
        /// Obtiene un usuario por su identificador.
        /// </summary>
        /// <param name="traceId">Identificador de trazabilidad</param>
        /// <param name="id">Identificador del usuario a obtener</param>
        /// <returns>Resultado con el usuario encontrado</returns>
        public async Task<IActionResult> ObtenerUsuarioPorIdAsync(string traceId, int id)
        {
            string nombreMetodo = this.ObtenerNombreMetodo();
            try
            {
                this._logger.Inicio(traceId, nombreMetodo);

                Expression<Func<UsuarioEntity, bool>> filtro = u => u.Id == id;
                var usuario = await this._consultarRepository.ConsultarAsync<UsuarioEntity>(traceId, filtro);
                if (usuario is null)
                    return new NotFoundObjectResult(new RespuestaModel 
                    { 
                        Mensaje = MENSAJE_USUARIO_NO_ENCONTRADO, 
                        Exito = ERROR 
                    });

                var respuesta = new UsuarioRespuestaModel();
                respuesta.Id = usuario.Id;
                respuesta.Identificacion = usuario.Identificacion;
                respuesta.IdTipoCedula = usuario.IdTipoCedula;
                respuesta.Nombre = usuario.Nombre;
                respuesta.Apellidos = usuario.Apellidos;
                respuesta.Correo = usuario.Correo;
                respuesta.FechaRegistro = usuario.FechaRegistro;
                respuesta.FechaEdicion = usuario.FechaEdicion;
                respuesta.Activo = usuario.Activo;

                return new OkObjectResult(new RespuestaModel 
                { 
                    Mensaje = MENSAJE_USUARIO_OBTENIDO, 
                    Exito = EXITO, 
                    Datos = respuesta 
                });
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
