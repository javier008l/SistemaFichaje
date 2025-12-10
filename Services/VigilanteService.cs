using Microsoft.EntityFrameworkCore;
using SistemaFichaje.Models;

namespace SistemaFichaje.Services
{
    public class VigilanteService : BackgroundService
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly ILogger<VigilanteService> _logger;

        // Configuramos cada cuánto tiempo revisa (ej. cada 1 hora) TimeSpan.FromHours(1)
        //  - aquí 30 segundos para pruebas
        private readonly TimeSpan _intervalo = TimeSpan.FromSeconds(30);

        public VigilanteService(IServiceProvider serviceProvider, ILogger<VigilanteService> logger)
        {
            _serviceProvider = serviceProvider;
            _logger = logger;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation("👀 Vigilante de Fichajes iniciado.");

            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    await RevisarOlvidos();
                }
                catch (Exception ex)
                {
                    _logger.LogError($"Error en el vigilante: {ex.Message}");
                }

                // Esperamos X tiempo antes de volver a revisar
                await Task.Delay(_intervalo, stoppingToken);
            }
        }

        private async Task RevisarOlvidos()
        {
            // Necesitamos crear un scope nuevo porque los BackgroundService son Singleton
            // y el DbContext es Scoped.
            using (var scope = _serviceProvider.CreateScope())
            {
                var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();
                var emailService = scope.ServiceProvider.GetRequiredService<IEmailService>();

                // REGLA: Si alguien entró hace más de 8 horas y NO ha salido .AddHours(-8)
                // para pruebas usamos 1 minuto
                var horaLimite = DateTimeOffset.UtcNow.AddMinutes(-1);

                // Buscamos los últimos eventos de cada usuario
                // (Esta consulta es simplificada, en producción se optimiza con GroupBy)
                var ultimosEventos = await context.FichajeEventos
                    .GroupBy(f => f.UsuarioExternoId)
                    .Select(g => g.OrderByDescending(e => e.FechaHora).FirstOrDefault())
                    .ToListAsync();

                foreach (var evento in ultimosEventos)
                {
                    if (evento != null && 
                        (evento.Tipo == TipoFichaje.Entrada || evento.Tipo == TipoFichaje.FinPausa) && 
                        evento.FechaHora < horaLimite)
                    {
                        // ¡ALERTA! Usuario se olvidó de salir
                        _logger.LogWarning($"⚠️ Usuario {evento.UsuarioExternoId} olvidó fichar salida.");

                        // Enviamos correo (Aquí usas tu correo fijo o buscas el del usuario si tuvieras tabla de usuarios)
                        await emailService.EnviarCorreoAsync(
                            "javierarangoaristizabal@gmail.com", // Aquí iría el email real del usuario
                            "⚠️ ¿Olvidaste fichar la salida?",
                            $"<h1>Detectamos una entrada sin salida</h1><p>Tu última entrada fue el {evento.FechaHora.ToLocalTime()}. Por favor, revisa tus resgitros.</p>"
                        );
                    }
                }
            }
        }
    }
}
