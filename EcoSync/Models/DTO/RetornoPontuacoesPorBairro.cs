namespace EcoSync.Models.DTO
{
    /// <summary>
    /// Representa o retorno das pontuações por bairro.
    /// </summary>
    public class RetornoPontuacoesPorBairro
    {
        public string NomeBairro { get; set; }
        public double PontuacaoDensidadePopulacional { get; set; }
        public double PontuacaoAreaVerde { get; set; }
        public double PontuacaoEstruturaDeServicos { get; set; }
        
        public double PontuacaoPoluicaoSonora { get; set; }
        public double PontuacaoTotal { get; set; }
    }
}