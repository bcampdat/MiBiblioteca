using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;

namespace MiBiblioteca.Models;

public partial class MiBibliotecaContext : DbContext
{
    public MiBibliotecaContext()
    {
    }

    public MiBibliotecaContext(DbContextOptions<MiBibliotecaContext> options)
        : base(options)
    {
    }

    public virtual DbSet<Autore> Autores { get; set; }

    public virtual DbSet<Editoriale> Editoriales { get; set; }

    public virtual DbSet<Libro> Libros { get; set; }

    public virtual DbSet<Operacione> Operaciones { get; set; }

    public virtual DbSet<Usuario> Usuarios { get; set; }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
#warning To protect potentially sensitive information in your connection string, you should move it out of source code. You can avoid scaffolding the connection string by using the Name= syntax to read it from configuration - see https://go.microsoft.com/fwlink/?linkid=2131148. For more guidance on storing connection strings, see https://go.microsoft.com/fwlink/?LinkId=723263.
        => optionsBuilder.UseSqlServer("Data Source=DESKTOP-6H3MG14;Initial Catalog=MiBiblioteca;Integrated Security=True;TrustServerCertificate=True");

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Autore>(entity =>
        {
            entity.HasKey(e => e.IdAutor).HasName("PK__Autores__DD33B031D305B28F");

            entity.Property(e => e.Nombre).HasMaxLength(100);
        });

        modelBuilder.Entity<Editoriale>(entity =>
        {
            entity.HasKey(e => e.IdEditorial).HasName("PK__Editoria__EF838671F715B6EF");

            entity.Property(e => e.Nombre).HasMaxLength(100);
        });

        modelBuilder.Entity<Libro>(entity =>
        {
            entity.HasKey(e => e.Isbn).HasName("PK__Libros__447D36EBC1627E28");

            entity.Property(e => e.Isbn).HasMaxLength(15);
            entity.Property(e => e.FotoPortadaUrl).HasMaxLength(100);
            entity.Property(e => e.Precio).HasColumnType("decimal(9, 2)");
            entity.Property(e => e.Titulo).HasMaxLength(100);

            entity.HasOne(d => d.Autor).WithMany(p => p.Libros)
                .HasForeignKey(d => d.AutorId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Libros_Autores");

            entity.HasOne(d => d.Editorial).WithMany(p => p.Libros)
                .HasForeignKey(d => d.EditorialId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Libros_Editoriales");
        });

        modelBuilder.Entity<Operacione>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__Operacio__3214EC0706561D30");

            entity.Property(e => e.Controller).HasMaxLength(50);
            entity.Property(e => e.FechaAccion).HasColumnType("datetime");
            entity.Property(e => e.Ip).HasMaxLength(50);
            entity.Property(e => e.Operacion).HasMaxLength(50);
        });

        modelBuilder.Entity<Usuario>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__Usuarios__3214EC07B879F70B");

            entity.Property(e => e.Email).HasMaxLength(100);
            entity.Property(e => e.EnlaceCambioPass).HasMaxLength(500);
            entity.Property(e => e.FechaEnvioEnlace).HasColumnType("datetime");
            entity.Property(e => e.Password).HasMaxLength(500);
        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
