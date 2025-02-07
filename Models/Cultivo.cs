using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ProyectoAPI.Models
{
    public class Cultivo
    {
        [Key]
        [Column("cult_id")]
        public int Id { get; set; }

        [Column("cul_nombre")]
        public string Nombre { get; set; }

        [Column("cul_fechainicio")]
        public DateTime FechaInicio { get; set; }

        [Column("cul_fechafin")]
        public DateTime? FechaFin { get; set; }

        [Column("cul_activo")]
        public bool Activo { get; set; }

        public virtual ICollection<Datos> Datos { get; set; }
        public virtual ICollection<Enfermedad> Enfermedades { get; set; }
    }
}