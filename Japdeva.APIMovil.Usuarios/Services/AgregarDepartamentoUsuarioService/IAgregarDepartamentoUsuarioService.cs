using Microsoft.AspNetCore.Mvc;
using Japdeva.APIMovil.Usuarios.Models;

namespace Japdeva.APIMovil.Usuarios.Services.AgregarDepartamentoUsuarioService
{
    /// <summary>
    /// Contrato para el servicio de asignación de usuarios a departamentos.
    /// </summary>
    public interface IAgregarDepartamentoUsuarioService
    {
        /// <summary>
        /// Asigna un usuario a un departamento.
        /// </summary>
        /// <param name="traceId">Identificador de trazabilidad.</param>
        /// <param name="solicitud">Datos de la asignación.</param>
        Task<IActionResult> AgregarDepartamentoUsuarioAsync(string traceId, AgregarDepartamentoUsuarioSolicitudModel solicitud);
    }
}
