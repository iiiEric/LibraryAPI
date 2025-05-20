using LibraryAPI.DTOs;
using LibraryAPI.Entities;
using System.Reflection;

namespace LibraryAPITests.UnitTests.Builders
{
    [TestClass]
    public class BuilderTests
    {
        public void Builder_ShouldSetAllProperties<TClass, TBuilder>()
        {
            // Act
            var classProperties = typeof(TClass).GetProperties(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance).Where(p => p.SetMethod != null).ToList();
            var builderFields = typeof(TBuilder).GetFields(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance).ToList();

            // Assert
            foreach (var classProperty in classProperties)
            {
                // Compara el nombre de las propiedades con los campos, ignorando mayúsculas y guiones bajos
                if (!builderFields.Any(x => x.Name.ToLower().Replace("_", "") == classProperty.Name.ToLower().Replace("_", "")))
                {
                    Assert.Fail($"Property '{classProperty.Name}' is missing in the builder.");
                }
            }
        }

        [TestMethod]
        public void AuthorBuilder_ShouldSetAllProperties()
        {
            Builder_ShouldSetAllProperties<Author, AuthorBuilder>();
        }

        [TestMethod]
        public void AuthorCreationDTOBuilder_ShouldSetAllProperties()
        {
            Builder_ShouldSetAllProperties<AuthorCreationDTO, AuthorCreationDTOBuilder>();
        }

        [TestMethod]
        public void AuthorDTOBuilder_ShouldSetAllProperties()
        {
            Builder_ShouldSetAllProperties<AuthorDTO, AuthorDTOBuilder>();
        }

        [TestMethod]
        public void AuthorFilterDTOBuilder_ShouldSetAllProperties()
        {
            Builder_ShouldSetAllProperties<AuthorFilterDTO, AuthorFilterDTOBuilder>();
        }

        [TestMethod]
        public void AuthorWithBooksDTOBuilder_ShouldSetAllProperties()
        {
            Builder_ShouldSetAllProperties<AuthorWithBooksDTO, AuthorWithBooksDTOBuilder>();
        }

        [TestMethod]
        public void AuthorCreationWithImageDTOBuilder_ShouldSetAllProperties()
        {
            Builder_ShouldSetAllProperties<AuthorCreationWithImageDTO, AuthorCreationWithImageDTOBuilder>();
        }
    }
}
