namespace ProyectoAPI.DTOs
{
    public class RestablecerContraseñaDTO
    {
        public string Correo { get; set; }
        public string Token { get; set; }
        public string NuevaContraseña { get; set; }
    }
}