namespace OnlineBookReader.Models
{
    public class Book : BaseEntity
    {
        public Guid Id { get; set; }
        public string Title { get; set; } = null!;

        public Guid CategoryId { get; set; }
        public Category Category { get; set; } = null!;

        public Guid AuthorId { get; set; }
        public Author Author { get; set; } = null!;

        public bool Type { get; set; }
        public string? UrlImage { get; set; }
        public string? ShortDescription { get; set; } 
        public string? Content { get; set; } 
        public virtual List<Chapter> Chapters { get; set; } 
    }   
}
