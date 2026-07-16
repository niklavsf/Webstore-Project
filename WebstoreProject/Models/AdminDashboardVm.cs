namespace WebstoreProject.Models
{
    public class AdminDashboardVm
    {
        //for tree categories
        public List<Category> Categories { get; set; } = new();
        public List<Category> CategoryRoots { get; set; } = new();
        public List<Product> Products { get; set; } = new();
    }
}
