using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace eros.Models
{
    public class MenuModel
    {

        
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int MenuId { get; set; }
        public int? ParentMenuId { get; set; }
        public string? icon { get; set; }
        public string Title { get; set; }
        public string Controller { get; set; }
        public string Action { get; set; }

       

    }
}
