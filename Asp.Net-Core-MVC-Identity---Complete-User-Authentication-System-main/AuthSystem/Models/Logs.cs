using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace eros.Models
{
    public class Logs
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)] // Auto-increment primary key

        [Display(Name = "Id")]
        public int id { get; set; }

        [Display(Name = "Page Name")]
        public string pagename { get; set; }

        [Display(Name = "Task Id")]
        public int taskid { get; set; }

        [Display(Name = "Task")]
        public string task { get; set; }
        
        [Display(Name = "Action")]
        public string action { get; set; }

        [Display(Name = "Date")]
        public string date { get; set; }

        [Display(Name = "Time")]
        public string time { get; set; }

        [Display(Name = "User Name")]
        public string username { get; set; }

 
    }
}
