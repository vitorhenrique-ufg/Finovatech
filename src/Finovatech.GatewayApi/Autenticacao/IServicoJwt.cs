namespace Finovatech.GatewayApi.Autenticacao;

public interface IServicoJwt
{
    string EmitaToken(string userId, string email);
    bool ValideToken(string token, out string? userId);
}
