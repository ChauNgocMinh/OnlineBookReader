using AutoMapper;
using OnlineBookReader.Models;

namespace OnlineBookReader.Helper
{
    public class AutoMapping : Profile
    {
        public AutoMapping()
        {
            // Book <-> BookViewModel
            CreateMap<Book, Author>().ReverseMap();
        }
    }
}
