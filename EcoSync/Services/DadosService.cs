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

    public DadosService(DbContextClass dbContext)
    {
        _dbContext = dbContext;
    }

    /// <summary>
    /// Obtém as pontuações de qualidade de vida de um bairro com base no CEP.
    /// </summary>
    public async Task<RetornoPontuacoesPorBairro> ObterPontuacoesPorCep(int cep)
    {
        string urlCep = $"https://viacep.com.br/ws/{cep}/json/";
        var httpClient = new HttpClient();
        var response = await httpClient.GetAsync(urlCep);

        if (!response.IsSuccessStatusCode)
        {
            throw new HttpRequestException($"Erro ao obter dados de CEP: {response.StatusCode} - {response.ReasonPhrase}");
        }

        var dadosCep = await response.Content.ReadAsStringAsync();
        if (string.IsNullOrEmpty(dadosCep))
        {
            throw new HttpRequestException("A resposta da API de CEP está vazia.");
        }

        var dadosCepDeserializado = JsonConvert.DeserializeObject<RetornoApiCep>(dadosCep);
        var nomeBairroApi = dadosCepDeserializado.Bairro;

        var bairro = await ObterBairroComNomeParecido(nomeBairroApi);
        if (bairro == null)
        {
            throw new ArgumentException($"Não foi encontrado um bairro com o nome {nomeBairroApi}");
        }

        // Pesos ajustados para indicadores
        const double pesoAreaVerde = 0.30;
        const double pesoEducacao = 0.25;
        const double pesoSaude = 0.25;
        const double pesoDensidadePopulacional = 0.20;

        var pontuacaoAreaVerde = CalcularPontuacaoAreaVerde(bairro);
        var pontuacaoEducacao = CalcularPontuacaoEducacao(bairro);
        var pontuacaoSaude = CalcularPontuacaoSaude(bairro);
        var pontuacaoDensidadePopulacional = CalcularPontuacaoDensidadePopulacional(bairro);

        var pontuacaoTotal =
            (pontuacaoAreaVerde * pesoAreaVerde) +
            (pontuacaoEducacao * pesoEducacao) +
            (pontuacaoSaude * pesoSaude) +
            (pontuacaoDensidadePopulacional * pesoDensidadePopulacional);

        return new RetornoPontuacoesPorBairro
        {
            NomeBairro = bairro.Nome,
            Latitude = bairro.Latitude,
            Longitude = bairro.Longitude,
            PontuacaoAreaVerde = pontuacaoAreaVerde,
            PontuacaoEducacao = pontuacaoEducacao,
            PontuacaoSaude = pontuacaoSaude,
            PontuacaoDensidadePopulacional = pontuacaoDensidadePopulacional,
            PontuacaoTotal = pontuacaoTotal
        };
    }

    public async Task<RetornoMediaPontuacoesGerais> ObterMediaPontuacoesGerais()
    {
        var bairros = await _dbContext.Bairros.Include(b => b.Pontuacoes).ToListAsync();

        if (bairros == null || bairros.Count == 0)
        {
            throw new InvalidOperationException("Não há bairros registrados para calcular as pontuações gerais.");
        }

        double totalPontuacaoAreaVerde = 0;
        double totalPontuacaoEducacao = 0;
        double totalPontuacaoSaude = 0;
        double totalPontuacaoDensidadePopulacional = 0;
        int totalBairros = bairros.Count;

        foreach (var bairro in bairros)
        {
            totalPontuacaoAreaVerde += CalcularPontuacaoAreaVerde(bairro);
            totalPontuacaoEducacao += CalcularPontuacaoEducacao(bairro);
            totalPontuacaoSaude += CalcularPontuacaoSaude(bairro);
            totalPontuacaoDensidadePopulacional += CalcularPontuacaoDensidadePopulacional(bairro);
        }

        var mediaPontuacaoAreaVerde = totalPontuacaoAreaVerde / totalBairros;
        var mediaPontuacaoEducacao = totalPontuacaoEducacao / totalBairros;
        var mediaPontuacaoSaude = totalPontuacaoSaude / totalBairros;
        var mediaPontuacaoDensidadePopulacional = totalPontuacaoDensidadePopulacional / totalBairros;

        var mediaPontuacaoTotal = (mediaPontuacaoAreaVerde * 0.30) +
                                  (mediaPontuacaoEducacao * 0.25) +
                                  (mediaPontuacaoSaude * 0.25) +
                                  (mediaPontuacaoDensidadePopulacional * 0.20);

        return new RetornoMediaPontuacoesGerais
        {
            MediaPontuacaoAreaVerde = mediaPontuacaoAreaVerde,
            MediaPontuacaoEducacao = mediaPontuacaoEducacao,
            MediaPontuacaoSaude = mediaPontuacaoSaude,
            MediaPontuacaoDensidadePopulacional = mediaPontuacaoDensidadePopulacional,
            MediaPontuacaoTotal = mediaPontuacaoTotal
        };
    }

    private double CalcularPontuacaoSaude(Bairro bairro)
    {
        if (bairro.Pontuacoes.Saude == 0)
        {
            return 0; // Sem serviços de saúde registrados
        }

        var populacao = bairro.Pontuacoes.DensidadePopulacional * bairro.AreaEmKm2;
        var unidadesSaudeIdeais = populacao / 3000; // 1 UBS para cada 3000 habitantes
        var pontuacao = (bairro.Pontuacoes.Saude / unidadesSaudeIdeais) * 100;

        return Math.Min(pontuacao, 100);
    }

    private double CalcularPontuacaoEducacao(Bairro bairro)
    {
        if (bairro.Pontuacoes.Educacao == 0)
        {
            return 0; // Sem infraestrutura educacional
        }

        var populacao = bairro.Pontuacoes.DensidadePopulacional * bairro.AreaEmKm2;
        var escolasIdeais = populacao / 500; // 1 escola para cada 500 alunos
        var pontuacao = (bairro.Pontuacoes.Educacao / escolasIdeais) * 100;

        return Math.Min(pontuacao, 100);
    }

    private double CalcularPontuacaoAreaVerde(Bairro bairro)
    {
        if (bairro.Pontuacoes.AreaVerde == 0)
        {
            return 0; // Sem área verde
        }

        var ideal = 5 * bairro.AreaEmKm2;
        var pontuacao = (bairro.Pontuacoes.AreaVerde / ideal) * 100;

        return Math.Min(pontuacao, 100);
    }

    private double CalcularPontuacaoDensidadePopulacional(Bairro bairro)
    {
        if (bairro.Pontuacoes.DensidadePopulacional == 0)
        {
            return 0; // Sem densidade registrada
        }

        var ideal = 15000 * bairro.AreaEmKm2;
        var pontuacao = (bairro.Pontuacoes.DensidadePopulacional / ideal) * 100;

        return Math.Min(pontuacao, 100);
    }

    private async Task<Bairro> ObterBairroComNomeParecido(string nomeBairroApi)
    {
        var bairros = await _dbContext.Bairros.Select(b => b.Nome).ToListAsync();
        var melhorMatch = bairros.Select(nome => new
        {
            Nome = nome,
            Similaridade = Fuzz.Ratio(nomeBairroApi, nome)
        })
        .OrderByDescending(result => result.Similaridade)
        .FirstOrDefault();

        var nome = melhorMatch?.Similaridade >= 70 ? melhorMatch.Nome : null;

        if (string.IsNullOrEmpty(nome))
        {
            return null;
        }

        return await _dbContext.Bairros
            .Include(b => b.Pontuacoes)
            .FirstOrDefaultAsync(b => b.Nome == nome);
    }
}