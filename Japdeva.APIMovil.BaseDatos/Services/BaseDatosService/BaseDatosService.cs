using Microsoft.Extensions.Logging;
using Npgsql;

namespace Japdeva.APIMovil.BaseDatos.Services.BaseDatosService
{
    /// <summary>
    /// Servicio para ejecutar operaciones SQL contra PostgreSQL usando Npgsql.
    /// </summary>
    public class BaseDatosService : IBaseDatosService
    {
        private readonly ILogger<BaseDatosService> _logger;
        private const string PLANTILLA_CONEXION = "Host={0};Port={1};Username={2};Password={3};Database={4};";
        private const string BASE_DATOS_POSTGRES = "postgres";

        /// <summary>
        /// Inicializa una nueva instancia de BaseDatosService.
        /// </summary>
        /// <param name="logger">Logger para registro de eventos.</param>
        public BaseDatosService(ILogger<BaseDatosService> logger)
        {
            this._logger = logger;
        }

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
        /// Verifica que el servidor sea alcanzable y las credenciales sean válidas.
        /// </summary>
        public async Task ProbarConexionAsync(string servidor, int puerto, string usuario, string contrasena)
        {
            string connectionString = string.Format(PLANTILLA_CONEXION, servidor, puerto, usuario, contrasena, BASE_DATOS_POSTGRES);
            await using NpgsqlConnection conexion = new NpgsqlConnection(connectionString);
            await conexion.OpenAsync();
        }

        public async Task EjecutarSentenciasAsync(string servidor, int puerto, string usuario, string contrasena, string baseDatos, IEnumerable<string> sentencias, Func<string, bool, string?, Task> onResultado)
        {
            string connectionString = string.Format(PLANTILLA_CONEXION, servidor, puerto, usuario, contrasena, baseDatos);
            await using NpgsqlConnection conexion = new NpgsqlConnection(connectionString);
            await conexion.OpenAsync();

            foreach (string sentencia in sentencias)
            {
                try
                {
                    this._logger.LogDebug("Ejecutando sentencia en '{BaseDatos}': {Sentencia}", baseDatos, sentencia[..Math.Min(80, sentencia.Length)]);
                    await using NpgsqlCommand comando = new NpgsqlCommand(sentencia, conexion);
                    await comando.ExecuteNonQueryAsync();
                    await onResultado(sentencia, true, null);
                }
                catch (Exception ex)
                {
                    this._logger.LogError(ex, "Error en sentencia de '{BaseDatos}'", baseDatos);
                    await onResultado(sentencia, false, ex.Message);
                }
            }
        }

        /// <summary>
        /// Termina todas las conexiones activas y elimina la base de datos si existe.
        /// </summary>
        /// <param name="servidor">Host del servidor PostgreSQL.</param>
        /// <param name="puerto">Puerto del servidor PostgreSQL.</param>
        /// <param name="usuario">Usuario de conexión.</param>
        /// <param name="contrasena">Contraseña de conexión.</param>
        /// <param name="nombreBaseDatos">Nombre de la base de datos a eliminar.</param>
        /// <summary>
        /// Retorna los nombres de las columnas actuales de una tabla consultando information_schema.
        /// </summary>
        public async Task<List<string>> ObtenerNombresColumnasAsync(string servidor, int puerto, string usuario, string contrasena, string baseDatos, string nombreTabla)
        {
            string connectionString = string.Format(PLANTILLA_CONEXION, servidor, puerto, usuario, contrasena, baseDatos);
            await using NpgsqlConnection conexion = new NpgsqlConnection(connectionString);
            await conexion.OpenAsync();

            List<string> columnas = [];
            string sql = "SELECT column_name FROM information_schema.columns WHERE table_schema = 'public' AND table_name = @nombreTabla ORDER BY ordinal_position";
            await using NpgsqlCommand comando = new NpgsqlCommand(sql, conexion);
            comando.Parameters.AddWithValue("nombreTabla", nombreTabla);
            await using NpgsqlDataReader lector = await comando.ExecuteReaderAsync();

            while (await lector.ReadAsync())
                columnas.Add(lector.GetString(0));

            return columnas;
        }

        /// <summary>
        /// Verifica si una base de datos existe consultando pg_database.
        /// </summary>
        public async Task<bool> BaseDatosExisteAsync(string servidor, int puerto, string usuario, string contrasena, string nombreBaseDatos)
        {
            string connectionString = string.Format(PLANTILLA_CONEXION, servidor, puerto, usuario, contrasena, BASE_DATOS_POSTGRES);
            await using NpgsqlConnection conexion = new NpgsqlConnection(connectionString);
            await conexion.OpenAsync();

            string sql = "SELECT 1 FROM pg_database WHERE datname = @nombre";
            await using NpgsqlCommand comando = new NpgsqlCommand(sql, conexion);
            comando.Parameters.AddWithValue("nombre", nombreBaseDatos);
            object? resultado = await comando.ExecuteScalarAsync();
            return resultado != null;
        }

        public async Task EliminarBaseDatosAsync(string servidor, int puerto, string usuario, string contrasena, string nombreBaseDatos)
        {
            try
            {
                this._logger.LogDebug("Eliminando base de datos '{NombreBaseDatos}'", nombreBaseDatos);

                string connectionString = string.Format(PLANTILLA_CONEXION, servidor, puerto, usuario, contrasena, BASE_DATOS_POSTGRES);
                await using NpgsqlConnection conexion = new NpgsqlConnection(connectionString);
                await conexion.OpenAsync();

                string sqlTerminar = $"""
                    SELECT pg_terminate_backend(pid)
                    FROM pg_stat_activity
                    WHERE datname = '{nombreBaseDatos}' AND pid <> pg_backend_pid()
                    """;

                await using NpgsqlCommand cmdTerminar = new NpgsqlCommand(sqlTerminar, conexion);
                await cmdTerminar.ExecuteNonQueryAsync();

                string sqlEliminar = $"""DROP DATABASE IF EXISTS "{nombreBaseDatos}" """;
                await using NpgsqlCommand cmdEliminar = new NpgsqlCommand(sqlEliminar, conexion);
                await cmdEliminar.ExecuteNonQueryAsync();
            }
            catch (Exception ex)
            {
                this._logger.LogError(ex, "Error al eliminar la base de datos '{NombreBaseDatos}'", nombreBaseDatos);
                throw;
            }
        }
    }
}
