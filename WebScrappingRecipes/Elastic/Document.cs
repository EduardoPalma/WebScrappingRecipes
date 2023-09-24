using Nest;

namespace WebScrappingRecipes.Elastic;

public abstract class Document
{
    public JoinField Join { get; set; } = null!;
}