using System;
using System.IO;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json.Linq;
using ProyectoAPI.Data;
using ProyectoAPI.Models;
using ProyectoAPI.DTOs;
using System.Text;
using System.Collections.Generic;
using System.Linq;

namespace ProyectoAPI.Services
{
    public class EnfermedadExternaService
    {
        private readonly ApplicationDbContext _context;
        private readonly IConfiguration _configuration;
        private readonly IHttpClientFactory _clientFactory;
        private readonly string _rutaGuardado;
        private readonly string _roboflowApiKey;
        private readonly string _roboflowEndpoint;

        public EnfermedadExternaService(ApplicationDbContext context, IConfiguration configuration, IHttpClientFactory clientFactory)
        {
            _context = context;
            _configuration = configuration;
            _clientFactory = clientFactory;
            _rutaGuardado = _configuration["ImagenSettings:RutaGuardadoExterna"];
            _roboflowApiKey = _configuration["Roboflow:ApiKey"];
            _roboflowEndpoint = _configuration["Roboflow:Endpoint"];
        }

        public async Task<object> ProcesarImagenAsync(IFormFile imagen)
        {
            string imagenBase64;
            using (var ms = new MemoryStream())
            {
                await imagen.CopyToAsync(ms);
                var imageBytes = ms.ToArray();
                imagenBase64 = Convert.ToBase64String(imageBytes);
            }

            var predicciones = await EnviarImagenARoboflowAsync(imagenBase64);
            var (rutaImagen, nombreImagen) = await GuardarImagenAsync(imagen);

            // Obtener todas las enfermedades detectadas
            var enfermedadesDetectadas = await AnalizarPrediccionesAsync(predicciones, nombreImagen);

            // Guardar solo la enfermedad con la confianza más alta
            var enfermedadConMayorConfianza = enfermedadesDetectadas
                .OrderByDescending(e => e.Confianza)
                .FirstOrDefault();

            if (enfermedadConMayorConfianza != null)
            {
                await RegistrarEnfermedadAsync(enfermedadConMayorConfianza);
            }

            return new
            {
                rutaImagen = rutaImagen,
                enfermedadesDetectadas = enfermedadesDetectadas
            };
        }

        public async Task<object> ProcesarImagenSinGuardarAsync(IFormFile imagen)
        {
            string imagenBase64;
            using (var ms = new MemoryStream())
            {
                await imagen.CopyToAsync(ms);
                var imageBytes = ms.ToArray();
                imagenBase64 = Convert.ToBase64String(imageBytes);
            }

            var predicciones = await EnviarImagenARoboflowAsync(imagenBase64);

            // Analizar predicciones y crear lista de enfermedades detectadas
            var enfermedadesDetectadas = predicciones["predictions"]
                .Where(p => (float)p["confidence"] > 0.5)
                .Select(p => new EnfermedadExternaDTO
                {
                    Nombre = p["class"].ToString(),
                    Confianza = (int)((float)p["confidence"] * 100),
                    FechaDeteccion = DateTime.Now
                })
                .ToList();

            // Retornar 
            return new
            {
                enfermedadesDetectadas = enfermedadesDetectadas.Select(e => new
                {
                    nombre = e.Nombre,
                    confianza = e.Confianza
                }).ToList()
            };
        }

        private async Task<JObject> EnviarImagenARoboflowAsync(string imagenBase64)
        {
            var client = _clientFactory.CreateClient();
            var content = new StringContent(imagenBase64, Encoding.UTF8, "application/x-www-form-urlencoded");
            var response = await client.PostAsync($"{_roboflowEndpoint}?api_key={_roboflowApiKey}", content);
            var responseContent = await response.Content.ReadAsStringAsync();
            return JObject.Parse(responseContent);
        }

        private async Task<(string rutaImagen, string nombreImagen)> GuardarImagenAsync(IFormFile imagen)
        {
            var fechaActual = DateTime.Now.ToString("yyyyMMdd_HHmmss");
            var fileName = $"{fechaActual}_{Guid.NewGuid()}{Path.GetExtension(imagen.FileName)}";
            var filePath = Path.Combine(_rutaGuardado, fileName);

            Directory.CreateDirectory(_rutaGuardado);

            using (var stream = new FileStream(filePath, FileMode.Create))
            {
                await imagen.CopyToAsync(stream);
            }

            return (filePath, fileName);
        }

        private async Task<List<EnfermedadExternaDTO>> AnalizarPrediccionesAsync(JObject predicciones, string nombreImagen)
        {
            var enfermedadesDetectadas = new List<EnfermedadExternaDTO>();

            foreach (var prediccion in predicciones["predictions"])
            {
                var nombreEnfermedad = prediccion["class"].ToString();
                var confianzaDecimal = (float)prediccion["confidence"];
                var confianzaEntero = (int)(confianzaDecimal * 100);

                if (confianzaEntero > 50) // Umbral de confianza para mostrar
                {
                    var enfermedadDTO = new EnfermedadExternaDTO
                    {
                        Nombre = nombreEnfermedad,
                        Confianza = confianzaEntero,
                        FechaDeteccion = DateTime.Now,
                        NombreImagen = nombreImagen
                    };
                    enfermedadesDetectadas.Add(enfermedadDTO);
                }
            }

            return enfermedadesDetectadas;
        }

        private List<object> AnalizarPrediccionesSinRegistrar(JObject predicciones)
        {
            return predicciones["predictions"]
                .Where(p => (float)p["confidence"] > 0.5)
                .Select(p => new
                {
                    nombre = p["class"].ToString(),
                    confianza = (int)((float)p["confidence"] * 100)
                })
                .ToList<object>();
        }

        private async Task RegistrarEnfermedadAsync(EnfermedadExternaDTO enfermedadDTO)
        {
            var enfermedad = new EnfermedadExterna
            {
                Nombre = enfermedadDTO.Nombre,
                Confianza = enfermedadDTO.Confianza,
                FechaDeteccion = enfermedadDTO.FechaDeteccion,
                NombreImagen = enfermedadDTO.NombreImagen
            };

            _context.EnfermedadesExternas.Add(enfermedad);
            await _context.SaveChangesAsync();
        }
    }
}