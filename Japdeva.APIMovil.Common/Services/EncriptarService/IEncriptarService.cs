namespace Japdeva.APIMovil.Common.Services.EncriptarService;

/// <summary>
/// Interfaz para servicios de encriptación
/// </summary>
public interface IEncriptarService
{
    /// <summary>
    /// Encripta un texto plano con una clave específica
    /// </summary>
    /// <param name="traceId">Identificador de rastreo</param>
    /// <param name="textoPlano">Texto a encriptar</param>
    /// <param name="clave">Clave de encriptación personalizada</param>
    /// <returns>Texto encriptado en formato Base64</returns>
    string EncriptarConClave(string traceId, string textoPlano, string clave);
}