using Microsoft.AspNetCore.Mvc;
using ProyectoAPI.Services;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using System;

namespace ProyectoAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ImagenExternaController : ControllerBase
    {
        private readonly EnfermedadExternaService _enfermedadExternaService;
        private readonly ILogger<ImagenExternaController> _logger;

        public ImagenExternaController(EnfermedadExternaService enfermedadExternaService, ILogger<ImagenExternaController> logger)
        {
            _enfermedadExternaService = enfermedadExternaService;
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

                var resultado = await _enfermedadExternaService.ProcesarImagenAsync(imagen);
                return Ok(resultado);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al procesar la imagen externa");
                return StatusCode(500, new { error = "Ha ocurrido un error al procesar la imagen externa", details = ex.Message, stackTrace = ex.StackTrace });
            }
        }

        [HttpPost("procesarSinGuardar")]
        public async Task<IActionResult> ProcesarImagenSinGuardar(IFormFile imagen)
        {
            try
            {
                if (imagen == null || imagen.Length == 0)
                {
                    return BadRequest("No se ha proporcionado ninguna imagen.");
                }

                var resultado = await _enfermedadExternaService.ProcesarImagenSinGuardarAsync(imagen);
                return Ok(resultado);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al procesar la imagen externa sin guardar");
                return StatusCode(500, new { error = "Ha ocurrido un error al procesar la imagen externa sin guardar", details = ex.Message, stackTrace = ex.StackTrace });
            }
        }
    }
}