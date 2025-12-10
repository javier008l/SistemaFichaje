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

         // POST: Recibe la acción del botón (Entrar, Salir, Pausa...)
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Fichar(TipoFichaje tipo, string? latitud, string? longitud)
        {
            // 1. Buscamos el estado REAL en la base de datos en este preciso instante
            var ultimoEvento = await _context.FichajeEventos
                .Where(f => f.UsuarioExternoId == _usuarioSimulado)
                .OrderByDescending(f => f.FechaHora)
                .FirstOrDefaultAsync();

            TipoFichaje? estadoActual = ultimoEvento?.Tipo;
            bool esAccionValida = false;

            // 2. VALIDACIÓN ESTRICTA DE TRANSICIONES
            switch (tipo)
            {
                case TipoFichaje.Entrada:
                    // Solo puedes ENTRAR si no hay registros o el último fue SALIDA
                    if (estadoActual == null || estadoActual == TipoFichaje.Salida) 
                        esAccionValida = true;
                    break;

                case TipoFichaje.Salida:
                    // Solo puedes SALIR si estás TRABAJANDO (Entrada o Volver de Pausa)
                    // (Opcional: Si quieres permitir salir estando en pausa, añade '|| estadoActual == TipoFichaje.InicioPausa')
                    if (estadoActual == TipoFichaje.Entrada || estadoActual == TipoFichaje.FinPausa) 
                        esAccionValida = true;
                    break;

                case TipoFichaje.InicioPausa:
                    // Solo puedes PAUSAR si estás TRABAJANDO
                    if (estadoActual == TipoFichaje.Entrada || estadoActual == TipoFichaje.FinPausa) 
                        esAccionValida = true;
                    break;

                case TipoFichaje.FinPausa: // (Volver de pausa)
                    // Solo puedes VOLVER si estabas en PAUSA
                    if (estadoActual == TipoFichaje.InicioPausa) 
                        esAccionValida = true;
                    break;
            }

            // 3. Si la acción no tiene sentido lógico, recargamos la página sin guardar
            if (!esAccionValida)
            {
                // Esto hará que la pestaña "vieja" se actualice y muestre los botones correctos
                return RedirectToAction(nameof(Index)); 
            }

            // --- AQUI EMPIEZA EL GUARDADO (Solo si pasó la validación) ---
            var nuevoFichaje = new FichajeEvento
            {
                Id = Guid.NewGuid(),
                UsuarioExternoId = _usuarioSimulado,
                Tipo = tipo,
                FechaHora = DateTimeOffset.UtcNow,
                Geolocalizacion = (latitud != null && longitud != null) ? $"{latitud},{longitud}" : "No permitida"
            };

            _context.FichajeEventos.Add(nuevoFichaje);
            await _context.SaveChangesAsync();

            // ENVIAR CORREO SI ES SALIDA
            if (tipo == TipoFichaje.Salida)
            {
                string emailDestino = "javierarangoaristizabal@gmail.com";
                string asunto = "✅ Registro de Salida Exitoso";
                string mensaje = $"<h1>Has salido correctamente</h1><p>Hora registrada: {DateTime.Now:HH:mm}</p>";
                
                // Fire & Forget (opcional para que no tarde la carga)
                _ = _emailService.EnviarCorreoAsync(emailDestino, asunto, mensaje);
            }

            return RedirectToAction(nameof(Index));
        }
        // GET: Descargar reporte en CSV
        public async Task<IActionResult> DescargarReporte()
        {
            // 1. Obtenemos TODOS los datos del usuario (o podrías filtrar por mes actual)
            var registros = await _context.FichajeEventos
                .Where(f => f.UsuarioExternoId == _usuarioSimulado)
                .OrderByDescending(f => f.FechaHora)
                .ToListAsync();

            // 2. Construimos el CSV en memoria
            var builder = new System.Text.StringBuilder();

            // Cabecera del Excel
            builder.AppendLine("Fecha,Hora,Tipo Accion,Geolocalizacion,Dispositivo");

            foreach (var item in registros)
            {
                // Convertimos a hora local para que sea legible
                var fechaLocal = item.FechaHora.ToLocalTime();

                // Formato CSV: Valor1,Valor2,Valor3...
                builder.AppendLine($"{fechaLocal:yyyy-MM-dd},{fechaLocal:HH:mm:ss},{item.Tipo},{item.Geolocalizacion},Web");
            }

            // 3. Devolvemos el archivo para descarga inmediata
            return File(
                System.Text.Encoding.UTF8.GetBytes(builder.ToString()),
                "text/csv",
                $"Reporte_Fichaje_{DateTime.Now:yyyyMMdd}.csv"
            );
        }

                // GET: Muestra la pantalla de fichar (con filtro opcional)
        public async Task<IActionResult> Index(DateTime? fechaInicio, DateTime? fechaFin)
        {
            // 1. Estado actual (para saber si pintar verde/rojo)
            var ultimoEvento = await _context.FichajeEventos
                .Where(f => f.UsuarioExternoId == _usuarioSimulado)
                .OrderByDescending(f => f.FechaHora)
                .FirstOrDefaultAsync();

            // 2. Consulta base
            var query = _context.FichajeEventos
                .Where(f => f.UsuarioExternoId == _usuarioSimulado);

            List<FichajeEvento> historial;

            // --- LÓGICA DEL FILTRO ---
            if (fechaInicio.HasValue && fechaFin.HasValue)
            {
                // Rango de fechas
                var inicioUtc = fechaInicio.Value.ToUniversalTime();
                var finUtc = fechaFin.Value.AddDays(1).AddTicks(-1).ToUniversalTime();

                historial = await query
                    .Where(f => f.FechaHora >= inicioUtc && f.FechaHora <= finUtc)
                    .OrderByDescending(f => f.FechaHora) // Ordenamos lo filtrado
                    .ToListAsync();
            }
            else
            {
                // Sin filtro: Traer los 20 MÁS RECIENTES
                // CORRECCIÓN: Primero ordenamos, LUEGO hacemos Take
                historial = await query
                    .OrderByDescending(f => f.FechaHora) // <--- ESTO ES LA CLAVE: Nuevos primero
                    .Take(20)                            // <--- Y de esos nuevos, cogemos 20
                    .ToListAsync();
            }

            var modelo = new FichajeViewModel
            {
                UltimoEstado = ultimoEvento?.Tipo,
                HistorialHoy = historial,
                FechaInicio = fechaInicio ?? DateTime.Today,
                FechaFin = fechaFin ?? DateTime.Today
            };

            return View(modelo);
        }



    }
}
