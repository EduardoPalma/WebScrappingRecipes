using System.Net;
using System.Text.RegularExpressions;
using HtmlAgilityPack;
using ScrapySharp.Extensions;
using WebScrappingRecipes.Utils;

namespace WebScrappingRecipes.ScrapedRecipes.ScratchPages;

public partial class DietDoctor : IInformationRecipe
{
    private const string MainUrl =
        $"https://www.dietdoctor.com/es/keto/recetas-cetogenicas/desayunos?s=&st=recipe&kd_recipe_type%5B%5D=952&kd_recipe_type%5B%5D=74&kd_recipe_type%5B%5D=888&kd_recipe_type%5B%5D=906&kd_recipe_type%5B%5D=55&kd_recipe_type%5B%5D=125&kd_recipe_type%5B%5D=81&kd_recipe_type%5B%5D=164&kd_recipe_type%5B%5D=56&kd_recipe_type%5B%5D=80&kd_recipe_type%5B%5D=120&sp=";

    [Obsolete("Obsolete")]
    public void GetInformationRecipes(int cantPages)
    {
        var html = new HtmlWeb();
        for (var i = 1; i <= cantPages; i++)
        {
            var loadPage = html.Load($"{MainUrl}{i}");
            var urlRecipes = loadPage.DocumentNode.CssSelect("[class='section-fixed-width']").CssSelect("li")
                .CssSelect("[class='inner']").Select(x => x.CssSelect("a").First().GetAttributeValue("href"));
            var recipes = InformationRecipes(urlRecipes, html);
            Elastic.Elastic.AddMultipleRecipes("recipes-es", recipes);
        }
    }

    [Obsolete("Obsolete")]
    private IEnumerable<Recipes> InformationRecipes(IEnumerable<string> urlRecipes, HtmlWeb html)
    {
        const string author = "DietDoctor";
        var recipes = new List<Recipes>();
        foreach (var urlRecipe in urlRecipes)
        {
            var loadPageRecipe = html.Load(urlRecipe);
            var nameRecipe = loadPageRecipe.DocumentNode
                .CssSelect("[id='huge-feature-box-title']").First().InnerText.Trim();
            var timeRecipe = GetTime(loadPageRecipe);
            var portions = GetPortionRecipe(loadPageRecipe);
            var ingredients = GetIngredientRecipe(loadPageRecipe);
            var preparations = GetStepRecipe(loadPageRecipe);
            var category = GetCategory(loadPageRecipe);
            var foodAndCategorysRecipe = CsvReading.GetFoodDaysAndCategory(category);
            var foodsDays = foodAndCategorysRecipe.Item1;
            var categoryRecipe = foodAndCategorysRecipe.Item2;
            var difficulty = GetDifficulty(loadPageRecipe);
            var guid = Guid.NewGuid().ToString();
            if (!foodsDays.Any() || portions == "") continue;
            var recipe = new Recipes
            {
                Author = author, Name = nameRecipe, Portions = Convert.ToInt32(portions), Url = urlRecipe,
                PreparationTime = timeRecipe,
                Ingredients = ingredients, Steps = preparations, IdImage = guid,
                Difficulty = difficulty,
                FoodDays = foodsDays, CategoryRecipe = categoryRecipe
            };
            recipes.Add(recipe);
            GetImageRecipe(loadPageRecipe, guid);
        }

        return recipes;
    }

    private static int GetTime(HtmlDocument loadPageRecipe)
    {
        try
        {
            var time = loadPageRecipe.DocumentNode.CssSelect("[class='ckdc-recipe-info']").First().InnerText;
            time = RegexTime().Replace(time, "");
            var timeInt = GetTimeClean(time);
            return timeInt;
        }
        catch (Exception)
        {
            return 0;
        }
    }

    private static string GetDifficulty(HtmlDocument loadPageRecipe)
    {
        var difficulty = loadPageRecipe.DocumentNode.CssSelect("[class='ckdc-recipe-info']").First().InnerText;
        if (difficulty.Contains("Fácil")) return "Fácil";
        if (difficulty.Contains("Moderado")) return "Media";
        if (difficulty.Contains("Principiante")) return "Fácil";
        return difficulty.Contains("Exigente") ? "Dificil" : "Media";
    }

    private static int GetTimeClean(string time)
    {
        try
        {
            if (time.Contains('m') && !time.Contains('h'))
            {
                if (!time.Contains('+')) return Convert.ToInt32(time.Replace(" ", "").Replace("m", ""));
                time = time.Replace(" ", "").Replace("+", " ").Replace("m", "");
                var split1 = time.Split(" ");
                var numero1 = int.Parse(split1[0]);
                var numero2 = int.Parse(split1[1]);
                var total = numero1 + numero2;
                return total;
            }

            time = time.Replace(" ", "");
            var split2 = time.Split("+");
            var min = int.Parse(split2[0].Replace("m", ""));
            var hora = int.Parse(split2[1].Replace("h", ""));
            var minutosTotales = hora * 60 + min;
            return minutosTotales;
        }
        catch (Exception)
        {
            Console.WriteLine(" error Input Format " + time);
            return 0;
        }
    }

    public ICollection<Ingredient> GetIngredientRecipe(HtmlDocument loadPageRecipe)
    {
        var ingredients = new List<Ingredient>();
        var listIngredient = loadPageRecipe.DocumentNode.CssSelect("[class='recipe-ingredients-list-wrapper']")
            .CssSelect("li");
        foreach (var ingredient in listIngredient)
        {
            try
            {
                var metricUs = ingredient.CssSelect("[class='ingredient-value ingredient-value-us']").First().InnerText
                    .CleanInnerText().Replace(".", "");
                var ingredientClean = ingredient.CssSelect("[class='ingredient-name-singular']").First().InnerText
                    .CleanInnerText().Trim();
                var ingredientCorrect = metricUs + " " + ingredientClean;
                var ingredientText = new Ingredient { IngredientText = ingredientCorrect };
                ingredients.Add(ingredientText);
            }
            catch (Exception)
            {
                var metricUs = ingredient.CssSelect("[class='ingredient-value ingredient-value-us']").First().InnerText
                    .CleanInnerText().Replace(".", "");
                var ingredientClean = ingredient.InnerText.CleanInnerText().Remove(0, metricUs.Length + 1)
                    .Replace(".", "");
                var ingredientText = new Ingredient { IngredientText = ingredientClean.Trim() };
                ingredients.Add(ingredientText);
            }
        }

        return ingredients;
    }

    private static ICollection<string> GetStepRecipe(HtmlDocument loadPageRecipe)
    {
        var steps = loadPageRecipe.DocumentNode.CssSelect("[class='recipe-steps-list']").CssSelect("li")
            .Select(x => x.InnerText.CleanInnerText().Trim()).ToList();
        return steps;
    }

    private static string GetCategory(HtmlDocument loadPageRecipe)
    {
        var categorys = loadPageRecipe.DocumentNode.CssSelect("[class='ckdc-recipe-section ckdc-recipe-section-steps']")
            .First().CssSelect("ol").First().CssSelect("li").First().NextSibling.InnerText;
        return categorys;
    }

    [Obsolete("Obsolete")]
    private static void GetImageRecipe(HtmlDocument loadPageRecipe, string guid)
    {
        try
        {
            using var oclient = new WebClient();
            var imgUri = loadPageRecipe.DocumentNode.CssSelect("[id='huge-feature-image']").First().CssSelect("img")
                .First().GetAttributeValue("src");
            oclient.DownloadFile(new Uri(imgUri),
                $"C:/Users/hello/RiderProjects/WebScrappingRecipes/WebScrappingRecipes/files/image_recipes/{guid}.jpg");
        }
        catch (Exception)
        {
            Console.WriteLine("no uri ");
        }
    }

    public string GetPortionRecipe(HtmlDocument loadPageRecipe)
    {
        try
        {
            var portions = loadPageRecipe.DocumentNode.CssSelect("[class='value hide-on-js']").First().InnerText
                .CleanInnerText();
            return portions;
        }
        catch (Exception)
        {
            Console.WriteLine("No contains Portions Recipe");
            return "";
        }
    }

    [GeneratedRegex("(Fácil|Principiante|Moderado|Exigente)")]
    private static partial Regex RegexTime();
}