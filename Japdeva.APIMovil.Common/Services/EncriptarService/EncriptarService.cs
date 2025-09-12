using System.Security.Cryptography;
using System.Text;
using Microsoft.Extensions.Logging;
using Japdeva.APIMovil.Common.Extensions;
using Japdeva.APIMovil.Common.Services.EncriptarHelperService;

namespace Japdeva.APIMovil.Common.Services.EncriptarService;

/// <summary>
/// Servicio robusto para encriptación usando AES-256-GCM
/// </summary>
public class EncriptarService : IEncriptarService
{
    private readonly ILogger<EncriptarService> _logger;
    private readonly IEncriptarHelperService _encriptarHelper;
    
    private const int TAMANO_NONCE_GCM = 12; // 96 bits recomendado para GCM
    private const int TAMANO_TAG_GCM = 16; // 128 bits
    private const string SEPARADOR_DATOS = ":";
    private const string MENSAJE_ERROR_TEXTO_VACIO = "El texto no puede estar vacío";
    private const string MENSAJE_ERROR_CLAVE_VACIA = "La clave no puede estar vacía";
    private const string MENSAJE_ERROR_ENCRIPTACION = "Error en proceso de encriptación";
    /// <summary>
    /// Inicializa una nueva instancia del servicio de encriptación
    /// </summary>
    /// <param name="logger">Logger para registro de eventos</param>
    /// <param name="encriptarHelper">Servicio helper para operaciones de encriptación</param>
    public EncriptarService(ILogger<EncriptarService> logger, IEncriptarHelperService encriptarHelper)
    {
        this._logger = logger ?? throw new ArgumentNullException(nameof(logger));
        this._encriptarHelper = encriptarHelper ?? throw new ArgumentNullException(nameof(encriptarHelper));
    }

    /// <summary>
    /// Encripta un texto plano con una clave específica usando AES-256-GCM
    /// </summary>
    /// <param name="traceId">Identificador de rastreo</param>
    /// <param name="textoPlano">Texto a encriptar</param>
    /// <param name="clave">Clave de encriptación personalizada</param>
    /// <returns>Texto encriptado en formato Base64</returns>
    public string EncriptarConClave(string traceId, string textoPlano, string clave)
    {
        string nombreMetodo = this.ObtenerNombreMetodo();
        
        try
        {
            this._logger.Inicio(traceId, nombreMetodo);
            
            if (string.IsNullOrWhiteSpace(textoPlano))
                throw new ArgumentException(MENSAJE_ERROR_TEXTO_VACIO, nameof(textoPlano));
            
            if (string.IsNullOrWhiteSpace(clave))
                throw new ArgumentException(MENSAJE_ERROR_CLAVE_VACIA, nameof(clave));

            // Derivar clave de 256 bits usando PBKDF2
            byte[] claveBytes = this._encriptarHelper.DerivarClave(traceId, clave);
            byte[] datosPlanos = Encoding.UTF8.GetBytes(textoPlano);

            using AesGcm aesGcm = new(claveBytes, TAMANO_TAG_GCM);
            
            // Generar nonce aleatorio
            byte[] nonce = new byte[TAMANO_NONCE_GCM];
            RandomNumberGenerator.Fill(nonce);
            
            // Preparar buffers
            byte[] datosEncriptados = new byte[datosPlanos.Length];
            byte[] tag = new byte[TAMANO_TAG_GCM];
            
            // Encriptar
            aesGcm.Encrypt(nonce, datosPlanos, datosEncriptados, tag);
            
            // Combinar nonce + datos encriptados + tag
            string resultado = $"{Convert.ToBase64String(nonce)}{SEPARADOR_DATOS}" +
                              $"{Convert.ToBase64String(datosEncriptados)}{SEPARADOR_DATOS}" +
                              $"{Convert.ToBase64String(tag)}";
                              
            return resultado;
        }
        catch (Exception ex)
        {
            this._logger.Error(traceId, nombreMetodo, ex);
            throw new InvalidOperationException(MENSAJE_ERROR_ENCRIPTACION, ex);
        }
        finally
        {
            this._logger.Fin(traceId, nombreMetodo);
        }
    }
}