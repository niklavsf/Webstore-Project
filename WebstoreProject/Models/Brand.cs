using System.ComponentModel.DataAnnotations;

namespace WebstoreProject.Models
{
    public class Brand
    {
        public int BrandId { get; set; }

        [Required, StringLength(120)]
        public string Name { get; set; } = "";

        public List<Product> Products { get; set; } = new List<Product>();
    }
}
