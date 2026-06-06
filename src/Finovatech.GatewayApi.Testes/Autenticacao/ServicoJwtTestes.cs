using Finovatech.GatewayApi.Autenticacao;
using Microsoft.Extensions.Configuration;

namespace Finovatech.GatewayApi.Testes.Autenticacao;

public class ServicoJwtTestes
{
    private static ServicoJwt CrieServico() => new(
        new ConfigurationBuilder().AddInMemoryCollection(new Dictionary<string, string?>
        {
            ["Jwt:ChaveSecreta"] = "finovatech-super-secret-key-2026-testes-longas!",
            ["Jwt:Emissor"]      = "finovatech",
            ["Jwt:Audiencia"]    = "novamart"
        }).Build());

    [Fact] public void EmitaToken_DeveRetornarStringNaoVazia()
        => Assert.NotEmpty(CrieServico().EmitaToken("u1", "u@test.com"));

    [Fact] public void EmitaToken_DeveRetornarJwtComTresPartes()
        => Assert.Equal(3, CrieServico().EmitaToken("u1", "u@test.com").Split('.').Length);

    [Fact] public void ValideToken_TokenValido_DeveRetornarTrueEUserId()
    {
        ServicoJwt s = CrieServico();
        string token = s.EmitaToken("user-42", "u@test.com");
        bool ok = s.ValideToken(token, out string? uid);
        Assert.True(ok);
        Assert.Equal("user-42", uid);
    }

    [Fact] public void ValideToken_TokenInvalido_DeveRetornarFalse()
    {
        bool ok = CrieServico().ValideToken("invalido.token.aqui", out string? uid);
        Assert.False(ok);
        Assert.Null(uid);
    }

    [Fact] public void ValideToken_TokenVazio_DeveRetornarFalse()
    {
        bool ok = CrieServico().ValideToken(string.Empty, out string? uid);
        Assert.False(ok);
        Assert.Null(uid);
    }

    [Fact] public void EmitaToken_DoisTokens_DevemSerDiferentes()
    {
        ServicoJwt s = CrieServico();
        Assert.NotEqual(s.EmitaToken("u1", "u@test.com"), s.EmitaToken("u1", "u@test.com"));
    }
}
