namespace Finovatech.GatewayApi.Autenticacao;

public record UsuarioEmMemoria(string Id, string Email, string Senha);

public static class RepositorioUsuariosEmMemoria
{
    private static readonly IReadOnlyList<UsuarioEmMemoria> _usuarios =
    [
        new("admin-1", "admin@finovatech.com", "1"),
        new("user-1",  "user@novamart.com",    "1"),
    ];

    public static UsuarioEmMemoria? Consulte(string email, string senha)
        => _usuarios.FirstOrDefault(u =>
            u.Email.Equals(email, StringComparison.OrdinalIgnoreCase));
}
