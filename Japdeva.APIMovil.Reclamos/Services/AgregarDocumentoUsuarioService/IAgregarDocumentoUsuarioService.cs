namespace Japdeva.APIMovil.Reclamos.Services.AgregarDocumentoUsuarioService
{
    /// <summary>
    /// Interfaz que define el contrato para el servicio de agregación de documentos de usuario.
    /// Proporciona operaciones para agregar documentos asociados a reclamos específicos.
    /// </summary>
    public interface IAgregarDocumentoUsuarioService
    {
        /// <summary>
        /// Agrega documentos de usuario asociados a un reclamo específico de forma asíncrona.
        /// </summary>
        /// <param name="traceId">Identificador único para rastreo de la operación.</param>
        /// <param name="idReclamo">Identificador del reclamo al cual se asociarán los documentos.</param>
        /// <param name="listaArchivos">Lista de archivos/documentos a procesar y almacenar.</param>
        Task AgregarDocumentoUsuarioAsync(string traceId, long idReclamo, List<string> listaArchivos);
    }
}
