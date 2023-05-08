using ElasticSearch.AdvancedAPI.SampleModels;
using ElasticSearch.Models;
using Microsoft.AspNetCore.Mvc;

namespace ElasticSearch.AdvancedAPI.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class ElasticSearchExamplesController : ControllerBase
    {
        private readonly IElasticSearch _elasticSearch;

        public ElasticSearchExamplesController(IElasticSearch elasticSearch)
        {
            _elasticSearch = elasticSearch;
        }

        //Bu örnek, iç içe nesnelerle daha karmaşık bir belge eklemeye gösterir.Yazar ve kitap nesneleri iç içe kullanılarak, daha zengin veri yapılarıyla çalışabileceğiz. Karmaşık yapıyı elasticSearch içerisindeki indexe belge eklemek için kullanacağız. Kullandığımız metotlara dikkat edelim.

        [HttpGet("AddComplexBook")]
        public async Task<IActionResult> AddComplexBook()
        {
            // Yeni bir yazar nesnesi oluşturun
            Author newAuthor = new Author
            {
                Name = "Örnek Yazar",
                Biography = "Örnek Yazar, dünyaca ünlü bir bilim kurgu yazarıdır.",
                BirthYear = 1985
            };

            // Yeni bir kitap nesnesi oluşturun
            Book newBook = new Book
            {
                Title = "Örnek Kitap",
                Author = newAuthor,
                PublicationYear = 2023,
                Categories = new[] { "Roman", "Bilim Kurgu" }
            };

            // ElasticSearchInsertUpdateModel nesnesi oluşturun (InsertAsync fonksiyonunu kullanmak için)
            ElasticSearchInsertUpdateModel insertModel = new()
            {
                IndexName = "books",
                ElasticId = Guid.NewGuid().ToString(),
                Item = newBook
            };

            // InsertAsync yöntemini kullanarak belgeyi ekleyin
            IElasticSearchResult result = await _elasticSearch.InsertAsync(insertModel);

            if (result.Success)
            {
                return Ok(result);
            }
            else
            {
                return BadRequest();
            }
        }

        [HttpGet("SearchBooksByAuthor")]
        public async Task<IActionResult> SearchBooksByAuthor(string value)
        {

            // SearchByFieldParameters nesnesi oluşturun
            //books index'inde Author.Name alanında parametre olarak gönderdiğimiz değeri ara ve sayfalama yaparak geriye döndürür.
            SearchByFieldParameters searchParameters = new SearchByFieldParameters
            {
                IndexName = "books",
                FieldName = "Author.Name",
                Value = value,
                From = 0,
                Size = 10
            };

            // GetSearchByField yöntemini kullanarak belgeleri arayın(Generic olarak bu şekilde kullanılır.)
            List<ElasticSearchGetModel<Book>> searchResults = await _elasticSearch.GetSearchByField<Book>(searchParameters);

            if (searchResults.Count > 0)
            {
                string count = $"Toplam {searchResults.Count} sonuç bulundu:";

                return Ok(searchResults);
            }
            else
            {
                return BadRequest();
            }

        }

        //Çoklu veri eklemek için yani bulk veri eklemek için kullanılan bir yapıdır.

        [HttpGet("AddMultipleBooksAsync")]
        public async Task<IActionResult> AddMultipleBooksAsync()
        {
            // Birden fazla kitap nesnesi oluşturun ve List<Book> içine ekleyin
            List<Book> books = new List<Book>
    {
        new Book
        {
            Title = "Örnek Kitap 1",
            Author = new Author
            {
                Name = "Yazar 1",
                Biography = "Yazar 1, deneme ve öykü türlerinde eserler vermiştir.",
                BirthYear = 1975
            },
            PublicationYear = 2019,
            Categories = new[] { "Deneme", "Öykü" }
        },
        new Book
        {
            Title = "Örnek Kitap 2",
            Author = new Author
            {
                Name = "Yazar 2",
                Biography = "Yazar 2, bilim kurgu ve fantastik romanlar yazmaktadır.",
                BirthYear = 1982
            },
            PublicationYear = 2021,
            Categories = new[] { "Bilim Kurgu", "Fantastik" }
        },
        new Book
    {
        Title = "Örnek Kitap 3",
        Author = new Author
        {
            Name = "Yazar 3",
            Biography = "Yazar 3, tarih ve biyografi türlerinde çalışmalar yapmaktadır.",
            BirthYear = 1965
        },
        PublicationYear = 2018,
        Categories = new[] { "Tarih", "Biyografi" }
    },
        new Book
    {
        Title = "Örnek Kitap 4",
        Author = new Author
        {
            Name = "Yazar 4",
            Biography = "Yazar 4, aşk romanları ve drama üzerine yazmaktadır.",
            BirthYear = 1978
        },
        PublicationYear = 2020,
        Categories = new[] { "Aşk Romanı", "Drama" }
    },
        new Book
    {
        Title = "Örnek Kitap 5",
        Author = new Author
        {
            Name = "Yazar 5",
            Biography = "Yazar 5, polisiye ve gerilim romanları yazmaktadır.",
            BirthYear = 1985
        },
        PublicationYear = 2017,
        Categories = new[] { "Polisiye", "Gerilim" }
    }
    };

            // InsertManyAsync yöntemini kullanarak birden fazla belge ekleyin
            // .ToArray() kullanarak List<Book> nesnesini object[] dizisine dönüştürün
            IElasticSearchResult result = await _elasticSearch.InsertManyAsync("books", books.ToArray());

            // İşlemin başarılı olup olmadığını kontrol edin ve sonucu ekrana yazdırın
            if (result.Success)
            {
                return Ok(result);
            }
            else
            {
                return BadRequest();
            }
        }


        //Girilen parametreye göre bütün alanlar için arama yapmak için bu yapıyı kullanıyoruz. Burada parametre olarak verdiğimiz alan Query ye verilmelidir.
        [HttpGet("SearchAll")]
        public async Task<IActionResult> SearchAll(string value)
        {
            // Parametreleri ayarlayın
            //Bu örnekte, "books" dizininde "title", "author.name" vb. alanlarında kullanıcıdan gelen kelimeyi arayacağız. Ayrıca, title alanının önceliğini yükseltmek için bu alana ^2 ekliyoruz. Bu, başlık alanındaki eşleşmelerin diğer alanlardaki eşleşmelere göre daha önemli olacağı anlamına gelir.
            SearchByQueryParameters queryParameters = new SearchByQueryParameters
            {
                IndexName = "books",
                Query = value,
                Fields = new[] { "title^2", "author.name", "author.biography", "author.birthYear", "publicationYear", "categories" }, // girilen parametre hangi alanlarda arama işlemi gerçekleştirsin. Onları burada belirtiyoruz.
                From = 0,
                Size = 10
            };

            // Oluşturduğumuz searchQueryParameters a göre burada arama işlemi gerçekleştirecek. Burada dikkat etmemiz gereken hususlardan biri bu metot generic olarak kullanılmaktadır.
            List<ElasticSearchGetModel<Book>> searchResults = await _elasticSearch.GetSearchBySimpleQueryString<Book>(queryParameters);

            // İşlemin başarılı olup olmadığını kontrol edin ve sonucu ekrana yazdırın
            if (searchResults.Count > 0)
            {
                return Ok(searchResults);
            }
            else
            {
                return BadRequest();
            }
        }


        //ElasticSearch içerisinde bulunan bütün indekslerin listesini getirir.
        [HttpGet("GetIndexList")]
        public IActionResult GetIndexList()
        {
            // GetIndexList yöntemini çağırarak indeks listesini alın
            var indexList = _elasticSearch.GetIndexList();

            // İşlemin başarılı olup olmadığını kontrol edin ve sonucu ekrana yazdırın
            if (indexList.Count > 0)
            {
                var indexListDetail = indexList.ToList();
                return Ok();
            }
            else
            {
                return BadRequest();
            }
        }

        [HttpGet("CreateSampleIndexAsync")]
        public async Task<IActionResult> CreateSampleIndexAsync(string indexName, string indexAlias)
        {
            //Oluşturulacak bir index için bir model oluşturalım.
            var indexModel = new IndexModel
            {
                IndexName = indexName,
                AliasName = indexAlias,
                NumberOfReplicas = 1,
                NumberOfShards = 5
            };

            /*
             
             
             NumberOfReplicas ve NumberOfShards, Elasticsearch'teki indekslerin performans ve ölçeklenebilirlik özelliklerini etkileyen iki önemli faktördür.

            NumberOfShards (Parça Sayısı): Bir Elasticsearch indeksi, birden fazla parçaya (shard) bölünebilir. Parçalar, indeksinizdeki verilerin alt kümesini temsil eder ve verilerinizi paralel olarak işlemenizi sağlar. Parça sayısını artırmak, genellikle arama ve dizinleme performansını artırır, çünkü işlemler birden fazla parçada eş zamanlı olarak gerçekleştirilebilir. Parça sayısı, indeks oluşturulduğunda belirlenir ve sonradan değiştirilemez. Bu nedenle, başlangıçta doğru bir parça sayısı belirlemek önemlidir.

            NumberOfReplicas (Replika Sayısı): Replikalar, parçaların kopyalarıdır ve yüksek kullanılabilirlik ve hızlı okuma performansı sağlar. Replikalar, başka bir düğümde bulunan birincil parçanın kopyasıdır ve bir düğüm arızalandığında veya aşırı yüklendiğinde istekleri yönlendirebilir. Replika sayısını artırmak, okuma performansını ve hatalara karşı dayanıklılığı artırır. Replika sayısı, indeks oluşturulduktan sonra da değiştirilebilir.

            NumberOfReplicas = 1 ve NumberOfShards = 5 ayarlarıyla, verileriniz 5 parçaya bölünür ve her parçanın bir replikası oluşturulur. Bu, ölçeklenebilirlik ve hata toleransı sağlar ve toplamda 10 parça (5 birincil parça ve 5 replika parça) oluşturur.
             
             
             */




            //Oluşturulan model neticesinde ElasticSearch içerisinde bir index oluştururuz.
            var result = await _elasticSearch.CreateNewIndexAsync(indexModel);

            if (result.Success)
            {
                return Ok(result);
            }
            else
            {
                return BadRequest();
            }
        }
    }
}
