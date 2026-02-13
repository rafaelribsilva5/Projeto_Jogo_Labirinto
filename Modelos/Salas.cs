using Supabase.Postgrest.Attributes;
using Supabase.Postgrest.Models;

namespace Projeto_Jogo_Labirinto.Models
{
    using Supabase.Postgrest.Attributes;
    using Supabase.Postgrest.Models;

    [Table("cities")]
    public class City : BaseModel
    {
        [PrimaryKey("id", false)]
        public int Id { get; set; }

        [Column("name")]
        public string Name { get; set; }
    }
}