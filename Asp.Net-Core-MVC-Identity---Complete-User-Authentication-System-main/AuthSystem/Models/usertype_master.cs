using System.ComponentModel.DataAnnotations;

namespace eros.Models
{
    public class usertype_master
    {
        [Key]
        [Display(Name = "User Id")]
        public int user_id { get; set; }

        [Required]
        [Display(Name = "User Type")]
        public string usertype_name { get; set; }

        [Display(Name = "Designation")]
        public string designation { get; set; }

    }
}
