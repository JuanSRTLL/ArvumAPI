using Microsoft.EntityFrameworkCore;
using ProyectoAPI.Data;
using ProyectoAPI.DTOs;
using ProyectoAPI.Models;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace ProyectoAPI.Services
{
    public class CultivoService
    {
        private readonly ApplicationDbContext _context;

        public CultivoService(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<(bool Succeeded, string Message)> IniciarCultivoAsync(string nombre)
        {
            var cultivoExistente = await _context.Cultivos.AnyAsync(c => c.Activo);
            if (cultivoExistente) return (false, "Ya existe un cultivo activo. No se puede crear uno nuevo.");

            var nuevoCultivo = new Cultivo
            {
                Nombre = nombre,
                FechaInicio = DateTime.Now,
                Activo = true
            };

            await _context.Cultivos.AddAsync(nuevoCultivo);
            await _context.SaveChangesAsync();

            return (true, $"Nuevo cultivo '{nombre}' iniciado correctamente");
        }

        public async Task<(bool Succeeded, string Message)> DesactivarCultivoAsync()
        {
            var cultivo = await _context.Cultivos.SingleOrDefaultAsync(c => c.Activo);
            if (cultivo == null) return (false, "No hay un cultivo activo para desactivar.");

            cultivo.Activo = false;
            cultivo.FechaFin = DateTime.Now;
            await _context.SaveChangesAsync();

            return (true, "Cultivo desactivado correctamente.");
        }

        public async Task<(bool Succeeded, string Message)> RegistrarDatosAsync(DatosDTO datosDTO)
        {
            try
            {
                var cultivo = await _context.Cultivos.SingleOrDefaultAsync(c => c.Activo);
                if (cultivo == null) return (false, "No hay cultivo activo para registrar datos.");
                var datos = new Datos
                {
                    tbl_cultivos_cult_id = cultivo.Id,
                    HumedadSuelo = datosDTO.HumedadSuelo,
                    HumedadAire = datosDTO.HumedadAire,
                    Temperatura = datosDTO.Temperatura,
                    indiceCalorC = datosDTO.IndiceCalorC,
                    indiceCalorF = datosDTO.IndiceCalorF,
                    NivelDeAgua = datosDTO.NivelDeAgua,
                    FechaHora = DateTime.Now
                };
                await _context.Datos.AddAsync(datos);
                await _context.SaveChangesAsync();
                return (true, "Datos registrados correctamente.");
            }
            catch (DbUpdateException dbEx)
            {
                // Error específico de la base de datos
                Console.WriteLine($"Error al guardar en la base de datos: {dbEx.InnerException?.Message ?? dbEx.Message}");
                return (false, $"Error al guardar en la base de datos: {dbEx.InnerException?.Message ?? dbEx.Message}");
            }
            catch (InvalidOperationException ioEx)
            {
                
                Console.WriteLine($"Error de operación: {ioEx.Message}");
                return (false, $"Error de operación al registrar los datos: {ioEx.Message}");
            }
            catch (Exception ex)
            {
                // Otros errores no específicos
                Console.WriteLine($"Error al registrar datos: {ex.Message}");
                return (false, $"Error general al registrar los datos: {ex.Message}");
            }
        }

        public async Task<object> ObtenerTodosLosDatosCultivoAsync()
        {
            var cultivoActivo = await _context.Cultivos
                .Where(c => c.Activo)
                .Select(s => new
                {
                    s.Id,
                    s.Nombre,
                    s.FechaInicio,
                    FechaFin = (DateTime?)null,
                    CantidadDatos = s.Datos.Count(),
                    PromedioHumedadSuelo = s.Datos.Any() ? s.Datos.Average(d => d.HumedadSuelo) : (float?)null,
                    PromedioHumedadAire = s.Datos.Any() ? s.Datos.Average(d => d.HumedadAire) : (float?)null,
                    PromedioTemperatura = s.Datos.Any() ? s.Datos.Average(d => d.Temperatura) : (float?)null,
                    PromedioIndiceCalorC = s.Datos.Any() ? s.Datos.Average(d => d.indiceCalorC) : (float?)null,
                    PromedioIndiceCalorF = s.Datos.Any() ? s.Datos.Average(d => d.indiceCalorF) : (float?)null,
                    PromedioNivelDeAgua = s.Datos.Any() ? s.Datos.Average(d => d.NivelDeAgua) : (float?)null,
                    Enfermedades = s.Enfermedades.Select(e => e.Nombre).ToList(),
                    EstadoSalud = s.Enfermedades.Any() ? "Enfermo" : "Sano"
                })
                .FirstOrDefaultAsync();

            var cultivosAnteriores = await _context.Cultivos
                .Where(c => !c.Activo)
                .OrderByDescending(c => c.FechaInicio)
                .Select(s => new
                {
                    s.Id,
                    s.Nombre,
                    s.FechaInicio,
                    s.FechaFin,
                    CantidadDatos = s.Datos.Count(),
                    PromedioHumedadSuelo = s.Datos.Any() ? s.Datos.Average(d => d.HumedadSuelo) : (float?)null,
                    PromedioHumedadAire = s.Datos.Any() ? s.Datos.Average(d => d.HumedadAire) : (float?)null,
                    PromedioTemperatura = s.Datos.Any() ? s.Datos.Average(d => d.Temperatura) : (float?)null,
                    PromedioIndiceCalorC = s.Datos.Any() ? s.Datos.Average(d => d.indiceCalorC) : (float?)null,
                    PromedioIndiceCalorF = s.Datos.Any() ? s.Datos.Average(d => d.indiceCalorF) : (float?)null,
                    PromedioNivelDeAgua = s.Datos.Any() ? s.Datos.Average(d => d.NivelDeAgua) : (float?)null,
                    Enfermedades = s.Enfermedades.Select(e => e.Nombre).ToList(),
                    EstadoSalud = s.Enfermedades.Any() ? "Enfermo" : "Sano"
                })
                .ToListAsync();

            var datosIndividuales = await _context.Cultivos
                .SelectMany(s => s.Datos.Select(d => new
                {
                    s.Id,
                    d.HumedadSuelo,
                    d.HumedadAire,
                    d.Temperatura,
                    d.indiceCalorC,
                    d.indiceCalorF,
                    d.NivelDeAgua,
                    d.FechaHora
                }))
                .OrderBy(d => d.Id)
                .ThenByDescending(d => d.FechaHora)
                .ToListAsync();

            return new
            {
                CultivoActivo = cultivoActivo,
                CultivosAnteriores = cultivosAnteriores,
                DatosIndividuales = datosIndividuales
            };
        }

        public async Task<(bool Succeeded, string Message)> EliminarCultivoAsync(int id)
        {
            var cultivo = await _context.Cultivos
                .Include(c => c.Datos)
                .FirstOrDefaultAsync(c => c.Id == id);

            if (cultivo == null)
                return (false, $"No se encontró un cultivo con el ID {id}.");

            _context.Datos.RemoveRange(cultivo.Datos);

            var enfermedades = await _context.Enfermedades
                .Where(e => e.tbl_cultivos_cult_id == id)
                .ToListAsync();
            _context.Enfermedades.RemoveRange(enfermedades);

            _context.Cultivos.Remove(cultivo);

            await _context.SaveChangesAsync();

            return (true, $"El Cultivo \"{cultivo.Nombre}\" ha sido eliminado.");
        }

        public async Task<(bool Succeeded, string Message)> ModificarNombreCultivoAsync(int id, string nuevoNombre)
        {
            using var transaction = await _context.Database.BeginTransactionAsync();

            try
            {
                var cultivo = await _context.Cultivos.FindAsync(id);
                if (cultivo == null)
                    return (false, $"No se encontró un cultivo con el ID {id}.");

                cultivo.Nombre = nuevoNombre;

                var enfermedades = await _context.Enfermedades
                    .Where(e => e.tbl_cultivos_cult_id == id)
                    .ToListAsync();

                foreach (var enfermedad in enfermedades)
                {
                    enfermedad.NombreCultivo = nuevoNombre;
                }

                await _context.SaveChangesAsync();
                await transaction.CommitAsync();

                return (true, $"El nombre del cultivo ha sido modificado a '{nuevoNombre}' y actualizado en las enfermedades asociadas.");
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                return (false, $"Error al modificar el nombre del cultivo: {ex.Message}");
            }
        }

        public async Task<(bool Succeeded, string Message, object Data)> ObtenerPromedioDiaActualAsync()
        {
            var hoy = DateTime.Today;
            var manana = hoy.AddDays(1);

            var cultivoActivo = await _context.Cultivos
                .Where(c => c.Activo)
                .FirstOrDefaultAsync();

            if (cultivoActivo == null)
                return (false, "No existe un cultivo activo.", null);

            var datosHoy = await _context.Datos
                .Where(d => d.tbl_cultivos_cult_id == cultivoActivo.Id && d.FechaHora >= hoy && d.FechaHora < manana)
                .ToListAsync();

            if (!datosHoy.Any())
                return (false, "No se han registrado datos para el cultivo activo en el día de hoy.", null);

            var promedioDiaActual = new
            {
                cultivoActivo.Id,
                cultivoActivo.Nombre,
                PromedioHumedadSuelo = datosHoy.Average(d => d.HumedadSuelo),
                PromedioHumedadAire = datosHoy.Average(d => d.HumedadAire),
                PromedioTemperatura = datosHoy.Average(d => d.Temperatura),
                PromedioIndiceCalorC = datosHoy.Average(d => d.indiceCalorC),
                PromedioIndiceCalorF = datosHoy.Average(d => d.indiceCalorF),
                PromedioNivelDeAgua = datosHoy.Average(d => d.NivelDeAgua)
            };

            return (true, "Datos promedio del día actual obtenidos con éxito.", promedioDiaActual);
        }
    }
}