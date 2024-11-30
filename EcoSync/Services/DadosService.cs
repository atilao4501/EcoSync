using EcoSync.Data;
using EcoSync.Models;
using EcoSync.Models.DTO;
using EcoSync.Services.Interfaces;
using FuzzySharp;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;

namespace EcoSync.Services;

/// <summary>
/// Serviço responsável por calcular as pontuações relacionadas à qualidade de vida nos bairros.
/// </summary>
public class DadosService : IDadosService
{
    private readonly DbContextClass _dbContext;

    /// <summary>
    /// Construtor do serviço de dados.
    /// </summary>
    /// <param name="dbContext">Contexto do banco de dados.</param>
    public DadosService(DbContextClass dbContext)
    {
        _dbContext = dbContext;
    }

    /// <summary>
    /// Obtém as pontuações de qualidade de vida de um bairro com base no nome fornecido.
    /// </summary>
    /// <param name="nomeBairro">Nome do bairro fornecido pela API.</param>
    /// <returns>Um objeto <see cref="RetornoPontuacoesPorBairro"/> contendo as pontuações calculadas.</returns>
    /// <exception cref="ArgumentException">Lançado se nenhum bairro correspondente for encontrado.</exception>
    public async Task<RetornoPontuacoesPorBairro> ObterPontuacoesPorCep(int cep)
    {
        string urlCep = "https://viacep.com.br/ws/" + cep + "/json/";
        var dadosCep = await new HttpClient().GetStringAsync(urlCep);

        if (!int.TryParse(dadosCep, out var statusCode) || statusCode != 200)
        {
            throw new HttpRequestException($"Erro ao obter dados de CEP: {dadosCep}");
        }

        var dadosCepDeserializado = JsonConvert.DeserializeObject<RetornoApiCep>(dadosCep);
        // Nome do bairro fornecido pela API
        var nomeBairroApi = dadosCepDeserializado.Bairro;

        // Tenta obter um bairro com nome semelhante ao fornecido
        var bairro = await ObterBairroComNomeParecido(nomeBairroApi);

        // Verifica se o bairro foi encontrado
        if (bairro == null)
        {
            throw new ArgumentException($"Nao foi encontrado um bairro com o nome {nomeBairroApi}");
        }

        // Pesos dos indicadores
        const double pesoAreaVerde = 0.30;
        const double pesoPoluicaoSonora = 0.25;
        const double pesoEstruturaDeServicos = 0.30;
        const double pesoDensidadePopulacional = 0.15;

        // Calcula as pontuações individuais
        var pontuacaoAreaVerde = CalcularPontuacaoAreaVerde(bairro);
        var pontuacaoDensidadePopulacional = CalcularPontuacaoDensidadePopulacional(bairro);
        var pontuacaoEstruturaDeServicos = CalcularPontuacaoEstruturaDeServicos(bairro);
        var pontuacaoPoluicaoSonora = CalcularPontuacaoPoluicaoSonora(bairro);

        // Calcula a pontuação total com base nos pesos
        var pontuacaoTotal =
            (pontuacaoAreaVerde * pesoAreaVerde) +
            (pontuacaoPoluicaoSonora * pesoPoluicaoSonora) +
            (pontuacaoEstruturaDeServicos * pesoEstruturaDeServicos) +
            (pontuacaoDensidadePopulacional * pesoDensidadePopulacional);

        // Cria o objeto de retorno com as pontuações calculadas
        var pontuacoesFinais = new RetornoPontuacoesPorBairro()
        {
            NomeBairro = bairro.Nome,
            PontuacaoAreaVerde = pontuacaoAreaVerde,
            PontuacaoDensidadePopulacional = pontuacaoDensidadePopulacional,
            PontuacaoEstruturaDeServicos = pontuacaoEstruturaDeServicos,
            PontuacaoPoluicaoSonora = pontuacaoPoluicaoSonora,
            PontuacaoTotal = pontuacaoTotal
        };

        return pontuacoesFinais;
    }

    /// <summary>
    /// Calcula a pontuação relacionada à poluição sonora para um bairro.
    /// </summary>
    /// <param name="bairroComPontuacoesBrutas">Objeto do bairro contendo as pontuações brutas.</param>
    /// <returns>Pontuação de 0 a 100, onde 100 representa ausência de poluição sonora.</returns>
    private double CalcularPontuacaoPoluicaoSonora(Bairro bairroComPontuacoesBrutas)
    {
        // Verifica se há registro de poluição sonora
        if (bairroComPontuacoesBrutas.Pontuacoes.PoluicaoSonoraEArTransito == 0)
        {
            return 100; // Sem poluição sonora, pontuação máxima
        }

        // Define o valor máximo aceitável de poluição sonora proporcional à área do bairro
        var maxPoluicaoAceitavel = 5 * bairroComPontuacoesBrutas.AreaEmKm2;

        // Calcula a proporção em relação ao valor ideal
        var proporcaoPoluicao = bairroComPontuacoesBrutas.Pontuacoes.PoluicaoSonoraEArTransito / maxPoluicaoAceitavel;

        // Calcula a pontuação invertida (quanto menos poluição, maior a pontuação)
        var pontuacao = Math.Max(0, (1 - proporcaoPoluicao) * 100);

        return pontuacao;
    }

    /// <summary>
    /// Calcula a pontuação relacionada à densidade populacional para um bairro.
    /// </summary>
    private double CalcularPontuacaoDensidadePopulacional(Bairro bairroComPontuacoesBrutas)
    {
        if (bairroComPontuacoesBrutas.Pontuacoes.DensidadePopulacional == 0)
        {
            return 0; // Sem densidade registrada
        }

        var ideal = 15000 * bairroComPontuacoesBrutas.AreaEmKm2;

        // Calcula a pontuação proporcional à densidade ideal
        var pontuacao = (bairroComPontuacoesBrutas.Pontuacoes.DensidadePopulacional / ideal) * 100;

        return pontuacao;
    }

    /// <summary>
    /// Calcula a pontuação relacionada à área verde para um bairro.
    /// </summary>
    private double CalcularPontuacaoAreaVerde(Bairro bairroComPontuacoesBrutas)
    {
        if (bairroComPontuacoesBrutas.Pontuacoes.AreaVerde == 0)
        {
            return 0; // Sem área verde
        }

        var ideal = 5 * bairroComPontuacoesBrutas.AreaEmKm2;

        // Calcula a pontuação proporcional à área verde ideal
        var pontuacao = (bairroComPontuacoesBrutas.Pontuacoes.AreaVerde / ideal) * 100;

        return pontuacao;
    }

    /// <summary>
    /// Calcula a pontuação relacionada à infraestrutura de serviços para um bairro.
    /// </summary>
    private double CalcularPontuacaoEstruturaDeServicos(Bairro bairroComPontuacoesBrutas)
    {
        if (bairroComPontuacoesBrutas.Pontuacoes.EstruturaDeServicos == 0)
        {
            return 0; // Sem estrutura de serviços
        }

        var ideal = 10 * bairroComPontuacoesBrutas.AreaEmKm2;

        // Calcula a pontuação proporcional à infraestrutura ideal
        var pontuacao = (bairroComPontuacoesBrutas.Pontuacoes.EstruturaDeServicos / ideal) * 100;

        return pontuacao;
    }

    /// <summary>
    /// Busca o bairro mais próximo com base no nome fornecido usando fuzzy matching.
    /// </summary>
    private async Task<Bairro> ObterBairroComNomeParecido(string nomeBairroApi)
    {
        // Obtém todos os nomes dos bairros do banco
        var bairros = await _dbContext.Bairros
            .Select(b => b.Nome)
            .ToListAsync();

        // Realiza fuzzy matching para encontrar o bairro mais semelhante
        var melhorMatch = bairros
            .Select(nome => new
            {
                Nome = nome,
                Similaridade = Fuzz.Ratio(nomeBairroApi, nome)
            })
            .OrderByDescending(result => result.Similaridade)
            .FirstOrDefault();

        var nome = melhorMatch?.Similaridade >= 70 ? melhorMatch.Nome : null;

        if (string.IsNullOrEmpty(nome))
        {
            return null; // Nenhum bairro encontrado com similaridade suficiente
        }

        // Retorna o bairro correspondente, incluindo as pontuações associadas
        var bairro = await _dbContext.Bairros
            .Include(b => b.Pontuacoes)
            .FirstOrDefaultAsync(b => b.Nome == nome);

        return bairro;
    }
}