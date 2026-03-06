using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SistemaTramites.Data;
using SistemaTramites.DTOs;
using SistemaTramites.Models;
using SistemaTramites.Services;

namespace SistemaTramites.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class ReportesController : ControllerBase
    {
        private readonly ApplicationDbContext _context;
        private readonly ITransactionService _transactionService;

        public ReportesController(ApplicationDbContext context, ITransactionService transactionService)
        {
            _context = context;
            _transactionService = transactionService;
        }

        [HttpGet("tramites-por-usuario")]
        public async Task<ActionResult> GetTramitesPorUsuario([FromQuery] DateTime? fechaInicio = null, [FromQuery] DateTime? fechaFin = null)
        {
            try
            {
                var query = _context.Tramites
                    .Include(t => t.UsuarioRegistro)
                    .AsQueryable();

                if (fechaInicio.HasValue)
                    query = query.Where(t => t.FechaRegistro >= fechaInicio.Value);

                if (fechaFin.HasValue)
                    query = query.Where(t => t.FechaRegistro <= fechaFin.Value);

                var reporte = await query
                    .GroupBy(t => new { t.UsuarioRegistroCedula, t.UsuarioRegistro.NombreCompleto })
                    .Select(g => new
                    {
                        UsuarioCedula = g.Key.UsuarioRegistroCedula,
                        UsuarioNombre = g.Key.NombreCompleto,
                        TotalTramites = g.Count(),
                        TramitesPorEstado = g.GroupBy(t => t.Estado)
                            .Select(sg => new { Estado = sg.Key, Cantidad = sg.Count() })
                            .ToList()
                    })
                    .ToListAsync();

                return Ok(new { message = "Reporte generado exitosamente", data = reporte });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Error interno del servidor", error = ex.Message });
            }
        }

        [HttpGet("tramites-por-fechas")]
        public async Task<ActionResult> GetTramitesPorFechas([FromQuery] DateTime fechaInicio, [FromQuery] DateTime fechaFin)
        {
            try
            {
                var reporte = await _context.Tramites
                    .Where(t => t.FechaRegistro >= fechaInicio && t.FechaRegistro <= fechaFin)
                    .GroupBy(t => t.FechaRegistro.Date)
                    .Select(g => new
                    {
                        Fecha = g.Key,
                        TotalTramites = g.Count(),
                        TramitesPorEstado = g.GroupBy(t => t.Estado)
                            .Select(sg => new { Estado = sg.Key, Cantidad = sg.Count() })
                            .ToList()
                    })
                    .OrderBy(r => r.Fecha)
                    .ToListAsync();

                return Ok(new { message = "Reporte generado exitosamente", data = reporte });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Error interno del servidor", error = ex.Message });
            }
        }

        [HttpGet("tramites-por-tipos")]
        public async Task<ActionResult> GetTramitesPorTipos([FromQuery] DateTime? fechaInicio = null, [FromQuery] DateTime? fechaFin = null)
        {
            try
            {
                var query = _context.Tramites.AsQueryable();

                if (fechaInicio.HasValue)
                    query = query.Where(t => t.FechaRegistro >= fechaInicio.Value);

                if (fechaFin.HasValue)
                    query = query.Where(t => t.FechaRegistro <= fechaFin.Value);

                var reporte = await query
                    .GroupBy(t => t.Tipo)
                    .Select(g => new
                    {
                        TipoTramite = g.Key,
                        TotalTramites = g.Count(),
                        TramitesPorEstado = g.GroupBy(t => t.Estado)
                            .Select(sg => new { Estado = sg.Key, Cantidad = sg.Count() })
                            .ToList(),
                        TiempoPromedioFinalizacion = g.Where(t => t.FechaFinalizacion.HasValue)
                            .Average(t => EF.Functions.DateDiffDay(t.FechaRegistro, t.FechaFinalizacion!.Value))
                    })
                    .ToListAsync();

                return Ok(new { message = "Reporte generado exitosamente", data = reporte });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Error interno del servidor", error = ex.Message });
            }
        }

        [HttpGet("tramites-por-estados")]
        public async Task<ActionResult> GetTramitesPorEstados([FromQuery] DateTime? fechaInicio = null, [FromQuery] DateTime? fechaFin = null)
        {
            try
            {
                var query = _context.Tramites.AsQueryable();

                if (fechaInicio.HasValue)
                    query = query.Where(t => t.FechaRegistro >= fechaInicio.Value);

                if (fechaFin.HasValue)
                    query = query.Where(t => t.FechaRegistro <= fechaFin.Value);

                var reporte = await query
                    .GroupBy(t => t.Estado)
                    .Select(g => new
                    {
                        Estado = g.Key,
                        TotalTramites = g.Count(),
                        Porcentaje = (double)g.Count() / query.Count() * 100
                    })
                    .ToListAsync();

                return Ok(new { message = "Reporte generado exitosamente", data = reporte });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Error interno del servidor", error = ex.Message });
            }
        }

        [HttpGet("tramites-por-departamentos")]
        public async Task<ActionResult> GetTramitesPorDepartamentos([FromQuery] DateTime? fechaInicio = null, [FromQuery] DateTime? fechaFin = null)
        {
            try
            {
                var query = _context.Tramites
                    .Include(t => t.Department)
                    .AsQueryable();

                if (fechaInicio.HasValue)
                    query = query.Where(t => t.FechaRegistro >= fechaInicio.Value);

                if (fechaFin.HasValue)
                    query = query.Where(t => t.FechaRegistro <= fechaFin.Value);

                var reporte = await query
                    .GroupBy(t => new { t.DepartmentId, t.Department.Nombre })
                    .Select(g => new
                    {
                        DepartamentoId = g.Key.DepartmentId,
                        DepartamentoNombre = g.Key.Nombre,
                        TotalTramites = g.Count(),
                        TramitesPorEstado = g.GroupBy(t => t.Estado)
                            .Select(sg => new { Estado = sg.Key, Cantidad = sg.Count() })
                            .ToList(),
                        TiempoPromedioFinalizacion = g.Where(t => t.FechaFinalizacion.HasValue)
                            .Average(t => EF.Functions.DateDiffDay(t.FechaRegistro, t.FechaFinalizacion!.Value))
                    })
                    .ToListAsync();

                return Ok(new { message = "Reporte generado exitosamente", data = reporte });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Error interno del servidor", error = ex.Message });
            }
        }

        [HttpGet("tramites-por-cliente")]
        public async Task<ActionResult> GetTramitesPorCliente([FromQuery] string? clienteCedula = null, [FromQuery] DateTime? fechaInicio = null, [FromQuery] DateTime? fechaFin = null)
        {
            try
            {
                var query = _context.Tramites.AsQueryable();

                if (!string.IsNullOrEmpty(clienteCedula))
                    query = query.Where(t => t.ClienteCedula == clienteCedula);

                if (fechaInicio.HasValue)
                    query = query.Where(t => t.FechaRegistro >= fechaInicio.Value);

                if (fechaFin.HasValue)
                    query = query.Where(t => t.FechaRegistro <= fechaFin.Value);

                var reporte = await query
                    .GroupBy(t => new { t.ClienteCedula, t.ClienteNombre })
                    .Select(g => new
                    {
                        ClienteCedula = g.Key.ClienteCedula,
                        ClienteNombre = g.Key.ClienteNombre,
                        TotalTramites = g.Count(),
                        TramitesPorEstado = g.GroupBy(t => t.Estado)
                            .Select(sg => new { Estado = sg.Key, Cantidad = sg.Count() })
                            .ToList(),
                        TramitesPorTipo = g.GroupBy(t => t.Tipo)
                            .Select(sg => new { Tipo = sg.Key, Cantidad = sg.Count() })
                            .ToList()
                    })
                    .OrderByDescending(r => r.TotalTramites)
                    .ToListAsync();

                return Ok(new { message = "Reporte generado exitosamente", data = reporte });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Error interno del servidor", error = ex.Message });
            }
        }

        [HttpGet("transacciones")]
        public async Task<ActionResult> GetTransacciones([FromQuery] DateTime? fechaInicio = null, [FromQuery] DateTime? fechaFin = null, [FromQuery] string? userCedula = null, [FromQuery] string? modulo = null)
        {
            try
            {
                var transacciones = await _transactionService.GetTransactionsAsync(fechaInicio, fechaFin, userCedula, modulo);
                
                return Ok(new { message = "Transacciones obtenidas exitosamente", data = transacciones });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Error interno del servidor", error = ex.Message });
            }
        }

        [HttpGet("alertas-favoritismo")]
        public async Task<ActionResult> GetAlertasFavoritismo()
        {
            try
            {
                var alertas = await _transactionService.GetAlertasFavoritismoAsync();
                
                return Ok(new { message = "Alertas de favoritismo obtenidas exitosamente", data = alertas });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Error interno del servidor", error = ex.Message });
            }
        }

        [HttpGet("dashboard-metricas")]
        public async Task<ActionResult> GetDashboardMetricas()
        {
            try
            {
                var fechaInicio = DateTime.UtcNow.AddDays(-30);
                
                var metricas = new
                {
                    TramitesTotales = await _context.Tramites.CountAsync(),
                    TramitesPendientes = await _context.Tramites.CountAsync(t => t.Estado == EstadoTramite.Registrado || t.Estado == EstadoTramite.Iniciado),
                    TramitesFinalizados = await _context.Tramites.CountAsync(t => t.Estado == EstadoTramite.Finalizado || t.Estado == EstadoTramite.Entregado),
                    UsuariosActivos = await _context.Users.CountAsync(u => u.Estado == EstadoUsuario.Activo),
                    TramitesPorMes = await _context.Tramites
                        .Where(t => t.FechaRegistro >= fechaInicio)
                        .GroupBy(t => new { t.FechaRegistro.Year, t.FechaRegistro.Month })
                        .Select(g => new
                        {
                            Año = g.Key.Year,
                            Mes = g.Key.Month,
                            Cantidad = g.Count()
                        })
                        .OrderBy(r => r.Año).ThenBy(r => r.Mes)
                        .ToListAsync(),
                    TramitesPorDepartamento = await _context.Tramites
                        .Include(t => t.Department)
                        .GroupBy(t => t.Department.Nombre)
                        .Select(g => new
                        {
                            Departamento = g.Key,
                            Cantidad = g.Count()
                        })
                        .ToListAsync(),
                    TiempoPromedioAtencion = await _context.Tramites
                        .Where(t => t.FechaFinalizacion.HasValue)
                        .AverageAsync(t => EF.Functions.DateDiffDay(t.FechaRegistro, t.FechaFinalizacion!.Value))
                };

                return Ok(new { message = "Métricas del dashboard obtenidas exitosamente", data = metricas });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Error interno del servidor", error = ex.Message });
            }
        }
    }
}
