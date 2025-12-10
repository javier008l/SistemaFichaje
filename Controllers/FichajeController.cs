using Microsoft.AspNetCore.Mvc;
using SistemaFichaje.Models;
using Microsoft.EntityFrameworkCore;
using SistemaFichaje.Services;


namespace SistemaFichaje.Controllers
{
    public class FichajeController : Controller
    {
        private readonly AppDbContext _context;
        private readonly IEmailService _emailService;

        // SIMULACIÓN: En tu CRM real, esto vendría de User.Identity.GetUserId()
        // Usamos un GUID fijo para que puedas probarlo ya mismo sin loguearte
        private readonly Guid _usuarioSimulado = Guid.Parse("aaaa1111-bb22-cc33-dd44-eeee55556666");

        public FichajeController(AppDbContext context, IEmailService emailService)
        {
            _context = context;
            _emailService = emailService;
        }

        // GET: Muestra la pantalla de fichar
        public async Task<IActionResult> Index()
        {
            // 1. Buscamos el ÚLTIMO evento de este usuario para saber su estado
            var ultimoEvento = await _context.FichajeEventos
                .Where(f => f.UsuarioExternoId == _usuarioSimulado)
                .OrderByDescending(f => f.FechaHora)
                .FirstOrDefaultAsync();

            // 2. Buscamos el historial ultimos 20 eventos
            var historial = await _context.FichajeEventos
                .Where(f => f.UsuarioExternoId == _usuarioSimulado)
                .OrderByDescending(f => f.FechaHora)
                .Take(20) // Limitamos a 20 para no sobrecargar
                .ToListAsync();

            var modelo = new FichajeViewModel
            {
                UltimoEstado = ultimoEvento?.Tipo,
                HistorialHoy = historial
            };

            return View(modelo);
        }

        // POST: Recibe la acción del botón (Entrar, Salir, Pausa...)
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Fichar(TipoFichaje tipo, string? latitud, string? longitud)
        {

            // --- VALIDACIÓN NUEVA ---
            // Verificamos el último estado real en BD antes de guardar nada
            var ultimoEvento = await _context.FichajeEventos
                .Where(f => f.UsuarioExternoId == _usuarioSimulado)
                .OrderByDescending(f => f.FechaHora)
                .FirstOrDefaultAsync();

            TipoFichaje? estadoActual = ultimoEvento?.Tipo;

            // Reglas de negocio básicas:
            // No puedes ENTRAR si ya estás DENTRO
            if (tipo == TipoFichaje.Entrada && (estadoActual == TipoFichaje.Entrada || estadoActual == TipoFichaje.FinPausa))
                return RedirectToAction(nameof(Index)); // Ignoramos el clic

            // No puedes SALIR si ya estás FUERA
            if (tipo == TipoFichaje.Salida && (estadoActual == null || estadoActual == TipoFichaje.Salida))
                return RedirectToAction(nameof(Index));

            // Creamos el registro inmutable
            var nuevoFichaje = new FichajeEvento
            {
                Id = Guid.NewGuid(),
                UsuarioExternoId = _usuarioSimulado,
                Tipo = tipo,
                FechaHora = DateTimeOffset.UtcNow, // SIEMPRE UTC en servidor
                Geolocalizacion = (latitud != null && longitud != null) ? $"{latitud},{longitud}" : "No permitida"
            };

            _context.FichajeEventos.Add(nuevoFichaje);
            await _context.SaveChangesAsync();

            // ENVIAR CORREO SI ES SALIDA ---
            if (tipo == TipoFichaje.Salida)
            {
                // para la prueba uso mi correo
                string emailDestino = "javierarangoaristizabal@gmail.com";

                string asunto = "✅ Registro de Salida Exitoso";
                string mensaje = $"<h1>Has salido correctamente</h1><p>Hora registrada: {DateTime.Now:HH:mm}</p>";
                await _emailService.EnviarCorreoAsync(emailDestino, asunto, mensaje);
            }

            return RedirectToAction(nameof(Index));
        }
    }
}
