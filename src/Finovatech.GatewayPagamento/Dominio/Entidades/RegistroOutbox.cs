namespace Finovatech.GatewayPagamento.Dominio.Entidades;

public class RegistroOutbox
{
    public Guid Id { get; private set; }
    public string Tipo { get; private set; } = string.Empty;
    public string Carga { get; private set; } = string.Empty;
    public DateTimeOffset CriadoEm { get; private set; }
    public bool Publicado { get; private set; }
    public DateTimeOffset? PublicadoEm { get; private set; }

    private RegistroOutbox() { }

    public static RegistroOutbox Crie(string tipo, string carga)
    {
        return new RegistroOutbox
        {
            Id = Guid.NewGuid(),
            Tipo = tipo,
            Carga = carga,
            CriadoEm = DateTimeOffset.UtcNow,
            Publicado = false
        };
    }

    public void MarqueComoPublicado()
    {
        Publicado = true;
        PublicadoEm = DateTimeOffset.UtcNow;
    }
}
