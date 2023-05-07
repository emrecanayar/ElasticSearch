namespace ElasticSearch.AdvancedAPI.SampleModels
{
    public class Book
    {
        public string Title { get; set; }
        public Author Author { get; set; }
        public int PublicationYear { get; set; }
        public string[] Categories { get; set; }
    }
}
