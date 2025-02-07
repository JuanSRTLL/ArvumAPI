using ProyectoAPI.Models;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

public class Datos
{
    [Key]
    [Column("dat_id")]
    public int Id { get; set; }

    [Column("tbl_cultivos_cult_id")]
    public int tbl_cultivos_cult_id { get; set; }

    [ForeignKey("tbl_cultivos_cult_id")]
    public virtual Cultivo Cultivo { get; set; }

    [Column("dat_humedadsuelo")]
    public float HumedadSuelo { get; set; }

    [Column("dat_humedadaire")]
    public float HumedadAire { get; set; }

    [Column("dat_temperatura")]
    public float Temperatura { get; set; }

    [Column("dat_indicecalorc")]
    public float indiceCalorC { get; set; }

    [Column("dat_indicecalorf")]
    public float indiceCalorF { get; set; }

    [Column("dat_niveldeagua")]
    public float NivelDeAgua { get; set; }

    [Column("dat_fechahora")]
    public DateTime FechaHora { get; set; }
}