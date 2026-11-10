using Inventario.Api.Models;
using Microsoft.EntityFrameworkCore;

namespace Inventario.Api.Data;

public class InventarioDbContext(DbContextOptions<InventarioDbContext> options) : DbContext(options)
{
    public DbSet<Bodega> Bodegas => Set<Bodega>();
    public DbSet<Existencia> Existencias => Set<Existencia>();
    public DbSet<MovimientoInventario> Movimientos => Set<MovimientoInventario>();
    public DbSet<ReservaStock> Reservas => Set<ReservaStock>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Bodega: tabla singular, clave de identidad y longitudes de texto.
        modelBuilder.Entity<Bodega>(bodega =>
        {
            bodega.ToTable("Bodega");
            bodega.HasKey(b => b.IdBodega);
            bodega.Property(b => b.IdBodega).UseIdentityColumn();
            bodega.Property(b => b.Nombre).HasMaxLength(100).IsRequired();
            bodega.Property(b => b.Ciudad).HasMaxLength(80).IsRequired();
            bodega.Property(b => b.Direccion).HasMaxLength(200);
        });

        modelBuilder.Entity<Existencia>(existencia =>
        {
            // La tabla singular impide registrar stock negativo.
            existencia.ToTable("Existencia", tabla =>
            {
                tabla.HasCheckConstraint("CK_Existencia_Stock", "[Stock] >= 0");
            });

            // Clave de identidad y existencia única por producto y bodega, sin FK de producto.
            existencia.HasKey(e => e.IdExistencia);
            existencia.Property(e => e.IdExistencia).UseIdentityColumn();
            existencia.HasIndex(e => new { e.IdProducto, e.IdBodega }).IsUnique();

            // La FK física impide eliminar una bodega que tenga existencias.
            existencia.HasOne(e => e.Bodega)
                .WithMany(b => b.Existencias)
                .HasForeignKey(e => e.IdBodega)
                .OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<MovimientoInventario>(movimiento =>
        {
            // Las restricciones SQL exigen cantidades positivas y tipos válidos.
            movimiento.ToTable("MovimientoInventario", tabla =>
            {
                tabla.HasCheckConstraint("CK_Movimiento_Cantidad", "[Cantidad] > 0");
                tabla.HasCheckConstraint("CK_Movimiento_Tipo", "[Tipo] IN ('ENTRADA','SALIDA')");
            });

            // Clave de identidad, tipo requerido y fecha predeterminada en SQL Server.
            movimiento.HasKey(m => m.IdMovimiento);
            movimiento.Property(m => m.IdMovimiento).UseIdentityColumn();
            movimiento.Property(m => m.Tipo).HasMaxLength(10).IsRequired();
            movimiento.Property(m => m.Fecha).HasDefaultValueSql("GETDATE()");

            // Índice de búsqueda por producto; no establece una FK hacia Catálogo.
            movimiento.HasIndex(m => m.IdProducto);

            // La FK física impide eliminar una bodega que tenga movimientos.
            movimiento.HasOne(m => m.Bodega)
                .WithMany(b => b.Movimientos)
                .HasForeignKey(m => m.IdBodega)
                .OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<ReservaStock>(reserva =>
        {
            // Las restricciones SQL exigen cantidades positivas y estados válidos.
            reserva.ToTable("ReservaStock", tabla =>
            {
                tabla.HasCheckConstraint("CK_Reserva_Cantidad", "[Cantidad] > 0");
                tabla.HasCheckConstraint("CK_Reserva_Estado", "[Estado] IN ('ACTIVA','CONFIRMADA','CANCELADA')");
            });

            // Clave de identidad y estado requerido con valor predeterminado SQL.
            reserva.HasKey(r => r.IdReserva);
            reserva.Property(r => r.IdReserva).UseIdentityColumn();
            reserva.Property(r => r.Estado).HasMaxLength(12).IsRequired()
                .HasDefaultValueSql("'ACTIVA'");

            // Índice de búsqueda por producto; no establece una FK hacia Catálogo.
            reserva.HasIndex(r => r.IdProducto);

            // La FK física impide eliminar una bodega que tenga reservas.
            reserva.HasOne(r => r.Bodega)
                .WithMany(b => b.Reservas)
                .HasForeignKey(r => r.IdBodega)
                .OnDelete(DeleteBehavior.Restrict);
        });
    }
}
