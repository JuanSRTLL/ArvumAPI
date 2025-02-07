﻿using System;

namespace ProyectoAPI.DTOs
{
    public class EnfermedadExternaDTO
    {
        public int Id { get; set; }
        public string Nombre { get; set; }
        public int Confianza { get; set; }
        public DateTime FechaDeteccion { get; set; }
        public string NombreImagen { get; set; }
    }
}