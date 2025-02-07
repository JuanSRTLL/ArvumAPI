using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ProyectoAPI.Models
{
    public class Esp32Cam
    {
        [Key]
        [Column("esp_id")]
        public int Id { get; set; }

        [Column("esp_hora")]
        public TimeSpan Hora { get; set; }
    }
}