namespace Japdeva.APIMovil.Common.Services.EncriptarHelperService;

/// <summary>
/// Interfaz para helpers de operaciones de encriptación
/// </summary>
public interface IEncriptarHelperService
{
    /// <summary>
    /// Deriva una clave de 256 bits usando PBKDF2
    /// </summary>
    /// <param name="traceId">Identificador de rastreo</param>
    /// <param name="clave">Clave original</param>
    /// <returns>Clave derivada de 256 bits</returns>
    byte[] DerivarClave(string traceId, string clave);

    /// <summary>
    /// Genera una clave AES-256 segura
    /// </summary>
    /// <param name="traceId">Identificador de rastreo</param>
    /// <returns>Clave en formato Base64</returns>
    string GenerarClaveAES256(string traceId);
}