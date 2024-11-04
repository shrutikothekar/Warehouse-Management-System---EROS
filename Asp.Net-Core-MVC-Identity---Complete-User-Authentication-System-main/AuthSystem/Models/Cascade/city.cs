using System.ComponentModel.DataAnnotations.Schema;

namespace eros.Models.Cascade
{
    public class city
    {
        public int Id { get; set; }
        public string Name { get; set; }

        public state state { get; set; }
        [NotMapped]
        public string statename { get; set; }
    }
}
