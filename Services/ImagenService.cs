using System;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;
using ProyectoAPI.Data;
using ProyectoAPI.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using System.Collections.Generic;
using System.IO;

namespace ProyectoAPI.Services
{
    public class ImagenService
    {
        private readonly IHttpClientFactory _clientFactory;
        private readonly ApplicationDbContext _context;
        private readonly IConfiguration _configuration;
        private readonly string _rutaGuardado;
        private readonly string _roboflowApiKey;
        private readonly string _roboflowEndpoint;

        public ImagenService(IHttpClientFactory clientFactory, ApplicationDbContext context, IConfiguration configuration)
        {
            _clientFactory = clientFactory;
            _context = context;
            _configuration = configuration;
            _rutaGuardado = _configuration["ImagenSettings:RutaGuardado"];
            _roboflowApiKey = _configuration["Roboflow:ApiKey"];
            _roboflowEndpoint = _configuration["Roboflow:Endpoint"];
        }

        public async Task<object> ProcesarImagenAsync(IFormFile imagen)
        {
            // Convertir imagen a base64
            string imagenBase64;
            using (var ms = new MemoryStream())
            {
                await imagen.CopyToAsync(ms);
                var imageBytes = ms.ToArray();
                imagenBase64 = Convert.ToBase64String(imageBytes);
            }

            // Procesar imagen con Roboflow
            var predicciones = await EnviarImagenARoboflowAsync(imagenBase64);

            // Guardar imagen
            var (rutaImagen, nombreImagen) = await GuardarImagenAsync(imagen);

            // Analizar predicciones y registrar enfermedades
            var enfermedadesDetectadas = await AnalizarPrediccionesAsync(predicciones, nombreImagen);

            return new
            {
                RutaImagen = rutaImagen,
                EnfermedadesDetectadas = enfermedadesDetectadas
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

            using (var stream = new FileStream(filePath, FileMode.Create))
            {
                await imagen.CopyToAsync(stream);
            }

            return (filePath, fileName);
        }

        private async Task<List<string>> AnalizarPrediccionesAsync(JObject predicciones, string nombreImagen)
        {
            var enfermedadesDetectadas = new List<string>();

            foreach (var prediccion in predicciones["predictions"])
            {
                var nombreEnfermedad = prediccion["class"].ToString();
                var confianzaDecimal = (float)prediccion["confidence"];
                var confianzaEntero = (int)(confianzaDecimal * 100);

                if (confianzaEntero > 80) // Umbral de confianza (85%)
                {
                    enfermedadesDetectadas.Add($"{nombreEnfermedad} (Confianza: {confianzaEntero}%)");
                    await RegistrarEnfermedadAsync(nombreEnfermedad, confianzaEntero, nombreImagen);
                }
            }

            return enfermedadesDetectadas;
        }

        private async Task RegistrarEnfermedadAsync(string nombreEnfermedad, int confianza, string nombreImagen)
        {
            var cultivoActivo = await _context.Cultivos.FirstOrDefaultAsync(c => c.Activo);
            if (cultivoActivo != null)
            {
                var enfermedad = new Enfermedad
                {
                    Nombre = nombreEnfermedad,
                    Confianza = confianza,
                    FechaDeteccion = DateTime.Now,
                    tbl_cultivos_cult_id = cultivoActivo.Id,
                    NombreImagen = nombreImagen,
                    NombreCultivo = cultivoActivo.Nombre
                };
                _context.Enfermedades.Add(enfermedad);
                await _context.SaveChangesAsync();
            }
        }
    }
}