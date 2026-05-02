using System.Text.RegularExpressions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;
using Japdeva.APIMovil.Common.Extensions;
using Japdeva.APIMovil.Common.Repositories.AgregarRepository;
using Japdeva.APIMovil.Common.Repositories.ConsultarRepository;
using Japdeva.APIMovil.Usuarios.Entities;
using Japdeva.APIMovil.Usuarios.Models;
using Japdeva.APIMovil.Usuarios.Services.AgregarUsuarioRolService;

namespace Japdeva.APIMovil.Usuarios.Services.AgregarUsuarioService
{
    /// <summary>
    /// Servicio para el registro de nuevos usuarios en el sistema.
    /// </summary>
    public class AgregarUsuarioService : IAgregarUsuarioService
    {
        private readonly ILogger<AgregarUsuarioService> _logger;
        private readonly IServiceProvider _serviceProvider;
        private readonly IAgregarUsuarioRolService _agregarUsuarioRolService;
        private const string MENSAJE_USUARIO_EXISTE = "Ya existe un usuario registrado con esa identificación o correo.";
        private const string MENSAJE_CORREO_REQUERIDO = "El correo es requerido.";
        private const string MENSAJE_CONTRASENA_REQUERIDA = "La contraseña es requerida.";
        private const string MENSAJE_NOMBRE_REQUERIDO = "El nombre es requerido.";
        private const string MENSAJE_IDENTIFICACION_REQUERIDA = "La identificación es requerida.";
        private const string MENSAJE_APELLIDOS_REQUERIDOS = "Los apellidos son requeridos.";
        private const string MENSAJE_CORREO_INVALIDO = "El formato del correo no es válido.";
        private const string MENSAJE_CEDULA_INVALIDO = "El tipo de cedula es invalido.";
        private const string MENSAJE_CEDULA_FORMATO_INVALIDO = "El formato de la cedula no es válido.";
        private const string PATRON_CORREO = @"^[^@\s]+@[^@\s]+\.[^@\s]+$";
        private const string TRACE_VACIO = "";
        private const bool ACTIVO = true;
        private const int ROL_USUARIO_EXTERNO = 4;

        /// <summary>
        /// Inicializa una nueva instancia de AgregarUsuarioService.
        /// </summary>
        /// <param name="logger">Logger para registro de eventos.</param>
        /// <param name="serviceProvider">Proveedor de servicios para resolver dependencias.</param>
        /// <param name="agregarUsuarioRolService">Servicio para asignar roles al usuario registrado.</param>
        public AgregarUsuarioService(ILogger<AgregarUsuarioService> logger, IServiceProvider serviceProvider, IAgregarUsuarioRolService agregarUsuarioRolService)
        {
            this._logger = logger;
            this._serviceProvider = serviceProvider ?? throw new ArgumentNullException(nameof(serviceProvider));
            this._agregarUsuarioRolService = agregarUsuarioRolService;
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
                using var scope = this._serviceProvider.CreateScope();
                var consultarRepository = scope.ServiceProvider.GetRequiredService<IConsultarRepository>();
                var agregarRepository = scope.ServiceProvider.GetRequiredService<IAgregarRepository>();

                this._logger.Inicio(traceId, nombreMetodo);
                if (solicitud is null) throw new ArgumentNullException(nameof(solicitud));
                this.ValidarCamposRequeridos(solicitud);

                var tipoCedula = await consultarRepository.ConsultarAsync<TipoCedulaEntity>(traceId, tipoCedula => tipoCedula.Id == solicitud.IdTipoCedula);
                if (tipoCedula is null) throw new ArgumentException(MENSAJE_CEDULA_INVALIDO);
                if (!Regex.IsMatch(solicitud.Identificacion, tipoCedula.Formato)) throw new ArgumentException(MENSAJE_CEDULA_FORMATO_INVALIDO);

                var usuarioExistente = await consultarRepository.ConsultarAsync<UsuarioEntity>(traceId, usuario => usuario.Identificacion == solicitud.Identificacion || usuario.Correo == solicitud.Correo);
                if (usuarioExistente is not null) throw new ArgumentException(MENSAJE_USUARIO_EXISTE);
                
                var nuevoUsuario = new UsuarioEntity();
                nuevoUsuario.Identificacion = solicitud.Identificacion;
                nuevoUsuario.IdTipoCedula = solicitud.IdTipoCedula;
                nuevoUsuario.Nombre = solicitud.Nombre;
                nuevoUsuario.Apellidos = solicitud.Apellidos;
                nuevoUsuario.Correo = solicitud.Correo;
                nuevoUsuario.Contrasena = solicitud.Contrasena;
                nuevoUsuario.Telefono = solicitud.Telefono;
                nuevoUsuario.FechaNacimiento = solicitud.FechaNacimiento;
                nuevoUsuario.FechaRegistro = DateTime.UtcNow;
                nuevoUsuario.Activo = ACTIVO;

                await agregarRepository.AgregarAsync<UsuarioEntity>(traceId, nuevoUsuario);
                var respuesta = new UsuarioRespuestaModel();
                respuesta.Id = nuevoUsuario.Id;
                respuesta.Identificacion = nuevoUsuario.Identificacion;
                respuesta.IdTipoCedula = nuevoUsuario.IdTipoCedula;
                respuesta.Nombre = nuevoUsuario.Nombre;
                respuesta.Apellidos = nuevoUsuario.Apellidos;
                respuesta.Correo = nuevoUsuario.Correo;
                respuesta.Telefono = nuevoUsuario.Telefono;
                respuesta.FechaNacimiento = nuevoUsuario.FechaNacimiento;
                respuesta.FechaRegistro = nuevoUsuario.FechaRegistro;
                respuesta.Activo = nuevoUsuario.Activo;

                await this._agregarUsuarioRolService.AgregarUsuarioRolAsync(traceId, nuevoUsuario.Id, ROL_USUARIO_EXTERNO);

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
        /// Valida que los campos requeridos del modelo de solicitud estén completos y con formato correcto.
        /// </summary>
        /// <param name="solicitud">Datos del usuario a validar.</param>
        public void ValidarCamposRequeridos(AgregarUsuarioSolicitudModel solicitud)
        {
            string nombreMetodo = this.ObtenerNombreMetodo();
            try
            {
                this._logger.Inicio(TRACE_VACIO, nombreMetodo);
                if (string.IsNullOrEmpty(solicitud.Correo)) throw new ArgumentException(MENSAJE_CORREO_REQUERIDO);
                if (!Regex.IsMatch(solicitud.Correo, PATRON_CORREO)) throw new ArgumentException(MENSAJE_CORREO_INVALIDO);
                if (string.IsNullOrEmpty(solicitud.Contrasena)) throw new ArgumentException(MENSAJE_CONTRASENA_REQUERIDA);
                if (string.IsNullOrEmpty(solicitud.Nombre)) throw new ArgumentException(MENSAJE_NOMBRE_REQUERIDO);
                if (string.IsNullOrEmpty(solicitud.Identificacion)) throw new ArgumentException(MENSAJE_IDENTIFICACION_REQUERIDA);
                if (string.IsNullOrEmpty(solicitud.Apellidos)) throw new ArgumentException(MENSAJE_APELLIDOS_REQUERIDOS);
            }
            catch (Exception ex)
            {
                this._logger.Error(TRACE_VACIO, nombreMetodo, ex);
                throw;
            }
            finally
            {
                this._logger.Fin(TRACE_VACIO, nombreMetodo);
            }
        }
    }
}
