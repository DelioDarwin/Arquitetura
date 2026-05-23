using Arquitetura.SharedKernel.Primitives;
using Clientes.Domain.Exceptions;

namespace Clientes.Domain.Entities;

public sealed class Cliente : Entity
{
    private Cliente() { } // EF Core

    private Cliente(Guid id, string nome, string email, string cpf)
        : base(id)
    {
        Nome = nome;
        Email = email;
        Cpf = cpf;
        CriadoEm = DateTime.UtcNow;
    }

    public string Nome { get; private set; } = string.Empty;
    public string Email { get; private set; } = string.Empty;
    public string Cpf { get; private set; } = string.Empty;
    public DateTime CriadoEm { get; private set; }
    public DateTime? AtualizadoEm { get; private set; }

    public static Cliente Criar(string nome, string email, string cpf)
    {
        if (string.IsNullOrWhiteSpace(nome))
            throw new ClienteException("O nome do cliente é obrigatório.");
        if (string.IsNullOrWhiteSpace(email))
            throw new ClienteException("O email do cliente é obrigatório.");
        if (string.IsNullOrWhiteSpace(cpf))
            throw new ClienteException("O CPF do cliente é obrigatório.");

        return new Cliente(Guid.NewGuid(), nome.Trim(), email.Trim(), cpf);
    }

    public void Atualizar(string nome, string email, string cpf)
    {
        if (string.IsNullOrWhiteSpace(nome))
            throw new ClienteException("O nome do cliente é obrigatório.");
        if (string.IsNullOrWhiteSpace(email))
            throw new ClienteException("O email do cliente é obrigatório.");
        if (string.IsNullOrWhiteSpace(cpf))
            throw new ClienteException("O CPF do cliente é obrigatório.");
        Nome = nome.Trim();
        Email = email.Trim();
        Cpf = cpf;
        AtualizadoEm = DateTime.UtcNow;
    }

}
