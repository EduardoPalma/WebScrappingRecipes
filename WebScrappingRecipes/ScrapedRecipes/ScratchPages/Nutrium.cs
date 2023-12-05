using System.Net;
using System.Text.RegularExpressions;
using HtmlAgilityPack;
using ScrapySharp.Extensions;
using WebScrappingRecipes.Utils;

namespace WebScrappingRecipes.ScrapedRecipes.ScratchPages;

public partial class Nutrium : IInformationRecipe
{
    private const string MainUrl = $"https://www.nutriumpfg.com/blog-nutricion-dietetica/recetas-saludables-faciles/";

    [Obsolete("Obsolete")]
    public void GetInformationRecipes(int cantPages)
    {
        var html = new HtmlWeb();
        for (var i = 1; i <= cantPages; i++)
        {
            //https://www.nutriumpfg.com/blog-nutricion-dietetica/recetas-saludables-faciles/page/2/
            var numPages = i == 1 ? "" : $"page/{i}/";
            var loadPage = html.Load($"{MainUrl}{numPages}");
            var urlRecipes = loadPage.DocumentNode.CssSelect("[class='elementor-post__thumbnail__link']")
                .Select(x => x.GetAttributeValue("href"));
            var enumerable = urlRecipes.ToList();
            if (enumerable.ToList().Contains("https://www.nutriumpfg.com/blog-nutricion/7-ensaladas-para-el-verano/"))
                enumerable.ToList().Remove("https://www.nutriumpfg.com/blog-nutricion/7-ensaladas-para-el-verano/");
            if (enumerable.ToList().Contains("https://www.nutriumpfg.com/desayunos-saludables/"))
                enumerable.ToList().Remove("https://www.nutriumpfg.com/desayunos-saludables/");
            var recipes = InformationRecipes(enumerable, html);
            Elastic.Elastic.AddMultipleRecipes("recipes-es", recipes);
        }
    }

    [Obsolete("Obsolete")]
    private IEnumerable<Recipes> InformationRecipes(IEnumerable<string> urlRecipes, HtmlWeb html)
    {
        var recipes = new List<Recipes>();
        foreach (var url in urlRecipes)
        {
            var loadPageRecipes = html.Load(url);
            var nameRecipe = loadPageRecipes.DocumentNode
                .CssSelect("[class='elementor-heading-title elementor-size-default']").First().InnerText;
            const string author = "Nutrium";
            var portions = GetPortionRecipe(loadPageRecipes);
            var ingredients = GetIngredientRecipe(loadPageRecipes);
            var preparations = GetPreparationRecipes(loadPageRecipes);
            var foodsDays = GetFoodDays(nameRecipe, url);
            var categoryRecipe = GetCategoryRecipes(nameRecipe, url);
            var guid = Guid.NewGuid().ToString();
            GetImageRecipe(loadPageRecipes, guid);

            var recipe = new Recipes
            {
                Author = author, Name = nameRecipe, Portions = Convert.ToInt32(portions), Url = url,
                PreparationTime = 0,
                Ingredients = ingredients, Steps = preparations, IdImage = guid,
                Difficulty = "Fácil",
                FoodDays = foodsDays, CategoryRecipe = categoryRecipe
            };
            if (ingredients.Any()) recipes.Add(recipe);
        }

        return recipes;
    }

    public string GetPortionRecipe(HtmlDocument loadPageRecipes)
    {
        try
        {
            var portions = loadPageRecipes.DocumentNode.CssSelect("p").Where(x =>
                x.InnerText.Contains("person") || x.InnerText.Contains("elaborar") ||
                x.InnerText.Contains("racion"));
            var portion = portions as HtmlNode[] ?? portions.ToArray();
            if (portion.Any()) return MyRegex().Matches(portion.First().InnerText).First().Value;

            var portionsNew =
                loadPageRecipes.DocumentNode.CssSelect("[class='has-medium-font-size wp-block-heading']").First()
                    .InnerText;
            var result = MyRegex().Matches(portionsNew);
            return result.First().Value;
        }
        catch (Exception)
        {
            // ignored
        }

        return "0";
    }

    [GeneratedRegex("\\d+")]
    private static partial Regex MyRegex();

    public ICollection<Ingredient> GetIngredientRecipe(HtmlDocument loadPageRecipes)
    {
        var ingredients = new List<Ingredient>();
        var container = loadPageRecipes.DocumentNode.Descendants("h2")
            .FirstOrDefault(node => node.InnerText.Contains("Ingredientes"));
        while (container != null && !container.InnerText.Contains("Elaboración"))
        {
            if (container.Name == "ul")
            {
                var ingredientsNodes = container.CssSelect("li");
                ingredients.AddRange(ingredientsNodes.Select(ingredientNode =>
                    new Ingredient { IngredientText = ingredientNode.InnerText.CleanInnerText() }));
            }

            container = container.NextSibling;
        }

        return ingredients;
    }

    private static ICollection<string> GetPreparationRecipes(HtmlDocument loadPageRecipes)
    {
        var preparations = new List<string>();
        var container = loadPageRecipes.DocumentNode.Descendants("h2")
            .FirstOrDefault(node => node.InnerText.Contains("Elaboración"));
        while (container != null && !container.InnerText.Contains("Información nutricional"))
        {
            var preparationNodes = container.CssSelect("li");

            var htmlNodes = preparationNodes.ToList();
            preparations.AddRange(htmlNodes.Select(preparationNode => preparationNode.InnerText.CleanInnerText()));
            if (container.Name == "p")
            {
                preparations.Add(container.InnerText.CleanInnerText());
            }

            container = container.NextSibling;
        }

        return preparations;
    }

    private static ICollection<string> GetFoodDays(string nameRecipe, string url)
    {
        var foodDays = new List<string>();
        if (nameRecipe.ToLower().Contains("ensalada"))
        {
            foodDays.Add("almuerzo");
            foodDays.Add("cena");
        }
        else
        {
            var foodDaysCsv = CsvReading.FoodDayRecipe(url);
            if (foodDaysCsv.Any()) return foodDaysCsv;
            foodDays.Add("almuerzo");
            foodDays.Add("cena");
        }


        return foodDays;
    }

    private static ICollection<string> GetCategoryRecipes(string nameRecipe, string url)
    {
        var categoryRecipes = new List<string>();
        if (nameRecipe.ToLower().Contains("ensalada")) categoryRecipes.Add("ensalada");
        else
        {
            var category = CsvReading.CategoryRecipe(url);
            if (category.Any()) return category;
            categoryRecipes.Add("plato principal");
        }

        return categoryRecipes;
    }

    [Obsolete("Obsolete")]
    private static void GetImageRecipe(HtmlDocument loadPageRecipe, string guid)
    {
        using var oclient = new WebClient();
        var img = loadPageRecipe.DocumentNode.CssSelect(
                "[class='elementor-section elementor-top-section elementor-element elementor-element-366cc8f elementor-section-boxed elementor-section-height-default elementor-section-height-default']")
            .First()
            .CssSelect(
                "[class='elementor-element elementor-element-eec8229 elementor-widget elementor-widget-theme-post-featured-image elementor-widget-image']")
            .First().CssSelect("[class='elementor-widget-container']").First().InnerHtml.CleanInnerText();
        var regex = MyRegex2();
        var match = regex.Match(img).Groups[1].Value.Split(",")[0].Replace(" 1000w", "");
        try
        {
            oclient.DownloadFile(new Uri(match),
                $"C:/Users/hello/RiderProjects/WebScrappingRecipes/WebScrappingRecipes/files/image_recipes/{guid}.jpg");
        }
        catch (Exception)
        {
            Console.WriteLine("no imagen");
        }
    }

    [GeneratedRegex("data-srcset=\"([^\"]*)\"")]
    private static partial Regex MyRegex2();
}