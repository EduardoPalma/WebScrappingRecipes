namespace WebScrappingRecipes.Utils;

public class AuxCategoryRecipe
{
    public string Url { get; set; } = null!;
    public ICollection<string> CategoryRecipe { get; set; } = null!;
    public ICollection<string> FoodDays { get; set; } = null!;
}