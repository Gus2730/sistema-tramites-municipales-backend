using Microsoft.EntityFrameworkCore;
using SistemaTramites.Models;

namespace SistemaTramites.Data
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options)
        {
        }

        public DbSet<User> Users { get; set; }
        public DbSet<Department> Departments { get; set; }
        public DbSet<Permission> Permissions { get; set; }
        public DbSet<UserPermission> UserPermissions { get; set; }
        public DbSet<UserDepartment> UserDepartments { get; set; }
        public DbSet<Tramite> Tramites { get; set; }
        public DbSet<Transaction> Transactions { get; set; }
        public DbSet<TramiteHistorial> TramiteHistorial { get; set; }

        // Agregar las nuevas entidades al DbContext
        public DbSet<TipoTramite> TipoTramites { get; set; }
        public DbSet<Requisito> Requisitos { get; set; }
        public DbSet<TramiteRequisito> TramiteRequisitos { get; set; }
        public DbSet<TramiteArchivo> TramiteArchivos { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Configuraciones de entidades
            modelBuilder.Entity<User>(entity =>
            {
                entity.HasKey(e => e.Cedula);
                entity.HasIndex(e => e.CorreoElectronico).IsUnique();
            });

            modelBuilder.Entity<UserPermission>(entity =>
            {
                entity.HasIndex(e => new { e.UserCedula, e.PermissionId }).IsUnique();
            });

            modelBuilder.Entity<UserDepartment>(entity =>
            {
                entity.HasIndex(e => new { e.UserCedula, e.DepartmentId }).IsUnique();
            });

            // Configurar relaciones para evitar ciclos de cascada
            modelBuilder.Entity<UserPermission>()
                .HasOne(up => up.AsignadoPor)
                .WithMany()
                .HasForeignKey(up => up.AsignadoPorCedula)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<UserDepartment>()
                .HasOne(ud => ud.AsignadoPor)
                .WithMany()
                .HasForeignKey(ud => ud.AsignadoPorCedula)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Tramite>()
                .HasOne(t => t.UsuarioAsignado)
                .WithMany()
                .HasForeignKey(t => t.UsuarioAsignadoCedula)
                .OnDelete(DeleteBehavior.Restrict);

            // Configuraciones adicionales para las nuevas entidades
            modelBuilder.Entity<Tramite>(entity =>
            {
                entity.Property(e => e.Tipo).HasMaxLength(100);
                entity.HasIndex(e => e.ClienteCedula);
                entity.HasIndex(e => e.FechaRegistro);
            });

            modelBuilder.Entity<TipoTramite>(entity =>
            {
                entity.HasIndex(e => e.Nombre);
                entity.Property(e => e.Costo).HasColumnType("decimal(10,2)");
            });

            modelBuilder.Entity<TramiteRequisito>(entity =>
            {
                entity.HasIndex(e => new { e.TramiteId, e.RequisitoId }).IsUnique();
            });

            // Datos iniciales
            SeedData(modelBuilder);

            // Datos iniciales adicionales
            SeedAdditionalData(modelBuilder);
        }

        private void SeedData(ModelBuilder modelBuilder)
        {
            // Permisos iniciales
            var permissions = new[]
            {
                new Permission { Id = "USU1", Descripcion = "Crear usuario", Modulo = "Usuarios" },
                new Permission { Id = "USU2", Descripcion = "Modificar usuario", Modulo = "Usuarios" },
                new Permission { Id = "USU3", Descripcion = "Inactivar usuario", Modulo = "Usuarios" },
                new Permission { Id = "USU4", Descripcion = "Consultar usuario", Modulo = "Usuarios" },
                new Permission { Id = "DEP1", Descripcion = "Crear Departamento", Modulo = "Departamentos" },
                new Permission { Id = "DEP2", Descripcion = "Modificar Departamento", Modulo = "Departamentos" },
                new Permission { Id = "DEP3", Descripcion = "Inactivar Departamento", Modulo = "Departamentos" },
                new Permission { Id = "DEP4", Descripcion = "Consultar Departamento", Modulo = "Departamentos" },
                new Permission { Id = "TRA1", Descripcion = "Registrar Trámite", Modulo = "Tramites" },
                new Permission { Id = "TRA2", Descripción = "Modificar Tramite", Modulo = "Tramites" },
                new Permission { Id = "TRA3", Descripcion = "Inactivar Tramite", Modulo = "Tramites" },
                new Permission { Id = "TRA4", Descripcion = "Finalizar Tramite", Modulo = "Tramites" },
                new Permission { Id = "TRA5", Descripcion = "Consultar Tramite", Modulo = "Tramites" },
                new Permission { Id = "TRA6", Descripcion = "Consultar Todos los Tramites", Modulo = "Tramites" },
                new Permission { Id = "REP1", Descripcion = "Consultar reporte de trámites por usuarios", Modulo = "Reportes" },
                new Permission { Id = "REP2", Descripcion = "Consultar reporte de trámites por fechas", Modulo = "Reportes" },
                new Permission { Id = "REP3", Descripcion = "Consultar reporte de trámites por tipos", Modulo = "Reportes" },
                new Permission { Id = "REP4", Descripcion = "Consultar reporte de trámites por estados", Modulo = "Reportes" },
                new Permission { Id = "REP5", Descripcion = "Consultar reporte de trámites por departamentos", Modulo = "Reportes" },
                new Permission { Id = "REP6", Descripcion = "Consultar reporte de trámites por cliente", Modulo = "Reportes" }
            };

            modelBuilder.Entity<Permission>().HasData(permissions);

            // Departamento inicial
            modelBuilder.Entity<Department>().HasData(
                new Department { Id = 1, Nombre = "Administración", Descripcion = "Departamento administrativo principal" }
            );

            // Usuario administrador inicial
            modelBuilder.Entity<User>().HasData(
                new User
                {
                    Cedula = "1234567890",
                    NombreCompleto = "Administrador Sistema",
                    CorreoElectronico = "admin@municipio.gov",
                    ContrasenaEncriptada = BCrypt.Net.BCrypt.HashPassword("Admin123!"),
                    Estado = EstadoUsuario.Activo
                }
            );
        }

        // Agregar método SeedAdditionalData después del método SeedData existente:
        private void SeedAdditionalData(ModelBuilder modelBuilder)
        {
            // Tipos de trámite iniciales
            modelBuilder.Entity<TipoTramite>().HasData(
                new TipoTramite
                {
                    Id = 1,
                    Nombre = "Licencia de Construcción",
                    Descripcion = "Permiso para construcción de obras civiles",
                    DepartmentId = 1,
                    TiempoEstimadoDias = 15,
                    Costo = 150.00m
                },
                new TipoTramite
                {
                    Id = 2,
                    Nombre = "Certificado de Residencia",
                    Descripcion = "Certificación de domicilio del solicitante",
                    DepartmentId = 1,
                    TiempoEstimadoDias = 3,
                    Costo = 25.00m
                },
                new TipoTramite
                {
                    Id = 3,
                    Nombre = "Permiso Comercial",
                    Descripcion = "Autorización para actividades comerciales",
                    DepartmentId = 1,
                    TiempoEstimadoDias = 10,
                    Costo = 100.00m
                }
            );

            // Requisitos iniciales
            modelBuilder.Entity<Requisito>().HasData(
                // Para Licencia de Construcción
                new Requisito { Id = 1, Nombre = "Planos arquitectónicos", Descripcion = "Planos firmados por arquitecto", TipoTramiteId = 1, Obligatorio = true },
                new Requisito { Id = 2, Nombre = "Escritura del terreno", Descripcion = "Documento que acredite la propiedad", TipoTramiteId = 1, Obligatorio = true },
                new Requisito { Id = 3, Nombre = "Estudio de suelos", Descripcion = "Análisis técnico del terreno", TipoTramiteId = 1, Obligatorio = false },

                // Para Certificado de Residencia
                new Requisito { Id = 4, Nombre = "Cédula de identidad", Descripcion = "Documento de identificación vigente", TipoTramiteId = 2, Obligatorio = true },
                new Requisito { Id = 5, Nombre = "Recibo de servicios", Descripcion = "Factura de luz, agua o teléfono", TipoTramiteId = 2, Obligatorio = true },

                // Para Permiso Comercial
                new Requisito { Id = 6, Nombre = "RUC o RISE", Descripcion = "Registro único de contribuyentes", TipoTramiteId = 3, Obligatorio = true },
                new Requisito { Id = 7, Nombre = "Permiso de bomberos", Descripcion = "Certificado de seguridad contra incendios", TipoTramiteId = 3, Obligatorio = true },
                new Requisito { Id = 8, Nombre = "Permiso sanitario", Descripcion = "Autorización del ministerio de salud", TipoTramiteId = 3, Obligatorio = false }
            );
        }
    }
}
