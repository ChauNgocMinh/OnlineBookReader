namespace OnlineBookReader.Models
{
    public class ChapterContent
    {
        public Guid Id { get; set; }
        public Guid ChapterId { get; set; }
        public string Content { get; set; }
        public Chapter Chapter { get; set; }
    }
}
