using Elasticsearch.Net;
using Nest;
using WebScrappingRecipes.ScrapedRecipes;

namespace WebScrappingRecipes.Elastic;

public static class Elastic
{
    private static readonly ElasticClient ClientSpanish = ConnectElastic();
    private static readonly ElasticClient ClientEnglish = ConnectElastic();

    private static ElasticClient ConnectElastic()
    {
        var pool = new SingleNodeConnectionPool(new Uri($"https://localhost:9200"));
        var settings = new ConnectionSettings(pool)
            .BasicAuthentication("elastic", "GSc_a89P7pd*6*m6Q0oF").EnableApiVersioningHeader()
            .ServerCertificateValidationCallback(CertificateValidations.AllowAll)
            .DisableDirectStreaming();
        var client = new ElasticClient(settings);
        return client;
    }

    public static void CreateIndex(string indexName, int lenguejeIndex)
    {
        if (lenguejeIndex == 1)
        {
            var createIndexResponse = ClientSpanish.Indices.Create(indexName, c => c
                .Map<Document>(m => m
                    .AutoMap<Recipes>()
                )
            );
            if (!createIndexResponse.IsValid) Console.WriteLine(createIndexResponse.DebugInformation);
        }
        else
        {
            var createIndexResponse = ClientEnglish.Indices.Create(indexName, c => c
                .Map<Document>(m => m
                    .AutoMap<Recipes>()
                )
            );
            if (!createIndexResponse.IsValid) Console.WriteLine(createIndexResponse.DebugInformation);
        }
    }

    public static void AddRecipe(string indexName, Recipes recipe)
    {
        var indexResponse = ClientSpanish.Index(recipe, d => d.Index(indexName));
        if (!indexResponse.IsValid) Console.WriteLine(indexResponse.DebugInformation);
    }
    
    public static void AddMultipleRecipes(string indexName, IEnumerable<Recipes> recipes)
    {
        var bulkIndexResponse = ClientSpanish.Bulk(b => b
            .Index(indexName)
            .IndexMany(recipes)
        );
        if (!bulkIndexResponse.IsValid) Console.WriteLine(bulkIndexResponse.DebugInformation);
    }
}