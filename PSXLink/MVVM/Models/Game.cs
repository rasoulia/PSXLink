using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;

namespace PSXLink.MVVM.Models
{
    public class Game
    {
        [Key, DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        [JsonIgnore]
        public int ID { get; set; }

        public string? Title { get; set; }

        public string? TitleID { get; set; }

        public int Region { get; set; }

        public string? Version { get; set; }

        public string? Console { get; set; }

        public string? XmlLink { get; set; }
    }
}
