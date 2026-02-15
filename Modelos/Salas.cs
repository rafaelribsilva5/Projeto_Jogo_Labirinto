using Supabase.Postgrest.Attributes;
using Supabase.Postgrest.Models;

namespace Projeto_Jogo_Labirinto.Models
{
    [Table("Salas")]
    public class Salas : BaseModel
    {
        [PrimaryKey("codigo", false)]
        public int codigo { get; set; }

        [Column("estado")]
        public string estado { get; set; } = "";
    }
}