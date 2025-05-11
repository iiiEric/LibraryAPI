using AutoMapper;
using LibraryAPI.Data;
using LibraryAPI.DTOs;
using LibraryAPI.Utils;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.VisualStudio.TestPlatform.TestHost;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http.Json;
using System.Security.Claims;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using LibraryAPI.DTOs.Users;

namespace LibraryAPITests.Utilidades
{
    public class TestBase
    {
        protected readonly JsonSerializerOptions jsonSerializerOptions = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };

        protected readonly Claim adminClaim = new Claim("Admin", "1");

        protected ApplicationDbContext BuildContext(string databaseName)
        {
            var options = new DbContextOptionsBuilder<ApplicationDbContext>().UseInMemoryDatabase(databaseName).Options;
            var dbContext = new ApplicationDbContext(options);
            return dbContext;
        }

        protected IMapper ConfigureAutoMapper()
        {
            var config = new MapperConfiguration(options =>
            {
                options.AddProfile(new AutoMapperProfiles());
            });
            return config.CreateMapper();
        }

        //protected WebApplicationFactory<Program> BuildWebApplicationFactory(string databaseName, bool ignoreSecurity = true)
        //{
        //    var factory = new WebApplicationFactory<Program>();

        //    factory = factory.WithWebHostBuilder(builder =>
        //    {
        //        builder.ConfigureTestServices(services =>
        //        {
        //            ServiceDescriptor descriptorDBContext = services.SingleOrDefault(d => d.ServiceType == typeof(IDbContextOptionsConfiguration<ApplicationDbContext>))!;

        //            if (descriptorDBContext is not null)  
        //                services.Remove(descriptorDBContext);
                    
        //            services.AddDbContext<ApplicationDbContext>(options => options.UseInMemoryDatabase(databaseName));

        //            if (ignoreSecurity)
        //            {
        //                services.AddSingleton<IAuthorizationHandler, AllowAnonymousHandler>();

        //                services.AddControllers(options =>
        //                {
        //                    options.Filters.Add(new UsuarioFalsoFiltro());
        //                });
        //            }

        //        });
        //    });

        //    return factory;
        //}

        protected async Task<string> CreateUser(string databaseName, WebApplicationFactory<Program> factory)
            => await CreateUser(databaseName, factory, [], "example@gmail.com");

        protected async Task<string> CreateUser(string databaseName, WebApplicationFactory<Program> factory, IEnumerable<Claim> claims)
            => await CreateUser(databaseName, factory, claims, "example@gmail.com");

        protected async Task<string> CreateUser(string databaseName, WebApplicationFactory<Program> factory, IEnumerable<Claim> claims, string email)
        {
            var registrationUrl = "/api/v1/users/register";
            string token = string.Empty;
            token = await GetToken(email, registrationUrl, factory);

            if (claims.Any())
            {
                var context = BuildContext(databaseName);
                var user = await context.Users.Where(x => x.Email == email).FirstAsync();
                Assert.IsNotNull(user);

                var userClaims = claims.Select(x => new IdentityUserClaim<string>
                {
                    UserId = user.Id,
                    ClaimType = x.Type,
                    ClaimValue = x.Value
                });

                context.UserClaims.AddRange(userClaims);
                await context.SaveChangesAsync();
                var loginUrl = "/api/v1/users/login";
                token = await GetToken(email, loginUrl, factory);
            }

            return token;
        }

        private async Task<string> GetToken(string email, string url, WebApplicationFactory<Program> factory)
        {
            var password = "aA123456!";
            var credentials = new UserCredentialsDTO { Email = email, Password = password };
            var client = factory.CreateClient();
            var response = await client.PostAsJsonAsync(url, credentials);
            response.EnsureSuccessStatusCode();

            var content = await response.Content.ReadAsStringAsync();
            var authenticationResponse = JsonSerializer.Deserialize<AuthenticationResponseDTO>(content, jsonSerializerOptions)!;

            Assert.IsNotNull(authenticationResponse.Token);

            return authenticationResponse.Token;
        }
    }
}