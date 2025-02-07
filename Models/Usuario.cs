using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ProyectoAPI.Models
{
    public class Usuario
    {
        [Key]
        [Column("usu_id")]
        public int Id { get; set; }

        [Column("usu_correo")]
        public string Correo { get; set; }

        [Column("usu_contraseña")]
        public string Contraseña { get; set; }

        [Column("usu_token_recuperacion")]
        public string? TokenRecuperacion { get; set; } 

        [Column("usu_token_expiracion")]
        public DateTime? TokenExpiracion { get; set; }
    }
}