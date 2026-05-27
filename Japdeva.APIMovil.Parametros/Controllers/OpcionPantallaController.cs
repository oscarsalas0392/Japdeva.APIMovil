using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Japdeva.APIMovil.Parametros.Services.ObtenerOpcionPantallaPorPerfilService;

namespace Japdeva.APIMovil.Parametros.Controllers
{
    /// <summary>
    /// Controlador para gestionar las operaciones relacionadas con las opciones de pantalla por perfil.
    /// </summary>
    [Authorize]
    [ApiController]
    [Route("api/[controller]")]
    public class OpcionPantallaController : Controller
    {
        /// <summary>
        /// Obtiene las opciones de pantalla activas asociadas a un perfil específico.
        /// </summary>
        /// <param name="obtenerOpcionPantallaPorPerfilService">Servicio para obtener las opciones de pantalla por perfil.</param>
        /// <param name="idPerfil">Identificador del perfil para filtrar las opciones.</param>
        /// <returns>Una tarea que representa la operación asincrónica y contiene el resultado de la acción.</returns>
        [HttpGet("ObtenerOpcionPantallaPorPerfil")]
        public Task<IActionResult> ObtenerOpcionPantallaPorPerfil(
            [FromServices] IObtenerOpcionPantallaPorPerfilService obtenerOpcionPantallaPorPerfilService,
            [FromQuery(Name = "id-perfil")] int idPerfil) =>
            obtenerOpcionPantallaPorPerfilService.ObtenerOpcionPantallaPorPerfilAsync(HttpContext.TraceIdentifier, idPerfil);
    }
}
