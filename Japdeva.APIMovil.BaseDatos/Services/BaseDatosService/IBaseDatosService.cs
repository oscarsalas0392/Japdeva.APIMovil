namespace Japdeva.APIMovil.BaseDatos.Services.BaseDatosService
{
    /// <summary>
    /// Contrato para operaciones de conexión y ejecución contra PostgreSQL.
    /// </summary>
    public interface IBaseDatosService
    {
        /// <summary>
        /// Ejecuta una lista de sentencias SQL individualmente contra la base de datos indicada,
        /// reutilizando la misma conexión e invocando el callback con el resultado de cada una.
        /// </summary>
        /// <param name="servidor">Host del servidor PostgreSQL.</param>
        /// <param name="puerto">Puerto del servidor PostgreSQL.</param>
        /// <param name="usuario">Usuario de conexión.</param>
        /// <param name="contrasena">Contraseña de conexión.</param>
        /// <param name="baseDatos">Nombre de la base de datos destino.</param>
        /// <param name="sentencias">Sentencias SQL a ejecutar en orden.</param>
        /// <param name="onResultado">Callback invocado por cada sentencia: (sentencia, exitoso, mensajeError).</param>
        Task EjecutarSentenciasAsync(string servidor, int puerto, string usuario, string contrasena, string baseDatos, IEnumerable<string> sentencias, Func<string, bool, string?, Task> onResultado);

        /// <summary>
        /// Termina todas las conexiones activas y elimina la base de datos si existe.
        /// </summary>
        /// <param name="servidor">Host del servidor PostgreSQL.</param>
        /// <param name="puerto">Puerto del servidor PostgreSQL.</param>
        /// <param name="usuario">Usuario de conexión.</param>
        /// <param name="contrasena">Contraseña de conexión.</param>
        /// <param name="nombreBaseDatos">Nombre de la base de datos a eliminar.</param>
        Task EliminarBaseDatosAsync(string servidor, int puerto, string usuario, string contrasena, string nombreBaseDatos);
    }
}
