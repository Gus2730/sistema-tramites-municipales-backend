using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SistemaTramites.Models
{
    public class TramiteArchivo
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public int TramiteId { get; set; }

        [Required]
        [StringLength(255)]
        public string NombreArchivo { get; set; } = string.Empty;

        [Required]
        [StringLength(500)]
        public string RutaArchivo { get; set; } = string.Empty;

        [Required]
        [StringLength(100)]
        public string TipoArchivo { get; set; } = string.Empty;

        [Required]
        public long TamanoBytes { get; set; }

        [Required]
        public TipoArchivoTramite Tipo { get; set; }

        public DateTime FechaSubida { get; set; } = DateTime.UtcNow;

        [Required]
        [StringLength(10)]
        public string SubidoPorCedula { get; set; } = string.Empty;

        [StringLength(500)]
        public string? Descripcion { get; set; }

        // Navigation Properties
        [ForeignKey("TramiteId")]
        public virtual Tramite Tramite { get; set; } = null!;

        [ForeignKey("SubidoPorCedula")]
        public virtual User SubidoPor { get; set; } = null!;
    }

    public enum TipoArchivoTramite
    {
        Requisito = 1,
        Generado = 2,
        Adicional = 3
    }
}
