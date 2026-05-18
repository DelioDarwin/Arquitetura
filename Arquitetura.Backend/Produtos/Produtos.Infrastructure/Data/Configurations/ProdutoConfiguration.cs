using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Produtos.Domain.Entities;

namespace Produtos.Infrastructure.Data.Configurations;

internal sealed class ProdutoConfiguration : IEntityTypeConfiguration<Produto>
{
    public void Configure(EntityTypeBuilder<Produto> builder)
    {
        builder.HasKey(p => p.Id);
        builder.Property(p => p.Nome).IsRequired().HasMaxLength(200);
        builder.Property(p => p.Descricao).HasMaxLength(1000);
        builder.Property(p => p.Preco).HasPrecision(18, 2);
        builder.Property(p => p.CriadoEm).IsRequired();
    }
}
