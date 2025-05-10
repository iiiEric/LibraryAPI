using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.Mvc;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace LibraryAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class SecurityController : ControllerBase
    {
        private readonly IDataProtector _dataProtector;
        private readonly ITimeLimitedDataProtector _timeLimitedDataProtector;

        public SecurityController(IDataProtectionProvider dataProtectionProvider)
        {
            _dataProtector = dataProtectionProvider.CreateProtector("SecurityController");
            _timeLimitedDataProtector = _dataProtector.ToTimeLimitedDataProtector();
        }

        [HttpGet("encrypt")]
        public ActionResult Encrypt(string plainText)
        {
            if (string.IsNullOrEmpty(plainText))
                return BadRequest("Plain text cannot be null or empty.");
            
            var encryptedText = _dataProtector.Protect(plainText);
            return Ok(new { encryptedText });
        }

        [HttpGet("decrypt")]
        public ActionResult Decrypt(string encryptedText)
        {
            if (string.IsNullOrEmpty(encryptedText))
                return BadRequest("Encrypted text cannot be null or empty.");
            
            var plainText = _dataProtector.Unprotect(encryptedText);
            return Ok(new { plainText });
        }

        [HttpGet("time-limited-encrypt")]
        public ActionResult TimeLimitedEncrypt(string plainText)
        {
            if (string.IsNullOrEmpty(plainText))
                return BadRequest("Plain text cannot be null or empty.");

            var encryptedText = _timeLimitedDataProtector.Protect(plainText, lifetime: TimeSpan.FromSeconds(30));
            return Ok(new { encryptedText });
        }

        [HttpGet("time-limited-decrypt")]
        public ActionResult TimeLimitedDecrypt(string encryptedText)
        {
            if (string.IsNullOrEmpty(encryptedText))
                return BadRequest("Encrypted text cannot be null or empty.");

            var plainText = _timeLimitedDataProtector.Unprotect(encryptedText);
            return Ok(new { plainText });
        }
    }
}
