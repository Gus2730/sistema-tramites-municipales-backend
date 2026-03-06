using System.ComponentModel.DataAnnotations;
using SistemaTramites.Models;

namespace SistemaTramites.DTOs
{
    public class TipoTramiteDto
    {
        public int Id { get; set; }
        public string Nombre { get; set; } = string.Empty;
        public string? Descripcion { get; set; }
        public DepartmentDto Department { get; set; } = null!;
        public int TiempoEstimadoDias { get; set; }
        public decimal? Costo { get; set; }
        public DateTime FechaCreacion { get; set; }
        public DateTime FechaUltimaModificacion { get; set; }
        public EstadoTipoTramite Estado { get; set; }
        public List<RequisitoDto> Requisitos { get; set; } = new();
    }

    public class CreateTipoTramiteDto
    {
        [Required(ErrorMessage = "El nombre del tipo de trámite es requerido")]
        [StringLength(100, ErrorMessage = "El nombre debe tener máximo 100 caracteres")]
        public string Nombre { get; set; } = string.Empty;

        [StringLength(500, ErrorMessage = "La descripción debe tener máximo 500 caracteres")]
        public string? Descripcion { get; set; }

        [Required(ErrorMessage = "El departamento es requerido")]
        public int DepartmentId { get; set; }

        [Required(ErrorMessage = "El tiempo estimado es requerido")]
        [Range(1, 365, ErrorMessage = "El tiempo estimado debe estar entre 1 y 365 días")]
        public int TiempoEstimadoDias { get; set; } = 5;

        [Range(0, 999999.99, ErrorMessage = "El costo debe ser un valor positivo")]
        public decimal? Costo { get; set; }

        public List<CreateRequisitoDto> Requisitos { get; set; } = new();
    }

    public class UpdateTipoTramiteDto
    {
        [Required(ErrorMessage = "El nombre del tipo de trámite es requerido")]
        [StringLength(100, ErrorMessage = "El nombre debe tener máximo 100 caracteres")]
        public string Nombre { get; set; } = string.Empty;

        [StringLength(500, ErrorMessage = "La descripción debe tener máximo 500 caracteres")]
        public string? Descripcion { get; set; }

        [Required(ErrorMessage = "El tiempo estimado es requerido")]
        [Range(1, 365, ErrorMessage = "El tiempo estimado debe estar entre 1 y 365 días")]
        public int TiempoEstimadoDias { get; set; }

        [Range(0, 999999.99, ErrorMessage = "El costo debe ser un valor positivo")]
        public decimal? Costo { get; set; }

        public EstadoTipoTramite Estado { get; set; }
    }

    public class CreateRequisitoDto
    {
        [Required(ErrorMessage = "El nombre del requisito es requerido")]
        [StringLength(200, ErrorMessage = "El nombre debe tener máximo 200 caracteres")]
        public string Nombre { get; set; } = string.Empty;

        [StringLength(1000, ErrorMessage = "La descripción debe tener máximo 1000 caracteres")]
        public string? Descripcion { get; set; }

        public bool Obligatorio { get; set; } = true;
    }
}
