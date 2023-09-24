using System.Text.RegularExpressions;
using HtmlAgilityPack;
using ScrapySharp.Extensions;
using WebScrappingRecipes.Utils;

namespace WebScrappingRecipes.ScrapedRecipes.ScratchPages;

public static partial class GuidingStarts
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

    public static void GetInformationRecipes(int cantPages)
    {
        var html = new HtmlWeb();
        for (var i = 0; i < cantPages; i++)
        {
            var url = Urls.ToList()[i].url;
            for (var j = 1; j <= 2; j++)
            {
                var loadPage = j > 1 ? html.Load($"{url}page/{j}/") : html.Load(url);
                var urlRecipes = loadPage.DocumentNode.CssSelect("[class='site-main']").First()
                    .CssSelect("[class='entry-content']")
                    .Select(x => x.CssSelect("a").First().GetAttributeValue("href"));
                var recipes = InformationRecipes(urlRecipes, html);
            }
        }
    }

    private static IEnumerable<Recipes> InformationRecipes(IEnumerable<string> urlRecipes, HtmlWeb html)
    {
        var recipes = new List<Recipes>();
        foreach (var urlRecipe in urlRecipes)
        {
            var loadPageRecipe = html.Load(urlRecipe);
            var nameRecipe = loadPageRecipe.DocumentNode.CssSelect("[class='entry-title']").First().InnerText
                .CleanInnerText();
            Console.WriteLine(urlRecipe);
            const string author = "GuidingStart";
            var portionAndTime = GetPortionTime(loadPageRecipe);
            var ingredients = GetIngredientRecipe(loadPageRecipe);
            var steps = GetStepsRecipe(loadPageRecipe);
            var guid = Guid.NewGuid().ToString();
            var categoryAndFoodDay = GetCategoryAndFoodDay(loadPageRecipe);
            if (portionAndTime.portion == 0) continue;
            var recipe = new Recipes
            {
                Name = nameRecipe, Portions = portionAndTime.portion, PreparationTime = portionAndTime.preparationTime,
                Difficulty = "", IdImage = guid, Autor = author, Ingredients = ingredients, Steps = steps,
                Url = urlRecipe, CategoryRecipe = categoryAndFoodDay.categoryRecipe,
                FoodDays = categoryAndFoodDay.foodDays
            };
            GetImageRecipe(loadPageRecipe);
            recipes.Add(recipe);
        }

        return recipes;
    }

    private static (int portion, int preparationTime) GetPortionTime(HtmlDocument loadPageRecipe)
    {
        var elements = loadPageRecipe.DocumentNode.CssSelect("[class='recipe__meta']").First();
        var portion = int.Parse(Number()
            .Matches(elements.CssSelect("[itemprop='recipeYield']").First().InnerText.CleanInnerText()).First().Value);
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
        catch (Exception )
        {
            return 0;
        }
    }

    private static ICollection<Ingredient> GetIngredientRecipe(HtmlDocument loadPageRecipe)
    {
        var listIngredient = loadPageRecipe.DocumentNode.CssSelect("[class='recipe__ingredients']").First()
            .CssSelect("[itemprop='ingredients']").Select(ingredientText =>
                new Ingredient { IngredientText = ingredientText.InnerText.CleanInnerText() }).ToList();
        return listIngredient;
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

    private static void GetImageRecipe(HtmlDocument loadPageRecipe)
    {
        var uri = loadPageRecipe.DocumentNode.CssSelect("[class='recipe__photo']").First().CssSelect("img").First()
            .GetAttributeValue("src");
        Console.WriteLine(uri);
    }

    [GeneratedRegex("\\d+")]
    private static partial Regex Number();
}