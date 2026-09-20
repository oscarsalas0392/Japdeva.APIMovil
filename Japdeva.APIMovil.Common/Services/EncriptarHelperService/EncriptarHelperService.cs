using System.Security.Cryptography;
using System.Text;
using Microsoft.Extensions.Logging;
using Japdeva.APIMovil.Common.Extensions;

namespace Japdeva.APIMovil.Common.Services.EncriptarHelperService;

/// <summary>
/// Servicio para helpers de operaciones de encriptación
/// </summary>
public class EncriptarHelperService : IEncriptarHelperService
{
    private readonly ILogger<EncriptarHelperService> _logger;
    private const int TAMANO_CLAVE_AES = 32; // 256 bits
    private const string SALT_ENCRIPTACION = "Japdeva.APIMovil.Salt.2025";
    private const int ITERACIONES_PBKDF2 = 100000;
    private const string MENSAJE_ERROR_DERIVACION = "Error al derivar clave";
    private const string MENSAJE_ERROR_GENERACION = "Error al generar clave AES256";

    /// <summary>
    /// Inicializa una nueva instancia del servicio helper de encriptación
    /// </summary>
    /// <param name="logger">Logger para registro de eventos</param>
    public EncriptarHelperService(ILogger<EncriptarHelperService> logger)
    {
        this._logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    /// <summary>
    /// Deriva una clave de 256 bits usando PBKDF2
    /// </summary>
    /// <param name="traceId">Identificador de rastreo</param>
    /// <param name="clave">Clave original</param>
    /// <returns>Clave derivada de 256 bits</returns>
    public byte[] DerivarClave(string traceId, string clave)
    {
        string nombreMetodo = this.ObtenerNombreMetodo();
        
        try
        {
            this._logger.Inicio(traceId, nombreMetodo);
            
            // Salt fijo para consistencia (en producción debería ser dinámico y almacenado)
            byte[] salt = Encoding.UTF8.GetBytes(SALT_ENCRIPTACION);
            
            return Rfc2898DeriveBytes.Pbkdf2(clave, salt, ITERACIONES_PBKDF2, HashAlgorithmName.SHA256, TAMANO_CLAVE_AES);
        }
        catch (Exception ex)
        {
            this._logger.Error(traceId, nombreMetodo, ex);
            throw new InvalidOperationException(MENSAJE_ERROR_DERIVACION, ex);
        }
        finally
        {
            this._logger.Fin(traceId, nombreMetodo);
        }
    }

    /// <summary>
    /// Genera una clave AES-256 segura
    /// </summary>
    /// <param name="traceId">Identificador de rastreo</param>
    /// <returns>Clave en formato Base64</returns>
    public string GenerarClaveAES256(string traceId)
    {
        string nombreMetodo = this.ObtenerNombreMetodo();
        
        try
        {
            this._logger.Inicio(traceId, nombreMetodo);
            
            byte[] clave = new byte[TAMANO_CLAVE_AES];
            RandomNumberGenerator.Fill(clave);
            return Convert.ToBase64String(clave);
        }
        catch (Exception ex)
        {
            this._logger.Error(traceId, nombreMetodo, ex);
            throw new InvalidOperationException(MENSAJE_ERROR_GENERACION, ex);
        }
        finally
        {
            this._logger.Fin(traceId, nombreMetodo);
        }
    }
}