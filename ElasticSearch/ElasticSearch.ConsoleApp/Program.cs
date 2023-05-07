using ElasticSearch.ConsoleApp;
using Nest;

//ElasticSearch sunucuna bağlanmak.
ConnectionSettings settings = new ConnectionSettings(new Uri("http://localhost:9200"))
    .DefaultIndex("myindex");

//Belirtilen bağlantı ayarlarına göre bir client nesnesi oluşturalım.
var client = new ElasticClient(settings);

//ElasticSearch içerisinde saklanacak olan örnek bir veri.
var myDocument = new MyDocument
{
    Id = 1,
    Title = "ElasticSearch Öğrenme",
    Content = "ElasticSearch güçlü bir arama motorudur."
};

//Oluşturulan veriyi elasticSearch üzerinde indexledik.
var indexResponse = await client.IndexDocumentAsync(myDocument);


//İndexlenen veriler üzerinden bir arama işlemi gerçekleştirelim.

string keyword = "ElasticSearch";

/*
 
 Bu örnekte, keyword adlı değişken aranacak anahtar kelimeyi içerir. From ve Size parametreleri, sonuçların sayfalanmasını sağlar. Bu örnekte, ilk sayfada en fazla 10 sonuç döndürülür.
 
 */

//Arama işlemini SearchAsync fonksiyonu üzerinden sayfalama işlemi yaparak gerçekleştirebiliriz.
var searchResponse = await client.SearchAsync<MyDocument>(s => s
    .From(0)
    .Size(10)
    .Query(q => q
         .Match(m => m
            .Field(f => f.Title)
            .Query(keyword)
         )
    )
);


if (searchResponse.IsValid)
{
    Console.WriteLine($"Toplam {searchResponse.Total} sonuç bulundu.");

    foreach (var hit in searchResponse.Hits)
    {
        Console.WriteLine($"Belge Id: {hit.Id} - Başlık: {hit.Source.Title}");
    }
}
else
{
    Console.WriteLine("Sorgu başarısız oldu: " + searchResponse.DebugInformation);
}