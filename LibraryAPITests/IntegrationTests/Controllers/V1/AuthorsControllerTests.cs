using LibraryAPI.DTOs;
using LibraryAPI.Entities;
using LibraryAPITests.UnitTests.Builders;
using LibraryAPITests.Utilidades;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using System.Net;
using System.Security.Claims;
using System.Text.Json;

namespace LibraryAPITests.IntegrationTests.Controllers.V1
{
    [TestClass]
    public class AuthorsControllerTests : TestBase
    {
        #region Test Data
        private static readonly string _url = "/api/v1/authors";
        private string _databaseName = Guid.NewGuid().ToString();
        private Author _defaultAuthor = new AuthorBuilder().WithName("Eric").WithSurname1("Aaa").Build();
        private Author _defaultAuthor2 = new AuthorBuilder().WithName("Alex").WithSurname1("Bbb").Build();
        private HttpClient _client = null!;
        private WebApplicationFactory<Program> _factory = null!;
        #endregion

        [TestInitialize]
        public void Setup()
        {
            _factory = BuildWebApplicationFactory(_databaseName, false);
            _client = _factory.CreateClient();
        }

        [TestMethod]
        public async Task Get_WhenAuthorIdDoesNotExist_ReturnsNotFound()
        {
            // Act
            var response = await _client.GetAsync($"{_url}/1");

            // Assert
            var statusCode = response.StatusCode;
            Assert.AreEqual(HttpStatusCode.NotFound, response.StatusCode);
        }

        [TestMethod]
        public async Task Get_WhenAuthorIdDoesExist_ReturnsAuthor()
        {
            // Arrange
            var context = BuildContext(_databaseName);
            context.Authors.Add(_defaultAuthor);
            context.Authors.Add(_defaultAuthor2);
            await context.SaveChangesAsync();

            // Act
            var response = await _client.GetAsync($"{_url}/1");

            // Assert
            response.EnsureSuccessStatusCode();
            var author = JsonSerializer.Deserialize<AuthorWithBooksDTO>(await response.Content.ReadAsStringAsync(), _jsonSerializerOptions)!;
            Assert.AreEqual(1, author.Id);
        }

        [TestMethod]
        public async Task Post_WhenUserIsNotAuthenticated_ReturnsUnauthorized()
        {
            // Arrange
            var authorCreationDTO = new AuthorCreationDTO
            {
                Name = "Eric",
                Surname1 = "Aaa",
                Identity = "123"
            };

            // Act
            var response = await _client.PostAsJsonAsync(_url, authorCreationDTO);

            // Assert
            Assert.AreEqual(HttpStatusCode.Unauthorized, response.StatusCode);
        }

        [TestMethod]
        public async Task Post_WhenUserIsNotAdmin_ReturnsForbidden()
        {
            // Arrange
            var token = await CreateUser(_databaseName, _factory);

            _client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);

            var authorCreationDTO = new AuthorCreationDTO
            {
                Name = "Eric",
                Surname1 = "Aaa",
                Identity = "123"
            };

            // Act
            var response = await _client.PostAsJsonAsync(_url, authorCreationDTO);

            // Assert
            Assert.AreEqual(HttpStatusCode.Forbidden, response.StatusCode);
        }

        [TestMethod]
        public async Task Post_WhenUserIsAdmin_ReturnsCreated()
        {
            // Arrange
            var claims = new List<Claim> { _adminClaim };
            var token = await CreateUser(_databaseName, _factory, claims);

            _client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);

            var authorCreationDTO = new AuthorCreationDTO
            {
                Name = "Eric",
                Surname1 = "Aaa",
                Identity = "123"
            };

            // Act
            var response = await _client.PostAsJsonAsync(_url, authorCreationDTO);
            response.EnsureSuccessStatusCode();

            // Assert
            Assert.AreEqual(HttpStatusCode.Created, response.StatusCode);
        }
    }
}