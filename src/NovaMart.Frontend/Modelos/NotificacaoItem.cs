namespace NovaMart.Frontend.Modelos;

public record NotificacaoItem(
    string Titulo,
    string Corpo,
    string Url,
    string Tempo,
    bool Lida
);
