using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using SistemaTramites.DTOs;
using SistemaTramites.Models;
using SistemaTramites.Services;

namespace SistemaTramites.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class TramitesController : ControllerBase
    {
        private readonly ITramiteService _tramiteService;

        public TramitesController(ITramiteService tramiteService)
        {
            _tramiteService = tramiteService;
        }

        private string GetCurrentUserCedula()
        {
            return User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "";
        }

        [HttpGet]
        public async Task<ActionResult<List<TramiteDto>>> GetAll([FromQuery] int? departmentId = null)
        {
            try
            {
                var userCedula = GetCurrentUserCedula();
                var tramites = await _tramiteService.GetAllAsync(userCedula, departmentId);
                
                return Ok(new { message = "Trámites obtenidos exitosamente", data = tramites });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Error interno del servidor", error = ex.Message });
            }
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<TramiteDto>> GetById(int id)
        {
            try
            {
                var userCedula = GetCurrentUserCedula();
                var tramite = await _tramiteService.GetByIdAsync(id, userCedula);
                
                if (tramite == null)
                {
                    return NotFound(new { message = "Trámite no encontrado" });
                }

                return Ok(new { message = "Trámite obtenido exitosamente", data = tramite });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Error interno del servidor", error = ex.Message });
            }
        }

        [HttpGet("cliente/{clienteCedula}")]
        public async Task<ActionResult<List<TramiteDto>>> GetByCliente(string clienteCedula)
        {
            try
            {
                var tramites = await _tramiteService.GetByClienteAsync(clienteCedula);
                
                return Ok(new { message = "Trámites del cliente obtenidos exitosamente", data = tramites });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Error interno del servidor", error = ex.Message });
            }
        }

        [HttpPost]
        public async Task<ActionResult<TramiteDto>> Create([FromBody] CreateTramiteDto createDto)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(new { message = "Datos de entrada inválidos", errors = ModelState });
                }

                var userCedula = GetCurrentUserCedula();
                var tramite = await _tramiteService.CreateAsync(createDto, userCedula);
                
                return CreatedAtAction(nameof(GetById), new { id = tramite.Id }, 
                    new { message = "Trámite creado exitosamente", data = tramite });
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Error interno del servidor", error = ex.Message });
            }
        }

        [HttpPut("{id}")]
        public async Task<ActionResult<TramiteDto>> Update(int id, [FromBody] UpdateTramiteDto updateDto)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(new { message = "Datos de entrada inválidos", errors = ModelState });
                }

                var userCedula = GetCurrentUserCedula();
                var tramite = await _tramiteService.UpdateAsync(id, updateDto, userCedula);
                
                if (tramite == null)
                {
                    return NotFound(new { message = "Trámite no encontrado" });
                }

                return Ok(new { message = "Trámite actualizado exitosamente", data = tramite });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Error interno del servidor", error = ex.Message });
            }
        }

        [HttpDelete("{id}")]
        public async Task<ActionResult> Delete(int id)
        {
            try
            {
                var userCedula = GetCurrentUserCedula();
                var result = await _tramiteService.DeleteAsync(id, userCedula);
                
                if (!result)
                {
                    return NotFound(new { message = "Trámite no encontrado" });
                }

                return Ok(new { message = "Trámite anulado exitosamente" });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Error interno del servidor", error = ex.Message });
            }
        }

        [HttpPatch("{id}/estado")]
        public async Task<ActionResult<TramiteDto>> CambiarEstado(int id, [FromBody] CambiarEstadoRequest request)
        {
            try
            {
                var userCedula = GetCurrentUserCedula();
                var tramite = await _tramiteService.CambiarEstadoAsync(id, request.NuevoEstado, userCedula, request.Observaciones);
                
                if (tramite == null)
                {
                    return NotFound(new { message = "Trámite no encontrado" });
                }

                return Ok(new { message = "Estado del trámite actualizado exitosamente", data = tramite });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Error interno del servidor", error = ex.Message });
            }
        }

        [HttpPatch("{tramiteId}/requisitos/{requisitoId}/verificar")]
        public async Task<ActionResult> VerificarRequisito(int tramiteId, int requisitoId, [FromBody] VerificarRequisitoRequest request)
        {
            try
            {
                var userCedula = GetCurrentUserCedula();
                var result = await _tramiteService.VerificarRequisitoAsync(tramiteId, requisitoId, request.Presentado, userCedula, request.Observaciones);
                
                if (!result)
                {
                    return NotFound(new { message = "Requisito no encontrado" });
                }

                return Ok(new { message = "Requisito verificado exitosamente" });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Error interno del servidor", error = ex.Message });
            }
        }
    }

    public class CambiarEstadoRequest
    {
        public EstadoTramite NuevoEstado { get; set; }
        public string? Observaciones { get; set; }
    }

    public class VerificarRequisitoRequest
    {
        public bool Presentado { get; set; }
        public string? Observaciones { get; set; }
    }
}
