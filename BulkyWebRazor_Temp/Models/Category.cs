using System.ComponentModel.DataAnnotations;
using System.ComponentModel;

namespace BulkyWebRazor_Temp.Models
{
    public class Category
    {   //This is our primary key
        [Key] // type key and then press enter. This is key data annotation
        public int Id { get; set; }

        [Required]   //Can't be null
        [DisplayName("Category Name")]
        [MaxLength(30, ErrorMessage = "Max character length is 30 characters")] //max length is 30 charaters 
        public string Name { get; set; }


        [DisplayName("Display Order")]
        [Range(1, 100, ErrorMessage = "Display Order must be between 1 and 100")] // minimum is 1 and max is 100
        public int DisplayOrder { get; set; } // what appears first on the screen
    }
}
