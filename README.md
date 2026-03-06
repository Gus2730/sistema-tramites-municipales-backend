# Sistema de Trámites Municipales - Backend

## Descripción

Un sistema integral de gestión de permisos y procedimientos administrativos municipales diseñado para manejar solicitudes de ciudadanos, rastrear el procesamiento de documentos, gestionar departamentos, usuarios y generar reportes. Digitaliza y administra diversos procedimientos municipales (licencias, certificados, permisos, etc.).

## Tecnologías Utilizadas

**Framework y Lenguaje:**
- **.NET 8.0** (C#) - Framework LTS más reciente
- **ASP.NET Core** API Web

**Base de Datos y ORM:**
- **MySQL 8.0** - Base de datos relacional
- **Entity Framework Core 8.0** - ORM para operaciones de base de datos
- **Pomelo.EntityFrameworkCore.MySql** - Proveedor MySQL para EF Core

**Autenticación y Seguridad:**
- **JWT (JSON Web Tokens)** para autenticación sin estado
- **BCrypt.Net-Next** para hash de contraseñas
- **Microsoft.AspNetCore.Authentication.JwtBearer** para validación JWT

**Documentación de API:**
- **Swagger/Swashbuckle 6.5.0** - Documentación OpenAPI y explorador interactivo de API

**Adicionales:**
- **CORS** habilitado para frontend Angular (localhost:4200)
- Inyección de dependencias y arquitectura orientada a servicios

## Instalación y Configuración

### Prerrequisitos
- .NET 8.0 SDK
- MySQL 8.0 Server
- Git (opcional)

### Pasos de Instalación

1. **Clonar el repositorio:**
   ```bash
   git clone <url-del-repositorio>
   cd sistema-tramites-municipales-backend
   ```

2. **Instalar dependencias:**
   ```bash
   dotnet restore
   ```

3. **Configurar la base de datos:**
   - Crear una base de datos MySQL llamada `SistemaTramitesMunicipales`
   - Actualizar la cadena de conexión en `appsettings.json` si es necesario

4. **Ejecutar migraciones (si aplica):**
   ```bash
   dotnet ef database update
   ```

5. **Ejecutar la aplicación:**
   ```bash
   dotnet run
   ```

La aplicación estará disponible en `https://localhost:5001` con Swagger en `https://localhost:5001/swagger`.

## Componentes Principales

### 1. Controladores (Endpoints de API)
- **AuthController** - Autenticación (login, validación de token)
- **TramitesController** - Gestión de trámites/permisos (operaciones CRUD)
- **UsersController** - Gestión de usuarios y administración
- **DepartmentsController** - Gestión de departamentos
- **PermissionsController** - Control de acceso basado en roles
- **ReportesController** - Generación de reportes y análisis

### 2. Capa de Servicios
Implementa lógica de negocio con inyección de dependencias:
- `IAuthService` / `AuthService` - Generación de tokens JWT, autenticación de usuarios
- `ITramiteService` / `TramiteService` - Operaciones de trámites
- `IUserService` / `UserService` - Gestión de usuarios
- `IDepartmentService` / `DepartmentService` - Operaciones de departamentos
- `ITransactionService` / `TransactionService` - Seguimiento de transacciones

### 3. Acceso a Datos
- **ApplicationDbContext** - DbContext de Entity Framework con todos los mapeos de entidades
- Semilla de base de datos con datos iniciales (usuario admin, departamentos, permisos, trámites)
- Relaciones de clave foránea apropiadas con restricciones de eliminación en cascada

## Esquema de Base de Datos

### Entidades Principales:

1. **User** (Clave Primaria: Cedula)
   - Cedula, NombreCompleto, CorreoElectronico, ContrasenaEncriptada
   - Estado (Activo/Inactivo), FechaCreacion, FechaUltimaModificacion

2. **Department**
   - Id, Nombre, Descripcion, Estado
   - Relaciones: UserDepartments, Tramites

3. **Tramite** (Trámite/Permiso)
   - Id, Tipo, Estado (Registrado, Iniciado, Anulado, Finalizado, Entregado, Calificado)
   - ClienteCedula, ClienteNombre, ClienteEmail, ClienteTelefono
   - DepartmentId, UsuarioRegistroCedula, UsuarioAsignadoCedula
   - FechaRegistro, FechaInicio, FechaFinalizacion, FechaEntrega
   - NotasInternas, NotasPublicas
   - Relaciones: Requisitos, Archivos, Historial

4. **TipoTramite** (Tipo de Trámite)
   - Id, Nombre, Descripcion, DepartmentId
   - TiempoEstimadoDias, Costo (decimal)
   - Relaciones: Requisitos, Tramites

5. **Requisito** (Requisito)
   - Id, Nombre, Descripcion, Obligatorio
   - TipoTramiteId, Estado
   - Tipos incorporados incluyen: Licencia de Construcción, Certificado de Residencia, Permiso Comercial

6. **Tablas de Soporte:**
   - **TramiteRequisito** - Unión entre Tramites y Requisitos
   - **TramiteArchivo** - Documentos adjuntos para trámites
   - **TramiteHistorial** - Registro de auditoría/historial de cambios
   - **Permission** - Permisos del sistema/ACL (prefijos USU*, DEP*, TRA*, REP*)
   - **UserPermission** - Asignaciones usuario-permiso
   - **UserDepartment** - Asignaciones usuario-departamento
   - **Transaction** - Registro de transacciones

## Endpoints de API

### Autenticación:
- `POST /api/auth/login` - Inicio de sesión de usuario (retorna token JWT)
- `POST /api/auth/validate-token` - Validación de token

### Trámites:
- `GET /api/tramites` - Listar todos los trámites (con filtro opcional de departamento)
- `GET /api/tramites/{id}` - Obtener detalles del trámite
- `GET /api/tramites/cliente/{clienteCedula}` - Obtener trámites por cliente
- `POST /api/tramites` - Crear nuevo trámite
- `PUT /api/tramites/{id}` - Actualizar trámite
- `DELETE /api/tramites/{id}` - Eliminar trámite

### Usuarios:
- `GET /api/users` - Listar todos los usuarios
- `GET /api/users/{cedula}` - Obtener detalles del usuario
- `GET /api/users/department/{departmentId}` - Obtener usuarios por departamento
- `POST /api/users` - Crear usuario
- `PUT /api/users/{cedula}` - Actualizar usuario

### Departamentos:
- `GET /api/departments` - Listar departamentos
- `GET /api/departments/{id}` - Obtener detalles del departamento
- `POST /api/departments` - Crear departamento
- `PUT /api/departments/{id}` - Actualizar departamento

### Permisos:
- `GET /api/permissions` - Listar todos los permisos
- `POST /api/permissions` - Asignar permiso

### Reportes:
- `GET /api/reportes/tramites-por-usuario` - Trámites por usuario (con filtrado por rango de fechas)
- Endpoints adicionales de reportes para análisis por fecha, tipo, estado, departamento y cliente

## Objetos de Transferencia de Datos (DTOs)

- **LoginDto** - Credenciales (cedula, password)
- **LoginResponseDto** - Respuesta con token, usuario, permisos, departamentos
- **UserDto** - Información del usuario
- **TramiteDto** - Detalles del trámite
- **CreateTramiteDto** - Creación de trámite
- **DepartmentDto** - Información del departamento
- **TipoTramiteDto** - Información del tipo de trámite

## Configuración (appsettings.json)

- Base de datos: MySQL en localhost (SistemaTramitesMunicipales)
- Clave secreta JWT: "SistemaTramitesMunicipales2024SecretKeyForJWTTokenGeneration"
- Emisor/Audiencia JWT: "SistemaTramitesMunicipales"
- CORS: Permite solicitudes desde localhost:4200 (aplicación Angular)

## Características Destacadas

1. **Autenticación basada en JWT** - API sin estado con autenticación de token Bearer
2. **Control de Acceso Basado en Roles (RBAC)** - Sistema de permisos con 16+ permisos predefinidos en módulos
3. **Gestión de Tipos de Trámites** - Tres tipos de trámites semilla (Licencia de Construcción, Certificado de Residencia, Permiso Comercial)
4. **Gestión de Requisitos** - Definir requisitos obligatorios/opcionales por tipo de trámite
5. **Registro de Auditoría Integral** - Seguimiento del historial vía TramiteHistorial
6. **Gestión de Documentos** - Soporte para adjuntos en trámites
7. **Soporte Multi-Departamento** - Organizar trámites en departamentos
8. **Reportes y Análisis** - Reportes incorporados agrupados por usuario, fecha, tipo, estado, departamento y cliente
9. **Registro de Transacciones** - Seguimiento de transacciones del sistema
10. **Semilla de Datos** - Pre-poblado con usuario admin y datos principales
11. **Documentación de API** - Integración Swagger/OpenAPI para pruebas interactivas de API

## Cuenta de Administrador por Defecto
- **Cédula:** 1234567890
- **Correo:** admin@municipio.gov
- **Contraseña:** Admin123! (hasheada con BCrypt)

## Licencia

Este proyecto está bajo la Licencia MIT - ver el archivo [LICENSE](LICENSE) para más detalles.

---

Este es un sistema de administración municipal completamente funcional construido con tecnologías .NET modernas, diseñado para escalabilidad y mantenibilidad con separación apropiada de responsabilidades a través de arquitectura orientada a servicios.