using WebScrappingRecipes.Elastic;

namespace WebScrappingRecipes.ScrapedRecipes;

public class Recipes : Document
{
    public string IdImage { get; set; } = null!;
    public string Name { get; set; } = null!;
    public string Autor { get; set; } = null!;
    public string Url { get; set; } = null!;
    public int Portions { get; set; }
    public int? PreparationTime { get; set; }
    public string Difficulty { get; set; } = null!;

    public ICollection<string> CategoryRecipe { get; set; } = null!;

    public ICollection<string> FoodDays { get; set; } = null!;
    public ICollection<Ingredient>? Ingredients { get; set; }
    public ICollection<string>? Steps { get; set; }
}