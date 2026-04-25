using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Npgsql;
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
        private const string SECCION_CONFIGURACION = "ConfiguracionConexion";
        private const string DIRECTORIO_BASES_DATOS = "BasesDatos";
        private const string CARPETA_BASE_DATOS = "BaseDatos";
        private const string CARPETA_TABLAS = "Tablas";
        private const string BASE_DATOS_POSTGRES = "postgres";
        private const string ARG_MICROSERVICIO = "--microservicio";
        private const string ARG_CARPETA = "--carpeta";
        private const string EXTENSION_SQL = "*.sql";
        private const string ARCHIVO_ORDEN = "orden.json";
        private const string OPCION_NO_RECREAR = "0";
        private const string OPCION_RECREAR_TODAS = "1";
        private const string OPCION_RECREAR_ESPECIFICAS = "2";
        private const string SEPARADOR_SELECCION = ",";
        private const int INDICE_BASE = 1;
        private const string PATRON_NOMBRE_BD   = @"CREATE\s+DATABASE\s+(?:IF\s+NOT\s+EXISTS\s+)?""?(\w+)""?";
        private const string CARPETA_INDICES     = "Indices";
        private const string CARPETA_DATOS       = "Datos";
        private const string PATRON_BASE_DATOS   = @"CREATE\s+DATABASE";
        private const string PATRON_TABLA         = @"CREATE\s+TABLE\s+IF\s+NOT\s+EXISTS";
        private const string PATRON_TABLA_SIN_IF = @"CREATE\s+TABLE\s+(?!IF\s+NOT\s+EXISTS)";
        private const string PATRON_INDICE        = @"CREATE\s+(UNIQUE\s+)?INDEX\s+IF\s+NOT\s+EXISTS";
        private const string PATRON_INDICE_SIN_IF = @"CREATE\s+(UNIQUE\s+)?INDEX\s+(?!IF\s+NOT\s+EXISTS)";
        private const string PATRON_INSERT       = @"INSERT\s+INTO";
        private const string PATRON_NOT_EXISTS   = @"WHERE\s+NOT\s+EXISTS";

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

                IEnumerable<BaseDatosModel> microservicios = config.BaseDatos.Where(m => m.Ejecutar);

                if (!string.IsNullOrEmpty(soloMicroservicio))
                {
                    microservicios = microservicios.Where(m => m.Nombre.Equals(soloMicroservicio, StringComparison.OrdinalIgnoreCase));
                    if (!microservicios.Any())
                        throw new ArgumentException($"Microservicio '{soloMicroservicio}' no encontrado o no esta activo.");
                }

                IEnumerable<string> carpetas = string.IsNullOrEmpty(soloCarpeta)
                    ? config.OrdenCarpetas
                    : [soloCarpeta];

                // 1. Validar estructura de archivos antes de pedir datos al usuario
                List<string> erroresArchivos = this.ValidarArchivos(microservicios, carpetas);
                if (erroresArchivos.Count > 0)
                {
                    Console.ForegroundColor = ConsoleColor.Red;
                    Console.WriteLine($"Se encontraron {erroresArchivos.Count} error(es) en los scripts. Ejecucion cancelada.");
                    Console.ResetColor();
                    return 1;
                }

                Console.WriteLine();

                // 2. Solicitar datos al usuario con reintento en caso de fallo de conexion
                this.EscribirEncabezado(config);

                string contrasena;
                while (true)
                {
                    config.Servidor        = this.LeerValor("Servidor", config.Servidor);
                    config.Puerto          = int.TryParse(this.LeerValor("Puerto", config.Puerto.ToString()), out int puerto) ? puerto : config.Puerto;
                    config.UsuarioPostgres = this.LeerValor("Usuario", config.UsuarioPostgres);
                    this.LeerOpcionRecreacion(config, microservicios);

                    Console.Write($"Contrasena [{config.UsuarioPostgres}]: ");
                    contrasena = this.LeerContrasena();
                    Console.WriteLine();
                    Console.WriteLine();

                    // 3. Validar conexion con las credenciales ingresadas
                    List<string> erroresConexion = await this.ValidarConexionAsync(config, contrasena);
                    if (erroresConexion.Count == 0)
                        break;

                    bool reintentar = this.LeerOpcionBinaria("Volver a digitar la conexion", false);
                    Console.WriteLine();
                    if (!reintentar)
                        return 1;

                    Console.WriteLine();
                }

                Console.WriteLine();

                int contadorArchivos = 0;
                int contadorErrores = 0;
                string dirBasesDatos = Path.Combine(AppContext.BaseDirectory, DIRECTORIO_BASES_DATOS);

                foreach (BaseDatosModel microservicio in microservicios)
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

                                if (config.RecrearTodasBasesDatos || microservicio.RecrearBaseDatos)
                                    await this._baseDatosService.EliminarBaseDatosAsync(config.Servidor, config.Puerto, config.UsuarioPostgres, contrasena, nombreBaseDatos);

                                bool existe = await this._baseDatosService.BaseDatosExisteAsync(config.Servidor, config.Puerto, config.UsuarioPostgres, contrasena, nombreBaseDatos);
                                if (existe)
                                {
                                    Console.ForegroundColor = ConsoleColor.DarkYellow;
                                    Console.WriteLine($"      SKIP '{nombreBaseDatos}' ya existe");
                                    Console.ResetColor();
                                    contadorArchivos++;
                                    continue;
                                }
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
                                    await Task.CompletedTask;
                                });

                            if (carpeta == CARPETA_TABLAS)
                                contadorErrores += await this.CompararYAjustarTablaAsync(config, contrasena, bdDestino, sql);

                            contadorArchivos++;
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

        private string? ValidarDatos(string contenido)
        {
            int totalInserts = Regex.Matches(contenido, PATRON_INSERT, RegexOptions.IgnoreCase).Count;
            if (totalInserts == 0)
                return "debe contener INSERT INTO";

            int totalWhereNotExists = Regex.Matches(contenido, PATRON_NOT_EXISTS, RegexOptions.IgnoreCase).Count;
            if (totalInserts != totalWhereNotExists)
                return $"todos los INSERT deben usar WHERE NOT EXISTS ({totalWhereNotExists} de {totalInserts} lo tienen)";

            return null;
        }

        private string? ValidarEstructuraScript(string carpeta, string contenido)
        {
            return carpeta switch
            {
                CARPETA_BASE_DATOS => Regex.IsMatch(contenido, PATRON_BASE_DATOS, RegexOptions.IgnoreCase)
                    ? null
                    : "debe contener CREATE DATABASE",

                CARPETA_TABLAS => !Regex.IsMatch(contenido, PATRON_TABLA, RegexOptions.IgnoreCase)
                    ? "debe contener CREATE TABLE IF NOT EXISTS"
                    : Regex.IsMatch(contenido, PATRON_TABLA_SIN_IF, RegexOptions.IgnoreCase)
                    ? "todas las tablas deben usar CREATE TABLE IF NOT EXISTS"
                    : null,

                CARPETA_INDICES => !Regex.IsMatch(contenido, PATRON_INDICE, RegexOptions.IgnoreCase)
                    ? "debe contener CREATE INDEX IF NOT EXISTS o CREATE UNIQUE INDEX IF NOT EXISTS"
                    : Regex.IsMatch(contenido, PATRON_INDICE_SIN_IF, RegexOptions.IgnoreCase)
                    ? "todos los indices deben usar IF NOT EXISTS"
                    : null,

                CARPETA_DATOS => this.ValidarDatos(contenido),

                _ => null
            };
        }

        private List<string> ValidarArchivos(IEnumerable<BaseDatosModel> microservicios, IEnumerable<string> carpetas)
        {
            List<string> errores = [];
            string dirBasesDatos = Path.Combine(AppContext.BaseDirectory, DIRECTORIO_BASES_DATOS);

            Console.ForegroundColor = ConsoleColor.Cyan;
            Console.WriteLine("[ Validacion de scripts ]");
            Console.ResetColor();

            foreach (BaseDatosModel microservicio in microservicios)
            {
                foreach (string carpeta in carpetas)
                {
                    string dirCarpeta = Path.Combine(dirBasesDatos, microservicio.Nombre, carpeta);

                    if (!Directory.Exists(dirCarpeta))
                        continue;

                    string[] archivos = this.ObtenerArchivosOrdenados(dirCarpeta);

                    foreach (string rutaArchivo in archivos)
                    {
                        string nombre = Path.GetFileName(rutaArchivo);

                        string? errorEstructura = this.ValidarEstructuraScript(carpeta, File.ReadAllText(rutaArchivo));
                        if (errorEstructura != null)
                        {
                            string error = $"{microservicio.Nombre}/{carpeta}/{nombre}: {errorEstructura}";
                            errores.Add(error);
                            Console.ForegroundColor = ConsoleColor.Red;
                            Console.WriteLine($"  ERR {error}");
                            Console.ResetColor();
                        }
                    }
                }
            }

            if (errores.Count == 0)
            {
                Console.ForegroundColor = ConsoleColor.Green;
                Console.WriteLine("  Scripts validados correctamente.");
                Console.ResetColor();
            }

            return errores;
        }

        private async Task<List<string>> ValidarConexionAsync(ConfiguracionModel config, string contrasena)
        {
            List<string> errores = [];

            Console.ForegroundColor = ConsoleColor.Cyan;
            Console.WriteLine("[ Validacion de conexion ]");
            Console.ResetColor();

            Console.Write("  Servidor y credenciales...");
            try
            {
                await this._baseDatosService.ProbarConexionAsync(config.Servidor, config.Puerto, config.UsuarioPostgres, contrasena);
                Console.ForegroundColor = ConsoleColor.Green;
                Console.WriteLine(" OK");
                Console.ResetColor();
            }
            catch (PostgresException ex)
            {
                string mensaje = ex.SqlState switch
                {
                    "28P01" => $"Credenciales incorrectas para el usuario '{config.UsuarioPostgres}'.",
                    "28000" => $"Acceso denegado para el usuario '{config.UsuarioPostgres}'.",
                    "3D000" => "La base de datos 'postgres' no existe.",
                    _       => $"Error de PostgreSQL ({ex.SqlState}): {ex.MessageText}"
                };
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine($" ERR: {mensaje}");
                Console.ResetColor();
                errores.Add(mensaje);
            }
            catch (Exception ex)
            {
                string mensaje = ex.InnerException?.Message ?? ex.Message;
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine($" ERR: {mensaje}");
                Console.ResetColor();
                errores.Add(mensaje);
            }

            return errores;
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

        private async Task<int> CompararYAjustarTablaAsync(ConfiguracionModel config, string contrasena, string baseDatos, string sql)
        {
            int errores = 0;

            string? nombreTabla = this.ExtraerNombreTabla(sql);
            if (string.IsNullOrEmpty(nombreTabla)) return errores;

            List<string> columnasActuales = await this._baseDatosService.ObtenerNombresColumnasAsync(
                config.Servidor, config.Puerto, config.UsuarioPostgres, contrasena, baseDatos, nombreTabla);

            if (columnasActuales.Count == 0) return errores;

            Dictionary<string, string> columnasScript = this.ExtraerColumnasScript(sql);

            foreach (KeyValuePair<string, string> columna in columnasScript)
            {
                if (columnasActuales.Any(c => c.Equals(columna.Key, StringComparison.OrdinalIgnoreCase)))
                    continue;

                string alterSql = $"""ALTER TABLE "{nombreTabla}" ADD COLUMN IF NOT EXISTS "{columna.Key}" {columna.Value}""";

                await this._baseDatosService.EjecutarSentenciasAsync(
                    config.Servidor, config.Puerto, config.UsuarioPostgres, contrasena, baseDatos,
                    [alterSql],
                    async (sentencia, exitoso, error) =>
                    {
                        if (exitoso)
                        {
                            Console.ForegroundColor = ConsoleColor.Green;
                            Console.WriteLine($"      OK  ALTER TABLE \"{nombreTabla}\" ADD COLUMN \"{columna.Key}\"");
                        }
                        else
                        {
                            Console.ForegroundColor = ConsoleColor.Red;
                            Console.WriteLine($"      ERR  ALTER TABLE \"{nombreTabla}\" ADD COLUMN \"{columna.Key}\"");
                            Console.WriteLine($"           {error}");
                            errores++;
                        }
                        Console.ResetColor();
                        await Task.CompletedTask;
                    });
            }

            return errores;
        }

        private string? ExtraerNombreTabla(string sql)
        {
            Match match = Regex.Match(sql, @"CREATE\s+TABLE\s+(?:IF\s+NOT\s+EXISTS\s+)?""?(\w+)""?", RegexOptions.IgnoreCase);
            return match.Success ? match.Groups[1].Value : null;
        }

        private Dictionary<string, string> ExtraerColumnasScript(string sql)
        {
            Dictionary<string, string> columnas = new(StringComparer.OrdinalIgnoreCase);

            int inicio = sql.IndexOf('(');
            int fin = sql.LastIndexOf(')');
            if (inicio < 0 || fin < 0) return columnas;

            string[] lineas = sql[(inicio + 1)..fin].Split('\n');

            foreach (string linea in lineas)
            {
                string trimmed = linea.Trim().TrimEnd(',').Trim();
                if (string.IsNullOrWhiteSpace(trimmed)) continue;

                string upper = trimmed.ToUpperInvariant();
                if (upper.StartsWith("CONSTRAINT") || upper.StartsWith("PRIMARY KEY") ||
                    upper.StartsWith("FOREIGN KEY") || upper.StartsWith("UNIQUE") ||
                    upper.StartsWith("CHECK") || upper.StartsWith("REFERENCES") ||
                    upper.StartsWith(")"))
                    continue;

                string nombre;
                string definicion;

                if (trimmed.StartsWith('"'))
                {
                    int cierre = trimmed.IndexOf('"', 1);
                    if (cierre < 0) continue;
                    nombre = trimmed[1..cierre];
                    definicion = trimmed[(cierre + 1)..].Trim();
                }
                else
                {
                    int espacio = trimmed.IndexOf(' ');
                    if (espacio < 0) continue;
                    nombre = trimmed[..espacio];
                    definicion = trimmed[(espacio + 1)..].Trim();
                }

                if (!string.IsNullOrWhiteSpace(nombre) && !string.IsNullOrWhiteSpace(definicion))
                    columnas[nombre] = definicion;
            }

            return columnas;
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

        private bool LeerOpcionBinaria(string etiqueta, bool valorDefecto)
        {
            string defecto = valorDefecto ? "1" : "0";

            while (true)
            {
                Console.Write($"{etiqueta,-10} (0=No, 1=Si) [{defecto}]: ");
                string entrada = Console.ReadLine() ?? string.Empty;

                if (string.IsNullOrWhiteSpace(entrada))
                    return valorDefecto;

                if (entrada == "0") return false;
                if (entrada == "1") return true;

                Console.ForegroundColor = ConsoleColor.Yellow;
                Console.WriteLine("                Valor invalido. Ingrese 0 o 1.");
                Console.ResetColor();
            }
        }

        private string LeerValor(string etiqueta, string valorDefecto)
        {
            Console.Write($"{etiqueta,-10} [{valorDefecto}]: ");
            string entrada = Console.ReadLine() ?? string.Empty;
            return string.IsNullOrWhiteSpace(entrada) ? valorDefecto : entrada.Trim();
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

        private void LeerOpcionRecreacion(ConfiguracionModel config, IEnumerable<BaseDatosModel> microservicios)
        {
            while (true)
            {
                Console.Write("Recrear BD     (0=No, 1=Todas, 2=Seleccionar) [0]: ");
                string entrada = Console.ReadLine() ?? string.Empty;

                if (string.IsNullOrWhiteSpace(entrada) || entrada == OPCION_NO_RECREAR)
                {
                    config.RecrearTodasBasesDatos = false;
                    return;
                }

                if (entrada == OPCION_RECREAR_TODAS)
                {
                    config.RecrearTodasBasesDatos = true;
                    return;
                }

                if (entrada == OPCION_RECREAR_ESPECIFICAS)
                {
                    this.LeerSeleccionEspecifica(microservicios);
                    return;
                }

                Console.ForegroundColor = ConsoleColor.Yellow;
                Console.WriteLine("                Valor invalido. Ingrese 0, 1 o 2.");
                Console.ResetColor();
            }
        }

        private void LeerSeleccionEspecifica(IEnumerable<BaseDatosModel> microservicios)
        {
            List<BaseDatosModel> lista = microservicios.ToList();

            Console.WriteLine();
            for (int i = 0; i < lista.Count; i++)
            {
                Console.ForegroundColor = ConsoleColor.Cyan;
                Console.Write($"  [{i + INDICE_BASE}] ");
                Console.ResetColor();
                Console.WriteLine(lista[i].Nombre);
            }
            Console.WriteLine();

            while (true)
            {
                Console.Write("Seleccione numeros separados por coma: ");
                string entrada = Console.ReadLine() ?? string.Empty;

                if (string.IsNullOrWhiteSpace(entrada))
                {
                    Console.ForegroundColor = ConsoleColor.Yellow;
                    Console.WriteLine("  Debe seleccionar al menos una base de datos.");
                    Console.ResetColor();
                    continue;
                }

                bool hayErrores = false;
                foreach (BaseDatosModel bd in lista) bd.RecrearBaseDatos = false;

                foreach (string parte in entrada.Split(SEPARADOR_SELECCION))
                {
                    if (int.TryParse(parte.Trim(), out int numero) && numero >= INDICE_BASE && numero <= lista.Count)
                    {
                        lista[numero - INDICE_BASE].RecrearBaseDatos = true;
                    }
                    else
                    {
                        Console.ForegroundColor = ConsoleColor.Yellow;
                        Console.WriteLine($"  Numero invalido: '{parte.Trim()}'. Debe estar entre {INDICE_BASE} y {lista.Count}.");
                        Console.ResetColor();
                        foreach (BaseDatosModel bd in lista) bd.RecrearBaseDatos = false;
                        hayErrores = true;
                        break;
                    }
                }

                if (!hayErrores) break;
            }
        }

        private void EscribirEncabezado(ConfiguracionModel config)
        {
            Console.ForegroundColor = ConsoleColor.Cyan;
            Console.WriteLine();
            Console.WriteLine("=============================================");
            Console.WriteLine("  Japdeva APIMovil - Ejecucion SQL");
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
