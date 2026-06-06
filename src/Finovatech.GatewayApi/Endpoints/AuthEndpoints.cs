using Finovatech.GatewayApi.Autenticacao;

namespace Finovatech.GatewayApi.Endpoints;

public static class AuthEndpoints
{
    public static void MapAuth(this WebApplication app)
    {
        app.MapPost("/auth/login", (CredenciaisRequest req, IServicoJwt jwt) =>
        {
            UsuarioEmMemoria? usuario = RepositorioUsuariosEmMemoria.Consulte(req.Email, req.Senha);
            if (usuario is null)
            {
                return Results.Unauthorized();
            }

            string token = jwt.EmitaToken(usuario.Id, usuario.Email);
            return Results.Ok(new TokenResponse(token));
        }).AllowAnonymous();
    }
}
