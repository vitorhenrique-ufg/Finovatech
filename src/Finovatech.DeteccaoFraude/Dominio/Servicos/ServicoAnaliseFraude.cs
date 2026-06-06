using Finovatech.DeteccaoFraude.Dominio.Interfaces;
using Finovatech.DeteccaoFraude.Dominio.ObjetosDeValor;
using Microsoft.Extensions.Options;

namespace Finovatech.DeteccaoFraude.Dominio.Servicos;

public class ServicoAnaliseFraude(IOptions<ConfiguracaoAnaliseFraude> opcoes) : IServicoAnaliseFraude
{
    private readonly ConfiguracaoAnaliseFraude _config = opcoes.Value;

    public ResultadoAnalise Analise(decimal valor, string moedaOrigem)
    {
        if (valor > _config.ValorMaximoPorTransacao)
        {
            return ResultadoAnalise.Rejeitar(
                $"Valor {valor:N2} excede o limite máximo de {_config.ValorMaximoPorTransacao:N2}");
        }

        if (_config.MoedasBloqueadas.Contains(moedaOrigem, StringComparer.OrdinalIgnoreCase))
        {
            return ResultadoAnalise.Rejeitar(
                $"Moeda de origem '{moedaOrigem}' não é permitida");
        }

        return ResultadoAnalise.Aprovar();
    }
}
