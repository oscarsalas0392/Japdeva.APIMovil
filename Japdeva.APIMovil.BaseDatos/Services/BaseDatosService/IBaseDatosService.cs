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
        /// <summary>
        /// Verifica que el servidor sea alcanzable y las credenciales sean válidas abriendo una conexión a la base de datos 'postgres'.
        /// Lanza excepción si la conexión falla.
        /// </summary>
        /// <param name="servidor">Host del servidor PostgreSQL.</param>
        /// <param name="puerto">Puerto del servidor PostgreSQL.</param>
        /// <param name="usuario">Usuario de conexión.</param>
        /// <param name="contrasena">Contraseña de conexión.</param>
        Task ProbarConexionAsync(string servidor, int puerto, string usuario, string contrasena);

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

        /// <summary>
        /// Retorna los nombres de las columnas actuales de una tabla en la base de datos.
        /// </summary>
        Task<List<string>> ObtenerNombresColumnasAsync(string servidor, int puerto, string usuario, string contrasena, string baseDatos, string nombreTabla);

        /// <summary>
        /// Verifica si una base de datos existe consultando pg_database.
        /// </summary>
        Task<bool> BaseDatosExisteAsync(string servidor, int puerto, string usuario, string contrasena, string nombreBaseDatos);
    }
}
