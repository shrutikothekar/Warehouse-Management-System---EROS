using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace eros.Models.Cascade
{
    public class country
    {
        [Key]
        public int Id { get; set; }

        public string Name { get; set; }
        [NotMapped]
        public string stateName { get; set; }
        [NotMapped]
        public string cityName { get; set; }
        
    }
}
