using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SistemaTramites.Models
{
    public class TramiteRequisito
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public int TramiteId { get; set; }

        [Required]
        public int RequisitoId { get; set; }

        [Required]
        public bool Presentado { get; set; } = false;

        public DateTime? FechaPresentacion { get; set; }

        [StringLength(500)]
        public string? Observaciones { get; set; }

        [StringLength(10)]
        public string? VerificadoPorCedula { get; set; }

        public DateTime? FechaVerificacion { get; set; }

        // Navigation Properties
        [ForeignKey("TramiteId")]
        public virtual Tramite Tramite { get; set; } = null!;

        [ForeignKey("RequisitoId")]
        public virtual Requisito Requisito { get; set; } = null!;

        [ForeignKey("VerificadoPorCedula")]
        public virtual User? VerificadoPor { get; set; }
    }
}
