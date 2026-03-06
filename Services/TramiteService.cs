using Microsoft.EntityFrameworkCore;
using SistemaTramites.Data;
using SistemaTramites.DTOs;
using SistemaTramites.Models;

namespace SistemaTramites.Services
{
    public class TramiteService : ITramiteService
    {
        private readonly ApplicationDbContext _context;
        private readonly ITransactionService _transactionService;

        public TramiteService(ApplicationDbContext context, ITransactionService transactionService)
        {
            _context = context;
            _transactionService = transactionService;
        }

        public async Task<TramiteDto?> GetByIdAsync(int id, string userCedula)
        {
            var user = await _context.Users
                .Include(u => u.UserDepartments)
                .FirstOrDefaultAsync(u => u.Cedula == userCedula);

            if (user == null) return null;

            var query = _context.Tramites
                .Include(t => t.Department)
                .Include(t => t.UsuarioRegistro)
                .Include(t => t.UsuarioAsignado)
                .Include(t => t.TramiteRequisitos)
                    .ThenInclude(tr => tr.Requisito)
                .Include(t => t.TramiteRequisitos)
                    .ThenInclude(tr => tr.VerificadoPor)
                .Include(t => t.TramiteArchivos)
                    .ThenInclude(ta => ta.SubidoPor)
                .AsQueryable();

            // Filtrar por departamentos del usuario si no tiene permiso para ver todos
            var hasPermissionToViewAll = user.UserPermissions.Any(up => up.PermissionId == "TRA6");
            if (!hasPermissionToViewAll)
            {
                var userDepartmentIds = user.UserDepartments.Select(ud => ud.DepartmentId).ToList();
                query = query.Where(t => userDepartmentIds.Contains(t.DepartmentId));
            }

            var tramite = await query.FirstOrDefaultAsync(t => t.Id == id);

            return tramite != null ? MapToDto(tramite) : null;
        }

        public async Task<List<TramiteDto>> GetAllAsync(string userCedula, int? departmentId = null)
        {
            var user = await _context.Users
                .Include(u => u.UserPermissions)
                .Include(u => u.UserDepartments)
                .FirstOrDefaultAsync(u => u.Cedula == userCedula);

            if (user == null) return new List<TramiteDto>();

            var query = _context.Tramites
                .Include(t => t.Department)
                .Include(t => t.UsuarioRegistro)
                .Include(t => t.UsuarioAsignado)
                .Include(t => t.TramiteRequisitos)
                    .ThenInclude(tr => tr.Requisito)
                .Include(t => t.TramiteArchivos)
                .AsQueryable();

            // Filtrar por departamentos del usuario si no tiene permiso para ver todos
            var hasPermissionToViewAll = user.UserPermissions.Any(up => up.PermissionId == "TRA6");
            if (!hasPermissionToViewAll)
            {
                var userDepartmentIds = user.UserDepartments.Select(ud => ud.DepartmentId).ToList();
                query = query.Where(t => userDepartmentIds.Contains(t.DepartmentId));
            }

            if (departmentId.HasValue)
            {
                query = query.Where(t => t.DepartmentId == departmentId.Value);
            }

            var tramites = await query.OrderByDescending(t => t.FechaRegistro).ToListAsync();

            return tramites.Select(MapToDto).ToList();
        }

        public async Task<List<TramiteDto>> GetByClienteAsync(string clienteCedula)
        {
            var tramites = await _context.Tramites
                .Include(t => t.Department)
                .Include(t => t.UsuarioRegistro)
                .Include(t => t.UsuarioAsignado)
                .Include(t => t.TramiteRequisitos)
                    .ThenInclude(tr => tr.Requisito)
                .Where(t => t.ClienteCedula == clienteCedula)
                .OrderByDescending(t => t.FechaRegistro)
                .ToListAsync();

            return tramites.Select(MapToDto).ToList();
        }

        public async Task<TramiteDto> CreateAsync(CreateTramiteDto createDto, string userCedula)
        {
            var tipoTramite = await _context.TipoTramites
                .Include(tt => tt.Requisitos)
                .FirstOrDefaultAsync(tt => tt.Id == createDto.TipoTramiteId);

            if (tipoTramite == null)
                throw new ArgumentException("Tipo de trámite no encontrado");

            var tramite = new Tramite
            {
                Tipo = tipoTramite.Nombre,
                Estado = EstadoTramite.Registrado,
                ClienteCedula = createDto.ClienteCedula,
                ClienteNombre = createDto.ClienteNombre,
                ClienteEmail = createDto.ClienteEmail,
                ClienteTelefono = createDto.ClienteTelefono,
                NotasPublicas = createDto.NotasPublicas,
                DepartmentId = tipoTramite.DepartmentId,
                UsuarioRegistroCedula = userCedula
            };

            _context.Tramites.Add(tramite);
            await _context.SaveChangesAsync();

            // Crear requisitos del trámite
            foreach (var requisito in tipoTramite.Requisitos.Where(r => r.Estado == EstadoRequisito.Activo))
            {
                var tramiteRequisito = new TramiteRequisito
                {
                    TramiteId = tramite.Id,
                    RequisitoId = requisito.Id,
                    Presentado = false
                };
                _context.TramiteRequisitos.Add(tramiteRequisito);
            }

            // Registrar historial
            var historial = new TramiteHistorial
            {
                TramiteId = tramite.Id,
                EstadoAnterior = EstadoTramite.Registrado,
                EstadoNuevo = EstadoTramite.Registrado,
                UsuarioCedula = userCedula,
                Observaciones = "Trámite registrado"
            };
            _context.TramiteHistorial.Add(historial);

            await _context.SaveChangesAsync();

            // Registrar transacción
            await _transactionService.LogTransactionAsync(userCedula, "Crear", "Tramites", tramite.Id.ToString(), 
                $"Creado trámite {tramite.Tipo} para cliente {tramite.ClienteNombre}");

            return await GetByIdAsync(tramite.Id, userCedula) ?? throw new InvalidOperationException("Error al crear el trámite");
        }

        public async Task<TramiteDto?> UpdateAsync(int id, UpdateTramiteDto updateDto, string userCedula)
        {
            var tramite = await _context.Tramites.FindAsync(id);
            if (tramite == null) return null;

            var estadoAnterior = tramite.Estado;

            tramite.NotasInternas = updateDto.NotasInternas;
            tramite.NotasPublicas = updateDto.NotasPublicas;
            tramite.UsuarioAsignadoCedula = updateDto.UsuarioAsignadoCedula;

            if (updateDto.Estado != tramite.Estado)
            {
                tramite.Estado = updateDto.Estado;
                
                // Actualizar fechas según el estado
                switch (updateDto.Estado)
                {
                    case EstadoTramite.Iniciado:
                        tramite.FechaInicio = DateTime.UtcNow;
                        break;
                    case EstadoTramite.Finalizado:
                        tramite.FechaFinalizacion = DateTime.UtcNow;
                        break;
                    case EstadoTramite.Entregado:
                        tramite.FechaEntrega = DateTime.UtcNow;
                        break;
                }

                // Registrar historial
                var historial = new TramiteHistorial
                {
                    TramiteId = tramite.Id,
                    EstadoAnterior = estadoAnterior,
                    EstadoNuevo = updateDto.Estado,
                    UsuarioCedula = userCedula,
                    Observaciones = updateDto.ObservacionesCambioEstado
                };
                _context.TramiteHistorial.Add(historial);
            }

            await _context.SaveChangesAsync();

            // Registrar transacción
            await _transactionService.LogTransactionAsync(userCedula, "Actualizar", "Tramites", tramite.Id.ToString(), 
                $"Actualizado trámite {tramite.Tipo}");

            return await GetByIdAsync(id, userCedula);
        }

        public async Task<bool> DeleteAsync(int id, string userCedula)
        {
            var tramite = await _context.Tramites.FindAsync(id);
            if (tramite == null) return false;

            // En lugar de eliminar, cambiar estado a anulado
            tramite.Estado = EstadoTramite.Anulado;

            // Registrar historial
            var historial = new TramiteHistorial
            {
                TramiteId = tramite.Id,
                EstadoAnterior = tramite.Estado,
                EstadoNuevo = EstadoTramite.Anulado,
                UsuarioCedula = userCedula,
                Observaciones = "Trámite anulado"
            };
            _context.TramiteHistorial.Add(historial);

            await _context.SaveChangesAsync();

            // Registrar transacción
            await _transactionService.LogTransactionAsync(userCedula, "Anular", "Tramites", tramite.Id.ToString(), 
                $"Anulado trámite {tramite.Tipo}");

            return true;
        }

        public async Task<TramiteDto?> CambiarEstadoAsync(int id, EstadoTramite nuevoEstado, string userCedula, string? observaciones = null)
        {
            var tramite = await _context.Tramites.FindAsync(id);
            if (tramite == null) return null;

            var estadoAnterior = tramite.Estado;
            tramite.Estado = nuevoEstado;

            // Actualizar fechas según el estado
            switch (nuevoEstado)
            {
                case EstadoTramite.Iniciado:
                    tramite.FechaInicio = DateTime.UtcNow;
                    break;
                case EstadoTramite.Finalizado:
                    tramite.FechaFinalizacion = DateTime.UtcNow;
                    break;
                case EstadoTramite.Entregado:
                    tramite.FechaEntrega = DateTime.UtcNow;
                    break;
            }

            // Registrar historial
            var historial = new TramiteHistorial
            {
                TramiteId = tramite.Id,
                EstadoAnterior = estadoAnterior,
                EstadoNuevo = nuevoEstado,
                UsuarioCedula = userCedula,
                Observaciones = observaciones
            };
            _context.TramiteHistorial.Add(historial);

            await _context.SaveChangesAsync();

            // Registrar transacción
            await _transactionService.LogTransactionAsync(userCedula, "CambiarEstado", "Tramites", tramite.Id.ToString(), 
                $"Cambio de estado de {estadoAnterior} a {nuevoEstado}");

            return await GetByIdAsync(id, userCedula);
        }

        public async Task<bool> VerificarRequisitoAsync(int tramiteId, int requisitoId, bool presentado, string userCedula, string? observaciones = null)
        {
            var tramiteRequisito = await _context.TramiteRequisitos
                .FirstOrDefaultAsync(tr => tr.TramiteId == tramiteId && tr.RequisitoId == requisitoId);

            if (tramiteRequisito == null) return false;

            tramiteRequisito.Presentado = presentado;
            tramiteRequisito.FechaPresentacion = presentado ? DateTime.UtcNow : null;
            tramiteRequisito.VerificadoPorCedula = userCedula;
            tramiteRequisito.FechaVerificacion = DateTime.UtcNow;
            tramiteRequisito.Observaciones = observaciones;

            await _context.SaveChangesAsync();

            // Registrar transacción
            await _transactionService.LogTransactionAsync(userCedula, "VerificarRequisito", "TramiteRequisitos", 
                tramiteRequisito.Id.ToString(), $"Requisito {(presentado ? "presentado" : "no presentado")}");

            return true;
        }

        private TramiteDto MapToDto(Tramite tramite)
        {
            return new TramiteDto
            {
                Id = tramite.Id,
                Tipo = tramite.Tipo,
                Estado = tramite.Estado,
                FechaRegistro = tramite.FechaRegistro,
                FechaInicio = tramite.FechaInicio,
                FechaFinalizacion = tramite.FechaFinalizacion,
                FechaEntrega = tramite.FechaEntrega,
                NotasInternas = tramite.NotasInternas,
                NotasPublicas = tramite.NotasPublicas,
                ClienteCedula = tramite.ClienteCedula,
                ClienteNombre = tramite.ClienteNombre,
                ClienteEmail = tramite.ClienteEmail,
                ClienteTelefono = tramite.ClienteTelefono,
                Department = new DepartmentDto
                {
                    Id = tramite.Department.Id,
                    Nombre = tramite.Department.Nombre,
                    Descripcion = tramite.Department.Descripcion,
                    FechaCreacion = tramite.Department.FechaCreacion,
                    FechaUltimaModificacion = tramite.Department.FechaUltimaModificacion,
                    Estado = tramite.Department.Estado
                },
                UsuarioRegistro = new UserDto
                {
                    Cedula = tramite.UsuarioRegistro.Cedula,
                    NombreCompleto = tramite.UsuarioRegistro.NombreCompleto,
                    CorreoElectronico = tramite.UsuarioRegistro.CorreoElectronico,
                    Estado = tramite.UsuarioRegistro.Estado
                },
                UsuarioAsignado = tramite.UsuarioAsignado != null ? new UserDto
                {
                    Cedula = tramite.UsuarioAsignado.Cedula,
                    NombreCompleto = tramite.UsuarioAsignado.NombreCompleto,
                    CorreoElectronico = tramite.UsuarioAsignado.CorreoElectronico,
                    Estado = tramite.UsuarioAsignado.Estado
                } : null,
                Requisitos = tramite.TramiteRequisitos.Select(tr => new TramiteRequisitoDto
                {
                    Id = tr.Id,
                    Requisito = new RequisitoDto
                    {
                        Id = tr.Requisito.Id,
                        Nombre = tr.Requisito.Nombre,
                        Descripcion = tr.Requisito.Descripcion,
                        Obligatorio = tr.Requisito.Obligatorio,
                        Estado = tr.Requisito.Estado
                    },
                    Presentado = tr.Presentado,
                    FechaPresentacion = tr.FechaPresentacion,
                    Observaciones = tr.Observaciones,
                    VerificadoPor = tr.VerificadoPor != null ? new UserDto
                    {
                        Cedula = tr.VerificadoPor.Cedula,
                        NombreCompleto = tr.VerificadoPor.NombreCompleto,
                        CorreoElectronico = tr.VerificadoPor.CorreoElectronico,
                        Estado = tr.VerificadoPor.Estado
                    } : null,
                    FechaVerificacion = tr.FechaVerificacion
                }).ToList(),
                Archivos = tramite.TramiteArchivos.Select(ta => new TramiteArchivoDto
                {
                    Id = ta.Id,
                    NombreArchivo = ta.NombreArchivo,
                    TipoArchivo = ta.TipoArchivo,
                    TamanoBytes = ta.TamanoBytes,
                    Tipo = ta.Tipo,
                    FechaSubida = ta.FechaSubida,
                    SubidoPor = new UserDto
                    {
                        Cedula = ta.SubidoPor.Cedula,
                        NombreCompleto = ta.SubidoPor.NombreCompleto,
                        CorreoElectronico = ta.SubidoPor.CorreoElectronico,
                        Estado = ta.SubidoPor.Estado
                    },
                    Descripcion = ta.Descripcion
                }).ToList()
            };
        }
    }
}
