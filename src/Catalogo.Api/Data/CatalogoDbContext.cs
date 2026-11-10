using Catalogo.Api.Models;
using Microsoft.EntityFrameworkCore;

namespace Catalogo.Api.Data;

public class CatalogoDbContext(DbContextOptions<CatalogoDbContext> options) : DbContext(options)
{
    public DbSet<Categoria> Categorias => Set<Categoria>();
    public DbSet<Marca> Marcas => Set<Marca>();
    public DbSet<Producto> Productos => Set<Producto>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Categoría: tabla singular, clave de identidad y longitudes de texto.
        modelBuilder.Entity<Categoria>(categoria =>
        {
            categoria.ToTable("Categoria");
            categoria.HasKey(c => c.IdCategoria);
            categoria.Property(c => c.IdCategoria).UseIdentityColumn();
            categoria.Property(c => c.Nombre).HasMaxLength(100).IsRequired();
            categoria.Property(c => c.Descripcion).HasMaxLength(250);

            categoria.Property(c => c.Estado)
                .HasMaxLength(10)
                .IsRequired()
                .HasDefaultValue("ACTIVO");
        });

        // Marca: tabla singular, clave de identidad y datos descriptivos.
        modelBuilder.Entity<Marca>(marca =>
        {
            marca.ToTable("Marca");
            marca.HasKey(m => m.IdMarca);
            marca.Property(m => m.IdMarca).UseIdentityColumn();
            marca.Property(m => m.Nombre).HasMaxLength(100).IsRequired();
            marca.Property(m => m.PaisOrigen).HasMaxLength(60);
            marca.Property(m => m.Estado)
                .HasMaxLength(10)
                .IsRequired()
                .HasDefaultValue("ACTIVO");
        });

        modelBuilder.Entity<Producto>(producto =>
        {
            // Las restricciones SQL exigen un precio positivo y estados válidos.
            producto.ToTable("Producto", tabla =>
            {
                tabla.HasCheckConstraint("CK_Producto_Precio", "[Precio] > 0");
                tabla.HasCheckConstraint("CK_Producto_Estado", "[Estado] IN ('ACTIVO','INACTIVO')");
            });

            // Clave de identidad, campos requeridos, precisión y estado predeterminado SQL.
            producto.HasKey(p => p.IdProducto);
            producto.Property(p => p.IdProducto).UseIdentityColumn();
            producto.Property(p => p.Nombre).HasMaxLength(150).IsRequired();
            producto.Property(p => p.SKU).HasMaxLength(30).IsRequired();
            producto.Property(p => p.Precio).HasColumnType("decimal(18,2)");
            producto.Property(p => p.Estado).HasMaxLength(10).IsRequired()
                .HasDefaultValueSql("'ACTIVO'");

            // El SKU identifica de forma única cada producto del catálogo.
            producto.HasIndex(p => p.SKU).IsUnique();

            // Impide eliminar categorías que todavía tengan productos asociados.
            producto.HasOne(p => p.Categoria)
                .WithMany(c => c.Productos)
                .HasForeignKey(p => p.IdCategoria)
                .OnDelete(DeleteBehavior.Restrict);

            // Impide eliminar marcas que todavía tengan productos asociados.
            producto.HasOne(p => p.Marca)
                .WithMany(m => m.Productos)
                .HasForeignKey(p => p.IdMarca)
                .OnDelete(DeleteBehavior.Restrict);
        });
    }
}
