using LibraryAPI.Validation;
using System.ComponentModel.DataAnnotations;

namespace LibraryAPITests.UnitTests.Validations
{
    [TestClass]
    public class FirstLetterCapitalAttributeTests
    {
        private FirstLetterCapitalAttribute _attribute = null!;
        private ValidationContext _validationContext = null!;

        [TestInitialize]
        public void Setup()
        {
            _attribute = new FirstLetterCapitalAttribute();
            _validationContext = new ValidationContext(new object());
        }

        [TestMethod]
        [DataRow("")]
        [DataRow(null)]
        [DataRow("Text")]
        public void IsValid_StringEmptyNullOrCapitalized_ReturnsSuccess(string value)
        {
            // Act
            var result = _attribute.GetValidationResult(value, _validationContext);

            // Assert
            Assert.AreEqual(ValidationResult.Success, result);
        }

        [TestMethod]
        [DataRow("text")]
        public void IsValid_StringStartingWithLowercase_ReturnsError(string value)
        {
            // Act
            var result = _attribute.GetValidationResult(value, _validationContext);

            // Assert
            Assert.IsNotNull(result);
            Assert.AreEqual(FirstLetterCapitalAttribute.DefaultErrorMessage, result!.ErrorMessage);
        }
    }
}
