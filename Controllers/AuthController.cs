using Microsoft.AspNetCore.Mvc;
using SistemaTramites.DTOs;
using SistemaTramites.Services;

namespace SistemaTramites.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AuthController : ControllerBase
    {
        private readonly IAuthService _authService;

        public AuthController(IAuthService authService)
        {
            _authService = authService;
        }

        [HttpPost("login")]
        public async Task<ActionResult<LoginResponseDto>> Login([FromBody] LoginDto loginDto)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(new { message = "Datos de entrada inválidos", errors = ModelState });
                }

                var result = await _authService.LoginAsync(loginDto);
                
                if (result == null)
                {
                    return Unauthorized(new { message = "Credenciales inválidas. Verifique su cédula y contraseña." });
                }

                return Ok(new { message = "Inicio de sesión exitoso", data = result });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Error interno del servidor", error = ex.Message });
            }
        }

        [HttpPost("validate-token")]
        public async Task<ActionResult> ValidateToken([FromBody] string token)
        {
            try
            {
                var isValid = await _authService.ValidateTokenAsync(token);
                
                if (!isValid)
                {
                    return Unauthorized(new { message = "Token inválido o expirado" });
                }

                var user = await _authService.GetUserFromTokenAsync(token);
                
                return Ok(new { message = "Token válido", data = user });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Error interno del servidor", error = ex.Message });
            }
        }
    }
}
