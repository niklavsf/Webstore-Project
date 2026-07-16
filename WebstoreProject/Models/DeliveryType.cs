using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace WebstoreProject.Models
{
    public class DeliveryType
    {
        public int DeliveryTypeId { get; set; }

        [Required, StringLength(100)]
        public string Name { get; set; } = "";

        [Column(TypeName = "numeric(10,2)")]
        public decimal Price { get; set; }

        public string? Description { get; set; }

        public bool IsActive { get; set; } = true;
    }
}
