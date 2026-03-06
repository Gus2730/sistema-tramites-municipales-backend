using Microsoft.EntityFrameworkCore;
using SistemaTramites.Data;
using SistemaTramites.DTOs;
using SistemaTramites.Models;

namespace SistemaTramites.Services
{
    public class DepartmentService : IDepartmentService
    {
        private readonly ApplicationDbContext _context;
        private readonly ITransactionService _transactionService;

        public DepartmentService(ApplicationDbContext context, ITransactionService transactionService)
        {
            _context = context;
            _transactionService = transactionService;
        }

        public async Task<List<DepartmentDto>> GetAllAsync()
        {
            var departments = await _context.Departments.ToListAsync();
            return departments.Select(MapToDto).ToList();
        }

        public async Task<DepartmentDto?> GetByIdAsync(int id)
        {
            var department = await _context.Departments.FindAsync(id);
            return department != null ? MapToDto(department) : null;
        }

        public async Task<DepartmentDto> CreateAsync(CreateDepartmentDto createDto, string createdBy)
        {
            // Verificar si ya existe un departamento con ese nombre
            if (await _context.Departments.AnyAsync(d => d.Nombre == createDto.Nombre))
            {
                throw new ArgumentException("Ya existe un departamento con ese nombre");
            }

            var department = new Department
            {
                Nombre = createDto.Nombre,
                Descripcion = createDto.Descripcion,
                Estado = EstadoDepartamento.Activo
            };

            _context.Departments.Add(department);
            await _context.SaveChangesAsync();

            // Registrar transacción
            await _transactionService.LogTransactionAsync(createdBy, "Crear", "Departamentos", department.Id.ToString(), 
                $"Departamento creado: {department.Nombre}");

            return MapToDto(department);
        }

        public async Task<DepartmentDto?> UpdateAsync(int id, UpdateDepartmentDto updateDto, string updatedBy)
        {
            var department = await _context.Departments.FindAsync(id);
            if (department == null) return null;

            // Verificar nombre único (excluyendo el departamento actual)
            if (await _context.Departments.AnyAsync(d => d.Nombre == updateDto.Nombre && d.Id != id))
            {
                throw new ArgumentException("Ya existe otro departamento con ese nombre");
            }

            department.Nombre = updateDto.Nombre;
            department.Descripcion = updateDto.Descripcion;
            department.Estado = updateDto.Estado;
            department.FechaUltimaModificacion = DateTime.UtcNow;

            await _context.SaveChangesAsync();

            // Registrar transacción
            await _transactionService.LogTransactionAsync(updatedBy, "Actualizar", "Departamentos", department.Id.ToString(), 
                $"Departamento actualizado: {department.Nombre}");

            return MapToDto(department);
        }

        public async Task<bool> DeleteAsync(int id, string deletedBy)
        {
            var department = await _context.Departments.FindAsync(id);
            if (department == null) return false;

            // Verificar si tiene usuarios asignados
            var hasUsers = await _context.UserDepartments.AnyAsync(ud => ud.DepartmentId == id);
            if (hasUsers)
            {
                throw new InvalidOperationException("No se puede eliminar un departamento que tiene usuarios asignados");
            }

            // Verificar si tiene trámites
            var hasTramites = await _context.Tramites.AnyAsync(t => t.DepartmentId == id);
            if (hasTramites)
            {
                throw new InvalidOperationException("No se puede eliminar un departamento que tiene trámites asociados");
            }

            // En lugar de eliminar, cambiar estado a inactivo
            department.Estado = EstadoDepartamento.Inactivo;
            department.FechaUltimaModificacion = DateTime.UtcNow;

            await _context.SaveChangesAsync();

            // Registrar transacción
            await _transactionService.LogTransactionAsync(deletedBy, "Inactivar", "Departamentos", department.Id.ToString(), 
                $"Departamento inactivado: {department.Nombre}");

            return true;
        }

        public async Task<List<DepartmentDto>> GetActiveAsync()
        {
            var departments = await _context.Departments
                .Where(d => d.Estado == EstadoDepartamento.Activo)
                .ToListAsync();
            
            return departments.Select(MapToDto).ToList();
        }

        private DepartmentDto MapToDto(Department department)
        {
            return new DepartmentDto
            {
                Id = department.Id,
                Nombre = department.Nombre,
                Descripcion = department.Descripcion,
                FechaCreacion = department.FechaCreacion,
                FechaUltimaModificacion = department.FechaUltimaModificacion,
                Estado = department.Estado
            };
        }
    }
}
