using System.ComponentModel.DataAnnotations;
using SistemaTramites.Models;

namespace SistemaTramites.DTOs
{
    public class TramiteDto
    {
        public int Id { get; set; }
        public string Tipo { get; set; } = string.Empty;
        public EstadoTramite Estado { get; set; }
        public DateTime FechaRegistro { get; set; }
        public DateTime? FechaInicio { get; set; }
        public DateTime? FechaFinalizacion { get; set; }
        public DateTime? FechaEntrega { get; set; }
        public string? NotasInternas { get; set; }
        public string? NotasPublicas { get; set; }
        public string ClienteCedula { get; set; } = string.Empty;
        public string ClienteNombre { get; set; } = string.Empty;
        public string? ClienteEmail { get; set; }
        public string? ClienteTelefono { get; set; }
        public DepartmentDto Department { get; set; } = null!;
        public UserDto UsuarioRegistro { get; set; } = null!;
        public UserDto? UsuarioAsignado { get; set; }
        public List<TramiteRequisitoDto> Requisitos { get; set; } = new();
        public List<TramiteArchivoDto> Archivos { get; set; } = new();
    }

    public class CreateTramiteDto
    {
        [Required(ErrorMessage = "El tipo de trámite es requerido")]
        public int TipoTramiteId { get; set; }

        [Required(ErrorMessage = "La cédula del cliente es requerida")]
        [StringLength(10, ErrorMessage = "La cédula debe tener máximo 10 caracteres")]
        public string ClienteCedula { get; set; } = string.Empty;

        [Required(ErrorMessage = "El nombre del cliente es requerido")]
        [StringLength(100, ErrorMessage = "El nombre debe tener máximo 100 caracteres")]
        public string ClienteNombre { get; set; } = string.Empty;

        [EmailAddress(ErrorMessage = "El formato del email no es válido")]
        [StringLength(50, ErrorMessage = "El email debe tener máximo 50 caracteres")]
        public string? ClienteEmail { get; set; }

        [StringLength(20, ErrorMessage = "El teléfono debe tener máximo 20 caracteres")]
        public string? ClienteTelefono { get; set; }

        [StringLength(1000, ErrorMessage = "Las notas públicas deben tener máximo 1000 caracteres")]
        public string? NotasPublicas { get; set; }
    }

    public class UpdateTramiteDto
    {
        public EstadoTramite Estado { get; set; }

        [StringLength(1000, ErrorMessage = "Las notas internas deben tener máximo 1000 caracteres")]
        public string? NotasInternas { get; set; }

        [StringLength(1000, ErrorMessage = "Las notas públicas deben tener máximo 1000 caracteres")]
        public string? NotasPublicas { get; set; }

        [StringLength(10, ErrorMessage = "La cédula del usuario asignado debe tener máximo 10 caracteres")]
        public string? UsuarioAsignadoCedula { get; set; }

        [StringLength(500, ErrorMessage = "Las observaciones deben tener máximo 500 caracteres")]
        public string? ObservacionesCambioEstado { get; set; }
    }

    public class TramiteRequisitoDto
    {
        public int Id { get; set; }
        public RequisitoDto Requisito { get; set; } = null!;
        public bool Presentado { get; set; }
        public DateTime? FechaPresentacion { get; set; }
        public string? Observaciones { get; set; }
        public UserDto? VerificadoPor { get; set; }
        public DateTime? FechaVerificacion { get; set; }
    }

    public class TramiteArchivoDto
    {
        public int Id { get; set; }
        public string NombreArchivo { get; set; } = string.Empty;
        public string TipoArchivo { get; set; } = string.Empty;
        public long TamanoBytes { get; set; }
        public TipoArchivoTramite Tipo { get; set; }
        public DateTime FechaSubida { get; set; }
        public UserDto SubidoPor { get; set; } = null!;
        public string? Descripcion { get; set; }
    }

    public class RequisitoDto
    {
        public int Id { get; set; }
        public string Nombre { get; set; } = string.Empty;
        public string? Descripcion { get; set; }
        public bool Obligatorio { get; set; }
        public EstadoRequisito Estado { get; set; }
    }
}
