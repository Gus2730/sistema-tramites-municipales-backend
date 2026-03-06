using Microsoft.EntityFrameworkCore;
using SistemaTramites.Data;
using SistemaTramites.DTOs;
using SistemaTramites.Models;

namespace SistemaTramites.Services
{
    public class UserService : IUserService
    {
        private readonly ApplicationDbContext _context;
        private readonly ITransactionService _transactionService;

        public UserService(ApplicationDbContext context, ITransactionService transactionService)
        {
            _context = context;
            _transactionService = transactionService;
        }

        public async Task<List<UserDto>> GetAllAsync()
        {
            var users = await _context.Users
                .Include(u => u.UserPermissions)
                    .ThenInclude(up => up.Permission)
                .Include(u => u.UserDepartments)
                    .ThenInclude(ud => ud.Department)
                .ToListAsync();

            return users.Select(MapToDto).ToList();
        }

        public async Task<UserDto?> GetByIdAsync(string cedula)
        {
            var user = await _context.Users
                .Include(u => u.UserPermissions)
                    .ThenInclude(up => up.Permission)
                .Include(u => u.UserDepartments)
                    .ThenInclude(ud => ud.Department)
                .FirstOrDefaultAsync(u => u.Cedula == cedula);

            return user != null ? MapToDto(user) : null;
        }

        public async Task<UserDto> CreateAsync(CreateUserDto createDto, string createdBy)
        {
            // Verificar si ya existe un usuario con esa cédula
            if (await _context.Users.AnyAsync(u => u.Cedula == createDto.Cedula))
            {
                throw new ArgumentException("Ya existe un usuario con esa cédula");
            }

            // Verificar si ya existe un usuario con ese email
            if (await _context.Users.AnyAsync(u => u.CorreoElectronico == createDto.CorreoElectronico))
            {
                throw new ArgumentException("Ya existe un usuario con ese correo electrónico");
            }

            var user = new User
            {
                Cedula = createDto.Cedula,
                NombreCompleto = createDto.NombreCompleto,
                CorreoElectronico = createDto.CorreoElectronico,
                ContrasenaEncriptada = BCrypt.Net.BCrypt.HashPassword(createDto.Contrasena),
                Estado = EstadoUsuario.Activo
            };

            _context.Users.Add(user);
            await _context.SaveChangesAsync();

            // Asignar permisos
            foreach (var permisoId in createDto.Permisos)
            {
                var userPermission = new UserPermission
                {
                    UserCedula = user.Cedula,
                    PermissionId = permisoId,
                    AsignadoPorCedula = createdBy
                };
                _context.UserPermissions.Add(userPermission);
            }

            // Asignar departamentos
            foreach (var departmentId in createDto.DepartamentosIds)
            {
                var userDepartment = new UserDepartment
                {
                    UserCedula = user.Cedula,
                    DepartmentId = departmentId,
                    AsignadoPorCedula = createdBy
                };
                _context.UserDepartments.Add(userDepartment);
            }

            await _context.SaveChangesAsync();

            // Registrar transacción
            await _transactionService.LogTransactionAsync(createdBy, "Crear", "Usuarios", user.Cedula, 
                $"Usuario creado: {user.NombreCompleto}");

            return await GetByIdAsync(user.Cedula) ?? throw new InvalidOperationException("Error al crear el usuario");
        }

        public async Task<UserDto?> UpdateAsync(string cedula, UpdateUserDto updateDto, string updatedBy)
        {
            var user = await _context.Users
                .Include(u => u.UserPermissions)
                .Include(u => u.UserDepartments)
                .FirstOrDefaultAsync(u => u.Cedula == cedula);

            if (user == null) return null;

            // Verificar email único (excluyendo el usuario actual)
            if (await _context.Users.AnyAsync(u => u.CorreoElectronico == updateDto.CorreoElectronico && u.Cedula != cedula))
            {
                throw new ArgumentException("Ya existe otro usuario con ese correo electrónico");
            }

            user.NombreCompleto = updateDto.NombreCompleto;
            user.CorreoElectronico = updateDto.CorreoElectronico;
            user.Estado = updateDto.Estado;
            user.FechaUltimaModificacion = DateTime.UtcNow;

            // Actualizar permisos
            _context.UserPermissions.RemoveRange(user.UserPermissions);
            foreach (var permisoId in updateDto.Permisos)
            {
                var userPermission = new UserPermission
                {
                    UserCedula = user.Cedula,
                    PermissionId = permisoId,
                    AsignadoPorCedula = updatedBy
                };
                _context.UserPermissions.Add(userPermission);
            }

            // Actualizar departamentos
            _context.UserDepartments.RemoveRange(user.UserDepartments);
            foreach (var departmentId in updateDto.DepartamentosIds)
            {
                var userDepartment = new UserDepartment
                {
                    UserCedula = user.Cedula,
                    DepartmentId = departmentId,
                    AsignadoPorCedula = updatedBy
                };
                _context.UserDepartments.Add(userDepartment);
            }

            await _context.SaveChangesAsync();

            // Registrar transacción
            await _transactionService.LogTransactionAsync(updatedBy, "Actualizar", "Usuarios", user.Cedula, 
                $"Usuario actualizado: {user.NombreCompleto}");

            return await GetByIdAsync(cedula);
        }

        public async Task<bool> DeleteAsync(string cedula, string deletedBy)
        {
            var user = await _context.Users.FindAsync(cedula);
            if (user == null) return false;

            // En lugar de eliminar, cambiar estado a inactivo
            user.Estado = EstadoUsuario.Inactivo;
            user.FechaUltimaModificacion = DateTime.UtcNow;

            await _context.SaveChangesAsync();

            // Registrar transacción
            await _transactionService.LogTransactionAsync(deletedBy, "Inactivar", "Usuarios", user.Cedula, 
                $"Usuario inactivado: {user.NombreCompleto}");

            return true;
        }

        public async Task<bool> ChangePasswordAsync(string cedula, string newPassword, string changedBy)
        {
            var user = await _context.Users.FindAsync(cedula);
            if (user == null) return false;

            user.ContrasenaEncriptada = BCrypt.Net.BCrypt.HashPassword(newPassword);
            user.FechaUltimaModificacion = DateTime.UtcNow;

            await _context.SaveChangesAsync();

            // Registrar transacción
            await _transactionService.LogTransactionAsync(changedBy, "CambiarContrasena", "Usuarios", user.Cedula, 
                $"Contraseña cambiada para: {user.NombreCompleto}");

            return true;
        }

        public async Task<List<UserDto>> GetByDepartmentAsync(int departmentId)
        {
            var users = await _context.Users
                .Include(u => u.UserPermissions)
                    .ThenInclude(up => up.Permission)
                .Include(u => u.UserDepartments)
                    .ThenInclude(ud => ud.Department)
                .Where(u => u.UserDepartments.Any(ud => ud.DepartmentId == departmentId))
                .ToListAsync();

            return users.Select(MapToDto).ToList();
        }

        private UserDto MapToDto(User user)
        {
            return new UserDto
            {
                Cedula = user.Cedula,
                NombreCompleto = user.NombreCompleto,
                CorreoElectronico = user.CorreoElectronico,
                FechaCreacion = user.FechaCreacion,
                FechaUltimaModificacion = user.FechaUltimaModificacion,
                Estado = user.Estado,
                Permisos = user.UserPermissions.Select(up => up.Permission.Id).ToList(),
                Departamentos = user.UserDepartments.Select(ud => new DepartmentDto
                {
                    Id = ud.Department.Id,
                    Nombre = ud.Department.Nombre,
                    Descripcion = ud.Department.Descripcion,
                    FechaCreacion = ud.Department.FechaCreacion,
                    FechaUltimaModificacion = ud.Department.FechaUltimaModificacion,
                    Estado = ud.Department.Estado
                }).ToList()
            };
        }
    }
}
