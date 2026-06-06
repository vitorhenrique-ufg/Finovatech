namespace NovaMart.Frontend.Servicos;

public class ServicoToast : IServicoToast
{
    public event Func<string, string, Task>? OnMostrar;

    public async Task MostreAsync(string mensagem, string icone = "fa-solid fa-check")
    {
        if (OnMostrar is not null)
        {
            await OnMostrar.Invoke(mensagem, icone);
        }
    }
}
