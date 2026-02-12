using Supabase.Postgrest.Attributes;
using Supabase.Postgrest.Models;

namespace Projeto_Jogo_Labirinto.Models
{
    [Table("salas")]
    public class Sala : BaseModel
    {
        [PrimaryKey("codigo")]
        public string Codigo { get; set; }

        [Column("funcao")]
        public string Funcao { get; set; }

        [Column("estado")]
        public string Estado { get; set; }

        [Column("estaCheia")]
        public int EstaCheia { get; set; }

        [Column("partida_id")]
        public int PartidaId { get; set; }

        [Column("tempo_restante")]
        public int TempoRestante { get; set; }

        [Column("estaTerminado")]
        public bool EstaTerminado { get; set; }

        [Column("posX")]
        public int PosX { get; set; }

        [Column("posY")]
        public int PosY { get; set; }
    }
}