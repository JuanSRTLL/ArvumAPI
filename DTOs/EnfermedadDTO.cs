using System;

namespace ProyectoAPI.DTOs
{
    public class EnfermedadDTO
    {
        public int Id { get; set; }
        public string Nombre { get; set; }
        public int Confianza { get; set; }
        public DateTime FechaDeteccion { get; set; }
        public int CultivoId { get; set; }  
        public string NombreCultivo { get; set; }
        public string NombreImagen { get; set; }
    }
}