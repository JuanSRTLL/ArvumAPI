using Microsoft.AspNetCore.Mvc;
using ProyectoAPI.DTOs;
using ProyectoAPI.Services;
using System.Threading.Tasks;

namespace ProyectoAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class CultivoController : ControllerBase
    {
        private readonly CultivoService _cultivoService;

        public CultivoController(CultivoService cultivoService)
        {
            _cultivoService = cultivoService;
        }

        [HttpPost("iniciar")]
        public async Task<IActionResult> IniciarCultivo([FromBody] CultivoDTO cultivoDTO)
        {
           
            if (string.IsNullOrWhiteSpace(cultivoDTO.Nombre))
            {
                return BadRequest("El nombre del cultivo es requerido.");
            }

            var result = await _cultivoService.IniciarCultivoAsync(cultivoDTO.Nombre);
            if (result.Succeeded)
                return Ok(result.Message);

            return BadRequest(result.Message);
        }

        [HttpPost("desactivar")]
        public async Task<IActionResult> DesactivarCultivo()
        {
            var result = await _cultivoService.DesactivarCultivoAsync();
            if (result.Succeeded)
                return Ok(result.Message);
            return BadRequest(result.Message);
        }

        [HttpPost("registrar-datos")]
        public async Task<IActionResult> RegistrarDatos([FromBody] DatosDTO datosDTO)
        {
            var result = await _cultivoService.RegistrarDatosAsync(datosDTO);
            if (result.Succeeded)
                return Ok(result.Message);
            return BadRequest(result.Message);
        }

        [HttpGet("todoslosdatoscultivo")]
        public async Task<IActionResult> TodosLosDatosCultivo()
        {
            var result = await _cultivoService.ObtenerTodosLosDatosCultivoAsync();
            return Ok(result);
        }

        [HttpDelete("eliminarcultivo/{id}")]
        public async Task<IActionResult> EliminarCultivo(int id)
        {
            var result = await _cultivoService.EliminarCultivoAsync(id);
            if (result.Succeeded)
                return Ok(result.Message);
            return BadRequest(result.Message);
        }

        [HttpPut("modificarnombre/{id}")]
        public async Task<IActionResult> ModificarNombreCultivo(int id, [FromBody] CultivoDTO cultivoDTO)
        {
            if (string.IsNullOrWhiteSpace(cultivoDTO.Nombre))
            {
                return BadRequest("El nombre del cultivo es requerido.");
            }

            var result = await _cultivoService.ModificarNombreCultivoAsync(id, cultivoDTO.Nombre);
            if (result.Succeeded)
                return Ok(result.Message);
            return BadRequest(result.Message);
        }

        [HttpGet("promediodiaactual")]
        public async Task<IActionResult> ObtenerPromedioDiaActual()
        {
            var result = await _cultivoService.ObtenerPromedioDiaActualAsync();
            if (result.Succeeded)
                return Ok(new { Message = result.Message, Data = result.Data });
            return BadRequest(result.Message);
        }
    }
}