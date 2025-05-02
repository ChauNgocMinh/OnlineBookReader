namespace OnlineBookReader.Models
{
    public class ChapterImage
    {
        public Guid Id { get; set; }
        public Guid ChapterId { get; set; }
        public string ImageUrl { get; set; }
        public int PageOrder { get; set; }
        public Chapter Chapter { get; set; }
    }
}
