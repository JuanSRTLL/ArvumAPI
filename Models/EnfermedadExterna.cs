using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ProyectoAPI.Models
{
    public class EnfermedadExterna
    {
        [Key]
        [Column("enfx_id")]
        public int Id { get; set; }

        [Required]
        [Column("enfx_nombre")]
        public string Nombre { get; set; }

        [Required]
        [Column("enfx_confianza")]
        public int Confianza { get; set; }

        [Required]
        [Column("enfx_fechadeteccion")]
        public DateTime FechaDeteccion { get; set; }

        [Required]
        [Column("enfx_nombreimagen")]
        public string NombreImagen { get; set; }
    }
}