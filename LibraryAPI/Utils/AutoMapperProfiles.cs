using AutoMapper;
using LibraryAPI.DTOs;
using LibraryAPI.Entities;
using LibraryAPI.Utils;

namespace LibraryAPI.Utils
{
    public class AutoMapperProfiles: Profile
    {
        public AutoMapperProfiles()
        {
            CreateMap<Author, AuthorDTO>()
                .ForMember(dto => dto.FullName, config => config.MapFrom(author => NameFormatter.GetAuthorFullName(author)))
                .ReverseMap();

            CreateMap<Author, AuthorWithBooksDTO>()
                .ForMember(dto => dto.FullName, config => config.MapFrom(author => NameFormatter.GetAuthorFullName(author)))
                .ForMember(dto => dto.Books, config => config.MapFrom(author => author.Books))
                .ReverseMap();

            CreateMap<AuthorCreationDTO, Author>();

            CreateMap<Book, BookDTO>()
                .ForMember(dto => dto.Title, config => config.MapFrom(book => book.Title))
                .ReverseMap();

            CreateMap<Book, BookWithAuthorDTO>()
               .ForMember(dto => dto.Title, config => config.MapFrom(book => book.Title))
               .ForMember(dto => dto.AuthorName, config => config.MapFrom(book => NameFormatter.GetAuthorFullName(book.Author)))
               .ReverseMap();

            CreateMap<BookCreationDTO, Book>();
        }
    }
}
