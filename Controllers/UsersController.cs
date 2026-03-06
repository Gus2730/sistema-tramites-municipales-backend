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
    public class UsersController : ControllerBase
    {
        private readonly IUserService _userService;

        public UsersController(IUserService userService)
        {
            _userService = userService;
        }

        private string GetCurrentUserCedula()
        {
            return User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "";
        }

        [HttpGet]
        public async Task<ActionResult<List<UserDto>>> GetAll()
        {
            try
            {
                var users = await _userService.GetAllAsync();
                return Ok(new { message = "Usuarios obtenidos exitosamente", data = users });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Error interno del servidor", error = ex.Message });
            }
        }

        [HttpGet("{cedula}")]
        public async Task<ActionResult<UserDto>> GetById(string cedula)
        {
            try
            {
                var user = await _userService.GetByIdAsync(cedula);
                
                if (user == null)
                {
                    return NotFound(new { message = "Usuario no encontrado" });
                }

                return Ok(new { message = "Usuario obtenido exitosamente", data = user });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Error interno del servidor", error = ex.Message });
            }
        }

        [HttpGet("department/{departmentId}")]
        public async Task<ActionResult<List<UserDto>>> GetByDepartment(int departmentId)
        {
            try
            {
                var users = await _userService.GetByDepartmentAsync(departmentId);
                return Ok(new { message = "Usuarios del departamento obtenidos exitosamente", data = users });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Error interno del servidor", error = ex.Message });
            }
        }

        [HttpPost]
        public async Task<ActionResult<UserDto>> Create([FromBody] CreateUserDto createDto)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(new { message = "Datos de entrada inválidos", errors = ModelState });
                }

                var currentUserCedula = GetCurrentUserCedula();
                var user = await _userService.CreateAsync(createDto, currentUserCedula);
                
                return CreatedAtAction(nameof(GetById), new { cedula = user.Cedula }, 
                    new { message = "Usuario creado exitosamente", data = user });
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

        [HttpPut("{cedula}")]
        public async Task<ActionResult<UserDto>> Update(string cedula, [FromBody] UpdateUserDto updateDto)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(new { message = "Datos de entrada inválidos", errors = ModelState });
                }

                var currentUserCedula = GetCurrentUserCedula();
                var user = await _userService.UpdateAsync(cedula, updateDto, currentUserCedula);
                
                if (user == null)
                {
                    return NotFound(new { message = "Usuario no encontrado" });
                }

                return Ok(new { message = "Usuario actualizado exitosamente", data = user });
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

        [HttpDelete("{cedula}")]
        public async Task<ActionResult> Delete(string cedula)
        {
            try
            {
                var currentUserCedula = GetCurrentUserCedula();
                var result = await _userService.DeleteAsync(cedula, currentUserCedula);
                
                if (!result)
                {
                    return NotFound(new { message = "Usuario no encontrado" });
                }

                return Ok(new { message = "Usuario inactivado exitosamente" });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Error interno del servidor", error = ex.Message });
            }
        }

        [HttpPatch("{cedula}/change-password")]
        public async Task<ActionResult> ChangePassword(string cedula, [FromBody] ChangePasswordRequest request)
        {
            try
            {
                if (string.IsNullOrEmpty(request.NewPassword) || request.NewPassword.Length < 6)
                {
                    return BadRequest(new { message = "La contraseña debe tener al menos 6 caracteres" });
                }

                var currentUserCedula = GetCurrentUserCedula();
                var result = await _userService.ChangePasswordAsync(cedula, request.NewPassword, currentUserCedula);
                
                if (!result)
                {
                    return NotFound(new { message = "Usuario no encontrado" });
                }

                return Ok(new { message = "Contraseña cambiada exitosamente" });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Error interno del servidor", error = ex.Message });
            }
        }
    }

    public class ChangePasswordRequest
    {
        public string NewPassword { get; set; } = string.Empty;
    }
}
