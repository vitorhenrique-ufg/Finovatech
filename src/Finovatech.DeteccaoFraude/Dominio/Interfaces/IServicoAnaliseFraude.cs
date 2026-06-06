using Finovatech.DeteccaoFraude.Dominio.ObjetosDeValor;

namespace Finovatech.DeteccaoFraude.Dominio.Interfaces;

public interface IServicoAnaliseFraude
{
    ResultadoAnalise Analise(decimal valor, string moedaOrigem);
}
