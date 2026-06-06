namespace NovaMart.Frontend.Servicos;

public interface IServicoToast
{
    event Func<string, string, Task>? OnMostrar;

    Task MostreAsync(string mensagem, string icone = "fa-solid fa-check");
}
