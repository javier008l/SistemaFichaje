using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SistemaFichaje.Models
{
    // Tipos de acciones posibles
    public enum TipoFichaje
    {
        Entrada = 1,
        Salida = 2,
        InicioPausa = 3,
        FinPausa = 4
    }

    public class FichajeEvento
    {
        [Key]
        public int Id { get; set; }

        // ID del usuario que viene del sistema externo (CRM, ERP, etc.)
        [Required]
        public Guid UsuarioExternoId { get; set; }

        // Qué hizo: Entrar, Salir...
        [Required]
        public TipoFichaje Tipo { get; set; }

        // Cuándo lo hizo (Con zona horaria para cumplir la ley)
        [Required]
        public DateTimeOffset FechaHora { get; set; }

        // Metadatos técnicos (IP, Navegador) para auditoría
        [MaxLength(500)]
        public string? MetadatosDispositivo { get; set; }
        
        // Coordenadas opcionales
        [MaxLength(100)]
        public string? Geolocalizacion { get; set; }

        // Auditoría interna: ¿Fue una corrección manual?
        public bool EsCorreccionManual { get; set; } = false;
        
        [MaxLength(250)]
        public string? MotivoCorreccion { get; set; }
    }
}
