namespace OnlineBookReader.Models
{
    public class Category : BaseEntity
    {
        public Guid Id { get; set; }
        public string Name { get; set; } = null!;
        public ICollection<Book> Books { get; set; } = new List<Book>();
    }
}
