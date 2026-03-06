using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using SistemaTramites.DTOs;
using SistemaTramites.Services;

namespace SistemaTramites.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class DepartmentsController : ControllerBase
    {
        private readonly IDepartmentService _departmentService;

        public DepartmentsController(IDepartmentService departmentService)
        {
            _departmentService = departmentService;
        }

        private string GetCurrentUserCedula()
        {
            return User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "";
        }

        [HttpGet]
        public async Task<ActionResult<List<DepartmentDto>>> GetAll()
        {
            try
            {
                var departments = await _departmentService.GetAllAsync();
                return Ok(new { message = "Departamentos obtenidos exitosamente", data = departments });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Error interno del servidor", error = ex.Message });
            }
        }

        [HttpGet("active")]
        public async Task<ActionResult<List<DepartmentDto>>> GetActive()
        {
            try
            {
                var departments = await _departmentService.GetActiveAsync();
                return Ok(new { message = "Departamentos activos obtenidos exitosamente", data = departments });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Error interno del servidor", error = ex.Message });
            }
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<DepartmentDto>> GetById(int id)
        {
            try
            {
                var department = await _departmentService.GetByIdAsync(id);
                
                if (department == null)
                {
                    return NotFound(new { message = "Departamento no encontrado" });
                }

                return Ok(new { message = "Departamento obtenido exitosamente", data = department });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Error interno del servidor", error = ex.Message });
            }
        }

        [HttpPost]
        public async Task<ActionResult<DepartmentDto>> Create([FromBody] CreateDepartmentDto createDto)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(new { message = "Datos de entrada inválidos", errors = ModelState });
                }

                var currentUserCedula = GetCurrentUserCedula();
                var department = await _departmentService.CreateAsync(createDto, currentUserCedula);
                
                return CreatedAtAction(nameof(GetById), new { id = department.Id }, 
                    new { message = "Departamento creado exitosamente", data = department });
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
        public async Task<ActionResult<DepartmentDto>> Update(int id, [FromBody] UpdateDepartmentDto updateDto)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(new { message = "Datos de entrada inválidos", errors = ModelState });
                }

                var currentUserCedula = GetCurrentUserCedula();
                var department = await _departmentService.UpdateAsync(id, updateDto, currentUserCedula);
                
                if (department == null)
                {
                    return NotFound(new { message = "Departamento no encontrado" });
                }

                return Ok(new { message = "Departamento actualizado exitosamente", data = department });
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

        [HttpDelete("{id}")]
        public async Task<ActionResult> Delete(int id)
        {
            try
            {
                var currentUserCedula = GetCurrentUserCedula();
                var result = await _departmentService.DeleteAsync(id, currentUserCedula);
                
                if (!result)
                {
                    return NotFound(new { message = "Departamento no encontrado" });
                }

                return Ok(new { message = "Departamento inactivado exitosamente" });
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Error interno del servidor", error = ex.Message });
            }
        }
    }
}
