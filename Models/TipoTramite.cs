using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SistemaTramites.Models
{
    public class TipoTramite
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [StringLength(100)]
        public string Nombre { get; set; } = string.Empty;

        [StringLength(500)]
        public string? Descripcion { get; set; }

        [Required]
        public int DepartmentId { get; set; }

        [Required]
        public int TiempoEstimadoDias { get; set; } = 5;

        [Column(TypeName = "decimal(10,2)")]
        public decimal? Costo { get; set; }

        public DateTime FechaCreacion { get; set; } = DateTime.UtcNow;
        public DateTime FechaUltimaModificacion { get; set; } = DateTime.UtcNow;

        [Required]
        public EstadoTipoTramite Estado { get; set; } = EstadoTipoTramite.Activo;

        // Navigation Properties
        [ForeignKey("DepartmentId")]
        public virtual Department Department { get; set; } = null!;

        public virtual ICollection<Requisito> Requisitos { get; set; } = new List<Requisito>();
        public virtual ICollection<Tramite> Tramites { get; set; } = new List<Tramite>();
    }

    public enum EstadoTipoTramite
    {
        Activo = 1,
        Inactivo = 0
    }
}
