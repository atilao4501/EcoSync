using System.ComponentModel.DataAnnotations;

namespace EcoSync.Models
{
    /// <summary>
    /// Representa informações básicas de um bairro.
    /// </summary>
    public class Bairro
    {
        /// <summary>
        /// Nome do bairro.
        /// </summary>

        [Key]
        public string Nome { get; set; }

        /// <summary>
        /// Latitude do bairro.
        /// </summary>
        public double Latitude { get; set; }

        /// <summary>
        /// Longitude do bairro.
        /// </summary>
        public double Longitude { get; set; }

        /// <summary>
        /// Pontuações associadas ao bairro.
        /// </summary>
        public Pontuacoes Pontuacoes { get; set; }

        /// <summary>
        /// Densidade populacional do bairro.
        /// </summary>
        public int DensidadePopulacional { get; set; }
        
        public double AreaEmKm2 { get; set; }
    }
}