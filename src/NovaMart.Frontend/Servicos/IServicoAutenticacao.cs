namespace NovaMart.Frontend.Servicos;

public interface IServicoAutenticacao
{
    Task<bool> EntreComCredenciaisAsync(string email, string senha);
    Task EncerreSecaoAsync();
}
