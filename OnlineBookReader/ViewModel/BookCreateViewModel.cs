namespace OnlineBookReader.ViewModel
{
    public class BookCreateViewModel
    {
        public string Title { get; set; }
        public Guid CategoryId { get; set; }
        public Guid AuthorId { get; set; }
        public IFormFile CoverImageFile { get; set; } 
        public string UrlImage { get; set; }
        public string ShortDescription { get; set; }

        public List<ChapterCreateViewModel> Chapters { get; set; } = new();
    }

    public class ChapterCreateViewModel
    {
        public string Title { get; set; }
        public int OrderNumber { get; set; }
        public bool IsText { get; set; }
        public string? HtmlContent { get; set; }
        public List<IFormFile>? Images { get; set; }
    }
}
