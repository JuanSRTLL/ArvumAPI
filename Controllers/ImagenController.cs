using Microsoft.AspNetCore.Mvc;
using ProyectoAPI.Services;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using System;

namespace ProyectoAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ImagenController : ControllerBase
    {
        private readonly ImagenService _imagenService;
        private readonly ILogger<ImagenController> _logger;

        public ImagenController(ImagenService imagenService, ILogger<ImagenController> logger)
        {
            _imagenService = imagenService;
            _logger = logger;
        }

        [HttpPost("procesar")]
        public async Task<IActionResult> ProcesarImagen(IFormFile imagen)
        {
            try
            {
                if (imagen == null || imagen.Length == 0)
                {
                    return BadRequest("No se ha proporcionado ninguna imagen.");
                }

                var resultado = await _imagenService.ProcesarImagenAsync(imagen);
                return Ok(resultado);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al procesar la imagen");
                return StatusCode(500, new { error = "Ha ocurrido un error al procesar la imagen", details = ex.Message, stackTrace = ex.StackTrace });
            }
        }
    }
}