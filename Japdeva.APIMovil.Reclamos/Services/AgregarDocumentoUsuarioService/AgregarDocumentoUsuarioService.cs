using System;
using Japdeva.APIMovil.Common.Extensions;
using Japdeva.APIMovil.Common.Repositories.AgregarRepository;
using Japdeva.APIMovil.Common.Repositories.ConsultarRepository;
using Japdeva.APIMovil.Reclamos.Entities;
using Japdeva.APIMovil.Reclamos.Models;

namespace Japdeva.APIMovil.Reclamos.Services.AgregarDocumentoUsuarioService
{
    /// <summary>
    /// Servicio para agregar documentos de usuario asociados a reclamos.
    /// Proporciona funcionalidades para validar y almacenar documentos proporcionados por usuarios.
    /// </summary>
    public class AgregarDocumentoUsuarioService : IAgregarDocumentoUsuarioService
    {
        private readonly ILogger<AgregarDocumentoUsuarioService> _logger;
        private readonly IServiceProvider _serviceProvider;

        private const string MENSAJE_ERROR_LISTA_NULA = "La lista de archivos no puede ser nula o vacía.";
        private const string MENSAJE_ERROR_RECLAMO_NO_ENCONTRADO = "El reclamo con Id {0} no fue encontrado.";
        private const int MINIMO_REGISTROS = 1;

        /// <summary>
        /// Inicializa una nueva instancia de la clase <see cref="AgregarDocumentoUsuarioService"/>.
        /// </summary>
        /// <param name="logger">Instancia del registrador de logs.</param>
        /// <param name="serviceProvider">Proveedor de servicios para la obtención de dependencias.</param>
        public AgregarDocumentoUsuarioService(ILogger<AgregarDocumentoUsuarioService> logger, IServiceProvider serviceProvider)
        {
            this._logger = logger;
            this._serviceProvider = serviceProvider;
        }

        /// <summary>
        /// Agrega documentos de usuario asociados a un reclamo específico.
        /// Valida la existencia del reclamo y procesa la lista de archivos para crear entidades de documentos.
        /// </summary>
        /// <param name="traceId">Identificador único para rastreo de la operación.</param>
        /// <param name="idReclamo">Identificador del reclamo al cual se asociarán los documentos.</param>
        /// <param name="listaArchivos">Lista de archivos/documentos a procesar y almacenar.</param>
        public async Task AgregarDocumentoUsuarioAsync(string traceId, long idReclamo, List<ArchivoSolicitudModel> listaArchivos)
        {
            string nombreMetodo = this.ObtenerNombreMetodo();
            try
            {
                this._logger.Inicio(traceId, nombreMetodo);
                List<DocumentoUsuarioEntity> documentosUsuario = new List<DocumentoUsuarioEntity>();

                using var scope = this._serviceProvider.CreateScope();
                var consultarRepository = scope.ServiceProvider.GetRequiredService<IConsultarRepository>();
                var agregarRepository = scope.ServiceProvider.GetRequiredService<IAgregarRepository>();

                var reclamo = await consultarRepository.ConsultarAsync<ReclamoEntity>(traceId, d => d.Id == idReclamo);
                
                if(reclamo is null)
                {
                    throw new ArgumentException(string.Format(MENSAJE_ERROR_RECLAMO_NO_ENCONTRADO, idReclamo));
                }
                
                if (listaArchivos is null || listaArchivos.Count < MINIMO_REGISTROS)
                {
                    throw new ArgumentNullException(MENSAJE_ERROR_LISTA_NULA);
                }

                foreach (var archivo in listaArchivos)
                {
                    if (string.IsNullOrEmpty(archivo.NombreArchivo) || string.IsNullOrEmpty(archivo.ContenidoArchivo)) continue;
                    DocumentoUsuarioEntity documentoUsuario = new DocumentoUsuarioEntity();
                    documentoUsuario.IdReclamo = idReclamo;
                    documentoUsuario.Documento = archivo.ContenidoArchivo;
                    documentoUsuario.NombreDocumento = archivo.NombreArchivo;
                    documentoUsuario.FechaRegistro = DateTime.Now;
                    documentosUsuario.Add(documentoUsuario);
                }

                if(documentosUsuario.Count < MINIMO_REGISTROS) throw new ArgumentException(MENSAJE_ERROR_LISTA_NULA);
                await agregarRepository.AgregarVariosAsync<DocumentoUsuarioEntity>(traceId, documentosUsuario);
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
