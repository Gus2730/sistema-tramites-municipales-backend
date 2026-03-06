using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SistemaTramites.Models
{
    public class UserPermission
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [StringLength(10)]
        public string UserCedula { get; set; } = string.Empty;

        [Required]
        [StringLength(10)]
        public string PermissionId { get; set; } = string.Empty;

        public DateTime FechaAsignacion { get; set; } = DateTime.UtcNow;

        [Required]
        [StringLength(10)]
        public string AsignadoPorCedula { get; set; } = string.Empty;

        // Navigation Properties
        [ForeignKey("UserCedula")]
        public virtual User User { get; set; } = null!;

        [ForeignKey("PermissionId")]
        public virtual Permission Permission { get; set; } = null!;

        [ForeignKey("AsignadoPorCedula")]
        public virtual User AsignadoPor { get; set; } = null!;
    }
}
