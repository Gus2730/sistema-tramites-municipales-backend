using SistemaTramites.DTOs;

namespace SistemaTramites.Services
{
    public interface IDepartmentService
    {
        Task<List<DepartmentDto>> GetAllAsync();
        Task<DepartmentDto?> GetByIdAsync(int id);
        Task<DepartmentDto> CreateAsync(CreateDepartmentDto createDto, string createdBy);
        Task<DepartmentDto?> UpdateAsync(int id, UpdateDepartmentDto updateDto, string updatedBy);
        Task<bool> DeleteAsync(int id, string deletedBy);
        Task<List<DepartmentDto>> GetActiveAsync();
    }
}
