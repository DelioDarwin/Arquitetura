namespace Arquitetura.Server.Endpoints;

/// <summary>
/// Contrato para todos os grupos de endpoints da API.
/// Cada feature implementa esta interface para registrar suas rotas.
/// </summary>
public interface IEndpointGroup
{
    void Map(RouteGroupBuilder group);
}
