namespace EcoSync.Models.DTO
{
    /// <summary>
    /// Representa o retorno das pontuações por bairro.
    /// </summary>
    public class RetornoPontuacoesPorBairro
    {
        public string NomeBairro { get; set; }
        
        /// <summary>
        /// Latitude do bairro.
        /// </summary>
        public double Latitude { get; set; }

        /// <summary>
        /// Longitude do bairro.
        /// </summary>
        public double Longitude { get; set; }
        public double PontuacaoDensidadePopulacional { get; set; }
        public double PontuacaoAreaVerde { get; set; }
        public double PontuacaoEstruturaDeServicos { get; set; }
        
        public double PontuacaoPoluicaoSonora { get; set; }
        public double PontuacaoTotal { get; set; }
    }
}