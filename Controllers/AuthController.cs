using Microsoft.AspNetCore.Mvc;
using ProyectoAPI.DTOs;
using ProyectoAPI.Services;
using System.Threading.Tasks;
using System.Text.RegularExpressions;

namespace ProyectoAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AuthController : ControllerBase
    {
        private readonly AuthService _authService;

        public AuthController(AuthService authService)
        {
            _authService = authService;
        }

        [HttpPost("register")]
        public async Task<IActionResult> Register([FromBody] usuarioDTO usuarioDTO)
        {
            // Validación del correo electrónico
            if (string.IsNullOrWhiteSpace(usuarioDTO.Correo) || !Regex.IsMatch(usuarioDTO.Correo, @"^[^@\s]+@[^@\s]+\.[^@\s]+$"))
            {
                return BadRequest("El correo electrónico no es válido.");
            }

            if (string.IsNullOrWhiteSpace(usuarioDTO.Contraseña) || usuarioDTO.Contraseña.Length < 6)
            {
                return BadRequest("La contraseña debe tener al menos 6 caracteres.");
            }

            var result = await _authService.RegisterAsync(usuarioDTO);
            if (result == "El usuario ya existe")
            {
                return Conflict(result); 
            }

            return Ok(result);
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] usuarioDTO usuarioDTO)
        {
            if (string.IsNullOrWhiteSpace(usuarioDTO.Correo) || string.IsNullOrWhiteSpace(usuarioDTO.Contraseña))
            {
                return BadRequest("El correo y la contraseña son requeridos.");
            }

            var (message, token) = await _authService.LoginAsync(usuarioDTO);
            if (token == null) return Unauthorized(message);

            return Ok(new { Token = token, Mensaje = message });
        }

        [HttpPost("forgot-password")]
        public async Task<IActionResult> ForgotPassword([FromBody] RecuperarContraseñaDTO forgotPasswordDTO)
        {
            if (string.IsNullOrWhiteSpace(forgotPasswordDTO.Correo) || !Regex.IsMatch(forgotPasswordDTO.Correo, @"^[^@\s]+@[^@\s]+\.[^@\s]+$"))
            {
                return BadRequest("El correo electrónico no es válido.");
            }

            var result = await _authService.ForgotPasswordAsync(forgotPasswordDTO);
            if (!result) return NotFound("Usuario no encontrado");
            return Ok("Se ha enviado un correo para restablecer su contraseña");
        }

        [HttpPost("reset-password")]
        public async Task<IActionResult> ResetPassword([FromBody] RestablecerContraseñaDTO resetPasswordDTO)
        {
            if (string.IsNullOrWhiteSpace(resetPasswordDTO.Correo) || string.IsNullOrWhiteSpace(resetPasswordDTO.Token) || string.IsNullOrWhiteSpace(resetPasswordDTO.NuevaContraseña))
            {
                return BadRequest("Todos los campos son requeridos.");
            }

            var result = await _authService.ResetPasswordAsync(resetPasswordDTO);
            if (!result) return BadRequest("El token es inválido o ha expirado");
            return Ok("Contraseña restablecida con éxito");
        }
    }
}