using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System.Text;
using System.Text.Json;
using System.Text.RegularExpressions;
using Japdeva.APIMovil.BaseDatos.Models;
using Japdeva.APIMovil.BaseDatos.Services.BaseDatosService;

namespace Japdeva.APIMovil.BaseDatos.Services.EjecutarScriptsService
{
    /// <summary>
    /// Servicio que orquesta la ejecución de scripts SQL por microservicio leyendo la configuración y las carpetas definidas.
    /// </summary>
    public class EjecutarScriptsService : IEjecutarScriptsService
    {
        private readonly ILogger<EjecutarScriptsService> _logger;
        private readonly IBaseDatosService _baseDatosService;
        private readonly IConfiguration _configuration;
        private const string SECCION_CONFIGURACION = "BaseDatos";
        private const string DIRECTORIO_BASES_DATOS = "BasesDatos";
        private const string CARPETA_BASE_DATOS = "BaseDatos";
        private const string BASE_DATOS_POSTGRES = "postgres";
        private const string ARG_MICROSERVICIO = "--microservicio";
        private const string ARG_CARPETA = "--carpeta";
        private const string EXTENSION_SQL = "*.sql";
        private const string ARCHIVO_ORDEN = "orden.json";
        private const string PATRON_NOMBRE_BD = @"CREATE\s+DATABASE\s+""?(\w+)""?";

        /// <summary>
        /// Inicializa una nueva instancia de EjecutarScriptsService.
        /// </summary>
        /// <param name="logger">Logger para registro de eventos.</param>
        /// <param name="baseDatosService">Servicio para operaciones contra PostgreSQL.</param>
        /// <param name="configuration">Configuración de la aplicación.</param>
        public EjecutarScriptsService(ILogger<EjecutarScriptsService> logger, IBaseDatosService baseDatosService, IConfiguration configuration)
        {
            this._logger = logger;
            this._baseDatosService = baseDatosService;
            this._configuration = configuration;
        }

        /// <summary>
        /// Lee la configuración, recorre las carpetas de cada microservicio y ejecuta los scripts SQL en orden.
        /// </summary>
        /// <param name="args">Argumentos de línea de comandos (--microservicio, --carpeta).</param>
        /// <returns>0 si todo fue exitoso, 1 si hubo errores.</returns>
        public async Task<int> EjecutarAsync(string[] args)
        {
            try
            {
                ConfiguracionModel config = this._configuration.GetSection(SECCION_CONFIGURACION).Get<ConfiguracionModel>()
                    ?? throw new InvalidOperationException($"No se pudo leer la sección '{SECCION_CONFIGURACION}' del appsettings.json");

                string soloMicroservicio = this.ObtenerArgumento(args, ARG_MICROSERVICIO);
                string soloCarpeta = this.ObtenerArgumento(args, ARG_CARPETA);

                this.EscribirEncabezado(config);

                Console.Write($"Contrasena para '{config.UsuarioPostgres}': ");
                string contrasena = this.LeerContrasena();
                Console.WriteLine();
                Console.WriteLine();

                IEnumerable<MicroservicioModel> microservicios = config.Microservicios.Where(m => m.Activo);

                if (!string.IsNullOrEmpty(soloMicroservicio))
                {
                    microservicios = microservicios.Where(m => m.Nombre.Equals(soloMicroservicio, StringComparison.OrdinalIgnoreCase));
                    if (!microservicios.Any())
                        throw new ArgumentException($"Microservicio '{soloMicroservicio}' no encontrado o no esta activo.");
                }

                IEnumerable<string> carpetas = string.IsNullOrEmpty(soloCarpeta)
                    ? config.OrdenCarpetas
                    : [soloCarpeta];

                int contadorArchivos = 0;
                int contadorErrores = 0;
                string dirBasesDatos = Path.Combine(AppContext.BaseDirectory, DIRECTORIO_BASES_DATOS);

                foreach (MicroservicioModel microservicio in microservicios)
                {
                    string dirMicroservicio = Path.Combine(dirBasesDatos, microservicio.Nombre);

                    if (!Directory.Exists(dirMicroservicio))
                    {
                        Console.ForegroundColor = ConsoleColor.Yellow;
                        Console.WriteLine($"Carpeta no encontrada, se omite: {dirMicroservicio}");
                        Console.ResetColor();
                        continue;
                    }

                    Console.ForegroundColor = ConsoleColor.Yellow;
                    Console.WriteLine($"[ {microservicio.Nombre} ]");
                    Console.ResetColor();

                    string? nombreBaseDatos = null;

                    foreach (string carpeta in carpetas)
                    {
                        string dirCarpeta = Path.Combine(dirMicroservicio, carpeta);

                        if (!Directory.Exists(dirCarpeta))
                            continue;

                        string[] archivos = this.ObtenerArchivosOrdenados(dirCarpeta);

                        if (archivos.Length == 0)
                            continue;

                        Console.ForegroundColor = ConsoleColor.DarkCyan;
                        Console.WriteLine($"  /{carpeta}");
                        Console.ResetColor();

                        foreach (string archivo in archivos)
                        {
                            Console.ForegroundColor = ConsoleColor.White;
                            Console.WriteLine($"    [{Path.GetFileName(archivo)}]");
                            Console.ResetColor();

                            string sql = File.ReadAllText(archivo);
                            string bdDestino = BASE_DATOS_POSTGRES;

                            if (carpeta == CARPETA_BASE_DATOS)
                            {
                                nombreBaseDatos = this.ExtraerNombreBaseDatos(sql);

                                if (config.RecrearBaseDatos)
                                    await this._baseDatosService.EliminarBaseDatosAsync(config.Servidor, config.Puerto, config.UsuarioPostgres, contrasena, nombreBaseDatos);
                            }
                            else
                            {
                                if (string.IsNullOrEmpty(nombreBaseDatos))
                                    nombreBaseDatos = this.ObtenerNombreBaseDatosDesdeArchivo(dirMicroservicio);

                                bdDestino = nombreBaseDatos;
                            }

                            IEnumerable<string> sentencias = this.DividirEnSentencias(sql);

                            await this._baseDatosService.EjecutarSentenciasAsync(
                                config.Servidor, config.Puerto, config.UsuarioPostgres, contrasena, bdDestino,
                                sentencias,
                                async (sentencia, exitoso, error) =>
                                {
                                    string resumen = this.ResumirSentencia(sentencia);
                                    if (exitoso)
                                    {
                                        Console.ForegroundColor = ConsoleColor.Green;
                                        Console.WriteLine($"      OK  {resumen}");
                                    }
                                    else
                                    {
                                        Console.ForegroundColor = ConsoleColor.Red;
                                        Console.WriteLine($"      ERR {resumen}");
                                        Console.WriteLine($"          {error}");
                                        contadorErrores++;
                                    }
                                    Console.ResetColor();
                                    contadorArchivos++;
                                    await Task.CompletedTask;
                                });
                        }
                    }

                    Console.WriteLine();
                }

                this.EscribirResumen(contadorArchivos, contadorErrores);
                return contadorErrores > 0 ? 1 : 0;
            }
            catch (Exception ex)
            {
                this._logger.LogError(ex, "Error fatal en la ejecucion de scripts");
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine($"Error: {ex.Message}");
                Console.ResetColor();
                return 1;
            }
        }

        private string[] ObtenerArchivosOrdenados(string dirCarpeta)
        {
            string rutaOrden = Path.Combine(dirCarpeta, ARCHIVO_ORDEN);

            if (File.Exists(rutaOrden))
            {
                string[] nombres = JsonSerializer.Deserialize<string[]>(File.ReadAllText(rutaOrden)) ?? [];
                return nombres
                    .Select(n => Path.Combine(dirCarpeta, n))
                    .Where(File.Exists)
                    .ToArray();
            }

            return Directory.GetFiles(dirCarpeta, EXTENSION_SQL).OrderBy(f => f).ToArray();
        }

        private string ExtraerNombreBaseDatos(string sql)
        {
            Match match = Regex.Match(sql, PATRON_NOMBRE_BD, RegexOptions.IgnoreCase);
            if (!match.Success)
                throw new InvalidOperationException("No se pudo extraer el nombre de la base de datos del script CreateDatabase.sql");

            return match.Groups[1].Value;
        }

        private string ObtenerNombreBaseDatosDesdeArchivo(string dirMicroservicio)
        {
            string dirBaseDatos = Path.Combine(dirMicroservicio, CARPETA_BASE_DATOS);
            string[] archivos = Directory.GetFiles(dirBaseDatos, EXTENSION_SQL);

            if (archivos.Length == 0)
                throw new InvalidOperationException($"No se encontro CreateDatabase.sql en: {dirBaseDatos}");

            return this.ExtraerNombreBaseDatos(File.ReadAllText(archivos[0]));
        }

        private IEnumerable<string> DividirEnSentencias(string sql)
        {
            List<string> sentencias = [];
            StringBuilder actual = new();
            bool dentroDeString = false;

            for (int i = 0; i < sql.Length; i++)
            {
                char c = sql[i];

                if (c == '\'' && !dentroDeString)
                {
                    dentroDeString = true;
                    actual.Append(c);
                }
                else if (c == '\'' && dentroDeString)
                {
                    actual.Append(c);
                    // Comilla escapada ''
                    if (i + 1 < sql.Length && sql[i + 1] == '\'')
                    {
                        actual.Append(sql[i + 1]);
                        i++;
                    }
                    else
                    {
                        dentroDeString = false;
                    }
                }
                else if (c == ';' && !dentroDeString)
                {
                    string sentencia = actual.ToString().Trim();
                    if (!string.IsNullOrWhiteSpace(sentencia))
                        sentencias.Add(sentencia);
                    actual.Clear();
                }
                else
                {
                    actual.Append(c);
                }
            }

            string ultima = actual.ToString().Trim();
            if (!string.IsNullOrWhiteSpace(ultima))
                sentencias.Add(ultima);

            return sentencias;
        }

        private string ResumirSentencia(string sentencia)
        {
            string linea = sentencia.ReplaceLineEndings(" ").Trim();
            return linea.Length > 80 ? linea[..80] + "..." : linea;
        }

        private string ObtenerArgumento(string[] argumentos, string nombre)
        {
            int indice = Array.IndexOf(argumentos, nombre);
            return indice >= 0 && indice + 1 < argumentos.Length ? argumentos[indice + 1] : string.Empty;
        }

        private string LeerContrasena()
        {
            string contrasena = string.Empty;
            ConsoleKeyInfo tecla;

            do
            {
                tecla = Console.ReadKey(intercept: true);
                if (tecla.Key == ConsoleKey.Backspace && contrasena.Length > 0)
                {
                    contrasena = contrasena[..^1];
                    Console.Write("\b \b");
                }
                else if (tecla.Key != ConsoleKey.Enter && !char.IsControl(tecla.KeyChar))
                {
                    contrasena += tecla.KeyChar;
                    Console.Write("*");
                }
            } while (tecla.Key != ConsoleKey.Enter);

            return contrasena;
        }

        private void EscribirEncabezado(ConfiguracionModel config)
        {
            Console.ForegroundColor = ConsoleColor.Cyan;
            Console.WriteLine();
            Console.WriteLine("=============================================");
            Console.WriteLine("  Japdeva APIMovil - Ejecucion SQL");
            Console.WriteLine($"  Servidor : {config.Servidor}:{config.Puerto}");
            Console.WriteLine($"  Usuario  : {config.UsuarioPostgres}");
            Console.WriteLine($"  Recrear  : {(config.RecrearBaseDatos ? "Si" : "No")}");
            Console.WriteLine("=============================================");
            Console.ResetColor();
            Console.WriteLine();
        }

        private void EscribirResumen(int contadorArchivos, int contadorErrores)
        {
            Console.ForegroundColor = ConsoleColor.Cyan;
            Console.WriteLine("=============================================");
            Console.WriteLine($"  Archivos procesados : {contadorArchivos}");
            Console.Write("  Errores             : ");

            if (contadorErrores > 0)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine(contadorErrores);
            }
            else
            {
                Console.ForegroundColor = ConsoleColor.Green;
                Console.WriteLine("Ninguno");
            }

            Console.ForegroundColor = ConsoleColor.Cyan;
            Console.WriteLine("=============================================");
            Console.ResetColor();
        }
    }
}
