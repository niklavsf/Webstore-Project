using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Globalization;

namespace WebstoreProject.Models
{
    public class Product
    {
        public int ProductId { get; set; }

        public int CategoryId { get; set; }
        public Category? Category { get; set; }

        // Brand
        public int? BrandId { get; set; }
        public Brand? Brand { get; set; }

        public string Name { get; set; } = "";
        public string? Description { get; set; }
        public string? ImageUrl { get; set; }

        public decimal Price { get; set; }
        public int DiscountPercent { get; set; } = 0;

        public int StockQty { get; set; }
        public bool IsActive { get; set; } = true;

        // Nutrition
        public int? EnergyKj { get; set; }
        public int? EnergyKcal { get; set; }
        public decimal? FatG { get; set; }
        public decimal? SatFatG { get; set; }
        public decimal? CarbsG { get; set; }
        public decimal? SugarG { get; set; }
        public decimal? ProteinG { get; set; }
        public decimal? SaltG { get; set; }


        public string UnitType { get; set; } = "pcs";

        public decimal UnitSize { get; set; } = 1m;

        //helper functs for discount
        public bool HasDiscount
        {
            get { return DiscountPercent > 0; }
        }

        public decimal DiscountedPrice
        {
            get
            {
                if (!HasDiscount) return Price;
                decimal multiplier = (100 - DiscountPercent) / 100m;
                return Math.Round(Price * multiplier, 2, MidpointRounding.AwayFromZero);
            }
        }

        public decimal PriceToUse
        {
            get { return HasDiscount ? DiscountedPrice : Price; }
        }

        public string UnitText
        {
            get { return UnitSize.ToString("0.###", CultureInfo.InvariantCulture) + " " + UnitType; }
        }

        public decimal? UnitPrice
        {
            get
            {
                if (UnitSize <= 0) return null;

                decimal p = PriceToUse;

                if (UnitType == "l") return p / UnitSize;                 
                if (UnitType == "ml") return p / (UnitSize / 1000m);      
                if (UnitType == "kg") return p / UnitSize;               
                if (UnitType == "g") return p / (UnitSize / 1000m);     

                return null;
            }
        }

        public string? UnitPriceLabel
        {
            get
            {
                if (UnitPrice == null) return null;
                if (UnitType == "l" || UnitType == "ml") return "€/l";
                if (UnitType == "kg" || UnitType == "g") return "€/kg";
                return null;
            }
        }


    }
}
