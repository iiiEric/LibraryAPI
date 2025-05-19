using AutoMapper;
using LibraryAPI.Controllers.V1;
using LibraryAPI.Data;
using LibraryAPI.DatabaseAccess.AuthorsRepository;
using LibraryAPI.DTOs;
using LibraryAPI.Entities;
using LibraryAPI.Services;
using LibraryAPI.UseCases.Authors.Delete;
using LibraryAPI.UseCases.Authors.GetAll;
using LibraryAPI.UseCases.Authors.GetByCriteria;
using LibraryAPI.UseCases.Authors.GetById;
using LibraryAPI.UseCases.Authors.Patch;
using LibraryAPI.UseCases.Authors.Post;
using LibraryAPI.UseCases.Authors.PostWithImage;
using LibraryAPI.UseCases.Authors.Put;
using LibraryAPITests.UnitTests.Builders;
using LibraryAPITests.Utilidades;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.JsonPatch;
using Microsoft.AspNetCore.JsonPatch.Operations;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using Microsoft.AspNetCore.OutputCaching;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using NSubstitute;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


namespace LibraryAPITests.UnitTests.Controllers.V1
{
    [TestClass]
    public class AuthorsControllerTests: TestBase
    {
        #region Test Data
        private Author _defaultAuthor = new AuthorBuilder().WithName("George Raymond").WithSurname1("Richard").Build();
        private Author _defaultAuthor2 = new AuthorBuilder().WithName("John Ronald").WithSurname1("Reuel").Build();
        private AuthorDTO _defaultAuthorDTO = new AuthorDTOBuilder().WithId(1).WithFullName("George Raymond Richard").Build();
        private AuthorCreationDTO _defaultAuthorCreationDTO = new AuthorCreationDTOBuilder().WithName("George Raymond").WithSurname1("Richard").Build();
        private AuthorFilterDTO _defaultAuthorFilterDTO = new AuthorFilterDTOBuilder().WithName("George Raymond").WithSurname1("Richard").WithBookTitle("Game of thrones").Build();
        private AuthorFilterDTO _nonMatchingFilterDTO = new AuthorFilterDTOBuilder().WithName("Zacarias").WithSurname1("Z").Build();
        private AuthorWithBooksDTO _defaultAuthorWithBooksDTO = new AuthorWithBooksDTOBuilder().WithId(1).WithFullName("George Raymond Richard")
            .WithBooks(new List<BookDTO>{new BookDTO { Id = 1, Title = "Game of thrones" }}).Build();
        #endregion

        ApplicationDbContext _context = null!;
        IMapper _mapper = null!;
        IFileStorageService _fileStorageService = null!;
        ILogger<AuthorsController> _logger = null!;
        IOutputCacheStore _outputCacheStore = null!;
        IAuthorsGetAllUseCase _authorsGetAllUseCase = null!;
        IAuthorsGetByCriteriaUseCase _authorsGetByCriteriaUseCase = null!;
        IAuthorGetByIdUseCase _authorGetByIdUseCase = null!;
        IAuthorPostUseCase _authorPostUseCase = null!;
        IAuthorPostWithImageUseCase _authorPostWithImageUseCase = null!;
        IAuthorPutUseCase _authorPutUseCase = null!;
        IAuthorPatchUseCase _authorPatchUseCase = null!;
        IDeleteAuthorUseCase _deleteAuthorUseCase = null!;
        private string _databaseName = Guid.NewGuid().ToString();
        private AuthorsController _controller = null!;

        [TestInitialize]
        public void Setup()
        {
            _context = BuildContext(_databaseName);
            _mapper = ConfigureAutoMapper();
            _fileStorageService = Substitute.For<IFileStorageService>();
            _logger = Substitute.For<ILogger<AuthorsController>>();
            _outputCacheStore = Substitute.For<IOutputCacheStore>();
            _authorsGetAllUseCase = Substitute.For<IAuthorsGetAllUseCase>();
            _authorsGetByCriteriaUseCase = Substitute.For<IAuthorsGetByCriteriaUseCase>();
            _authorGetByIdUseCase = Substitute.For<IAuthorGetByIdUseCase>();
            _authorPostUseCase = Substitute.For<IAuthorPostUseCase>();
            _authorPostWithImageUseCase = Substitute.For<IAuthorPostWithImageUseCase>();
            _authorPutUseCase = Substitute.For<IAuthorPutUseCase>();
            _authorPatchUseCase = Substitute.For<IAuthorPatchUseCase>();
            _deleteAuthorUseCase = Substitute.For<IDeleteAuthorUseCase>();
            _controller = new AuthorsController(_authorsGetAllUseCase, _authorsGetByCriteriaUseCase, _authorGetByIdUseCase, _authorPostUseCase, _authorPostWithImageUseCase,
                _authorPutUseCase, _authorPatchUseCase, _deleteAuthorUseCase);
        }

        [TestMethod]
        public async Task Get_WhenAuthorIdDoesNotExist_ReturnsNotFound()
        {
            // Arrange
            _authorGetByIdUseCase.Run(999).Returns((AuthorWithBooksDTO?)null);

            // Act
            var response = await _controller.Get(1);

            // Assert
            var result = response.Result as NotFoundResult;
            Assert.IsNotNull(result);
            Assert.AreEqual(StatusCodes.Status404NotFound, result!.StatusCode);
        }

        [TestMethod]
        public async Task Get_WhenAuthorIdDoesExist_ReturnsAuthor()
        {
            // Arrange
            _authorGetByIdUseCase.Run(1).Returns(_defaultAuthorWithBooksDTO);

            // Act
            var response = await _controller.Get(1);

            // Assert
            var result = response.Result as OkObjectResult;
            Assert.IsNotNull(result);

            var authorWithBooksDTO = result!.Value as AuthorWithBooksDTO;
            Assert.IsNotNull(authorWithBooksDTO);
            Assert.AreEqual(1, authorWithBooksDTO!.Id);
        }

        [TestMethod]
        public async Task GetByCriteria_WhenNoMatch_ReturnsEmptyList()
        {
            // Arrange     
            _authorsGetByCriteriaUseCase.Run(_nonMatchingFilterDTO).Returns(Enumerable.Empty<AuthorWithBooksDTO>());

            // Act
            var response = await _controller.GetByCriteria(_nonMatchingFilterDTO);

            // Assert
            var result = response.Result as OkObjectResult;
            Assert.IsNotNull(result);

            var authorsList = result!.Value as IEnumerable<AuthorWithBooksDTO>;
            Assert.IsNotNull(authorsList);
            Assert.AreEqual(0, authorsList.Count());
        }

        [TestMethod]
        public async Task GetByCriteria_WhenMatchesExist_ReturnsAuthorWithBooksDTOList()
        {
            // Arrange
            _authorsGetByCriteriaUseCase.Run(_defaultAuthorFilterDTO).Returns(new List<AuthorWithBooksDTO> { _defaultAuthorWithBooksDTO });

            // Act
            var response = await _controller.GetByCriteria(_defaultAuthorFilterDTO);

            // Assert
            var result = response.Result as OkObjectResult;
            Assert.IsNotNull(result);

            var authorsList = result.Value as IEnumerable<AuthorWithBooksDTO>;
            Assert.IsNotNull(authorsList);
            Assert.IsTrue(authorsList.Any());
        }


        [TestMethod]
        public async Task Post_WhenValidAuthorProvided_CreatesNewAuthor()
        {
            // Arrange
            _authorPostUseCase.Run(_defaultAuthorCreationDTO).Returns(_defaultAuthorDTO);

            // Act
            var response = await _controller.Post(_defaultAuthorCreationDTO);

            // Assert
            var result = response as CreatedAtRouteResult;
            Assert.IsNotNull(result);
            Assert.AreEqual(201, result!.StatusCode);
        }
    }
}
