using System.Net;
using System.Text.RegularExpressions;
using HtmlAgilityPack;
using ScrapySharp.Extensions;
using WebScrappingRecipes.Utils;

namespace WebScrappingRecipes.ScrapedRecipes.ScratchPages;

public partial class GuidingStarts : IInformationRecipe
{
    private static readonly ICollection<(string url, int cant)> Urls = new List<(string, int)>
    {
        ("https://guidingstars.com/dish/appetizers-and-snacks/", 17), ("https://guidingstars.com/dish/beverages/",
            4),
        ("https://guidingstars.com/dish/bread/", 2), ("https://guidingstars.com/dish/breakfast/", 10),
        ("https://guidingstars.com/dish/desserts/", 6),
        ("https://guidingstars.com/dish/dressings-dips-and-spreads/", 8),
        ("https://guidingstars.com/dish/entrees/", 44),
        ("https://guidingstars.com/dish/lunch/", 9), ("https://guidingstars.com/dish/salads/", 16),
        ("https://guidingstars.com/dish/sandwich-entrees/", 4), ("https://guidingstars.com/dish/soups/", 10),
        ("https://guidingstars.com/dish/vegan-vegetarian-2/", 44), ("https://guidingstars.com/dish/vegetarian-2/", 77)
    };

    [Obsolete($"Obsolete")]
    public void GetInformationRecipes(int cantPages)
    {
        var html = new HtmlWeb();
        for (var i = 0; i < cantPages; i++)
        {
            var url = Urls.ToList()[i].url;
            for (var j = 1; j <= Urls.ToList()[i].cant; j++)
            {
                var loadPage = j > 1 ? html.Load($"{url}page/{j}/") : html.Load(url);
                try
                {
                    var urlRecipes = loadPage.DocumentNode.CssSelect("[class='site-main']").First()
                        .CssSelect("[class='entry-content']")
                        .Select(x => x.CssSelect("a").First().GetAttributeValue("href")).ToList();
                    if (!urlRecipes.Any()) continue;
                    var recipes = InformationRecipes(urlRecipes, html);
                    Elastic.Elastic.AddMultipleRecipes("recipes-en", recipes);
                }
                catch (Exception)
                {
                    Console.WriteLine("error urlRecipes " + loadPage);
                    throw;
                }
            }
        }
    }

    [Obsolete("Obsolete")]
    private IEnumerable<Recipes> InformationRecipes(IEnumerable<string> urlRecipes, HtmlWeb html)
    {
        var recipes = new List<Recipes>();
        foreach (var urlRecipe in urlRecipes)
        {
            try
            {
                var loadPageRecipe = html.Load(urlRecipe);
                var nameRecipe = loadPageRecipe.DocumentNode.CssSelect("[class='entry-title']").First().InnerText
                    .CleanInnerText();
                const string author = "Guiding Stars";
                var portionAndTime = GetPortionTime(loadPageRecipe);
                var ingredients = GetIngredientRecipe(loadPageRecipe);
                var steps = GetStepsRecipe(loadPageRecipe);
                var guid = Guid.NewGuid().ToString();
                var categoryAndFoodDay = GetCategoryAndFoodDay(loadPageRecipe);
                if (portionAndTime.portion == 0 || !ingredients.Any()) continue;
                var recipe = new Recipes
                {
                    Name = nameRecipe, Portions = portionAndTime.portion,
                    PreparationTime = portionAndTime.preparationTime,
                    Difficulty = "", IdImage = guid, Author = author, Ingredients = ingredients, Steps = steps,
                    Url = urlRecipe, CategoryRecipe = categoryAndFoodDay.categoryRecipe,
                    FoodDays = categoryAndFoodDay.foodDays
                };
                GetImageRecipe(loadPageRecipe, guid);
                recipes.Add(recipe);
            }
            catch (Exception)
            {
                Console.WriteLine("Error in page Recipe : " + urlRecipe);
            }
        }

        return recipes;
    }

    private (int portion, int preparationTime) GetPortionTime(HtmlDocument loadPageRecipe)
    {
        var elements = loadPageRecipe.DocumentNode.CssSelect("[class='recipe__meta']").First();
        var portion = int.Parse(GetPortionRecipe(loadPageRecipe));
        var preparationTime =
            GetTimeRecipe(elements.CssSelect("[itemprop='cookTime']").First().InnerText.CleanInnerText());
        return (portion, preparationTime);
    }

    private static int GetTimeRecipe(string textWithTime)
    {
        try
        {
            if (!textWithTime.Contains("hour")) return int.Parse(Number().Match(textWithTime).Value);
            return int.Parse(Number().Match(textWithTime).Value) * 60;
        }
        catch (Exception)
        {
            return 0;
        }
    }

    public ICollection<Ingredient> GetIngredientRecipe(HtmlDocument loadPageRecipe)
    {
        var listIngredient = loadPageRecipe.DocumentNode.CssSelect("[class='recipe__ingredients']").First();

        try
        {
            var existsH3 = listIngredient.CssSelect("[class='h4']").First().InnerText;
            Console.WriteLine("H4 obtenido" + existsH3);
            var ingredients = listIngredient
                .CssSelect("[itemprop='ingredients']")
                .Select(ingredientText => new Ingredient
                    { IngredientText = ingredientText.InnerText.CleanInnerText() })
                .ToList();
            return ingredients;
        }
        catch (Exception)
        {
            var ingredients = listIngredient.CssSelect("li")
                .Select(ingredientText => new Ingredient { IngredientText = ingredientText.InnerText.CleanInnerText() })
                .ToList();
            return ingredients;
        }
    }

    public string GetPortionRecipe(HtmlDocument loadPageRecipe)
    {
        var elements = loadPageRecipe.DocumentNode.CssSelect("[class='recipe__meta']").First();
        var portion = Number()
            .Matches(elements.CssSelect("[itemprop='recipeYield']").First().InnerText.CleanInnerText()).First().Value;
        return portion;
    }

    private static ICollection<string> GetStepsRecipe(HtmlDocument loadPageRecipe)
    {
        var listStep = loadPageRecipe.DocumentNode.CssSelect("[class='recipe__directions']").First().CssSelect("li")
            .Select(step => step.InnerText.CleanInnerText()).ToList();

        return listStep;
    }

    private static (ICollection<string> foodDays, ICollection<string> categoryRecipe) GetCategoryAndFoodDay(
        HtmlDocument loadPageRecipe)
    {
        var listCategory = loadPageRecipe.DocumentNode.CssSelect("[class='recipe__tax-list']").First().CssSelect("li")
            .Select(x => x.InnerText.CleanInnerText()).ToList();
        var categoryAndFoodDay = CsvReading.GetCategoryRecipeGs(listCategory);
        return (categoryAndFoodDay.foodDays, categoryAndFoodDay.categoryRecipe);
    }

    [Obsolete("Obsolete")]
    private static void GetImageRecipe(HtmlDocument loadPageRecipe, string guid)
    {
        using var oClient = new WebClient();
        var uri = loadPageRecipe.DocumentNode.CssSelect("[class='recipe__photo']").First().CssSelect("img").First()
            .GetAttributeValue("src");
        oClient.DownloadFile(new Uri(uri),
            $"C:/Users/hello/RiderProjects/WebScrappingRecipes/WebScrappingRecipes/files/image_recipes/{guid}.jpg");
    }

    [GeneratedRegex("\\d+")]
    private static partial Regex Number();
}