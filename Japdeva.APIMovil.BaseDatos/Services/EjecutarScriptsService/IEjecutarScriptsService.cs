namespace Japdeva.APIMovil.BaseDatos.Services.EjecutarScriptsService
{
    /// <summary>
    /// Contrato para el servicio que orquesta la ejecución de scripts SQL por microservicio.
    /// </summary>
    public interface IEjecutarScriptsService
    {
        /// <summary>
        /// Lee la configuración, recorre las carpetas de cada microservicio y ejecuta los scripts SQL en orden.
        /// </summary>
        /// <param name="args">Argumentos de línea de comandos (--microservicio, --carpeta).</param>
        /// <returns>0 si todo fue exitoso, 1 si hubo errores.</returns>
        Task<int> EjecutarAsync(string[] args);
    }
}
