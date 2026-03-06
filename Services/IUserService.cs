using SistemaTramites.DTOs;

namespace SistemaTramites.Services
{
    public interface IUserService
    {
        Task<List<UserDto>> GetAllAsync();
        Task<UserDto?> GetByIdAsync(string cedula);
        Task<UserDto> CreateAsync(CreateUserDto createDto, string createdBy);
        Task<UserDto?> UpdateAsync(string cedula, UpdateUserDto updateDto, string updatedBy);
        Task<bool> DeleteAsync(string cedula, string deletedBy);
        Task<bool> ChangePasswordAsync(string cedula, string newPassword, string changedBy);
        Task<List<UserDto>> GetByDepartmentAsync(int departmentId);
    }
}
