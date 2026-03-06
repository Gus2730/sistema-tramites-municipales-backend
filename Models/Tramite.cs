using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SistemaTramites.Models
{
    public class Tramite
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [StringLength(100)]
        public string Tipo { get; set; } = string.Empty;

        [Required]
        public EstadoTramite Estado { get; set; } = EstadoTramite.Registrado;

        public DateTime FechaRegistro { get; set; } = DateTime.UtcNow;
        public DateTime? FechaInicio { get; set; }
        public DateTime? FechaFinalizacion { get; set; }
        public DateTime? FechaEntrega { get; set; }

        [StringLength(1000)]
        public string? NotasInternas { get; set; }

        [StringLength(1000)]
        public string? NotasPublicas { get; set; }

        [Required]
        [StringLength(10)]
        public string ClienteCedula { get; set; } = string.Empty;

        [Required]
        [StringLength(100)]
        public string ClienteNombre { get; set; } = string.Empty;

        [StringLength(50)]
        public string? ClienteEmail { get; set; }

        [StringLength(20)]
        public string? ClienteTelefono { get; set; }

        // Foreign Keys
        [Required]
        public int DepartmentId { get; set; }

        [Required]
        [StringLength(10)]
        public string UsuarioRegistroCedula { get; set; } = string.Empty;

        [StringLength(10)]
        public string? UsuarioAsignadoCedula { get; set; }

        // Navigation Properties
        [ForeignKey("DepartmentId")]
        public virtual Department Department { get; set; } = null!;

        [ForeignKey("UsuarioRegistroCedula")]
        public virtual User UsuarioRegistro { get; set; } = null!;

        [ForeignKey("UsuarioAsignadoCedula")]
        public virtual User? UsuarioAsignado { get; set; }

        public virtual ICollection<TramiteRequisito> TramiteRequisitos { get; set; } = new List<TramiteRequisito>();
        public virtual ICollection<TramiteArchivo> TramiteArchivos { get; set; } = new List<TramiteArchivo>();
        public virtual ICollection<TramiteHistorial> TramiteHistorial { get; set; } = new List<TramiteHistorial>();
    }

    public enum EstadoTramite
    {
        Registrado = 1,
        Iniciado = 2,
        Anulado = 3,
        Finalizado = 4,
        Entregado = 5,
        Calificado = 6
    }
}
