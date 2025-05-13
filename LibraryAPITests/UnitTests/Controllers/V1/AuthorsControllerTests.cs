using AutoMapper;
using LibraryAPI.Controllers.V1;
using LibraryAPI.Data;
using LibraryAPI.DTOs;
using LibraryAPI.Entities;
using LibraryAPI.Services;
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
        private AuthorCreationDTO _defaultAuthorCreationDTO = new AuthorCreationDTOBuilder().WithName("George Raymond").WithSurname1("Richard").Build();
        #endregion

        ApplicationDbContext _context = null!;
        IMapper _mapper = null!;
        IFileStorageService _fileStorageService = null!;
        ILogger<AuthorsController> _logger = null!;
        IOutputCacheStore _outputCacheStore = null!;
        //IServicioAutores _servicioAutores = null!;
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
            _controller = new AuthorsController(_context, _mapper, _logger, _fileStorageService, _outputCacheStore);
        }

        [TestMethod]
        public async Task Get_WhenAuthorIdDoesNotExist_ReturnsNotFound()
        {
            // Act
            var response = await _controller.Get(1);

            // Assert
            var result = response.Result as NotFoundObjectResult;
            Assert.IsNotNull(result);
            Assert.AreEqual(StatusCodes.Status404NotFound, result!.StatusCode);
        }

        [TestMethod]
        public async Task Get_WhenAuthorIdDoesExist_ReturnsAuthor()
        {
            // Arrange            
            _context.Authors.Add(_defaultAuthor);
            _context.Authors.Add(_defaultAuthor2);
            await _context.SaveChangesAsync();

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
        public async Task Post_WhenValidAuthorProvided_CreatesNewAuthor()
        {
            // Act
            var response = await _controller.Post(_defaultAuthorCreationDTO);

            // Assert
            var result = response as CreatedAtRouteResult;
            Assert.IsNotNull(result);

            var authorsCount = await _context.Authors.CountAsync();
            Assert.AreEqual(1, authorsCount);
        }
    }
}
