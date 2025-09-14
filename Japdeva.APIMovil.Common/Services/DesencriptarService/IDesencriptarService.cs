namespace Japdeva.APIMovil.Common.Services.DesencriptarService;

/// <summary>
/// Interfaz para servicio de desencriptación
/// </summary>
public interface IDesencriptarService
{
    /// <summary>
    /// Desencripta un texto con una clave específica usando AES-256-GCM
    /// </summary>
    /// <param name="traceId">Identificador de rastreo</param>
    /// <param name="textoEncriptado">Texto encriptado en formato Base64</param>
    /// <param name="clave">Clave de desencriptación</param>
    /// <returns>Texto plano desencriptado</returns>
    string DesencriptarConClave(string traceId, string textoEncriptado, string clave);
}