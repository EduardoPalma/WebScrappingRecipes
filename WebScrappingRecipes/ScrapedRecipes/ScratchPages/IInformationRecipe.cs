using HtmlAgilityPack;

namespace WebScrappingRecipes.ScrapedRecipes.ScratchPages;

public interface IInformationRecipe
{
    ICollection<Ingredient> GetIngredientRecipe(HtmlDocument loadPageRecipe);
    string GetPortionRecipe(HtmlDocument loadPageRecipe);

}