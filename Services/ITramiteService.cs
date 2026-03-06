using SistemaTramites.DTOs;
using SistemaTramites.Models;

namespace SistemaTramites.Services
{
    public interface ITramiteService
    {
        Task<TramiteDto?> GetByIdAsync(int id, string userCedula);
        Task<List<TramiteDto>> GetAllAsync(string userCedula, int? departmentId = null);
        Task<List<TramiteDto>> GetByClienteAsync(string clienteCedula);
        Task<TramiteDto> CreateAsync(CreateTramiteDto createDto, string userCedula);
        Task<TramiteDto?> UpdateAsync(int id, UpdateTramiteDto updateDto, string userCedula);
        Task<bool> DeleteAsync(int id, string userCedula);
        Task<TramiteDto?> CambiarEstadoAsync(int id, EstadoTramite nuevoEstado, string userCedula, string? observaciones = null);
        Task<bool> VerificarRequisitoAsync(int tramiteId, int requisitoId, bool presentado, string userCedula, string? observaciones = null);
    }
}
