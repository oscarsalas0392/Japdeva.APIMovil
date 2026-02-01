using Microsoft.AspNetCore.Mvc;
using Japdeva.APIMovil.Parametros.Services.ObtenerMenuPorPerfilService;

namespace Japdeva.APIMovil.Parametros.Controllers
{
    /// <summary>
    /// Controlador para gestionar las operaciones relacionadas con los menús.
    /// </summary>
    [ApiController]
    [Route("api/[controller]")]
    public class MenuController : Controller
    {
        /// <summary>
        /// Obtiene los menús activos asociados a un perfil específico.
        /// </summary>
        /// <param name="obtenerMenuPorPerfilService">Servicio para obtener los menús por perfil.</param>
        /// <param name="idPerfil">Identificador del perfil para filtrar los menús.</param>
        /// <returns>Una tarea que representa la operación asincrónica y contiene el resultado de la acción.</returns>
        [HttpGet("ObtenerMenusPorPerfil")]
        public Task<IActionResult> ObtenerMenusPorPerfil(
            [FromServices] IObtenerMenuPorPerfilService obtenerMenuPorPerfilService,
            [FromQuery(Name = "id-perfil")] int idPerfil) =>
            obtenerMenuPorPerfilService.ObtenerMenusPorPerfilAsync(HttpContext.TraceIdentifier, idPerfil);
    }
}
