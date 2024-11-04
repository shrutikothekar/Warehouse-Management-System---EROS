using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace eros.Models
{
    public class loginlog
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)] // Auto-increment primary key
        [Display(Name = "Id")]
        public int Id { get; set; }

        [Display(Name = "User Name ")]
        public string username { get; set; }

        [Display(Name = "Login Date")]
        public string logindt { get; set; }

        [Display(Name = "Login Time")]
        public string logintime { get; set; }

        [Display(Name = "LogOut Date")]
        public string logoutdt { get; set; }

        [Display(Name = "Logout Time")]
        public string logouttime { get; set; }

        [Display(Name = "Date Time")]
        public string date { get; set; } = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
    }
}
