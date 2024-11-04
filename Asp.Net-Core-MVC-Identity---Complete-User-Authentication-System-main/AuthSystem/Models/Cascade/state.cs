using System.ComponentModel.DataAnnotations.Schema;

namespace eros.Models.Cascade
{
    public class state
    {
        public int Id { get; set; }

        public string Name { get; set; }

        public country country { get; set; }
        [NotMapped]
        public string countryname { get; set; }

    }
}
