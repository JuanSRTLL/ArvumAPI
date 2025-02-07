using Microsoft.Extensions.Configuration;
using ProyectoAPI.Data;
using ProyectoAPI.DTOs;
using ProyectoAPI.Helpers;
using ProyectoAPI.Models;
using System;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;

namespace ProyectoAPI.Services
{
    public class AuthService
    {
        private readonly IConfiguration _configuration;
        private readonly ApplicationDbContext _context;

        public AuthService(IConfiguration configuration, ApplicationDbContext context)
        {
            _configuration = configuration;
            _context = context;
        }

        public async Task<string> RegisterAsync(usuarioDTO usuarioDTO)
        {
            var existingUser = await _context.Usuarios.FirstOrDefaultAsync(u => u.Correo == usuarioDTO.Correo);
            if (existingUser != null)
                return "El usuario ya existe"; // Retorna un mensaje específico

            var hashedPassword = PasswordHelper.HashPassword(usuarioDTO.Contraseña);

            var user = new Usuario
            {
                Correo = usuarioDTO.Correo,
                Contraseña = hashedPassword
            };

            await _context.Usuarios.AddAsync(user);
            await _context.SaveChangesAsync();

            return "Usuario registrado exitosamente";
        }

        public async Task<(string Message, string Token)> LoginAsync(usuarioDTO usuarioDTO)
        {
            var user = await _context.Usuarios.FirstOrDefaultAsync(u => u.Correo == usuarioDTO.Correo);
            if (user == null || !PasswordHelper.VerifyPassword(usuarioDTO.Contraseña, user.Contraseña))
            {
                return ("Correo o contraseña inválidos", null);
            }

            var token = JwtHelper.GenerateJwtToken(user, _configuration);

            return ("Inicio de sesión exitoso", token);
        }

        public async Task<bool> ForgotPasswordAsync(RecuperarContraseñaDTO forgotPasswordDTO)
        {
            var user = await _context.Usuarios.FirstOrDefaultAsync(u => u.Correo == forgotPasswordDTO.Correo);
            if (user == null)
                return false;

            // Generar token de recuperación
            user.TokenRecuperacion = Guid.NewGuid().ToString();
            user.TokenExpiracion = DateTime.Now.AddHours(1); // El token expira en 1 hora

            _context.Usuarios.Update(user);
            await _context.SaveChangesAsync();

            // Enviar correo electrónico
            try
            {
                EmailHelper.EnviarCorreoRecuperacion(user, _configuration);
            }
            catch (Exception ex)
            {
                
                throw new Exception($"No se pudo enviar el correo de recuperación: {ex.Message}", ex);
            }

            return true;
        }

        public async Task<bool> ResetPasswordAsync(RestablecerContraseñaDTO resetPasswordDTO)
        {
            var user = await _context.Usuarios.FirstOrDefaultAsync(u =>
                u.Correo == resetPasswordDTO.Correo &&
                u.TokenRecuperacion == resetPasswordDTO.Token &&
                u.TokenExpiracion > DateTime.Now);

            if (user == null)
                return false;

            // Actualizar contraseña
            user.Contraseña = PasswordHelper.HashPassword(resetPasswordDTO.NuevaContraseña);
            user.TokenRecuperacion = null;
            user.TokenExpiracion = null;

            _context.Usuarios.Update(user);
            await _context.SaveChangesAsync();

            return true;
        }
    }
}