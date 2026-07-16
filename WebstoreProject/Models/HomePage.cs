namespace WebstoreProject.Models
{
    public class HomePage
    {
        public List<HomeSection> Sections { get; set; } = new();
    }

    public class HomeSection
    {
        public Category TopCategory { get; set; } = new Category();
        public List<Product> Products { get; set; } = new List<Product>();
    }
}
