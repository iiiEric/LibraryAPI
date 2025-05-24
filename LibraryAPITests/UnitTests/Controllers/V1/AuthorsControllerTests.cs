using LibraryAPI.Controllers.V1;
using LibraryAPI.DTOs;
using LibraryAPI.UseCases.Authors.Delete;
using LibraryAPI.UseCases.Authors.GetAll;
using LibraryAPI.UseCases.Authors.GetByCriteria;
using LibraryAPI.UseCases.Authors.GetById;
using LibraryAPI.UseCases.Authors.Patch;
using LibraryAPI.UseCases.Authors.Post;
using LibraryAPI.UseCases.Authors.PostWithImage;
using LibraryAPI.UseCases.Authors.Put;
using LibraryAPI.Utils;
using LibraryAPITests.UnitTests.Builders;
using LibraryAPITests.Utilidades;
using Microsoft.AspNetCore.JsonPatch;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using NSubstitute;
using System.Text;


namespace LibraryAPITests.UnitTests.Controllers.V1
{
    [TestClass]
    public class AuthorsControllerTests: TestBase
    {
        #region Test Data
        private AuthorDTO _defaultAuthorDTO = new AuthorDTOBuilder().WithId(1).WithFullName("George Raymond Richard").Build();
        private AuthorCreationDTO _defaultAuthorCreationDTO = new AuthorCreationDTOBuilder().WithName("George Raymond").WithSurname1("Richard").Build();
        private AuthorFilterDTO _defaultAuthorFilterDTO = new AuthorFilterDTOBuilder().WithName("George Raymond").WithSurname1("Richard").WithBookTitle("Game of thrones").Build();
        private AuthorFilterDTO _nonMatchingFilterDTO = new AuthorFilterDTOBuilder().WithName("Zacarias").WithSurname1("Z").Build();
        private AuthorWithBooksDTO _defaultAuthorWithBooksDTO = new AuthorWithBooksDTOBuilder().WithId(1).WithFullName("George Raymond Richard")
            .WithBooks(new List<BookDTO> {new BookDTO { Id = 1, Title = "Game of thrones" }}).Build();
        #endregion

        IAuthorsGetAllUseCase _authorsGetAllUseCase = null!;
        IAuthorsGetByCriteriaUseCase _authorsGetByCriteriaUseCase = null!;
        IAuthorGetByIdUseCase _authorGetByIdUseCase = null!;
        IAuthorPostUseCase _authorPostUseCase = null!;
        IAuthorPostWithImageUseCase _authorPostWithImageUseCase = null!;
        IAuthorPutUseCase _authorPutUseCase = null!;
        IAuthorPatchUseCase _authorPatchUseCase = null!;
        IAuthorDeleteUseCase _authorDeleteUseCase = null!;
        private string _databaseName = Guid.NewGuid().ToString();
        private AuthorsController _controller = null!;

        [TestInitialize]
        public void Setup()
        {
            _authorsGetAllUseCase = Substitute.For<IAuthorsGetAllUseCase>();
            _authorsGetByCriteriaUseCase = Substitute.For<IAuthorsGetByCriteriaUseCase>();
            _authorGetByIdUseCase = Substitute.For<IAuthorGetByIdUseCase>();
            _authorPostUseCase = Substitute.For<IAuthorPostUseCase>();
            _authorPostWithImageUseCase = Substitute.For<IAuthorPostWithImageUseCase>();
            _authorPutUseCase = Substitute.For<IAuthorPutUseCase>();
            _authorPatchUseCase = Substitute.For<IAuthorPatchUseCase>();
            _authorDeleteUseCase = Substitute.For<IAuthorDeleteUseCase>();
            _controller = new AuthorsController(_authorsGetAllUseCase, _authorsGetByCriteriaUseCase, _authorGetByIdUseCase, _authorPostUseCase, _authorPostWithImageUseCase,
                _authorPutUseCase, _authorPatchUseCase, _authorDeleteUseCase);
        }

        [TestMethod]
        public async Task Get_WhenAuthorIdDoesNotExist_ReturnsNotFound()
        {
            // Arrange
            _authorGetByIdUseCase.Run(1).Returns((AuthorWithBooksDTO?)null);

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
            Assert.AreEqual(StatusCodes.Status201Created, result!.StatusCode);
        }

        [TestMethod]
        public async Task PostWithImage_WhenValidAuthorProvided_CreatesNewAuthor()
        {
            // Arrange
            string fileName = "test-image.jpg";
            string content = "fake image content";
            byte[] contentBytes = Encoding.UTF8.GetBytes(content);
            MemoryStream stream = new MemoryStream(contentBytes);

            IFormFile mockedImage = Substitute.For<IFormFile>();
            mockedImage.FileName.Returns(fileName);
            mockedImage.Length.Returns(stream.Length);
            mockedImage.OpenReadStream().Returns(stream);
            mockedImage.ContentType.Returns("image/jpeg");
            mockedImage.Name.Returns("Image");
            AuthorCreationWithImageDTO _defaultAuthorCreationWithImageDTO = new AuthorCreationWithImageDTOBuilder().WithName("George Raymond").WithSurname1("Richard")
                .WithImage(mockedImage).Build();
            _authorPostWithImageUseCase.Run(_defaultAuthorCreationWithImageDTO).Returns(_defaultAuthorDTO);

            // Act
            var response = await _controller.PostWithImage(_defaultAuthorCreationWithImageDTO);

            // Assert
            var result = response as CreatedAtRouteResult;
            Assert.IsNotNull(result);
            Assert.AreEqual(StatusCodes.Status201Created, result!.StatusCode);
        }

        [TestMethod]
        public async Task Put_WhenAuthorIdDoesNotExist_ReturnsNotFound()
        {
            // Arrange
            string fileName = "test-image.jpg";
            string content = "fake image content";
            byte[] contentBytes = Encoding.UTF8.GetBytes(content);
            MemoryStream stream = new MemoryStream(contentBytes);

            IFormFile mockedImage = Substitute.For<IFormFile>();
            mockedImage.FileName.Returns(fileName);
            mockedImage.Length.Returns(stream.Length);
            mockedImage.OpenReadStream().Returns(stream);
            mockedImage.ContentType.Returns("image/jpeg");
            mockedImage.Name.Returns("Image");
            AuthorCreationWithImageDTO _defaultAuthorCreationWithImageDTO = new AuthorCreationWithImageDTOBuilder().WithName("George Raymond").WithSurname1("Richard")
                .WithImage(mockedImage).Build();
            _authorPutUseCase.Run(1, _defaultAuthorCreationWithImageDTO).Returns(Task.FromResult(Result.NotFound()));

            // Act
            var response = await _controller.Put(1, _defaultAuthorCreationWithImageDTO);

            // Assert
            var result = response as NotFoundResult;
            Assert.IsNotNull(result);
            Assert.AreEqual(StatusCodes.Status404NotFound, result!.StatusCode);
        }


        [TestMethod]
        public async Task Put_WhenAuthorIdDoesExist_UpdatesAuthor()
        {
            // Arrange
            string fileName = "test-image.jpg";
            string content = "fake image content";
            byte[] contentBytes = Encoding.UTF8.GetBytes(content);
            MemoryStream stream = new MemoryStream(contentBytes);

            IFormFile mockedImage = Substitute.For<IFormFile>();
            mockedImage.FileName.Returns(fileName);
            mockedImage.Length.Returns(stream.Length);
            mockedImage.OpenReadStream().Returns(stream);
            mockedImage.ContentType.Returns("image/jpeg");
            mockedImage.Name.Returns("Image");
            AuthorCreationWithImageDTO _defaultAuthorCreationWithImageDTO = new AuthorCreationWithImageDTOBuilder().WithName("George Raymond").WithSurname1("Richard")
                .WithImage(mockedImage).Build();
            _authorPutUseCase.Run(1, _defaultAuthorCreationWithImageDTO).Returns(Task.FromResult(Result.Success()));

            // Act
            var response = await _controller.Put(1, _defaultAuthorCreationWithImageDTO);

            // Assert
            var result = response as NoContentResult;
            Assert.IsNotNull(result);
            Assert.AreEqual(StatusCodes.Status204NoContent, result!.StatusCode);
        }

        [TestMethod]
        public async Task Patch_WhenPatchDocumentIsNull_ReturnsBadRequest()
        {
            // Arrange
            _authorPatchUseCase.Run(1, null, Arg.Any<ModelStateDictionary>()).Returns(Task.FromResult(Result.BadRequest()));

            // Act
            var response = await _controller.Patch(1, null!);

            // Assert
            var result = response as BadRequestResult;
            Assert.IsNotNull(result);
            Assert.AreEqual(StatusCodes.Status400BadRequest, result!.StatusCode);
        }

        [TestMethod]
        public async Task Patch_WhenAuthorIdDoesNotExist_ReturnsNotFound()
        {
            // Arrange
            var patchDoc = new JsonPatchDocument<AuthorPatchDTO>();
            _authorPatchUseCase.Run(1, patchDoc, Arg.Any<ModelStateDictionary>()).Returns(Task.FromResult(Result.NotFound()));

            // Act
            var response = await _controller.Patch(1, patchDoc);

            // Assert
            var result = response as NotFoundResult;
            Assert.IsNotNull(result);
            Assert.AreEqual(StatusCodes.Status404NotFound, result!.StatusCode);
        }

        [TestMethod]
        public async Task Patch_WhenValidPatch_ReturnsNoContent()
        {
            // Arrange
            var patchDoc = new JsonPatchDocument<AuthorPatchDTO>();
            patchDoc.Replace(a => a.Name, "Isabel");
            patchDoc.Replace(a => a.Surname1, "Allende");

            // Simular que el caso de uso devuelve true (actualizado correctamente)
            _authorPatchUseCase.Run(1, patchDoc, Arg.Any<ModelStateDictionary>()).Returns(Task.FromResult(Result.Success()));

            // Act
            var response = await _controller.Patch(1, patchDoc);

            // Assert
            var result = response as NoContentResult;
            Assert.IsNotNull(result);
            Assert.AreEqual(StatusCodes.Status204NoContent, result!.StatusCode);
        }

        [TestMethod]
        public async Task Delete_WhenAuthorIdDoesNotExist_ReturnsNotFound()
        {
            // Arrange
            _authorDeleteUseCase.Run(1).Returns(Task.FromResult(Result.NotFound()));

            // Act
            var response = await _controller.Delete(1);

            // Assert
            var result = response as NotFoundResult;
            Assert.IsNotNull(result);
            Assert.AreEqual(StatusCodes.Status404NotFound, result!.StatusCode);
        }


        [TestMethod]
        public async Task Delete_WhenAuthorIdDoesExist_DeletesAuthor()
        {
            // Arrange
            _authorDeleteUseCase.Run(1).Returns(Task.FromResult(Result.Success()));

            // Act
            var response = await _controller.Delete(1);

            // Assert
            var result = response as NoContentResult;
            Assert.IsNotNull(result);
            Assert.AreEqual(StatusCodes.Status204NoContent, result!.StatusCode);
        }
    }
}
