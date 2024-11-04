using Microsoft.AspNetCore.Mvc.Rendering;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace eros.Models
{
    public class UserManagment
    {
        public int Id { get; set; }

        [Required]
        public string UserName { get; set; }
     
        public string PageName { get; set; }

        [Required]
        public string Role { get; set; }
        [NotMapped ]
        public List<string>? selectedpages { get; set; }


    }

}
