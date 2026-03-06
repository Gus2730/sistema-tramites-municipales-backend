using System.ComponentModel.DataAnnotations;
using SistemaTramites.Models;

namespace SistemaTramites.DTOs
{
    public class UserDto
    {
        public string Cedula { get; set; } = string.Empty;
        public string NombreCompleto { get; set; } = string.Empty;
        public string CorreoElectronico { get; set; } = string.Empty;
        public DateTime FechaCreacion { get; set; }
        public DateTime FechaUltimaModificacion { get; set; }
        public EstadoUsuario Estado { get; set; }
        public List<string> Permisos { get; set; } = new();
        public List<DepartmentDto> Departamentos { get; set; } = new();
    }

    public class CreateUserDto
    {
        [Required(ErrorMessage = "La cédula es requerida")]
        [StringLength(10, ErrorMessage = "La cédula debe tener máximo 10 caracteres")]
        public string Cedula { get; set; } = string.Empty;

        [Required(ErrorMessage = "El nombre completo es requerido")]
        [StringLength(50, ErrorMessage = "El nombre debe tener máximo 50 caracteres")]
        public string NombreCompleto { get; set; } = string.Empty;

        [Required(ErrorMessage = "El correo electrónico es requerido")]
        [EmailAddress(ErrorMessage = "El formato del correo electrónico no es válido")]
        [StringLength(50, ErrorMessage = "El correo debe tener máximo 50 caracteres")]
        public string CorreoElectronico { get; set; } = string.Empty;

        [Required(ErrorMessage = "La contraseña es requerida")]
        [StringLength(50, MinimumLength = 6, ErrorMessage = "La contraseña debe tener entre 6 y 50 caracteres")]
        public string Contrasena { get; set; } = string.Empty;

        public List<string> Permisos { get; set; } = new();
        public List<int> DepartamentosIds { get; set; } = new();
    }

    public class UpdateUserDto
    {
        [Required(ErrorMessage = "El nombre completo es requerido")]
        [StringLength(50, ErrorMessage = "El nombre debe tener máximo 50 caracteres")]
        public string NombreCompleto { get; set; } = string.Empty;

        [Required(ErrorMessage = "El correo electrónico es requerido")]
        [EmailAddress(ErrorMessage = "El formato del correo electrónico no es válido")]
        [StringLength(50, ErrorMessage = "El correo debe tener máximo 50 caracteres")]
        public string CorreoElectronico { get; set; } = string.Empty;

        public EstadoUsuario Estado { get; set; }
        public List<string> Permisos { get; set; } = new();
        public List<int> DepartamentosIds { get; set; } = new();
    }
}
