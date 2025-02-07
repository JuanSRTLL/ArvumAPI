using Microsoft.EntityFrameworkCore;
using ProyectoAPI.Models;

namespace ProyectoAPI.Data
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }
        public DbSet<EnfermedadExterna> EnfermedadesExternas { get; set; }
        public DbSet<Cultivo> Cultivos { get; set; }
        public DbSet<Datos> Datos { get; set; }
        public DbSet<Enfermedad> Enfermedades { get; set; }
        public DbSet<Esp32Cam> Esp32Cams { get; set; }
        public DbSet<Usuario> Usuarios { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Cultivo>()
                .ToTable("tbl_cultivos")
                .HasMany(c => c.Datos)
                .WithOne(d => d.Cultivo)
                .HasForeignKey(d => d.tbl_cultivos_cult_id);

            modelBuilder.Entity<Datos>().ToTable("tbl_datos");
            modelBuilder.Entity<Enfermedad>().ToTable("tbl_enfermedades");
            modelBuilder.Entity<EnfermedadExterna>().ToTable("tbl_enfermedades_externas");
            modelBuilder.Entity<Esp32Cam>().ToTable("tbl_esp32cam");
            modelBuilder.Entity<Usuario>().ToTable("tbl_usuarios");

            base.OnModelCreating(modelBuilder);
        }
    }
}