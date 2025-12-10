using System.Net;
using System.Net.Mail;
using Microsoft.Extensions.Configuration;

namespace SistemaFichaje.Services
{
    // 1. Definimos el "contrato": Qué sabe hacer este servicio
    public interface IEmailService
    {
        Task EnviarCorreoAsync(string destinatario, string asunto, string mensajeHtml);
    }

    // 2. La implementación real usando Gmail
    public class SmtpEmailService : IEmailService
    {
        private readonly IConfiguration _config;

        public SmtpEmailService(IConfiguration config)
        {
            _config = config;
        }

        public async Task EnviarCorreoAsync(string destinatario, string asunto, string mensajeHtml)
        {
            // Leemos la configuración (User Secrets o appsettings.json)
            var gmailSettings = _config.GetSection("GmailSettings");
            
            string? emailOrigen = gmailSettings["Email"];
            string? password = gmailSettings["AppPassword"];
            string? nombreRemitente = gmailSettings["SenderName"];

            // Configuración del cliente SMTP de Gmail
            var clienteSmtp = new SmtpClient("smtp.gmail.com")
            {
                Port = 587,
                Credentials = new NetworkCredential(emailOrigen, password),
                EnableSsl = true,
            };

            var mensaje = new MailMessage
            {
                From = new MailAddress(emailOrigen, nombreRemitente),
                Subject = asunto,
                Body = mensajeHtml,
                IsBodyHtml = true,
            };

            mensaje.To.Add(destinatario);

            try
            {
                await clienteSmtp.SendMailAsync(mensaje);
            }
            catch (Exception ex)
            {
                // Aquí podrías guardar un log si falla
                Console.WriteLine($"❌ Error enviando correo a {destinatario}: {ex.Message}");
                throw; // Re-lanzamos el error para verlo en pantalla
            }
        }
    }
}
