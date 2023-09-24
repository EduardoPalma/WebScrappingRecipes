using System.Net;
using System.Text.RegularExpressions;
using HtmlAgilityPack;
using ScrapySharp.Extensions;
using WebScrappingRecipes.Utils;

namespace WebScrappingRecipes.ScrapedRecipes.ScratchPages;

public static partial class ComidasTipicasChilenas
{
    private static readonly string[] Urls =
    {
        "https://www.comidastipicaschilenas.com/platos-principales-tipicos-de-chile/",
        "https://www.comidastipicaschilenas.com/postres-tipicos-chilenos/",
        "https://www.comidastipicaschilenas.com/sandwiches-tipicos-chilenos/",
        "https://www.comidastipicaschilenas.com/aperitivos-tipicos-chilenos/",
        "https://www.comidastipicaschilenas.com/bebidas-tipicas-chilenas/",
        "https://www.comidastipicaschilenas.com/panes-tipicos-de-chile/",
        "https://www.comidastipicaschilenas.com/recetas-saludables-chilenas/",
        "https://www.comidastipicaschilenas.com/ensaladas-tipicas-chilenas/"
    };

    [Obsolete("Obsolete")]
    public static void GetInformationRecipes(int cantPages)
    {
        var html = new HtmlWeb();
        for (var i = 0; i < cantPages; i++)
        {
            var loadPage = html.Load(Urls[1]);
            var urlRecipes = loadPage.DocumentNode.CssSelect("[class='pt-cv-wrapper']").First()
                .CssSelect("[class='pt-cv-ifield']").Select(x => x.CssSelect("a").First().GetAttributeValue("href"));
            var recipes = InformationRecipes(urlRecipes, html, i);
            Elastic.Elastic.AddMultipleRecipes("recipes-es",recipes);
        }
    }

    [Obsolete("Obsolete")]
    private static IEnumerable<Recipes> InformationRecipes(IEnumerable<string> urlRecipes, HtmlWeb html,
        int urlCategory)
    {
        var recipes = new List<Recipes>();
        foreach (var url in urlRecipes)
        {
            var loadPageRecipes = html.Load(url);
            var nameRecipe = loadPageRecipes.DocumentNode.CssSelect("[class='entry-title']").First().InnerText;
            var author = "ComidasTipicasChilenas";
            var portionAndTime = GetPortionsAndTime(loadPageRecipes);
            var ingredients = GetIngredientsRecipe(loadPageRecipes);
            var steps = GetStepsRecipe(loadPageRecipes);
            var guid = Guid.NewGuid().ToString();
            var categoryAndFoodDay = CsvReading.GetCategoryRecipeCtc(urlCategory);
            if (portionAndTime.portion == 0) continue;
            var recipe = new Recipes
            {
                Autor = author, Name = nameRecipe, Portions = portionAndTime.portion,
                PreparationTime = portionAndTime.timeRecipe, Ingredients = ingredients, Steps = steps, Url = url,
                Difficulty = "", IdImage = guid, CategoryRecipe = categoryAndFoodDay.categoryRecipe,
                FoodDays = categoryAndFoodDay.foodDays
            };
            GetImageRecipe(loadPageRecipes, guid);
            recipes.Add(recipe);
        }

        return recipes;
    }

    private static (int portion, int timeRecipe) GetPortionsAndTime(HtmlDocument loadPageRecipes)
    {
        var elements = loadPageRecipes.DocumentNode.CssSelect("[class='wp-block-table']").CssSelect("tr")
            .Select(x => x.InnerText.CleanInnerText()).ToList();
        var getNumber = Number().Matches(elements[4]);
        var portion = getNumber.Any() ? int.Parse(getNumber.First().Value) : 0;
        var timeRecipe = GetTimeRecipe(elements[2], elements[3]);

        return (portion, timeRecipe);
    }

    private static int GetTimeRecipe(string preparation, string cooking)
    {
        var timePreparation = GetTimeRecipe(preparation);
        var timeCooking = GetTimeRecipe(cooking);
        return timePreparation + timeCooking;
    }

    private static int GetTimeRecipe(string textWithTime)
    {
        if (textWithTime.Contains("hora") && textWithTime.Contains("minuto"))
        {
            var getNumbers = Number().Matches(textWithTime).ToList();
            var hora = int.Parse(getNumbers[0].Value) * 60;
            var minuts = int.Parse(getNumbers[1].Value);
            var time = hora + minuts;
            return time;
        }

        if (textWithTime.Contains("hora"))
        {
            var getNumber = Number().Match(textWithTime).Value;
            var time = int.Parse(getNumber) * 60;
            return time;
        }

        return int.Parse(Number().Match(textWithTime).Value);
    }

    private static ICollection<Ingredient> GetIngredientsRecipe(HtmlDocument loadPageRecipe)
    {
        var listIngredient = loadPageRecipe.DocumentNode.CssSelect("[id='checklist-id-0']").CssSelect("li")
            .Select(x => x.InnerText);

        return listIngredient.Select(ingredientText => new Ingredient { IngredientText = ingredientText }).ToList();
    }

    private static ICollection<string> GetStepsRecipe(HtmlDocument loadPageRecipe)
    {
        var listSteps = loadPageRecipe.DocumentNode.CssSelect("[id='checklist-id-1']").CssSelect("li")
            .Select(x => x.InnerText.CleanInnerText());

        return listSteps.ToList();
    }

    [Obsolete("Obsolete")]
    private static void GetImageRecipe(HtmlDocument loadPageRecipe, string guid)
    {
        using var oClient = new WebClient();
        var uriHtml = loadPageRecipe.DocumentNode.CssSelect("[class='featured-image page-header-image-single']").First()
            .CssSelect("img").First();
        var uriOne = uriHtml.GetAttributeValue("data-ezsrcset").Split(" ")[0];
        var uriTwo = uriHtml.GetAttributeValue("src");
        oClient.DownloadFile(string.IsNullOrEmpty(uriOne) ? new Uri(uriTwo) : new Uri(uriOne),
            $"C:/Users/hello/RiderProjects/WebScrappingRecipes/WebScrappingRecipes/files/image_recipes/{guid}.jpg");
    }

    [GeneratedRegex("\\d+")]
    private static partial Regex Number();
}