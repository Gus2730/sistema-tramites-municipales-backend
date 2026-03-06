using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using SistemaTramites.Data;
using SistemaTramites.DTOs;
using SistemaTramites.Models;

namespace SistemaTramites.Services
{
    public class AuthService : IAuthService
    {
        private readonly ApplicationDbContext _context;
        private readonly IConfiguration _configuration;

        public AuthService(ApplicationDbContext context, IConfiguration configuration)
        {
            _context = context;
            _configuration = configuration;
        }

        public async Task<LoginResponseDto?> LoginAsync(LoginDto loginDto)
        {
            var user = await _context.Users
                .Include(u => u.UserPermissions)
                    .ThenInclude(up => up.Permission)
                .Include(u => u.UserDepartments)
                    .ThenInclude(ud => ud.Department)
                .FirstOrDefaultAsync(u => u.Cedula == loginDto.Cedula && u.Estado == EstadoUsuario.Activo);

            if (user == null || !BCrypt.Net.BCrypt.Verify(loginDto.Contrasena, user.ContrasenaEncriptada))
            {
                return null;
            }

            var userDto = new UserDto
            {
                Cedula = user.Cedula,
                NombreCompleto = user.NombreCompleto,
                CorreoElectronico = user.CorreoElectronico,
                FechaCreacion = user.FechaCreacion,
                FechaUltimaModificacion = user.FechaUltimaModificacion,
                Estado = user.Estado
            };

            var permisos = user.UserPermissions.Select(up => up.Permission.Id).ToList();
            var departamentos = user.UserDepartments.Select(ud => new DepartmentDto
            {
                Id = ud.Department.Id,
                Nombre = ud.Department.Nombre,
                Descripcion = ud.Department.Descripcion,
                FechaCreacion = ud.Department.FechaCreacion,
                FechaUltimaModificacion = ud.Department.FechaUltimaModificacion,
                Estado = ud.Department.Estado
            }).ToList();

            var token = GenerateJwtToken(userDto, permisos);

            return new LoginResponseDto
            {
                Token = token,
                Usuario = userDto,
                Permisos = permisos,
                Departamentos = departamentos
            };
        }

        public string GenerateJwtToken(UserDto user, List<string> permissions)
        {
            var tokenHandler = new JwtSecurityTokenHandler();
            var key = Encoding.ASCII.GetBytes(_configuration["Jwt:Key"] ?? "");
            
            var claims = new List<Claim>
            {
                new(ClaimTypes.NameIdentifier, user.Cedula),
                new(ClaimTypes.Name, user.NombreCompleto),
                new(ClaimTypes.Email, user.CorreoElectronico)
            };

            // Agregar permisos como claims
            claims.AddRange(permissions.Select(permission => new Claim("permission", permission)));

            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(claims),
                Expires = DateTime.UtcNow.AddHours(8),
                SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature),
                Issuer = _configuration["Jwt:Issuer"],
                Audience = _configuration["Jwt:Audience"]
            };

            var token = tokenHandler.CreateToken(tokenDescriptor);
            return tokenHandler.WriteToken(token);
        }

        public async Task<bool> ValidateTokenAsync(string token)
        {
            try
            {
                var tokenHandler = new JwtSecurityTokenHandler();
                var key = Encoding.ASCII.GetBytes(_configuration["Jwt:Key"] ?? "");

                tokenHandler.ValidateToken(token, new TokenValidationParameters
                {
                    ValidateIssuerSigningKey = true,
                    IssuerSigningKey = new SymmetricSecurityKey(key),
                    ValidateIssuer = true,
                    ValidIssuer = _configuration["Jwt:Issuer"],
                    ValidateAudience = true,
                    ValidAudience = _configuration["Jwt:Audience"],
                    ClockSkew = TimeSpan.Zero
                }, out SecurityToken validatedToken);

                return true;
            }
            catch
            {
                return false;
            }
        }

        public async Task<UserDto?> GetUserFromTokenAsync(string token)
        {
            try
            {
                var tokenHandler = new JwtSecurityTokenHandler();
                var jsonToken = tokenHandler.ReadJwtToken(token);
                
                var cedula = jsonToken.Claims.FirstOrDefault(x => x.Type == ClaimTypes.NameIdentifier)?.Value;
                
                if (string.IsNullOrEmpty(cedula))
                    return null;

                var user = await _context.Users
                    .Include(u => u.UserPermissions)
                        .ThenInclude(up => up.Permission)
                    .Include(u => u.UserDepartments)
                        .ThenInclude(ud => ud.Department)
                    .FirstOrDefaultAsync(u => u.Cedula == cedula && u.Estado == EstadoUsuario.Activo);

                if (user == null)
                    return null;

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
            catch
            {
                return null;
            }
        }
    }
}
