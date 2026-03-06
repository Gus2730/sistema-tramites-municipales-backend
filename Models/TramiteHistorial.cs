using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SistemaTramites.Models
{
    public class TramiteHistorial
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public int TramiteId { get; set; }

        [Required]
        public EstadoTramite EstadoAnterior { get; set; }

        [Required]
        public EstadoTramite EstadoNuevo { get; set; }

        public DateTime FechaCambio { get; set; } = DateTime.UtcNow;

        [Required]
        [StringLength(10)]
        public string UsuarioCedula { get; set; } = string.Empty;

        [StringLength(500)]
        public string? Observaciones { get; set; }

        // Navigation Properties
        [ForeignKey("TramiteId")]
        public virtual Tramite Tramite { get; set; } = null!;

        [ForeignKey("UsuarioCedula")]
        public virtual User Usuario { get; set; } = null!;
    }
}
