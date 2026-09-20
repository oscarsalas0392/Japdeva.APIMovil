using Microsoft.AspNetCore.Mvc;
using Japdeva.APIMovil.Common.Extensions;
using Japdeva.APIMovil.Common.Repositories.AgregarRepository;
using Japdeva.APIMovil.Reclamos.Entities;
using Japdeva.APIMovil.Reclamos.Models;
using Japdeva.APIMovil.Reclamos.Services.AgregarApelacionReclamoDetalleService;
using Japdeva.APIMovil.Reclamos.Services.AgregarDocumentoUsuarioService;
using Japdeva.APIMovil.Reclamos.Services.EstadoReclamoCacheService;
using Japdeva.APIMovil.Reclamos.Services.NivelProcesoCacheService;
using Japdeva.APIMovil.Reclamos.Services.NotificarDepartamentoService;
using Japdeva.APIMovil.Reclamos.Services.NotificarUsuarioReclamoService;

namespace Japdeva.APIMovil.Reclamos.Services.AgregarApelacionReclamoService
{
    /// <summary>
    /// Servicio para la gestión de agregación de apelaciones de reclamo en el sistema.
    /// Crea la apelación principal, sus documentos y su detalle inicial de forma transaccional.
    /// </summary>
    public class AgregarApelacionReclamoService : IAgregarApelacionReclamoService
    {
        private readonly ILogger<AgregarApelacionReclamoService> _logger;
        private readonly IAgregarDocumentoUsuarioService _agregarDocumentoUsuarioService;
        private readonly IServiceProvider _serviceProvider;
        private readonly IAgregarApelacionReclamoDetalleService _agregarApelacionReclamoDetalleService;
        private readonly IEstadoReclamoCacheService _estadoReclamoCacheService;
        private readonly INivelProcesoCacheService _nivelProcesoCacheService;
        private readonly INotificarUsuarioReclamoService _notificarUsuarioReclamoService;
        private readonly INotificarDepartamentoService _notificarDepartamentoService;

        private const int MINIMO_REGISTROS = 1;
        private const string MENSAJE_TITULO_REQUERIDO = "El titulo es requerido";
        private const string MENSAJE_DESCRIPCION_REQUERIDA = "La descripcion es requerida";
        private const string MENSAJE_ERROR_LISTA_NULA = "La lista de archivos no puede ser nula o vacía.";
        private const string MENSAJE_ERROR_ESTADO_RECLAMO_NO_EXISTE = "El estado reclamo no existe.";
        private const string MENSAJE_ERROR_NIVEL_PROCESO_NO_EXISTE = "El nivel de proceso inicial para Apelación no existe.";

        /// <summary>
        /// Inicializa una nueva instancia de la clase <see cref="AgregarApelacionReclamoService"/>.
        /// </summary>
        public AgregarApelacionReclamoService(
            ILogger<AgregarApelacionReclamoService> logger,
            IAgregarDocumentoUsuarioService agregarDocumentoUsuarioService,
            IServiceProvider serviceProvider,
            IAgregarApelacionReclamoDetalleService agregarApelacionReclamoDetalleService,
            IEstadoReclamoCacheService estadoReclamoCacheService,
            INivelProcesoCacheService nivelProcesoCacheService,
            INotificarUsuarioReclamoService notificarUsuarioReclamoService,
            INotificarDepartamentoService notificarDepartamentoService)
        {
            this._logger = logger;
            this._agregarDocumentoUsuarioService = agregarDocumentoUsuarioService;
            this._serviceProvider = serviceProvider;
            this._agregarApelacionReclamoDetalleService = agregarApelacionReclamoDetalleService;
            this._estadoReclamoCacheService = estadoReclamoCacheService;
            this._nivelProcesoCacheService = nivelProcesoCacheService;
            this._notificarUsuarioReclamoService = notificarUsuarioReclamoService;
            this._notificarDepartamentoService = notificarDepartamentoService;
        }

        /// <summary>
        /// Agrega una nueva apelación al sistema de forma transaccional.
        /// Crea la apelación principal y ejecuta las operaciones dependientes (documentos y detalle) en paralelo.
        /// </summary>
        /// <param name="traceId">Identificador único para rastreo de la operación.</param>
        /// <param name="apelacion">Modelo con los datos de la apelación a crear.</param>
        /// <returns>Un IActionResult con los datos de la apelación creada exitosamente.</returns>
        public async Task<IActionResult> AgregarApelacionReclamoAsync(string traceId, AgregarApelacionReclamoSolicitudModel apelacion)
        {
            string nombreMetodo = this.ObtenerNombreMetodo();
            try
            {
                this._logger.Inicio(traceId, nombreMetodo);
                using var scope = this._serviceProvider.CreateScope();
                var agregarRepository = scope.ServiceProvider.GetRequiredService<IAgregarRepository>();

                if (string.IsNullOrEmpty(apelacion.Titulo)) throw new ArgumentException(MENSAJE_TITULO_REQUERIDO);
                if (string.IsNullOrEmpty(apelacion.Descripcion)) throw new ArgumentException(MENSAJE_DESCRIPCION_REQUERIDA);
                if (apelacion.ListaDocumentos is null || apelacion.ListaDocumentos.Count < MINIMO_REGISTROS) throw new ArgumentException(MENSAJE_ERROR_LISTA_NULA);

                var estadoReclamo = this._estadoReclamoCacheService.ObtenerEstadoReclamo(traceId, (int)EstadoReclamoModel.Pendiente);
                if (estadoReclamo is null) throw new Exception(MENSAJE_ERROR_ESTADO_RECLAMO_NO_EXISTE);

                var nivelProceso = this._nivelProcesoCacheService.ObtenerPrimerNivelPorProceso(traceId, (int)ProcesoModel.Apelacion);
                if (nivelProceso is null) throw new Exception(MENSAJE_ERROR_NIVEL_PROCESO_NO_EXISTE);

                ApelacionReclamoEntity apelacionEntity = new ApelacionReclamoEntity();
                apelacionEntity.IdReclamo = apelacion.IdReclamo;
                apelacionEntity.Titulo = apelacion.Titulo;
                apelacionEntity.Descripcion = apelacion.Descripcion;
                apelacionEntity.IdEstadoReclamo = estadoReclamo.Id;
                apelacionEntity.FechaRegistro = DateTime.UtcNow;
                apelacionEntity.IdUsuarioExterno = apelacion.IdUsuarioExterno;
                apelacionEntity.IdDepartamentoActual = nivelProceso.IdDepartamento;
                await agregarRepository.AgregarAsync<ApelacionReclamoEntity>(traceId, apelacionEntity);

                Task agregarDocumento = this._agregarDocumentoUsuarioService.AgregarDocumentoUsuarioAsync(traceId, apelacionEntity.Id, apelacion.ListaDocumentos);
                Task agregarDetalle = this._agregarApelacionReclamoDetalleService.AgregarApelacionReclamoDetalleAsync(traceId, apelacionEntity.Id, nivelProceso.Id);
                await Task.WhenAll(agregarDocumento, agregarDetalle);

                _ = Task.Run(() => this._notificarUsuarioReclamoService.NotificarNuevoReclamoAsync(traceId, apelacionEntity.Id, apelacionEntity.IdUsuarioExterno));
                _ = Task.Run(() => this._notificarDepartamentoService.NotificarNuevoReclamoAsync(traceId, apelacionEntity.IdDepartamentoActual, apelacionEntity.Id, apelacionEntity.IdUsuarioExterno));

                AgregarApelacionReclamoRespuestaModel respuesta = new AgregarApelacionReclamoRespuestaModel();
                respuesta.Id = apelacionEntity.Id;
                respuesta.IdReclamo = apelacionEntity.IdReclamo;
                respuesta.Titulo = apelacionEntity.Titulo;
                respuesta.Descripcion = apelacionEntity.Descripcion;

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
