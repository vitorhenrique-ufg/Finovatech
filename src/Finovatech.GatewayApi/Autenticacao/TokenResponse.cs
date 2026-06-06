namespace Finovatech.GatewayApi.Autenticacao;

public record TokenResponse(string AccessToken, string TokenType = "Bearer");
