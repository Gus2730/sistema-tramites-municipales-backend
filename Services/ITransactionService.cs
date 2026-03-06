using SistemaTramites.DTOs;

namespace SistemaTramites.Services
{
    public interface ITransactionService
    {
        Task LogTransactionAsync(string userCedula, string accion, string modulo, string? entidadAfectada = null, string? detalles = null);
        Task<List<TransactionDto>> GetTransactionsAsync(DateTime? fechaInicio = null, DateTime? fechaFin = null, string? userCedula = null, string? modulo = null);
        Task<List<AlertaFavoritismoDto>> GetAlertasFavoritismoAsync();
    }

    public class TransactionDto
    {
        public int Id { get; set; }
        public UserDto User { get; set; } = null!;
        public string Accion { get; set; } = string.Empty;
        public string Modulo { get; set; } = string.Empty;
        public string? EntidadAfectada { get; set; }
        public string? IdEntidadAfectada { get; set; }
        public string? Detalles { get; set; }
        public DateTime FechaHora { get; set; }
        public string? DireccionIP { get; set; }
        public string? UserAgent { get; set; }
    }

    public class AlertaFavoritismoDto
    {
        public string UsuarioCedula { get; set; } = string.Empty;
        public string UsuarioNombre { get; set; } = string.Empty;
        public int CantidadTramitesFueraOrden { get; set; }
        public DateTime FechaDeteccion { get; set; }
        public string Descripcion { get; set; } = string.Empty;
    }
}
