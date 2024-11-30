namespace EcoSync.Models
{
    /// <summary>
    /// Representa as pontuações relacionadas a sustentabilidade e qualidade ambiental.
    /// </summary>
    public class Pontuacoes
    {
        public int id { get; set; }
        
        /// <summary>
        /// Pontuação atribuída à área verde do local.
        /// </summary>
        public double AreaVerde { get; set; } = 0.0;

        /// <summary>
        /// Pontuação referente à poluição sonora e de ar devido ao trânsito.
        /// </summary>
        public double PoluicaoSonoraEArTransito { get; set; } = 0.0;

        /// <summary>
        /// Pontuação da estrutura de serviços disponíveis.
        /// </summary>
        public double Educacao { get; set; } = 0.0;
        
        public double Saude { get; set; } = 0.0;

        public double DensidadePopulacional { get; set; } = 0.0;
        
    }
}