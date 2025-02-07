using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ProyectoAPI.Models
{
    public class Enfermedad
    {
        [Key]
        [Column("enf_id")]
        public int Id { get; set; }

        [Column("enf_nombre")]
        public string Nombre { get; set; }

        [Column("enf_confianza")]
        public int Confianza { get; set; }

        [Column("enf_fechadeteccion")]
        public DateTime FechaDeteccion { get; set; }

        [Column("enf_nombreimagen")]
        public string NombreImagen { get; set; }

        [Column("tbl_cultivos_cult_id")]
        public int tbl_cultivos_cult_id { get; set; } 

        [ForeignKey("tbl_cultivos_cult_id")]
        public virtual Cultivo Cultivo { get; set; }

        [Column("enf_nombrecultivo")]
        public string NombreCultivo { get; set; }
    }
}