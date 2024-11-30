using EcoSync.Models.DTO;

namespace EcoSync.Services.Interfaces;

public interface IDadosService
{
    /// <summary>
    /// Obtém as pontua es de qualidade de vida de um bairro com base no nome fornecido.
    /// </summary>
    /// <param name="nomeBairro">Nome do bairro fornecido pela API.</param>
    /// <returns>Um objeto <see cref="RetornoPontuacoesPorBairro"/> contendo as pontua es calculadas.</returns>
    /// <exception cref="ArgumentException">Lan adio se nenhum bairro correspondente for encontrado.</exception>
    Task<RetornoPontuacoesPorBairro> ObterPontuacoesPorCep(int cep);
    
    Task<RetornoMediaPontuacoesGerais> ObterMediaPontuacoesGerais();
}