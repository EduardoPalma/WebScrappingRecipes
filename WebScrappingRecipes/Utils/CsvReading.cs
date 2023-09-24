namespace WebScrappingRecipes.Utils;

public static class CsvReading
{
    private const string Path =
        $"C:/Users/hello/RiderProjects/WebScrappingRecipes/WebScrappingRecipes/files/recipe_category.txt";

    private const string PathDiccionary =
        $"C:/Users/hello/RiderProjects/WebScrappingRecipes/WebScrappingRecipes/files/diccionary_category.txt";

    private const string PathCategoryCtc =
        $"C:/Users/hello/RiderProjects/WebScrappingRecipes/WebScrappingRecipes/files/category_recipe_comidas_tipicas.txt";

    private const string PathCategoryGuidingStar =
        $"C:/Users/hello/RiderProjects/WebScrappingRecipes/WebScrappingRecipes/files/guiding_category.txt";

    private static readonly ICollection<AuxCategoryRecipe> CategoryRecipes = ReadCategoryRecipes();

    private static ICollection<AuxCategoryRecipe> ReadCategoryRecipes()
    {
        var auxCategoryRecipeFiles = File.ReadAllLines(Path);
        var auxCategoryRecipeObject = auxCategoryRecipeFiles.Select(x => x.Split(","));
        var categoryRecipeObject = auxCategoryRecipeObject.ToList();
        var urlRecipe = categoryRecipeObject.Select(x => x[0]).ToList();
        var categoryFoodsDay = categoryRecipeObject.Select(x => x[2]).ToList();
        var categorys = categoryRecipeObject.Select(x => x[1]).ToList();

        return urlRecipe.Select((t, i) => new AuxCategoryRecipe
            { Url = t, CategoryRecipe = categorys[i].Split(";"), FoodDays = categoryFoodsDay[i].Split(";") }).ToList();
    }

    private static IDictionary<string, List<string>> GetCategoryAndFoodDayRecipe()
    {
        var dictionaryFoodDays = new Dictionary<string, List<string>>();
        var lineFile = File.ReadAllLines(PathDiccionary);
        var auxLineFile = lineFile.Select(x => x.Split(","));
        foreach (var foodDay in auxLineFile)
        {
            var nameFoodDay = foodDay[0];
            var categorys = foodDay[1].Split(";").ToList();
            dictionaryFoodDays.Add(nameFoodDay, categorys);
        }

        return dictionaryFoodDays;
    }

    public static ICollection<string> CategoryRecipe(string url)
    {
        var category = CategoryRecipes.ToList().Find(x => x.Url.Equals(url));
        return category != null ? category.CategoryRecipe : new List<string>();
    }

    public static ICollection<string> FoodDayRecipe(string url)
    {
        var foodDay = CategoryRecipes.ToList().Find(x => x.Url.Equals(url));
        return foodDay != null ? foodDay.FoodDays : new List<string>();
    }

    public static (ICollection<string>, ICollection<string>) GetFoodDaysAndCategory(string category)
    {
        var foodDayDiccionary = GetCategoryAndFoodDayRecipe();
        var foodDayRecipe = new List<string>();
        var categoryRecipe = new List<string>();
        foreach (var keyValuePair in foodDayDiccionary)
        {
            categoryRecipe.AddRange(keyValuePair.Value.Where(categoryKey => categoryKey.Equals(category)));
            if (keyValuePair.Value.Contains(category)) foodDayRecipe.Add(keyValuePair.Key);
        }

        return (foodDayRecipe, categoryRecipe);
    }

    public static (ICollection<string> foodDays, ICollection<string> categoryRecipe) GetCategoryRecipeCtc(int numUrl)
    {
        var lines = File.ReadAllLines(PathCategoryCtc).Select(x => x.Split(",")).ToList();
        var categoryRecipe = lines[numUrl][0].Split(";");
        var foodDays = lines[numUrl][1].Split(";");
        return (foodDays, categoryRecipe);
    }

    public static (ICollection<string> foodDays, ICollection<string> categoryRecipe) GetCategoryRecipeGs(
        ICollection<string> categoris)
    {
        var fileCategoris = File.ReadAllLines(PathCategoryGuidingStar).Select(x => x.Split(";")).ToList();
        var foodDays = new List<string>();
        var categoryRecipe = new List<string>();
        foreach (var category in categoris)
        {
            foreach (var fileCategory in fileCategoris.Where(fileCategory => category.Equals(fileCategory[0])))
            {
                AddCategoryAndFoodDay(foodDays, categoryRecipe, fileCategory);
            }
        }

        return (foodDays, categoryRecipe);
    }

    private static void AddCategoryAndFoodDay(ICollection<string> foodDays, ICollection<string> categoryRecipe,
        IReadOnlyList<string> fileCategory)
    {
        var separate = fileCategory[1].Split(".");
        var foodDaysFile = separate[0].Split("|");
        var categorysFile = separate[1].Split("|");
        foreach (var foodDayFile in foodDaysFile)
        {
            if (!foodDays.Contains(foodDayFile)) foodDays.Add(foodDayFile);
        }

        foreach (var categoriFile in categorysFile)
        {
            if (!categoryRecipe.Contains(categoriFile)) categoryRecipe.Add(categoriFile);
        }
    }
}