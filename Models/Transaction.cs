using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SistemaTramites.Models
{
    public class Transaction
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [StringLength(10)]
        public string UserCedula { get; set; } = string.Empty;

        [Required]
        [StringLength(100)]
        public string Accion { get; set; } = string.Empty;

        [Required]
        [StringLength(100)]
        public string Modulo { get; set; } = string.Empty;

        [StringLength(50)]
        public string? EntidadAfectada { get; set; }

        [StringLength(50)]
        public string? IdEntidadAfectada { get; set; }

        [StringLength(1000)]
        public string? Detalles { get; set; }

        public DateTime FechaHora { get; set; } = DateTime.UtcNow;

        [StringLength(45)]
        public string? DireccionIP { get; set; }

        [StringLength(500)]
        public string? UserAgent { get; set; }

        // Navigation Properties
        [ForeignKey("UserCedula")]
        public virtual User User { get; set; } = null!;
    }
}
