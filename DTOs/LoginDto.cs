using System.ComponentModel.DataAnnotations;

namespace SistemaTramites.DTOs
{
    public class LoginDto
    {
        [Required(ErrorMessage = "La cédula es requerida")]
        [StringLength(10, ErrorMessage = "La cédula debe tener máximo 10 caracteres")]
        public string Cedula { get; set; } = string.Empty;

        [Required(ErrorMessage = "La contraseña es requerida")]
        [StringLength(50, MinimumLength = 6, ErrorMessage = "La contraseña debe tener entre 6 y 50 caracteres")]
        public string Contrasena { get; set; } = string.Empty;
    }

    public class LoginResponseDto
    {
        public string Token { get; set; } = string.Empty;
        public UserDto Usuario { get; set; } = null!;
        public List<string> Permisos { get; set; } = new();
        public List<DepartmentDto> Departamentos { get; set; } = new();
    }
}
