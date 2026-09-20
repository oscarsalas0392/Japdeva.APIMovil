using System.Security.Cryptography;
using System.Text;
using Microsoft.Extensions.Logging;
using Japdeva.APIMovil.Common.Extensions;
using Japdeva.APIMovil.Common.Services.EncriptarHelperService;

namespace Japdeva.APIMovil.Common.Services.DesencriptarService;

/// <summary>
/// Servicio robusto para desencriptación usando AES-256-GCM
/// </summary>
public class DesencriptarService : IDesencriptarService
{
    private readonly ILogger<DesencriptarService> _logger;
    private readonly IEncriptarHelperService _encriptarHelper;
    
    private const int TAMANO_TAG_GCM = 16; // 128 bits
    private const string SEPARADOR_DATOS = ":";
    private const string MENSAJE_ERROR_TEXTO_VACIO = "El texto no puede estar vacío";
    private const string MENSAJE_ERROR_CLAVE_VACIA = "La clave no puede estar vacía";
    private const string MENSAJE_ERROR_FORMATO_INVALIDO = "Formato de texto encriptado inválido";
    private const string MENSAJE_ERROR_AUTENTICACION = "Error de autenticación en desencriptación";
    private const string MENSAJE_ERROR_DESENCRIPTACION = "Error en proceso de desencriptación";
    private const int INDICE_INICIAL = 0;
    private const int INDICE_DATOS_ENCRIPTADOS = 1;
    private const int INDICE_TAG = 2;
    private const int COMPONENTES_ESPERADOS = 3;

    /// <summary>
    /// Inicializa una nueva instancia del servicio de desencriptación
    /// </summary>
    /// <param name="logger">Logger para registro de eventos</param>
    /// <param name="encriptarHelper">Servicio helper para operaciones de encriptación</param>
    public DesencriptarService(ILogger<DesencriptarService> logger, IEncriptarHelperService encriptarHelper)
    {
        this._logger = logger ?? throw new ArgumentNullException(nameof(logger));
        this._encriptarHelper = encriptarHelper ?? throw new ArgumentNullException(nameof(encriptarHelper));
    }

    /// <summary>
    /// Desencripta un texto con una clave específica usando AES-256-GCM
    /// </summary>
    /// <param name="traceId">Identificador de rastreo</param>
    /// <param name="textoEncriptado">Texto encriptado en formato Base64</param>
    /// <param name="clave">Clave de desencriptación</param>
    /// <returns>Texto plano desencriptado</returns>
    public string DesencriptarConClave(string traceId, string textoEncriptado, string clave)
    {
        string nombreMetodo = this.ObtenerNombreMetodo();
        
        try
        {
            this._logger.Inicio(traceId, nombreMetodo);
            
            if (string.IsNullOrWhiteSpace(textoEncriptado))
                throw new ArgumentException(MENSAJE_ERROR_TEXTO_VACIO, nameof(textoEncriptado));
            
            if (string.IsNullOrWhiteSpace(clave))
                throw new ArgumentException(MENSAJE_ERROR_CLAVE_VACIA, nameof(clave));

            string[] componentes = textoEncriptado.Split(SEPARADOR_DATOS);
            if (componentes.Length != COMPONENTES_ESPERADOS)
                throw new ArgumentException(MENSAJE_ERROR_FORMATO_INVALIDO);
            byte[] nonce = Convert.FromBase64String(componentes[INDICE_INICIAL]);
            byte[] datosEncriptados = Convert.FromBase64String(componentes[INDICE_DATOS_ENCRIPTADOS]);
            byte[] tag = Convert.FromBase64String(componentes[INDICE_TAG]);
            

            byte[] claveBytes = this._encriptarHelper.DerivarClave(traceId, clave);
            
            using AesGcm aesGcm = new(claveBytes, TAMANO_TAG_GCM);
            byte[] datosDesencriptados = new byte[datosEncriptados.Length];
            
            try
            {
                aesGcm.Decrypt(nonce, datosEncriptados, tag, datosDesencriptados);           
                return Encoding.UTF8.GetString(datosDesencriptados);
            }
            catch (AuthenticationTagMismatchException)
            {
                throw new InvalidOperationException(MENSAJE_ERROR_AUTENTICACION);
            }
        }
        catch (Exception ex)
        {
            this._logger.Error(traceId, nombreMetodo, ex);
            throw new InvalidOperationException(MENSAJE_ERROR_DESENCRIPTACION, ex);
        }
        finally
        {
            this._logger.Fin(traceId, nombreMetodo);
        }
    }
}