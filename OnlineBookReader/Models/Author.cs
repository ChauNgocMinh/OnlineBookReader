namespace OnlineBookReader.Models
{
    public class Author : BaseEntity
    {
        public Guid Id { get; set; }
        public string Name { get; set; } = null!;
        public string? Bio { get; set; } 
        public ICollection<Book> Books { get; set; } = new List<Book>();
    }
}
