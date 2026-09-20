using Microsoft.Extensions.DependencyInjection;
using Japdeva.APIMovil.Common.Extensions;
using Japdeva.APIMovil.Common.Repositories.AgregarRepository;
using Japdeva.APIMovil.Common.Repositories.ConsultarRepository;
using Japdeva.APIMovil.Usuarios.Entities;
using Japdeva.APIMovil.Usuarios.Services.RolCacheService;

namespace Japdeva.APIMovil.Usuarios.Services.AgregarUsuarioRolService
{
    /// <summary>
    /// Servicio para la asignación de roles a usuarios.
    /// </summary>
    public class AgregarUsuarioRolService : IAgregarUsuarioRolService
    {
        private readonly ILogger<AgregarUsuarioRolService> _logger;
        private readonly IServiceProvider _serviceProvider;
        private readonly IRolCacheService _rolCacheService;
        private const string MENSAJE_ASIGNACION_EXISTE = "El usuario ya tiene asignado un rol.";
        private const string MENSAJE_ROL_NO_EXISTE = "El id del rol no existe.";
        private const bool EXITO = true;

        /// <summary>
        /// Inicializa una nueva instancia de AgregarUsuarioRolService.
        /// </summary>
        /// <param name="logger">Logger para registro de eventos.</param>
        /// <param name="rolCacheService">Servicio de caché de roles.</param>
        /// <param name="serviceProvider">Proveedor de servicios para resolver dependencias.</param>
        public AgregarUsuarioRolService(ILogger<AgregarUsuarioRolService> logger, IRolCacheService rolCacheService, IServiceProvider serviceProvider)
        {
            this._logger = logger;
            this._rolCacheService = rolCacheService;
            this._serviceProvider = serviceProvider ?? throw new ArgumentNullException(nameof(serviceProvider));
        }

        /// <summary>
        /// Asigna un rol a un usuario.
        /// </summary>
        /// <param name="traceId">Identificador de trazabilidad.</param>
        /// <param name="idUsuario">Identificador del usuario.</param>
        /// <param name="idRol">Identificador del rol a asignar.</param>
        public async Task AgregarUsuarioRolAsync(string traceId, int idUsuario, int idRol)
        {
            string nombreMetodo = this.ObtenerNombreMetodo();
            try
            {
                this._logger.Inicio(traceId, nombreMetodo);
                using var scope = this._serviceProvider.CreateScope();
                var consultarRepository = scope.ServiceProvider.GetRequiredService<IConsultarRepository>();
                var agregarRepository = scope.ServiceProvider.GetRequiredService<IAgregarRepository>();
                var asignacionExistente = await consultarRepository.ConsultarAsync<UsuarioRolEntity>(traceId, usuario => usuario.IdUsuario == idUsuario && usuario.Activo);
                if (asignacionExistente is not null) throw new ArgumentException(MENSAJE_ASIGNACION_EXISTE);

                var rol = this._rolCacheService.ObtenerPorId(traceId, idRol);
                if (rol is null) throw new ArgumentException(MENSAJE_ROL_NO_EXISTE);

                var nuevaAsignacion = new UsuarioRolEntity();
                nuevaAsignacion.IdUsuario = idUsuario;
                nuevaAsignacion.IdRol = idRol;
                nuevaAsignacion.FechaRegistro = DateTime.UtcNow;
                nuevaAsignacion.Activo = EXITO;
                await agregarRepository.AgregarAsync<UsuarioRolEntity>(traceId, nuevaAsignacion);
           
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
