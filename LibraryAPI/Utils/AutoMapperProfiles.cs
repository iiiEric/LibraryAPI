﻿using AutoMapper;
using LibraryAPI.DTOs;
using LibraryAPI.DTOs.Users;
using LibraryAPI.Entities;
using LibraryAPI.Utils;

namespace LibraryAPI.Utils
{
    public class AutoMapperProfiles: Profile
    {
        public AutoMapperProfiles()
        {
            CreateMap<Author, AuthorDTO>()
                .ForMember(dto => dto.FullName, config => config.MapFrom(ent => NameFormatter.GetAuthorFullName(ent)));

            CreateMap<Author, AuthorWithBooksDTO>()
                .ForMember(dto => dto.FullName, config => config.MapFrom(ent => NameFormatter.GetAuthorFullName(ent)));

            CreateMap<AuthorCreationDTO, Author>();
            CreateMap<AuthorCreationWithImageDTO, Author>()
                .ForMember(ent => ent.ImageUrl, config => config.Ignore());

            CreateMap<Author, AuthorPatchDTO>().ReverseMap();
           

            CreateMap<Book, BookDTO>()
                .ForMember(dto => dto.Title, config => config.MapFrom(ent => ent.Title));

            CreateMap<Book, BookWithAuthorsDTO>();

            CreateMap<BookCreationDTO, Book>()
                .ForMember(ent => ent.Authors, config => config.MapFrom(dto => dto.AuthorsIds.Select(id => new AuthorBook { AuthorId = id })));

            CreateMap<BookCreationDTO, AuthorBook>()
                .ForMember(ent => ent.Book, config => config.MapFrom(dto => new Book { Title = dto.Title }));


            CreateMap<AuthorBook, BookDTO>()
                .ForMember(dto => dto.Id, config => config.MapFrom(ent => ent.BookId))
                .ForMember(dto => dto.Title, config => config.MapFrom(ent => ent.Book!.Title));

            CreateMap<AuthorBook, AuthorDTO>()
                .ForMember(dto => dto.Id, config => config.MapFrom(ent => ent.AuthorId))
                .ForMember(dto => dto.FullName, config => config.MapFrom(ent => NameFormatter.GetAuthorFullName(ent.Author!)));


            CreateMap<CommentCreationDTO, Comment>();
            CreateMap<Comment, CommentDTO>()
                .ForMember(dto => dto.UserEmail, config => config.MapFrom(ent => ent.User!.Email));
            CreateMap<CommentPatchDTO, Comment>().ReverseMap();

            CreateMap<User, UserDTO>();
        }
    }
}
