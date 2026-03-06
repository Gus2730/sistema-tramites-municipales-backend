using System.ComponentModel.DataAnnotations;
using SistemaTramites.Models;

namespace SistemaTramites.DTOs
{
    public class DepartmentDto
    {
        public int Id { get; set; }
        public string Nombre { get; set; } = string.Empty;
        public string? Descripcion { get; set; }
        public DateTime FechaCreacion { get; set; }
        public DateTime FechaUltimaModificacion { get; set; }
        public EstadoDepartamento Estado { get; set; }
    }

    public class CreateDepartmentDto
    {
        [Required(ErrorMessage = "El nombre del departamento es requerido")]
        [StringLength(100, ErrorMessage = "El nombre debe tener máximo 100 caracteres")]
        public string Nombre { get; set; } = string.Empty;

        [StringLength(500, ErrorMessage = "La descripción debe tener máximo 500 caracteres")]
        public string? Descripcion { get; set; }
    }

    public class UpdateDepartmentDto
    {
        [Required(ErrorMessage = "El nombre del departamento es requerido")]
        [StringLength(100, ErrorMessage = "El nombre debe tener máximo 100 caracteres")]
        public string Nombre { get; set; } = string.Empty;

        [StringLength(500, ErrorMessage = "La descripción debe tener máximo 500 caracteres")]
        public string? Descripcion { get; set; }

        public EstadoDepartamento Estado { get; set; }
    }
}
