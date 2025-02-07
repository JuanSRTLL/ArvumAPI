using Microsoft.AspNetCore.Mvc;
using ProyectoAPI.DTOs;
using ProyectoAPI.Services;
using System.Threading.Tasks;

namespace ProyectoAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ConversacionController : ControllerBase
    {
        private readonly ConversacionService _conversacionService;

        public ConversacionController(ConversacionService conversacionService)
        {
            _conversacionService = conversacionService;
        }

        [HttpPost("chat")]
        public async Task<IActionResult> ManejarConversacion([FromBody] ConversacionDTO conversacionDTO)
        {
            if (string.IsNullOrWhiteSpace(conversacionDTO.Mensaje))
            {
                return StatusCode(400, "El apartado 'mensaje' no debe estar vacío");
            }

            try
            {
                var result = await _conversacionService.ManejarConversacionAsync(conversacionDTO.Mensaje, conversacionDTO.ConversationId);
                return Ok(result);
            }
            catch (HttpRequestException ex)
            {
                return StatusCode(500, ex.Message);
            }
        }
    }
}