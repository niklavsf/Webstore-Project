using System.Collections.Generic;

namespace WebstoreProject.Models
{
    public class Admin
    {
        public string Tab { get; set; } = "Products";

        public List<Category> Categories { get; set; } = new List<Category>();
        public List<Brand> Brands { get; set; } = new List<Brand>();
        public List<Product> Products { get; set; } = new List<Product>();
        public List<DeliveryType> DeliveryTypes { get; set; } = new List<DeliveryType>();
        public List<Order> Orders { get; set; } = new List<Order>();


        // Add forms
        public Category NewCategory { get; set; } = new Category();
        public Brand NewBrand { get; set; } = new Brand();
        public Product NewProduct { get; set; } = new Product();
        public DeliveryType NewDeliveryType { get; set; } = new DeliveryType();
    }
}
