using System.ComponentModel.DataAnnotations;

namespace SistemaTramites.Models
{
    public class Permission
    {
        [Key]
        [StringLength(10)]
        public string Id { get; set; } = string.Empty;

        [Required]
        [StringLength(100)]
        public string Descripcion { get; set; } = string.Empty;

        [StringLength(500)]
        public string? Notas { get; set; }

        [Required]
        [StringLength(50)]
        public string Modulo { get; set; } = string.Empty;

        public DateTime FechaCreacion { get; set; } = DateTime.UtcNow;

        // Relaciones
        public virtual ICollection<UserPermission> UserPermissions { get; set; } = new List<UserPermission>();
    }
}
