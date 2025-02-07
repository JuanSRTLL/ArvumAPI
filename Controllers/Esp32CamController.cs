using Microsoft.AspNetCore.Mvc;
using ProyectoAPI.Data;
using ProyectoAPI.Models;
using ProyectoAPI.DTOs;
using Microsoft.EntityFrameworkCore;
using System.Threading.Tasks;
using System;
using System.Text.RegularExpressions;

namespace ProyectoAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class Esp32CamController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public Esp32CamController(ApplicationDbContext context)
        {
            _context = context;
        }

        [HttpPost("registrar-hora")]
        public async Task<IActionResult> RegistrarHora([FromBody] object body)
        {
            // Verificar si el body está vacío
            if (body == null)
            {
                return BadRequest(new { error = "Falta el campo \"hora\"" });
            }

            // Verificar si existe la propiedad hora
            var jsonObject = System.Text.Json.JsonSerializer.Deserialize<Dictionary<string, string>>(
                System.Text.Json.JsonSerializer.Serialize(body));

            if (!jsonObject.ContainsKey("hora"))
            {
                return BadRequest(new { error = "Falta el campo \"hora\"" });
            }

            string horaStr = jsonObject["hora"];

            // Verificar si la hora está vacía
            if (string.IsNullOrWhiteSpace(horaStr))
            {
                return BadRequest("El formato de la hora no es correcto, debes ingresar un formato hh:mm:ss");
               
            }

            // Validar el formato de la hora usando expresión regular
            var regex = new Regex(@"^([0-1][0-9]|2[0-3]):([0-5][0-9]):([0-5][0-9])$");
            if (!regex.IsMatch(horaStr))
            {
                return BadRequest("El formato de la hora no es correcto, debes ingresar un formato hh:mm:ss");
            }

            // Intentar convertir la hora
            if (!TimeSpan.TryParse(horaStr, out TimeSpan hora))
            {
                return BadRequest("El formato de la hora no es correcto, debes ingresar un formato hh:mm:ss");
            }

            var esp32Cam = new Esp32Cam
            {
                Hora = hora
            };

            _context.Esp32Cams.Add(esp32Cam);
            await _context.SaveChangesAsync();

            return Ok(new { mensaje = "Hora registrada con éxito." });
        }

        [HttpGet("listar-ultima-configuracion-hora")]
        public async Task<IActionResult> ListarUltimaConfiguracionHora()
        {
            var ultimaConfiguracion = await _context.Esp32Cams
                .OrderByDescending(e => e.Id)
                .Select(e => new Esp32CamDTO { Hora = e.Hora })
                .FirstOrDefaultAsync();

            if (ultimaConfiguracion == null)
            {
                return NotFound("No se encontró ninguna configuración de hora.");
            }

            return Ok(ultimaConfiguracion);
        }
    }
}