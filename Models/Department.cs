using System.ComponentModel.DataAnnotations;

namespace SistemaTramites.Models
{
    public class Department
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [StringLength(100)]
        public string Nombre { get; set; } = string.Empty;

        [StringLength(500)]
        public string? Descripcion { get; set; }

        public DateTime FechaCreacion { get; set; } = DateTime.UtcNow;
        public DateTime FechaUltimaModificacion { get; set; } = DateTime.UtcNow;

        [Required]
        public EstadoDepartamento Estado { get; set; } = EstadoDepartamento.Activo;

        // Relaciones
        public virtual ICollection<UserDepartment> UserDepartments { get; set; } = new List<UserDepartment>();
        public virtual ICollection<Tramite> Tramites { get; set; } = new List<Tramite>();
    }

    public enum EstadoDepartamento
    {
        Activo = 1,
        Inactivo = 0
    }
}
