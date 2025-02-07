using Microsoft.AspNetCore.Mvc;
using ProyectoAPI.Data;
using ProyectoAPI.Models;
using ProyectoAPI.DTOs;
using Microsoft.EntityFrameworkCore;
using System;
using System.Threading.Tasks;

namespace ProyectoAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class EnfermedadesController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public EnfermedadesController(ApplicationDbContext context)
        {
            _context = context;
        }

        [HttpPost("registrar")]
        public async Task<IActionResult> RegistrarEnfermedad([FromBody] EnfermedadDTO enfermedadDTO)
        {
            if (string.IsNullOrWhiteSpace(enfermedadDTO.Nombre))
            {
                return BadRequest("El nombre de la enfermedad no puede estar vacío.");
            }

            var cultivoActivo = await _context.Cultivos.FirstOrDefaultAsync(c => c.Activo);
            if (cultivoActivo == null)
            {
                return BadRequest("No hay un cultivo activo en este momento.");
            }

            var enfermedad = new Enfermedad
            {
                Nombre = enfermedadDTO.Nombre,
                tbl_cultivos_cult_id = cultivoActivo.Id,
                FechaDeteccion = DateTime.Now,
                NombreImagen = enfermedadDTO.NombreImagen,
                NombreCultivo = cultivoActivo.Nombre,
                Confianza = enfermedadDTO.Confianza
            };

            _context.Enfermedades.Add(enfermedad);
            await _context.SaveChangesAsync();

            return Ok($"Enfermedad '{enfermedadDTO.Nombre}' registrada con éxito para el cultivo '{cultivoActivo.Nombre}'.");
        }

        [HttpGet("listar")]
        public async Task<IActionResult> ListarEnfermedades()
        {
            var enfermedadesInternas = await _context.Enfermedades
                .Select(e => new EnfermedadDTO
                {
                    Id = e.Id,
                    Nombre = e.Nombre,
                    Confianza = e.Confianza,
                    FechaDeteccion = e.FechaDeteccion,
                    CultivoId = e.tbl_cultivos_cult_id,
                    NombreCultivo = e.NombreCultivo,
                    NombreImagen = e.NombreImagen
                })
                .ToListAsync();

            var enfermedadesExternas = await _context.EnfermedadesExternas
                .Select(e => new EnfermedadExternaDTO
                {
                    Id = e.Id,
                    Nombre = e.Nombre,
                    Confianza = e.Confianza,
                    FechaDeteccion = e.FechaDeteccion,
                    NombreImagen = e.NombreImagen
                })
                .ToListAsync();

            var resultado = new
            {
                EnfermedadesInternas = enfermedadesInternas,
                EnfermedadesExternas = enfermedadesExternas
            };

            return Ok(resultado);
        }
    }
}