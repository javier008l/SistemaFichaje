using Microsoft.EntityFrameworkCore;

namespace SistemaFichaje.Models
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
        {
        }

        // Tabla en la base de datos
        public DbSet<FichajeEvento> FichajeEventos { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
            
            // Configuración adicional si fuera necesaria
            // Por ejemplo, creamos un índice para buscar rápido por usuario y fecha
            modelBuilder.Entity<FichajeEvento>()
                .HasIndex(e => new { e.UsuarioExternoId, e.FechaHora });
        }
    }
}
