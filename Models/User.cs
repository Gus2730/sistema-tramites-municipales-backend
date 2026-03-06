using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SistemaTramites.Models
{
    public class User
    {
        [Key]
        [StringLength(10)]
        public string Cedula { get; set; } = string.Empty;

        [Required]
        [StringLength(50)]
        public string NombreCompleto { get; set; } = string.Empty;

        [Required]
        [StringLength(50)]
        [EmailAddress]
        public string CorreoElectronico { get; set; } = string.Empty;

        [Required]
        [StringLength(255)]
        public string ContrasenaEncriptada { get; set; } = string.Empty;

        public DateTime FechaCreacion { get; set; } = DateTime.UtcNow;
        public DateTime FechaUltimaModificacion { get; set; } = DateTime.UtcNow;

        [Required]
        public EstadoUsuario Estado { get; set; } = EstadoUsuario.Activo;

        // Relaciones
        public virtual ICollection<UserDepartment> UserDepartments { get; set; } = new List<UserDepartment>();
        public virtual ICollection<UserPermission> UserPermissions { get; set; } = new List<UserPermission>();
        public virtual ICollection<Transaction> Transactions { get; set; } = new List<Transaction>();
    }

    public enum EstadoUsuario
    {
        Activo = 1,
        Inactivo = 0
    }
}
