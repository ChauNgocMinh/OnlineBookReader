namespace OnlineBookReader.Models
{
    public class Chapter
    {
        public Guid Id { get; set; }
        public Guid BookId { get; set; }
        public string Title { get; set; }
        public int OrderNumber { get; set; }
        public Book Book { get; set; }
        public ICollection<ChapterContent> ChapterContents { get; set; }
        public ICollection<ChapterImage> ChapterImages { get; set; }
    }
}
