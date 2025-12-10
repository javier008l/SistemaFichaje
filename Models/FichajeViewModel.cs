using System;
using System.Collections.Generic;

namespace SistemaFichaje.Models
{
    public class FichajeViewModel
    {
        // ¿Qué está haciendo el usuario ahora mismo?
        public TipoFichaje? UltimoEstado { get; set; }
        
        // Lista de fichajes de HOY para mostrar en la tabla
        public List<FichajeEvento> HistorialHoy { get; set; } = new List<FichajeEvento>();
        
        // Para mostrar mensajes de error o éxito
        public string? Mensaje { get; set; }
    }
}
