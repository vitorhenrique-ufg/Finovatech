namespace Finovatech.ServicoNotificacao.Dominio.Entidades;

public record AssinaturaPush(
    string UsuarioId,
    string Endpoint,
    string P256dh,
    string Auth
);
