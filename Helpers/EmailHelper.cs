using Microsoft.Extensions.Configuration;
using ProyectoAPI.Models;
using System;
using System.Net;
using System.Net.Mail;

namespace ProyectoAPI.Helpers
{
    public static class EmailHelper
    {
        public static bool EnviarCorreoRecuperacion(Usuario usuario, IConfiguration configuration)
        {
            try
            {
                var smtpSettings = configuration.GetSection("SmtpSettings");
                var fromAddress = new MailAddress(smtpSettings["FromEmail"], smtpSettings["FromName"]);
                var toAddress = new MailAddress(usuario.Correo);
                string fromPassword = smtpSettings["Password"];
                string subject = "Recuperación de contraseña - Arvum";

                
                string resetLink = $"https://arvum.gestionproyectoiot.com/Privado/RestablecerContraseña.aspx?correo={usuario.Correo}&token={usuario.TokenRecuperacion}";

                string body = $"Hola,\n\nPara restablecer tu contraseña, haz clic en el siguiente enlace:\n{resetLink}\n\nEste enlace expirará en 1 hora.\n\nSi no solicitaste este correo, por favor ignóralo.\n\nSaludos,\nEl equipo de Arvum";

                var smtp = new SmtpClient
                {
                    Host = smtpSettings["Server"],
                    Port = int.Parse(smtpSettings["Port"]),
                    EnableSsl = bool.Parse(smtpSettings["EnableSsl"]),
                    DeliveryMethod = SmtpDeliveryMethod.Network,
                    UseDefaultCredentials = false,
                    Credentials = new NetworkCredential(smtpSettings["Username"], fromPassword)
                };

                using (var message = new MailMessage(fromAddress, toAddress)
                {
                    Subject = subject,
                    Body = body
                })
                {
                    smtp.Send(message);
                }

                return true;
            }
            catch (Exception)
            {
                
                return false;
            }
        }
    }
}