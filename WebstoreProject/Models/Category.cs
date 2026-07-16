using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;



namespace WebstoreProject.Models
{
    public class Category
    {
        public int CategoryId { get; set; }

        public int? ParentId { get; set; }

        [Required, StringLength(150)]
        public string Name { get; set; } = "";

        public string? Description { get; set; }

        public int SortOrder { get; set; } = 0;

        public bool IsActive { get; set; } = true;

        // Tree navigation
        public Category? Parent { get; set; }
        public List<Category> Children { get; set; } = new();

        // Products to leafs
        public List<Product> Products { get; set; } = new();
        public string? IconUrl { get; set; }


    }
}
