using WebScrappingRecipes.ScrapedRecipes.ScratchPages;


#pragma warning disable CS0618

Console.WriteLine("Obteniendo Recetas Nutrium");
var nutrium = new Nutrium();
nutrium.GetInformationRecipes(13);
Console.WriteLine("Obteniendo Recetas Diet Doctor");
var dietDoctor = new DietDoctor();
dietDoctor.GetInformationRecipes(121);
Console.WriteLine("Obteniendo Recetas Comidas Tipicas Chilenas");
var comidas = new ComidasTipicasChilenas();
comidas.GetInformationRecipes(8);
Console.WriteLine("Obteniendo Recetas GuidingStarts");
var guiding = new GuidingStarts();
guiding.GetInformationRecipes(13);
#pragma warning restore CS0618