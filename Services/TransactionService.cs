using Microsoft.EntityFrameworkCore;
using SistemaTramites.Data;
using SistemaTramites.DTOs;
using SistemaTramites.Models;

namespace SistemaTramites.Services
{
    public class TransactionService : ITransactionService
    {
        private readonly ApplicationDbContext _context;
        private readonly IHttpContextAccessor _httpContextAccessor;

        public TransactionService(ApplicationDbContext context, IHttpContextAccessor httpContextAccessor)
        {
            _context = context;
            _httpContextAccessor = httpContextAccessor;
        }

        public async Task LogTransactionAsync(string userCedula, string accion, string modulo, string? entidadAfectada = null, string? detalles = null)
        {
            var httpContext = _httpContextAccessor.HttpContext;
            
            var transaction = new Transaction
            {
                UserCedula = userCedula,
                Accion = accion,
                Modulo = modulo,
                EntidadAfectada = entidadAfectada,
                IdEntidadAfectada = entidadAfectada,
                Detalles = detalles,
                DireccionIP = httpContext?.Connection?.RemoteIpAddress?.ToString(),
                UserAgent = httpContext?.Request?.Headers["User-Agent"].ToString()
            };

            _context.Transactions.Add(transaction);
            await _context.SaveChangesAsync();
        }

        public async Task<List<TransactionDto>> GetTransactionsAsync(DateTime? fechaInicio = null, DateTime? fechaFin = null, string? userCedula = null, string? modulo = null)
        {
            var query = _context.Transactions
                .Include(t => t.User)
                .AsQueryable();

            if (fechaInicio.HasValue)
                query = query.Where(t => t.FechaHora >= fechaInicio.Value);

            if (fechaFin.HasValue)
                query = query.Where(t => t.FechaHora <= fechaFin.Value);

            if (!string.IsNullOrEmpty(userCedula))
                query = query.Where(t => t.UserCedula == userCedula);

            if (!string.IsNullOrEmpty(modulo))
                query = query.Where(t => t.Modulo == modulo);

            var transactions = await query
                .OrderByDescending(t => t.FechaHora)
                .Take(1000) // Limitar resultados
                .ToListAsync();

            return transactions.Select(t => new TransactionDto
            {
                Id = t.Id,
                User = new UserDto
                {
                    Cedula = t.User.Cedula,
                    NombreCompleto = t.User.NombreCompleto,
                    CorreoElectronico = t.User.CorreoElectronico,
                    Estado = t.User.Estado
                },
                Accion = t.Accion,
                Modulo = t.Modulo,
                EntidadAfectada = t.EntidadAfectada,
                IdEntidadAfectada = t.IdEntidadAfectada,
                Detalles = t.Detalles,
                FechaHora = t.FechaHora,
                DireccionIP = t.DireccionIP,
                UserAgent = t.UserAgent
            }).ToList();
        }

        public async Task<List<AlertaFavoritismoDto>> GetAlertasFavoritismoAsync()
        {
            // Detectar posibles casos de favoritismo analizando el orden de atención de trámites
            var alertas = new List<AlertaFavoritismoDto>();

            // Obtener trámites finalizados en los últimos 30 días
            var fechaLimite = DateTime.UtcNow.AddDays(-30);
            var tramitesRecientes = await _context.Tramites
                .Include(t => t.UsuarioAsignado)
                .Where(t => t.Estado == EstadoTramite.Finalizado && 
                           t.FechaFinalizacion >= fechaLimite &&
                           t.UsuarioAsignadoCedula != null)
                .OrderBy(t => t.FechaRegistro)
                .ToListAsync();

            // Agrupar por usuario asignado
            var tramitesPorUsuario = tramitesRecientes.GroupBy(t => t.UsuarioAsignadoCedula);

            foreach (var grupo in tramitesPorUsuario)
            {
                var tramitesUsuario = grupo.OrderBy(t => t.FechaRegistro).ToList();
                var tramitesFueraOrden = 0;

                for (int i = 1; i < tramitesUsuario.Count; i++)
                {
                    // Si un trámite registrado después se finalizó antes, podría ser favoritismo
                    if (tramitesUsuario[i].FechaRegistro > tramitesUsuario[i-1].FechaRegistro &&
                        tramitesUsuario[i].FechaFinalizacion < tramitesUsuario[i-1].FechaFinalizacion)
                    {
                        tramitesFueraOrden++;
                    }
                }

                // Si hay más de 3 casos sospechosos, generar alerta
                if (tramitesFueraOrden > 3)
                {
                    var usuario = tramitesUsuario.First().UsuarioAsignado;
                    alertas.Add(new AlertaFavoritismoDto
                    {
                        UsuarioCedula = grupo.Key!,
                        UsuarioNombre = usuario?.NombreCompleto ?? "Usuario no encontrado",
                        CantidadTramitesFueraOrden = tramitesFueraOrden,
                        FechaDeteccion = DateTime.UtcNow,
                        Descripcion = $"Se detectaron {tramitesFueraOrden} trámites atendidos fuera del orden de registro en los últimos 30 días"
                    });
                }
            }

            return alertas;
        }
    }
}
