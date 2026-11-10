using Microsoft.EntityFrameworkCore;
using Pedidos.Api.Models;

namespace Pedidos.Api.Data;

public class PedidosDbContext(DbContextOptions<PedidosDbContext> options) : DbContext(options)
{
    public DbSet<Cliente> Clientes => Set<Cliente>();
    public DbSet<Pedido> Pedidos => Set<Pedido>();
    public DbSet<DetallePedido> Detalles => Set<DetallePedido>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<Cliente>(cliente =>
        {
            // Tabla singular y restricción SQL para una cédula de exactamente diez dígitos.
            cliente.ToTable("Cliente", tabla =>
            {
                tabla.HasCheckConstraint("CK_Cliente_Cedula", "LEN([Cedula]) = 10 AND [Cedula] NOT LIKE '%[^0-9]%'");
            });

            // Clave de identidad y campos de texto obligatorios con sus longitudes.
            cliente.HasKey(c => c.IdCliente);
            cliente.Property(c => c.IdCliente).UseIdentityColumn();
            cliente.Property(c => c.Nombres).HasMaxLength(150).IsRequired();
            cliente.Property(c => c.Cedula).HasColumnType("char(10)").IsRequired();
            cliente.Property(c => c.Correo).HasMaxLength(150).IsRequired();

            // Los índices únicos evitan clientes con cédula o correo repetidos.
            cliente.HasIndex(c => c.Cedula).IsUnique();
            cliente.HasIndex(c => c.Correo).IsUnique();
        });

        modelBuilder.Entity<Pedido>(pedido =>
        {
            // Tabla singular y restricciones SQL para estados válidos y totales no negativos.
            pedido.ToTable("Pedido", tabla =>
            {
                tabla.HasCheckConstraint("CK_Pedido_Estado", "[Estado] IN ('PENDIENTE','CONFIRMADO','ENTREGADO','ANULADO')");
                tabla.HasCheckConstraint("CK_Pedido_Total", "[Total] >= 0");
            });

            // Clave de identidad, precisión monetaria y valores predeterminados en SQL Server.
            pedido.HasKey(p => p.IdPedido);
            pedido.Property(p => p.IdPedido).UseIdentityColumn();
            pedido.Property(p => p.Total).HasColumnType("decimal(18,2)");
            pedido.Property(p => p.Estado).HasMaxLength(12).IsRequired()
                .HasDefaultValueSql("'PENDIENTE'");
            pedido.Property(p => p.Fecha).HasDefaultValueSql("GETDATE()");

            // FK física con Restrict: impide borrar un cliente con pedidos para conservar su historial.
            pedido.HasOne(p => p.Cliente)
                .WithMany(c => c.Pedidos)
                .HasForeignKey(p => p.IdCliente)
                .OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<DetallePedido>(detalle =>
        {
            // Tabla singular y restricciones SQL para cantidades positivas y precios no negativos.
            detalle.ToTable("DetallePedido", tabla =>
            {
                tabla.HasCheckConstraint("CK_Detalle_Cantidad", "[Cantidad] > 0");
                tabla.HasCheckConstraint("CK_Detalle_Precio", "[PrecioUnitario] >= 0");
            });

            // Clave de identidad y precisión monetaria del precio unitario.
            detalle.HasKey(d => d.IdDetalle);
            detalle.Property(d => d.IdDetalle).UseIdentityColumn();
            detalle.Property(d => d.PrecioUnitario).HasColumnType("decimal(18,2)");

            // Índice normal por producto; la referencia a Catálogo es lógica y no crea una FK SQL.
            detalle.HasIndex(d => d.IdProducto);

            // FK física con Cascade: al borrar un pedido se eliminan sus detalles, que no existen sin él.
            detalle.HasOne(d => d.Pedido)
                .WithMany(p => p.Detalles)
                .HasForeignKey(d => d.IdPedido)
                .OnDelete(DeleteBehavior.Cascade);
        });
    }
}
