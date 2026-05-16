using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Pedidos.Domain.Entities;

namespace Pedidos.Infrastructure.Data.Configurations;

internal sealed class PedidoConfiguration : IEntityTypeConfiguration<Pedido>
{
    public void Configure(EntityTypeBuilder<Pedido> builder)
    {
        builder.HasKey(p => p.Id);
        builder.Property(p => p.ClienteId).IsRequired();
        builder.Property(p => p.Status).IsRequired();
        builder.Property(p => p.CriadoEm).IsRequired();

        builder.OwnsMany(p => p.Itens, itens =>
        {
            itens.WithOwner().HasForeignKey(i => i.PedidoId);
            itens.HasKey(i => i.Id);
            itens.Property(i => i.NomeProduto).HasMaxLength(200).IsRequired();
            itens.Property(i => i.PrecoUnitario).HasPrecision(18, 2).IsRequired();
            itens.Property(i => i.Quantidade).IsRequired();
        });
    }
}
