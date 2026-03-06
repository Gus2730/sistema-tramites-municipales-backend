using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SistemaTramites.Data;
using SistemaTramites.DTOs;

namespace SistemaTramites.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class PermissionsController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public PermissionsController(ApplicationDbContext context)
        {
            _context = context;
        }

        [HttpGet]
        public async Task<ActionResult> GetAll()
        {
            try
            {
                var permissions = await _context.Permissions
                    .OrderBy(p => p.Modulo)
                    .ThenBy(p => p.Descripcion)
                    .Select(p => new PermissionDto
                    {
                        Id = p.Id,
                        Descripcion = p.Descripcion,
                        Modulo = p.Modulo,
                        Notas = p.Notas,
                        FechaCreacion = p.FechaCreacion
                    })
                    .ToListAsync();

                return Ok(new { message = "Permisos obtenidos exitosamente", data = permissions });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Error interno del servidor", error = ex.Message });
            }
        }

        [HttpGet("by-module")]
        public async Task<ActionResult> GetByModule()
        {
            try
            {
                var permissionsByModule = await _context.Permissions
                    .GroupBy(p => p.Modulo)
                    .Select(g => new
                    {
                        Modulo = g.Key,
                        Permisos = g.Select(p => new PermissionDto
                        {
                            Id = p.Id,
                            Descripcion = p.Descripcion,
                            Modulo = p.Modulo,
                            Notas = p.Notas,
                            FechaCreacion = p.FechaCreacion
                        }).ToList()
                    })
                    .ToListAsync();

                return Ok(new { message = "Permisos agrupados por módulo obtenidos exitosamente", data = permissionsByModule });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Error interno del servidor", error = ex.Message });
            }
        }
    }

    public class PermissionDto
    {
        public string Id { get; set; } = string.Empty;
        public string Descripcion { get; set; } = string.Empty;
        public string Modulo { get; set; } = string.Empty;
        public string? Notas { get; set; }
        public DateTime FechaCreacion { get; set; }
    }
}
