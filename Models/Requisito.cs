using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SistemaTramites.Models
{
    public class Requisito
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [StringLength(200)]
        public string Nombre { get; set; } = string.Empty;

        [StringLength(1000)]
        public string? Descripcion { get; set; }

        [Required]
        public bool Obligatorio { get; set; } = true;

        [Required]
        public int TipoTramiteId { get; set; }

        public DateTime FechaCreacion { get; set; } = DateTime.UtcNow;

        [Required]
        public EstadoRequisito Estado { get; set; } = EstadoRequisito.Activo;

        // Navigation Properties
        [ForeignKey("TipoTramiteId")]
        public virtual TipoTramite TipoTramite { get; set; } = null!;

        public virtual ICollection<TramiteRequisito> TramiteRequisitos { get; set; } = new List<TramiteRequisito>();
    }

    public enum EstadoRequisito
    {
        Activo = 1,
        Inactivo = 0
    }
}
