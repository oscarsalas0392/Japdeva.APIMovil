using System;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore.Storage;
using Japdeva.APIMovil.Common.Extensions;
using Japdeva.APIMovil.Common.Repositories.AgregarRepository;
using Japdeva.APIMovil.Common.Repositories.GeneralRepository;
using Japdeva.APIMovil.Reclamos.Entities;
using Japdeva.APIMovil.Reclamos.Models;
using Japdeva.APIMovil.Reclamos.Services.AgregarDocumentoUsuarioService;
using Japdeva.APIMovil.Reclamos.Services.AgregarReclamoDetalleService;
using Japdeva.APIMovil.Reclamos.Services.EstadoReclamoCacheService;
using Japdeva.APIMovil.Reclamos.Services.NivelProcesoCacheService;


namespace Japdeva.APIMovil.Reclamos.Services.AgregarReclamoService
{
    /// <summary>
    /// Servicio para la gestión de agregación de reclamos en el sistema.
    /// Proporciona funcionalidades para crear reclamos completos incluyendo documentos y detalles,
    /// manejando transacciones de base de datos para garantizar la consistencia de los datos.
    /// </summary>
    public class AgregarReclamoService : IAgregarReclamoService
    {
        private readonly ILogger<AgregarReclamoService> _logger;
        private readonly IAgregarDocumentoUsuarioService _agregarDocumentoUsuarioService;
        private readonly IServiceProvider _serviceProvider;
        private readonly IAgregarReclamoDetalleService _agregarReclamoDetalleService;
        private readonly IEstadoReclamoCacheService _estadoReclamoCacheService;
        private readonly INivelProcesoCacheService _nivelProcesoCacheService;

        private const int MINIMO_REGISTROS = 1;
        private const string MENSAJE_TITULO_REQUERIDO = "El titulo es requerido";
        private const string MENSAJE_DESCRIPCION_REQUERIDA = "El descripcion es requerida";
        private const string MENSAJE_ERROR_LISTA_NULA = "La lista de archivos no puede ser nula o vacía.";
        private const string MENSAJE_ERROR_ESTADO_RECLAMO_NO_EXISTE = "El estado reclamo no existe.";
        private const string MENSAJE_ERROR_ORDEN_PROCESO_NO_EXISTE = "La orden de proceso inicial no existe.";


        /// <summary>
        /// Inicializa una nueva instancia de la clase <see cref="AgregarReclamoService"/>.
        /// </summary>
        /// <param name="logger">El registrador de eventos para la clase.</param>
        /// <param name="agregarDocumentoUsuarioService">Servicio para agregar documentos de usuario.</param>
        /// <param name="serviceProvider">Proveedor de servicios para la obtención de dependencias.</param>
        /// <param name="agregarReclamoDetalleService">Servicio para agregar detalles del reclamo.</param>
        /// <param name="estadoReclamoCacheService">Servicio de caché para estados de reclamo.</param>
        /// <param name="nivelProcesoCacheService">Servicio de caché para niveles de proceso.</param>
        public AgregarReclamoService(
            ILogger<AgregarReclamoService> logger,
            IAgregarDocumentoUsuarioService agregarDocumentoUsuarioService,
            IServiceProvider serviceProvider,
            IAgregarReclamoDetalleService agregarReclamoDetalleService,
            IEstadoReclamoCacheService estadoReclamoCacheService,
            INivelProcesoCacheService nivelProcesoCacheService)
        {
            this._logger = logger;
            this._agregarDocumentoUsuarioService = agregarDocumentoUsuarioService;
            this._serviceProvider = serviceProvider;
            this._agregarReclamoDetalleService = agregarReclamoDetalleService;
            this._estadoReclamoCacheService = estadoReclamoCacheService;
            this._nivelProcesoCacheService = nivelProcesoCacheService;
        }

        /// <summary>
        /// Agrega un nuevo reclamo al sistema de forma transaccional.
        /// Crea el reclamo principal y ejecuta las operaciones dependientes (documentos y detalles) en paralelo.
        /// En caso de error, realiza rollback automático para mantener la consistencia de los datos.
        /// </summary>
        /// <param name="traceId">Identificador único para rastreo de la operación.</param>
        /// <param name="reclamo">Modelo con los datos del reclamo a crear.</param>
        /// <returns>Un IActionResult con los datos del reclamo creado exitosamente.</returns>
        public async Task<IActionResult> AgregarReclamoAsync(string traceId, AgregarReclamoSolicitudModel reclamo)
        {
            string nombreMetodo = this.ObtenerNombreMetodo();
            IDbContextTransaction? transaccion = null;
            using var scope = this._serviceProvider.CreateScope();
            var generalRepository = scope.ServiceProvider.GetRequiredService<IGeneralRepository>();
            try
            {
                this._logger.Inicio(traceId, nombreMetodo);
                var agregarRepository = scope.ServiceProvider.GetRequiredService<IAgregarRepository>();
              
                transaccion = await generalRepository.ObtenerTransaccionBaseDatosAsync(traceId);
              
                if (string.IsNullOrEmpty(reclamo.Titulo)) throw new ArgumentException(MENSAJE_TITULO_REQUERIDO);
                if (string.IsNullOrEmpty(reclamo.Descripcion)) throw new ArgumentException(MENSAJE_DESCRIPCION_REQUERIDA);

                if (reclamo.ListaDocumentos is null || reclamo.ListaDocumentos.Count < MINIMO_REGISTROS) throw new ArgumentException(MENSAJE_ERROR_LISTA_NULA);

                var estadoReclamo = this._estadoReclamoCacheService.ObtenerEstadoReclamo(traceId, (int)EstadoReclamoModel.Pendiente);
                if (estadoReclamo is null) throw new Exception(MENSAJE_ERROR_ESTADO_RECLAMO_NO_EXISTE);

                var nivelProceso = this._nivelProcesoCacheService.ObtenerPrimerNivel(traceId);
                if (nivelProceso is null) throw new Exception(MENSAJE_ERROR_ORDEN_PROCESO_NO_EXISTE);

                ReclamoEntity reclamoEntity = new ReclamoEntity();
                reclamoEntity.Titulo = reclamo.Titulo;
                reclamoEntity.Descripcion = reclamo.Descripcion;
                reclamoEntity.IdEstadoReclamo = estadoReclamo.Id;
                reclamoEntity.FechaRegistro = DateTime.Now;
                reclamoEntity.IdUsuarioExterno = reclamo.IdUsuarioExterno;
                reclamoEntity.IdDepartamentoActual = nivelProceso.IdDepartamento;

                await agregarRepository.AgregarAsync<ReclamoEntity>(traceId, reclamoEntity);

                Task agregarDocumento = this._agregarDocumentoUsuarioService.AgregarDocumentoUsuarioAsync(traceId, reclamoEntity.Id, reclamo.ListaDocumentos);
                Task agregarDetalleReclamo = this._agregarReclamoDetalleService.AgregarReclamoDetalleAsync(traceId, reclamoEntity.Id, nivelProceso.Id);
                await Task.WhenAll(agregarDocumento, agregarDetalleReclamo);

                await generalRepository.RealizarCommitBaseDatosAsync(traceId, transaccion);

                AgregarReclamoRespuestaModel agregarReclamoRespuestaModel = new AgregarReclamoRespuestaModel();
                agregarReclamoRespuestaModel.Id = reclamoEntity.Id;
                agregarReclamoRespuestaModel.Titulo = reclamoEntity.Titulo;
                agregarReclamoRespuestaModel.Descripcion = reclamoEntity.Descripcion;

                return new OkObjectResult(agregarReclamoRespuestaModel);
            }
            catch (Exception ex)
            {
                await generalRepository.RealizarDevolucionCambiosBaseDatosAsync(traceId, transaccion);
                this._logger.Error(traceId, nombreMetodo, ex);
                throw;
            }
            finally
            {
                await generalRepository.LimpiarTransaccionAsync(traceId, transaccion);
                this._logger.Fin(traceId, nombreMetodo);
            }
        }

    }
}
